using InTheHand.Net.Bluetooth;

using FontAwesome.Sharp;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Text.Json.Nodes;
namespace KASIR.OfflineMode
{
    public partial class Offline_updatePerItemForm : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        //private successTransaction SuccessTransaction { get; set; }
        private List<CartDetailTransaction> item = new List<CartDetailTransaction>();
        private List<RefundModel> refundItems = new List<RefundModel>();
        private readonly string MacAddressKasir;
        private readonly string PinPrinterKasir;
        private readonly string BaseOutletName;
        public bool ReloadDataInBaseForm { get; private set; }
        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        private readonly string baseOutlet;
        GetTransactionDetail dataTransaction;
        string cart_detail;
        Dictionary<string, object> jsondict;
        public Offline_updatePerItemForm(Dictionary<string, object> json, string cartDetail)
        {
            cart_detail = cartDetail;
            jsondict = json;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;

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

            // KeluarButtonPrintReportShiftClicked = true;
            this.Close();
        }
        private void AddItem(string name, string amount)
        {


        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel13_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //LoggerUtil.LogPrivateMethod(nameof(button2_Click));

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

        private void txtJumlahCicil_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
        private void txtSelesaiShift_TextChanged(object sender, EventArgs e)
        {

        }


    }

}

