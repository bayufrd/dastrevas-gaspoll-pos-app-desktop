using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using FontAwesome.Sharp;
using KASIR.komponen;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.OffineMode;
using KASIR.OfflineMode;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;
using Color = System.Drawing.Color;
using DrawingColor = System.Drawing.Color;
using Path = System.IO.Path;


namespace KASIR
{
    public partial class Form1 : Form
    {
        private IconButton currentBtn;
        private Panel leftBorderBtn;
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet.ToString();

        string DownloadPath, VersionUpdaterApp, PathKasir, baseOutletName;
        private static Random random = new Random();

        public Form1()
        {
            InitializeComponent();
            SetDoubleBufferedForAllControls(this);
            //this.Height = 768;
            //this.Width = 1024;
            this.Load += btnMaximize_Click;
            this.Text = string.Empty;
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            leftBorderBtn = new Panel();
            leftBorderBtn.Size = new Size(7, 60);
            panel2.Controls.Add(leftBorderBtn);
            this.Height += 100;
            button2.Visible = true;
            StarterApp();
            if (baseOutlet != "4")
            {
                btnDev.Visible = false;
            }
            this.Shown += Form1_Shown; // Tambahkan ini
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            RefreshIconButtons();
        }
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

            string outletName = await GetOutletNameAsync();
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
            Offline_masterPos offlineMasterPos = new Offline_masterPos
            {
                TopLevel = false,
                Dock = DockStyle.Fill
            };

            RemoveExistingForm<masterPos>();
            panel1.Controls.Add(offlineMasterPos);
            offlineMasterPos.BringToFront();
            offlineMasterPos.Show();
            await offlineMasterPos.LoadCart();
        }

        private async Task LoadMasterPos()
        {
            masterPos m = new masterPos
            {
                TopLevel = false,
                Dock = DockStyle.Fill
            };

            RemoveExistingForm<Offline_masterPos>();
            panel1.Controls.Add(m);
            m.BringToFront();
            m.Show();
            await m.LoadCart();
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
            SignalPing.Text = "Ping Test";
            SignalPing.ForeColor = DrawingColor.White;
            lblPing.ForeColor = DrawingColor.White;
            SignalPing.IconColor = DrawingColor.White;
        }

        public static void DuplicateTemp()
        {
            if (!File.Exists("icon\\OutletLogo.bmp"))
            {
                return;
            }
            string outletLogoPath = "icon\\OutletLogo.bmp";
            string tempCopyPath = "icon\\TempCopy.bmp";

            try
            {
                if (!File.Exists(tempCopyPath))
                {
                    File.Copy(outletLogoPath, tempCopyPath);
                }
                else
                {
                    File.Delete(outletLogoPath);
                    File.Copy(tempCopyPath, outletLogoPath);
                }

            }
            catch (IOException ioEx)
            {
                LoggerUtil.LogError(ioEx, "Terjadi kesalahan IO: {ioEx.Message}", ioEx.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Terjadi kesalahan:", ex.Message);
            }
        }
        public async Task OpenDualMonitor()
        {
            try
            {
                string data = "ON";
                await File.WriteAllTextAsync("setting\\configDualMonitor.data", data);

                //System.Windows.MessageBox.Show("Contact PM Project.");
                string path = System.Windows.Forms.Application.StartupPath + "KASIRDualMonitor\\KASIR Dual Monitor.exe";
                //OpenApplicationOnSecondMonitor(System.Windows.Forms.Application.StartupPath + @"KASIRDualMonitor\\KASIR Dual Monitor.exe");

                Process p = new Process();
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

            // Jika status offline ON, tampilkan Offline_masterPos
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                string TypeCacheEksekusi = "Sync";

                CacheDataApp CacheDataApp = new CacheDataApp(TypeCacheEksekusi);
                CacheDataApp.Show();
            }
            SettingsForm c = new SettingsForm(this);
            await c.LoadConfig();
            await headerName();
        }
        private async void LoadingAllSetting(string allSettingsData)
        {
            await sendDataSyncPerHours(allSettingsData);
            if (allSettingsData == "ON")
            {

                // Create an instance of transactionFileMover and then call the method
                var fileMover = new transactionFileMover();
                await fileMover.refreshCacheTransaction();
            }

            SettingsForm clean = new SettingsForm(this);
            await clean.CleanupPrinterSettings();

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
            if (string.IsNullOrWhiteSpace(text)) return; // Avoid null or empty

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
                if (!NetworkInterface.GetIsNetworkAvailable())
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
                MessageBox.Show("Error" + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private async Task DownloadUpdaterApp()
        {
            try
            {
                await headerOutletName("Checking New Updater...");

                using (var httpClient = new HttpClient())
                {
                    string urlVersion = "https://raw.githubusercontent.com/bayufrd/update/main/updaterVersionApp.txt";
                    string newVersion = await httpClient.GetStringAsync(urlVersion);
                    if (string.IsNullOrWhiteSpace(newVersion)) throw new Exception("Version data is empty.");

                    string changeVersion = newVersion.Trim();
                    await headerOutletName($"New Version Updater is {changeVersion}");

                    // Version comparison
                    newVersion = newVersion.Replace(".", "");
                    VersionUpdaterApp = VersionUpdaterApp.Replace(".", "");
                    if (Convert.ToInt32(newVersion) > Convert.ToInt32(VersionUpdaterApp))
                    {
                        await headerOutletName("Downloading New Updater...");
                        string fileUrl = "https://raw.githubusercontent.com/bayufrd/update/main/Dastrevas.rar";
                        string destinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{DownloadPath}\\Dastrevas.rar");

                        await DownloadFileAsync(httpClient, fileUrl, destinationPath);
                        await ExtractAndUpdateAsync(destinationPath, changeVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Updater Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                using (var archive = ArchiveFactory.Open(rarFilePath))
                {
                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entry.WriteToDirectory(extractDirectory, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
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
                MessageBox.Show($"Extraction Error: {ex.Message}", "Gaspol");
                LoggerUtil.LogError(ex, "Extraction failed: {ErrorMessage}", ex.Message);
            }
        }

        private async void findingdownloadpath()
        {
            try
            {
                string cariFolder1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
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
                MessageBox.Show("Error" + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        static bool IsValidVersion(string version)
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
                var dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);

                var json = new
                {
                    outlet_name = dataOutlet.data.name,
                    version = currentVersion + " UpdaterVer: " + VersionUpdaterApp,
                    new_version = newVersion,
                    last_updated = GetCurrentTimeInIndonesianFormat()
                };
                // Mengubah objek menjadi string JSON
                string jsonString = JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented); // Tidak ada indentasi
                IApiService apiService = new ApiService();
                HttpResponseMessage response = await apiService.notifikasiPengeluaran(jsonString, $"/update-confirm?outlet_id={baseOutlet}");
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
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    return;
                }

                using (var httpClient = new HttpClient())
                {
                    var urlVersion = Properties.Settings.Default.BaseAddressVersion.ToString();
                    var newVersion = await httpClient.GetStringAsync(urlVersion);
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
                        var urlOutletFocus = RemoveApiPrefix(originalUrl);
                        var focusOutletData = await httpClient.GetStringAsync($"{urlOutletFocus}/update/outletUpdate.txt");

                        // Parsing data focus outlet
                        var focusOutlets = focusOutletData.Trim(new char[] { ' ', '\n', '\r' })
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

                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    LoggerUtil.LogWarning("No internet connection");
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
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
                MessageBox.Show($"Error opening updater: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task<string> GetOutletNameAsync()
        {
            string filePath = $"DT-Cache\\DataOutlet{baseOutlet}.data";

            // Cek apakah file ada dan baca data dari file atau API
            if (File.Exists(filePath))
            {
                return GetOutletNameFromFile(filePath);
            }
            else
            {
                string outletName = await GetOutletNameFromApi();
                if (outletName != null)
                {
                    // Simpan data ke file jika berhasil mendapatkan nama outlet
                    await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(new { data = new { name = outletName } }));
                    return outletName;
                }
                else
                {
                    return " (Hybrid)";
                }
            }
        }
        public async Task headerName()
        {
            try
            {
                string outletName = await GetOutletNameAsync();
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
        public string GetOutletNameFromFile(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            var outletData = JsonConvert.DeserializeObject<JObject>(fileContent);
            return outletData["data"]?["name"]?.ToString();
        }

        public async Task<string> GetOutletNameFromApi()
        {
            IApiService apiService = new ApiService();
            string response = await apiService.CekShift("/outlet/" + baseOutlet);
            if (response != null)
            {
                var apiResponse = JsonConvert.DeserializeObject<JObject>(response);
                File.WriteAllText($"DT-Cache\\DataOutlet{baseOutlet}.data", JsonConvert.SerializeObject(response));

                return apiResponse["data"]?["name"]?.ToString();
            }
            return null;
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
            Color[] colors = {
            RGBColors.color1,
            RGBColors.color2,
            RGBColors.color3,
            RGBColors.color4,
            RGBColors.color5,
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
                currentBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
                currentBtn.ImageAlign = ContentAlignment.MiddleLeft;
            }
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            Color randomColor = PickRandomColor();  // Pick a random color for button
            ActivateButton(sender, randomColor);    // Activate the button

            try
            {
                // Read the OfflineMode status
                string config = "setting\\OfflineMode.data";
                string allSettingsData = File.ReadAllText(config);  // Get the current OfflineMode setting

                // Check if OfflineMode is ON
                if (allSettingsData == "ON")
                {

                    btnShiftLaporan.Enabled = false;
                    iconButton1.Enabled = false;
                    iconButton2.Enabled = false;
                    // If OfflineMode is ON, load the Offline_masterPos form
                    Offline_masterPos offlineMasterPos = new Offline_masterPos();
                    offlineMasterPos.TopLevel = false;
                    offlineMasterPos.Dock = DockStyle.Fill;

                    // Close any existing masterPos form
                    foreach (Control c in panel1.Controls)
                    {
                        if (c is masterPos)
                        {
                            c.Dispose(); // Dispose of the masterPos form
                        }
                    }

                    // Add the Offline_masterPos form to the panel
                    panel1.Controls.Add(offlineMasterPos);
                    offlineMasterPos.BringToFront();
                    offlineMasterPos.Show();
                    await offlineMasterPos.LoadCart();

                    btnShiftLaporan.Enabled = true;
                    iconButton1.Enabled = true;
                    iconButton2.Enabled = true;
                    lblTitleChildForm.Text = "Menu - Offline Mode Transaksi"; // Update label for Offline Mode
                }
                else
                {
                    // If OfflineMode is OFF, load the masterPos form
                    masterPos m = new masterPos();
                    m.TopLevel = false;
                    m.Dock = DockStyle.Fill;

                    // Close any existing Offline_masterPos form
                    foreach (Control c in panel1.Controls)
                    {
                        if (c is Offline_masterPos)
                        {
                            c.Dispose(); // Dispose of the Offline_masterPos form
                        }
                    }

                    // Add the masterPos form to the panel
                    panel1.Controls.Add(m);
                    m.BringToFront();
                    m.Show();

                    lblTitleChildForm.Text = "Menu - Semua Transaksi"; // Update label for normal mode
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void link_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the URL to open
                string url = "http://cms.gaspollmanagementcenter.com";

                // Use the default browser to open the URL
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Show an error message if the operation fails
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                MessageBox.Show(ex.Message);
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
                string allSettingsData = File.ReadAllText(config);  // Get the current OfflineMode setting

                // Check if OfflineMode is ON
                if (allSettingsData == "ON")
                {

                    btnShiftLaporan.Enabled = false;
                    iconButton1.Enabled = false;
                    iconButton2.Enabled = false;
                    Offline_successTransaction c = new Offline_successTransaction();
                    if (c == null)
                    {
                        MessageBox.Show("Terjadi kesalah coba restart aplikasi", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    c.Dock = DockStyle.Fill;
                    panel1.Controls.Add(c);
                    c.BringToFront();
                    c.Show();
                    await c.LoadData();

                    btnShiftLaporan.Enabled = true;
                    iconButton1.Enabled = true;
                    iconButton2.Enabled = true;
                    lblTitleChildForm.Text = "Transactions - History Transactions";
                }
                else
                {
                    successTransaction c = new successTransaction();

                    if (c == null)
                    {
                        return;
                    }
                    c.Dock = DockStyle.Fill;
                    panel1.Controls.Add(c);
                    c.BringToFront();
                    c.Show();
                    lblTitleChildForm.Text = "Transactions - History Transactions";
                    Form background = CreateOverlayForm();

                }

            }
            catch (NullReferenceException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
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
                WindowState = FormWindowState.Maximized;
            else
                WindowState = FormWindowState.Normal;
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
                FormBorderStyle = FormBorderStyle.None;
            else
                FormBorderStyle = FormBorderStyle.Sizable;
        }
        private void btnEditSettings_Click(object sender, EventArgs e)
        {
            Form background = CreateOverlayForm();

            // Create the SettingsForm
            using (SettingsForm settingsForm = new SettingsForm(this))
            {
                // Set the position of the SettingsForm manually
                settingsForm.StartPosition = FormStartPosition.CenterParent;

                // Set the Owner of the SettingsForm to the background form
                settingsForm.Owner = background;

                // Set the TopMost property of the SettingsForm to true
                settingsForm.TopMost = true;

                // Set the FormBorderStyle property of the SettingsForm to None
                settingsForm.FormBorderStyle = FormBorderStyle.None;

                // Set the BackColor property of the SettingsForm to a light color
                settingsForm.BackColor = Color.White;

                // Show the background form first
                background.Show();

                // Show the SettingsForm as a dialog
                DialogResult result = settingsForm.ShowDialog();

                // Handle the result if needed
                if (result == DialogResult.OK)
                {
                    string applicationName = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(applicationName);
                    Environment.Exit(0);
                    // Settings were successfully updated, perform any necessary actions
                }

                // Dispose of the background form
                background.Dispose();
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            // Disable timer while processing to prevent multiple overlapping executions
            SyncTimer.Enabled = false;

            try
            {
                // Read the OfflineMode status
                string config = "setting\\OfflineMode.data";
                string allSettingsData = File.ReadAllText(config);  // Get the current OfflineMode setting

                // Check if OfflineMode is ON
                if (allSettingsData != "ON")
                {
                    return;
                }

                if (!NetworkInterface.GetIsNetworkAvailable())
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

                    UpdateSyncStatus(Color.White, $"Last Sync \n{DateTime.Now:HH:mm}");

                    await Task.Run(async () =>
                    {
                        await cekVersionAndData();
                        await headerName();
                    });
                }
                catch (Exception ex)
                {
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
                using (shiftReport c = new shiftReport())
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
            lblPing.ForeColor = color;
            lblPing.Text = text;
            SignalPing.ForeColor = color;
            SignalPing.Text = text;
            SignalPing.IconColor = color;
        }
        public static readonly object FileLock = new object();
        private async void btnTestSpeed_Click(object sender, EventArgs e)
        {
            SignalPing.Text = "Testing...";
            SignalPing.ForeColor = Color.White; // Default warna saat testing

            try
            {

                int ping = await TestPing("8.8.8.8"); // Ping ke Google DNS
                UpdatePingColor(ping); // Perbarui warna berdasarkan nilai ping

                SignalPing.Text = $"{ping} ms";
                await Task.Delay(2000);
                UpdatePingLabel(Color.White, "Test Ping");

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                UpdatePingLabel(Color.Red, $"Error: {ex.Message}");
            }
        }
        private async Task<int> TestPing(string host)
        {
            Ping pingSender = new Ping();
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
            lblPing.ForeColor = color;
            lblPing.Text = text;
            SignalPing.ForeColor = color;
            SignalPing.Text = text;
            SignalPing.IconColor = color;
        }

        private void UpdatePingColor(int ping)
        {
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
            System.Drawing.Rectangle secondScreen = screens[1].Bounds;

            // Open the application with the specified path and position
            ProcessStartInfo startInfo = new ProcessStartInfo(path);
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
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SyncTimer.Enabled = true;
            }
        }

        private async Task LoadOfflineSuccessTransaction()
        {
            Offline_shiftReport offlineTransaction = new Offline_shiftReport();
            offlineTransaction.Dock = DockStyle.Fill;

            panel1.Controls.Add(offlineTransaction);
            offlineTransaction.BringToFront();
            await offlineTransaction.LoadData();
            lblTitleChildForm.Text = "Shift Report (Offline)";
        }

        private async Task LoadOnlineSuccessTransaction()
        {
            shiftReport onlineTransaction = new shiftReport();
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
                Location = this.Location,
                ShowInTaskbar = false
            };
        }
        private void btnContact_Click(object sender, EventArgs e)
        {

            Form background = CreateOverlayForm();

            using (Offline_Complaint c = new Offline_Complaint())
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

            DevMonitor c = new DevMonitor();

            c.Dock = DockStyle.Fill;
            panel1.Controls.Add(c);
            c.BringToFront();
            c.Show();
        }
    }
}