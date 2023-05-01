namespace BitcoinCash.Models
{
    public class Currency
    {
        private Currency(string value) { Value = value; }

        public string Value { get; private set; }

        public static Currency BitcoinCash { get { return new Currency("bch"); } }
        public static Currency Satoshis { get { return new Currency("sat"); } }
        public static Currency USDollar { get { return new Currency("usd"); } }
        public static Currency ChineseYuan { get { return new Currency("cny"); } }
        public static Currency Euro { get { return new Currency("eur"); } }
        public static Currency JapaneseYen { get { return new Currency("jpy"); } }
        public static Currency BritishPound { get { return new Currency("gbp"); } }
        public static Currency SouthKoreanWon { get { return new Currency("krw"); } }
        public static Currency IndianRupee { get { return new Currency("inr"); } }
        public static Currency CanadianDollar { get { return new Currency("cad"); } }
        public static Currency HongKongDollar { get { return new Currency("hkd"); } }
        public static Currency BrazilianReal { get { return new Currency("brl"); } }
        public static Currency TaiwanDollar { get { return new Currency("twd"); } }
        public static Currency AustralianDollar { get { return new Currency("aud"); } }
        public static Currency SwissFranc { get { return new Currency("chf"); } }
        public static Currency RussianRuble { get { return new Currency("rub"); } }
        public static Currency MexicanPeso { get { return new Currency("mxn"); } }
        public static Currency ThaiBaht { get { return new Currency("thb"); } }
        public static Currency SaudiRiyal { get { return new Currency("sar"); } }
        public static Currency SingaporeDollar { get { return new Currency("sgd"); } }
        public static Currency UAEDirham { get { return new Currency("aed"); } }
        public static Currency VietnameseDong { get { return new Currency("vnd"); } }
        public static Currency IndonesianRupiah { get { return new Currency("idr"); } }
        public static Currency MalaysianRinggit { get { return new Currency("myr"); } }
        public static Currency PolishZloty { get { return new Currency("pln"); } }
        public static Currency IsraeliShekel { get { return new Currency("ils"); } }
        public static Currency TurkishLira { get { return new Currency("try"); } }
        public static Currency SwedishKrona { get { return new Currency("sek"); } }
        public static Currency ChileanPeso { get { return new Currency("clp"); } }
        public static Currency NorwegianKrone { get { return new Currency("nok"); } }
        public static Currency CzechKoruna { get { return new Currency("czk"); } }
        public static Currency PhilippinePeso { get { return new Currency("php"); } }
        public static Currency DanishKrone { get { return new Currency("dkk"); } }
        public static Currency SouthAfricanRand { get { return new Currency("zar"); } }
        public static Currency NewZealandDollar { get { return new Currency("nzd"); } }
        public static Currency KuwaitiDinar { get { return new Currency("kwd"); } }
        public static Currency HungarianForint { get { return new Currency("huf"); } }
        public static Currency NigerianNaira { get { return new Currency("ngn"); } }
        public static Currency PakistaniRupee { get { return new Currency("pkr"); } }
        public static Currency ArgentinePeso { get { return new Currency("ars"); } }
        public static Currency UkrainianHryvnia { get { return new Currency("uah"); } }

        public override string ToString()
        {
            return Value;
        }
    }
}
