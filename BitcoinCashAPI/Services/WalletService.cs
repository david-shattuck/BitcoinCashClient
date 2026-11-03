using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.Models;

namespace BitcoinCash.API.Services
{
    public class WalletService(ICoinGeckoClient coinGeckoClient, IBlockChairClient blockChairClient, IFulcrumClient fulcrumClient) : IWalletService
    {
        private readonly IBlockChairClient _blockChairClient = blockChairClient;
        private readonly ICoinGeckoClient _coinGeckoClient = coinGeckoClient;
        private readonly IFulcrumClient _fulcrumClient = fulcrumClient;

        public async Task<List<Wallet>?> GetWalletInfo(List<string> addresses, string currency)
        {
            var wallets = addresses.Select(a => new Wallet { PublicAddress = a }).ToList();

            var utxos = await _fulcrumClient.GetUtxos(addresses);

            if (utxos == null)
                return null;

            var bchValue = await _coinGeckoClient.GetValue(currency);

            wallets.ForEach(w =>
            {
                w.utxos = utxos.Where(u => u.address == w.PublicAddress).ToList();
                w.Value = bchValue == 0 ? null : (decimal)w.Balance! / Constants.SatoshiMultiplier * bchValue;
                w.ValueCurrency = currency;
            });

            return wallets;
        }

        public async Task<List<KeyValuePair<string, long>>?> GetWalletBalances(List<string> addresses) => await _blockChairClient.GetWalletBalances(addresses);
    }
}
