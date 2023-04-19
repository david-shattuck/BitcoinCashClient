using BitcoinCash.Models;
using Newtonsoft.Json;

namespace BitcoinCash.Client
{
    public class ApiClient
    {
        private readonly string _baseUrl = "https://bch-api.com";
        //private readonly string _baseUrl = "https://localhost:7035";

        public List<Wallet> GetWalletInfo(List<string> addresses)
        {
            var addrs = string.Join(",", addresses);

            var url = $"{_baseUrl}/wallet?addresses={addrs}";

            return GetFromApi<List<Wallet>>(url);
        }

        public decimal GetFiatValue(string currency)
        {
            var url = $"{_baseUrl}/fiat/getvalue?currency={currency}";

            return GetFromApi<decimal>(url);
        }

        private static T GetFromApi<T>(string url)
        {
            var client = new HttpClient();

            var response = client.GetAsync(url).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject<T>(result)!;
        }
    }
}
