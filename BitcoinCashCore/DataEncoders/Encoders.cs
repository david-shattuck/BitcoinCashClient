using System.Numerics;
using BitcoinCash.Core.Crypto;

namespace BitcoinCash.Core.DataEncoders
{
    /// <summary>
    /// Conversion between bytes and their hexadecimal representation
    /// </summary>
    public static class Hex
    {
        /// <summary>
        /// Render bytes as a lower case hexadecimal string
        /// </summary>
        /// <param name="data">The bytes to encode</param>
        /// <returns>The hexadecimal string</returns>
        public static string Encode(ReadOnlySpan<byte> data) => Convert.ToHexStringLower(data);

        /// <summary>
        /// Parse a hexadecimal string back into bytes
        /// </summary>
        /// <param name="hex">The string to decode</param>
        /// <returns>The decoded bytes</returns>
        public static byte[] Decode(string hex) => Convert.FromHexString(hex);
    }

    /// <summary>
    /// The Base58Check encoding used by legacy addresses and by private keys in
    /// wallet import format, which is Base58 over a payload plus a four byte checksum
    /// </summary>
    public static class Base58
    {
        private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        /// <summary>
        /// Append a checksum to the payload and encode the result in Base58
        /// </summary>
        /// <param name="data">The payload to encode</param>
        /// <returns>The Base58Check string</returns>
        public static string EncodeCheck(ReadOnlySpan<byte> data)
        {
            var checksum = Hashes.Hash256(data);

            return Encode([.. data, .. checksum.AsSpan(0, 4)]);
        }

        /// <summary>
        /// Decode a Base58Check string and verify its checksum
        /// </summary>
        /// <param name="encoded">The string to decode</param>
        /// <returns>The payload without its checksum</returns>
        /// <exception cref="FormatException">The string is not valid Base58 or its checksum does not match</exception>
        public static byte[] DecodeCheck(string encoded)
        {
            var data = Decode(encoded);

            if (data.Length < 4)
                throw new FormatException("Invalid Base58Check string: too short");

            var payload = data.AsSpan(0, data.Length - 4);
            var checksum = Hashes.Hash256(payload);

            for (var i = 0; i < 4; i++)
                if (checksum[i] != data[data.Length - 4 + i])
                    throw new FormatException("Invalid Base58Check string: bad checksum");

            return payload.ToArray();
        }

        /// <summary>
        /// Encode bytes in Base58, without a checksum
        /// </summary>
        /// <param name="data">The bytes to encode</param>
        /// <returns>The Base58 string</returns>
        public static string Encode(ReadOnlySpan<byte> data)
        {
            var leadingZeroes = 0;
            while (leadingZeroes < data.Length && data[leadingZeroes] == 0)
                leadingZeroes++;

            var value = new BigInteger(data, isUnsigned: true, isBigEndian: true);

            var digits = new Stack<char>();
            while (value > 0)
            {
                value = BigInteger.DivRem(value, 58, out var remainder);
                digits.Push(Alphabet[(int)remainder]);
            }

            // A leading zero byte carries no value, so it is encoded as an explicit leading digit
            for (var i = 0; i < leadingZeroes; i++)
                digits.Push(Alphabet[0]);

            return new string([.. digits]);
        }

        /// <summary>
        /// Decode a Base58 string, without verifying a checksum
        /// </summary>
        /// <param name="encoded">The string to decode</param>
        /// <returns>The decoded bytes</returns>
        /// <exception cref="FormatException">The string contains a character outside the Base58 alphabet</exception>
        public static byte[] Decode(string encoded)
        {
            var value = BigInteger.Zero;

            foreach (var character in encoded)
            {
                var digit = Alphabet.IndexOf(character);

                if (digit < 0)
                    throw new FormatException($"Invalid Base58 character: {character}");

                value = (value * 58) + digit;
            }

            byte[] digits = value.IsZero ? [] : value.ToByteArray(isUnsigned: true, isBigEndian: true);

            var leadingZeroes = 0;
            while (leadingZeroes < encoded.Length && encoded[leadingZeroes] == Alphabet[0])
                leadingZeroes++;

            return [.. new byte[leadingZeroes], .. digits];
        }
    }
}
