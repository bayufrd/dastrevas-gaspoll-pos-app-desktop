using System.Data;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using KASIR.Model;
using KASIR.Network;
using KASIR.Printer;
using Newtonsoft.Json;
using Serilog;
namespace KASIR.Komponen
{
    public partial class refund : Form
    {
         
        private List<CartDetailTransaction> item = new List<CartDetailTransaction>();
        private List<RefundModel> refundItems = new List<RefundModel>();
        private readonly string MacAddressKasir;
        private readonly string PinPrinterKasir;
        private readonly string BaseOutletName;
        public bool ReloadDataInBaseForm { get; private set; }
        string idTransaksi, cartId, paymentodNgentod = "";

        private readonly string baseOutlet;
        GetTransactionDetail dataTransaction;
        public refund(string transaksiId)
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
                    lblDetailPayment.Text = "Payment Sebelumnya : " + data.payment_type.ToString();
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

        private void btnCicil_Click(object sender, EventArgs e)
        {
        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cmbRefundType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRefundType = cmbRefundType.SelectedItem.ToString();
            if (selectedRefundType == "Semua")
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