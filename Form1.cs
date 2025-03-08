using KASIR.komponen;
using KASIR.Komponen;
using FontAwesome.Sharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using System.Runtime.InteropServices;
using System.Net;
using System.Windows.Shapes;
using System.Diagnostics;
using KASIR.Model;
using System.Net.NetworkInformation;
using Serilog;
using KASIR.Network;
using Newtonsoft.Json;
using Menu = KASIR.Model.Menu;
using Path = System.IO.Path;
using System.Xml;
using SharpCompress.Archives;
using SharpCompress.Common;
using DrawingColor = System.Drawing.Color;
using Color = System.Drawing.Color;
using KASIR.OfflineMode;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Linq;
using KASIR.Printer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using KASIR.OffineMode;
using System.Globalization;


namespace KASIR
{
    public partial class Form1 : Form
    {
        private IconButton currentBtn;
        private Panel leftBorderBtn;
        private Form currentChildForm;
        private readonly ILogger _log = LoggerService.Instance._log;

        private static Form1 _instance;
        private readonly string namaOutlet = Properties.Settings.Default.BaseOutletName.ToString();
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet.ToString();
        private readonly string macKasir = Properties.Settings.Default.MacAddressKasir.ToString();
        private readonly string macBar = Properties.Settings.Default.MacAddressBar.ToString();
        private readonly string macKitchen = Properties.Settings.Default.MacAddressKitchen.ToString();
        private readonly string footerStruk = Properties.Settings.Default.FooterStruk.ToString();

        string DownloadPath, VersionUpdaterApp, PathKasir, baseOutletName;
        private static Random random = new Random();

        public Form1()
        {
            InitializeComponent();
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
            lblDetail.Visible = false;
            progressBar.Visible = false;
            StarterApp();
        }
        private async void ConfigOfflineMode()
        {
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
            if (File.Exists(Config) && baseOutlet != "1")
            {
                File.WriteAllText(Config, "ON"); //paksa sahulu untuk inisiasi ramadhan sambel colek
            }
            else
            {
                File.WriteAllText(Config, "OFF"); //paksa sahulu untuk inisiasi ramadhan sambel colek
            }


            string allSettingsData = File.ReadAllText(Config); // Ambil status offline

            // Jika status offline ON, tampilkan Offline_masterPos
            if (allSettingsData == "ON")
            {
                Offline_masterPos offlineMasterPos = new Offline_masterPos();
                offlineMasterPos.TopLevel = false;
                offlineMasterPos.Dock = DockStyle.Fill;

                // Jika ada form masterPos yang sudah terbuka, tutup dulu
                foreach (Control c in panel1.Controls)
                {
                    if (c is masterPos)
                    {
                        c.Dispose(); // Menutup form masterPos jika ada
                    }
                }

                panel1.Controls.Add(offlineMasterPos);
                offlineMasterPos.BringToFront();
                offlineMasterPos.Show();
                await offlineMasterPos.LoadCart();
            }
            else
            {
                masterPos m = new masterPos();
                m.TopLevel = false;
                m.Dock = DockStyle.Fill;

                // Jika ada form Offline_masterPos yang sudah terbuka, tutup dulu
                foreach (Control c in panel1.Controls)
                {
                    if (c is Offline_masterPos)
                    {
                        c.Dispose(); // Menutup form Offline_masterPos jika ada
                    }
                }

                panel1.Controls.Add(m);
                m.BringToFront();
                m.Show();
                await m.LoadCart();

            }
        }
        public async void UpdateContent()
        {
            string Config = "setting\\OfflineMode.data";
            string allSettingsData = File.ReadAllText(Config); // Ambil status offline

            // Jika status offline ON, tampilkan Offline_masterPos
            if (allSettingsData == "ON")
            {
                Offline_masterPos offlineMasterPos = new Offline_masterPos();
                offlineMasterPos.TopLevel = false;
                offlineMasterPos.Dock = DockStyle.Fill;

                // Menutup form masterPos jika ada
                foreach (Control c in panel1.Controls)
                {
                    if (c is masterPos)
                    {
                        c.Dispose();
                    }
                }

                panel1.Controls.Add(offlineMasterPos);
                offlineMasterPos.BringToFront();
                offlineMasterPos.Show();
            }
            else
            {
                masterPos m = new masterPos();
                m.TopLevel = false;
                m.Dock = DockStyle.Fill;

                // Menutup form Offline_masterPos jika ada
                foreach (Control c in panel1.Controls)
                {
                    if (c is Offline_masterPos)
                    {
                        c.Dispose();
                    }
                }

                panel1.Controls.Add(m);
                m.BringToFront();
                m.Show();
            }
        }


        public void initPingTest()
        {
            SignalPing.Text = "Ping Test";
            SignalPing.ForeColor = DrawingColor.White;
            lblPing.ForeColor = DrawingColor.White;
            SignalPing.IconColor = DrawingColor.White;
        }

        static string ReplaceColonWithDash(string mac)
        {
            // Gunakan Replace untuk mengganti semua ":" dengan "-"
            return mac.Replace(":", "-");
        }
        private async Task checkCacheData()
        {
            try
            {
                await headerOutletName("Checking cache data...");
                // Pastikan folder 'setting' ada
                string settingsFolder = "setting";
                if (!Directory.Exists(settingsFolder))
                {
                    Directory.CreateDirectory(settingsFolder);
                }
                if (!File.Exists("setting\\printerSettings.data"))
                {
                    //settingPrinter
                    string filePath = "setting\\printerSettings.data";
                    string kasir = ReplaceColonWithDash(macKasir);
                    string bar = ReplaceColonWithDash(macBar);
                    string kitchen = ReplaceColonWithDash(macKitchen);
                    // Konten yang ingin ditulis ke file
                    string[] lines = {
                    $"inter1:{kasir}",
                    $"inter2:{bar}",
                    $"inter3:{kitchen}"
                     };

                    // Menulis semua konten ke file
                    File.WriteAllLines(filePath, lines);

                    //settingComboboxPrinter
                    filePath = "setting\\checkBoxSettings.data";
                    // Konten yang ingin ditulis ke file
                    string[] lines2 = {
                    "checkBoxMakananPrinter1:False",
                    "checkBoxKasirPrinter2:False",
                    "checkBoxMakananPrinter3:True",
                    "checkBoxKasirPrinter1:True",
                    "checkBoxKasirPrinter3:False",
                    "checkBoxMinumanPrinter1:False",
                    "checkBoxMakananPrinter2:False",
                    "checkBoxMinumanPrinter3:False",
                    "checkBoxCheckerPrinter1:True",
                    "checkBoxMinumanPrinter2:True",
                    "checkBoxCheckerPrinter2:False"
                     };

                    // Menulis semua konten ke file
                    File.WriteAllLines(filePath, lines2);
                }
                if (!File.Exists("setting\\FooterStruk.data"))
                {
                    string data = footerStruk;
                    await File.WriteAllTextAsync("setting\\FooterStruk.data", data);
                }
                if (!File.Exists("setting\\RunningText.data"))
                {
                    string data = footerStruk;
                    await File.WriteAllTextAsync("setting\\RunningText.data", data);
                }
                //DuplicateTemp();

                //CacheDataApp.Show();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Terjadi kesalahan:", ex.Message);
            }
        }

        public static void DuplicateTemp()
        {
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
            headerOutletName("");
            ConfigOfflineMode();

            await Task.Run(async () =>
            {
                await DualMonitorChecker();
                await cekLastUpdaterApp();
                await cekVersionAndData();
                await checkCacheData();

            });
            initPingTest();

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
                File.WriteAllText(Config, "OFF");
            }
            string allSettingsData = File.ReadAllText(Config); // Ambil status offline

            // Jika status offline ON, tampilkan Offline_masterPos
            if (allSettingsData == "ON")
            {
                Offline_masterPos cache = new Offline_masterPos();
                //await refreshCacheTransaction();
                isSyncing = true;
                cache.refreshCacheTransaction();
                isSyncing = false;
            }

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                string TypeCacheEksekusi = "Sync";

                CacheDataApp CacheDataApp = new CacheDataApp(TypeCacheEksekusi);
                CacheDataApp.Show();
            }
            //await CacheDataApp.LoadData(TypeCacheEksekusi);
            await headerName();
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
        private async Task headerOutletName(string text)
        {
            if (lblNamaOutlet.InvokeRequired)
            {
                // Jika kita berada di thread yang berbeda, panggil metode ini di UI thread
                lblNamaOutlet.Invoke(new Action(() => headerOutletName(text)));
            }
            else
            {
                // Jika kita berada di UI thread, lakukan pembaruan langsung
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
                string linkFolderName = "KASIRinstaller";
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
                string changeVersion;
                var urlVersion = "https://raw.githubusercontent.com/bayufrd/update/main/updaterVersionApp.txt";
                var newVersion = (new WebClient().DownloadString(urlVersion));
                string fileUrl2 = "https://raw.githubusercontent.com/bayufrd/update/main/Dastrevas.rar";
                changeVersion = newVersion.ToString();
                await headerOutletName($"New Version Updater is {changeVersion}");

                newVersion = newVersion.Replace(".", "");
                VersionUpdaterApp = VersionUpdaterApp.Replace(".", "");

                if (Convert.ToInt32(newVersion) > Convert.ToInt32(VersionUpdaterApp))
                {
                    await headerOutletName("Downloading New Updater...");

                    string destinationPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{DownloadPath}\\Dastrevas.rar");
                    using (WebClient wb = new WebClient())
                    {

                        // Start the download
                        byte[] fileData2 = wb.DownloadData(fileUrl2);
                        // //MessageBox.Show($"Downloaded {(fileData.Length) / 100000} MB.");
                        System.IO.File.WriteAllBytes(destinationPath2, fileData2);
                    }
                    string rarFilePath = $"{DownloadPath}\\Dastrevas.rar";
                    string extractDirectory = $"{PathKasir}\\update";

                    try
                    {
                        await headerOutletName("Extracting New Updater...");

                        // Ensure the extraction directory exists
                        if (!Directory.Exists(extractDirectory))
                        {
                            Directory.CreateDirectory(extractDirectory);
                        }

                        // Log the start of the extraction process

                        using (var archive = ArchiveFactory.Open(rarFilePath))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                // Extract each entry in the RAR file to the specified directory
                                if (!entry.IsDirectory)
                                {
                                    entry.WriteToDirectory(extractDirectory, new ExtractionOptions()
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = true // Set to true to overwrite existing files
                                    });
                                }
                            }
                        }
                        // Path ke file .txt yang akan diubah
                        string filePath = $"{PathKasir}\\update\\versionUpdater.txt";

                        // Konten baru yang akan ditulis ke file
                        string contentToWrite = changeVersion.ToString();
                        await headerOutletName("Installing New Updater...");

                        // Menulis konten baru ke file
                        File.WriteAllText(filePath, contentToWrite);
                        Thread.Sleep(3000);
                        OpenUpdaterExe();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error" + ex.Message, "Gaspol");
                        LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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

                LoggerUtil.LogWarning("Open Updater App Dastrevass");

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


        private async Task cekCacheData()
        {
            await CompareDataAsync();
        }

        static async Task CompareDataAsync()
        {
            try
            {
                IApiService apiService = new ApiService();

                // Memuat data dari API
                string baseOutlet = Properties.Settings.Default.BaseOutlet.ToString();
                string apiResponse = await apiService.Get("/menu?outlet_id=" + baseOutlet);
                GetMenuModel apiMenuModel = JsonConvert.DeserializeObject<GetMenuModel>(apiResponse);
                List<Menu> apiMenuList = apiMenuModel.data.ToList();

                // Memuat data dari file .data
                string filePath = $"DT-Cache\\menu_outlet_id_{baseOutlet}.data";
                List<Menu> fileMenuList = LoadMenuFromFile(filePath);

                // Membandingkan data
                CompareMenuData(apiMenuList, fileMenuList);
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        static List<Menu> LoadMenuFromFile(string filePath)
        {
            List<Menu> menuList = new List<Menu>();
            try
            {
                string json = File.ReadAllText(filePath);
                GetMenuModel fileMenuModel = JsonConvert.DeserializeObject<GetMenuModel>(json);
                menuList = fileMenuModel.data.ToList();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File not found: " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading file: " + ex.Message);
            }
            return menuList;
        }

        static void CompareMenuData(List<Menu> apiMenuList, List<Menu> fileMenuList)
        {
            // Membandingkan data antara API dan file
            foreach (Menu apiMenu in apiMenuList)
            {
                bool found = false;
                bool notequal = false;
                foreach (Menu fileMenu in fileMenuList)
                {
                    if (apiMenu.id == fileMenu.id)
                    {
                        // Memeriksa apakah ada perbedaan data
                        if (!AreMenusEqual(apiMenu, fileMenu))
                        {

                            notequal = true;
                        }
                        found = true;
                        break;
                    }
                }
                // Menjalankan fungsi jika ID baru ditemukan di API
                if (!found || notequal)
                {
                    YourFunctionToHandleChanges();
                    //YourFunctionToAddNewID();
                    break;
                }
            }
        }

        static bool AreMenusEqual(Menu menu1, Menu menu2)
        {
            return menu1.name == menu2.name && menu1.price == menu2.price && menu1.menu_type == menu2.menu_type;
        }

        static void YourFunctionToHandleChanges()
        {
            // Fungsi Anda untuk menangani perbedaan antara data dari API dan file
            // Anda bisa menambahkan log, pembaruan, atau tindakan lain sesuai kebutuhan
            MessageBox.Show("Terdapat Perubahan Item Menu!\nSilahkan Sinkronisasi ulang pada setting bawah kiri\nLalu jalankan tombol Sync!");

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
                    version = currentVersion,
                    new_version = newVersion,
                    last_updated = DateTime.Now.ToString("yyyy-MM-dd HH=mm=ss", CultureInfo.InvariantCulture)
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
        private async Task cekVersionAndData()
        {
            try
            {
                await headerOutletName("Checking Kasir Version...");

                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    return;
                }
                var urlVersion = Properties.Settings.Default.BaseAddressVersion.ToString();
                var newVersion = (new WebClient().DownloadString(urlVersion));
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
                    var focusOutletData = (new WebClient().DownloadString($"{urlOutletFocus}/update/outletUpdate.txt"));

                    // Parsing data focus outlet
                    var focusOutlets = focusOutletData.Trim(new char[] { ' ', '\n', '\r' })
                                                       .Split(',')
                                                       .Select(s => s.Trim()) // Menghapus spasi di sekitar
                                                       .ToArray();

                    // Cek apakah baseOutlet ada dalam focusOutlets
                    if (focusOutlets.Contains(baseOutlet) || focusOutlets.Contains("0"))
                    {
                        shouldUpdate = true;
                    }

                    if (shouldUpdate)
                    {
                        Util n = new Util();
                        n.sendLogTelegramNetworkError("Open Updater");
                        await headerOutletName("Opening Kasir Updater...");

                        OpenUpdaterExe();
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

        private async void OpenUpdaterExe()
        {
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "update\\update.exe"))
            {
                LoggerUtil.LogWarning("Open Update.exe");


                string path = System.Windows.Forms.Application.StartupPath + "update\\update.exe";

                Process p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.Arguments = "";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.Verb = "runas";
                p.Start();
                Thread.Sleep(3000);

                System.Windows.Forms.Application.Exit();
            }
            else
            {
                DownloadUpdaterApp();
            }
        }

        public async Task headerName()
        {
            try
            {
                string filePath = $"DT-Cache\\DataOutlet{baseOutlet}.data";
                string outletName;

                // Cek apakah file ada dan baca data dari file atau API
                if (File.Exists(filePath))
                {
                    outletName = GetOutletNameFromFile(filePath);
                }
                else
                {
                    outletName = await GetOutletNameFromApi();
                    if (outletName != null)
                    {
                        // Simpan data ke file jika berhasil mendapatkan nama outlet
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(new { data = new { name = outletName } }));
                    }
                    else
                    {
                        outletName = " (Hybrid)";
                    }
                }

                // Update label UI
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
        private void OpenChildForm(Form childForm)
        {
            //open only form
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }
            currentChildForm = childForm;
            //End
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panel2.Controls.Add(childForm);
            panel2.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
            lblTitleChildForm.Text = childForm.Text;
        }
        private void Reset()
        {
            DisableButton();
            leftBorderBtn.Visible = false;
            iconCurrentChildForm.IconChar = IconChar.Home;
            iconCurrentChildForm.IconColor = Color.MediumPurple;
            lblTitleChildForm.Text = "Menu";
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


        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Color randomColor = PickRandomColor();
            ActivateButton(sender, randomColor);

            masterMenu c = new masterMenu();

            panel3.Height = button2.Height;
            panel3.Top = button2.Top;
            c.Dock = DockStyle.Fill;
            panel1.Controls.Add(c);
            c.BringToFront();
            c.Show();
            lblTitleChildForm.Text = "Home";

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        private void link_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(link_Click));

            try
            {
                // Specify the URL to open
                string url = "http://cms.gaspol-services.site";

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
        private async void button1_Click(object sender, EventArgs e)
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
                    // If OfflineMode is ON, load the Offline_masterPos form
                    Offline_successTransaction c = new Offline_successTransaction();
                    if (c == null)
                    {
                        MessageBox.Show("Terjadi kesalah coba restart aplikasi", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    //panel3.Height = btn1.Height;
                    //panel3.Top = btn1.Top;
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

                    // Misalkan 'obj' adalah objek yang mungkin null
                    if (c == null)
                    {
                        return;
                    }
                    //panel3.Height = btn1.Height;
                    //panel3.Top = btn1.Top;
                    c.Dock = DockStyle.Fill;
                    panel1.Controls.Add(c);
                    c.BringToFront();
                    c.Show();
                    lblTitleChildForm.Text = "Transactions - History Transactions";
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
                    // Lakukan operasi dengan 'obj'
                    // ...
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
            // Create the background form
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
        private static bool isSyncing = false;  // Flag to check if sync is in progress

        private async void timer1_Tick(object sender, EventArgs e)
        {
            // Read the OfflineMode status
            string config = "setting\\OfflineMode.data";
            string allSettingsData = File.ReadAllText(config);  // Get the current OfflineMode setting

            // Check if OfflineMode is ON
            if (allSettingsData != "ON")
            {
                return;
            }

            if (isSyncing) // Check if sync is already in progress
            {
                return; // If sync is already running, exit
            }

            try
            {
                isSyncing = true;  // Set the flag to indicate sync is in progress

                lblPing.ForeColor = Color.LightGreen;
                lblPing.Text = "Sync...";
                SignalPing.ForeColor = Color.LightGreen;
                SignalPing.Text = "Sync...";
                SignalPing.IconColor = Color.LightGreen;

                await sendDataSyncPerHours(allSettingsData);

                lblPing.ForeColor = Color.White;
                lblPing.Text = $"Last Sync \n{DateTime.Now:HH:mm}";
                SignalPing.ForeColor = Color.White;
                SignalPing.Text = $"Last Sync \n{DateTime.Now:HH:mm}";
                SignalPing.IconColor = Color.White;
            }
            finally
            {
                isSyncing = false;  // Reset the flag when sync is finished
            }
        }

        public async Task sendDataSyncPerHours(string allSettingsData)
        {
            // Check if OfflineMode is ON
            if (allSettingsData == "ON")
            {
                /*    Offline_masterPos m = new Offline_masterPos();

                    m.refreshCacheTransaction();*/
                shiftReport c = new shiftReport();
                //c.SyncCompleted += SyncCompletedHandler;
                c.SyncDataTransactions();
            }
        }
        private void SyncCompletedHandler()
        {
            string filePath = "DT-Cache\\Transaction\\transaction.data";

            // Memperbarui file yang telah disinkronkan atau memperbarui status UI
            SyncSuccess(filePath);

            // Misalnya, memanggil fungsi untuk memperbarui status is_sent_sync di form utama
        }
        public static readonly object FileLock = new object();
        public static void SyncSuccess(string filePath)
        {
            try
            {
                lock (FileLock) // Pastikan hanya satu thread yang menulis ke file
                {
                    // Baca file JSON
                    string jsonData = File.ReadAllText(filePath);
                    JObject data = JObject.Parse(jsonData);

                    // Dapatkan array "data"
                    JArray transactions = (JArray)data["data"];

                    // Iterasi setiap transaksi dan hanya perbarui yang statusnya is_sent_sync = 0
                    foreach (JObject transaction in transactions)
                    {
                        if (transaction["is_sent_sync"] != null && (int)transaction["is_sent_sync"] == 0)
                        {
                            // Update status menjadi 1 jika transaksi berhasil
                            transaction["is_sent_sync"] = 1;

                            // Simpan perubahan hanya untuk transaksi yang berhasil disinkronkan
                        }
                    }
                    File.WriteAllText(filePath, data.ToString());

                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred during SyncSuccess: {ErrorMessage}", ex.Message);
            }
        }
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

            //====by
            ActivateButton(sender, RGBColors.color4);
            try
            {
                if (isSyncing)
                {
                    // If syncing is already in progress, don't do anything
                    MessageBox.Show("Data sedang di load. Tolong tunggu sebentar!");
                    return;
                }
                isSyncing = true;
                shiftReport c = new shiftReport();

                // Misalkan 'obj' adalah objek yang mungkin null
                if (c == null)
                {
                    MessageBox.Show("Terjadi kesalahan cek koneksi anda", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                btnShiftLaporan.Enabled = false;
                iconButton1.Enabled = false;
                iconButton2.Enabled = false;
                //panel3.Height = btn1.Height;
                //panel3.Top = btn1.Top;
                c.Dock = DockStyle.Fill;
                panel1.Controls.Add(c);
                c.BringToFront();
                c.Show();
                await c.LoadData();
                btnShiftLaporan.Enabled = true;
                iconButton1.Enabled = true;
                iconButton2.Enabled = true;
                lblTitleChildForm.Text = "Shift Report - Report Shift and Shift Transactions, Print Shift and Cash Out";


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
                // Lakukan operasi dengan 'obj'
                // ...
            }
            catch (NullReferenceException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Log error jika diperlukan
            }
            finally
            {
                isSyncing = false;
            }

        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            /* //settingsButton
             //====by
             ActivateButton(sender, RGBColors.color4);
             try
             {
                 SettingsForm c = new SettingsForm(this);

                 // Misalkan 'obj' adalah objek yang mungkin null
                 if (c == null)
                 {
                     MessageBox.Show("Terjadi kesalahan cek koneksi anda", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     return;
                 }
                 //panel3.Height = btn1.Height;
                 //panel3.Top = btn1.Top;
                 c.Dock = DockStyle.Fill;
                 panel1.Controls.Add(c);
                 c.BringToFront();
                 c.Show();
                 lblTitleChildForm.Text = "Settings - ";
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
                 // Lakukan operasi dengan 'obj'
                 // ...
             }
             catch (NullReferenceException ex)
             {
                 MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 // Log error jika diperlukan
             }*/
        }

        private void btnContact_Click(object sender, EventArgs e)
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

            using (Offline_Complaint c = new Offline_Complaint())
            {
                c.Owner = background;

                background.Show();

                DialogResult dialogResult = c.ShowDialog();

                background.Dispose();
            }
        }
    }
}