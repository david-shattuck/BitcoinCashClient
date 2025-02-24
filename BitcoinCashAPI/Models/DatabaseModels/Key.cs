using System.ComponentModel.DataAnnotations;

namespace BitcoinCash.API.Models.DatabaseModels
{
    public class Key
    {
        [Key]
        public int ID { get; set; }
        public required string PrivateKey { get; set; }
        public required string Address { get; set; }
        public int RemainingCalls { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
