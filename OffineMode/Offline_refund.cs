
using FontAwesome.Sharp;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using KASIR.komponen;
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
using KASIR.Printer;
namespace KASIR.OfflineMode
{
    public partial class Offline_refund : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        public event EventHandler RefundSuccessful;
        private List<CartDetailTransaction> item = new List<CartDetailTransaction>();
        private List<RefundModel> refundItems = new List<RefundModel>();
        private readonly string MacAddressKasir;
        private readonly string PinPrinterKasir;
        private readonly string BaseOutletName;
        public bool ReloadDataInBaseForm { get; private set; }
        string idTransaksi, cartId, paymentodNgentod = "";

        private readonly string baseOutlet;
        GetTransactionDetail dataTransaction;
        public Offline_refund(string transaksiId)
        {
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
            InitializeComponent();
            btnRefund.Visible = false;

            idTransaksi = transaksiId;
            cmbPayform.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayform.DrawMode = DrawMode.OwnerDrawVariable;
            cmbPayform.DrawItem += CmbPayform_DrawItem;

            cmbPayform.ItemHeight = 25;


            LoadData();
            // InitializeComboBox();
            cmbRefundType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRefundType.DrawMode = DrawMode.OwnerDrawVariable;
            cmbRefundType.DrawItem += CmbRefundType_DrawItem;
            cmbRefundType.ItemHeight = 25;
            cmbRefundType.Items.Add("Semua");
            cmbRefundType.Items.Add("Per Item");
            cmbRefundType.SelectedIndex = 1;


        }

        private void CmbPayform_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();

                int verticalMargin = 5;
                string itemText = cmbPayform.GetItemText(cmbPayform.Items[e.Index]);

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black, new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width, e.Bounds.Height - verticalMargin));

                e.DrawFocusRectangle();
            }
        }
        private void CmbRefundType_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();

                int verticalMargin = 5;
                string itemText = cmbRefundType.GetItemText(cmbRefundType.Items[e.Index]);

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black, new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width, e.Bounds.Height - verticalMargin));

                e.DrawFocusRectangle();
            }
        }
        private async void LoadDataPaymentType()
        {
            if (File.Exists("DT-Cache" + "\\LoadDataPayment_" + "Outlet_" + baseOutlet + ".data"))
            {
                string json = File.ReadAllText("DT-Cache" + "\\LoadDataPayment_" + "Outlet_" + baseOutlet + ".data");
                PaymentTypeModel payment = JsonConvert.DeserializeObject<PaymentTypeModel>(json);
                List<PaymentType> data = payment.data;
                //data.Insert(0, new PaymentType { id = -1, name = "Pilih Tipe Pengembalian" });
                cmbPayform.DataSource = data;
                cmbPayform.DisplayMember = "name";
                cmbPayform.ValueMember = "id";

                for (int kimak = 0; kimak <= cmbPayform.Items.Count; kimak++)
                {
                    cmbPayform.SelectedIndex = kimak;
                    if (cmbPayform.Text.ToString() == paymentodNgentod)
                    {
                        cmbPayform.SelectedIndex = kimak;
                        break;
                    }
                }
                return;
            }
            try
            {

                IApiService apiService = new ApiService();
                string response = await apiService.GetPaymentType("/payment-type");
                PaymentTypeModel payment = JsonConvert.DeserializeObject<PaymentTypeModel>(response);
                List<PaymentType> data = payment.data;
                //data.Insert(0, new PaymentType { id = -1, name = "Pilih Tipe Pengembalian" });
                cmbPayform.DataSource = data;
                cmbPayform.DisplayMember = "name";
                cmbPayform.ValueMember = "id";

                for (int kimak = 0; kimak <= cmbPayform.Items.Count; kimak++)
                {
                    cmbPayform.SelectedIndex = kimak;
                    if (cmbPayform.Text.ToString() == paymentodNgentod)
                    {
                        cmbPayform.SelectedIndex = kimak;
                        break;
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
                MessageBox.Show("Gagal tampil data tipe serving " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

        }
        private async void LoadData()
        {
            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.GetActiveCart("/transaction/" + idTransaksi + "?outlet_id=" + baseOutlet);
                if (response != null)
                {
                    GetTransactionDetail transactionDetail = JsonConvert.DeserializeObject<GetTransactionDetail>(response);
                    DataTransaction data = transactionDetail.data;
                    dataTransaction = transactionDetail;
                    cartId = data.cart_id.ToString();
                    lblDetailPayment.Text = "Payment Sebelumnya : "+ data.payment_type.ToString();
                    lblCustomerName.Text = data.customer_name;
                    item = data.cart_details;
                    PopulateDynamicList();
                    paymentodNgentod = data.payment_type.ToString();
                    LoadDataPaymentType();


                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load keranjang " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            finally
            {
                btnRefund.Visible = true;
            }

        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void AddItem(string name, string amount)
        {


        }
        private void PopulateDynamicList()
        {
            panel13.Controls.Clear();
            int totalWidth = panel13.ClientSize.Width;

            foreach (var items in item)
            {
                Panel itemPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 60, // Increased height to accommodate the variant label
                };

                Label nameLabel = new Label
                {
                    Text = items.qty.ToString() + "X " + items.menu_name,
                    Width = (int)(totalWidth * 0.7),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                Label variantLabel = new Label
                {
                    Text = "     Variant: " + items.varian,
                    Width = (int)(totalWidth * 0.7),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Top = 30,
                };

                TableLayoutPanel quantityPanel = new TableLayoutPanel
                {
                    Width = (int)(totalWidth * 0.3),
                    Height = 30,
                    Dock = DockStyle.Right,
                    ColumnCount = 3,
                };

                Label quantityLabel = new Label
                {
                    Text = "0",
                    TextAlign = ContentAlignment.MiddleCenter,
                };

                Label minusButtonLabel = new Label
                {
                    Text = "-",
                    Width = 30,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    ForeColor = Color.Red,
                    Cursor = Cursors.Hand // Change cursor to indicate it's clickable
                };

                Label plusButtonLabel = new Label
                {
                    Text = "+",
                    Width = 30,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    ForeColor = Color.Green,
                    Cursor = Cursors.Hand // Change cursor to indicate it's clickable
                };

                minusButtonLabel.MouseClick += (sender, e) =>
                {
                    int currentQuantity = int.Parse(quantityLabel.Text);
                    int maxQuantity = int.Parse(items.qty.ToString());
                    if (currentQuantity > 0)
                    {
                        currentQuantity -= 1;
                        quantityLabel.Text = currentQuantity.ToString();

                        var existingRefundItem = refundItems.FirstOrDefault(r => r.CartDetailId == items.cart_detail_id);
                        if (existingRefundItem != null)
                        {
                            existingRefundItem.Qty -= 1;
                            if (existingRefundItem.Qty <= 0)
                            {
                                refundItems.Remove(existingRefundItem);
                            }
                        }
                    }
                };

                plusButtonLabel.MouseClick += (sender, e) =>
                {
                    int currentQuantity = int.Parse(quantityLabel.Text);
                    int maxQuantity = int.Parse(items.qty.ToString());
                    if (currentQuantity + 1 > maxQuantity)
                    {
                        MessageBox.Show("Kuantitas maksimal refund telah tercapai", "Gaspol", MessageBoxButtons.OK);
                        return;
                    }
                    if (currentQuantity < maxQuantity)
                    {
                        currentQuantity += 1;
                        quantityLabel.Text = currentQuantity.ToString();

                        var existingRefundItem = refundItems.FirstOrDefault(r => r.CartDetailId == items.cart_detail_id);
                        if (existingRefundItem != null)
                        {
                            existingRefundItem.Qty += 1;
                        }
                        else
                        {
                            refundItems.Add(new RefundModel
                            {
                                CartDetailId = items.cart_detail_id,
                                Qty = 1,
                                RefundReason = ""
                            });
                        }
                    }
                };

                quantityPanel.Controls.Add(minusButtonLabel);
                quantityPanel.Controls.Add(quantityLabel);
                quantityPanel.Controls.Add(plusButtonLabel);

                itemPanel.Controls.Add(nameLabel);
                itemPanel.Controls.Add(variantLabel);
                itemPanel.Controls.Add(quantityPanel);

                panel13.Controls.Add(itemPanel);
            }
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel13_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ////LoggerUtil.LogPrivateMethod(nameof(button2_Click));
                string selectedRefundType = cmbRefundType.SelectedItem.ToString();
                if (txtNotes.Text == "")
                {
                    MessageBox.Show("Masukkan alasan", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (cmbRefundType.Text.ToString() == "Pilih Tipe Refund")
                {
                    MessageBox.Show("Pilih tipe refund", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (cmbPayform.Text.ToString() == "Pilih Tipe Pengembalian")
                {
                    MessageBox.Show("Pilih tipe pengembalian", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (refundItems.Count == 0 && selectedRefundType != "Semua")
                {
                    MessageBox.Show("Kuantitas item yang ingin di refund masih nol", "Refund Items", MessageBoxButtons.OK);
                    return;
                }
                if (cmbRefundType.SelectedItem != null)
                {
                    btnRefund.Text = "Mencetak...";
                    btnRefund.BackColor = Color.Gainsboro;
                    btnRefund.Enabled = false;
                    bool isRefundAll = selectedRefundType == "Semua" ? true : false;
                    Dictionary<string, object> json = new Dictionary<string, object>
                {
                    { "transaction_id", dataTransaction.data.transaction_id },
                    { "cart_id", dataTransaction.data.cart_id },
                    { "is_refund_all", isRefundAll },// Set is_refund_all based on selectedRefundType
                    { "payment_type_id", cmbPayform.SelectedValue.ToString() },
                    { "refund_reason", ""+txtNotes.Text }
                };
                    if (selectedRefundType == "Per Item")
                    {
                        List<Dictionary<string, object>> refundDetails = new List<Dictionary<string, object>>();
                        foreach (var refundItem in refundItems)
                        {
                            Dictionary<string, object> refundItemDict = new Dictionary<string, object>
                        {
                            { "cart_detail_id", refundItem.CartDetailId },
                            { "qty_refund", refundItem.Qty },
                        };
                            refundDetails.Add(refundItemDict);
                        }
                        json["cart_details"] = refundDetails;
                    }
                    else
                    {
                        json["refund_reason"] = txtNotes.Text;
                    }
                    string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                    IApiService apiService = new ApiService();
                    string response = await apiService.Refund(jsonString, "/refund");
                    if (response != null)
                    {
                        string CountingTransaksi = await apiService.Get("/transaction?outlet_id=" + baseOutlet + "&is_success=true");

                        GetMenuModel Counter = JsonConvert.DeserializeObject<GetMenuModel>(CountingTransaksi);
                        List<KASIR.Model.Menu> menuList = Counter.data.ToList();
                        int NomorTransaksi = 0;
                        if (menuList != null && menuList.Count > 0)
                        {
                            foreach (KASIR.Model.Menu menu in menuList.AsEnumerable().Reverse())
                            {
                                NomorTransaksi++;
                                if (menu.id == int.Parse(idTransaksi)) break;
                            }
                            if (NomorTransaksi > menuList.Count)
                            {
                                NomorTransaksi++;
                            }
                        }

                        await HandleSuccessfulTransaction(response, NomorTransaksi);
                    }
                    else
                    {
                        MessageBox.Show("Gagal cetak ulang struk", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetButtonState();
                    }
                }
                else
                {
                    MessageBox.Show("Pilih tipe refund terlebih dahulu");
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
                MessageBox.Show("Gagal cetak ulang struk " + ex.Message, "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
        }

        private async Task HandleSuccessfulTransaction(string response, int NomorTransaksi)
        {
            try
            {
                PrinterModel printerModel = new PrinterModel();
                RefundStrukModel menuModel = JsonConvert.DeserializeObject<RefundStrukModel>(response);
                DataRefundStruk data = menuModel.Data;

                if (data != null)
                {
                    List<CartDetailRefundStruk> listCart = data.cart_details;
                    List<RefundDetailStruk> refundDetailStruks = data.refund_details;

                    await Task.Run(() =>
                    {
                        printerModel.PrintModelRefund(data, refundDetailStruks, NomorTransaksi);
                    });
                }

                btnRefund.Text = "Selesai.";
                btnRefund.Enabled = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal cetak ulang struk " + ex.Message, "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
        }

        private void ResetButtonState()
        {
            btnRefund.Text = "Cetak Struk";
            btnRefund.Enabled = true;
            btnRefund.BackColor = Color.DarkRed;
        }


        private async void PrintPurchaseReceipt(DataRefundStruk datas, List<RefundDetailStruk> refundDetailStruks)
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

                    strukText += kodeHeksadesimalSizeBesar + CenterText("Refund") + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += "--------------------------------\n";
                    strukText += CenterText(datas.receipt_number) + "\n";
                    strukText += FormatSimpleLine(datas.customer_name, nomorMeja) + "\n";

                    // Iterate through cart details and group by serving_type_name
                    var servingTypes = refundDetailStruks.Select(cd => cd.serving_type_name).Distinct();

                    foreach (var servingType in servingTypes)
                    {
                        // Filter cart details by serving_type_name
                        var itemsForServingType = refundDetailStruks.Where(cd => cd.serving_type_name == servingType).ToList();

                        // Skip if there are no items for this serving type
                        if (itemsForServingType.Count == 0)
                            continue;

                        // Add a section for the serving type
                        strukText += "--------------------------------\n";
                        strukText += CenterText(servingType) + "\n";
                        strukText += "--------------------------------\n";

                        // Iterate through items for this serving type
                        foreach (var refundDetail in itemsForServingType)
                        {
                            strukText += FormatItemLine(refundDetail.menu_name, refundDetail.qty_refund_item, string.Format("{0:n0}", refundDetail.menu_price)) + "\n";
                            // Add detail items
                            if (!string.IsNullOrEmpty(refundDetail.varian))
                                strukText += FormatDetailItemLine("Varian", refundDetail.varian) + "\n";
                            if (!string.IsNullOrEmpty(refundDetail.note_item?.ToString()))
                                strukText += FormatDetailItemLine("Note", refundDetail.note_item) + "\n";
                            if (!string.IsNullOrEmpty(refundDetail.discount_code))
                                strukText += FormatDetailItemLine("Discount Code", refundDetail.discount_code) + "\n";
                            if (refundDetail.discounted_price.HasValue && refundDetail.discounted_price != 0)
                                strukText += FormatDetailItemLine("Total Discount", string.Format("{0:n0}", refundDetail.discounted_price)) + "\n";
                            strukText += FormatDetailItemLine("Payment Type Refund", refundDetail.payment_type_name) + "\n";
                            strukText += FormatDetailItemLine("Total Refund", string.Format("{0:n0}", refundDetail.total_refund_price)) + "\n";
                            strukText += FormatDetailItemLine("Refund Reason", refundDetail.refund_reason_item) + "\n";

                            // Add an empty line between items
                            strukText += "\n";
                        }
                    }
                    strukText += "--------------------------------\n";
                    strukText += FormatSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal)) + "\n";
                    if (!string.IsNullOrEmpty(datas.discount_code))
                        strukText += FormatSimpleLine("Discount Code", datas.discount_code) + "\n";
                    if (datas.discounts_value.HasValue && datas.discounts_value != 0)
                        strukText += FormatSimpleLine("Discount Value", datas.discounts_value) + "\n";
                    strukText += FormatSimpleLine("Total", datas.total) + "\n";
                    strukText += FormatSimpleLine("Payment Type", datas.payment_type) + "\n";
                    if (!string.IsNullOrEmpty(datas.refund_reason))
                        strukText += FormatSimpleLine("Refund Reason ", datas.refund_reason) + "\n";
                    strukText += FormatSimpleLine("Total Refund ", string.Format("{0:n0}", datas.total_refund)) + "\n";
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
                Console.WriteLine("Error printing: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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
        private void btnCicil_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnCicil_Click));

            cicilRefund cicilRefund = new cicilRefund(cartId);
            cicilRefund.Show();
        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cmbRefundType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRefundType = cmbRefundType.SelectedItem.ToString();
            if(selectedRefundType == "Semua")
            {
                panel13.Enabled = false;
            }
            else
            {
                panel13.Enabled = true;
            }
        }
    }
}