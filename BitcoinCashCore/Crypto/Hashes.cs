using System.Security.Cryptography;

namespace BitcoinCash.Core.Crypto
{
    /// <summary>
    /// The hash functions used throughout the Bitcoin Cash protocol
    /// </summary>
    public static class Hashes
    {
        /// <summary>
        /// Compute the SHA-256 digest of the given data
        /// </summary>
        /// <param name="data">The bytes to hash</param>
        /// <returns>The 32 byte digest</returns>
        public static byte[] SHA256(ReadOnlySpan<byte> data) => System.Security.Cryptography.SHA256.HashData(data);

        /// <summary>
        /// Compute the double SHA-256 digest used for transaction and block hashes
        /// </summary>
        /// <param name="data">The bytes to hash</param>
        /// <returns>The 32 byte digest</returns>
        public static byte[] Hash256(ReadOnlySpan<byte> data) => SHA256(SHA256(data));

        /// <summary>
        /// Compute the RIPEMD-160 of the SHA-256 digest, used for key and script hashes
        /// </summary>
        /// <param name="data">The bytes to hash</param>
        /// <returns>The 20 byte digest</returns>
        public static byte[] Hash160(ReadOnlySpan<byte> data) => RIPEMD160.Hash(SHA256(data));

        /// <summary>
        /// Compute an HMAC using SHA-256 as the underlying hash function
        /// </summary>
        /// <param name="key">The HMAC key</param>
        /// <param name="data">The message to authenticate</param>
        /// <returns>The 32 byte authentication code</returns>
        public static byte[] HMACSHA256(byte[] key, ReadOnlySpan<byte> data) => System.Security.Cryptography.HMACSHA256.HashData(key, data);

        /// <summary>
        /// Fill the given buffer with cryptographically strong random bytes
        /// </summary>
        /// <param name="buffer">The buffer to fill</param>
        public static void GetRandomBytes(Span<byte> buffer) => RandomNumberGenerator.Fill(buffer);
    }
}
