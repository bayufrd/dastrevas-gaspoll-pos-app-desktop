using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using System.Timers;
using FontAwesome.Sharp;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using Menu = KASIR.Model.Menu;


namespace KASIR.komponen
{
    public partial class masterPos : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;

        private ApiService apiService;
        private DataTable listDataTable;
        string totalCart;
        string cartID;
        string customer_name;
        string customer_seat;
        private readonly string baseOutlet;
        private readonly string baseUrl;
        private BindingSource bindingSource = new BindingSource();
        private DataTable dataTable2;
        List<DataDiscountCart> dataDiscountListCart;
        int subTotalPrice;
        // List<DataTable> dataList;
        Dictionary<Menu, Image> menuImageDictionary = new Dictionary<Menu, Image>();
        public bool ReloadDataInBaseForm { get; private set; }

        //untuk search
        private Dictionary<string, DataGridViewRow> nameToRowMap;

        //untuk delete
        Dictionary<string, object> jsondict;
        int isSplitted = 0, diskonID = 0;
        // Declare a private field to store the original data source

        // Create two separate FlowLayoutPanels
        List<Panel> originalPanelControls;
        //hitung items
        int items = 0;

        private string configFolderPath = "setting";
        private string configFilePath = "setting\\configListMenu.data";

        //for paging
        private int currentPageIndex = 1;
        private int pageSize = 35;
        private int totalPageCount = 0;
        private List<Menu> allMenuItems;
        private bool isLoading = false;
        private bool allDataLoaded = false;

        public masterPos()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            baseUrl = Properties.Settings.Default.BaseAddress;
            InitializeComponent();
            apiService = new ApiService();
            panel8.Margin = new Padding(0, 0, 0, 0);       // No margin at the bottom
            dataGridView3.Margin = new Padding(0, 0, 0, 0);
            LoadCart();

            //LoadConfig();
            txtCariMenu.Enabled = false;

            InitializeComboBox();
            InitializeVisualRounded();
            //paging begin
            btnCari.Visible = false;
            dataGridView3.Scroll += dataGridView3_Scroll;

            //peging end
            this.Shown += MasterPos_ShownWrapper; //LoadData();

            cmbFilter.SelectedIndexChanged += cmbFilter_SelectedIndexChanged;

            bindingSource.DataSource = dataTable2;

            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.DrawMode = DrawMode.OwnerDrawVariable;
            cmbFilter.DrawItem += CmbFilter_DrawItem;
            cmbFilter.ItemHeight = 25;

            // Mengaitkan event handler dengan form utama
            KeyPreview = true;
            KeyDown += YourForm_KeyDown;
        }
        private void YourForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Periksa apakah Ctrl dan Space ditekan bersamaan
            if (e.Control && e.KeyCode == Keys.F)
            {
                // Mengatur fokus ke CariMenu
                if (txtCariMenu.Visible == true)
                {
                    txtCariMenu.Focus();

                    // Menambahkan teks ke CariMenu (opsional)
                    txtCariMenu.Text += ">";

                    // Memindahkan kursor ke akhir teks di CariMenu
                    txtCariMenu.SelectionStart = txtCariMenu.Text.Length;
                    txtCariMenu.SelectionLength = 0;

                }
                else
                {
                    txtCariMenuList.Focus();

                    // Menambahkan teks ke CariMenu (opsional)
                    txtCariMenuList.Text += ">";

                    // Memindahkan kursor ke akhir teks di CariMenu
                    txtCariMenuList.SelectionStart = txtCariMenuList.Text.Length;
                    txtCariMenuList.SelectionLength = 0;
                }

                //e.Handled = true; // Menandai event sudah ditangani
            }

        }

        // Pagenation Begin
        private async Task LoadDataWithPagingAsync()
        {
            if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }
            // Load all menu data
            if (File.Exists($"DT-Cache\\menu_outlet_id_{baseOutlet}.data"))
            {
                string json = File.ReadAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data");
                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(json);
                allMenuItems = menuModel.data.ToList();
            }
            else
            {
                string response = await apiService.Get("/menu?outlet_id=" + baseOutlet);
                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                allMenuItems = menuModel.data.ToList();
                File.WriteAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data", JsonConvert.SerializeObject(menuModel));
            }

            totalPageCount = (int)Math.Ceiling((double)allMenuItems.Count / pageSize);

            await LoadCurrentPage();
        }
        private async Task LoadCurrentPage()
        {
            isLoading = true;

            var pagedData = allMenuItems.Skip((currentPageIndex - 1) * pageSize).Take(pageSize).ToList();
            dataGridView3.SuspendLayout();
            items = 0;

            foreach (var menu in pagedData)
            {
                items += 1;
                Panel tileButton = CreateTileButton(menu);
                dataGridView3.Controls.Add(tileButton);

                if (menu != pagedData.First())
                {
                    Panel spacerPanel = new Panel { Dock = DockStyle.Top, Height = 110, Width = 10 };
                    dataGridView3.Controls.Add(spacerPanel);
                }

                await LoadImageToPictureBox((PictureBox)tileButton.Controls[0].Controls[0], menu);
                lblCountingItems.Text = $"{items} items";
            }

            dataGridView3.ResumeLayout();
            dataGridView3.AutoScroll = true;
            txtCariMenu.Enabled = true;

            originalPanelControls = dataGridView3.Controls.OfType<Panel>().ToList();
            txtCariMenu.PlaceholderText = "Cari Menu Items...";

            isLoading = false;
            if (currentPageIndex >= totalPageCount)
            {
                allDataLoaded = true;
            }
        }

        private void dataGridView3_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll && !isLoading && !allDataLoaded)
            {
                if (dataGridView3.VerticalScroll.Value + dataGridView3.VerticalScroll.LargeChange >= dataGridView3.VerticalScroll.Maximum)
                {
                    currentPageIndex++;
                    LoadCurrentPage();
                }
            }
        }
        private Panel CreateTileButton(Menu menu)
        {
            Panel tileButton = new Panel
            {
                Width = 90,
                Height = 120
            };

            Panel pictureBoxPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            PictureBox pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };

            pictureBoxPanel.Controls.Add(pictureBox);
            tileButton.Controls.Add(pictureBoxPanel);

            Label nameLabel = new Label
            {
                Text = menu.name,
                ForeColor = Color.FromArgb(30, 31, 68),
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };
            tileButton.Controls.Add(nameLabel);

            Label typeLabel = new Label
            {
                Text = menu.menu_type,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            tileButton.Controls.Add(typeLabel);

            Label priceLabel = new Label
            {
                Text = string.Format("Rp. {0:n0},-", menu.price),
                ForeColor = Color.FromArgb(30, 31, 68),
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8, FontStyle.Regular)
            };
            tileButton.Controls.Add(priceLabel);

            pictureBox.Click += async (sender, e) => await HandlePictureBoxClick(menu);

            return tileButton;
        }
        private async void UpdateDisplayWithSearchResults(List<Menu> searchResults)
        {
            dataGridView3.SuspendLayout();
            dataGridView3.Controls.Clear();
            items = 0;

            foreach (var menu in searchResults)
            {
                items += 1;
                Panel tileButton = CreateTileButton(menu);  // Pastikan metode ini dipanggil dengan benar
                dataGridView3.Controls.Add(tileButton);

                if (menu != searchResults.First())
                {
                    Panel spacerPanel = new Panel { Dock = DockStyle.Top, Height = 110, Width = 10 };
                    dataGridView3.Controls.Add(spacerPanel);
                }

                // Muat ulang gambar untuk setiap menu
                await ReloadImageToPictureBox((PictureBox)tileButton.Controls[0].Controls[0], menu);
                lblCountingItems.Text = $"{items} items";
            }

            dataGridView3.ResumeLayout();
            dataGridView3.AutoScroll = true;
            txtCariMenu.Enabled = true;
            originalPanelControls = dataGridView3.Controls.OfType<Panel>().ToList();
            txtCariMenu.PlaceholderText = "Cari Menu Items...";
        }
        private async Task ReloadImageToPictureBox(PictureBox pictureBox, Menu menu)
        {
            Image cachedImage = await LoadImageFromCache(menu.id.ToString());

            if (cachedImage != null)
            {
                menuImageDictionary[menu] = cachedImage;

                if (pictureBox.InvokeRequired)
                {
                    pictureBox.Invoke((MethodInvoker)delegate
                    {
                        pictureBox.Image = cachedImage;
                    });
                }
                else
                {
                    pictureBox.Image = cachedImage;
                }
            }
            else
            {
                try
                {
                    Image downloadedImage = await LoadImageAsync(menu);
                    downloadedImage = ResizeImage(downloadedImage, 70);
                    CheckImageSize(downloadedImage);

                    menuImageDictionary[menu] = downloadedImage;

                    if (pictureBox.InvokeRequired)
                    {
                        pictureBox.Invoke((MethodInvoker)delegate
                        {
                            pictureBox.Image = downloadedImage;
                        });
                    }
                    else
                    {
                        pictureBox.Image = downloadedImage;
                    }

                    SaveImageToCache(downloadedImage, menu.id.ToString());
                }
                catch (ArgumentException ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
        }

        private async Task HandlePictureBoxClick(Menu menu)
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

            using (addCartForm addCartForm = new addCartForm(menu.id.ToString(), menu.name.ToString()))
            {
                addCartForm.Owner = background;
                background.Show();

                DialogResult result = addCartForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    background.Dispose();
                    LoadCart();
                }
                else
                {
                    MessageBox.Show("Gagal Menambahkan item, silahkan coba lagi");
                    ReloadCart();
                    LoadCart();
                    background.Dispose();
                }
            }
        }


        // pagenation end

        //Config pengaturan list view
        private async Task LoadConfig()
        {
            if (!Directory.Exists("setting"))
            {
                Directory.CreateDirectory(configFolderPath);
            }

            if (!File.Exists("setting\\configListMenu.data"))
            {
                string data = "OFF";
                await File.WriteAllTextAsync("setting\\configListMenu.data", data);
            }
            else
            {
                string allSettingsData = await File.ReadAllTextAsync("setting\\configListMenu.data");

                if (!string.IsNullOrEmpty(allSettingsData))
                {
                    if (allSettingsData == "ON")
                    {
                        await OnListView();

                    }
                    else
                    {
                        await OfflistView();
                    }
                }
                else
                {
                    await OfflistView();
                    string data = "OFF";
                    await File.WriteAllTextAsync("setting\\configListMenu.data", data);
                }
            }
        }


        private async Task OnListView()
        {
            dataGridView3.Enabled = false;
            dataGridView3.Visible = false;
            dataGridView2.Enabled = true;
            dataGridView2.Visible = true;
            txtCariMenuList.Visible = false;
            txtCariMenuList.Enabled = false;
            txtCariMenu.Visible = true;
            txtCariMenu.Enabled = true;
            await LoadDataListby();
            dataGridView2.CellFormatting += DataGridView2_CellFormatting;
        }



        private async Task OfflistView()
        {
            dataGridView3.Enabled = true;
            dataGridView3.Visible = true;
            dataGridView2.Enabled = false;
            dataGridView2.Visible = false;
            txtCariMenuList.Visible = false;
            txtCariMenuList.Enabled = false;
            //loadDataAsync();
            await LoadDataWithPagingAsync();
        }
        private void DataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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

        private async Task MasterPos_Shown(object sender, EventArgs e)
        {
            //await loadDataAsync();
            await LoadConfig();
        }

        private async void MasterPos_ShownWrapper(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                // Do any long-running work here

                // Invoke the MasterPos_Shown method on the UI thread
                this.Invoke((MethodInvoker)delegate
                {
                    MasterPos_Shown(sender, e);
                });
            });
        }

        private void InitializeVisualRounded()
        {
            // Create a GraphicsPath object with rounded rectangles
            RoundedPanel(panel8);
        }

        public void RoundedPanel(Panel panel)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 20; // adjust the radius value to change the roundness of the corners
            path.AddLine(panel.Left, panel.Top + radius, panel.Left, panel.Top);
            path.AddArc(panel.Left, panel.Top, radius, radius, 180, 90);
            path.AddLine(panel.Left + radius, panel.Top, panel.Width - radius, panel.Top);
            path.AddArc(panel.Width - radius, panel.Top, radius, radius, 270, 90);
            path.AddLine(panel.Width, panel.Top + radius, panel.Width, panel.Bottom - radius);
            path.AddArc(panel.Width - radius, panel.Bottom - radius, radius, radius, 0, 90);
            path.AddLine(panel.Width - radius, panel.Bottom, panel.Left + radius, panel.Bottom);
            path.AddArc(panel.Left, panel.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            panel.Region = new Region(path);
            panel.Location = new Point(panel.Left, panel.Top);
        }

        private void PayForm_KeluarButtonClicked(object sender, EventArgs e)
        {
            // Refresh the data when the Keluar button in payForm is clicked
            ////LoggerUtil.LogPrivateMethod(nameof(PayForm_KeluarButtonClicked));

            loadDataAsync();
        }
        private void InitializeComboBox()
        {
            cmbFilter.Items.Add("Semua");
            cmbFilter.Items.Add("Makanan");
            cmbFilter.Items.Add("Minuman");
            cmbFilter.Items.Add("Additional Makanan");
            cmbFilter.Items.Add("Additional Minuman");
            cmbFilter.SelectedIndex = 0;
            /*          cmbFilter.Items.Add("Dessert");*/

        }

        private void CmbFilter_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();

                int verticalMargin = 5;
                string itemText = cmbFilter.GetItemText(cmbFilter.Items[e.Index]);

                e.Graphics.DrawString(itemText, e.Font, Brushes.Black, new Rectangle(e.Bounds.Left, e.Bounds.Top + verticalMargin, e.Bounds.Width, e.Bounds.Height - verticalMargin));

                e.DrawFocusRectangle();

            }
        }

        private async Task LoadDataDiscount()
        {

            try
            {
                string folderAddCartForm = "DT-Cache\\addCartForm";
                if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }
                if (!Directory.Exists(folderAddCartForm)) { Directory.CreateDirectory(folderAddCartForm); }
                // Load all menu data
                if (File.Exists($"{folderAddCartForm}\\LoadDataDiscountItem_Outlet_{baseOutlet}.data"))
                {
                    string json = File.ReadAllText("DT-Cache" + "\\LoadDiscountPerCart_" + "Outlet_" + baseOutlet + ".data");
                    DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(json);
                    List<DataDiscountCart> data = menuModel.data;
                    var options = data;
                    dataDiscountListCart = data;
                    options.Insert(0, new DataDiscountCart { id = -1, code = "Tidak ada Diskon" });
                    cmbDiskon.DataSource = options;
                    cmbDiskon.DisplayMember = "code";
                    cmbDiskon.ValueMember = "id";
                }
                else
                {
                    IApiService apiService = new ApiService();
                    string response = await apiService.GetDiscount($"/discount?outlet_id={baseOutlet}&is_discount_cart=", "1");
                    DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(response);
                    File.WriteAllText("DT-Cache" + "\\LoadDiscountPerCart_" + "Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(menuModel));
                    List<DataDiscountCart> data = menuModel.data;
                    var options = data;
                    dataDiscountListCart = data;
                    options.Insert(0, new DataDiscountCart { id = -1, code = "Tidak ada Diskon" });
                    cmbDiskon.DataSource = options;
                    cmbDiskon.DisplayMember = "code";
                    cmbDiskon.ValueMember = "id";
                }
                string searchText = diskonID.ToString() ?? "";
                if (searchText != "0")
                {
                    for (int kimak = 0; kimak < cmbDiskon.Items.Count; kimak++)
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
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal tampil data diskon " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void txtCariMenu_TextChanged(object sender, EventArgs e)
        {
            if (dataGridView3.Visible == true)
            {
                if (txtCariMenu.Text != "" && txtCariMenu.Text != null)
                {
                    btnCari.Visible = true;
                }
                else
                {
                    PerformSearch();
                    btnCari.Visible = false;
                }
                //PerformSearch();
            }
            if (dataGridView2.Visible == true)
            {
                PerformSearchList();
            }
            ////LoggerUtil.LogPrivateMethod(nameof(txtCariMenu_TextChanged));

        }
        private void PerformSearchList()
        {
            try
            {
                if (listDataTable == null)
                    return;

                string searchTerm = txtCariMenu.Text.ToLower();

                DataTable filteredDataTable = listDataTable.Clone();
                items = 0;
                IEnumerable<DataRow> filteredRows = listDataTable.AsEnumerable()
                    .Where(row => row.ItemArray.Any(field => field.ToString().ToLower().Contains(searchTerm)));

                foreach (DataRow row in filteredRows)
                {
                    filteredDataTable.ImportRow(row);
                    items++;
                }
                lblCountingItems.Text = $"{items} items";

                dataGridView2.DataSource = filteredDataTable;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private Dictionary<string, Control> nameToControlMap;

        private void InitializeNameToControlMap(FlowLayoutPanel panel)
        {
            nameToControlMap = new Dictionary<string, Control>();

            // Populate the nameToControlMap with the name and corresponding control for each control in the panel
            foreach (Control control in panel.Controls)
            {
                if (control is Panel tileButton && control.Controls.Count >= 2)
                {
                    Label nameLabel = control.Controls[1] as Label;
                    if (nameLabel != null)
                    {
                        string name = nameLabel.Text.ToLower();
                        nameToControlMap[name] = control;
                    }
                }
            }
        }
        private void PerformSearch()
        {
            string searchQuery = txtCariMenu.Text.ToLower();
            try
            {
                if (allMenuItems != null)
                {
                    // Filter the allMenuItems based on the search query
                    var searchResults = allMenuItems.Where(menu =>
                        menu.name.ToLower().Contains(searchQuery) ||
                        menu.menu_type.ToLower().Contains(searchQuery) ||
                        string.Format("Rp. {0:n0},-", menu.price).ToLower().Contains(searchQuery)
                    ).ToList();

                    // Update the display with the search results
                    UpdateDisplayWithSearchResults(searchResults);
                }
                else
                {
                    return;
                }
            }
            /*
            try
            {
                //string json = File.ReadAllText("DT-Cache\\MenuListPanel_Outlet_" + baseOutlet + ".data");
                //List<Panel> originalPanelControls = JsonConvert.DeserializeObject<List<Panel>>(json);

                // Create a copy of the original data source
                var clonedPanelControls = new List<Panel>(originalPanelControls);
                // If the search query is empty, show the original data source
                if (string.IsNullOrEmpty(searchQuery))
                {
                    // Clear the original data source
                    dataGridView3.Controls.Clear();
                    items = 0;
                    // Add the original items to the original data source
                    foreach (var panelControl in originalPanelControls)
                    {
                        dataGridView3.Controls.Add(panelControl);
                        items++;
                    }


                }
                // Filter the Panel controls based on the search query
                var filteredPanelControls = clonedPanelControls.Where(panel =>
                {
                    if (panel.Controls.OfType<Label>().Any())
                    {
                        items = 0;

                        var labelControl = panel.Controls.OfType<Label>().First();
                        string itemName = labelControl.Text.ToLower();
                        return string.IsNullOrEmpty(searchQuery) || itemName.Contains(searchQuery);
                    }
                    return false;
                }).ToList();

                // Clear the original data source
                dataGridView3.Controls.Clear();

                // Add the filtered items to the original data source
                foreach (var panelControl in filteredPanelControls)
                {
                    dataGridView3.Controls.Add(panelControl);

                    items++;

                }

                lblCountingItems.Text = $"{items} items";
            }
            */
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }






        private async Task EmptyCart()
        {
            try
            {
                HttpResponseMessage responseMessage = await apiService.DeleteCart("/empty-cart?outlet_id=" + baseOutlet, "cart_id=" + cartID);

                if (responseMessage.IsSuccessStatusCode)
                {
                    //cartID = "";
                    //terjadi bug GhostMenu jika dihidupkan
                    ReloadCart();
                }
                else
                {
                    MessageBox.Show("Terjadi kesalahan saat menghapus keranjang. Silahkan coba lagi.");
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

            }
        }

        //button deletecart
        private async void button5_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                string response = await apiService.Get("/cart?outlet_id=" + baseOutlet);
                if (string.IsNullOrEmpty(response))
                {
                    // Handle the case where the API call returned no data
                    return;
                }

                GetCartModel dataModel = JsonConvert.DeserializeObject<GetCartModel>(response);
                if (dataModel == null || dataModel.data == null)
                {
                    // Handle the case where the deserialization failed or the data object is null
                    MessageBox.Show("Keranjang Kosong!");

                    return;
                }

                cartID = dataModel.data.cart_id.ToString();
                CheckIsOrderedModel data = JsonConvert.DeserializeObject<CheckIsOrderedModel>(await apiService.CheckIsOrdered("/check-ordered-cart?outlet_id=", baseOutlet));
                if (data.data?.is_ordered != 1)
                {
                    await EmptyCart(); //API Empty
                    return;
                }

                ////API DeleteCart

                using (var background = new Form
                {
                    StartPosition = FormStartPosition.Manual,
                    FormBorderStyle = FormBorderStyle.None,
                    Opacity = 0.7d,
                    BackColor = Color.Black,
                    WindowState = FormWindowState.Maximized,
                    TopMost = true,
                    Location = this.Location,
                    ShowInTaskbar = false,
                })
                using (var deleteForm = new deleteForm(cartID.ToString()))
                {
                    deleteForm.Owner = background;
                    background.Show();

                    DialogResult result = deleteForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        background.Dispose();
                        ReloadCart();
                        LoadCart();
                    }
                    else
                    {
                        MessageBox.Show("Gagal hapus keranjang, Silahkan coba lagi");
                        ReloadCart();
                        LoadCart();
                        background.Dispose();
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

                MessageBox.Show("An error occurred while retrieving data: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }
        public async Task LoadDataListby()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("No network connection available. Please check your internet connection and try again.", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                //dataGridView3.Enabled = false;
                //string response = await apiService.Get("/menu?outlet_id=" + baseOutlet);
                items = 0;

                items = 0;
                string json; // Deklarasikan json di luar blok if-else

                if (!File.Exists($"DT-Cache\\menu_outlet_id_{baseOutlet}.data"))
                {
                    json = await apiService.Get("/menu?outlet_id=" + baseOutlet);
                    var apiMenuModel = JsonConvert.DeserializeObject<GetMenuModel>(json);

                    File.WriteAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data", JsonConvert.SerializeObject(apiMenuModel));

                }
                else
                {
                    json = File.ReadAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data");
                }

                // Deserialize json setelah memastikan json memiliki nilai
                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(json);
                List<Menu> menuList = menuModel.data.ToList();

                dataGridView2.Controls.Clear();

                DataTable dataTable2 = new DataTable();

                dataTable2.Columns.Add("MenuID", typeof(int));
                dataTable2.Columns.Add("Nama Menu", typeof(string)); // Change this line
                dataTable2.Columns.Add("Menu Type", typeof(string));
                dataTable2.Columns.Add("Menu Price", typeof(string));

                foreach (var Menu in menuList)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = $"Downloading Data...[{items} / {menuModel.data.Count}]";
                    });
                    dataTable2.Rows.Add(Menu.id, Menu.name.ToString(), Menu.menu_type.ToString(), string.Format("Rp. {0:n0},-", Menu.price));
                    items++;
                }
                // Set the column header name
                dataTable2.Columns["Nama Menu"].ColumnName = "Nama Menu";

                dataGridView2.DataSource = dataTable2;
                listDataTable = dataTable2.Copy();

                dataGridView2.Columns["MenuID"].Visible = false;
                dataGridView2.Columns["Nama Menu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.Invoke((MethodInvoker)delegate
                {
                    txtCariMenu.PlaceholderText = "Cari menu items...";
                    lblCountingItems.Text = $"{items} items";
                });
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while retrieving data: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);


            }
        }

        private void DataGridView2_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show("Demo ListView! Contact PM Project");

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView2.Rows[e.RowIndex];
                string id = selectedRow.Cells["MenuID"].Value.ToString();
                DataGridViewRow selectedRow2 = dataGridView2.Rows[e.RowIndex];
                string nama = selectedRow2.Cells["Nama Menu"].Value.ToString();


                //var selectedMenu = menuList[e.RowIndex];

                CartDetailClick(id, nama);

            }
        }
        private void CartDetailClick(string id, string nama)
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

            using (addCartForm addCartForm = new addCartForm(id.ToString(), nama.ToString()))
            {
                addCartForm.Owner = background;

                background.Show();

                DialogResult result = addCartForm.ShowDialog();

                if (result == DialogResult.OK)
                {

                    background.Dispose();
                    LoadCart();
                    // Settings were successfully updated, perform any necessary actions
                }
                else
                {

                    MessageBox.Show("Gagal Menambahkan item, silahkan coba lagi");
                    ReloadCart();
                    LoadCart();
                    background.Dispose();
                }
            }
        }


        public async Task loadDataAsync()
        {

            if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }

            // Load menu data from local file if available
            if (File.Exists($"DT-Cache\\menu_outlet_id_{baseOutlet}.data"))
            {
                try
                {
                    items = 0;
                    string json = File.ReadAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data");
                    GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(json);
                    List<Menu> menuList = menuModel.data.ToList();
                    // Clear the existing controls and add new ones
                    dataGridView3.SuspendLayout();
                    dataGridView3.Controls.Clear();
                    foreach (Menu menu in menuList)
                    {
                        // ... same code as before to create and add controls ...
                        items += 1;
                        // Use BeginInvoke to marshal the call to the UI thread
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            txtCariMenu.PlaceholderText = $"Loading Data...[{items} / {menuModel.data.Count}]";
                        });

                        // Use EndInvoke to release the delegate when the call is complete
                        this.EndInvoke(this.BeginInvoke((MethodInvoker)delegate
                        {
                            txtCariMenu.PlaceholderText = $"Loading Data...[{items} / {menuModel.data.Count}]";
                        }));
                        Panel tileButton = new Panel();//Panel tileButton = new Panel();
                        tileButton.Width = 90;
                        tileButton.Height = 120;

                        Panel pictureBoxPanel = new Panel();
                        pictureBoxPanel.Dock = DockStyle.Fill;


                        tileButton.Controls.Add(pictureBoxPanel);

                        //RoundedPanel(pictureBoxPanel);
                        //pictureBoxPanel.BorderStyle = BorderStyle.None; // Add this line to set the border style

                        PictureBox pictureBox = new PictureBox();
                        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox.Dock = DockStyle.Fill;
                        pictureBox.BorderStyle = BorderStyle.None; // Add this line to set the border style

                        pictureBoxPanel.Controls.Add(pictureBox);
                        tileButton.Controls.Add(pictureBoxPanel); // Add the PictureBox to the Panel

                        //AutoSizeLabel nameLabel = new AutoSizeLabel();
                        Label nameLabel = new Label();
                        nameLabel.Text = menu.name;
                        nameLabel.ForeColor = Color.FromArgb(30, 31, 68);
                        nameLabel.Dock = DockStyle.Bottom;
                        nameLabel.TextAlign = ContentAlignment.MiddleCenter;
                        nameLabel.Font = new Font("Arial", 8, FontStyle.Bold);
                        //nameLabel.BackColor = Color.White;

                        tileButton.Controls.Add(nameLabel);

                        Label typeLabel = new Label();
                        typeLabel.Text = menu.menu_type;
                        typeLabel.Dock = DockStyle.Bottom;
                        typeLabel.TextAlign = ContentAlignment.MiddleCenter;
                        tileButton.Controls.Add(typeLabel);
                        //tileButton.Controls.Add(pictureBoxPanel); // Add the PictureBox to the Panel

                        Label priceLabel = new Label();
                        priceLabel.Text = string.Format("Rp. {0:n0},-", menu.price);
                        priceLabel.ForeColor = Color.FromArgb(30, 31, 68);
                        priceLabel.Dock = DockStyle.Bottom;
                        priceLabel.TextAlign = ContentAlignment.MiddleCenter;
                        priceLabel.Font = new Font("Arial", 8, FontStyle.Regular);
                        tileButton.Controls.Add(priceLabel);


                        // Send the pictureBox to the back
                        pictureBox.SendToBack();
                        if (menu != menuList.First())
                        {
                            Panel spacerPanel = new Panel();
                            spacerPanel.Dock = DockStyle.Top;
                            spacerPanel.Height = 110;
                            spacerPanel.Width = 10;// Set the height of the spacer panel
                            dataGridView3.Controls.Add(spacerPanel);
                        }

                        typeLabel.Visible = false;

                        // Use the Invoke method to marshal the call back to the UI thread
                        // this.Invoke((MethodInvoker)delegate
                        //{
                        pictureBox.Click += async (sender, e) =>
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

                            // Create the addCartForm on the UI thread
                            using (addCartForm addCartForm = new addCartForm(menu.id.ToString(), menu.name.ToString()))
                            {
                                addCartForm.Owner = background;

                                background.Show();

                                DialogResult result = addCartForm.ShowDialog();

                                if (result == DialogResult.OK)
                                {
                                    // Dispose of the background form now that the addCartForm form has been closed
                                    background.Dispose();
                                    LoadCart();
                                }
                                else
                                {
                                    MessageBox.Show("Gagal Menambahkan item, silahkan coba lagi");
                                    ReloadCart();
                                    LoadCart();
                                    // Dispose of the background form now that the addCartForm form has been closed
                                    background.Dispose();
                                }
                            }
                        };
                        //});
                        dataGridView3.ResumeLayout();
                        dataGridView3.Controls.Add(tileButton);
                        // Initialize the nameToControlMap dictionary after the dataGridView3 is populated
                        InitializeNameToControlMap(dataGridView3);


                        // Load gambar dari cache atau unduh jika tidak ada di cache
                        await LoadImageToPictureBox(pictureBox, menu);

                        // Use BeginInvoke to marshal the call to the UI thread
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            lblCountingItems.Text = $"{items} items";
                        });

                        // Use EndInvoke to release the delegate when the call is complete
                        this.EndInvoke(this.BeginInvoke((MethodInvoker)delegate
                        {
                            lblCountingItems.Text = $"{items} items";
                        }));
                    }

                    // Initialize the nameToControlMap dictionary after the dataGridView3 is populated
                    InitializeNameToControlMap(dataGridView3);

                    // Resume the layout engine after adding all the controls
                    dataGridView3.ResumeLayout();

                    dataGridView3.AutoScroll = true;
                    txtCariMenu.Enabled = true;

                    // Initialize the original data source
                    originalPanelControls = dataGridView3.Controls.OfType<Panel>().ToList();
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = "Cari Menu Items...";

                    });
                    // Use EndInvoke to release the delegate when the call is complete
                    this.EndInvoke(this.BeginInvoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = "Cari Menu Items...";

                    }));
                    //LoadCart();
                    cmbFilter.SelectedIndex = 0;

                    return;
                }
                catch (TaskCanceledException ex)
                {
                    MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    // Resume the layout engine after adding all the controls
                    dataGridView3.ResumeLayout();

                    string dataFileContents = File.ReadAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data");
                    string response = await apiService.Get("/menu?outlet_id=" + baseOutlet);

                    // Check if the contents match, regardless of length
                    if (response != dataFileContents)
                    {
                        // Contents do not match, take appropriate action
                        MessageBox.Show("Data rusak, Silahkan Reset Cache di setting.");
                    }
                }


            }
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    MessageBox.Show("No Connection Available.");
                    return;
                }


                string response = await apiService.Get("/menu?outlet_id=" + baseOutlet);

                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                List<Menu> menuList = menuModel.data.ToList();

                // Save the menu data to a local file
                File.WriteAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data", JsonConvert.SerializeObject(menuModel));

                // Clear the existing controls and add new ones
                dataGridView3.SuspendLayout();
                dataGridView3.Controls.Clear();
                foreach (Menu menu in menuList)
                {
                    // ... same code as before to create and add controls ...
                    items += 1;
                    // Use BeginInvoke to marshal the call to the UI thread
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = $"Loading Data...[{items} / {menuModel.data.Count}]";
                    });

                    // Use EndInvoke to release the delegate when the call is complete
                    this.EndInvoke(this.BeginInvoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = $"Loading Data...[{items} / {menuModel.data.Count}]";
                    }));

                    Panel tileButton = new Panel();//Panel tileButton = new Panel();
                    tileButton.Width = 90;
                    tileButton.Height = 120;

                    Panel pictureBoxPanel = new Panel();
                    pictureBoxPanel.Dock = DockStyle.Fill;


                    tileButton.Controls.Add(pictureBoxPanel);

                    //RoundedPanel(pictureBoxPanel);
                    //pictureBoxPanel.BorderStyle = BorderStyle.None; // Add this line to set the border style

                    PictureBox pictureBox = new PictureBox();
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.Dock = DockStyle.Fill;
                    pictureBox.BorderStyle = BorderStyle.None; // Add this line to set the border style

                    pictureBoxPanel.Controls.Add(pictureBox);
                    tileButton.Controls.Add(pictureBoxPanel); // Add the PictureBox to the Panel

                    //AutoSizeLabel nameLabel = new AutoSizeLabel();
                    Label nameLabel = new Label();
                    nameLabel.Text = menu.name;
                    nameLabel.ForeColor = Color.FromArgb(30, 31, 68);
                    nameLabel.Dock = DockStyle.Bottom;
                    nameLabel.TextAlign = ContentAlignment.MiddleCenter;
                    nameLabel.Font = new Font("Arial", 8, FontStyle.Bold);
                    //nameLabel.BackColor = Color.White;

                    tileButton.Controls.Add(nameLabel);

                    Label typeLabel = new Label();
                    typeLabel.Text = menu.menu_type;
                    typeLabel.Dock = DockStyle.Bottom;
                    typeLabel.TextAlign = ContentAlignment.MiddleCenter;
                    tileButton.Controls.Add(typeLabel);
                    //tileButton.Controls.Add(pictureBoxPanel); // Add the PictureBox to the Panel

                    Label priceLabel = new Label();
                    priceLabel.Text = string.Format("Rp. {0:n0},-", menu.price);
                    priceLabel.ForeColor = Color.FromArgb(30, 31, 68);
                    priceLabel.Dock = DockStyle.Bottom;
                    priceLabel.TextAlign = ContentAlignment.MiddleCenter;
                    priceLabel.Font = new Font("Arial", 8, FontStyle.Regular);
                    tileButton.Controls.Add(priceLabel);


                    // Send the pictureBox to the back
                    pictureBox.SendToBack();
                    if (menu != menuList.First())
                    {
                        Panel spacerPanel = new Panel();
                        spacerPanel.Dock = DockStyle.Top;
                        spacerPanel.Height = 110;
                        spacerPanel.Width = 10;// Set the height of the spacer panel
                        dataGridView3.Controls.Add(spacerPanel);
                    }

                    typeLabel.Visible = false;

                    // Use the Invoke method to marshal the call back to the UI thread
                    // this.Invoke((MethodInvoker)delegate
                    //{
                    pictureBox.Click += async (sender, e) =>
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

                        // Create the addCartForm on the UI thread
                        using (addCartForm addCartForm = new addCartForm(menu.id.ToString(), menu.name.ToString()))
                        {
                            addCartForm.Owner = background;

                            background.Show();

                            DialogResult result = addCartForm.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                // Dispose of the background form now that the addCartForm form has been closed
                                background.Dispose();
                                LoadCart();
                            }
                            else
                            {
                                MessageBox.Show("Gagal Menambahkan item, silahkan coba lagi");
                                ReloadCart();
                                LoadCart();
                                // Dispose of the background form now that the addCartForm form has been closed
                                background.Dispose();
                            }
                        }
                    };
                    //});
                    dataGridView3.ResumeLayout();
                    dataGridView3.Controls.Add(tileButton);
                    // Initialize the nameToControlMap dictionary after the dataGridView3 is populated
                    InitializeNameToControlMap(dataGridView3);


                    // Load gambar dari cache atau unduh jika tidak ada di cache
                    await LoadImageToPictureBox(pictureBox, menu);

                    // Use BeginInvoke to marshal the call to the UI thread
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        lblCountingItems.Text = $"{items} items";
                    });

                    // Use EndInvoke to release the delegate when the call is complete
                    this.EndInvoke(this.BeginInvoke((MethodInvoker)delegate
                    {
                        lblCountingItems.Text = $"{items} items";
                    }));

                }
                // Initialize the nameToControlMap dictionary after the dataGridView3 is populated
                InitializeNameToControlMap(dataGridView3);

                // Resume the layout engine after adding all the controls
                dataGridView3.ResumeLayout();

                dataGridView3.AutoScroll = true;
                txtCariMenu.Enabled = true;

                // Initialize the original data source
                originalPanelControls = dataGridView3.Controls.OfType<Panel>().ToList();

                this.BeginInvoke((MethodInvoker)delegate
                {
                    txtCariMenu.PlaceholderText = "Cari Menu Items...";

                });
                // Use EndInvoke to release the delegate when the call is complete
                this.EndInvoke(this.BeginInvoke((MethodInvoker)delegate
                {
                    txtCariMenu.PlaceholderText = "Cari Menu Items...";

                }));

                //LoadCart();
                cmbFilter.SelectedIndex = 0;


            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                // Resume the layout engine after adding all the controls
                dataGridView3.ResumeLayout();

                MessageBox.Show("An error occurred while retrieving data: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }

        }



        // Fungsi untuk memuat gambar ke PictureBox dengan mengambil dari cache atau unduh jika tidak ada di cache
        private async Task LoadImageToPictureBox(PictureBox pictureBox, Menu menu)
        {

            if (!menuImageDictionary.ContainsKey(menu))
            {
                menuImageDictionary.Add(menu, LoadPlaceholderImage(70, 70)); // Placeholder image initially

                Image cachedImage = await LoadImageFromCache(menu.id.ToString());

                if (cachedImage != null)
                {

                    try //just add try for error except
                    {

                        using (Graphics graphics = Graphics.FromHwnd(pictureBox.Handle))
                        {
                            Rectangle rect = new Rectangle(0, 0, pictureBox.Width, pictureBox.Height);
                            graphics.FillRectangle(new SolidBrush(Color.White), rect);
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            // Create a GraphicsPath object for the rounded rectangle
                            GraphicsPath path = new GraphicsPath();
                            path.AddArc(rect.X, rect.Y, rect.Width / 2, rect.Height / 2, 180, 90);
                            path.AddArc(rect.Right - rect.Width / 2, rect.Y, rect.Width / 2, rect.Height / 2, 270, 90);
                            path.AddArc(rect.Right - rect.Width / 2, rect.Bottom - rect.Height / 2, rect.Width / 2, rect.Height / 2, 0, 90);
                            path.AddArc(rect.X, rect.Bottom - rect.Height / 2, rect.Width / 2, rect.Height / 2, 90, 90);
                            path.CloseFigure();

                            // Create a new Region object based on the GraphicsPath object
                            Region region = new Region(path);

                            // Set the PictureBox region to the rounded rectangle
                            pictureBox.Region = region;
                        }

                        // Set the PictureBox image and size
                        pictureBox.Image = cachedImage;
                        //pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle the error by displaying a message or logging the error
                        Console.WriteLine($"Error: {ex.Message}");
                        LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                    }
                }
                else
                {
                    try
                    {
                        Image downloadedImage = await LoadImageAsync(menu);
                        // Resize the image before adding it to the dictionary
                        //agar tidak out of memory saat load gambar dengan menggunakan method resize

                        //downloadedImage = ResizeImage(downloadedImage, 70);
                        ResizeImage(downloadedImage, 70);
                        // Check if the image size is within the limits of the PictureBox
                        CheckImageSize(downloadedImage);
                        try
                        {
                            //hapus sampe sini jika msh error
                            menuImageDictionary[menu] = downloadedImage;

                            if (pictureBox.InvokeRequired)
                            {
                                pictureBox.Invoke((MethodInvoker)delegate
                                {
                                    pictureBox.Image = downloadedImage;
                                });
                            }
                            else
                            {
                                pictureBox.Image = downloadedImage;
                            }
                        }
                        catch (ArgumentException ex)
                        {

                            // Handle the error by displaying a message or logging the error
                            Console.WriteLine($"Error: {ex.Message}");
                            LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                        }
                        // Save the downloaded image to cache
                        SaveImageToCache(downloadedImage, menu.id.ToString());
                    }
                    catch (ArgumentException ex)
                    {
                        //Console.WriteLine($"Error loading image: {ex.Message}");
                        LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    }
                }
            }
            else
            {
                pictureBox.Image = menuImageDictionary[menu];
            }
        }

        public static Image ResizeImage(Image image, int size)
        {
            // Create a new Bitmap with white background
            //perhitungan manual
            int deviden;
            int width, height;
            deviden = image.Height / size;
            height = image.Height * deviden;
            width = image.Width * deviden;

            var finalImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(finalImage))
            {
                // Set the background color to white
                graphics.Clear(Color.White);

                // Maintain aspect ratio while resizing
                float ratio = Math.Min((float)width / image.Width, (float)height / image.Height);
                int newWidth = (int)(image.Width * ratio);
                int newHeight = (int)(image.Height * ratio);

                // Calculate the position to center the image on the white background
                int posX = (width - newWidth) / 2;
                int posY = (height - newHeight) / 2;

                // Draw the image onto the white background
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, posX, posY, newWidth, newHeight);
            }

            return finalImage;
        }

        private void CheckImageSize(Image image)
        {
            // Get the size of the image
            Size imageSize = image.Size;

            // Check if the image size is within the limits of the PictureBox
            if (imageSize.Width > 150 || imageSize.Height > 150)
            {
                throw new ArgumentException("The image size is too large for the PictureBox.");

            }
        }
        //======================================resize image


        // Fungsi untuk memuat gambar dari cache lokal
        private async Task<Image> LoadImageFromCache(string imageName)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(appDirectory)?.Parent?.Parent?.FullName;
            string localImagePath = Path.Combine(projectDirectory, "ImageCache", imageName + ".Jpeg");

            if (File.Exists(localImagePath))
            {
                try
                {
                    using (FileStream stream = new FileStream(localImagePath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(stream);
                    }
                }
                catch (ArgumentException ex)
                {
                    // Log the error and return null if the image file is not valid
                    //Console.WriteLine($"Error loading image from cache: {ex.Message}");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                    return null;
                }
            }

            return null;
        }

        // Fungsi untuk menyimpan gambar ke cache lokal
        //mengganti jpg to png >>>> If possible, store images in a lossless format, such as PNG, to reduce memory usage.
        private async void SaveImageToCache(Image image, string imageName)
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Directory.GetParent(appDirectory)?.Parent?.Parent?.FullName;
                string localDirectory = Path.Combine(projectDirectory, "ImageCache");
                Directory.CreateDirectory(localDirectory);

                string localImagePath = Path.Combine(localDirectory, imageName + ".Jpeg"); //before jpg
                                                                                           // Generate a random pastel color

                // Generate a random pastel color
                Random rand = new Random();
                int r = rand.Next(200, 255);
                int g = rand.Next(200, 255);
                int b = rand.Next(200, 255);
                Color pastelColor = Color.FromArgb(r, g, b);

                // Create a new bitmap with the pastel color background
                Bitmap bmp = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics graphic = Graphics.FromImage(bmp))
                {
                    graphic.Clear(pastelColor);
                    graphic.DrawImage(image, new Rectangle(new Point(), image.Size), new Rectangle(new Point(), image.Size), GraphicsUnit.Pixel);
                }

                // Save the new image
                using (FileStream stream = new FileStream(localImagePath, FileMode.Create, FileAccess.Write))
                {
                    bmp.Save(stream, ImageFormat.Jpeg);
                }
                /*
                using (FileStream stream = new FileStream(localImagePath, FileMode.Create, FileAccess.Write))
                {
                    image.Save(stream, ImageFormat.Jpeg);
                }
                */
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error saving image to cache: {ex.Message}");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }
        private async Task<Image> LoadImageAsync(Menu menu)
        {
            try
            {
                //start replace
                string imageUrl = baseUrl + "/" + menu.image_url;


                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                /*
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    return Image.FromStream(stream);
                }
                */

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    Image originalImage = Image.FromStream(stream);

                    // Generate a random pastel color
                    Random rand = new Random();
                    int r = rand.Next(200, 255);
                    int g = rand.Next(200, 255);
                    int b = rand.Next(200, 255);

                    Color pastelColor = Color.FromArgb(r, g, b);
                    // Create a new bitmap with the pastel color background
                    int width = Math.Min(70, originalImage.Width);
                    int height = Math.Min(70, originalImage.Height);
                    int left = (70 - width) / 2;
                    int top = (70 - height) / 2;
                    Bitmap bmp = new Bitmap(70, 70, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics graphic = Graphics.FromImage(bmp))
                    {
                        graphic.Clear(pastelColor);
                        graphic.DrawImage(originalImage, left, top, width, height);
                    }

                    return bmp;

                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error downloading image: {ex.Message}");
                return LoadPlaceholderImage(70, 70);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        // Method to asynchronously load the image
        private Image LoadPlaceholderImage(int width, int height)
        {
            // Path to the placeholder image
            string startupPath = AppDomain.CurrentDomain.BaseDirectory; // Get the startup folder
            string placeholderImagePath = Path.Combine(startupPath, "icon", "ImageNotFound.png");

            try
            {
                // Check if the file exists
                if (File.Exists(placeholderImagePath))
                {
                    // Load the image from file
                    Image placeholderImage = Image.FromFile(placeholderImagePath);

                    // Resize the image to the specified width and height
                    Image resizedPlaceholder = new Bitmap(placeholderImage, new Size(width, height));
                    return resizedPlaceholder;
                }
                else
                {
                    Console.WriteLine("Placeholder image not found at: " + placeholderImagePath);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception if necessary
                Console.WriteLine("Error loading placeholder image: " + ex.Message);
            }

            // If the image file is not found or an error occurs, use the generated placeholder logic
            Bitmap placeholder = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(placeholder))
            {
                // Generate a random pastel color
                Random Rand = new Random();
                int r = Rand.Next(128, 255); // Random red value between 128 and 255
                int g = Rand.Next(128, 255); // Random green value between 128 and 255
                int b = Rand.Next(128, 255); // Random blue value between 128 and 255
                Color pastelColor = Color.FromArgb(r, g, b);

                graphics.FillRectangle(new SolidBrush(pastelColor), 0, 0, width, height);

                // Calculate the center point of the image
                int x = (width - 70) / 2;
                int y = (height - 35) / 2;

                // Draw the placeholder text at the center point
                graphics.DrawString("Image\nTidak\nDiUpload", SystemFonts.DefaultFont, new SolidBrush(Color.FromArgb(30, 31, 68)), new PointF(x, y));
            }
            return placeholder;
        }

        //reload dual monitor keranjang
        public async Task SignalReload()
        {
            if (!Directory.Exists("C:\\Temp"))
            {
                Directory.CreateDirectory("C:\\Temp");
            }
            File.WriteAllText("C:\\Temp\\reload_signal.txt", "ReloadChart");
        }
        public async Task SaveCartDataLocally(GetCartModel json)
        {
            try
            {
                // Membuat instance dari pengirim dan penerima
                //var sender = new SignalSender();

                // Mengirimkan sinyal
                string filePath = @"C:\Temp\Cart.data";

                // Pastikan direktori ada
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                // Menyimpan JSON ke file
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    string jsonData = JsonConvert.SerializeObject(json);
                    writer.Write(jsonData);
                }
                //sender.SendSignal("ReloadChart");

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        public async Task Ex_LoadCart()
        {
            int retryCount = 3; // Maksimal 3 kali percobaan
            int attempt = 0;
            bool success = false;

            while (attempt < retryCount && !success)
            {
                try
                {
                    attempt++;
                    string response = await apiService.Get("/cart?outlet_id=" + baseOutlet);

                    if (string.IsNullOrEmpty(response))
                    {
                        throw new Exception("Response is null or empty");
                    }

                    GetCartModel dataModel = JsonConvert.DeserializeObject<GetCartModel>(response);
                    if (dataModel == null || dataModel.data == null)
                    {
                        throw new Exception("Invalid cart data from server.");
                    }

                    SaveCartDataLocally(dataModel);

                    // Proses data untuk UI dan tabel
                    ProcessCartData(dataModel);

                    success = true; // Tandai bahwa percobaan berhasil
                }
                catch (TaskCanceledException ex)
                {
                    if (attempt >= retryCount)
                    {
                        //MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoggerUtil.LogError(ex, $"Timeout after {retryCount} attempts: {ex.Message}");
                        return;
                    }

                    await Task.Delay(1000); // Tunggu 1 detik sebelum mencoba lagi
                }
                catch (Exception ex)
                {
                    if (attempt >= retryCount)
                    {
                        LoggerUtil.LogError(ex, $"An error occurred after {retryCount} attempts: {ex.Message}");
                        //MessageBox.Show("Terjadi kesalahan saat memuat data. Silakan coba lagi nanti.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    await Task.Delay(1000); // Tunggu 1 detik sebelum mencoba lagi
                }
            }
        }

        private async void ProcessCartData(GetCartModel dataModel)
        {
            // Menangani UI jika keranjang kosong
            if (dataModel.data == null || dataModel.data.cart_details == null || dataModel.data.cart_details.Count == 0)
            {
                lblDetailKeranjang.Text = "Keranjang: Kosong";
                lblDiskon1.Text = "Rp. 0";
                lblSubTotal1.Text = "Rp. 0,-";
                lblTotal1.Text = "Rp. 0,-";
                iconButton1.Text = "Bayar ";
                subTotalPrice = 0;
                dataGridView1.DataSource = null;
                return;
            }

            // Update UI untuk total, subtotal, dan diskon
            lblDetailKeranjang.Text = "Keranjang: " + (string.IsNullOrEmpty(dataModel.data.customer_name) ? "name?" : dataModel.data.customer_name) +
                                      " - " + (string.IsNullOrEmpty(dataModel.data.customer_seat) ? "seat?" : dataModel.data.customer_seat);
            int result = dataModel.data.total - dataModel.data.subtotal;
            lblDiskon1.Text = string.Format("Rp. {0:n0},-", result);
            lblSubTotal1.Text = string.Format("Rp. {0:n0},-", dataModel.data.subtotal);
            lblTotal1.Text = string.Format("Rp. {0:n0},-", dataModel.data.total);
            iconButton1.Text = string.Format("Bayar Rp. {0:n0},-", dataModel.data.total);
            subTotalPrice = dataModel.data.subtotal;

            // Proses detail keranjang ke dalam DataTable
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("MenuID", typeof(string));
            dataTable.Columns.Add("CartDetailID", typeof(int));
            dataTable.Columns.Add("Jenis", typeof(string));
            dataTable.Columns.Add("Menu", typeof(string));
            dataTable.Columns.Add("Total Harga", typeof(string));
            dataTable.Columns.Add("Note", typeof(string));

            Dictionary<string, List<DetailCart>> menuGroups = new Dictionary<string, List<DetailCart>>();

            foreach (DetailCart menu in dataModel.data.cart_details)
            {
                string servingTypeName = menu.serving_type_name ?? "Unknown";
                if (!menuGroups.ContainsKey(servingTypeName))
                {
                    menuGroups[servingTypeName] = new List<DetailCart>();
                }
                menuGroups[servingTypeName].Add(menu);
            }

            foreach (var group in menuGroups)
            {
                AddSeparatorRow(dataTable, group.Key, dataGridView1);

                foreach (DetailCart menu in group.Value)
                {
                    /*string menuName = menu.menu_name ?? "Unknown";
                    string variant = menu.varian ?? "No variant";
                    string note = menu.note_item ?? "";*/

                    dataTable.Rows.Add(
                        menu.menu_id ?? 0,
                        menu.cart_detail_id,
                        menu.serving_type_name ?? "",
                        (menu.qty > 0 ? menu.qty.ToString() : "0") + "X " + (menu.is_ordered == "1" ? "(Ordered) " : "") + menu.menu_name + " " + menu.varian,
                        string.Format("Rp. {0:n0},-", menu.total_price),
                        menu.note_item
                    );

                    if (menu.discounted_price != 0)
                    {
                        dataTable.Rows.Add(
                            null,
                            null,
                            null,
                            "  *catatan : " + menu.note_item,
                            "*Discount :" + (string.IsNullOrEmpty(menu.discount_code) ? "No code" : menu.discount_code),
                            null
                        );
                    }
                }
            }

            // Update DataGridView
            dataGridView1.DataSource = dataTable;
            dataGridView1.Columns["MenuID"].Visible = false;
            dataGridView1.Columns["CartDetailID"].Visible = false;
            dataGridView1.Columns["Jenis"].Visible = false;
            dataGridView1.Columns["Note"].Visible = false;

            DataGridViewCellStyle boldStyle = new DataGridViewCellStyle();
            boldStyle.Font = new Font(dataGridView1.Font, FontStyle.Italic);
            dataGridView1.Columns["Menu"].DefaultCellStyle = boldStyle;
            dataGridView1.Columns["Menu"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.Columns["Menu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Salin data ke tabel lain jika diperlukan
            dataTable2 = dataTable.Copy();

            // Muat diskon (asynchronous)
            await LoadDataDiscount();
            autoFillDiskon();
        }
        public async Task LoadCart()
        {
            iconButton1.Enabled = false;
            iconButtonHps.Enabled = false;
            iconButtonGet.Enabled = false;
            iconButton2.Enabled = false;
            button7.Enabled = false;
            await LoadCartData();
            iconButton1.Enabled = true;
            iconButtonHps.Enabled = true;
            iconButtonGet.Enabled = true;
            iconButton2.Enabled = true;
            button7.Enabled = true;
        }
        public async Task LoadCartData()
        {

            try
            {
                string response = await apiService.Get("/cart?outlet_id=" + baseOutlet);
                GetCartModel dataModel = JsonConvert.DeserializeObject<GetCartModel>(response);
                SaveCartDataLocally(dataModel);

                if (dataModel.data == null)
                {
                    lblDetailKeranjang.Text = "Keranjang: Kosong";
                    lblDiskon1.Text = "Rp. 0";
                    lblSubTotal1.Text = "Rp. 0,-";
                    lblTotal1.Text = "Rp. 0,-";
                    iconButton1.Text = "Bayar ";
                    subTotalPrice = 0;
                }
                else
                {
                    lblDetailKeranjang.Text = "Keranjang: Kosong";

                    if (dataModel.data.total == 0)
                    {
                        customer_seat = "";
                        customer_name = "";
                        lblDiskon1.Text = "Rp. 0";
                        lblSubTotal1.Text = "Rp. 0,-";
                        lblTotal1.Text = "Rp. 0,-";
                        iconButton1.Text = "Bayar ";
                        subTotalPrice = 0;
                    }
                    else
                    {
                        customer_seat = dataModel.data.customer_seat.ToString();
                        customer_name = dataModel.data.customer_name.ToString();
                        int result = dataModel.data.total - dataModel.data.subtotal;
                        lblDiskon1.Text = string.Format("Rp. {0:n0},-", result);
                        subTotalPrice = dataModel.data.subtotal;
                        lblSubTotal1.Text = string.Format("Rp. {0:n0},-", dataModel.data.subtotal);
                        lblTotal1.Text = string.Format("Rp. {0:n0},-", dataModel.data.total);
                        iconButton1.Text = string.Format("Bayar Rp. {0:n0},-", dataModel.data.total);
                        lblDetailKeranjang.Text = "Keranjang: " + (string.IsNullOrEmpty(customer_name) ? "name?" : customer_name) + " - " + (string.IsNullOrEmpty(customer_seat) ? "seat?" : customer_seat);
                    }

                    totalCart = dataModel.data.total.ToString();
                    cartID = dataModel.data.cart_id.ToString();
                    isSplitted = dataModel.data.is_splitted;
                    diskonID = dataModel.data.discount_id;
                    List<DetailCart> cartList = dataModel.data.cart_details;

                    Dictionary<string, List<DetailCart>> menuGroups = new Dictionary<string, List<DetailCart>>();

                    foreach (DetailCart menu in cartList)
                    {
                        if (!menuGroups.ContainsKey(menu.serving_type_name))
                        {
                            menuGroups[menu.serving_type_name] = new List<DetailCart>();
                        }
                        menuGroups[menu.serving_type_name].Add(menu);
                    }

                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("MenuID", typeof(string));
                    dataTable.Columns.Add("CartDetailID", typeof(int));
                    dataTable.Columns.Add("Jenis", typeof(string));
                    dataTable.Columns.Add("Menu", typeof(string));
                    //dataTable.Columns.Add("Jumlah", typeof(string));
                    dataTable.Columns.Add("Total Harga", typeof(string));
                    dataTable.Columns.Add("Note", typeof(string));
                    foreach (var group in menuGroups)
                    {
                        // Panggil metode untuk menambahkan separator row
                        AddSeparatorRow(dataTable, group.Key, dataGridView1);
                        //dataTable.Rows.Add(null, null, null, group.Key + "s\n", null, null); // Add a separator row


                        foreach (DetailCart menu in group.Value)
                        {
                            if (menu.is_ordered == "1")
                            {

                                dataTable.Rows.Add(
                                menu.menu_id ?? 0,  // Ensure menu_id is always non-null
                                menu.cart_detail_id,
                                menu.serving_type_name ?? "",  // Ensure serving_type_name is non-null
                                (menu.qty > 0 ? menu.qty.ToString() : "0") + "X " + "(Ordered) " + (menu.menu_name ?? "Unknown") + " " + (menu.varian ?? "No variant"),
                                string.Format("Rp. {0:n0},-", menu.total_price),
                                menu.note_item ?? ""  // Ensure note_item is always non-null
                            );

                            }
                            else
                            {
                                dataTable.Rows.Add(
                                    menu.menu_id,
                                    menu.cart_detail_id,
                                    menu.serving_type_name,
                                    menu.qty + "X " + menu.menu_name + " " + menu.varian,
                                    string.Format("Rp. {0:n0},-", menu.total_price),
                                    null);
                            }
                            if (menu.discounted_price != 0)
                            {
                                if (!string.IsNullOrEmpty(menu.note_item))
                                {
                                    dataTable.Rows.Add(
                                        null,
                                        null,
                                        null,
                                        "  *catatan : " + (menu.note_item ?? ""),
                                        "*Discount :" + (string.IsNullOrEmpty(menu.discount_code) ? "No code" : menu.discount_code),
                                        null);
                                }
                                else
                                {
                                    dataTable.Rows.Add(
                                        null,
                                        null,
                                        null,
                                        null,
                                        "*Discount :" + (string.IsNullOrEmpty(menu.discount_code) ? "No code" : menu.discount_code),
                                        null);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(menu.note_item))
                                {
                                    dataTable.Rows.Add(
                                        null,
                                        null,
                                        null,
                                        "  *catatan : " + (menu.note_item ?? ""),
                                        null,
                                        null);
                                }
                            }
                        }
                    }

                    dataGridView1.DataSource = dataTable;

                    dataTable2 = dataTable.Copy();
                    dataGridView1.Columns["MenuID"].Visible = false;
                    DataGridViewCellStyle boldStyle = new DataGridViewCellStyle();
                    boldStyle.Font = new Font(dataGridView1.Font, FontStyle.Italic);
                    dataGridView1.Columns["Menu"].DefaultCellStyle = boldStyle;
                    dataGridView1.Columns["Menu"].DefaultCellStyle.WrapMode = DataGridViewTriState.True; // Enable text wrapping
                    dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; // Auto size rows to fit text
                    dataGridView1.Columns["Menu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dataGridView1.Columns["CartDetailID"].Visible = false;
                    dataGridView1.Columns["Jenis"].Visible = false;
                    dataGridView1.Columns["Note"].Visible = false;
                    dataGridView1.CellFormatting += DataGridView1_CellFormatting;

                    await LoadDataDiscount(); // Ensure LoadDataDiscount is awaited

                    autoFillDiskon();
                }
                await SignalReload();
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
        private void Ex_AddSeparatorRow(DataTable dataTable, string groupKey, DataGridView dataGridView)
        {
            // Tambahkan separator row ke DataTable
            dataTable.Rows.Add(null, null, null, groupKey + "s\n", null, null); // Add a separator row

            // Ambil indeks baris terakhir yang baru saja ditambahkan
            int lastRowIndex = dataTable.Rows.Count - 1;

            // Menambahkan row ke DataGridView
            dataGridView.DataSource = dataTable;

            // Mengatur gaya sel untuk kolom tertentu
            int[] cellIndexesToStyle = { 3, 4, 5 }; // Indeks kolom yang ingin diatur
            SetCellStyle(dataGridView.Rows[lastRowIndex], cellIndexesToStyle, Color.WhiteSmoke, FontStyle.Bold);
        }
        private void AddSeparatorRow(DataTable dataTable, string groupKey, DataGridView dataGridView)
        {
            // Tambahkan baris separator ke DataTable
            dataTable.Rows.Add(null, null, null, $"{groupKey}s", null, null);
            // Cek apakah kita sudah memiliki data, jika ya tambahkan ke DataGridView
            if (dataTable.Rows.Count > 0)
            {
                dataGridView.DataSource = dataTable;
            }
        }

        // Then, after all data is loaded and the DataGridView's DataSource is set:
        private void StyleSeparatorRows()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                // Check if this is a separator row (perhaps by checking if certain cells are null)
                if (row.Cells["MenuID"].Value == null && row.Cells["CartDetailID"].Value == null)
                {
                    int[] cellIndexesToStyle = { 3, 4, 5 };
                    SetCellStyle(row, cellIndexesToStyle, Color.WhiteSmoke, FontStyle.Bold);
                }
            }
        }
        private void SetCellStyle(DataGridViewRow row, int[] cellIndexes, Color backgroundColor, FontStyle fontStyle)
        {
            foreach (int index in cellIndexes)
            {
                row.Cells[index].Style.BackColor = backgroundColor;
                row.Cells[index].Style.Font = new Font(dataGridView1.Font, fontStyle);
            }
        }
        private void autoFillDiskon()
        {
            // Mengambil ID diskon yang dipilih
            int selectedDiskon = diskonID;

            // Memeriksa apakah ada diskon yang diterapkan
            if (selectedDiskon != 0)
            {
                iconButtonGet.Text = "Hapus Disc";
                iconButtonGet.IconChar = IconChar.TrashRestoreAlt;
                iconButtonGet.ForeColor = Color.DarkRed;
                iconButtonGet.IconColor = Color.DarkRed;
            }
            else
            {
                iconButtonGet.Text = "Gunakan Disc";
                iconButtonGet.IconChar = IconChar.Tag;
                iconButtonGet.ForeColor = Color.FromArgb(31, 30, 68);
                iconButtonGet.IconColor = Color.FromArgb(31, 30, 68);
            }
        }



        private void DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

        }

        private void ReloadData()
        {
            loadDataAsync();

        }
        public void ReloadData2()
        {
            loadDataAsync();
        }

        public async void ReloadCart()
        {
            cartID = null;
            totalCart = null;
            iconButton1.Text = "Bayar";
            lblDiskon1.Text = null;
            lblSubTotal1.Text = null;
            lblTotal1.Text = null;
            dataGridView1.DataSource = null;
            LoadCart();
        }


        private void masterPos_Load(object sender, EventArgs e)
        {

        }

        //reload dual monitor pembayaran
        public void SignalReloadPayform()
        {
            if (!Directory.Exists("C:\\Temp"))
            {
                Directory.CreateDirectory("C:\\Temp");
            }
            File.WriteAllText("C:\\Temp\\payment_signal.txt", "Payment");
        }
        public void SignalReloadPayformDone()
        {
            if (!Directory.Exists("C:\\Temp"))
            {
                Directory.CreateDirectory("C:\\Temp");
            }
            File.WriteAllText("C:\\Temp\\payment_signal.txt", "PaymentDone");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //var pengirim = new SignalSender();
            if (string.IsNullOrEmpty(cartID)) // For strings
            {
                MessageBox.Show("Keranjang masih kosong", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ////LoggerUtil.LogPrivateMethod(nameof(button1_Click));

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

            using (payForm payForm = new payForm(baseOutlet, cartID, totalCart, lblTotal1.Text.ToString(), customer_seat, customer_name, this))
            {
                SignalReloadPayform();

                //pengirim.SendSignal("Payment");
                payForm.Owner = background;

                background.Show();

                //DialogResult dialogResult = dataBill.ShowDialog();

                //background.Dispose();
                DialogResult result = payForm.ShowDialog();

                // Handle the result if needed
                if (result == DialogResult.OK)
                {
                    background.Dispose();
                    ReloadCart();
                    LoadCart();
                    cartID = "";
                    // Settings were successfully updated, perform any necessary actions
                    SignalReloadPayformDone();
                    //pengirim.SendSignal("Payment");

                }
                else
                {
                    MessageBox.Show("Gagal Simpan, Silahkan coba lagi");
                    SignalReloadPayformDone();
                    //pengirim.SendSignal("PaymentDone");
                    background.Dispose();
                    ReloadCart();
                    LoadCart();
                }
            }


        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtCariMenu_TextChanged_1(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                try
                {
                    DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                    int id = 0;
                    int cartdetailid = 0;

                    // Periksa nilai DBNull untuk MenuID
                    if (selectedRow.Cells["MenuID"].Value != DBNull.Value)
                    {
                        id = Convert.ToInt32(selectedRow.Cells["MenuID"].Value);
                    }
                    else
                    {
                        //MessageBox.Show("MenuID tidak valid.");
                        return;
                    }

                    // Periksa nilai DBNull untuk CartDetailID
                    if (selectedRow.Cells["CartDetailID"].Value != DBNull.Value)
                    {
                        cartdetailid = Convert.ToInt32(selectedRow.Cells["CartDetailID"].Value);
                    }
                    else
                    {
                        //MessageBox.Show("CartDetailID tidak valid.");
                        return;
                    }

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
                    this.Invoke((MethodInvoker)delegate
                    {
                        using (updateCartForm updateCartForm = new updateCartForm(id.ToString(), cartdetailid.ToString()))
                        {
                            updateCartForm.Owner = background;

                            background.Show();

                            DialogResult result = updateCartForm.ShowDialog();

                            // Handle the result if needed
                            if (result == DialogResult.OK)
                            {
                                background.Dispose();
                                ReloadCart();
                                LoadCart();
                            }
                            else
                            {
                                MessageBox.Show("Gagal update item, Silahkan coba lagi");
                                background.Dispose();
                                ReloadCart();
                                LoadCart();
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    return;
                }
            }
        }




        private void button2_Click(object sender, EventArgs e)
        {
            int rowCount = dataGridView1.RowCount;
            if (rowCount == 0)
            {
                MessageBox.Show("Keranjang masih kosong!", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
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

            ////LoggerUtil.LogPrivateMethod(nameof(button2_Click));
            if (isSplitted != 0)
            {
                MessageBox.Show("Keranjang ini telah di Split! tidak bisa diSimpan.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {



                using (saveBill saveBill = new saveBill(cartID, customer_name, customer_seat))
                {
                    saveBill.Owner = background;

                    background.Show();

                    //DialogResult dialogResult = dataBill.ShowDialog();

                    //background.Dispose();
                    DialogResult result = saveBill.ShowDialog();

                    // Handle the result if needed
                    if (result == DialogResult.OK)
                    {
                        background.Dispose();
                        ReloadCart();
                        LoadCart();
                        // Settings were successfully updated, perform any necessary actions
                    }
                    else
                    {
                        MessageBox.Show("Gagal Simpan, Silahkan coba lagi");
                        background.Dispose();
                        ReloadCart();
                        LoadCart();
                    }
                }
            }
        }
        private async void btnGet_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnGet_Click));
            if (string.IsNullOrEmpty(cartID) || dataDiscountListCart == null || !dataDiscountListCart.Any())
            {
                MessageBox.Show("Keranjang kosong. Silakan tambahkan item ke keranjang sebelum menerapkan diskon.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int selectedDiskon = (int)cmbDiskon.SelectedValue;

            if (diskonID != 0) // Jika diskon sudah diterapkan, maka hapus diskon
            {
                selectedDiskon = 0;
            }
            else
            {
                if (selectedDiskon == -1)
                {
                    MessageBox.Show("Diskon belum dipilih !", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int diskonMinimum = dataDiscountListCart.FirstOrDefault(d => d.id == selectedDiskon)?.min_purchase ?? -1;
                if (diskonMinimum > subTotalPrice)
                {
                    int resultDiskon = diskonMinimum - subTotalPrice;
                    MessageBox.Show("Minimum diskon kurang " + string.Format("Rp. {0:n0},-", resultDiskon) + " lagi", "Gaspol");
                    return;
                }
            }

            try
            {
                if (await cekPeritemDiskon() != 0 && selectedDiskon != 0)
                {
                    MessageBox.Show("Memasang Diskon Cart, Diskon peritem akan dilepas");
                }

                var json = new
                {
                    cart_id = cartID,
                    discount_id = selectedDiskon
                };

                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                HttpResponseMessage response = await apiService.PayBill(jsonString, "/discount-transaction");

                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        PostModel responseModel = JsonConvert.DeserializeObject<PostModel>(responseContent);
                        diskonID = selectedDiskon; // Update diskonID to the applied discount

                        // Memuat ulang keranjang untuk melihat perubahan harga
                        await LoadCart();
                    }
                    else
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        PostModel responseModel = JsonConvert.DeserializeObject<PostModel>(responseContent);
                        MessageBox.Show(responseModel.message, "Pesan", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Gagal menggunakan diskon: " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        private async Task<int> cekPeritemDiskon()
        {
            try
            {
                string response = await apiService.Get("/cart?outlet_id=" + baseOutlet);
                GetCartModel dataModel = JsonConvert.DeserializeObject<GetCartModel>(response);
                if (dataModel.data != null)
                {
                    List<DetailCart> cartList = dataModel.data.cart_details;
                    foreach (DetailCart menu in cartList)
                    {
                        if (menu.discounted_price != 0)
                        {
                            return 1;
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
                MessageBox.Show("Gagal cek diskon: " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

            return 0;
        }

        private void lblSubTotal1_Click(object sender, EventArgs e)
        {

        }


        private void listBill_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(listBill_Click));

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

            using (dataBill dataBill = new dataBill())
            {
                dataBill.Owner = background;

                background.Show();

                //DialogResult dialogResult = dataBill.ShowDialog();

                //background.Dispose();
                DialogResult result = dataBill.ShowDialog();

                // Handle the result if needed
                if (result == DialogResult.OK)
                {
                    background.Dispose();
                    ReloadCart();
                    LoadCart();
                    // Settings were successfully updated, perform any necessary actions
                }
                else
                {
                    MessageBox.Show("gagal menampilkan, silahkan coba lagi");
                    ReloadCart();
                    LoadCart();
                    background.Dispose();
                }
            }


        }

        private void FilterMenuItems(string selectedMenuType)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(FilterMenuItems));


            // Create a copy of the original data source
            var clonedDataGridViewControls = new List<Control>(dataGridView3.Controls.OfType<Control>());

            // Filter the cloned data source based on the selected menu type
            var filteredDataGridViewControls = clonedDataGridViewControls.OfType<Panel>().Where(panel =>
            {
                if (panel.Controls.OfType<Label>().Any())
                {
                    var typeLabel = panel.Controls.OfType<Label>().FirstOrDefault(label => label.Name == "typeLabel");
                    if (typeLabel != null)
                    {
                        // Check if the selected menu type matches the typeLabel's text
                        string itemName = typeLabel.Text.ToLower();
                        return string.IsNullOrEmpty(selectedMenuType) || itemName.Contains(selectedMenuType.ToLower());
                    }
                }
                return false;
            }).ToList();

            // Clear the original data source
            dataGridView3.Controls.Clear();

            // Add the filtered items to the original data source
            foreach (var panelControl in filteredDataGridViewControls)
            {
                dataGridView3.Controls.Add(panelControl);
            }

        }

        private async void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //if (cmbFilter.SelectedItem != null)
                //{
                string selectedFilter = cmbFilter.SelectedItem.ToString();

                //LoadFlowLayoutPanelBasedOnLabelType(cmbFilter.SelectedItem.ToString());
                items = 0;
                string config = File.ReadAllText(configFilePath);
                // Iterate through the items in FlowLayoutPanel and filter based on menu_type

                if (dataGridView1.Visible == true && selectedFilter == "Semua")
                {
                    await LoadDataListby();
                    return;
                }
                if (selectedFilter != "Semua")
                {
                    searchSemua(selectedFilter);
                }
                else
                {
                    ListMati();
                    items = 0;
                    foreach (Control control in dataGridView3.Controls)
                    {
                        if (control is Panel tileButton && control.Controls.Count >= 3)
                        {
                            Label typeLabel = control.Controls[2] as Label;
                            if (typeLabel != null)
                            {
                                string menuType = typeLabel.Text;

                                // Determine whether to show or hide the item based on the filter
                                bool showItem = (selectedFilter == "Semua");
                                control.Visible = showItem;

                                //counter items
                                items++;
                                lblCountingItems.Text = items + " items";

                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                //MessageBox.Show("Error: terlalu banyak aksi, aplikasi akan dijalankan ulang");
                //Application.Restart();
                //Environment.Exit(0);
            }

        }

        private async void searchSemua(string selectedFilter)
        {
            await LoadDataListby();

            try
            {
                items = 0;
                ListHidup();

                // Filter data based on Menu Type column
                var filteredData = from row in listDataTable.AsEnumerable()
                                   where row.Field<string>("Menu Type") == selectedFilter
                                   select row;

                DataTable filteredTable = filteredData.CopyToDataTable();
                dataGridView2.DataSource = filteredTable;
                items = dataGridView2.RowCount;
                lblCountingItems.Text = items + " items";
            }
            catch (Exception)
            {
                // MessageBox.Show($"Tidak ada pilihan {selectedFilter}");
            }
        }

        private void ListHidup()
        {
            dataGridView3.Enabled = false;
            dataGridView3.Visible = false;
            dataGridView2.Enabled = true;
            dataGridView2.Visible = true;
        }
        private void ListMati()
        {
            dataGridView3.Enabled = true;
            dataGridView3.Visible = true;
            dataGridView2.Enabled = false;
            dataGridView2.Visible = false;
        }
        private void lblTotal1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(button3_Click));

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

            using (dataDiskon dataDiskon = new dataDiskon())
            {
                dataDiskon.Owner = background;

                background.Show();

                DialogResult dialogResult = dataDiskon.ShowDialog();

                background.Dispose();

                if (dialogResult == DialogResult.OK && dataDiskon.ReloadDataInBaseForm)
                {
                    ReloadData();
                }
            }
        }

        private void cmbDiskon_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(cartID))
            {
                MessageBox.Show("Keranjang masih kosong ");
                return;
            }
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
            ////LoggerUtil.LogPrivateMethod(nameof(button7_Click));
            using (splitBill splitBill = new splitBill(cartID))
            {
                splitBill.Owner = background;

                background.Show();

                //DialogResult dialogResult = dataBill.ShowDialog();

                //background.Dispose();
                DialogResult result = splitBill.ShowDialog();

                // Handle the result if needed
                if (result == DialogResult.OK)
                {
                    background.Dispose();
                    ReloadCart();
                    LoadCart();
                    // Settings were successfully updated, perform any necessary actions
                }
                else
                {
                    MessageBox.Show("Gagal Simpan, Silahkan coba lagi");
                    background.Dispose();
                    ReloadCart();
                    LoadCart();
                }
            }
        }

        private void iconButton4_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(cartID))
            {
                MessageBox.Show("Keranjang masih kosong ");
                return;
            }
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
            ////LoggerUtil.LogPrivateMethod(nameof(iconButton4_Click));

            using (splitBill splitBill = new splitBill(cartID))
            {

                splitBill.Owner = background;

                background.Show();

                //DialogResult dialogResult = dataBill.ShowDialog();

                //background.Dispose();
                DialogResult result = splitBill.ShowDialog();

                // Handle the result if needed
                if (result == DialogResult.OK)
                {
                    background.Dispose();
                    ReloadCart();
                    LoadCart();
                    // Settings were successfully updated, perform any necessary actions
                }
                else
                {
                    MessageBox.Show("Gagal Simpan, Silahkan coba lagi");
                    background.Dispose();
                    ReloadCart();
                    LoadCart();
                }
            }
        }

        private void iconButton4_Click_1(object sender, EventArgs e)
        {

        }

        private async void lblDetailKeranjang_Click(object sender, EventArgs e)
        {
            ReloadCart();
            LoadCart();
        }

        private void txtCariMenuList_TextChanged(object sender, EventArgs e)
        {
            PerformSearchList();
        }

        /*
        private void sButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (sButton1.Checked == true)
            {
                MessageBox.Show("Demo ListView! Contact PM Project");

                // ListView Active
                dataGridView3.Enabled = false;
                dataGridView3.Visible = false;
                dataGridView2.Enabled = true;
                dataGridView2.Visible = true;
                // Pencarian Active
                txtCariMenu.Visible = false;
                txtCariMenu.Enabled = false;
                txtCariMenuList.Visible = true;
                txtCariMenuList.Enabled = true;

                System.Timers.Timer t = new System.Timers.Timer();
                t.Interval = 1000 * 60 * 60;
                t.Elapsed += OnTimedEvent;
                t.AutoReset = false;
                t.Start();
                LoadDataListby();
            }
            else
            {
                // GridView Active
                dataGridView3.Enabled = true;
                dataGridView3.Visible = true;
                dataGridView2.Enabled = false;
                dataGridView2.Visible = false;
                // Pencarian Active
                txtCariMenu.Visible = true;
                txtCariMenu.Enabled = true;
                txtCariMenuList.Visible = false;
                txtCariMenuList.Enabled = false;
                loadDataAsync();
            }
        }

        */
        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                MessageBox.Show("Demo ListView/1jam off. Contact PM");
            }
            catch (Exception)
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void txtCariMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Menghentikan bunyi "ding" saat Enter ditekan
                PerformSearch(); // Panggil metode yang diinginkan
            }
        }



    }
}
