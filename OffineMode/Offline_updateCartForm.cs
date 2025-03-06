using FontAwesome.Sharp;
using KASIR.komponen;
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
using System.Windows.Markup;
using static System.Windows.Forms.Design.AxImporter;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Windows.Forms.Design;
using KASIR.Komponen;
using Newtonsoft.Json.Linq;
using System.Globalization;
namespace KASIR.OfflineMode
{
    public partial class Offline_updateCartForm : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private int numericValue = 0;
        private List<Button> radioButtonsList = new List<Button>();
        string idmenu;
        private DataMenuDetail datas;
        private Label dynamicLabel;
        public string btnServingType;
        public string cartdetail;
        private readonly string baseOutlet;
        List<MenuDetailDataCart> menuDetailDataCarts;
        List<DataDiscountCart> dataDiscount;
        List<DataDiscountCart> dataDiskonList;
        string is_ordered = "0";
        string searchText = "";
        string searchTextserving = "", lblnamaitem;
        int kuantitas = 900000;
        string folder = "DT-Cache\\addCartForm";
        public bool ReloadDataInBaseForm { get; private set; }
        int is_saveBillCart = 0, maxQtyOrdered;
        string updateReason;

        public Offline_updateCartForm(string id, string cartdetailid)
        {
            InitializeComponent();
            lblNameCart.Text = "Loading Data...";
            btnHapus.Enabled = false;
            btnSimpan.Enabled = false;
            btnTambah.Enabled = false;
            btnKurang.Enabled = false;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.AutoScroll = false;
            cartdetail = cartdetailid;
            txtKuantitas.Text = "0";

            idmenu = id;


            foreach (var button in radioButtonsList)
            {
                button.Click += RadioButton_Click;
            }

            cmbVarian.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVarian.DrawMode = DrawMode.OwnerDrawVariable;
            cmbVarian.DrawItem += CmbVarian_DrawItem;
            cmbVarian.ItemHeight = 25; // Set the desired item height
            LoadDataVarianAsync();
            /*int newHeight = Screen.PrimaryScreen.WorkingArea.Height - 100;
            Height = newHeight;*/
        }
    /*    private async void LoadDataDiscount()
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

                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Gagal tambah data " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                CacheDataApp form3 = new CacheDataApp("Sync");
                DialogResult = DialogResult.Cancel;
                this.Close();
                form3.Show();
            }

        }*/
        private async void LoadDataVarianAsync()
        {
            try
            {
                if (File.Exists(folder + "\\LoadDataVarian_" + idmenu + "_Outlet_" + baseOutlet + ".data"))
                {
                    string json = File.ReadAllText(folder + "\\LoadDataVarian_" + idmenu + "_Outlet_" + baseOutlet + ".data");
                    GetMenuDetailCartModel menuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(json);
                    DataMenuDetail data = menuModel.data;

                    var options = data.menu_details.Where(x => x.menu_detail_id != 0).ToList();
                    options.Insert(0, new MenuDetailDataCart { index = -1, varian = "Normal" });
                    //cmbVarian.SelectedIndex = menuModel.data.id;
                    cmbVarian.DataSource = options;
                    cmbVarian.DisplayMember = "varian";
                    cmbVarian.ValueMember = "menu_detail_id";
                    menuDetailDataCarts = data.menu_details;

                    datas = menuModel.data;
                }
                else
                {
                    MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                    CacheDataApp form3 = new CacheDataApp("Sync");
                    DialogResult = DialogResult.Cancel;
                    this.Close();
                    form3.Show();
                }
                LoadDataDiscount();

                LoadItemOnCart();
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal tampil data " + ex, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private async void LoadItemOnCart()
        {
            try
            {
                string ConfigCart = "DT-Cache\\Transaction\\Cart.data";
                string json = File.ReadAllText(ConfigCart);
                CartDataCache cartData = JsonConvert.DeserializeObject<CartDataCache>(json);

                // Helper function to get item by cart detail ID
                DataItemOnCart GetItemByCartDetailId(string cartDetailId)
                {
                    return cartData.cart_details
                        .Where(c => c.cart_detail_id == cartDetailId)
                        .Select(item => new DataItemOnCart
                        {
                            cart_detail_id = int.Parse(item.cart_detail_id),
                            menu_id = item.menu_id,
                            menu_name = item.menu_name,
                            menu_type = item.menu_type,
                            menu_detail_id = item.menu_detail_id,
                            varian = item.menu_detail_name,
                            menu_detail_name = item.menu_detail_name,
                            is_ordered = item.is_ordered.ToString(),
                            discount_code = item.discount_code?.ToString(),
                            serving_type_id = item.serving_type_id,
                            serving_type_name = item.serving_type_name.ToString(),
                            price = item.price,
                            subtotal_price = int.Parse(item.subtotal_price.ToString()),
                            subtotal = int.Parse(item.subtotal_price.ToString()),
                            total_price = item.total_price,
                            qty = item.qty,
                            note_item = item.note_item
                        })
                        .FirstOrDefault();
                }

                string cartDetailIdToFind = cartdetail;
                DataItemOnCart data = GetItemByCartDetailId(cartDetailIdToFind);

                if (data != null)
                {
                    Console.WriteLine($"Menu Name: {data.menu_name}, Price: {data.price}, Quantity: {data.qty}");
                    // Set labels and fields based on the data retrieved
                    is_ordered = data.is_ordered;
                    lblTipe.Text = $"Tipe Penjualan : {data.serving_type_name ?? ""}";
                    txtKuantitas.Text = data.qty.ToString();
                    txtNotes.Text = data.note_item ?? string.Empty;
                    lblVarian.Text = $"Varian : {data.menu_detail_name ?? "Normal"}";
                    lblVarian.Enabled = true;
                    lblTipe.Enabled = true;
                    lblDiscount.Text = $"Discount : {data.discount_code ?? ""}";
                    searchTextserving = data.serving_type_name ?? string.Empty;
                    lblnamaitem = data.menu_name ?? "";
                    is_saveBillCart = int.Parse(data.is_ordered.ToString());
                    maxQtyOrdered = int.Parse(data.qty.ToString());
                    LoadDataServingType(searchTextserving);
                    // Ensure that menu_detail_id is converted to a non-nullable int (default to 0 if null)
                    //LoadDataVariants(data.menu_detail_id ?? 0);
                    SetComboBoxSelectionByNameVarian(data.menu_detail_name);  // Search by variant name
                    string discount_id = data.discount_id.ToString();
                    if (discount_id != null && discount_id != "0")
                    {
                        string discountcode = data.discount_code ?? string.Empty;
                        for (int i = 0; i < cmbDiskon.Items.Count; i++)
                        {
                            var item = (DataDiscountCart)cmbDiskon.Items[i];  // Assumed ServingType is the type of items in comboBox1
                            if (item.code.Equals(discountcode, StringComparison.OrdinalIgnoreCase)) // Compare by name
                            {
                                cmbDiskon.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Item not found.");
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal tampil data: {ex}");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }



        private async void LoadDataServingType(string servingTypeName)
        {
            try
            {
                string filePath = $"{folder}\\LoadDataServingType_{idmenu}_Outlet_{baseOutlet}.data";
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(json);
                    DataMenu data = menuModel.data;

                    // Ensure that the data.serving_types is the correct list of ServingType objects
                    var options = data.serving_types;

                    comboBox1.DataSource = options;  // Ensure this is a list of ServingType
                    comboBox1.DisplayMember = "name";
                    comboBox1.ValueMember = "id";

                    // Set selected item based on serving_type_id
                    SetComboBoxSelectionByName(data.serving_types, comboBox1, servingTypeName);
                }
                else
                {
                    MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                    CacheDataApp form3 = new CacheDataApp("Sync");
                    DialogResult = DialogResult.Cancel;
                    this.Close();
                    form3.Show();
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal tampil data tipe serving: {ex.Message}", "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            finally
            {
                btnHapus.Enabled = true;
                btnSimpan.Enabled = true;
                btnTambah.Enabled = true;
                btnKurang.Enabled = true;
                lblNameCart.Text = lblnamaitem;
            }
        }
        // Helper method to set ComboBox selection by name (search by name/text)
        private void SetComboBoxSelectionByNameVarian(string searchText)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                for (int kimak = 0; kimak < cmbVarian.Items.Count; kimak++)
                {
                    cmbVarian.SelectedIndex = kimak;
                    if (cmbVarian.Text == searchText)
                    {
                        cmbVarian.SelectedIndex = kimak;
                        break;
                    }
                }
            }
        }

        // Helper method to set ComboBox selection by name (search by name/text)
        private void SetComboBoxSelectionByName(List<ServingType> serving_types, ComboBox comboBox, string servingTypeName)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                var item = (ServingType)comboBox.Items[i];  // Assumed ServingType is the type of items in comboBox1
                if (item.name.Equals(servingTypeName, StringComparison.OrdinalIgnoreCase)) // Compare by name
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
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

        private async void btnSimpan_ClickAsync(object sender, EventArgs e)
        {

        }

        private void RadioButton_Click(object sender, EventArgs e)
        {
            //LoggerUtil.LogPrivateMethod(nameof(RadioButton_Click));

            var clickedButton = (Button)sender;

            foreach (var button in radioButtonsList)
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

            this.Close();
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void btnHapus_Click(object sender, EventArgs e)
        {

            if (is_ordered == "1")
            {
                Form background = new Form
                {
                    StartPosition = FormStartPosition.Manual,
                    FormBorderStyle = FormBorderStyle.None,
                    Opacity = 0.7d,
                    BackColor = Color.Black,
                    WindowState = FormWindowState.Maximized,
                    TopMost = true,
                    Location = this.Location,
                    ShowInTaskbar = false,
                };

                using (Offline_deletePerItemForm Offline_deletePerItemForm = new Offline_deletePerItemForm(cartdetail))
                {
                    Offline_deletePerItemForm.Owner = background;

                    background.Show();

                    DialogResult result = Offline_deletePerItemForm.ShowDialog();

                    // Handle the result if needed
                    if (result == DialogResult.OK)
                    {
                        DialogResult = DialogResult.OK;
                        background.Dispose();
                        Close();
                        // Settings were successfully updated, perform any necessary actions
                    }
                    else
                    {
                        DialogResult = DialogResult.Cancel;
                        background.Dispose();
                        Close();
                    }
                }
            }
            else
            {
                try
                {
                    lblNameCart.Text = "Menghapus Data...";

                    string configCart = "DT-Cache\\Transaction\\Cart.data";

                    if (File.Exists(configCart))
                    {
                        // Read the cart file content as a string
                        string json = File.ReadAllText(configCart);

                        // Parse the JSON string into a JObject (dynamic)
                        JObject cartData = JObject.Parse(json);

                        // Find and remove the item with the matching cart_detail_id
                        JArray cartDetails = (JArray)cartData["cart_details"];
                        var itemToRemove = cartDetails.FirstOrDefault(item => item["cart_detail_id"].ToString() == cartdetail);
                        if (itemToRemove != null)
                        {
                            // Remove the item from the array
                            cartDetails.Remove(itemToRemove);

                            // Recalculate subtotal and total
                            decimal subtotal = 0;
                            foreach (var item in cartDetails)
                            {
                                subtotal += (int)item["total_price"];
                            }

                            // Update the cart totals
                            cartData["subtotal"] = int.Parse(subtotal.ToString());
                            cartData["subtotal_price"] = int.Parse(subtotal.ToString());
                            cartData["total"] = int.Parse(subtotal.ToString());
                            cartData["is_sent_sync"] = 0;

                            // Save the updated cart data back to the file
                            File.WriteAllText(configCart, cartData.ToString(Formatting.Indented));

                            Console.WriteLine("Item successfully removed.");
                        }
                        else
                        {
                            Console.WriteLine("Item not found in the cart.");
                        }
                    }
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (TaskCanceledException ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
                catch (Exception ex)
                {
                    DialogResult = DialogResult.Cancel;
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    Close();
                }
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

        private void btnTambah_Click_1(object sender, EventArgs e)
        {
            if (is_ordered == "1" && int.Parse(txtKuantitas.Text.ToString()) < maxQtyOrdered)
            {
                int numericValueOrdered = int.Parse(txtKuantitas.Text.ToString());
                numericValueOrdered++;
                txtKuantitas.Text = numericValueOrdered.ToString();
            }
            if (is_ordered == "1")
            {
                return;
            }
            if (int.TryParse(txtKuantitas.Text, out int numericValue))
            {
                numericValue++;
                txtKuantitas.Text = numericValue.ToString();
            }
        }

        private void btnKurang_Click_1(object sender, EventArgs e)
        {
            if (is_ordered == "1")
            {
              /*  if (cmbDiskon.SelectedIndex != 0)
                {
                    cmbDiskon.SelectedIndex = 0;
                    MessageBox.Show("Jika melakukan perubahan kuantitas, Diskon akan direset.", "Peringatan!");
                }*/
            }
            if (int.TryParse(txtKuantitas.Text, out int numericValue))
            {
                if (numericValue > 1)
                {
                    numericValue--;
                    txtKuantitas.Text = numericValue.ToString();
                }
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void btnSimpan_Click(object sender, EventArgs e)
        {

            try
            {
                if (is_ordered == "1")
                {
                    Form background = new Form
                    {
                        StartPosition = FormStartPosition.Manual,
                        FormBorderStyle = FormBorderStyle.None,
                        Opacity = 0.7d,
                        BackColor = Color.Black,
                        WindowState = FormWindowState.Maximized,
                        TopMost = true,
                        Location = this.Location,
                        ShowInTaskbar = false,
                    };

                    using (Offline_updatePerItemForm Offline_updatePerItemForm = new Offline_updatePerItemForm())
                    {
                        Offline_updatePerItemForm.Owner = background;

                        background.Show();
                        DialogResult result = Offline_updatePerItemForm.ShowDialog();

                        // Handle the result if needed
                        if (result == DialogResult.OK)
                        {
                            background.Dispose();
                            updateReason = Offline_updatePerItemForm.cancelReason.ToString();
                        }
                        else
                        {
                            DialogResult = DialogResult.Cancel;
                            background.Dispose();
                            Close();
                            return;
                        }
                    }
                }
                
                await UpdateCartItemLocally(cartdetail, updateReason);


                DialogResult = DialogResult.OK;
                Close();
            }

            
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show("Gagal ubah data " + ex.Message, "Gaspol");
            }

        }
        private async Task UpdateCartItemLocally(string cartDetailId, string updateReason)
        {
            try
            {
                string configCart = "DT-Cache\\Transaction\\Cart.data";

                if (File.Exists(configCart))
                {
                    // Read the cart file content as a string
                    string json = File.ReadAllText(configCart);

                    // Parse the JSON string into a JObject (dynamic)
                    JObject cartData = JObject.Parse(json);

                    // Find the item with the matching cart_detail_id
                    JArray cartDetails = (JArray)cartData["cart_details"];
                    var itemToUpdate = cartDetails.FirstOrDefault(item => item["cart_detail_id"].ToString() == cartDetailId);

                    if (itemToUpdate != null)
                    {
                        int selectedVarian = int.TryParse(cmbVarian.SelectedValue?.ToString(), out var varianResult) ? varianResult : -1;
                        string VarianName = cmbVarian.Text.ToString();
                        if(VarianName == "Normal") VarianName = (string)null;
                        int serving_type = int.Parse(comboBox1.SelectedValue.ToString());
                        int quantity = int.Parse(txtKuantitas.Text.ToString());
                        string notes = txtNotes.Text.ToString();
                        int? menuDetailIdValue = null;
                        string pricefix = "0", servingTypeName = comboBox1.Text.ToString();
                        //int selectedDiskon = int.Parse(cmbDiskon.SelectedValue.ToString());
                        int diskon = 0;
                        if (comboBox1.Text.ToString() == "Pilih Tipe Serving")
                        {
                            MessageBox.Show("Pilih tipe serving", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        if (int.Parse(txtKuantitas.Text.ToString()) <= 0 || txtKuantitas.Text.ToString() == "")
                        {
                            MessageBox.Show("Masukan jumlah kuantitas!", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        // Update the item with the new values
                        itemToUpdate["qty"] = quantity;  // Update quantity
                        itemToUpdate["note_item"] = notes;  // Update notes

                        // Update serving type (serving_type_id and serving_type_name)
                        itemToUpdate["serving_type_id"] = serving_type;
                        itemToUpdate["serving_type_name"] = servingTypeName;


                        // Update serving type (serving_type_id and serving_type_name)
                        itemToUpdate["menu_detail_id"] = selectedVarian;
                        itemToUpdate["menu_detail_name"] = VarianName;
                        itemToUpdate["varian"] = VarianName;

                        itemToUpdate["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                        if (selectedVarian == -1)
                        {
                            pricefix = await returnPriceByServingTypeAsync(serving_type.ToString(), "0");
                        }
                        else
                        {
                            pricefix = await returnPriceByServingTypeAsync(serving_type.ToString(), selectedVarian.ToString());
                        }


                        // Recalculate price (price * quantity)
                        //pricefix = itemToUpdate["price"].ToString();
                        itemToUpdate["price"] = int.Parse(pricefix);
                        int subtotal_item = int.Parse(pricefix) * quantity;
                        itemToUpdate["subtotal"] = subtotal_item;
                        itemToUpdate["subtotal_price"] = subtotal_item;

                        int total_item_withDiscount = subtotal_item;
                        int discountPercent = 0;
                        int discountValue = 0;
                        string? discountCode = (string)null;
                        int discountId = 0;
                        int discountedPrice = 0;
                        diskon = int.Parse(cmbDiskon.SelectedValue.ToString());
                        if (diskon == -1)
                        {
                            discountId = diskon;
                        }
                        if (diskon != -1)
                        {
                            discountPercent = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.is_percent ?? 0;
                            discountValue = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.value ?? 0;

                            int discountMax = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.max_discount ?? int.MaxValue;
                            diskon = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.id ?? 0;
                            int tempTotal = 0;
                            discountCode = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.code.ToString() ?? string.Empty;

                            if (discountPercent != 0)
                            {
                                tempTotal = subtotal_item * discountValue / 100;
                                if (tempTotal > discountMax)
                                {
                                    discountedPrice = discountMax;
                                }
                                else
                                {
                                    discountedPrice = subtotal_item - tempTotal;
                                }
                                total_item_withDiscount = subtotal_item - discountedPrice;
                                discountedPrice = discountedPrice / quantity;
                            }
                            else
                            {
                                tempTotal = subtotal_item - discountValue;
                                if (tempTotal > discountMax)
                                {
                                    discountedPrice = discountMax;
                                }
                                else
                                {
                                    discountedPrice = subtotal_item - tempTotal;
                                }
                                total_item_withDiscount = subtotal_item - discountedPrice;
                                discountedPrice = discountedPrice / quantity;
                            }
                        }
                        itemToUpdate["total_price"] = total_item_withDiscount;
                        itemToUpdate["edited_reason"] = updateReason ?? "";
                        itemToUpdate["discount_id"] = diskon;
                        itemToUpdate["discount_code"] = discountCode.ToString();
                        itemToUpdate["discounts_value"] = discountValue;
                        itemToUpdate["discounts_is_percent"] = discountPercent;
                        itemToUpdate["discounted_price"] = discountedPrice;
                        var cartDetailsArray = (JArray)cartData["cart_details"];
                        // Update the subtotal and total based on cart details
                        var subtotal = cartDetailsArray.Sum(item => (int)item["price"] * (int)item["qty"]);
                        var total = cartDetailsArray.Sum(item => (int)item["total_price"]);
                        // Update the cart totals
                        cartData["subtotal"] = subtotal;
                        cartData["subtotal_price"] = subtotal;
                        cartData["total"] = total;
                        cartData["is_sent_sync"] = 0;
                        // Save the updated cart data back to the file
                        File.WriteAllText(configCart, cartData.ToString(Formatting.Indented));

                    }
                    else
                    {
                        MessageBox.Show("Item not found in the cart.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Cart file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void LoadDataDiscount()
        {
            try
            {
                // Load menu data from local file if available
                if (File.Exists(folder + "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data"))
                {
                    string json = File.ReadAllText(folder + "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data");
                    DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(json);
                    List<DataDiscountCart> data = menuModel.data;
                    dataDiscount = data;
                    dataDiskonList = data;
                    var options = data;
                    options.Insert(0, new DataDiscountCart { id = -1, code = "Tidak ada Diskon" });
                    cmbDiskon.DataSource = options;
                    cmbDiskon.DisplayMember = "code";
                    cmbDiskon.ValueMember = "id";

                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal tampil data diskon " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

        }

        private void cmbDiskon_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private async void cekKeranjangDiskon()
        {
            if (await cekPeritemDiskon() != 0)
            {
                MessageBox.Show("Memasang Diskon Peritem, Diskon Cart akan di Hapus");
            }
        }
        private async Task<int> cekPeritemDiskon()
        {
            try
            {
                ApiService apiService = new ApiService();
                string response = await apiService.Get("/cart?outlet_id=" + baseOutlet);
                GetCartModel dataModel = JsonConvert.DeserializeObject<GetCartModel>(response);
                if (dataModel.data != null)
                {
                    if (dataModel.data.discount_id != 0)
                    {
                        return 1;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal cek diskon: " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

            return 0;
        }

        private void txtKuantitas_TextChanged(object sender, EventArgs e)
        {
            if (is_ordered == "1")
            {
                if (int.Parse(txtKuantitas.Text) > kuantitas)
                {
                    MessageBox.Show("Item Ordered tidak dapat menambah kuantitas, silahkan tambah dari Menu :)");
                    txtKuantitas.Text = kuantitas.ToString();
                }
            }
        }
    }
}
