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

        /// <summary>
        /// Generate a new private key and its associated public address
        /// </summary>
        /// <returns>A new empty wallet</returns>
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

        /// <summary>
        /// Get the wallet associated with the given private key
        /// </summary>
        /// <param name="privateKey">Any valid BCH wallet private key</param>
        /// <returns>A live wallet, including its public address, sendable balance, value, and utxos</returns>
        public Wallet GetWallet(string privateKey)
        {
            return GetWallets(new List<string> { privateKey }).First();
        }

        /// <summary>
        /// Get the list of wallets associated with the given private keys
        /// </summary>
        /// <param name="privateKeys">A list of valid BCH wallet private keys</param>
        /// <returns>A list of live wallets, including their public addresses, sendable balances, values, and utxos</returns>
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

        /// <summary>
        /// Get the wallet associated with the given public address
        /// </summary>
        /// <param name="address">Any valid BCH public address</param>
        /// <returns>A read-only wallet, including its balance, value, and utxos</returns>
        public Wallet GetWalletByAddress(string address)
        {
            return GetWalletsByAddresses(new List<string> { address }).First();
        }        

        /// <summary>
        /// Get the wallets associated with the given public addresses
        /// </summary>
        /// <param name="addresses">A list of valid BCH public addresses</param>
        /// <returns>A list of read-only wallets, including their balances, values, and utxos</returns>
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