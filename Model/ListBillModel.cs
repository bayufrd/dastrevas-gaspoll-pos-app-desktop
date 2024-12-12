using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class ListBill
    {
        public int id { get; set; }
        public string receipt_number { get; set; }
        public int outlet_id { get; set; }
        public int cart_id { get; set; }
        public string customer_name { get; set; }
        public int customer_seat { get; set; }
        public string updated_at { get; set; }
    }

    public class ListBillModel
    {
        public List<ListBill> data { get; set; }
    }
}
