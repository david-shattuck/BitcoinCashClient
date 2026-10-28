using Newtonsoft.Json;

namespace BitcoinCash.API.Models
{
    public class Bcmr
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("filemeta")]
        public BcmrFileMeta? FileMeta { get; set; }

        [JsonProperty("token")]
        public BcmrToken? Token { get; set; }

        [JsonProperty("uris")]
        public Dictionary<string, string>? Uris { get; set; }
    }

    public class BcmrFileMeta
    {
        [JsonProperty("actual_hash")]
        public string? ActualHash { get; set; }

        [JsonProperty("expected_hash")]
        public string? ExpectedHash { get; set; }

        [JsonProperty("source")]
        public string? Source { get; set; }
    }

    public class BcmrToken
    {
        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        [JsonProperty("symbol")]
        public string? Symbol { get; set; }
    }
}
