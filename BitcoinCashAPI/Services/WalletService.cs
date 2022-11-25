using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.Models;

namespace BitcoinCash.API.Services
{
    public class WalletService : IWalletService
    {
        private readonly IBlockChairClient _blockChairClient;

        public WalletService(IBlockChairClient blockChairClient)
        {
            _blockChairClient = blockChairClient;
        }

        public List<Wallet> GetWalletInfo(List<string> addresses)
        {
            var wallets = addresses.Select(a => new Wallet { PublicAddress = a }).ToList();

            var utxos = _blockChairClient.GetUtxos(addresses);

            wallets.ForEach(w => w.utxos = utxos.Where(u => u.cashAddr == w.PublicAddress).ToList());

            return wallets;
        }
    }
}
