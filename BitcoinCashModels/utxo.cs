using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinCash.Models
{
    public class utxo
    {
        public int block_id { get; set; }
        public string address { get; set; }
        public string transaction_hash { get; set; }
        public uint index { get; set; }
        public uint value { get; set; }
        public string cashAddr
        {
            get
            {
                return $"bitcoincash:{address}";
            }
        }
    }
}
