using BitcoinCash.Models;
using Newtonsoft.Json;

namespace BitcoinCash.Client
{
    public class ApiClient
    {
        public List<Wallet> GetWalletInfo(List<string> addresses, string currency)
        {
            var addrs = string.Join(",", addresses);

            var url = $"{Constants.ApiUrl}/wallet?addresses={addrs}&currency={currency}";

            return GetFromApi<List<Wallet>>(url);
        }

        public decimal GetFiatValue(string currency)
        {
            var url = $"{Constants.ApiUrl}/fiat/getvalue?currency={currency}";

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
