using BitcoinCash.Core.DataEncoders;

namespace BitcoinCash.Core
{
    /// <summary>
    /// A 256 bit hash. The bytes are held in the little endian order used on the wire,
    /// but the string form is the big endian order that block explorers display.
    /// </summary>
    public class uint256 : IEquatable<uint256>
    {
        /// <summary>
        /// The number of bytes in a 256 bit hash
        /// </summary>
        public const int Size = 32;

        private readonly byte[] _value;

        /// <summary>
        /// Create a hash of all zeroes
        /// </summary>
        public uint256() => _value = new byte[Size];

        /// <summary>
        /// Create a hash from its raw bytes
        /// </summary>
        /// <param name="value">Thirty two bytes in the little endian order used on the wire</param>
        public uint256(ReadOnlySpan<byte> value)
        {
            if (value.Length != Size)
                throw new FormatException($"A uint256 must be {Size} bytes");

            _value = value.ToArray();
        }

        /// <summary>
        /// Create a hash from its big endian hexadecimal representation
        /// </summary>
        /// <param name="hex">Sixty four hexadecimal characters</param>
        public uint256(string hex) => _value = FromHex(hex);

        /// <summary>
        /// A hash of all zeroes
        /// </summary>
        public static uint256 Zero { get; } = new();

        /// <summary>
        /// Parse a hash from its big endian hexadecimal representation
        /// </summary>
        /// <param name="hex">Sixty four hexadecimal characters</param>
        /// <returns>The parsed hash</returns>
        public static uint256 Parse(string hex) => new(hex);

        private static byte[] FromHex(string hex)
        {
            hex = hex.Trim();

            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex[2..];

            if (hex.Length != Size * 2)
                throw new FormatException($"A uint256 must be {Size * 2} hexadecimal characters");

            var value = Hex.Decode(hex);

            Array.Reverse(value);

            return value;
        }

        /// <summary>
        /// The raw bytes of the hash, in the little endian order used on the wire
        /// </summary>
        /// <returns>A copy of the thirty two bytes</returns>
        public byte[] ToBytes() => (byte[])_value.Clone();

        /// <summary>
        /// The big endian hexadecimal representation, as displayed by block explorers
        /// </summary>
        /// <returns>Sixty four hexadecimal characters</returns>
        public override string ToString()
        {
            var reversed = (byte[])_value.Clone();

            Array.Reverse(reversed);

            return Hex.Encode(reversed);
        }

        /// <summary>
        /// Compare this hash with another
        /// </summary>
        /// <param name="other">The hash to compare against</param>
        /// <returns>True when both hashes hold the same bytes</returns>
        public bool Equals(uint256? other) => other is not null && _value.AsSpan().SequenceEqual(other._value);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as uint256);

        /// <inheritdoc/>
        public override int GetHashCode() => BitConverter.ToInt32(_value, 0);

        /// <summary>
        /// Compare two hashes
        /// </summary>
        /// <param name="a">The first hash</param>
        /// <param name="b">The second hash</param>
        /// <returns>True when both hashes hold the same bytes</returns>
        public static bool operator ==(uint256? a, uint256? b) => a is null ? b is null : a.Equals(b);

        /// <summary>
        /// Compare two hashes
        /// </summary>
        /// <param name="a">The first hash</param>
        /// <param name="b">The second hash</param>
        /// <returns>True when the hashes differ</returns>
        public static bool operator !=(uint256? a, uint256? b) => !(a == b);
    }
}
