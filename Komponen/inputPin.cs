using System.Data;
using System.Net.NetworkInformation;
using KASIR.Model;
using KASIR.Network;
using KASIR.Printer;
using Newtonsoft.Json;
using Serilog;
namespace KASIR.Komponen
{
    public partial class inputPin : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private readonly string baseOutlet;
        private readonly string MacAddressKasir;
        private readonly string MacAddressKitchen;
        private readonly string MacAddressBar;
        private readonly string PinPrinterKasir;
        private readonly string PinPrinterKitchen;
        private readonly string PinPrinterBar;
        private PrinterModel printerModel; // Pastikan ini telah diinisialisasi dengan benar

        private readonly string BaseOutletName;
        string cartId;
        GetTransactionDetail dataTransaction;
        private List<CartDetailTransaction> item = new List<CartDetailTransaction>();
        private List<RefundDetailTransaction> refundDetails = new List<RefundDetailTransaction>();
        private List<RefundModel> refundItems = new List<RefundModel>();
        public bool ReloadDataInBaseForm { get; private set; }

        int idid;
        int totalTransactions;
        public inputPin(int id, int urutanRiwayat)
        {
            idid = id;
            totalTransactions = urutanRiwayat;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            MacAddressKitchen = Properties.Settings.Default.MacAddressKitchen;
            MacAddressBar = Properties.Settings.Default.MacAddressBar;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            PinPrinterKitchen = Properties.Settings.Default.PinPrinterKitchen;
            PinPrinterBar = Properties.Settings.Default.PinPrinterBar;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
            InitializeComponent();



            LoadData();
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;

        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPin_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private async void btnKonfirmasi_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnKonfirmasi_Click));

            try
            {

                if (textPin.Text.ToString() == "" || textPin.Text == null)
                {
                    MessageBox.Show("Masukan pin terlebih dahulu", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var json = new
                {
                    outlet_id = baseOutlet,
                    pin = textPin.Text
                };
                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                IApiService apiService = new ApiService();
                HttpResponseMessage response = await apiService.inputPin(jsonString, "/check-pin");
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        // DialogResult result = MessageBox.Show("Autentikasi berhasil", "Gaspol", MessageBoxButtons.OK);
                        // ReloadDataInBaseForm = true;
                        this.Close();
                        Form background = new Form
                        {
                            StartPosition = FormStartPosition.CenterScreen,
                            FormBorderStyle = FormBorderStyle.None,
                            Opacity = 0.7d,
                            BackColor = Color.Black,
                            WindowState = FormWindowState.Maximized,
                            TopMost = true,
                            Location = this.Location,
                            ShowInTaskbar = false,
                        };
                        using (refund refund = new refund(idid.ToString()))
                        {
                            refund.Owner = background;

                            background.Show();

                            DialogResult dialogResult = refund.ShowDialog();

                            background.Dispose();

                            if (dialogResult == DialogResult.OK)
                            {
                                this.Close();
                            }
                            else
                            {
                                this.Close();
                            }
                        }
                    }
                    else
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            MessageBox.Show("PIN salah. Silakan coba lagi.", "Kesalahan Autentikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            MessageBox.Show($"Terjadi kesalahan: {response.ReasonPhrase}", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Tidak ada respons dari server. Silakan coba lagi nanti.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Autentikasi gagal", "Gaspol", MessageBoxButtons.OK);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0) // Memastikan kita hanya memformat sel data
            {
                if (e.RowIndex % 2 == 0) // Baris genap
                {
                    e.CellStyle.BackColor = Color.White; // Warna untuk baris genap
                }
                else // Baris ganjil
                {
                    e.CellStyle.BackColor = Color.WhiteSmoke; // Warna untuk baris ganjil
                }
            }
        }
        private async void LoadData()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("No network connection available. Please check your internet connection and try again.", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            const int maxRetryAttempts = 3;
            int retryAttempts = 0;
            bool success = false;

            while (retryAttempts < maxRetryAttempts && !success)
            {
                try
                {
                    IApiService apiService = new ApiService();
                    string response = await apiService.GetActiveCart("/transaction/" + idid + "?outlet_id=" + baseOutlet);
                    if (response != null)
                    {
                        GetTransactionDetail transactionDetail = JsonConvert.DeserializeObject<GetTransactionDetail>(response);
                        DataTransaction data = transactionDetail.data;
                        dataTransaction = transactionDetail;
                        cartId = data.cart_id.ToString();
                        lblCustomerReceipt.Text = data.receipt_number.ToString();
                        lblWaktu.Text = data.invoice_due_date.ToString();
                        lblCustomerName.Text = data.customer_name;
                        lblCustomerSeat.Text = data.customer_seat.ToString();
                        lblPaymentType.Text = "Payment Type  : " + data.payment_type;
                        lblDiscountCode.Text = "Discount Code  : " + "-";
                        lblDiscountValue.Text = "Discount Value : " + "-";
                        lblTotalRefund.Text = "Total Refund           : -";
                        lblDiscountPrice.Text = "Discount Type : " + "-";
                        if (data.discount_code != null) lblDiscountCode.Text = "Discount Code : " + data.discount_code;
                        if (data.discount_code != null) lblDiscountValue.Text = "Discount Value : " + (data.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", data.discounts_value.ToString()) : data.discounts_value.ToString() + " %");
                        if (data.discount_code != null) lblDiscountPrice.Text = "Discount Price  : " + (string.Format("{0:n0}", data.total - data.subtotal));
                        if (data.total_refund != null) lblTotalRefund.Text = "Total Refund           : " + string.Format("{0:n0}", data.total_refund);

                        lblTotal.Text = "Total                          : " + string.Format("{0:n0}", data.total);
                        lblCustomerCash.Text = "Customer Cash        : " + string.Format("{0:n0}", data.customer_cash);
                        lblKembalian.Text = "Customer Change  : " + string.Format("{0:n0}", data.customer_change);
                        item = data.cart_details;

                        if (data.is_refund_all == "true" || data.is_refund_all == "1")
                        {
                            btnSimpan.Enabled = false;
                            btnSimpan.Text = "Item Kosong!";
                            btnSimpan.BackColor = Color.Gainsboro;
                        }
                        refundDetails = data.refund_details;
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("MenuID", typeof(string));
                        dataTable.Columns.Add("CartDetailID", typeof(int));
                        dataTable.Columns.Add("Jenis", typeof(string));
                        dataTable.Columns.Add("Menu", typeof(string));
                        dataTable.Columns.Add("Total Harga", typeof(string));
                        dataTable.Columns.Add("Note", typeof(string));
                        //dataTable.Rows.Clear();
                        dataTable.Rows.Add(null, null, null, "Sold items: -", null, null); // Add a separator row

                        foreach (var items in item)
                        {
                            dataTable.Rows.Add(items.menu_id, items.menu_detail_id, items.menu_type, items.qty + "X " + items.menu_name, string.Format("{0:n0}", items.total_price), null);
                            if (!string.IsNullOrEmpty(items.varian))
                            {
                                dataTable.Rows.Add(null, null, null, "    Varian : " + items.varian, null, null);
                            }

                            if (!string.IsNullOrEmpty(items.note_item))
                            {
                                dataTable.Rows.Add(null, null, null, "    *note : " + items.note_item, null, null);
                            }
                        }
                        dataTable.Rows.Add(null, null, null, " ", null, null); // Add a separator row

                        dataTable.Rows.Add(null, null, null, "Refund items: -", null, null); // Add a separator row

                        foreach (var items in refundDetails)
                        {
                            dataTable.Rows.Add(null, items.cart_detail_id, null, items.qty_refund_item + "X " + items.menu_name, "(Refunded) " + string.Format("{0:n0}", items.total_refund_price), null);

                            if (!string.IsNullOrEmpty(items.varian))
                            {
                                dataTable.Rows.Add(null, null, null, "    Varian : " + items.varian, null, null);
                            }
                            if (!string.IsNullOrEmpty(items.refund_reason_item))
                            {
                                dataTable.Rows.Add(null, null, null, "    *reason : " + items.refund_reason_item.ToString(), null, null);
                            }
                        }

                        dataGridView1.DataSource = dataTable;

                        if (dataGridView1.Columns.Contains("MenuID"))
                        {
                            dataGridView1.Columns["MenuID"].Visible = false;
                        }
                        if (dataGridView1 != null && dataGridView1.Columns.Contains("Menu"))
                        {
                            DataGridViewCellStyle boldStyle = new DataGridViewCellStyle();
                            boldStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

                            dataGridView1.Columns["Menu"].DefaultCellStyle = boldStyle;
                            dataGridView1.Columns["Menu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            dataGridView1.Columns["CartDetailID"].Visible = false;
                            dataGridView1.Columns["Jenis"].Visible = false;
                            dataGridView1.Columns["Note"].Visible = false;
                        }
                        success = true; // Successfully loaded data
                    }
                }
                catch (TaskCanceledException ex)
                {
                    MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    break; // Do not retry on TaskCanceledException
                }
                catch (NullReferenceException ex)
                {
                    retryAttempts++;
                    if (retryAttempts >= maxRetryAttempts)
                    {
                        MessageBox.Show("A null reference error occurred: " + ex.Message, "Null Reference Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoggerUtil.LogError(ex, "A null reference error occurred: {ErrorMessage}", ex.Message);
                        break; // Stop retrying after max attempts
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal load Cart " + ex.Message);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    break; // Stop retrying on other exceptions
                }
            }
        }

        private async void btnCetakStruk_Click(object sender, EventArgs e)
        {
            btnCetakStruk.Text = "Mencetak...";

            // Memastikan agar fungsi tidak melanjutkan jika idid tidak valid
            if (idid <= 0) // Asumsikan idid harus lebih besar dari 0
            {
                MessageBox.Show("ID tidak valid", "Error");
                return;
            }

            int maxRetryAttempts = 3; // Tentukan maksimal percobaan ulang
            int retryDelay = 2000; // Waktu tunggu antara percobaan dalam milidetik (contoh: 2000ms = 2 detik)
            string response = string.Empty;

            // Mencoba memanggil API beberapa kali
            for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
            {
                try
                {
                    IApiService apiService = new ApiService();
                    response = await apiService.Restruk("/transaction/" + idid + "?outlet_id=" + baseOutlet);

                    // Log the response for debugging purposes
                    Console.WriteLine($"Attempt {attempt}: Response: " + response);

                    // Jika response valid (tidak null, whitespace, dan mengandung "data")
                    if (!string.IsNullOrWhiteSpace(response) && response.Contains("data"))
                    {
                        break;  // Keluar dari loop jika respons valid
                    }

                    // Jika respons tidak sesuai, tunggu sejenak dan coba lagi
                    if (attempt < maxRetryAttempts)
                    {
                        MessageBox.Show("Response tidak valid, mencoba lagi...", "Error");
                        await Task.Delay(retryDelay); // Tunggu sebelum mencoba lagi
                    }
                    else
                    {
                        MessageBox.Show("Gagal cetak ulang struk setelah beberapa percobaan.", "Error");
                        return; // Keluar jika semua percobaan gagal
                    }
                }
                catch (Exception ex)
                {
                    // Tangani kesalahan jaringan atau lainnya
                    if (attempt == maxRetryAttempts)
                    {
                        MessageBox.Show("Gagal cetak ulang struk: " + ex.Message, "Error");
                        LoggerUtil.LogError(ex, "An error occurred during the retry attempts: {ErrorMessage}", ex.Message);
                        return;
                    }

                    // Jika terjadi kesalahan selain pada percobaan terakhir, coba lagi
                    await Task.Delay(retryDelay);
                }
            }

            try
            {
                RestrukModel restrukModel = JsonConvert.DeserializeObject<RestrukModel>(response);

                // Check if deserialization was successful
                if (restrukModel == null || restrukModel.data == null)
                {
                    MessageBox.Show("Deserialization gagal: data tidak ada.", "Error");
                    return;
                }

                DataRestruk data = restrukModel.data;
                List<CartDetailRestruk> cartDetails = data.cart_details;
                List<RefundDetailRestruk> cartRefundDetails = data.refund_details;
                List<CanceledItemStrukCustomerRestruk> canceledItems = data.canceled_items;

                // Memanggil metode untuk menangani pencetakan
                await HandlePrint(data, cartDetails, cartRefundDetails, canceledItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal cetak ulang struk " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred during the printing process: {ErrorMessage}", ex.Message);
            }
            finally
            {
                btnCetakStruk.Text = "Cetak Struk"; // Mengembalikan teks button
            }
        }



        private async Task HandlePrint(DataRestruk data, List<CartDetailRestruk> cartDetails, List<RefundDetailRestruk> cartRefundDetails, List<CanceledItemStrukCustomerRestruk> canceledItems)
        {
            PrinterModel printerModel = new PrinterModel();

            if (printerModel != null)
            {
                await Task.Run(() =>
                {
                    printerModel.PrintModelInputPin(data, cartDetails, cartRefundDetails, canceledItems, totalTransactions);
                });
            }
            else
            {
                throw new InvalidOperationException("printerModel is null");
            }
        }


        private void texttPin_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }
    }
}
