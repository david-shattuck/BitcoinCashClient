namespace BitcoinCash.Core.DataEncoders
{
    /// <summary>
    /// Whether a hash identifies a public key or a script
    /// </summary>
    public enum CashAddrType
    {
        /// <summary>
        /// The hash of a public key
        /// </summary>
        P2PKH = 0,

        /// <summary>
        /// The hash of a redeem script
        /// </summary>
        P2SH = 8,

        /// <summary>
        /// The hash of a public key, signalling a wallet that can receive CashTokens
        /// </summary>
        TokenP2PKH = 16,

        /// <summary>
        /// The hash of a redeem script, signalling a contract that can receive CashTokens
        /// </summary>
        TokenP2SH = 24
    }

    /// <summary>
    /// The prefix, type and hash carried by a decoded CashAddr address
    /// </summary>
    /// <param name="Prefix">The network prefix, for example bitcoincash</param>
    /// <param name="Type">Whether the hash identifies a public key or a script</param>
    /// <param name="Hash">The hash itself</param>
    public record CashAddrData(string Prefix, CashAddrType Type, byte[] Hash);

    /// <summary>
    /// The CashAddr address format, a base32 encoding with a BCH checksum that
    /// replaced Base58Check addresses on Bitcoin Cash
    /// </summary>
    public static class CashAddr
    {
        private const string Digits = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

        private static readonly long[] Generator = [0x98f2bc8e61, 0x79b76d99e2, 0xf33e5fb3c4, 0xae2eabe2a8, 0x1e4f43e470];

        /// <summary>
        /// Encode a hash as a CashAddr address
        /// </summary>
        /// <param name="prefix">The network prefix, for example bitcoincash</param>
        /// <param name="type">Whether the hash identifies a public key or a script</param>
        /// <param name="hash">The hash to encode</param>
        /// <returns>The address, including its prefix</returns>
        public static string Encode(string prefix, CashAddrType type, byte[] hash)
        {
            var versionByte = (byte)((byte)type + GetHashSizeBits(hash));

            var payload = ConvertBits([versionByte, .. hash], 8, 5, false);

            var checksum = Polymod([.. PrefixToByte5Array(prefix), 0, .. payload, .. new byte[8]]);

            return $"{prefix}:{Base32Encode([.. payload, .. ChecksumToByte5Array(checksum)])}";
        }

        /// <summary>
        /// Decode a CashAddr address into its prefix, type and hash
        /// </summary>
        /// <param name="address">The address to decode, including its prefix</param>
        /// <returns>The decoded address</returns>
        /// <exception cref="FormatException">The address is malformed or its checksum does not match</exception>
        public static CashAddrData Decode(string address)
        {
            var pieces = address.ToLowerInvariant().Split(':');

            if (pieces.Length != 2)
                throw new FormatException($"Missing prefix: {address}");

            var prefix = pieces[0];
            var payload = Base32Decode(pieces[1]);

            if (Polymod([.. PrefixToByte5Array(prefix), 0, .. payload]) != 0)
                throw new FormatException($"Invalid checksum: {address}");

            var payloadData = ConvertBits(payload.AsSpan(0, payload.Length - 8), 5, 8, true);

            var versionByte = payloadData[0];
            var hash = payloadData[1..];

            if (GetHashSize(versionByte) != hash.Length * 8)
                throw new FormatException($"Invalid hash size: {address}");

            return new CashAddrData(prefix, GetType(versionByte), hash);
        }

        private static CashAddrType GetType(byte versionByte) => (versionByte & 120) switch
        {
            0 => CashAddrType.P2PKH,
            8 => CashAddrType.P2SH,
            16 => CashAddrType.TokenP2PKH,
            24 => CashAddrType.TokenP2SH,
            _ => throw new FormatException($"Invalid address type in version byte: {versionByte}")
        };

        private static byte GetHashSizeBits(byte[] hash) => (hash.Length * 8) switch
        {
            160 => 0,
            192 => 1,
            224 => 2,
            256 => 3,
            320 => 4,
            384 => 5,
            448 => 6,
            512 => 7,
            _ => throw new FormatException($"Invalid hash size: {hash.Length}")
        };

        private static int GetHashSize(byte versionByte) => (versionByte & 7) switch
        {
            0 => 160,
            1 => 192,
            2 => 224,
            3 => 256,
            4 => 320,
            5 => 384,
            6 => 448,
            _ => 512
        };

        /// <summary>
        /// The BCH checksum that protects a CashAddr address against typing errors
        /// </summary>
        private static long Polymod(byte[] data)
        {
            long checksum = 1;

            foreach (var value in data)
            {
                var topBits = checksum >> 35;

                checksum = ((checksum & 0x07ffffffff) << 5) ^ value;

                for (var j = 0; j < Generator.Length; ++j)
                    if (((topBits >> j) & 1) == 1)
                        checksum ^= Generator[j];
            }

            return checksum ^ 1;
        }

        private static byte[] PrefixToByte5Array(string prefix)
        {
            var result = new byte[prefix.Length];

            for (var i = 0; i < prefix.Length; i++)
                result[i] = (byte)(prefix[i] & 31);

            return result;
        }

        private static byte[] ChecksumToByte5Array(long checksum)
        {
            var result = new byte[8];

            for (var i = 0; i < 8; ++i)
            {
                result[7 - i] = (byte)(checksum & 31);
                checksum >>= 5;
            }

            return result;
        }

        /// <summary>
        /// Regroup the bits of the given values into differently sized units, padding
        /// with zeroes at the end unless strict mode forbids it
        /// </summary>
        private static byte[] ConvertBits(ReadOnlySpan<byte> data, int from, int to, bool strict)
        {
            var length = strict ? data.Length * from / to : (int)Math.Ceiling(data.Length * from / (double)to);
            var mask = (1 << to) - 1;

            var result = new byte[length];
            var index = 0;
            var accumulator = 0;
            var bits = 0;

            foreach (var value in data)
            {
                if (value >> from != 0)
                    throw new FormatException($"Invalid value: {value}");

                accumulator = (accumulator << from) | value;
                bits += from;

                while (bits >= to)
                {
                    bits -= to;
                    result[index++] = (byte)((accumulator >> bits) & mask);
                }
            }

            if (!strict)
            {
                if (bits > 0)
                    result[index] = (byte)((accumulator << (to - bits)) & mask);
            }
            else if (bits >= from || ((accumulator << (to - bits)) & mask) != 0)
                throw new FormatException($"Input cannot be converted to {to} bits without padding");

            return result;
        }

        private static string Base32Encode(byte[] data)
        {
            var result = new char[data.Length];

            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] >= 32)
                    throw new FormatException($"Invalid value: {data[i]}");

                result[i] = Digits[data[i]];
            }

            return new string(result);
        }

        private static byte[] Base32Decode(string encoded)
        {
            if (encoded.Length == 0)
                throw new FormatException("Invalid encoded string");

            var result = new byte[encoded.Length];

            for (var i = 0; i < encoded.Length; i++)
            {
                var digit = Digits.IndexOf(encoded[i]);

                if (digit < 0)
                    throw new FormatException($"Invalid character: {encoded[i]}");

                result[i] = (byte)digit;
            }

            return result;
        }
    }
}
