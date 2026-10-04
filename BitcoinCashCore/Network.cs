using BitcoinCash.Core.DataEncoders;

namespace BitcoinCash.Core
{
    /// <summary>
    /// The parameters that distinguish one Bitcoin Cash network from another
    /// </summary>
    public class Network
    {
        /// <summary>
        /// Describe a network
        /// </summary>
        /// <param name="name">The name of the network</param>
        /// <param name="cashAddrPrefix">The prefix that CashAddr addresses carry on this network</param>
        /// <param name="pubKeyAddressVersion">The Base58Check version bytes of a legacy public key address</param>
        /// <param name="scriptAddressVersion">The Base58Check version bytes of a legacy script address</param>
        /// <param name="secretKeyVersion">The Base58Check version bytes of a private key</param>
        /// <param name="magic">The four bytes that begin every peer to peer message</param>
        /// <param name="defaultPort">The port peers listen on</param>
        public Network(
            string name,
            string cashAddrPrefix,
            byte[] pubKeyAddressVersion,
            byte[] scriptAddressVersion,
            byte[] secretKeyVersion,
            uint magic,
            int defaultPort)
        {
            Name = name;
            CashAddrPrefix = cashAddrPrefix;
            PubKeyAddressVersion = pubKeyAddressVersion;
            ScriptAddressVersion = scriptAddressVersion;
            SecretKeyVersion = secretKeyVersion;
            Magic = magic;
            DefaultPort = defaultPort;
        }

        /// <summary>
        /// The name of this network
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The prefix that CashAddr addresses carry on this network
        /// </summary>
        public string CashAddrPrefix { get; }

        /// <summary>
        /// The Base58Check version bytes of a legacy public key address
        /// </summary>
        public byte[] PubKeyAddressVersion { get; }

        /// <summary>
        /// The Base58Check version bytes of a legacy script address
        /// </summary>
        public byte[] ScriptAddressVersion { get; }

        /// <summary>
        /// The Base58Check version bytes of a private key
        /// </summary>
        public byte[] SecretKeyVersion { get; }

        /// <summary>
        /// The four bytes that begin every peer to peer message on this network
        /// </summary>
        public uint Magic { get; }

        /// <summary>
        /// The port peers listen on
        /// </summary>
        public int DefaultPort { get; }

        /// <summary>
        /// The highest peer to peer protocol version this library speaks
        /// </summary>
        public uint MaxP2PVersion { get; } = 70016;

        /// <summary>
        /// Build the address that pays to the holder of the given public key hash
        /// </summary>
        /// <param name="keyId">The public key hash</param>
        /// <returns>A CashAddr address</returns>
        public BitcoinPubKeyAddress CreateP2PKHAddress(KeyId keyId) =>
            new(CashAddr.Encode(CashAddrPrefix, CashAddrType.P2PKH, keyId.Hash), keyId, this);

        /// <summary>
        /// Build the token-aware address that pays to the holder of the given public key hash.
        /// It pays to the same script as the P2PKH address but tells senders the wallet can receive CashTokens
        /// </summary>
        /// <param name="keyId">The public key hash</param>
        /// <returns>A token-aware CashAddr address</returns>
        public BitcoinPubKeyAddress CreateTokenP2PKHAddress(KeyId keyId) =>
            new(CashAddr.Encode(CashAddrPrefix, CashAddrType.TokenP2PKH, keyId.Hash), keyId, this);

        /// <summary>
        /// Build the address that pays to the given script hash
        /// </summary>
        /// <param name="scriptId">The script hash</param>
        /// <returns>A CashAddr address</returns>
        public BitcoinScriptAddress CreateP2SHAddress(ScriptId scriptId) =>
            new(CashAddr.Encode(CashAddrPrefix, CashAddrType.P2SH, scriptId.Hash), scriptId, this);

        /// <summary>
        /// Build the token-aware address that pays to the given script hash
        /// </summary>
        /// <param name="scriptId">The script hash</param>
        /// <returns>A token-aware CashAddr address</returns>
        public BitcoinScriptAddress CreateTokenP2SHAddress(ScriptId scriptId) =>
            new(CashAddr.Encode(CashAddrPrefix, CashAddrType.TokenP2SH, scriptId.Hash), scriptId, this);

        /// <summary>
        /// Derive the token-aware form of an address. Both forms commit to the same hash,
        /// so no private key is needed
        /// </summary>
        /// <param name="address">An address in any form accepted by <see cref="ParseAddress"/></param>
        /// <returns>The token-aware CashAddr address</returns>
        /// <exception cref="FormatException">The address is not valid on this network</exception>
        public BitcoinAddress GetTokenAddress(string address) => ParseAddress(address) switch
        {
            BitcoinPubKeyAddress pubKeyAddress => CreateTokenP2PKHAddress(pubKeyAddress.KeyId),
            BitcoinScriptAddress scriptAddress => CreateTokenP2SHAddress(scriptAddress.ScriptId),
            var other => throw new FormatException($"Unsupported address {other}")
        };

        /// <summary>
        /// Derive the standard, non-token-aware form of an address
        /// </summary>
        /// <param name="address">An address in any form accepted by <see cref="ParseAddress"/></param>
        /// <returns>The standard CashAddr address</returns>
        /// <exception cref="FormatException">The address is not valid on this network</exception>
        public BitcoinAddress GetStandardAddress(string address) => ParseAddress(address) switch
        {
            BitcoinPubKeyAddress pubKeyAddress => CreateP2PKHAddress(pubKeyAddress.KeyId),
            BitcoinScriptAddress scriptAddress => CreateP2SHAddress(scriptAddress.ScriptId),
            var other => throw new FormatException($"Unsupported address {other}")
        };

        /// <summary>
        /// Parse an address in either the CashAddr or the legacy Base58Check form
        /// </summary>
        /// <param name="address">The address to parse</param>
        /// <returns>The parsed address</returns>
        /// <exception cref="FormatException">The address is not valid on this network</exception>
        public BitcoinAddress ParseAddress(string address)
        {
            if (address.StartsWith($"{CashAddrPrefix}:", StringComparison.OrdinalIgnoreCase))
            {
                var decoded = CashAddr.Decode(address);

                return decoded.Type is CashAddrType.P2PKH or CashAddrType.TokenP2PKH
                    ? new BitcoinPubKeyAddress(address, new KeyId(decoded.Hash), this)
                    : new BitcoinScriptAddress(address, new ScriptId(decoded.Hash), this);
            }

            var data = Base58.DecodeCheck(address);

            if (StartsWith(data, PubKeyAddressVersion))
                return new BitcoinPubKeyAddress(address, new KeyId(data[PubKeyAddressVersion.Length..]), this);

            if (StartsWith(data, ScriptAddressVersion))
                return new BitcoinScriptAddress(address, new ScriptId(data[ScriptAddressVersion.Length..]), this);

            throw new FormatException($"Invalid address {address}");
        }

        private static bool StartsWith(byte[] data, byte[] prefix) =>
            data.Length > prefix.Length && data.AsSpan(0, prefix.Length).SequenceEqual(prefix);
    }

    /// <summary>
    /// The Bitcoin Cash networks
    /// </summary>
    public static class Networks
    {
        /// <summary>
        /// The live Bitcoin Cash network.
        /// </summary>
        public static Network Mainnet { get; } = new(
            name: "bch-main",
            cashAddrPrefix: "bitcoincash",
            pubKeyAddressVersion: [28],
            scriptAddressVersion: [40],
            secretKeyVersion: [128],
            magic: 0xe8f3e1e3,
            defaultPort: 8333);

        /// <summary>
        /// The Bitcoin Cash test network
        /// </summary>
        public static Network Testnet { get; } = new(
            name: "bch-test",
            cashAddrPrefix: "bchtest",
            pubKeyAddressVersion: [111],
            scriptAddressVersion: [196],
            secretKeyVersion: [239],
            magic: 0xf4f3e5f4,
            defaultPort: 18333);

        /// <summary>
        /// A private Bitcoin Cash network for local testing
        /// </summary>
        public static Network Regtest { get; } = new(
            name: "bch-reg",
            cashAddrPrefix: "bchreg",
            pubKeyAddressVersion: [111],
            scriptAddressVersion: [196],
            secretKeyVersion: [239],
            magic: 0xfabfb5da,
            defaultPort: 18444);
    }
}
