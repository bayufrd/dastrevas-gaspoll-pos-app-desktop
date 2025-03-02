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
using System.Threading.Tasks;
using System.Windows.Forms;
using KASIR.komponen;
using Serilog;
using FontAwesome.Sharp;
using System.Net.NetworkInformation;
using KASIR.Printer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static System.Windows.Forms.DataFormats;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using KASIR.OfflineMode;
using System.Transactions;


namespace KASIR.OffineMode
{
    public partial class Offline_listBill : Form
    {
        private readonly string baseOutlet;

        private PrinterModel printerModel; // Pastikan ini telah diinisialisasi dengan benar
        private List<ListBill> Model;
        int nomor = 0;

        public List<CartDetail> cart_details { get; set; }

        // Refund details
        public List<RefundDetail> refund_details { get; set; }

        // Canceled items
        public List<CanceledItem> canceled_items { get; set; }

        public bool ReloadDataInBaseForm { get; private set; }
        public Offline_listBill()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;

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
        public async Task LoadData()
        {
            try
            {
                string transactionDataPath = "DT-Cache\\Transaction\\saveBill.data";
                // Membaca isi file transaction.data
                string transactionJson = File.ReadAllText(transactionDataPath);
                var transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                // Ambil array data transaksi
                var transactionDetails = transactionData["data"] as JArray;

                // Begin Counting Transaction Queue
                int numberQueue = transactionDetails.Count + 1; // Start queue number
                var reversedTransactionDetails = transactionDetails.Reverse().ToList();
/*
                IApiService apiService = new ApiService();
                string response = await apiService.GetListBill("/transaction?outlet_id=", baseOutlet);

                ListBillModel menuModel = JsonConvert.DeserializeObject<ListBillModel>(response);
                List<ListBill> menuList = menuModel.data.ToList();*/
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(int));
                dataTable.Columns.Add("Nama", typeof(string));
                dataTable.Columns.Add("Seat", typeof(string));
                dataTable.Columns.Add("Terakhir Update", typeof(string));
                dataTable.Columns.Add("NumberQueue", typeof(int));


                string format = "dddd, dd MMMM yyyy - HH:mm";
                // Loop through each transaction to fill the DataTable
                foreach (var transaction in reversedTransactionDetails)
                {
                    numberQueue -= 1; // Decrease number for the next entry

                    // Parsing tanggal dari string API dan format ulang
                    DateTime updatedAt;
                    // Parsing tanggal menggunakan format yang sesuai
                    if (DateTime.TryParseExact(transaction["updated_at"]?.ToString(), format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out updatedAt))
                    {
                        // Format ulang menjadi lebih singkat, misalnya "19 Oct 2024, 10:38"
                        string formattedDate = updatedAt.ToString("dd MMM yyyy, HH:mm");
                        dataTable.Rows.Add(
                            transaction["transaction_id"]?.ToString(),
                            numberQueue.ToString() + "." + transaction["customer_name"].ToString() , 
                            transaction["customer_seat"].ToString().ToString(),
                            formattedDate);
                    }
                    else
                    {
                        // Jika parsing gagal, tampilkan seperti apa adanya
                        dataTable.Rows.Add(
                            transaction["transaction_id"]?.ToString(),
                            numberQueue.ToString() + "." + transaction["customer_name"].ToString(),
                            transaction["customer_seat"].ToString().ToString(),
                            transaction["updated_at"].ToString().ToString());
                    }
                }

                dataGridView1.DataSource = dataTable;
                DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
                buttonColumn.HeaderText = "Pilih Bill";
                buttonColumn.Text = "Pilih";
                buttonColumn.FlatStyle = FlatStyle.Flat;
                buttonColumn.UseColumnTextForButtonValue = true; // Displays the "Add to Cart" text on the button
                DataGridViewButtonColumn buttonColumn1 = new DataGridViewButtonColumn();
                buttonColumn1.HeaderText = "Struk";
                buttonColumn1.Text = "Cetak";
                buttonColumn1.FlatStyle = FlatStyle.Flat;
                buttonColumn1.UseColumnTextForButtonValue = true;
                dataGridView1.Columns.Add(buttonColumn);
                dataGridView1.Columns.Add(buttonColumn1);
                if (dataGridView1.DataSource != null)
                {
                    // Pastikan kolom "ID" ada dalam DataGridView sebelum mencoba mengaksesnya
                    if (dataGridView1.Columns.Contains("ID"))
                    {
                        dataGridView1.Columns["ID"].Visible = false;
                    }
                    // Pastikan kolom "ID" ada dalam DataGridView sebelum mencoba mengaksesnya
                    if (dataGridView1.Columns.Contains("NumberQueue"))
                    {
                        dataGridView1.Columns["NumberQueue"].Visible = false;
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
                MessageBox.Show("Gagal tampil data bill  " + ex.Message,"Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1 == null || dataGridView1.Columns.Count <= e.ColumnIndex || e.RowIndex < 0)
            {
                MessageBox.Show("Invalid selection", "Error");
                return;
            }

            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Pilih Bill" && e.RowIndex >= 0)
            {
                var cellValue = dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
                if (cellValue == null)
                {
                    MessageBox.Show("Invalid cell value", "Error");
                    return;
                }

                string selectedId = cellValue.ToString();
                try
                {
                    loadBill(selectedId);

                    DialogResult = DialogResult.OK;

                    Close();

                }
                catch (Exception ex)
                {
                    DialogResult = DialogResult.Cancel;
                    MessageBox.Show("Gagal load keranjang " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    Close();
                }
            }
            else if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Struk" && e.RowIndex >= 0)
            {
                PrinterModel printerModel = new PrinterModel();
                var cellValue = dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
                if (cellValue == null)
                {
                    MessageBox.Show("Invalid cell value", "Error");
                    return;
                }

                int selectedId = Convert.ToInt32(cellValue);

                var antrianCell = dataGridView1.Rows[e.RowIndex].Cells["NumberQueue"].Value;
                int AntrianSaveBill = 0;

                if (antrianCell != null)
                {
                    // Mencari angka sebelum titik menggunakan regex
                    Regex regex = new Regex(@"^\d+"); // Mencocokkan angka di awal string
                    Match match = regex.Match(antrianCell.ToString());

                    if (match.Success)
                    {
                        AntrianSaveBill = int.Parse(match.Value); // Ambil angka yang ditemukan
                    }
                }

                try
                {
                    // Read cart.data to extract cart details and transaction id
                    string cartDataPath = "DT-Cache\\Transaction\\saveBill.data";
                    // Cek apakah file transaction.data ada
                    if (!File.Exists(cartDataPath))
                    {
                        return;
                    }

                    var transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(cartDataPath));

                    // Get the array of transaction data
                    var transactionDetails = transactionData["data"] as JArray;

                    // Filter the transaction based on selectedId
                    var cartData = transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == selectedId.ToString());
                    // Extract cart details from the transaction
                    var cartDetails = cartData["cart_details"] as JArray;

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
                            transaction_id = int.Parse(cartData["transaction_id"]?.ToString()),
                            receipt_number = cartData["receipt_number"]?.ToString(),
                            invoice_due_date = cartData["invoice_due_date"]?.ToString(),
                            customer_name = cartData["customer_name"]?.ToString(),
                            customer_seat = int.Parse(cartData["customer_seat"]?.ToString()),
                            payment_type = cartData["payment_type"]?.ToString(),
                            delivery_type = null,
                            delivery_note = null,
                            cart_id = 0,
                            subtotal = int.Parse(cartData["subtotal"]?.ToString()),
                            total = int.Parse(cartData["total"]?.ToString()),
                            discount_id = 0,
                            discount_code = null,
                            discounts_value = null,
                            discounts_is_percent = null,
                            cart_details = new List<CartDetailStrukCustomerTransaction>(),
                            canceled_items = new List<CanceledItemStrukCustomerTransaction>(),
                            kitchenBarCartDetails = new List<KitchenAndBarCartDetails>(),
                            kitchenBarCanceledItems = new List<KitchenAndBarCanceledItems>(),
                            customer_cash = int.Parse(cartData["customer_cash"]?.ToString()),
                            member_name = null,
                            member_phone_number = null
                        }
                    };
                    foreach (var item in cartDetails)
                    {
                        // Cast the JToken to JObject
                        var cartDetailObject = item as JObject;

                        // Now, check if it is not null and if 'is_ordered' exists
                        if (cartDetailObject != null && cartDetailObject["is_ordered"]?.ToString() == "0")
                        {
                            // Membuat objek CartDetailStrukCustomerTransaction
                            var cartDetail = new CartDetailStrukCustomerTransaction
                            {
                                cart_detail_id = int.Parse(cartDetailObject["cart_detail_id"].ToString()), // Mengonversi string ke int
                                menu_id = int.Parse(cartDetailObject["menu_id"].ToString()), // Mengonversi string ke int
                                menu_name = cartDetailObject["menu_name"]?.ToString(),
                                menu_type = cartDetailObject["menu_type"]?.ToString(),
                                menu_detail_id = int.Parse(cartDetailObject["menu_detail_id"]?.ToString() ?? "0"),
                                varian = cartDetailObject["menu_detail_name"]?.ToString(), // Tidak ada data varian
                                serving_type_id = int.Parse(cartDetailObject["serving_type_id"]?.ToString() ?? "0"),
                                serving_type_name = cartDetailObject["serving_type_name"]?.ToString(),
                                discount_id = null, // Tidak ada data discount
                                discount_code = cartDetailObject["discount_code"]?.ToString(),
                                discounts_value = null,
                                discounted_price = 0,
                                discounts_is_percent = null,
                                price = int.Parse(cartDetailObject["price"]?.ToString() ?? "0"), // Mengonversi string ke int
                                total_price = int.Parse(cartDetailObject["price"]?.ToString() ?? "0") * int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                                subtotal = int.Parse(cartDetailObject["price"]?.ToString() ?? "0") * int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                                subtotal_price = int.Parse(cartDetailObject["price"]?.ToString() ?? "0") * int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                                qty = int.Parse(cartDetailObject["qty"]?.ToString() ?? "0"),
                                note_item = string.IsNullOrEmpty(cartDetailObject["note_item"]?.ToString()) ? "" : cartDetailObject["note_item"].ToString(),
                                is_ordered = int.Parse(cartDetailObject["is_ordered"]?.ToString() ?? "0")
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
                    }

                    // Serialisasi ke JSON
                    string response = JsonConvert.SerializeObject(strukCustomerTransaction);
                    /*IApiService apiService = new ApiService();
                    string response = await apiService.Restruk("/transaction/" + selectedId + "?outlet_id=" + baseOutlet + "&is_struct=1");*/
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        await HandleSuccessfulTransaction(response, AntrianSaveBill);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal cetak ulang struk " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
        }
        private void loadBill(string id)
        {
            try
            {
                // Path untuk file transaction.data
                string saveBillDataPath = "DT-Cache\\Transaction\\saveBill.data";
                // Path untuk file Cart.data yang baru
                string cartPath = "DT-Cache\\Transaction\\Cart.data";

                // Cek apakah file transaction.data ada
                if (!File.Exists(saveBillDataPath))
                {
                    return;
                }

                var transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(saveBillDataPath));

                // Ambil array data transaksi
                var transactionDetails = transactionData["data"] as JArray;

                // Filter transaksi berdasarkan transaction_id
                var filteredTransaction = transactionDetails.FirstOrDefault(t => t["transaction_id"]?.ToString() == id);

                if (filteredTransaction != null)
                {
                    // Ambil cart_details dari transaksi
                    var cartDetails = filteredTransaction["cart_details"] as JArray;

                    // Buat format JSON baru
                    var newTransaction = new JObject
                    {
                        ["transaction_ref"] = filteredTransaction["transaction_ref"]?.ToString(),
                        ["transaction_id"] = int.Parse(filteredTransaction["transaction_id"]?.ToString()),
                        ["receipt_number"] = filteredTransaction["receipt_number"]?.ToString(),
                        ["transaction_ref_split"] = filteredTransaction["transaction_ref_split"]?.ToString(),
                        ["invoice_number"] = filteredTransaction["invoice_number"]?.ToString(),
                        ["invoice_due_date"] = filteredTransaction["invoice_due_date"]?.ToString(),
                        ["payment_type_id"] = int.Parse(filteredTransaction["payment_type_id"]?.ToString()),
                        ["payment_type_name"] = filteredTransaction["payment_type_name"]?.ToString(),
                        ["customer_name"] = filteredTransaction["customer_name"]?.ToString(),
                        ["customer_seat"] = int.Parse(filteredTransaction["customer_seat"]?.ToString()),
                        ["customer_cash"] = int.Parse(filteredTransaction["customer_cash"]?.ToString()),
                        ["customer_change"] = int.Parse(filteredTransaction["customer_change"]?.ToString()),
                        ["subtotal"] = cartDetails.Sum(cart => (int)cart["subtotal_price"]),
                        ["total"] = cartDetails.Sum(cart => (int)cart["total_price"]),
                        ["created_at"] = filteredTransaction["created_at"]?.ToString(),
                        ["updated_at"] = filteredTransaction["updated_at"]?.ToString(),
                        ["deleted_at"] = filteredTransaction["deleted_at"]?.ToString(),
                        ["is_refund"] = filteredTransaction["is_refund"]?.ToString(),
                        ["refund_reason"] = filteredTransaction["refund_reason"]?.ToString(),
                        ["delivery_type"] = filteredTransaction["delivery_type"]?.ToString(),
                        ["delivery_note"] = filteredTransaction["delivery_note"]?.ToString(),
                        ["discount_id"] = int.Parse(filteredTransaction["discount_id"]?.ToString()),
                        ["discount_code"] = filteredTransaction["discount_code"]?.ToString(),
                        ["discounts_value"] = filteredTransaction["discounts_value"]?.ToString(),
                        ["discounts_is_percent"] = filteredTransaction["discounts_is_percent"]?.ToString(),
                        ["member_name"] = filteredTransaction["member_name"]?.ToString(),
                        ["member_phone_number"] = filteredTransaction["member_phone_number"]?.ToString(),
                        ["is_refund_all"] = int.Parse(filteredTransaction["is_refund_all"]?.ToString()),
                        ["refund_reason_all"] = filteredTransaction["refund_reason_all"]?.ToString(),
                        ["refund_payment_id_all"] = 0,
                        ["refund_created_at_all"] = filteredTransaction["refund_created_at_all"]?.ToString(),
                        ["total_refund"] = 0,
                        ["refund_payment_name_all"] = filteredTransaction["refund_payment_name_all"]?.ToString(),
                        ["is_savebill"] = 1,
                        ["is_edited_sync"] = int.Parse(filteredTransaction["is_edited_sync"]?.ToString()),
                        ["is_sent_sync"] = int.Parse(filteredTransaction["is_sent_sync"]?.ToString()),
                        ["cart_details"] = cartDetails,
                        ["refund_details"] = new JArray(),
                        ["canceled_items"] = new JArray()
                    };

                    // Menulis JSON baru ke dalam file baru
                    File.WriteAllText(cartPath, newTransaction.ToString());
                    // Hapus transaksi dari saveBill.data jika berhasil disalin ke Cart.data
                    DeleteTransactionFromSaveBill(id);
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private void DeleteTransactionFromSaveBill(string transactionId)
        {
            try
            {
                // Path untuk file saveBill.data
                string saveBillDataPath = "DT-Cache\\Transaction\\saveBill.data";

                // Cek apakah file saveBill.data ada
                if (!File.Exists(saveBillDataPath))
                {
                    return;
                }

                var transactionData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(saveBillDataPath));

                // Ambil array data transaksi
                var transactionDetails = transactionData["data"] as JArray;

                // Filter transaksi yang tidak memiliki transaction_id yang sesuai
                var remainingTransactions = transactionDetails.Where(t => t["transaction_id"]?.ToString() != transactionId).ToArray();

                // Update data transaksi setelah penghapusan
                transactionData["data"] = new JArray(remainingTransactions);

                // Simpan kembali file saveBill.data yang telah diperbarui
                File.WriteAllText(saveBillDataPath, JsonConvert.SerializeObject(transactionData, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred while deleting transaction: {ErrorMessage}", ex.Message);
            }
        }

        private async Task HandleSuccessfulTransaction(string response, int AntrianSaveBill)
        {
            try
            {
                PrinterModel printerModel = new PrinterModel();

                RestrukModel restrukModel = JsonConvert.DeserializeObject<RestrukModel>(response);

                if (restrukModel == null)
                {
                    throw new InvalidOperationException("Deserialization failed: restrukModel is null");
                }
                DataRestruk data = restrukModel.data;

                if (data == null)
                {
                    throw new InvalidOperationException("Deserialization failed: data is null");
                }
                List<CartDetailRestruk> listCart = data.cart_details;
                List<CanceledItemStrukCustomerRestruk> listCancel = data.canceled_items;

                DataRestruk datas = restrukModel.data;
                List<CartDetailRestruk> cartDetails = datas.cart_details;
                List<CanceledItemStrukCustomerRestruk> canceledItems = datas.canceled_items;

                if (printerModel != null)
                {
                    await Task.Run(() =>
                    {
                        printerModel.PrintModelDataBill(datas, cartDetails, canceledItems, AntrianSaveBill);
                    });
                    //await printerModel.PrintModelDataBill(datas, cartDetails, canceledItems, AntrianSaveBill);
                }
                else
                {
                    throw new InvalidOperationException("printerModel is null");
                }
                DialogResult = DialogResult.OK;

                Close();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
}
}
