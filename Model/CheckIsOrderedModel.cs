using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace KASIR.Model
{
    public class CheckIsOrderedModel
    {
        public DataIsOrdered data { get; set; }
    }

    public class DataIsOrdered
    {
        public int is_ordered { get; set; }
    }
}
