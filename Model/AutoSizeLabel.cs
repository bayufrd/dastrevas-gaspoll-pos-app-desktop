using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace KASIR.Model
{
    public class AutoSizeLabel : Label
    {
        public AutoSizeLabel()
        {
            this.AutoSize = true;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Size = this.GetPreferredSize(Size.Empty);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.Size = this.GetPreferredSize(Size.Empty);
        }
    }
}
