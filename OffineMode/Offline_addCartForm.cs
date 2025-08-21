using System.Globalization;
using System.Reflection;
using FontAwesome.Sharp;
using KASIR.Database;
using KASIR.Database.ModalDatabase;
using KASIR.Helper;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Properties;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.OfflineMode
{
    [Serializable]
    public partial class Offline_addCartForm : Form
    {

        private readonly string baseOutlet;
        public string btnServingType;
        private List<DataDiscountCart> dataDiskonList;
        private DataMenuDetail datas;
        private string folder = "DT-Cache\\addCartForm";
        private string idmenu;
        private List<MenuDetailDataCart> menuDetailDataCarts;
        private string namelabel;
        private List<Button> radioButtonsList = new();
        private List<ServingType> servingType;
        private int servingtypeall;

        public Offline_addCartForm(string id, string name, int selectedServingTypeall)
        {
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
            SetDoubleBufferedForAllControls(this);

            btnSimpan.Enabled = false;
            lblNameCart.Text = "Checking Data...";
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            txtKuantitas.Text = "1";
            idmenu = id;
            servingtypeall = selectedServingTypeall;
            foreach (Button button in radioButtonsList)
            {
                button.Click += RadioButton_Click;
            }

            cmbVarian.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVarian.DrawMode = DrawMode.OwnerDrawVariable;
            cmbVarian.DrawItem += CmbVarian_DrawItem;

            cmbVarian.ItemHeight = 25; // Set the desired item height
            namelabel = name;

            cmbDiskon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDiskon.DrawMode = DrawMode.OwnerDrawVariable;
            cmbDiskon.DrawItem += CmbDiskon_DrawItem;

            cmbDiskon.ItemHeight = 25; // Set the desired item height

            LoadDataVarianAsync();

            lblNameCart.TextAlign = ContentAlignment.MiddleCenter;

            Shown += Form1_Shown; // Tambahkan ini
            //Shown += (sender, e) => btnSimpan_ClickAsync(null, EventArgs.Empty);
        }

        public int selectedServingTypeall { get; set; }

        public bool ReloadDataInBaseForm { get; private set; }

        // Event handler untuk form shown
        private void Form1_Shown(object sender, EventArgs e)
        {
            RefreshIconButtons();
        }

        // Method untuk refresh icon buttons
        private void RefreshIconButtons()
        {
            SuspendLayout();
            foreach (Control c in Controls)
            {
                RecursiveRefreshIcons(c);
            }

            ResumeLayout(true);
        }

        // Method untuk recursive refresh
        private void RecursiveRefreshIcons(Control control)
        {
            if (control is IconButton iconBtn)
            {
                iconBtn.Invalidate(); // Force redraw
            }

            foreach (Control child in control.Controls)
            {
                RecursiveRefreshIcons(child);
            }
        }

        // Tambahkan method ini di Form1
        public static void SetDoubleBufferedForAllControls(Control control)
        {
            foreach (Control c in control.Controls)
            {
                PropertyInfo prop = c.GetType().GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                prop?.SetValue(c, true, null);

                SetDoubleBufferedForAllControls(c);
            }
        }


        private async void LoadDataVarianAsync()
        {
            if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }

            // Load menu data from local file if available
            if (File.Exists(folder + "\\LoadDataVarian_" + idmenu + "_Outlet_" + baseOutlet + ".data"))
            {
                try
                {
                    string json =
                        File.ReadAllText(folder + "\\LoadDataVarian_" + idmenu + "_Outlet_" + baseOutlet + ".data");
                    GetMenuDetailCartModel menuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(json);
                    DataMenuDetail data = menuModel.data;

                    List<MenuDetailDataCart> options = data.menu_details.Where(x => x.menu_detail_id != 0).ToList();
                    options.Insert(0, new MenuDetailDataCart { index = -1, varian = "Normal" });
                    cmbVarian.DataSource = options;
                    cmbVarian.DisplayMember = "varian";
                    cmbVarian.ValueMember = "menu_detail_id";
                    menuDetailDataCarts = data.menu_details;
                    datas = menuModel.data;

                    LoadDataDiscount();
                    return;
                }
                catch (Exception ex)
                {
                    NotifyHelper.Error("Gagal tampil data " + ex);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }

            try
            {
                NotifyHelper.Error("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
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
                NotifyHelper.Error("Gagal tampil data " + ex);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async void LoadDataServingType()
        {
            if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }

            // Load menu data from local file if available
            if (File.Exists(folder + "\\LoadDataServingType_" + idmenu + "_Outlet_" + baseOutlet + ".data"))
            {
                try
                {
                    string json = File.ReadAllText(folder + "\\LoadDataServingType_" + idmenu + "_Outlet_" +
                                                   baseOutlet + ".data");
                    GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(json);
                    DataMenu data = menuModel.data;
                    List<ServingType> options = data.serving_types;
                    // Set the selected item of cmbVarian
                    cmbVarian.SelectedItem = 1;

                    // Set the data source, display member, and value member of comboBox1
                    comboBox1.DataSource = options;
                    comboBox1.DisplayMember = "name";
                    comboBox1.ValueMember = "id";

                    // Set the font size of the items in comboBox1
                    comboBox1.Font = new Font(comboBox1.Font.FontFamily, 12); // Set font size to 14

                    // Set the item height to increase spacing
                    comboBox1.ItemHeight = 30; // Set item height to 30

                    // Optionally, set the ComboBox drop-down width to accommodate larger text
                    comboBox1.DropDownWidth = 200; // Adjust width as necessary


                    servingType = data.serving_types;
                    btnSimpan.Enabled = true;
                    lblNameCart.Text = namelabel;
                    if (servingtypeall != null)
                    {
                        SetComboBoxSelectionByName(data.serving_types, comboBox1, servingtypeall);
                    }

                    return;
                }
                catch (Exception ex)
                {
                    NotifyHelper.Error("Gagal tampil data " + ex);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }

            try
            {
                NotifyHelper.Error("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                CacheDataApp form3 = new("Sync");
                Close();
                form3.Show();
            }
            catch (TaskCanceledException ex)
            {
                NotifyHelper.Error("Koneksi tidak stabil. Coba beberapa saat lagi.");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal tampil data tipe serving " + ex);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        } // Helper method to set ComboBox selection by name (search by name/text)

        private void SetComboBoxSelectionByName(List<ServingType> serving_types, ComboBox comboBox, int servingTypeName)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                ServingType
                    item = (ServingType)comboBox.Items[i]; // Assumed ServingType is the type of items in comboBox1
                if (item.id == servingTypeName) // Compare by name
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private async Task<string> returnPriceByServingTypeAsync(string serving_type_id, string varian)
        {
            // Membaca data dari file cache
            string cachedData =
                File.ReadAllText(folder + "\\LoadDataServingType_" + idmenu + "_Outlet_" + baseOutlet + ".data");

            // Deserialize data dari cache
            GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(cachedData);

            // Validasi data
            if (menuModel == null || menuModel.data == null)
            {
                throw new InvalidOperationException("Menu data is invalid.");
            }

            DataMenu data = menuModel.data;
            List<MenuDetailS> menuDetailDataList = data.menu_details;

            // Jika varian tidak dipilih (selectedVarian == -1)
            if (string.IsNullOrEmpty(varian) || varian == "0")
            {
                // Cari harga berdasarkan serving_type_id di menu_prices
                MenuPrice? menuPrice = data.menu_prices
                    .FirstOrDefault(price => price.serving_type_id == int.Parse(serving_type_id));

                if (menuPrice != null)
                {
                    return menuPrice.price.ToString(); // Return harga dari menu utama
                }
            }
            else
            {
                // Jika varian dipilih, varian adalah menu_detail_id
                int varianId = int.Parse(varian);

                if (varianId != -1)
                {
                    // Cari menu_detail yang memiliki menu_detail_id yang sesuai dengan varian
                    MenuDetailS? menuDetail = menuDetailDataList
                        .FirstOrDefault(detail => detail.menu_detail_id == varianId); // Mencocokkan menu_detail_id

                    if (menuDetail != null)
                    {
                        // Cari harga berdasarkan serving_type_id dari menu_prices dalam menu_detail
                        MenuPrice? menuPrice = menuDetail.menu_prices
                            .FirstOrDefault(price =>
                                price.serving_type_id == int.Parse(serving_type_id)); // Mencocokkan serving_type_id

                        if (menuPrice != null)
                        {
                            return menuPrice.price.ToString(); // Return harga berdasarkan menu_detail_id
                        }
                    }
                }
            }

            // Jika tidak ditemukan harga untuk serving_type_id yang diminta
            return "0"; // Kembalikan "0" jika tidak ditemukan
        }


        private async Task<bool> ValidateInputsAsync()
        {
            if (comboBox1.Text == "Pilih Tipe Serving")
            {
                NotifyHelper.Warning("Pilih tipe serving");
                return false;
            }

            if (int.TryParse(txtKuantitas.Text, out int quantity))
            {
                if (quantity <= 0)
                {
                    NotifyHelper.Warning("Masukan jumlah kuantitas!");
                    return false;
                }
            }
            else
            {
                NotifyHelper.Warning("Masukan jumlah kuantitas yang valid!");
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateDiscountAsync(int diskon, int price, int quantity)
        {
            if (diskon != -1)
            {
                int diskonMinimum = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.min_purchase ?? -1;
                if (diskonMinimum > price * quantity)
                {
                    int resultDiskon = diskonMinimum - (price * quantity);
                    NotifyHelper.Error("Minimum diskon kurang Rp " + resultDiskon + " lagi");
                    return false;
                }
            }

            return true;
        }

        private async Task SendDataAsync(int serving_type, string pricefix, int diskon, int quantity, string notes,
            int? selectedVarian)
        {
            try
            {
                btnSimpan.Enabled = false;
                lblNameCart.Text = "Membuat Data...";

                // Path for LoadServingType data
                string servingTypeFilePath = "DT-Cache/addCartForm/LoadDataServingType_" + datas.id + "_Outlet_" +
                                             baseOutlet + ".data";

                // Read the LoadServingType.data file if it exists
                string servingTypeJson =
                    File.Exists(servingTypeFilePath) ? File.ReadAllText(servingTypeFilePath) : "{}";
                JObject? servingTypeData = JsonConvert.DeserializeObject<JObject>(servingTypeJson);

                if (servingTypeData == null || servingTypeData["data"] == null)
                {
                    LoggerUtil.LogError(null, "Serving type data not found in file.");
                    return; // Return or handle as appropriate
                }

                // Find the menu and serving type information from the loaded data
                JToken? menuData = servingTypeData["data"];
                JArray? menuDetails = menuData["menu_details"] as JArray;
                JArray? servingTypes = menuData["serving_types"] as JArray;
                // Get menu detail name based on selected variant

                if (menuDetails == null)
                {
                    LoggerUtil.LogError(null, "Menu details not found.");
                    return; // Handle as appropriate
                }
                // Get menu detail name based on selected variant
                JToken? selectedMenuDetail =
                    menuDetails.FirstOrDefault(detail => (int)detail["menu_detail_id"] == selectedVarian);

                if (selectedMenuDetail == null && selectedVarian != 0)
                {
                    // Only log the error if selectedVarian is not 0
                    LoggerUtil.LogError(null, "Selected menu detail not found for variant: {selectedVarian}", selectedVarian);
                    return; // Optionally handle the case further
                }

                string menuDetailName = selectedMenuDetail?["varian"]?.ToString();

                // Get the serving type name based on the serving_type_id
                if (servingTypes == null)
                {
                    LoggerUtil.LogError(null, "Serving types not found.");
                    return; // Handle as appropriate
                }

                // Get the serving type name based on the serving_type_id
                JToken? selectedServingType = servingTypes.FirstOrDefault(type => (int)type["id"] == serving_type);
                string servingTypeName = selectedServingType?["name"]?.ToString();

                // Convert pricefix to integer
                if (!int.TryParse(pricefix, out int priceItem))
                {
                    LoggerUtil.LogError(null, "Invalid pricefix value: {pricefix}", pricefix);
                    return; // Handle error
                }

                int subtotal_item = int.Parse(pricefix) * quantity;
                int price_item = int.Parse(pricefix);
                int total_item_withDiscount = subtotal_item;
                int discountPercent = 0;
                int discountValue = 0;
                string? discountCode = null;
                int discountId = 0;
                int discountedPrice = 0;
                int discounted_peritemPrice = 0;

                if (diskon == -1)
                {
                    discountId = diskon;
                }

                if (diskon != -1)
                {
                    discountPercent = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.is_percent ?? 0;
                    discountValue = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.value ?? 0;
                    int discountMax = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.max_discount ?? int.MaxValue;
                    discountCode = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.code ?? null;
                    discountId = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.id ?? 0;

                    int tempTotal = 0;


                    if (discountPercent != 0) // Jika diskon berupa persentase
                    {
                        // Menghitung nilai diskon berdasarkan persentase
                        tempTotal = subtotal_item * discountValue / 100;
                        if (tempTotal > discountMax)
                        {
                            discountedPrice = discountMax; // Potongan diskon maksimal
                        }
                        else
                        {
                            discountedPrice = tempTotal; // Potongan diskon sesuai persen
                        }

                        total_item_withDiscount = subtotal_item - discountedPrice; // Total setelah diskon
                        discounted_peritemPrice = discountedPrice / quantity; // Harga per item setelah diskon
                    }
                    else // Jika diskon berupa nilai tetap
                    {
                        // Mengurangi subtotal dengan diskon nilai tetap
                        tempTotal = subtotal_item - discountValue;
                        if (tempTotal > discountMax)
                        {
                            discountedPrice = discountMax; // Potongan diskon maksimal
                        }
                        else
                        {
                            discountedPrice = subtotal_item - tempTotal; // Potongan diskon tetap
                        }

                        total_item_withDiscount = subtotal_item - discountedPrice; // Total setelah diskon
                        discounted_peritemPrice = discountedPrice / quantity; // Harga per item setelah diskon
                    }
                }
                NotifyHelper.Info($"Item {menuData["name"].ToString()} ditambahkan.");
                string created_atTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                // Prepare the new item for cart_details
                JObject newItem = new()
                {
                    {
                        "cart_detail_id", int.Parse(baseOutlet + DateTime.Now.ToString("HHmmss"))
                    }, // Unique ID based on timestamp
                    { "menu_id", datas.id },
                    { "menu_name", menuData["name"] }, // Menu name from the loaded data
                    { "menu_type", menuData["menu_type"] }, // Menu type from the loaded data
                    { "menu_detail_id", selectedVarian ?? null },
                    { "menu_detail_name", menuDetailName }, // Varian name
                    { "varian", menuDetailName }, // Varian name
                    { "is_ordered", 0 },
                    { "serving_type_id", serving_type },
                    { "serving_type_name", servingTypeName }, // Serving type name
                    { "price", price_item },
                    { "qty", quantity },
                    { "note_item", notes },
                    { "created_at", created_atTime },
                    { "updated_at", created_atTime },
                    { "discount_id", discountId },
                    { "discount_code", discountCode },
                    { "discounts_value", discountValue },
                    { "discounted_price", discountedPrice },
                    { "discounted_item_price", discounted_peritemPrice },
                    { "discounts_is_percent", discountPercent },
                    { "subtotal_price", int.Parse(subtotal_item.ToString()) },
                    { "total_price", total_item_withDiscount }
                };

                // Set file path for cart data cache
                string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";
                string currentCartJson = File.Exists(cacheFilePath) ? File.ReadAllText(cacheFilePath) : "{}";

                // Deserialize existing cart data
                JObject? cartData = JsonConvert.DeserializeObject<JObject>(currentCartJson);

                // If cart_details array doesn't exist, create it
                if (cartData["cart_details"] == null)
                {
                    cartData["cart_details"] = new JArray();
                }

                // Add the new item to the cart_details array
                JArray? cartDetailsArray = (JArray)cartData["cart_details"];
                cartDetailsArray.Add(newItem);

                // Update the subtotal and total based on cart details
                int subtotal = cartDetailsArray.Sum(item => (int)item["price"] * (int)item["qty"]);
                int total = cartDetailsArray.Sum(item => (int)item["total_price"]);

                cartData["subtotal"] = int.Parse(subtotal.ToString());
                cartData["total"] = int.Parse(total.ToString());

                if (string.IsNullOrEmpty(cartData["transaction_ref"]?.ToString()))
                {
                    string randomName = "";

                    generateRandomFill(ref randomName);
                    string transaction_ref_time = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    cartData["transaction_ref"] = $"{baseOutlet}-{transaction_ref_time}-{randomName}";
                }

                // Serialize the updated cart data back to JSON
                string updatedJsonString = JsonConvert.SerializeObject(cartData, Formatting.Indented);

                // Save the updated cart data to cache (file)
                await SaveToCache(updatedJsonString);

                // Optionally, send the updated data to the server
            }
            catch (TaskCanceledException ex)
            {
                // Handle timeout error
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                // Handle general error
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private string GenerateTransactionRef()
        {
            string randomName = "";

            generateRandomFill(ref randomName);
            string transaction_ref_time = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            return $"{baseOutlet}-{transaction_ref_time}-{randomName}";
        }
        private async Task SendDataAsyncWithSQL(int serving_type, string pricefix, int diskon, int quantity, string notes,
            int? selectedVarian)
        {
            try
            {
                btnSimpan.Enabled = false;
                lblNameCart.Text = "Membuat Data...";

                CartDetailsRepository cartRepo = new CartDetailsRepository();

                using (var context = new AppDbContext())
                {
                    // Cek apakah ada cart modal  
                    var existingCart = await context.Carts.Include(c => c.CartDetails).FirstOrDefaultAsync();

                    // Jika tidak ada cart modal, buat cart baru  
                    if (existingCart == null)
                    {
                        existingCart = new dbCartModal
                        {
                            TransactionRef = GenerateTransactionRef(), // Generate unique transaction reference  
                            Subtotal = 0,
                            Total = 0,
                            DiscountsValue = 0,
                            DiscountId = 0,
                            DiscountCode = null,
                            DiscountsIsPercent = 0,
                            DiscountedPrice = 0,
                            CartDetails = new List<dbCartDetails>()
                        };

                        context.Carts.Add(existingCart); // Menambah cart modal baru ke context  
                        await context.SaveChangesAsync(); // Simpan perubahan  
                    }

                    // Path for LoadServingType data
                    string servingTypeFilePath = "DT-Cache/addCartForm/LoadDataServingType_" + datas.id + "_Outlet_" +
                                             baseOutlet + ".data";

                    // Read the LoadServingType.data file if it exists
                    string servingTypeJson =
                        File.Exists(servingTypeFilePath) ? File.ReadAllText(servingTypeFilePath) : "{}";
                    JObject? servingTypeData = JsonConvert.DeserializeObject<JObject>(servingTypeJson);

                    if (servingTypeData == null || servingTypeData["data"] == null)
                    {
                        LoggerUtil.LogError(null, "Serving type data not found in file.");
                        return; // Return or handle as appropriate
                    }

                    // Find the menu and serving type information from the loaded data
                    JToken? menuData = servingTypeData["data"];
                    JArray? menuDetails = menuData["menu_details"] as JArray;
                    JArray? servingTypes = menuData["serving_types"] as JArray;
                    // Get menu detail name based on selected variant

                    if (menuDetails == null)
                    {
                        LoggerUtil.LogError(null, "Menu details not found.");
                        return; // Handle as appropriate
                    }
                    // Get menu detail name based on selected variant
                    JToken? selectedMenuDetail =
                        menuDetails.FirstOrDefault(detail => (int)detail["menu_detail_id"] == selectedVarian);

                    if (selectedMenuDetail == null && selectedVarian != 0)
                    {
                        // Only log the error if selectedVarian is not 0
                        LoggerUtil.LogError(null, "Selected menu detail not found for variant: {selectedVarian}", selectedVarian);
                        return; // Optionally handle the case further
                    }

                    string menuDetailName = selectedMenuDetail?["varian"]?.ToString();

                    // Get the serving type name based on the serving_type_id
                    if (servingTypes == null)
                    {
                        LoggerUtil.LogError(null, "Serving types not found.");
                        return; // Handle as appropriate
                    }

                    // Get the serving type name based on the serving_type_id
                    JToken? selectedServingType = servingTypes.FirstOrDefault(type => (int)type["id"] == serving_type);
                    string servingTypeName = selectedServingType?["name"]?.ToString();

                    // Convert pricefix to integer
                    if (!int.TryParse(pricefix, out int priceItem))
                    {
                        LoggerUtil.LogError(null, "Invalid pricefix value: {pricefix}", pricefix);
                        return; // Handle error
                    }

                    int subtotal_item = int.Parse(pricefix) * quantity;
                    int price_item = int.Parse(pricefix);
                    int total_item_withDiscount = subtotal_item;
                    int discountPercent = 0;
                    int discountValue = 0;
                    string? discountCode = null;
                    int discountId = 0;
                    int discountedPrice = 0;
                    int discounted_peritemPrice = 0;

                    if (diskon == -1)
                    {
                        discountId = diskon;
                    }

                    if (diskon != -1)
                    {
                        discountPercent = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.is_percent ?? 0;
                        discountValue = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.value ?? 0;
                        int discountMax = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.max_discount ?? int.MaxValue;
                        discountCode = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.code ?? null;
                        discountId = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.id ?? 0;

                        int tempTotal = 0;


                        if (discountPercent != 0) // Jika diskon berupa persentase
                        {
                            // Menghitung nilai diskon berdasarkan persentase
                            tempTotal = subtotal_item * discountValue / 100;
                            if (tempTotal > discountMax)
                            {
                                discountedPrice = discountMax; // Potongan diskon maksimal
                            }
                            else
                            {
                                discountedPrice = tempTotal; // Potongan diskon sesuai persen
                            }

                            total_item_withDiscount = subtotal_item - discountedPrice; // Total setelah diskon
                            discounted_peritemPrice = discountedPrice / quantity; // Harga per item setelah diskon
                        }
                        else // Jika diskon berupa nilai tetap
                        {
                            // Mengurangi subtotal dengan diskon nilai tetap
                            tempTotal = subtotal_item - discountValue;
                            if (tempTotal > discountMax)
                            {
                                discountedPrice = discountMax; // Potongan diskon maksimal
                            }
                            else
                            {
                                discountedPrice = subtotal_item - tempTotal; // Potongan diskon tetap
                            }

                            total_item_withDiscount = subtotal_item - discountedPrice; // Total setelah diskon
                            discounted_peritemPrice = discountedPrice / quantity; // Harga per item setelah diskon
                        }
                    }

                    string created_atTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    // Buat item baru untuk cart details
                    var newItem = new dbCartDetails
                    {
                        CartDetailId = existingCart.Id, // Mengaitkan dengan cart modal
                        MenuId = datas.id,
                        MenuName = menuData["name"].ToString(),
                        MenuType = menuData["menu_type"].ToString(),
                        MenuDetailId = selectedVarian ?? null,
                        MenuDetailName = menuDetailName,
                        ServingTypeId = serving_type,
                        ServingTypeName = servingTypeName,
                        Price = priceItem,
                        Qty = quantity,
                        NoteItem = notes,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DiscountId = discountId,
                        DiscountCode = discountCode,
                        DiscountsValue = discountValue,
                        DiscountedPrice = discountedPrice,
                        DiscountedPerItemPrice = discounted_peritemPrice,
                        DiscountsIsPercent = discountPercent,
                        SubtotalPrice = subtotal_item,
                        TotalPrice = total_item_withDiscount
                    };

                    existingCart.CartDetails.Add(newItem); // Menambahkan item ke collection cart
                    existingCart.Subtotal += subtotal_item; // Update subtotal cart
                    existingCart.Total += total_item_withDiscount; // Update total cart

                    await context.SaveChangesAsync(); // Simpan perubahan ke database
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
        }

        private void generateRandomFill(ref string randomName) // 'ref' allows modification of the value
        {
            Random random = new();

            string[] consonants =
            {
                "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y",
                "z"
            };
            string[] vowels = { "a", "e", "i", "o", "u" };

            randomName = ""; // Initialize the randomName

            int nameLength = random.Next(3, 10);
            for (int i = 0; i < nameLength; i++)
            {
                randomName += i % 2 == 0
                    ? consonants[random.Next(consonants.Length)]
                    : vowels[random.Next(vowels.Length)];
            }

            randomName = char.ToUpper(randomName[0]) + randomName.Substring(1); // Capitalize the first letter
        }

        private async Task SaveToCache(string jsonString)
        {
            try
            {
                // Set cache file path (adjust as necessary for your environment)
                string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";

                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(cacheFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Save the jsonString to the cache file
                File.WriteAllText(cacheFilePath, jsonString);
                //NotifyHelper.Error("File saved successfully at: " + cacheFilePath);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error saving to cache: {ErrorMessage}", ex.Message);
            }
        }


        private async void btnSimpan_ClickAsync(object sender, EventArgs e)
        {
            btnSimpan.Enabled = false;

            try
            {
                if (!await ValidateInputsAsync())
                {
                    btnSimpan.Enabled = true;
                    return;
                }

                int selectedVarian = int.TryParse(cmbVarian.SelectedValue?.ToString(), out int varianResult)
                    ? varianResult
                    : -1;
                int selectedDiskon = int.TryParse(cmbDiskon.SelectedValue?.ToString(), out int diskonResult)
                    ? diskonResult
                    : -1;
                int diskon = 0;

                int serving_type = int.Parse(comboBox1.SelectedValue?.ToString() ?? "0");
                int quantity = int.TryParse(txtKuantitas.Text, out int qtyResult) ? qtyResult : 0;
                string notes = txtNotes.Text ?? string.Empty;
                string pricefix = "0";

                if (selectedVarian == -1)
                {
                    pricefix = await returnPriceByServingTypeAsync(serving_type.ToString(), "0");
                }
                else
                {
                    pricefix = await returnPriceByServingTypeAsync(serving_type.ToString(), selectedVarian.ToString());
                }

                if (selectedDiskon != -1)
                {
                    diskon = selectedDiskon;
                    if (!await ValidateDiscountAsync(diskon, int.Parse(pricefix), quantity))
                    {
                        btnSimpan.Enabled = true;
                        return;
                    }
                }

                // Call SendDataAsync without awaiting it
                await SendDataAsync(serving_type, pricefix, diskon, quantity, notes, selectedVarian);

                //await SendDataAsyncWithSQL(serving_type, pricefix, diskon, quantity, notes, selectedVarian);

                DialogResult = DialogResult.OK;
                selectedServingTypeall = int.Parse(comboBox1.SelectedValue?.ToString());
                Close();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal tambah Menu, silakan coba ulang: " + ex.Message);
                LoggerUtil.LogError(ex, "Terjadi kesalahan: {ErrorMessage}", ex.Message);
            }
            finally
            {
                btnSimpan.Enabled = true;
            }
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

            btnServingType = clickedButton.Tag.ToString();
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {

            if (int.TryParse(txtKuantitas.Text, out int numericValue))
            {
                numericValue++;
                txtKuantitas.Text = numericValue.ToString();
            }
        }

        private void btnKurang_Click(object sender, EventArgs e)
        {

            if (int.TryParse(txtKuantitas.Text, out int numericValue))
            {
                if (numericValue > 1)
                {
                    numericValue--;
                    txtKuantitas.Text = numericValue.ToString();
                }
            }
        }
        private async void LoadDataDiscount()
        {
            if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }

            // Load menu data from local file if available
            if (File.Exists(folder + "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data"))
            {
                try
                {
                    string json =
                        File.ReadAllText(folder + "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data");
                    DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(json);
                    List<DataDiscountCart> data = menuModel.data;
                    List<DataDiscountCart> options = data;
                    dataDiskonList = data;
                    options.Insert(0, new DataDiscountCart { id = -1, code = "Pilih Diskon" });
                    cmbDiskon.DataSource = options;
                    cmbDiskon.DisplayMember = "code";
                    cmbDiskon.ValueMember = "id";

                    LoadDataServingType();
                    return;
                }
                catch (Exception ex)
                {
                    //NotifyHelper.Error("Gagal tambah data " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }

            try
            {
                NotifyHelper.Error("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                CacheDataApp form3 = new("Sync");
                Close();
                form3.Show();
            }
            catch (TaskCanceledException ex)
            {
                //NotifyHelper.Error("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal tampil data diskon " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void CmbVarian_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();

                int verticalMargin = 5;
                string itemText = cmbVarian.GetItemText(cmbVarian.Items[e.Index]);

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black,
                    new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width,
                        e.Bounds.Height - verticalMargin));

                e.DrawFocusRectangle();
            }
        }

        private void CmbDiskon_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();

                int verticalMargin = 5;
                string itemText = cmbDiskon.GetItemText(cmbDiskon.Items[e.Index]);

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black,
                    new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width,
                        e.Bounds.Height - verticalMargin));

                e.DrawFocusRectangle();
            }
        }
    }
}