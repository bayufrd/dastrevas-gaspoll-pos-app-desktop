using System.Data;
using KASIR.Model;
using KASIR.Printer;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KASIR.Helper;
using System.Windows.Markup;
using System.Runtime.InteropServices;


namespace KASIR.OfflineMode
{
    public partial class Offline_inputPin : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        private readonly string baseOutlet;

        private readonly int totalTransactions;
        private readonly string transactionId;

        public Offline_inputPin(string id, int urutanRiwayat)
        {
            transactionId = id;
            totalTransactions = urutanRiwayat;
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));


            LoadData(transactionId);
            //dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        public bool ReloadDataInBaseForm { get; private set; }

        private void button11_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            Close();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            Close();
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void btnKonfirmasi_Click(object sender, EventArgs e)
        {
            try
            {
                if (textPin.Text == "" || textPin.Text == null)
                {
                    NotifyHelper.Warning("Masukan pin terlebih dahulu");
                    return;
                }

                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);

                if (textPin.Text == dataOutlet.data.pin.ToString())
                {
                    Close();
                    using (Offline_refund Offline_refund = new(transactionId))
                    {

                        QuestionHelper bg = new(null, null, null, null);
                        Form background = bg.CreateOverlayForm();

                        Offline_refund.Owner = background;

                        background.Show();

                        DialogResult dialogResult = Offline_refund.ShowDialog();

                        background.Dispose();

                        if (dialogResult == DialogResult.OK)
                        {
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                        else
                        {
                            DialogResult = DialogResult.Cancel;
                            Close();
                        }
                    }
                }
                else
                {
                    lblKonfirmasi.Text = "Pin/Password salah!";
                    lblKonfirmasi.ForeColor = Color.Red;
                }
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Autentikasi gagal: "+ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0) // Memastikan kita hanya memformat sel data
            {
                if (e.RowIndex % 2 == 0) // Baris genap
                {
                    e.CellStyle.BackColor = Color.White; // Warna untuk baris genap
                }
                else // Baris ganjil
                {
                    e.CellStyle.BackColor = Color.WhiteSmoke; // Warna untuk baris ganjil
                }
            }
        }

        /// <summary>
        /// test flowlayout panel
        /// </summary>
        /// <param name="transactionId"></param>



        private async void LoadData(string transactionId)
        {
            try
            {
                // Bersihkan kontrol yang ada di FlowLayoutPanel sebelumnya
                flowLayoutPanel1.Controls.Clear();

                string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";

                if (File.Exists(transactionDataPath))
                {
                    string transactionJson = File.ReadAllText(transactionDataPath);
                    JObject? transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                    JArray? transactionDetails = transactionData["data"] as JArray;

                    JToken? filteredTransaction =
                        transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == transactionId);

                    if (filteredTransaction != null)
                    {
                        // Buat panel utama untuk informasi transaksi
                        Panel mainTransactionPanel = CreateTransactionInfoCard(filteredTransaction);
                        flowLayoutPanel1.Controls.Add(mainTransactionPanel);

                        //Informasi Detail
                        Panel panelDetail = CreateSectionHeaderPanel("Informasi Detail");
                        Panel detailTransactionPanel = CreateDetailInfoCard(filteredTransaction);
                        flowLayoutPanel1.Controls.Add(panelDetail);
                        flowLayoutPanel1.Controls.Add(detailTransactionPanel);


                        // Buat panel untuk item terjual
                        JArray? cartDetails = filteredTransaction["cart_details"] as JArray;
                        if (cartDetails != null && cartDetails.Count > 0)
                        {
                            Panel soldItemsHeaderPanel = CreateSectionHeaderPanel("Sold Items");
                            flowLayoutPanel1.Controls.Add(soldItemsHeaderPanel);

                            foreach (JToken item in cartDetails)
                            {
                                if (int.Parse(item["qty"].ToString()) != 0)
                                {
                                    Panel itemPanel = CreateItemCard(item);
                                    flowLayoutPanel1.Controls.Add(itemPanel);
                                }
                            }
                        }

                        // Buat panel untuk item refund
                        JArray? refundDetails = filteredTransaction["refund_details"] as JArray;
                        if (refundDetails != null && refundDetails.Count > 0)
                        {
                            Panel refundHeaderPanel = CreateSectionHeaderPanel("Refund Items");
                            flowLayoutPanel1.Controls.Add(refundHeaderPanel);

                            foreach (JToken refundItem in refundDetails)
                            {
                                Panel refundItemPanel = CreateRefundItemCard(refundItem);
                                flowLayoutPanel1.Controls.Add(refundItemPanel);
                            }
                        }

                    }
                    else
                    {
                        NotifyHelper.Error("Transaksi dengan ID yang ditentukan tidak ditemukan.");
                    }
                }
                else
                {
                    NotifyHelper.Error("File data transaksi tidak ditemukan.");
                }
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal memuat Cart: " + ex.Message + ex.ToString());
                LoggerUtil.LogError(ex, "Terjadi kesalahan: {ErrorMessage}", ex.Message);
            }
        }

        private Panel CreateTransactionInfoCard(JToken transactionData)
        {
            Panel cardPanel = new Panel
            {
                Width = flowLayoutPanel1.Width - 40,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };

            // Tambahkan efek bayangan
            cardPanel.Paint += (sender, e) =>
            {
                using (Pen shadowPen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawRectangle(shadowPen, 0, 0, cardPanel.Width - 1, cardPanel.Height - 1);
                }
            };

            int currentY = 15;

            // Informasi transaksi
            Label lblReceiptNumber = CreateLabel("No. Struk: " +
                (transactionData["receipt_number"]?.ToString() ?? "-"),
                new Font("Segoe UI", 12, FontStyle.Bold));
            lblReceiptNumber.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblReceiptNumber);
            currentY += 30;

            Label lblTransactionTime = CreateLabel(
                DateTime.TryParse(transactionData["created_at"]?.ToString(), out DateTime transactionTime)
                    ? transactionTime.ToString("dd MMM yyyy, HH:mm")
                    : "-",
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblTransactionTime.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblTransactionTime);
            currentY += 30;

            // Helper method untuk parsing decimal dengan penanganan error
            string FormatDecimal(JToken token)
            {
                try
                {
                    // Coba parsing sebagai string, hilangkan koma atau spasi yang tidak diperlukan
                    if (token != null)
                    {
                        string valueStr = token.ToString().Trim().Replace(",", "").Replace(" ", "");
                        if (decimal.TryParse(valueStr, out decimal value))
                        {
                            return value.ToString("N0");
                        }
                    }
                    return "-";
                }
                catch
                {
                    return "-";
                }
            }

            // Helper method untuk parsing integer dengan penanganan error
            string FormatInteger(JToken token)
            {
                try
                {
                    if (token != null)
                    {
                        string valueStr = token.ToString().Trim().Replace(",", "").Replace(" ", "");
                        if (int.TryParse(valueStr, out int value))
                        {
                            return value.ToString("#,#");
                        }
                    }
                    return "-";
                }
                catch
                {
                    return "-";
                }
            }

            // Detail pelanggan
            Label lblCustomerName = CreateLabel("Nama: " +
                (transactionData["customer_name"]?.ToString() ?? "-"),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblCustomerName.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblCustomerName);
            currentY += 30;

            Label lblCustomerSeat = CreateLabel("Tempat Duduk: " +
                (transactionData["customer_seat"]?.ToString() ?? "-"),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblCustomerSeat.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblCustomerSeat);
            currentY += 30;

            // Metode Pembayaran
            Label lblPaymentType = CreateLabel("Metode Pembayaran: " +
                (transactionData["payment_type_name"]?.ToString() ?? "-"),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblPaymentType.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblPaymentType);
            currentY += 30;

            // Total Pembayaran
            Label lblTotal = CreateLabel("Total: Rp " +
                FormatDecimal(transactionData["total"]),
                new Font("Segoe UI", 12, FontStyle.Bold));
            lblTotal.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblTotal);
            currentY += 30;

            // Atur tinggi panel berdasarkan kontrol
            cardPanel.Height = currentY + 20;

            return cardPanel;
        }

        private Panel CreateDetailInfoCard(JToken transactionData)
        {
            Panel cardPanel = new Panel
            {
                Width = flowLayoutPanel1.Width - 40,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };

            // Tambahkan efek bayangan
            cardPanel.Paint += (sender, e) =>
            {
                using (Pen shadowPen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawRectangle(shadowPen, 0, 0, cardPanel.Width - 1, cardPanel.Height - 1);
                }
            };

            int currentY = 15;

            // Helper method untuk parsing decimal dengan penanganan error
            string FormatDecimal(JToken token)
            {
                try
                {
                    // Coba parsing sebagai string, hilangkan koma atau spasi yang tidak diperlukan
                    if (token != null)
                    {
                        string valueStr = token.ToString().Trim().Replace(",", "").Replace(" ", "");
                        if (decimal.TryParse(valueStr, out decimal value))
                        {
                            return value.ToString("N0");
                        }
                    }
                    return "-";
                }
                catch
                {
                    return "-";
                }
            }

            // Helper method untuk parsing integer dengan penanganan error
            string FormatInteger(JToken token)
            {
                try
                {
                    if (token != null)
                    {
                        string valueStr = token.ToString().Trim().Replace(",", "").Replace(" ", "");
                        if (int.TryParse(valueStr, out int value))
                        {
                            return value.ToString("#,#");
                        }
                    }
                    return "-";
                }
                catch
                {
                    return "-";
                }
            }

            // Informasi Diskon
            Label lblDiscountCode = CreateLabel("Kode Diskon: " +
                (transactionData["discount_code"]?.ToString() ?? "-"),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblDiscountCode.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblDiscountCode);
            currentY += 30;

            Label lblDiscountValue = CreateLabel("Nilai Diskon: Rp " +
                FormatDecimal(transactionData["discounts_value"]),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblDiscountValue.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblDiscountValue);
            currentY += 30;

            Label lblDiscountPrice = CreateLabel("Harga Setelah Diskon: Rp " +
                FormatDecimal(transactionData["discounted_price"]),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblDiscountPrice.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblDiscountPrice);
            currentY += 30;

            // Pembayaran Tunai dan Kembalian
            Label lblCustomerCash = CreateLabel("Pembayaran Tunai: Rp " +
                FormatDecimal(transactionData["customer_cash"]),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblCustomerCash.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblCustomerCash);
            currentY += 30;

            Label lblKembalian = CreateLabel("Kembalian: Rp " +
                FormatDecimal(transactionData["customer_change"]),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblKembalian.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblKembalian);
            currentY += 30;

            // Total Refund (jika ada)
            Label lblTotalRefund = CreateLabel("Total Refund: Rp " +
                FormatDecimal(transactionData["total_refund"]),
                new Font("Segoe UI", 10, FontStyle.Regular));
            lblTotalRefund.Location = new Point(15, currentY);
            cardPanel.Controls.Add(lblTotalRefund);
            currentY += 30;

            // Informasi Member (jika ada)
            if (!string.IsNullOrEmpty(transactionData["member_id"]?.ToString()))
            {
                Label lblMemberName = CreateLabel("Nama Member: " +
                    (transactionData["member_name"]?.ToString() ?? "-"),
                    new Font("Segoe UI", 10, FontStyle.Regular));
                lblMemberName.Location = new Point(15, currentY);
                cardPanel.Controls.Add(lblMemberName);
                currentY += 30;

                Label lblMemberPoints = CreateLabel("Poin Member: " +
                    FormatInteger(transactionData["member_point"]),
                    new Font("Segoe UI", 10, FontStyle.Regular));
                lblMemberPoints.Location = new Point(15, currentY);
                cardPanel.Controls.Add(lblMemberPoints);
                currentY += 30;
            }

            cardPanel.Height = currentY + 20;

            return cardPanel;
        }

        private Panel CreateItemCard(JToken item)
        {
            Panel cardPanel = new Panel
            {
                Width = flowLayoutPanel1.Width - 40,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };

            // Tambahkan efek bayangan ringan
            cardPanel.Paint += (sender, e) =>
            {
                using (Pen shadowPen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawRectangle(shadowPen, 0, 0, cardPanel.Width - 1, cardPanel.Height - 1);
                }
            };

            // Nama menu dan detail
            Label lblMenuName = CreateLabel(
                $"{item["qty"]}x {item["menu_name"]} {item["menu_detail_name"]}",
                new Font("Segoe UI", 11, FontStyle.Regular));
            lblMenuName.Location = new Point(15, 15);
            cardPanel.Controls.Add(lblMenuName);

            // Catatan item jika ada
            if (!string.IsNullOrEmpty(item["note_item"]?.ToString()))
            {
                Label lblNote = CreateLabel(
                    $"Catatan: {item["note_item"]}",
                    new Font("Segoe UI", 9, FontStyle.Italic));
                lblNote.Location = new Point(15, 45);
                cardPanel.Controls.Add(lblNote);
            }

            // Harga total
            Label lblTotalPrice = CreateLabel(
                "Rp " + string.Format("{0:N0}", item["total_price"]),
                new Font("Segoe UI", 11, FontStyle.Bold));
            lblTotalPrice.Location = new Point(15,
                (!string.IsNullOrEmpty(item["note_item"]?.ToString()) ? 75 : 45));
            cardPanel.Controls.Add(lblTotalPrice);

            // Atur tinggi panel
            cardPanel.Height = (!string.IsNullOrEmpty(item["note_item"]?.ToString()) ? 110 : 80);

            return cardPanel;
        }

        private Panel CreateRefundItemCard(JToken refundItem)
        {
            Panel cardPanel = new Panel
            {
                Width = flowLayoutPanel1.Width - 40,
                BackColor = Color.FromArgb(255, 240, 240),  // Warna merah muda ringan
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };

            // Nama menu dan detail refund
            Label lblMenuName = CreateLabel(
                $"{refundItem["refund_qty"]}x {refundItem["menu_name"]} {refundItem["menu_detail_name"]}",
                new Font("Segoe UI", 11, FontStyle.Regular));
            lblMenuName.Location = new Point(15, 15);
            cardPanel.Controls.Add(lblMenuName);

            // Alasan refund
            Label lblRefundReason = CreateLabel(
                $"Alasan: {refundItem["refund_reason_item"]}",
                new Font("Segoe UI", 9, FontStyle.Italic));
            lblRefundReason.Location = new Point(15, 45);
            cardPanel.Controls.Add(lblRefundReason);

            // Harga total refund
            Label lblRefundTotal = CreateLabel(
                "Rp " + string.Format("{0:N0}", refundItem["refund_total"]) + " (Refunded)",
                new Font("Segoe UI", 11, FontStyle.Bold));
            lblRefundTotal.Location = new Point(15, 75);
            cardPanel.Controls.Add(lblRefundTotal);

            // Atur tinggi panel
            cardPanel.Height = 110;

            return cardPanel;
        }

        private Panel CreateSectionHeaderPanel(string title)
        {
            Panel headerPanel = new Panel
            {
                Width = flowLayoutPanel1.Width - 40,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240),
                Margin = new Padding(10, 10, 10, 5)
            };

            Label lblHeader = CreateLabel(title, new Font("Segoe UI", 12, FontStyle.Bold));
            lblHeader.Location = new Point(15, 10);
            headerPanel.Controls.Add(lblHeader);

            return headerPanel;
        }

        private Label CreateLabel(string text, Font font)
        {
            return new Label
            {
                Text = text,
                Font = font,
                AutoSize = true,
                ForeColor = Color.Black
            };
        }

        private async void btnCetakStruk_Click(object sender, EventArgs e)
        {
            btnCetakStruk.Text = "Mencetak...";

            string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";

            // Memastikan file ada
            if (!File.Exists(transactionDataPath))
            {
                NotifyHelper.Error("File transaction.data tidak ditemukan.");
                return;
            }

            try
            {
                // Membaca file transaction.data
                string jsonData = File.ReadAllText(transactionDataPath);
                TransactionCache restrukModel = JsonConvert.DeserializeObject<TransactionCache>(jsonData);

                if (restrukModel == null || restrukModel.data == null || restrukModel.data.Count == 0)
                {
                    NotifyHelper.Error("Tidak ada data transaksi di dalam file.");
                    return;
                }

                // Mencari transaksi yang sesuai dengan transaction_id (idid)
                CartDataCache targetTransaksi = restrukModel.data
                    .FirstOrDefault(t =>
                        t.transaction_id == transactionId); // Mencocokkan dengan transactionId yang diberikan

                if (targetTransaksi == null)
                {
                    NotifyHelper.Error("Transaksi dengan ID tersebut tidak ditemukan.");
                    return;
                }

                // Membuat DataRestruk untuk memetakan data dari CartDataCache
                DataRestruk dataRestruk = new()
                {
                    // Pemetaan langsung dari CartDataCache ke DataRestruk
                    transaction_id =
                        int.Parse(targetTransaksi.transaction_id), // Pastikan transaction_id dalam bentuk integer
                    receipt_number = targetTransaksi.receipt_number,
                    invoice_number = targetTransaksi.invoice_number,
                    payment_type = targetTransaksi.payment_type_name,
                    payment_category = targetTransaksi.payment_type_id.ToString(),
                    customer_name = targetTransaksi.customer_name,
                    customer_seat = targetTransaksi.customer_seat,
                    customer_cash = targetTransaksi.customer_cash,
                    total = targetTransaksi.total,
                    subtotal = targetTransaksi.subtotal,
                    discount_id = targetTransaksi.discount_id.ToString(),
                    discount_code = targetTransaksi.discount_code,
                    discounts_value = targetTransaksi.discounts_value,
                    discounts_is_percent = targetTransaksi.discounts_is_percent,
                    invoice_due_date = targetTransaksi.invoice_due_date,
                    cart_details =
                        MapCartDetails(targetTransaksi.cart_details), // Mengonversi CartDetail ke CartDetailRestruk
                    canceled_items =
                        MapCanceledItems(targetTransaksi.canceled_items), // Menyertakan canceled items jika ada
                    refund_details =
                        MapRefundDetails(targetTransaksi.refund_details), // Menyertakan refund details jika ada
                    customer_change = targetTransaksi.customer_change ?? 0, // Handling nullable values
                    refund_reason =
                        targetTransaksi.refund_reason ?? string.Empty // Handling null value for refund_reason
                };
                
                await HandlePrint(dataRestruk, MapCartDetails(targetTransaksi.cart_details), dataRestruk.refund_details,
                    dataRestruk.canceled_items); // Menyertakan refund dan canceled items jika ada
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal membaca atau memproses file transaction.data: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred while processing transaction data: {ErrorMessage}",
                    ex.Message);
            }
            finally
            {
                btnCetakStruk.Text = "Cetak Struk"; // Mengembalikan teks button
            }
        }

        private List<CartDetailRestruk> MapCartDetails(List<CartDetail> cartDetails)
        {
            List<CartDetailRestruk> cartDetailRestruks = new();

            foreach (CartDetail cartDetail in cartDetails)
            {
                cartDetailRestruks.Add(new CartDetailRestruk
                {
                    // Pemetaan langsung dari CartDetail ke CartDetailRestruk
                    cart_detail_id =
                        int.Parse(cartDetail.cart_detail_id), // Pastikan cart_detail_id dalam bentuk integer
                    menu_id = cartDetail.menu_id,
                    menu_name = cartDetail.menu_name,
                    menu_type = cartDetail.menu_type,
                    menu_detail_id = cartDetail.menu_detail_id.ToString(),
                    varian = cartDetail.menu_detail_name,
                    serving_type_id = cartDetail.serving_type_id,
                    serving_type_name = cartDetail.serving_type_name,
                    price = int.Parse(cartDetail.price.ToString()), // Pastikan harga dalam bentuk integer
                    qty = cartDetail.qty,
                    note_item = cartDetail.note_item,
                    discount_id = 0, // Handling nullable values
                    discount_code =
                        cartDetail.discount_code?.ToString() ??
                        string.Empty, // Jika discount_code null, akan digantikan dengan string kosong
                    discounts_value = 0, // Handling nullable values
                    discounted_price = 0, // Handling nullable values
                    discounts_is_percent = cartDetail.discounts_is_percent ?? string.Empty, // Handling nullable values
                    total_price = cartDetail.total_price
                });
            }

            return cartDetailRestruks;
        }

        private List<CanceledItemStrukCustomerRestruk> MapCanceledItems(List<CanceledItem> canceledItems)
        {
            List<CanceledItemStrukCustomerRestruk> canceledItemRestruks = new();

            foreach (CanceledItem canceledItem in canceledItems)
            {
                canceledItemRestruks.Add(new CanceledItemStrukCustomerRestruk
                {
                    cart_detail_id = canceledItem.cart_detail_id,
                    menu_id = canceledItem.menu_id,
                    menu_name = canceledItem.menu_name,
                    menu_type = canceledItem.menu_type,
                    menu_detail_id = canceledItem.menu_detail_id ?? new object(), // Handle nullable
                    varian = canceledItem.varian ?? string.Empty,
                    serving_type_id = canceledItem.serving_type_id,
                    serving_type_name = canceledItem.serving_type_name,
                    discount_id = canceledItem.discount_id,
                    discount_code = canceledItem.discount_code ?? string.Empty,
                    discounts_value = canceledItem.discounts_value ?? new object(),
                    discounted_price = canceledItem.discounted_price ?? 0,
                    discounts_is_percent = canceledItem.discounts_is_percent ?? new object(),
                    price = canceledItem.price,
                    total_price = canceledItem.total_price,
                    qty = canceledItem.qty,
                    note_item = canceledItem.note_item ?? new object(),
                    cancel_reason = canceledItem.cancel_reason ?? string.Empty
                });
            }

            return canceledItemRestruks;
        }

        private List<RefundDetailRestruk> MapRefundDetails(List<RefundDetail> refundDetails)
        {
            List<RefundDetailRestruk> refundDetailRestruks = new();

            foreach (RefundDetail refundDetail in refundDetails)
            {
                refundDetailRestruks.Add(new RefundDetailRestruk
                {
                    cart_detail_id = refundDetail.cart_detail_id,
                    refund_reason_item = refundDetail.refund_reason_item ?? string.Empty,
                    qty_refund_item = refundDetail.qty_refund_item,
                    total_refund_price = refundDetail.total_refund_price,
                    payment_type_name = refundDetail.payment_type_name ?? string.Empty,
                    payment_category_name = refundDetail.payment_category_name ?? string.Empty,
                    menu_name = refundDetail.menu_name ?? string.Empty,
                    varian = refundDetail.varian ?? string.Empty,
                    serving_type_name = refundDetail.serving_type_name ?? string.Empty,
                    discount_code = refundDetail.discount_code ?? string.Empty,
                    discounts_value = refundDetail.discounts_value ?? string.Empty,
                    discounted_price = refundDetail.discounted_price ?? string.Empty,
                    menu_price = refundDetail.menu_price,
                    note_item = refundDetail.note_item ?? string.Empty
                });
            }

            return refundDetailRestruks;
        }

        private async Task HandlePrint(DataRestruk data, List<CartDetailRestruk> cartDetails,
            List<RefundDetailRestruk> cartRefundDetails, List<CanceledItemStrukCustomerRestruk> canceledItems)
        {
            using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(30))) // 30-second timeout
            {
                try
                {
                    PrinterModel printerModel = new();
                    if (printerModel == null)
                    {
                        throw new InvalidOperationException("printerModel is null");
                    }

                    // Save print job details for potential recovery
                    SaveInputPinPrintJobForRecovery(data, cartDetails, cartRefundDetails, canceledItems,
                        totalTransactions);

                    try
                    {
                        Close(); // Menutup form setelah proses selesai

                        // Execute print operation with timeout
                        await Task.Run(async () =>
                        {
                            await printerModel.PrintModelInputPin(data, cartDetails, cartRefundDetails, canceledItems,
                                totalTransactions);
                        }, cts.Token);

                        // If successful, remove the saved print job
                        RemoveSavedInputPinPrintJob(totalTransactions);
                    }
                    catch (OperationCanceledException)
                    {
                        // The operation timed out
                        LoggerUtil.LogWarning("Input pin print operation timed out, will retry in background");

                        // Continue printing in background
                        ThreadPool.QueueUserWorkItem(async _ =>
                        {
                            try
                            {
                                // Use a new instance to avoid any shared state issues
                                PrinterModel backgroundPrinterModel = new();

                                await backgroundPrinterModel.PrintModelInputPin(data, cartDetails, cartRefundDetails,
                                    canceledItems, totalTransactions);

                                // If successful, remove the saved print job
                                RemoveSavedInputPinPrintJob(totalTransactions);
                            }
                            catch (Exception ex)
                            {
                                LoggerUtil.LogError(ex, "Background input pin printing failed: {ErrorMessage}",
                                    ex.Message);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred during input pin printing: {ErrorMessage}", ex.Message);
                    throw; // Rethrow to allow calling code to handle the error
                }
            }
        }

        // Helper methods for input pin print job persistence
        private void SaveInputPinPrintJobForRecovery(
            DataRestruk data,
            List<CartDetailRestruk> cartDetails,
            List<RefundDetailRestruk> cartRefundDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int transactionNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "InputPin");
                Directory.CreateDirectory(printJobsDir);

                InputPinPrintJob inputPinPrintJob = new()
                {
                    Data = data,
                    CartDetails = cartDetails,
                    CartRefundDetails = cartRefundDetails,
                    CanceledItems = canceledItems,
                    TransactionNumber = transactionNumber,
                    Timestamp = DateTime.Now
                };

                string filename = Path.Combine(printJobsDir,
                    $"InputPinPrintJob_{transactionNumber}_{DateTime.Now.Ticks}.json");
                File.WriteAllText(filename, JsonConvert.SerializeObject(inputPinPrintJob, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to save input pin print job for recovery");
            }
        }

        private void RemoveSavedInputPinPrintJob(int transactionNumber)
        {
            try
            {
                string printJobsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrintJobs", "InputPin");
                if (Directory.Exists(printJobsDir))
                {
                    string pattern = $"InputPinPrintJob_{transactionNumber}_*.json";
                    foreach (string file in Directory.GetFiles(printJobsDir, pattern))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to remove saved input pin print job");
            }
        }


        private void texttPin_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
        }

        // Class to store input pin print job information
        private class InputPinPrintJob
        {
            public DataRestruk Data { get; set; }
            public List<CartDetailRestruk> CartDetails { get; set; }
            public List<RefundDetailRestruk> CartRefundDetails { get; set; }
            public List<CanceledItemStrukCustomerRestruk> CanceledItems { get; set; }
            public int TransactionNumber { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}