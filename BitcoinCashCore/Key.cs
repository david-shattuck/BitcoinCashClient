using System.Numerics;
using BitcoinCash.Core.Crypto;

namespace BitcoinCash.Core
{
    /// <summary>
    /// A public key on the secp256k1 curve
    /// </summary>
    public class PubKey
    {
        private readonly byte[] _compressed;

        /// <summary>
        /// Create a public key from its compressed encoding
        /// </summary>
        /// <param name="compressed">Thirty three bytes in compressed SEC format</param>
        public PubKey(byte[] compressed)
        {
            if (compressed.Length != 33)
                throw new FormatException("A compressed public key must be 33 bytes");

            _compressed = compressed;
        }

        /// <summary>
        /// The compressed encoding of this key
        /// </summary>
        /// <returns>A copy of the thirty three bytes</returns>
        public byte[] ToBytes() => (byte[])_compressed.Clone();

        /// <summary>
        /// The hash of this key, which is what an address commits to
        /// </summary>
        public KeyId Hash => new(Hashes.Hash160(_compressed));

        /// <summary>
        /// The script that pays to the holder of this key
        /// </summary>
        public Script ScriptPubKey => Hash.ScriptPubKey;

        /// <summary>
        /// The address of this key on the given network
        /// </summary>
        /// <param name="type">The kind of address to build</param>
        /// <param name="network">The network the address belongs to</param>
        /// <returns>The address</returns>
        public BitcoinAddress GetAddress(ScriptPubKeyType type, Network network)
        {
            if (type != ScriptPubKeyType.Legacy)
                throw new NotSupportedException($"Bitcoin Cash does not support {type} addresses");

            return network.CreateP2PKHAddress(Hash);
        }
    }

    /// <summary>
    /// A private key on the secp256k1 curve
    /// </summary>
    public class Key
    {
        private readonly byte[] _value;

        /// <summary>
        /// Generate a new random private key
        /// </summary>
        public Key()
        {
            var value = new byte[32];

            do
            {
                Hashes.GetRandomBytes(value);
            }
            while (!IsValid(value));

            _value = value;
        }

        /// <summary>
        /// Load a private key from its raw bytes
        /// </summary>
        /// <param name="value">The thirty two byte key</param>
        public Key(byte[] value)
        {
            if (value.Length != 32 || !IsValid(value))
                throw new FormatException("Invalid private key");

            _value = value;
        }

        private static bool IsValid(byte[] value)
        {
            var candidate = Secp256k1.ToBigInteger(value);

            return candidate > BigInteger.Zero && candidate < Secp256k1.N;
        }

        /// <summary>
        /// The raw bytes of this key
        /// </summary>
        /// <returns>A copy of the thirty two bytes</returns>
        public byte[] ToBytes() => (byte[])_value.Clone();

        private PubKey? _pubKey;

        /// <summary>
        /// The public key derived from this private key
        /// </summary>
        public PubKey PubKey => _pubKey ??= new PubKey(Secp256k1.MultiplyG(Secp256k1.ToBigInteger(_value)).ToCompressedBytes());

        /// <summary>
        /// Sign a message hash with this key
        /// </summary>
        /// <param name="hash">The hash to sign</param>
        /// <returns>A canonical signature over the hash</returns>
        public ECDSASignature Sign(uint256 hash) => ECDSA.Sign(_value, hash.ToBytes());

        /// <summary>
        /// Wrap this key in its wallet import format representation for the given network
        /// </summary>
        /// <param name="network">The network the key belongs to</param>
        /// <returns>The key in wallet import format</returns>
        public BitcoinSecret GetBitcoinSecret(Network network) => new(this, network);

        /// <summary>
        /// The address of this key on the given network
        /// </summary>
        /// <param name="type">The kind of address to build</param>
        /// <param name="network">The network the address belongs to</param>
        /// <returns>The address</returns>
        public BitcoinAddress GetAddress(ScriptPubKeyType type, Network network) => PubKey.GetAddress(type, network);
    }
}
