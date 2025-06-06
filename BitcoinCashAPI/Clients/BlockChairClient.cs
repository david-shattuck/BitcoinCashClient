﻿using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitcoinCash.API.Clients
{
    public class BlockChairClient : IBlockChairClient
    {
        // https://blockchair.com/api/docs
        private readonly string _key;
        private readonly string _baseUrl = "https://api.blockchair.com/bitcoin-cash";

        private readonly IConfiguration _configuration;

        public BlockChairClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _key = _configuration["BlockchairAPIKey"] ?? "";
        }

        public async Task<List<utxo>?> GetUtxos(List<string> addresses)
        {
            using var client = new HttpClient();
            addresses = addresses.Select(a => a[(a.IndexOf(':') + 1)..]).ToList();
            var addrs = string.Join(",", addresses);

            try
            {
                var response = await client.GetAsync($"{_baseUrl}/dashboards/addresses/{addrs}?key={_key}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return [];

                if (!response.IsSuccessStatusCode)
                    return null;

                var jo = JObject.Parse(await response.Content.ReadAsStringAsync());

                var utxos = jo?["data"]?["utxo"]?.ToString();

                return JsonConvert.DeserializeObject<List<utxo>>(utxos!);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<string>?> GetValidTxHashes(List<string> hashes)
        {
            var validTxHashes = new List<string>();

            int pageSize = 10;
            int pages = (hashes.Count - 1) / pageSize + 1;

            using (var client = new HttpClient())
            {
                for(var i = 0; i < pages; i++)
                {
                    var batchHashes = hashes.Skip(i * pageSize).Take(pageSize).ToList();
                    var hashCsv = string.Join(",", batchHashes);

                    var response = await client.GetAsync($"{_baseUrl}/dashboards/transactions/{hashCsv}?key={_key}");

                    if(!response.IsSuccessStatusCode)
                        return null;

                    var jo = JObject.Parse(await response.Content.ReadAsStringAsync());

                    try
                    {
                        foreach (var hash in batchHashes)
                        {
                            var tx = jo?["data"]?[hash]?.ToString();

                            if (!string.IsNullOrWhiteSpace(tx))
                                validTxHashes.Add(hash);
                        }
                    } catch (Exception) {}
                    
                }
            }

            return validTxHashes;
        }

        public async Task<List<KeyValuePair<string, long>>?> GetWalletBalances(List<string> addresses)
        {
            var walletBalances = new List<KeyValuePair<string, long>>();

            using (var client = new HttpClient())
            {
                addresses = addresses.Select(a => a[(a.IndexOf(':') + 1)..]).ToList();
                var addressCsv = string.Join(",", addresses);

                var stringContent = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("addresses", addressCsv),
                    new KeyValuePair<string, string>("key", _key)
                ]);

                var response = await client.PostAsync($"{_baseUrl}/addresses/balances", stringContent);

                if (!response.IsSuccessStatusCode)
                    return null;

                var jo = JObject.Parse(await response.Content.ReadAsStringAsync());

                try
                {
                    foreach (var address in addresses)
                    {
                        var balance = jo?["data"]?[address]?.ToString();

                        if (!string.IsNullOrWhiteSpace(balance))
                            walletBalances.Add(new KeyValuePair<string, long>(address, Convert.ToInt64(balance)));
                    }
                }
                catch (Exception) { }
            }

            return walletBalances;
        }
    }
}
