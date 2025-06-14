using System.Data;
using KASIR.Model;
using KASIR.Network;
using KASIR.Printer;
using Newtonsoft.Json;
using Serilog;
namespace KASIR.komponen
{
    public partial class saveBill : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        string cart_id;
        private readonly string baseOutlet;
        private readonly string MacAddressKasir;
        private readonly string MacAddressKitchen;
        private readonly string MacAddressBar;
        private readonly string PinPrinterKasir;
        private readonly string PinPrinterKitchen;
        private readonly string PinPrinterBar;
        private readonly string BaseOutletName;
        public bool ReloadDataInBaseForm { get; private set; }
        private string name, seat;
        public saveBill(string cartId, string customerName, string customerSeat)
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            cart_id = cartId;
            name = customerName;
            seat = customerSeat;
            InitializeComponent();
            txtNama.Text = name.ToString();
            txtSeat.Text = seat.ToString();
            generateRandomFill();
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            MacAddressKitchen = Properties.Settings.Default.MacAddressKitchen;
            MacAddressBar = Properties.Settings.Default.MacAddressBar;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            PinPrinterKitchen = Properties.Settings.Default.PinPrinterKitchen;
            PinPrinterBar = Properties.Settings.Default.PinPrinterBar;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
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

                txtNama.Text = name + " (DT-AutoFill)";
                txtSeat.Text = "0";
            }

        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private async void btnSimpan_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnSimpan_Click));

            try
            {
                int seat = 0;
                if (txtNama.Text.ToString() == "" || txtNama.Text == null)
                {
                    MessageBox.Show("Masukan nama pelanggan", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (txtSeat.Text.ToString() == "" || txtSeat.Text == null)
                {

                    MessageBox.Show("Masukan seat pelanggan", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    seat = int.Parse(txtSeat.Text.ToString());
                }
                var json = new
                {
                    outlet_id = baseOutlet,
                    cart_id = int.Parse(cart_id),
                    customer_name = txtNama.Text.ToString(),
                    customer_seat = txtSeat.Text.ToString(),
                };
                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                IApiService apiService = new ApiService();
                string response = await apiService.SaveBill(jsonString, "/transaction");

                string responseAntrianSaveBill = await apiService.GetListBill("/transaction?outlet_id=", baseOutlet);

                ListBillModel menuModel = JsonConvert.DeserializeObject<ListBillModel>(responseAntrianSaveBill);
                List<ListBill> menuList = menuModel.data.ToList();
                int AntrianSaveBill = 0;
                foreach (ListBill menu in menuList)
                {
                    AntrianSaveBill++;
                    if (menu.cart_id == int.Parse(cart_id)) break;
                }
                if (AntrianSaveBill <= menuList.Count)
                {
                    //
                }
                else
                {
                    AntrianSaveBill++;
                }

                if (!string.IsNullOrWhiteSpace(response))
                {
                    await HandleSuccessfulTransaction(response, AntrianSaveBill);
                }
                else
                {
                    MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetButtonState();
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show("Terjadi kesalahan, silakan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetButtonState();
            }
        }

        private void ResetButtonState()
        {
            btnSimpan.Enabled = true;
            btnSimpan.Text = "Simpan";
            btnSimpan.BackColor = Color.FromArgb(31, 30, 68);
        }

        private async Task HandleSuccessfulTransaction(string response, int AntrianSaveBill)
        {
            try
            {
                PrinterModel printerModel = new PrinterModel();

                GetStrukCustomerTransaction menuModel = JsonConvert.DeserializeObject<GetStrukCustomerTransaction>(response);

                if (menuModel == null)
                {
                    throw new InvalidOperationException("Deserialization failed: menuModel is null");
                }

                DataStrukCustomerTransaction data = menuModel.data;
                if (data == null)
                {
                    throw new InvalidOperationException("Deserialization failed: data is null");
                }


                List<CartDetailStrukCustomerTransaction> listCart = data.cart_details;
                List<CanceledItemStrukCustomerTransaction> listCanceled = data.canceled_items;

                List<CartDetailStrukCustomerTransaction> kitchenItems = listCart?.Where(cd => cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList();
                List<CartDetailStrukCustomerTransaction> barItems = listCart?.Where(cd => cd.menu_type == "Minuman" || cd.menu_type == "Additional Minuman").ToList();

                List<CanceledItemStrukCustomerTransaction> canceledKitchenItems = listCanceled?.Where(cd => cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList();
                List<CanceledItemStrukCustomerTransaction> canceledBarItems = listCanceled?.Where(cd => cd.menu_type == "Minuman" || cd.menu_type == "Additional Minuman").ToList();


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
                    await Task.Run(() =>
                    {
                        printerModel.PrinterModelSimpan(menuModel, kitchenItems, canceledKitchenItems, barItems, canceledBarItems, AntrianSaveBill);
                    });
                }
                else
                {
                    throw new InvalidOperationException("printerModel is null");
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
                        u.sendLogTelegramNetworkError("Gagal Mencetak, Log Outlet : " + BaseOutletName);
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

        private void saveBill_Load(object sender, EventArgs e)
        {

        }

        private void txtSeat_Enter(object sender, EventArgs e)
        {
            if (txtSeat.Text != null) txtSeat.Text = "";

        }

        private void txtNama_Click(object sender, EventArgs e)
        {
            if (txtNama.Text != null) txtNama.Text = "";

        }
    }
}