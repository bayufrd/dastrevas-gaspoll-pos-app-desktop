using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Helper;
using KASIR.Model;
using KASIR.OffineMode;
using KASIR.Printer;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.Komponen
{
    public partial class Offline_shiftReport : UserControl
    {
        private readonly string baseOutlet;
        private CancellationTokenSource cts;
        private Label loadingLabel;

        private Panel loadingPanel;
        private ProgressBar progressBar;
        private int ending_cash = 0,
            totalTransaksiOulet = 0,
            totalDiscountAmount = 0,
            cashIncomeReal = 0,
            cashOutRefund = 0, 
            cashOutExpenditure = 0;


        public Offline_shiftReport()
        {
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
            btnCetakStruk.Enabled = true;
            lblNotifikasi.Visible = false;
            InitializeLoadingUI();
            txtNamaKasir.Text = "Dastrevas (AutoFill)";
            txtActualCash.Text = "0";
            Disposed += (s, e) =>
            {
                cts?.Cancel();
                cts?.Dispose();
            };
        }

        public bool IsBackgroundOperation { get; set; }

        private bool ShouldShowProgress()
        {
            return Visible && IsHandleCreated && !IsDisposed && !IsBackgroundOperation;
        }

        private void InitializeLoadingUI()
        {
            try
            {
                loadingPanel = new Panel
                {
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.Fixed3D,
                    Size = new Size(300, 120),
                    Location = new Point((ClientSize.Width - 400) / 2, (ClientSize.Height - 200) / 2),
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
                Controls.Add(loadingPanel);
                loadingPanel.BringToFront();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error initializing loading UI: {ErrorMessage}", ex.Message);
            }
        }

        private void ShowLoading()
        {
            if (!ShouldShowProgress())
            {
                return;
            }

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
            if (!ShouldShowProgress())
            {
                return;
            }

            try
            {
                loadingPanel.Visible = false;
            }
            catch (Exception) { }
        }

        private void UpdateProgress(int percentage, string message = null)
        {
            if (!ShouldShowProgress())
            {
                return;
            }

            try
            {
                progressBar.Value = Math.Min(100, Math.Max(0, percentage));
                if (!string.IsNullOrEmpty(message))
                {
                    loadingLabel.Text = message;
                }
            }
            catch (Exception) { }
        }

        private string GetStartAt()
        {
            string shiftDataPath = @"DT-Cache\Transaction\shiftData.data";

            if (!File.Exists(shiftDataPath) || new FileInfo(shiftDataPath).Length == 0)
            {
                return DateTime.Now.Date.AddHours(6).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            string shiftDataJson = File.ReadAllText(shiftDataPath);
            JObject shiftData = JObject.Parse(shiftDataJson);
            JArray shiftDataArray = (JArray)shiftData["data"];

            // If no shift data is available, return today's date at 6 AM
            if (shiftDataArray.Count == 0)
            {
                return DateTime.Now.Date.AddHours(6).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            JObject lastShift = (JObject)shiftDataArray.Last;
            string lastEndAt = lastShift["end_at"].ToString();

            // Parse the "End_at" date and add 1 second to it
            DateTime lastEndAtDate =
                DateTime.ParseExact(lastEndAt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime newStartAt = lastEndAtDate.AddSeconds(1);

            // Return the new "start_at" value
            return newStartAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private async void btnCetakStruk_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNamaKasir.Text == "" || txtNamaKasir.Text == null)
                {
                    NotifyHelper.Warning("Nama kasir masih kurang tepat");
                    return;
                }

                if (txtActualCash.Text == "" || txtActualCash.Text == null)
                {
                    NotifyHelper.Warning("Uang kasir masih kurang tepat");
                    return;
                }

                string fulus = Regex.Replace(txtActualCash.Text, "[^0-9]", "");

                int shiftNumber = GetShiftNumber();
                string startAt = GetStartAt();
                DateTime akhirshift = DateTime.Now;

                DateTime startAtDateTime = DateTime.Parse(startAt);

                TimeSpan timeDifference = akhirshift - startAtDateTime;
                /*if (timeDifference.TotalHours < 1)
                {
                    MessageBox.Show($"Jarak Cetak Laporan Terlalu dekat\n\nStart : {startAt.ToString()}\nEnd : {akhirshift.ToString("yyyy-MM-dd HH:mm:ss")}", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }*/

                string generateIDshift = $"{baseOutlet}{startAtDateTime.ToString("yyyyMMddHHmmss")}";

                DialogResult yakin = MessageBox.Show(
                    $"Melakukan End Shift {shiftNumber.ToString()} pada waktu \n{startAt} sampai {akhirshift.ToString("yyyy-MM-dd HH:mm:ss")}\nNama Kasir : {txtNamaKasir.Text}\nActual Cash : Rp.{txtActualCash.Text},- \nCash Different : {string.Format("{0:n0}", Convert.ToInt32(fulus) - ending_cash)}?",
                    "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (yakin != DialogResult.Yes)
                {
                    NotifyHelper.Info("Cetak Shift diCancel");
                }
                else
                {
                    btnCetakStruk.Enabled = false;
                    btnCetakStruk.Text = "Waiting...";

                    int cashDiff = Convert.ToInt32(fulus) - ending_cash;
                    string? casherName = string.IsNullOrEmpty(txtNamaKasir.Text) ? "" : txtNamaKasir.Text;
                    string actualCash = string.IsNullOrEmpty(fulus) ? "0" : fulus;

                    var json = new
                    {
                        outlet_id = baseOutlet,
                        actual_ending_cash = actualCash,
                        cash_difference = cashDiff,
                        start_date = startAt,
                        end_date = akhirshift.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        created_at = akhirshift.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        updated_at = akhirshift.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        expected_ending_cash = ending_cash,
                        total_amount = totalTransaksiOulet,
                        total_discount = totalDiscountAmount,
                        shift_number = shiftNumber.ToString(),
                        casher_name = casherName,

                        // cache
                        id = generateIDshift,
                        start_at = startAt,
                        end_at = akhirshift.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        is_sync = 0
                    };

                    string shiftPath = "DT-Cache\\Transaction\\shiftData.data";

                    JArray shiftDataArray = new();
                    if (File.Exists(shiftPath))
                    {
                        string existingData = File.ReadAllText(shiftPath);
                        JObject? existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                        shiftDataArray = existingTransactions["data"] as JArray ?? new JArray();
                    }

                    shiftDataArray.Add(JToken.FromObject(json));

                    JObject newTransactionData = new() { { "data", shiftDataArray } };
                    await GenerateShiftReport(generateIDshift);

                    File.WriteAllText(shiftPath, JsonConvert.SerializeObject(newTransactionData, Formatting.Indented));
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
            finally
            {
                ResetButtonState();
                LoadData();
            }
        }

        public async Task GenerateShiftReport(string IDshiftData)
        {
            try
            {
                string transactionFilePath = @"DT-Cache\Transaction\transaction.data";
                string expenditureFilePath = @"DT-Cache\Transaction\expenditure.data";
                string savebillFilePath = @"DT-Cache\Transaction\saveBill.data";
                string outletFilePath = $"DT-Cache\\DataOutlet{baseOutlet}.data";

                string transactionJson = await File.ReadAllTextAsync(transactionFilePath);
                string expenditureJson = await File.ReadAllTextAsync(expenditureFilePath);
                string outletJson = await File.ReadAllTextAsync(outletFilePath);

                TransactionData transactionDataRaw = JsonConvert.DeserializeObject<TransactionData>(transactionJson);
                ExpenditureData expendituresDataRaw = JsonConvert.DeserializeObject<ExpenditureData>(expenditureJson);

                OutletData outletData = JsonConvert.DeserializeObject<OutletData>(outletJson);

                string savebillJson = await File.ReadAllTextAsync(savebillFilePath);
                TransactionData transactionDataSavebill = JsonConvert.DeserializeObject<TransactionData>(savebillJson);

                string outletName = outletData?.Data.Name ?? "Unknown Outlet";
                string outletAddress = outletData?.Data.Address ?? "Unknown Address";
                string outletPhoneNumber = outletData?.Data.PhoneNumber ?? "Unknown Phone";

                string startAt = GetStartAt();

                int shiftNumber = GetShiftNumber();
                string startDate = startAt;
                DateTime akhirshift = DateTime.Now;
                string endDate = akhirshift.ToString("yyyy-MM-dd HH:mm:ss");

                DateTime startAtDateTime = DateTime.Parse(startAt);

                //filtering
                List<Transaction> transactionDataRawFiltered = transactionDataRaw.data
                    .Where(t => DateTime.TryParse(t.updated_at, out DateTime createdAt) && createdAt > startAtDateTime)
                    .ToList();

                // Memfilter pengeluaran berdasarkan updated_at
                List<ExpenditureStrukShift> expendituresDataRawFiltered = expendituresDataRaw.data
                    .Where(e => DateTime.TryParse(e.created_at, out DateTime expenditureCreatedAt) &&
                                expenditureCreatedAt > startAtDateTime)
                    .ToList();

                // Membungkus filteredTransactions menjadi TransactionData
                TransactionData transactionData = new() { data = transactionDataRawFiltered };

                ExpenditureData expenditureData = new() { data = expendituresDataRawFiltered };

                var expenditures = expenditureData.data.Select(e => new { e.description, e.nominal }).ToList();

                int totalExpenditure = expenditures.Sum(e => e.nominal);

                var successfulCartDetails = transactionData.data.SelectMany(t => t.cart_details)
                    .Where(cd => cd.qty > 0)
                    .GroupBy(cd =>
                        new { cd.menu_id, cd.menu_detail_id })
                    .Select(g => new
                    {
                        g.Key.menu_id,
                        g.Key.menu_detail_id,
                        g.First().menu_name,
                        g.First().menu_type,
                        varian =
                            g.First().menu_detail_name,
                        qty = g.Sum(cd => cd.qty),
                        total_price = g.Sum(cd => cd.total_price)
                    })
                    .ToList();

                int totalSuccessfulAmount = successfulCartDetails.Sum(cd => cd.total_price);

                var pendingCartDetails = transactionDataSavebill.data.SelectMany(t => t.cart_details)
                    .Where(cd => cd.qty > 0)
                    .GroupBy(cd =>
                        new { cd.menu_id, cd.menu_detail_id })
                    .Select(g => new
                    {
                        g.Key.menu_id,
                        g.Key.menu_detail_id,
                        g.First().menu_name, // Mengambil satu nilai menu_name dari salah satu item
                        g.First().menu_type, // Mengambil satu nilai menu_type dari salah satu item
                        varian =
                            g.First().menu_detail_name, // Mengambil satu nilai menu_detail_name dari salah satu item
                        qty = g.Sum(cd => cd.qty), // Menjumlahkan qty
                        total_price = g.Sum(cd => cd.total_price) // Menjumlahkan total_price, jika diperlukan
                    })
                    .ToList();

                int totalPendingAmount = pendingCartDetails.Sum(cd => cd.total_price);

                var canceledCartDetails = transactionDataSavebill.data.SelectMany(t => t.canceled_items)
                    .Where(cd => cd.qty > 0) // Hanya ambil item dengan qty lebih dari 0
                    .GroupBy(cd =>
                        new { cd.menu_id, cd.menu_detail_id }) // Mengelompokkan berdasarkan menu_id dan menu_detail_id
                    .Select(g => new
                    {
                        g.Key.menu_id,
                        g.Key.menu_detail_id,
                        g.First().menu_name, // Mengambil satu nilai menu_name dari salah satu item
                        g.First().menu_type, // Mengambil satu nilai menu_type dari salah satu item
                        varian =
                            g.First().menu_detail_name, // Mengambil satu nilai menu_detail_name dari salah satu item
                        qty = g.Sum(cd => cd.qty), // Menjumlahkan qty
                        total_price = g.Sum(cd => cd.total_price) // Menjumlahkan total_price, jika diperlukan
                    })
                    .ToList();

                int totalCanceledAmount = canceledCartDetails.Sum(cd => cd.total_price);

                // Mengambil detail refund
                var refundDetails = transactionData.data.SelectMany(t => t.refund_details)
                    .GroupBy(rd =>
                        new
                        {
                            rd.menu_id,
                            rd.menu_detail_name
                        }) // Mengelompokkan berdasarkan menu_id dan menu_detail_name
                    .Select(g => new
                    {
                        g.Key.menu_id,
                        g.First().menu_name, // Mengambil satu nilai menu_name dari salah satu item
                        g.First().menu_type, // Mengambil satu nilai menu_name dari salah satu item
                        varian = g.Key.menu_detail_name, // Mengambil nilai menu_detail_name dari kunci grup
                        qty_refund_item = g.Sum(rd => rd.refund_qty), // Menjumlahkan qty_refund_item
                        total_refund_price = g.Sum(rd => rd.refund_total) // Menjumlahkan total_refund_price
                    })
                    .ToList();

                int totalRefundAmount = refundDetails.Sum(rd => rd.total_refund_price);

                List<PaymentType> paymentTypes = LoadPaymentTypes();

                var groupedPayments = transactionData.data
                    .GroupBy(t => t.payment_type_id)
                    .Select(g => new
                    {
                        PaymentTypeId = g.Key,
                        PaymentTypeName =
                            paymentTypes.FirstOrDefault(p => p.id == g.Key)?.name ??
                            "Unknown", // Match payment type ID to name
                        TotalAmount = g.Sum(x => x.total) // Sum the total amount for each payment type
                    }).ToList();

                var groupedRefunds = transactionData.data
                    .Where(t => t.is_refund_all == 1) // Consider only transactions marked as "all refunded"
                    .Select(t => new
                    {
                        PaymentTypeId = t.refund_payment_id_all, // Refund ID is on the transaction level
                        TotalRefund = t.total_refund // Use total_refund for the entire transaction
                    })
                    .ToList();

                string actual_cash = Regex.Replace(txtActualCash.Text, "[^0-9]", "");
                List<dynamic> paymentDetails = new();
                int categoryId = 1; // Inisialisasi ID kategori pembayaran.
                int CashOnPOS = 0;
                foreach (var payment in groupedPayments)
                {
                    var refund = groupedRefunds.FirstOrDefault(r => r.PaymentTypeId == payment.PaymentTypeId);
                    int totalRefund = refund?.TotalRefund ?? 0; // Use 0 if no refund is found

                    // Calculate netAmount considering transaction-level refunds
                    int netAmount = payment.TotalAmount - totalRefund;

                    // Match the payment category by name considering both payment type name and refund payment type
                    var matchingRefunds = refundDetails
                        .Where(rd => rd.menu_name.Equals(payment.PaymentTypeName, StringComparison.OrdinalIgnoreCase) ||
                                     rd.menu_type.Equals(payment.PaymentTypeName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Sum item-level refunds related to this payment type
                    int totalItemRefund = matchingRefunds.Sum(rd => rd.total_refund_price);

                    // Update netAmount to consider both transaction refunds and item-level refunds
                    netAmount -= totalItemRefund;

                    string paymentypeString = payment.PaymentTypeName;
                    // Check if the payment type is "Tunai"
                    if (payment.PaymentTypeName.Equals("Tunai", StringComparison.OrdinalIgnoreCase))
                    {
                        paymentypeString = "Cash On POS";
                        //netAmount -= totalExpenditure; // Deduct total expenditures if payment type is "Tunai"
                    }

                    // Add constructed result object
                    paymentDetails.Add(new
                    {
                        payment_category = paymentypeString, // Use the formatted payment category
                        payment_type_detail = new List<dynamic>(), // Initialize as an empty array
                        total_amount = netAmount,
                        payment_category_id = categoryId++ // Increment category ID for each entry
                    });
                }

                int discountsCarts = transactionData.data
                    .Where(t => t.discounted_price > 0) // Filter transactions with discounted_price > 0
                    .Sum(t => t.discounted_price); // Sum the discounted_price of each transaction


                int discountsDetails = transactionData.data
                    .SelectMany(t => t.cart_details) // Flatten the cart details
                    .Where(cd => cd.discounted_price > 0) // Filter only items with discounted_price > 0
                    .Sum(cd => cd.discounted_price); // Sum the discounted_price of each cart detail

                int totalDiscounts = discountsCarts + discountsDetails;

                int grandTotal = transactionData.data
                    .SelectMany(t => t.cart_details) // Meratakan list cart_details dari setiap Transaction
                    .Sum(cd => cd.total_price); // Menjumlahkan total_price untuk setiap CartDetails

                //Membership
                int totalMemberUsePoints = int.Parse(CalculateTotalMemberUsePoints(transactionData.data).ToString());
                paymentDetails.Add(new
                {
                    payment_category = "POINT MEMBER USED", // Use the formatted payment category
                    payment_type_detail = new List<dynamic>(), // Initialize as an empty array
                    total_amount = totalMemberUsePoints,
                    payment_category_id = categoryId++ // Increment category ID for each entry
                });

                grandTotal -= totalRefundAmount;
                grandTotal -= totalMemberUsePoints;
                int totalTransaction = grandTotal - totalExpenditure;
                totalTransaksiOulet = totalTransaction;
                totalDiscountAmount = totalDiscounts;

                int cash_difference = int.Parse(actual_cash) - int.Parse(ending_cash.ToString());

                var finalReport = new
                {
                    code = 200,
                    message = "Struk shift berhasil ditampilkan!",
                    data = new
                    {
                        outlet_name = outletName,
                        outlet_address = outletAddress,
                        outlet_phone_number = outletPhoneNumber,
                        casher_name = txtNamaKasir.Text,
                        shift_number = shiftNumber,
                        start_date = startDate,
                        end_date = endDate,
                        expenditures,
                        expenditures_total = totalExpenditure,
                        ending_cash_expected = ending_cash,
                        ending_cash_actual = actual_cash,
                        cash_difference = cash_difference.ToString(),
                        discount_amount_transactions = discountsCarts,
                        discount_amount_per_items = discountsDetails,
                        discount_total_amount = totalDiscounts,
                        cart_details_success = successfulCartDetails,
                        totalSuccessQty = successfulCartDetails.Sum(cd => cd.qty),
                        totalCartSuccessAmount = totalSuccessfulAmount,
                        cart_details_pending = pendingCartDetails,
                        totalPendingQty = pendingCartDetails.Sum(cd => cd.qty),
                        totalCartPendingAmount = totalPendingAmount,
                        cart_details_canceled = canceledCartDetails,
                        totalCanceledQty = canceledCartDetails.Sum(cd => cd.qty),
                        totalCartCanceledAmount = totalCanceledAmount,
                        refund_details = refundDetails,
                        totalRefundQty = refundDetails.Sum(rd => rd.qty_refund_item),
                        totalCartRefundAmount = totalRefundAmount,
                        payment_details = paymentDetails,
                        total_transaction = totalTransaction
                    }
                };

                string directoryPath =
                    Path.GetDirectoryName($"DT-Cache\\Transaction\\ShiftReport\\shiftReport-{IDshiftData}.data");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string finalJson = JsonConvert.SerializeObject(finalReport, Formatting.Indented);
                string filePath = $"DT-Cache\\Transaction\\ShiftReport\\shiftReport-{IDshiftData}.data";
                await File.WriteAllTextAsync(filePath, finalJson);
                HandleSuccessfulPrint(finalJson);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error writing new shift ID: {ErrorMessage}", ex.Message);
            }
        }

        private void ResetButtonState()
        {
            btnCetakStruk.Enabled = true;
            btnCetakStruk.Text = "Cetak Struk";
            btnCetakStruk.BackColor = Color.FromArgb(15, 90, 94);
        }

        private async Task HandleSuccessfulPrint(string response)
        {
            try
            {
                PrinterModel printerModel = new();

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

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error writing new shift ID: {ErrorMessage}", ex.Message);
            }
        }

        private void txtActualCash_TextChanged(object sender, EventArgs e)
        {
            if (txtActualCash.Text == "" || txtActualCash.Text == "0")
            {
                return;
            }

            decimal number;
            try
            {
                number = decimal.Parse(txtActualCash.Text, NumberStyles.Currency);
            }
            catch (FormatException)
            {
                NotifyHelper.Warning("inputan hanya bisa Numeric");
                if (txtActualCash.Text.Length > 0)
                {
                    txtActualCash.Text = txtActualCash.Text.Substring(0, txtActualCash.Text.Length - 1);
                    txtActualCash.SelectionStart = txtActualCash.Text.Length; // Move the cursor to the end
                }

                return;
            }

            txtActualCash.Text = number.ToString("#,#");
            txtActualCash.SelectionStart = txtActualCash.Text.Length;
        }

        private void btnPengeluaran_Click(object sender, EventArgs e)
        {
            Form background = CreateBackgroundForm();


            using (Offline_notifikasiPengeluaran notifikasiPengeluaran = new())
            {
                notifikasiPengeluaran.Owner = background;

                background.Show();

                DialogResult dialogResult = notifikasiPengeluaran.ShowDialog();

                background.Dispose();
            }
        }

        private void btnRiwayatShift_Click(object sender, EventArgs e)
        {
            Form background = CreateBackgroundForm();

            Invoke((MethodInvoker)delegate
            {
                using (Offline_HistoryShift payForm = new())
                {
                    payForm.Owner = background;
                    background.Show();

                    DialogResult dialogResult = payForm.ShowDialog();

                    background.Dispose();

                    if (dialogResult == DialogResult.OK)
                    {
                        // Option Pilih
                        NotifyHelper.Warning("Fiture Belum Tersedia");
                        return;
                        ShiftData selectedShift = payForm.SelectedShift;

                        DirectSwipeShiftData();
                        ClearSourceFileAsync("DT-Cache\\Transaction\\shiftData.data");

                        DateTime startAtDateTime = DateTime.Parse(selectedShift.StartAt);
                        string destinationFilePath =
                            $"DT-Cache\\Transaction\\HistoryTransaction\\History_transaction_DT-{baseOutlet}_{startAtDateTime:yyyyMMdd}.data";
                        CopyShiftDataAsync("DT-Cache\\Transaction\\shiftData.data", destinationFilePath);
                        SelectedShiftID(selectedShift.id);
                        LoadData();
                    }

                    if (dialogResult == DialogResult.Continue)
                    {
                        // Option Printing
                        ShiftData selectedShift = payForm.SelectedShift;

                        NotifyHelper.Info(
                            $"Printing Shift ID : {selectedShift.id}.\n\nCasher Name : {selectedShift.CasherName}\nShift : {selectedShift.ShiftNumber}\nStart at : {selectedShift.StartAt}\nEnd at : {selectedShift.EndAt}");

                        printingReportHistory(selectedShift.id);
                    }
                }
            });
        }

        private async Task printingReportHistory(string id)
        {
            try
            {
                string filePath = $"DT-Cache\\Transaction\\ShiftReport\\shiftReport-{id}.data";
                if (!File.Exists(filePath))
                {
                    return;
                }
                string fileContent = await File.ReadAllTextAsync(filePath);

                object? shiftReport = JsonConvert.DeserializeObject(fileContent);

                string shiftReportJson = JsonConvert.SerializeObject(shiftReport, Formatting.Indented);

                await HandleSuccessfulPrint(shiftReportJson);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error Print Ulang Shift : {ErrorMessage}", ex.Message);
            }
        }

        private async void SelectedShiftID(string ID)
        {
            try
            {
                string destinationFilePath = "DT-Cache\\Transaction\\selectedShiftID.data";

                // Menulis ID shift baru ke file
                File.WriteAllText(destinationFilePath, ID);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error writing new shift ID: {ErrorMessage}", ex.Message);
            }
        }


        private Form CreateBackgroundForm()
        {
            return new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = Location,
                ShowInTaskbar = false
            };
        }

        public async Task CopyShiftDataAsync(string sourceFilePath, string destinationFilePath)
        {
            transactionFileMover c = new();

            string directory = Path.GetDirectoryName(destinationFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string sourceData = await c.ReadJsonFileAsync(sourceFilePath);
            JObject sourceJson = JObject.Parse(sourceData);
            JArray sourceDataArray = (JArray)sourceJson["data"];

            if (File.Exists(destinationFilePath))
            {
                string existingData = await c.ReadJsonFileAsync(destinationFilePath);
                JObject destinationJson = JObject.Parse(existingData);
                JArray destinationData = (JArray)destinationJson["data"];

                foreach (JObject trans in sourceDataArray)
                {
                    destinationData.Add(trans);
                }

                await c.WriteJsonToFile(destinationFilePath, destinationJson.ToString());
            }
            else
            {
                await c.WriteJsonToFile(destinationFilePath, sourceJson.ToString());
            }
        }

        public async Task ClearSourceFileAsync(string sourcePath)
        {
            JObject emptyJson = new() { ["data"] = new JArray() };
            transactionFileMover c = new();

            try
            {
                await c.WriteJsonToFile(sourcePath, emptyJson.ToString());
            }
            catch (IOException ex)
            {
                LoggerUtil.LogError(ex, $"Could not clear the source file: {ex.Message}");
            }
        }

        public async void DirectSwipeShiftData()
        {
            await ArchiveDataShiftAsync("DT-Cache\\Transaction\\shiftData.data", "DT-Cache\\Transaction");
        }

        public async Task ArchiveDataShiftAsync(string sourceFilePath, string archiveDirectory)
        {
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            transactionFileMover c = new();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            string fileExtension = Path.GetExtension(sourceFilePath);
            string newFileName = $"ShiftSwiperDateTimeNow{fileExtension}";
            string destinationPath = Path.Combine(archiveDirectory, newFileName);

            string sourceData = await c.ReadJsonFileAsync(sourceFilePath);
            JObject sourceJson = JObject.Parse(sourceData);
            JArray sourceDataArray = (JArray)sourceJson["data"];

            if (File.Exists(destinationPath))
            {
                string existingData = await c.ReadJsonFileAsync(destinationPath);
                JObject destinationJson = JObject.Parse(existingData);
                JArray destinationData = (JArray)destinationJson["data"];

                foreach (JObject trans in sourceDataArray)
                {
                    destinationData.Add(trans);
                }

                await c.WriteJsonToFile(destinationPath, destinationJson.ToString());
            }
            else
            {
                await c.WriteJsonToFile(destinationPath, sourceJson.ToString());
            }
        }

        private (DateTime startShift, DateTime lastShift) ExGetStartAndLastShiftOrder(TransactionData transactionData)
        {
            // Extract all created_at dates from the transactions and parse them safely
            List<DateTime?> transactionTimes = transactionData.data
                .Select(t =>
                {
                    DateTime parsedDate;
                    bool isValidDate = DateTime.TryParse(t.created_at, out parsedDate);
                    return isValidDate ? parsedDate : (DateTime?)null;
                })
                .Where(t => t.HasValue) // Filter out null values
                .OrderBy(t => t.Value) // Order by date in ascending order
                .ToList();

            if (transactionTimes.Count > 0)
            {
                DateTime startShift = transactionTimes.First().Value; // First transaction is the start of the shift
                DateTime lastShift = transactionTimes.Last().Value; // Last transaction is the end of the shift

                return (startShift, lastShift);
            }

            // If no valid transactions, return default values (in case of an error)
            return (DateTime.MinValue, DateTime.MinValue);
        }
        private (DateTime startShift, DateTime lastShift) GetStartAndLastShiftOrder(TransactionData transactionData)
        {
            try
            {
                DateTime defaultShiftStart = DateTime.Today.AddHours(6);
                // Ekstrak dan parsing tanggal dengan penanganan error yang lebih baik
                List<DateTime?> transactionTimes = transactionData?.data?
                    .Select(t =>
                    {
                        if (string.IsNullOrWhiteSpace(t?.created_at))
                            return defaultShiftStart;

                        DateTime parsedDate;
                        // Gunakan format parsing yang lebih spesifik dan kultur yang sesuai
                        if (DateTime.TryParse(t.created_at, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                        {
                            return parsedDate;
                        }
                        return (DateTime?)defaultShiftStart;
                    })
                    .Where(t => t.HasValue)
                    .OrderBy(t => t.Value)
                    .ToList() ?? new List<DateTime?>();

                // Jika tidak ada transaksi valid, kembalikan default hari ini jam 6 pagi
                if (transactionTimes.Count == 0)
                {
                    defaultShiftStart = DateTime.Today.AddHours(6);
                    return (defaultShiftStart, defaultShiftStart);
                }

                // Ambil tanggal awal dan akhir shift
                DateTime startShift = transactionTimes.First().Value;
                DateTime lastShift = transactionTimes.Last().Value;

                return (startShift, lastShift);
            }
            catch (Exception)
            {
                // Penanganan exception terakhir, kembalikan default
                DateTime defaultShiftStart = DateTime.Today.AddHours(6);
                return (defaultShiftStart, defaultShiftStart);
            }
        }

        private int GetShiftNumber()
        {
            string shiftPath = "DT-Cache\\Transaction\\shiftData.data";
            if (File.Exists(shiftPath))
            {
                try
                {
                    string existingData = File.ReadAllText(shiftPath);
                    JObject? existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);

                    JArray? shiftDataArray = existingTransactions["data"] as JArray;

                    if (shiftDataArray == null || shiftDataArray.Count == 0)
                    {
                        return 1; // Default shift number when no shifts exist
                    }

                    return shiftDataArray.Count + 1;
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "Error occurred while reading or processing the shift data: {ErrorMessage}",
                        ex.Message);
                    return 1; // Return default shift number in case of error
                }
            }

            LoggerUtil.LogError(null, "Shift data file not found, using default shift number 1.");
            return 1;
        }

        private void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                string defaultContent = "{ \"data\": [] }";

                try
                {
                    File.WriteAllText(filePath, defaultContent);
                }
                catch (IOException ex)
                {
                    LoggerUtil.LogError(ex, $"Failed to create file: {filePath}");
                }
            }
        }

        //LOAD DATA Transaction
        public async Task LoadData(bool isBackground = false)
        {
            try
            {

                IsBackgroundOperation = isBackground;

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

                int shiftNumber = GetShiftNumber(); // Get the shift number dynamically

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
                TransactionData transactionDataSaveBill =
                    JsonConvert.DeserializeObject<TransactionData>(jsonContentSavebill);

                DataTable dataTable = CreateDataTable();

                if (ShouldShowProgress())
                {
                    UpdateProgress(90, "Memformat data untuk tampilan...");
                }

                string startAt = GetStartAt();
                DateTime startAtDateTime = DateTime.Parse(startAt);

                //filtering

                // Memfilter transaksi berdasarkan created_at
                List<Transaction> transactionDataFiltered = transactionData.data
                    .Where(t => DateTime.TryParse(t.updated_at, out DateTime createdAt) && createdAt > startAtDateTime)
                    .ToList();

                // Memfilter pengeluaran berdasarkan created_at
                List<ExpenditureStrukShift> filteredExpenditures = expenditureData.data
                    .Where(e => DateTime.TryParse(e.created_at, out DateTime expenditureCreatedAt) &&
                                expenditureCreatedAt > startAtDateTime)
                    .ToList();

                List<Transaction> filteredSavebillCancelBefore = transactionDataSaveBill.data
                    .Where(t => DateTime.TryParse(t.created_at, out DateTime createdAt) && createdAt < startAtDateTime)
                    .ToList();

                List<Transaction> filteredSavebillCancelAfter = transactionDataSaveBill.data
                    .Where(t => DateTime.TryParse(t.created_at, out DateTime createdAt) && createdAt > startAtDateTime)
                    .ToList();

                // Membungkus filteredTransactions menjadi TransactionData
                TransactionData filteredTransactions = new() { data = transactionDataFiltered };
                TransactionData filteredTransactionsCanceledSavebillBefore = new() { data = filteredSavebillCancelBefore };
                TransactionData filteredTransactionsCanceledSavebillAfter = new() { data = filteredSavebillCancelAfter };


                ExpenditureData filteredexpenditureData = new() { data = filteredExpenditures };
                // Get the start and last shift based on transaction times
                var (startShift, lastShift) = GetStartAndLastShiftOrder(filteredTransactions);
                // Add the Start and Last Shift Order to the DataTable
                AddSeparatorRowBold(dataTable, "SHIFT NUMBER " + shiftNumber, dataGridView1);

                AddSeparatorRowBold(dataTable, "Start Shift : " + startAtDateTime, dataGridView1);
                dataTable.Rows.Add("Start Shift Order", $"{startShift.ToString("yyyy-MM-dd HH:mm:ss")}");
                dataTable.Rows.Add("Last Shift Order", $"{lastShift.ToString("yyyy-MM-dd HH:mm:ss")}");

                dataTable.Rows.Add("", "");

                // Process the cart details
                ProcessCartDetails(filteredTransactions, dataTable, "SOLD ITEMS");

                decimal totalProcessedCartQty = CalculateTotalProcessCartDetailsQty(filteredTransactions);
                dataTable.Rows.Add("Total Qty Sold Items", $"{totalProcessedCartQty}");

                decimal totalProcessedCart = CalculateTotalProcessCartDetails(filteredTransactions);
                dataTable.Rows.Add("Total Sold Items", $"{totalProcessedCart:n0}");

                dataTable.Rows.Add("", "");

                // Process refunded items
                ProcessRefundDetails(filteredTransactions, dataTable);

                // Calculate total refund amount
                decimal totalRefundAmount = CalculateTotalRefund(filteredTransactions);
                dataTable.Rows.Add("Total Refund", $"{totalRefundAmount:n0}");

                dataTable.Rows.Add("", "");
                // Process the savebill details
                ProcessCartDetails(transactionDataSaveBill, dataTable, "SAVEBILL/PENDING ITEMS");

                decimal totalPendingCart = CalculateTotalProcessCartDetails(transactionDataSaveBill);
                dataTable.Rows.Add("Total Savebill/Pending Items", $"{totalPendingCart:n0}");

                dataTable.Rows.Add("", "");
                // Cancel the savebill details
                ProcessCartDetails(transactionDataSaveBill, dataTable, "CANCELED ITEMS");

                decimal totalCancelCartBefore = CalculateTotalCanceledItems(filteredTransactionsCanceledSavebillBefore);
                if (totalCancelCartBefore > 0)
                {
                    dataTable.Rows.Add("Total Canceled Items Shift Sebelumnya", $"{totalCancelCartBefore:n0}");
                }

                decimal totalCancelCart = CalculateTotalCanceledItems(filteredTransactionsCanceledSavebillAfter);
                dataTable.Rows.Add("Total Canceled Items Shift Sekarang", $"{totalCancelCart:n0}");

                dataTable.Rows.Add("", "");

                // Process the Expenditure details
                ProcessExpenditureDetails(filteredExpenditures, dataTable, "EXPENDITURE ITEMS");

                decimal totalExpenditureItems = CalculateTotalProcessExpenditures(filteredexpenditureData);
                cashOutExpenditure = int.Parse(totalExpenditureItems.ToString());
                dataTable.Rows.Add("Total Expense Items", $"{totalExpenditureItems:n0}");

                dataTable.Rows.Add("", "");

                // Process the Discounts details
                ProcessDiscountDetails(filteredTransactions, dataTable, "DISCOUNT ITEMS");

                dataTable.Rows.Add("", "");

                // Process payment details
                ProcessPaymentDetails(filteredTransactions, dataTable, totalExpenditureItems);
                dataTable.Rows.Add("", "");
                AddSeparatorRowBold(dataTable, $"CASH DETAILS", dataGridView1);
                dataTable.Rows.Add("Cash Income (Not Include Exp & Ref)", $"{cashIncomeReal:n0}");
                dataTable.Rows.Add("Cash Out Expenditure", $"{cashOutExpenditure:n0}");
                dataTable.Rows.Add("Cash Out Refund", $"{cashOutRefund:n0}");
                ending_cash -= cashOutExpenditure;
                dataTable.Rows.Add("Expected Ending Cash", $"{ending_cash:n0}");

                dataTable.Rows.Add("", "");

                // Add the grand total
                decimal grandTotal = filteredTransactions.data
                    .SelectMany(t => t.cart_details) // Meratakan list cart_details dari setiap Transaction
                    .Sum(cd => cd.total_price); // Menjumlahkan total_price untuk setiap CartDetails
                decimal totalMemberUsePoints = CalculateTotalMemberUsePoints(filteredTransactions.data);

                AddSeparatorRowBold(dataTable, "GRAND TOTAL", dataGridView1);
                grandTotal -= totalRefundAmount;
                totalProcessedCart -= totalExpenditureItems;
                totalProcessedCart -= totalMemberUsePoints;
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
            catch(Exception ex)
            {
                NotifyHelper.Error(ex.Message);
                LoggerUtil.LogError(ex,ex.ToString());
            }
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
            DataTable dataTable = new();
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

        private decimal CalculateTotalCanceledItems(TransactionData transactionData)
        {
            var canceledCartDetails = transactionData.data.SelectMany(t => t.canceled_items)
                    .Where(cd => cd.qty > 0) // Hanya ambil item dengan qty lebih dari 0
                    .GroupBy(cd =>
                        new { cd.menu_id, cd.menu_detail_id }) // Mengelompokkan berdasarkan menu_id dan menu_detail_id
                    .Select(g => new
                    {
                        g.Key.menu_id,
                        g.Key.menu_detail_id,
                        g.First().menu_name, // Mengambil satu nilai menu_name dari salah satu item
                        g.First().menu_type, // Mengambil satu nilai menu_type dari salah satu item
                        varian =
                            g.First().menu_detail_name, // Mengambil satu nilai menu_detail_name dari salah satu item
                        qty = g.Sum(cd => cd.qty), // Menjumlahkan qty
                        total_price = g.Sum(cd => cd.total_price) // Menjumlahkan total_price, jika diperlukan
                    })
                    .ToList();
            decimal totalCanceledAmount = canceledCartDetails.Sum(cd => cd.total_price);
            return totalCanceledAmount;
        }
        private decimal CalculateTotalProcessCartDetails(TransactionData transactionData)
        {
            decimal totalProcessedCart = transactionData.data
                .SelectMany(t => t.cart_details) // Flatten the cart details
                .Where(cd => cd.qty > 0) // Filter only items with quantity > 0
                .Sum(cd => cd.total_price); // Sum the total_price of each cart detail

            return totalProcessedCart;
        }
        private decimal CalculateTotalProcessCartDetailsQty(TransactionData transactionData)
        {
            decimal totalProcessedCart = transactionData.data
                .SelectMany(t => t.cart_details) // Flatten the cart details
                .Where(cd => cd.qty > 0) // Filter only items with quantity > 0
                .Sum(cd => cd.qty); // Sum the total_price of each cart detail

            return totalProcessedCart;
        }

        private void ProcessExpenditureDetails(List<ExpenditureStrukShift> filteredExpenditures, DataTable dataTable,
            string text)
        {
            AddSeparatorRowBold(dataTable, text, dataGridView1);
            foreach (ExpenditureStrukShift expenditure in filteredExpenditures)
            {
                dataTable.Rows.Add(expenditure.description, $"{expenditure.nominal:n0}");
            }
        }

        private void ProcessDiscountDetails(TransactionData transactionData, DataTable dataTable, string text)
        {
            AddSeparatorRowBold(dataTable, text, dataGridView1);
            decimal groupedCartCarts = transactionData.data
                .Where(t => t.discounted_price > 0) // Filter transactions with discounted_price > 0
                .Sum(t => t.discounted_price); // Sum the discounted_price of each transaction
            dataTable.Rows.Add("Discount Per Carts", $"{groupedCartCarts:n0}");
            decimal groupedCartDetails = transactionData.data
                .SelectMany(t => t.cart_details) // Flatten the cart details
                .Where(cd => cd.discounted_price > 0) // Filter only items with discounted_price > 0
                .Sum(cd => cd.discounted_price); // Sum the discounted_price of each cart detail
            dataTable.Rows.Add("Discount Per Items", $"{groupedCartDetails:n0}");
            decimal totalDiscounts = groupedCartCarts + groupedCartDetails;
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

        private void ProcessPaymentDetails(TransactionData transactionData, DataTable dataTable, decimal expenditures)
        {
            AddSeparatorRowBold(dataTable, "PAYMENT DETAILS", dataGridView1);
            List<PaymentType> paymentTypes = LoadPaymentTypes();

            var groupedPayments = transactionData.data
                .GroupBy(t => t.payment_type_id)
                .Select(g => new
                {
                    PaymentTypeId = g.Key,
                    PaymentTypeName = paymentTypes.FirstOrDefault(p => p.id == g.Key)?.name ?? "Unknown",
                    TotalAmount = g.Sum(x => x.total),
                    MemberUsePoints = g.Sum(x => x.member_use_point ?? 0) // Sum member_use_points for the payment type
                }).ToList();

            var groupedRefunds = transactionData.data
                .Where(t => t.is_refund_all == 1)
                .Select(t => new
                {
                    PaymentTypeId = t.refund_payment_id_all,
                    TotalRefund = t.total_refund
                }).ToList();

            var groupedItemRefunds = transactionData.data
                .SelectMany(t => t.refund_details.Select(r => new
                {
                    PaymentTypeId = r.refund_payment_type_id_item,
                    RefundTotal = r.refund_total
                }))
                .GroupBy(r => r.PaymentTypeId)
                .Select(g => new
                {
                    PaymentTypeId = g.Key,
                    TotalItemRefund = g.Sum(x => x.RefundTotal)
                }).ToList();

            decimal totalMemberUsePoints = CalculateTotalMemberUsePoints(transactionData.data);
            foreach (var payment in groupedPayments)
            {
                var refund = groupedRefunds.FirstOrDefault(r => r.PaymentTypeId == payment.PaymentTypeId);
                decimal totalRefund = refund?.TotalRefund ?? 0; // Default to 0 if not found

                var itemRefund = groupedItemRefunds.FirstOrDefault(r => r.PaymentTypeId == payment.PaymentTypeId);
                decimal totalItemRefund = itemRefund?.TotalItemRefund ?? 0; // Default to 0 if not found

                decimal netAmount = payment.TotalAmount - (totalRefund + totalItemRefund);


                string paymentTypeString = payment.PaymentTypeName;
                if (payment.PaymentTypeName.Equals("Tunai", StringComparison.OrdinalIgnoreCase))
                {
                    paymentTypeString = "CASH ON POS";
                    cashOutRefund = int.Parse(totalRefund.ToString()) + int.Parse(totalItemRefund.ToString());
                    cashIncomeReal = payment.TotalAmount;
                    //CashOnPOS = netAmount;
                    //netAmount -= expenditures;
                    txtActualCash.Text = $"{netAmount:n0}";
                    ending_cash = int.Parse(netAmount.ToString());
                }

                dataTable.Rows.Add(paymentTypeString, $"{netAmount:n0}");
            }

            dataTable.Rows.Add("", $"");

            AddSeparatorRowBold(dataTable, "POINT MEMBER DETAILS", dataGridView1);
            dataTable.Rows.Add("POINT MEMBER USED", $"{totalMemberUsePoints:n0}");
        }
        private decimal CalculateTotalMemberUsePoints(IEnumerable<Transaction> transactions)
        {
            decimal totalUsePoints = 0;

            foreach (var transaction in transactions)
            {
                if (transaction.member_use_point.HasValue)
                {
                    totalUsePoints += transaction.member_use_point.Value;
                }
            }

            return totalUsePoints;
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
                        DataGridViewCellStyle boldStyle = new()
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
                    dataGridView1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point,
                        0);
                }

                dataGridView1.DataSource =
                    dataTable; // Assuming dataTable is a class-level variable or passed as a parameter
                DataGridViewCellStyle boldStyle = new() { Font = new Font(dataGridView1.Font, FontStyle.Italic) };
                dataGridView1.Columns["DATA"].DefaultCellStyle = boldStyle;
                dataGridView1.Columns["ID"].Visible = false;

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred while reinitializing dataGridView1: {ErrorMessage}",
                    ex.Message);
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
            DataRow separatorRow = dataTable.NewRow();
            separatorRow["DATA"] = text;

            dataTable.Rows.Add(separatorRow);

            dataGridView.DataSource = dataTable;
            dataGridView.Refresh(); // Ensure DataGridView is updated

            if (dataGridView.Rows.Count > 0)
            {
                DataGridViewCellStyle boldStyle = new() { Font = new Font(dataGridView.Font, FontStyle.Bold) };

                int lastRowIndex = dataGridView.Rows.Count - 1;
                if (dataGridView.Columns.Contains("DATA"))
                {
                    dataGridView.Rows[lastRowIndex].Cells["DATA"].Style = boldStyle;
                }
                else
                {
                    LoggerUtil.LogError(null, "The 'DATA' column does not exist in the DataGridView.");
                }
            }
            else
            {
                LoggerUtil.LogError(null, "No rows found in the DataGridView to apply bold style.");
            }
        }
    }
}