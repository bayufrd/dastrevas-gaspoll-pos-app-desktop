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


namespace KASIR.komponen
{
    [Serializable]
    public partial class addCartForm : Form
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
        public addCartForm(string id, string name)
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            btnSimpan.Enabled = false;
            lblNameCart.Text = "Downloading Data...";
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


        private bool IsComboBoxEmpty()
        {
            //return comboBox1.Items.Count == 0;
            // Assuming comboBox1 is the name of your ComboBox
            if (cmbVarian.Items.Count > 2)
            {
                object itemAtIndex1 = cmbVarian.Items[2];

                if (itemAtIndex1 == null || string.IsNullOrEmpty(itemAtIndex1.ToString()))
                {
                    // Index 1 is empty
                    return true;
                }
            }

            // Index 1 is not empty or ComboBox doesn't have enough items
            return false;
        }
        private void CheckDropdownItems()
        {
            // Assuming comboBox1 is the name of your ComboBox
            foreach (var item in cmbVarian.Items)
            {
                // Your logic to check each item
                if (cmbVarian.Items == null)
                {
                    cmbVarian.Enabled = true;
                }
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
                IApiService apiService = new ApiService();
                string response = await apiService.GetMenuDetailByID("/menu-detail", idmenu);
                GetMenuDetailCartModel menuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(response);
                // Save the menu data to a local file
                File.WriteAllText(folder + "\\LoadDataVarian_" + idmenu + "_Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(menuModel));
                DataMenuDetail data = menuModel.data;

                var options = data.menu_details.Where(x => x.menu_detail_id != 0).ToList();
                options.Insert(0, new MenuDetailDataCart { index = -1, varian = "Normal" });
                cmbVarian.DataSource = options;
                cmbVarian.DisplayMember = "varian";
                cmbVarian.ValueMember = "menu_detail_id";
                cmbVarian.Font = new Font(comboBox1.Font.FontFamily, 12); // Set font size to 14

                // Set the item height to increase spacing
                cmbVarian.ItemHeight = 30; // Set item height to 30

                // Optionally, set the ComboBox drop-down width to accommodate larger text
                cmbVarian.DropDownWidth = 200; // Adjust width as necessary
                menuDetailDataCarts = data.menu_details;
                datas = menuModel.data;

                LoadDataDiscount();
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                IApiService apiService = new ApiService();
                string response = await apiService.GetMenuByID("/menu", idmenu);
                GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(response);
                // Save the menu data to a local file
                File.WriteAllText(folder + "\\LoadDataServingType_" + idmenu + "_Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(menuModel));
                DataMenu data = menuModel.data;
                var options = data.serving_types;
                cmbVarian.SelectedItem = 1;
                comboBox1.DataSource = options;
                comboBox1.DisplayMember = "name";
                comboBox1.ValueMember = "id";
                servingType = data.serving_types;
                btnSimpan.Enabled = true;
                lblNameCart.Text = namelabel;

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
        public static async Task WaitForSecondsAsync(int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
        }
        private async Task<string> returnPriceByServingTypeAsync(string id, string varian)
        {
            IApiService apiService = new ApiService();
            string response = await apiService.GetMenuDetailByID("/menu-detail", "" + idmenu + "?menu_detail_id=" + varian);

            if (string.IsNullOrWhiteSpace(response))
            {
                throw new InvalidOperationException("API response is empty or null.");
            }

            GetMenuDetailCartModel menuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(response);

            if (menuModel == null || menuModel.data == null)
            {
                throw new InvalidOperationException("Deserialization failed: menuModel or data is null.");
            }

            DataMenuDetail data = menuModel.data;
            List<MenuDetailDataCart> menuDetailDataList = data.menu_details;

            if (menuDetailDataList == null || !menuDetailDataList.Any())
            {
                throw new InvalidOperationException("Menu details are null or empty.");
            }

            List<ServingTypes> servingTypes = menuDetailDataList[0].serving_types;

            if (servingTypes == null || !servingTypes.Any())
            {
                throw new InvalidOperationException("Serving types are null or empty.");
            }

            var servingType = servingTypes.FirstOrDefault(serving => serving.id == int.Parse(id));

            if (servingType != null)
            {
                return servingType.price.ToString();
            }
            else
            {
                return "0";
            }
        }

        private async Task<string> Ex_returnPriceByServingTypeAsync(string id, string varian)
        {
            IApiService apiService = new ApiService();
            string response = await apiService.GetMenuDetailByID("/menu-detail", "" + idmenu + "?menu_detail_id=" + varian);
            GetMenuDetailCartModel menuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(response);
            DataMenuDetail data = menuModel.data;
            List<MenuDetailDataCart> menuDetailDataList = data.menu_details;
            List<ServingTypes> servingTypes = menuDetailDataList[0].serving_types;
            var servingType = servingTypes.FirstOrDefault(serving => serving.id == int.Parse(id));
            if (servingType != null)
            {
                return servingType.price.ToString();
            }
            else
            {
                return "0";
            }
        }


        private string returnMenuDetailIdByIndex(string id, List<MenuDetailDataCart> dataDetail)
        {
            string result = "0";
            result = dataDetail[int.Parse(id)].menu_detail_id.ToString();
            return result;
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
                lblNameCart.Text = "Mengirim Data...";

                Dictionary<string, object> json = new Dictionary<string, object>
    {
        { "outlet_id", baseOutlet },
        { "menu_id", datas.id },
        { "serving_type_id", serving_type },
        { "price", pricefix },
        { "discount_id", diskon.ToString() },
        { "qty", quantity },
        { "note_item", notes }
    };

                if (selectedVarian != null && selectedVarian != -1)
                {
                    json["menu_detail_id"] = selectedVarian;
                    // Now you can use the selectedValueAsString as needed
                }

                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                IApiService apiService = new ApiService();
                HttpResponseMessage response = await apiService.CreateCart(jsonString, "/cart");

                /*if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Menu gagal ditambahkan silahkan coba ulang", "Gaspol");
                        DialogResult = DialogResult.Cancel;
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("Gagal tambah data silahkan coba ulang" + response.ToString(), "Gaspol");
                    DialogResult = DialogResult.Cancel;
                }*/
                masterPos m = new masterPos();
                m.ReloadCart();
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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

        private async void Ex_btnSimpan_ClickAsync(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnSimpan_ClickAsync));
            btnSimpan.Enabled = false;

            try
            {
                if (!await ValidateInputsAsync())
                {
                    return;
                    btnSimpan.Enabled = true;
                }

                int selectedVarian = int.Parse(cmbVarian.SelectedValue.ToString());
                int selectedDiskon = int.Parse(cmbDiskon.SelectedValue.ToString());
                int diskon = 0;

                int serving_type = int.Parse(comboBox1.SelectedValue.ToString());
                var quantity = int.Parse(txtKuantitas.Text.ToString());
                var notes = txtNotes.Text.ToString();
                int? menuDetailIdValue = null;
                string pricefix = "0";

                if (selectedVarian == null || selectedVarian == -1)
                {
                    pricefix = await returnPriceByServingTypeAsync(serving_type.ToString(), "0");
                }
                else
                {
                    pricefix = await returnPriceByServingTypeAsync(serving_type.ToString(), "" + selectedVarian);
                }

                if (selectedDiskon != -1)
                {
                    diskon = selectedDiskon;
                    if (!await ValidateDiscountAsync(diskon, int.Parse(pricefix), quantity))
                        return;
                }

                await SendDataAsync(serving_type, pricefix, diskon, quantity, notes, selectedVarian);
            }
            catch (Exception ex)
            {
                btnSimpan.Enabled = true;
                MessageBox.Show("Gagal tambah data silahkan coba ulang" + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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
                    MessageBox.Show("Gagal tambah data " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.GetDiscount($"/discount?outlet_id={baseOutlet}&is_discount_cart=", "0");
                DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(response);
                File.WriteAllText(folder + "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(menuModel));
                List<DataDiscountCart> data = menuModel.data;
                var options = data;
                dataDiskonList = data;
                options.Insert(0, new DataDiscountCart { id = -1, code = "Pilih Diskon" });
                cmbDiskon.DataSource = options;
                cmbDiskon.DisplayMember = "code";
                cmbDiskon.ValueMember = "id";

                LoadDataServingType();
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
