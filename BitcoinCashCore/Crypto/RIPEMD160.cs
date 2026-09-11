namespace BitcoinCash.Core.Crypto
{
    /// <summary>
    /// A managed implementation of the RIPEMD-160 message digest, which is no longer
    /// provided by the .NET cryptography stack but is required for Bitcoin Cash address hashes.
    /// </summary>
    public static class RIPEMD160
    {
        private static readonly int[] R1 =
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            7, 4, 13, 1, 10, 6, 15, 3, 12, 0, 9, 5, 2, 14, 11, 8,
            3, 10, 14, 4, 9, 15, 8, 1, 2, 7, 0, 6, 13, 11, 5, 12,
            1, 9, 11, 10, 0, 8, 12, 4, 13, 3, 7, 15, 14, 5, 6, 2,
            4, 0, 5, 9, 7, 12, 2, 10, 14, 1, 3, 8, 11, 6, 15, 13
        ];

        private static readonly int[] R2 =
        [
            5, 14, 7, 0, 9, 2, 11, 4, 13, 6, 15, 8, 1, 10, 3, 12,
            6, 11, 3, 7, 0, 13, 5, 10, 14, 15, 8, 12, 4, 9, 1, 2,
            15, 5, 1, 3, 7, 14, 6, 9, 11, 8, 12, 2, 10, 0, 4, 13,
            8, 6, 4, 1, 3, 11, 15, 0, 5, 12, 2, 13, 9, 7, 10, 14,
            12, 15, 10, 4, 1, 5, 8, 7, 6, 2, 13, 14, 0, 3, 9, 11
        ];

        private static readonly int[] S1 =
        [
            11, 14, 15, 12, 5, 8, 7, 9, 11, 13, 14, 15, 6, 7, 9, 8,
            7, 6, 8, 13, 11, 9, 7, 15, 7, 12, 15, 9, 11, 7, 13, 12,
            11, 13, 6, 7, 14, 9, 13, 15, 14, 8, 13, 6, 5, 12, 7, 5,
            11, 12, 14, 15, 14, 15, 9, 8, 9, 14, 5, 6, 8, 6, 5, 12,
            9, 15, 5, 11, 6, 8, 13, 12, 5, 12, 13, 14, 11, 8, 5, 6
        ];

        private static readonly int[] S2 =
        [
            8, 9, 9, 11, 13, 15, 15, 5, 7, 7, 8, 11, 14, 14, 12, 6,
            9, 13, 15, 7, 12, 8, 9, 11, 7, 7, 12, 7, 6, 15, 13, 11,
            9, 7, 15, 11, 8, 6, 6, 14, 12, 13, 5, 14, 13, 13, 7, 5,
            15, 5, 8, 11, 14, 14, 6, 14, 6, 9, 12, 9, 12, 5, 15, 8,
            8, 5, 12, 9, 12, 5, 14, 6, 8, 13, 6, 5, 15, 13, 11, 11
        ];

        private static readonly uint[] K1 = [0x00000000, 0x5A827999, 0x6ED9EBA1, 0x8F1BBCDC, 0xA953FD4E];
        private static readonly uint[] K2 = [0x50A28BE6, 0x5C4DD124, 0x6D703EF3, 0x7A6D76E9, 0x00000000];

        /// <summary>
        /// Compute the RIPEMD-160 digest of the given data
        /// </summary>
        /// <param name="data">The bytes to hash</param>
        /// <returns>The 20 byte digest</returns>
        public static byte[] Hash(ReadOnlySpan<byte> data)
        {
            Span<uint> h = [0x67452301u, 0xEFCDAB89u, 0x98BADCFEu, 0x10325476u, 0xC3D2E1F0u];

            var padded = Pad(data);

            Span<uint> x = stackalloc uint[16];

            for (var block = 0; block < padded.Length; block += 64)
            {
                for (var i = 0; i < 16; i++)
                    x[i] = BitConverter.ToUInt32(padded, block + (i * 4));

                Compress(h, x);
            }

            var result = new byte[20];
            for (var i = 0; i < 5; i++)
                BitConverter.GetBytes(h[i]).CopyTo(result, i * 4);

            return result;
        }

        private static void Compress(Span<uint> h, ReadOnlySpan<uint> x)
        {
            uint a1 = h[0], b1 = h[1], c1 = h[2], d1 = h[3], e1 = h[4];
            uint a2 = h[0], b2 = h[1], c2 = h[2], d2 = h[3], e2 = h[4];

            for (var j = 0; j < 80; j++)
            {
                var round = j / 16;

                var t = RotateLeft(a1 + F(round, b1, c1, d1) + x[R1[j]] + K1[round], S1[j]) + e1;
                a1 = e1; e1 = d1; d1 = RotateLeft(c1, 10); c1 = b1; b1 = t;

                t = RotateLeft(a2 + F(4 - round, b2, c2, d2) + x[R2[j]] + K2[round], S2[j]) + e2;
                a2 = e2; e2 = d2; d2 = RotateLeft(c2, 10); c2 = b2; b2 = t;
            }

            var temp = h[1] + c1 + d2;
            h[1] = h[2] + d1 + e2;
            h[2] = h[3] + e1 + a2;
            h[3] = h[4] + a1 + b2;
            h[4] = h[0] + b1 + c2;
            h[0] = temp;
        }

        private static uint F(int round, uint x, uint y, uint z) => round switch
        {
            0 => x ^ y ^ z,
            1 => (x & y) | (~x & z),
            2 => (x | ~y) ^ z,
            3 => (x & z) | (y & ~z),
            _ => x ^ (y | ~z)
        };

        private static uint RotateLeft(uint value, int bits) => (value << bits) | (value >> (32 - bits));

        private static byte[] Pad(ReadOnlySpan<byte> data)
        {
            var length = ((data.Length + 8) / 64 * 64) + 64;
            var padded = new byte[length];

            data.CopyTo(padded);
            padded[data.Length] = 0x80;

            BitConverter.GetBytes((ulong)data.Length * 8).CopyTo(padded, length - 8);

            return padded;
        }
    }
}
