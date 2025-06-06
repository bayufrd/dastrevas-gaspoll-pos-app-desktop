using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Model;
using KASIR.Network;
using KASIR.OffineMode;
using KASIR.Printer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.Komponen
{
    public partial class Offline_shiftReport : UserControl
    {
        private ApiService apiService;
        private readonly string baseOutlet;
        private int bedaCash = 0, NewDataChecker = 0;
        private DateTime mulaishift, akhirshift;
        private CancellationTokenSource cts;

        // Add these to the class fields
        private Panel loadingPanel;
        private Label loadingLabel;
        private ProgressBar progressBar;

        public bool IsBackgroundOperation { get; set; } = false;

        public Offline_shiftReport()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            apiService = new ApiService();
            btnCetakStruk.Enabled = true;
            lblNotifikasi.Visible = false;
            InitializeLoadingUI();
            txtNamaKasir.Text = "Dastrevas (AutoFill)";
            this.Disposed += (s, e) =>
            {
                cts?.Cancel();
                cts?.Dispose();
            };
        }

        private bool ShouldShowProgress() => this.Visible && this.IsHandleCreated && !this.IsDisposed && !IsBackgroundOperation;

        private void InitializeLoadingUI()
        {
            try
            {
                loadingPanel = new Panel
                {
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.Fixed3D,
                    Size = new Size(300, 120),
                    Location = new Point((this.ClientSize.Width - 400) / 2, (this.ClientSize.Height - 200) / 2),
                    Visible = false
                };

                loadingLabel = new Label
                {
                    Text = "Mengambil data dari server...",
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = Color.White,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(280, 30),
                    Location = new Point(10, 20)
                };

                progressBar = new ProgressBar
                {
                    Style = ProgressBarStyle.Continuous,
                    Size = new Size(280, 25),
                    Location = new Point(10, 60),
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0
                };

                loadingPanel.BackColor = Color.FromArgb(30, 31, 68);
                loadingPanel.Controls.Add(loadingLabel);
                loadingPanel.Controls.Add(progressBar);
                this.Controls.Add(loadingPanel);
                loadingPanel.BringToFront();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error initializing loading UI: {ErrorMessage}", ex.Message);
            }
        }

        private void ShowLoading()
        {
            if (!ShouldShowProgress()) return;

            try
            {
                loadingPanel.Visible = true;
                progressBar.Value = 0;
                loadingLabel.Text = "Mengambil data dari server...";
            }
            catch (Exception) { }
        }

        private void HideLoading()
        {
            if (!ShouldShowProgress()) return;

            try
            {
                loadingPanel.Visible = false;
            }
            catch (Exception) { }
        }

        private void UpdateProgress(int percentage, string message = null)
        {
            if (!ShouldShowProgress()) return;

            try
            {
                progressBar.Value = Math.Min(100, Math.Max(0, percentage));
                if (!string.IsNullOrEmpty(message))
                    loadingLabel.Text = message;
            }
            catch (Exception) { }
        }
        // Function to determine the start_at based on shift data file
        private string GetStartAt()
        {
            string shiftDataPath = @"DT-Cache\Transaction\shiftData.data";

            // If the file doesn't exist or is empty, return today at 6 AM
            if (!File.Exists(shiftDataPath) || new FileInfo(shiftDataPath).Length == 0)
            {
                // Return today's date at 6:00 AM
                return DateTime.Now.Date.AddHours(6).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            // Read the existing shift data from the file
            string shiftDataJson = File.ReadAllText(shiftDataPath);
            JObject shiftData = JObject.Parse(shiftDataJson);
            JArray shiftDataArray = (JArray)shiftData["data"];

            // If no shift data is available, return today's date at 6 AM
            if (shiftDataArray.Count == 0)
            {
                return DateTime.Now.Date.AddHours(6).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            // Get the "End_at" of the last shift (last element in the array)
            JObject lastShift = (JObject)shiftDataArray.Last;
            string lastEndAt = lastShift["end_at"].ToString();

            // Parse the "End_at" date and add 1 second to it
            DateTime lastEndAtDate = DateTime.ParseExact(lastEndAt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime newStartAt = lastEndAtDate.AddSeconds(1);

            // Return the new "start_at" value
            return newStartAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
        private async void btnCetakStruk_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNamaKasir.Text.ToString() == "" || txtNamaKasir.Text == null)
                {
                    MessageBox.Show("Nama kasir masih kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (txtActualCash.Text.ToString() == "" || txtActualCash.Text == null)
                {
                    MessageBox.Show("Uang kasir masih kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string fulus = Regex.Replace(txtActualCash.Text, "[^0-9]", "");

                var shiftNumber = GetShiftNumber();
                string startAt = GetStartAt();
                DateTime akhirshift = DateTime.Now;  // Use DateTime directly for current time

                DialogResult yakin = MessageBox.Show($"Melakukan End Shift {shiftNumber.ToString()} pada waktu \n{startAt.ToString()} sampai {akhirshift.ToString("yyyy-MM-dd HH:mm:ss")}\nNama Kasir : {txtNamaKasir.Text}\nActual Cash : Rp.{txtActualCash.Text},- \nCash Different : {string.Format("{0:n0}", Convert.ToInt32(fulus) - bedaCash)}?", "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (yakin != DialogResult.Yes)
                {
                    MessageBox.Show("Cetak Shift diCancel");
                    return;
                }
                else
                {
                    btnCetakStruk.Enabled = false;
                    btnCetakStruk.Text = "Waiting...";

                    var casherName = string.IsNullOrEmpty(txtNamaKasir.Text) ? "" : txtNamaKasir.Text;
                    var actualCash = string.IsNullOrEmpty(fulus) ? "0" : fulus;

                    var json = new
                    {
                        outlet_id = baseOutlet,
                        casher_name = casherName,
                        actual_ending_cash = actualCash,

                        //adding for cache
                        shift_number = shiftNumber.ToString(),
                        created_at = akhirshift.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        start_at = startAt,  // Set the start_at value
                        end_at = akhirshift.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        is_sync = 0
                    };

                    // Save transaction data to transaction.data
                    string shiftPath = "DT-Cache\\Transaction\\shiftData.data";

                    JArray shiftDataArray = new JArray();
                    if (File.Exists(shiftPath))
                    {
                        // If the transaction file exists, read and append the new transaction
                        string existingData = File.ReadAllText(shiftPath);
                        var existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                        shiftDataArray = existingTransactions["data"] as JArray ?? new JArray();
                    }

                    // Add new transaction
                    shiftDataArray.Add(JToken.FromObject(json));

                    // Serialize and save back to transaction.data
                    var newTransactionData = new JObject { { "data", shiftDataArray } };
                    File.WriteAllText(shiftPath, JsonConvert.SerializeObject(newTransactionData, Formatting.Indented));

                    //await HandleSuccessfulPrint(response);
                }
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
        }
        private void ResetButtonState()
        {
            btnCetakStruk.Enabled = true;
            btnCetakStruk.Text = "Cetak Struk";
            btnCetakStruk.BackColor = Color.FromArgb(31, 30, 68);
        }
        private async Task HandleSuccessfulPrint(string response)
        {
            int retryCount = 3; // Jumlah maksimal percobaan ulang
            int delayBetweenRetries = 2000; // Jeda antara percobaan dalam milidetik
            int currentRetry = 0;

            while (currentRetry < retryCount)
            {
                try
                {
                    PrinterModel printerModel = new PrinterModel();
                    btnCetakStruk.Text = "Mencetak...";
                    btnCetakStruk.Enabled = false;

                    GetStrukShift shiftModel = JsonConvert.DeserializeObject<GetStrukShift>(response);
                    DataStrukShift datas = shiftModel.data;

                    await printerModel.PrintModelCetakLaporanShift(
                        datas,
                        datas.expenditures,
                        datas.cart_details_success,
                        datas.cart_details_pending,
                        datas.cart_details_canceled,
                        datas.refund_details,
                        datas.payment_details
                    );

                    MessageBox.Show("Cetak laporan sukses", "Gaspol", MessageBoxButtons.OK);
                    btnCetakStruk.Enabled = false;
                    // Jika sukses, keluar dari loop
                    break;
                }
                catch (Exception ex)
                {
                    currentRetry++;

                    if (currentRetry >= retryCount)
                    {
                        LoggerUtil.LogError(ex, $"Error printing after {currentRetry}", ex);
                    }
                    else
                    {
                        await Task.Delay(delayBetweenRetries); // Menunggu sebelum mencoba lagi
                    }
                }
            }
        }
        private void txtActualCash_TextChanged(object sender, EventArgs e)
        {
            if (txtActualCash.Text == "" || txtActualCash.Text == "0") return;
            decimal number;
            try
            {
                number = decimal.Parse(txtActualCash.Text, System.Globalization.NumberStyles.Currency);
            }
            catch (FormatException)
            {
                // The text could not be parsed as a decimal number.
                // You can handle this exception in different ways, such as displaying a message to the user.
                MessageBox.Show("inputan hanya bisa Numeric");
                return;
            }
            txtActualCash.Text = number.ToString("#,#");
            txtActualCash.SelectionStart = txtActualCash.Text.Length;
        }

        private void btnPengeluaran_Click(object sender, EventArgs e)
        {
            Form background = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = this.Location,
                ShowInTaskbar = false,
            };

            using (Offline_notifikasiPengeluaran notifikasiPengeluaran = new Offline_notifikasiPengeluaran())
            {
                notifikasiPengeluaran.Owner = background;

                background.Show();

                DialogResult dialogResult = notifikasiPengeluaran.ShowDialog();

                background.Dispose();
            }
        }

        private void btnRiwayatShift_Click(object sender, EventArgs e)
        {
            Form background = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = this.Location,
                ShowInTaskbar = false,
            };

            this.Invoke((MethodInvoker)delegate
            {
                using (Offline_HistoryShift payForm = new Offline_HistoryShift()) // No List<ShiftData> passed
                {
                    payForm.Owner = background;

                    background.Show();

                    DialogResult dialogResult = payForm.ShowDialog();

                    background.Dispose();

                    // Check if the user clicked "Pilih" and returned data
                    if (dialogResult == DialogResult.OK)
                    {
                        // Get the selected shift data
                        ShiftData selectedShift = payForm.SelectedShift;

                        // Process the selected shift data, for example:
                        MessageBox.Show($"Shift Number: {selectedShift.ShiftNumber}\nStart At: {selectedShift.StartAt}\nEnd At: {selectedShift.EndAt}");

                        // Call a method to load additional data or perform operations
                        LoadData();
                    }
                    else
                    {
                        // Handle case where user cancels or closes the dialog
                        MessageBox.Show("No shift selected or action cancelled.");
                    }
                }
            });
        }

        private (DateTime startShift, DateTime lastShift) GetStartAndLastShiftOrder(TransactionData transactionData)
        {
            // Extract all created_at dates from the transactions and parse them safely
            var transactionTimes = transactionData.data
                .Select(t =>
                {
                    // Attempt to parse created_at, return null if invalid
                    DateTime parsedDate;
                    bool isValidDate = DateTime.TryParse(t.created_at, out parsedDate);
                    return isValidDate ? parsedDate : (DateTime?)null;
                })
                .Where(t => t.HasValue) // Filter out null values
                .OrderBy(t => t.Value) // Order by date in ascending order
                .ToList();

            // Ensure we have at least one transaction
            if (transactionTimes.Count > 0)
            {
                DateTime startShift = transactionTimes.First().Value; // First transaction is the start of the shift
                DateTime lastShift = transactionTimes.Last().Value; // Last transaction is the end of the shift

                return (startShift, lastShift);
            }

            // If no valid transactions, return default values (in case of an error)
            return (DateTime.MinValue, DateTime.MinValue);
        }
        private int GetShiftNumber()
        {
            string shiftPath = "DT-Cache\\Transaction\\ShiftData.data";

            // Check if the shift data file exists
            if (File.Exists(shiftPath))
            {
                // Read the existing shift data from the file
                string existingData = File.ReadAllText(shiftPath);
                var existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);

                // Retrieve the "data" array, which contains all shift transactions
                var shiftDataArray = existingTransactions["data"] as JArray;

                // If there are no shifts, return 1 (default shift number)
                if (shiftDataArray == null || shiftDataArray.Count == 0)
                {
                    return 1; // Default shift number when no shifts exist
                }
                else
                {
                    // Otherwise, return the next shift number (the count of current shifts + 1)
                    return shiftDataArray.Count + 1;
                }
            }
            else
            {
                // If the file doesn't exist, return 1 (default shift number)
                return 1;
            }
        }
        private void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // If the file does not exist, create it with the default content
                string defaultContent = "{ \"data\": [] }";

                try
                {
                    // Create the file and write the default content
                    File.WriteAllText(filePath, defaultContent);
                }
                catch (IOException ex)
                {
                    // Handle any I/O exceptions that may occur during file creation
                    LoggerUtil.LogError(ex, $"Failed to create file: {filePath}");
                }
            }
        }

        //LOAD DATA Transaction
        public async Task LoadData(bool isBackground = false)
        {
            // Set background operation flag
            IsBackgroundOperation = isBackground;

            // Cancel previous token if it exists
            CancelPreviousOperation();

            if (ShouldShowProgress())
            {
                btnCetakStruk.Enabled = false;
                ShowLoading();
                UpdateProgress(10, "Memeriksa konfigurasi...");
            }

            if (ShouldShowProgress())
            {
                UpdateProgress(80, "Data diterima, memproses...");
            }
            var shiftNumber = GetShiftNumber(); // Get the shift number dynamically

            string filePath = @"DT-Cache/Transaction/transaction.data";

            string fileSavebillPath = @"DT-Cache/Transaction/saveBill.data";

            string fileExpenditurePath = @"DT-Cache/Transaction/expenditure.data";

            // Ensure the transaction file exists
            EnsureFileExists(filePath);

            // Ensure the savebill file exists
            EnsureFileExists(fileSavebillPath);

            // Ensure the expenditure file exists
            EnsureFileExists(fileExpenditurePath);

            string jsonContent = File.ReadAllText(filePath);

            string jsonContentSavebill = File.ReadAllText(fileSavebillPath);

            string jsonContentExpenditure = File.ReadAllText(fileExpenditurePath);


            ExpenditureData expenditureData = JsonConvert.DeserializeObject<ExpenditureData>(jsonContentExpenditure);

            // Deserialize the JSON content into the `TransactionData` object
            TransactionData transactionData = JsonConvert.DeserializeObject<TransactionData>(jsonContent);

            // Deserialize the JSON content into the `TransactionData` object
            TransactionData transactionDataSaveBill = JsonConvert.DeserializeObject<TransactionData>(jsonContentSavebill);

            DataTable dataTable = CreateDataTable();

            if (ShouldShowProgress())
            {
                UpdateProgress(90, "Memformat data untuk tampilan...");
            }
            // Get the start and last shift based on transaction times
            var (startShift, lastShift) = GetStartAndLastShiftOrder(transactionData);

            // Add the Start and Last Shift Order to the DataTable
            AddSeparatorRowBold(dataTable, "SHIFT NUMBER " + shiftNumber.ToString(), dataGridView1);

            dataTable.Rows.Add("Start Shift Order", $"{startShift.ToString("yyyy-MM-dd HH:mm:ss")}");
            dataTable.Rows.Add("Last Shift Order", $"{lastShift.ToString("yyyy-MM-dd HH:mm:ss")}");

            dataTable.Rows.Add("", $"");

            // Process the cart details
            ProcessCartDetails(transactionData, dataTable, "SOLD ITEMS");

            decimal totalProcessedCart = CalculateTotalProcessCartDetails(transactionData);
            dataTable.Rows.Add("Total Sold Items", $"{totalProcessedCart:n0}");

            dataTable.Rows.Add("", $"");

            // Process refunded items
            ProcessRefundDetails(transactionData, dataTable);

            // Calculate total refund amount
            decimal totalRefundAmount = CalculateTotalRefund(transactionData);
            dataTable.Rows.Add("Total Refund", $"{totalRefundAmount:n0}");

            dataTable.Rows.Add("", $"");
            // Process the savebill details
            ProcessCartDetails(transactionDataSaveBill, dataTable, "SAVEBILL/PENDING ITEMS");

            decimal totalPendingCart = CalculateTotalProcessCartDetails(transactionDataSaveBill);
            dataTable.Rows.Add("Total Savebill/Pending Items", $"{totalPendingCart:n0}");

            dataTable.Rows.Add("", $"");
            // Cancel the savebill details
            ProcessCartDetails(transactionDataSaveBill, dataTable, "CANCELED ITEMS");

            decimal totalCancelCart = CalculateTotalProcessCartDetails(transactionDataSaveBill);
            dataTable.Rows.Add("Total Canceled Items", $"{totalCancelCart:n0}");

            dataTable.Rows.Add("", $"");

            // Process the Expenditure details
            ProcessExpenditureDetails(expenditureData, dataTable, "EXPENDITURE ITEMS");

            decimal totalExpenditureItems = CalculateTotalProcessExpenditures(expenditureData);
            dataTable.Rows.Add("Total Expense Items", $"{totalExpenditureItems:n0}");

            dataTable.Rows.Add("", $"");

            // Process the Discounts details
            ProcessDiscountDetails(transactionData, dataTable, "DISCOUNT ITEMS");

            dataTable.Rows.Add("", $"");

            // Process payment details
            ProcessPaymentDetails(transactionData, dataTable);

            dataTable.Rows.Add("", $"");

            // Add the grand total
            decimal grandTotal = transactionData.data
            .SelectMany(t => t.cart_details) // Meratakan list cart_details dari setiap Transaction
            .Sum(cd => cd.total_price); // Menjumlahkan total_price untuk setiap CartDetails

            AddSeparatorRowBold(dataTable, "GRAND TOTAL", dataGridView1);
            grandTotal -= totalRefundAmount;
            dataTable.Rows.Add("Total Transactions", $"{totalProcessedCart:n0}");

            // Display data in DataGridView
            DisplayDataInDataGridView(dataTable);

            // Update progress and final steps
            if (ShouldShowProgress())
            {
                UpdateProgress(100, "Selesai!");
                await Task.Delay(500); // Short delay to show completion
                HideLoading();
            }

            btnCetakStruk.Enabled = true;
        }
        private List<PaymentType> LoadPaymentTypes()
        {
            string json = File.ReadAllText("DT-Cache" + "\\LoadDataPayment_" + "Outlet_" + baseOutlet + ".data");
            PaymentTypeModel payment = JsonConvert.DeserializeObject<PaymentTypeModel>(json);

            return payment.data; // Return the list of payment types
        }

        private void CancelPreviousOperation()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            cts = new CancellationTokenSource();
        }
        private DataTable CreateDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DATA", typeof(string));
            dataTable.Columns.Add("Detail", typeof(string));
            return dataTable;
        }
        private decimal CalculateTotalProcessExpenditures(ExpenditureData transactionData)
        {
            // Calculate the total price for all cart details where quantity is greater than 0
            decimal totalProcessedCart = transactionData.data
                .Sum(cd => cd.nominal); // Sum the total_price of each cart detail

            return totalProcessedCart;
        }

        private decimal CalculateTotalProcessCartDetails(TransactionData transactionData)
        {
            // Calculate the total price for all cart details where quantity is greater than 0
            decimal totalProcessedCart = transactionData.data
                .SelectMany(t => t.cart_details) // Flatten the cart details
                .Where(cd => cd.qty > 0) // Filter only items with quantity > 0
                .Sum(cd => cd.total_price); // Sum the total_price of each cart detail

            return totalProcessedCart;
        }
        private void ProcessExpenditureDetails(ExpenditureData transactionData, DataTable dataTable, string text)
        {
            // Add a separator row with the provided text
            AddSeparatorRowBold(dataTable, text, dataGridView1);

            // Process each transaction and add the description and nominal to the DataTable
            foreach (var expenditure in transactionData.data)
            {
                // Add rows to the DataTable with 'description' and 'nominal' (formatted as currency)
                dataTable.Rows.Add(expenditure.description, $"{decimal.Parse(expenditure.nominal.ToString()):n0}");
            }
        }
        private void ProcessDiscountDetails(TransactionData transactionData, DataTable dataTable, string text)
        {
            // Add a separator row with the provided text
            AddSeparatorRowBold(dataTable, text, dataGridView1);

            // Sum up the discounted_price from the transaction level (if applicable)
            decimal groupedCartCarts = transactionData.data
                .Where(t => t.discounted_price > 0) // Filter transactions with discounted_price > 0
                .Sum(t => t.discounted_price); // Sum the discounted_price of each transaction

            // Display the discount per cart
            dataTable.Rows.Add("Discount Per Carts", $"{groupedCartCarts:n0}");

            // Now sum the discounted_price from the cart_details nested within each transaction
            decimal groupedCartDetails = transactionData.data
                .SelectMany(t => t.cart_details) // Flatten the cart details
                .Where(cd => cd.discounted_price > 0) // Filter only items with discounted_price > 0
                .Sum(cd => cd.discounted_price); // Sum the discounted_price of each cart detail

            // Display the discount per items
            dataTable.Rows.Add("Discount Per Items", $"{groupedCartDetails:n0}");

            // Calculate the total discounts by summing both
            decimal totalDiscounts = groupedCartCarts + groupedCartDetails;

            // Display the total discount
            dataTable.Rows.Add("Total Discount Items", $"{totalDiscounts:n0}");
        }

        private void ProcessCartDetails(TransactionData transactionData, DataTable dataTable, string text)
        {
            AddSeparatorRowBold(dataTable, text, dataGridView1);

            var groupedCartDetails = transactionData.data
                .SelectMany(t => t.cart_details)
                .GroupBy(cd => new { cd.menu_type, cd.menu_name, cd.menu_detail_name })
                .OrderBy(g => GetMenuTypeOrder(g.Key.menu_type))
                .Select(g => new
                {
                    MenuType = g.Key.menu_type,
                    MenuName = g.Key.menu_name,
                    Varian = g.Key.menu_detail_name,
                    Qty = g.Sum(x => x.qty),
                    TotalPrice = g.Sum(x => x.price * x.qty)
                }).ToList();

            foreach (var group in groupedCartDetails)
            {
                string displayMenuName = group.MenuName;
                if (group.Qty > 0)
                {
                    if (!string.IsNullOrEmpty(group.Varian) && group.Varian != "null")
                    {
                        displayMenuName += "\n - " + group.Varian;
                    }
                    dataTable.Rows.Add($"{group.Qty}x {displayMenuName}", $"{group.TotalPrice:n0}");
                }
            }
        }

        private void ProcessRefundDetails(TransactionData transactionData, DataTable dataTable)
        {
            AddSeparatorRowBold(dataTable, "REFUNDED ITEMS", dataGridView1);

            var groupedRefundDetails = transactionData.data
                .SelectMany(t => t.refund_details)
                .GroupBy(rd => new { rd.menu_name, rd.menu_detail_name })
                .Select(g => new
                {
                    MenuName = g.Key.menu_name,
                    Varian = g.Key.menu_detail_name,
                    RefundQty = g.Sum(x => x.refund_qty),
                    RefundTotal = g.Sum(x => x.refund_total)
                }).ToList();

            foreach (var group in groupedRefundDetails)
            {
                string displayMenuName = group.MenuName;
                if (!string.IsNullOrEmpty(group.Varian) && group.Varian != "null")
                {
                    displayMenuName += "\n - " + group.Varian;
                }
                dataTable.Rows.Add($"{group.RefundQty}x {displayMenuName}", $"{group.RefundTotal:n0}");
            }
        }

        private decimal CalculateTotalRefund(TransactionData transactionData)
        {
            return transactionData.data
                .Sum(t => t.refund_details.Sum(rd => rd.refund_total));
        }
        private void ProcessPaymentDetails(TransactionData transactionData, DataTable dataTable)
        {
            AddSeparatorRowBold(dataTable, "PAYMENT DETAILS", dataGridView1);

            // Load payment types from the external file
            var paymentTypes = LoadPaymentTypes();

            // Group payments by payment_type_id and calculate the total amount for each payment type
            var groupedPayments = transactionData.data
                .GroupBy(t => t.payment_type_id)
                .Select(g => new
                {
                    PaymentTypeId = g.Key,
                    PaymentTypeName = paymentTypes.FirstOrDefault(p => p.id == g.Key)?.name ?? "Unknown",  // Match payment type ID to name
                    TotalAmount = g.Sum(x => x.total)  // Sum the total amount for each payment type
                }).ToList();

            // Group refunds by transaction-level refund_payment_id_all
            var groupedRefunds = transactionData.data
                .Where(t => t.is_refund_all == 1) // Consider only transactions marked as "all refunded"
                .Select(t => new
                {
                    PaymentTypeId = t.refund_payment_id_all,  // Refund ID is on the transaction level
                    TotalRefund = t.total_refund  // Use total_refund for the entire transaction
                })
                .ToList();

            // Iterate over each payment type and subtract corresponding refunds
            foreach (var payment in groupedPayments)
            {
                // Find the refund amount for this payment type
                var refund = groupedRefunds.FirstOrDefault(r => r.PaymentTypeId == payment.PaymentTypeId);
                decimal totalRefund = refund?.TotalRefund ?? 0;  // Default to 0 if no refund is found

                // Subtract the refund from the total payment amount
                decimal netAmount = payment.TotalAmount - totalRefund;

                // Add payment type and net amount to the data table
                dataTable.Rows.Add(payment.PaymentTypeName, $"{netAmount:n0}");

                // If the payment type is "Tunai", update the txtActualCash field
                if (payment.PaymentTypeName.Equals("Tunai", StringComparison.OrdinalIgnoreCase))
                {
                    txtActualCash.Text = $"{netAmount:n0}";  // Set the "Tunai" value to the net amount
                }
            }
        }


        private void DisplayDataInDataGridView(DataTable dataTable)
        {
            if (dataGridView1 == null)
            {
                ReloadDataGridView(dataTable);
            }
            else
            {
                dataGridView1.DataSource = dataTable;

                if (dataGridView1.Font == null)
                {
                    dataGridView1.Font = new Font("Arial", 8.25f, FontStyle.Regular);
                    ReloadDataGridView(dataTable);
                }
                else
                {
                    if (dataGridView1.Columns.Contains("DATA"))
                    {
                        DataGridViewCellStyle boldStyle = new DataGridViewCellStyle
                        {
                            Font = new Font(dataGridView1.Font, FontStyle.Italic)
                        };
                        dataGridView1.Columns["DATA"].DefaultCellStyle = boldStyle;
                    }

                    if (dataGridView1.Columns.Contains("ID"))
                    {
                        dataGridView1.Columns["ID"].Visible = false;
                    }
                }
            }
        }
        private void ReloadDataGridView(DataTable dataTable)
        {
            try
            {
                // Attempt to reinitialize dataGridView1 if it is null
                if (dataGridView1 == null)
                {
                    dataGridView1 = new DataGridView();
                }

                // Attempt to set the Font property if it is null
                if (dataGridView1.Font == null)
                {
                    dataGridView1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                }

                // Reapply data source and styles
                dataGridView1.DataSource = dataTable; // Assuming dataTable is a class-level variable or passed as a parameter
                DataGridViewCellStyle boldStyle = new DataGridViewCellStyle
                {
                    Font = new Font(dataGridView1.Font, FontStyle.Italic)
                };
                dataGridView1.Columns["DATA"].DefaultCellStyle = boldStyle;
                dataGridView1.Columns["ID"].Visible = false;

                // Log success
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the reinitialization process
                LoggerUtil.LogError(ex, "An error occurred while reinitializing dataGridView1: {ErrorMessage}", ex.Message);
            }
        }
        private int GetMenuTypeOrder(string menuType)
        {
            switch (menuType)
            {
                case "Minuman": return 1;
                case "Additional Minuman": return 2;
                case "Makanan": return 3;
                case "Additional Makanan": return 4;
                default: return 5; // Default for unknown types
            }
        }

        private void AddSeparatorRowBold(DataTable dataTable, string text, DataGridView dataGridView)
        {
            // Create a new row
            DataRow separatorRow = dataTable.NewRow();
            separatorRow["DATA"] = text;

            // Add the row to the DataTable
            dataTable.Rows.Add(separatorRow);

            // Refresh DataGridView after adding the row
            dataGridView.DataSource = dataTable;
            dataGridView.Refresh(); // Ensure DataGridView is updated

            // Apply bold style to the last added row
            if (dataGridView.Rows.Count > 0)
            {
                // Apply bold style to the "DATA" column of the last row
                DataGridViewCellStyle boldStyle = new DataGridViewCellStyle
                {
                    Font = new Font(dataGridView.Font, FontStyle.Bold)
                };

                // Apply the bold style to the last row that was added
                int lastRowIndex = dataGridView.Rows.Count - 1;
                if (dataGridView.Columns.Contains("DATA"))
                {
                    dataGridView.Rows[lastRowIndex].Cells["DATA"].Style = boldStyle;
                }
                else
                {
                    // Handle the error (perhaps log it or show a message)
                    LoggerUtil.LogError(null, "The 'DATA' column does not exist in the DataGridView.");
                }
            }
            else
            {
                // Handle the error (no rows available in the DataGridView)
                LoggerUtil.LogError(null, "No rows found in the DataGridView to apply bold style.");
            }
        }


    }
}
