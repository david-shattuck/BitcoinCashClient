using BitcoinCash.Client;
using BitcoinCash.Models;
using NBitcoin;
using NBitcoin.Altcoins;

namespace BitcoinCash
{
    public class BitcoinCashClient
    {
        private readonly Network _network = BCash.Instance.Mainnet;

        private readonly ApiClient _apiClient;                

        public BitcoinCashClient()
        {
            _apiClient = new ApiClient();
        }

        public Wallet GetWallet()
        {
            var rawKey = new Key();
            var secret = rawKey.GetBitcoinSecret(_network);
            var address = secret.GetAddress(ScriptPubKeyType.Legacy);

            return new Wallet
            {
                PrivateKey = secret.ToString(),
                PublicAddress = address.ToString(),
                utxos = new List<utxo>()
            };
        }

        public Wallet GetWallet(string privateKey)
        {
            var secret = new BitcoinSecret(privateKey, _network);
            var address = secret.GetAddress(ScriptPubKeyType.Legacy).ToString();

            var wallet = new Wallet
            {
                PrivateKey = privateKey,
                PublicAddress = address
            };

            return FillWalletInfo(new List<Wallet> { wallet } ).First();
        }

        private List<Wallet> FillWalletInfo(List<Wallet> wallets)
        {
            var addresses = wallets.Select(w => w.PublicAddress).ToList();

            var filledWallets = _apiClient.GetWalletInfo(addresses);

            return wallets.Select(w => new Wallet
            {
                PrivateKey = w.PrivateKey,
                PublicAddress = w.PublicAddress,
                utxos = filledWallets.FirstOrDefault(fw => fw.PublicAddress == w.PublicAddress)?.utxos
            }).ToList();
        }
    }
}