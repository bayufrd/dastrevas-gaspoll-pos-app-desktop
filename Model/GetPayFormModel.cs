using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class GetPayFormModel
    {
        public List<PayFormMethod> data { get; set; }
    }
    public class PayFormMethod
    {
        public int id { get; set; }
        public string name { get; set; }
        public string menu_type { get; set; }
        public int price { get; set; }
        public string image_url { get; set; }
    }

    
}
