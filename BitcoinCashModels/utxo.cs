namespace BitcoinCash.Models
{
    /// <summary>
    /// An unspent transaction output, including any CashTokens it carries
    /// </summary>
    public class utxo
    {
        /// <summary>
        /// The number of the block that contains this utxo (0 or less if it is still in the mempool)
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// The address of the wallet that owns this utxo, in the form it was requested
        /// </summary>
        public string? address { get; set; }

        /// <summary>
        /// The unique identifier of the transaction that created this utxo
        /// </summary>
        public string? tx_hash { get; set; }

        /// <summary>
        /// Distinguishes this utxo from the other outputs of the tx
        /// </summary>
        public uint tx_pos { get; set; }

        /// <summary>
        /// The number of satoshis held by this utxo
        /// </summary>
        public long value { get; set; }

        /// <summary>
        /// The CashTokens held by this utxo, or null if it holds none
        /// </summary>
        public token_data? token_data { get; set; }
    }

    /// <summary>
    /// The CashTokens carried by a utxo
    /// </summary>
    public class token_data
    {
        /// <summary>
        /// The token category (token id) in hex
        /// </summary>
        public string? category { get; set; }

        /// <summary>
        /// The number of fungible tokens held by the utxo (0 if it only holds an NFT)
        /// </summary>
        public long amount { get; set; }

        /// <summary>
        /// The non-fungible token held by the utxo, or null if it holds none
        /// </summary>
        public token_nft? nft { get; set; }
    }

    /// <summary>
    /// A CashTokens non-fungible token
    /// </summary>
    public class token_nft
    {
        /// <summary>
        /// The NFT's capability: "none", "mutable" or "minting"
        /// </summary>
        public string? capability { get; set; }

        /// <summary>
        /// The NFT's commitment in hex (empty if it has none)
        /// </summary>
        public string? commitment { get; set; }
    }
}
