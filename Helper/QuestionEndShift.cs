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
    public partial class QuestionEndShift : Form
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

        public QuestionEndShift(string title, string msg, string cancelText, string okText, string start, string end, string numbershift)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));


            lblTitle.Text = title;
            lblDisc.Text = msg;
            btnCancel.Text = cancelText;
            btnConfirm.Text = okText;
            start_shift.Text = start;
            end_shift.Text = end;
            shift_number.Text = numbershift;
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
        public string EditedStartShift { get; private set; }
        public string EditedEndShift { get; private set; }
        public string EditedNumberShift { get; private set; }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DateTime parsedStart;
            DateTime parsedEnd;

            if (!DateTime.TryParseExact(start_shift.Text,
                                        "yyyy-MM-dd HH:mm:ss",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None,
                                        out parsedStart))
            {
                NotifyHelper.Error("Format Start Shift harus yyyy-MM-dd HH:mm:ss");
                return; 
            }

            if (!DateTime.TryParseExact(end_shift.Text,
                                        "yyyy-MM-dd HH:mm:ss",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None,
                                        out parsedEnd))
            {
                NotifyHelper.Error("Format End Shift harus yyyy-MM-dd HH:mm:ss");
                return;
            }
            if (string.IsNullOrEmpty(shift_number.Text) || !int.TryParse(shift_number.Text, out _))
            {
                NotifyHelper.Error("Shift Number tidak benar!");
                return;
            }
            EditedNumberShift = shift_number.Text;
            EditedStartShift = parsedStart.ToString("yyyy-MM-dd HH:mm:ss");
            EditedEndShift = parsedEnd.ToString("yyyy-MM-dd HH:mm:ss");
            


            DialogResult = DialogResult.OK;
            Close();
        }


        private void PanelText_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
