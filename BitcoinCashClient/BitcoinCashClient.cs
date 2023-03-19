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

        public void DoesNothingTest()
        {
            //this does nothing
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
            return GetWallets(new List<string> { privateKey }).First();
        }

        public List<Wallet> GetWallets(List<string> privateKeys)
        {
            var wallets = new List<Wallet>();

            foreach (var key in privateKeys)
            {
                var secret = new BitcoinSecret(key, _network);
                var address = secret.GetAddress(ScriptPubKeyType.Legacy).ToString();

                wallets.Add(new Wallet
                {
                    PrivateKey = key,
                    PublicAddress = address
                });
            }

            return FillWalletInfo(wallets);
        }

        public Wallet GetWalletByAddress(string address)
        {
            return GetWalletsByAddresses(new List<string> { address }).First();
        }        

        public List<Wallet> GetWalletsByAddresses(List<string> addresses)
        {
            var wallets = addresses.Select(a => new Wallet
            {
                PublicAddress = a
            }).ToList();

            return FillWalletInfo(wallets);
        }

        private List<Wallet> FillWalletInfo(List<Wallet> wallets)
        {
            var addresses = wallets.Select(w => w.PublicAddress).ToList();

            var filledWallets = _apiClient.GetWalletInfo(addresses!);

            return wallets.Select(w =>
            {
                var filledWallet = filledWallets.FirstOrDefault(fw => fw.PublicAddress == w.PublicAddress);
                if (filledWallet == null)
                    return w;

                filledWallet.PrivateKey = w.PrivateKey;

                return filledWallet;
            }).ToList();
        }
    }
}