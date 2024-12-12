
using FontAwesome.Sharp;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
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
using Serilog;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Globalization;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using KASIR.Printer;
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
                        string currentJenis = null;
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
        private string CenterText(string text)
        {
            int spaces = Math.Max(0, (32 - text.Length) / 2);
            return new string(' ', spaces) + text;
        }

        // Fungsi untuk memformat baris dengan dua kolom (key, value)
        private string FormatSimpleLine(string left, object right)
        {
            // Jika objek right null, maka atur rightString sebagai string kosong
            string rightString = right != null ? right.ToString() : string.Empty;

            // Hitung panjang teks yang seharusnya ditambahkan ke kiri
            int paddingLength = Math.Max(0, 32 - rightString.Length);

            // Format baris dengan padding dan alignment yang benar
            string formattedLine = left.PadRight(paddingLength) + rightString;

            return formattedLine;
        }

        // Fungsi untuk memformat baris dengan tiga kolom (Item, Kuantitas, Harga)
        private string FormatItemLine(string item, object quantity, object price)
        {
            int column1Width = 20; // Adjust as needed
            int column2Width = 6;  // Adjust as needed

            string quantityString = quantity.ToString() + "x ";
            string priceString = price.ToString();

            // Format the line with padding and alignment
            string formattedLine = "\x1B\x45\x01" + item.PadRight(column1Width) + "\x1B\x45\x00" +
                                   quantityString.PadLeft(column2Width) +
                                   priceString.PadLeft(priceString.Length);
            return formattedLine;
        }

        // Fungsi untuk memformat baris dengan dua kolom detail item
        private string FormatDetailItemLine(string column1, object column2)
        {
            // Gabungkan kolom menjadi satu string dengan format "column1: column2"
            string combinedColumns = column1 + ": " + column2.ToString();

            // Jika panjang kombinasi kolom lebih dari 32 karakter
            if (combinedColumns.Length > 32)
            {
                // Inisialisasi string untuk menyimpan hasil yang akan dikembalikan
                string formattedLine = "";

                // Hitung berapa karakter yang masih dapat dimasukkan ke baris ini
                int charactersToFitInLine = 32;

                // Indeks untuk memulai bagian berikutnya dari teks yang akan diproses
                int startIndex = 0;

                while (startIndex < combinedColumns.Length)
                {
                    // Bagian berikutnya dari teks yang akan diproses
                    string nextPart = combinedColumns.Substring(startIndex, Math.Min(charactersToFitInLine, combinedColumns.Length - startIndex));

                    // Tambahkan ke baris yang akan dikembalikan
                    formattedLine += nextPart;

                    // Periksa apakah masih ada lebih banyak teks yang harus diproses
                    if (startIndex + nextPart.Length < combinedColumns.Length)
                    {
                        // Tambahkan newline (\n) jika masih ada teks yang harus diproses
                        formattedLine += "\n";

                        // Sisakan karakter yang dapat dimasukkan ke baris berikutnya
                        charactersToFitInLine = 32;
                    }

                    // Perbarui indeks untuk memulai bagian berikutnya
                    startIndex += nextPart.Length;
                }

                return formattedLine;
            }
            else
            {
                // Jika panjang tidak melebihi 32 karakter, langsung lakukan padding
                int paddingSpaces = 32 - combinedColumns.Length;
                string formattedLine = "".PadLeft(paddingSpaces) + combinedColumns;
                return formattedLine;
            }
        }

        private string FormatKitchenBarLine(string left, object right)
        {
            // Jika objek right null, maka atur rightString sebagai string kosong
            string rightString = right.ToString();

            // Tambahkan tanda kurung buka dan tutup ke string "left"
            left = "( " + left + " )";

            // Gabungkan string "left" dengan string "right" tanpa spasi tambahan di antaranya
            string formattedLine = left + " " + rightString;

            return formattedLine;
        }

        // Struct Pembayaran
        private async void PrintPurchaseReceipt(DataRestruk datas, List<CartDetailRestruk> cartDetails, List<RefundDetailRestruk> cartRefundDetails, List<CanceledItemStrukCustomerRestruk> canceledItems)
        {
            try
            {
                BluetoothDeviceInfo printer = new BluetoothDeviceInfo(BluetoothAddress.Parse(MacAddressKasir));
                if (printer == null)
                {
                    MessageBox.Show("Printer" + MacAddressKasir + "not found.", "Gaspol");
                    return;
                }

                BluetoothClient client = new BluetoothClient();
                BluetoothEndPoint endpoint = new BluetoothEndPoint(printer.DeviceAddress, BluetoothService.SerialPort);

                using (BluetoothClient clientSocket = new BluetoothClient())
                {
                    if (!BluetoothSecurity.PairRequest(printer.DeviceAddress, PinPrinterKasir))
                    {
                        MessageBox.Show("Pairing failed to " + MacAddressKasir, "Gaspol");
                        return;
                    }
                    clientSocket.Connect(endpoint);
                    // Kode setelah koneksi berhasil
                    System.IO.Stream stream = clientSocket.GetStream();

                    // Custom variable
                    string kodeHeksadesimalBold = "\x1B\x45\x01";
                    string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                    string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                    string nomorMeja = "Meja No." + datas.customer_seat;

                    // Struct template
                    string strukText = "\n" + kodeHeksadesimalBold + CenterText(BaseOutletName) + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += "--------------------------------\n";

                    strukText += kodeHeksadesimalSizeBesar + CenterText("Checker") + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += CenterText(datas.receipt_number) + "\n";
                    strukText += FormatSimpleLine(datas.customer_name, nomorMeja) + "\n";

                    strukText += "--------------------------------\n";
                    strukText += kodeHeksadesimalBold + CenterText("Sold Items") + "\n";
                    strukText += kodeHeksadesimalNormal;

                    // Iterate through cart details and group by serving_type_name
                    var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();

                    foreach (var servingType in servingTypes)
                    {
                        // Filter cart details by serving_type_name
                        var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                        // Skip if there are no items for this serving type
                        if (itemsForServingType.Count == 0)
                            continue;

                        // Add a section for the serving type
                        strukText += "--------------------------------\n";
                        strukText += CenterText(servingType) + "\n";
                        strukText += "--------------------------------\n";

                        // Iterate through items for this serving type
                        foreach (var cartDetail in itemsForServingType)
                        {
                            strukText += FormatItemLine(cartDetail.menu_name, cartDetail.qty, string.Format("{0:n0}", cartDetail.total_price)) + "\n";
                            // Add detail items
                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                strukText += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";
                            if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                strukText += FormatDetailItemLine("Discount Code", cartDetail.discount_code) + "\n";
                            if (cartDetail.discounted_price != null)
                                strukText += FormatDetailItemLine("Total Discount", string.Format("{0:n0}", cartDetail.discounted_price)) + "\n";

                            // Add an empty line between items
                            strukText += "\n";
                        }
                    }

                    if (canceledItems.Count != 0)
                    {
                        // Iterate through cart details and group by serving_type_name
                        var cancelServingTypes = canceledItems.Select(cd => cd.serving_type_name).Distinct();
                        strukText += "\n--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText("Cancel Items") + "\n";
                        strukText += kodeHeksadesimalNormal;

                        foreach (var servingType in cancelServingTypes)
                        {
                            // Filter cart details by serving_type_name
                            var itemsForServingType = canceledItems.Where(cd => cd.serving_type_name == servingType).ToList();

                            // Skip if there are no items for this serving type
                            if (itemsForServingType.Count == 0)
                                continue;

                            // Add a section for the serving type
                            strukText += "--------------------------------\n";
                            strukText += CenterText(servingType) + "\n\n";

                            // Iterate through items for this serving type
                            foreach (var canceledItem in itemsForServingType)
                            {
                                strukText += FormatItemLine(canceledItem.menu_name, canceledItem.qty, string.Format("{0:n0}", canceledItem.total_price)) + "\n";
                                // Add detail items
                                if (!string.IsNullOrEmpty(canceledItem.varian))
                                    strukText += FormatDetailItemLine("Varian", canceledItem.varian) + "\n";
                                if (!string.IsNullOrEmpty(canceledItem.note_item?.ToString()))
                                    strukText += FormatDetailItemLine("Note", canceledItem.note_item) + "\n";
                                if (!string.IsNullOrEmpty(canceledItem.discount_code))
                                    strukText += FormatDetailItemLine("Discount Code", canceledItem.discount_code) + "\n";
                                if (canceledItem.discounted_price.HasValue && canceledItem.discounted_price != 0)
                                    strukText += FormatDetailItemLine("Total Discount", string.Format("{0:n0}", canceledItem.discounted_price)) + "\n";
                                if (!string.IsNullOrEmpty(canceledItem.cancel_reason))
                                    strukText += FormatDetailItemLine("Canceled Reason", canceledItem.cancel_reason) + "\n";
                                // Add an empty line between items
                                strukText += "\n";
                            }
                        }
                    }

                    if (cartRefundDetails.Count != 0)
                    {
                        strukText += "--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText("Refund Items") + "\n";
                        strukText += kodeHeksadesimalNormal;
                        var servingTypesRefund = cartRefundDetails.Select(cd => cd.serving_type_name).Distinct();
                        foreach (var servingTypeRefund in servingTypesRefund)
                        {
                            // Filter cart details by serving_type_name
                            var itemsForServingType = cartRefundDetails.Where(cd => cd.serving_type_name == servingTypeRefund).ToList();

                            // Skip if there are no items for this serving type
                            if (itemsForServingType.Count == 0)
                                continue;

                            // Add a section for the serving type
                            strukText += "--------------------------------\n";
                            strukText += CenterText(servingTypeRefund) + "\n";
                            strukText += "--------------------------------\n";

                            // Iterate through items for this serving type
                            foreach (var cartDetail in itemsForServingType)
                            {
                                strukText += FormatItemLine(cartDetail.menu_name, cartDetail.qty_refund_item, string.Format("{0:n0}", cartDetail.total_refund_price)) + "\n";
                                // Add detail items
                                if (!string.IsNullOrEmpty(cartDetail.varian))
                                    strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                                if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                    strukText += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";
                                if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                    strukText += FormatDetailItemLine("Discount Code", cartDetail.discount_code) + "\n";
                                if (cartDetail.discounted_price != null)
                                    strukText += FormatDetailItemLine("Total Discount", string.Format("{0:n0}", cartDetail.discounted_price)) + "\n";
                                if (cartDetail.payment_type_name != null)
                                    strukText += FormatDetailItemLine("Payment Type", cartDetail.payment_type_name) + "\n";
                                if (cartDetail.refund_reason_item != null)
                                    strukText += FormatDetailItemLine("Refund Reason", cartDetail.refund_reason_item) + "\n";
                                // Add an empty line between items
                                strukText += "\n";
                            }
                        }
                    }
                    strukText += "--------------------------------\n";
                    strukText += FormatSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal)) + "\n";
                    if (!string.IsNullOrEmpty(datas.discount_code))
                        strukText += FormatSimpleLine("Discount Code", datas.discount_code) + "\n";
                    if (datas.discounts_value != null)
                        strukText += FormatSimpleLine("Discount Value", datas.discounts_value) + "\n";
                    strukText += FormatSimpleLine("Total", string.Format("{0:n0}", datas.total)) + "\n";
                    strukText += FormatSimpleLine("Payment Type", datas.payment_type) + "\n";
                    strukText += FormatSimpleLine("Cash", string.Format("{0:n0}", datas.customer_cash)) + "\n";
                    strukText += FormatSimpleLine("Change", string.Format("{0:n0}", datas.customer_change)) + "\n";
                    if (datas.total_refund != null)
                        strukText += FormatSimpleLine("Total Refund", datas.total_refund) + "\n";
                    strukText += "--------------------------------\n";
                    strukText += "Terima kasih atas kunjungan Anda\n\n\n\n\n";

                    // Encode your text into bytes (you might need to adjust the encoding)
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

                    // Send the text to the printer
                    stream.Write(buffer, 0, buffer.Length);

                    // Flush the stream to ensure all data is sent to the printer
                    stream.Flush();

                    // Close the stream and disconnect
                    clientSocket.GetStream().Close();
                    stream.Close();
                    clientSocket.Close();
                }

            }
            catch (Exception ex)
            {
               // MessageBox.Show("Gagal cetak ulang struk " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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

        /*     private async void btnCetakStruk_Click(object sender, EventArgs e)
             {
                 btnCetakStruk.Text = "Mencetak...";

                 // Memastikan agar fungsi tidak melanjutkan jika idid tidak valid
                 if (idid <= 0) // Asumsikan idid harus lebih besar dari 0
                 {
                     MessageBox.Show("ID tidak valid", "Error");
                     return;
                 }

                 try
                 {
                     IApiService apiService = new ApiService();
                     string response = await apiService.Restruk("/transaction/" + idid + "?outlet_id=" + baseOutlet);

                     if (string.IsNullOrWhiteSpace(response))
                     {
                         MessageBox.Show("Gagal cetak ulang struk. Response tidak valid.", "Error");
                         return;
                     }

                     // Log the response for debugging purposes
                     Console.WriteLine("Response: " + response); // Or use a logger for better tracking

                     RestrukModel restrukModel = JsonConvert.DeserializeObject<RestrukModel>(response);

                     // Check if deserialization was successful
                     if (restrukModel == null)
                     {
                         throw new InvalidOperationException("Deserialization failed: restrukModel is null");
                     }

                     // Check if the 'data' field is null
                     if (restrukModel.data == null)
                     {
                         throw new InvalidOperationException("Deserialization failed: data is null");
                     }

                     DataRestruk data = restrukModel.data;
                     List<CartDetailRestruk> cartDetails = data.cart_details;
                     List<RefundDetailRestruk> cartRefundDetails = data.refund_details;
                     List<CanceledItemStrukCustomerRestruk> canceledItems = data.canceled_items;

                     // Memanggil metode untuk menangani pencetakan
                     await HandlePrint(data, cartDetails, cartRefundDetails, canceledItems);
                 }
                 catch (TaskCanceledException ex)
                 {
                     MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                 }
                 catch (InvalidOperationException ex)
                 {
                     MessageBox.Show("Gagal cetak ulang struk: " + ex.Message, "Gaspol");
                     LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show("Gagal cetak ulang struk " + ex.Message, "Gaspol");
                     LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                 }
                 finally
                 {
                     btnCetakStruk.Text = "Cetak Struk"; // Mengembalikan teks button
                 }
             }*/

        private async void Ex_btnCetakStruk_Click(object sender, EventArgs e)
        {
            btnCetakStruk.Text = "Mencetak...";

            // Memastikan agar fungsi tidak melanjutkan jika idid tidak valid
            if (idid <= 0) // Asumsikan idid harus lebih besar dari 0
            {
                MessageBox.Show("ID tidak valid", "Error");
                return;
            }

            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.Restruk("/transaction/" + idid + "?outlet_id=" + baseOutlet);

                if (string.IsNullOrWhiteSpace(response))
                {
                    MessageBox.Show("Gagal cetak ulang struk. Response tidak valid.", "Error");
                    return;
                }

                RestrukModel restrukModel = JsonConvert.DeserializeObject<RestrukModel>(response);

                if (restrukModel == null || restrukModel.data == null)
                {
                    throw new InvalidOperationException("Deserialization failed: restrukModel or data is null");
                }

                DataRestruk data = restrukModel.data;
                List<CartDetailRestruk> cartDetails = data.cart_details;
                List<RefundDetailRestruk> cartRefundDetails = data.refund_details;
                List<CanceledItemStrukCustomerRestruk> canceledItems = data.canceled_items;

                // Memanggil metode untuk menangani pencetakan
                await HandlePrint(data, cartDetails, cartRefundDetails, canceledItems);
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal cetak ulang struk " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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
