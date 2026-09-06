namespace BitcoinCash.Models
{
    /// <summary>
    /// The amount and recipient for a single tx output
    /// </summary>
    public class ApiKey
    {
        /// <summary>
        /// The secret value to pass in with any API call to identify the caller
        /// </summary>
        public required string Secret { get; set; }

        /// <summary>
        /// The public BCH address to send funds to in order to refill the API key
        /// </summary>
        public required string Address { get; set; }

        /// <summary>
        /// The number of API calls remaining for the key
        /// </summary>
        public int RemainingCalls { get; set; }
    }
}
