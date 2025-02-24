using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BitcoinCash.API.Models.DatabaseModels
{
    public class BchTransaction
    {
        [Key]
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public int PaymentType { get; set; }
        public int FromType { get; set; }
        public int? FromID { get; set; }
        public int ToType { get; set; }
        public int? ToID { get; set; }
        public required string TxId { get; set; }
        public decimal BchAmount { get; set; }
        public decimal BchPrice { get; set; }
        public int Status { get; set; }

        [NotMapped]
        public decimal ValueUSD
        {
            get
            {
                return BchAmount * BchPrice;
            }
        }
    }
}
