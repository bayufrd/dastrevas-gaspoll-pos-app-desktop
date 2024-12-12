using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class RefundModel
    {
        public int CartDetailId { get; set; }
        public int Qty { get; set; }
        public string RefundReason { get; set; }
    }

}
