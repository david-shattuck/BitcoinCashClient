using Newtonsoft.Json;
using BitcoinCash.API.Clients.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BitcoinCash.API.Clients
{
    public class CoinGeckoClient(IMemoryCache cache) : ICoinGeckoClient
    {
        // https://www.coingecko.com/en/api/documentation
        private readonly string _baseUrl = "https://api.coingecko.com/api/v3";

        private readonly IMemoryCache _cache = cache;

        public decimal GetValue(string currency = "usd")
        {
            if(_cache.TryGetValue(currency, out decimal value))
                return value;

            var client = new HttpClient();

            var response = client.GetAsync($"{_baseUrl}/simple/price?ids=bitcoin-cash&vs_currencies={currency}").Result;

            if (!response.IsSuccessStatusCode)
                return 0;

            var json = response.Content.ReadAsStringAsync().Result;

            dynamic? data = JsonConvert.DeserializeObject(json);

            if (data == null) return 0;

            value = data["bitcoin-cash"][currency];

            SaveToCache(currency, value);

            return value;
        }

        private void SaveToCache(string currency, decimal value) 
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.UtcNow.AddMinutes(2)
            };

            _cache.Set(currency, value, options);
        }
    }
}
