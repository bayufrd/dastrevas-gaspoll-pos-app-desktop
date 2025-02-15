using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class GetItemOnCartByIdModel
    {
        public DataItemOnCart data { get; set; }
    }

    public class DataItemOnCart
    {
        public int cart_detail_id { get; set; }
        public int menu_id { get; set; }
        public string is_ordered { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public int? discount_id { get; set; }
        public int? menu_detail_id { get; set; }
        public string menu_detail_name { get; set; }
        public string varian { get; set; }
        public int serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public int serving_type_percent { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int subtotal { get; set; }
        public int qty { get; set; }
        public string note_item { get; set; }

        public string MenuDetailIdAsString
        {
            get { return menu_detail_id.HasValue ? menu_detail_id.ToString() : ""; }
        }
    }


}
