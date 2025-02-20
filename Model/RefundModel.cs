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
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public int menu_detail_id { get; set; }
        public string menu_detail_name { get; set; }
        public int price { get; set; }
        public string serving_type_name { get; set; }
    }


}
