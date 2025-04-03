using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using System.Data;
using Menu = KASIR.Model.Menu;
using FontAwesome.Sharp;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using System.Timers;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using KASIR.Komponen;
using System.IO;
using KASIR.OffineMode;
using SharpCompress.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using MessageBox = System.Windows.Forms.MessageBox;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Drawing.Point;
using SystemFonts = System.Drawing.SystemFonts;
using Size = System.Drawing.Size;
using System.Windows;
using Polly.Caching;
using System.Reflection;


namespace KASIR.OfflineMode
{
    public partial class Offline_masterPos : Form
    {
        private Offline_payForm Offline_payForm;
        private ApiService apiService;
        private DataTable originalDataTable, listDataTable;
        string totalCart;
        string cartID;
        string customer_name;
        string customer_seat;
        int selectedServingTypeallItems;
        private readonly string baseOutlet;
        private readonly string baseUrl;
        private BindingSource bindingSource = new BindingSource();
        private DataTable dataTable2;
        List<DataDiscountCart> dataDiscountListCart;
        int subTotalPrice;
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

        public Offline_masterPos()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            baseUrl = Properties.Settings.Default.BaseAddress;
            InitializeComponent();
            // Panggil di constructor setelah InitializeComponent()
            SetDoubleBufferedForAllControls(this);
            apiService = new ApiService();
            panel8.Margin = new Padding(0, 0, 0, 0);       // No margin at the bottom
            dataGridView3.Margin = new Padding(0, 0, 0, 0);


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
            this.Shown += Form1_Shown; // Tambahkan ini
        }
        // Event handler untuk form shown
        private void Form1_Shown(object sender, EventArgs e)
        {
            RefreshIconButtons();
        }
        // Method untuk refresh icon buttons
        private void RefreshIconButtons()
        {
            this.SuspendLayout();
            foreach (Control c in this.Controls)
            {
                RecursiveRefreshIcons(c);
            }
            this.ResumeLayout(true);
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
                if (prop != null)
                {
                    prop.SetValue(c, true, null);
                }

                SetDoubleBufferedForAllControls(c);
            }
        }
        public async Task refreshCacheTransaction()
        {
            string sourceDirectory = "DT-Cache\\Transaction\\transaction.data"; // Path to source
            string destinationDirectory = "DT-Cache\\Transaction\\HistoryTransaction"; // Path to destination
            TimeSpan timeSpan = TimeSpan.FromHours(20); // 20 hours time span

            try
            {
                // 1. Read JSON file
                if (!File.Exists(sourceDirectory))
                {
                    return; // Exit if the file does not exist
                }
                string jsonData = File.ReadAllText(sourceDirectory);
                JObject data = JObject.Parse(jsonData);

                // 2. Get the "data" array
                JArray transactions = (JArray)data["data"];
                if (transactions == null || transactions.Count == 0)
                {
                    return; // Exit if there are no transactions
                }

                // 3. Parse the "updated_at" of the first transaction
                JObject firstTransaction = (JObject)transactions[0];
                DateTime? firstTransactionDate = null;

                // 4. Check if the "updated_at" exists and parse it
                if (firstTransaction["created_at"] != null)
                {
                    // Mengambil nilai dari invoice_due_date dan mengganti titik dengan titik koma
                    string invoiceDueDate = firstTransaction["created_at"].ToString();

                    // Menggunakan Regex untuk mengganti semua titik (.) dengan titik koma (:) hanya di bagian waktu
                    string updatedInvoiceDueDate = Regex.Replace(invoiceDueDate, @"(\d)\.(\d)", "$1:$2");

                    // Parse the invoice_due_date string to a DateTime object
                    DateTime parsedInvoiceDueDate;
                    if (DateTime.TryParseExact(updatedInvoiceDueDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedInvoiceDueDate))
                    {
                        // Store the parsed date and time from invoice_due_date
                        firstTransactionDate = parsedInvoiceDueDate;
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(sourceDirectory);
                        DateTime fileCreationDate = fileInfo.CreationTime.Date; // Mengambil tanggal pembuatan file
                        if (fileCreationDate != DateTime.Now.Date)
                        {
                            // Check if today is after 6 AM before executing the block
                            if (DateTime.Now.Hour >= 6)
                            {
                                try
                                {
                                    shiftReport c = new shiftReport();
                                    //c.SyncCompleted += SyncCompletedHandler;
                                    await c.SyncDataTransactions();
                                    // Move the files created after the specified time span
                                    transactionFileMover.MoveFilesCreatedAfter(baseOutlet.ToString(), sourceDirectory, destinationDirectory, TimeSpan.FromHours(20));
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    LoggerUtil.LogError(ex, "Error moving transaction files: {ErrorMessage}", ex.Message);
                                }
                            }
                        }
                    }
                }
                // 5. Compare the "invoice_due_date" with current date and time
                if (firstTransactionDate.HasValue)
                {
                    DateTime currentDateTime = DateTime.Now; // Current local date and time
                    // Check if the first transaction date is different from today's date
                    if (firstTransactionDate.HasValue && firstTransactionDate.Value.Date != currentDateTime.Date)
                    {
                        // Check if the current time is after 6 AM
                        if (currentDateTime.Hour >= 6)
                        {
                            try
                            {
                                shiftReport c = new shiftReport();
                                //c.SyncCompleted += SyncCompletedHandler;
                                await c.SyncDataTransactions();

                                // Move the files created after the specified time span (20 hours in this case)
                                transactionFileMover.MoveFilesCreatedAfter(baseOutlet.ToString(), sourceDirectory, destinationDirectory, TimeSpan.FromHours(20));
                            }
                            catch (Exception ex)
                            {
                                LoggerUtil.LogError(ex, "Error moving transaction files: {ErrorMessage}", ex.Message);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Unexpected error: {ErrorMessage}", ex.Message);
            }
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

            using (Offline_addCartForm Offline_addCartForm = new Offline_addCartForm(menu.id.ToString(), menu.name.ToString(), selectedServingTypeallItems))
            {
                Offline_addCartForm.Owner = background;
                background.Show();

                DialogResult result = Offline_addCartForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    background.Dispose();
                    LoadCart();
                    selectedServingTypeallItems = Offline_addCartForm.selectedServingTypeall;
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
            LoadDataListby();
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
            LoadDataWithPagingAsync();
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
                    MessageBox.Show("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                    CacheDataApp form3 = new CacheDataApp("Sync");
                    form3.Show();
                }/*
                string searchText = diskonID.ToString() ?? "";
                if (searchText != "0" && searchText != "-1")
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
                }*/
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

        //button delete
        private async void button5_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                // Path for the cart data cache file
                string filePath = "DT-Cache\\Transaction\\Cart.data";

                // Cek apakah file Cart.data ada
                if (File.Exists(filePath))
                {
                    cmbDiskon.SelectedIndex = 0;
                    // Membaca isi file Cart.data
                    string cartJson = File.ReadAllText(filePath);

                    // Deserialize data file Cart.data
                    var cartData = JsonConvert.DeserializeObject<JObject>(cartJson);


                    // Ambil daftar cart_details
                    var cartDetails = cartData["cart_details"] as JArray;

                    // Cek apakah ada item yang sudah dipesan (is_ordered == 1)
                    bool isAnyOrdered = cartDetails.Any(item => item["is_ordered"].ToString() == "1");

                    if (!isAnyOrdered)
                    {
                        // Jika tidak ada yang dipesan, hapus file Cart.data
                        await DeleteCartFile();
                        ReloadCart();
                        return;
                    }

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
                    using (var Offline_deleteForm = new Offline_deleteForm(cartID.ToString()))
                    {
                        Offline_deleteForm.Owner = background;
                        background.Show();

                        DialogResult result = Offline_deleteForm.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            background.Dispose();
                            ReloadCart();
                        }
                        else
                        {
                            MessageBox.Show("Gagal hapus keranjang, Silahkan coba lagi");
                            ReloadCart();
                            background.Dispose();
                        }
                    }

                }
                else
                {
                    MessageBox.Show("Keranjang kosong.", "DT-Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while reading the cart file: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        public async Task DeleteCartFile()
        {
            try
            {
                // Tentukan lokasi path file yang ingin dihapus
                string filePath = "DT-Cache\\Transaction\\Cart.data";

                // Cek apakah file tersebut ada
                if (File.Exists(filePath))
                {
                    // Hapus file
                    File.Delete(filePath);
                }
                else
                {
                    MessageBox.Show("Keranjang kosong.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Menangani error jika terjadi masalah saat penghapusan file
                MessageBox.Show("Terjadi kesalahan saat menghapus file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task LoadDataListby()
        {
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

            using (Offline_addCartForm Offline_addCartForm = new Offline_addCartForm(id.ToString(), nama.ToString(), selectedServingTypeallItems))
            {
                Offline_addCartForm.Owner = background;

                background.Show();

                DialogResult result = Offline_addCartForm.ShowDialog();

                if (result == DialogResult.OK)
                {

                    background.Dispose();
                    LoadCart();
                    // Settings were successfully updated, perform any necessary actions
                    selectedServingTypeallItems = Offline_addCartForm.selectedServingTypeall;

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
                            using (Offline_addCartForm Offline_addCartForm = new Offline_addCartForm(menu.id.ToString(), menu.name.ToString(), selectedServingTypeallItems))
                            {
                                Offline_addCartForm.Owner = background;

                                background.Show();

                                DialogResult result = Offline_addCartForm.ShowDialog();

                                if (result == DialogResult.OK)
                                {
                                    // Dispose of the background form now that the addCartForm form has been closed
                                    background.Dispose();
                                    LoadCart();
                                    selectedServingTypeallItems = Offline_addCartForm.selectedServingTypeall;
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
                        using (Offline_addCartForm Offline_addCartForm = new Offline_addCartForm(menu.id.ToString(), menu.name.ToString(), selectedServingTypeallItems))
                        {
                            Offline_addCartForm.Owner = background;

                            background.Show();

                            DialogResult result = Offline_addCartForm.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                // Dispose of the background form now that the addCartForm form has been closed
                                background.Dispose();
                                LoadCart();
                                selectedServingTypeallItems = Offline_addCartForm.selectedServingTypeall;
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

        // Method to save FlowLayoutPanel data to a local binary file
        public async void SaveFlowLayoutPanelToBinFile()
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream("Menu.bin", FileMode.Create, FileAccess.Write, FileShare.None);

                List<SerializablePanel> panelControls = new List<SerializablePanel>();
                foreach (Control control in dataGridView3.Controls)
                {
                    if (control is SerializablePanel panel)
                    {
                        panelControls.Add(panel);
                    }
                }

                formatter.Serialize(stream, panelControls);
                stream.Close();
            }
            catch (Exception ex)
            {
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
        public async Task LoadCart()
        {
            buttonDelete.Enabled = false;
            buttonPayment.Enabled = false;
            await LoadCartData();
            buttonDelete.Enabled = true;
            buttonPayment.Enabled = true;
        }
        public async Task LoadCartData()
        {
            try
            {

                // Path for the cart data cache
                string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";

                // Check if the cart file exists
                if (File.Exists(cacheFilePath))
                {

                    string cartJson = File.ReadAllText(cacheFilePath);
                    var cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

                    // Initialize default values in case no data is available
                    if (cartData["cart_details"] == null || cartData["cart_details"].Count() == 0)
                    {
                        File.Delete(cacheFilePath);
                        lblDetailKeranjang.Text = "Keranjang: Kosong";
                        lblDiskon1.Text = "Rp. 0";
                        lblSubTotal1.Text = "Rp. 0,-";
                        lblTotal1.Text = "Rp. 0,-";
                        buttonPayment.Text = "Bayar ";
                        subTotalPrice = 0; diskonID = 0;

                        //set tombol disc
                        iconButtonGet.Text = "Gunakan Disc";
                        iconButtonGet.ForeColor = Color.FromArgb(31, 30, 68);
                        isDiscountActive = false;
                        iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);

                        //reset servingtype
                        selectedServingTypeallItems = 1;

                    }
                    else
                    {
                        // Retrieve cart details
                        var cartDetails = cartData["cart_details"] as JArray;
                        if (!string.IsNullOrEmpty(cartData["is_splitted"]?.ToString()) && cartData["is_splitted"]?.ToString() == "1")
                        {
                            ButtonSplit.Enabled = false;
                        }
                        else
                        {
                            ButtonSplit.Enabled = true;
                        }
                        // Set the first cart_detail_id as cart_id
                        var cartDetail = cartDetails.FirstOrDefault();
                        cartID = cartDetail?["cart_detail_id"].ToString() ?? "null"; // Get first cart_detail_id for cart_id
                        totalCart = cartData["total"]?.ToString() ?? "0";
                        diskonID = 0; //belum pakai disc
                                      // Memastikan jika discount_id adalah -1, ubah menjadi 0
                        int discountId = cartData["discount_id"] != null ? (int)cartData["discount_id"] : -1;
                        if (discountId == -1)
                        {
                            discountId = 0; // Set to 0 if it's -1
                            cartData["discount_id"] = discountId; // Update the cartData
                            File.WriteAllText(cacheFilePath, cartData.ToString()); // Save back to file
                        }

                        if (cartData["discount_id"] != null && int.Parse(cartData["discount_id"].ToString()) != 0)
                        {
                            iconButtonGet.Text = "Hapus Disc";
                            iconButtonGet.ForeColor = Color.Red;
                            isDiscountActive = true;
                            iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);
                        }
                        else
                        {
                            iconButtonGet.Text = "Gunakan Disc";
                            iconButtonGet.ForeColor = Color.FromArgb(31, 30, 68);
                            isDiscountActive = false;
                            iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);

                        }

                        customer_name = (string)null;
                        customer_seat = (string)null;
                        // Set customer information
                        customer_name = cartData["customer_name"]?.ToString() ?? (string)null;
                        customer_seat = cartData["customer_seat"]?.ToString() ?? (string)null;
                        if (!string.IsNullOrEmpty(customer_name) && !string.IsNullOrEmpty(customer_seat))
                        {
                            lblDetailKeranjang.Text = $"Keranjang: Nama : {customer_name} Seat : {customer_seat}";
                        }
                        else
                        {
                            lblDetailKeranjang.Text = $"Keranjang: Nama : ?? Seat : ??";
                        }

                        // Calculate subtotal and total
                        int subtotal = cartData["subtotal"] != null ? int.Parse(cartData["subtotal"].ToString()) : 0;

                        int total = cartData["total"] != null ? int.Parse(cartData["total"].ToString()) : 0;
                        lblDiskon1.Text = string.Format("Rp. {0:n0},-", total - subtotal);
                        lblSubTotal1.Text = string.Format("Rp. {0:n0},-", subtotal);
                        subTotalPrice = subtotal;
                        lblTotal1.Text = string.Format("Rp. {0:n0},-", total);
                        buttonPayment.Text = string.Format("Bayar Rp. {0:n0},-", total);

                        // Prepare data for the DataGrid
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("MenuID", typeof(string));
                        dataTable.Columns.Add("CartDetailID", typeof(string));
                        dataTable.Columns.Add("Jenis", typeof(string));
                        dataTable.Columns.Add("Menu", typeof(string));
                        dataTable.Columns.Add("Total Harga", typeof(string));
                        dataTable.Columns.Add("Note", typeof(string));

                        // Group cart items by serving type
                        var menuGroups = cartDetails.GroupBy(x => x["serving_type_name"].ToString()).ToList();

                        foreach (var group in menuGroups)
                        {
                            // Add a separator row for each serving type group
                            AddSeparatorRow(dataTable, group.Key, dataGridView1);

                            foreach (var menu in group)
                            {
                                string menuName = menu["menu_name"].ToString();
                                string menuType = menu["menu_type"].ToString();
                                string? discountedPrice = menu["discounted_price"].ToString();
                                string servingTypeName = menu["serving_type_name"].ToString();
                                int quantity = menu["qty"] != null ? (int)menu["qty"] : 0;
                                decimal price = menu["price"] != null ? decimal.Parse(menu["price"].ToString()) : 0;
                                string noteItem = menu["note_item"]?.ToString() ?? "";
                                int totalPrice = menu["total_price"] != null ? (int)menu["total_price"] : 0;
                                string ordered = "";
                                if (int.Parse(menu["is_ordered"].ToString()) == 1)
                                {
                                    ordered = " (Ordered)";
                                }

                                if (quantity == 0)
                                {
                                    continue;
                                }
                                // Add rows for each cart item
                                dataTable.Rows.Add(
                                    menu["menu_id"],
                                    menu["cart_detail_id"],
                                    servingTypeName,
                                    $"{quantity}X {menuName} {menu["menu_detail_name"]} {ordered}",
                                    string.Format("Rp. {0:n0},-", totalPrice),
                                    noteItem
                                );
                                if (!string.IsNullOrEmpty(noteItem))
                                {
                                    if (!string.IsNullOrEmpty(discountedPrice) && discountedPrice != "0")
                                    {
                                        dataTable.Rows.Add(
                                            null,
                                            null,
                                            null,
                                            "  *catatan : " + (noteItem),
                                            "  *discounted ",
                                            null);
                                        return;
                                    }
                                    dataTable.Rows.Add(
                                        null,
                                        null,
                                        null,
                                        "  *catatan : " + (noteItem),
                                        null,
                                        null);
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(discountedPrice) && discountedPrice != "0")
                                    {
                                        dataTable.Rows.Add(
                                            null,
                                            null,
                                            null,
                                            null,
                                            "  *discounted ",
                                            null);
                                    }
                                }
                            }
                        }

                        // Bind the data to the DataGridView
                        dataGridView1.DataSource = dataTable;

                        // Hide unnecessary columns
                        dataGridView1.Columns["MenuID"].Visible = false;
                        dataGridView1.Columns["CartDetailID"].Visible = false;
                        dataGridView1.Columns["Jenis"].Visible = false;
                        dataGridView1.Columns["Note"].Visible = false;

                        // Apply formatting to the DataGridView
                        DataGridViewCellStyle boldStyle = new DataGridViewCellStyle();
                        boldStyle.Font = new Font(dataGridView1.Font, FontStyle.Italic);
                        dataGridView1.Columns["Menu"].DefaultCellStyle = boldStyle;
                        dataGridView1.Columns["Menu"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                        dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                        dataGridView1.Columns["Menu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                        await LoadDataDiscount(); // Ensure LoadDataDiscount is awaited
                        if (discountId != 0)
                        {
                            // Mencari dan memilih diskon yang sesuai
                            for (int i = 0; i < cmbDiskon.Items.Count; i++)
                            {
                                // Mengambil objek DataDiscountCart dari ComboBox
                                var discount = cmbDiskon.Items[i] as DataDiscountCart;

                                if (discount != null && discount.id == discountId)
                                {
                                    // Menentukan indeks item yang sesuai dan memilihnya
                                    cmbDiskon.SelectedIndex = i;
                                    break;
                                }
                            }
                        }

                    }
                }
                else
                {
                    string cacheFilePathSplit = "DT-Cache\\Transaction\\Cart_main_split.data";
                    if (File.Exists(cacheFilePathSplit))
                    {
                        // Ganti nama file
                        File.Move(cacheFilePathSplit, cacheFilePath);
                        LoadCartData();
                        selectedServingTypeallItems = 1;
                        return;
                    }
                    else
                    {
                        // If file does not exist, set defaults
                        lblDetailKeranjang.Text = "Keranjang: Kosong";
                        lblDiskon1.Text = "Rp. 0";
                        lblSubTotal1.Text = "Rp. 0,-";
                        lblTotal1.Text = "Rp. 0,-";
                        buttonPayment.Text = "Bayar ";
                        subTotalPrice = 0;
                        ButtonSplit.Enabled = true;

                        //set tombol disc
                        iconButtonGet.Text = "Gunakan Disc";
                        iconButtonGet.ForeColor = Color.FromArgb(31, 30, 68);
                        isDiscountActive = false;
                        iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);

                        //reset servingtype
                        selectedServingTypeallItems = 1;
                    }
                }
                ReloadDisc();

                await SignalReload(); // Ensure the UI is updated
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
        private void AddSeparatorRow(DataTable dataTable, string groupKey, DataGridView dataGridView)
        {
            // Add separator row to DataTable
            dataTable.Rows.Add(null, null, null, groupKey + "s\n", null, null); // Add a separator row with groupKey in column 4

            // Get the last row index just added
            int lastRowIndex = dataTable.Rows.Count - 1;

            // Add the row to DataGridView
            dataGridView.DataSource = dataTable;

            // Apply styles to specific columns for the separator row
            int[] cellIndexesToStyle = { 3, 4 }; // Columns 4 and 5 for styling
            SetCellStyle(dataGridView.Rows[lastRowIndex], cellIndexesToStyle, Color.WhiteSmoke, FontStyle.Bold);

            // Simulate hiding the row by setting its height to 0
            dataGridView.Rows[lastRowIndex].Height = 0;

            // Optionally, you can adjust column visibility only for specific cells if needed
            // For example: Hide columns 1, 2, 3, 6 only for this row
            // This is handled by applying the height to the row, not changing cell visibility.
        }


        /*
                private void AddSeparatorRow(DataTable dataTable, string groupKey, DataGridView dataGridView)
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
                }*/
        private void SetCellStyle(DataGridViewRow row, int[] cellIndexes, Color backgroundColor, FontStyle fontStyle)
        {
            foreach (int index in cellIndexes)
            {
                row.Cells[index].Style.BackColor = backgroundColor;
                row.Cells[index].Style.Font = new Font(dataGridView1.Font, fontStyle);
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
            buttonPayment.Text = "Bayar";
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
                    string id = "0";
                    string cartdetailid = "0";

                    // Periksa nilai DBNull untuk MenuID
                    if (selectedRow.Cells["MenuID"].Value != DBNull.Value)
                    {
                        id = selectedRow.Cells["MenuID"].Value.ToString();
                        //id = Convert.ToInt32(selectedRow.Cells["MenuID"].Value);
                    }
                    else
                    {
                        //MessageBox.Show("MenuID tidak valid.");
                        return;
                    }

                    // Periksa nilai DBNull untuk CartDetailID
                    if (selectedRow.Cells["CartDetailID"].Value != DBNull.Value)
                    {
                        cartdetailid = selectedRow.Cells["CartDetailID"].Value.ToString();
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
                        using (Offline_updateCartForm Offline_updateCartForm = new Offline_updateCartForm(id.ToString(), cartdetailid.ToString()))
                        {
                            Offline_updateCartForm.Owner = background;

                            background.Show();

                            DialogResult result = Offline_updateCartForm.ShowDialog();

                            // Handle the result if needed
                            if (result == DialogResult.OK)
                            {
                                background.Dispose();
                                ReloadCart();
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




        private void SimpanBill_Click(object sender, EventArgs e)
        {
            int rowCount = dataGridView1.RowCount;
            if (rowCount == 0)
            {
                MessageBox.Show("Keranjang masih kosong!", "DT-Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

            if (isSplitted != 0)
            {
                MessageBox.Show("Keranjang ini telah di Split! tidak bisa diSimpan.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                using (Offline_saveBill saveBill = new Offline_saveBill(cartID, customer_name, customer_seat))
                {
                    saveBill.Owner = background;

                    background.Show();

                    //DialogResult dialogResult = dataBill.ShowDialog();

                    //background.Dispose();
                    DialogResult result = saveBill.ShowDialog();

                    // Handle the result if needed
                    if (result == DialogResult.OK)
                    {
                        cmbDiskon.SelectedIndex = 0;

                        background.Dispose();
                        ReloadCart();
                    }
                    else
                    {
                        MessageBox.Show("Gagal Simpan, Silahkan coba lagi");
                        background.Dispose();
                        ReloadCart();
                    }
                }
            }
        }
        private async void ReloadDisc()
        {
            if (cmbDiskon.SelectedItem == null || cmbDiskon == null) { return; }
            int selectedDiskon = (int)cmbDiskon.SelectedValue;
            if (selectedDiskon == 0 || selectedDiskon == -1) { return; }
            ProcessDiscountCart(selectedDiskon);

        }
        private async void btnGet_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnGet_Click));
            if (string.IsNullOrEmpty(cartID) || dataDiscountListCart == null || !dataDiscountListCart.Any())
            {
                MessageBox.Show("Keranjang kosong. Silakan tambahkan item ke keranjang sebelum menerapkan diskon.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (isDiscountActive)
            {
                // Mengambil data cart dari file cache
                string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
                string cartDataJson = File.ReadAllText(cartDataPath);
                var cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);
                // Memperbarui data cart dengan nilai diskon yang dipilih
                cartData["discount_id"] = 0; // Gunakan selectedDiskon yang sudah diganti
                cartData["discount_code"] = (string)null;
                cartData["discounts_value"] = (string)null;
                cartData["discounts_is_percent"] = (string)null;
                cartData["discounted_price"] = 0;
                cartData["total"] = cartData["subtotal"];

                // Menyimpan kembali data cart yang telah diperbarui ke file cache
                File.WriteAllText(cartDataPath, cartData.ToString());
                // Refresh UI dengan data terbaru
                ReloadCart();
                cmbDiskon.SelectedIndex = 0;
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
                    LoadCart();
                }
                // Mengambil nilai diskon yang dipilih dari combobox

                // Jika nilai diskon yang dipilih adalah -1, ubah menjadi 0
                if (selectedDiskon == -1)
                {
                    selectedDiskon = 0;
                }
                ProcessDiscountCart(selectedDiskon);
                // Refresh UI dengan data terbaru
                ReloadCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menggunakan diskon: " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private async void ProcessDiscountCart(int selectedDiskon)
        {
            try
            {
                // Mengambil data cart dari file cache
                string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
                string cartDataJson = File.ReadAllText(cartDataPath);
                var cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);

                // Menentukan subtotal dan quantity item dalam cart
                var cartDetailsArray = (JArray)cartData["cart_details"];
                var quantity = cartDetailsArray.Sum(item => (int)item["qty"]);
                int subtotal_item = cartDetailsArray.Sum(item => (int)item["price"] * (int)item["qty"]);

                int total_item_withDiscount = 0;
                int discountPercent = 0;
                int discounted_peritemPrice = 0;
                int discountValue = 0;
                int discountedPrice = 0;
                int discountMax = 0;
                int tempTotal = 0;

                string discountCode = (string)null;

                if (selectedDiskon != 0) // Jika diskon yang dipilih bukan 0, proses diskon
                {
                    // Mendapatkan informasi diskon berdasarkan id diskon yang dipilih
                    var discount = dataDiscountListCart.FirstOrDefault(d => d.id == selectedDiskon);
                    if (discount != null)
                    {
                        discountPercent = discount.is_percent;
                        discountValue = discount.value;
                        discountMax = discount.max_discount ?? int.MaxValue;
                        discountCode = discount.code ?? "";


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
                }
                // Memperbarui data cart dengan nilai diskon yang dipilih
                cartData["discount_id"] = selectedDiskon; // Gunakan selectedDiskon yang sudah diganti
                cartData["discount_code"] = discountCode;
                cartData["discounts_value"] = discountValue;
                cartData["discounts_is_percent"] = discountPercent;
                cartData["discounted_price"] = discountedPrice;
                cartData["total"] = total_item_withDiscount;


                lblTotal1.Text = string.Format("Rp. {0:n0},-", total_item_withDiscount);
                lblDiskon1.Text = string.Format("Rp. - {0:n0},-", discountedPrice);
                // Menyimpan kembali data cart yang telah diperbarui ke file cache
                File.WriteAllText(cartDataPath, cartData.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menggunakan diskon: " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        bool isDiscountActive = false;

        private async Task<int> cekPeritemDiskon()
        {
            int lanjutan;

            try
            {
                // Path for the cart data cache
                string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";
                // Check if the cart file exists
                if (File.Exists(cacheFilePath))
                {
                    string cartJson = File.ReadAllText(cacheFilePath);
                    var cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

                    // Retrieve cart details
                    var cartDetails = cartData["cart_details"] as JArray;
                    foreach (var itema in cartDetails)
                    {
                        if (int.Parse(itema["discounted_price"].ToString()) != 0)
                        {
                            lanjutan = 1;

                            itema["discount_id"] = 0;
                            itema["discount_code"] = (string)null;
                            itema["discounts_value"] = 0;
                            itema["discounted_price"] = 0;
                            itema["discounts_is_percent"] = 0;
                            itema["total_price"] = (int)itema["price"] * (int)itema["qty"];
                            cartData["total"] = cartDetails.Sum(item => (int)item["price"] * (int)item["qty"]);
                        }
                    }
                    // Menyimpan kembali data cart yang telah diperbarui ke file cache
                    File.WriteAllText(cacheFilePath, cartData.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal cek diskon: " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

            lanjutan = 0;
            if (lanjutan == 1)
            {
                return 1;
            }
            return 0;
        }

        private void lblSubTotal1_Click(object sender, EventArgs e)
        {

        }


        private async void listBill_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    MessageBox.Show("Keranjang belum kosong. tidak dapat membuka keranjang lagi", "DT-Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

                using (Offline_listBill Offline_listBill = new Offline_listBill())
                {
                    Offline_listBill.Owner = background;
                    background.Show();
                    await Offline_listBill.LoadData();
                    DialogResult result = Offline_listBill.ShowDialog();

                    background.Dispose();
                    ReloadCart();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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

            using (Offline_dataDiskon Offline_dataDiskon = new Offline_dataDiskon())
            {
                Offline_dataDiskon.Owner = background;

                background.Show();

                DialogResult dialogResult = Offline_dataDiskon.ShowDialog();

                background.Dispose();

                if (dialogResult == DialogResult.OK && Offline_dataDiskon.ReloadDataInBaseForm)
                {
                    ReloadData();
                }
            }
        }

        private void cmbDiskon_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Mengambil nilai diskon yang dipilih dari combobox
                int selectedDiskon = (int)cmbDiskon.SelectedValue;

                // Jika nilai diskon yang dipilih adalah -1, ubah menjadi 0
                if (selectedDiskon == -1)
                {
                    selectedDiskon = 0;
                }

                // Mengambil data cart dari file cache
                string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
                string cartDataJson = File.ReadAllText(cartDataPath);
                var cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);

                // Menentukan subtotal dan quantity item dalam cart
                var cartDetailsArray = (JArray)cartData["cart_details"];
                var quantity = cartDetailsArray.Sum(item => (int)item["qty"]);
                int subtotal_item = int.Parse(cartData["subtotal"].ToString());

                int total_item_withDiscount = 0;
                int discountPercent = 0;
                int discountValue = 0;
                int discountedPrice = 0;
                int discountMax = 0;

                if (selectedDiskon != 0) // Jika diskon yang dipilih bukan 0, proses diskon
                {
                    // Mendapatkan informasi diskon berdasarkan id diskon yang dipilih
                    var discount = dataDiscountListCart.FirstOrDefault(d => d.id == selectedDiskon);
                    if (discount != null)
                    {
                        discountPercent = discount.is_percent;
                        discountValue = discount.value;
                        discountMax = discount.max_discount ?? 0;

                        int tempTotal = 0;
                        if (discountPercent != 0)
                        {
                            tempTotal = subtotal_item * discountValue / 100;
                            discountedPrice = Math.Min(tempTotal, discountMax);
                            total_item_withDiscount = subtotal_item - discountedPrice;
                            discountedPrice = discountedPrice / quantity;
                        }
                        else
                        {
                            tempTotal = subtotal_item - discountValue;
                            discountedPrice = Math.Min(tempTotal, discountMax);
                            total_item_withDiscount = subtotal_item - discountedPrice;
                            discountedPrice = discountedPrice / quantity;
                        }
                    }
                }

                // Memperbarui data cart dengan nilai diskon yang dipilih
                cartData["discount_id"] = selectedDiskon; // Gunakan selectedDiskon yang sudah diganti
                cartData["discount_code"] = cmbDiskon.SelectedText.ToString();
                cartData["discounts_value"] = discountValue;
                cartData["discounts_is_percent"] = discountPercent;
                cartData["discounted_price"] = discountedPrice;
                cartData["total"] = total_item_withDiscount;

                // Menyimpan kembali data cart yang telah diperbarui ke file cache
                File.WriteAllText(cartDataPath, cartData.ToString());

                // Refresh UI dengan data terbaru
                LoadCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
            }
        }


        private void ButtonSplit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null || dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Keranjang masih kosong!", "DT-Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            using (Offline_splitBill splitBill = new Offline_splitBill(cartID))
            {
                splitBill.Owner = background;

                background.Show();
                DialogResult result = splitBill.ShowDialog();
                background.Dispose();
                ReloadCart();
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

        private void buttonPayment_Click(object sender, EventArgs e)
        {
            // Path for the cart data cache file
            string filePath = "DT-Cache\\Transaction\\Cart.data";

            // Cek apakah file Cart.data ada
            if (File.Exists(filePath))
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

                using (Offline_payForm Offline_payForm = new Offline_payForm(baseOutlet, cartID, totalCart, lblTotal1.Text.ToString(), customer_seat, customer_name, this))
                {
                    SignalReloadPayform();

                    //pengirim.SendSignal("Payment");
                    Offline_payForm.Owner = background;

                    background.Show();

                    //DialogResult dialogResult = dataBill.ShowDialog();

                    //background.Dispose();
                    DialogResult result = Offline_payForm.ShowDialog();

                    // Handle the result if needed
                    if (result == DialogResult.OK)
                    {
                        cmbDiskon.SelectedIndex = 0;
                        background.Dispose();
                        ReloadCart();
                        LoadCart();
                        //cartID = "";
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
            else
            {
                MessageBox.Show("Keranjang masih kosong!", "DT-Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
