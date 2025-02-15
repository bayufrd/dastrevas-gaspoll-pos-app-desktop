
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
using Newtonsoft.Json.Linq;
using System.Transactions;
namespace KASIR.OfflineMode
{
    public partial class Offline_inputPin : Form
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
        string transactionId;
        public Offline_inputPin(string id, int urutanRiwayat)
        {
            transactionId = id.ToString();
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

       

            LoadData(transactionId);
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
                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                var dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);

                if (textPin.Text.ToString() == dataOutlet.data.pin.ToString())
                {
                       
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
                        using (Offline_refund Offline_refund = new Offline_refund(idid.ToString()))
                        {
                            Offline_refund.Owner = background;

                            background.Show();

                            DialogResult dialogResult = Offline_refund.ShowDialog();

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
                    lblKonfirmasi.Text = "Pin/Password salah!";
                    lblKonfirmasi.ForeColor = Color.Red;
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
        private async void LoadData(string transactionId)
        {
            
                try
            {
                // Path untuk file transaction.data
                string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";

                // Cek apakah file transaction.data ada
                if (File.Exists(transactionDataPath))
                {
                    // Membaca isi file transaction.data
                    string transactionJson = File.ReadAllText(transactionDataPath);
                    var transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                    // Ambil array data transaksi
                    var transactionDetails = transactionData["data"] as JArray;

                    // Filter transaksi berdasarkan transaction_id
                    var filteredTransaction = transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == transactionId);

                    if (filteredTransaction != null)
                    {
                        // Menampilkan data transaksi berdasarkan transaction_id
                        string receiptNumber = filteredTransaction["receipt_number"]?.ToString() ?? "-";
                        string customerName = filteredTransaction["customer_name"]?.ToString() ?? "-";
                        string customerSeat = filteredTransaction["customer_seat"]?.ToString() ?? "0";
                        decimal total = filteredTransaction["total"] != null ? decimal.Parse(filteredTransaction["total"].ToString()) : 0;
                        string paymentType = filteredTransaction["payment_type_name"]?.ToString() ?? "-";
                        DateTime transactionTime;
                        string formattedDate = "-";

                        if (DateTime.TryParse(filteredTransaction["created_at"]?.ToString(), out transactionTime))
                        {
                            formattedDate = transactionTime.ToString("dd MMM yyyy, HH:mm");
                        }

                        // Menampilkan data transaksi di UI
                        lblCustomerReceipt.Text = receiptNumber;
                        lblWaktu.Text = formattedDate;
                        lblCustomerName.Text = customerName;
                        lblCustomerSeat.Text = customerSeat;
                        lblPaymentType.Text = "Payment Type: " + paymentType;
                        lblTotal.Text = "Total: " + string.Format("{0:n0}", total);
                        decimal cash = filteredTransaction["total"] != null ? decimal.Parse(filteredTransaction["customer_cash"].ToString()) : 0;
                        decimal kembalian = filteredTransaction["customer_change"] != null ? decimal.Parse(filteredTransaction["customer_change"].ToString()) : 0;
                        decimal refund = filteredTransaction["customer_change"] != null ? decimal.Parse(filteredTransaction["customer_change"].ToString()) : 0;

                        lblDiscountCode.Text = "Discount Code: " + "-";
                        lblDiscountValue.Text = "Discount Value: " + "-";
                        lblDiscountPrice.Text = "Discount Price: " + "-";
                        lblCustomerCash.Text = "Customer Cash: " + string.Format("{0:n0}", cash);
                        lblKembalian.Text = "Change: " + string.Format("{0:n0}", kembalian);
                        lblTotalRefund.Text = "Refund: " + string.Format("{0:n0}", "0");

                        // Ambil data cart_details dan refund_details
                        var cartDetails = filteredTransaction["cart_details"] as JArray;
                        var refundDetails = filteredTransaction["refund_details"] as JArray;

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("MenuID", typeof(string));
                        dataTable.Columns.Add("CartDetailID", typeof(int));
                        dataTable.Columns.Add("Jenis", typeof(string));
                        dataTable.Columns.Add("Menu", typeof(string));
                        dataTable.Columns.Add("Total Harga", typeof(string));
                        dataTable.Columns.Add("Note", typeof(string));

                        // Tambahkan separator untuk item yang terjual
                        dataTable.Rows.Add(null, null, null, "Sold items: -", null, null);

                        foreach (var item in cartDetails)
                        {
                            dataTable.Rows.Add(
                                item["menu_id"]?.ToString(),
                                item["cart_detail_id"]?.ToObject<int>(),
                                item["menu_type"]?.ToString(),
                                $"{item["qty"]}X {item["menu_name"]}",
                                string.Format("{0:n0}", item["total_price"]),
                                item["note_item"]?.ToString()
                            );
                        }

                        /*// Tambahkan separator untuk item refund
                        dataTable.Rows.Add(null, null, null, "Refund items: -", null, null);

                        foreach (var refundItem in refundDetails)
                        {
                            dataTable.Rows.Add(
                                null,
                                refundItem["cart_detail_id"]?.ToObject<int>(),
                                null,
                                $"{refundItem["qty_refund_item"]}X {refundItem["menu_name"]}",
                                "(Refunded) " + string.Format("{0:n0}", refundItem["total_refund_price"]),
                                refundItem["refund_reason_item"]?.ToString()
                            );
                        }*/

                        // Menampilkan data pada DataGridView
                        dataGridView1.DataSource = dataTable;

                        // Menyembunyikan kolom yang tidak diperlukan
                        if (dataGridView1.Columns.Contains("MenuID"))
                        {
                            dataGridView1.Columns["MenuID"].Visible = false;
                        }
                        if (dataGridView1.Columns.Contains("CartDetailID"))
                        {
                            dataGridView1.Columns["CartDetailID"].Visible = false;
                        }
                        if (dataGridView1.Columns.Contains("Jenis"))
                        {
                            dataGridView1.Columns["Jenis"].Visible = false;
                        }
                        if (dataGridView1.Columns.Contains("Note"))
                        {
                            dataGridView1.Columns["Note"].Visible = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Transaction with the specified ID not found.");
                    }
                }
                else
                {
                    MessageBox.Show("Transaction data file not found.");
                }
            }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal load Cart " + ex.Message);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            
        }
        private async void btnCetakStruk_Click(object sender, EventArgs e)
        {
            btnCetakStruk.Text = "Mencetak...";

            string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";

            // Memastikan file ada
            if (!File.Exists(transactionDataPath))
            {
                MessageBox.Show("File transaction.data tidak ditemukan.", "Error");
                return;
            }

            try
            {
                // Membaca file transaction.data
                string jsonData = File.ReadAllText(transactionDataPath);
                TransactionCache restrukModel = JsonConvert.DeserializeObject<TransactionCache>(jsonData);

                if (restrukModel == null || restrukModel.data == null || restrukModel.data.Count == 0)
                {
                    MessageBox.Show("Tidak ada data transaksi di dalam file.", "Error");
                    return;
                }

                // Mencari transaksi yang sesuai dengan transaction_id (idid)
                CartDataCache targetTransaksi = restrukModel.data
                    .FirstOrDefault(t => t.transaction_id == transactionId); // Mencocokkan dengan transactionId yang diberikan

                if (targetTransaksi == null)
                {
                    MessageBox.Show("Transaksi dengan ID tersebut tidak ditemukan.", "Error");
                    return;
                }
                // Membuat DataRestruk untuk memetakan data dari CartDataCache
                DataRestruk dataRestruk = new DataRestruk
                {
                    // Pemetaan langsung dari CartDataCache ke DataRestruk
                    transaction_id = int.Parse(targetTransaksi.transaction_id), // Pastikan transaction_id dalam bentuk integer
                    receipt_number = targetTransaksi.receipt_number,
                    invoice_number = targetTransaksi.invoice_number,
                    payment_type = targetTransaksi.payment_type_name,
                    payment_category = targetTransaksi.payment_type_id.ToString(),
                    customer_name = targetTransaksi.customer_name,
                    customer_seat = targetTransaksi.customer_seat,
                    customer_cash = targetTransaksi.customer_cash,
                    total = targetTransaksi.total,
                    subtotal = targetTransaksi.subtotal,
                    discount_id = targetTransaksi.discount_id.ToString(),
                    discount_code = targetTransaksi.discount_code,
                    discounts_value = targetTransaksi.discounts_value,
                    discounts_is_percent = targetTransaksi.discounts_is_percent,
                    invoice_due_date = targetTransaksi.invoice_due_date,
                    cart_details = MapCartDetails(targetTransaksi.cart_details), // Mengonversi CartDetail ke CartDetailRestruk
                    canceled_items = MapCanceledItems(targetTransaksi.canceled_items), // Menyertakan canceled items jika ada
                    refund_details = MapRefundDetails(targetTransaksi.refund_details), // Menyertakan refund details jika ada
                    customer_change = targetTransaksi.customer_change ?? 0, // Handling nullable values
                    refund_reason = targetTransaksi.refund_reason ?? string.Empty, // Handling null value for refund_reason
                };
                // Memanggil metode untuk menangani pencetakan
                await HandlePrint(dataRestruk, MapCartDetails(targetTransaksi.cart_details), dataRestruk.refund_details, dataRestruk.canceled_items); // Menyertakan refund dan canceled items jika ada
                this.Close(); // Menutup form setelah proses selesai
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca atau memproses file transaction.data: " + ex.Message, "Error");
                LoggerUtil.LogError(ex, "An error occurred while processing transaction data: {ErrorMessage}", ex.Message);
            }
            finally
            {
                btnCetakStruk.Text = "Cetak Struk"; // Mengembalikan teks button
            }
        }

        private List<CartDetailRestruk> MapCartDetails(List<CartDetail> cartDetails)
        {
            List<CartDetailRestruk> cartDetailRestruks = new List<CartDetailRestruk>();

            foreach (var cartDetail in cartDetails)
            {
                cartDetailRestruks.Add(new CartDetailRestruk
                {
                    // Pemetaan langsung dari CartDetail ke CartDetailRestruk
                    cart_detail_id = int.Parse(cartDetail.cart_detail_id), // Pastikan cart_detail_id dalam bentuk integer
                    menu_id = cartDetail.menu_id,
                    menu_name = cartDetail.menu_name,
                    menu_type = cartDetail.menu_type,
                    menu_detail_id = cartDetail.menu_detail_id.ToString(),
                    varian = cartDetail.menu_detail_name,
                    serving_type_id = cartDetail.serving_type_id,
                    serving_type_name = cartDetail.serving_type_name,
                    price = int.Parse(cartDetail.price.ToString()), // Pastikan harga dalam bentuk integer
                    qty = cartDetail.qty,
                    note_item = cartDetail.note_item,
                    discount_id = 0, // Handling nullable values
                    discount_code = cartDetail.discount_code?.ToString() ?? string.Empty, // Jika discount_code null, akan digantikan dengan string kosong
                    discounts_value = 0, // Handling nullable values
                    discounted_price = 0, // Handling nullable values
                    discounts_is_percent = cartDetail.discounts_is_percent ?? string.Empty, // Handling nullable values
                    total_price = cartDetail.total_price,
                });
            }

            return cartDetailRestruks;
        }
        private List<CanceledItemStrukCustomerRestruk> MapCanceledItems(List<CanceledItem> canceledItems)
        {
            List<CanceledItemStrukCustomerRestruk> canceledItemRestruks = new List<CanceledItemStrukCustomerRestruk>();

            foreach (var canceledItem in canceledItems)
            {
                canceledItemRestruks.Add(new CanceledItemStrukCustomerRestruk
                {
                    cart_detail_id = canceledItem.cart_detail_id,
                    menu_id = canceledItem.menu_id,
                    menu_name = canceledItem.menu_name,
                    menu_type = canceledItem.menu_type,
                    menu_detail_id = canceledItem.menu_detail_id ?? new object(), // Handle nullable
                    varian = canceledItem.varian ?? string.Empty,
                    serving_type_id = canceledItem.serving_type_id,
                    serving_type_name = canceledItem.serving_type_name,
                    discount_id = canceledItem.discount_id,
                    discount_code = canceledItem.discount_code ?? string.Empty,
                    discounts_value = canceledItem.discounts_value ?? new object(),
                    discounted_price = canceledItem.discounted_price ?? 0,
                    discounts_is_percent = canceledItem.discounts_is_percent ?? new object(),
                    price = canceledItem.price,
                    total_price = canceledItem.total_price,
                    qty = canceledItem.qty,
                    note_item = canceledItem.note_item ?? new object(),
                    cancel_reason = canceledItem.cancel_reason ?? string.Empty
                });
            }

            return canceledItemRestruks;
        }
        private List<RefundDetailRestruk> MapRefundDetails(List<RefundDetail> refundDetails)
        {
            List<RefundDetailRestruk> refundDetailRestruks = new List<RefundDetailRestruk>();

            foreach (var refundDetail in refundDetails)
            {
                refundDetailRestruks.Add(new RefundDetailRestruk
                {
                    cart_detail_id = refundDetail.cart_detail_id,
                    refund_reason_item = refundDetail.refund_reason_item ?? string.Empty,
                    qty_refund_item = refundDetail.qty_refund_item,
                    total_refund_price = refundDetail.total_refund_price,
                    payment_type_name = refundDetail.payment_type_name ?? string.Empty,
                    payment_category_name = refundDetail.payment_category_name ?? string.Empty,
                    menu_name = refundDetail.menu_name ?? string.Empty,
                    varian = refundDetail.varian ?? string.Empty,
                    serving_type_name = refundDetail.serving_type_name ?? string.Empty,
                    discount_code = refundDetail.discount_code ?? string.Empty,
                    discounts_value = refundDetail.discounts_value ?? string.Empty,
                    discounted_price = refundDetail.discounted_price ?? string.Empty,
                    menu_price = refundDetail.menu_price,
                    note_item = refundDetail.note_item ?? string.Empty
                });
            }

            return refundDetailRestruks;
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
