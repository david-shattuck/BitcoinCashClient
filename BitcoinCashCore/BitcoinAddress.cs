using BitcoinCash.Core.DataEncoders;

namespace BitcoinCash.Core
{
    /// <summary>
    /// The kind of output an address pays to
    /// </summary>
    public enum ScriptPubKeyType
    {
        /// <summary>
        /// A pay to public key hash output
        /// </summary>
        Legacy
    }

    /// <summary>
    /// A Bitcoin Cash address
    /// </summary>
    public abstract class BitcoinAddress
    {
        /// <summary>
        /// Create an address that renders as the given string on the given network
        /// </summary>
        /// <param name="value">The textual form of the address</param>
        /// <param name="network">The network the address belongs to</param>
        protected BitcoinAddress(string value, Network network)
        {
            Value = value;
            Network = network;
        }

        /// <summary>
        /// The textual form of this address
        /// </summary>
        protected string Value { get; }

        /// <summary>
        /// The network this address belongs to
        /// </summary>
        public Network Network { get; }

        /// <summary>
        /// The script that pays to this address
        /// </summary>
        public abstract Script ScriptPubKey { get; }

        /// <summary>
        /// Parse an address, accepting either the CashAddr or the legacy Base58Check form
        /// </summary>
        /// <param name="address">The address to parse</param>
        /// <param name="network">The network the address must belong to</param>
        /// <returns>The parsed address</returns>
        /// <exception cref="FormatException">The address is not valid on the given network</exception>
        public static BitcoinAddress Create(string address, Network network)
        {
            ArgumentNullException.ThrowIfNull(address);
            ArgumentNullException.ThrowIfNull(network);

            return network.ParseAddress(address.Trim());
        }

        /// <inheritdoc/>
        public override string ToString() => Value;
    }

    /// <summary>
    /// An address that pays to the holder of a public key
    /// </summary>
    public class BitcoinPubKeyAddress : BitcoinAddress
    {
        /// <summary>
        /// Create an address for the given public key hash
        /// </summary>
        /// <param name="value">The textual form of the address</param>
        /// <param name="keyId">The public key hash the address commits to</param>
        /// <param name="network">The network the address belongs to</param>
        public BitcoinPubKeyAddress(string value, KeyId keyId, Network network) : base(value, network) => KeyId = keyId;

        /// <summary>
        /// The public key hash this address commits to
        /// </summary>
        public KeyId KeyId { get; }

        /// <inheritdoc/>
        public override Script ScriptPubKey => KeyId.ScriptPubKey;
    }

    /// <summary>
    /// An address that pays to a redeem script
    /// </summary>
    public class BitcoinScriptAddress : BitcoinAddress
    {
        /// <summary>
        /// Create an address for the given script hash
        /// </summary>
        /// <param name="value">The textual form of the address</param>
        /// <param name="scriptId">The script hash the address commits to</param>
        /// <param name="network">The network the address belongs to</param>
        public BitcoinScriptAddress(string value, ScriptId scriptId, Network network) : base(value, network) => ScriptId = scriptId;

        /// <summary>
        /// The script hash this address commits to
        /// </summary>
        public ScriptId ScriptId { get; }

        /// <inheritdoc/>
        public override Script ScriptPubKey => ScriptId.ScriptPubKey;
    }

    /// <summary>
    /// A private key in wallet import format, the Base58Check encoding used to move
    /// keys between wallets
    /// </summary>
    public class BitcoinSecret
    {
        private readonly string _wif;

        /// <summary>
        /// Wrap a private key for the given network
        /// </summary>
        /// <param name="key">The private key</param>
        /// <param name="network">The network the key belongs to</param>
        public BitcoinSecret(Key key, Network network)
        {
            PrivateKey = key;
            Network = network;

            _wif = Base58.EncodeCheck([.. network.SecretKeyVersion, .. key.ToBytes(), 0x01]);
        }

        /// <summary>
        /// Read a private key from its wallet import format representation
        /// </summary>
        /// <param name="wif">The key in wallet import format</param>
        /// <param name="network">The network the key must belong to</param>
        /// <exception cref="FormatException">The string is not a valid key for the given network</exception>
        public BitcoinSecret(string wif, Network network)
        {
            var data = Base58.DecodeCheck(wif);

            var version = network.SecretKeyVersion;

            if (data.Length < version.Length || !data.AsSpan(0, version.Length).SequenceEqual(version))
                throw new FormatException("The private key is not valid on this network");

            var body = data.AsSpan(version.Length);

            // A trailing 0x01 marks a key whose public key is used in compressed form,
            // which every modern wallet does
            if (body.Length == 33 && body[32] == 0x01)
                body = body[..32];
            else if (body.Length != 32)
                throw new FormatException("The private key is malformed");

            PrivateKey = new Key(body.ToArray());
            Network = network;
            _wif = wif;
        }

        /// <summary>
        /// The private key this secret wraps
        /// </summary>
        public Key PrivateKey { get; }

        /// <summary>
        /// The network this key belongs to
        /// </summary>
        public Network Network { get; }

        /// <summary>
        /// The public key derived from this private key
        /// </summary>
        public PubKey PubKey => PrivateKey.PubKey;

        /// <summary>
        /// The address of this key
        /// </summary>
        /// <param name="type">The kind of address to build</param>
        /// <returns>The address</returns>
        public BitcoinAddress GetAddress(ScriptPubKeyType type) => PrivateKey.PubKey.GetAddress(type, Network);

        /// <inheritdoc/>
        public override string ToString() => _wif;
    }
}
