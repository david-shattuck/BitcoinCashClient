namespace BitcoinCash.Core
{
    /// <summary>
    /// The unit in which an amount of Bitcoin Cash is expressed
    /// </summary>
    public enum MoneyUnit
    {
        /// <summary>
        /// One whole Bitcoin Cash, which is one hundred million satoshis
        /// </summary>
        BCH = 100000000,

        /// <summary>
        /// One thousandth of a Bitcoin Cash
        /// </summary>
        MilliBCH = 100000,

        /// <summary>
        /// One millionth of a Bitcoin Cash
        /// </summary>
        Bit = 100,

        /// <summary>
        /// The smallest indivisible unit
        /// </summary>
        Satoshi = 1
    }

    /// <summary>
    /// An amount of Bitcoin Cash, held internally as a whole number of satoshis
    /// </summary>
    public class Money : IEquatable<Money>, IComparable<Money>
    {
        /// <summary>
        /// The amount in satoshis
        /// </summary>
        public long Satoshi { get; }

        /// <summary>
        /// Create an amount from a number of satoshis
        /// </summary>
        /// <param name="satoshis">The amount in satoshis</param>
        public Money(long satoshis) => Satoshi = satoshis;

        /// <summary>
        /// Create an amount from a quantity of the given unit
        /// </summary>
        /// <param name="amount">The quantity</param>
        /// <param name="unit">The unit the quantity is expressed in</param>
        public Money(long amount, MoneyUnit unit) => Satoshi = checked(amount * (long)unit);

        /// <summary>
        /// An amount of nothing
        /// </summary>
        public static Money Zero { get; } = new(0);

        /// <summary>
        /// Add two amounts together
        /// </summary>
        /// <param name="a">The first amount</param>
        /// <param name="b">The second amount</param>
        /// <returns>The sum</returns>
        public static Money operator +(Money a, Money b) => new(a.Satoshi + b.Satoshi);

        /// <summary>
        /// Subtract one amount from another
        /// </summary>
        /// <param name="a">The amount to subtract from</param>
        /// <param name="b">The amount to subtract</param>
        /// <returns>The difference</returns>
        public static Money operator -(Money a, Money b) => new(a.Satoshi - b.Satoshi);

        /// <summary>
        /// Convert a number of satoshis into an amount
        /// </summary>
        /// <param name="satoshis">The amount in satoshis</param>
        public static implicit operator Money(long satoshis) => new(satoshis);

        /// <summary>
        /// Compare this amount with another
        /// </summary>
        /// <param name="other">The amount to compare against</param>
        /// <returns>True when both amounts are equal</returns>
        public bool Equals(Money? other) => other is not null && Satoshi == other.Satoshi;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as Money);

        /// <inheritdoc/>
        public override int GetHashCode() => Satoshi.GetHashCode();

        /// <summary>
        /// Order this amount against another
        /// </summary>
        /// <param name="other">The amount to compare against</param>
        /// <returns>A negative number, zero, or a positive number</returns>
        public int CompareTo(Money? other) => other is null ? 1 : Satoshi.CompareTo(other.Satoshi);

        /// <inheritdoc/>
        public override string ToString() => Satoshi.ToString();
    }
}
