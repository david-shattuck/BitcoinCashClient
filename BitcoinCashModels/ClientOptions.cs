namespace BitcoinCash.Models
{
    /// <summary>
    /// Configuration options for the BchClient
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// The key required to request blockchain data from the api
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// The default fiat currency for the client
        /// </summary>
        public Currency? Currency { get; set; }
    }
}
