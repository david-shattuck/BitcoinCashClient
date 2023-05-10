namespace BitcoinCash.Models
{
    /// <summary>
    /// A psuedo-enum for the list of supported currencies
    /// </summary>
    public class Currency
    {
        private Currency(string value) { Value = value; }

        /// <summary>
        /// The selected currency
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Bitcoin, as described by Satoshi Nakamoto
        /// </summary>
        public static Currency BitcoinCash { get { return new Currency("bch"); } }

        /// <summary>
        /// One-one-hundred-millionth of a single BCH coin
        /// </summary>
        public static Currency Satoshis { get { return new Currency("sat"); } }

        /// <summary>
        /// The official currency of the United States
        /// </summary>
        public static Currency USDollar { get { return new Currency("usd"); } }

        /// <summary>
        /// The official currency of the People's Republic of China
        /// </summary>
        public static Currency ChineseYuan { get { return new Currency("cny"); } }

        /// <summary>
        /// The official currency of the European Union
        /// </summary>
        public static Currency Euro { get { return new Currency("eur"); } }

        /// <summary>
        /// The official currency of Japan
        /// </summary>
        public static Currency JapaneseYen { get { return new Currency("jpy"); } }

        /// <summary>
        /// The official currency of the United Kingdom
        /// </summary>
        public static Currency BritishPound { get { return new Currency("gbp"); } }

        /// <summary>
        /// The official currency of South Korea
        /// </summary>
        public static Currency SouthKoreanWon { get { return new Currency("krw"); } }

        /// <summary>
        /// The official currency of the Republic of India
        /// </summary>
        public static Currency IndianRupee { get { return new Currency("inr"); } }

        /// <summary>
        /// The official currency of Canada
        /// </summary>
        public static Currency CanadianDollar { get { return new Currency("cad"); } }

        /// <summary>
        /// The official currency of Hong Kong
        /// </summary>
        public static Currency HongKongDollar { get { return new Currency("hkd"); } }

        /// <summary>
        /// The official currency of Brazil
        /// </summary>
        public static Currency BrazilianReal { get { return new Currency("brl"); } }

        /// <summary>
        /// The official currency of Taiwan
        /// </summary>
        public static Currency TaiwanDollar { get { return new Currency("twd"); } }

        /// <summary>
        /// The official currency of Australia
        /// </summary>
        public static Currency AustralianDollar { get { return new Currency("aud"); } }

        /// <summary>
        /// The official currency of Switzerland
        /// </summary>
        public static Currency SwissFranc { get { return new Currency("chf"); } }

        /// <summary>
        /// The official currency of Russia
        /// </summary>
        public static Currency RussianRuble { get { return new Currency("rub"); } }

        /// <summary>
        /// The official currency of Mexico
        /// </summary>
        public static Currency MexicanPeso { get { return new Currency("mxn"); } }

        /// <summary>
        /// The official currency of Thailand
        /// </summary>
        public static Currency ThaiBaht { get { return new Currency("thb"); } }

        /// <summary>
        /// The official currency of Saudi Arabia
        /// </summary>
        public static Currency SaudiRiyal { get { return new Currency("sar"); } }

        /// <summary>
        /// The official currency of Singapore
        /// </summary>
        public static Currency SingaporeDollar { get { return new Currency("sgd"); } }

        /// <summary>
        /// The official currency of the United Arab Emirates
        /// </summary>
        public static Currency UAEDirham { get { return new Currency("aed"); } }

        /// <summary>
        /// The official currency of Vietnam
        /// </summary>
        public static Currency VietnameseDong { get { return new Currency("vnd"); } }

        /// <summary>
        /// The official currency of Indonesia
        /// </summary>
        public static Currency IndonesianRupiah { get { return new Currency("idr"); } }

        /// <summary>
        /// The official currency of Malaysia
        /// </summary>
        public static Currency MalaysianRinggit { get { return new Currency("myr"); } }

        /// <summary>
        /// The official currency of Poland
        /// </summary>
        public static Currency PolishZloty { get { return new Currency("pln"); } }

        /// <summary>
        /// The official currency of Israel
        /// </summary>
        public static Currency IsraeliShekel { get { return new Currency("ils"); } }

        /// <summary>
        /// The official currency of Turkey
        /// </summary>
        public static Currency TurkishLira { get { return new Currency("try"); } }

        /// <summary>
        /// The official currency of Sweden
        /// </summary>
        public static Currency SwedishKrona { get { return new Currency("sek"); } }

        /// <summary>
        /// The official currency of Chile
        /// </summary>
        public static Currency ChileanPeso { get { return new Currency("clp"); } }

        /// <summary>
        /// The official currency of Norway
        /// </summary>
        public static Currency NorwegianKrone { get { return new Currency("nok"); } }

        /// <summary>
        /// The official currency of the Czech Republic
        /// </summary>
        public static Currency CzechKoruna { get { return new Currency("czk"); } }

        /// <summary>
        /// The official currency of the Philippines
        /// </summary>
        public static Currency PhilippinePeso { get { return new Currency("php"); } }

        /// <summary>
        /// The official currency of Denmark
        /// </summary>
        public static Currency DanishKrone { get { return new Currency("dkk"); } }

        /// <summary>
        /// The official currency of South Africa
        /// </summary>
        public static Currency SouthAfricanRand { get { return new Currency("zar"); } }

        /// <summary>
        /// The official currency of New Zealand
        /// </summary>
        public static Currency NewZealandDollar { get { return new Currency("nzd"); } }

        /// <summary>
        /// The official currency of Kuwait
        /// </summary>
        public static Currency KuwaitiDinar { get { return new Currency("kwd"); } }

        /// <summary>
        /// The official currency of Hungary
        /// </summary>
        public static Currency HungarianForint { get { return new Currency("huf"); } }

        /// <summary>
        /// The official currency of Nigeria
        /// </summary>
        public static Currency NigerianNaira { get { return new Currency("ngn"); } }

        /// <summary>
        /// The official currency of Pakistan
        /// </summary>
        public static Currency PakistaniRupee { get { return new Currency("pkr"); } }

        /// <summary>
        /// The official currency of Argentina
        /// </summary>
        public static Currency ArgentinePeso { get { return new Currency("ars"); } }

        /// <summary>
        /// The official currency of Ukraine
        /// </summary>
        public static Currency UkrainianHryvnia { get { return new Currency("uah"); } }

        /// <summary>
        /// Get the ISO 4217 code for the selected currency
        /// </summary>
        /// <returns>A string of the 3-letter currency code</returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
