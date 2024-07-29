namespace BitcoinCash.Models
{
    /// <summary>
    /// The amount and recipient for a single tx output
    /// </summary>
    public class Send
    {
        /// <summary>
        /// A valid BCH address - the recipient of this send
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// The amount to send
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// The currency units of the send amount
        /// </summary>
        public Currency? Currency { get; set; }        
    }
}
