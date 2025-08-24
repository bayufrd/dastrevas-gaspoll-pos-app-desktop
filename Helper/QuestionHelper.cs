using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace KASIR.Helper
{
    public partial class QuestionHelper : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        private string _mode;
        private Timer slideTimer;
        private int targetY;

        public QuestionHelper(string title, string msg, string cancelText, string okText, string mode = "")
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));


            lblTitle.Text = title;
            lblDisc.Text = msg;
            btnCancel.Text = cancelText;
            btnConfirm.Text = okText;

            _mode = mode?.ToLower();

            if (_mode == "update")
            {
                SetupUpdatePosition();
            }
        }

        private void SetupUpdatePosition()
        {
            var screen = Screen.PrimaryScreen.WorkingArea;

            // mulai dari bawah pojok kiri
            this.StartPosition = FormStartPosition.Manual;
            this.Left = 0;
            this.Top = screen.Bottom;
            this.BackColor = Color.Gainsboro;
            targetY = screen.Bottom - this.Height;

            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideUp;
            slideTimer.Start();
        }

        private void SlideUp(object sender, EventArgs e)
        {
            if (this.Top > targetY)
            {
                this.Top -= 20; // kecepatan naik
            }
            else
            {
                this.Top = targetY;
                slideTimer.Stop();
            }
        }
        public Form CreateOverlayForm()
        {
            // Temukan form yang memiliki nama "Form1"
            Form form1 = Application.OpenForms["Form1"];

            // Jika tidak ditemukan, coba ambil form pertama yang terbuka
            if (form1 == null)
            {
                if (Application.OpenForms.Count > 0)
                {
                    form1 = Application.OpenForms[0];
                }
                else
                {
                    // Jika tidak ada form yang terbuka, kembalikan null atau buat default
                    return null;
                }
            }

            // Buat overlay form
            Form overlay = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                Size = form1.ClientSize,
                Location = form1.PointToScreen(Point.Empty),
                TopMost = true,
                ShowInTaskbar = false
            };

            return overlay;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void PanelText_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
