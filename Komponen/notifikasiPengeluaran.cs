
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
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Text.RegularExpressions;
using System.Windows.Markup;
namespace KASIR.Komponen
{
    public partial class notifikasiPengeluaran : Form
    {
        private successTransaction SuccessTransaction { get; set; }
        private List<CartDetailTransaction> item = new List<CartDetailTransaction>();
        private List<RefundModel> refundItems = new List<RefundModel>();
        private readonly string MacAddressKasir;
        private readonly string PinPrinterKasir;
        private readonly string BaseOutletName;
        public bool ReloadDataInBaseForm { get; private set; }
        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        private readonly string baseOutlet;
        GetTransactionDetail dataTransaction;
        private readonly ILogger _log = LoggerService.Instance._log;
        public notifikasiPengeluaran()
        {
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;

            InitializeComponent();
            txtNotes.PlaceholderText = "Tujuan Pengeluaran?";
            button2.Enabled = false;
            LoadData();

        }

        private async void LoadData()
        {
            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.CekShift("/shift?outlet_id=" + baseOutlet);

                if (string.IsNullOrEmpty(response))
                {
                    MessageBox.Show("Tidak ada respon dari server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var cekShift = JsonConvert.DeserializeObject<GetShift>(response);
                if (cekShift?.data == null)
                {
                    MessageBox.Show("Data shift tidak tersedia atau tidak valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var datas = cekShift.data;
                var expenditures = datas.expenditures ?? new List<ExpenditureStrukShift>();
                var cartDetailsSuccess = datas.cart_details_success ?? new List<CartDetailsSuccessStrukShift>();
                var cartDetailsPending = datas.cart_details_pending ?? new List<CartDetailsPendingStrukShift>();
                var cartDetailsCanceled = datas.cart_details_canceled ?? new List<CartDetailsCanceledStrukShift>();
                var refundDetails = datas.refund_details ?? new List<RefundDetailStrukShift>();
                var paymentDetails = datas.payment_details ?? new List<PaymentDetailStrukShift>();

                var dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(string));
                dataTable.Columns.Add("DATA", typeof(string));
                dataTable.Columns.Add("Detail", typeof(string));

                dataTable.Rows.Add(null, datas.outlet_name, null);
                dataTable.Rows.Add(null, "Start Date :", datas.start_date);
                dataTable.Rows.Add(null, "End Date :", datas.end_date);
                dataTable.Rows.Add(null, "Shift Number :", datas.shift_number);
                dataTable.Rows.Add(null, "--------------------------------", null);

                if (expenditures.Any())
                {
                    dataTable.Rows.Add(null, "EXPENSE", null);
                    foreach (var expense in expenditures)
                    {
                        dataTable.Rows.Add(null, expense.description, $"Rp. {expense.nominal:n0},-");
                    }
                }

                dataGridView1.DataSource = dataTable;

                var boldStyle = new DataGridViewCellStyle
                {
                    Font = new Font(dataGridView1.Font, FontStyle.Italic)
                };
                dataGridView1.Columns["DATA"].DefaultCellStyle = boldStyle;
                dataGridView1.Columns["ID"].Visible = false;

                button2.Enabled = true;
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "TaskCanceledException: {ErrorMessage}", ex.Message);
            }
            catch (JsonSerializationException ex)
            {
                MessageBox.Show("Gagal memproses data dari server.", "Deserialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "Deserialization error: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Unhandled Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "Unexpected error: {ErrorMessage}", ex.Message);
                button2.Enabled = true;
            }
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
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
            ////LoggerUtil.LogPrivateMethod(nameof(button2_Click));
            string fulus = Regex.Replace(txtNominal.Text, "[^0-9]", "");
            try
            {

                if (fulus == null || fulus.ToString() == "")
                {
                    MessageBox.Show("Format nominal kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (txtNotes.Text == null || txtNotes.ToString() == "")
                {
                    MessageBox.Show("Format notes kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                DialogResult yakin = MessageBox.Show($"Yakin menambahkan Expenditure Rp.{txtNominal.Text},- dengan tujuan \n {txtNotes.Text} ?", "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (yakin != DialogResult.Yes)
                {
                    MessageBox.Show("Penambahan Expenditure Shift diCancel");
                    return;
                    this.Close();
                }
                var json = new
                {
                    nominal = fulus.ToString(),
                    description = txtNotes.Text.ToString(),
                    outlet_id = baseOutlet
                };
                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                IApiService apiService = new ApiService();

                HttpResponseMessage response = await apiService.notifikasiPengeluaran(jsonString, "/expenditure");

                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        DialogResult result = MessageBox.Show("Input notifikasi pengeluaran berhasil", "Gaspol", MessageBoxButtons.OK);
                        if (result == DialogResult.OK)
                        {
                            this.Close(); // Close the payForm
                        }
                        this.DialogResult = result;
                    }
                    else
                    {
                        MessageBox.Show("Input notifikasi pengeluaran gagal  " + response.StatusCode);
                        _log.Error("gagal input notifikasi pengeluaran");
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
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

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNominal_TextChanged(object sender, EventArgs e)
        {
            if (txtNominal.Text == "" || txtNominal.Text == "0") return;
            decimal number;
            try
            {
                number = decimal.Parse(txtNominal.Text, System.Globalization.NumberStyles.Currency);
            }
            catch (FormatException)
            {
                // The text could not be parsed as a decimal number.
                // You can handle this exception in different ways, such as displaying a message to the user.
                MessageBox.Show("inputan hanya bisa Numeric");
                return;
            }
            txtNominal.Text = number.ToString("#,#");
            txtNominal.SelectionStart = txtNominal.Text.Length;
        }
    }

}

