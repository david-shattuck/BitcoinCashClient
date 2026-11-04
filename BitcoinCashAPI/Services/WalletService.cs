using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Models;
using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.Models;

namespace BitcoinCash.API.Services
{
    public class WalletService(ICoinGeckoClient coinGeckoClient, IBlockChairClient blockChairClient, IFulcrumClient fulcrumClient, IRiftenClient riftenClient) : IWalletService
    {
        private readonly IBlockChairClient _blockChairClient = blockChairClient;
        private readonly ICoinGeckoClient _coinGeckoClient = coinGeckoClient;
        private readonly IFulcrumClient _fulcrumClient = fulcrumClient;
        private readonly IRiftenClient _riftenClient = riftenClient;

        public async Task<List<Wallet>?> GetWalletInfo(List<string> addresses, string currency)
        {
            var wallets = addresses.Select(a => new Wallet { PublicAddress = a }).ToList();

            var utxos = await _fulcrumClient.GetUtxos(addresses);

            if (utxos == null)
                return null;

            var bchValue = await _coinGeckoClient.GetValue(currency);

            var bcmrs = await GetBcmrs(utxos);

            wallets.ForEach(w =>
            {
                w.utxos = utxos.Where(u => u.address == w.PublicAddress).ToList();
                w.Value = bchValue == 0 ? null : (decimal)w.Balance! / Constants.SatoshiMultiplier * bchValue;
                w.ValueCurrency = currency;
                w.FungibleTokens = GetFungibleTokens(w.utxos, bcmrs);
            });

            return wallets;
        }

        private static List<FungibleToken> GetFungibleTokens(List<utxo> utxos, Dictionary<string, Bcmr?> bcmrs) =>
            FungibleTokenUtxos(utxos)
                .GroupBy(u => u.token_data!.category!)
                .Select(g =>
                {
                    var bcmr = bcmrs.GetValueOrDefault(g.Key);

                    return new FungibleToken
                    {
                        Name = bcmr?.Name,
                        Category = g.Key,
                        Balance = g.Sum(u => u.token_data!.amount),
                        DecimalPlaces = bcmr?.Token?.Decimals ?? 0
                    };
                })
                .ToList();

        private async Task<Dictionary<string, Bcmr?>> GetBcmrs(List<utxo> utxos)
        {
            var categories = FungibleTokenUtxos(utxos).Select(u => u.token_data!.category!).Distinct().ToList();

            var bcmrs = await Task.WhenAll(categories.Select(_riftenClient.GetTokenBcmr));

            return categories.Zip(bcmrs).ToDictionary(p => p.First, p => p.Second);
        }

        private static IEnumerable<utxo> FungibleTokenUtxos(IEnumerable<utxo> utxos) =>
            utxos.Where(u => u.token_data?.amount > 0 && !string.IsNullOrEmpty(u.token_data.category));

        public async Task<List<KeyValuePair<string, long>>?> GetWalletBalances(List<string> addresses) => await _blockChairClient.GetWalletBalances(addresses);
    }
}
