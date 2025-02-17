using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class RefundModel
    {
        public int CartDetailId { get; set; } // Untuk menyimpan CartDetailId
        public int QtyMax { get; set; } // Menyimpan qty asli dari cartDetails
        public int Qty { get; set; } // Menyimpan jumlah yang di-refund
        public int QtyRemaining { get; set; } // Menyimpan sisa qty yang bisa di-refund
        public string RefundReason { get; set; } // Alasan refund
    }


}
