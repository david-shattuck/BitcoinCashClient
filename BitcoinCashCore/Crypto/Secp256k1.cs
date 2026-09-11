using System.Numerics;

namespace BitcoinCash.Core.Crypto
{
    /// <summary>
    /// The secp256k1 elliptic curve over which every Bitcoin Cash key pair is defined
    /// </summary>
    public static class Secp256k1
    {
        /// <summary>
        /// The prime that defines the finite field of the curve
        /// </summary>
        public static readonly BigInteger P = Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F");

        /// <summary>
        /// The order of the generator point, one more than the largest valid private key
        /// </summary>
        public static readonly BigInteger N = Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141");

        /// <summary>
        /// Half the curve order, above which the S value of a signature is not canonical
        /// </summary>
        public static readonly BigInteger HalfN = N >> 1;

        private static readonly BigInteger Gx = Parse("79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798");
        private static readonly BigInteger Gy = Parse("483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8");

        /// <summary>
        /// The generator point of the curve
        /// </summary>
        public static readonly ECPoint G = new(Gx, Gy);

        private static BigInteger Parse(string hex) => BigInteger.Parse("0" + hex, System.Globalization.NumberStyles.HexNumber);

        /// <summary>
        /// Multiply the generator point by the given scalar
        /// </summary>
        /// <param name="k">The scalar, typically a private key</param>
        /// <returns>The resulting public point</returns>
        public static ECPoint MultiplyG(BigInteger k) => G.Multiply(k);

        /// <summary>
        /// Reduce a big endian 32 byte buffer into a scalar modulo the curve order
        /// </summary>
        /// <param name="bytes">The buffer to interpret</param>
        /// <returns>The reduced scalar</returns>
        public static BigInteger ToScalar(ReadOnlySpan<byte> bytes)
        {
            var value = ToBigInteger(bytes);

            return value >= N ? value - N : value;
        }

        /// <summary>
        /// Interpret a big endian buffer as a non-negative integer
        /// </summary>
        /// <param name="bytes">The buffer to interpret</param>
        /// <returns>The integer value</returns>
        public static BigInteger ToBigInteger(ReadOnlySpan<byte> bytes) => new(bytes, isUnsigned: true, isBigEndian: true);

        /// <summary>
        /// Render an integer as a fixed width big endian buffer
        /// </summary>
        /// <param name="value">The integer to render</param>
        /// <param name="size">The width of the buffer in bytes</param>
        /// <returns>The big endian representation</returns>
        public static byte[] ToBytes(BigInteger value, int size)
        {
            var result = new byte[size];
            var digits = value.ToByteArray(isUnsigned: true, isBigEndian: true);

            if (digits.Length > size)
                throw new ArgumentException($"Value does not fit in {size} bytes", nameof(value));

            digits.CopyTo(result, size - digits.Length);

            return result;
        }

        /// <summary>
        /// Compute the modular inverse of a value in the given modulus
        /// </summary>
        /// <param name="value">The value to invert</param>
        /// <param name="modulus">The modulus, which must be prime</param>
        /// <returns>The inverse of the value</returns>
        public static BigInteger Inverse(BigInteger value, BigInteger modulus) => BigInteger.ModPow(Mod(value, modulus), modulus - 2, modulus);

        /// <summary>
        /// Reduce a value into the range zero to the modulus, regardless of its sign
        /// </summary>
        /// <param name="value">The value to reduce</param>
        /// <param name="modulus">The modulus</param>
        /// <returns>The non-negative remainder</returns>
        public static BigInteger Mod(BigInteger value, BigInteger modulus)
        {
            var result = value % modulus;

            return result.Sign < 0 ? result + modulus : result;
        }
    }

    /// <summary>
    /// A point on the secp256k1 curve
    /// </summary>
    public readonly struct ECPoint
    {
        /// <summary>
        /// The affine x coordinate of this point
        /// </summary>
        public BigInteger X { get; }

        /// <summary>
        /// The affine y coordinate of this point
        /// </summary>
        public BigInteger Y { get; }

        /// <summary>
        /// True when this is the point at infinity, the identity of the group
        /// </summary>
        public bool IsInfinity { get; }

        /// <summary>
        /// Create a point from its affine coordinates
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public ECPoint(BigInteger x, BigInteger y)
        {
            X = x;
            Y = y;
            IsInfinity = false;
        }

        private ECPoint(bool infinity)
        {
            X = BigInteger.Zero;
            Y = BigInteger.Zero;
            IsInfinity = infinity;
        }

        /// <summary>
        /// The point at infinity
        /// </summary>
        public static ECPoint Infinity { get; } = new(true);

        /// <summary>
        /// Multiply this point by a scalar using a Jacobian double and add ladder
        /// </summary>
        /// <param name="k">The scalar</param>
        /// <returns>The resulting point</returns>
        public ECPoint Multiply(BigInteger k)
        {
            if (k.IsZero || IsInfinity)
                return Infinity;

            var result = Jacobian.Infinity;
            var addend = Jacobian.FromAffine(this);

            for (var bit = 0; bit < k.GetBitLength(); bit++)
            {
                if (!((k >> bit) & BigInteger.One).IsZero)
                    result = result.Add(addend);

                addend = addend.Double();
            }

            return result.ToAffine();
        }

        /// <summary>
        /// Encode this point in the compressed SEC format used by Bitcoin Cash public keys
        /// </summary>
        /// <returns>Thirty three bytes, a parity prefix followed by the x coordinate</returns>
        public byte[] ToCompressedBytes()
        {
            var result = new byte[33];

            result[0] = (byte)(Y.IsEven ? 0x02 : 0x03);
            Secp256k1.ToBytes(X, 32).CopyTo(result, 1);

            return result;
        }

        /// <summary>
        /// A point in Jacobian projective coordinates, where the affine point is (X/Z^2, Y/Z^3).
        /// Working projectively keeps modular inversions out of the multiplication ladder.
        /// </summary>
        private readonly struct Jacobian(BigInteger x, BigInteger y, BigInteger z)
        {
            private readonly BigInteger _x = x;
            private readonly BigInteger _y = y;
            private readonly BigInteger _z = z;

            public static Jacobian Infinity { get; } = new(BigInteger.One, BigInteger.One, BigInteger.Zero);

            public static Jacobian FromAffine(ECPoint point) => point.IsInfinity ? Infinity : new Jacobian(point.X, point.Y, BigInteger.One);

            public ECPoint ToAffine()
            {
                if (_z.IsZero)
                    return ECPoint.Infinity;

                var zInverse = Secp256k1.Inverse(_z, Secp256k1.P);
                var zInverse2 = zInverse * zInverse % Secp256k1.P;
                var zInverse3 = zInverse2 * zInverse % Secp256k1.P;

                return new ECPoint(_x * zInverse2 % Secp256k1.P, _y * zInverse3 % Secp256k1.P);
            }

            public Jacobian Double()
            {
                if (_z.IsZero || _y.IsZero)
                    return Infinity;

                var p = Secp256k1.P;

                var a = _y * _y % p;
                var b = 4 * _x * a % p;
                var c = 8 * a * a % p;
                var d = 3 * _x * _x % p;

                var x = Secp256k1.Mod((d * d) - (2 * b), p);
                var y = Secp256k1.Mod((d * (b - x)) - c, p);
                var z = 2 * _y * _z % p;

                return new Jacobian(x, y, z);
            }

            public Jacobian Add(Jacobian other)
            {
                if (_z.IsZero)
                    return other;

                if (other._z.IsZero)
                    return this;

                var p = Secp256k1.P;

                var z1Squared = _z * _z % p;
                var z2Squared = other._z * other._z % p;

                var u1 = _x * z2Squared % p;
                var u2 = other._x * z1Squared % p;
                var s1 = _y * z2Squared % p * other._z % p;
                var s2 = other._y * z1Squared % p * _z % p;

                if (u1 == u2)
                    return s1 == s2 ? Double() : Infinity;

                var h = Secp256k1.Mod(u2 - u1, p);
                var r = Secp256k1.Mod(s2 - s1, p);

                var hSquared = h * h % p;
                var hCubed = hSquared * h % p;
                var u1HSquared = u1 * hSquared % p;

                var x = Secp256k1.Mod((r * r) - hCubed - (2 * u1HSquared), p);
                var y = Secp256k1.Mod((r * (u1HSquared - x)) - (s1 * hCubed), p);
                var z = _z * other._z % p * h % p;

                return new Jacobian(x, y, z);
            }
        }
    }
}
