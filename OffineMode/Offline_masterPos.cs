using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;
using FontAwesome.Sharp;
using KASIR.Database;
using KASIR.Helper;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.OffineMode;
using KASIR.Properties;
using KASIR.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TheArtOfDevHtmlRenderer.Adapters;
using FontStyle = System.Drawing.FontStyle;
using Menu = KASIR.Model.Menu;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using SystemFonts = System.Drawing.SystemFonts;


namespace KASIR.OfflineMode
{
    public partial class Offline_masterPos : Form
    {
        private readonly IInternetService _internetServices;

        private readonly string baseOutlet = Settings.Default.BaseOutlet;
        private readonly string baseUrl = Settings.Default.BaseAddress;
        private bool allDataLoaded;
        private List<Menu> allMenuItems;
        private readonly ApiService apiService;
        private string cartID;
        private int currentPageIndex = 1;
        private string customer_name;
        private string customer_seat;
        private List<DataDiscountCart> dataDiscountListCart;

        private bool isDiscountActive;
        private bool isLoading;
        private bool configViewDataProductGrid;
        private int diskonID;
        private string cardDetailID = "cardID";

        private int items;
        private DataTable listDataTable;
        private readonly Dictionary<Menu, Image> menuImageDictionary = new();

        private Dictionary<string, Control> nameToControlMap;

        private readonly int pageSize = 35;
        private int selectedServingTypeallItems;
        private int subTotalPrice;
        private int totalPageCount;
        private int hardcodePajak = 10;

        public Offline_masterPos()
        {
            //Init Component
            InitializeComponent();

            panel8.Margin = new Padding(0, 0, 0, 0);
            dataGridView3.Margin = new Padding(0, 0, 0, 0);

            SetDoubleBufferedForAllControls(this);
            InitializeVisualRounded();

            apiService = new ApiService();


            _internetServices = new InternetService();
            txtCariMenu.Enabled = false;

            btnCari.Visible = false;
            dataGridView3.Scroll += dataGridView3_Scroll;

            Shown += MasterPos_ShownWrapper;

            KeyPreview = true;
            KeyDown += YourForm_KeyDown;
            Shown += Form1_Shown; // Tambahkan ini
        }

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

        private void YourForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Periksa apakah Ctrl dan Space ditekan bersamaan
            if (e.Control && e.KeyCode == Keys.F)
            {
                // Mengatur fokus ke CariMenu
                if (txtCariMenu.Visible)
                {
                    _ = txtCariMenu.Focus();

                    // Menambahkan teks ke CariMenu (opsional)
                    txtCariMenu.Text += ">";

                    // Memindahkan kursor ke akhir teks di CariMenu
                    txtCariMenu.SelectionStart = txtCariMenu.Text.Length;
                    txtCariMenu.SelectionLength = 0;
                }
                else
                {
                    _ = txtCariMenuList.Focus();

                    txtCariMenuList.Text += ">";

                    txtCariMenuList.SelectionStart = txtCariMenuList.Text.Length;
                    txtCariMenuList.SelectionLength = 0;
                }
            }
        }

        // Pagenation Begin
        private async Task LoadDataWithPagingAsync()
        {
            // Buat direktori cache jika tidak ada
            if (!Directory.Exists("DT-Cache"))
            {
                _ = Directory.CreateDirectory("DT-Cache");
            }

            // Muat semua data menu
            allMenuItems = await LoadMenuDataAsync();
            totalPageCount = (int)Math.Ceiling((double)allMenuItems.Count / pageSize);
            await LoadCurrentPage();
        }

        private async Task<List<Menu>> LoadMenuDataAsync()
        {
            string cachePath = $"DT-Cache\\menu_outlet_id_{baseOutlet}.data";

            if (File.Exists(cachePath))
            {
                string json = await File.ReadAllTextAsync(cachePath);
                GetMenuModel? menuModel = JsonConvert.DeserializeObject<GetMenuModel>(json);
                return menuModel.data.ToList();
            }
            else
            {
                string response = await apiService.Get("/menu?outlet_id=" + baseOutlet);
                GetMenuModel? menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                await File.WriteAllTextAsync(cachePath, JsonConvert.SerializeObject(menuModel));
                return menuModel.data.ToList();
            }
        }

        [DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(
    int nLeftRect,      // x-coordinate of upper-left corner
    int nTopRect,       // y-coordinate of upper-left corner
    int nRightRect,     // x-coordinate of lower-right corner
    int nBottomRect,    // y-coordinate of lower-right corner
    int nWidthEllipse,  // width of ellipse
    int nHeightEllipse  // height of ellipse
);

        private async Task LoadCurrentPage()
        {
            isLoading = true;

            List<Menu> pagedData = allMenuItems
                .Skip((currentPageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            dataGridView3.SuspendLayout();
            dataGridView3.Controls.Clear(); // reset dulu biar ga numpuk
            items = 0;

            foreach (Menu menu in pagedData)
            {
                items++;

                Panel tileButton = CreateTileButton(menu);
                dataGridView3.Controls.Add(tileButton);

                // ambil pictureBox pertama dari panel gambar
                PictureBox pic = tileButton
                    .Controls.OfType<Panel>().First()
                    .Controls.OfType<PictureBox>().First();

                await LoadImageToPictureBox(pic, menu);

                lblCountingItems.Text = $"{items} items";
            }

            dataGridView3.ResumeLayout();
            txtCariMenu.Enabled = true;

            txtCariMenu.PlaceholderText = "Cari Menu Items...";

            isLoading = false;
            if (currentPageIndex >= totalPageCount)
                allDataLoaded = true;
        }

        private Panel CreateTileButton(Menu menu)
        {
            // Panel utama (kotak produk)
            Panel tileButton = new()
            {
                Width = 130 * 80 / 100,
                Height = 180 * 80 / 100,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(5),
                Cursor = Cursors.Hand,
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 130 * 80 / 100, 180 * 80 / 100, 15, 15))
            };

            // Panel gambar (atas)
            Panel pictureBoxPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 90 * 80 / 100,
                BackColor = Color.White,
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 130 * 80 / 100, 90 * 80 / 100, 10, 10)),
                Padding = new Padding(3)
            };

            PictureBox pictureBox = new()
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            pictureBoxPanel.Controls.Add(pictureBox);

            // Nama produk (tengah)
            Label nameLabel = new()
            {
                Text = menu.name,
                ForeColor = Color.FromArgb(40, 40, 70),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9, FontStyle.Bold),
                AutoSize = false,
                Height = 30 * 80 / 100
            };

            // Harga (bawah)
            Label priceLabel = new()
            {
                Text = string.Format("Rp. {0:n0},-", menu.price),
                ForeColor = Color.FromArgb(80, 80, 110),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8, FontStyle.Regular),
                AutoSize = false,
                Height = 25 * 80 / 100
            };

            // urutannya HARUS: harga dulu → nama → gambar (biar gambar benar2 di atas)
            tileButton.Controls.Add(priceLabel);
            tileButton.Controls.Add(nameLabel);
            tileButton.Controls.Add(pictureBoxPanel);

            // Event klik
            pictureBox.Click += async (sender, e) => await HandlePictureBoxClick(menu);
            nameLabel.Click += async (sender, e) => await HandlePictureBoxClick(menu);
            priceLabel.Click += async (sender, e) => await HandlePictureBoxClick(menu);

            return tileButton;
        }



        private void dataGridView3_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll && !isLoading && !allDataLoaded)
            {
                if (dataGridView3.VerticalScroll.Value + dataGridView3.VerticalScroll.LargeChange >=
                    dataGridView3.VerticalScroll.Maximum)
                {
                    currentPageIndex++;
                    _ = LoadCurrentPage();
                }
            }
        }



        private Panel Ex_CreateTileButton(Menu menu)
        {
            Panel tileButton = new() { Width = 90, Height = 120 };

            Panel pictureBoxPanel = new() { Dock = DockStyle.Fill };

            PictureBox pictureBox = new()
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };

            pictureBoxPanel.Controls.Add(pictureBox);
            tileButton.Controls.Add(pictureBoxPanel);

            Label nameLabel = new()
            {
                Text = menu.name,
                ForeColor = Color.FromArgb(30, 31, 68),
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };
            tileButton.Controls.Add(nameLabel);

            Label typeLabel = new()
            {
                Text = menu.menu_type,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            tileButton.Controls.Add(typeLabel);

            Label priceLabel = new()
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

            foreach (Menu menu in searchResults)
            {
                items++;

                // buat tile
                Panel tileButton = CreateTileButton(menu);
                dataGridView3.Controls.Add(tileButton);

                // cari PictureBox di dalam tile
                PictureBox picBox = tileButton.Controls
                    .OfType<Panel>()
                    .SelectMany(p => p.Controls.OfType<PictureBox>())
                    .FirstOrDefault();

                if (picBox != null)
                {
                    await ReloadImageToPictureBox(picBox, menu);
                }

                lblCountingItems.Text = $"{items} items";
            }

            dataGridView3.ResumeLayout();
            dataGridView3.AutoScroll = true;
            txtCariMenu.Enabled = true;

            // simpan original panel list
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
                    _ = pictureBox.Invoke((MethodInvoker)delegate
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
                        _ = pictureBox.Invoke((MethodInvoker)delegate
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

            using (Offline_addCartForm Offline_addCartForm =
                   new(menu.id.ToString(), menu.name, selectedServingTypeallItems))
            {
                QuestionHelper c = new(null,null,null,null);
                Form background = c.CreateOverlayForm();

                Offline_addCartForm.Owner = background;
                background.Show();

                DialogResult result = Offline_addCartForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    background.Dispose();
                    _ = LoadCart();
                    selectedServingTypeallItems = Offline_addCartForm.selectedServingTypeall;
                }
                else
                {
                    NotifyHelper.Error("Gagal Menambahkan item, silahkan coba lagi");
                    ReloadCart();
                    _ = LoadCart();
                    background.Dispose();
                }
            }
        }
        // pagenation end

        //Config pengaturan list view
        private static readonly SemaphoreSlim semaphore = new(1, 1); // Semaphore for file access

        private async Task LoadConfig()
        {
            string configDirectory = "setting";
            string configFilePath = Path.Combine(configDirectory, "configListMenu.data");

            // Ensure the directory exists
            if (!Directory.Exists(configDirectory))
            {
                _ = Directory.CreateDirectory(configDirectory);
            }

            // Use semaphore to control access
            await semaphore.WaitAsync(); // Wait to enter the semaphore
            try
            {
                if (!File.Exists(configFilePath))
                {
                    string data = "OFF";
                    await File.WriteAllTextAsync(configFilePath, data); // Asynchronously write the initial state
                }
                else
                {
                    string allSettingsData = await File.ReadAllTextAsync(configFilePath); // Asynchronously read the content

                    if (!string.IsNullOrEmpty(allSettingsData))
                    {
                        if (allSettingsData == "ON")
                        {
                            await OnListView();
                            configViewDataProductGrid = false;
                        }
                        else
                        {
                            await OfflistView();
                            configViewDataProductGrid = true;
                        }
                    }
                    else
                    {
                        await OfflistView();
                        string data = "OFF";
                        await File.WriteAllTextAsync(configFilePath, data); // Asynchronously write the reset state
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur while reading or writing to the file
                LoggerUtil.LogError(ex, "An error occurred while loading configuration: {ErrorMessage}", ex.Message);
            }
            finally
            {
                _ = semaphore.Release(); // Release the semaphore
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
            _ = LoadDataListby();
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
            _ = LoadDataWithPagingAsync();
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
            await LoadConfig();
        }

        private async void MasterPos_ShownWrapper(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                _ = Invoke((MethodInvoker)delegate
                {
                    _ = MasterPos_Shown(sender, e);
                });
            });
        }

        private void InitializeVisualRounded()
        {
            // Create a GraphicsPath object with rounded rectangles
            RoundedPanel(panel8);
            RoundedPanel(panelCartArea);
            RoundedPanel(PanelDetailTotal, 8);
            RoundedPanel(panelSearchBox, 8);
        }
        public void IconButtonRounded(IconButton button, bool? isRounded = true, Color? outlineColor = null, int cornerRadius = 20)
        {
            // Jika parameter isRounded tidak diset atau false, kembalikan ke button standar
            if (isRounded == null || isRounded == false)
            {
                button.FlatStyle = FlatStyle.Standard;
                return;
            }

            // Set flat style untuk kontrol penuh
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;

            // Event handler untuk custom drawing
            button.Paint += (sender, e) =>
            {
                // Bersihkan background default
                e.Graphics.Clear(button.Parent.BackColor);

                // Buat path rounded
                GraphicsPath path = new();
                Rectangle bounds = new(0, 0, button.Width, button.Height);

                // Tambahkan arc untuk setiap sudut
                path.AddArc(0, 0, cornerRadius * 2, cornerRadius * 2, 180, 90); // Sudut kiri atas
                path.AddLine(cornerRadius, 0, button.Width - cornerRadius, 0); // Garis atas
                path.AddArc(button.Width - (cornerRadius * 2), 0, cornerRadius * 2, cornerRadius * 2, 270, 90); // Sudut kanan atas
                path.AddLine(button.Width, cornerRadius, button.Width, button.Height - cornerRadius); // Garis kanan
                path.AddArc(button.Width - (cornerRadius * 2), button.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 0, 90); // Sudut kanan bawah
                path.AddLine(button.Width - cornerRadius, button.Height, cornerRadius, button.Height); // Garis bawah
                path.AddArc(0, button.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 90, 90); // Sudut kiri bawah
                path.AddLine(0, button.Height - cornerRadius, 0, cornerRadius); // Garis kiri

                path.CloseFigure();

                // Set region untuk button
                button.Region = new Region(path);

                // Gambar background button
                using (SolidBrush brush = new(button.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Gambar outline jika warna outline ditentukan
                if (outlineColor.HasValue)
                {
                    using (Pen outlinePen = new(outlineColor.Value, 1.8f))
                    {
                        outlinePen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                        e.Graphics.DrawPath(outlinePen, path);
                    }
                }

                // Gambar ikon
                if (button.IconChar != IconChar.None)
                {
                    // Hitung posisi ikon di tengah
                    SizeF iconSize = new(button.IconSize, button.IconSize);
                    PointF iconLocation = new(
                        (button.Width - iconSize.Width) / 2,
                        (button.Height - iconSize.Height) / 2
                    );

                    // Konversi ikon FontAwesome ke bitmap
                    using (Bitmap iconBitmap = GetIconBitmap(button.IconChar, button.IconColor, button.IconSize))
                    {
                        e.Graphics.DrawImage(iconBitmap, iconLocation);
                    }
                }

                // Jika ada teks, gambar teks
                if (!string.IsNullOrEmpty(button.Text))
                {
                    TextRenderer.DrawText(
                        e.Graphics,
                        button.Text,
                        button.Font,
                        bounds,
                        button.ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                    );
                }
            };

            // Event handler untuk resize
            button.Resize += (sender, e) =>
            {
                button.Invalidate();
            };
        }

        // Metode utility untuk konversi ikon FontAwesome ke Bitmap
        private Bitmap GetIconBitmap(IconChar iconChar, Color iconColor, int iconSize)
        {
            // Buat bitmap baru dengan ukuran ikon
            Bitmap iconBitmap = new(iconSize, iconSize);

            using (Graphics g = Graphics.FromImage(iconBitmap))
            {
                // Bersihkan background
                g.Clear(Color.Transparent);

                // Aktifkan anti-aliasing untuk rendering halus
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Buat font ikon
                using (Font iconFont = new("Font Awesome 6 Free", iconSize, FontStyle.Regular))
                {
                    // Konversi karakter ikon ke string
                    string iconString = char.ConvertFromUtf32((int)iconChar);

                    // Gambar ikon
                    using (SolidBrush brush = new(iconColor))
                    {
                        g.DrawString(iconString, iconFont, brush, 0, 0);
                    }
                }
            }

            return iconBitmap;
        }
        public void RoundedPanel(Panel panel, int radius = 20)
        {
            // Pastikan panel tidak null
            if (panel == null) return;

            GraphicsPath path = new();

            // Definisikan rectangle untuk drawing
            Rectangle bounds = new(0, 0, panel.Width, panel.Height);

            // Tambahkan arc untuk setiap sudut
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90); // Sudut kiri atas
            path.AddLine(radius, 0, panel.Width - radius, 0); // Garis atas
            path.AddArc(panel.Width - (radius * 2), 0, radius * 2, radius * 2, 270, 90); // Sudut kanan atas
            path.AddLine(panel.Width, radius, panel.Width, panel.Height - radius); // Garis kanan
            path.AddArc(panel.Width - (radius * 2), panel.Height - (radius * 2), radius * 2, radius * 2, 0, 90); // Sudut kanan bawah
            path.AddLine(panel.Width - radius, panel.Height, radius, panel.Height); // Garis bawah
            path.AddArc(0, panel.Height - (radius * 2), radius * 2, radius * 2, 90, 90); // Sudut kiri bawah
            path.AddLine(0, panel.Height - radius, 0, radius); // Garis kiri

            path.CloseFigure();

            // Set region menggunakan path
            panel.Region = new Region(path);

            // Tambahkan event handler untuk menjaga konsistensi saat resize
            panel.Resize += (sender, e) =>
            {
                GraphicsPath resizePath = new();

                // Ulang proses yang sama saat resize
                resizePath.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                resizePath.AddLine(radius, 0, panel.Width - radius, 0);
                resizePath.AddArc(panel.Width - (radius * 2), 0, radius * 2, radius * 2, 270, 90);
                resizePath.AddLine(panel.Width, radius, panel.Width, panel.Height - radius);
                resizePath.AddArc(panel.Width - (radius * 2), panel.Height - (radius * 2), radius * 2, radius * 2, 0, 90);
                resizePath.AddLine(panel.Width - radius, panel.Height, radius, panel.Height);
                resizePath.AddArc(0, panel.Height - (radius * 2), radius * 2, radius * 2, 90, 90);
                resizePath.AddLine(0, panel.Height - radius, 0, radius);

                resizePath.CloseFigure();
                panel.Region = new Region(resizePath);
            };
        }

        private void PayForm_KeluarButtonClicked(object sender, EventArgs e)
        {
            _ = loadDataAsync();
        }

        private async Task LoadDataDiscount()
        {
            try
            {
                string folderAddCartForm = "DT-Cache\\addCartForm";
                if (!Directory.Exists("DT-Cache")) { _ = Directory.CreateDirectory("DT-Cache"); }

                if (!Directory.Exists(folderAddCartForm)) { _ = Directory.CreateDirectory(folderAddCartForm); }

                // Load all menu data
                if (File.Exists($"{folderAddCartForm}\\LoadDataDiscountItem_Outlet_{baseOutlet}.data"))
                {
                    string json =
                        File.ReadAllText("DT-Cache" + "\\LoadDiscountPerCart_" + "Outlet_" + baseOutlet + ".data");
                    DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(json);
                    List<DataDiscountCart> data = menuModel.data;
                    List<DataDiscountCart> options = data;
                    dataDiscountListCart = data;
                    options.Insert(0, new DataDiscountCart { id = -1, code = "Tidak ada Diskon" });
                    cmbDiskon.DataSource = options;
                    cmbDiskon.DisplayMember = "code";
                    cmbDiskon.ValueMember = "id";
                }
                else
                {
                    NotifyHelper.Warning("Terjadi kesalahan Load Cache, Akan Syncronize ulang");
                    CacheDataApp form3 = new("Sync");
                    form3.Show();
                }
            }
            catch (TaskCanceledException ex)
            {
                NotifyHelper.Error($"Koneksi tidak stabil. Coba beberapa saat lagi.{ex.Message}");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal tampil data diskon " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async void txtCariMenu_TextChanged(object sender, EventArgs e)
        {
            if (dataGridView3.Visible)
            {
                if (txtCariMenu.Text != "" && txtCariMenu.Text != null)
                {
                    btnCari.Visible = true;
                }
                else
                {
                    //PerformSearch();
                    _ = LoadDataWithPagingAsync();

                    btnCari.Visible = false;
                }
            }

            if (dataGridView2.Visible)
            {
                PerformSearchList();
            }
        }

        private void PerformSearchList()
        {
            try
            {
                if (listDataTable == null)
                {
                    return;
                }

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

        private void InitializeNameToControlMap(FlowLayoutPanel panel)
        {
            nameToControlMap = new Dictionary<string, Control>();

            // Populate the nameToControlMap with the name and corresponding control for each control in the panel
            foreach (Control control in panel.Controls)
            {
                if (control is Panel && control.Controls.Count >= 2)
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
                    List<Menu> searchResults = allMenuItems.Where(menu =>
                        menu.name.ToLower().Contains(searchQuery) ||
                        menu.menu_type.ToLower().Contains(searchQuery) ||
                        string.Format("Rp. {0:n0},-", menu.price).ToLower().Contains(searchQuery)
                    ).ToList();

                    // Update the display with the search results
                    UpdateDisplayWithSearchResults(searchResults);
                }
            }

            catch (Exception ex)
            {
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
                    ClearCartFile();
                }
                else
                {
                    NotifyHelper.Warning("Keranjang kosong.");
                }
            }
            catch (Exception ex)
            {
                // Menangani error jika terjadi masalah saat penghapusan file
                NotifyHelper.Error("Terjadi kesalahan saat menghapus file: " + ex.Message);
            }
        }

        public async Task LoadDataListby()
        {
            try
            {
                items = 0;
                string json; // Deklarasikan json di luar blok if-else

                if (!File.Exists($"DT-Cache\\menu_outlet_id_{baseOutlet}.data"))
                {
                    json = await apiService.Get("/menu?outlet_id=" + baseOutlet);
                    GetMenuModel? apiMenuModel = JsonConvert.DeserializeObject<GetMenuModel>(json);

                    File.WriteAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data",
                        JsonConvert.SerializeObject(apiMenuModel));
                }
                else
                {
                    json = File.ReadAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data");
                }

                // Deserialize json setelah memastikan json memiliki nilai
                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(json);
                List<Menu> menuList = menuModel.data.ToList();

                dataGridView2.Controls.Clear();

                DataTable dataTable2 = new();

                _ = dataTable2.Columns.Add("MenuID", typeof(int));
                _ = dataTable2.Columns.Add("Nama Menu", typeof(string)); // Change this line
                _ = dataTable2.Columns.Add("Menu Type", typeof(string));
                _ = dataTable2.Columns.Add("Menu Price", typeof(string));

                foreach (Menu Menu in menuList)
                {
                    _ = Invoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = $"Downloading Data...[{items} / {menuModel.data.Count}]";
                    });
                    _ = dataTable2.Rows.Add(Menu.id, Menu.name, Menu.menu_type,
                        string.Format("Rp. {0:n0},-", Menu.price));
                    items++;
                }

                // Set the column header name
                dataTable2.Columns["Nama Menu"].ColumnName = "Nama Menu";

                dataGridView2.DataSource = dataTable2;
                listDataTable = dataTable2.Copy();

                dataGridView2.Columns["MenuID"].Visible = false;
                dataGridView2.Columns["Nama Menu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                _ = Invoke((MethodInvoker)delegate
                {
                    txtCariMenu.PlaceholderText = "Cari menu items...";
                    lblCountingItems.Text = $"{items} items";
                });
            }
            catch (TaskCanceledException ex)
            {
                NotifyHelper.Error("Koneksi tidak stabil. Coba beberapa saat lagi." + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("An error occurred while retrieving data: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void DataGridView2_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView2.Rows[e.RowIndex];
                string id = selectedRow.Cells["MenuID"].Value.ToString();
                DataGridViewRow selectedRow2 = dataGridView2.Rows[e.RowIndex];
                string nama = selectedRow2.Cells["Nama Menu"].Value.ToString();
                CartDetailClick(id, nama);
            }
        }

        private void CartDetailClick(string id, string nama)
        {

            using (Offline_addCartForm Offline_addCartForm = new(id, nama, selectedServingTypeallItems))
            {
                QuestionHelper bg = new(null, null, null, null);
                Form background = bg.CreateOverlayForm();

                Offline_addCartForm.Owner = background;

                background.Show();

                DialogResult result = Offline_addCartForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    background.Dispose();
                    _ = LoadCart();
                    // Settings were successfully updated, perform any necessary actions
                    selectedServingTypeallItems = Offline_addCartForm.selectedServingTypeall;
                }
                else
                {
                    NotifyHelper.Error("Gagal Menambahkan item, silahkan coba lagi");
                    ReloadCart();
                    _ = LoadCart();
                    background.Dispose();
                }
            }
        }


        public async Task loadDataAsync()
        {
            if (!Directory.Exists("DT-Cache")) { _ = Directory.CreateDirectory("DT-Cache"); }

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
                        _ = BeginInvoke((MethodInvoker)delegate
                        {
                            txtCariMenu.PlaceholderText = $"Loading Data...[{items} / {menuModel.data.Count}]";
                        });

                        // Use EndInvoke to release the delegate when the call is complete
                        _ = EndInvoke(BeginInvoke((MethodInvoker)delegate
                        {
                            txtCariMenu.PlaceholderText = $"Loading Data...[{items} / {menuModel.data.Count}]";
                        }));
                        Panel tileButton = new(); //Panel tileButton = new Panel();
                        tileButton.Width = 90;
                        tileButton.Height = 120;

                        Panel pictureBoxPanel = new();
                        pictureBoxPanel.Dock = DockStyle.Fill;


                        tileButton.Controls.Add(pictureBoxPanel);

                        PictureBox pictureBox = new();
                        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox.Dock = DockStyle.Fill;
                        pictureBox.BorderStyle = BorderStyle.None; // Add this line to set the border style

                        pictureBoxPanel.Controls.Add(pictureBox);
                        tileButton.Controls.Add(pictureBoxPanel); // Add the PictureBox to the Panel

                        Label nameLabel = new();
                        nameLabel.Text = menu.name;
                        nameLabel.ForeColor = Color.FromArgb(30, 31, 68);
                        nameLabel.Dock = DockStyle.Bottom;
                        nameLabel.TextAlign = ContentAlignment.MiddleCenter;
                        nameLabel.Font = new Font("Arial", 8, FontStyle.Bold);

                        tileButton.Controls.Add(nameLabel);

                        Label typeLabel = new();
                        typeLabel.Text = menu.menu_type;
                        typeLabel.Dock = DockStyle.Bottom;
                        typeLabel.TextAlign = ContentAlignment.MiddleCenter;
                        tileButton.Controls.Add(typeLabel);
                        //tileButton.Controls.Add(pictureBoxPanel); // Add the PictureBox to the Panel

                        Label priceLabel = new();
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
                            Panel spacerPanel = new();
                            spacerPanel.Dock = DockStyle.Top;
                            spacerPanel.Height = 110;
                            spacerPanel.Width = 10; // Set the height of the spacer panel
                            dataGridView3.Controls.Add(spacerPanel);
                        }

                        typeLabel.Visible = false;

                        pictureBox.Click += async (sender, e) =>
                        {
                            
                            // Create the addCartForm on the UI thread
                            using (Offline_addCartForm Offline_addCartForm = new(menu.id.ToString(),
                                       menu.name, selectedServingTypeallItems))
                            {
                                QuestionHelper bg = new(null, null, null, null);
                                Form background = bg.CreateOverlayForm();

                                Offline_addCartForm.Owner = background;

                                background.Show();

                                DialogResult result = Offline_addCartForm.ShowDialog();

                                if (result == DialogResult.OK)
                                {
                                    // Dispose of the background form now that the addCartForm form has been closed
                                    background.Dispose();
                                    _ = LoadCart();
                                    selectedServingTypeallItems = Offline_addCartForm.selectedServingTypeall;
                                }
                                else
                                {
                                    NotifyHelper.Error("Gagal Menambahkan item, silahkan coba lagi");
                                    ReloadCart();
                                    _ = LoadCart();
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
                        _ = BeginInvoke((MethodInvoker)delegate
                        {
                            lblCountingItems.Text = $"{items} items";
                        });

                        // Use EndInvoke to release the delegate when the call is complete
                        _ = EndInvoke(BeginInvoke((MethodInvoker)delegate
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
                    _ = BeginInvoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = "Cari Menu Items...";
                    });
                    // Use EndInvoke to release the delegate when the call is complete
                    _ = EndInvoke(BeginInvoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = "Cari Menu Items...";
                    }));
                    //LoadCart();

                    return;
                }
                catch (TaskCanceledException ex)
                {
                    NotifyHelper.Error("Koneksi tidak stabil. Coba beberapa saat lagi." + ex.Message);
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
                        NotifyHelper.Error("Data rusak, Silahkan Reset Cache di setting.");
                    }
                }
            }

            try
            {
                if (!_internetServices.IsInternetConnected())
                {
                    NotifyHelper.Error("No Connection Available.");
                    return;
                }


                string response = await apiService.Get("/menu?outlet_id=" + baseOutlet);

                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                List<Menu> menuList = menuModel.data.ToList();

                // Save the menu data to a local file
                File.WriteAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data",
                    JsonConvert.SerializeObject(menuModel));

                // Clear the existing controls and add new ones
                dataGridView3.SuspendLayout();
                dataGridView3.Controls.Clear();
                foreach (Menu menu in menuList)
                {
                    // ... same code as before to create and add controls ...
                    items += 1;
                    // Use BeginInvoke to marshal the call to the UI thread
                    _ = BeginInvoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = $"Loading Data...[{items} / {menuModel.data.Count}]";
                    });

                    // Use EndInvoke to release the delegate when the call is complete
                    _ = EndInvoke(BeginInvoke((MethodInvoker)delegate
                    {
                        txtCariMenu.PlaceholderText = $"Loading Data...[{items} / {menuModel.data.Count}]";
                    }));

                    Panel tileButton = new(); //Panel tileButton = new Panel();
                    tileButton.Width = 90;
                    tileButton.Height = 120;

                    Panel pictureBoxPanel = new();
                    pictureBoxPanel.Dock = DockStyle.Fill;


                    tileButton.Controls.Add(pictureBoxPanel);

                    //RoundedPanel(pictureBoxPanel);
                    //pictureBoxPanel.BorderStyle = BorderStyle.None; // Add this line to set the border style

                    PictureBox pictureBox = new();
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.Dock = DockStyle.Fill;
                    pictureBox.BorderStyle = BorderStyle.None; // Add this line to set the border style

                    pictureBoxPanel.Controls.Add(pictureBox);
                    tileButton.Controls.Add(pictureBoxPanel); // Add the PictureBox to the Panel

                    //AutoSizeLabel nameLabel = new AutoSizeLabel();
                    Label nameLabel = new();
                    nameLabel.Text = menu.name;
                    nameLabel.ForeColor = Color.FromArgb(30, 31, 68);
                    nameLabel.Dock = DockStyle.Bottom;
                    nameLabel.TextAlign = ContentAlignment.MiddleCenter;
                    nameLabel.Font = new Font("Arial", 8, FontStyle.Bold);
                    //nameLabel.BackColor = Color.White;

                    tileButton.Controls.Add(nameLabel);

                    Label typeLabel = new();
                    typeLabel.Text = menu.menu_type;
                    typeLabel.Dock = DockStyle.Bottom;
                    typeLabel.TextAlign = ContentAlignment.MiddleCenter;
                    tileButton.Controls.Add(typeLabel);
                    //tileButton.Controls.Add(pictureBoxPanel); // Add the PictureBox to the Panel

                    Label priceLabel = new();
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
                        Panel spacerPanel = new();
                        spacerPanel.Dock = DockStyle.Top;
                        spacerPanel.Height = 110;
                        spacerPanel.Width = 10; // Set the height of the spacer panel
                        dataGridView3.Controls.Add(spacerPanel);
                    }

                    typeLabel.Visible = false;

                    // Use the Invoke method to marshal the call back to the UI thread
                    // this.Invoke((MethodInvoker)delegate
                    //{
                    pictureBox.Click += async (sender, e) =>
                    {
                        // Create the addCartForm on the UI thread
                        using (Offline_addCartForm Offline_addCartForm = new(menu.id.ToString(),
                                   menu.name, selectedServingTypeallItems))
                        {
                            QuestionHelper bg = new(null, null, null, null);
                            Form background = bg.CreateOverlayForm();

                            Offline_addCartForm.Owner = background;

                            background.Show();

                            DialogResult result = Offline_addCartForm.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                // Dispose of the background form now that the addCartForm form has been closed
                                background.Dispose();
                                _ = LoadCart();
                                selectedServingTypeallItems = Offline_addCartForm.selectedServingTypeall;
                            }
                            else
                            {
                                NotifyHelper.Error("Gagal Menambahkan item, silahkan coba lagi");
                                ReloadCart();
                                _ = LoadCart();
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
                    _ = BeginInvoke((MethodInvoker)delegate
                    {
                        lblCountingItems.Text = $"{items} items";
                    });

                    // Use EndInvoke to release the delegate when the call is complete
                    _ = EndInvoke(BeginInvoke((MethodInvoker)delegate
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

                _ = BeginInvoke((MethodInvoker)delegate
                {
                    txtCariMenu.PlaceholderText = "Cari Menu Items...";
                });
                // Use EndInvoke to release the delegate when the call is complete
                _ = EndInvoke(BeginInvoke((MethodInvoker)delegate
                {
                    txtCariMenu.PlaceholderText = "Cari Menu Items...";
                }));

                //LoadCart();
            }
            catch (TaskCanceledException ex)
            {
                NotifyHelper.Error("Koneksi tidak stabil. Coba beberapa saat lagi." + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                // Resume the layout engine after adding all the controls
                dataGridView3.ResumeLayout();

                NotifyHelper.Error("An error occurred while retrieving data: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

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
                            Rectangle rect = new(0, 0, pictureBox.Width, pictureBox.Height);
                            graphics.FillRectangle(new SolidBrush(Color.White), rect);
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            // Create a GraphicsPath object for the rounded rectangle
                            GraphicsPath path = new();
                            path.AddArc(rect.X, rect.Y, rect.Width / 2, rect.Height / 2, 180, 90);
                            path.AddArc(rect.Right - (rect.Width / 2), rect.Y, rect.Width / 2, rect.Height / 2, 270,
                                90);
                            path.AddArc(rect.Right - (rect.Width / 2), rect.Bottom - (rect.Height / 2), rect.Width / 2,
                                rect.Height / 2, 0, 90);
                            path.AddArc(rect.X, rect.Bottom - (rect.Height / 2), rect.Width / 2, rect.Height / 2, 90,
                                90);
                            path.CloseFigure();

                            // Create a new Region object based on the GraphicsPath object
                            Region region = new(path);

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
                        _ = ResizeImage(downloadedImage, 70);
                        // Check if the image size is within the limits of the PictureBox
                        CheckImageSize(downloadedImage);
                        try
                        {
                            //hapus sampe sini jika msh error
                            menuImageDictionary[menu] = downloadedImage;

                            if (pictureBox.InvokeRequired)
                            {
                                _ = pictureBox.Invoke((MethodInvoker)delegate
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
            int deviden;
            int width, height;
            deviden = image.Height / size;
            height = image.Height * deviden;
            width = image.Width * deviden;

            Bitmap finalImage = new(width, height);

            using (Graphics graphics = Graphics.FromImage(finalImage))
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
                    using (FileStream stream = new(localImagePath, FileMode.Open, FileAccess.Read))
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
                _ = Directory.CreateDirectory(localDirectory);

                string localImagePath = Path.Combine(localDirectory, imageName + ".Jpeg"); //before jpg
                // Generate a random pastel color

                // Generate a random pastel color
                Random rand = new();
                int r = rand.Next(200, 255);
                int g = rand.Next(200, 255);
                int b = rand.Next(200, 255);
                Color pastelColor = Color.FromArgb(r, g, b);

                // Create a new bitmap with the pastel color background
                Bitmap bmp = new(image.Width, image.Height, PixelFormat.Format32bppArgb);
                using (Graphics graphic = Graphics.FromImage(bmp))
                {
                    graphic.Clear(pastelColor);
                    graphic.DrawImage(image, new Rectangle(new Point(), image.Size),
                        new Rectangle(new Point(), image.Size), GraphicsUnit.Pixel);
                }

                // Save the new image
                using (FileStream stream = new(localImagePath, FileMode.Create, FileAccess.Write))
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


                HttpClient httpClient = new();
                HttpResponseMessage response = await httpClient.GetAsync(imageUrl);
                _ = response.EnsureSuccessStatusCode();


                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    Image originalImage = Image.FromStream(stream);

                    // Generate a random pastel color
                    Random rand = new();
                    int r = rand.Next(200, 255);
                    int g = rand.Next(200, 255);
                    int b = rand.Next(200, 255);

                    Color pastelColor = Color.FromArgb(r, g, b);
                    // Create a new bitmap with the pastel color background
                    int width = Math.Min(70, originalImage.Width);
                    int height = Math.Min(70, originalImage.Height);
                    int left = (70 - width) / 2;
                    int top = (70 - height) / 2;
                    Bitmap bmp = new(70, 70, PixelFormat.Format32bppArgb);
                    using (Graphics graphic = Graphics.FromImage(bmp))
                    {
                        graphic.Clear(pastelColor);
                        graphic.DrawImage(originalImage, left, top, width, height);
                    }

                    return bmp;
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error downloading image: {ex.Message}");
                return LoadPlaceholderImage(70, 70);
                Exception ex;
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

                Console.WriteLine("Placeholder image not found at: " + placeholderImagePath);
            }
            catch (Exception ex)
            {
                // Log or handle the exception if necessary
                Console.WriteLine("Error loading placeholder image: " + ex.Message);
            }

            // If the image file is not found or an error occurs, use the generated placeholder logic
            Bitmap placeholder = new(width, height);
            using (Graphics graphics = Graphics.FromImage(placeholder))
            {
                // Generate a random pastel color
                Random Rand = new();
                int r = Rand.Next(128, 255); // Random red value between 128 and 255
                int g = Rand.Next(128, 255); // Random green value between 128 and 255
                int b = Rand.Next(128, 255); // Random blue value between 128 and 255
                Color pastelColor = Color.FromArgb(r, g, b);

                graphics.FillRectangle(new SolidBrush(pastelColor), 0, 0, width, height);

                // Calculate the center point of the image
                int x = (width - 70) / 2;
                int y = (height - 35) / 2;

                // Draw the placeholder text at the center point
                graphics.DrawString("Image\nTidak\nDiUpload", SystemFonts.DefaultFont,
                    new SolidBrush(Color.FromArgb(30, 31, 68)), new PointF(x, y));
            }

            return placeholder;
        }

        //reload dual monitor keranjang
        public async Task SignalReload()
        {
            if (!Directory.Exists("C:\\Temp"))
            {
                _ = Directory.CreateDirectory("C:\\Temp");
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
                    _ = Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                // Menyimpan JSON ke file
                using (StreamWriter writer = new(filePath))
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
            buttonPayment.Enabled = false;
            await LoadCartData();
            buttonPayment.Enabled = true;
        }

        public void ClearCartFile()
        {
            string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";

            // Mengecek apakah file ada
            if (File.Exists(cacheFilePath))
            {
                // Membuat objek JSON kosong yang mencerminkan format yang Anda gunakan
                JObject emptyCartData = new()
                {
                    ["cart_details"] = new JArray(), // Cart kosong
                    ["subtotal"] = 0,
                    ["total"] = 0,
                    ["transaction_ref"] = "",
                    ["discount_id"] = 0
                };

                // Menulis objek kosong ke dalam file, mengosongkan isi file
                File.WriteAllText(cacheFilePath, emptyCartData.ToString());
            }
            else
            {
                // Jika file belum ada, buat file baru dengan format kosong
                JObject emptyCartData = new()
                {
                    ["cart_details"] = new JArray(),
                    ["subtotal"] = 0,
                    ["total"] = 0,
                    ["transaction_ref"] = "",
                    ["discount_id"] = 0
                };

                // Menulis file kosong baru
                File.WriteAllText(cacheFilePath, emptyCartData.ToString());
            }
        }

        public async Task LoadCartDataSQL()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Ambil cart modal yang terbaru
                    var cart = await context.Carts.Include(c => c.CartDetails)
                                                    .Where(c => c.DeletedAt == null)
                                                   .OrderByDescending(c => c.Id)
                                                   .FirstOrDefaultAsync();

                    if (cart == null || cart.CartDetails.Count == 0)
                    {
                        lblDetailKeranjang.Text = "Keranjang: Kosong";
                        lblDiskon1.Text = "Rp. 0";
                        lblSubTotal1.Text = "Rp. 0,-";
                        lblTotal1.Text = "Rp. 0,-";
                        buttonPayment.Text = "Proses Pembayaran";
                        subTotalPrice = 0;
                        diskonID = 0;

                        iconButtonGet.Text = "Pakai";
                        iconButtonGet.ForeColor = Color.Black;
                        isDiscountActive = false;
                        iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);

                        selectedServingTypeallItems = 1;

                        ButtonSplit.Enabled = true;
                    }
                    else
                    {
                        // Jika cart ada, ambil informasi dari cart
                        //customer_name = cart.CustomerName ?? "??";
                        //customer_seat = cart.CustomerSeat ?? "??";
                        lblDetailKeranjang.Text = $"Keranjang: Nama : {customer_name} Seat : {customer_seat}";

                        diskonID = cart.DiscountId;

                        // Logika untuk mendeteksi diskon
                        if (diskonID != 0)
                        {
                            iconButtonGet.Text = "Hapus Disc";
                            iconButtonGet.ForeColor = Color.Red;
                            isDiscountActive = true;
                            iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);
                        }
                        else
                        {
                            iconButtonGet.Text = "Pakai";
                            iconButtonGet.ForeColor = Color.Black;
                            isDiscountActive = false;
                            iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);
                        }

                        int subtotal = cart.Subtotal;
                        int total = cart.Total;

                        lblDiskon1.Text = string.Format("Rp. {0:n0},-", total - subtotal);
                        lblSubTotal1.Text = string.Format("Rp. {0:n0},-", subtotal);
                        subTotalPrice = subtotal; // Simpan subtotal untuk digunakan nanti
                        lblTotal1.Text = string.Format("Rp. {0:n0},-", total);
                        buttonPayment.Text = string.Format("Bayar Rp. {0:n0},-", total);

                        // Membuat DataTable untuk menyimpan hasil
                        DataTable dataTable = new();
                        _ = dataTable.Columns.Add("MenuID", typeof(string));
                        _ = dataTable.Columns.Add("CartDetailID", typeof(string));
                        _ = dataTable.Columns.Add("Jenis", typeof(string));
                        _ = dataTable.Columns.Add("Menu", typeof(string));
                        _ = dataTable.Columns.Add("Total Harga", typeof(string));
                        _ = dataTable.Columns.Add("Note", typeof(string));

                        // Mengelompokkan item cart berdasarkan serving type
                        var menuGroups = cart.CartDetails
                                             .Where(item => item.Qty > 0) // Hanya ambil item dengan Qty lebih dari 0
                                             .GroupBy(item => item.ServingTypeName)
                                             .ToList();

                        foreach (var group in menuGroups)
                        {
                            // Tambahkan baris pemisah untuk setiap grup serving type
                            string servingTypeName = group.Key;
                            AddSeparatorRow(dataTable, servingTypeName, dataGridView1); // Fungsi untuk menambahkan baris pemisah

                            // Menambahkan item ke dalam group
                            foreach (var item in group)
                            {
                                string menuName = item.MenuName;
                                int quantity = item.Qty;
                                decimal price = item.Price;
                                int totalPrice = item.TotalPrice;
                                string? noteItem = item.NoteItem;
                                string discountedPrice = item.DiscountedPrice?.ToString() ?? "0"; // Mengambil discounted price jika ada
                                int discountValue = item.DiscountsValue ?? 0; // Mengambil nilai diskon
                                bool hasDiscount = item.DiscountedPrice.HasValue && item.DiscountedPrice > 0; // Mengecek apakah ada diskon

                                // Menambahkan baris data untuk setiap item
                                _ = dataTable.Rows.Add(
                                    item.MenuId.ToString(),
                                    item.CartDetailId.ToString(),
                                    servingTypeName,
                                    $"{quantity}X {menuName}",
                                    string.Format("Rp. {0:n0},-", totalPrice),
                                    noteItem
                                );

                                // Menambahkan catatan jika ada
                                if (!string.IsNullOrEmpty(noteItem))
                                {
                                    if (hasDiscount)
                                    {
                                        _ = dataTable.Rows.Add(
                                            null,
                                            null,
                                            null,
                                            $"  *catatan: {noteItem}",
                                            "  *diskon diterapkan",
                                            null
                                        );
                                    }
                                    else
                                    {
                                        _ = dataTable.Rows.Add(
                                            null,
                                            null,
                                            null,
                                            $"  *catatan: {noteItem}",
                                            null,
                                            null
                                        );
                                    }
                                }

                                // Menambahkan informasi diskon jika ada
                                if (hasDiscount)
                                {
                                    _ = dataTable.Rows.Add(
                                        null,
                                        null,
                                        null,
                                        null,
                                        $"  *diskon: Rp. {discountValue} (dari Rp. {price})",
                                        null
                                    );
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
                        DataGridViewCellStyle boldStyle = new();
                        boldStyle.Font = new Font(dataGridView1.Font, FontStyle.Italic);
                        dataGridView1.Columns["Menu"].DefaultCellStyle = boldStyle;
                        dataGridView1.Columns["Menu"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                        dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                        dataGridView1.Columns["Menu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                        await LoadDataDiscount();

                        if (diskonID != 0)
                        {
                            for (int i = 0; i < cmbDiskon.Items.Count; i++)
                            {
                                DataDiscountCart? discount = cmbDiskon.Items[i] as DataDiscountCart;

                                if (discount != null && discount.id == diskonID)
                                {
                                    // Set index diskon yang sesuai
                                    cmbDiskon.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                NotifyHelper.Error("Koneksi tidak stabil. Coba beberapa saat lagi." + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        public async Task LoadCartData()
        {
            try
            {
                string cacheFilePath = "DT-Cache\\Transaction\\Cart.data";
                string cacheFilePathSplit = "DT-Cache\\Transaction\\Cart_main_split.data";

                if (File.Exists(cacheFilePath))
                {
                    string cartJson = File.ReadAllText(cacheFilePath);
                    JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

                    // Initialize default values in case no data is available
                    if (cartData["cart_details"] == null || cartData["cart_details"].Count() == 0)
                    {
                        ClearCartFile();
                        cardDetailID = "##";
                        lblDetailKeranjang.Text = "Keranjang: Kosong";
                        lblDiskon1.Text = "Rp. 0";
                        lblSubTotal1.Text = "Rp. 0,-";
                        lblTotal1.Text = "Rp. 0,-";
                        buttonPayment.Text = "Proses Pembayaran";
                        subTotalPrice = 0;
                        diskonID = 0;

                        //Pajak Checker
                        if (PajakHelper.TryGetPajak(out string pajakText))
                        {
                            Pajak.Visible = true;
                            lblPajak.Text = "Rp. 0,-";
                            lblPajak.Visible = true;
                        }
                        else
                        {
                            Pajak.Visible = false;
                            lblPajak.Visible = false;
                        }

                        //set tombol disc
                        iconButtonGet.Text = "Pakai";
                        iconButtonGet.ForeColor = Color.Black;
                        isDiscountActive = false;
                        iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);

                        //reset servingtype
                        selectedServingTypeallItems = 1;

                        //split main check
                        if (File.Exists(cacheFilePathSplit))
                        {
                            if (File.Exists(cacheFilePath))
                            {
                                File.Delete(cacheFilePath);
                            }

                            File.Move(cacheFilePathSplit, cacheFilePath);

                            _ = LoadCartData();
                            selectedServingTypeallItems = 1;
                            return;
                        }
                    }
                    else
                    {
                        JArray? cartDetails = cartData["cart_details"] as JArray;
                        if (!string.IsNullOrEmpty(cartData["is_splitted"]?.ToString()) &&
                            cartData["is_splitted"]?.ToString() == "1")
                        {
                            ButtonSplit.Enabled = false;
                        }
                        else
                        {
                            ButtonSplit.Enabled = true;
                        }

                        JToken? cartDetail = cartDetails.FirstOrDefault();
                        cartID = cartDetail?["cart_detail_id"].ToString() ??
                                 "null";
                        cardDetailID = cartID;

                        diskonID = 0;
                        int discountId = cartData["discount_id"] != null ? (int)cartData["discount_id"] : -1;
                        if (discountId == -1)
                        {
                            discountId = 0;
                            cartData["discount_id"] = discountId;
                            File.WriteAllText(cacheFilePath, cartData.ToString());
                        }

                        if (cartData["discount_id"] != null && int.Parse(cartData["discount_id"].ToString()) != 0)
                        {
                            iconButtonGet.Text = "Hapus Disc";
                            //iconButtonGet.ForeColor = Color.White;
                            iconButtonGet.ForeColor = Color.DarkRed;
                            isDiscountActive = true;
                            iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);
                        }
                        else
                        {
                            iconButtonGet.Text = "Pakai";
                            iconButtonGet.ForeColor = Color.Black;
                            iconButtonGet.BackColor = Color.White;
                            isDiscountActive = false;
                            iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);
                        }

                        customer_name = null;
                        customer_seat = null;
                        // Set customer information
                        customer_name = cartData["customer_name"]?.ToString() ?? null;
                        customer_seat = cartData["customer_seat"]?.ToString() ?? null;
                        if (!string.IsNullOrEmpty(customer_name) && !string.IsNullOrEmpty(customer_seat))
                        {
                            lblDetailKeranjang.Text = $"Keranjang: Nama : {customer_name} Seat : {customer_seat}";
                        }
                        else
                        {
                            lblDetailKeranjang.Text = "Keranjang: Nama : ?? Seat : ??";
                        }

                        int subtotal = cartData["subtotal"] != null ? int.Parse(cartData["subtotal"].ToString()) : 0;

                        int total = cartData["total"] != null ? int.Parse(cartData["total"].ToString()) : 0;
                        lblDiskon1.Text = string.Format("Rp. {0:n0},-", total - subtotal);
                        lblSubTotal1.Text = string.Format("Rp. {0:n0},-", subtotal);
                        subTotalPrice = subtotal;
                        lblTotal1.Text = string.Format("Rp. {0:n0},-", total);
                        buttonPayment.Text = string.Format("Bayar Rp. {0:n0},-", total);

                        //=========================Pajak Checker=============================\\
                        if (PajakHelper.TryGetPajak(out string pajakText))
                        {
                            int pajak = int.Parse(pajakText);
                            Pajak.Visible = true;
                            lblPajak.Text = string.Format("Rp. {0:n0},-", total * pajak / 100);
                            lblPajak.Visible = true;
                            lblTotal1.Text = string.Format("Rp. {0:n0},-", total * (pajak+100) / 100);
                            buttonPayment.Text = string.Format("Bayar Rp. {0:n0},-", total * (pajak + 100) / 100);
                        }
                        else
                        {
                            Pajak.Visible = false;
                            lblPajak.Visible = false;
                        }
                        //=======================End Pajak Checker============================\\


                        DataTable dataTable = new();
                        _ = dataTable.Columns.Add("MenuID", typeof(string));
                        _ = dataTable.Columns.Add("CartDetailID", typeof(string));
                        _ = dataTable.Columns.Add("Jenis", typeof(string));
                        _ = dataTable.Columns.Add("Menu", typeof(string));
                        _ = dataTable.Columns.Add("Total Harga", typeof(string));
                        _ = dataTable.Columns.Add("Note", typeof(string));

                        // Group cart items by serving type
                        List<IGrouping<string, JToken>> menuGroups =
                            cartDetails.GroupBy(x => x["serving_type_name"].ToString()).ToList();

                        foreach (IGrouping<string, JToken> group in menuGroups)
                        {
                            // Add a separator row for each serving type group
                            AddSeparatorRow(dataTable, group.Key, dataGridView1);

                            foreach (JToken menu in group)
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
                                _ = dataTable.Rows.Add(
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
                                        _ = dataTable.Rows.Add(
                                            null,
                                            null,
                                            null,
                                            "  *catatan : " + noteItem,
                                            "  *discounted ",
                                            null);
                                    }
                                    else
                                    {
                                        _ = dataTable.Rows.Add(
                                            null,
                                            null,
                                            null,
                                            "  *catatan : " + noteItem,
                                            null,
                                            null);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(discountedPrice) && discountedPrice != "0")
                                    {
                                        _ = dataTable.Rows.Add(
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
                        if (cartData["canceled_items"] is JArray cardCanceled && cardCanceled.Count > 0)
                        {
                            // Group cart items by serving type
                            List<IGrouping<string, JToken>> menuGroupsCancel =
                                cardCanceled.GroupBy(x => x["serving_type_name"].ToString()).ToList();
                            AddSeparatorRow(dataTable, "Canceled", dataGridView1);

                            foreach (IGrouping<string, JToken> group in menuGroupsCancel)
                            {
                                foreach (JToken menu in group)
                                {
                                    string menuName = menu["menu_name"].ToString();
                                    string menuType = menu["menu_type"].ToString();
                                    string? discountedPrice = menu["discounted_price"].ToString();
                                    string servingTypeName = menu["serving_type_name"].ToString();
                                    int quantity = menu["qty"] != null ? (int)menu["qty"] : 0;
                                    decimal price = menu["price"] != null ? decimal.Parse(menu["price"].ToString()) : 0;
                                    string cancelReason = menu["cancel_reason"]?.ToString() ?? "";
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
                                    _ = dataTable.Rows.Add(
                                        menu["menu_id"],
                                        menu["cart_detail_id"],
                                        servingTypeName,
                                        $"{quantity}X {menuName} {menu["menu_detail_name"]} {ordered}",
                                        string.Format("Rp. {0:n0},-", totalPrice),
                                        cancelReason
                                    );
                                    if (!string.IsNullOrEmpty(cancelReason))
                                    {
                                        if (!string.IsNullOrEmpty(discountedPrice) && discountedPrice != "0")
                                        {
                                            _ = dataTable.Rows.Add(
                                                null,
                                                null,
                                                null,
                                                "  *CancelReason : " + cancelReason,
                                                null,
                                                null);
                                        }
                                        else
                                        {
                                            _ = dataTable.Rows.Add(
                                                null,
                                                null,
                                                null,
                                                "  *Cancel Reason : " + cancelReason,
                                                null,
                                                null);
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(discountedPrice) && discountedPrice != "0")
                                        {
                                            _ = dataTable.Rows.Add(
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
                        }


                        // Bind the data to the DataGridView
                        dataGridView1.DataSource = dataTable;

                        // Hide unnecessary columns
                        dataGridView1.Columns["MenuID"].Visible = false;
                        dataGridView1.Columns["CartDetailID"].Visible = false;
                        dataGridView1.Columns["Jenis"].Visible = false;
                        dataGridView1.Columns["Note"].Visible = false;

                        // Apply formatting to the DataGridView
                        DataGridViewCellStyle boldStyle = new();
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
                                DataDiscountCart? discount = cmbDiskon.Items[i] as DataDiscountCart;

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


                    cardDetailID = "##";
                    // If file does not exist, set defaults
                    lblDetailKeranjang.Text = "Keranjang: Kosong";
                    lblDiskon1.Text = "Rp. 0";
                    lblSubTotal1.Text = "Rp. 0,-";
                    lblTotal1.Text = "Rp. 0,-";
                    buttonPayment.Text = "Proses Pembayaran";
                    subTotalPrice = 0;
                    ButtonSplit.Enabled = true;

                    //set tombol disc
                    iconButtonGet.Text = "Pakai";
                    iconButtonGet.ForeColor = Color.Black;
                    isDiscountActive = false;
                    iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold);

                    //reset servingtype
                    selectedServingTypeallItems = 1;
                    //Pajak Checker
                    if (PajakHelper.TryGetPajak(out string pajakText))
                    {
                        Pajak.Visible = true;
                        lblPajak.Text = "Rp. 0,-";
                        lblPajak.Visible = true;
                    }
                    else
                    {
                        Pajak.Visible = false;
                        lblPajak.Visible = false;
                    }
                }

                ReloadDisc();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("An error occurred: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void AddSeparatorRow(DataTable dataTable, string groupKey, DataGridView dataGridView)
        {
            // Add separator row to DataTable
            _ = dataTable.Rows.Add(null, null, null, groupKey + "s\n", null,
                null); // Add a separator row with groupKey in column 4

            // Get the last row index just added
            int lastRowIndex = dataTable.Rows.Count - 1;

            // Add the row to DataGridView
            dataGridView.DataSource = dataTable;

            // Apply styles to specific columns for the separator row
            int[] cellIndexesToStyle = { 3, 4 }; // Columns 4 and 5 for styling
            SetCellStyle(dataGridView.Rows[lastRowIndex], cellIndexesToStyle, Color.WhiteSmoke, FontStyle.Bold);

            // Simulate hiding the row by setting its height to 0
            dataGridView.Rows[lastRowIndex].Height = 0;
        }

        private void SetCellStyle(DataGridViewRow row, int[] cellIndexes, Color backgroundColor, FontStyle fontStyle)
        {
            foreach (int index in cellIndexes)
            {
                row.Cells[index].Style.BackColor = backgroundColor;
                row.Cells[index].Style.Font = new Font(dataGridView1.Font, fontStyle);
            }
        }

        private void ReloadData()
        {
            _ = loadDataAsync();
        }

        public void ReloadData2()
        {
            _ = loadDataAsync();
        }

        public async void ReloadCart()
        {
            cartID = null;
            buttonPayment.Text = "Proses Pembayaran";
            lblDiskon1.Text = null;
            lblSubTotal1.Text = null;
            lblTotal1.Text = null;
            dataGridView1.DataSource = null;
            _ = LoadCart();
        }

        public void SignalReloadPayform()
        {
            if (!Directory.Exists("C:\\Temp"))
            {
                _ = Directory.CreateDirectory("C:\\Temp");
            }

            File.WriteAllText("C:\\Temp\\payment_signal.txt", "Payment");
        }

        public void SignalReloadPayformDone()
        {
            if (!Directory.Exists("C:\\Temp"))
            {
                _ = Directory.CreateDirectory("C:\\Temp");
            }

            File.WriteAllText("C:\\Temp\\payment_signal.txt", "PaymentDone");
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
                    }
                    else
                    {
                        return;
                    }

                    if (selectedRow.Cells["CartDetailID"].Value != DBNull.Value)
                    {
                        cartdetailid = selectedRow.Cells["CartDetailID"].Value.ToString();
                    }
                    else
                    {
                        return;
                    }
                    _ = Invoke((MethodInvoker)delegate
                    {
                        using (Offline_updateCartForm Offline_updateCartForm = new(id, cartdetailid))
                        {
                            QuestionHelper bg = new(null, null, null, null);
                            Form background = bg.CreateOverlayForm();

                            Offline_updateCartForm.Owner = background;

                            background.Show();

                            DialogResult result = Offline_updateCartForm.ShowDialog();

                            // Handle the result if needed
                            if (result == DialogResult.OK)
                            {
                                NotifyHelper.Success($"Keranjang ID: #{cardDetailID}. berhasil diupdate.");

                                background.Dispose();
                                ReloadCart();
                            }
                            background.Dispose();

                        }
                    });
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
        }

        private void SimpanBill_Click(object sender, EventArgs e)
        {
            int rowCount = dataGridView1.RowCount;
            if (rowCount == 0)
            {
                NotifyHelper.Warning("Keranjang masih kosong!");

                return;
            }

            using (Offline_saveBill saveBill = new(cartID, customer_name, customer_seat))
            {

                QuestionHelper bg = new(null, null, null, null);
                Form background = bg.CreateOverlayForm();

                saveBill.Owner = background;

                background.Show();

                DialogResult result = saveBill.ShowDialog();

                // Handle the result if needed
                if (result == DialogResult.OK)
                {
                    cmbDiskon.SelectedIndex = 0;
                    NotifyHelper.Success($"Keranjang ID: #{cardDetailID}. berhasil disimpan.");
                    background.Dispose();
                    ReloadCart();
                }
                else
                {
                    NotifyHelper.Error("Gagal Simpan, Silahkan coba lagi");
                    background.Dispose();
                    ReloadCart();
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
            if (string.IsNullOrEmpty(cartID) || dataDiscountListCart == null || !dataDiscountListCart.Any())
            {
                NotifyHelper.Warning("Keranjang kosong. Silakan tambahkan item ke keranjang sebelum menerapkan diskon.");
                return;
            }

            if (isDiscountActive)
            {
                // Mengambil data cart dari file cache
                string cartDataPath = "DT-Cache\\Transaction\\Cart.data";
                string cartDataJson = File.ReadAllText(cartDataPath);
                JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);
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
                    NotifyHelper.Info("Diskon belum dipilih !");
                    return;
                }

                int diskonMinimum = dataDiscountListCart.FirstOrDefault(d => d.id == selectedDiskon)?.min_purchase ??
                                    -1;
                if (diskonMinimum > subTotalPrice)
                {
                    int resultDiskon = diskonMinimum - subTotalPrice;
                    NotifyHelper.Info("Minimum diskon kurang " + string.Format("Rp. {0:n0},-", resultDiskon) + " lagi");
                    return;
                }
            }

            try
            {
                if (await cekPeritemDiskon() != 0 && selectedDiskon != 0)
                {
                    _ = LoadCart();
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
                NotifyHelper.Error("Gagal menggunakan diskon: " + ex.Message);
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
                JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);

                // Menentukan subtotal dan quantity item dalam cart
                JArray? cartDetailsArray = (JArray)cartData["cart_details"];
                int quantity = cartDetailsArray.Sum(item => (int)item["qty"]);
                int subtotal_item = cartDetailsArray.Sum(item => (int)item["price"] * (int)item["qty"]);

                int total_item_withDiscount = 0;
                int discountPercent = 0;
                int discounted_peritemPrice = 0;
                int discountValue = 0;
                int discountedPrice = 0;
                int discountMax = 0;
                int tempTotal = 0;

                string discountCode = null;

                if (selectedDiskon != 0) // Jika diskon yang dipilih bukan 0, proses diskon
                {
                    // Mendapatkan informasi diskon berdasarkan id diskon yang dipilih
                    DataDiscountCart? discount = dataDiscountListCart.FirstOrDefault(d => d.id == selectedDiskon);
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
                NotifyHelper.Success("DIscount dipakai");
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal menggunakan diskon: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

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
                    JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartJson);

                    // Retrieve cart details
                    JArray? cartDetails = cartData["cart_details"] as JArray;
                    foreach (JToken itema in cartDetails)
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
                NotifyHelper.Error("Gagal cek diskon: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

            lanjutan = 0;
            if (lanjutan == 1)
            {
                return 1;
            }

            return 0;
        }

        private async void listBill_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    NotifyHelper.Info("Keranjang belum kosong. tidak dapat membuka keranjang lagi");

                    return;
                }


                using (Offline_listBill Offline_listBill = new())
                {
                    QuestionHelper bg = new(null, null, null, null);
                    Form background = bg.CreateOverlayForm();

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
                NotifyHelper.Error(ex.ToString());
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
                EnumerableRowCollection<DataRow> filteredData = from row in listDataTable.AsEnumerable()
                                                                where row.Field<string>("Menu Type") == selectedFilter
                                                                select row;

                DataTable filteredTable = filteredData.CopyToDataTable();
                dataGridView2.DataSource = filteredTable;
                items = dataGridView2.RowCount;
                lblCountingItems.Text = items + " items";
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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

        private void button3_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(button3_Click));


            using (Offline_dataDiskon Offline_dataDiskon = new())
            {
                QuestionHelper bg = new(null, null, null, null);
                Form background = bg.CreateOverlayForm();

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
                JObject? cartData = JsonConvert.DeserializeObject<JObject>(cartDataJson);

                // Menentukan subtotal dan quantity item dalam cart
                JArray? cartDetailsArray = (JArray)cartData["cart_details"];
                int quantity = cartDetailsArray.Sum(item => (int)item["qty"]);
                int subtotal_item = int.Parse(cartData["subtotal"].ToString());

                int total_item_withDiscount = 0;
                int discountPercent = 0;
                int discountValue = 0;
                int discountedPrice = 0;
                int discountMax = 0;

                if (selectedDiskon != 0) // Jika diskon yang dipilih bukan 0, proses diskon
                {
                    DataDiscountCart? discount = dataDiscountListCart.FirstOrDefault(d => d.id == selectedDiskon);
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
                cartData["discount_code"] = cmbDiskon.SelectedText;
                cartData["discounts_value"] = discountValue;
                cartData["discounts_is_percent"] = discountPercent;
                cartData["discounted_price"] = discountedPrice;
                cartData["total"] = total_item_withDiscount;

                File.WriteAllText(cartDataPath, cartData.ToString());

                _ = LoadCart();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Terjadi kesalahan: " + ex.Message);
            }
        }


        private void ButtonSplit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null || dataGridView1.Rows.Count == 0)
            {
                NotifyHelper.Warning("Keranjang masih kosong!");
                return;
            }


            using (Offline_splitBill splitBill = new(cartID))
            {

                QuestionHelper bg = new(null, null, null, null);
                Form background = bg.CreateOverlayForm();

                splitBill.Owner = background;

                background.Show();
                DialogResult result = splitBill.ShowDialog();
                background.Dispose();
                ReloadCart();
            }
        }

        private async void lblDetailKeranjang_Click(object sender, EventArgs e)
        {
            ReloadCart();
            _ = LoadCart();
        }

        private void txtCariMenuList_TextChanged(object sender, EventArgs e)
        {
            PerformSearchList();
        }

        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                NotifyHelper.Error("Demo ListView/1jam off. Contact PM");
            }
            catch (Exception)
            {
            }
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
            try
            {
                // Path for the cart data cache file
                string filePath = "DT-Cache\\Transaction\\Cart.data";

                // Cek apakah file Cart.data ada
                if (File.Exists(filePath))
                {
                    // Read the cart data
                    string cartContent = File.ReadAllText(filePath);

                    // Parse the JSON using your existing model
                    var cartData = JsonConvert.DeserializeObject<DataCart>(cartContent);

                    // Check if cart is effectively empty
                    if (cartData.cart_details == null ||
                        !cartData.cart_details.Any() ||
                        cartData.total <= 0)
                    {
                        NotifyHelper.Warning("Keranjang masih kosong!");
                        return;
                    }

                    // Use values from parsed cart data
                    using (Offline_payForm Offline_payForm = new(
                        baseOutlet,
                        cartData.cart_id.ToString(),
                        cartData.total.ToString(),
                        lblTotal1.Text,
                        cartData.customer_seat ?? "",
                        cartData.customer_name ?? "",
                        this))
                    {
                        SignalReloadPayform();

                        QuestionHelper bg = new(null, null, null, null);
                        Form background = bg.CreateOverlayForm();

                        Offline_payForm.Owner = background;
                        background.Show();
                        DialogResult result = Offline_payForm.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            cmbDiskon.SelectedIndex = 0;
                            background.Dispose();
                            NotifyHelper.Success($"Keranjang ID: #{cardDetailID}. berhasil diproses.");

                            ReloadCart();
                            _ = LoadCart();
                        }
                        else
                        {
                            NotifyHelper.Error("Gagal Simpan, Silahkan coba lagi");
                            background.Dispose();
                            ReloadCart();
                            _ = LoadCart();
                        }
                    }
                }
                else
                {
                    NotifyHelper.Warning("Keranjang masih kosong!");
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void lblDeleteCart_Click(object sender, EventArgs e)
        {
            try
            {
                // Path for the cart data cache file
                string filePath = "DT-Cache\\Transaction\\Cart.data";

                // Cek apakah file Cart.data ada
                if (File.Exists(filePath))
                {
                    // Safely set selected index only if items exist
                    if (cmbDiskon.Items.Count > 0)
                    {
                        try
                        {
                            cmbDiskon.SelectedIndex = 0;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // Log or handle the specific case where setting index fails
                            LoggerUtil.LogWarning("Unable to set discount combo box index to 0");
                        }
                    }

                    // Membaca isi file Cart.data
                    string cartJson = File.ReadAllText(filePath);

                    // Deserialize data file Cart.data using your existing model
                    var cartData = JsonConvert.DeserializeObject<DataCart>(cartJson);

                    //---------------- Question Begin -----------------\\
                    string titleQuest = "Hapus ?";
                    string msgQuest = "Hapus Keranjang ?";
                    string cancelQuest = "Batal";
                    string okQuest = "Hapus";

                    QuestionHelper c = new(titleQuest, msgQuest, cancelQuest, okQuest);
                    Form background = c.CreateOverlayForm();

                    c.Owner = background;

                    background.Show();

                    DialogResult dialogResult = c.ShowDialog();
                    if (dialogResult == DialogResult.Cancel)
                    {
                        background.Dispose();
                        return;
                    }

                    background.Dispose();

                    //--------------- Question Result -------------------\\

                    // Comprehensive cart emptiness check
                    if (cartData.cart_details == null ||
                        !cartData.cart_details.Any() ||
                        cartData.total <= 0)
                    {
                        ClearCartFile();
                        NotifyHelper.Warning("Keranjang kosong.");
                        return;
                    }

                    // Cek apakah ada item yang sudah dipesan (is_ordered == 1)
                    bool isAnyOrdered = cartData.cart_details.Any(item =>
                        item.is_ordered == "1");

                    if (!isAnyOrdered)
                    {
                        // Jika tidak ada item yang dipesan, langsung clear
                        ClearCartFile();
                        NotifyHelper.Success($"Keranjang ID: #{cardDetailID}. berhasil dihapus.");

                        ReloadCart();
                        return;
                    }

                    using (Offline_deleteForm Offline_deleteForm = new(cartData.cart_id.ToString()))
                    {
                        background = c.CreateOverlayForm();

                        Offline_deleteForm.Owner = background;
                        background.Show();

                        DialogResult result = Offline_deleteForm.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            background.Dispose();
                            NotifyHelper.Success($"Keranjang ID: #{cardDetailID}. berhasil dihapus.");

                            ReloadCart();
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            background.Dispose();
                            NotifyHelper.Info($"Keranjang ID: #{cardDetailID}. proses hapus dicancel.");

                            ReloadCart();
                        }
                        else
                        {
                            NotifyHelper.Error("Gagal hapus keranjang, Silahkan coba lagi");
                            ReloadCart();
                            background.Dispose();
                        }
                    }
                }
                else
                {
                    NotifyHelper.Warning("Keranjang kosong.");
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

        private async void btnCategoryAll_Click(object sender, EventArgs e)
        {
            items = 0;

            if (configViewDataProductGrid)
            {
                string searchQuery = "";
                try
                {
                    if (allMenuItems != null)
                    {
                        // Filter the allMenuItems based on the search query
                        List<Menu> searchResults = allMenuItems.Where(menu =>
                            menu.name.ToLower().Contains(searchQuery) ||
                            menu.menu_type.ToLower().Contains(searchQuery) ||
                            string.Format("Rp. {0:n0},-", menu.price).ToLower().Contains(searchQuery)
                        ).ToList();

                        // Update the display with the search results
                        UpdateDisplayWithSearchResults(searchResults);
                    }
                }

                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
            else
            {
                await LoadDataListby();
            }
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (configViewDataProductGrid)
                {
                    string searchQuery = "makanan";

                    if (allMenuItems != null)
                    {
                        // Filter the allMenuItems based on the search query
                        List<Menu> searchResults = allMenuItems.Where(menu =>
                            menu.menu_type.ToLower().Contains(searchQuery)
                        ).ToList();

                        // Update the display with the search results
                        UpdateDisplayWithSearchResults(searchResults);
                    }
                }

                else
                {
                    items = 0;
                    if (listDataTable == null)
                    {
                        return;
                    }

                    string searchTerm = "makanan";

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
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void btnCategoryMin_Click(object sender, EventArgs e)
        {
            try
            {
                if (configViewDataProductGrid)
                {
                    string searchQuery = "minuman";

                    if (allMenuItems != null)
                    {
                        // Filter the allMenuItems based on the search query
                        List<Menu> searchResults = allMenuItems.Where(menu =>
                            menu.menu_type.ToLower().Contains(searchQuery)
                        ).ToList();


                        // Update the display with the search results
                        UpdateDisplayWithSearchResults(searchResults);
                    }
                    else
                    {
                        NotifyHelper.Info("allMenuItems is null");
                    }
                }
                else
                {
                    items = 0;
                    if (listDataTable == null)
                    {
                        return;
                    }

                    string searchTerm = "minuman";

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
            }
            catch (Exception ex)
            {
                NotifyHelper.Info($"Error Occurred: {ex.Message}");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async void btnGridView_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.Enabled)
                {
                    dataGridView2.Controls.Clear();
                }
                else
                {
                    dataGridView3.Controls.Clear();
                }
                string configDirectory = "setting";
                string configFilePath = Path.Combine(configDirectory, "configListMenu.data");

                string data = "OFF";
                await File.WriteAllTextAsync(configFilePath, data); // Asynchronously write the initial state
                configViewDataProductGrid = true;

                await LoadConfig();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async void btnListView_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.Enabled)
                {
                    dataGridView2.Controls.Clear();
                }
                else
                {
                    dataGridView3.Controls.Clear();
                }
                string configDirectory = "setting";
                string configFilePath = Path.Combine(configDirectory, "configListMenu.data");

                string data = "ON";
                await File.WriteAllTextAsync(configFilePath, data); // Asynchronously write the initial state
                configViewDataProductGrid = false;
                await LoadConfig();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async void btnReload_Click(object sender, EventArgs e)
        {
            try
            {
                string TypeCacheEksekusi = "Sync";
                using (CacheDataApp form3 = new(TypeCacheEksekusi))
                {
                    // ShowDialog akan "block" sampai form ditutup
                    form3.ShowDialog();
                }

                // ✅ Baru jalan setelah form3 ditutup
                await LoadConfig();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error($"Gagal Download Data : {ex}");
            }
        }

    }
}