﻿

using FontAwesome.Sharp;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Windows.Forms.VisualStyles;
using System.Windows.Controls;
using System.Net.NetworkInformation;
using System.Xaml;
using InTheHand.Net.Bluetooth.AttributeIds;
using KASIR.Komponen;
using System.Globalization;
using System.Windows.Forms.Design;
using System.Net.Sockets;
using System.Net.Sockets;
using System.Text;
using System.Net.Mail;
using KASIR.Printer;
using System.Windows.Markup;
namespace KASIR.komponen
{
    public partial class payForm : Form
    {
        private masterPos _masterPos;
        private masterPos MasterPosForm { get; set; }
        private List<System.Windows.Forms.Button> radioButtonsList = new List<System.Windows.Forms.Button>();
        public string btnPayType;
        string outletID, cartID, totalCart, ttl2;
        private readonly string baseOutlet;
        private readonly string MacAddressKasir;
        private readonly string MacAddressKitchen;
        private readonly string MacAddressBar;
        private readonly string PinPrinterKasir;
        private readonly string PinPrinterKitchen;
        private readonly string PinPrinterBar;
        private readonly string BaseOutletName;
        private string Kakimu;
        private PrinterModel printerModel; // Pastikan ini telah diinisialisasi dengan benar

        private readonly ILogger _log = LoggerService.Instance._log;
        public bool KeluarButtonClicked { get; private set; }

        public bool ReloadDataInBaseForm { get; private set; }
        private DataTable originalDataTable, listDataTable;
        int items = 0;
        int customePrice = 0;
        int SelectedId, totalTransactions;
        string namaMember, emailMember, hpMember;

        public payForm(string outlet_id, string cart_id, string total_cart, string ttl1, string seat, string name, masterPos masterPosForm)
        {
            InitializeComponent();
            //panel4.Visible = false;
            btnSimpan.Enabled = false;
            _masterPos = masterPosForm;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            MacAddressKitchen = Properties.Settings.Default.MacAddressKitchen;
            MacAddressBar = Properties.Settings.Default.MacAddressBar;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            PinPrinterKitchen = Properties.Settings.Default.PinPrinterKitchen;
            PinPrinterBar = Properties.Settings.Default.PinPrinterBar;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
            //Kakimu = Properties.Settings.Default.FooterStruk;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = true;
            ttl2 = ttl1;
            outletID = outlet_id;
            cartID = cart_id;
            totalCart = total_cart;
            txtJumlahPembayaran.Text = ttl2;
            txtSeat.Text = seat;
            txtNama.Text = name;
            generateRandomFill();
            string cleanedTtl1 = Regex.Replace(ttl1, "[^0-9]", "");
            loadFooterStruct();
            loadCountingStruct();

            customePrice = Int32.Parse(cleanedTtl1);

            txtJumlahPembayaran.Text = ttl1;
            //int customePrice = Int32.Parse(ttl1.Replace("Rp.", "").Replace(",", "").Replace(",-", "").Replace(" ", ""));

            btnSetPrice1.Text = ttl1;
            if (customePrice < 10000)
            {
                btnSetPrice2.Text = "Rp. 10,000,-";
                btnSetPrice3.Text = "Rp. 20,000,-";
            }
            else if (customePrice < 20000)
            {
                btnSetPrice2.Text = "Rp. 20,000,-";
                btnSetPrice3.Text = "Rp. 50,000,-";
            }
            else if (customePrice < 50000)
            {
                btnSetPrice2.Text = "Rp. 50,000,-";
                btnSetPrice3.Text = "Rp. 100,000,-";
            }
            else if (customePrice < 100000)
            {
                btnSetPrice2.Text = "Rp. 100,000,-";
                btnSetPrice3.Text = "Rp. 150,000,-";
            }
            else if (customePrice < 500000)
            {
                btnSetPrice2.Text = "Rp. 500,000,-";
                btnSetPrice3.Text = "Rp. 1,000,000,-";
            }
            else if (customePrice < 1000000)
            {
                btnSetPrice2.Text = "Rp. 1,000,000,-";
                btnSetPrice3.Text = "Rp. 1,500,000,-";
            }


            foreach (var button in radioButtonsList)
            {
                button.Click += RadioButton_Click;
            }
            cmbPayform.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayform.DrawMode = DrawMode.OwnerDrawVariable;
            cmbPayform.DrawItem += CmbPayform_DrawItem;

            cmbPayform.ItemHeight = 25;
            //default
            //cmbPayform.SelectedItem = 1;
            /* int newHeight = Screen.PrimaryScreen.WorkingArea.Height - 100;
             Height = newHeight;*/
            LoadDataPaymentType();

            //auto keisi payment minimum
            txtCash.Text = Regex.Replace(totalCart, "[^0-9]", "");

            panel8.Visible = false;
            panel13.Visible = false;
            panel14.Visible = false;
            btnDataMember.Visible = false;
            lblPoint.Visible = false;
        }
        private async void loadFooterStruct()
        {
                Kakimu = await File.ReadAllTextAsync("setting\\FooterStruk.data");
        }
        private async void loadCountingStruct()
        {
            try
            {
                apiService = new ApiService();
                string response = await apiService.Get("/transaction?outlet_id=" + baseOutlet + "&is_success=true");

                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                List<KASIR.Model.Menu> menuList = menuModel.data.ToList();
                totalTransactions = menuList.Count + 1;
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void generateRandomFill()
        {
            Random random = new Random();

            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
            string[] vowels = { "a", "e", "i", "o", "u" };

            string name = "";

            int nameLength = random.Next(3, 10);
            if (txtNama.Text == "" | txtNama.Text == null)
            {
                for (int i = 0; i < nameLength; i++)
                {
                    name += i % 2 == 0 ? consonants[random.Next(consonants.Length)] : vowels[random.Next(vowels.Length)];
                }

                name = char.ToUpper(name[0]) + name.Substring(1);

                txtNama.Text = name + " (DT-AutoFIll)";
                txtSeat.Text = "0";
            }

        }

        private async void LoadDataPaymentType()
        {
            try
            {
                if (File.Exists("DT-Cache" + "\\LoadDataPayment_" + "Outlet_" + baseOutlet + ".data"))
                {
                    string json = File.ReadAllText("DT-Cache" + "\\LoadDataPayment_" + "Outlet_" + baseOutlet + ".data");
                    PaymentTypeModel payment = JsonConvert.DeserializeObject<PaymentTypeModel>(json);
                    List<PaymentType> data = payment.data;
                    //data.Insert(0, new PaymentType { id = -1, name = "Pilih Tipe Pembayaran" });
                    //default
                    cmbPayform.DataSource = data;
                    cmbPayform.DisplayMember = "name";
                    cmbPayform.ValueMember = "id";
                }
                else
                {
                    IApiService apiService = new ApiService();
                    string response = await apiService.GetPaymentType("/payment-type");
                    PaymentTypeModel payment = JsonConvert.DeserializeObject<PaymentTypeModel>(response);
                    List<PaymentType> data = payment.data;
                    //data.Insert(0, new PaymentType { id = -1, name = "Pilih Tipe Pembayaran" });
                    //default
                    cmbPayform.DataSource = data;
                    cmbPayform.DisplayMember = "name";
                    cmbPayform.ValueMember = "id";
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
            finally
            {
                await WaitForSecondsAsync(1);
                btnSimpan.Enabled = true;
            }

        }
        public static async Task WaitForSecondsAsync(int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
        }
        private void CmbPayform_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                try
                {
                    e.DrawBackground();

                    int verticalMargin = 5;
                    string itemText = cmbPayform.GetItemText(cmbPayform.Items[e.Index]);

                    e.Graphics.DrawString(itemText, e.Font, Brushes.Black, new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width, e.Bounds.Height - verticalMargin));

                    e.DrawFocusRectangle();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("gagal load payform: " + ex.Message);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
        }
        private async void btnSimpan_Click(object sender, EventArgs e)
        {
            btnSimpan.Enabled = false;
            btnSimpan.Text = "Waiting...";
            btnSimpan.BackColor = Color.Gainsboro;

            try
            {
                if (btnSimpan.Text == "Selesai.")
                {
                    btnSimpan.Enabled = true;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    string fulus = Regex.Replace(txtCash.Text, "[^0-9]", "");
                    if (string.IsNullOrEmpty(txtNama.Text))
                    {
                        MessageBox.Show("Masukan nama pelanggan", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }
                    if (string.IsNullOrEmpty(txtSeat.Text))
                    {
                        MessageBox.Show("Masukan seat pelanggan", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    int fulusAmount;
                    if (!int.TryParse(fulus, out fulusAmount))
                    {
                        MessageBox.Show("Invalid amount entered.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    int totalCartAmount;
                    if (!int.TryParse(totalCart, out totalCartAmount))
                    {
                        MessageBox.Show("Invalid total cart value.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    if (fulusAmount < totalCartAmount)
                    {
                        MessageBox.Show("Uang anda belum cukup.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    if (cmbPayform.Text == "Pilih Tipe Pembayaran")
                    {
                        MessageBox.Show("Pilih tipe pembayaran", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    int change = fulusAmount - totalCartAmount;

                    var json = new
                    {
                        outlet_id = baseOutlet,
                        cart_id = int.Parse(cartID),
                        customer_name = txtNama.Text,
                        customer_seat = txtSeat.Text,
                        customer_cash = fulusAmount,
                        payment_type_id = cmbPayform.SelectedValue.ToString(),
                        member_id = SelectedId
                    };
                    string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                    IApiService apiService = new ApiService();
                    string response = await apiService.PayBillTransaction(jsonString, "/transaction");

                    if (response != null)
                    {
                        HandleSuccessfulTransaction(response, fulus);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetButtonState();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show("Terjadi kesalahan, silakan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetButtonState();
            }
        }

        private async void Ex_btnSimpan_Click(object sender, EventArgs e)
        {
            btnSimpan.Enabled = false;
            ////LoggerUtil.LogPrivateMethod(nameof(btnSimpan_Click));
            btnSimpan.Text = "Waiting...";
            btnSimpan.BackColor = Color.Gainsboro;

            try
            {
                if (btnSimpan.Text == "Selesai.")
                {
                    btnSimpan.Enabled = true;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    string fulus = Regex.Replace(txtCash.Text, "[^0-9]", "");

                    if (string.IsNullOrEmpty(txtNama.Text))
                    {
                        MessageBox.Show("Masukan nama pelanggan", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }
                    if (string.IsNullOrEmpty(txtSeat.Text))
                    {
                        MessageBox.Show("Masukan seat pelanggan", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }
                    if (int.Parse(fulus) < int.Parse(totalCart))
                    {
                        MessageBox.Show("Uang anda belum cukup ", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }
                    if (cmbPayform.Text == "Pilih Tipe Pembayaran")
                    {
                        MessageBox.Show("Pilih tipe pembayaran", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    int change = int.Parse(fulus) - int.Parse(totalCart);

                    var json = new
                    {
                        outlet_id = baseOutlet,
                        cart_id = int.Parse(cartID),
                        customer_name = txtNama.Text,
                        customer_seat = txtSeat.Text,
                        customer_cash = int.Parse(fulus),
                        payment_type_id = cmbPayform.SelectedValue.ToString(),
                        member_id = SelectedId
                    };
                    string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                    IApiService apiService = new ApiService();
                    string response = await apiService.PayBillTransaction(jsonString, "/transaction");

                    if (response != null)
                    {
                        HandleSuccessfulTransaction(response, fulus);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetButtonState();
                    }
                }
            }
            catch (Exception ex)
            {
                //karena keadaan kecetak jadi gaperlu notif ganggu ini
                //MessageBox.Show("Gagal bayar menu " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show("Terjadi kesalahan, silakan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetButtonState();
            }
        }
        private async Task HandleSuccessfulTransaction(string response, string fulus)
        {
            try
            {
                PrinterModel printerModel = new PrinterModel();
                GetStrukCustomerTransaction menuModel = JsonConvert.DeserializeObject<GetStrukCustomerTransaction>(response);

                if (menuModel != null && menuModel.data != null)
                {
                    DataStrukCustomerTransaction data = menuModel.data;
                    List<CartDetailStrukCustomerTransaction> listCart = data.cart_details;
                    List<KitchenAndBarCartDetails> kitchenBarCart = data.kitchenBarCartDetails;
                    List<KitchenAndBarCanceledItems> kitchenBarCanceled = data.kitchenBarCanceledItems;

                    List<CartDetailStrukCustomerTransaction> cartDetails = data.cart_details;
                    List<KitchenAndBarCartDetails> kitchenItems = kitchenBarCart.Where(cd => cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList();
                    List<KitchenAndBarCartDetails> barItems = kitchenBarCart.Where(cd => cd.menu_type == "Minuman" || cd.menu_type == "Additional Minuman").ToList();
                    List<KitchenAndBarCanceledItems> canceledKitchenItems = kitchenBarCanceled.Where(cd => cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList();
                    List<KitchenAndBarCanceledItems> canceledBarItems = kitchenBarCanceled.Where(cd => cd.menu_type == "Minuman" || cd.menu_type == "Additional Minuman").ToList();

                    if (btnSimpan != null)
                    {
                        btnSimpan.Text = "Mencetak...";
                    }
                    else
                    {
                        throw new InvalidOperationException("btnSimpan is null");
                    }
                    if (printerModel != null)
                    {
                        // Run the print method in a background task
                        await Task.Run(() =>
                        {
                            printerModel.PrintModelPayform(menuModel, cartDetails, kitchenItems, barItems, canceledKitchenItems, canceledBarItems, totalTransactions, Kakimu);
                        });
                        //await printerModel.PrintModelPayform(menuModel, cartDetails, kitchenItems, barItems, canceledKitchenItems, canceledBarItems, totalTransactions, Kakimu);
                    }
                    else
                    {
                        throw new InvalidOperationException("printerModel is null");
                    }
                }
                btnSimpan.Text = "Selesai.";
                btnSimpan.Enabled = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        /*
        private async void HandleSuccessfulTransaction(string response, string fulus)
        {
            try
            {
                GetStrukCustomerTransaction menuModel = JsonConvert.DeserializeObject<GetStrukCustomerTransaction>(response);

                if (menuModel != null && menuModel.data != null)
                {
                    DataStrukCustomerTransaction data = menuModel.data;
                    List<CartDetailStrukCustomerTransaction> listCart = data.cart_details;
                    List<KitchenAndBarCartDetails> kitchenBarCart = data.kitchenBarCartDetails;
                    List<KitchenAndBarCanceledItems> kitchenBarCanceled = data.kitchenBarCanceledItems;

                    List<CartDetailStrukCustomerTransaction> cartDetails = data.cart_details;
                    List<KitchenAndBarCartDetails> kitchenItems = kitchenBarCart.Where(cd => cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList();
                    List<KitchenAndBarCartDetails> barItems = kitchenBarCart.Where(cd => cd.menu_type == "Minuman" || cd.menu_type == "Additional Minuman").ToList();
                    List<KitchenAndBarCanceledItems> canceledKitchenItems = kitchenBarCanceled.Where(cd => cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList();
                    List<KitchenAndBarCanceledItems> canceledBarItems = kitchenBarCanceled.Where(cd => cd.menu_type == "Minuman" || cd.menu_type == "Additional Minuman").ToList();

                    btnSimpan.Text = "Mencetak...";
                    var tasks = new List<Task>();

                    if (MacAddressKitchen.Length > 10 && (kitchenItems.Any() || canceledKitchenItems.Any()))
                    {
                        tasks.Add(PrintKitchenAndBarReceiptsAsync(menuModel, kitchenItems, "Kitchen", MacAddressKitchen, PinPrinterKitchen, canceledKitchenItems));

                        //PrintKitchenAndBarReceipts(menuModel, kitchenItems, "Kitchen", MacAddressKitchen, PinPrinterKitchen, canceledKitchenItems);
                    }

                    if (MacAddressBar.Length > 10 && (barItems.Any() || canceledBarItems.Any()))
                    {
                        tasks.Add(PrintKitchenAndBarReceiptsAsync(menuModel, barItems, "Bar", MacAddressBar, PinPrinterBar, canceledBarItems));

                        //PrintKitchenAndBarReceipts(menuModel, barItems, "Bar", MacAddressBar, PinPrinterBar, canceledBarItems);
                    }
                    tasks.Add(PrintPurchaseReceiptAsync(menuModel, cartDetails));


                    // Run all print tasks in parallel
                    await Task.WhenAll(tasks);
                    //PrintPurchaseReceipt(menuModel, cartDetails);
                }
                btnSimpan.Text = "Selesai.";
                btnSimpan.Enabled = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        */
        private void ResetButtonState()
        {
            btnSimpan.Enabled = true;
            btnSimpan.Text = "Simpan";
            btnSimpan.BackColor = Color.FromArgb(31, 30, 68);
        }


        private void RadioButton_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(RadioButton_Click));

            var clickedButton = (System.Windows.Forms.Button)sender;

            foreach (var button in radioButtonsList)
            {

                button.BackColor = SystemColors.ControlDark;
            }

            clickedButton.ForeColor = Color.White;
            clickedButton.BackColor = Color.SteelBlue;

            btnPayType = clickedButton.Tag.ToString();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            //KeluarButtonClicked = true;
            DialogResult = DialogResult.OK;

            this.Close();
        }
        private ApiService apiService;

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

        private async Task RetryPolicyAsync(Func<Task> printTask, int maxAttempts, int retryDelayMilliseconds)
        {
            int currentAttempt = 1;
            Exception lastException = null;

            while (currentAttempt <= maxAttempts)
            {
                try
                {
                    // Menjalankan tugas cetak yang diterima dari parameter printTask
                    await printTask();
                    return; // Keluar dari loop jika cetak berhasil
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, $"Printing failed on attempt {currentAttempt}.");
                    lastException = ex;

                    if (currentAttempt >= maxAttempts)
                    {
                        Util u = new Util();
                        // Semua percobaan gagal, tampilkan pesan error
                        MessageBox.Show("Gagal mencetak setelah beberapa kali percobaan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        u.sendLogTelegram("Gagal Mencetak, Log Outlet : " + BaseOutletName);
                        return;
                    }

                    // Menunggu sebelum mencoba lagi
                    await Task.Delay(retryDelayMilliseconds);
                }

                currentAttempt++;
            }

            // Jika sampai di sini, maka seluruh percobaan gagal, lempar pengecualian terakhir
            if (lastException != null)
            {
                throw lastException;
            }
        }

        // Struct Pembayaran
        private async Task PrintPurchaseReceiptAsync(GetStrukCustomerTransaction datas, List<CartDetailStrukCustomerTransaction> cartDetails)
        {
            await RetryPolicyAsync(async () =>
            {
                try
                {
                    // Panggil metode ConnectToBluetoothPrinter dengan timeout 10 detik
                    BluetoothClient clientSocket = await ConnectToBluetoothPrinter(MacAddressKasir, PinPrinterKasir, 10);
                    System.IO.Stream stream = clientSocket.GetStream();

                    // Custom variable
                    string kodeHeksadesimalBold = "\x1B\x45\x01";
                    string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                    string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                    string kodeHeksadesimalSpasiKarakter = "\x1B\x20\x02";
                    string nomorMeja = "Meja No." + datas.data.customer_seat;

                    // Struct template pembelian
                    string strukText = kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText(totalTransactions.ToString()) + "\n";
                    strukText += "\n" + kodeHeksadesimalBold + CenterText(datas.data.outlet_name) + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += CenterText(datas.data.outlet_address) + "\n";
                    strukText += CenterText(datas.data.outlet_phone_number) + "\n";
                    strukText += "--------------------------------\n";

                    strukText += kodeHeksadesimalSizeBesar + CenterText("Pembelian") + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += "--------------------------------\n";
                    strukText += CenterText(datas.data.receipt_number) + "\n";
                    strukText += CenterText(datas.data.invoice_due_date) + "\n \n";
                    strukText += FormatSimpleLine(datas.data.customer_name, nomorMeja) + "\n";

                    // Membership area
                    if (datas.data.member_name != null)
                    {
                        strukText += "--------------------------------\n";
                        strukText += FormatSimpleLine("MEMBER : ", datas.data.member_name) + "\n";
                        strukText += FormatSimpleLine("No. HP : ", datas.data.member_phone_number) + "\n";
                        strukText += "--------------------------------\n";
                    }

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
                            strukText += FormatItemLine(cartDetail.menu_name, cartDetail.qty, cartDetail.price) + "\n";
                            // Add detail items
                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                strukText += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";
                            if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                strukText += FormatDetailItemLine("Discount Code", cartDetail.discount_code) + "\n";
                            if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                                strukText += FormatDetailItemLine("Total Discount", string.Format("Rp. {0:n0},-", cartDetail.discounted_price)) + "\n";
                            strukText += FormatDetailItemLine("Total Price", string.Format("Rp. {0:n0},-", cartDetail.total_price)) + "\n";

                            // Add an empty line between items
                            strukText += "\n";
                        }
                    }
                    strukText += "--------------------------------\n";
                    strukText += FormatSimpleLine("Subtotal", string.Format("Rp. {0:n0},-", datas.data.subtotal)) + "\n";
                    if (!string.IsNullOrEmpty(datas.data.discount_code))
                        strukText += FormatSimpleLine("Discount Code", datas.data.discount_code) + "\n";
                    if (datas.data.discounts_value.HasValue && datas.data.discounts_value != 0)
                        strukText += FormatSimpleLine("Discount Value", datas.data.discounts_value) + "\n";
                    strukText += FormatSimpleLine("Total", string.Format("Rp. {0:n0},-", datas.data.total)) + "\n";
                    strukText += FormatSimpleLine("Payment Type", datas.data.payment_type) + "\n";
                    strukText += FormatSimpleLine("Cash", string.Format("Rp. {0:n0},-", datas.data.customer_cash)) + "\n";
                    strukText += FormatSimpleLine("Change", string.Format("Rp. {0:n0},-", datas.data.customer_change)) + "\n";
                    strukText += "--------------------------------\n";
                    strukText += CenterText(Kakimu) + "\n";
                    strukText += CenterText(datas.data.outlet_footer) + "\n\n\n\n\n\n\n\n\n\n\n";

                    string strukTextChecker = kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText(totalTransactions.ToString()) + "\n";
                    strukTextChecker += "\n" + kodeHeksadesimalBold + CenterText(datas.data.outlet_name) + "\n";
                    strukTextChecker += kodeHeksadesimalNormal;
                    strukTextChecker += CenterText(datas.data.outlet_address) + "\n";
                    strukTextChecker += CenterText(datas.data.outlet_phone_number) + "\n";
                    strukTextChecker += "--------------------------------\n";

                    strukTextChecker += kodeHeksadesimalSizeBesar + CenterText("Checker") + "\n";
                    strukTextChecker += kodeHeksadesimalNormal;
                    strukTextChecker += "--------------------------------\n";
                    strukTextChecker += CenterText(datas.data.receipt_number) + "\n";
                    strukTextChecker += CenterText(datas.data.invoice_due_date) + "\n \n";
                    strukTextChecker += kodeHeksadesimalSizeBesar + FormatSimpleLine(datas.data.customer_name, nomorMeja) + kodeHeksadesimalNormal + "\n";

                    // Iterate through cart details and group by serving_type_name
                    var checkerServingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();

                    foreach (var checkerServingType in checkerServingTypes)
                    {
                        // Filter cart details by serving_type_name
                        var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == checkerServingType).ToList();

                        // Skip if there are no items for this serving type
                        if (itemsForServingType.Count == 0)
                            continue;

                        // Add a section for the serving type
                        strukTextChecker += "--------------------------------\n";
                        strukTextChecker += CenterText(checkerServingType) + "\n";
                        strukTextChecker += "--------------------------------\n";

                        // Iterate through items for this serving type
                        foreach (var cartDetail in itemsForServingType)
                        {
                            strukTextChecker += kodeHeksadesimalBold + FormatSimpleLine(cartDetail.menu_name, cartDetail.qty) + kodeHeksadesimalNormal + "\n";
                            // Add detail items
                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                strukTextChecker += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                strukTextChecker += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";

                            // Add an empty line between itemsprint
                            strukTextChecker += "\n";
                        }
                    }
                    strukTextChecker += "--------------------------------\n";
                    strukTextChecker += CenterText(datas.data.outlet_footer) + "\n\n\n\n\n\n";


                    // Encode your text into bytes (you might need to adjust the encoding)
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);
                    byte[] bufferChecker = System.Text.Encoding.UTF8.GetBytes(strukTextChecker);

                    // Create a combined byte array
                    byte[] combinedBuffer = new byte[buffer.Length + bufferChecker.Length];
                    Buffer.BlockCopy(buffer, 0, combinedBuffer, 0, buffer.Length);
                    Buffer.BlockCopy(bufferChecker, 0, combinedBuffer, buffer.Length, bufferChecker.Length);

                    // Send the combined text to the printer
                    stream.Write(combinedBuffer, 0, combinedBuffer.Length);

                    // Flush the stream to ensure all data is sent to the printer
                    stream.Flush();

                    // Close the stream and disconnect
                    clientSocket.GetStream().Close();
                    stream.Close();
                    clientSocket.Close();
                }
                
                catch (TaskCanceledException ex)
                {
                    throw new Exception("Pairing with the printer timed out.", ex);
                }
                catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                throw; // Lempar ulang exception untuk di-handle di RetryPolicyAsync
                }
            }, maxAttempts: 3, retryDelayMilliseconds: 2000);
        }



        private async void PrintPurchaseReceipt(GetStrukCustomerTransaction datas, List<CartDetailStrukCustomerTransaction> cartDetails)
        {
            try
            {
                BluetoothDeviceInfo printer = new BluetoothDeviceInfo(BluetoothAddress.Parse(MacAddressKasir));
                if (printer == null)
                {
                    //MessageBox.Show("Printer" + MacAddressKasir + "not found.", "Gaspol");
                    return;
                }

                BluetoothClient client = new BluetoothClient();
                BluetoothEndPoint endpoint = new BluetoothEndPoint(printer.DeviceAddress, BluetoothService.SerialPort);

                using (BluetoothClient clientSocket = new BluetoothClient())
                {
                    if (!BluetoothSecurity.PairRequest(printer.DeviceAddress, PinPrinterKasir))
                    {
                        //MessageBox.Show("Pairing failed to " + MacAddressKasir, "Gaspol");
                        return;
                    }

                    clientSocket.Connect(endpoint);
                    System.IO.Stream stream = clientSocket.GetStream();

                    // Custom variable
                    string kodeHeksadesimalBold = "\x1B\x45\x01";
                    string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                    string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                    string kodeHeksadesimalSpasiKarakter = "\x1B\x20\x02";
                    string nomorMeja = "Meja No." + datas.data.customer_seat;

                    // Struct template pembelian
                    string strukText = kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText(totalTransactions.ToString()) + "\n";
                    strukText += "\n" + kodeHeksadesimalBold + CenterText(datas.data.outlet_name) + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += CenterText(datas.data.outlet_address) + "\n";
                    strukText += CenterText(datas.data.outlet_phone_number) + "\n";
                    strukText += "--------------------------------\n";

                    strukText += kodeHeksadesimalSizeBesar + CenterText("Pembelian") + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += "--------------------------------\n";
                    strukText += CenterText(datas.data.receipt_number) + "\n";
                    strukText += CenterText(datas.data.invoice_due_date) + "\n \n";
                    strukText += FormatSimpleLine(datas.data.customer_name, nomorMeja) + "\n";

                    // Membership area
                    if(datas.data.member_name != null)
                    {
                        strukText += "--------------------------------\n";
                        strukText += FormatSimpleLine("MEMBER : ",datas.data.member_name) + "\n";
                        strukText += FormatSimpleLine("No. HP : ",datas.data.member_phone_number) + "\n";
                        strukText += "--------------------------------\n";
                    }

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
                            strukText += FormatItemLine(cartDetail.menu_name, cartDetail.qty, cartDetail.price) + "\n";
                            // Add detail items
                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                strukText += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";
                            if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                strukText += FormatDetailItemLine("Discount Code", cartDetail.discount_code) + "\n";
                            if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                                strukText += FormatDetailItemLine("Total Discount", string.Format("Rp. {0:n0},-", cartDetail.discounted_price)) + "\n";
                            strukText += FormatDetailItemLine("Total Price", string.Format("Rp. {0:n0},-", cartDetail.total_price)) + "\n";

                            // Add an empty line between items
                            strukText += "\n";
                        }
                    }
                    strukText += "--------------------------------\n";
                    strukText += FormatSimpleLine("Subtotal", string.Format("Rp. {0:n0},-", datas.data.subtotal)) + "\n";
                    if (!string.IsNullOrEmpty(datas.data.discount_code))
                        strukText += FormatSimpleLine("Discount Code", datas.data.discount_code) + "\n";
                    if (datas.data.discounts_value.HasValue && datas.data.discounts_value != 0)
                        strukText += FormatSimpleLine("Discount Value", datas.data.discounts_value) + "\n";
                    strukText += FormatSimpleLine("Total", string.Format("Rp. {0:n0},-", datas.data.total)) + "\n";
                    strukText += FormatSimpleLine("Payment Type", datas.data.payment_type) + "\n";
                    strukText += FormatSimpleLine("Cash", string.Format("Rp. {0:n0},-", datas.data.customer_cash)) + "\n";
                    strukText += FormatSimpleLine("Change", string.Format("Rp. {0:n0},-", datas.data.customer_change)) + "\n";
                    strukText += "--------------------------------\n";
                    strukText += CenterText(Kakimu) + "\n";
                    strukText += CenterText(datas.data.outlet_footer) + "\n\n\n\n\n\n\n\n\n\n\n";

                    string strukTextChecker = kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText(totalTransactions.ToString()) + "\n";
                    strukTextChecker += "\n" + kodeHeksadesimalBold + CenterText(datas.data.outlet_name) + "\n";
                    strukTextChecker += kodeHeksadesimalNormal;
                    strukTextChecker += CenterText(datas.data.outlet_address) + "\n";
                    strukTextChecker += CenterText(datas.data.outlet_phone_number) + "\n";
                    strukTextChecker += "--------------------------------\n";

                    strukTextChecker += kodeHeksadesimalSizeBesar + CenterText("Checker") + "\n";
                    strukTextChecker += kodeHeksadesimalNormal;
                    strukTextChecker += "--------------------------------\n";
                    strukTextChecker += CenterText(datas.data.receipt_number) + "\n";
                    strukTextChecker += CenterText(datas.data.invoice_due_date) + "\n \n";
                    strukTextChecker += kodeHeksadesimalSizeBesar + FormatSimpleLine(datas.data.customer_name, nomorMeja) + kodeHeksadesimalNormal + "\n";

                    // Iterate through cart details and group by serving_type_name
                    var checkerServingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();

                    foreach (var checkerServingType in checkerServingTypes)
                    {
                        // Filter cart details by serving_type_name
                        var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == checkerServingType).ToList();

                        // Skip if there are no items for this serving type
                        if (itemsForServingType.Count == 0)
                            continue;

                        // Add a section for the serving type
                        strukTextChecker += "--------------------------------\n";
                        strukTextChecker += CenterText(checkerServingType) + "\n";
                        strukTextChecker += "--------------------------------\n";

                        // Iterate through items for this serving type
                        foreach (var cartDetail in itemsForServingType)
                        {
                            strukTextChecker += kodeHeksadesimalBold + FormatSimpleLine(cartDetail.menu_name, cartDetail.qty) + kodeHeksadesimalNormal + "\n";
                            // Add detail items
                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                strukTextChecker += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                strukTextChecker += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";

                            // Add an empty line between itemsprint
                            strukTextChecker += "\n";
                        }
                    }
                    strukTextChecker += "--------------------------------\n";
                    strukTextChecker += CenterText(datas.data.outlet_footer) + "\n\n\n\n\n\n";


                    // Encode your text into bytes (you might need to adjust the encoding)
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);
                    byte[] bufferChecker = System.Text.Encoding.UTF8.GetBytes(strukTextChecker);

                    // Create a combined byte array
                    byte[] combinedBuffer = new byte[buffer.Length + bufferChecker.Length];
                    Buffer.BlockCopy(buffer, 0, combinedBuffer, 0, buffer.Length);
                    Buffer.BlockCopy(bufferChecker, 0, combinedBuffer, buffer.Length, bufferChecker.Length);

                    // Send the combined text to the printer
                    stream.Write(combinedBuffer, 0, combinedBuffer.Length);

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
                //MessageBox.Show("Gagal bayar menu " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private bool IsValidIPAddress(string address)
        {
            // Try to parse the string as an IP address
            return System.Net.IPAddress.TryParse(address, out _);
        }

        private async Task<BluetoothClient> ConnectToBluetoothPrinter(string macAddress, string pinPrinter, int timeoutSeconds = 10)
        {
            BluetoothDeviceInfo printer = new BluetoothDeviceInfo(BluetoothAddress.Parse(macAddress));
            if (printer == null)
            {
                throw new Exception("Printer tidak ditemukan: " + macAddress);
            }

            BluetoothClient clientSocket = new BluetoothClient();
            BluetoothEndPoint endpoint = new BluetoothEndPoint(printer.DeviceAddress, BluetoothService.SerialPort);

            // Mengatur timeout menggunakan CancellationTokenSource
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                bool isPaired = await Task.Run(() => BluetoothSecurity.PairRequest(printer.DeviceAddress, pinPrinter), cts.Token);

                if (!isPaired)
                {
                    throw new Exception("Gagal melakukan pairing dengan printer: " + macAddress);
                }

                clientSocket.Connect(endpoint);
            }

            return clientSocket;
        }


        // Struct Kitchen&Bar
        private async Task PrintKitchenAndBarReceiptsAsync(GetStrukCustomerTransaction datas, List<KitchenAndBarCartDetails> cartDetails, string header, string macAddress, string pinPrinter, List<KitchenAndBarCanceledItems> cancelItems)
        {
            await RetryPolicyAsync(async () =>
            {
                try
            {
                    // Panggil metode ConnectToBluetoothPrinter dengan timeout 10 detik
                    BluetoothClient clientSocket = await ConnectToBluetoothPrinter(macAddress, pinPrinter, 10);
                    System.IO.Stream stream = clientSocket.GetStream();

                    // Template
                    string kodeHeksadesimalBold = "\x1B\x45\x01";
                    string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                    string kodeHeksadesimalSpasiKarakter = "\x1B\x20\x02";
                    string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00" + "\x1B\x20\x00";

                    // Custom variable
                    string nomorMeja = "Meja No." + datas.data.customer_seat;

                    // Generate struk text
                    string strukText = kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText(totalTransactions.ToString()) + "\n";

                    strukText += "--------------------------------\n";
                    strukText += kodeHeksadesimalBold + CenterText(header) + "\n";
                    //string strukText = kodeHeksadesimalBold + CenterText(header) + "\n";
                    //strukText += kodeHeksadesimalBold + CenterText(totalTransactions + 1) + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += "--------------------------------\n";
                    strukText += CenterText(datas.data.receipt_number) + "\n";
                    strukText += CenterText(datas.data.customer_name) + "\n";
                    strukText += CenterText(nomorMeja) + "\n";

                    if (cartDetails.Count != 0)
                    {
                        var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();
                        strukText += "\n--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText("ORDER") + "\n";
                        strukText += kodeHeksadesimalNormal;

                        foreach (var servingType in servingTypes)
                        {
                            var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                            if (itemsForServingType.Count == 0)
                                continue;

                            strukText += "--------------------------------\n";
                            strukText += CenterText(servingType) + "\n\n";

                            foreach (var cartDetail in itemsForServingType)
                            {
                                string qtyMenu = "x " + cartDetail.qty.ToString();
                                strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + kodeHeksadesimalSpasiKarakter + FormatKitchenBarLine(qtyMenu, cartDetail.menu_name) + kodeHeksadesimalNormal + "\n";

                                if (!string.IsNullOrEmpty(cartDetail.varian))
                                    strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                                if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                    strukText += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";

                                // Add an empty line between items
                                strukText += "\n";
                            }
                        }
                    }

                    if (cancelItems.Count != 0)
                    {
                        var servingTypes = cancelItems.Select(cd => cd.serving_type_name).Distinct();
                        strukText += "\n--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText("CANCELED") + "\n";
                        strukText += kodeHeksadesimalNormal;

                        foreach (var servingType in servingTypes)
                        {
                            var itemsForServingType = cancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                            if (itemsForServingType.Count == 0)
                                continue;

                            strukText += "--------------------------------\n";
                            strukText += CenterText(servingType) + "\n\n";

                            foreach (var cancelItem in itemsForServingType)
                            {
                                string qtyMenu = "x " + cancelItem.qty.ToString();
                                strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + kodeHeksadesimalSpasiKarakter + FormatKitchenBarLine(qtyMenu, cancelItem.menu_name) + kodeHeksadesimalNormal + "\n";

                                if (!string.IsNullOrEmpty(cancelItem.varian))
                                    strukText += FormatDetailItemLine("Varian", cancelItem.varian) + "\n";
                                if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                    strukText += FormatDetailItemLine("Note", cancelItem.note_item) + "\n";
                                if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                    strukText += FormatDetailItemLine("Canceled Reason", cancelItem.cancel_reason) + "\n";
                                // Add an empty line between items
                                strukText += "\n";
                            }
                        }
                    }

                    strukText += "--------------------------------\n\n\n\n\n";

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
                    btnSimpan.Text = "Selesai.";

                
            }

                catch (TaskCanceledException ex)
                {
                    throw new Exception("Pairing with the printer timed out.", ex);
                }
                catch (Exception ex)
            {
                btnSimpan.Text = "Selesai.";
                //MessageBox.Show("Gagal bayar menu " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    throw;
            }
            }, maxAttempts: 3, retryDelayMilliseconds: 2000);
        }

        private async void PrintKitchenAndBarReceipts(GetStrukCustomerTransaction datas, List<KitchenAndBarCartDetails> cartDetails, string header, string macAddress, string pinPrinter, List<KitchenAndBarCanceledItems> cancelItems)
        {
            try
            {


                BluetoothDeviceInfo printer = new BluetoothDeviceInfo(BluetoothAddress.Parse(macAddress));
                if (printer == null)
                {
                    //MessageBox.Show("Printer" + macAddress + "not found.", "Gaspol");
                    return;
                }

                BluetoothClient client = new BluetoothClient();
                BluetoothEndPoint endpoint = new BluetoothEndPoint(printer.DeviceAddress, BluetoothService.SerialPort);

                using (BluetoothClient clientSocket = new BluetoothClient())
                {
                    if (!BluetoothSecurity.PairRequest(printer.DeviceAddress, pinPrinter))
                    {
                        //MessageBox.Show("Pairing failed to " + macAddress, "Gaspol");
                        return;
                    }
                    clientSocket.Connect(endpoint);
                    // Kode setelah koneksi berhasil
                    System.IO.Stream stream = clientSocket.GetStream();

                    // Template
                    string kodeHeksadesimalBold = "\x1B\x45\x01";
                    string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                    string kodeHeksadesimalSpasiKarakter = "\x1B\x20\x02";
                    string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00" + "\x1B\x20\x00";

                    // Custom variable
                    string nomorMeja = "Meja No." + datas.data.customer_seat;

                    // Generate struk text
                    string strukText = kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText(totalTransactions.ToString()) + "\n";

                    strukText += "--------------------------------\n";
                    strukText += kodeHeksadesimalBold + CenterText(header) + "\n";
                    //string strukText = kodeHeksadesimalBold + CenterText(header) + "\n";
                    //strukText += kodeHeksadesimalBold + CenterText(totalTransactions + 1) + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += "--------------------------------\n";
                    strukText += CenterText(datas.data.receipt_number) + "\n";
                    strukText += CenterText(datas.data.customer_name) + "\n";
                    strukText += CenterText(nomorMeja) + "\n";

                    if (cartDetails.Count != 0)
                    {
                        var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();
                        strukText += "\n--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText("ORDER") + "\n";
                        strukText += kodeHeksadesimalNormal;

                        foreach (var servingType in servingTypes)
                        {
                            var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                            if (itemsForServingType.Count == 0)
                                continue;

                            strukText += "--------------------------------\n";
                            strukText += CenterText(servingType) + "\n\n";

                            foreach (var cartDetail in itemsForServingType)
                            {
                                string qtyMenu = "x " + cartDetail.qty.ToString();
                                strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + kodeHeksadesimalSpasiKarakter + FormatKitchenBarLine(qtyMenu, cartDetail.menu_name) + kodeHeksadesimalNormal + "\n";

                                if (!string.IsNullOrEmpty(cartDetail.varian))
                                    strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                                if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                    strukText += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";

                                // Add an empty line between items
                                strukText += "\n";
                            }
                        }
                    }

                    if (cancelItems.Count != 0)
                    {
                        var servingTypes = cancelItems.Select(cd => cd.serving_type_name).Distinct();
                        strukText += "\n--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText("CANCELED") + "\n";
                        strukText += kodeHeksadesimalNormal;

                        foreach (var servingType in servingTypes)
                        {
                            var itemsForServingType = cancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                            if (itemsForServingType.Count == 0)
                                continue;

                            strukText += "--------------------------------\n";
                            strukText += CenterText(servingType) + "\n\n";

                            foreach (var cancelItem in itemsForServingType)
                            {
                                string qtyMenu = "x " + cancelItem.qty.ToString();
                                strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + kodeHeksadesimalSpasiKarakter + FormatKitchenBarLine(qtyMenu, cancelItem.menu_name) + kodeHeksadesimalNormal + "\n";

                                if (!string.IsNullOrEmpty(cancelItem.varian))
                                    strukText += FormatDetailItemLine("Varian", cancelItem.varian) + "\n";
                                if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                    strukText += FormatDetailItemLine("Note", cancelItem.note_item) + "\n";
                                if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                    strukText += FormatDetailItemLine("Canceled Reason", cancelItem.cancel_reason) + "\n";
                                // Add an empty line between items
                                strukText += "\n";
                            }
                        }
                    }

                    strukText += "--------------------------------\n\n\n\n\n";

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
                    btnSimpan.Text = "Selesai.";
                    //OnSimpanSuccess();

                }
            }
            catch (Exception ex)
            {
                btnSimpan.Text = "Selesai.";

                //OnSimpanSuccess();

                //karena keadaan kecetak jadi gaperlu notif ganggu ini
                //MessageBox.Show("Gagal bayar menu " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private async void PrintKitchenAndBarReceiptsLAN(GetStrukCustomerTransaction datas, List<KitchenAndBarCartDetails> cartDetails, string header, string macAddress, string pinPrinter, List<KitchenAndBarCanceledItems> cancelItems)
        {
            try
            {
                string printerIpAddress = MacAddressKasir; // Ganti dengan IP address printer Anda
                int printerPort = Int32.Parse(PinPrinterKasir.ToString()); // Port standar untuk printer jaringan

                using (TcpClient client = new TcpClient(printerIpAddress, printerPort))
                {
                    using (NetworkStream stream = client.GetStream())
                    {

                        // Template
                        string kodeHeksadesimalBold = "\x1B\x45\x01";
                        string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                        string kodeHeksadesimalSpasiKarakter = "\x1B\x20\x02";
                        string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00" + "\x1B\x20\x00";

                        // Custom variable
                        string nomorMeja = "Meja No." + datas.data.customer_seat;

                        // Generate struk text
                        string strukText = kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText(totalTransactions.ToString()) + "\n";

                        strukText += "--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText(header) + "\n";
                        //string strukText = kodeHeksadesimalBold + CenterText(header) + "\n";
                        //strukText += kodeHeksadesimalBold + CenterText(totalTransactions + 1) + "\n";
                        strukText += kodeHeksadesimalNormal;
                        strukText += "--------------------------------\n";
                        strukText += CenterText(datas.data.receipt_number) + "\n";
                        strukText += CenterText(datas.data.customer_name) + "\n";
                        strukText += CenterText(nomorMeja) + "\n";

                        if (cartDetails.Count != 0)
                        {
                            var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();
                            strukText += "\n--------------------------------\n";
                            strukText += kodeHeksadesimalBold + CenterText("ORDER") + "\n";
                            strukText += kodeHeksadesimalNormal;

                            foreach (var servingType in servingTypes)
                            {
                                var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                                if (itemsForServingType.Count == 0)
                                    continue;

                                strukText += "--------------------------------\n";
                                strukText += CenterText(servingType) + "\n\n";

                                foreach (var cartDetail in itemsForServingType)
                                {
                                    string qtyMenu = "x " + cartDetail.qty.ToString();
                                    strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + kodeHeksadesimalSpasiKarakter + FormatKitchenBarLine(qtyMenu, cartDetail.menu_name) + kodeHeksadesimalNormal + "\n";

                                    if (!string.IsNullOrEmpty(cartDetail.varian))
                                        strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                                    if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                        strukText += FormatDetailItemLine("Note", cartDetail.note_item) + "\n";

                                    // Add an empty line between items
                                    strukText += "\n";
                                }
                            }
                        }

                        if (cancelItems.Count != 0)
                        {
                            var servingTypes = cancelItems.Select(cd => cd.serving_type_name).Distinct();
                            strukText += "\n--------------------------------\n";
                            strukText += kodeHeksadesimalBold + CenterText("CANCELED") + "\n";
                            strukText += kodeHeksadesimalNormal;

                            foreach (var servingType in servingTypes)
                            {
                                var itemsForServingType = cancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                if (itemsForServingType.Count == 0)
                                    continue;

                                strukText += "--------------------------------\n";
                                strukText += CenterText(servingType) + "\n\n";

                                foreach (var cancelItem in itemsForServingType)
                                {
                                    string qtyMenu = "x " + cancelItem.qty.ToString();
                                    strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + kodeHeksadesimalSpasiKarakter + FormatKitchenBarLine(qtyMenu, cancelItem.menu_name) + kodeHeksadesimalNormal + "\n";

                                    if (!string.IsNullOrEmpty(cancelItem.varian))
                                        strukText += FormatDetailItemLine("Varian", cancelItem.varian) + "\n";
                                    if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                        strukText += FormatDetailItemLine("Note", cancelItem.note_item) + "\n";
                                    if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                        strukText += FormatDetailItemLine("Canceled Reason", cancelItem.cancel_reason) + "\n";
                                    // Add an empty line between items
                                    strukText += "\n";
                                }
                            }
                        }

                        strukText += "--------------------------------\n\n\n\n\n";


                        // Encode your text into bytes (you might need to adjust the encoding)
                        byte[] buffer = Encoding.UTF8.GetBytes(strukText);


                        // Send the combined text to the printer
                        await stream.WriteAsync(buffer, 0, buffer.Length);

                        // Flush the stream to ensure all data is sent to the printer
                        await stream.FlushAsync();
                        btnSimpan.Text = "Selesai.";

                    }
                }
                }
            catch (Exception ex)
            {
                btnSimpan.Text = "Selesai.";

                //OnSimpanSuccess();

                //karena keadaan kecetak jadi gaperlu notif ganggu ini
                //MessageBox.Show("Gagal bayar menu " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSetPrice1_Click(object sender, EventArgs e)
        {
            txtCash.Text = Regex.Replace(btnSetPrice1.Text, "[^0-9]", "");
            ////LoggerUtil.LogPrivateMethod(nameof(btnSetPrice1_Click));

        }
        private void btnSetPrice2_Click_1(object sender, EventArgs e)
        {
            txtCash.Text = Regex.Replace(btnSetPrice2.Text, "[^0-9]", "");
            ////LoggerUtil.LogPrivateMethod(nameof(btnSetPrice2_Click_1));

        }

        private void btnSetPrice3_Click(object sender, EventArgs e)
        {
            txtCash.Text = Regex.Replace(btnSetPrice3.Text, "[^0-9]", "");
            ////LoggerUtil.LogPrivateMethod(nameof(btnSetPrice3_Click));

        }

        private void txtCash_TextChanged(object sender, EventArgs e)
        {
            if (txtCash.Text == "" || txtCash.Text == "0") return;
            decimal number;
            try
            {
                number = decimal.Parse(txtCash.Text, System.Globalization.NumberStyles.Currency);
                // Menghitung nilai kembalian
                int KembalianSekarang = Int32.Parse(Regex.Replace(txtCash.Text, "[^0-9]", "")) - Int32.Parse(Regex.Replace(ttl2, "[^0-9]", ""));

                // Mengatur format budaya Indonesia
                CultureInfo culture = new CultureInfo("id-ID");

                // Menampilkan hasil kembalian dalam format mata uang rupiah
                lblKembalian.Text = "CHANGES \n\n" + KembalianSekarang.ToString("C", culture);
            }
            catch (FormatException)
            {
                // The text could not be parsed as a decimal number.
                // You can handle this exception in different ways, such as displaying a message to the user.
                MessageBox.Show("inputan hanya bisa Numeric");
                return;
            }
            txtCash.Text = number.ToString("#,#");
            txtCash.SelectionStart = txtCash.Text.Length;
        }

        private void sButton1_CheckedChanged(object sender, EventArgs e)
        {
            if(sButton1.Checked == true)
            {
                panel8.Visible = true;
                panel13.Visible = true;
                panel14.Visible = true;
                btnDataMember.Visible = true;
                lblPoint.Visible = true;

            }
            else
            {
                panel8.Visible = false;
                panel13.Visible = false;
                panel14.Visible = false;
                btnDataMember.Visible = false;
                lblPoint.Visible = false;
            }
        }

        private void btnDataMember_Click(object sender, EventArgs e)
        {
            try
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

                using (dataMember listMember = new dataMember())
                {
                    listMember.Owner = background;

                    background.Show();

                    //DialogResult dialogResult = dataBill.ShowDialog();

                    //background.Dispose();
                    DialogResult result = listMember.ShowDialog();

                    // Handle the result if needed
                    if (result == DialogResult.OK)
                    {
                        SelectedId = listMember.SelectedId;
                        lblNamaMember.Text = listMember.namaMember;
                        lblEmailMember.Text = listMember.emailMember;
                        lblHPMember.Text = listMember.hpMember;
                        txtNama.Text = listMember.namaMember;
                        background.Dispose();
                        // Settings were successfully updated, perform any necessary actions
                    }
                    else
                    {
                        //MessageBox.Show("Gagal Simpan, Silahkan coba lagi");
                        background.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                DialogResult = DialogResult.Cancel;
                MessageBox.Show("Gagal load Member " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                Close();
            }
        }
    }
}