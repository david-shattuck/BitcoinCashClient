namespace BitcoinCash.Models
{
    /// <summary>
    /// A summary of the fungible CashTokens of one category held by a wallet
    /// </summary>
    public class FungibleToken
    {
        /// <summary>
        /// The token's name from its BCMR metadata, or null if no metadata was found
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The token category (token id) in hex
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// The total number of fungible tokens of this category held by the wallet, in base units (not adjusted for decimal places)
        /// </summary>
        public long Balance { get; set; }

        /// <summary>
        /// The number of decimal places the token is divided into, from its BCMR metadata (0 if no metadata was found)
        /// </summary>
        public int DecimalPlaces { get; set; }

        /// <summary>
        /// The human-readable balance: Balance shifted by DecimalPlaces (e.g. a Balance of 12345 with 2 decimal places is 123.45)
        /// </summary>
        public decimal DisplayBalance
        {
            get
            {
                var places = Math.Clamp(DecimalPlaces, 0, 28);

                decimal divisor = 1;
                for (var i = 0; i < places; i++)
                    divisor *= 10;

                return Balance / divisor;
            }
        }
    }
}
