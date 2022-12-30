namespace BitcoinCash.Models
{
    public class Currency
    {
        private Currency(string value) { Value = value; }

        public string Value { get; private set; }

        public static Currency BitcoinCash { get { return new Currency("bch"); } }
        public static Currency Satoshis { get { return new Currency("sat"); } }
        public static Currency USDollars { get { return new Currency("usd"); } }

        public override string ToString()
        {
            return Value;
        }
    }
}
