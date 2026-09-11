using BitcoinCash.Core.DataEncoders;

namespace BitcoinCash.Core
{
    /// <summary>
    /// The hash of a public key, which is what a standard address commits to
    /// </summary>
    public class KeyId
    {
        /// <summary>
        /// The twenty byte hash
        /// </summary>
        public byte[] Hash { get; }

        /// <summary>
        /// Create a key hash from its bytes
        /// </summary>
        /// <param name="hash">The twenty byte hash</param>
        public KeyId(byte[] hash)
        {
            if (hash.Length != 20)
                throw new FormatException("A key hash must be 20 bytes");

            Hash = hash;
        }

        /// <summary>
        /// The script that pays to the holder of this key
        /// </summary>
        public Script ScriptPubKey => Script.PayToPubkeyHash(this);

        /// <inheritdoc/>
        public override string ToString() => Hex.Encode(Hash);
    }

    /// <summary>
    /// The hash of a redeem script, which is what a pay to script hash address commits to
    /// </summary>
    public class ScriptId
    {
        /// <summary>
        /// The twenty byte hash
        /// </summary>
        public byte[] Hash { get; }

        /// <summary>
        /// Create a script hash from its bytes
        /// </summary>
        /// <param name="hash">The twenty byte hash</param>
        public ScriptId(byte[] hash)
        {
            if (hash.Length != 20)
                throw new FormatException("A script hash must be 20 bytes");

            Hash = hash;
        }

        /// <summary>
        /// The script that pays to this redeem script
        /// </summary>
        public Script ScriptPubKey => Script.PayToScriptHash(this);

        /// <inheritdoc/>
        public override string ToString() => Hex.Encode(Hash);
    }
}
