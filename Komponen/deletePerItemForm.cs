﻿
using FontAwesome.Sharp;
using InTheHand.Net.Bluetooth;
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
using Serilog;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
namespace KASIR.Komponen
{
    public partial class deletePerItemForm : Form
    {
        private successTransaction SuccessTransaction { get; set; }
        private List<CartDetailTransaction> item = new List<CartDetailTransaction>();
        private List<RefundModel> refundItems = new List<RefundModel>();
        private readonly string MacAddressKasir;
        private readonly string PinPrinterKasir;
        private readonly string BaseOutletName;
        private readonly ILogger _log = LoggerService.Instance._log;
        public bool ReloadDataInBaseForm { get; private set; }
        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        private readonly string baseOutlet;
        GetTransactionDetail dataTransaction;
        string cart_detail;
        public deletePerItemForm(string cartDetail)
        {
            cart_detail = cartDetail;

            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;

            InitializeComponent();


        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
            DialogResult = DialogResult.OK;

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
            ////LoggerUtil.LogPrivateMethod(nameof(button2_Click));

            if (txtReason.Text == null || txtReason.Text.ToString() == "")
            {
                MessageBox.Show("Masukkan alasan hapus item", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (textPin.Text.ToString() == "" || textPin.Text == null)
            {
                MessageBox.Show("Masukan pin terlebih dahulu", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                /*
                var json = new
                {
                    outlet_id = baseOutlet,
                    cancel_reason = txtReason.Text.ToString(),
                    pin = textPin.Text
                };
                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                */
                IApiService apiService = new ApiService();
                HttpResponseMessage response = await apiService.DeleteCart("/cart/" + cart_detail + "?outlet_id=" + baseOutlet + "&cancel_reason=" + txtReason.Text.ToString() + "&pin=" + textPin.Text.ToString());
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {

                        if (Application.OpenForms["masterPos"] is masterPos masterPosForm)
                        {
                            // Call a method in the MasterPos form to refresh the cart
                            masterPosForm.ReloadCart();
                            masterPosForm.ReloadData2();
                            // You'll need to define this method in MasterPos
                        }
                        // ReloadDataInBaseForm = true;
                        DialogResult = DialogResult.OK;
                        Close();

                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show("PIN salah. Silakan coba lagi.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        DialogResult = DialogResult.Cancel;
                        MessageBox.Show("Hapus item gagal: " + response.StatusCode, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken.IsCancellationRequested)
                {
                    MessageBox.Show("PIN salah atau koneksi tidak stabil. Silakan coba beberapa saat lagi.", "Timeout/Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Gagal hapus data " + ex.Message);
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

