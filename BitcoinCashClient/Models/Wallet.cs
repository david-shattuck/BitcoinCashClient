namespace BitcoinCash.Models
{
    public class Wallet
    {
        public string PrivateKey { get; set; }
        public string PublicAddress { get; set; }
        public uint Balance { get; set; }
    }
}
