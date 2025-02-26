using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.DAL.Repositories.Interfaces;
using BitcoinCash.API.Models;
using BitcoinCash.API.Models.DatabaseModels;
using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.API.Utilities.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BitcoinCash.API.Services
{
    public class KeyService(ICipher cipher, 
                            IMemoryCache cache,
                            IBitcoinClient bitcoinClient,
                            IKeyRepository keyRepository,
                            ICoinGeckoClient coinGeckoClient,
                            IBchTransactionService bchTransactionService) : IKeyService
    {
        private readonly ICipher _cipher = cipher;
        private readonly IMemoryCache _cache = cache;
        private readonly IBitcoinClient _bitcoinClient = bitcoinClient;
        private readonly IKeyRepository _keyRepository = keyRepository;
        private readonly ICoinGeckoClient _coinGeckoClient = coinGeckoClient;
        private readonly IBchTransactionService _bchTransactionService = bchTransactionService;

        public string GetKey()
        {
            var wallet = _bitcoinClient.CreateWallet();
            var encryptedKey = _cipher.Encrypt(wallet.PrivateKey!);

            var key = new Key
            {
                PrivateKey = encryptedKey,
                Address = wallet.PublicAddress!,
                RemainingCalls = 0,
                LastActivity = DateTime.UtcNow
            };

            _keyRepository.Add(key);

            return wallet.PublicAddress!;
        }

        public int? GetCalls(string address)
        {
            var cacheKey = $"RemainingCalls-{address}";
            if (_cache.TryGetValue(cacheKey, out int? remainingCalls))
                return remainingCalls;

            if (!_bitcoinClient.IsAddressValid(address))
            {
                remainingCalls = null;
                SaveToCache(cacheKey, remainingCalls!);
                return remainingCalls;
            }                

            var key = _keyRepository.Get(address);

            if (key == null)
                remainingCalls = null;
            else
                remainingCalls = key.RemainingCalls;

            SaveToCache(cacheKey, remainingCalls!);
            return remainingCalls;
        }

        public bool IsValid(string address)
        {
            if (!_cache.TryGetValue(CacheKeys.InvalidAddresses, out List<string>? invalidAddresses))
                invalidAddresses = [];

            if (invalidAddresses!.Contains(address))
                return false;

            if (!_bitcoinClient.IsAddressValid(address))
                return false;

            var key = _keyRepository.Get(address);

            if (key == null || key.RemainingCalls <= 0)
            {
                invalidAddresses!.Add(address);
                SaveToCache(CacheKeys.InvalidAddresses, invalidAddresses);
                return false;
            }

            _keyRepository.UpdateCalls(address, -1);
            return true;
        }

        public bool CanGetKey()
        {
            if (!_cache.TryGetValue(CacheKeys.UnlockTime, out DateTime unlockTime))
                unlockTime = DateTime.UtcNow.AddMinutes(-1);

            if (!_cache.TryGetValue(CacheKeys.RecentAttempts, out List<DateTime>? keyCreateAttempts))
                keyCreateAttempts = [];

            keyCreateAttempts = keyCreateAttempts?.Where(kca => kca > DateTime.UtcNow.AddHours(-1)).ToList() ?? [];

            keyCreateAttempts.Add(DateTime.UtcNow);

            var newUnlockTime = DateTime.UtcNow.AddSeconds(keyCreateAttempts.Count);

            SaveToCache(CacheKeys.UnlockTime, newUnlockTime);
            SaveToCache(CacheKeys.RecentAttempts, keyCreateAttempts);

            return unlockTime < DateTime.UtcNow;
        }

        public async Task CheckForPayments()
        {
            if (!_cache.TryGetValue(CacheKeys.NextPaymentCheck, out DateTime nextPaymentCheck))
                nextPaymentCheck = DateTime.UtcNow.AddMinutes(-1);

            if (nextPaymentCheck > DateTime.UtcNow)
                return;

            SaveToCache(CacheKeys.NextPaymentCheck, DateTime.UtcNow.AddHours(1));

            var activeKeys = _keyRepository.GetActive();

            var balances = await _bchTransactionService.GetFundedKeyBalances(activeKeys);

            if (balances == null || balances.Count == 0)
                return;

            var bchValue = await _coinGeckoClient.GetValue();
            List<Key> fundedKeys = [];

            foreach (var balance in balances)
            {
                var address = balance.Key;
                var sats = balance.Value;

                var usdSent = (decimal)sats / 100000000 * bchValue;
                var requestCost = GetRequestCost(usdSent);

                int requestsPurchased = Convert.ToInt32(usdSent / requestCost);

                _keyRepository.UpdateCalls(address, requestsPurchased);

                fundedKeys.Add(activeKeys.First(ak => ak.Address == address));
            }

            await _bchTransactionService.BuyRequests(fundedKeys);
        }

        private static decimal GetRequestCost(decimal usdSent)
        {
            if (usdSent < 10)
                return 0.0015m;

            if (usdSent < 50)
                return 0.00145m;

            if (usdSent < 100)
                return 0.0014m;

            if (usdSent < 500)
                return 0.00135m;

            if (usdSent < 1000)
                return 0.0013m;

            if (usdSent < 5000)
                return 0.00125m;

            if (usdSent < 10000)
                return 0.0012m;

            return 0.00115m;
        }

        private void SaveToCache(string key, object obj)
        {
            _cache.Set(key, obj, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.UtcNow.AddHours(1)
            });
        }
    }
}
