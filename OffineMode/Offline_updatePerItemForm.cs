using KASIR.Model;
using KASIR.Properties;
using Newtonsoft.Json;
using KASIR.Helper;
using System.Runtime.InteropServices;

namespace KASIR.OfflineMode
{
    public partial class Offline_updatePerItemForm : Form
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

        private readonly string baseOutlet;

        public Offline_updatePerItemForm()
        {
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

        }

        public string cancelReason { get; set; }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (textPin.Text == "" || textPin.Text == null)
                {
                    NotifyHelper.Warning("Masukan pin terlebih dahulu");
                    return;
                }

                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);

                if (textPin.Text == dataOutlet.data.pin.ToString())
                {
                    cancelReason = txtReason.Text;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    NotifyHelper.Error("Password salah");
                }
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal ubah data " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}