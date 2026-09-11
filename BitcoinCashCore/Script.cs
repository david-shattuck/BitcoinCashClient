using BitcoinCash.Core.DataEncoders;

namespace BitcoinCash.Core
{
    /// <summary>
    /// A Bitcoin Cash script, held as the raw bytes that go on the wire
    /// </summary>
    public class Script
    {
        private const byte OP_DUP = 0x76;
        private const byte OP_EQUALVERIFY = 0x88;
        private const byte OP_HASH160 = 0xa9;
        private const byte OP_CHECKSIG = 0xac;
        private const byte OP_EQUAL = 0x87;

        private readonly byte[] _script;

        /// <summary>
        /// Create an empty script
        /// </summary>
        public Script() => _script = [];

        /// <summary>
        /// Create a script from its raw bytes
        /// </summary>
        /// <param name="script">The compiled script</param>
        public Script(ReadOnlySpan<byte> script) => _script = script.ToArray();

        /// <summary>
        /// The number of bytes in this script
        /// </summary>
        public int Length => _script.Length;

        /// <summary>
        /// The raw bytes of this script
        /// </summary>
        /// <returns>A copy of the compiled script</returns>
        public byte[] ToBytes() => (byte[])_script.Clone();

        /// <summary>
        /// Build the script that pays to the holder of the given public key hash
        /// </summary>
        /// <param name="keyId">The hash of the recipient public key</param>
        /// <returns>A pay to public key hash script</returns>
        public static Script PayToPubkeyHash(KeyId keyId) =>
            new([OP_DUP, OP_HASH160, (byte)keyId.Hash.Length, .. keyId.Hash, OP_EQUALVERIFY, OP_CHECKSIG]);

        /// <summary>
        /// Build the script that pays to the given script hash
        /// </summary>
        /// <param name="scriptId">The hash of the redeem script</param>
        /// <returns>A pay to script hash script</returns>
        public static Script PayToScriptHash(ScriptId scriptId) =>
            new([OP_HASH160, (byte)scriptId.Hash.Length, .. scriptId.Hash, OP_EQUAL]);

        /// <summary>
        /// Build the signature script that unlocks a pay to public key hash output
        /// </summary>
        /// <param name="signature">The DER signature followed by its hash type byte</param>
        /// <param name="publicKey">The public key matching the hash in the output</param>
        /// <returns>A signature script</returns>
        public static Script PayToPubkeyHashSignature(byte[] signature, byte[] publicKey) =>
            new([.. PushData(signature), .. PushData(publicKey)]);

        /// <summary>
        /// Encode a value so that running the script pushes it onto the stack. Only the
        /// short forms are needed here, because signatures and public keys are never large.
        /// </summary>
        private static byte[] PushData(byte[] data)
        {
            if (data.Length >= 0x4c)
                throw new ArgumentException("Only small pushes are supported", nameof(data));

            return [(byte)data.Length, .. data];
        }

        /// <inheritdoc/>
        public override string ToString() => Hex.Encode(_script);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Script other && _script.AsSpan().SequenceEqual(other._script);

        /// <inheritdoc/>
        public override int GetHashCode() => ToString().GetHashCode();
    }
}
