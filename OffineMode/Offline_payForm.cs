﻿using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.Printer;
using KASIR.Properties;
using KASIR.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.OfflineMode
{
    public partial class Offline_payForm : Form
    {
        private IInternetService _internetServices;

        private readonly string baseOutlet;
        private readonly int customePrice;
        private readonly List<Button> radioButtonsList = new();
        private string totalCart;
        private string ttl2;
        public string btnPayType;
        private string FooterTextStruk;
        private string transactionId;
        private int totalTransactions, membershipUsingPoint = 0, bonusMember = 1;
        public Member getMember { get; private set; } = new();

        public Offline_payForm(string outlet_id, string cart_id, string total_cart, string ttl1, string seat,
            string name, Offline_masterPos masterPosForm)
        {
            InitializeComponent();
            _internetServices = new InternetService();
            InitializeButtonListeners();

            btnSimpan.Enabled = false;
            baseOutlet = Settings.Default.BaseOutlet;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = true;

            ttl2 = ttl1;
            totalCart = total_cart;
            txtSeat.Text = seat;
            txtNama.Text = name;
            generateRandomFill();
            string cleanedTtl1 = CleanInput(ttl1);
            loadFooterStruct();
            loadCountingStruct();

            customePrice = int.Parse(cleanedTtl1);

            txtJumlahPembayaran.Text = ttl1;

            btnSetPrice1.Text = ttl1;
            SetButtonPrices(customePrice);

            cmbPayform.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayform.DrawMode = DrawMode.OwnerDrawVariable;
            cmbPayform.DrawItem += CmbPayform_DrawItem;

            cmbPayform.ItemHeight = 25;
            LoadDataPaymentType();

            txtCash.Text = CleanInput(totalCart);

            panel8.Visible = false;
            panel13.Visible = false;
            panel14.Visible = false;
            btnDataMember.Visible = false;
            lblPoint.Visible = false;

            txtSeat.KeyPress += txtNumberOnly_KeyPress;
        }
        private string CleanInput(string input)
        {
            return Regex.Replace(input, "[^0-9]", "");
        }
        private void SetButtonPrices(int customePrice)
        {
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
        }
        private void InitializeButtonListeners()
        {
            foreach (Button button in radioButtonsList)
            {
                button.Click += RadioButton_Click;
            }
        }
        private Offline_masterPos MasterPosForm { get; set; }
        public bool KeluarButtonClicked { get; private set; }

        public bool ReloadDataInBaseForm { get; private set; }

        private void txtNumberOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private async void loadFooterStruct()
        {
            string configFooterStruk = "setting\\FooterStruk.data";
            if (File.Exists(configFooterStruk))
            {
                FooterTextStruk = await File.ReadAllTextAsync(configFooterStruk);
            }
            else
            {
                FooterTextStruk = "TERIMAKASIH ATAS KUNJUNGANNYA";
            }
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
                txtNama.Text = name + "DT";
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
        private bool ValidateInputs(out string errorMessage)
        {
            int CashCustomer = int.Parse(CleanInput(txtCash.Text));
            errorMessage = string.Empty;
            // Check if a member has been selected
            if (sButton1.Checked && getMember.member_id == 0)
                return SetErrorMessage("Member Belum Dipilih!", ref errorMessage);

            // Check if the customer name has been entered
            if (string.IsNullOrEmpty(txtNama.Text))
                return SetErrorMessage("Masukan nama pelanggan", ref errorMessage);

            // Validate the seat number
            if (!int.TryParse(txtSeat.Text, out _))
                return SetErrorMessage("Masukan seat pelanggan dengan benar", ref errorMessage);

            // Clean and validate the cash input
            if (string.IsNullOrWhiteSpace(CashCustomer.ToString()))
                return SetErrorMessage("Masukkan harga dengan benar.", ref errorMessage);

            // Validate that fulus can be parsed to an integer
            if (!int.TryParse(CashCustomer.ToString(), out int fulusAmount))
                return SetErrorMessage("Harga tidak valid", ref errorMessage);
            string total = CleanInput(txtJumlahPembayaran.Text);
            // Validate total cart amount
            if (!int.TryParse(total, out int totalCartAmount))
                return SetErrorMessage("Harga gagal diolah", ref errorMessage);

            // Ensure there is enough cash provided
            if (CashCustomer < totalCartAmount)
                return SetErrorMessage("Uang belum cukup", ref errorMessage);

            // Ensure a payment type is selected
            if (cmbPayform.Text == "Pilih Tipe Pembayaran")
                return SetErrorMessage("Pilih tipe Pembayaran", ref errorMessage);

            // Check for cart data path
            string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
            if (!File.Exists(cartDataPath))
                return SetErrorMessage("Keranjang masih kosong/gagal load keranjang", ref errorMessage);

            return true; // All validations passed.
        }
        private bool SetErrorMessage(string message, ref string errorMessage)
        {
            errorMessage = message; // Set the error message
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetButtonState();
            return false; // Validation failed
        }
        private string GetFormattedReceiptNumber()
        {
            string formattedDate = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            return $"DT-{txtNama.Text}-{txtSeat.Text}-{formattedDate}";
        }
        private void WriteJsonFile<T>(string filePath, T data)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
        private async void btnSimpan_Click(object sender, EventArgs e)
        {
            btnSimpan.Enabled = false;
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
                    // Input validation
                    if (!ValidateInputs(out string errorMessage))
                    {
                        return;
                    }

                    string fulus = CleanInput(txtCash.Text);
                    int fulusAmount = int.Parse(fulus);
                    int totalCartAmount = int.Parse(totalCart);
                    int change = fulusAmount - totalCartAmount;

                    string cartDataPath = "DT-Cache\\Transaction\\Cart.data";

                    string cartDataJson = File.ReadAllText(cartDataPath);
                    JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);

                    JArray? cartDetails = cartData["cart_details"] as JArray;

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
                        formattedreceiptMaker = invoiceDate.ToString("yyyyMMdd-HHmmss");
                    }
                    else
                    {
                        formattedreceiptMaker = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    }

                    string receipt_numberfix = GetFormattedReceiptNumber();
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

                    if (sButton1.Checked == true && !string.IsNullOrEmpty(getMember.member_id.ToString()) && ButtonSwitchUsePoint.Checked != true)
                    {
                        if (getMember.member_id > 0)
                        {
                            processMembershipArea(totalCartAmount);
                        }
                    }

                    // Prepare transaction data
                    var transactionData = new
                    {
                        transaction_id = int.Parse(transactionId),
                        receipt_number = receipt_numberfix,
                        transaction_ref = transaction_ref_sent,
                        transaction_ref_split = transaction_ref_splitted,
                        invoice_number =
                            $"INV-{invoiceMaker}{paymentTypedId}",
                        invoice_due_date = invoiceDue,
                        payment_type_id = paymentTypedId,
                        payment_type_name =
                            paymentTypeName,
                        customer_name = txtNama.Text,
                        customer_seat = int.Parse(txtSeat.Text),
                        customer_cash = fulusAmount,
                        customer_change = change,
                        total = totalCartAmount,
                        subtotal = subtotalCart,
                        created_at =
                            receiptMaker ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        deleted_at = (string)null,
                        is_refund = 0,
                        refund_reason = (string)null,
                        delivery_type = (string)null,
                        delivery_note = (string)null,
                        discount_id = discount_idConv,
                        discount_code = discount_codeConv,
                        discounts_value = discounts_valueConv,
                        discounts_is_percent = discounts_is_percentConv,
                        discounted_price = discounted_price1,
                        discounted_peritem_price = discounted_priceperitem,
                        member_id = getMember?.member_id > 0 ? getMember.member_id : (int?)null,
                        member_name = !string.IsNullOrEmpty(getMember?.member_name) ? getMember.member_name : (string)null,
                        member_phone_number = !string.IsNullOrEmpty(getMember?.member_phone_number) ? getMember.member_phone_number : (string)null,
                        member_email = !string.IsNullOrEmpty(getMember?.member_email) ? getMember.member_email : (string)null,
                        member_point = getMember?.member_points > 0 ? getMember.member_points : (int?)null,
                        member_use_point = membershipUsingPoint > 0 ? membershipUsingPoint : (int?)null,
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
                        refund_details = new JArray(),
                        canceled_items = cancelDetails
                    };

                    string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";
                    JArray transactionDataArray = new();
                    if (File.Exists(transactionDataPath))
                    {
                        string existingData = File.ReadAllText(transactionDataPath);
                        JObject? existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                        transactionDataArray = existingTransactions["data"] as JArray ?? new JArray();
                    }

                    transactionDataArray.Add(JToken.FromObject(transactionData));

                    JObject newTransactionData = new() { { "data", transactionDataArray } };
                    WriteJsonFile(transactionDataPath, newTransactionData);

                    //membership
                    if (getMember?.member_id > 0 && !string.IsNullOrEmpty(getMember.member_name.ToString()))
                    {
                        membershipSavingPointCache(transaction_ref_sent);
                    }


                    convertData(fulus, change, paymentTypeName, receipt_numberfix, invoiceDue, discount_idConv,
                        discount_codeConv, discounts_valueConv, discounts_is_percentConv);

                    DialogResult = DialogResult.OK;

                    Offline_masterPos del = new();
                    del.ClearCartFile();
                    Close();
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

        private async void membershipSavingPointCache(string transactionReference)
        {
            try
            {
                int point = getMember.member_points;
                if(ButtonSwitchUsePoint.Checked == true)
                {
                    point = 0;
                }

                var membershipData = new
                {
                    id = getMember.member_id,
                    points = point,
                    outlet_id = baseOutlet,
                    transaction_ref = string.IsNullOrEmpty(transactionReference)
        ? null
        : transactionReference.ToString(),

                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    is_sync = 0
                };

                // Save membership data to membership.data
                string membershipDataPath = "DT-Cache\\Transaction\\membershipSyncPoint.data";
                JArray membershipDataArray = new();
                if (File.Exists(membershipDataPath))
                {
                    // If the membership file exists, read and append the new membership
                    string existingData = File.ReadAllText(membershipDataPath);
                    JObject? existingmemberships = JsonConvert.DeserializeObject<JObject>(existingData);
                    membershipDataArray = existingmemberships["data"] as JArray ?? new JArray();
                }

                // Add new membership
                membershipDataArray.Add(JToken.FromObject(membershipData));

                // Serialize and save back to membership.data
                JObject newmembershipData = new() { { "data", membershipDataArray } };
                File.WriteAllText(membershipDataPath,
                    JsonConvert.SerializeObject(newmembershipData, Formatting.Indented));

                // Update member points in DataMember_Outlet{baseoutlet}.data
                UpdateMemberPointsInOutletFile(getMember.member_id, point);
                if (_internetServices.IsInternetConnected())
                {
                    shiftReport c = new shiftReport();
                    c.SyncmembershipData(membershipDataPath);
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private void UpdateMemberPointsInOutletFile(int memberId, int newPoints)
        {
            string outletMemberDataPath = $"DT-Cache//DataMember_Outlet{baseOutlet}.data";

            try
            {
                // Validasi file ada
                if (!File.Exists(outletMemberDataPath))
                {
                    Console.WriteLine($"File {outletMemberDataPath} tidak ditemukan.");
                    return;
                }

                // Baca dan parse JSON
                string jsonContent = File.ReadAllText(outletMemberDataPath);
                JObject memberData = JObject.Parse(jsonContent);

                if (memberData?["data"] is JArray membersArray)
                {
                    // Gunakan LINQ untuk update
                    var memberToUpdate = membersArray
                        .FirstOrDefault(m => m["member_id"]?.Value<int>() == memberId);

                    if (memberToUpdate != null)
                    {
                        // Force update points dan timestamp
                        memberToUpdate["member_points"] = newPoints;
                        memberToUpdate["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        // Simpan kembali seluruh JSON
                        File.WriteAllText(outletMemberDataPath,
                            memberData.ToString(Formatting.Indented));

                        Console.WriteLine($"Berhasil update points member {memberId} menjadi {newPoints}");
                    }
                    else
                    {
                        Console.WriteLine($"Member dengan ID {memberId} tidak ditemukan.");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex,$"Error updating member points: {ex.Message}");
            }
        }
        private async void processMembershipArea(int totalCartAmount)
        {
            try
            {
                string pathMembershipPathData = $"DT-Cache\\DataMember_Outlet{baseOutlet}.data";
                if (!File.Exists(pathMembershipPathData))
                {
                    string cacheFilePath = $"DT-Cache\\DataMember_Outlet{baseOutlet}.data";

                    IApiService apiServiceNew = new ApiService();
                    string apiResponseNew = await apiServiceNew.GetMember("/membership");

                    var apiMemberList = JsonConvert.DeserializeObject<GetMemberModel>(apiResponseNew)?.data;

                    if (apiMemberList != null)
                    {
                        List<Member> finalMemberList = new List<Member>();
                        Offline_MemberData c = new();
                        c.PopulateMemberList(apiMemberList, finalMemberList, cacheFilePath);

                        File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(new GetMemberModel { data = finalMemberList }));
                    }
                }
                int pointMember = totalCartAmount * bonusMember / 100;
                getMember.member_points += pointMember;
                string json = File.ReadAllText(pathMembershipPathData);
                JObject memberData = JObject.Parse(json);
                JArray memberArray = (JArray)memberData["data"];
                var memberToUpdate = memberArray.FirstOrDefault(m => (int)m["member_id"] == getMember.member_id);

                if (memberToUpdate != null)
                {
                    memberToUpdate["member_points"] = (int)memberToUpdate["member_points"] + pointMember;
                    File.WriteAllText(pathMembershipPathData, memberData.ToString(Formatting.Indented));
                }
                else
                {
                    MessageBox.Show($"Member with ID {getMember.member_id} not found.");
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        private async Task convertData(string fulus, int change, string paymentTypeName, string receipt_numberfix,
            string invoiceDue, int discount_idConv, string discount_codeConv, string discounts_valueConv,
            string discounts_is_percentConv)
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
                CartDataCache? cartData = JsonConvert.DeserializeObject<CartDataCache>(cartDataJson);

                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                int discounts_valueConvIntm = 0;
                if (!string.IsNullOrEmpty(discounts_valueConv) && discounts_valueConv != "null")
                {
                    bool isValidDiscount = int.TryParse(discounts_valueConv, out discounts_valueConvIntm);
                    if (!isValidDiscount)
                    {
                        discounts_valueConvIntm = 0;
                    }
                }

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
                        member_id = getMember?.member_id > 0 ? getMember.member_id : (int?)null,
                        member_name = !string.IsNullOrEmpty(getMember?.member_name) ? getMember.member_name : (string)null,
                        member_phone_number = !string.IsNullOrEmpty(getMember?.member_phone_number) ? getMember.member_phone_number : (string)null,
                        member_email = !string.IsNullOrEmpty(getMember?.member_email) ? getMember.member_email : (string)null,
                        member_point = getMember?.member_points > 0 ? getMember.member_points : (int?)null,
                        member_use_point = membershipUsingPoint > 0 ? membershipUsingPoint : (int?)null,
                    }
                };

                foreach (CartDetail item in cartData.cart_details)
                {
                    CartDetailStrukCustomerTransaction cartDetail = new()
                    {
                        cart_detail_id = int.Parse(item.cart_detail_id),
                        menu_id = item.menu_id,
                        menu_name = item.menu_name,
                        menu_type = item.menu_type,
                        menu_detail_id = item.menu_detail_id,
                        varian = item.menu_detail_name,
                        serving_type_id = item.serving_type_id,
                        serving_type_name = item.serving_type_name,
                        discount_id = int.Parse(item.cart_detail_id),
                        discount_code = item.discount_code?.ToString(),
                        discounts_value = int.Parse(item.discounts_value.ToString()),
                        discounted_price = int.Parse(item.discounted_price.ToString()),
                        discounts_is_percent = int.Parse(item.discounts_is_percent.ToString()),
                        price = item.price,
                        total_price = item.price * item.qty,
                        subtotal = item.price * item.qty,
                        subtotal_price = item.price * item.qty,
                        qty = item.qty,
                        note_item = string.IsNullOrEmpty(item.note_item) ? "" : item.note_item,
                        is_ordered = item.is_ordered
                    };

                    strukCustomerTransaction.data.cart_details.Add(cartDetail);

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

                        strukCustomerTransaction.data.kitchenBarCartDetails.Add(kitchenAndBarCartDetail);
                    }
                }

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
                        using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(30))) // 30-second timeout
                        {
                            try
                            {
                                await Task.Run(async () =>
                                {
                                    PrintJob printJob = new()
                                    {
                                        MenuModel = menuModel,
                                        CartDetails = cartDetails,
                                        KitchenItems = kitchenItems,
                                        BarItems = barItems,
                                        CanceledKitchenItems = canceledKitchenItems,
                                        CanceledBarItems = canceledBarItems,
                                        TransactionNumber = totalTransactions,
                                        FooterText = FooterTextStruk
                                    };

                                    SavePrintJobForRecovery(printJob);

                                    await printerModel.PrintModelPayform(
                                        menuModel, cartDetails, kitchenItems, barItems,
                                        canceledKitchenItems, canceledBarItems,
                                        totalTransactions, FooterTextStruk);

                                    RemoveSavedPrintJob(printJob);
                                }, cts.Token);

                                btnSimpan.Text = "Selesai.";
                            }
                            catch (OperationCanceledException)
                            {
                                MessageBox.Show("Print operation timed out, will retry in background");
                                btnSimpan.Text = "Selesai, print akan dilanjutkan di background.";

                                ThreadPool.QueueUserWorkItem(async _ =>
                                {
                                    try
                                    {
                                        await printerModel.PrintModelPayform(
                                            menuModel, cartDetails, kitchenItems, barItems,
                                            canceledKitchenItems, canceledBarItems,
                                            totalTransactions, FooterTextStruk);
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
                if (btnSimpan != null)
                {
                    btnSimpan.Text = "Print Ulang,";
                    btnSimpan.Enabled = true;
                }
            }
        }
        private void SavePrintJobForRecovery(PrintJob job)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs");
                Directory.CreateDirectory(printJobsDir);

                string filename = Path.Combine(printJobsDir,
                    $"PrintJob_{job.TransactionNumber}_{DateTime.Now.Ticks}.json");
                WriteJsonFile(filename, job);
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
            Button clickedButton = (Button)sender;

            foreach (Button button in radioButtonsList)
            {
                button.BackColor = SystemColors.ControlDark;
            }

            clickedButton.ForeColor = Color.White;
            clickedButton.BackColor = Color.SteelBlue;

            btnPayType = clickedButton.Tag.ToString();
        }
        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Close();
        }

        private void btnSetPrice1_Click(object sender, EventArgs e)
        {
            txtCash.Text = CleanInput(btnSetPrice1.Text);
        }

        private void btnSetPrice2_Click_1(object sender, EventArgs e)
        {
            txtCash.Text = CleanInput(btnSetPrice2.Text);
        }

        private void btnSetPrice3_Click(object sender, EventArgs e)
        {
            txtCash.Text = CleanInput(btnSetPrice3.Text);
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
                int KembalianSekarang = int.Parse(CleanInput(txtCash.Text)) -
                                        int.Parse(CleanInput(ttl2));
                CultureInfo culture = new("id-ID");

                lblKembalian.Text = "CHANGES \n\n" + KembalianSekarang.ToString("C", culture);
            }
            catch (FormatException)
            {
                MessageBox.Show("inputan hanya bisa Numeric");
                if (txtCash.Text.Length > 0)
                {
                    txtCash.Text = txtCash.Text.Substring(0, txtCash.Text.Length - 1);
                    txtCash.SelectionStart = txtCash.Text.Length;
                }

                return;
            }

            txtCash.Text = number.ToString("#,#");
            txtCash.SelectionStart = txtCash.Text.Length;
        }
        // Simpan settings ke file lokal
        public static void SaveMemberBonusSettings(int pointPercentage, string settingsFilePath)
        {
            try
            {
                // Buat objek settings
                var settings = new MemberBonusSettings
                {
                    point_percentage = pointPercentage,
                };

                // Pastikan direktori ada
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));

                // Serialize dan simpan
                string jsonSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, jsonSettings);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error saving member bonus settings: {ex.Message}");
            }
        }
        // Implementasi method loadBonusMember yang diperluas
        private async Task loadBonusMember()
        {
            try
            {
                string settings = "DT-Cache//MembershipSettingsBonus.data";

                if (!_internetServices.IsInternetConnected())
                {
                    await SetDefaultBonusMemberAsync(settings);
                    return;
                }
                IApiService apiService = new ApiService();

                string response = await apiService.GetMember("/membership-bonus-point");

                // Parsing point percentage
                int pointPercentage = ParsePointPercentage(response);

                // Simpan ke local settings
                SaveMemberBonusSettings(pointPercentage, settings);

                bonusMember = pointPercentage;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error in loadBonusMember: {ErrorMessage}", ex.Message);
                string settings = "DT-Cache//MembershipSettingsBonus.data";
                await SetDefaultBonusMemberAsync(settings);
            }
            finally
            {
                UpdateBonusLabel();
            }

            // Method parsing point percentage
            int ParsePointPercentage(string jsonResponse)
            {
                try
                {
                    // Parsing berbagai kemungkinan struktur JSON
                    var jsonObject = JObject.Parse(jsonResponse);

                    // Coba berbagai path
                    int pointPercentage =
                        jsonObject.SelectToken("data.point_percentage")?.Value<int>() ??
                        jsonObject.SelectToken("point_percentage")?.Value<int>() ??
                        1;

                    return pointPercentage > 0 ? pointPercentage : 1;
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, $"Point Percentage Parsing Error: {ex.Message}");
                    return 1;
                }
            }

            // Method untuk set default dengan membaca dari local settings
            async Task SetDefaultBonusMemberAsync(string settings)
            {
                // Ambil dari local settings
                var localSettings = GetMemberBonusSettings(settings);

                // Gunakan point percentage dari local settings
                bonusMember = localSettings.point_percentage;
            }

            void UpdateBonusLabel()
            {
                if (lblBonusMember.InvokeRequired)
                {
                    lblBonusMember.Invoke(new Action(() =>
                    {
                        lblBonusMember.Text = $"Bonus Member {bonusMember}%";
                    }));
                }
                else
                {
                    lblBonusMember.Text = $"Bonus Member {bonusMember}%";
                }
            }
        }
        // Baca settings dari file lokal
        public static MemberBonusSettings GetMemberBonusSettings(string SettingsFilePath)
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return new MemberBonusSettings
                    {
                        point_percentage = 1,
                    };
                }

                string jsonSettings = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<MemberBonusSettings>(jsonSettings);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error reading member bonus settings: {ex.Message}");
                return new MemberBonusSettings
                {
                    point_percentage = 1,
                };
            }
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
                ButtonSwitchUsePoint.Visible = true;
                ButtonSwitchUsePoint.Enabled = false;
                lblUsePoint.Visible = true;
                lblBonusMember.Visible = true;
                loadBonusMember();
            }
            else
            {
                panel8.Visible = false;
                panel13.Visible = false;
                panel14.Visible = false;
                btnDataMember.Visible = false;
                lblPoint.Visible = false;
                txtNama.Enabled = true;
                ButtonSwitchUsePoint.Visible = false;
                ButtonSwitchUsePoint.Enabled = false;
                lblUsePoint.Visible = false;
                lblBonusMember.Visible = false;
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
                        Member selectedMember = listMember.SelectedMember;

                        if (selectedMember != null)
                        {
                            getMember.member_id = selectedMember.member_id;
                            getMember.member_name = selectedMember.member_name;
                            getMember.member_email = selectedMember.member_email;
                            getMember.member_phone_number = selectedMember.member_phone_number;
                            getMember.member_points = selectedMember.member_points;

                            lblNamaMember.Text = getMember.member_name;
                            lblEmailMember.Text = getMember.member_email;
                            lblHPMember.Text = getMember.member_phone_number;
                            decimal points = decimal.Parse(getMember.member_points.ToString());
                            lblPoint.Text = $"Total Point : {points:n0}";

                            txtNama.Text = getMember.member_name;
                            txtNama.Enabled = false;

                            if (getMember.member_points > 0 && getMember.member_points != null)
                            {
                                ButtonSwitchUsePoint.Enabled = true;
                            }
                            else
                            {
                                ButtonSwitchUsePoint.Enabled = false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Member selection was canceled or invalid.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

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

        private void ButtonSwitchUsePoint_CheckedChanged(object sender, EventArgs e)
        {
            if (ButtonSwitchUsePoint.Checked == true)
            {
                int currentAmount = int.Parse(CleanInput(txtJumlahPembayaran.Text.ToString()));
                int shouldPayed = currentAmount - getMember.member_points;

                // Make sure the amount doesn't go negative
                if (shouldPayed < 0) shouldPayed = 0;

                txtJumlahPembayaran.Text = string.Format("Rp. {0:n0},-", shouldPayed);
                totalCart = CleanInput(txtJumlahPembayaran.Text);
                // Update ttl2 for calculations
                ttl2 = string.Format("Rp. {0:n0},-", shouldPayed);

                txtCash.Text = CleanInput(txtJumlahPembayaran.Text);
                decimal points = 0;
                lblPoint.Text = $"Total Point : {points:n0}";
                membershipUsingPoint = getMember.member_points;
            }
            else
            {
                int currentAmount = int.Parse(CleanInput(txtJumlahPembayaran.Text.ToString()));
                int shouldPayed = currentAmount + getMember.member_points;

                txtJumlahPembayaran.Text = string.Format("Rp. {0:n0},-", shouldPayed);
                totalCart = CleanInput(txtJumlahPembayaran.Text);

                // Update ttl2 for calculations
                ttl2 = string.Format("Rp. {0:n0},-", shouldPayed);

                txtCash.Text = CleanInput(txtJumlahPembayaran.Text);
                lblPoint.Text = $"Total Point : {getMember.member_points:n0}";
                membershipUsingPoint = 0;
            }
        }
    }
}