using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;

using Serilog;
namespace KASIR.Komponen
{
    public partial class updatePerItemForm : Form
    {
        public bool ReloadDataInBaseForm { get; private set; }
        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        string cart_detail;
        Dictionary<string, object> jsondict;
        public updatePerItemForm(Dictionary<string, object> json, string cartDetail)
        {
            cart_detail = cartDetail;
            jsondict = json;
            InitializeComponent();
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (txtReason.Text == null || txtReason.Text.ToString() == "")
            {
                MessageBox.Show("Masukkan alasan Update item", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                jsondict["cancel_reason"] = txtReason.Text.ToString();
                jsondict["pin"] = textPin.Text;
                string jsonString = JsonConvert.SerializeObject(jsondict, Formatting.Indented);
                IApiService apiService = new ApiService();
                HttpResponseMessage response = await apiService.UpdateCart(jsonString, "/cart/" + cart_detail);
                if (response != null)
                {
                    DialogResult = DialogResult.OK;

                    if (response.IsSuccessStatusCode)
                    {

                        Close();
                    }
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                    Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal ubah data " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

        }
    }

}

