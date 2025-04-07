using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.Printer;
using Newtonsoft.Json;
using Serilog;
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
        private string Kakimu;
        private PrinterModel printerModel; // Pastikan ini telah diinisialisasi dengan benar

        private readonly ILogger _log = LoggerService.Instance._log;
        public bool KeluarButtonClicked { get; private set; }

        public bool ReloadDataInBaseForm { get; private set; }
        private DataTable originalDataTable, listDataTable;
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

                // Check if menuModel.data is null or empty, and set totalTransactions to 0
                if (menuModel?.data == null || !menuModel.data.Any())
                {
                    totalTransactions = 0;
                }
                else
                {
                    List<KASIR.Model.Menu> menuList = menuModel.data.ToList();
                    totalTransactions = menuList.Count + 1;
                }
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                totalTransactions = 0; // Set totalTransactions to 0 in case of timeout or error
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                totalTransactions = 0; // Set totalTransactions to 0 in case of other errors
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
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                // Optionally, you can show a message to the user about the error
                if (btnSimpan != null)
                {
                    btnSimpan.Text = "Print Ulang,";
                    btnSimpan.Enabled = true;
                }
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
                        MessageBox.Show("Gagal mencetak setelah beberapa kali percobaan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoggerUtil.LogError(ex, $"Gagal mencetak setelah beberapa kali percobaan. {currentAttempt}.");

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
            if (sButton1.Checked == true)
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