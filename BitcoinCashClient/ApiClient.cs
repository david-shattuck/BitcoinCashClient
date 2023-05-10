using BitcoinCash.Models;
using Newtonsoft.Json;

namespace BitcoinCash.Client
{
    /// <summary>
    /// An interface to the back-end API
    /// </summary>
    public class ApiClient
    {
        /// <summary>
        /// Get utxos and current value of specified wallets
        /// </summary>
        /// <param name="addresses">Public addresses of wallets to lookup</param>
        /// <param name="currency">Fiat currency to denominate wallet value</param>
        /// <returns>A list of wallets with utxos and value populated</returns>
        public static List<Wallet> GetWalletInfo(List<string> addresses, string currency)
        {
            var addrs = string.Join(",", addresses);

            var url = $"{Constants.ApiUrl}/wallet?addresses={addrs}&currency={currency}";

            return GetFromApi<List<Wallet>>(url);
        }

        /// <summary>
        /// Get the current market value of Bitcoin Cash
        /// </summary>
        /// <param name="currency">Fiat currency to denominate return value</param>
        /// <returns>The current market value of BCH</returns>
        public static decimal GetFiatValue(string currency)
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
