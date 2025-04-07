using System.Data;
using System.Globalization;
using KASIR.Model;
using KASIR.Network;
using KASIR.Printer;
using Newtonsoft.Json;

namespace KASIR.Komponen
{
    public partial class printReportShift : Form
    {
        private successTransaction SuccessTransaction { get; set; }
        public bool ReloadDataInBaseForm { get; private set; }
        private readonly string baseOutlet;
        DateTime mulaishift, akhirshift;
        public printReportShift()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            btnPrintUlang.Enabled = false;
            LoadDataCetakUlang();
            comboBoxPrinters.Visible = false;
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
        public bool IsStartShiftMoreThanOneHourAgo()
        {
            TimeSpan difference = akhirshift - mulaishift;
            if (difference.TotalHours > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void convertDateTime(string mulai, string akhir)
        {
            // Attempt to convert the string to a DateTime object using the DateTime.ParseExact method
            try
            {
                mulaishift = DateTime.ParseExact(mulai, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
                akhirshift = DateTime.ParseExact(akhir, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        private async void LoadDataCetakUlang()
        {
            const int maxRetryAttempts = 3;
            int retryAttempts = 0;
            bool success = false;

            while (retryAttempts < maxRetryAttempts && !success)
            {
                try
                {
                    IApiService apiService = new ApiService();

                    string response = await apiService.Get("/struct-shift?outlet_id=" + baseOutlet);

                    if (response != null)
                    {
                        try
                        {
                            GetShift cetakUlang = JsonConvert.DeserializeObject<GetShift>(response);

                            DataShift datas = cetakUlang.data;
                            List<ExpenditureStrukShift> expenditures = datas.expenditures;
                            List<CartDetailsSuccessStrukShift> cartDetailsSuccess = datas.cart_details_success;
                            List<CartDetailsPendingStrukShift> cartDetailsPending = datas.cart_details_pending;
                            List<CartDetailsCanceledStrukShift> cartDetailsCanceled = datas.cart_details_canceled;
                            List<RefundDetailStrukShift> refundDetails = datas.refund_details;
                            List<PaymentDetailStrukShift> paymentDetails = datas.payment_details;
                            DataTable dataTable = new DataTable();
                            DataTable dataTablePending = new DataTable();
                            DataTable dataTableCancel = new DataTable();
                            DataTable dataTableRefund = new DataTable();
                            DataTable dataTableExpense = new DataTable();
                            DataTable dataTablePayment = new DataTable();

                            dataTable.Columns.Add("ID", typeof(string)); // Add a column to avoid header error
                            dataTable.Columns.Add("DATA", typeof(string));
                            dataTable.Columns.Add("Detail", typeof(string));

                            dataTablePending.Columns.Add("ID", typeof(string)); // Add a column to avoid header error
                            dataTablePending.Columns.Add("DATA", typeof(string));
                            dataTablePending.Columns.Add("Detail", typeof(string));

                            dataTableCancel.Columns.Add("ID", typeof(string)); // Add a column to avoid header error
                            dataTableCancel.Columns.Add("DATA", typeof(string));
                            dataTableCancel.Columns.Add("Detail", typeof(string));

                            dataTableRefund.Columns.Add("ID", typeof(string)); // Add a column to avoid header error
                            dataTableRefund.Columns.Add("DATA", typeof(string));
                            dataTableRefund.Columns.Add("Detail", typeof(string));

                            dataTableExpense.Columns.Add("ID", typeof(string)); // Add a column to avoid header error
                            dataTableExpense.Columns.Add("DATA", typeof(string));
                            dataTableExpense.Columns.Add("Detail", typeof(string));

                            dataTablePayment.Columns.Add("ID", typeof(string)); // Add a column to avoid header error
                            dataTablePayment.Columns.Add("DATA", typeof(string));
                            dataTablePayment.Columns.Add("Detail", typeof(string));

                            lblDateTime.Text = $"Start Date : {datas.start_date}\nEnd Date : {datas.end_date}";
                            lblCashierName.Text = $"Cashier Name : {datas.casher_name}";

                            lblRiwayatShift.Text += " | Shift: " + datas.shift_number.ToString();


                            var sortedcartDetailSuccess = cartDetailsSuccess.OrderBy(x =>
                            {
                                if (x.menu_type.Contains("Minuman")) return 1;
                                if (x.menu_type.Contains("Additional Minuman")) return 2;
                                if (x.menu_type.Contains("Makanan")) return 3;
                                if (x.menu_type.Contains("Additional Makanan")) return 4;
                                return 5;
                            })
                            .ThenBy(x => x.menu_name);

                            foreach (var cartDetail in sortedcartDetailSuccess)
                            {
                                // Add varian to the cart detail name if it's not null
                                string displayMenuName = cartDetail.menu_name;
                                if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                {
                                    displayMenuName += "\n - " + cartDetail.varian;
                                }

                                dataTable.Rows.Add(null, string.Format("{0}", cartDetail.qty) + "x " + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                            }

                            lblQTY1.Text = "Qty : " + datas.totalSuccessQty.ToString();
                            lblTotal1.Text = string.Format("{0:n0}", datas.totalCartSuccessAmount);

                            if (cartDetailsPending.Count != 0)
                            {

                                var sortedcartDetailPendings = cartDetailsPending.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);

                                foreach (var cartDetail in sortedcartDetailPendings)
                                {
                                    // Add varian to the cart detail name if it's not null
                                    string displayMenuName = cartDetail.menu_name;
                                    if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                    {
                                        displayMenuName += "\n - " + cartDetail.varian;
                                    }

                                    dataTablePending.Rows.Add(null, string.Format("{0}", cartDetail.qty) + "x " + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                                }

                                lblQTY2.Text = "Qty : " + datas.totalPendingQty.ToString();
                                lblTotal2.Text = string.Format("{0:n0}", datas.totalCartPendingAmount);
                            }

                            if (cartDetailsCanceled.Count != 0)
                            {

                                var sortedcartDetailCanceled = cartDetailsCanceled.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);

                                foreach (var cartDetail in sortedcartDetailCanceled)
                                {
                                    // Add varian to the cart detail name if it's not null
                                    string displayMenuName = cartDetail.menu_name;
                                    if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                    {
                                        displayMenuName += "\n - " + cartDetail.varian;
                                    }

                                    dataTableCancel.Rows.Add(null, string.Format("{0}", cartDetail.qty) + "x " + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                                }

                                lblQTY3.Text = "Qty : " + datas.totalCanceledQty.ToString();
                                lblTotal3.Text = string.Format("{0:n0}", datas.totalCartCanceledAmount);
                            }
                            if (refundDetails.Count != 0)
                            {

                                var sortedrefoundDetails = refundDetails.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);

                                foreach (var cartDetail in sortedrefoundDetails)
                                {
                                    // Add varian to the cart detail name if it's not null
                                    string displayMenuName = cartDetail.menu_name;
                                    if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                    {
                                        displayMenuName += "\n - " + cartDetail.varian;
                                    }
                                    dataTableRefund.Rows.Add(null, string.Format("Qty : {0}", cartDetail.qty_refund_item) + "x " + displayMenuName, string.Format("{0:n0}", cartDetail.total_refund_price));
                                }

                                lblQTY1.Text = "Qty : " + datas.totalRefundQty.ToString();
                                lblTotal1.Text = string.Format("{0:n0}", datas.totalCartRefundAmount);
                            }
                            if (expenditures.Count != 0)
                            {
                                foreach (var expense in expenditures)
                                {
                                    dataTableExpense.Rows.Add(null, expense.description, string.Format("{0:n0}", expense.nominal));
                                }
                                lblTotalExpense.Text = string.Format("{0:n0}", datas.expenditures_total);
                            }
                            lblPaymentDetail.Text = $"Expected Ending Cash: {string.Format("{0:n0}", datas.ending_cash_expected)}" + "\n" +
                                "Actual Ending Cash: " + string.Format("{0:n0}", datas.ending_cash_actual) +
                                "\n" + "Cash Difference: " + string.Format("{0:n0}", datas.cash_difference) + "\n";
                            lblTotalDetail.Text = "All Discount Items: " + string.Format("{0:n0}", datas.discount_amount_per_items) + "\n" +
                            "All Discount Cart: " + string.Format("{0:n0}", datas.discount_amount_transactions) + "\n" +
                            "TOTAL AMOUNT: " + string.Format("{0:n0}", datas.discount_total_amount);

                            foreach (var paymentDetail in paymentDetails)
                            {
                                dataTablePayment.Rows.Add(null, paymentDetail.payment_category, "");
                                foreach (var paymentType in paymentDetail.payment_type_detail)
                                {
                                    dataTablePayment.Rows.Add(null, paymentType.payment_type, string.Format("{0:n0}", paymentType.total_payment));
                                }
                                lblTotalPayment.Text = string.Format("{0:n0}", paymentDetail.total_amount);

                            }
                            lblTotalTransaction.Text = "TOTAL TRANSACTION : " + string.Format("{0:n0}", datas.total_transaction);


                            if (dataGridView2 == null || dataGridView2.Font == null)
                            {
                                ReloadDataGridView2(dataTable);
                            }
                            else
                            {
                                dataGridView1.DataSource = dataTablePending;
                                dataGridView2.DataSource = dataTable;
                                dataGridView3.DataSource = dataTableCancel;
                                dataGridView4.DataSource = dataTableRefund;
                                dataGridView5.DataSource = dataTableExpense;
                                dataGridView6.DataSource = dataTablePayment;

                                // Check dataGridView2 for null before accessing its properties
                                if (dataGridView2.Columns["ID"] != null)
                                {
                                    dataGridView2.Columns["ID"].Visible = false;
                                }
                                if (dataGridView1.Columns["ID"] != null)
                                {
                                    dataGridView1.Columns["ID"].Visible = false;
                                }
                                if (dataGridView3.Columns["ID"] != null)
                                {
                                    dataGridView3.Columns["ID"].Visible = false;
                                }
                                if (dataGridView4.Columns["ID"] != null)
                                {
                                    dataGridView4.Columns["ID"].Visible = false;
                                }
                                if (dataGridView5.Columns["ID"] != null)
                                {
                                    dataGridView5.Columns["ID"].Visible = false;
                                }
                                if (dataGridView6.Columns["ID"] != null)
                                {
                                    dataGridView6.Columns["ID"].Visible = false;
                                }
                            }
                            btnPrintUlang.Enabled = true;
                            success = true; // Successfully loaded data
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
                            btnPrintUlang.Enabled = true;
                            MessageBox.Show("Error: " + ex.Message);
                            LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                            break; // Stop retrying on other exceptions
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    break; // Do not retry on TaskCanceledException
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    break; // Stop retrying on other exceptions
                }
            }
        }
        private void ReloadDataGridView2(DataTable dataTable)
        {
            try
            {
                // Attempt to reinitialize dataGridView1 if it is null
                if (dataGridView2 == null)
                {
                    dataGridView2 = new DataGridView();
                }

                // Attempt to set the Font property if it is null
                if (dataGridView2.Font == null)
                {
                    dataGridView2.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                }

                // Reapply data source and styles
                dataGridView2.DataSource = dataTable; // Assuming dataTable is a class-level variable or passed as a parameter
                DataGridViewCellStyle boldStyle = new DataGridViewCellStyle
                {
                    Font = new Font(dataGridView2.Font, FontStyle.Italic)
                };
                dataGridView2.Columns["DATA"].DefaultCellStyle = boldStyle;
                dataGridView2.Columns["ID"].Visible = false;

                // Log success
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the reinitialization process
                LoggerUtil.LogError(ex, "An error occurred while reinitializing dataGridView1: {ErrorMessage}", ex.Message);
            }
        }
        private string LeftingText(string param1, string param2)
        {
            // Menyusun teks dengan parameter 1, spasi, dan parameter 2
            string formattedText = $"{param1} {param2}";

            // Menentukan panjang spasi yang diperlukan untuk rata kiri (32 karakter)
            int paddingLength = Math.Max(0, 32 - formattedText.Length);

            // Format baris dengan padding dan alignment yang benar
            string result = formattedText.PadRight(paddingLength);

            return result;
        }

        // Fungsi untuk mengatur teks di tengah

        private string CenterText(string text)
        {
            //outlet 2 try fix by
            if (text == null)
            {
                return string.Empty; // or some other default value
            }
            //
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
        private async void cekShiftSekarang(DataShift datas, List<ExpenditureStrukShift> expenditures, List<CartDetailsSuccessStrukShift> cartDetailsSuccess, List<CartDetailsPendingStrukShift> cartDetailsPendings, List<CartDetailsCanceledStrukShift> cartDetailsCanceled, List<RefundDetailStrukShift> refundDetails, List<PaymentDetailStrukShift> paymentDetails)
        {
            try
            {

                string strukText = "\n" + CenterText(datas.outlet_name) + "\n";
                strukText += CenterText(datas.outlet_address) + "\n";
                strukText += CenterText(datas.outlet_phone_number) + "\n";
                strukText += "--------------------------------\n";
                strukText += CenterText("SHIFT PRINT") + "\n";
                strukText += "--------------------------------\n";
                strukText += FormatSimpleLine("Start Date", datas.start_date) + "\n";
                strukText += FormatSimpleLine("End Date", datas.end_date) + "\n";
                strukText += FormatSimpleLine("Shift Number", datas.shift_number) + "\n";
                strukText += "--------------------------------\n";
                strukText += CenterText("ORDER DETAILS") + "\n";
                strukText += "--------------------------------\n";
                strukText += LeftingText("SOLD ITEMS", "") + "\n";
                var sortedcartDetailSuccess = cartDetailsSuccess.OrderBy(x =>
                {
                    if (x.menu_type.Contains("Minuman")) return 1;
                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                    if (x.menu_type.Contains("Makanan")) return 3;
                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                    return 5;
                })
.ThenBy(x => x.menu_name);
                //foreach (var cartDetail in cartDetailsSuccess)
                foreach (var cartDetail in sortedcartDetailSuccess)
                {
                    // Add varian to the cart detail name if it's not null
                    string displayMenuName = cartDetail.menu_name;
                    if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                    {
                        displayMenuName += "\n - " + cartDetail.varian;
                    }

                    strukText += LeftingText(displayMenuName, "") + "\n";
                    strukText += FormatSimpleLine(cartDetail.qty.ToString(), string.Format("{0:n0}", cartDetail.total_price)) + "\n";
                }
                strukText += FormatSimpleLine("Item Sold Qty", datas.totalSuccessQty);
                strukText += FormatSimpleLine("\nItem Sold Amount", string.Format("{0:n0}", datas.totalCartSuccessAmount));
                if (cartDetailsPendings.Count != 0)
                {
                    strukText += "\n\n--------------------------------\n";
                    strukText += LeftingText("PENDING ITEMS", "") + "\n";
                    var sortedcartDetailPendings = cartDetailsPendings.OrderBy(x =>
                    {
                        if (x.menu_type.Contains("Minuman")) return 1;
                        if (x.menu_type.Contains("Additional Minuman")) return 2;
                        if (x.menu_type.Contains("Makanan")) return 3;
                        if (x.menu_type.Contains("Additional Makanan")) return 4;
                        return 5;
                    })
.ThenBy(x => x.menu_name);
                    //foreach (var cartDetail in cartDetailsPendings)
                    foreach (var cartDetail in sortedcartDetailPendings)
                    {
                        // Add varian to the cart detail name if it's not null
                        string displayMenuName = cartDetail.menu_name;
                        if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                        {
                            displayMenuName += "\n - " + cartDetail.varian;
                        }

                        strukText += LeftingText(displayMenuName, "") + "\n";
                        strukText += FormatSimpleLine(cartDetail.qty.ToString(), string.Format("{0:n0}", cartDetail.total_price)) + "\n";
                    }
                    strukText += FormatSimpleLine("Item Pending Qty", datas.totalPendingQty);
                    strukText += FormatSimpleLine("\nItem Pending Amount", string.Format("{0:n0}", datas.totalCartPendingAmount));
                }
                if (cartDetailsCanceled.Count != 0)
                {
                    strukText += "\n--------------------------------\n";
                    strukText += LeftingText("CANCEL ITEMS", "") + "\n";
                    var sortedcartDetailCanceled = cartDetailsCanceled.OrderBy(x =>
                    {
                        if (x.menu_type.Contains("Minuman")) return 1;
                        if (x.menu_type.Contains("Additional Minuman")) return 2;
                        if (x.menu_type.Contains("Makanan")) return 3;
                        if (x.menu_type.Contains("Additional Makanan")) return 4;
                        return 5;
                    })
.ThenBy(x => x.menu_name);
                    //foreach (var cartDetail in cartDetailsCanceled)
                    foreach (var cartDetail in sortedcartDetailCanceled)
                    {
                        // Add varian to the cart detail name if it's not null
                        string displayMenuName = cartDetail.menu_name;
                        if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                        {
                            displayMenuName += "\n - " + cartDetail.varian;
                        }

                        strukText += LeftingText(displayMenuName, "") + "\n";
                        strukText += FormatSimpleLine(cartDetail.qty.ToString(), string.Format("{0:n0}", cartDetail.total_price)) + "\n";
                    }
                    strukText += FormatSimpleLine("Item Cancel Qty", datas.totalCanceledQty);
                    strukText += FormatSimpleLine("\nItem Cancel Amount", string.Format("{0:n0}", datas.totalCartCanceledAmount));
                }
                if (refundDetails.Count != 0)
                {
                    strukText += "\n--------------------------------\n";
                    strukText += LeftingText("REFUND ITEMS", "") + "\n";
                    var sortedrefoundDetails = refundDetails.OrderBy(x =>
                    {
                        if (x.menu_type.Contains("Minuman")) return 1;
                        if (x.menu_type.Contains("Additional Minuman")) return 2;
                        if (x.menu_type.Contains("Makanan")) return 3;
                        if (x.menu_type.Contains("Additional Makanan")) return 4;
                        return 5;
                    })
.ThenBy(x => x.menu_name);
                    //foreach (var cartDetail in refundDetails)
                    foreach (var cartDetail in sortedrefoundDetails)
                    {
                        // Add varian to the cart detail name if it's not null
                        string displayMenuName = cartDetail.menu_name;
                        if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                        {
                            displayMenuName += "\n - " + cartDetail.varian;
                        }

                        strukText += LeftingText(displayMenuName, "") + "\n";
                        strukText += FormatSimpleLine(cartDetail.qty_refund_item.ToString(), string.Format("{0:n0}", cartDetail.total_refund_price)) + "\n";
                    }
                    strukText += FormatSimpleLine("Item Refund Qty", datas.totalRefundQty);
                    strukText += FormatSimpleLine("\nItem Refund Amount", string.Format("{0:n0}", datas.totalCartRefundAmount));
                }
                strukText += "\n--------------------------------\n";
                strukText += CenterText("CASH MANAGEMENT") + "\n";
                strukText += "--------------------------------\n";
                if (expenditures.Count != 0)
                {
                    strukText += LeftingText("EXPENSE", "") + "\n";
                    foreach (var expense in expenditures)
                    {
                        strukText += FormatSimpleLine(expense.description, string.Format("{0:n0}", expense.nominal)) + "\n";
                    }
                }
                strukText += FormatSimpleLine("Expected Ending Cash", string.Format("{0:n0}", datas.ending_cash_expected)) + "\n";
                strukText += FormatSimpleLine("Actual Ending Cash", string.Format("{0:n0}", datas.ending_cash_actual)) + "\n";
                strukText += FormatSimpleLine("Cash Difference", string.Format("{0:n0}", datas.cash_difference)) + "\n";
                strukText += "--------------------------------\n";
                strukText += LeftingText("DISCOUNTS", "") + "\n";
                strukText += FormatSimpleLine("All Discount items", string.Format("{0:n0}", datas.discount_amount_per_items)) + "\n";
                strukText += FormatSimpleLine("All Discount Cart", string.Format("{0:n0}", datas.discount_amount_per_items)) + "\n";
                strukText += FormatSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", datas.discount_total_amount)) + "\n";
                strukText += "--------------------------------\n";
                strukText += CenterText("PAYMENT DETAIL") + "\n";
                strukText += "--------------------------------\n";
                foreach (var paymentDetail in paymentDetails)
                {
                    strukText += LeftingText(paymentDetail.payment_category, "") + "\n";
                    foreach (var paymentType in paymentDetail.payment_type_detail)
                    {
                        strukText += FormatSimpleLine(paymentType.payment_type, string.Format("{0:n0}", paymentType.total_payment)) + "\n";
                    }
                    strukText += FormatSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount)) + "\n";
                    strukText += "--------------------------------\n";
                }
                strukText += FormatSimpleLine("TOTAL TRANSACTION", string.Format("{0:n0}", datas.total_transaction)) + "\n";
                strukText += "\n\n\n\n\n";
                System.Windows.Forms.Label strukLabel = new System.Windows.Forms.Label();
                strukLabel.Text = strukText;
                strukLabel.BackColor = Color.Transparent;
                strukLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                strukLabel.AutoSize = true;
                panel13.Controls.Add(strukLabel);

                // Iterate through cart details and group by serving_type_name
                var servingTypes = cartDetailsSuccess.Select(cd => cd.serving_type_name).Distinct();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }
        private async Task HandleSuccessfulTransaction(string response)
        {
            int retryCount = 3; // Jumlah maksimal percobaan ulang
            int delayBetweenRetries = 2000; // Jeda antara percobaan dalam milidetik (2 detik)
            int currentRetry = 0;

            while (currentRetry < retryCount)
            {
                try
                {
                    GetStrukShift shiftUlang = JsonConvert.DeserializeObject<GetStrukShift>(response);
                    PrinterModel printerModel = new PrinterModel();

                    if (shiftUlang != null && shiftUlang.data != null)
                    {
                        DataStrukShift dataShifts = shiftUlang.data;
                        List<ExpenditureStrukShift> expenditures = dataShifts.expenditures;
                        List<CartDetailsSuccessStrukShift> cartDetailsSuccess = dataShifts.cart_details_success;
                        List<CartDetailsPendingStrukShift> cartDetailsPending = dataShifts.cart_details_pending;
                        List<CartDetailsCanceledStrukShift> cartDetailsCanceled = dataShifts.cart_details_canceled;
                        List<RefundDetailStrukShift> refundDetails = dataShifts.refund_details;
                        List<PaymentDetailStrukShift> paymentDetails = dataShifts.payment_details;

                        await printerModel.PrintModelCetakUlangShift(dataShifts, expenditures, cartDetailsSuccess, cartDetailsPending, cartDetailsCanceled, refundDetails, paymentDetails);
                    }

                    // Jika berhasil, keluar dari loop
                    break;
                }
                catch (Exception ex)
                {
                    currentRetry++;

                    if (currentRetry >= retryCount)
                    {
                        // Jika sudah mencapai batas retry, log error
                        LoggerUtil.LogError(ex, $"Error printing transaction after {currentRetry} attempts", ex);
                    }
                    else
                    {
                        // Tunda sebelum mencoba ulang
                        await Task.Delay(delayBetweenRetries); // Menunggu sebelum mencoba lagi
                    }
                }
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

        private void txtSelesaiShift_TextChanged_1(object sender, EventArgs e)
        {
            System.Windows.Forms.TextBox textBox = (System.Windows.Forms.TextBox)sender;
            string input = textBox.Text.Replace(":", "");

            if (input.Length >= 2)
            {
                string hours = input.Substring(0, 2);
                int parsedHours;
                if (int.TryParse(hours, out parsedHours) && parsedHours >= 0 && parsedHours <= 24)
                {
                    string minutes = input.Substring(2).PadRight(2, '0').Substring(0, 2);
                    textBox.Text = $"{parsedHours:D2}:{minutes}";
                }
                else
                {
                    textBox.Text = "00:00";
                }
            }
        }

        private void txtMulaiShift_TextChanged(object sender, EventArgs e)
        {

            System.Windows.Forms.TextBox textBox = (System.Windows.Forms.TextBox)sender;
            string input = textBox.Text.Replace(":", "");

            if (input.Length >= 2)
            {
                string hours = input.Substring(0, 2);
                int parsedHours;
                if (int.TryParse(hours, out parsedHours) && parsedHours >= 0 && parsedHours <= 24)
                {
                    string minutes = input.Substring(2).PadRight(2, '0').Substring(0, 2);
                    textBox.Text = $"{parsedHours:D2}:{minutes}";
                }
                else
                {
                    textBox.Text = "00:00";
                }
            }
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void maskedTextBox_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private async void btnPrintUlang_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnPrintUlang_Click));

            try
            {
                IApiService apiService = new ApiService();

                string response = await apiService.Get("/struct-shift?outlet_id=" + baseOutlet);


                if (response != null)
                {
                    try
                    {
                        GetStrukShift shifUlang = JsonConvert.DeserializeObject<GetStrukShift>(response);

                        DataStrukShift dataShifts = shifUlang.data;
                        List<ExpenditureStrukShift> expenditures = dataShifts.expenditures;
                        List<CartDetailsSuccessStrukShift> cartDetailsSuccess = dataShifts.cart_details_success;
                        List<CartDetailsPendingStrukShift> cartDetailsPending = dataShifts.cart_details_pending;
                        List<CartDetailsCanceledStrukShift> cartDetailsCanceled = dataShifts.cart_details_canceled;
                        List<RefundDetailStrukShift> refundDetails = dataShifts.refund_details;
                        List<PaymentDetailStrukShift> paymentDetails = dataShifts.payment_details;
                        //util.sendLogTelegram("Cetak Ulang Shift \nBefore Print\n" + dataShifts.ToString());
                        if (response != null)
                        {
                            await HandleSuccessfulTransaction(response);
                        }
                        else
                        {
                            MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetButtonState();
                        }

                        /*
                        await Task.Run(async () =>
                        {
                            //PrintShiftUlangReceipt2(dataShifts, expenditures, cartDetailsSuccess, cartDetailsPending, cartDetailsCanceled, refundDetails, paymentDetails);
                            PrintShiftUlangReceipt(dataShifts, expenditures, cartDetailsSuccess, cartDetailsPending, cartDetailsCanceled, refundDetails, paymentDetails);
                        });
                        */
                        this.Close(); // Close the payForm
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error printing: " + ex.Message);
                        LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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
                MessageBox.Show("Gagal cetak laporan " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        private void ResetButtonState()
        {
            btnSimpan.Enabled = true;
            btnSimpan.Text = "Cetak Shift Ulang";
            btnSimpan.BackColor = Color.FromArgb(31, 30, 68);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}