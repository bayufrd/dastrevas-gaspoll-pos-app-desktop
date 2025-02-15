using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome.Sharp;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms.Design;
using KASIR.Printer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using Newtonsoft.Json.Linq;
using KASIR.Komponen;


namespace KASIR.OfflineMode
{
    [Serializable]
    public partial class Offline_addCartForm : Form
    {
        private List<Button> radioButtonsList = new List<Button>();
        string idmenu;
        private DataMenuDetail datas;
        public string btnServingType;
        public bool ReloadDataInBaseForm { get; private set; }
        private readonly string baseOutlet;
        List<MenuDetailDataCart> menuDetailDataCarts;
        List<DataDiscountCart> dataDiskonList;
        List<ServingType> servingType;
        private readonly ILogger _log = LoggerService.Instance._log;
        string namelabel;
        string folder = "DT-Cache\\addCartForm";
        int lblprice, lblsubtotal, lbltotal;
        public Offline_addCartForm(string id, string name)
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            btnSimpan.Enabled = false;
            lblNameCart.Text = "Checking Data...";
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            txtKuantitas.Text = "1";
            idmenu = id;
            foreach (var button in radioButtonsList)
            {
                button.Click += RadioButton_Click;
            }
            cmbVarian.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVarian.DrawMode = DrawMode.OwnerDrawVariable;
            cmbVarian.DrawItem += CmbVarian_DrawItem;

            cmbVarian.ItemHeight = 25; // Set the desired item height
            namelabel = name.ToString();

            cmbDiskon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDiskon.DrawMode = DrawMode.OwnerDrawVariable;
            cmbDiskon.DrawItem += CmbDiskon_DrawItem;

            cmbDiskon.ItemHeight = 25; // Set the desired item height

            LoadDataVarianAsync();

            lblNameCart.TextAlign = ContentAlignment.MiddleCenter;
        }



        private async void LoadDataVarianAsync()
        {
            if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }

            // Load menu data from local file if available
            if (File.Exists(folder + "\\LoadDataVarian_" + idmenu + "_Outlet_" + baseOutlet + ".data"))
            {
                try
                {
                    string json = File.ReadAllText(folder + "\\LoadDataVarian_" + idmenu + "_Outlet_" + baseOutlet + ".data");
                    GetMenuDetailCartModel menuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(json);
                    DataMenuDetail data = menuModel.data;

                    var options = data.menu_details.Where(x => x.menu_detail_id != 0).ToList();
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
                    MessageBox.Show("Gagal tampil data " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
            try
            {
                MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                CacheDataApp form3 = new CacheDataApp("Sync");
                this.Close();
                form3.Show();
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Gagal tampil data " + ex.Message, "Gaspol");
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
                    string json = File.ReadAllText(folder + "\\LoadDataServingType_" + idmenu + "_Outlet_" + baseOutlet + ".data");
                    GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(json);
                    DataMenu data = menuModel.data;
                    var options = data.serving_types;
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
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal tampil data " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }

            try
            {
                MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                CacheDataApp form3 = new CacheDataApp("Sync");
                this.Close();
                form3.Show();

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
        private async Task<string> returnPriceByServingTypeAsync(string serving_type_id, string varian)
        {
            // Membaca data dari file cache
            string cachedData = File.ReadAllText(folder + "\\LoadDataServingType_" + idmenu + "_Outlet_" + baseOutlet + ".data");

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
                var menuPrice = data.menu_prices
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
                    var menuDetail = menuDetailDataList
                        .FirstOrDefault(detail => detail.menu_detail_id == varianId); // Mencocokkan menu_detail_id

                    if (menuDetail != null)
                    {
                        // Cari harga berdasarkan serving_type_id dari menu_prices dalam menu_detail
                        var menuPrice = menuDetail.menu_prices
                            .FirstOrDefault(price => price.serving_type_id == int.Parse(serving_type_id)); // Mencocokkan serving_type_id

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
                MessageBox.Show("Pilih tipe serving", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (int.TryParse(txtKuantitas.Text, out int quantity))
            {
                if (quantity <= 0)
                {
                    MessageBox.Show("Masukan jumlah kuantitas!", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Masukan jumlah kuantitas yang valid!", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateDiscountAsync(int diskon, int price, int quantity)
        {
            if (diskon != -1)
            {
                int diskonMinimum = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.min_purchase ?? -1;
                if (diskonMinimum > (price * quantity))
                {
                    int resultDiskon = diskonMinimum - (price * quantity);
                    MessageBox.Show("Minimum diskon kurang Rp " + resultDiskon + " lagi", "Gaspol");
                    return false;
                }
            }

            return true;
        }

        private async Task SendDataAsync(int serving_type, string pricefix, int diskon, int quantity, string notes, int? selectedVarian)
        {
            try
            {
                btnSimpan.Enabled = false;
                lblNameCart.Text = "Membuat Data...";

                // Path for LoadServingType data
                string servingTypeFilePath = "DT-Cache/addCartForm/LoadDataServingType_" + datas.id + "_Outlet_" + baseOutlet + ".data";

                // Read the LoadServingType.data file if it exists
                string servingTypeJson = File.Exists(servingTypeFilePath) ? File.ReadAllText(servingTypeFilePath) : "{}";
                var servingTypeData = JsonConvert.DeserializeObject<JObject>(servingTypeJson);

                // Find the menu and serving type information from the loaded data
                var menuData = servingTypeData["data"];
                var menuDetails = menuData["menu_details"] as JArray;
                var servingTypes = menuData["serving_types"] as JArray;

                // Get menu detail name based on selected variant
                var selectedMenuDetail = menuDetails.FirstOrDefault(detail => (int)detail["menu_detail_id"] == selectedVarian);
                string menuDetailName = selectedMenuDetail?["varian"]?.ToString();

                // Get the serving type name based on the serving_type_id
                var selectedServingType = servingTypes.FirstOrDefault(type => (int)type["id"] == serving_type);
                string servingTypeName = selectedServingType?["name"]?.ToString();
                int total_item = int.Parse(pricefix) * quantity;
                // Prepare the new item for cart_details
                var newItem = new JObject
                {
                    { "cart_detail_id", DateTime.Now.ToString("HHmmss") },  // Unique ID based on timestamp
                    { "menu_id", datas.id },
                    { "menu_name", menuData["name"] },  // Menu name from the loaded data
                    { "menu_type", menuData["menu_type"] },  // Menu type from the loaded data
                    { "menu_detail_id", selectedVarian ?? null },
                    { "menu_detail_name", menuDetailName },  // Varian name
                    { "varian", menuDetailName },  // Varian name
                    { "is_ordered", 0 },
                    { "serving_type_id", serving_type },
                    { "serving_type_name", servingTypeName },  // Serving type name
                    { "price", pricefix },
                    { "qty", quantity },
                    { "note_item", notes },
                    { "created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "update_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "discount_id", null },
                    { "discount_code", null },
                    { "discounts_value", null },
                    { "discounted_price", 0 },
                    { "discounts_is_percent", null },
                    { "subtotal", total_item },
                    { "total_price", total_item }
                };

                // Set file path for cart data cache
                string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";
                string currentCartJson = File.Exists(cacheFilePath) ? File.ReadAllText(cacheFilePath) : "{}";

                // Deserialize existing cart data
                var cartData = JsonConvert.DeserializeObject<JObject>(currentCartJson);

                // If cart_details array doesn't exist, create it
                if (cartData["cart_details"] == null)
                {
                    cartData["cart_details"] = new JArray();
                }

                // Add the new item to the cart_details array
                var cartDetailsArray = (JArray)cartData["cart_details"];
                cartDetailsArray.Add(newItem);

                // Update the subtotal and total based on cart details
                var total = cartDetailsArray.Sum(item => (decimal)item["price"] * (int)item["qty"]);
                cartData["subtotal"] = total;
                cartData["total"] = total;

                // Serialize the updated cart data back to JSON
                string updatedJsonString = JsonConvert.SerializeObject(cartData, Formatting.Indented);

                // Save the updated cart data to cache (file)
                await SaveToCache(updatedJsonString);

                // Optionally, send the updated data to the server
                /*IApiService apiService = new ApiService();
                HttpResponseMessage response = await apiService.CreateCart(updatedJsonString, "/cart");*/

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
                //MessageBox.Show("File saved successfully at: " + cacheFilePath);

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

                int selectedVarian = int.TryParse(cmbVarian.SelectedValue?.ToString(), out var varianResult) ? varianResult : -1;
                int selectedDiskon = int.TryParse(cmbDiskon.SelectedValue?.ToString(), out var diskonResult) ? diskonResult : -1;
                int diskon = 0;

                int serving_type = int.Parse(comboBox1.SelectedValue?.ToString() ?? "0");
                int quantity = int.TryParse(txtKuantitas.Text, out var qtyResult) ? qtyResult : 0;
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
                //_ = SendDataAsync(serving_type, pricefix, diskon, quantity, notes, selectedVarian);
                await SendDataAsync(serving_type, pricefix, diskon, quantity, notes, selectedVarian);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal tambah Menu, silakan coba ulang: " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "Terjadi kesalahan: {ErrorMessage}", ex.Message);
            }
            finally
            {
                btnSimpan.Enabled = true;
            }
        }

        private void RadioButton_Click(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;
            ////LoggerUtil.LogPrivateMethod(nameof(RadioButton_Click));

            foreach (var button in radioButtonsList)
            {

                button.BackColor = SystemColors.ControlDark;
            }

            clickedButton.ForeColor = Color.White;
            clickedButton.BackColor = Color.SteelBlue;

            btnServingType = clickedButton.Tag.ToString();
        }

        private void label1_Click_2(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnTambah_Click));

            if (int.TryParse(txtKuantitas.Text, out int numericValue))
            {
                numericValue++;
                txtKuantitas.Text = numericValue.ToString();
            }
        }

        private void btnKurang_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnKurang_Click));

            if (int.TryParse(txtKuantitas.Text, out int numericValue))
            {
                if (numericValue > 1)
                {
                    numericValue--;
                    txtKuantitas.Text = numericValue.ToString();
                }
            }
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnShopeeFood_Click(object sender, EventArgs e)
        {

        }

        private void btnTakeaway_Click(object sender, EventArgs e)
        {

        }

        private void addCartForm_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void LoadDataDiscount()
        {
            if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }

            // Load menu data from local file if available
            if (File.Exists(folder + "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data"))
            {
                try
                {
                    string json = File.ReadAllText(folder + "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data");
                    DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(json);
                    List<DataDiscountCart> data = menuModel.data;
                    var options = data;
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
                    //MessageBox.Show("Gagal tambah data " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
            try
            {
                MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                CacheDataApp form3 = new CacheDataApp("Sync");
                this.Close();
                form3.Show();
            }
            catch (TaskCanceledException ex)
            {
                //MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Gagal tampil data diskon " + ex.Message, "Gaspol");
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

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black, new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width, e.Bounds.Height - verticalMargin));

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

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black, new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width, e.Bounds.Height - verticalMargin));

                e.DrawFocusRectangle();
            }
        }

        private void cmbDiskon_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel12_Paint(object sender, PaintEventArgs e)
        {

        }

        private void iconButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
