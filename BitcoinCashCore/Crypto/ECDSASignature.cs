using System.Numerics;

namespace BitcoinCash.Core.Crypto
{
    /// <summary>
    /// An ECDSA signature, made up of the two values conventionally called R and S
    /// </summary>
    /// <param name="r">The R value</param>
    /// <param name="s">The S value</param>
    public class ECDSASignature(BigInteger r, BigInteger s)
    {
        /// <summary>
        /// The R value of the signature
        /// </summary>
        public BigInteger R { get; } = r;

        /// <summary>
        /// The S value of the signature
        /// </summary>
        public BigInteger S { get; } = s;

        /// <summary>
        /// Encode the signature using the strict DER form that Bitcoin Cash consensus rules require
        /// </summary>
        /// <returns>The DER encoded signature</returns>
        public byte[] ToDER()
        {
            var r = ToDerInteger(R);
            var s = ToDerInteger(S);

            var result = new byte[6 + r.Length + s.Length];

            result[0] = 0x30;
            result[1] = (byte)(4 + r.Length + s.Length);
            result[2] = 0x02;
            result[3] = (byte)r.Length;
            r.CopyTo(result, 4);
            result[4 + r.Length] = 0x02;
            result[5 + r.Length] = (byte)s.Length;
            s.CopyTo(result, 6 + r.Length);

            return result;
        }

        /// <summary>
        /// Render a value as a DER integer, which is big endian, minimally sized, and
        /// prefixed with a zero byte whenever the leading bit would otherwise mark it negative
        /// </summary>
        private static byte[] ToDerInteger(BigInteger value)
        {
            var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);

            if (bytes.Length == 0)
                return [0x00];

            return (bytes[0] & 0x80) != 0 ? [0x00, .. bytes] : bytes;
        }
    }
}
