using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class SearchTree
    {
        public string Text { get; set; }
        public List<Control> Controls { get; set; }
        public SearchTree Left { get; set; }
        public SearchTree Right { get; set; }

        public SearchTree(string text, List<Control> controls)
        {
            Text = text;
            Controls = controls;
        }
    }
}
