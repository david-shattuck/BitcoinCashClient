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
        /// Get current balances of specified wallets
        /// </summary>
        /// <param name="addresses">Public addresses of wallets to lookup</param>
        /// <returns>A list of addresses with their current balances. Empty wallets will not be included.</returns>
        public static List<KeyValuePair<string, long>> GetWalletBalances(List<string> addresses)
        {
            var addrs = string.Join(",", addresses);

            var url = $"{Constants.ApiUrl}/wallet/getbalances";

            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string> ("addresses", addrs)
            };

            return PostToApi<List<KeyValuePair<string, long>>>(url, data);
        }

        /// <summary>
        /// Get the list of tx hashes from provided list that exist in the blockchain or mempool
        /// </summary>
        /// <param name="hashes">The list of transaction hashes to be checked</param>
        /// <returns>A list of transaction hashes that exist in the blockchain or mempool</returns>
        public static List<string> GetValidTxHashes(List<string> hashes)
        {
            var hashCsv = string.Join(',', hashes);

            var url = $"{Constants.ApiUrl}/transaction/getvalidtxhashes?hashes={hashCsv}";

            return GetFromApi<List<string>>(url);
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

        private static T PostToApi<T>(string url, List<KeyValuePair<string, string>> data)
        {
            var client = new HttpClient();

            var stringContent = new FormUrlEncodedContent(data);

            var response = client.PostAsync(url, stringContent).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject<T>(result)!;
        }
    }
}
