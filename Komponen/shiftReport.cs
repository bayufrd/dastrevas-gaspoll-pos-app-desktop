﻿
using FontAwesome.Sharp;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Printer;
namespace KASIR.Komponen
{
    public partial class shiftReport : UserControl
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private ApiService apiService;
        private readonly string baseOutlet;

        private readonly string MacAddressKasir;
        private readonly string PinPrinterKasir;
        private readonly string BaseOutletName;
        int bedaCash = 0;
        int shiftnumber;
        DateTime mulaishift, akhirshift;
        Util util = new Util();

        public shiftReport()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
            InitializeComponent();
            apiService = new ApiService();
            btnCetakStruk.Enabled = true;
            lblNotifikasi.Visible = false;
            LoadData();
            lblShiftSekarang.Visible = false;
        }
        private async void LoadData()
        {
            const int maxRetryAttempts = 3;
            int retryAttempts = 0;
            bool success = false;

            while (retryAttempts < maxRetryAttempts && !success)
            {
                try
                {
                    IApiService apiService = new ApiService();

                    string response = await apiService.CekShift("/shift?outlet_id=" + baseOutlet);
                    if (response != null)
                    {
                        try
                        {
                            GetShift cekShift = JsonConvert.DeserializeObject<GetShift>(response);

                            DataShift datas = cekShift.data;
                            List<ExpenditureStrukShift> expenditures = datas.expenditures;
                            List<CartDetailsSuccessStrukShift> cartDetailsSuccess = datas.cart_details_success;
                            List<CartDetailsPendingStrukShift> cartDetailsPending = datas.cart_details_pending;
                            List<CartDetailsCanceledStrukShift> cartDetailsCanceled = datas.cart_details_canceled;
                            List<RefundDetailStrukShift> refundDetails = datas.refund_details;
                            List<PaymentDetailStrukShift> paymentDetails = datas.payment_details;
                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("ID", typeof(string)); // Add a column to avoid header error
                            dataTable.Columns.Add("DATA", typeof(string));
                            dataTable.Columns.Add("Detail", typeof(string));

                            //dataTable.Rows.Add(null, "SHIFT REPORT : -", null); // Add a separator row
                            // Panggil metode untuk menambahkan separator row
                            AddSeparatorRow(dataTable, "SHIFT REPORT", dataGridView1);
                            // Panggil metode untuk menambahkan separator row
                            // Mengonversi string ke DateTime
                            DateTime dateTime = DateTime.Parse(datas.start_date.ToString(), CultureInfo.InvariantCulture);

                            // Format tanggal dan waktu sesuai dengan yang diinginkan
                            string formattedDateTime = dateTime.ToString("dddd, dd MMMM yyyy 'Pukul' HH:mm:ss", new CultureInfo("id-ID"));

                            AddSeparatorRow(dataTable, "Start Date : " + formattedDateTime, dataGridView1);
                            // Panggil metode untuk menambahkan separator row
                            //AddSeparatorRow(dataTable, "End Date : " + datas.end_date.ToString(), dataGridView1);
                            //dataTable.Rows.Add(null, "Start Date :", datas.start_date);
                            //dataTable.Rows.Add(null, "End Date :", datas.end_date);
                            string mulai = datas.start_date.ToString();
                            string akhir = datas.end_date.ToString();
                            convertDateTime(mulai, akhir);
                            shiftnumber = datas.shift_number;
                            shiftnumber += 1;
                            lblShiftSekarang.Text += " | Shift: " + shiftnumber;
                            AddSeparatorRow(dataTable, "SHIFT NUMBER : " + shiftnumber.ToString(), dataGridView1);


                            //dataTable.Rows.Add(null, "---------------------", null); // Add a separator 
                            AddSeparatorRow(dataTable, " ", dataGridView1);

                            AddSeparatorRow(dataTable, "ORDER DETAILS", dataGridView1);
                            //dataTable.Rows.Add(null, "---------------------", null); // Add a separator 
                            AddSeparatorRow(dataTable, "SOLD ITEMS", dataGridView1);

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

                                //dataTable.Rows.Add(null, displayMenuName, null);
                                dataTable.Rows.Add(null, string.Format("{0}x ", cartDetail.qty) + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                            }

                            dataTable.Rows.Add(null, "Item Sold Qty :", datas.totalSuccessQty);
                            dataTable.Rows.Add(null, "Item Sold Amount :", string.Format("{0:n0}", datas.totalCartSuccessAmount));

                            if (cartDetailsPending.Count != 0)
                            {
                                AddSeparatorRow(dataTable, "PENDING ITEMS", dataGridView1);

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

                                    //dataTable.Rows.Add(null, displayMenuName, null);
                                    dataTable.Rows.Add(null, string.Format("{0}x ", cartDetail.qty) + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                                }

                                dataTable.Rows.Add(null, "Item Pending Qty :", datas.totalPendingQty);
                                dataTable.Rows.Add(null, "Item Pending Amount :", string.Format("{0:n0}", datas.totalCartPendingAmount));
                            }

                            if (cartDetailsCanceled.Count != 0)
                            {
                                AddSeparatorRow(dataTable, "CANCELED ITEMS", dataGridView1);

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

                                    //dataTable.Rows.Add(null, displayMenuName, null);
                                    dataTable.Rows.Add(null, string.Format("{0}x ", cartDetail.qty) + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                                }

                                dataTable.Rows.Add(null, "Item Cancel Qty :", datas.totalCanceledQty);
                                dataTable.Rows.Add(null, "Item Canceled Amount : ", string.Format("{0:n0}", datas.totalCartCanceledAmount));
                            }
                            if (refundDetails.Count != 0)
                            {
                                AddSeparatorRow(dataTable, "REFUNDED ITEMS", dataGridView1);

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

                                    //dataTable.Rows.Add(null, displayMenuName, null);
                                    dataTable.Rows.Add(null, string.Format("{0}x ", cartDetail.qty_refund_item) + displayMenuName, string.Format("{0:n0}", cartDetail.total_refund_price));
                                }

                                dataTable.Rows.Add(null, "Item Refund Qty :", datas.totalRefundQty);
                                dataTable.Rows.Add(null, "Item Refund Amount : ", string.Format("{0:n0}", datas.totalCartRefundAmount));
                            }
                            AddSeparatorRow(dataTable, "CASH MANAGEMENT", dataGridView1);

                            if (expenditures.Count != 0)
                            {
                                AddSeparatorRow(dataTable, "EXPENSE", dataGridView1);

                                foreach (var expense in expenditures)
                                {
                                    dataTable.Rows.Add(null, expense.description, string.Format("{0:n0}", expense.nominal));
                                }
                            }
                            dataTable.Rows.Add(null, "Expected Ending Cash", string.Format("{0:n0}", datas.ending_cash_expected));
                            dataTable.Rows.Add(null, "Actual Ending Cash", string.Format("{0:n0}", datas.ending_cash_actual));
                            dataTable.Rows.Add(null, "Cash Difference", string.Format("{0:n0}", datas.cash_difference));
                            AddSeparatorRow(dataTable, " ", dataGridView1);

                            dataTable.Rows.Add(null, "DISCOUNTS", "");
                            dataTable.Rows.Add(null, "All Discount items", string.Format("{0:n0}", datas.discount_amount_per_items));
                            dataTable.Rows.Add(null, "All Discount Cart", string.Format("{0:n0}", datas.discount_amount_transactions));
                            dataTable.Rows.Add(null, "TOTAL AMOUNT", string.Format("{0:n0}", datas.discount_total_amount));
                            AddSeparatorRow(dataTable, "PAYMENT DETAIL", dataGridView1);

                            foreach (var paymentDetail in paymentDetails)
                            {
                                AddSeparatorRow(dataTable, paymentDetail.payment_category, dataGridView1);
                                //dataTable.Rows.Add(null, paymentDetail.payment_category, "");
                                foreach (var paymentType in paymentDetail.payment_type_detail)
                                {
                                    dataTable.Rows.Add(null, paymentType.payment_type, string.Format("{0:n0}", paymentType.total_payment));
                                }
                                dataTable.Rows.Add(null, "TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount));
                                AddSeparatorRow(dataTable, " ", dataGridView1);

                            }
                            dataTable.Rows.Add(null, "TOTAL TRANSACTION", string.Format("{0:n0}", datas.total_transaction));

                            if (dataGridView1 == null)
                            {
                                ReloadDataGridView(dataTable);
                            }
                            else if (dataGridView1.Font == null)
                            {
                                // Mengatur font default jika Font null
                                dataGridView1.Font = new Font("Arial", 8.25f, FontStyle.Regular);
                                ReloadDataGridView(dataTable);
                            }
                            else
                            {
                                dataGridView1.DataSource = dataTable;

                                if (dataGridView1.Columns.Contains("DATA"))
                                {
                                    DataGridViewCellStyle boldStyle = new DataGridViewCellStyle
                                    {
                                        Font = new Font(dataGridView1.Font, FontStyle.Italic)
                                    };
                                    dataGridView1.Columns["DATA"].DefaultCellStyle = boldStyle;
                                }
                                else
                                {
                                    // Menangani situasi jika kolom "DATA" tidak ada
                                    Console.WriteLine("Kolom 'DATA' tidak ditemukan.");
                                }

                                if (dataGridView1.Columns.Contains("ID"))
                                {
                                    dataGridView1.Columns["ID"].Visible = false;
                                }
                                else
                                {
                                    // Menangani situasi jika kolom "ID" tidak ada
                                    Console.WriteLine("Kolom 'ID' tidak ditemukan.");
                                }
                            }

                            btnCetakStruk.Enabled = true;
                            bedaCash = datas.ending_cash_expected;
                            txtActualCash.Text = datas.ending_cash_expected.ToString();
                            txtNamaKasir.Text = "Dastrevas (AutoFill)";
                            bool isMoreThanOneHourAgo = IsStartShiftMoreThanOneHourAgo();
                            if (isMoreThanOneHourAgo)
                            {
                                btnCetakStruk.Enabled = true;
                            }
                            else
                            {
                                lblNotifikasi.Visible = true;
                                lblNotifikasi.Text = $"Tidak dapat akhiri laporan karena belum ada jarak 1 jam dari mulai shift.\nUntuk Cetak Ulang Laporan Shift [{datas.shift_number}] Tersedia.";
                                btnCetakStruk.Enabled = false;
                            }
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
        private void AddSeparatorRow(DataTable dataTable, string groupKey, DataGridView dataGridView)
        {
            // Tambahkan separator row ke DataTable
            dataTable.Rows.Add(null, groupKey , null); // Add a separator row

            // Ambil indeks baris terakhir yang baru saja ditambahkan
            int lastRowIndex = dataTable.Rows.Count - 1;

            // Menambahkan row ke DataGridView
            dataGridView.DataSource = dataTable;

            // Mengatur gaya sel untuk kolom tertentu
            int[] cellIndexesToStyle = { 1 , 2 }; // Indeks kolom yang ingin diatur
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
            ////LoggerUtil.LogPrivateMethod(nameof(btnPengeluaran_Click));

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

            using (notifikasiPengeluaran notifikasiPengeluaran = new notifikasiPengeluaran())
            {
                notifikasiPengeluaran.Owner = background;

                background.Show();

                DialogResult dialogResult = notifikasiPengeluaran.ShowDialog();

                background.Dispose();
            }
        }

        private void btnRiwayatShift_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnRiwayatShift_Click));

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
                using (printReportShift payForm = new printReportShift())
                {
                    payForm.Owner = background;

                    background.Show();

                    DialogResult dialogResult = payForm.ShowDialog();

                    background.Dispose();

                    /*if (printReportShift.KeluarButtonPrintReportShiftClicked)
                    {
                        LoadData(); 
                    }
    */
                    if (dialogResult == DialogResult.OK && payForm.ReloadDataInBaseForm)
                    {
                        LoadData();

                    }
                }
            });
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


                DialogResult yakin = MessageBox.Show($"Melakukan End Shift {shiftnumber.ToString()} pada waktu \n{mulaishift.ToString()} sampai {akhirshift.ToString()}\nNama Kasir : {txtNamaKasir.Text}\nActual Cash : Rp.{txtActualCash.Text},- \nCash Different : {string.Format("{0:n0}", Convert.ToInt32(fulus) - bedaCash)}?", "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (yakin != DialogResult.Yes)
                {
                    MessageBox.Show("Cetak Shift diCancel");
                    return;
                }
                else
                {
                    btnCetakStruk.Enabled = false;
                    btnCetakStruk.Text = "Waiting...";
                    ////LoggerUtil.LogPrivateMethod(nameof(btnCetakStruk_Click));

                    var casherName = string.IsNullOrEmpty(txtNamaKasir.Text) ? "" : txtNamaKasir.Text;
                    var actualCash = string.IsNullOrEmpty(fulus) ? "0" : fulus;

                    var json = new
                    {
                        outlet_id = baseOutlet,
                        casher_name = casherName,
                        actual_ending_cash = actualCash
                    };
                    string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);

                    IApiService api = new ApiService();

                    string response = await api.CetakLaporanShift(jsonString, "/struct-shift");

                    if (response != null)
                    {
                        await HandleSuccessfulPrint(response);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetButtonState();
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal cetak laporan " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
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
                        // Jika sudah mencapai batas retry, tampilkan pesan error
                        //MessageBox.Show("Error printing after multiple attempts: " + ex.Message);
                        LoggerUtil.LogError(ex, $"Error printing after {currentRetry}", ex);
                        util.sendLogTelegram($"An error printing occurred after {currentRetry} attempts {BaseOutletName}");
                    }
                    else
                    {
                        // Tunda sebelum mencoba ulang
                        //MessageBox.Show($"Error printing attempt {currentRetry}: {ex.Message}, retrying...");
                        await Task.Delay(delayBetweenRetries); // Menunggu sebelum mencoba lagi
                    }
                }
            }
        }


        private void ResetButtonState()
        {
            btnCetakStruk.Enabled = true;
            btnCetakStruk.Text = "Cetak Struk";
            btnCetakStruk.BackColor = Color.FromArgb(31, 30, 68);
        }
    }
}
