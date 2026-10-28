using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace BitcoinCash.API.Clients
{
    public class RiftenClient(IMemoryCache cache) : IRiftenClient
    {
        // https://docs.riftenlabs.com/cauldron/API/
        private readonly string _baseUrl = "https://indexer.cauldron.quest";

        private readonly IMemoryCache _cache = cache;

        public async Task<Bcmr?> GetTokenBcmr(string category)
        {
            try
            {
                var cacheKey = $"{CacheKeys.TokenBcmr}_{category}";

                if (!_cache.TryGetValue(cacheKey, out Bcmr? tokenBcmr))
                {
                    using var client = new HttpClient();

                    var response = await client.GetAsync($"{_baseUrl}/bcmr/token/{category}");

                    if (!response.IsSuccessStatusCode)
                        return null;

                    var json = await response.Content.ReadAsStringAsync();

                    tokenBcmr = JsonConvert.DeserializeObject<Bcmr>(json);

                    if (tokenBcmr != null)
                        AddToCache(cacheKey, tokenBcmr);
                }

                return tokenBcmr;
            }
            catch
            {
                return null;
            }
        }

        private void AddToCache(string cacheKey, object obj)
        {
            _cache.Set(cacheKey, obj, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.UtcNow.AddDays(1)
            });
        }
    }
}
