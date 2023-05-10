namespace BitcoinCash.Models
{
    /// <summary>
    /// An unspent transaction output
    /// </summary>
    public class utxo
    {
        /// <summary>
        /// The number of the block that contains this utxo
        /// </summary>
        public int block_id { get; set; }

        /// <summary>
        /// The public address of the wallet that owns this utxo
        /// </summary>
        public string? address { get; set; }

        /// <summary>
        /// The unique identifier of the transaction that created this utxo
        /// </summary>
        public string? transaction_hash { get; set; }

        /// <summary>
        /// Distinguishes this utxo from the other outputs of the tx
        /// </summary>
        public uint index { get; set; }

        /// <summary>
        /// The number of satoshis held by this utxo
        /// </summary>
        public long value { get; set; }

        /// <summary>
        /// The public address in the CashAddr format
        /// </summary>
        public string cashAddr
        {
            get
            {
                return $"bitcoincash:{address}";
            }
        }
    }
}
