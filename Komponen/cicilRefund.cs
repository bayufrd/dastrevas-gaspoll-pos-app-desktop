using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KASIR.komponen
{
    public partial class cicilRefund : Form
    {
        string cart_id;
        int row;
        private readonly string baseOutlet;
        private readonly ILogger _log = LoggerService.Instance._log;
        public bool ReloadDataInBaseForm { get; private set; }
        public cicilRefund(string cartId)
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            cart_id = cartId;

            InitializeComponent();
            LoadCicil();
        }

        private async void LoadCicil()
        {
            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.GetCicilDetail("installment-cart/" + cart_id);

                CicilRefundModel cicilRefundModel = JsonConvert.DeserializeObject<CicilRefundModel>(response);
                DataCicil data = cicilRefundModel.data;
                txtTotalCart.Text = "Total Jumlah Keranjang = Rp. " + data.total_cart.ToString();
                txtBelumDibayar.Text = "Total Belum Bayar  = Rp. " + data.unpaid_balance.ToString();
                txtSudahBayar.Text = "Total Sudah Bayar  = Rp. " + data.paid_balance.ToString();


            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal tampil data " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void btnSimpan_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnSimpan_Click));

            try
            {
                var json = new
                {
                    cart_id = int.Parse(cart_id),
                    cash = txtJumlahCicil.Text
                };
                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                IApiService apiService = new ApiService();
                HttpResponseMessage response = await apiService.cicilRefund(jsonString, "/installment-cart");
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        DialogResult result = MessageBox.Show("Cicil refund sukses", "Gaspol", MessageBoxButtons.OK);
                        if (result == DialogResult.OK)
                        {
                            ReloadDataInBaseForm = true;

                        }
                        this.DialogResult = result;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cicil refund gagal");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
    }
}
