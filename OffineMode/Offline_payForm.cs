﻿using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Printer;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace KASIR.OfflineMode
{
    public partial class Offline_payForm : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private readonly string baseOutlet;
        private Offline_masterPos _masterPos;
        public string btnPayType;
        private readonly int customePrice;
        private string Kakimu;
        private string namaMember, transactionId;
        private string outletID, cartID;
        private readonly string totalCart;
        private readonly string ttl2;
        private readonly List<Button> radioButtonsList = new();
        private int SelectedId, totalTransactions;

        public Offline_payForm(string outlet_id, string cart_id, string total_cart, string ttl1, string seat,
            string name, Offline_masterPos masterPosForm)
        {
            InitializeComponent();
            btnSimpan.Enabled = false;
            _masterPos = masterPosForm;
            baseOutlet = Settings.Default.BaseOutlet;
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

            customePrice = int.Parse(cleanedTtl1);

            txtJumlahPembayaran.Text = ttl1;

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


            foreach (Button button in radioButtonsList)
            {
                button.Click += RadioButton_Click;
            }

            cmbPayform.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayform.DrawMode = DrawMode.OwnerDrawVariable;
            cmbPayform.DrawItem += CmbPayform_DrawItem;

            cmbPayform.ItemHeight = 25;
            LoadDataPaymentType();

            //auto keisi payment minimum
            txtCash.Text = Regex.Replace(totalCart, "[^0-9]", "");

            panel8.Visible = false;
            panel13.Visible = false;
            panel14.Visible = false;
            btnDataMember.Visible = false;
            lblPoint.Visible = false;
            // Attach the KeyPress event handler
            txtSeat.KeyPress += txtNumberOnly_KeyPress;
        }

        private Offline_masterPos MasterPosForm { get; set; }
        public bool KeluarButtonClicked { get; private set; }

        public bool ReloadDataInBaseForm { get; private set; }

        // KeyPress event handler to allow numbers only
        private void txtNumberOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits and Backspace (to delete characters)
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true; // Prevent non-numeric characters
            }
        }

        private async void loadFooterStruct()
        {
            Kakimu = await File.ReadAllTextAsync("setting\\FooterStruk.data");
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
                    JObject? transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                    // Pastikan data ada di dalam file
                    if (transactionData["data"] == null)
                    {
                        totalTransactions = 1;
                        return;
                    }

                    // Ambil array data transaksi
                    JArray? transactionDetails = transactionData["data"] as JArray;

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

        private void generateRandomFill()
        {
            Random random = new();

            string[] consonants =
            {
                "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y",
                "z"
            };
            string[] vowels = { "a", "e", "i", "o", "u" };

            string name = "";

            int nameLength = random.Next(3, 10);
            if ((txtNama.Text == "") | (txtNama.Text == null))
            {
                for (int i = 0; i < nameLength; i++)
                {
                    name += i % 2 == 0
                        ? consonants[random.Next(consonants.Length)]
                        : vowels[random.Next(vowels.Length)];
                }

                name = char.ToUpper(name[0]) + name.Substring(1);
                txtNama.Text = name + "(Auto)";
                txtSeat.Text = "0";
            }
        }

        private async void LoadDataPaymentType()
        {
            try
            {
                if (File.Exists("DT-Cache" + "\\LoadDataPayment_" + "Outlet_" + baseOutlet + ".data"))
                {
                    string json =
                        File.ReadAllText("DT-Cache" + "\\LoadDataPayment_" + "Outlet_" + baseOutlet + ".data");
                    PaymentTypeModel payment = JsonConvert.DeserializeObject<PaymentTypeModel>(json);
                    List<PaymentType> data = payment.data;
                    cmbPayform.DataSource = data;
                    cmbPayform.DisplayMember = "name";
                    cmbPayform.ValueMember = "id";
                }
                else
                {
                    MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                    CacheDataApp form3 = new("Sync");
                    Close();
                    form3.Show();
                }
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            finally
            {
                btnSimpan.Enabled = true;
            }
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

                    e.Graphics.DrawString(itemText, e.Font, Brushes.Black,
                        new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width,
                            e.Bounds.Height - verticalMargin));

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
                        MessageBox.Show("Masukan nama pelanggan", "Gaspol", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    int seatAmount;
                    if (!int.TryParse(txtSeat.Text, out seatAmount))
                    {
                        MessageBox.Show("Masukan seat pelanggan dengan benar", "Gaspol", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    fulus = Regex.Replace(txtCash.Text, "[^0-9]", "");
                    if (string.IsNullOrWhiteSpace(fulus))
                    {
                        MessageBox.Show("Masukkan harga dengan benar.", "Gaspol", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    int fulusAmount;
                    if (!int.TryParse(fulus, out fulusAmount))
                    {
                        MessageBox.Show("Harga tidak valid.", "Gaspol", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    int totalCartAmount;
                    if (!int.TryParse(totalCart, out totalCartAmount))
                    {
                        MessageBox.Show("Harga gagal diolah.", "Gaspol", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    if (fulusAmount < totalCartAmount)
                    {
                        MessageBox.Show("Uang anda belum cukup.", "Gaspol", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    if (cmbPayform.Text == "Pilih Tipe Pembayaran")
                    {
                        MessageBox.Show("Pilih tipe pembayaran", "Gaspol", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        ResetButtonState();
                        return;
                    }

                    int change = fulusAmount - totalCartAmount;

                    // Read cart.data to extract cart details and transaction id
                    string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
                    if (!File.Exists(cartDataPath))
                    {
                        MessageBox.Show("Keranjang Masih Kosong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetButtonState();
                        return;
                    }

                    string cartDataJson = File.ReadAllText(cartDataPath);
                    JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);

                    // Get the first cart_detail_id to set as transaction_id
                    JArray? cartDetails = cartData["cart_details"] as JArray;

                    // Check if canceled_items exists, if not create it
                    if (cartData["canceled_items"] == null)
                    {
                        cartData["canceled_items"] = new JArray();
                    }

                    JArray? cancelDetails = cartData["canceled_items"] as JArray;

                    string firstCartDetailId = cartDetails?.FirstOrDefault()?["cart_detail_id"].ToString();
                    transactionId = firstCartDetailId;
                    string paymentTypeName = cmbPayform.Text;
                    int paymentTypedId = int.Parse(cmbPayform.SelectedValue.ToString());
                    int subtotalCart = int.Parse(cartData["subtotal"].ToString());
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
                    string transaction_ref_splitted = null;
                    if (!string.IsNullOrEmpty(cartData["transaction_ref_split"]?.ToString()))
                    {
                        transaction_ref_splitted = cartData["transaction_ref_split"]?.ToString();
                    }

                    int edited_sync = 0;
                    int sent_sync = 0;
                    int savebill = 0;
                    if (!string.IsNullOrEmpty(cartData["is_savebill"]?.ToString()) &&
                        int.Parse(cartData["is_savebill"]?.ToString()) == 1)
                    {
                        edited_sync = 1;
                        sent_sync = 0;
                        savebill = 1;
                    }

                    int discount_idConv = cartData["discount_id"]?.ToString() != null
                        ? int.Parse(cartData["discount_id"]?.ToString())
                        : 0;
                    string discount_codeConv = cartData["discount_code"]?.ToString() != null
                        ? cartData["discount_code"]?.ToString()
                        : null;
                    string discounts_valueConv = cartData["discounts_value"]?.ToString() != null
                        ? cartData["discounts_value"]?.ToString()
                        : null; // Null if no discount value
                    string discounts_is_percentConv = cartData["discounts_is_percent"]?.ToString() != null
                        ? cartData["discounts_is_percent"]?.ToString()
                        : null;

                    int qtyTotal = cartDetails.Sum(item => (int)item["qty"]);
                    int discounted_price1 = subtotalCart - totalCartAmount;
                    int discounted_priceperitem = discounted_price1 / int.Parse(qtyTotal.ToString());
                    // Prepare transaction data
                    var transactionData = new
                    {
                        transaction_id = int.Parse(transactionId),
                        receipt_number = receipt_numberfix,
                        transaction_ref = transaction_ref_sent,
                        transaction_ref_split = transaction_ref_splitted,
                        invoice_number =
                            $"INV-{invoiceMaker}{paymentTypedId}", // Custom invoice number with formatted date
                        invoice_due_date = invoiceDue, // Adjust due date as needed
                        payment_type_id = paymentTypedId,
                        payment_type_name =
                            paymentTypeName, // No need for .ToString() if paymentTypeName is already a string
                        customer_name = txtNama.Text,
                        customer_seat = int.Parse(txtSeat.Text),
                        customer_cash = fulusAmount,
                        customer_change = change,
                        total = totalCartAmount,
                        subtotal = subtotalCart, // You can replace this with actual subtotal if available
                        created_at =
                            receiptMaker ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        deleted_at = (string)null, // Ensure deleted_at is null, not a string "null"
                        is_refund = 0,
                        refund_reason = (string)null, // Null if no refund reason
                        delivery_type = (string)null, // Null value for delivery_type
                        delivery_note = (string)null, // Null value for delivery_note
                        discount_id = discount_idConv,
                        discount_code = discount_codeConv,
                        discounts_value = discounts_valueConv,
                        discounts_is_percent = discounts_is_percentConv,
                        discounted_price = discounted_price1,
                        discounted_peritem_price = discounted_priceperitem,
                        member_name = (string)null, // Null if no member name
                        member_phone_number = (string)null, // Null if no member phone number
                        is_refund_all = 0,
                        refund_reason_all = (string)null,
                        refund_payment_id_all = 0,
                        refund_created_at_all = (string)null,
                        total_refund = 0,
                        refund_payment_name_all = (string)null,
                        is_edited_sync = edited_sync,
                        is_sent_sync = sent_sync,
                        is_savebill = savebill,
                        cart_details = cartDetails,
                        refund_details = new JArray(), // Empty array for refund_details
                        canceled_items = cancelDetails // Empty array for canceled_items
                    };

                    // Save transaction data to transaction.data
                    string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";
                    JArray transactionDataArray = new();
                    if (File.Exists(transactionDataPath))
                    {
                        // If the transaction file exists, read and append the new transaction
                        string existingData = File.ReadAllText(transactionDataPath);
                        JObject? existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                        transactionDataArray = existingTransactions["data"] as JArray ?? new JArray();
                    }

                    // Add new transaction
                    transactionDataArray.Add(JToken.FromObject(transactionData));

                    // Serialize and save back to transaction.data
                    JObject newTransactionData = new() { { "data", transactionDataArray } };
                    File.WriteAllText(transactionDataPath,
                        JsonConvert.SerializeObject(newTransactionData, Formatting.Indented));
                    convertData(fulus, change, paymentTypeName, receipt_numberfix, invoiceDue, discount_idConv,
                        discount_codeConv, discounts_valueConv, discounts_is_percentConv);
                    //HandleSuccessfulTransaction(response, fulus);
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show($"Terjadi kesalahan, silakan coba lagi.{ex}", "Gaspol", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ResetButtonState();
            }
        }

        private async Task convertData(string fulus, int change, string paymentTypeName, string receipt_numberfix,
            string invoiceDue, int discount_idConv, string discount_codeConv, string discounts_valueConv,
            string discounts_is_percentConv)
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
                CartDataCache? cartData = JsonConvert.DeserializeObject<CartDataCache>(cartDataJson);

                // Membaca file JSON
                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                int discounts_valueConvIntm = 0;
                if (!string.IsNullOrEmpty(discounts_valueConv) && discounts_valueConv != "null")
                {
                    bool isValidDiscount = int.TryParse(discounts_valueConv, out discounts_valueConvIntm);
                    if (!isValidDiscount)
                    {
                        // Log the issue or set a default value (already set 0 by default)
                        discounts_valueConvIntm = 0;
                    }
                }

                // Konversi ke format GetStrukCustomerTransaction
                GetStrukCustomerTransaction strukCustomerTransaction = new()
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
                        payment_type = paymentTypeName,
                        delivery_type = null,
                        delivery_note = null,
                        cart_id = 0,
                        subtotal = cartData.subtotal,
                        total = cartData.total,
                        discount_id = discount_idConv,
                        discount_code = discount_codeConv,
                        discounts_value = discounts_valueConvIntm,
                        discounts_is_percent = discounts_is_percentConv,
                        cart_details = new List<CartDetailStrukCustomerTransaction>(),
                        canceled_items = new List<CanceledItemStrukCustomerTransaction>(),
                        kitchenBarCartDetails = new List<KitchenAndBarCartDetails>(),
                        kitchenBarCanceledItems = new List<KitchenAndBarCanceledItems>(),
                        customer_cash = int.Parse(fulus),
                        customer_change = change,
                        member_name = null,
                        member_phone_number = null
                    }
                };

                // Mengisi cart_details dan kitchenBarCartDetails
                foreach (CartDetail item in cartData.cart_details)
                {
                    // Membuat objek CartDetailStrukCustomerTransaction
                    CartDetailStrukCustomerTransaction cartDetail = new()
                    {
                        cart_detail_id = int.Parse(item.cart_detail_id), // Mengonversi string ke int
                        menu_id = item.menu_id,
                        menu_name = item.menu_name,
                        menu_type = item.menu_type,
                        menu_detail_id = item.menu_detail_id,
                        varian = item.menu_detail_name, // Tidak ada data varian
                        serving_type_id = item.serving_type_id,
                        serving_type_name = item.serving_type_name,
                        discount_id = int.Parse(item.cart_detail_id), // Tidak ada data discount
                        discount_code = item.discount_code?.ToString(),
                        discounts_value = int.Parse(item.discounts_value.ToString()),
                        discounted_price = int.Parse(item.discounted_price.ToString()),
                        discounts_is_percent = int.Parse(item.discounts_is_percent.ToString()),
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

                    if (item.is_ordered == 0)
                    {
                        KitchenAndBarCartDetails kitchenAndBarCartDetail = new()
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
                }


                // Serialisasi ke JSON
                string response = JsonConvert.SerializeObject(strukCustomerTransaction);
                HandleSuccessfulTransaction(response, fulus);

                DialogResult = DialogResult.OK;

                Offline_masterPos del = new();
                del.ClearCartFile();

                Close();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async Task HandleSuccessfulTransaction(string response, string fulus)
        {
            try
            {
                PrinterModel printerModel = new();
                GetStrukCustomerTransaction menuModel =
                    JsonConvert.DeserializeObject<GetStrukCustomerTransaction>(response);

                if (menuModel != null && menuModel.data != null)
                {
                    DataStrukCustomerTransaction data = menuModel.data;
                    List<CartDetailStrukCustomerTransaction> listCart = data.cart_details;
                    List<KitchenAndBarCartDetails> kitchenBarCart = data.kitchenBarCartDetails;
                    List<KitchenAndBarCanceledItems> kitchenBarCanceled = data.kitchenBarCanceledItems;

                    List<CartDetailStrukCustomerTransaction> cartDetails = data.cart_details;
                    List<KitchenAndBarCartDetails> kitchenItems = kitchenBarCart
                        .Where(cd => cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList();
                    List<KitchenAndBarCartDetails> barItems = kitchenBarCart
                        .Where(cd => cd.menu_type == "Minuman" || cd.menu_type == "Additional Minuman").ToList();
                    List<KitchenAndBarCanceledItems> canceledKitchenItems = kitchenBarCanceled
                        .Where(cd => cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList();
                    List<KitchenAndBarCanceledItems> canceledBarItems = kitchenBarCanceled
                        .Where(cd => cd.menu_type == "Minuman" || cd.menu_type == "Additional Minuman").ToList();

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
                        // Create a CancellationTokenSource with timeout
                        using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(30))) // 30-second timeout
                        {
                            try
                            {
                                // Run the print operation with cancellation support
                                await Task.Run(async () =>
                                {
                                    // Store print job details in case we need to retry later
                                    PrintJob printJob = new()
                                    {
                                        MenuModel = menuModel,
                                        CartDetails = cartDetails,
                                        KitchenItems = kitchenItems,
                                        BarItems = barItems,
                                        CanceledKitchenItems = canceledKitchenItems,
                                        CanceledBarItems = canceledBarItems,
                                        TransactionNumber = totalTransactions,
                                        FooterText = Kakimu
                                    };

                                    // Serialize and save the print job for recovery if needed
                                    SavePrintJobForRecovery(printJob);

                                    // Perform the actual printing with advanced retry logic
                                    await printerModel.PrintModelPayform(
                                        menuModel, cartDetails, kitchenItems, barItems,
                                        canceledKitchenItems, canceledBarItems,
                                        totalTransactions, Kakimu);

                                    // If successful, remove the saved print job
                                    RemoveSavedPrintJob(printJob);
                                }, cts.Token);

                                btnSimpan.Text = "Selesai.";
                            }
                            catch (OperationCanceledException)
                            {
                                // The operation timed out
                                LoggerUtil.LogWarning("Print operation timed out, will retry in background");
                                btnSimpan.Text = "Selesai, print akan dilanjutkan di background.";

                                // Start a background thread that won't block UI
                                ThreadPool.QueueUserWorkItem(async _ =>
                                {
                                    try
                                    {
                                        // Retry the print operation in background with max retries
                                        await printerModel.PrintModelPayform(
                                            menuModel, cartDetails, kitchenItems, barItems,
                                            canceledKitchenItems, canceledBarItems,
                                            totalTransactions, Kakimu);
                                    }
                                    catch (Exception ex)
                                    {
                                        LoggerUtil.LogError(ex, "Background printing failed after timeout");
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("printerModel is null");
                    }
                }
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

        // Helper methods for print job persistence
        private void SavePrintJobForRecovery(PrintJob job)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs");
                Directory.CreateDirectory(printJobsDir);

                string filename = Path.Combine(printJobsDir,
                    $"PrintJob_{job.TransactionNumber}_{DateTime.Now.Ticks}.json");
                File.WriteAllText(filename, JsonConvert.SerializeObject(job));
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to save print job for recovery");
            }
        }

        private void RemoveSavedPrintJob(PrintJob job)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs");
                if (Directory.Exists(printJobsDir))
                {
                    string pattern = $"PrintJob_{job.TransactionNumber}_*.json";
                    foreach (string file in Directory.GetFiles(printJobsDir, pattern))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to remove saved print job");
            }
        }

        private void ResetButtonState()
        {
            btnSimpan.Enabled = true;
            btnSimpan.Text = "Simpan";
            btnSimpan.BackColor = Color.FromArgb(31, 30, 68);
        }


        private void RadioButton_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(RadioButton_Click));

            Button clickedButton = (Button)sender;

            foreach (Button button in radioButtonsList)
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

            Close();
        }

        private void btnSetPrice1_Click(object sender, EventArgs e)
        {
            txtCash.Text = Regex.Replace(btnSetPrice1.Text, "[^0-9]", "");
        }

        private void btnSetPrice2_Click_1(object sender, EventArgs e)
        {
            txtCash.Text = Regex.Replace(btnSetPrice2.Text, "[^0-9]", "");
        }

        private void btnSetPrice3_Click(object sender, EventArgs e)
        {
            txtCash.Text = Regex.Replace(btnSetPrice3.Text, "[^0-9]", "");
        }

        private void txtCash_TextChanged(object sender, EventArgs e)
        {
            if (txtCash.Text == "" || txtCash.Text == "0")
            {
                return;
            }

            decimal number;
            try
            {
                number = decimal.Parse(txtCash.Text, NumberStyles.Currency);
                // Menghitung nilai kembalian
                int KembalianSekarang = int.Parse(Regex.Replace(txtCash.Text, "[^0-9]", "")) -
                                        int.Parse(Regex.Replace(ttl2, "[^0-9]", ""));

                // Mengatur format budaya Indonesia
                CultureInfo culture = new("id-ID");

                // Menampilkan hasil kembalian dalam format mata uang rupiah
                lblKembalian.Text = "CHANGES \n\n" + KembalianSekarang.ToString("C", culture);
            }
            catch (FormatException)
            {
                // The text could not be parsed as a decimal number.
                // You can handle this exception in different ways, such as displaying a message to the user.
                MessageBox.Show("inputan hanya bisa Numeric");
                // Remove the last character from the input if it's invalid
                if (txtCash.Text.Length > 0)
                {
                    txtCash.Text = txtCash.Text.Substring(0, txtCash.Text.Length - 1);
                    txtCash.SelectionStart = txtCash.Text.Length; // Move the cursor to the end
                }

                return;
            }

            txtCash.Text = number.ToString("#,#");
            txtCash.SelectionStart = txtCash.Text.Length;
        }

        private void sButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (sButton1.Checked)
            {
                panel8.Visible = true;
                panel13.Visible = true;
                panel14.Visible = true;
                btnDataMember.Visible = true;
                lblPoint.Visible = true;
                txtNama.Enabled = false;
            }
            else
            {
                panel8.Visible = false;
                panel13.Visible = false;
                panel14.Visible = false;
                btnDataMember.Visible = false;
                lblPoint.Visible = false;
                txtNama.Enabled = true;
            }
        }

        private void btnDataMember_Click(object sender, EventArgs e)
        {
            try
            {
                Form background = new()
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

                using (Offline_MemberData listMember = new())
                {
                    listMember.Owner = background;

                    background.Show();

                    DialogResult result = listMember.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        SelectedId = listMember.SelectedId;
                        lblNamaMember.Text = listMember.namaMember;
                        lblEmailMember.Text = listMember.emailMember;
                        lblHPMember.Text = listMember.hpMember;
                        txtNama.Text = listMember.namaMember;
                        txtNama.Enabled = false;
                        background.Dispose();
                    }
                    else
                    {
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

        private void txtSeat_TextChanged(object sender, EventArgs e)
        {
        }

        // Add this class to store print job information
        public class PrintJob
        {
            public GetStrukCustomerTransaction MenuModel { get; set; }
            public List<CartDetailStrukCustomerTransaction> CartDetails { get; set; }
            public List<KitchenAndBarCartDetails> KitchenItems { get; set; }
            public List<KitchenAndBarCartDetails> BarItems { get; set; }
            public List<KitchenAndBarCanceledItems> CanceledKitchenItems { get; set; }
            public List<KitchenAndBarCanceledItems> CanceledBarItems { get; set; }
            public int TransactionNumber { get; set; }
            public string FooterText { get; set; }
        }
    }
}