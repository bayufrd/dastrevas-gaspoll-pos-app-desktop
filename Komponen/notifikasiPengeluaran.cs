
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
using System.Security.Cryptography.Xml;
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

        // Modifikasi method LoadData() untuk menambahkan handling error
        private async void LoadData()
        {
            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.CekShift("/shift?outlet_id=" + baseOutlet);

                if (string.IsNullOrEmpty(response))
                {
                    CleanFormAndAddRetryButton("Tidak ada respon dari server.");
                    return;
                }

                var cekShift = JsonConvert.DeserializeObject<GetShift>(response);
                if (cekShift?.data == null)
                {
                    CleanFormAndAddRetryButton("Data shift tidak tersedia atau tidak valid.");
                    return;
                }

                var datas = cekShift.data;

                // Pastikan semua properti diakses dengan aman menggunakan null conditional operator
                var expenditures = datas?.expenditures ?? new List<ExpenditureStrukShift>();
                var cartDetailsSuccess = datas?.cart_details_success ?? new List<CartDetailsSuccessStrukShift>();
                var cartDetailsPending = datas?.cart_details_pending ?? new List<CartDetailsPendingStrukShift>();
                var cartDetailsCanceled = datas?.cart_details_canceled ?? new List<CartDetailsCanceledStrukShift>();
                var refundDetails = datas?.refund_details ?? new List<RefundDetailStrukShift>();
                var paymentDetails = datas?.payment_details ?? new List<PaymentDetailStrukShift>();

                var dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(string));
                dataTable.Columns.Add("DATA", typeof(string));
                dataTable.Columns.Add("Detail", typeof(string));

                // Pastikan datas.outlet_name tidak null
                dataTable.Rows.Add(null, datas?.outlet_name ?? "Unknown Outlet", null);
                dataTable.Rows.Add(null, "Start Date :", datas?.start_date);
                dataTable.Rows.Add(null, "End Date :", datas?.end_date);
                dataTable.Rows.Add(null, "Shift Number :", datas?.shift_number);
                dataTable.Rows.Add(null, "--------------------------------", null);

                if (expenditures != null && expenditures.Any())
                {
                    dataTable.Rows.Add(null, "EXPENSE", null);
                    foreach (var expense in expenditures)
                    {
                        dataTable.Rows.Add(null, expense?.description ?? "No description", $"Rp. {expense?.nominal:n0},-");
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
                LoggerUtil.LogError(ex, "TaskCanceledException: {ErrorMessage}", ex.Message);
                CleanFormAndAddRetryButton("Koneksi tidak stabil: " + ex.Message);
            }
            catch (JsonSerializationException ex)
            {
                LoggerUtil.LogError(ex, "Deserialization error: {ErrorMessage}", ex.Message);
                CleanFormAndAddRetryButton("Gagal memproses data: " + ex.Message);
            }
            catch (NullReferenceException ex)
            {
                LoggerUtil.LogError(ex, "Null reference error: {ErrorMessage}", ex.Message);
                CleanFormAndAddRetryButton("Data referensi tidak ditemukan: " + ex.Message);
                button2.Enabled = false;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Unexpected error: {ErrorMessage}", ex.Message);
                CleanFormAndAddRetryButton("Terjadi kesalahan: " + ex.Message);
                button2.Enabled = false;
            }
        }

        // Tambahkan method CleanFormAndAddRetryButton dari contoh yang diberikan
        private void CleanFormAndAddRetryButton(string ex)
        {
            // Bersihkan form
            if (dataGridView1 != null && dataGridView1.DataSource != null)
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
            }

            // Tambahkan PictureBox untuk gambar
            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = "ErrorImg";
            pictureBox.Size = new Size(100, 100); // Sesuaikan ukuran gambar
            pictureBox.Location = new Point((this.ClientSize.Width - pictureBox.Width) / 2, (this.ClientSize.Height - pictureBox.Height) / 2 - 110); // Posisi di atas tombol
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Atur ukuran gambar agar sesuai dengan PictureBox
            try
            {
                using (FileStream fs = new FileStream("icon\\OutletLogo.bmp", FileMode.Open, FileAccess.Read))
                {
                    pictureBox.Image = Image.FromStream(fs);
                }
            }
            catch
            {
                // Jika gagal memuat gambar, jangan tampilkan PictureBox
                pictureBox.Visible = false;
            }

            // Tambahkan tombol retry
            System.Windows.Forms.Button btnRetry = new System.Windows.Forms.Button();
            btnRetry.Name = "btnRetry";
            btnRetry.Text = "Retry Load Data\nOut of Service";
            btnRetry.Size = new Size(190, 60);
            btnRetry.Location = new Point((this.ClientSize.Width - btnRetry.Width) / 2, (this.ClientSize.Height - btnRetry.Height) / 2);
            btnRetry.BackColor = Color.FromArgb(30, 31, 68);
            btnRetry.FlatStyle = FlatStyle.Flat;
            btnRetry.Font = new Font("Arial", 10, FontStyle.Bold);
            btnRetry.ForeColor = Color.White; // Mengatur warna teks tombol menjadi putih
            btnRetry.Click += new EventHandler(BtnRetry_Click);

            // Membuat sudut membulat
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 20, 20, 180, 90);
            path.AddArc(btnRetry.Width - 20, 0, 20, 20, 270, 90);
            path.AddArc(btnRetry.Width - 20, btnRetry.Height - 20, 20, 20, 0, 90);
            path.AddArc(0, btnRetry.Height - 20, 20, 20, 90, 90);
            path.CloseFigure();
            btnRetry.Region = new Region(path);

            // Menambahkan label di bawah tombol
            Label lblInfo = new Label();
            lblInfo.Name = "ErrorMsg";
            lblInfo.Text = ex.ToString(); // Teks yang ingin ditampilkan
            lblInfo.Font = new Font("Arial", 9, FontStyle.Regular);
            lblInfo.ForeColor = Color.Black; // Warna teks label
            lblInfo.AutoSize = true; // Agar label menyesuaikan ukuran teks

            // Mengatur posisi label agar berada di tengah
            lblInfo.Location = new Point((this.ClientSize.Width - lblInfo.Width) / 4, btnRetry.Bottom + 10); // Posisi di bawah tombol

            // Menambahkan kontrol ke form
            this.Controls.Add(pictureBox); // Menambahkan PictureBox ke form
            this.Controls.Add(btnRetry);
            this.Controls.Add(lblInfo); // Menambahkan label ke form

            btnRetry.BringToFront();
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            // Hapus tombol retry
            if (sender is System.Windows.Forms.Button btnRetry)
            {
                this.Controls.Remove(btnRetry);
            }

            // Hapus label informasi jika ada
            var lblInfo = this.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "ErrorMsg");
            if (lblInfo != null)
            {
                this.Controls.Remove(lblInfo);
            }

            var ErrImg = this.Controls.OfType<PictureBox>().FirstOrDefault(lbl => lbl.Name == "ErrorImg");
            if (ErrImg != null)
            {
                this.Controls.Remove(ErrImg);
            }

            // Muat data kembali
            LoadData();
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

