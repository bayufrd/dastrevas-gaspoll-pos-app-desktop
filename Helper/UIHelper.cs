using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontAwesome.Sharp;

namespace KASIR.Helper
{
    public static class UIHelper
    {
        /// <summary>
        /// Membuat IconButton dengan tampilan rounded
        /// </summary>
        /// <param name="button">IconButton yang akan dimodifikasi</param>
        /// <param name="config">Konfigurasi styling button (opsional)</param>
        /// 

        //    Contoh penggunaan untuk IconButton
        //    iconButton1.ApplyRoundedStyle(); // Dengan default
        
        //    // Atau dengan konfigurasi khusus
        //    iconButton2.ApplyRoundedStyle(new UIHelper.RoundedButtonConfig 
        //    { 
        //        CornerRadius = 30, 
        //        OutlineColor = Color.Blue
        //     });

        //    // Contoh penggunaan untuk Panel
        //    panel1.ApplyRoundedCorners(); // Dengan radius default
        //    panel2.ApplyRoundedCorners(40); // Dengan radius khusus

        public static void ApplyRoundedStyle(this IconButton button, RoundedButtonConfig config = null)
        {
            // Gunakan konfigurasi default jika tidak disediakan
            config ??= new RoundedButtonConfig();

            // Jika parameter isRounded tidak diset atau false, kembalikan ke button standar
            if (config.IsRounded == null || config.IsRounded == false)
            {
                button.FlatStyle = FlatStyle.Standard;
                return;
            }

            // Set flat style untuk kontrol penuh
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;

            // Event handler untuk custom drawing
            button.Paint += (sender, e) =>
            {
                // Bersihkan background default
                e.Graphics.Clear(button.Parent.BackColor);

                // Buat path rounded
                GraphicsPath path = new GraphicsPath();
                Rectangle bounds = new Rectangle(0, 0, button.Width, button.Height);

                // Tambahkan arc untuk setiap sudut
                path.AddArc(0, 0, config.CornerRadius * 2, config.CornerRadius * 2, 180, 90); // Sudut kiri atas
                path.AddLine(config.CornerRadius, 0, button.Width - config.CornerRadius, 0); // Garis atas
                path.AddArc(button.Width - (config.CornerRadius * 2), 0, config.CornerRadius * 2, config.CornerRadius * 2, 270, 90); // Sudut kanan atas
                path.AddLine(button.Width, config.CornerRadius, button.Width, button.Height - config.CornerRadius); // Garis kanan
                path.AddArc(button.Width - (config.CornerRadius * 2), button.Height - (config.CornerRadius * 2), config.CornerRadius * 2, config.CornerRadius * 2, 0, 90); // Sudut kanan bawah
                path.AddLine(button.Width - config.CornerRadius, button.Height, config.CornerRadius, button.Height); // Garis bawah
                path.AddArc(0, button.Height - (config.CornerRadius * 2), config.CornerRadius * 2, config.CornerRadius * 2, 90, 90); // Sudut kiri bawah
                path.AddLine(0, button.Height - config.CornerRadius, 0, config.CornerRadius); // Garis kiri

                path.CloseFigure();

                // Set region untuk button
                button.Region = new Region(path);

                // Gambar background button
                using (SolidBrush brush = new SolidBrush(button.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Gambar outline jika warna outline ditentukan
                if (config.OutlineColor.HasValue)
                {
                    using (Pen outlinePen = new Pen(config.OutlineColor.Value, 1.8f))
                    {
                        outlinePen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                        e.Graphics.DrawPath(outlinePen, path);
                    }
                }

                // Gambar ikon
                if (button.IconChar != IconChar.None)
                {
                    // Hitung posisi ikon di tengah
                    SizeF iconSize = new SizeF(button.IconSize, button.IconSize);
                    PointF iconLocation = new PointF(
                        (button.Width - iconSize.Width) / 2,
                        (button.Height - iconSize.Height) / 2
                    );

                    // Konversi ikon FontAwesome ke bitmap
                    using (Bitmap iconBitmap = GetIconBitmap(button.IconChar, button.IconColor, button.IconSize))
                    {
                        e.Graphics.DrawImage(iconBitmap, iconLocation);
                    }
                }

                // Jika ada teks, gambar teks
                if (!string.IsNullOrEmpty(button.Text))
                {
                    TextRenderer.DrawText(
                        e.Graphics,
                        button.Text,
                        button.Font,
                        bounds,
                        button.ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                    );
                }
            };

            // Event handler untuk resize
            button.Resize += (sender, e) =>
            {
                button.Invalidate();
            };
        }

        /// <summary>
        /// Membuat Panel dengan sudut rounded
        /// </summary>
        /// <param name="panel">Panel yang akan dimodifikasi</param>
        /// <param name="radius">Radius sudut (default 20)</param>
        public static void ApplyRoundedCorners(this Panel panel, int radius = 20)
        {
            // Implementasi sama seperti metode RoundedPanel sebelumnya
            if (panel == null) return;

            GraphicsPath path = new GraphicsPath();
            Rectangle bounds = new Rectangle(0, 0, panel.Width, panel.Height);

            // [Sisipkan logika drawing path yang sama seperti sebelumnya]

            panel.Region = new Region(path);

            // Tambahkan event handler resize
            panel.Resize += (sender, e) =>
            {
                // [Sisipkan logika resize yang sama seperti sebelumnya]
            };
        }

        /// <summary>
        /// Kelas konfigurasi untuk button rounded
        /// </summary>
        public class RoundedButtonConfig
        {
            public bool? IsRounded { get; set; } = true;
            public int CornerRadius { get; set; } = 20;
            public Color? OutlineColor { get; set; } = null;
            public Color? BackgroundColor { get; set; } = null;
            public Color? IconColor { get; set; } = null;
        }

        /// <summary>
        /// Metode utility untuk konversi ikon FontAwesome ke Bitmap
        /// </summary>
        private static Bitmap GetIconBitmap(IconChar iconChar, Color iconColor, int iconSize)
        {
            // Buat bitmap baru dengan ukuran ikon
            Bitmap iconBitmap = new Bitmap(iconSize, iconSize);

            using (Graphics g = Graphics.FromImage(iconBitmap))
            {
                // Bersihkan background
                g.Clear(Color.Transparent);

                // Aktifkan anti-aliasing untuk rendering halus
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Buat font ikon
                using (Font iconFont = new Font("Font Awesome 6 Free", iconSize, FontStyle.Regular))
                {
                    // Konversi karakter ikon ke string
                    string iconString = char.ConvertFromUtf32((int)iconChar);

                    // Gambar ikon
                    using (SolidBrush brush = new SolidBrush(iconColor))
                    {
                        g.DrawString(iconString, iconFont, brush, 0, 0);
                    }
                }
            }

            return iconBitmap;
        }
    }
}
