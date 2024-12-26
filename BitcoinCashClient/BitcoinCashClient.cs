using BitcoinCash.Client;
using BitcoinCash.Models;
using NBitcoin;
using NBitcoin.Altcoins;
using SharpCashAddr;

namespace BitcoinCash
{
    /// <summary>
    /// Create or fetch Bitcoin Cash wallets
    /// </summary>
    public class BitcoinCashClient
    {
        private string? _defaultCurrency;
        private readonly Network _network = BCash.Instance.Mainnet;        

        /// <summary>
        /// Instantiate BchClient with default options
        /// </summary>
        public BitcoinCashClient() => SetOptions();

        /// <summary>
        /// Instantiate BchClient with custom options
        /// </summary>
        /// <param name="options">Configuration object with desired options set</param>
        public BitcoinCashClient(ClientOptions options) => SetOptions(options);

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
                utxos = [],
                Value = 0,
                ValueCurrency = _defaultCurrency
            };
        }

        /// <summary>
        /// Get the wallet associated with the given private key
        /// </summary>
        /// <param name="privateKey">Any valid BCH wallet private key</param>
        /// <returns>A live wallet, including its public address, sendable balance, value, and utxos</returns>
        public async Task<Wallet> GetWallet(string privateKey) => (await GetWallets([privateKey])).First();

        /// <summary>
        /// Get the list of wallets associated with the given private keys
        /// </summary>
        /// <param name="privateKeys">A list of valid BCH wallet private keys</param>
        /// <returns>A list of live wallets, including their public addresses, sendable balances, values, and utxos</returns>
        public async Task<List<Wallet>> GetWallets(List<string> privateKeys)
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

            return await FillWalletInfo(wallets);
        }

        /// <summary>
        /// Get the wallet associated with the given public address
        /// </summary>
        /// <param name="address">Any valid BCH public address</param>
        /// <returns>A read-only wallet, including its balance, value, and utxos</returns>
        public async Task<Wallet> GetWalletByAddress(string address) => (await GetWalletsByAddresses([address])).First();

        /// <summary>
        /// Get the wallets associated with the given public addresses
        /// </summary>
        /// <param name="addresses">A list of valid BCH public addresses</param>
        /// <returns>A list of read-only wallets, including their balances, values, and utxos</returns>
        public async Task<List<Wallet>> GetWalletsByAddresses(List<string> addresses)
        {
            var wallets = addresses.Select(a => new Wallet
            {
                PublicAddress = GetCashAddr(a)
            }).ToList();

            return await FillWalletInfo(wallets);
        }

        /// <summary>
        /// Get the current balances of all addresses in the provided list that have balances
        /// </summary>
        /// <param name="addresses">The list of addresses to be checked</param>
        /// <returns>A list that includes every address that has a non-0 balance along with its associated balance</returns>
        public async Task<List<KeyValuePair<string, long>>> GetWalletBalances(List<string> addresses)
        {
            if (addresses.Count == 0)
                return [];

            var balances = await ApiClient.GetWalletBalances(addresses);

            return balances.Select(b => new KeyValuePair<string, long>(GetCashAddr(b.Key), b.Value)).ToList();
        }

        /// <summary>
        /// Get the list of tx hashes from provided list that exist in the blockchain or mempool
        /// </summary>
        /// <param name="txHashes">The list of transaction hashes to be checked</param>
        /// <returns>A list of transaction hashes that exist in the blockchain or mempool</returns>
        public static async Task<List<string>> GetValidTxHashes(List<string> txHashes) => await ApiClient.GetValidTxHashes(txHashes);

        /// <summary>
        /// Convert a BCH address in any valid format into CashAddr
        /// </summary>
        /// <param name="address">Any legacy or modern BCH address</param>
        /// <returns>A CashAddr format BCH address or an empty string if input was invalid</returns>
        public string GetCashAddr(string address)
        {
            if (address.StartsWith("1") || address.StartsWith("3"))
                address = address.ToCashAddress();

            if (!address.StartsWith("bitcoincash:"))
                address = string.Concat("bitcoincash:", address);

            return IsAddressValid(address) ? address : string.Empty;
        }

        /// <summary>
        /// Check to see if given string is valid BCH address
        /// </summary>
        /// <param name="address">The address to check</param>
        /// <returns>True if address is valid, otherwise false</returns>
        public bool IsAddressValid(string address)
        {
            try
            {
                BitcoinAddress.Create(address, _network);
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get the current market value of BCH in the default fiat currency
        /// </summary>
        /// <returns>The current fiat value of BCH</returns>
        public async Task<decimal> GetFiatValue() => await ApiClient.GetFiatValue(_defaultCurrency!);

        /// <summary>
        /// Get the current market value of BCH in the specified fiat currency
        /// </summary>
        /// <param name="currency">A Currency object from BitcoinCash.Models.Currency</param>
        /// <returns>The current fiat value of BCH</returns>
        public async Task<decimal> GetFiatValue(Currency currency)
        {
            ValidateFiat(currency);

            return await ApiClient.GetFiatValue(currency.Value);
        }

        private async Task<List<Wallet>> FillWalletInfo(List<Wallet> wallets)
        {
            var addresses = wallets.Select(w => w.PublicAddress).ToList();

            var filledWallets = await ApiClient.GetWalletInfo(addresses!, _defaultCurrency!);

            return wallets.Select(w =>
            {
                var filledWallet = filledWallets.FirstOrDefault(fw => fw.PublicAddress == w.PublicAddress);
                if (filledWallet == null)
                    return w;

                filledWallet.PrivateKey = w.PrivateKey;

                return filledWallet;
            }).ToList();
        }

        private void SetOptions()
        {
            var options = new ClientOptions
            {
                Currency = Currency.USDollar
            };

            SetOptions(options);
        }

        private void SetOptions(ClientOptions options)
        {
            ValidateOptions(options);

            _defaultCurrency = options.Currency != null ? options.Currency.Value : Currency.USDollar.Value;            
        }

        private static void ValidateOptions(ClientOptions options)
        {
            if (options.Currency != null)
                ValidateFiat(options.Currency);
        }

        private static void ValidateFiat(Currency currency)
        {
            if (currency.Value == Currency.BitcoinCash.Value ||
                currency.Value == Currency.Satoshis.Value)
                throw new Exception("BCH cannot be used for fiat value pair");
        }
    }
}