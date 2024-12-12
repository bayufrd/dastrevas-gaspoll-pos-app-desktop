using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class GetMenuModel
    {
        public List<Menu> data { get; set; }
    }

    public class Menu
    {
        public int id { get; set; }
        public string name { get; set; }
        public string menu_type { get; set; }
        public float price { get; set; }
        public string image_url { get; set; }
        public string receipt_number { get; set; }

        public string outlet_id { get; set; }

        public string cart_id { get; set; }

        public string customer_name { get; set;}
        public string customer_seat { get; set;}
        public string payment_type { get; set; }
        public string payment_category {  get; set; }
        public float customer_cash { get; set; }
        public float customer_change { get; set; }
        public string invoice_due_date { get; set; }

    }
}
