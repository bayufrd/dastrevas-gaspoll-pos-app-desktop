using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class AddMenuModel
    {
        public string name { get; set; }
        public string menu_type { get; set; }
        public int price { get; set; }
        public List<MenuDetail> menu_details { get; set; }
    }

    public class MenuDetail
    {
        public string varian { get; set; }
        public int price { get; set; }
    }
}
