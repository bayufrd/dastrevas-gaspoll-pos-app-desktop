using System.Data;
using System.Globalization;
using System.Windows.Forms;
using FontAwesome.Sharp;
using KASIR.Helper;
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
            //await LoadCart();
            await LoadCartToFlow();
            btnSimpan.Enabled = true;
            btnKeluar.Enabled = true;
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Close();
        }
        private async Task LoadCartToFlow()
        {
            try
            {
                string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";

                if (!File.Exists(cacheFilePath))
                {
                    NotifyHelper.Error("tidak ada data diterima from the lokal.");
                    return;
                }

                string cartJson = File.ReadAllText(cacheFilePath);
                JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

                if (cartData?["cart_details"] == null || !cartData["cart_details"].Any())
                    return;

                JArray cartDetailsJson = (JArray)cartData["cart_details"];

                // Bersihkan dulu panel
                flowLayoutPanel1.Controls.Clear();
                flowLayoutPanel1.AutoScroll = true;
                flowLayoutPanel1.WrapContents = true;
                flowLayoutPanel1.Padding = new Padding(10);

                foreach (var menu in cartDetailsJson)
                {
                    int qty = menu["qty"] != null ? (int)menu["qty"] : 0;
                    decimal price = menu["price"] != null ? decimal.Parse(menu["price"].ToString()) : 0;
                    decimal totalPrice = price * qty;
                    string totPrice = string.Format("{0:n0},-", totalPrice);
                    string noteItem = menu["note_item"]?.ToString() ?? "";

                    int cartDetailId = (int)menu["cart_detail_id"];
                    int maxQty = qty;

                    // === PANEL ITEM ===
                    Panel itemPanel = new Panel
                    {
                        Width = flowLayoutPanel1.ClientSize.Width - 30,
                        Height = 80,
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Padding = new Padding(10),
                        Margin = new Padding(5)
                    };

                    // Label nama menu
                    Label lblName = new Label
                    {
                        Text = $"{menu["menu_name"]} {menu["cart_detail_name"]}",
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        AutoSize = true,
                        ForeColor = Color.Black,
                        Location = new Point(10, 10)
                    };

                    // Label harga
                    Label lblPrice = new Label
                    {
                        Text = "Rp " + totPrice,
                        Font = new Font("Segoe UI", 9, FontStyle.Regular),
                        AutoSize = true,
                        ForeColor = Color.Gray,
                        Location = new Point(10, 35)
                    };

                    // Tombol - qty +
                    IconButton buttonMinus = new IconButton
                    {
                        Text = "",
                        IconChar = IconChar.CircleMinus,   // pilih icon
                        IconColor = Color.Black,
                        IconSize = 20,
                        TextImageRelation = TextImageRelation.ImageBeforeText,
                        BackColor = Color.White,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Width = 35,
                        Height = 35,
                        Location = new Point(itemPanel.Width - 180, 15),
                        Cursor = Cursors.Hand,
                        Tag = cartDetailId
                    };
                    buttonMinus.FlatAppearance.BorderSize = 0;
                    IconButton buttonPlus = new IconButton
                    {
                        Text = "",
                        IconChar = IconChar.CirclePlus,   // pilih icon
                        IconColor = Color.Black,
                        IconSize = 20,
                        TextImageRelation = TextImageRelation.ImageBeforeText,
                        BackColor = Color.White,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Width = 35,
                        Height = 35,
                        Location = new Point(itemPanel.Width - 100, 15),
                        Cursor = Cursors.Hand,
                        Tag = cartDetailId
                    };
                    buttonPlus.FlatAppearance.BorderSize = 0;

                    Button btnMinus = new Button
                    {
                        Text = "-",
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.Red,
                        Width = 40,
                        Height = 30,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.White,
                        Tag = cartDetailId
                    };

                    Label lblQty = new Label
                    {
                        Text = "0", // default hasil split
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Width = 30,
                        AutoSize = true,
                        Location = new Point(itemPanel.Width - 100, 15),
                    };

                    Button btnPlus = new Button
                    {
                        Text = "+",
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.Green,
                        Width = 40,
                        Height = 30,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.White,
                        Tag = cartDetailId
                    };

                    FlowLayoutPanel qtyPanel = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Right,
                        Width = 140,
                        FlowDirection = FlowDirection.LeftToRight
                    };
                    qtyPanel.Controls.Add(buttonMinus);
                    qtyPanel.Controls.Add(lblQty);
                    qtyPanel.Controls.Add(buttonPlus);

                    // Event handler tombol
                    buttonMinus.Click += (s, e) =>
                    {
                        int currentQty = int.TryParse(lblQty.Text, out int q) ? q : 0;
                        if (currentQty > 0) currentQty--;
                        lblQty.Text = currentQty.ToString();
                        UpdateCartDetail(cartDetailId, currentQty, maxQty);
                    };

                    buttonPlus.Click += (s, e) =>
                    {
                        int currentQty = int.TryParse(lblQty.Text, out int q) ? q : 0;
                        if (currentQty < maxQty) currentQty++;
                        else NotifyHelper.Warning("Kuantitas telah mencapai batas maksimal");
                        lblQty.Text = currentQty.ToString();
                        UpdateCartDetail(cartDetailId, currentQty, maxQty);
                    };

                    // Susun panel
                    itemPanel.Controls.Add(qtyPanel);
                    itemPanel.Controls.Add(lblPrice);
                    itemPanel.Controls.Add(lblName);

                    flowLayoutPanel1.Controls.Add(itemPanel);

                    // Catatan tambahan
                    if (!string.IsNullOrEmpty(noteItem))
                    {
                        Label lblNote = new Label
                        {
                            Text = "*catatan: " + noteItem,
                            Font = new Font("Segoe UI", 8, FontStyle.Italic),
                            ForeColor = Color.DarkGray,
                            AutoSize = true,
                            Margin = new Padding(20, 0, 0, 5)
                        };
                        flowLayoutPanel1.Controls.Add(lblNote);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void UpdateCartDetail(int cartDetailId, int qty, int maxQty)
        {
            RequestCartModel? existingItem = cartDetails
                .FirstOrDefault(item => int.Parse(item.cart_detail_id) == cartDetailId);

            if (qty == 0)
            {
                // Hapus item jika qty 0
                if (existingItem != null)
                    cartDetails.Remove(existingItem);
            }
            else
            {
                if (existingItem != null)
                {
                    // Update qty kalau sudah ada
                    existingItem.qty_to_split = qty.ToString();
                }
                else
                {
                    // Tambahkan item baru kalau belum ada
                    cartDetails.Add(new RequestCartModel
                    {
                        cart_detail_id = cartDetailId.ToString(),
                        qty_to_split = qty.ToString(),
                        originQty = maxQty.ToString()
                    });
                }
            }
        }


        private async void btnSimpanSplit_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                string source = "DT-Cache\\Transaction\\Cart.data";
                string main_split_cart = "DT-Cache\\Transaction\\Cart_main_split.data";
                string new_cart = "DT-Cache\\Transaction\\Cart.data"; // File baru untuk item yang telah dipecah

                if (cartDetails.Count == 0 || cartDetails == null)
                {
                    NotifyHelper.Warning("Kuantitas item yang ingin di split masih nol");
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
                NotifyHelper.Error("Terjadi kesalahan saat memproses split item.");
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