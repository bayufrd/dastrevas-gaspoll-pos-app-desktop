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
namespace KASIR.Komponen
{
    public partial class updateCartForm : Form
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
        public updateCartForm(string id, string cartdetailid)
        {
            InitializeComponent();
            lblNameCart.Text = "Downloading Data...";
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
                    IApiService apiService = new ApiService();
                    string response = await apiService.GetMenuDetailByID("/menu-detail", idmenu);
                    GetMenuDetailCartModel menuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(response);
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

                LoadItemOnCart();
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
        private async void LoadItemOnCart()
        {
            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.GetItemOnCart("cart/" + cartdetail + "?outlet_id=" + baseOutlet);
                GetItemOnCartByIdModel itemModel = JsonConvert.DeserializeObject<GetItemOnCartByIdModel>(response);

                if (itemModel == null || itemModel.data == null)
                {
                    MessageBox.Show("Data tidak ditemukan atau response kosong (Masalah jaringan.");
                    return;
                }

                DataItemOnCart data = itemModel.data;
                LoadDataDiscount(data.discount_id);

                is_ordered = data.is_ordered?.ToString() ?? "";

                List<DataDiscountCart> dataDiscounts = dataDiscount;

                lblTipe.Text = "Tipe Penjualan : " + (data.serving_type_name?.ToString() ?? "Tidak tersedia");

                txtKuantitas.Text = data.qty.ToString() ?? "0";
                kuantitas = int.Parse(txtKuantitas.Text);

                txtNotes.Text = data.note_item?.ToString() ?? "";

                lblVarian.Text = "Varian : " + (data.varian?.ToString() ?? "Tidak tersedia");
                lblVarian.Enabled = true;
                lblTipe.Enabled = true;

                searchTextserving = data.serving_type_name?.ToString() ?? "";

                lblnamaitem = data.menu_name?.ToString() ?? "Tidak tersedia";

                LoadDataServingType();

                // Set varian
                searchText = data.varian?.ToString() ?? "";
                if (!string.IsNullOrEmpty(searchText))
                {
                    for (int kimak = 0; kimak < cmbVarian.Items.Count; kimak++)
                    {
                        cmbVarian.SelectedIndex = kimak;
                        if (cmbVarian.Text.ToString() == searchText)
                        {
                            cmbVarian.SelectedIndex = kimak;
                            break;
                        }
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
                MessageBox.Show("Gagal tampil data " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }






        private async void LoadDataServingType()
        {
            try
            {
                if (File.Exists(folder + "\\LoadDataServingType_" + idmenu + "_Outlet_" + baseOutlet + ".data"))
                {
                    string json = File.ReadAllText(folder + "\\LoadDataServingType_" + idmenu + "_Outlet_" + baseOutlet + ".data");
                    GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(json);
                    DataMenu data = menuModel.data;
                    var options = data.serving_types;

                    //options.Insert(0, new ServingType { id = -1, name = "Pilih Tipe Serving" });
                    //default

                    comboBox1.DataSource = options;
                    comboBox1.DisplayMember = "name";
                    comboBox1.ValueMember = "id";
                    if (!string.IsNullOrEmpty(searchTextserving))
                    {
                        for (int kimak = 0; kimak < comboBox1.Items.Count; kimak++)
                        {
                            comboBox1.SelectedIndex = kimak;
                            if (comboBox1.Text.ToString() == searchTextserving)
                            {
                                comboBox1.SelectedIndex = kimak;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    IApiService apiService = new ApiService();
                    string response = await apiService.GetMenuByID("/menu", idmenu);
                    GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(response);
                    DataMenu data = menuModel.data;
                    var options = data.serving_types;

                    //options.Insert(0, new ServingType { id = -1, name = "Pilih Tipe Serving" });
                    //default

                    comboBox1.DataSource = options;
                    comboBox1.DisplayMember = "name";
                    comboBox1.ValueMember = "id";
                    if (!string.IsNullOrEmpty(searchTextserving))
                    {
                        for (int kimak = 0; kimak < comboBox1.Items.Count; kimak++)
                        {
                            comboBox1.SelectedIndex = kimak;
                            if (comboBox1.Text.ToString() == searchTextserving)
                            {
                                comboBox1.SelectedIndex = kimak;
                                break;
                            }
                        }
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
            finally
            {
                await WaitForSecondsAsync(1);
                btnHapus.Enabled = true;
                btnSimpan.Enabled = true;
                btnTambah.Enabled = true;
                btnKurang.Enabled = true;
                lblNameCart.Text = lblnamaitem.ToString();
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
            //LoggerUtil.LogPrivateMethod(nameof(btnHapus_Click));

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
                lblNameCart.Text = "Mengirim Data...";

                using (deletePerItemForm deletePerItemForm = new deletePerItemForm(cartdetail))
                {
                    deletePerItemForm.Owner = background;

                    background.Show();

                    //DialogResult dialogResult = dataBill.ShowDialog();

                    //background.Dispose();
                    DialogResult result = deletePerItemForm.ShowDialog();

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
                    lblNameCart.Text = "Mengirim Data...";

                    IApiService apiService = new ApiService();
                    HttpResponseMessage response = await apiService.DeleteCart("/cart/" + cartdetail + "?outlet_id=" + baseOutlet);
                    if (response != null)
                    {
                        DialogResult = DialogResult.OK;
                        if (response.IsSuccessStatusCode)
                        {
                            DialogResult = DialogResult.OK;

                            if (Application.OpenForms["masterPos"] is masterPos masterPosForm)
                            {
                                // Call a method in the MasterPos form to refresh the cart
                                masterPosForm.LoadCart(); // You'll need to define this method in MasterPos
                            }
                            /*
                            DialogResult result = MessageBox.Show("Menu berhasil dihapus", "Gaspol", MessageBoxButtons.OK);

                            if (result == DialogResult.OK)
                            {
                               
                                ReloadDataInBaseForm = true;

                            }
                            this.DialogResult = result;
                            */
                            Close();
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
                    DialogResult = DialogResult.Cancel;
                    MessageBox.Show("Gagal tampil data " + ex.Message);
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
            //LoggerUtil.LogPrivateMethod(nameof(btnTambah_Click_1));

            if (int.TryParse(txtKuantitas.Text, out int numericValue))
            {
                numericValue++;
                txtKuantitas.Text = numericValue.ToString();
            }
        }

        private void btnKurang_Click_1(object sender, EventArgs e)
        {
            //LoggerUtil.LogPrivateMethod(nameof(btnKurang_Click_1));
            if (is_ordered == "1")
            {
                if (cmbDiskon.SelectedIndex != 0)
                {
                    cmbDiskon.SelectedIndex = 0;
                    MessageBox.Show("Jika melakukan perubahan kuantitas, Diskon akan direset.", "Peringatan!");
                }
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
            //LoggerUtil.LogPrivateMethod(nameof(btnSimpan_Click));
            cekKeranjangDiskon();

            try
            {
                int selectedVarian = int.Parse(cmbVarian.SelectedValue.ToString());
                int selectedDiskon = int.Parse(cmbDiskon.SelectedValue.ToString());
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
                int serving_type = int.Parse(comboBox1.SelectedValue.ToString());
                var quantity = int.Parse(txtKuantitas.Text.ToString());
                var notes = txtNotes.Text.ToString();
                int? menuDetailIdValue = null;
                string pricefix = "0";

                if (selectedVarian == null || selectedVarian == -1)
                {
                    // MenuDetailDataCart paramData = menuDetailDataCarts[0];
                    pricefix = await returnPriceByServingTypeAsync(serving_type.ToString(), "0");
                }
                else
                {
                    // MenuDetailDataCart paramData = menuDetailDataCarts[selectedVarian];
                    pricefix = await returnPriceByServingTypeAsync(serving_type.ToString(), "" + selectedVarian);
                }
                if (selectedDiskon == -1)
                {
                    diskon = 0;
                }
                else
                {
                    diskon = selectedDiskon;
                    if (diskon != -1)
                    {
                        int diskonMinimum = dataDiskonList.FirstOrDefault(d => d.id == diskon)?.min_purchase ?? -1;
                        if (diskonMinimum > (int.Parse(pricefix) * quantity))
                        {
                            int resultDiskon = diskonMinimum - (int.Parse(pricefix) * quantity);
                            MessageBox.Show("Minimum diskon kurang Rp " + resultDiskon + " lagi", "Gaspol");
                            return;
                        }
                    }
                }
                lblNameCart.Text = "Mengirim Data...";
                btnSimpan.Enabled = false;
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

                    using (updatePerItemForm updatePerItemForm = new updatePerItemForm(json, cartdetail))
                    {
                        updatePerItemForm.Owner = background;

                        background.Show();

                        DialogResult dialogResult = updatePerItemForm.ShowDialog();

                        background.Dispose();

                        if (dialogResult == DialogResult.OK)
                        {
                            masterPos m = new masterPos();
                            m.ReloadCart();
                            DialogResult = DialogResult.OK;

                            this.Close();
                        }
                    }
                }
                else
                {
                    IApiService apiService = new ApiService();
                    HttpResponseMessage response = await apiService.UpdateCart(jsonString, "/cart/" + cartdetail);
                    if (response != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {

                            if (Application.OpenForms["masterPos"] is masterPos masterPosForm)
                            {
                                // Call a method in the MasterPos form to refresh the cart
                                masterPosForm.LoadCart();
                            }
                            DialogResult = DialogResult.OK;
                            Close();
                            /*
                        DialogResult result = MessageBox.Show("Menu berhasil diperbaharui", "Gaspol", MessageBoxButtons.OK);

                        if (result == DialogResult.OK)
                        {
                            ReloadDataInBaseForm = true;

                        }
                        this.DialogResult = result;
                            */
                        }
                        else
                        {
                            DialogResult = DialogResult.Cancel;
                            MessageBox.Show("Menu gagal diperbaharui", "Gaspol");
                            Close();
                        }
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
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show("Gagal ubah data " + ex.Message, "Gaspol");
            }

        }

        private async void LoadDataDiscount(int? discount_id)
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



                    if (discount_id != null)
                    {
                        DataDiscountCart selectedData = data.FirstOrDefault(item => item.id == discount_id);
                        /*   DataDiscountCart dataDiscountCart = dataDiscount.Where(x => x.id == data.discount_id).First();
                           lblDiscount.Text = "Diskon : " + dataDiscountCart.code.ToString();*/
                        //default varian


                        if (selectedData != null)
                        {
                            lblDiscount.Text = "Diskon : " + selectedData.code.ToString();
                            string searchText = selectedData.code?.ToString() ?? "";
                            if (searchText != "")
                            {
                                for (int kimak = 0; kimak <= cmbDiskon.Items.Count; kimak++)
                                {
                                    cmbDiskon.SelectedIndex = kimak;
                                    if (cmbDiskon.Text.ToString() == searchText)
                                    {
                                        cmbDiskon.SelectedIndex = kimak;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            lblDiscount.Text = "Diskon : ";
                        }

                    }
                }
                else
                {
                    IApiService apiService = new ApiService();
                    string response = await apiService.GetDiscount($"/discount?outlet_id={baseOutlet}&is_discount_cart=", "0");
                    DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(response);
                    List<DataDiscountCart> data = menuModel.data;
                    dataDiscount = data;
                    dataDiskonList = data;
                    var options = data;
                    options.Insert(0, new DataDiscountCart { id = -1, code = "Tidak ada Diskon" });
                    cmbDiskon.DataSource = options;
                    cmbDiskon.DisplayMember = "code";
                    cmbDiskon.ValueMember = "id";



                    if (discount_id != null)
                    {
                        DataDiscountCart selectedData = data.FirstOrDefault(item => item.id == discount_id);
                        /*   DataDiscountCart dataDiscountCart = dataDiscount.Where(x => x.id == data.discount_id).First();
                           lblDiscount.Text = "Diskon : " + dataDiscountCart.code.ToString();*/
                        //default varian


                        if (selectedData != null)
                        {
                            lblDiscount.Text = "Diskon : " + selectedData.code.ToString();
                            string searchText = selectedData.code?.ToString() ?? "";
                            if (searchText != "")
                            {
                                for (int kimak = 0; kimak <= cmbDiskon.Items.Count; kimak++)
                                {
                                    cmbDiskon.SelectedIndex = kimak;
                                    if (cmbDiskon.Text.ToString() == searchText)
                                    {
                                        cmbDiskon.SelectedIndex = kimak;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            lblDiscount.Text = "Diskon : ";
                        }

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
