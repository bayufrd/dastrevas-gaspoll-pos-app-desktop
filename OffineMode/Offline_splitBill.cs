﻿
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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using KASIR.komponen;
using Serilog;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using Polly.Caching;

namespace KASIR.OffineMode
{
    public partial class Offline_splitBill : Form
    {
        private readonly string baseOutlet;
        private DataTable dataTable2;
        private Dictionary<int, int> originalQuantities = new Dictionary<int, int>();
        List<RequestCartModel> cartDetails = new List<RequestCartModel>();
        public bool ReloadDataInBaseForm { get; private set; }
        string cart_id;
        public Offline_splitBill(string cartID)
        {
            cart_id = cartID;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            Openform();
        }

        private async void Openform()
        {
            btnSimpan.Enabled = false;
            btnKeluar.Enabled = false;
            await LoadCart();
            btnSimpan.Enabled = true;
            btnKeluar.Enabled = true;
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }
        public async Task LoadCart()
        {
                try
                {
                    string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";


                    // Check if the response is not empty or null
                    if (File.Exists(cacheFilePath))
                    {
                            string cartJson = File.ReadAllText(cacheFilePath);
                            var cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

                            // Ensure dataModel and its properties are not null
                            if (cartData["cart_details"] != null || cartData["cart_details"].Count() != 0)
                            {
                                var cartDetails = cartData["cart_details"] as JArray;
                                // Set the first cart_detail_id as cart_id
                                var cartDetail = cartDetails.FirstOrDefault();
                                cart_id = cartDetail?["cart_detail_id"].ToString() ?? "null"; // Get first cart_detail_id for cart_id

                                // Initialize the DataTable for the DataGridView
                                DataTable dataTable = new DataTable();
                                dataTable.Columns.Add("MenuID", typeof(string));
                                dataTable.Columns.Add("CartDetailID", typeof(int));
                                dataTable.Columns.Add("Jenis", typeof(string));
                                dataTable.Columns.Add("Menu", typeof(string));
                                dataTable.Columns.Add("Jumlah", typeof(string));
                                dataTable.Columns.Add("Total Harga", typeof(string));
                                dataTable.Columns.Add("Note", typeof(string));
                                dataTable.Columns.Add("Minus", typeof(string));
                                dataTable.Columns.Add("Hasil", typeof(string));
                                dataTable.Columns.Add("Plus", typeof(string));
                                
                                foreach (var menu in cartDetails)
                                {
                                    int quantity = menu["qty"] != null ? (int)menu["qty"] : 0;
                                    decimal price = menu["price"] != null ? decimal.Parse(menu["price"].ToString()) : 0;
                                    string noteItem = menu["note_item"]?.ToString() ?? "";
                                    decimal totalPrice = price * quantity;
                                    string totprice = string.Format("{0:n0},-", totalPrice);

                                    dataTable.Rows.Add(
                                        menu["menu_id"].ToString(),
                                        menu["cart_detail_id"],
                                        menu["serving_type_name"].ToString(),
                                        menu["menu_name"].ToString() + " " + menu["cart_detail_name"],
                                        "x" + menu["qty"] != null ? (int)menu["qty"] : 0,
                                        "Rp " + totprice,
                                        null,
                                        "-",
                                        "0",
                                        "+");

                                    if (!string.IsNullOrEmpty(noteItem))
                                    {
                                        dataTable.Rows.Add(null, null, null, "*catatan : " + noteItem, null, null, null, null, null, null);
                                    }
                                }

                                // Check if dataGridView1 is initialized
                                if (dataGridView1 != null)
                                {
                                    dataGridView1.DataSource = dataTable;
                                    dataTable2 = dataTable.Copy();

                                    // Check if the columns exist before trying to access them
                                    if (dataGridView1.Columns.Contains("MenuID"))
                                    {
                                        dataGridView1.Columns["MenuID"].Visible = false;
                                    }
                                    if (dataGridView1.Columns.Contains("CartDetailID"))
                                    {
                                        dataGridView1.Columns["CartDetailID"].Visible = false;
                                    }
                                    if (dataGridView1.Columns.Contains("Jenis"))
                                    {
                                        dataGridView1.Columns["Jenis"].Visible = false;
                                    }
                                    if (dataGridView1.Columns.Contains("Note"))
                                    {
                                        dataGridView1.Columns["Note"].Visible = false;
                                    }

                                    int minusColumn = dataGridView1.Columns["Minus"].Index;
                                    int plusColumn = dataGridView1.Columns["Plus"].Index;

                                    foreach (DataGridViewRow row in dataGridView1.Rows)
                                    {
                                        if (row.Cells["Jenis"].Value != null) // Check if the row is not a separator row
                                        {
                                            DataGridViewTextBoxCell minusButtonCell = new DataGridViewTextBoxCell();
                                            minusButtonCell.Value = "-";
                                            minusButtonCell.Style.Font = new Font("Arial", 10, FontStyle.Bold);
                                            minusButtonCell.Style.ForeColor = Color.Red;

                                            row.Cells[minusColumn] = minusButtonCell;

                                            DataGridViewTextBoxCell plusButtonCell = new DataGridViewTextBoxCell();
                                            plusButtonCell.Value = "+";
                                            plusButtonCell.Style.Font = new Font("Arial", 10, FontStyle.Bold);
                                            plusButtonCell.Style.ForeColor = Color.Green;

                                            row.Cells[plusColumn] = plusButtonCell;
                                        }
                                    }

                                    for (int rowIndex = 0; rowIndex < dataGridView1.Rows.Count; rowIndex++)
                                    {
                                        var menuValue = dataGridView1.Rows[rowIndex].Cells[3].Value?.ToString();

                                        if (menuValue != null && (menuValue.EndsWith("s") || menuValue.StartsWith("*")))
                                        {
                                            dataGridView1.Rows[rowIndex].Cells[minusColumn].Value = "";
                                            dataGridView1.Rows[rowIndex].Cells[plusColumn].Value = "";
                                        }
                                    }

                                    dataGridView1.ColumnHeadersVisible = false;

                                    dataGridView1.Columns["Minus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                    dataGridView1.Columns["Minus"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                                    dataGridView1.Columns["Plus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                    dataGridView1.Columns["Plus"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                                    dataGridView1.Columns["Hasil"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                    dataGridView1.Columns["Hasil"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                }
                            }
                            else
                            {

                                // Log or handle the case where cart_details is null or dataModel is invalid
                                /*MessageBox.Show("Data not found or in unexpected format.");*/
                                return; // Exit the function as the data format is incorrect
                            }
                        
                    }
                    else
                    {
                        MessageBox.Show("No data 2received from the server.");
                    }
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == dataGridView1.Columns["Minus"].Index || e.ColumnIndex == dataGridView1.Columns["Plus"].Index))
            {
                var cartDetailId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["CartDetailID"].Value);
                var qtyToSplitCell = dataGridView1.Rows[e.RowIndex].Cells["Hasil"]; // Access the QtyToSplit column
                var jumlahCell = dataGridView1.Rows[e.RowIndex].Cells["Jumlah"]; // Access the Jumlah column

                if (!originalQuantities.ContainsKey(cartDetailId))
                {
                    originalQuantities[cartDetailId] = 1;
                }

                int originalQuantity = originalQuantities[cartDetailId];

                // Get the maximum quantity allowed from the Jumlah column
                int maxQuantity = int.Parse(jumlahCell.Value.ToString().Replace("x", ""));

                int currentQuantity;
                if (int.TryParse(qtyToSplitCell.Value?.ToString(), out currentQuantity))
                {
                    if (e.ColumnIndex == dataGridView1.Columns["Minus"].Index)
                    {
                        if (currentQuantity > 0)
                        {
                            currentQuantity--;
                        }
                        else
                        {
                            //MessageBox.Show("Kuantitas telah mencapai batas minimum");
                            return;
                        }
                    }
                    else if (e.ColumnIndex == dataGridView1.Columns["Plus"].Index)
                    {
                        if (currentQuantity < maxQuantity)
                        {
                            currentQuantity++;
                        }
                        else
                        {
                            MessageBox.Show("Kuantitas telah mencapai batas maksimal");
                            return;
                        }
                    }

                    qtyToSplitCell.Value = currentQuantity.ToString();

                    // Update or add the item to the cartDetails list
                    var existingItem = cartDetails.FirstOrDefault(item => int.Parse(item.cart_detail_id) == cartDetailId);
                    if (existingItem != null)
                    {
                        existingItem.qty_to_split = currentQuantity.ToString();
                    }
                    else
                    {
                        cartDetails.Add(new RequestCartModel
                        {
                            cart_detail_id = cartDetailId.ToString(),
                            qty_to_split = currentQuantity.ToString()
                        });
                    }
                }
                else
                {
                    MessageBox.Show("Kuantitas Invalid");
                }
            }
        }


        private async void btnSimpanSplit_ClickAsync(object sender, EventArgs e)
        {
            string source = "DT-Cache\\Transaction\\Cart.data";
            string main_split_cart = "DT-Cache\\Transaction\\Cart_main_split.data";
            string new_cart = "DT-Cache\\Transaction\\Cart.data"; // File baru untuk item yang telah dipecah

            // Salin file Cart.data ke Cart_main_split.data sebagai backup
            File.Copy(source, main_split_cart);
            File.Delete(source); // Menghapus file lama jika tidak diperlukan

            if (cartDetails.Count == 0)
            {
                MessageBox.Show("Kuantitas item yang ingin di split masih nol", "Split Bill", MessageBoxButtons.OK);
                return;
            }

            // Deserialisasi data dari file Cart_main_split
            string cartJson = File.ReadAllText(main_split_cart);
            var cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

            // Membuat list untuk cart details yang telah dipecah
            List<JObject> splitItems = new List<JObject>();

            // Proses item yang dipecah (split) dan simpan ke file cart baru (Cart.data)
            foreach (var cartItem in cartData["cart_details"])
            {
                int cartDetailId = cartItem["cart_detail_id"].ToObject<int>();
                var splitItem = cartDetails.FirstOrDefault(item => item.cart_detail_id == cartDetailId.ToString());

                if (splitItem != null) // Jika item ada dalam cartDetails yang dipecah
                {
                    // Ambil kuantitas baru yang dipecah
                    int newQty = int.Parse(splitItem.qty_to_split);

                    // Update nilai pada cartItem
                    cartItem["qty"] = newQty;
                    cartItem["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    cartItem["total_price"] = newQty * cartItem["price"].ToObject<int>();

                    // Simpan item yang dipecah ke list baru
                    splitItems.Add(cartItem as JObject);
                }
            }

            // Buat file Cart.data baru dengan item yang dipecah saja
            var splitCartData = new JObject
            {
                ["cart_details"] = new JArray(splitItems),
                ["subtotal"] = splitItems.Sum(item => item["total_price"].ToObject<int>()),
                ["total"] = splitItems.Sum(item => item["total_price"].ToObject<int>())
            };

            // Simpan data item yang dipecah ke Cart.data baru
            File.WriteAllText(new_cart, JsonConvert.SerializeObject(splitCartData, Formatting.Indented));

            // Simpan kembali perubahan ke file Cart_main_split
            File.WriteAllText(main_split_cart, JsonConvert.SerializeObject(cartData, Formatting.Indented));
            
        }

    }
}
