namespace BitcoinCash.Models
{
    /// <summary>
    /// Configuration options for the BchClient
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// The default fiat currency for the client
        /// </summary>
        public Currency? Currency { get; set; }
    }
}
