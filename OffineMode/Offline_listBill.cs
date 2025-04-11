using System.Data;
using System.Text.RegularExpressions;
using KASIR.Model;
using KASIR.Printer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace KASIR.OffineMode
{
    public partial class Offline_listBill : Form
    {
        private readonly string baseOutlet;

        private PrinterModel printerModel; // Pastikan ini telah diinisialisasi dengan benar
        private List<ListBill> Model;
        int nomor = 0;

        public List<CartDetail> cart_details { get; set; }

        // Refund details
        public List<RefundDetail> refund_details { get; set; }

        // Canceled items
        public List<CanceledItem> canceled_items { get; set; }

        public bool ReloadDataInBaseForm { get; private set; }
        public Offline_listBill()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;

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
        public async Task LoadData()
        {
            try
            {
                string transactionDataPath = "DT-Cache\\Transaction\\saveBill.data";
                // Membaca isi file transaction.data
                if (!File.Exists(transactionDataPath))
                {
                    return;
                    this.Close();
                }
                string transactionJson = File.ReadAllText(transactionDataPath);
                var transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                // Ambil array data transaksi
                var transactionDetails = transactionData["data"] as JArray;

                // Begin Counting Transaction Queue
                int numberQueue = transactionDetails.Count + 1; // Start queue number
                var reversedTransactionDetails = transactionDetails.Reverse().ToList();
                /*
                                IApiService apiService = new ApiService();
                                string response = await apiService.GetListBill("/transaction?outlet_id=", baseOutlet);

                                ListBillModel menuModel = JsonConvert.DeserializeObject<ListBillModel>(response);
                                List<ListBill> menuList = menuModel.data.ToList();*/
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(int));
                dataTable.Columns.Add("Nama", typeof(string));
                dataTable.Columns.Add("Seat", typeof(string));
                dataTable.Columns.Add("Terakhir Update", typeof(string));
                dataTable.Columns.Add("NumberQueue", typeof(int));


                string format = "dddd, dd MMMM yyyy - HH:mm";
                // Loop through each transaction to fill the DataTable
                foreach (var transaction in reversedTransactionDetails)
                {
                    numberQueue -= 1; // Decrease number for the next entry

                    // Parsing tanggal dari string API dan format ulang
                    DateTime updatedAt;
                    // Parsing tanggal menggunakan format yang sesuai
                    if (DateTime.TryParseExact(transaction["updated_at"]?.ToString(), format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out updatedAt))
                    {
                        // Format ulang menjadi lebih singkat, misalnya "19 Oct 2024, 10:38"
                        string formattedDate = updatedAt.ToString("dd MMM yyyy, HH:mm");
                        dataTable.Rows.Add(
                            transaction["transaction_id"]?.ToString(),
                            numberQueue.ToString() + "." + transaction["customer_name"].ToString(),
                            transaction["customer_seat"].ToString().ToString(),
                            formattedDate);
                    }
                    else
                    {
                        // Jika parsing gagal, tampilkan seperti apa adanya
                        dataTable.Rows.Add(
                            transaction["transaction_id"]?.ToString(),
                            numberQueue.ToString() + "." + transaction["customer_name"].ToString(),
                            transaction["customer_seat"].ToString().ToString(),
                            transaction["updated_at"].ToString().ToString());
                    }
                }

                dataGridView1.DataSource = dataTable;
                DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
                buttonColumn.HeaderText = "Pilih Bill";
                buttonColumn.Text = "Pilih";
                buttonColumn.FlatStyle = FlatStyle.Flat;
                buttonColumn.UseColumnTextForButtonValue = true; // Displays the "Add to Cart" text on the button
                DataGridViewButtonColumn buttonColumn1 = new DataGridViewButtonColumn();
                buttonColumn1.HeaderText = "Struk";
                buttonColumn1.Text = "Cetak";
                buttonColumn1.FlatStyle = FlatStyle.Flat;
                buttonColumn1.UseColumnTextForButtonValue = true;
                dataGridView1.Columns.Add(buttonColumn);
                dataGridView1.Columns.Add(buttonColumn1);
                if (dataGridView1.DataSource != null)
                {
                    // Pastikan kolom "ID" ada dalam DataGridView sebelum mencoba mengaksesnya
                    if (dataGridView1.Columns.Contains("ID"))
                    {
                        dataGridView1.Columns["ID"].Visible = false;
                    }
                    // Pastikan kolom "ID" ada dalam DataGridView sebelum mencoba mengaksesnya
                    if (dataGridView1.Columns.Contains("NumberQueue"))
                    {
                        dataGridView1.Columns["NumberQueue"].Visible = false;
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
                MessageBox.Show("Gagal tampil data bill  " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1 == null || dataGridView1.Columns.Count <= e.ColumnIndex || e.RowIndex < 0)
            {
                MessageBox.Show("Invalid selection", "Error");
                return;
            }

            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Pilih Bill" && e.RowIndex >= 0)
            {
                var cellValue = dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
                if (cellValue == null)
                {
                    MessageBox.Show("Invalid cell value", "Error");
                    return;
                }

                string selectedId = cellValue.ToString();
                try
                {
                    loadBill(selectedId);

                    DialogResult = DialogResult.OK;

                    Close();

                }
                catch (Exception ex)
                {
                    DialogResult = DialogResult.Cancel;
                    MessageBox.Show("Gagal load keranjang " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    Close();
                }
            }
            else if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Struk" && e.RowIndex >= 0)
            {
                PrinterModel printerModel = new PrinterModel();
                var cellValue = dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
                if (cellValue == null)
                {
                    MessageBox.Show("Invalid cell value", "Error");
                    return;
                }

                int selectedId = Convert.ToInt32(cellValue);

                var antrianCell = dataGridView1.Rows[e.RowIndex].Cells["NumberQueue"].Value;
                int AntrianSaveBill = 0;

                if (antrianCell != null)
                {
                    // Mencari angka sebelum titik menggunakan regex
                    Regex regex = new Regex(@"^\d+"); // Mencocokkan angka di awal string
                    Match match = regex.Match(antrianCell.ToString());

                    if (match.Success)
                    {
                        AntrianSaveBill = int.Parse(match.Value); // Ambil angka yang ditemukan
                    }
                }

                try
                {
                    // Read cart.data to extract cart details and transaction id
                    string cartDataPath = "DT-Cache\\Transaction\\saveBill.data";
                    // Cek apakah file transaction.data ada
                    if (!File.Exists(cartDataPath))
                    {
                        return;
                    }

                    var transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(cartDataPath));

                    // Get the array of transaction data
                    var transactionDetails = transactionData["data"] as JArray;

                    // Filter the transaction based on selectedId
                    var cartData = transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == selectedId.ToString());
                    // Extract cart details from the transaction
                    var cartDetails = cartData["cart_details"] as JArray;

                    // Membaca file JSON
                    string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                    // Deserialize JSON ke object CartDataCache
                    var dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                    // Konversi ke format GetStrukCustomerTransaction
                    var strukCustomerTransaction = new GetStrukCustomerTransaction
                    {
                        code = 201,
                        message = "Transaksi Sukses!",
                        data = new DataStrukCustomerTransaction
                        {
                            outlet_name = dataOutlet.data.name,
                            outlet_address = dataOutlet.data.address,
                            outlet_phone_number = dataOutlet.data.phone_number,
                            outlet_footer = dataOutlet.data.footer,
                            transaction_id = int.TryParse(cartData["transaction_id"]?.ToString(), out var transId) ? transId : 0,
                            receipt_number = cartData["receipt_number"]?.ToString(),
                            invoice_due_date = cartData["invoice_due_date"]?.ToString(),
                            customer_name = cartData["customer_name"]?.ToString(),
                            customer_seat = int.TryParse(cartData["customer_seat"]?.ToString(), out var seat) ? seat : 0,
                            customer_change = int.TryParse(cartData["customer_change"]?.ToString(), out var change) ? change : 0,
                            payment_type = cartData["payment_type"]?.ToString(),
                            delivery_type = null,
                            delivery_note = null,
                            cart_id = 0,
                            subtotal = int.TryParse(cartData["subtotal"]?.ToString(), out var subTotal) ? subTotal : 0,
                            total = int.TryParse(cartData["total"]?.ToString(), out var total) ? total : 0,
                            discount_id = int.TryParse(cartData["discount_id"]?.ToString(), out var discountId) && discountId != 0 ? discountId : 0,
                            discount_code = cartData["discount_code"]?.ToString(),
                            discounts_value = int.TryParse(cartData["discounts_value"]?.ToString(), out var discountValue) ? discountValue : 0,
                            discounts_is_percent = cartData["discounts_is_percent"]?.ToString(),
                            cart_details = new List<CartDetailStrukCustomerTransaction>(),
                            canceled_items = new List<CanceledItemStrukCustomerTransaction>(),
                            kitchenBarCartDetails = new List<KitchenAndBarCartDetails>(),
                            kitchenBarCanceledItems = new List<KitchenAndBarCanceledItems>(),
                            customer_cash = int.TryParse(cartData["customer_cash"]?.ToString(), out var customerCash) ? customerCash : 0,
                            member_name = null,
                            member_phone_number = null
                        }
                    };

                    foreach (var item in cartDetails)
                    {
                        // Cast the JToken to JObject
                        var cartDetailObject = item as JObject;


                        // Membuat objek CartDetailStrukCustomerTransaction
                        var cartDetail = new CartDetailStrukCustomerTransaction
                        {
                            cart_detail_id = int.Parse(cartDetailObject["cart_detail_id"].ToString()), // Mengonversi string ke int
                            menu_id = int.Parse(cartDetailObject["menu_id"].ToString()), // Mengonversi string ke int
                            menu_name = cartDetailObject["menu_name"]?.ToString(),
                            menu_type = cartDetailObject["menu_type"]?.ToString(),
                            menu_detail_id = int.Parse(cartDetailObject["menu_detail_id"]?.ToString() ?? "0"),
                            varian = cartDetailObject["menu_detail_name"]?.ToString(), // Tidak ada data varian
                            serving_type_id = int.Parse(cartDetailObject["serving_type_id"]?.ToString() ?? "0"),
                            serving_type_name = cartDetailObject["serving_type_name"]?.ToString(),
                            discount_id = null, // Tidak ada data discount
                            discount_code = cartDetailObject["discount_code"]?.ToString(),
                            discounts_value = null,
                            discounted_price = 0,
                            discounts_is_percent = null,
                            price = int.Parse(cartDetailObject["price"]?.ToString() ?? "0"), // Mengonversi string ke int
                            total_price = int.Parse(cartDetailObject["price"]?.ToString() ?? "0") * int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                            subtotal = int.Parse(cartDetailObject["price"]?.ToString() ?? "0") * int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                            subtotal_price = int.Parse(cartDetailObject["price"]?.ToString() ?? "0") * int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                            qty = int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                            note_item = string.IsNullOrEmpty(cartDetailObject["note_item"]?.ToString()) ? "" : cartDetailObject["note_item"].ToString(),
                            is_ordered = int.Parse(cartDetailObject["is_ordered"]?.ToString() ?? "0")
                        };

                        // Menambahkan ke cart_details
                        strukCustomerTransaction.data.cart_details.Add(cartDetail);

                        // Membuat objek KitchenAndBarCartDetails dan menyalin data dari cartDetail
                        var kitchenAndBarCartDetail = new KitchenAndBarCartDetails
                        {
                            cart_detail_id = cartDetail.cart_detail_id,
                            menu_id = cartDetail.menu_id,
                            menu_name = cartDetail.menu_name,
                            menu_type = cartDetail.menu_type,
                            menu_detail_id = cartDetail.menu_detail_id,
                            varian = cartDetail.varian,
                            serving_type_id = cartDetail.serving_type_id,
                            serving_type_name = cartDetail.serving_type_name,
                            discount_id = cartDetail.discount_id,
                            discount_code = cartDetail.discount_code,
                            discounts_value = cartDetail.discounts_value,
                            discounted_price = cartDetail.discounted_price,
                            discounts_is_percent = cartDetail.discounts_is_percent,
                            price = cartDetail.price,
                            total_price = cartDetail.total_price,
                            qty = cartDetail.qty,
                            note_item = cartDetail.note_item,
                            is_ordered = cartDetail.is_ordered
                        };

                        // Menambahkan ke kitchenBarCartDetails
                        strukCustomerTransaction.data.kitchenBarCartDetails.Add(kitchenAndBarCartDetail);
                    }

                    // Serialisasi ke JSON
                    string response = JsonConvert.SerializeObject(strukCustomerTransaction);
                    /*IApiService apiService = new ApiService();
                    string response = await apiService.Restruk("/transaction/" + selectedId + "?outlet_id=" + baseOutlet + "&is_struct=1");*/
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        await HandleSuccessfulTransaction(response, AntrianSaveBill);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal cetak ulang struk " + ex.ToString(), "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
        }
        private void loadBill(string id)
        {
            try
            {
                // Path untuk file transaction.data
                string saveBillDataPath = "DT-Cache\\Transaction\\saveBill.data";
                // Path untuk file Cart.data yang baru
                string cartPath = "DT-Cache\\Transaction\\Cart.data";

                // Cek apakah file transaction.data ada
                if (!File.Exists(saveBillDataPath))
                {
                    return;
                }

                var transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(saveBillDataPath));

                // Ambil array data transaksi
                var transactionDetails = transactionData["data"] as JArray;

                // Filter transaksi berdasarkan transaction_id
                var filteredTransaction = transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == id);

                if (filteredTransaction != null)
                {
                    // Ambil cart_details dari transaksi
                    var cartDetails = filteredTransaction["cart_details"] as JArray;

                    // Check if canceled_items exists, if not create it
                    if (filteredTransaction["canceled_items"] == null)
                    {
                        filteredTransaction["canceled_items"] = new JArray();
                    }
                    var cancelDetails = filteredTransaction["canceled_items"] as JArray;


                    // Buat format JSON baru
                    var newTransaction = new JObject
                    {
                        ["transaction_ref"] = filteredTransaction["transaction_ref"]?.ToString(),
                        ["transaction_id"] = int.Parse(filteredTransaction["transaction_id"]?.ToString()),
                        ["receipt_number"] = filteredTransaction["receipt_number"]?.ToString(),
                        ["transaction_ref_split"] = filteredTransaction["transaction_ref_split"]?.ToString(),
                        ["invoice_number"] = filteredTransaction["invoice_number"]?.ToString(),
                        ["invoice_due_date"] = filteredTransaction["invoice_due_date"]?.ToString(),
                        ["payment_type_id"] = int.Parse(filteredTransaction["payment_type_id"]?.ToString()),
                        ["payment_type_name"] = filteredTransaction["payment_type_name"]?.ToString(),
                        ["customer_name"] = filteredTransaction["customer_name"]?.ToString(),
                        ["customer_seat"] = int.Parse(filteredTransaction["customer_seat"]?.ToString()),
                        ["customer_cash"] = int.Parse(filteredTransaction["customer_cash"]?.ToString()),
                        ["customer_change"] = int.Parse(filteredTransaction["customer_change"]?.ToString()),
                        ["subtotal"] = cartDetails.Sum(cart => (int)cart["subtotal_price"]),
                        ["total"] = int.Parse(filteredTransaction["total"]?.ToString()),
                        ["created_at"] = filteredTransaction["created_at"]?.ToString(),
                        ["updated_at"] = filteredTransaction["updated_at"]?.ToString(),
                        ["deleted_at"] = filteredTransaction["deleted_at"]?.ToString(),
                        ["is_refund"] = filteredTransaction["is_refund"]?.ToString(),
                        ["refund_reason"] = filteredTransaction["refund_reason"]?.ToString(),
                        ["delivery_type"] = filteredTransaction["delivery_type"]?.ToString(),
                        ["delivery_note"] = filteredTransaction["delivery_note"]?.ToString(),
                        ["discount_id"] = int.Parse(filteredTransaction["discount_id"]?.ToString()),
                        ["discount_code"] = filteredTransaction["discount_code"]?.ToString(),
                        ["discounts_value"] = filteredTransaction["discounts_value"]?.ToString(),
                        ["discounted_price"] = filteredTransaction["discounted_price"]?.ToString(),
                        ["discounts_is_percent"] = filteredTransaction["discounts_is_percent"]?.ToString(),
                        ["member_name"] = filteredTransaction["member_name"]?.ToString(),
                        ["member_phone_number"] = filteredTransaction["member_phone_number"]?.ToString(),
                        ["is_refund_all"] = int.Parse(filteredTransaction["is_refund_all"]?.ToString()),
                        ["refund_reason_all"] = filteredTransaction["refund_reason_all"]?.ToString(),
                        ["refund_payment_id_all"] = 0,
                        ["refund_created_at_all"] = filteredTransaction["refund_created_at_all"]?.ToString(),
                        ["total_refund"] = 0,
                        ["refund_payment_name_all"] = filteredTransaction["refund_payment_name_all"]?.ToString(),
                        ["is_savebill"] = 1,
                        ["is_edited_sync"] = int.Parse(filteredTransaction["is_edited_sync"]?.ToString()),
                        ["is_sent_sync"] = int.Parse(filteredTransaction["is_sent_sync"]?.ToString()),
                        ["cart_details"] = cartDetails,
                        ["refund_details"] = new JArray(),
                        ["canceled_items"] = cancelDetails
                    };

                    // Menulis JSON baru ke dalam file baru
                    File.WriteAllText(cartPath, newTransaction.ToString());
                    // Hapus transaksi dari saveBill.data jika berhasil disalin ke Cart.data
                    DeleteTransactionFromSaveBill(id);
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private void DeleteTransactionFromSaveBill(string transactionId)
        {
            try
            {
                // Path untuk file saveBill.data
                string saveBillDataPath = "DT-Cache\\Transaction\\saveBill.data";

                // Cek apakah file saveBill.data ada
                if (!File.Exists(saveBillDataPath))
                {
                    return;
                }

                var transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(saveBillDataPath));

                // Ambil array data transaksi
                var transactionDetails = transactionData["data"] as JArray;

                // Filter transaksi yang tidak memiliki transaction_id yang sesuai
                var remainingTransactions = transactionDetails.Where(t => t["transaction_id"]?.ToString() != transactionId).ToArray();

                // Update data transaksi setelah penghapusan
                transactionData["data"] = new JArray(remainingTransactions);

                // Simpan kembali file saveBill.data yang telah diperbarui
                File.WriteAllText(saveBillDataPath, JsonConvert.SerializeObject(transactionData, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred while deleting transaction: {ErrorMessage}", ex.Message);
            }
        }

        private async Task HandleSuccessfulTransaction(string response, int AntrianSaveBill)
        {
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30-second timeout

            try
            {
                PrinterModel printerModel = new PrinterModel();
                RestrukModel restrukModel = JsonConvert.DeserializeObject<RestrukModel>(response);

                if (restrukModel == null)
                {
                    throw new InvalidOperationException("Deserialization failed: restrukModel is null");
                }

                DataRestruk data = restrukModel.data;
                if (data == null)
                {
                    throw new InvalidOperationException("Deserialization failed: data is null");
                }

                List<CartDetailRestruk> cartDetails = data.cart_details ?? new List<CartDetailRestruk>();
                List<CanceledItemStrukCustomerRestruk> canceledItems = data.canceled_items ?? new List<CanceledItemStrukCustomerRestruk>();

                if (printerModel != null)
                {
                    // Save print job for potential recovery
                    SaveDataBillPrintJobForRecovery(data, cartDetails, canceledItems, AntrianSaveBill);

                    try
                    {
                        // Execute the print with timeout
                        await Task.Run(async () =>
                        {
                            await printerModel.PrintModelDataBill(data, cartDetails, canceledItems, AntrianSaveBill);
                        }, cts.Token);

                        // If successful, remove the saved print job
                        RemoveSavedDataBillPrintJob(AntrianSaveBill);

                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    catch (OperationCanceledException)
                    {
                        // The operation timed out
                        LoggerUtil.LogWarning("Data bill print operation timed out, will retry in background");

                        // Allow form to close but continue printing in background
                        DialogResult = DialogResult.OK;

                        // Start background processing
                        ThreadPool.QueueUserWorkItem(async _ =>
                        {
                            try
                            {
                                // Use a new printer model instance to avoid shared state issues
                                PrinterModel backgroundPrinterModel = new PrinterModel();

                                // Retry the print operation in background
                                await backgroundPrinterModel.PrintModelDataBill(data, cartDetails, canceledItems, AntrianSaveBill);

                                // If successful, remove the saved print job
                                RemoveSavedDataBillPrintJob(AntrianSaveBill);
                            }
                            catch (Exception ex)
                            {
                                LoggerUtil.LogError(ex, "Background data bill printing failed: {ErrorMessage}", ex.Message);
                            }
                        });

                        Close();
                    }
                }
                else
                {
                    throw new InvalidOperationException("printerModel is null");
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred during data bill printing: {ErrorMessage}", ex.Message);
                MessageBox.Show($"Terjadi kesalahan saat mencetak: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cts.Dispose();
            }
        }

        // Helper methods for data bill print job persistence
        private void SaveDataBillPrintJobForRecovery(
            DataRestruk data,
            List<CartDetailRestruk> cartDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int antrianNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "DataBills");
                Directory.CreateDirectory(printJobsDir);

                var dataBillPrintJob = new DataBillPrintJob
                {
                    Data = data,
                    CartDetails = cartDetails,
                    CanceledItems = canceledItems,
                    AntrianNumber = antrianNumber,
                    Timestamp = DateTime.Now
                };

                string filename = Path.Combine(printJobsDir, $"DataBillPrintJob_{antrianNumber}_{DateTime.Now.Ticks}.json");
                File.WriteAllText(filename, JsonConvert.SerializeObject(dataBillPrintJob, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to save data bill print job for recovery");
            }
        }

        private void RemoveSavedDataBillPrintJob(int antrianNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "DataBills");
                if (Directory.Exists(printJobsDir))
                {
                    string pattern = $"DataBillPrintJob_{antrianNumber}_*.json";
                    foreach (var file in Directory.GetFiles(printJobsDir, pattern))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to remove saved data bill print job");
            }
        }

        // Class to store data bill print job information
        public class DataBillPrintJob
        {
            public DataRestruk Data { get; set; }
            public List<CartDetailRestruk> CartDetails { get; set; }
            public List<CanceledItemStrukCustomerRestruk> CanceledItems { get; set; }
            public int AntrianNumber { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
