namespace BitcoinCash.Core
{
    /// <summary>
    /// Writes the little endian primitives and variable length encodings that make up
    /// the Bitcoin Cash wire format
    /// </summary>
    public sealed class BitcoinWriter
    {
        private readonly MemoryStream _stream = new();

        /// <summary>
        /// Everything written so far
        /// </summary>
        /// <returns>The serialized bytes</returns>
        public byte[] ToBytes() => _stream.ToArray();

        /// <summary>
        /// Write a single byte
        /// </summary>
        /// <param name="value">The byte to write</param>
        public void Write(byte value) => _stream.WriteByte(value);

        /// <summary>
        /// Write bytes verbatim
        /// </summary>
        /// <param name="value">The bytes to write</param>
        public void Write(ReadOnlySpan<byte> value) => _stream.Write(value);

        /// <summary>
        /// Write an unsigned 32 bit integer in little endian order
        /// </summary>
        /// <param name="value">The value to write</param>
        public void WriteUInt32(uint value)
        {
            Span<byte> buffer = stackalloc byte[4];

            BitConverter.TryWriteBytes(buffer, value);

            if (!BitConverter.IsLittleEndian)
                buffer.Reverse();

            _stream.Write(buffer);
        }

        /// <summary>
        /// Write an unsigned 64 bit integer in little endian order
        /// </summary>
        /// <param name="value">The value to write</param>
        public void WriteUInt64(ulong value)
        {
            Span<byte> buffer = stackalloc byte[8];

            BitConverter.TryWriteBytes(buffer, value);

            if (!BitConverter.IsLittleEndian)
                buffer.Reverse();

            _stream.Write(buffer);
        }

        /// <summary>
        /// Write a signed 64 bit integer in little endian order
        /// </summary>
        /// <param name="value">The value to write</param>
        public void WriteInt64(long value) => WriteUInt64((ulong)value);

        /// <summary>
        /// Write a hash in the byte order it takes on the wire
        /// </summary>
        /// <param name="value">The hash to write</param>
        public void Write(uint256 value) => Write(value.ToBytes());

        /// <summary>
        /// Write a length using the compact size encoding
        /// </summary>
        /// <param name="value">The length to write</param>
        public void WriteVarInt(ulong value)
        {
            if (value < 0xFD)
            {
                Write((byte)value);
            }
            else if (value <= 0xFFFF)
            {
                Write((byte)0xFD);
                Write((byte)value);
                Write((byte)(value >> 8));
            }
            else if (value <= 0xFFFFFFFF)
            {
                Write((byte)0xFE);
                WriteUInt32((uint)value);
            }
            else
            {
                Write((byte)0xFF);
                WriteUInt64(value);
            }
        }

        /// <summary>
        /// Write bytes preceded by their length
        /// </summary>
        /// <param name="value">The bytes to write</param>
        public void WriteVarBytes(ReadOnlySpan<byte> value)
        {
            WriteVarInt((ulong)value.Length);
            Write(value);
        }

        /// <summary>
        /// Write a script, preceded by its length
        /// </summary>
        /// <param name="value">The script to write</param>
        public void Write(Script value) => WriteVarBytes(value.ToBytes());

        /// <summary>
        /// Write a string as ASCII bytes preceded by their length
        /// </summary>
        /// <param name="value">The string to write</param>
        public void WriteVarString(string value) => WriteVarBytes(System.Text.Encoding.ASCII.GetBytes(value));
    }
}
