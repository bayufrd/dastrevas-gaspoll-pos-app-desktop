
using FontAwesome.Sharp;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using System.Data;
using Serilog;
using KASIR.Printer;
using Newtonsoft.Json.Linq;
using System.Transactions;
using SharpCompress.Common;
using KASIR.OfflineMode;
using System.Globalization;
namespace KASIR.OffineMode
{
    public partial class Offline_saveBill : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        public event EventHandler MasterPos;
        string cart_id;
        int row;
        private readonly string baseOutlet;
        private readonly string BaseOutletName;
        public bool ReloadDataInBaseForm { get; private set; }
        private string name, seat;
        string namaMember, emailMember, hpMember, transactionId;
        int totalTransactions;
        public Offline_saveBill(string cartId, string customerName, string customerSeat)
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            cart_id = cartId;
            name = customerName;
            seat = customerSeat;
            InitializeComponent();
            generateRandomFill();
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
        }

        private void generateRandomFill()
        {
            Random random = new Random();

            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
            string[] vowels = { "a", "e", "i", "o", "u" };

            string newname = "";

            int nameLength = random.Next(3, 10);
            if (name == "" || seat == null)
            {
                for (int i = 0; i < nameLength; i++)
                {
                    newname += i % 2 == 0 ? consonants[random.Next(consonants.Length)] : vowels[random.Next(vowels.Length)];
                }

                newname = char.ToUpper(newname[0]) + newname.Substring(1);

                txtNama.Text = newname + " (DT)";
                txtSeat.Text = "0";
            }
            else
            {
                txtNama.Text = name.ToString();
                txtSeat.Text = seat.ToString();
            }

        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }
        private void loadCountingStruct()
        {
            try
            {
                // Path untuk file transaction.data
                string transactionFilePath = "DT-Cache\\Transaction\\transaction.data";

                // Cek apakah file transaction.data ada
                if (File.Exists(transactionFilePath))
                {
                    // Membaca isi file transaction.data
                    string transactionJson = File.ReadAllText(transactionFilePath);

                    // Deserialize data file transaction.data
                    var transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                    // Pastikan data ada di dalam file
                    if (transactionData["data"] == null)
                    {
                        totalTransactions = 1;
                        return;
                    }

                    // Ambil array data transaksi
                    var transactionDetails = transactionData["data"] as JArray;

                    // Hitung jumlah transaksi berdasarkan transaction_id
                    totalTransactions = transactionDetails.Count + 1;

                }
                else
                {
                    totalTransactions = 1;
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                totalTransactions = 1;
            }
        }

        private async void btnSimpan_Click(object sender, EventArgs e)
        {
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
                // Read cart.data to extract cart details and transaction id
                string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
                if (!File.Exists(cartDataPath))
                {
                    MessageBox.Show("Keranjang Masih Kosong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }
                loadCountingStruct();
                string cartDataJson = File.ReadAllText(cartDataPath);
                var cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);

                // Get the first cart_detail_id to set as transaction_id
                var cartDetails = cartData["cart_details"] as JArray;
                // Iterasi setiap transaksi dan hanya perbarui yang statusnya is_sent_sync = 0
                foreach (var items in cartDetails)
                {
                    if (items["is_ordered"] != null && (int)items["is_ordered"] == 0)
                    {
                        // Update status menjadi 1 jika transaksi berhasil
                        items["is_ordered"] = 1;
                        // Simpan perubahan hanya untuk transaksi yang berhasil disinkronkan
                    }
                }
                string firstCartDetailId = cartDetails?.FirstOrDefault()?["cart_detail_id"].ToString();
                transactionId = firstCartDetailId;
                int totalCartAmount = int.Parse(cartData["total"].ToString());
                int subtotalcart = int.Parse(cartData["subtotal"].ToString());
                string receiptMaker = cartDetails?.FirstOrDefault()?["created_at"].ToString();
                string invoiceMaker = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string formattedreceiptMaker;
                DateTime invoiceDate;
                if (DateTime.TryParse(receiptMaker, out invoiceDate))
                {
                    // Jika berhasil, format tanggal invoice sesuai kebutuhan
                    formattedreceiptMaker = invoiceDate.ToString("yyyyMMdd-HHmmss");
                }
                else
                {
                    // Jika parsing gagal, berikan nilai default atau tampilkan error
                    formattedreceiptMaker = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                }
                string receipt_numberfix = $"DT-{txtNama.Text}-{txtSeat.Text}-{formattedreceiptMaker}";
                string invoiceDue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                string transaction_ref_sent = cartData["transaction_ref"].ToString();
                string transaction_ref_splitted = (string)null;

                if (!string.IsNullOrEmpty(cartData["transaction_ref_split"]?.ToString()))
                {
                    transaction_ref_splitted = cartData["transaction_ref_split"]?.ToString();
                }
                int discount_idc = int.Parse(cartData["discount_id"]?.ToString() ?? "0");  // If null, set to 0
                string discount_codec = cartData["discount_code"]?.ToString() ?? "";  // If null, set to empty string
                string discounts_valuec = cartData["discounts_value"]?.ToString() ?? "0";  // If null, set to "0"
                string discounts_is_percentc = cartData["discounts_is_percent"]?.ToString() ?? "0";  // If null, set to "0"
                int discounted_pricec = int.TryParse(cartData["discounted_price"]?.ToString(), out int result) ? result : 0;  // If null, set to 0

                // Prepare transaction data
                var transactionData = new
                {
                    transaction_id = int.Parse(transactionId),
                    receipt_number = receipt_numberfix,
                    transaction_ref = transaction_ref_sent,
                    transaction_ref_split = transaction_ref_splitted,
                    invoice_number = (string)null,  // Custom invoice number with formatted date
                    invoice_due_date = (string)null, // Adjust due date as needed
                    payment_type_id = 0,
                    payment_type_name = (string)null, // No need for .ToString() if paymentTypeName is already a string
                    customer_name = txtNama.Text,
                    customer_seat = int.Parse(txtSeat.Text),
                    customer_cash = 0,
                    customer_change = 0,
                    total = totalCartAmount,
                    subtotal = subtotalcart, // You can replace this with actual subtotal if available
                    created_at = receiptMaker,
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    deleted_at = (string)null, // Ensure deleted_at is null, not a string "null"
                    is_refund = 0,
                    refund_reason = (string)null, // Null if no refund reason
                    delivery_type = (string)null, // Null value for delivery_type
                    delivery_note = (string)null, // Null value for delivery_note
                    discount_id = discount_idc,
                    discount_code = discount_codec,
                    discounts_value = discounts_valuec,
                    discounts_is_percent = discounts_is_percentc,
                    discounted_price = discounted_pricec,
                    member_name = (string)null, // Null if no member name
                    member_phone_number = (string)null, // Null if no member phone number
                    is_refund_all = 0,
                    refund_reason_all = (string)null,
                    refund_payment_id_all = 0,
                    refund_created_at_all = (string)null,
                    total_refund = 0,
                    refund_payment_name_all = (string)null,
                    is_savebill = 1,
                    is_edited_sync = 0,
                    is_sent_sync = 0,
                    cart_details = cartDetails,
                    refund_details = new JArray(), // Empty array for refund_details
                    canceled_items = new JArray() // Empty array for canceled_items
                };

                // Save transaction data to transaction.data
                string saveBillPath = "DT-Cache\\Transaction\\saveBill.data";
                JArray saveBillDataArray = new JArray();
                if (File.Exists(saveBillPath))
                {
                    // If the transaction file exists, read and append the new transaction
                    string existingData = File.ReadAllText(saveBillPath);
                    var existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                    saveBillDataArray = existingTransactions["data"] as JArray ?? new JArray();
                }

                // Add new transaction
                saveBillDataArray.Add(JToken.FromObject(transactionData));

                // Serialize and save back to transaction.data
                var newTransactionData = new JObject { { "data", saveBillDataArray } };
                File.WriteAllText(saveBillPath, JsonConvert.SerializeObject(newTransactionData, Formatting.Indented));
                convertData(receipt_numberfix, invoiceDue);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show($"Terjadi kesalahan, silakan coba lagi.{ex.ToString()}", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetButtonState();
            }
        }

        private void ResetButtonState()
        {
            btnSimpan.Enabled = true;
            btnSimpan.Text = "Simpan";
            btnSimpan.BackColor = Color.FromArgb(31, 30, 68);
        }
        private async Task convertData(string receipt_numberfix, string invoiceDue)
        {
            try
            {
                // Read cart.data to extract cart details and transaction id
                string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
                if (!File.Exists(cartDataPath))
                {
                    MessageBox.Show("Keranjang Masih Kosong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }

                string cartDataJson = File.ReadAllText(cartDataPath);
                // Deserialize cart data
                var cartData = JsonConvert.DeserializeObject<CartDataCache>(cartDataJson);

                // Membaca file JSON
                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                var dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                // Konversi ke format GetStrukCustomerTransaction
                var strukCustomerTransaction = new GetStrukCustomerTransaction
                {
                    code = 201,
                    message = "Transaksi Sukses!",
                    data = new DataStrukCustomerTransaction
                    {
                        outlet_name = dataOutlet.data.name,
                        outlet_address = dataOutlet.data.address,
                        outlet_phone_number = dataOutlet.data.phone_number,
                        outlet_footer = dataOutlet.data.footer,
                        transaction_id = int.Parse(transactionId),
                        receipt_number = receipt_numberfix,
                        invoice_due_date = invoiceDue,
                        customer_name = txtNama.Text,
                        customer_seat = int.Parse(txtSeat.Text),
                        payment_type = (string)null,
                        delivery_type = null,
                        delivery_note = null,
                        cart_id = 0,
                        subtotal = (int)cartData.subtotal,
                        total = (int)cartData.total,
                        discount_id = cartData?.discount_id != 0 ? cartData.discount_id : 0, // If discount_id is not 0, use it, else default to 0
                        discount_code = string.IsNullOrEmpty(cartData?.discount_code) ? (string)null : cartData.discount_code, // Set to null if discount_code is empty or null
                        discounts_value = string.IsNullOrEmpty(cartData?.discounts_value?.ToString()) ? (int?)null : (int?)int.Parse(cartData?.discounts_value.ToString()), // Set to null if no discount
                        discounts_is_percent = (cartData?.discounts_is_percent == null) ? (string)null : cartData?.discounts_is_percent.ToString(), // Set to null if no discount
                        cart_details = new List<CartDetailStrukCustomerTransaction>(),
                        canceled_items = new List<CanceledItemStrukCustomerTransaction>(),
                        kitchenBarCartDetails = new List<KitchenAndBarCartDetails>(),
                        kitchenBarCanceledItems = new List<KitchenAndBarCanceledItems>(),
                        customer_cash = 0,
                        customer_change = 0,
                        member_name = null,
                        member_phone_number = null
                    }
                };

                // Mengisi cart_details
                // Mengisi cart_details dan kitchenBarCartDetails
                foreach (var item in cartData.cart_details)
                {
                    if(item.is_ordered == 1 || item.is_printed == 1)
                    {
                        continue;
                    }
                    // Membuat objek CartDetailStrukCustomerTransaction
                    var cartDetail = new CartDetailStrukCustomerTransaction
                    {
                        cart_detail_id = int.Parse(item.cart_detail_id), // Mengonversi string ke int
                        menu_id = item.menu_id,
                        menu_name = item.menu_name,
                        menu_type = item.menu_type,
                        menu_detail_id = item.menu_detail_id,
                        varian = item.menu_detail_name, // Tidak ada data varian
                        serving_type_id = item.serving_type_id,
                        serving_type_name = item.serving_type_name,
                        discount_id = null, // Tidak ada data discount
                        discount_code = null,
                        discounts_value = null,
                        discounted_price = 0,
                        discounts_is_percent = null,
                        price = item.price, // Mengonversi string ke int
                        total_price = item.price * item.qty, // Total price dihitung dari price * qty
                        subtotal = item.price * item.qty, // Total price dihitung dari price * qty
                        subtotal_price = item.price * item.qty, // Total price dihitung dari price * qty
                        qty = item.qty,
                        note_item = string.IsNullOrEmpty(item.note_item) ? "" : item.note_item,
                        is_ordered = item.is_ordered
                    };

                    // Menambahkan ke cart_details
                    strukCustomerTransaction.data.cart_details.Add(cartDetail);

                    // Membuat objek KitchenAndBarCartDetails dan menyalin data dari cartDetail
                    var kitchenAndBarCartDetail = new KitchenAndBarCartDetails
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
                if(cartData.canceled_items != null && cartData.canceled_items.Count > 0)
                {
                    foreach (var item in cartData.canceled_items)
                    {
                        if (item.is_printed == 1)
                        {
                            continue;
                        }
                        // Membuat objek CartDetailStrukCustomerTransaction
                        var cartDetail = new CanceledItemStrukCustomerTransaction
                        {
                            cart_detail_id = int.Parse(item.cart_detail_id.ToString()), // Mengonversi string ke int
                            menu_id = item.menu_id,
                            menu_name = item.menu_name,
                            menu_type = item.menu_type,
                            menu_detail_id = item.menu_detail_id,
                            varian = item.varian, // Tidak ada data varian
                            serving_type_id = item.serving_type_id,
                            serving_type_name = item.serving_type_name,
                            discount_id = null, // Tidak ada data discount
                            discount_code = null,
                            discounts_value = null,
                            discounted_price = 0,
                            discounts_is_percent = null,
                            price = item.price, // Mengonversi string ke int
                            total_price = item.price * item.qty, // Total price dihitung dari price * qty
                            subtotal = item.price * item.qty, // Total price dihitung dari price * qty
                            subtotal_price = item.price * item.qty, // Total price dihitung dari price * qty
                            qty = item.qty,
                            note_item = string.IsNullOrEmpty(item.note_item) ? "" : item.note_item,
                            is_ordered = item.is_ordered
                        };

                        // Menambahkan ke cart_details
                        strukCustomerTransaction.data.canceled_items.Add(cartDetail);

                        // Membuat objek KitchenAndBarCartDetails dan menyalin data dari cartDetail
                        var kitchenAndBarCartDetail = new KitchenAndBarCanceledItems
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
                        strukCustomerTransaction.data.kitchenBarCanceledItems.Add(kitchenAndBarCartDetail);
                    }
                }
                

                // Serialisasi ke JSON
                string response = JsonConvert.SerializeObject(strukCustomerTransaction);
                HandleSuccessfulTransaction(response, totalTransactions);

                DialogResult = DialogResult.OK;

                Offline_masterPos offline_MasterPos = new Offline_masterPos();
                offline_MasterPos.DeleteCartFile();

                string saveBillPath = "DT-Cache\\Transaction\\saveBill.data";

                SimplifyAndSaveData(saveBillPath);
                Close();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show(ex.ToString());
            }

        }
        public static void SimplifyAndSaveData(string filePath)
        {
            try
            {
                // 1. Baca file JSON
                string jsonData = File.ReadAllText(filePath);
                JObject data = JObject.Parse(jsonData);

                // 2. Dapatkan array "data"
                JArray transactions = (JArray)data["data"];

                // 3. Iterasi setiap transaksi dan hapus elemen yang tidak diperlukan
                for (int i = transactions.Count - 1; i >= 0; i--) // Iterasi mundur untuk menghindari masalah saat menghapus
                {
                    JObject transaction = (JObject)transactions[i];
                    // Iterasi ke cart_details dan refund_details untuk menghapus field yang tidak dibutuhkan
                    JArray cartDetails = (JArray)transaction["cart_details"];
                    foreach (JObject cartItem in cartDetails)
                    {
                        cartItem["is_printed"] = 1;
                    }

                    // Hapus field yang tidak diperlukan di refund_details jika ada
                    JArray canceledDetails = (JArray)transaction["refund_details"];
                    if (canceledDetails != null && canceledDetails.Count > 0) 
                    {
                        foreach (JObject cancelItem in canceledDetails)
                        {
                            cancelItem["is_printed"] = 1;
                        }
                    }
                }
                // 4. Simpan data yang sudah disederhanakan ke file baru atau file yang sama
                File.WriteAllText(filePath, data.ToString());

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
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
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

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