
    using KASIR.Properties;
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    namespace KASIR.Helper
    {
        public partial class NotifAlert : Form
        {
            // Konstanta untuk pengaturan
            private const int MAX_WIDTH = 400;
            private const int MIN_WIDTH = 250;
            private const int ICON_WIDTH = 50;
            private const int PADDING_HORIZONTAL = 15;
            private const int PADDING_VERTICAL = 10;
            private const int VERTICAL_SPACING = 10;

            public NotifAlert()
            {
                InitializeComponent();

                // Konfigurasi label
                ConfigureLabel();

                // Konfigurasi button close
                ConfigureCloseButton();

                // Hapus border pada form
                this.FormBorderStyle = FormBorderStyle.None;
            }

            private void ConfigureLabel()
            {
                // Atur label untuk auto-wrap dan padding
                lblMsg.MaximumSize = new Size(MAX_WIDTH - ICON_WIDTH - (PADDING_HORIZONTAL * 2), 0);
                lblMsg.AutoSize = true;

                // Atur padding
                lblMsg.Padding = new Padding(PADDING_HORIZONTAL, PADDING_VERTICAL, PADDING_HORIZONTAL, PADDING_VERTICAL);

                // Atur font dan warna teks
                lblMsg.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                lblMsg.ForeColor = Color.White;
            }

            private void ConfigureCloseButton()
            {
                // Atur button close
                button1.FlatStyle = FlatStyle.Flat;
                button1.FlatAppearance.BorderSize = 0;
                button1.BackColor = Color.Transparent;
                button1.ForeColor = Color.White;

                // Posisikan button di pojok kanan atas
                button1.Location = new Point(this.Width - button1.Width - 5, 5);
            }

            private int x, y;
            public enum enmAction
            {
                wait,
                start,
                close
            }
            public enum enmType
            {
                Success,
                Warning,
                Error,
                Info
            }

            private enmAction action;
            private void showAlert(string msg, enmType type)
            {
                this.Opacity = 0.0;
                this.StartPosition = FormStartPosition.Manual;

                // Atur ukuran form berdasarkan panjang pesan
                AdjustFormSize(msg);

                // Dapatkan area kerja layar utama
                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

                // Cari posisi vertikal untuk notifikasi baru
                int verticalOffset = GetVerticalOffset();

                // Tentukan posisi di kiri bawah
                this.x = 10; // 10 pixel dari tepi kiri
                this.y = workingArea.Height - this.Height - 10 - verticalOffset; // Geser ke atas berdasarkan notifikasi sebelumnya

                // Atur posisi form
                this.Location = new Point(this.x, this.y);

                // Tentukan warna dan ikon berdasarkan tipe
                ConfigureNotificationType(type);

                this.lblMsg.Text = msg;
                this.Show();
                this.action = enmAction.start;
                this.timer1.Interval = 1;
                timer1.Start();
            }

            private void ConfigureNotificationType(enmType type)
            {
                switch (type)
                {
                    case enmType.Success:
                        this.pictureBox1.Image = Resources.success;
                        this.BackColor = Color.FromArgb(40, 167, 69); // Hijau yang lebih lembut
                        break;
                    case enmType.Error:
                        this.pictureBox1.Image = Resources.warn;
                        this.BackColor = Color.FromArgb(220, 53, 69); // Merah yang lebih lembut
                        break;
                    case enmType.Info:
                        this.pictureBox1.Image = Resources.info;
                        this.BackColor = Color.FromArgb(23, 162, 184); // Biru yang lebih lembut
                        break;
                    case enmType.Warning:
                        this.pictureBox1.Image = Resources.network;
                        this.BackColor = Color.FromArgb(255, 193, 7); // Kuning yang lebih lembut
                        break;
                }
            }

            private void AdjustFormSize(string message)
            {
                // Buat label sementara untuk menghitung ukuran teks
                using (Graphics g = lblMsg.CreateGraphics())
                {
                    // Hitung ukuran teks
                    SizeF textSize = g.MeasureString(
                        message,
                        lblMsg.Font,
                        MAX_WIDTH - ICON_WIDTH - (PADDING_HORIZONTAL * 2)
                    );

                    // Tentukan lebar form
                    int formWidth = Math.Max(
                        Math.Min(
                            (int)textSize.Width + ICON_WIDTH + (PADDING_HORIZONTAL * 2),
                            MAX_WIDTH
                        ),
                        MIN_WIDTH
                    );

                    // Tentukan tinggi form
                    int formHeight = (int)textSize.Height + (PADDING_VERTICAL * 2) + 20;

                    // Atur ukuran form
                    this.Width = formWidth;
                    this.Height = formHeight;

                    // Atur ukuran label
                    lblMsg.Width = formWidth - ICON_WIDTH - (PADDING_HORIZONTAL * 2);

                    // Atur ulang posisi button close
                    button1.Location = new Point(this.Width - button1.Width - 5, 5);
                }
            }

            private int GetVerticalOffset()
            {
                int offset = 0;
                // Cari semua form NotifAlert yang sedang terbuka
                var openNotifications = Application.OpenForms
                    .OfType<NotifAlert>()
                    .Where(f => f.Opacity > 0)
                    .ToList();

                // Hitung offset berdasarkan jumlah notifikasi yang sudah ada
                offset = openNotifications.Count * (this.Height + VERTICAL_SPACING);

                return offset;
            }

            private void button1_Click(object sender, EventArgs e)
            {
                timer1.Interval = 1;
                action = enmAction.close;
            }

            private void timer1_Tick(object sender, EventArgs e)
            {
                switch (this.action)
                {
                    case enmAction.wait:
                        timer1.Interval = 5000;
                        action = enmAction.close;
                        break;
                    case enmAction.start:
                        timer1.Interval = 1;
                        this.Opacity += 0.1;
                        if (this.x > this.Location.X)
                        {
                            this.Left++;
                        }
                        else
                        {
                            if (this.Opacity == 1.0)
                            {
                                action = enmAction.wait;
                            }
                        }
                        break;
                    case enmAction.close:
                        timer1.Interval = 1;
                        this.Opacity -= 0.1;

                        this.Left += 3;
                        if (base.Opacity == 0.0)
                        {
                            base.Close();
                        }
                        break;
                }
            }
        }
    }
