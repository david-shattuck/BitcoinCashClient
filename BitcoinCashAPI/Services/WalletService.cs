using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.Models;

namespace BitcoinCash.API.Services
{
    public class WalletService(ICoinGeckoClient coinGeckoClient, IBlockChairClient blockChairClient) : IWalletService
    {
        private readonly IBlockChairClient _blockChairClient = blockChairClient;
        private readonly ICoinGeckoClient _coinGeckoClient = coinGeckoClient;

        public List<Wallet> GetWalletInfo(List<string> addresses, string currency)
        {
            var wallets = addresses.Select(a => new Wallet { PublicAddress = a }).ToList();

            var utxos = _blockChairClient.GetUtxos(addresses);

            var bchValue = _coinGeckoClient.GetValue(currency);

            wallets.ForEach(w =>
            {
                w.utxos = utxos.Where(u => u.cashAddr == w.PublicAddress).ToList();
                w.Value = bchValue == 0 ? null : (decimal)w.Balance! / Constants.SatoshiMultiplier * bchValue;
                w.ValueCurrency = currency;
            });

            return wallets;
        }

        public List<KeyValuePair<string, long>> GetWalletBalances(List<string> addresses) => _blockChairClient.GetWalletBalances(addresses);
    }
}
