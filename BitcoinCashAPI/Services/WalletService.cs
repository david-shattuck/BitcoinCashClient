using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.Models;

namespace BitcoinCash.API.Services
{
    public class WalletService : IWalletService
    {
        private readonly IBlockChairClient _blockChairClient;
        private readonly ICoinGeckoClient _coinGeckoClient;

        public WalletService(ICoinGeckoClient coinGeckoClient, IBlockChairClient blockChairClient)
        {
            _coinGeckoClient = coinGeckoClient;
            _blockChairClient = blockChairClient;
        }

        public List<Wallet> GetWalletInfo(List<string> addresses, string currency)
        {
            var wallets = addresses.Select(a => new Wallet { PublicAddress = a }).ToList();

            var utxos = _blockChairClient.GetUtxos(addresses);

            var bchValue = _coinGeckoClient.GetValue(currency);

            wallets.ForEach(w =>
            {
                w.utxos = utxos.Where(u => u.cashAddr == w.PublicAddress).ToList();
                w.Value = bchValue == 0 ? null : (decimal)w.Balance! / 100000000 * bchValue;
                w.ValueCurrency = currency;
            });

            return wallets;
        }

        public List<KeyValuePair<string, long>> GetWalletBalances(List<string> addresses) => _blockChairClient.GetWalletBalances(addresses);
    }
}
