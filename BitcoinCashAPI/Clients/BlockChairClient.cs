using BitcoinCash.API.Clients.Interfaces;
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
            _key = _configuration["BlockchairAPIKey"];
        }

        public List<utxo> GetUtxos(List<string> addresses)
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
            catch (Exception)
            {
                return new List<utxo>();
            }
        }
    }
}
