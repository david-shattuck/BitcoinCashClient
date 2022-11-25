using BitcoinCash.Models;
using Newtonsoft.Json;

namespace BitcoinCash.Client
{
    public class ApiClient
    {
        private readonly string _baseUrl = "https://localhost:7035";

        public List<Wallet> GetWalletInfo(List<string> addresses)
        {
            var client = new HttpClient();

            var addrs = string.Join(",", addresses);

            try
            {
                var response = client.GetAsync($"{_baseUrl}/wallet?addresses={addrs}").Result;

                var result = response.Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<List<Wallet>>(result);
            }
            catch (Exception ex)
            {
                return new List<Wallet>();
            }
        }
    }
}
