using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using FontAwesome.Sharp;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.OffineMode;
using KASIR.OfflineMode;
using KASIR.Properties;
using KASIR.Services;
using KASIR.Helper;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Common;
using Color = System.Drawing.Color;
using DrawingColor = System.Drawing.Color;
using Path = System.IO.Path;
using Timer = System.Windows.Forms.Timer;

namespace KASIR
{
    public partial class Form1 : Form
    {
        private IconButton currentBtn;
        private Panel leftBorderBtn;
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet.ToString();
        private readonly OutletService _outletService;
        private string DownloadPath, VersionUpdaterApp, PathKasir, baseOutletName;
        private static Random random = new();
        private IInternetService _internetServices;
        public Form1()
        {
            InitializeComponent();
            _internetServices = new InternetService();

            // Inisialisasi dengan dependency injection atau manual
            _outletService = new OutletService(
                Settings.Default.BaseOutlet,
                new InternetService(),
                new ApiService(),
                LoggerService.Instance._log
            );
            SetDoubleBufferedForAllControls(this);
            Load += btnMaximize_Click;
            Text = string.Empty;
            ControlBox = false;
            DoubleBuffered = true;
            MaximizedBounds = Screen.FromHandle(Handle).WorkingArea;
            leftBorderBtn = new Panel();
            leftBorderBtn.Size = new Size(7, 60);
            panel2.Controls.Add(leftBorderBtn);
            Height += 100;
            LogoKasir.Visible = true;
            StarterApp();
            if (baseOutlet != "4")
            {
                btnDev.Visible = false;
            }
            Shown += Form1_Shown;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            RefreshIconButtons();
        }

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
                if (prop != null)
                {
                    prop.SetValue(c, true, null);
                }

                SetDoubleBufferedForAllControls(c);
            }
        }

        private async Task EnsureOfflineModeConfig()
        {
            string configPath = "setting\\OfflineMode.data";
            string directoryPath = Path.GetDirectoryName(configPath);

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create or overwrite the config file
            if (!File.Exists(configPath))
            {
                await File.WriteAllTextAsync(configPath, "ON");
            }
        }

        private async void ConfigOfflineMode()
        {
            await EnsureOfflineModeConfig();

            string outletName = await _outletService.GetOutletNameAsync();
            string allSettingsData = await File.ReadAllTextAsync("setting\\OfflineMode.data");

            if (allSettingsData == "ON")
            {
                await LoadOfflineMasterPos();
                UpdateOutletLabel(outletName + " (Hybrid)");
            }
            else
            {
                await LoadMasterPos();
                UpdateOutletLabel(outletName + " (Online)");
            }
        }

        private async Task LoadOfflineMasterPos()
        {
            Offline_masterPos offlineMasterPos = new() { TopLevel = false, Dock = DockStyle.Fill };

            panel1.Controls.Add(offlineMasterPos);
            offlineMasterPos.BringToFront();
            offlineMasterPos.Show();
            await offlineMasterPos.LoadCart();
        }

        private async Task LoadMasterPos()
        {
        }

        private void RemoveExistingForm<T>() where T : Form
        {
            foreach (Control c in panel1.Controls)
            {
                if (c is T)
                {
                    c.Dispose();
                }
            }
        }

        public async void UpdateContent()
        {
            ConfigOfflineMode();
        }


        public void initPingTest()
        {
            if (isOpenNavbar)
            {
                SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
                SignalPing.Text = "Ping Test";
            }
            else
            {
                SignalPing.TextImageRelation = TextImageRelation.ImageAboveText;
                SignalPing.Text = "\n\nPing\nTest";
            }
            SignalPing.ForeColor = DrawingColor.White;
            lblPing.ForeColor = DrawingColor.White;
            SignalPing.IconColor = DrawingColor.White;
        }

        public async Task OpenDualMonitor()
        {
            try
            {
                string data = "ON";
                await File.WriteAllTextAsync("setting\\configDualMonitor.data", data);

                string path = Application.StartupPath + "KASIRDualMonitor\\KASIR Dual Monitor.exe";
                //OpenApplicationOnSecondMonitor(System.Windows.Forms.Application.StartupPath + @"KASIRDualMonitor\\KASIR Dual Monitor.exe");

                Process p = new();
                p.StartInfo.FileName = path;
                p.StartInfo.Arguments = "";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.Verb = "runas";
                p.Start();

                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async void StarterApp()
        {
            /*SettingsForm c = new(this);
            await c.LoadConfig();*/
            await headerOutletName("");
            initPingTest();
            ConfigOfflineMode();
            // Mengecek apakah sButtonOffline dalam status checked
            string Config = "setting\\OfflineMode.data";
            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(Config);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Memeriksa apakah file ada
            if (!File.Exists(Config))
            {
                // Membuat file dan menulis "OFF" ke dalamnya jika file tidak ada
                File.WriteAllText(Config, "ON");
            }

            string allSettingsData = File.ReadAllText(Config); // Ambil status offline

            await Task.Run(async () =>
            {
                await DualMonitorChecker();
                await cekLastUpdaterApp();
                await cekVersionAndData();
            });
            LoadingAllSetting(allSettingsData);

            if (await _internetServices.IsInternetConnectedAsync())
            {
                string TypeCacheEksekusi = "Sync";

                CacheDataApp cacheDataApp = new(TypeCacheEksekusi);

                // Minimkan dan tampilkan di system tray
                cacheDataApp.Load += (sender, e) =>
                {
                    cacheDataApp.WindowState = FormWindowState.Minimized;

                    // Tambahkan notifikasi sistem (opsional)
                    ShowSystemTrayNotification("Sinkronisasi dimulai", "Proses sinkronisasi data sedang berjalan");
                };

                cacheDataApp.Show();
            }
            await headerName();
        }
        // Metode bantuan untuk notifikasi
        private void ShowSystemTrayNotification(string title, string message)
        {
            NotifyIcon notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = SystemIcons.Information, // Ganti dengan icon kustom jika perlu
                Text = title
            };

            notifyIcon.ShowBalloonTip(5000, title, message, ToolTipIcon.Info);

            // Atur untuk menghilang setelah beberapa saat
            Timer timer = new Timer();
            timer.Interval = 5000; // 5 detik
            timer.Tick += (sender, e) =>
            {
                notifyIcon.Dispose();
                ((Timer)sender).Stop();
            };
            timer.Start();
        }
        private async void LoadingAllSetting(string allSettingsData)
        {
            await sendDataSyncPerHours(allSettingsData);
            if (allSettingsData == "ON")
            {
                // Create an instance of transactionFileMover and then call the method
                transactionFileMover fileMover = new();
                await fileMover.refreshCacheTransaction();
            }
        }

        private async Task DualMonitorChecker()
        {
            try
            {
                if (!File.Exists("setting\\configDualMonitor.data"))
                {
                    string data = "OFF";
                    await File.WriteAllTextAsync("setting\\configDualMonitor.data", data);
                }
                else
                {
                    string allSettingsData = await File.ReadAllTextAsync("setting\\configDualMonitor.data");
                    if (allSettingsData == "ON")
                    {
                        await OpenDualMonitor();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        public async Task headerOutletName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return; // Avoid null or empty
            }

            if (lblNamaOutlet.InvokeRequired)
            {
                await Task.Factory.FromAsync(
                    lblNamaOutlet.BeginInvoke(new Action(() => lblNamaOutlet.Text = "Please Wait.. " + text), null),
                    ar => lblNamaOutlet.EndInvoke(ar)
                );
            }
            else
            {
                lblNamaOutlet.Text = "Please Wait.. " + text;
            }
        }

        private async Task cekLastUpdaterApp()
        {
            try
            {
                if (!_internetServices.IsInternetConnected())
                {
                    return;
                }

                await headerOutletName("Check Last Update..");
                findingdownloadpath();
                string startupPath = AppDomain.CurrentDomain.BaseDirectory;
                PathKasir = startupPath;
                string directoryPath = "update";
                string filePath = Path.Combine(directoryPath, "versionUpdater.txt");

                if (Directory.Exists(directoryPath) && File.Exists(filePath))
                {
                    VersionUpdaterApp = File.ReadAllText(filePath);
                    await headerOutletName($"Version Updater {VersionUpdaterApp.ToString()}");
                }
                else
                {
                    // Jika folder tidak ada, buat foldernya
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Tulis konten default ke file
                    string contentToWrite = "1.0.0.1";
                    File.WriteAllText(filePath, contentToWrite);
                    VersionUpdaterApp = File.ReadAllText(filePath);
                    await headerOutletName($"Version Updater {VersionUpdaterApp.ToString()}");
                }

                await DownloadUpdaterApp();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Error" + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async Task DownloadUpdaterApp()
        {
            try
            {
                await headerOutletName("Checking New Updater...");

                using (HttpClient httpClient = new())
                {
                    string urlVersion = "https://raw.githubusercontent.com/bayufrd/update/main/updaterVersionApp.txt";
                    string newVersion = await httpClient.GetStringAsync(urlVersion);
                    if (string.IsNullOrWhiteSpace(newVersion))
                    {
                        throw new Exception("Version data is empty.");
                    }

                    string changeVersion = newVersion.Trim();
                    await headerOutletName($"New Version Updater is {changeVersion}");

                    // Version comparison
                    newVersion = newVersion.Replace(".", "");
                    VersionUpdaterApp = VersionUpdaterApp.Replace(".", "");
                    if (Convert.ToInt32(newVersion) > Convert.ToInt32(VersionUpdaterApp))
                    {
                        await headerOutletName("Downloading New Updater...");
                        string fileUrl = "https://raw.githubusercontent.com/bayufrd/update/main/Dastrevas.rar";
                        string destinationPath =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                $"{DownloadPath}\\Dastrevas.rar");

                        await DownloadFileAsync(httpClient, fileUrl, destinationPath);
                        await ExtractAndUpdateAsync(destinationPath, changeVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyHelper.Error($"Error: {ex.Message}");
                LoggerUtil.LogError(ex, "An error occurred during updating: {ErrorMessage}", ex.Message);
            }
        }

        private async Task DownloadFileAsync(HttpClient httpClient, string fileUrl, string destinationPath)
        {
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

            // Download file
            byte[] fileData = await httpClient.GetByteArrayAsync(fileUrl);
            await File.WriteAllBytesAsync(destinationPath, fileData);
        }

        private async Task ExtractAndUpdateAsync(string rarFilePath, string changeVersion)
        {
            try
            {
                await headerOutletName("Extracting New Updater...");

                string extractDirectory = $"{PathKasir}\\update";

                // Ensure the extraction directory exists
                Directory.CreateDirectory(extractDirectory);

                // Extract RAR file
                using (IArchive archive = ArchiveFactory.Open(rarFilePath))
                {
                    foreach (IArchiveEntry entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entry.WriteToDirectory(extractDirectory,
                            new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                    }
                }

                // Update version file
                string versionFilePath = $"{PathKasir}\\update\\versionUpdater.txt";
                await File.WriteAllTextAsync(versionFilePath, changeVersion);

                // Small delay and open updater
                await Task.Delay(3000);
                OpenUpdaterExe();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error($"Extraction Error: {ex.Message}");
                LoggerUtil.LogError(ex, "Extraction failed: {ErrorMessage}", ex.Message);
            }
        }

        private async void findingdownloadpath()
        {
            try
            {
                string cariFolder1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads");
                string cariFolder2 = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                string userName = Environment.UserName;
                string cariFolder3 = Path.Combine("C:\\Users", userName, "Downloads");


                if (Directory.Exists(cariFolder1))
                {
                    DownloadPath = cariFolder1;
                }
                else if (Directory.Exists(cariFolder2))
                {
                    DownloadPath = cariFolder2;
                }
                else if (Directory.Exists(cariFolder3))
                {
                    DownloadPath = cariFolder3;
                }
                else
                {
                    throw new DirectoryNotFoundException("Download directory not found.");
                }

                string filePath = Path.Combine(DownloadPath, "Dastrevas.rar");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                LoggerUtil.LogError(ex, "Unauthorized access: {ErrorMessage}", ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                LoggerUtil.LogError(ex, "Directory not found: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Error" + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        private static bool IsValidVersion(string version)
        {
            // Memastikan hanya angka dan maksimal 5 digit
            return Regex.IsMatch(version, @"^\d{1,5}$");
        }

        private async Task ConfirmUpdate(string newVersion, string currentVersion)
        {
            try
            {
                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);

                var json = new
                {
                    outlet_name = dataOutlet.data.name,
                    version = currentVersion + " UpdaterVer: " + VersionUpdaterApp,
                    new_version = newVersion,
                    last_updated = GetCurrentTimeInIndonesianFormat()
                };
                // Mengubah objek menjadi string JSON
                string jsonString =
                    JsonConvert.SerializeObject(json, Formatting.Indented); // Tidak ada indentasi
                IApiService apiService = new ApiService();
                HttpResponseMessage response =
                    await apiService.notifikasiPengeluaran(jsonString, $"/update-confirm?outlet_id={baseOutlet}");
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private string GetCurrentTimeInIndonesianFormat()
        {
            TimeZoneInfo indonesiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime localTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, indonesiaTimeZone);
            return localTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private async Task cekVersionAndData()
        {
            try
            {
                await headerOutletName("Checking Kasir Version...");
                if (!_internetServices.IsInternetConnected())
                {
                    return;
                }

                using (HttpClient httpClient = new())
                {
                    string urlVersion = Properties.Settings.Default.BaseAddressVersion.ToString();
                    string newVersion = await httpClient.GetStringAsync(urlVersion);
                    string currentVersion = Properties.Settings.Default.Version.ToString();

                    await headerOutletName($"Current Version is {currentVersion}, New is {newVersion}");
                    await ConfirmUpdate(newVersion.ToString(), currentVersion);

                    newVersion = newVersion.Replace(".", "");
                    currentVersion = currentVersion.Replace(".", "");

                    // Validasi input
                    if (!IsValidVersion(currentVersion))
                    {
                        currentVersion = "1080"; // Set default version jika tidak valid
                    }

                    bool shouldUpdate = false;
                    if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
                    {
                        // Ambil data focus outlet
                        string originalUrl = Properties.Settings.Default.BaseAddressDev.ToString();
                        string urlOutletFocus = RemoveApiPrefix(originalUrl);
                        string focusOutletData =
                            await httpClient.GetStringAsync($"{urlOutletFocus}/update/outletUpdate.txt");

                        // Parsing data focus outlet
                        string[] focusOutlets = focusOutletData.Trim(new char[] { ' ', '\n', '\r' })
                            .Split(',')
                            .Select(s => s.Trim()) // Menghapus spasi di sekitar
                            .ToArray();

                        // Cek apakah baseOutlet ada dalam focusOutlets
                        if (focusOutlets.Contains(baseOutlet) || focusOutlets.Contains("0"))
                        {
                            shouldUpdate = true;
                            await headerOutletName("Opening Kasir Updater...");
                        }

                        if (shouldUpdate)
                        {
                            LoggerUtil.LogNetwork("Open Updater");
                            await OpenUpdaterExe();

                            return; // Keluar dari method setelah membuka updater
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private string RemoveApiPrefix(string url)
        {
            // Memeriksa apakah URL mengandung "api."
            if (url.Contains("api."))
            {
                // Mengganti "api." dengan string kosong
                return url.Replace("api.", "");
            }

            return url; // Kembalikan URL asli jika tidak ada "api."
        }

        private async Task OpenUpdaterExe()
        {
            try
            {
                string updatePath = Path.Combine(
                    Application.StartupPath,
                    "update",
                    "update.exe"
                );

                if (!File.Exists(updatePath))
                {
                    LoggerUtil.LogWarning($"Update file not found: {updatePath}");
                    return;
                }

                if (!_internetServices.IsInternetConnected())
                {
                    LoggerUtil.LogWarning("No internet connection");
                    return;
                }

                ProcessStartInfo startInfo = new()
                {
                    FileName = updatePath,
                    Arguments = "",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);

                await Task.Delay(1000);

                Application.Exit();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to open updater");
                NotifyHelper.Error($"Error opening updater: {ex.Message}");
            }
        }
        public async Task headerName()
        {
            try
            {
                string outletName = await _outletService.GetOutletNameAsync();
                UpdateOutletLabel(outletName + " (Hybrid)");
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
        public void UpdateOutletLabel(string outletName)
        {
            if (lblNamaOutlet.InvokeRequired)
            {
                lblNamaOutlet.Invoke(new MethodInvoker(() => lblNamaOutlet.Text = outletName));
            }
            else
            {
                lblNamaOutlet.Text = outletName;
            }

            baseOutletName = lblNamaOutlet.Text;
        }


        private struct RGBColors
        {
            public static Color color1 = Color.FromArgb(172, 126, 241);
            public static Color color2 = Color.FromArgb(249, 118, 176);
            public static Color color3 = Color.FromArgb(253, 138, 114);
            public static Color color4 = Color.FromArgb(95, 77, 221);
            public static Color color5 = Color.FromArgb(249, 88, 155);
            public static Color color6 = Color.FromArgb(24, 161, 251);
        }

        private static Color PickRandomColor()
        {
            Color[] colors =
            {
                RGBColors.color1, RGBColors.color2, RGBColors.color3, RGBColors.color4, RGBColors.color5,
                RGBColors.color6
            };

            int index = random.Next(colors.Length);
            return colors[index];
        }

        //Methods
        private void ActivateButton(object senderBtn, Color color)
        {
            if (senderBtn != null)
            {
                DisableButton();
                //Button
                currentBtn = (IconButton)senderBtn;
                currentBtn.BackColor = Color.WhiteSmoke;
                currentBtn.ForeColor = Color.Black;
                currentBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
                currentBtn.TextAlign = ContentAlignment.MiddleCenter;
                currentBtn.IconColor = color;
                currentBtn.TextImageRelation = TextImageRelation.TextBeforeImage;
                currentBtn.ImageAlign = ContentAlignment.MiddleRight;
                //Left border button
                leftBorderBtn.BackColor = color;
                leftBorderBtn.Location = new Point(0, currentBtn.Location.Y);
                leftBorderBtn.Visible = true;
                leftBorderBtn.BringToFront();
                //Current Child Form Icon
                iconCurrentChildForm.IconChar = currentBtn.IconChar;
                iconCurrentChildForm.IconColor = color;
            }
        }

        private void DisableButton()
        {
            if (currentBtn != null)
            {
                currentBtn.BackColor = Color.Transparent;
                currentBtn.ForeColor = Color.Gainsboro;
                currentBtn.TextAlign = ContentAlignment.MiddleLeft;
                currentBtn.IconColor = Color.Gainsboro;
                currentBtn.TextImageRelation = TextImageRelation.ImageAboveText;
                currentBtn.ImageAlign = ContentAlignment.MiddleLeft;
            }
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            Color randomColor = PickRandomColor(); // Pick a random color for button
            ActivateButton(sender, randomColor); // Activate the button

            try
            {
                btnShiftLaporan.Enabled = false;
                MenuBtn.Enabled = false;
                TransBtn.Enabled = false;
                // If OfflineMode is ON, load the Offline_masterPos form
                Offline_masterPos offlineMasterPos = new();
                offlineMasterPos.TopLevel = false;
                offlineMasterPos.Dock = DockStyle.Fill;


                // Add the Offline_masterPos form to the panel
                panel1.Controls.Add(offlineMasterPos);
                offlineMasterPos.BringToFront();
                offlineMasterPos.Show();
                await offlineMasterPos.LoadCart();

                btnShiftLaporan.Enabled = true;
                MenuBtn.Enabled = true;
                TransBtn.Enabled = true;
                lblTitleChildForm.Text = "Menu - Offline Mode Transaksi"; // Update label for Offline Mode
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                NotifyHelper.Error("Terjadi kesalahan: " + ex.Message);
            }
        }

        private void link_Click(object sender, EventArgs e)
        {
            try
            {
                NavBarBtn_Click(this, EventArgs.Empty); // Dengan parameter lengkap
                // Specify the URL to open
                //string url = "http://cms.gaspollmanagementcenter.com";

                // Use the default browser to open the URL
                //Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                NotifyHelper.Error(ex.Message);
            }
        }

        //button shiftreport / shift report end shift
        private async void buttonHistoryTransaction(object sender, EventArgs e)
        {
            Color randomColor = PickRandomColor();
            ActivateButton(sender, randomColor);

            try
            {
                // Read the OfflineMode status
                string config = "setting\\OfflineMode.data";
                string allSettingsData = File.ReadAllText(config); // Get the current OfflineMode setting

                btnShiftLaporan.Enabled = false;
                MenuBtn.Enabled = false;
                TransBtn.Enabled = false;
                Offline_successTransaction c = new();
                if (c == null)
                {
                    NotifyHelper.Warning($"Terjadi kesalah coba restart aplikasi");
                    return;
                }

                c.Dock = DockStyle.Fill;
                panel1.Controls.Add(c);
                c.BringToFront();
                c.Show();
                await c.LoadData();

                btnShiftLaporan.Enabled = true;
                MenuBtn.Enabled = true;
                TransBtn.Enabled = true;
                lblTitleChildForm.Text = "Transactions - History Transactions";
            }
            catch (NullReferenceException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                NotifyHelper.Error("Terjadi kesalahan: " + ex.Message);
            }
        }

        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }

        //Close-Maximize-Minimize
        private async void btnExit_Click(object sender, EventArgs e)
        {
            //string data = "OFF";
            //await File.WriteAllTextAsync("setting\\configDualMonitor.data", data);

            Application.Exit();
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        //Remove transparent border in maximized state
        private void FormMainMenu_Resize(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(FormMainMenu_Resize));

            if (WindowState == FormWindowState.Maximized)
            {
                FormBorderStyle = FormBorderStyle.None;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
            }
        }

        private void btnEditSettings_Click(object sender, EventArgs e)
        {
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            // Disable timer while processing to prevent multiple overlapping executions
            SyncTimer.Enabled = false;

            try
            {
                if (isOpenNavbar)
                {
                    SignalPing.Width = 103;
                    SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
                }
                else
                {
                    SignalPing.Width = 50;
                    SignalPing.TextImageRelation = TextImageRelation.ImageAboveText;
                }
                // Read the OfflineMode status
                string config = "setting\\OfflineMode.data";
                string allSettingsData = File.ReadAllText(config); // Get the current OfflineMode setting

                // Check if OfflineMode is ON
                if (allSettingsData != "ON")
                {
                    return;
                }

                if (!_internetServices.IsInternetConnected())
                {
                    SignalPing.ForeColor = Color.Red;
                    SignalPing.Text = $"Error Sync \n{DateTime.Now:HH:mm}";
                    SignalPing.IconColor = Color.White;
                    return;
                }

                if (TransactionSync.IsSyncing) // Check using the shared manager
                {
                    return; // If sync is already running, exit
                }

                try
                {
                    UpdateSyncStatus(Color.LightGreen, "Sync...");

                    await sendDataSyncPerHours(allSettingsData);

                    if (isOpenNavbar)
                    {
                        UpdateSyncStatus(Color.White, $"Last Sync \n{DateTime.Now:HH:mm}");
                    }
                    else
                    {
                        UpdateSyncStatus(Color.White, $"Last\nSync \n{DateTime.Now:HH:mm}");
                    }

                    await Task.Run(async () =>
                    {
                        await cekVersionAndData();
                        await headerName();
                    });
                }
                catch (Exception ex)
                {
                    if (isOpenNavbar)
                    {
                        SignalPing.Width = 103;
                        SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
                    }
                    else
                    {
                        SignalPing.Width = 50;
                        SignalPing.TextImageRelation = TextImageRelation.ImageAboveText;
                    }
                    SignalPing.ForeColor = Color.Red;
                    SignalPing.Text = $"Error Sync \n{DateTime.Now:HH:mm}";
                    SignalPing.IconColor = Color.White;
                    LoggerUtil.LogError(ex, "An error occurred during SyncSuccess: {ErrorMessage}", ex.Message);
                }
            }
            finally
            {
                // Re-enable timer after processing is complete
                SyncTimer.Enabled = true;
            }
        }

        public async Task sendDataSyncPerHours(string allSettingsData)
        {
            // Check if OfflineMode is ON
            if (allSettingsData == "ON")
            {
                using (shiftReport c = new())
                {
                    // Set background operation flag to true 
                    // karena tidak menampilkan form
                    c.IsBackgroundOperation = true;

                    // Sinkronisasi transaksi
                    await c.SyncDataTransactions();
                }
            }
        }

        private void UpdateSyncStatus(Color color, string text)
        {
            if (isOpenNavbar)
            {
                SignalPing.Width = 103;
                SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
            else
            {
                SignalPing.Width = 50;
                SignalPing.TextImageRelation = TextImageRelation.ImageAboveText;
            }
            lblPing.ForeColor = color;
            lblPing.Text = text;
            SignalPing.ForeColor = color;
            SignalPing.Text = text;
            SignalPing.IconColor = color;
        }

        public static readonly object FileLock = new();

        private async void btnTestSpeed_Click(object sender, EventArgs e)
        {
            if (isOpenNavbar)
            {
                SignalPing.Width = 103;
                SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
            else
            {
                SignalPing.Width = 50;
                SignalPing.TextImageRelation = TextImageRelation.ImageAboveText;
            }

            SignalPing.Text = "Testing...";
            SignalPing.ForeColor = Color.White; // Default warna saat testing

            try
            {
                int ping = await TestPing("8.8.8.8"); // Ping ke Google DNS
                UpdatePingColor(ping); // Perbarui warna berdasarkan nilai ping

                SignalPing.Text = $"{ping} ms";
                await Task.Delay(2000);
                if (isOpenNavbar)
                {
                    UpdatePingLabel(Color.White, "Test Ping");
                }
                else
                {
                    UpdatePingLabel(Color.White, "");
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                UpdatePingLabel(Color.Red, $"Error: {ex.Message}");
            }
        }

        private async Task<int> TestPing(string host)
        {
            Ping pingSender = new();
            PingReply reply = await pingSender.SendPingAsync(host);

            if (reply.Status == IPStatus.Success)
            {
                return (int)reply.RoundtripTime; // Waktu ping dalam ms
            }
            else
            {
                throw new Exception("Ping failed.");
            }
        }

        private void UpdatePingLabel(Color color, string text)
        {
            if (isOpenNavbar)
            {
                SignalPing.Width = 103;
                SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
            else
            {
                SignalPing.Width = 50;
                SignalPing.TextImageRelation = TextImageRelation.ImageAboveText;
            }
            lblPing.ForeColor = color;
            lblPing.Text = text;
            SignalPing.ForeColor = color;
            SignalPing.Text = text;
            SignalPing.IconColor = color;
        }

        private void UpdatePingColor(int ping)
        {
            if (isOpenNavbar)
            {
                SignalPing.Width = 103;
                SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
            else
            {
                SignalPing.Width = 50;
                SignalPing.TextImageRelation = TextImageRelation.ImageAboveText;
            }
            if (ping < 50) // Jika ping di bawah 50 ms, warna hijau
            {
                UpdatePingLabel(Color.LimeGreen, $"{ping} ms - Excellent");
            }
            else if (ping >= 50 && ping <= 100) // Jika ping antara 50 dan 100 ms, warna kuning
            {
                UpdatePingLabel(Color.Orange, $"{ping} ms - Good");
            }
            else // Jika ping di atas 100 ms, warna merah
            {
                UpdatePingLabel(Color.Red, $"{ping} ms - Poor");
            }
        }

        public static void OpenApplicationOnSecondMonitor(string path)
        {
            // Get the screen dimensions
            Screen[] screens = Screen.AllScreens;
            Rectangle secondScreen = screens[1].Bounds;

            // Open the application with the specified path and position
            ProcessStartInfo startInfo = new(path);
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;
            startInfo.Arguments = $"{secondScreen.X} {secondScreen.Y} {secondScreen.Width} {secondScreen.Height}";
            Process.Start(startInfo);
        }

        public async void RefreshMenu()
        {
            if (button6 != null)
            {
                button6.PerformClick();
            }
        }

        private async void btnShiftLaporan_Click(object sender, EventArgs e)
        {
            SyncTimer.Enabled = false;
            ActivateButton(sender, RGBColors.color4);

            try
            {
                string allSettingsData = await File.ReadAllTextAsync("setting\\OfflineMode.data");
                if (allSettingsData == "ON")
                {
                    await LoadOfflineSuccessTransaction();
                }
                else
                {
                    await LoadOnlineSuccessTransaction();
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                NotifyHelper.Error("Terjadi kesalahan: " + ex.Message);
            }
            finally
            {
                SyncTimer.Enabled = true;
            }
        }

        private async Task LoadOfflineSuccessTransaction()
        {
            Offline_shiftReport offlineTransaction = new();
            offlineTransaction.Dock = DockStyle.Fill;

            panel1.Controls.Add(offlineTransaction);
            offlineTransaction.BringToFront();
            await offlineTransaction.LoadData();
            lblTitleChildForm.Text = "Shift Report (Offline)";
        }

        private async Task LoadOnlineSuccessTransaction()
        {
            shiftReport onlineTransaction = new();
            onlineTransaction.Dock = DockStyle.Fill;

            panel1.Controls.Add(onlineTransaction);
            onlineTransaction.BringToFront();
            await onlineTransaction.LoadData();
            lblTitleChildForm.Text = "Shift Report (Online)";
        }

        private Form CreateOverlayForm()
        {
            return new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = Location,
                ShowInTaskbar = false
            };
        }

        private void btnContact_Click(object sender, EventArgs e)
        {
            Form background = CreateOverlayForm();

            using (Offline_Complaint c = new())
            {
                c.Owner = background;

                background.Show();

                DialogResult dialogResult = c.ShowDialog();

                background.Dispose();
            }
        }

        private void btnDev_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color4);

            DevMonitor c = new();

            c.Dock = DockStyle.Fill;
            panel1.Controls.Add(c);
            c.BringToFront();
            c.Show();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color4);

            Offline_settingsForm c = new();

            c.Dock = DockStyle.Fill;
            panel1.Controls.Add(c);
            c.BringToFront();
            c.Show();
        }

        private void BtnSettingForm_Click(object sender, EventArgs e)
        {

            ActivateButton(sender, RGBColors.color4);

            Offline_settingsForm c = new();

            c.Dock = DockStyle.Fill;
            panel1.Controls.Add(c);
            c.BringToFront();
            c.Show();
        }

        private bool isOpenNavbar = true; // Tambahkan variabel status navbar

        private void NavBarBtn_Click(object sender, EventArgs e)
        {
            // Toggle status navbar
            isOpenNavbar = !isOpenNavbar;

            if (isOpenNavbar)
            {
                // Mode navbar terbuka
                gradientPanel2.Width = 103;
                LogoKasir.Width = 103;
                panel2.Width = 103;
                SignalPing.Width = 103;
                lblPing.Width = 103;

                LogoKasir.Image = Resources.a_2_1;
                LogoKasir.Height = 103;

                MenuBtn.Text = "Menu";
                TransBtn.Text = "Transaction";
                btnShiftLaporan.Text = "Shift Report";
                BtnSettingForm.Text = "Settings";
                btnContact.Text = "Contact Us";
                btnDev.Text = "Developer";
                lblPing.Text = "Ping";
                SignalPing.Text = "Test Ping";
            }
            else
            {
                // Mode navbar tertutup
                gradientPanel2.Width = 50;
                LogoKasir.Width = 50;
                panel2.Width = 50;

                LogoKasir.Image = Resources.logoblack;
                LogoKasir.Height = 50;

                SignalPing.Width = 50;
                lblPing.Width = 50;

                MenuBtn.Text = "";
                TransBtn.Text = "";
                btnShiftLaporan.Text = "";
                BtnSettingForm.Text = "";
                btnContact.Text = "";
                btnDev.Text = "";
                lblPing.Text = "";
                SignalPing.Text = "";
                SignalPing.ImageAlign = ContentAlignment.MiddleLeft;
            }
        }
    }
}