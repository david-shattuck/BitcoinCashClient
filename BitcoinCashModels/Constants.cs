using System.Collections.ObjectModel;

namespace BitcoinCash.Models
{
    /// <summary>
    /// Unchanging variables used in multiple places
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The URL of the BchClient API
        /// </summary>
        public const string ApiUrl = "https://bch-api.com";
        //public const string ApiUrl = "https://localhost:7035";

        /// <summary>
        /// The public BCH address of the developer of this library
        /// </summary>
        public const string DevAddress = "bitcoincash:qr5v4l9gs6cy4avs8gyndgs8a03yu6nazyt4nngdqf";

        /// <summary>
        /// The number of Satoshis in a single BCH coin
        /// </summary>
        public const int SatoshiMultiplier = 100000000;

        /// <summary>
        /// The list of BCH full nodes for tx broadcasting
        /// </summary>
        public static readonly IList<string> Nodes = new ReadOnlyCollection<string>
        (
            new List<string>
            {
                "cashnode.bch.ninja:8333",
                "bch.imaginary.cash:8333",
                "seed.bchd.cash:8333"
            }
        );
    }
}
