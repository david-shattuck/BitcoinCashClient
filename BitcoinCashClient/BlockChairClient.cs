using BitcoinCash.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitcoinCash
{
    public class BlockChairClient
    {
        // https://blockchair.com/api/docs
        private readonly string _key = "A___uan91DP96zb6dWLxX3gAWbqv8yIt";
        private readonly string _baseUrl = "https://api.blockchair.com/bitcoin-cash";

        public uint GetBalance(string address)
        {
            var utxos = GetUtxos(new List<string> { address });
            return (uint)utxos.Sum(u => u.value);
        }

        private List<utxo> GetUtxos(List<string> addresses)
        {
            var client = new HttpClient();

            addresses = addresses.Select(a => a[(a.IndexOf(':') + 1)..]).ToList();
            var addrs = string.Join(",", addresses);

            try
            {
                var response = client.GetAsync($"{_baseUrl}/dashboards/addresses/{addrs}?key={_key}").Result;

                var jo = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                var utxos = jo["data"]["utxo"].ToString();

                return JsonConvert.DeserializeObject<List<utxo>>(utxos);
            }
            catch (Exception ex)
            {
                return new List<utxo>();
            }
        }
    }
}
