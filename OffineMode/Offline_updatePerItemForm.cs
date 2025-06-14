using KASIR.Model;
using KASIR.Properties;
using Newtonsoft.Json;

namespace KASIR.OfflineMode
{
    public partial class Offline_updatePerItemForm : Form
    {
        private readonly string baseOutlet;

        public Offline_updatePerItemForm()
        {
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
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
                    MessageBox.Show("Masukan pin terlebih dahulu", "Gaspol", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
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
                    MessageBox.Show("Password salah", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal ubah data " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}