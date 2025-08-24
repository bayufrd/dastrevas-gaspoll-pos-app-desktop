using KASIR.Properties;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KASIR.Helper
{
    public partial class NotifAlert : Form
    {
        // Konstanta
        private const int MAX_WIDTH = 400;
        private const int MIN_WIDTH = 250;
        private const int ICON_SIZE = 40;
        private const int CLOSE_BTN_SIZE = 30;
        private const int PADDING_HORIZONTAL = 15;
        private const int PADDING_VERTICAL = 10;
        private const int VERTICAL_SPACING = 10;

        public NotifAlert()
        {
            InitializeComponent();

            // Konfigurasi Form
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black; // default

            // PictureBox (ikon)
            pictureBox1.Size = new Size(ICON_SIZE, ICON_SIZE);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Location = new Point(PADDING_HORIZONTAL, PADDING_VERTICAL);

            // Label pesan
            lblMsg.AutoSize = false;
            lblMsg.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblMsg.ForeColor = Color.White;
            lblMsg.TextAlign = ContentAlignment.MiddleLeft;

            // Tombol Close
            button1.Size = new Size(CLOSE_BTN_SIZE, CLOSE_BTN_SIZE);
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;
            button1.BackColor = Color.Transparent;
            button1.ForeColor = Color.White;
            button1.Text = "X";
            button1.Click += button1_Click;

            this.TopMost = true;
        }

        private int x, y;
        public enum enmAction { wait, start, close }
        public enum enmType { Success, Warning, Error, Info }

        private enmAction action;

        private void showAlert(string msg, enmType type)
        {
            this.Opacity = 0.0;
            this.StartPosition = FormStartPosition.Manual;

            // Atur ukuran dan posisi form sesuai isi pesan
            AdjustLayout(msg);

            // Posisi form (kiri bawah)
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.x = 10;
            this.y = workingArea.Height - this.Height - 10;
            this.Location = new Point(this.x, this.y);

            // Warna + ikon
            ConfigureNotificationType(type);

            lblMsg.Text = msg;
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
                    pictureBox1.Image = Resources.success;
                    this.BackColor = Color.FromArgb(40, 167, 69);
                    break;
                case enmType.Error:
                    pictureBox1.Image = Resources.warn;
                    this.BackColor = Color.FromArgb(220, 53, 69);
                    break;
                case enmType.Info:
                    pictureBox1.Image = Resources.info;
                    this.BackColor = Color.FromArgb(23, 162, 184);
                    break;
                case enmType.Warning:
                    pictureBox1.Image = Resources.warn__2_;
                    this.BackColor = Color.FromArgb(255, 193, 7);
                    break;
            }
        }

        private string TruncateMessage(string message, int maxChars = 250)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;

            if (message.Length > maxChars)
                return message.Substring(0, maxChars) + "...";

            return message;
        }

        private void AdjustLayout(string message)
        {
            // potong dulu kalau terlalu panjang
            string truncated = TruncateMessage(message);

            using (Graphics g = lblMsg.CreateGraphics())
            {
                // Hitung ukuran teks dengan batas maksimal width
                SizeF textSize = g.MeasureString(
                    truncated,
                    lblMsg.Font,
                    MAX_WIDTH - ICON_SIZE - CLOSE_BTN_SIZE - (PADDING_HORIZONTAL * 4)
                );

                int textWidth = (int)textSize.Width + 10;
                int textHeight = (int)textSize.Height + 10;

                int formWidth = Math.Max(
                    Math.Min(textWidth + ICON_SIZE + CLOSE_BTN_SIZE + (PADDING_HORIZONTAL * 4), MAX_WIDTH),
                    MIN_WIDTH
                );

                int formHeight = Math.Max(textHeight + (PADDING_VERTICAL * 2), ICON_SIZE + (PADDING_VERTICAL * 2));

                this.Width = formWidth;
                this.Height = formHeight;

                // Atur posisi ikon
                pictureBox1.Location = new Point(PADDING_HORIZONTAL, (this.Height - ICON_SIZE) / 2);

                // Atur posisi tombol close
                button1.Location = new Point(this.Width - CLOSE_BTN_SIZE - PADDING_HORIZONTAL, (this.Height - CLOSE_BTN_SIZE) / 2);

                // Atur posisi label
                int lblX = pictureBox1.Right + PADDING_HORIZONTAL;
                int lblWidth = button1.Left - lblX - PADDING_HORIZONTAL;
                lblMsg.Location = new Point(lblX, PADDING_VERTICAL);
                lblMsg.Size = new Size(lblWidth, this.Height - (PADDING_VERTICAL * 2));

                lblMsg.Text = truncated;
            }
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
                    if (this.Opacity == 1.0)
                        action = enmAction.wait;
                    break;
                case enmAction.close:
                    timer1.Interval = 1;
                    this.Opacity -= 0.1;
                    this.Left += 3;
                    if (this.Opacity == 0.0)
                        this.Close();
                    break;
            }
        }
    }
}
