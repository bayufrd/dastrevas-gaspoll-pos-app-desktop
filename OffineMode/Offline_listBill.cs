using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using FontAwesome.Sharp;
using KASIR.Helper;
using KASIR.Model;
using KASIR.Printer;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.OffineMode
{
    public partial class Offline_listBill : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        private readonly string baseOutlet;

        public Offline_listBill()
        {
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

        }

        public bool ReloadDataInBaseForm { get; private set; }

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
                    Close();
                }

                string transactionJson = File.ReadAllText(transactionDataPath);
                JObject? transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                // Ambil array data transaksi
                JArray? transactionDetails = transactionData["data"] as JArray;

                // Begin Counting Transaction Queue
                int numberQueue = transactionDetails.Count + 1; // Start queue number
                List<JToken> reversedTransactionDetails = transactionDetails.Reverse().ToList();
                LoadDataToFlowLayout(reversedTransactionDetails);

            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal tampil data bill  " + ex.Message);
            }
        }
        private async void LoadDataToFlowLayout(IEnumerable<JToken> reversedTransactionDetails)
        {
            flowLayoutPanel1.Controls.Clear();

            int numberQueue = reversedTransactionDetails.Count();
            string format = "dddd, dd MMMM yyyy - HH:mm";

            foreach (JToken transaction in reversedTransactionDetails)
            {
                numberQueue--;
                string transactionId = transaction["transaction_id"]?.ToString() ?? null;

                // Parsing tanggal
                string formattedDate = transaction["updated_at"]?.ToString();
                if (DateTime.TryParseExact(formattedDate, format,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime updatedAt))
                {
                    formattedDate = updatedAt.ToString("dd MMM yyyy, HH:mm");
                }

                // Buat card
                Panel card = new Panel
                {
                    Width = flowLayoutPanel1.ClientSize.Width - 30,
                    Height = 80,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5),
                    Padding = new Padding(10)
                };

                // Label judul (Nama + Queue)
                Label lblNama = new Label
                {
                    Text = $"{numberQueue + 1}. {transaction["customer_name"]}",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    AutoSize = true,
                    ForeColor = Color.Black,
                    Location = new Point(10, 10)
                };

                // Label seat
                Label lblSeat = new Label
                {
                    Text = $"Seat: {transaction["customer_seat"]}",
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    ForeColor = Color.Gray,
                    Location = new Point(10, 35)
                };

                // Label update time
                Label lblUpdate = new Label
                {
                    Text = $"Terakhir di Update: {formattedDate}",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    AutoSize = true,
                    ForeColor = Color.DarkGray,
                    Location = new Point(10, 55)
                };

                // Tombol pilih
                IconButton btnPilih = new IconButton
                {
                    Text = "",
                    IconChar = IconChar.CheckCircle,   // pilih icon
                    IconColor = Color.Black,
                    IconSize = 20,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    BackColor = Color.White,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Width = 35,
                    Height = 35,
                    Location = new Point(card.Width - 160, 25),
                    Cursor = Cursors.Hand,
                    Tag = transactionId
                };
                btnPilih.FlatAppearance.BorderSize = 0;

                // Tombol cetak
                IconButton btnCetak = new IconButton
                {
                    Text = "",
                    IconChar = IconChar.Print,
                    IconColor = Color.Black,
                    IconSize = 20,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    BackColor = Color.White,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Width = 35,
                    Height = 35,
                    Location = new Point(card.Width - 100, 25),
                    Cursor = Cursors.Hand,
                    Tag = (transactionId, numberQueue)
                };
                btnCetak.FlatAppearance.BorderSize = 0;

                // Event klik button
                btnPilih.Click += (s, e) =>
                {
                    try
                    {
                        loadBill(transactionId);   // langsung pakai fungsi kamu
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        NotifyHelper.Error("Gagal load keranjang " + ex.Message);
                        LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                        DialogResult = DialogResult.Cancel;
                        Close();
                    }
                };

                btnCetak.Click += async (s, e) =>
                {
                    try
                    {
                        var (id, queue) = ((string, int))((Button)s).Tag;
                        Close();
                        await CetakBill(id, queue); // convert int ke string
                        NotifyHelper.Success($"Cetak struk #{queue} dengan ID {id}");
                    }
                    catch (Exception ex)
                    {
                        NotifyHelper.Error("Gagal load keranjang " + ex.Message);
                        LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                        DialogResult = DialogResult.Cancel;
                        Close();
                    }
                };

                ToolTip toolTip = new ToolTip();

                // Setting optional
                toolTip.AutoPopDelay = 5000;   // durasi muncul
                toolTip.InitialDelay = 500;    // delay awal
                toolTip.ReshowDelay = 200;     // delay tampil ulang
                toolTip.ShowAlways = true;     // tetap tampil walau tidak fokus

                // Assign tooltip ke button
                toolTip.SetToolTip(btnPilih, "Klik untuk memilih transaksi");
                toolTip.SetToolTip(btnCetak, "Klik untuk mencetak struk");
                toolTip.SetToolTip(btnKeluar, "Klik untuk keluar dari list savebill");

                // Tambah semua ke card
                card.Controls.Add(lblNama);
                card.Controls.Add(lblSeat);
                card.Controls.Add(lblUpdate);
                card.Controls.Add(btnPilih);
                card.Controls.Add(btnCetak);

                // Tambahkan ke FlowLayoutPanel
                flowLayoutPanel1.Controls.Add(card);
                flowLayoutPanel1.AutoScroll = true;         // wajib, biar muncul scrollbar
                flowLayoutPanel1.WrapContents = false;      // biar nggak pindah ke samping
                flowLayoutPanel1.FlowDirection = FlowDirection.TopDown; // item ditumpuk ke bawah

            }
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Close();
        }

        private async Task CetakBill(string id, int queue)
        {
            PrinterModel printerModel = new();
            int selectedId = int.Parse(id);  
            int antrianCell = queue;         
            int AntrianSaveBill = 0;

            if (antrianCell != null)
            {
                // Mencari angka sebelum titik menggunakan regex
                Regex regex = new(@"^\d+"); // Mencocokkan angka di awal string
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

                JObject? transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(cartDataPath));

                // Get the array of transaction data
                JArray? transactionDetails = transactionData["data"] as JArray;

                // Filter the transaction based on selectedId
                JToken? cartData =
                    transactionDetails.FirstOrDefault(t =>
                        t["transaction_id"]?.ToString() == selectedId.ToString());
                // Extract cart details from the transaction
                JArray? cartDetails = cartData["cart_details"] as JArray;

                // Membaca file JSON
                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                // Konversi ke format GetStrukCustomerTransaction
                GetStrukCustomerTransaction strukCustomerTransaction = new()
                {
                    code = 201,
                    message = "Transaksi Sukses!",
                    data = new DataStrukCustomerTransaction
                    {
                        outlet_name = dataOutlet.data.name,
                        outlet_address = dataOutlet.data.address,
                        outlet_phone_number = dataOutlet.data.phone_number,
                        outlet_footer = dataOutlet.data.footer,
                        transaction_id =
                            int.TryParse(cartData["transaction_id"]?.ToString(), out int transId)
                                ? transId
                                : 0,
                        receipt_number = cartData["receipt_number"]?.ToString(),
                        invoice_due_date = cartData["invoice_due_date"]?.ToString(),
                        customer_name = cartData["customer_name"]?.ToString(),
                        customer_seat =
                            int.TryParse(cartData["customer_seat"]?.ToString(), out int seat) ? seat : 0,
                        customer_change =
                            int.TryParse(cartData["customer_change"]?.ToString(), out int change)
                                ? change
                                : 0,
                        payment_type = cartData["payment_type"]?.ToString(),
                        delivery_type = null,
                        delivery_note = null,
                        cart_id = 0,
                        subtotal =
                            int.TryParse(cartData["subtotal"]?.ToString(), out int subTotal) ? subTotal : 0,
                        total = int.TryParse(cartData["total"]?.ToString(), out int total) ? total : 0,
                        discount_id =
                            int.TryParse(cartData["discount_id"]?.ToString(), out int discountId) &&
                            discountId != 0
                                ? discountId
                                : 0,
                        discount_code = cartData["discount_code"]?.ToString(),
                        discounts_value =
                            int.TryParse(cartData["discounts_value"]?.ToString(), out int discountValue)
                                ? discountValue
                                : 0,
                        discounts_is_percent = cartData["discounts_is_percent"]?.ToString(),
                        cart_details = new List<CartDetailStrukCustomerTransaction>(),
                        canceled_items = new List<CanceledItemStrukCustomerTransaction>(),
                        kitchenBarCartDetails = new List<KitchenAndBarCartDetails>(),
                        kitchenBarCanceledItems = new List<KitchenAndBarCanceledItems>(),
                        customer_cash =
                            int.TryParse(cartData["customer_cash"]?.ToString(), out int customerCash)
                                ? customerCash
                                : 0,
                        member_name = null,
                        member_phone_number = null
                    }
                };

                foreach (JToken item in cartDetails)
                {
                    // Cast the JToken to JObject
                    JObject? cartDetailObject = item as JObject;


                    // Membuat objek CartDetailStrukCustomerTransaction
                    CartDetailStrukCustomerTransaction cartDetail = new()
                    {
                        cart_detail_id =
                            int.Parse(cartDetailObject["cart_detail_id"].ToString()), // Mengonversi string ke int
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
                        price =
                            int.Parse(cartDetailObject["price"]?.ToString() ?? "0"), // Mengonversi string ke int
                        total_price =
                            int.Parse(cartDetailObject["price"]?.ToString() ?? "0") *
                            int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                        subtotal =
                            int.Parse(cartDetailObject["price"]?.ToString() ?? "0") *
                            int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                        subtotal_price =
                            int.Parse(cartDetailObject["price"]?.ToString() ?? "0") *
                            int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                        qty = int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                        note_item =
                            string.IsNullOrEmpty(cartDetailObject["note_item"]?.ToString())
                                ? ""
                                : cartDetailObject["note_item"].ToString(),
                        is_ordered = int.Parse(cartDetailObject["is_ordered"]?.ToString() ?? "0")
                    };

                    // Menambahkan ke cart_details
                    strukCustomerTransaction.data.cart_details.Add(cartDetail);

                    // Membuat objek KitchenAndBarCartDetails dan menyalin data dari cartDetail
                    KitchenAndBarCartDetails kitchenAndBarCartDetail = new()
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
                    NotifyHelper.Error("Gagal memproses transaksi. Silahkan coba lagi.");
                }
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal cetak ulang struk " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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

                JObject? transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(saveBillDataPath));

                // Ambil array data transaksi
                JArray? transactionDetails = transactionData["data"] as JArray;

                // Filter transaksi berdasarkan transaction_id
                JToken? filteredTransaction =
                    transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == id);

                if (filteredTransaction != null)
                {
                    // Ambil cart_details dari transaksi
                    JArray? cartDetails = filteredTransaction["cart_details"] as JArray;

                    // Check if canceled_items exists, if not create it
                    if (filteredTransaction["canceled_items"] == null)
                    {
                        filteredTransaction["canceled_items"] = new JArray();
                    }

                    JArray? cancelDetails = filteredTransaction["canceled_items"] as JArray;


                    // Buat format JSON baru
                    JObject newTransaction = new()
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
                        ["created_at"] =
                            filteredTransaction["created_at"]?.ToString() ??
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
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

                JObject? transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(saveBillDataPath));

                // Ambil array data transaksi
                JArray? transactionDetails = transactionData["data"] as JArray;

                // Filter transaksi yang tidak memiliki transaction_id yang sesuai
                JToken[] remainingTransactions = transactionDetails
                    .Where(t => t["transaction_id"]?.ToString() != transactionId).ToArray();

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
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30)); // 30-second timeout

            try
            {
                PrinterModel printerModel = new();
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
                List<CanceledItemStrukCustomerRestruk> canceledItems =
                    data.canceled_items ?? new List<CanceledItemStrukCustomerRestruk>();

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
                                PrinterModel backgroundPrinterModel = new();

                                // Retry the print operation in background
                                await backgroundPrinterModel.PrintModelDataBill(data, cartDetails, canceledItems,
                                    AntrianSaveBill);

                                // If successful, remove the saved print job
                                RemoveSavedDataBillPrintJob(AntrianSaveBill);
                            }
                            catch (Exception ex)
                            {
                                LoggerUtil.LogError(ex, "Background data bill printing failed: {ErrorMessage}",
                                    ex.Message);
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
                NotifyHelper.Error($"Terjadi kesalahan saat mencetak: {ex.Message}");
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

                DataBillPrintJob dataBillPrintJob = new()
                {
                    Data = data,
                    CartDetails = cartDetails,
                    CanceledItems = canceledItems,
                    AntrianNumber = antrianNumber,
                    Timestamp = DateTime.Now
                };

                string filename = Path.Combine(printJobsDir,
                    $"DataBillPrintJob_{antrianNumber}_{DateTime.Now.Ticks}.json");
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
                    foreach (string file in Directory.GetFiles(printJobsDir, pattern))
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