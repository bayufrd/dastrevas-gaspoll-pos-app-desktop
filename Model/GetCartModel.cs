using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class GetCartModel
    {
        public DataCart data { get; set; }
    }
    public class DataCart
    {
        public string customer_name { get; set; }
        public string customer_seat { get; set; }
        public int cart_id { get; set; }
        public int subtotal { get; set; }
        public int total { get; set; }
        public int is_splitted {  get; set; }
        public int discount_id { get; set; }
        public List<DetailCart> cart_details { get; set; }
    }

    public class DetailCart
    {
        public int? cart_detail_id { get; set; }
        public int? menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public object menu_detail_id { get; set; }
        public string is_ordered { get; set; }
        public object varian { get; set; }
        public int? serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public int? discount_id { get; set; }
        public string discount_code { get; set; }
        public int? discounts_value { get; set; }
        public int? discounted_price { get; set; }
        public int? discounts_is_percent { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
        public string note_item { get; set; }
    }
}
