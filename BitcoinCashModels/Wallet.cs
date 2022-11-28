namespace BitcoinCash.Models
{
    public class Wallet
    {
        public string PrivateKey { get; set; }
        public string PublicAddress { get; set; }
        public uint? Balance
        {
            get
            {
                return utxos == null ? null : (uint)utxos.Sum(u => u.value);
            }
        }
        public decimal? Value { get; set; }
        public static string ValueCurrency 
        { 
            get 
            {
                return "usd";
            } 
        }
        public List<utxo>? utxos { get; set; }
    }
}