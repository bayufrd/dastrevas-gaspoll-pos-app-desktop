﻿using System.Data;
using KASIR.Model;
using KASIR.Printer;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.OfflineMode
{
    public partial class Offline_inputPin : Form
    {
        private readonly string baseOutlet;

        private readonly int totalTransactions;
        private readonly string transactionId;

        public Offline_inputPin(string id, int urutanRiwayat)
        {
            transactionId = id;
            totalTransactions = urutanRiwayat;
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();

            LoadData(transactionId);
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        public bool ReloadDataInBaseForm { get; private set; }

        private void button11_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void btnKonfirmasi_Click(object sender, EventArgs e)
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
                    Close();
                    Form background = new()
                    {
                        StartPosition = FormStartPosition.CenterScreen,
                        FormBorderStyle = FormBorderStyle.None,
                        Opacity = 0.7d,
                        BackColor = Color.Black,
                        WindowState = FormWindowState.Maximized,
                        TopMost = true,
                        Location = Location,
                        ShowInTaskbar = false
                    };
                    using (Offline_refund Offline_refund = new(transactionId))
                    {
                        Offline_refund.Owner = background;

                        background.Show();

                        DialogResult dialogResult = Offline_refund.ShowDialog();

                        background.Dispose();

                        if (dialogResult == DialogResult.OK)
                        {
                            Close();
                        }
                        else
                        {
                            Close();
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
                string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";

                if (File.Exists(transactionDataPath))
                {
                    string transactionJson = File.ReadAllText(transactionDataPath);
                    JObject? transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                    JArray? transactionDetails = transactionData["data"] as JArray;

                    JToken? filteredTransaction =
                        transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == transactionId);

                    if (filteredTransaction != null)
                    {
                        string receiptNumber = filteredTransaction["receipt_number"]?.ToString() ?? "-";
                        string customerName = filteredTransaction["customer_name"]?.ToString() ?? "-";
                        string customerSeat = filteredTransaction["customer_seat"]?.ToString() ?? "0";
                        decimal total = filteredTransaction["total"] != null
                            ? decimal.Parse(filteredTransaction["total"].ToString())
                            : 0;
                        string paymentType = filteredTransaction["payment_type_name"]?.ToString() ?? "-";
                        DateTime transactionTime;
                        string formattedDate = "-";

                        if (DateTime.TryParse(filteredTransaction["created_at"]?.ToString(), out transactionTime))
                        {
                            formattedDate = transactionTime.ToString("dd MMM yyyy, HH:mm");
                        }

                        lblCustomerReceipt.Text = receiptNumber;
                        lblWaktu.Text = formattedDate;
                        lblCustomerName.Text = customerName;
                        lblCustomerSeat.Text = customerSeat;
                        lblPaymentType.Text = "Payment Type: " + paymentType;
                        lblTotal.Text = "Total: " + string.Format("{0:n0}", total);
                        int cash = filteredTransaction["customer_cash"] != null
                            ? int.Parse(filteredTransaction["customer_cash"].ToString())
                            : 0;
                        int kembalian = filteredTransaction["customer_change"] != null
                            ? int.Parse(filteredTransaction["customer_change"].ToString())
                            : 0;
                        int refund = filteredTransaction["total_refund"] != null
                            ? int.Parse(filteredTransaction["total_refund"].ToString())
                            : 0;
                        string? discountPrice = filteredTransaction["discounted_price"]?.ToString() != "0"
                            ? string.Format("{0:n0}", int.Parse(filteredTransaction["discounted_price"]?.ToString()))
                            : "-";
                        lblDiscountCode.Text = "Discount Code: ";
                        lblDiscountValue.Text = "Discount Value: ";
                        lblDiscountPrice.Text = "Discount Price: ";
                        lblCustomerCash.Text = "Customer Cash: ";
                        lblKembalian.Text = "Change: ";
                        lblTotalRefund.Text = "Refund: ";

                        lblDiscountCode.Text += filteredTransaction["discount_code"].ToString() != null
                            ? filteredTransaction["discount_code"].ToString()
                            : "-";
                        lblDiscountValue.Text += filteredTransaction["discounts_value"]?.ToString() != "0"
                            ? filteredTransaction["discounts_value"]?.ToString()
                            : "-";
                        lblDiscountPrice.Text += discountPrice;
                        lblCustomerCash.Text += string.Format("{0:n0}", cash);
                        lblKembalian.Text += string.Format("{0:n0}", kembalian);
                        lblTotalRefund.Text += string.Format("{0:n0}", refund);

                        // Ambil data cart_details dan refund_details
                        JArray? cartDetails = filteredTransaction["cart_details"] as JArray;
                        JArray? refundDetails = filteredTransaction["refund_details"] as JArray;
                        JArray? cancelDetails = filteredTransaction["canceled_items"] as JArray;

                        DataTable dataTable = new();
                        dataTable.Columns.Add("MenuID", typeof(string));
                        dataTable.Columns.Add("CartDetailID", typeof(int));
                        dataTable.Columns.Add("Jenis", typeof(string));
                        dataTable.Columns.Add("Menu", typeof(string));
                        dataTable.Columns.Add("Total Harga", typeof(string));

                        if (cartDetails != null && cartDetails.Count > 0)
                        {
                            // Tambahkan separator untuk item yang terjual
                            //dataTable.Rows.Add(null, null, null, "Sold items: ", null);
                            bool hasItems = false; // Variabel untuk memeriksa apakah ada item dengan qty > 0

                            AddSeparatorRow(dataTable, "  #Sold items: ", dataGridView1);

                            foreach (JToken item in cartDetails)
                            {
                                if (int.Parse(item["qty"].ToString()) != 0)
                                {
                                    hasItems = true; // Set menjadi true jika ada item dengan qty > 0

                                    dataTable.Rows.Add(
                                        item["menu_id"]?.ToString(),
                                        item["cart_detail_id"]?.ToObject<int>(),
                                        item["menu_type"]?.ToString(),
                                        $"{item["qty"]}x {item["menu_name"]} {item["menu_detail_name"]} {item["note_item"]}",
                                        string.Format("{0:n0}", item["total_price"])
                                    );
                                    if (!string.IsNullOrEmpty(item["note_item"].ToString()))
                                    {
                                        dataTable.Rows.Add(null, null, null, $"  *Notes: {item["note_item"]} ", null);
                                    }
                                }
                            }

                            int points = 0;
                            if (!string.IsNullOrEmpty(filteredTransaction["member_id"].ToString()) && !string.IsNullOrEmpty(filteredTransaction["member_name"].ToString()))
                            {
                                points = int.Parse(filteredTransaction["member_point"].ToString());
                                dataTable.Rows.Add(null, null, null, $"  *Data Member:", null);
                                dataTable.Rows.Add(null, null, null, $"    Member Name: {filteredTransaction["member_name"].ToString()} ", null);
                                dataTable.Rows.Add(null, null, null, $"    Member Points: {points.ToString("#,#")} ", null);
                                if (!string.IsNullOrEmpty(filteredTransaction["member_use_point"].ToString()))
                                {
                                    points = int.Parse(filteredTransaction["member_use_point"].ToString());
                                    dataTable.Rows.Add(null, null, null, $"    Member Use Points: {points.ToString("#,#")} ", null);
                                }
                            }

                            btnSimpan.Enabled = hasItems; // Aktifkan atau nonaktifkan tombol Simpan
                            if (btnSimpan.Enabled != true)
                            {
                                btnSimpan.Text = "No Item to refund!";
                                btnSimpan.BackColor = Color.Gainsboro;
                            }
                        }

                        if (refundDetails != null && refundDetails.Count > 0)
                        {
                            AddSeparatorRow(dataTable, "  #Refund items: ", dataGridView1);

                            foreach (JToken refundItem in refundDetails)
                            {
                                dataTable.Rows.Add(
                                    refundItem["menu_id"]?.ToString(),
                                    refundItem["cart_detail_id"]?.ToObject<int>(),
                                    refundItem["menu_type"]?.ToString(),
                                    $"{refundItem["refund_qty"]}x {refundItem["menu_name"]} {refundItem["menu_detail_name"]} {refundItem["note_item"]}",
                                    string.Format("{0:n0}", refundItem["refund_total"]) + " (Refunded)"
                                );
                                dataTable.Rows.Add(null, null, null, $"  *Reason: {refundItem["refund_reason_item"]} ",
                                    null);
                            }
                        }

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
                MessageBox.Show("Gagal load Cart " + ex);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void AddSeparatorRow(DataTable dataTable, string groupKey, DataGridView dataGridView)
        {
            // Tambahkan separator row ke DataTable
            dataTable.Rows.Add(null, null, null, groupKey + "\n", null); // Add a separator row

            // Ambil indeks baris terakhir yang baru saja ditambahkan
            int lastRowIndex = dataTable.Rows.Count - 1;

            // Menambahkan row ke DataGridView
            dataGridView.DataSource = dataTable;

            // Mengatur gaya sel untuk kolom tertentu
            int[] cellIndexesToStyle = { 1, 2, 3, 4 }; // Indeks kolom yang ingin diatur
            SetCellStyle(dataGridView.Rows[lastRowIndex], cellIndexesToStyle, Color.WhiteSmoke, FontStyle.Bold);
        }

        private void SetCellStyle(DataGridViewRow row, int[] cellIndexes, Color backgroundColor, FontStyle fontStyle)
        {
            foreach (int index in cellIndexes)
            {
                row.Cells[index].Style.BackColor = backgroundColor;
                row.Cells[index].Style.Font = new Font(dataGridView1.Font, fontStyle);
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
                    .FirstOrDefault(t =>
                        t.transaction_id == transactionId); // Mencocokkan dengan transactionId yang diberikan

                if (targetTransaksi == null)
                {
                    MessageBox.Show("Transaksi dengan ID tersebut tidak ditemukan.", "Error");
                    return;
                }

                // Membuat DataRestruk untuk memetakan data dari CartDataCache
                DataRestruk dataRestruk = new()
                {
                    // Pemetaan langsung dari CartDataCache ke DataRestruk
                    transaction_id =
                        int.Parse(targetTransaksi.transaction_id), // Pastikan transaction_id dalam bentuk integer
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
                    cart_details =
                        MapCartDetails(targetTransaksi.cart_details), // Mengonversi CartDetail ke CartDetailRestruk
                    canceled_items =
                        MapCanceledItems(targetTransaksi.canceled_items), // Menyertakan canceled items jika ada
                    refund_details =
                        MapRefundDetails(targetTransaksi.refund_details), // Menyertakan refund details jika ada
                    customer_change = targetTransaksi.customer_change ?? 0, // Handling nullable values
                    refund_reason =
                        targetTransaksi.refund_reason ?? string.Empty // Handling null value for refund_reason
                };
                // Memanggil metode untuk menangani pencetakan
                await HandlePrint(dataRestruk, MapCartDetails(targetTransaksi.cart_details), dataRestruk.refund_details,
                    dataRestruk.canceled_items); // Menyertakan refund dan canceled items jika ada
                Close(); // Menutup form setelah proses selesai
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca atau memproses file transaction.data: " + ex.Message, "Error");
                LoggerUtil.LogError(ex, "An error occurred while processing transaction data: {ErrorMessage}",
                    ex.Message);
            }
            finally
            {
                btnCetakStruk.Text = "Cetak Struk"; // Mengembalikan teks button
            }
        }

        private List<CartDetailRestruk> MapCartDetails(List<CartDetail> cartDetails)
        {
            List<CartDetailRestruk> cartDetailRestruks = new();

            foreach (CartDetail cartDetail in cartDetails)
            {
                cartDetailRestruks.Add(new CartDetailRestruk
                {
                    // Pemetaan langsung dari CartDetail ke CartDetailRestruk
                    cart_detail_id =
                        int.Parse(cartDetail.cart_detail_id), // Pastikan cart_detail_id dalam bentuk integer
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
                    discount_code =
                        cartDetail.discount_code?.ToString() ??
                        string.Empty, // Jika discount_code null, akan digantikan dengan string kosong
                    discounts_value = 0, // Handling nullable values
                    discounted_price = 0, // Handling nullable values
                    discounts_is_percent = cartDetail.discounts_is_percent ?? string.Empty, // Handling nullable values
                    total_price = cartDetail.total_price
                });
            }

            return cartDetailRestruks;
        }

        private List<CanceledItemStrukCustomerRestruk> MapCanceledItems(List<CanceledItem> canceledItems)
        {
            List<CanceledItemStrukCustomerRestruk> canceledItemRestruks = new();

            foreach (CanceledItem canceledItem in canceledItems)
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
            List<RefundDetailRestruk> refundDetailRestruks = new();

            foreach (RefundDetail refundDetail in refundDetails)
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

        private async Task HandlePrint(DataRestruk data, List<CartDetailRestruk> cartDetails,
            List<RefundDetailRestruk> cartRefundDetails, List<CanceledItemStrukCustomerRestruk> canceledItems)
        {
            using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(30))) // 30-second timeout
            {
                try
                {
                    PrinterModel printerModel = new();
                    if (printerModel == null)
                    {
                        throw new InvalidOperationException("printerModel is null");
                    }

                    // Save print job details for potential recovery
                    SaveInputPinPrintJobForRecovery(data, cartDetails, cartRefundDetails, canceledItems,
                        totalTransactions);

                    try
                    {
                        // Execute print operation with timeout
                        await Task.Run(async () =>
                        {
                            await printerModel.PrintModelInputPin(data, cartDetails, cartRefundDetails, canceledItems,
                                totalTransactions);
                        }, cts.Token);

                        // If successful, remove the saved print job
                        RemoveSavedInputPinPrintJob(totalTransactions);
                    }
                    catch (OperationCanceledException)
                    {
                        // The operation timed out
                        LoggerUtil.LogWarning("Input pin print operation timed out, will retry in background");

                        // Continue printing in background
                        ThreadPool.QueueUserWorkItem(async _ =>
                        {
                            try
                            {
                                // Use a new instance to avoid any shared state issues
                                PrinterModel backgroundPrinterModel = new();

                                await backgroundPrinterModel.PrintModelInputPin(data, cartDetails, cartRefundDetails,
                                    canceledItems, totalTransactions);

                                // If successful, remove the saved print job
                                RemoveSavedInputPinPrintJob(totalTransactions);
                            }
                            catch (Exception ex)
                            {
                                LoggerUtil.LogError(ex, "Background input pin printing failed: {ErrorMessage}",
                                    ex.Message);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred during input pin printing: {ErrorMessage}", ex.Message);
                    throw; // Rethrow to allow calling code to handle the error
                }
            }
        }

        // Helper methods for input pin print job persistence
        private void SaveInputPinPrintJobForRecovery(
            DataRestruk data,
            List<CartDetailRestruk> cartDetails,
            List<RefundDetailRestruk> cartRefundDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int transactionNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "InputPin");
                Directory.CreateDirectory(printJobsDir);

                InputPinPrintJob inputPinPrintJob = new()
                {
                    Data = data,
                    CartDetails = cartDetails,
                    CartRefundDetails = cartRefundDetails,
                    CanceledItems = canceledItems,
                    TransactionNumber = transactionNumber,
                    Timestamp = DateTime.Now
                };

                string filename = Path.Combine(printJobsDir,
                    $"InputPinPrintJob_{transactionNumber}_{DateTime.Now.Ticks}.json");
                File.WriteAllText(filename, JsonConvert.SerializeObject(inputPinPrintJob, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to save input pin print job for recovery");
            }
        }

        private void RemoveSavedInputPinPrintJob(int transactionNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "InputPin");
                if (Directory.Exists(printJobsDir))
                {
                    string pattern = $"InputPinPrintJob_{transactionNumber}_*.json";
                    foreach (string file in Directory.GetFiles(printJobsDir, pattern))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to remove saved input pin print job");
            }
        }


        private void texttPin_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
        }

        // Class to store input pin print job information
        private class InputPinPrintJob
        {
            public DataRestruk Data { get; set; }
            public List<CartDetailRestruk> CartDetails { get; set; }
            public List<RefundDetailRestruk> CartRefundDetails { get; set; }
            public List<CanceledItemStrukCustomerRestruk> CanceledItems { get; set; }
            public int TransactionNumber { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}