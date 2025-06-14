using System.Data;
using System.Globalization;
using KASIR.Model;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.OffineMode
{
    public partial class Offline_splitBill : Form
    {
        private readonly string baseOutlet;
        private string cart_id;
        private readonly List<RequestCartModel> cartDetails = new();
        private DataTable dataTable2;
        private readonly Dictionary<int, int> originalQuantities = new();

        public Offline_splitBill(string cartID)
        {
            cart_id = cartID;
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
            Openform();
        }

        public bool ReloadDataInBaseForm { get; private set; }

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

            Close();
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
                    JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

                    // Ensure dataModel and its properties are not null
                    if (cartData["cart_details"] != null || cartData["cart_details"].Count() != 0)
                    {
                        JArray? cartDetails = cartData["cart_details"] as JArray;
                        // Set the first cart_detail_id as cart_id
                        JToken? cartDetail = cartDetails.FirstOrDefault();
                        cart_id = cartDetail?["cart_detail_id"].ToString() ??
                                  "null"; // Get first cart_detail_id for cart_id

                        // Initialize the DataTable for the DataGridView
                        DataTable dataTable = new();
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

                        foreach (JToken menu in cartDetails)
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
                                menu["menu_name"] + " " + menu["cart_detail_name"],
                                "x" + menu["qty"] != null ? (int)menu["qty"] : 0,
                                "Rp " + totprice,
                                null,
                                "-",
                                "0",
                                "+");

                            if (!string.IsNullOrEmpty(noteItem))
                            {
                                dataTable.Rows.Add(null, null, null, "*catatan : " + noteItem, null, null, null, null,
                                    null, null);
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
                                    DataGridViewTextBoxCell minusButtonCell = new();
                                    minusButtonCell.Value = "-";
                                    minusButtonCell.Style.Font = new Font("Arial", 10, FontStyle.Bold);
                                    minusButtonCell.Style.ForeColor = Color.Red;

                                    row.Cells[minusColumn] = minusButtonCell;

                                    DataGridViewTextBoxCell plusButtonCell = new();
                                    plusButtonCell.Value = "+";
                                    plusButtonCell.Style.Font = new Font("Arial", 10, FontStyle.Bold);
                                    plusButtonCell.Style.ForeColor = Color.Green;

                                    row.Cells[plusColumn] = plusButtonCell;
                                }
                            }

                            for (int rowIndex = 0; rowIndex < dataGridView1.Rows.Count; rowIndex++)
                            {
                                string? menuValue = dataGridView1.Rows[rowIndex].Cells[3].Value?.ToString();

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
                    // Log or handle the case where cart_details is null or dataModel is invalid
                    /*MessageBox.Show("Data not found or in unexpected format.");*/
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
            try
            {
                if (e.RowIndex >= 0 && (e.ColumnIndex == dataGridView1.Columns["Minus"].Index ||
                                        e.ColumnIndex == dataGridView1.Columns["Plus"].Index))
                {
                    int cartDetailId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["CartDetailID"].Value);
                    DataGridViewCell?
                        qtyToSplitCell = dataGridView1.Rows[e.RowIndex].Cells["Hasil"]; // Access the QtyToSplit column
                    DataGridViewCell?
                        jumlahCell = dataGridView1.Rows[e.RowIndex].Cells["Jumlah"]; // Access the Jumlah column

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
                        RequestCartModel? existingItem =
                            cartDetails.FirstOrDefault(item => int.Parse(item.cart_detail_id) == cartDetailId);
                        if (existingItem != null)
                        {
                            existingItem.qty_to_split = currentQuantity.ToString();
                        }
                        else
                        {
                            cartDetails.Add(new RequestCartModel
                            {
                                cart_detail_id = cartDetailId.ToString(),
                                qty_to_split = currentQuantity.ToString(),
                                originQty = maxQuantity.ToString()
                            });
                        }
                    }
                    else
                    {
                        MessageBox.Show("Kuantitas Invalid");
                    }
                }
            }

            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        private async void btnSimpanSplit_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                string source = "DT-Cache\\Transaction\\Cart.data";
                string main_split_cart = "DT-Cache\\Transaction\\Cart_main_split.data";
                string new_cart = "DT-Cache\\Transaction\\Cart.data"; // File baru untuk item yang telah dipecah

                if (cartDetails.Count == 0)
                {
                    MessageBox.Show("Kuantitas item yang ingin di split masih nol", "Split Bill", MessageBoxButtons.OK);
                    return;
                }

                // Membaca data dari file Cart.data (yang lama)
                string cartJson = File.ReadAllText(source);
                JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

                // List untuk menampung item yang telah dipecah
                List<JObject> splitItems = new();
                // List untuk menampung item sisa
                List<JObject> remainingItems = new();

                foreach (JToken cartItem in cartData["cart_details"])
                {
                    int cartDetailId = cartItem["cart_detail_id"].ToObject<int>();
                    RequestCartModel? splitItem =
                        cartDetails.FirstOrDefault(item => item.cart_detail_id == cartDetailId.ToString());

                    if (splitItem != null) // Jika item ada dalam cartDetails yang dipecah
                    {
                        // Ambil kuantitas baru yang dipecah
                        int newQty = int.Parse(splitItem.qty_to_split);
                        int originalQty = int.Parse(cartItem["qty"].ToString());

                        // Update nilai pada cartItem untuk item yang dipecah
                        cartItem["qty"] = newQty;
                        cartItem["updated_at"] =
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        cartItem["total_price"] = newQty * cartItem["price"].ToObject<int>();

                        // Simpan item yang dipecah ke list baru (cart.data)
                        splitItems.Add(cartItem as JObject);

                        // Jika masih ada sisa kuantitas, simpan ke list sisa (cart_main_split)
                        int remainingQty = originalQty - newQty;
                        if (remainingQty > 0)
                        {
                            // Buat item sisa
                            JObject? remainingItem = cartItem.DeepClone() as JObject;
                            remainingItem["qty"] = remainingQty;
                            remainingItem["total_price"] = remainingQty * cartItem["price"].ToObject<int>();
                            remainingItems.Add(remainingItem);
                        }
                    }
                    else
                    {
                        // Jika item tidak dipecah, simpan ke list sisa
                        remainingItems.Add(cartItem as JObject);
                    }
                }

                // Menyimpan data yang telah dipecah ke file Cart.data
                JObject splitCartData = new()
                {
                    ["cart_details"] = new JArray(splitItems),
                    ["subtotal"] = splitItems.Sum(item => item["total_price"].ToObject<int>()),
                    ["total"] = splitItems.Sum(item => item["total_price"].ToObject<int>()),
                    ["transaction_ref_split"] = cartData["transaction_ref"].ToString(),
                    ["transaction_ref"] =
                        $"{baseOutlet}-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}-{GenerateRandomName()}",
                    ["is_splitted"] = 1
                };

                // Simpan file baru Cart.data
                File.WriteAllText(new_cart, JsonConvert.SerializeObject(splitCartData, Formatting.Indented));

                // Menyimpan data sisa ke Cart_main_split.data
                JObject remainingCartData = new()
                {
                    ["cart_details"] = new JArray(remainingItems),
                    ["subtotal"] = remainingItems.Sum(item => item["total_price"].ToObject<int>()),
                    ["total"] = remainingItems.Sum(item => item["total_price"].ToObject<int>()),
                    ["transaction_ref"] = cartData["transaction_ref"].ToString(),
                    ["is_splitted"] = 0 // Menandakan bahwa ini adalah cart dengan sisa
                };

                // Simpan file Cart_main_split
                File.WriteAllText(main_split_cart, JsonConvert.SerializeObject(remainingCartData, Formatting.Indented));

                // Log dan konfirmasi
                Close(); // Tutup form
            }
            catch (Exception ex)
            {
                // Tangani error
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show("Terjadi kesalahan saat memproses split item.", "Error", MessageBoxButtons.OK);
            }
        }


        private string GenerateRandomName()
        {
            Random random = new();
            string[] consonants =
            {
                "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y",
                "z"
            };
            string[] vowels = { "a", "e", "i", "o", "u" };

            string randomName = ""; // Initialize the randomName
            int nameLength = random.Next(3, 10);

            for (int i = 0; i < nameLength; i++)
            {
                randomName += i % 2 == 0
                    ? consonants[random.Next(consonants.Length)]
                    : vowels[random.Next(vowels.Length)];
            }

            return char.ToUpper(randomName[0]) + randomName.Substring(1); // Capitalize the first letter
        }
    }
}