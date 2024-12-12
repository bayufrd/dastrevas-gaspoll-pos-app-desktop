
using FontAwesome.Sharp;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
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
using System.Windows.Forms;
using Serilog;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using KASIR.Printer;
namespace KASIR.komponen
{
    public partial class saveBill : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        public event EventHandler MasterPos;
        string cart_id;
        int row;
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
                    //await printerModel.PrinterModelSimpan(menuModel, kitchenItems, canceledKitchenItems, barItems, canceledBarItems, AntrianSaveBill);
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

        private void OnSimpanSuccess()
        {
            // Refresh or reload the cart in the MasterPos form
            if (Application.OpenForms["masterPos"] is masterPos masterPosForm)
            {
                // Call a method in the MasterPos form to refresh the cart
                masterPosForm.LoadCart(); // You'll need to define this method in MasterPos
            }
            // Display a confirmation dialog
            DialogResult confirmationResult = MessageBox.Show("Pesanan selesai disimpan ?", "Simpan Bill", MessageBoxButtons.OK);

            // Check the user's response
            if (confirmationResult == DialogResult.OK)
            {
                // Close the form
                masterPos MasterposInstance = new masterPos();
                MasterposInstance.LoadCart();
                MasterposInstance.ReloadCart(); //gamau refresh :((
                MasterposInstance.ReloadData2();
                this.Close();


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
        private async Task PrintPurchaseReceipt(GetStrukCustomerTransaction datas, List<CartDetailStrukCustomerTransaction> cartDetails, List<CanceledItemStrukCustomerTransaction> canceledItems)
        {
            await RetryPolicyAsync(async () =>
            {
                try
            {
                    BluetoothDeviceInfo printer = new BluetoothDeviceInfo(BluetoothAddress.Parse(MacAddressKasir));
                    if (printer == null)
                    {
                        throw new Exception("Printer" + MacAddressKasir + "not found."); // Trigger retry if printer is not found
                                                                                         //MessageBox.Show("Printer" + MacAddressKasir + "not found.", "Gaspol");
                                                                                         //return;
                    }

                    BluetoothClient client = new BluetoothClient();
                BluetoothEndPoint endpoint = new BluetoothEndPoint(printer.DeviceAddress, BluetoothService.SerialPort);

                using (BluetoothClient clientSocket = new BluetoothClient())
                {
                        // Timeout Pairing Bluetooth
                        TimeSpan pairingTimeout = TimeSpan.FromSeconds(10);
                        CancellationTokenSource cts = new CancellationTokenSource();
                        cts.CancelAfter(pairingTimeout);

                        // Pairing with Timeout check
                        bool isPaired = await Task.Run(() => BluetoothSecurity.PairRequest(printer.DeviceAddress, PinPrinterKasir), cts.Token);

                        if (!isPaired)
                        {
                            throw new Exception("Pairing failed to " + MacAddressKasir + " after timeout of " + pairingTimeout.Seconds + " seconds."); // Trigger retry if pairing fails
                        }

                        clientSocket.Connect(endpoint);
                        System.IO.Stream stream = clientSocket.GetStream();

                        // Custom variable
                        string kodeHeksadesimalBold = "\x1B\x45\x01";
                    string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                    string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                    string nomorMeja = "Meja No." + datas.data.customer_seat;

                    // Struct template
                    string strukText = "\n" + kodeHeksadesimalBold + CenterText(datas.data.outlet_name) + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += CenterText(datas.data.outlet_address) + "\n";
                    strukText += CenterText(datas.data.outlet_phone_number) + "\n";
                    strukText += "--------------------------------\n";

                    strukText += kodeHeksadesimalSizeBesar + CenterText("Checker") + "\n";
                    strukText += kodeHeksadesimalNormal;
                    strukText += "--------------------------------\n";
                    strukText += CenterText(datas.data.receipt_number) + "\n";
                    strukText += CenterText(datas.data.customer_name) + "\n";
                    strukText += CenterText(nomorMeja) + "\n";

                    if (cartDetails.Count != 0)
                    {
                        // Iterate through cart details and group by serving_type_name
                        var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();
                        strukText += "\n--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText("ORDER") + "\n";
                        strukText += kodeHeksadesimalNormal;

                        foreach (var servingType in servingTypes)
                        {
                            // Filter cart details by serving_type_name
                            var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                            // Skip if there are no items for this serving type
                            if (itemsForServingType.Count == 0)
                                continue;

                            // Add a section for the serving type
                            strukText += "--------------------------------\n";
                            strukText += CenterText(servingType) + "\n\n";

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
                    }

                    if (canceledItems.Count != 0)
                    {
                        // Iterate through cart details and group by serving_type_name
                        var servingTypes = canceledItems.Select(cd => cd.serving_type_name).Distinct();
                        strukText += "\n--------------------------------\n";
                        strukText += kodeHeksadesimalBold + CenterText("CANCELED") + "\n";
                        strukText += kodeHeksadesimalNormal;

                        foreach (var servingType in servingTypes)
                        {
                            // Filter cart details by serving_type_name
                            var itemsForServingType = canceledItems.Where(cd => cd.serving_type_name == servingType).ToList();

                            // Skip if there are no items for this serving type
                            if (itemsForServingType.Count == 0)
                                continue;

                            // Add a section for the serving type
                            strukText += "--------------------------------\n";
                            strukText += CenterText(servingType) + "\n\n";

                            // Iterate through items for this serving type
                            foreach (var canceledItem in itemsForServingType)
                            {
                                strukText += FormatItemLine(canceledItem.menu_name, canceledItem.qty, canceledItem.total_price) + "\n";
                                // Add detail items
                                if (!string.IsNullOrEmpty(canceledItem.varian))
                                    strukText += FormatDetailItemLine("Varian", canceledItem.varian) + "\n";
                                if (!string.IsNullOrEmpty(canceledItem.note_item?.ToString()))
                                    strukText += FormatDetailItemLine("Note", canceledItem.note_item) + "\n";
                                if (!string.IsNullOrEmpty(canceledItem.discount_code))
                                    strukText += FormatDetailItemLine("Discount Code", canceledItem.discount_code) + "\n";
                                if (canceledItem.discounted_price.HasValue && canceledItem.discounted_price != 0)
                                    strukText += FormatDetailItemLine("Total Discount", string.Format("Rp. {0:n0},-", canceledItem.discounted_price)) + "\n";
                                if (!string.IsNullOrEmpty(canceledItem.cancel_reason))
                                    strukText += FormatDetailItemLine("Canceled Reason", canceledItem.cancel_reason) + "\n";
                                // Add an empty line between items
                                strukText += "\n";
                            }
                        }
                    }

                    strukText += "--------------------------------\n";
                    strukText += FormatSimpleLine("Subtotal", string.Format("Rp. {0:n0},-", datas.data.subtotal)) + "\n";
                    if (!string.IsNullOrEmpty(datas.data.discount_code))
                        strukText += FormatSimpleLine("Discount Code", datas.data.discount_code) + "\n";
                    if (datas.data.discounts_value.HasValue && datas.data.discounts_value != 0)
                        strukText += FormatSimpleLine("Discount Value", datas.data.discounts_value) + "\n";
                    strukText += FormatSimpleLine("Total", string.Format("Rp. {0:n0},-", datas.data.total)) + "\n";
                    strukText += "--------------------------------\n";
                    strukText += CenterText(datas.data.outlet_footer) + "\n\n\n\n\n";

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


        // Struct Kitchen&Bar
        private async Task PrintKitchenAndBarReceipts(GetStrukCustomerTransaction datas, List<CartDetailStrukCustomerTransaction> cartDetails, string header, string macAddress, string pinPrinter, List<CanceledItemStrukCustomerTransaction> cancelItems)
        {
                await RetryPolicyAsync(async () =>
                {
                    try
            {


                        BluetoothDeviceInfo printer = new BluetoothDeviceInfo(BluetoothAddress.Parse(macAddress));
                        if (printer == null)
                        {
                            throw new Exception("Printer" + macAddress + "not found."); // Trigger retry if printer is not found
                                                                                             //MessageBox.Show("Printer" + MacAddressKasir + "not found.", "Gaspol");
                                                                                             //return;
                        }

                        BluetoothClient client = new BluetoothClient();
                BluetoothEndPoint endpoint = new BluetoothEndPoint(printer.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            // Timeout Pairing Bluetooth
                            TimeSpan pairingTimeout = TimeSpan.FromSeconds(10);
                            CancellationTokenSource cts = new CancellationTokenSource();
                            cts.CancelAfter(pairingTimeout);

                            // Pairing with Timeout check
                            bool isPaired = await Task.Run(() => BluetoothSecurity.PairRequest(printer.DeviceAddress, PinPrinterKasir), cts.Token);

                            if (!isPaired)
                            {
                                throw new Exception("Pairing failed to " + MacAddressKasir + " after timeout of " + pairingTimeout.Seconds + " seconds."); // Trigger retry if pairing fails
                            }

                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            // Template
                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalSpasiKarakter = "\x1B\x20\x02";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00" + "\x1B\x20\x00";

                            // Custom variable
                            string nomorMeja = "Meja No." + datas.data.customer_seat;

                            // Generate struk text
                            string strukText = kodeHeksadesimalBold + CenterText(header) + "\n";
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
                            //OnSimpanSuccess();

                        }
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


        // Fungsi untuk mengatur teks di tengah
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