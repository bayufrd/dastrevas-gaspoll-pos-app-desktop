using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KASIR.Model;

namespace KASIR.Komponen
{
    public partial class BlurredBackgroundForm : Form
    {
        public BlurredBackgroundForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = 0.7d;
            this.BackColor = Color.Black;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Apply Gaussian blur effect to the background
            using (Graphics graphics = Graphics.FromImage(this.BackgroundImage))
            {
                GaussianBlur blur = new GaussianBlur();
                blur.Blur(graphics, new Rectangle(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height), 10);
            }
        }
    }
}
