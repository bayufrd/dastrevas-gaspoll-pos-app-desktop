using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace KASIR.Model
{
    public class RoundedButton : Button
    {
        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(255, 255, 255);
            Padding = new Padding(10);
            Size = new Size(100, 50);
            Font = new Font("Segoe UI", 12, FontStyle.Regular);
            ForeColor = Color.FromArgb(66, 66, 66);
            UseVisualStyleBackColor = true;
            Cursor = Cursors.Hand;
            TextAlign = ContentAlignment.MiddleCenter;
            BorderRadius = 25;
        }

        public IconChar Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                Text = $" {_icon.ToString()}";
            }
        }

        private IconChar _icon;

        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = value;
                Invalidate();
            }
        }

        private int _borderRadius;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
                Region = new Region(gp);
            }
            using (var g = CreateGraphics())
            {
                using (var b = new SolidBrush(BackColor))
                {
                    g.FillEllipse(b, 0, 0, ClientSize.Width, ClientSize.Height);
                }
                using (var p = new Pen(Color.FromArgb(200, 200, 200), 2))
                {
                    g.DrawEllipse(p, new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1));
                }
            }
        }
    }
}
