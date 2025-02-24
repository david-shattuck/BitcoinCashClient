using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Models;
using BitcoinCash.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BitcoinCash.API.Clients
{
    public class BitcoinClient : IBitcoinClient
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration Configuration;
        private readonly BitcoinCashClient _bitcoinCashClient;

        public BitcoinClient(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            Configuration = configuration;            

            var options = new ClientOptions
            {
                ApiKey = Configuration["BchApiKey"]
            };

            _bitcoinCashClient = new BitcoinCashClient(options);
        }

        public Wallet CreateWallet() => _bitcoinCashClient.GetWallet();

        public async Task<Wallet> GetWallet(string key) => await _bitcoinCashClient.GetWallet(key);

        public async Task<List<Wallet>> GetWallets(List<string> keys) => await _bitcoinCashClient.GetWallets(keys);

        public async Task<List<KeyValuePair<string, long>>> GetWalletBalances(List<string> addresses, bool getNullOnError = false)
        {
            try
            {
                return await _bitcoinCashClient.GetWalletBalances(addresses);
            }
            catch
            {
                return getNullOnError ? null : [];
            }
        } 

        public async Task<List<Wallet>> GetWalletsByAddresses(List<string> addresses)
        {
            try
            {
                return await _bitcoinCashClient.GetWalletsByAddresses(addresses);
            }
            catch
            {
                return null;
            }            
        }

        public async Task<List<string>> GetValidTxHashes(List<string> txHashes) => await _bitcoinCashClient.GetValidTxHashes(txHashes);

        public async Task<decimal> GetValue()
        {
            try
            {
                var cacheKey = CacheKeys.BchPrice;

                if (!_cache.TryGetValue(cacheKey, out decimal bchPrice))
                {
                    bchPrice = await _bitcoinCashClient.GetFiatValue();

                    AddToCache(cacheKey, bchPrice);
                }

                return bchPrice;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<long> GetValueInSats(int usd)
        {
            var dollarsPerBch = await GetValue();

            if (dollarsPerBch == 0)
                return 0;

            var dollarsPerSat = dollarsPerBch / Constants.SatoshiMultiplier;

            var satsPerDollar = (int)(1 / dollarsPerSat);

            return usd * satsPerDollar;
        }

        public async Task<int> GetValueInUsd(long sats)
        {
            var dollarsPerBch = await GetValue();

            if (dollarsPerBch == 0)
                return 0;

            decimal bch = sats / (decimal)Constants.SatoshiMultiplier;

            return Convert.ToInt32(bch * dollarsPerBch);
        }

        public async Task SendPayments(List<Wallet> wallets) => await SendPayments(wallets, Addresses.DevWallet);

        public async Task SendPayments(List<Wallet> wallets, string address)
        {
            foreach (var wallet in wallets)
                await wallet.SendAll(address);
        }

        public bool IsAddressValid(string address) => _bitcoinCashClient.IsAddressValid(address);

        private void AddToCache(string cacheKey, object obj)
        {
            _cache.Set(cacheKey, obj, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.UtcNow.AddMinutes(1)
            });
        }
    }
}
