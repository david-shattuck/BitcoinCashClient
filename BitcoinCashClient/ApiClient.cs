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
        /// Get a new API key
        /// </summary>
        /// <returns>A public BCH address which is also an API key</returns>
        public static async Task<string> GetApiKey()
        {
            var url = $"{Constants.ApiUrl}/key";

            return await GetStringFromApi<string>(url);
        }

        /// <summary>
        /// Get the number of requests remaining on an API Key before it 
        /// needs to be refilled
        /// </summary>
        /// <param name="key">The API key to be checked</param>
        /// <returns>How many more calls can be made using this API key without refilling it</returns>
        public static async Task<int> GetApiKeyBalance(string key)
        {
            var url = $"{Constants.ApiUrl}/key/remainingcount?key={key}";

            return await GetFromApi<int>(url);
        }

        /// <summary>
        /// Get utxos and current value of specified wallets
        /// </summary>
        /// <param name="addresses">Public addresses of wallets to lookup</param>
        /// <param name="currency">Fiat currency to denominate wallet value</param>
        /// <param name="key">The api key, a funded BCH address</param>
        /// <returns>A list of wallets with utxos and value populated</returns>
        public static async Task<List<Wallet>> GetWalletInfo(List<string> addresses, string currency, string key)
        {
            var addrs = string.Join(",", addresses);

            var url = $"{Constants.ApiUrl}/wallet?addresses={addrs}&currency={currency}&key={key}";

            return await GetFromApi<List<Wallet>>(url);
        }

        /// <summary>
        /// Get current balances of specified wallets
        /// </summary>
        /// <param name="addresses">Public addresses of wallets to lookup</param>
        /// <param name="key">The api key, a funded BCH address</param>
        /// <returns>A list of addresses with their current balances. Empty wallets will not be included.</returns>
        public static async Task<List<KeyValuePair<string, long>>> GetWalletBalances(List<string> addresses, string key)
        {
            var addrs = string.Join(",", addresses);

            var url = $"{Constants.ApiUrl}/wallet/getbalances?key={key}";

            var data = new List<KeyValuePair<string, string>>
            {
                new("addresses", addrs)
            };

            return await PostToApi<List<KeyValuePair<string, long>>>(url, data);
        }

        /// <summary>
        /// Get the list of tx hashes from provided list that exist in the blockchain or mempool
        /// </summary>
        /// <param name="hashes">The list of transaction hashes to be checked</param>
        /// <param name="key">The api key, a funded BCH address</param>
        /// <returns>A list of transaction hashes that exist in the blockchain or mempool</returns>
        public static async Task<List<string>> GetValidTxHashes(List<string> hashes, string key)
        {
            var hashCsv = string.Join(',', hashes);

            var url = $"{Constants.ApiUrl}/transaction/getvalidtxhashes?hashes={hashCsv}&key={key}";

            return await GetFromApi<List<string>>(url);
        }

        /// <summary>
        /// Get the current market value of Bitcoin Cash
        /// </summary>
        /// <param name="currency">Fiat currency to denominate return value</param>
        /// <returns>The current market value of BCH</returns>
        public static async Task<decimal> GetFiatValue(string currency)
        {
            var url = $"{Constants.ApiUrl}/fiat/getvalue?currency={currency}";

            return await GetFromApi<decimal>(url);
        }

        private static async Task<string> GetStringFromApi<T>(string url)
        {
            using var client = new HttpClient();

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API returned Error Status Code: {(int)response.StatusCode}");

            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<T> GetFromApi<T>(string url)
        {
            using var client = new HttpClient();

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API returned Error Status Code: {(int)response.StatusCode}");

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result)!;
        }

        private static async Task<T> PostToApi<T>(string url, List<KeyValuePair<string, string>> data)
        {
            using var client = new HttpClient();

            var stringContent = new FormUrlEncodedContent(data);

            var response = await client.PostAsync(url, stringContent);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API returned Error Status Code: {(int)response.StatusCode}");

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result)!;
        }
    }
}
