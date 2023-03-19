using Newtonsoft.Json;
using BitcoinCash.API.Clients.Interfaces;

namespace BitcoinCash.API.Clients
{
    public class CoinGeckoClient : ICoinGeckoClient
    {
        // https://www.coingecko.com/en/api/documentation
        private readonly string _baseUrl = "https://api.coingecko.com/api/v3";

        public decimal GetValue(string currency = "usd")
        {
            var client = new HttpClient();

            var response = client.GetAsync($"{_baseUrl}/simple/price?ids=bitcoin-cash&vs_currencies={currency}").Result;

            if (!response.IsSuccessStatusCode)
                return 0;

            var json = response.Content.ReadAsStringAsync().Result;

            dynamic? data = JsonConvert.DeserializeObject(json);

            if (data == null) return 0;

            return data["bitcoin-cash"][currency];
        }
    }
}
