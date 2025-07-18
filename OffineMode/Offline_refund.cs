﻿using System.Globalization;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Printer;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.OfflineMode
{
    public partial class Offline_refund : Form
    {
        private readonly string baseOutlet;
        private string cartId, paymentTypeName = "";
        private DataRefundStruk refundData;

        // Menyimpan data refund ke model sementara
        private readonly List<RefundDetailStruk> refundDetailStruks = new();
        private readonly List<RefundModel> refundItems = new();
        private int TotalRefunded;
        private readonly int Nomortransaks;
        private int isrefundall;

        public Offline_refund(string transaksiId)
        {
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
            btnRefund.Visible = true;
            cartId = transaksiId;
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

        public bool ReloadDataInBaseForm { get; private set; }

        private void CmbPayform_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();

                int verticalMargin = 5;
                string itemText = cmbPayform.GetItemText(cmbPayform.Items[e.Index]);

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black,
                    new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width,
                        e.Bounds.Height - verticalMargin));

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

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black,
                    new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width,
                        e.Bounds.Height - verticalMargin));

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
                    if (cmbPayform.Text == paymentTypeName)
                    {
                        cmbPayform.SelectedIndex = kimak;
                        break;
                    }
                }

                return;
            }

            try
            {
                MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                CacheDataApp form3 = new("Sync");
                Close();
                form3.Show();
            }
            catch (TaskCanceledException ex)
            {
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
                string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";

                // Membaca isi file transaction.data
                string transactionJson = File.ReadAllText(transactionDataPath);
                JObject? transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                // Ambil array data transaksi
                JArray? transactionDetails = transactionData["data"] as JArray;
                JToken? filteredTransaction =
                    transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == cartId);

                if (filteredTransaction != null)
                {
                    cartId = filteredTransaction["transaction_id"]?.ToString() ?? "0";
                    lblDetailPayment.Text = "Payment Sebelumnya : " + filteredTransaction["payment_type_name"] ?? "-";
                    lblCustomerName.Text = filteredTransaction["customer_name"]?.ToString() ?? "-";
                    paymentTypeName = filteredTransaction["payment_type_name"]?.ToString() ?? "-";
                    JArray? cartDetails = filteredTransaction["cart_details"] as JArray;
                    JArray? refundDetails = filteredTransaction["refund_details"] as JArray;
                    LoadDataPaymentType();
                    panel13.Controls.Clear();
                    int totalWidth = panel13.ClientSize.Width;

                    for (int i = 0; i < cartDetails.Count; i++)
                    {
                        JObject items = (JObject)cartDetails[i];

                        if ((int)items["qty"] == 0)
                        {
                            continue; // Skip this iteration if qty is 0
                        }

                        RefundModel refundItem = new()
                        {
                            CartDetailId = int.Parse(items["cart_detail_id"].ToString()),
                            QtyMax = int.Parse(items["qty"].ToString()), // Qty asli (max qty)
                            Qty = 0, // Refund quantity dimulai dengan 0
                            QtyRemaining = int.Parse(items["qty"].ToString()), // QtyRemaining yang bisa di-refund
                            RefundReason = "", // Reason akan diisi nanti
                            menu_id = int.Parse(items["menu_id"].ToString()),
                            menu_name = items["menu_name"].ToString(),
                            menu_type = items["menu_type"].ToString(),
                            menu_detail_id = int.Parse(items["menu_detail_id"].ToString()),
                            menu_detail_name = items["menu_detail_name"].ToString(),
                            price = int.Parse(items["price"].ToString()),
                            serving_type_name = items["serving_type_name"].ToString()
                        };
                        refundItems.Add(refundItem); // Menambahkan item ke dalam refundItems

                        Panel itemPanel = new() { Dock = DockStyle.Top, Height = 60 };

                        Label nameLabel = new()
                        {
                            Text = items["qty"] + "x " + items["menu_name"],
                            Width = (int)(totalWidth * 0.7),
                            TextAlign = ContentAlignment.MiddleLeft
                        };

                        Label variantLabel = new()
                        {
                            Text = "Variant: " + items["varian"] ?? "Normal",
                            Width = (int)(totalWidth * 0.7),
                            TextAlign = ContentAlignment.MiddleLeft,
                            Top = 30
                        };

                        TableLayoutPanel quantityPanel = new()
                        {
                            Width = (int)(totalWidth * 0.3), Height = 30, Dock = DockStyle.Right, ColumnCount = 3
                        };

                        Label quantityLabel = new()
                        {
                            Text = "0", //items["qty"].ToString(),
                            TextAlign = ContentAlignment.MiddleCenter
                        };

                        Label minusButtonLabel = new()
                        {
                            Text = "-",
                            Width = 30,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Font = new Font("Arial", 10, FontStyle.Bold),
                            ForeColor = Color.Red,
                            Cursor = Cursors.Hand
                        };

                        Label plusButtonLabel = new()
                        {
                            Text = "+",
                            Width = 30,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Font = new Font("Arial", 10, FontStyle.Bold),
                            ForeColor = Color.Green,
                            Cursor = Cursors.Hand
                        };

                        // Minus button action (decreasing quantity)
                        minusButtonLabel.MouseClick += (sender, e) =>
                        {
                            int currentQuantity = int.Parse(quantityLabel.Text);
                            int maxQuantity = int.Parse(items["qty"].ToString()); // Max quantity available

                            if (currentQuantity > 0)
                            {
                                currentQuantity -= 1; // Increase quantity
                                quantityLabel.Text = currentQuantity.ToString();

                                // Update refund qty if necessary
                                RefundModel? refundItem = refundItems.FirstOrDefault(r =>
                                    r.CartDetailId == int.Parse(items["cart_detail_id"].ToString()));
                                if (refundItem != null)
                                {
                                    refundItem.Qty -= 1; // Decrease refund qty
                                    refundItem.QtyRemaining += 1; // Increase remaining refundable qty
                                }
                            }
                        };

                        // Plus button action (increasing quantity)
                        plusButtonLabel.MouseClick += (sender, e) =>
                        {
                            int maxQuantity = int.Parse(items["qty"].ToString()); // Max quantity available
                            int currentQuantity = int.Parse(quantityLabel.Text);
                            if (currentQuantity < maxQuantity)
                            {
                                currentQuantity += 1; // Decrease quantity
                                quantityLabel.Text = currentQuantity.ToString();

                                // Update qty in the model
                                RefundModel? refundItem = refundItems.FirstOrDefault(r =>
                                    r.CartDetailId == int.Parse(items["cart_detail_id"].ToString()));
                                if (refundItem != null)
                                {
                                    refundItem.Qty += 1; // Update refund qty
                                    refundItem.QtyRemaining -= 1; // Decrease remaining refundable qty
                                }
                            }
                        };

                        // Update the UI with the item panel
                        quantityPanel.Controls.Add(minusButtonLabel);
                        quantityPanel.Controls.Add(quantityLabel);
                        quantityPanel.Controls.Add(plusButtonLabel);
                        itemPanel.Controls.Add(nameLabel);
                        itemPanel.Controls.Add(variantLabel);
                        itemPanel.Controls.Add(quantityPanel);
                        panel13.Controls.Add(itemPanel);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async void Refundbutton_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedRefundType = cmbRefundType.SelectedItem.ToString();

                if (txtNotes.Text == "")
                {
                    MessageBox.Show("Masukkan alasan", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (cmbRefundType.Text == "Pilih Tipe Refund")
                {
                    MessageBox.Show("Pilih tipe refund", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (cmbPayform.Text == "Pilih Tipe Pengembalian")
                {
                    MessageBox.Show("Pilih tipe pengembalian", "Gaspol", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                bool isAnyItemRefunded = false;
                foreach (RefundModel refundItem in refundItems)
                {
                    if (refundItem.Qty > 0) // Jika qty lebih dari 0, berarti ada item yang di-refund
                    {
                        isAnyItemRefunded = true;
                        break;
                    }
                }

                if (!isAnyItemRefunded && selectedRefundType != "Semua")
                {
                    MessageBox.Show("Kuantitas item yang ingin di refund masih nol", "Refund Items",
                        MessageBoxButtons.OK);
                    return;
                }

                string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";
                string transactionJson = File.ReadAllText(transactionDataPath);
                JObject? transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);
                JArray? transactionDetails = transactionData["data"] as JArray;
                JToken? filteredTransaction =
                    transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == cartId);
                int discounted_peritemPrice = 0;
                if (filteredTransaction != null)
                {
                    int refundpaymenttype = 0;
                    int.TryParse(cmbPayform.SelectedValue?.ToString(), out refundpaymenttype);

                    JArray? cartDetails = filteredTransaction["cart_details"] as JArray;
                    JArray? refundDetails = filteredTransaction["refund_details"] as JArray;

                    if (selectedRefundType == "Semua")
                    {
                        // Update TotalRefunded dengan total harga semua item
                        TotalRefunded = cartDetails.Sum(item => int.Parse(item["total_price"]?.ToString() ?? "0"));
                        int refund_payment_id_all = int.Parse(cmbPayform.SelectedValue.ToString());
                        string refund_payment_name_all = cmbPayform.Text;
                        AddAllItemsToRefundDetails(cartDetails, refundDetails, txtNotes.Text, refundpaymenttype);
                        TotalRefunded = cartDetails.Sum(item => int.Parse(item["total_price"]?.ToString() ?? "0"));
                        foreach (JToken cartItem in cartDetails)
                        {
                            MessageBox.Show(cartItem["qty"]?.ToString());
                            refundDetailStruks.Add(new RefundDetailStruk
                            {
                                cart_detail_id = int.Parse(cartItem["cart_detail_id"]?.ToString() ?? "0"),
                                refund_reason_item = txtNotes.Text,
                                payment_type_name = refund_payment_name_all,
                                qty_refund_item = int.Parse(cartItem["qty"]?.ToString() ?? "0"),
                                total_refund_price = int.Parse(cartItem["total_price"]?.ToString() ?? "0"),
                                menu_name = cartItem["menu_name"]?.ToString() ?? "",
                                varian = cartItem["menu_detail_name"]?.ToString() ?? "",
                                serving_type_name = cartItem["serving_type_name"]?.ToString() ?? "",
                                discount_code = null,
                                discounts_value = 0,
                                discounted_price = 0,
                                discounts_is_percent = null,
                                menu_price = int.Parse(cartItem["price"]?.ToString() ?? "0"),
                                note_item = cartItem["note_item"]?.ToString() ?? ""
                            });
                        }

                        filteredTransaction["is_refund"] = 1;
                        filteredTransaction["total"] = 0;
                        filteredTransaction["subtotal"] = 0;
                        filteredTransaction["is_refund_all"] = 1;
                        filteredTransaction["refund_reason_all"] =
                            txtNotes.Text; // Menggunakan Trim untuk menghilangkan spasi
                        filteredTransaction["refund_payment_name_all"] = refund_payment_name_all;
                        filteredTransaction["refund_payment_id_all"] = refund_payment_id_all;
                        filteredTransaction["refund_created_at_all"] =
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        filteredTransaction["is_refund_all"] = 0;
                        filteredTransaction["is_refund"] = 1;

                        // Process refundItems and add them to refundDetails
                        foreach (RefundModel refundItem in refundItems)
                        {
                            JToken? cartItem = cartDetails.FirstOrDefault(item =>
                                item["cart_detail_id"]?.ToString() == refundItem.CartDetailId.ToString());

                            int discountedPrice = 0;
                            int subtotal_item =
                                int.TryParse(cartItem["subtotal_price"]?.ToString(), out int subTotalItem)
                                    ? subTotalItem
                                    : 0;
                            int quantity = int.TryParse(cartItem["qty"]?.ToString(), out int qty) ? qty : 0;
                            int discountPercent =
                                int.TryParse(cartItem["discounts_is_percent"]?.ToString(), out int discPercent)
                                    ? discPercent
                                    : 0;
                            int discountValue = int.TryParse(cartItem["discounts_value"]?.ToString(), out int discValue)
                                ? discValue
                                : 0;
                            string discountedcode = cartItem["discount_code"]?.ToString();

                            //jika ada diskon
                            //jika peritem
                            if (!string.IsNullOrEmpty(cartItem["discount_id"].ToString()) &&
                                cartItem["discount_id"].ToString() != "0")
                            {
                                int tempTotal;

                                if (discountPercent != 0) // Jika diskon berupa persentase
                                {
                                    // Menghitung nilai diskon berdasarkan persentase
                                    tempTotal = subtotal_item * discountValue / 100;

                                    discountedPrice = tempTotal; // Potongan diskon sesuai persen
                                    discounted_peritemPrice =
                                        discountedPrice / quantity; // Harga per item setelah diskon
                                }
                                else // Jika diskon berupa nilai tetap
                                {
                                    // Mengurangi subtotal dengan diskon nilai tetap
                                    tempTotal = subtotal_item - discountValue;
                                    discountedPrice = subtotal_item - tempTotal;
                                    discounted_peritemPrice =
                                        discountedPrice / quantity; // Harga per item setelah diskon
                                }
                            }

                            if (!string.IsNullOrEmpty(filteredTransaction["subtotal"].ToString()) &&
                                filteredTransaction["discount_id"].ToString() != "0" &&
                                filteredTransaction["discounted_price"].ToString() != "0")
                            {
                                int tempTotal;
                                subtotal_item =
                                    int.TryParse(cartItem["subtotal_price"]?.ToString(), out int subTotalItem1)
                                        ? subTotalItem1
                                        : 0;
                                discountPercent =
                                    int.TryParse(cartItem["discounts_is_percent"]?.ToString(), out int discPercent1)
                                        ? discPercent1
                                        : 0;
                                discountValue =
                                    int.TryParse(cartItem["discounts_value"]?.ToString(), out int discValue1)
                                        ? discValue1
                                        : 0;

                                int refundSum = refundDetails != null && refundDetails.Any()
                                    ? refundDetails.Sum(item => int.Parse(item["qty"]?.ToString() ?? "0"))
                                    : 0;

                                quantity = cartDetails.Sum(item => int.Parse(item["qty"]?.ToString() ?? "0")) +
                                           refundSum;

                                if (discountPercent != 0) // Jika diskon berupa persentase
                                {
                                    // Menghitung nilai diskon berdasarkan persentase
                                    tempTotal = subtotal_item * discountValue / 100;

                                    discountedPrice = tempTotal; // Potongan diskon sesuai persen
                                    discounted_peritemPrice =
                                        discountedPrice / quantity; // Harga per item setelah diskon
                                }
                                else // Jika diskon berupa nilai tetap
                                {
                                    // Mengurangi subtotal dengan diskon nilai tetap
                                    tempTotal = subtotal_item - discountValue;
                                    discountedPrice = subtotal_item - tempTotal;
                                    discounted_peritemPrice =
                                        discountedPrice / quantity; // Harga per item setelah diskon
                                }
                            }

                            if (cartItem != null && refundItem.Qty > 0)
                            {
                                int refundtot = int.Parse(refundItem.Qty.ToString()) *
                                                int.Parse(cartItem["price"].ToString());
                                refundDetails.Add(new JObject
                                {
                                    ["cart_detail_id"] = int.Parse(refundItem.CartDetailId.ToString()),
                                    ["menu_id"] = int.Parse(refundItem.menu_id.ToString()),
                                    ["menu_name"] = refundItem.menu_name ?? "",
                                    ["menu_type"] = refundItem.menu_type ?? "",
                                    ["menu_detail_id"] = int.Parse(refundItem.menu_detail_id.ToString()),
                                    ["menu_detail_name"] = refundItem.menu_detail_name ?? "",
                                    ["price"] = int.Parse(refundItem.price.ToString()),
                                    ["refund_qty"] = int.Parse(refundItem.Qty.ToString()),
                                    ["refund_total"] = int.Parse(refundtot.ToString()),
                                    ["discounted_item_price"] = discounted_peritemPrice,
                                    ["refund_reason_item"] = txtNotes.Text,
                                    ["refund_payment_type_id_item"] = refundpaymenttype,
                                    ["refund_payment_type_name"] = cmbPayform.Text ?? "Tunai",
                                    ["created_at"] =
                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                                    ["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",
                                        CultureInfo.InvariantCulture)
                                });
                                string priceString = cartItem["price"]?.ToString();
                                if (string.IsNullOrEmpty(priceString))
                                {
                                    // Tangani jika harga kosong atau null
                                    priceString = "0"; // Set harga default jika kosong
                                }

                                int price = int.Parse(priceString); // Parsing harga
                                string refund_payment_name_all = cmbPayform.Text;
                                refundDetailStruks.Add(new RefundDetailStruk
                                {
                                    cart_detail_id = refundItem.CartDetailId,
                                    refund_reason_item = txtNotes.Text,
                                    payment_type_name = refund_payment_name_all,
                                    qty_refund_item = refundItem.Qty,
                                    discounted_item_price = discounted_peritemPrice,
                                    total_refund_price =
                                        discounted_peritemPrice != 0
                                            ? (price - discounted_peritemPrice) * refundItem.Qty
                                            : refundItem.Qty * price, // Hitung total refund
                                    menu_name = refundItem.menu_name, // Set default jika null
                                    varian = refundItem.menu_detail_name,
                                    serving_type_name = refundItem.serving_type_name, // Set default jika null
                                    discount_code = discountedcode,
                                    discounts_value = discountValue,
                                    discounted_price =
                                        discounted_peritemPrice != 0 ? discounted_peritemPrice * refundItem.Qty : 0,
                                    discounts_is_percent = discountPercent.ToString(),
                                    menu_price = refundItem.price,
                                    note_item = ""
                                });
                                // If refunded quantity is the entire item quantity, remove the item from cartDetails
                                if (refundItem.QtyRemaining == 0) //Qty == int.Parse(cartItem["qty"].ToString()))
                                {
                                    cartItem["qty"] = 0;
                                    cartItem["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",
                                        CultureInfo.InvariantCulture);
                                    cartItem["subtotal"] = 0;
                                    cartItem["subtotal_price"] = 0;
                                    cartItem["total_price"] = 0;
                                }
                                else
                                {
                                    // Partial refund: update the cart item quantity
                                    int newQuantity = int.Parse(cartItem["qty"].ToString()) - refundItem.Qty;
                                    cartItem["qty"] = newQuantity;

                                    // Update subtotal and total price after partial refund
                                    int newSubtotal = newQuantity * int.Parse(cartItem["price"].ToString());
                                    int pricewithdisc = int.Parse(cartItem["price"].ToString()) -
                                                        discounted_peritemPrice;
                                    int newtotal = newQuantity * pricewithdisc;
                                    cartItem["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",
                                        CultureInfo.InvariantCulture);
                                    cartItem["subtotal"] = newSubtotal;
                                    cartItem["subtotal_price"] = newSubtotal;
                                    cartItem["total_price"] = newtotal;
                                }
                            }
                        }
                    }

                    if (int.Parse(filteredTransaction["is_sent_sync"].ToString()) != null &&
                        int.Parse(filteredTransaction["is_sent_sync"].ToString()) == 1)
                    {
                        filteredTransaction["is_edited_sync"] = 1;
                        filteredTransaction["is_sent_sync"] = 0;
                    }

                    filteredTransaction["updated_at"] =
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    // Pastikan filteredTransaction bertipe JObject
                    JObject transactionObject = filteredTransaction as JObject;
                    // Update is_refund field
                    filteredTransaction["is_refund"] = 1;
                    // Update totals (total, subtotal, refund_total) berdasarkan cartDetails dan refundDetails
                    UpdateTransactionTotals(cartDetails, refundDetails, transactionObject, discounted_peritemPrice);

                    // Save the updated transaction data back to the file
                    File.WriteAllText(transactionDataPath, transactionData.ToString());
                    //MessageBox.Show(transactionData.ToString());
                    // Ambil data outlet dari file DataOutlet.data
                    OutletData outletData = GetOutletData();
                    int transaction_id = 0;
                    int.TryParse(filteredTransaction["transaction_id"]?.ToString(), out transaction_id);

                    int customer_seat = 0;
                    int.TryParse(filteredTransaction["customer_seat"]?.ToString(), out customer_seat);

                    int subtotal = 0;
                    int.TryParse(filteredTransaction["subtotal"]?.ToString(), out subtotal);

                    int total = 0;
                    int.TryParse(filteredTransaction["total"]?.ToString(), out total);

                    int discount_id = 0;
                    int.TryParse(filteredTransaction["discount_id"]?.ToString(), out discount_id);

                    int discounts_value = 0;
                    int.TryParse(filteredTransaction["discounts_value"]?.ToString(), out discounts_value);

                    int customer_cash = 0;
                    int.TryParse(filteredTransaction["customer_cash"]?.ToString(), out customer_cash);

                    int customer_change = 0;
                    int.TryParse(filteredTransaction["customer_change"]?.ToString(), out customer_change);

                    // Membuat DataRefundStruk untuk pencetakan
                    refundData = new DataRefundStruk
                    {
                        transaction_id = transaction_id,
                        receipt_number = filteredTransaction["receipt_number"]?.ToString() ?? "No Receipt",
                        customer_name = filteredTransaction["customer_name"]?.ToString() ?? "Unknown Customer",
                        customer_seat = customer_seat,
                        payment_type = filteredTransaction["payment_type_name"]?.ToString() ?? "Unknown Payment Type",
                        delivery_type = "", // Jenis pengiriman
                        delivery_note = "", // Catatan pengiriman
                        cart_id = transaction_id, // ID keranjang
                        subtotal = subtotal,
                        total = total,
                        discount_id = discount_id,
                        discount_code = filteredTransaction["discount_code"]?.ToString() ?? "",
                        discounts_value = discounts_value,
                        discounts_is_percent =
                            filteredTransaction["discounts_is_percent"]?.ToString() ?? "0", // Default diskon persen/*
                        customer_cash = customer_cash,
                        customer_change = customer_change,
                        invoice_due_date = filteredTransaction["invoice_due_date"]?.ToString() ?? "Unknown Date",
                        is_refund_all = int.Parse(isrefundall.ToString()), // Refund semua
                        refund_reason = txtNotes.Text, // Alasan refund
                        total_refund = int.Parse(TotalRefunded.ToString()),
                        refund_details = refundDetailStruks // Data refund yang di-refund
                    };


                    // Optionally, print the receipt here (Handle print)
                    await HandleSuccessfulTransaction(refundDetailStruks);
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void UpdateTransactionTotals(JArray cartDetails, JArray refundDetails, JObject filteredTransaction,
            int discountedPrice)
        {
            int totalRefund = 0;
            int cartTotal = 0;
            int cartsubTotal = 0;
            int discountedPriced = 0;
            int qtytot = 0;

            // Calculate total refund based on refundDetails
            if (refundDetails != null)
            {
                foreach (JToken refundItem in refundDetails)
                {
                    // Safely parse the price and discounted_item_price values
                    int price = 0;
                    int discountedItemPrice = 0;

                    // Safe parsing with null checks
                    int.TryParse(refundItem["price"]?.ToString() ?? "0", out price);
                    int.TryParse(refundItem["discounted_item_price"]?.ToString() ?? "0", out discountedItemPrice);

                    int refundQty = 0;
                    int.TryParse(refundItem["refund_qty"]?.ToString() ?? "0", out refundQty);

                    int pricefix = price - discountedItemPrice;
                    int refundTotal = pricefix * refundQty;

                    // Update the refund_total in the JSON
                    refundItem["refund_total"] = refundTotal;
                    totalRefund += refundTotal;
                }
            }

            // Update subtotal and total based on cartDetails
            if (cartDetails != null)
            {
                foreach (JToken cartItem in cartDetails)
                {
                    // Safely get total_price or default to 0
                    int totalPrice = 0;
                    int.TryParse(cartItem["total_price"]?.ToString() ?? "0", out totalPrice);
                    cartTotal += totalPrice;

                    // If you need to calculate qty for other purposes
                    int.TryParse(cartItem["qty"]?.ToString() ?? "0", out int cartQty);
                    qtytot += cartQty;
                }
            }

            // Update the refund_total in filteredTransaction
            int existingRefundTotal = 0;
            int.TryParse(filteredTransaction["refund_total"]?.ToString() ?? "0", out existingRefundTotal);

            // Calculate new total refund
            filteredTransaction["total_refund"] = Math.Max(totalRefund, 0); // Ensure refund_total is not less than 0
            TotalRefunded = Math.Max(totalRefund, 0); // Ensure TotalRefunded is not less than 0

            // Safely calculate discountedPriced
            int discountPerItemPrice = 0;
            int.TryParse(filteredTransaction["discounted_peritem_price"]?.ToString() ?? "0", out discountPerItemPrice);
            discountedPriced = qtytot * discountPerItemPrice;
            filteredTransaction["is_refund"] = 1; // Ensure subtotal is not less than 0

            // Update subtotal and total
            filteredTransaction["subtotal"] = Math.Max(cartsubTotal, 0); // Ensure subtotal is not less than 0
            filteredTransaction["total"] = Math.Max(cartTotal, 0); // Ensure total is not less than 0
        }

        private void AddAllItemsToRefundDetails(JArray cartDetails, JArray refundDetails, string refundReason,
            int refundPaymentType)
        {
            foreach (JToken cartItem in cartDetails)
            {
                refundDetails.Add(new JObject
                {
                    ["cart_detail_id"] = cartItem["cart_detail_id"],
                    ["menu_id"] = cartItem["menu_id"],
                    ["menu_name"] = cartItem["menu_name"],
                    ["menu_type"] = cartItem["menu_type"],
                    ["menu_detail_id"] = cartItem["menu_detail_id"],
                    ["menu_detail_name"] =
                        cartItem
                            ["menu_detail_name"], // Perbaiki typo pada "menu_detail_namee" menjadi "menu_detail_name"
                    ["price"] = cartItem["price"],
                    ["refund_qty"] = cartItem["qty"], // Refund semua, jadi ambil qty penuh
                    ["refund_total"] = cartItem["total_price"], // Total harga untuk item
                    ["refund_reason_item"] = refundReason, // Alasan refund
                    ["refund_payment_type_id_item"] = refundPaymentType, // Tipe pembayaran
                    ["note_item"] = cartItem["note_item"], // Tipe pembayaran
                    ["created_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    ["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                });
                cartItem["qty"] = 0;
                cartItem["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public OutletData GetOutletData()
        {
            string outletDataPath = $"DT-Cache\\DataOutlet{baseOutlet}.data";
            string outletJson = File.ReadAllText(outletDataPath);
            OutletData? outletData = JsonConvert.DeserializeObject<OutletData>(outletJson);
            return outletData;
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Method to handle successful transaction
        private async Task HandleSuccessfulTransaction(List<RefundDetailStruk> refundDetailStruks)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(30)); // 30-second timeout

            try
            {
                PrinterModel printerModel = new();

                if (btnRefund != null)
                {
                    btnRefund.Text = "Mencetak...";
                }
                else
                {
                    throw new InvalidOperationException("btnRefund is null");
                }

                if (printerModel != null)
                {
                    // Save print job for potential recovery
                    SaveRefundPrintJobForRecovery(refundData, refundDetailStruks, Nomortransaks);

                    try
                    {
                        await Task.Run(async () =>
                        {
                            await printerModel.PrintModelRefund(refundData, refundDetailStruks, Nomortransaks);
                        }, cts.Token);

                        // If successful, remove the saved print job
                        RemoveSavedRefundPrintJob(Nomortransaks);

                        btnRefund.Text = "Selesai.";
                        btnRefund.Enabled = true;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    catch (OperationCanceledException)
                    {
                        // The operation timed out
                        LoggerUtil.LogWarning("Refund print operation timed out, will retry in background");

                        btnRefund.Text = "Selesai (Print di background)";
                        btnRefund.Enabled = true;
                        DialogResult = DialogResult.OK;
                        Close();

                        // Continue printing in background without blocking UI
                        ThreadPool.QueueUserWorkItem(async _ =>
                        {
                            try
                            {
                                // Use a new instance to avoid any shared state issues
                                PrinterModel backgroundPrinterModel = new();

                                // Retry the print operation in background
                                await backgroundPrinterModel.PrintModelRefund(refundData, refundDetailStruks,
                                    Nomortransaks);

                                // If successful, remove the saved print job
                                RemoveSavedRefundPrintJob(Nomortransaks);
                            }
                            catch (Exception ex)
                            {
                                LoggerUtil.LogError(ex, "Background refund printing failed: {ErrorMessage}",
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
                LoggerUtil.LogError(ex, "An error occurred during refund printing: {ErrorMessage}", ex.Message);

                if (btnRefund != null)
                {
                    btnRefund.Text = "Print Ulang";
                    btnRefund.Enabled = true;
                }
            }
            finally
            {
                cts.Dispose();
            }
        }

        private void SaveRefundPrintJobForRecovery(
            DataRefundStruk refundData,
            List<RefundDetailStruk> refundDetailStruks,
            int transactionNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "Refunds");
                Directory.CreateDirectory(printJobsDir);

                RefundPrintJob refundPrintJob = new()
                {
                    RefundData = refundData,
                    RefundDetails = refundDetailStruks,
                    TransactionNumber = transactionNumber,
                    OutletId = Settings.Default.BaseOutlet,
                    Timestamp = DateTime.Now
                };

                string filename = Path.Combine(printJobsDir,
                    $"RefundPrintJob_{transactionNumber}_{DateTime.Now.Ticks}.json");
                File.WriteAllText(filename, JsonConvert.SerializeObject(refundPrintJob, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to save refund print job for recovery");
            }
        }

        private void RemoveSavedRefundPrintJob(int transactionNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "Refunds");
                if (Directory.Exists(printJobsDir))
                {
                    string pattern = $"RefundPrintJob_{transactionNumber}_*.json";
                    foreach (string file in Directory.GetFiles(printJobsDir, pattern))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to remove saved refund print job");
            }
        }

        private void cmbRefundType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRefundType = cmbRefundType.SelectedItem.ToString();
            if (selectedRefundType == "Semua")
            {
                panel13.Enabled = false;
                isrefundall = 1;
            }
            else
            {
                isrefundall = 0;
                panel13.Enabled = true;
            }
        }

        public class RefundPrintJob
        {
            public DataRefundStruk RefundData { get; set; }
            public List<RefundDetailStruk> RefundDetails { get; set; }
            public int TransactionNumber { get; set; }
            public string OutletId { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}