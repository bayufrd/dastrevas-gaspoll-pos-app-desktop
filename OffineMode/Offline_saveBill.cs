﻿using System.Globalization;
using KASIR.Model;
using KASIR.OfflineMode;
using KASIR.Printer;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.OffineMode
{
    public partial class Offline_saveBill : Form
    {
        private readonly string baseOutlet;
        private readonly string name;
        private readonly string seat;
        private int totalTransactions;
        private int transactionId;

        public Offline_saveBill(string cartId, string customerName, string customerSeat)
        {
            baseOutlet = Settings.Default.BaseOutlet;
            name = customerName;
            seat = customerSeat;
            InitializeComponent();
            generateRandomFill();
            baseOutlet = Settings.Default.BaseOutlet;
        }

        public bool ReloadDataInBaseForm { get; private set; }

        private void generateRandomFill()
        {
            Random random = new();

            string[] consonants =
            {
                "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y",
                "z"
            };
            string[] vowels = { "a", "e", "i", "o", "u" };

            string newname = "";

            int nameLength = random.Next(3, 10);
            if (name == "" || seat == null)
            {
                for (int i = 0; i < nameLength; i++)
                {
                    newname += i % 2 == 0
                        ? consonants[random.Next(consonants.Length)]
                        : vowels[random.Next(vowels.Length)];
                }

                newname = char.ToUpper(newname[0]) + newname.Substring(1);

                txtNama.Text = newname + "DT";
                txtSeat.Text = "0";
            }
            else
            {
                txtNama.Text = name;
                txtSeat.Text = seat;
            }
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void loadCountingStruct()
        {
            try
            {
                string transactionFilePath = "DT-Cache\\Transaction\\transaction.data";

                if (File.Exists(transactionFilePath))
                {
                    string transactionJson = File.ReadAllText(transactionFilePath);

                    JObject? transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                    if (transactionData["data"] == null)
                    {
                        totalTransactions = 1;
                        return;
                    }

                    JArray? transactionDetails = transactionData["data"] as JArray;

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
                if (string.IsNullOrEmpty(txtNama.Text))
                {
                    MessageBox.Show("Masukan nama pelanggan", "Gaspol", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (string.IsNullOrEmpty(txtSeat.Text))
                {
                    MessageBox.Show("Masukan seat pelanggan", "Gaspol", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (!int.TryParse(txtSeat.Text.Trim(), out seat))
                {
                    MessageBox.Show("Seat harus berupa angka", "Gaspol", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
                if (!File.Exists(cartDataPath))
                {
                    MessageBox.Show("Keranjang Masih Kosong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }

                loadCountingStruct();
                string cartDataJson = File.ReadAllText(cartDataPath);

                if (string.IsNullOrEmpty(cartDataJson))
                {
                    MessageBox.Show("Data keranjang kosong atau tidak valid", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }

                JObject cartData;
                try
                {
                    cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);
                    if (cartData == null)
                    {
                        MessageBox.Show("Format data keranjang tidak valid", "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        ResetButtonState();
                        return;
                    }
                }
                catch (JsonException ex)
                {
                    LoggerUtil.LogError(ex, "JSON parsing error: {ErrorMessage}", ex.Message);
                    MessageBox.Show("Format data keranjang tidak valid: " + ex.Message, "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }

                // Get the first cart_detail_id to set as transaction_id
                JArray? cartDetails = cartData["cart_details"] as JArray;
                if (cartDetails == null || cartDetails.Count == 0)
                {
                    MessageBox.Show("Tidak ada item dalam keranjang", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }

                if (cartData["canceled_items"] == null)
                {
                    cartData["canceled_items"] = new JArray();
                }

                JArray? cancelDetails = cartData["canceled_items"] as JArray;

                foreach (JToken items in cartDetails)
                {
                    if (items["is_ordered"] != null && (int)items["is_ordered"] == 0)
                    {
                        items["is_ordered"] = 1;
                    }
                }

                string firstCartDetailId = cartDetails?.FirstOrDefault()?["cart_detail_id"]?.ToString();
                if (string.IsNullOrEmpty(firstCartDetailId))
                {
                    MessageBox.Show("ID detail keranjang tidak valid", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }

                transactionId = int.Parse(firstCartDetailId);

                int totalCartAmount = 0;
                if (!int.TryParse(cartData["total"]?.ToString() ?? "0", out totalCartAmount))
                {
                    totalCartAmount = 0; // Default jika konversi gagal
                }

                int subtotalcart = 0;
                if (!int.TryParse(cartData["subtotal"]?.ToString() ?? "0", out subtotalcart))
                {
                    subtotalcart = 0; // Default jika konversi gagal
                }

                string receiptMaker = cartDetails?.FirstOrDefault()?["created_at"]?.ToString() ??
                                      DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string invoiceMaker = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string formattedreceiptMaker;

                DateTime invoiceDate;
                if (DateTime.TryParse(receiptMaker, out invoiceDate))
                {
                    formattedreceiptMaker = invoiceDate.ToString("yyyyMMdd-HHmmss");
                }
                else
                {
                    formattedreceiptMaker = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                }

                string receipt_numberfix = $"DT-{txtNama.Text}-{txtSeat.Text}-{formattedreceiptMaker}";
                string invoiceDue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                string transaction_ref_sent = cartData["transaction_ref"]?.ToString() ?? "";
                string transaction_ref_splitted = cartData["transaction_ref_split"]?.ToString();

                int discount_idc = 0;
                if (!int.TryParse(cartData["discount_id"]?.ToString() ?? "0", out discount_idc))
                {
                    discount_idc = 0;
                }

                string discount_codec = cartData["discount_code"]?.ToString() ?? "";
                string discounts_valuec = cartData["discounts_value"]?.ToString() ?? "0";
                string discounts_is_percentc = cartData["discounts_is_percent"]?.ToString() ?? "0";

                int discounted_pricec = 0;
                if (!int.TryParse(cartData["discounted_price"]?.ToString() ?? "0", out discounted_pricec))
                {
                    discounted_pricec = 0;
                }

                var transactionData = new
                {
                    transaction_id = transactionId,
                    receipt_number = receipt_numberfix,
                    transaction_ref = transaction_ref_sent,
                    transaction_ref_split = transaction_ref_splitted,
                    invoice_number = (string)null,
                    invoice_due_date = (string)null,
                    payment_type_id = 0,
                    payment_type_name = (string)null,
                    customer_name = txtNama.Text,
                    customer_seat = seat,
                    customer_cash = 0,
                    customer_change = 0,
                    total = totalCartAmount,
                    subtotal = subtotalcart,
                    created_at =
                        receiptMaker ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    deleted_at = (string)null,
                    is_refund = 0,
                    refund_reason = (string)null,
                    delivery_type = (string)null,
                    delivery_note = (string)null,
                    discount_id = discount_idc,
                    discount_code = discount_codec,
                    discounts_value = discounts_valuec,
                    discounts_is_percent = discounts_is_percentc,
                    discounted_price = discounted_pricec,
                    member_name = (string)null,
                    member_phone_number = (string)null,
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
                    refund_details = new JArray(),
                    canceled_items = cancelDetails
                };

                string saveBillPath = "DT-Cache\\Transaction\\saveBill.data";
                JArray saveBillDataArray = new();

                if (File.Exists(saveBillPath))
                {
                    try
                    {
                        string existingData = File.ReadAllText(saveBillPath);
                        JObject? existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                        saveBillDataArray = existingTransactions["data"] as JArray ?? new JArray();
                    }
                    catch (Exception ex)
                    {
                        LoggerUtil.LogError(ex, "Error reading saveBill.data: {ErrorMessage}", ex.Message);
                        saveBillDataArray = new JArray();
                    }
                }

                saveBillDataArray.Add(JToken.FromObject(transactionData));

                JObject newTransactionData = new() { { "data", saveBillDataArray } };
                File.WriteAllText(saveBillPath, JsonConvert.SerializeObject(newTransactionData, Formatting.Indented));

                await convertData(receipt_numberfix, invoiceDue);
            }
            catch (FormatException ex)
            {
                LoggerUtil.LogError(ex, "Format error: {ErrorMessage}", ex.Message);
                MessageBox.Show(
                    $"Format data tidak valid. Pastikan semua input angka sudah benar.\nDetail: {ex.Message}", "Gaspol",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetButtonState();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show($"Terjadi kesalahan, silakan coba lagi.\nDetail: {ex.Message}", "Gaspol",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                string cartDataPath = "DT-Cache\\Transaction\\Cart.data";

                if (!File.Exists(cartDataPath))
                {
                    MessageBox.Show("Keranjang Masih Kosong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }

                string cartDataJson = File.ReadAllText(cartDataPath);
                //CartDataCache? cartData = JsonConvert.DeserializeObject<CartDataCache>(cartDataJson);
                // Use more robust deserialization
                CartDataCache cartData = JsonConvert.DeserializeObject<CartDataCache>(cartDataJson, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });

                if (cartData == null)
                {
                    MessageBox.Show("Gagal membaca data keranjang", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetButtonState();
                    return;
                }

                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
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
                        transaction_id = transactionId,
                        receipt_number = receipt_numberfix,
                        invoice_due_date = invoiceDue,
                        customer_name = txtNama.Text,
                        customer_seat = int.Parse(txtSeat.Text),
                        payment_type = null,
                        delivery_type = null,
                        delivery_note = null,
                        cart_id = 0,
                        subtotal = cartData.subtotal,
                        total = cartData.total,
                        discount_id =
                            cartData?.discount_id != 0
                                ? cartData.discount_id
                                : 0, // If discount_id is not 0, use it, else default to 0
                        discount_code =
                            string.IsNullOrEmpty(cartData?.discount_code)
                                ? null
                                : cartData.discount_code, // Set to null if discount_code is empty or null
                        discounts_value =
                            string.IsNullOrEmpty(cartData?.discounts_value)
                                ? null
                                : int.Parse(cartData?.discounts_value), // Set to null if no discount
                        discounts_is_percent =
                            cartData?.discounts_is_percent == null
                                ? null
                                : cartData?.discounts_is_percent, // Set to null if no discount
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

                foreach (CartDetail item in cartData.cart_details)
                {
                    if (item.is_ordered == 1 || item.is_printed == 1)
                    {
                        continue;
                    }

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

                    strukCustomerTransaction.data.cart_details.Add(cartDetail);

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

                    strukCustomerTransaction.data.kitchenBarCartDetails.Add(kitchenAndBarCartDetail);
                }

                if (cartData.canceled_items != null && cartData.canceled_items.Count > 0)
                {
                    foreach (CanceledItem item in cartData.canceled_items)
                    {
                        if (item.is_printed == 1)
                        {
                            continue;
                        }

                        CanceledItemStrukCustomerTransaction cartDetail = new()
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

                        strukCustomerTransaction.data.canceled_items.Add(cartDetail);

                        KitchenAndBarCanceledItems kitchenAndBarCartDetail = new()
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

                        strukCustomerTransaction.data.kitchenBarCanceledItems.Add(kitchenAndBarCartDetail);
                    }
                }


                string response = JsonConvert.SerializeObject(strukCustomerTransaction);
                HandleSuccessfulTransaction(response, totalTransactions);

                DialogResult = DialogResult.OK;

                Offline_masterPos offline_MasterPos = new();
                offline_MasterPos.ClearCartFile();

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
                string jsonData = File.ReadAllText(filePath);
                JObject data = JObject.Parse(jsonData);

                JArray transactions = (JArray)data["data"];

                for (int i = transactions.Count - 1;
                     i >= 0;
                     i--) // Iterasi mundur untuk menghindari masalah saat menghapus
                {
                    JObject transaction = (JObject)transactions[i];
                    JArray cartDetails = (JArray)transaction["cart_details"];
                    foreach (JObject cartItem in cartDetails)
                    {
                        cartItem["is_printed"] = 1;
                    }

                    JArray canceledDetails = (JArray)transaction["refund_details"];
                    if (canceledDetails != null && canceledDetails.Count > 0)
                    {
                        foreach (JObject cancelItem in canceledDetails)
                        {
                            cancelItem["is_printed"] = 1;
                        }
                    }
                }

                File.WriteAllText(filePath, data.ToString());
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async Task HandleSuccessfulTransaction(string response, int AntrianSaveBill)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30)); // 30-second timeout

            try
            {
                PrinterModel printerModel = new();
                GetStrukCustomerTransaction menuModel =
                    JsonConvert.DeserializeObject<GetStrukCustomerTransaction>(response);

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

                List<CartDetailStrukCustomerTransaction> kitchenItems = listCart?.Where(cd =>
                                                                            cd.menu_type == "Makanan" ||
                                                                            cd.menu_type == "Additional Makanan")
                                                                        .ToList() ??
                                                                        new List<CartDetailStrukCustomerTransaction>();

                List<CartDetailStrukCustomerTransaction> barItems = listCart?.Where(cd =>
                                                                        cd.menu_type == "Minuman" ||
                                                                        cd.menu_type == "Additional Minuman")
                                                                    .ToList() ??
                                                                    new List<CartDetailStrukCustomerTransaction>();

                List<CanceledItemStrukCustomerTransaction> canceledKitchenItems = listCanceled?.Where(cd =>
                        cd.menu_type == "Makanan" || cd.menu_type == "Additional Makanan").ToList() ??
                    new List<CanceledItemStrukCustomerTransaction>();

                List<CanceledItemStrukCustomerTransaction> canceledBarItems = listCanceled?.Where(cd =>
                                                                                  cd.menu_type == "Minuman" ||
                                                                                  cd.menu_type == "Additional Minuman")
                                                                              .ToList() ??
                                                                              new List<
                                                                                  CanceledItemStrukCustomerTransaction>();

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
                    SaveSaveBillPrintJobForRecovery(menuModel, kitchenItems, canceledKitchenItems, barItems,
                        canceledBarItems, AntrianSaveBill);

                    try
                    {
                        await Task.Run(async () =>
                        {
                            await printerModel.PrinterModelSimpan(menuModel, kitchenItems, canceledKitchenItems,
                                barItems, canceledBarItems, AntrianSaveBill);
                        }, cts.Token);

                        RemoveSavedSaveBillPrintJob(AntrianSaveBill);

                        btnSimpan.Text = "Selesai.";
                        btnSimpan.Enabled = true;
                    }
                    catch (OperationCanceledException)
                    {
                        LoggerUtil.LogWarning("Save bill print operation timed out, will retry in background");

                        btnSimpan.Text = "Selesai (Print di background)";
                        btnSimpan.Enabled = true;

                        ThreadPool.QueueUserWorkItem(async _ =>
                        {
                            try
                            {
                                PrinterModel backgroundPrinterModel = new();

                                await backgroundPrinterModel.PrinterModelSimpan(menuModel, kitchenItems,
                                    canceledKitchenItems, barItems, canceledBarItems, AntrianSaveBill);

                                RemoveSavedSaveBillPrintJob(AntrianSaveBill);
                            }
                            catch (Exception ex)
                            {
                                LoggerUtil.LogError(ex, "Background save bill printing failed: {ErrorMessage}",
                                    ex.Message);
                            }
                        });
                    }
                }
                else
                {
                    throw new InvalidOperationException("printerModel is null");
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred during save bill printing: {ErrorMessage}", ex.Message);

                if (btnSimpan != null)
                {
                    btnSimpan.Text = "Print Ulang";
                    btnSimpan.Enabled = true;
                }
            }
            finally
            {
                cts.Dispose();
            }
        }

        private void SaveSaveBillPrintJobForRecovery(
            GetStrukCustomerTransaction menuModel,
            List<CartDetailStrukCustomerTransaction> kitchenItems,
            List<CanceledItemStrukCustomerTransaction> canceledKitchenItems,
            List<CartDetailStrukCustomerTransaction> barItems,
            List<CanceledItemStrukCustomerTransaction> canceledBarItems,
            int antrianNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "SaveBills");
                Directory.CreateDirectory(printJobsDir);

                SaveBillPrintJob saveBillPrintJob = new()
                {
                    MenuModel = menuModel,
                    KitchenItems = kitchenItems,
                    CanceledKitchenItems = canceledKitchenItems,
                    BarItems = barItems,
                    CanceledBarItems = canceledBarItems,
                    AntrianNumber = antrianNumber,
                    Timestamp = DateTime.Now
                };

                string filename = Path.Combine(printJobsDir,
                    $"SaveBillPrintJob_{antrianNumber}_{DateTime.Now.Ticks}.json");
                File.WriteAllText(filename, JsonConvert.SerializeObject(saveBillPrintJob, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to save save bill print job for recovery");
            }
        }

        private void RemoveSavedSaveBillPrintJob(int antrianNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "SaveBills");
                if (Directory.Exists(printJobsDir))
                {
                    string pattern = $"SaveBillPrintJob_{antrianNumber}_*.json";
                    foreach (string file in Directory.GetFiles(printJobsDir, pattern))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to remove saved save bill print job");
            }
        }
        private void txtSeat_Enter(object sender, EventArgs e)
        {
            if (txtSeat.Text != null)
            {
                txtSeat.Text = "";
            }
        }

        private void txtNama_Click(object sender, EventArgs e)
        {
            if (txtNama.Text != null)
            {
                txtNama.Text = "";
            }
        }

        public class SaveBillPrintJob
        {
            public GetStrukCustomerTransaction MenuModel { get; set; }
            public List<CartDetailStrukCustomerTransaction> KitchenItems { get; set; }
            public List<CanceledItemStrukCustomerTransaction> CanceledKitchenItems { get; set; }
            public List<CartDetailStrukCustomerTransaction> BarItems { get; set; }
            public List<CanceledItemStrukCustomerTransaction> CanceledBarItems { get; set; }
            public int AntrianNumber { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}