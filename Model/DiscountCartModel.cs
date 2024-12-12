using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    [Serializable]
    public class DiscountCartModel
    {
        public List<DataDiscountCart> data { get; set; }
    }

    [Serializable]
    public class DataDiscountCart
    {
        public int id { get; set; }
        public string code { get; set; }
        public int is_percent { get; set; }
        public int is_discount_cart { get; set; }
        public int value { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int min_purchase { get; set; }
        public int? max_discount { get; set; }
    }
}
