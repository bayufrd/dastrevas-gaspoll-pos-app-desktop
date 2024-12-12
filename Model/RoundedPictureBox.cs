using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class RoundedPictureBox : UserControl
    {
        private PictureBox pictureBox;

        public RoundedPictureBox()
        {
            this.pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            this.Controls.Add(this.pictureBox);

            this.Paint += (sender, e) =>
            {
                using (var g = e.Graphics)
                {
                    using (var pen = new Pen(Color.FromArgb(30, 31, 68), 2))
                    {
                        var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                        var diameter = 20;
                        var size = new Size(diameter, diameter);
                        var arc = new Rectangle(0, 0, size.Width, size.Height);

                        // Top left arc
                        g.DrawArc(pen, arc, 180, 90);

                        // Top right arc
                        arc.X = rect.Width - diameter;
                        g.DrawArc(pen, arc, 270, 90);

                        // Bottom right arc
                        arc.Y = rect.Height - diameter;
                        g.DrawArc(pen, arc, 0, 90);

                        // Bottom left arc
                        arc.X = 0;
                        g.DrawArc(pen, arc, 90, 90);
                    }
                }
            };
        }

        public Image Image
        {
            get { return this.pictureBox.Image; }
            set { this.pictureBox.Image = value; }
        }
    }
}
