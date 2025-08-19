using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Helper
{
    public class RoundedPanel : Panel
    {
        private int _cornerRadius = 20;

        [Category("Appearance")]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                Invalidate(); // Paksa redraw
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (GraphicsPath path = GetRoundedPath())
            {
                // Gambar background
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Gambar border
                using (Pen pen = new Pen(this.BackColor, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath GetRoundedPath()
        {
            GraphicsPath path = new GraphicsPath();
            int radius = CornerRadius;

            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90); // Sudut kiri atas
            path.AddArc(Width - (radius * 2), 0, radius * 2, radius * 2, 270, 90); // Sudut kanan atas
            path.AddArc(Width - (radius * 2), Height - (radius * 2), radius * 2, radius * 2, 0, 90); // Sudut kanan bawah
            path.AddArc(0, Height - (radius * 2), radius * 2, radius * 2, 90, 90); // Sudut kiri bawah

            path.CloseAllFigures();
            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
