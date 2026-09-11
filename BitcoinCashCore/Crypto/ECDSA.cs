namespace BitcoinCash.Core.Crypto
{
    /// <summary>
    /// Produces ECDSA signatures over secp256k1 the way Bitcoin Cash nodes expect them:
    /// with a deterministic nonce, a canonical low S value, and a low R value so that
    /// the DER encoding is always the shorter of the two possible lengths
    /// </summary>
    public static class ECDSA
    {
        /// <summary>
        /// Sign a message hash with the given private key
        /// </summary>
        /// <param name="privateKey">The thirty two byte private key</param>
        /// <param name="hash">The thirty two byte message hash</param>
        /// <returns>A canonical signature over the hash</returns>
        public static ECDSASignature Sign(ReadOnlySpan<byte> privateKey, ReadOnlySpan<byte> hash)
        {
            var signature = Sign(privateKey, hash, null);

            byte[]? extraEntropy = null;
            var counter = 0u;

            while (signature.R > Secp256k1.HalfN)
            {
                extraEntropy ??= new byte[32];

                BitConverter.TryWriteBytes(extraEntropy, ++counter);

                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(extraEntropy, 0, 4);

                signature = Sign(privateKey, hash, extraEntropy);
            }

            return signature;
        }

        private static ECDSASignature Sign(ReadOnlySpan<byte> privateKey, ReadOnlySpan<byte> hash, byte[]? extraEntropy)
        {
            var key = Secp256k1.ToBigInteger(privateKey);
            var message = Secp256k1.ToScalar(hash);

            for (var counter = 0u; ; counter++)
            {
                var nonce = Secp256k1.ToBigInteger(RFC6979.GetNonce(privateKey, hash, extraEntropy, counter));

                if (nonce.IsZero || nonce >= Secp256k1.N)
                    continue;

                var point = Secp256k1.MultiplyG(nonce);

                var r = Secp256k1.Mod(point.X, Secp256k1.N);

                if (r.IsZero)
                    continue;

                var s = Secp256k1.Mod(Secp256k1.Inverse(nonce, Secp256k1.N) * (message + (r * key)), Secp256k1.N);

                if (s.IsZero)
                    continue;

                if (s > Secp256k1.HalfN)
                    s = Secp256k1.N - s;

                return new ECDSASignature(r, s);
            }
        }
    }
}
