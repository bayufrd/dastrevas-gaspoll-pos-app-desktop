using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using FontAwesome.Sharp;
using KASIR.Helper;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.OffineMode;
using KASIR.OfflineMode;
using KASIR.Properties;
using KASIR.Services;
using Newtonsoft.Json;
using Color = System.Drawing.Color;
using DrawingColor = System.Drawing.Color;
using Path = System.IO.Path;
using Timer = System.Windows.Forms.Timer;


namespace KASIR
{
    public partial class Form1 : Form
    {
        private IconButton currentBtn;
        private readonly Panel leftBorderBtn;
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet.ToString();
        private readonly OutletService _outletService;
        private string baseOutletName;
        private static readonly Random random = new();
        private readonly IInternetService _internetServices;
        private readonly UpdaterHelper _updaterHelper;

        public Form1()
        {
            InitializeComponent();
            _internetServices = new InternetService();
            _updaterHelper = new UpdaterHelper(_internetServices);

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
            if (baseOutlet != "4")
            {
                btnDev.Visible = false;
            }
            StarterApp();
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

        private void RecursiveRefreshIcons(Control control)
        {
            if (control is IconButton iconBtn)
            {
                iconBtn.Invalidate();
            }

            foreach (Control child in control.Controls)
            {
                RecursiveRefreshIcons(child);
            }
        }

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

            if (!Directory.Exists(directoryPath))
            {
                _ = Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(configPath))
            {
                await File.WriteAllTextAsync(configPath, "ON");
            }
            else
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
                await LoadOfflineMasterPos();
                UpdateOutletLabel(outletName + " (Online)");
            }
            LoadingAllSetting(allSettingsData);

        }

        private async Task LoadOfflineMasterPos()
        {
            Offline_masterPos offlineMasterPos = new() { TopLevel = false, Dock = DockStyle.Fill };

            panel1.Controls.Add(offlineMasterPos);
            offlineMasterPos.BringToFront();
            offlineMasterPos.Show();
            await offlineMasterPos.LoadCart();
        }
        public async void UpdateContent()
        {
            ConfigOfflineMode();
        }


        public async void initPingTest()
        {
            if (isOpenNavbar)
            {
                SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
                SignalPing.Text = "Ping Test";

                Whatsapp_Config whatsappConfig = new();
                var connectionStatus = await whatsappConfig.CheckConnectionStatusAsync();
                if (connectionStatus.Connected)
                {
                    btnWhatsapp.Text = "Connected";
                }
                else
                {
                    btnWhatsapp.Text = "Whatsapp Config";
                }
            }
            else
            {
                SignalPing.TextImageRelation = TextImageRelation.ImageAboveText;
                SignalPing.Text = "\n\nPing\nTest";

                Whatsapp_Config whatsappConfig = new();
                var connectionStatus = await whatsappConfig.CheckConnectionStatusAsync();
                if (connectionStatus.Connected)
                {
                    btnWhatsapp.Text = "";
                    btnWhatsapp.ForeColor = Color.LimeGreen;
                }
                else
                {
                    btnWhatsapp.ForeColor = Color.WhiteSmoke;
                    btnWhatsapp.Text = "";
                }
            }
            SignalPing.ForeColor = DrawingColor.White;
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
                _ = p.Start();

                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private void SignApplication()
        {
            string executablePath = Application.ExecutablePath;
            string certificatePath = "DastrevasPOS.pfx";

            bool signingResult = CodeSigner.SignExecutable(
                executablePath,
                certificatePath,
                "DT2025"
            );

            if (signingResult)
            {
                NotifyHelper.Success("Aplikasi berhasil ditandatangani!");
            }
            else
            {
                NotifyHelper.Error("Signing Aplikasi gagal.");
            }
        }
        private async void StarterApp()
        {
            await headerName();
            SignApplication();
            initPingTest();
            ConfigOfflineMode();

            await Task.Run(async () =>
            {
                await DualMonitorChecker();

                if (await _internetServices.IsInternetConnectedAsync())
                {
                    await _updaterHelper.CheckLastUpdaterAppAsync();

                }
            });
            if (await _internetServices.IsInternetConnectedAsync())
            {
                await checkVersionAppWindows();

                string TypeCacheEksekusi = "Sync";

                CacheDataApp cacheDataApp = new(TypeCacheEksekusi);

                cacheDataApp.Load += (sender, e) =>
                {
                    ShowSystemTrayNotification("Sinkronisasi dimulai", "Proses sinkronisasi data sedang berjalan");
                };

                cacheDataApp.Show();
            }
        }
        private void ShowSystemTrayNotification(string title, string message)
        {
            NotifyIcon notifyIcon = new()
            {
                Visible = true,
                Icon = SystemIcons.Information,
                Text = title
            };

            notifyIcon.ShowBalloonTip(5000, title, message, ToolTipIcon.Info);

            Timer timer = new();
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
                return;
            }

            if (lblNamaOutlet.InvokeRequired)
            {
                _ = await Task.Factory.FromAsync(
                    lblNamaOutlet.BeginInvoke(new Action(() => lblNamaOutlet.Text = "Please Wait.. " + text), null),
                    ar => lblNamaOutlet.EndInvoke(ar)
                );
            }
            else
            {
                lblNamaOutlet.Text = "Please Wait.. " + text;
            }
        }


        private async Task DownloadFileAsync(HttpClient httpClient, string fileUrl, string destinationPath)
        {
            try
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                using (var request = new HttpRequestMessage(HttpMethod.Get, fileUrl))
                {
                    using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        _ = response.EnsureSuccessStatusCode();

                        long? totalBytes = response.Content.Headers.ContentLength;

                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            byte[] buffer = new byte[8192];
                            long totalBytesRead = 0;
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);

                                totalBytesRead += bytesRead;

                                if (totalBytes.HasValue)
                                {
                                    int percentComplete = (int)((totalBytesRead * 100) / totalBytes.Value);

                                    if (percentComplete % 10 == 0)
                                    {
                                        NotifyHelper.Success($"Mengunduh Pembaruan: {percentComplete}%");
                                    }
                                }
                            }
                        }
                    }
                }

                NotifyHelper.Success("Unduhan Pembaruan Selesai");
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal mengunduh file");
                NotifyHelper.Error($"Kesalahan Unduhan: {ex.Message}");
                throw;
            }
        }

        private static bool IsValidVersion(string version)
        {
            return Regex.IsMatch(version, @"^\d{1,5}$");
        }

        private async Task ConfirmUpdate(string newVersion, string currentVersion)
        {
            try
            {
                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                string versionUpdaterApp = await _updaterHelper.CheckVersionNewAppAsync();
                var json = new
                {
                    outlet_name = dataOutlet.data.name,
                    version = currentVersion + " UpdaterVer: " + versionUpdaterApp,
                    new_version = newVersion,
                    last_updated = GetCurrentTimeInIndonesianFormat()
                };
                string jsonString =
                    JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented); // Tidak ada indentasi
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

        /// <summary>
        /// Proses utama pengecekan dan manajemen versi aplikasi
        /// </summary>
        public async Task checkVersionAppWindows()
        {
            try
            {
                // Langkah 1: Validasi Koneksi Internet
                if (!_internetServices.IsInternetConnected())
                {
                    return;
                }

                using (HttpClient httpClient = new())
                {
                    // Langkah 2: Persiapan URL Versi
                    string oldUrl = Properties.Settings.Default.BaseAddressProd.ToString();
                    string urlVersion = RemoveApiPrefix(oldUrl);

                    // Langkah 3: Ambil Informasi Versi
                    string newVersion = await httpClient.GetStringAsync(urlVersion + "/server/version.txt");
                    string currentVersion = Properties.Settings.Default.Version.ToString();

                    NotifyHelper.Success($"Versi Saat Ini: {currentVersion}, Versi Baru: {newVersion}");

                    // Langkah 4: Konfirmasi Update
                    await ConfirmUpdate(newVersion.ToString(), currentVersion);
                    string newVersionAppBatch = newVersion;

                    // Langkah 5: Persiapan Versi untuk Perbandingan
                    newVersion = newVersion.Replace(".", "");
                    string currentAppNow = currentVersion;
                    currentVersion = currentVersion.Replace(".", "");

                    // Validasi versi
                    if (!IsValidVersion(currentVersion))
                    {
                        currentVersion = "1080"; // Versi default jika tidak valid
                    }

                    // Langkah 6: Pemeriksaan Kebutuhan Update
                    bool shouldUpdate = false;
                    if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
                    {
                        try
                        {
                            // Dialog konfirmasi update
                            string titleHelper = "Update tersedia";
                            string msgHelper = $"Update tersedia Versi: {newVersionAppBatch}. Versi saat ini: #{currentAppNow}. Lanjutkan update ?";
                            string cancelHelper = "Lain kali";
                            string okHelper = "Update";
                            string updateHelper = "update";

                            using (var c = new QuestionHelper(titleHelper, msgHelper, cancelHelper, okHelper, updateHelper))
                            {
                                // Pakai this kalau dipanggil dari Form, agar jadi owner
                                var result = c.ShowDialog(this); // atau c.ShowDialog();

                                if (result != DialogResult.OK)
                                    return;

                                // --- lanjut proses update ---
                                string focusOutletData =
                                    await httpClient.GetStringAsync($"{urlVersion}/server/outletUpdate.txt");

                                string[] focusOutlets = focusOutletData
                                    .Trim(' ', '\n', '\r')
                                    .Split(',')
                                    .Select(s => s.Trim())
                                    .ToArray();

                                if (focusOutlets.Contains(baseOutlet) || focusOutlets.Contains("0"))
                                {
                                    shouldUpdate = true;
                                    NotifyHelper.Success("Mempersiapkan Updater Kasir...");
                                }

                                if (shouldUpdate)
                                {
                                    LoggerUtil.LogNetwork("Memulai Proses Update");
                                    await OpenUpdaterAsync(currentAppNow, urlVersion, newVersionAppBatch);
                                }

                                LoggerUtil.LogWarning($"Old Url {oldUrl}\n" +
                                    $"NewUrl :{urlVersion}/server/version.txt\n" +
                                    $"Current Version : {currentAppNow}\n" +
                                    $"New Version : {newVersionAppBatch}\n" +
                                    $"Url Version : {urlVersion}\n" +
                                    $"Choose : {result.ToString()}\n" +
                                    $"FocusOutlet : {focusOutletData.ToString()}");
                            }
                        }
                        catch (Exception updateEx)
                        {
                            // Fallback: Paksa update jika terjadi kesalahan
                            LoggerUtil.LogError(updateEx, "Kesalahan saat memeriksa update");
                            await OpenUpdaterAsync(currentAppNow, urlVersion, newVersionAppBatch);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Kesalahan umum dalam proses update");
            }
        }

        /// <summary>
        /// Menghapus prefix API dari URL
        /// </summary>
        private string RemoveApiPrefix(string url)
        {
            return url.Contains("api.") ? url.Replace("api.", "") : url;
        }

        /// <summary>
        /// Proses update aplikasi
        /// </summary>
        public async Task OpenUpdaterAsync(string version, string urlVersion, string newVersionAppBatch)
        {
            try
            {
                // Langkah 1: Inisialisasi Proses Update
                NotifyHelper.Success("Memulai Proses Update...");

                // Langkah 2: Siapkan Path dan Folder
                string rootPath = AppDomain.CurrentDomain.BaseDirectory;
                string backupFolder = Path.Combine("TempData", "BackupApp", version);
                string updateTempFolder = Path.Combine("TempData", "updateTemp");
                string updateFile = Path.Combine(updateTempFolder, "update.zip");
                string fileUrl = urlVersion + "/server/update.zip";

                // Buat folder backup dan temp
                _ = Directory.CreateDirectory(backupFolder);
                _ = Directory.CreateDirectory(updateTempFolder);

                // Langkah 3: Backup File Aplikasi
                string[] backupFiles = {
            "KASIR.deps.json",
            "KASIR.dll",
            "KASIR.exe",
            "KASIR.pdb",
            "KASIR.runtimeconfig.json"
        };

                // Proses backup file
                foreach (var fileName in backupFiles)
                {
                    string sourcePath = Path.Combine(rootPath, fileName);
                    string destPath = Path.Combine(backupFolder, fileName);

                    if (File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, destPath, true);
                    }
                }


                // Langkah 4: Kompres Backup
                string backupZipPath = Path.Combine("TempData", "BackupApp", $"{version}.zip"); // Gunakan .zip

                // Proses backup file
                foreach (var fileName in backupFiles)
                {
                    string sourcePath = Path.Combine(rootPath, fileName);
                    string destPath = Path.Combine(backupFolder, fileName);

                    if (File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, destPath, true);
                    }
                }

                try
                {
                    // Kompresi file-file spesifik
                    using (FileStream zipToCreate = new(backupZipPath, FileMode.Create))
                    {
                        using (ZipArchive archive = new(zipToCreate, ZipArchiveMode.Create))
                        {
                            foreach (var fileName in backupFiles)
                            {
                                string filePath = Path.Combine(backupFolder, fileName);
                                if (File.Exists(filePath))
                                {
                                    // Tambahkan setiap file ke dalam arsip
                                    _ = archive.CreateEntryFromFile(filePath, fileName, CompressionLevel.Optimal);
                                }
                            }
                        }
                    }

                    LoggerUtil.LogNetwork($"Berhasil membuat arsip: {backupZipPath}");
                }
                catch (Exception compressEx)
                {
                    LoggerUtil.LogError(compressEx, "Gagal membuat arsip backup");
                    NotifyHelper.Error($"Kesalahan kompresi: {compressEx.Message}");
                }


                // Langkah 5: Download Pembaruan
                using (HttpClient httpClient = new())
                {
                    NotifyHelper.Success("Mengunduh Pembaruan...");
                    await DownloadFileAsync(httpClient, fileUrl, updateFile);
                }

                // Langkah 6: Persiapan Update Mandiri
                await PrepareSelfUpdate(rootPath, newVersionAppBatch);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal melakukan update");
                NotifyHelper.Error($"Kesalahan update: {ex.Message}");
            }
        }

        public async Task PrepareSelfUpdate(string rootPath, string newVersionAppBatch)
        {
            try
            {
                string updateTempPath = Path.Combine(rootPath, "TempData", "updateTemp");
                _ = Directory.CreateDirectory(updateTempPath);

                string logPath = Path.Combine(updateTempPath, "update_log.txt");
                string zipPath = Path.Combine(updateTempPath, "update.zip");
                string configPath = Path.Combine(rootPath, "KASIR.dll.config");
                string exePath = Path.Combine(rootPath, "KASIR.exe");

                string batchFile = Path.Combine(Path.GetTempPath(), "self_update.bat");

                string batchScript = $@"
@echo off
setlocal enabledelayedexpansion

cd /d ""{rootPath}""

echo ================================================ >> ""{logPath}""
echo [INFO] Update dimulai pada %date% %time% >> ""{logPath}""
echo ================================================ >> ""{logPath}""

echo [INFO] Menutup aplikasi lama...
:waitloop
tasklist /FI ""IMAGENAME eq {Path.GetFileName(exePath)}"" | find /I ""{Path.GetFileName(exePath)}"" >nul
if not errorlevel 1 (
    echo [WAIT] Menunggu aplikasi tertutup...
    ping 127.0.0.1 -n 2 > nul
    goto waitloop
)
echo [OK] Aplikasi tertutup >> ""{logPath}""
echo [OK] Aplikasi tertutup

echo [INFO] Ekstrak file update...
powershell -ExecutionPolicy Bypass -NoLogo -NoProfile -Command ""Expand-Archive -Force '{zipPath}' '{rootPath}'""
if %errorlevel% neq 0 (
    echo [ERR] Ekstraksi gagal >> ""{logPath}""
    echo [ERR] Ekstraksi gagal
    pause
    exit /b 1
)
echo [OK] Ekstraksi berhasil >> ""{logPath}""
echo [OK] Ekstraksi berhasil

echo [INFO] Update konfigurasi KASIR.dll.config...

echo [INFO] Menjalankan ulang aplikasi...
start """" ""{exePath}""
echo [OK] Aplikasi dimulai kembali >> ""{logPath}""
echo [OK] Aplikasi dimulai kembali

echo.
echo [DONE] Update selesai. Tekan ENTER untuk keluar.
pause > nul
";
                string kasirConfig = "KASIR.dll.config";

                // Pastikan file konfigurasi ada
                if (!File.Exists(kasirConfig))
                {
                    NotifyHelper.Error($"Konfigurasi tidak ditemukan di: {kasirConfig}");
                    return;
                }
                NotifyHelper.Success($"Sukses Konfigurasi");

                var doc = new XmlDocument();
                doc.Load(kasirConfig);

                // Update versi
                XmlNode versionNode = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='Version']/value");
                if (versionNode != null)
                {
                    versionNode.InnerText = newVersionAppBatch;
                }

                // Simpan perubahan
                doc.Save(kasirConfig);

                File.WriteAllText(batchFile, batchScript, Encoding.Default);

                var proc = new Process();
                proc.StartInfo.FileName = batchFile;
                proc.StartInfo.CreateNoWindow = false; // tampilkan CMD
                proc.StartInfo.UseShellExecute = true;
                _ = proc.Start();

                Application.Exit();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(rootPath, "TempData", "updateTemp", "error_log.txt"),
                    $"Kesalahan update: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Dalam method utama
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
                _ = lblNamaOutlet.Invoke(new MethodInvoker(() => lblNamaOutlet.Text = outletName));
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
            if (isOpenNavbar)
            {
                closeNavbar();
                isOpenNavbar = false;
            }
            Color randomColor = PickRandomColor();
            ActivateButton(sender, randomColor);

            try
            {
                btnShiftLaporan.Enabled = false;
                MenuBtn.Enabled = false;
                TransBtn.Enabled = false;
                // If OfflineMode is ON, load the Offline_masterPos form
                Offline_masterPos offlineMasterPos = new();
                offlineMasterPos.TopLevel = false;
                offlineMasterPos.Dock = DockStyle.Fill;

                panel1.Controls.Add(offlineMasterPos);
                offlineMasterPos.BringToFront();
                offlineMasterPos.Show();
                await offlineMasterPos.LoadCart();

                btnShiftLaporan.Enabled = true;
                MenuBtn.Enabled = true;
                TransBtn.Enabled = true;
                lblTitleChildForm.Text = "Menu - Offline Mode Transaksi";
            }
            catch (Exception ex)
            {
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

        private async void buttonHistoryTransaction(object sender, EventArgs e)
        {
            if (isOpenNavbar)
            {
                closeNavbar();
                isOpenNavbar = false;
            }
            Color randomColor = PickRandomColor();
            ActivateButton(sender, randomColor);

            try
            {
                string config = "setting\\OfflineMode.data";
                string allSettingsData = File.ReadAllText(config);

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

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }

        private async void btnExit_Click(object sender, EventArgs e)
        {
            // coba keluar normal dulu
            Application.Exit();

            // tunggu sebentar (0.5 detik)
            await Task.Delay(500);

            // cek kalau masih hidup → paksa kill
            var currentProcess = Process.GetCurrentProcess();
            try
            {
                if (!currentProcess.HasExited)
                {
                    currentProcess.Kill();
                }
            }
            catch { /* abaikan error kalau sudah mati */ }
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

        private void FormMainMenu_Resize(object sender, EventArgs e)
        {

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
                string config = "setting\\OfflineMode.data";
                string allSettingsData = File.ReadAllText(config);

                if (allSettingsData != "ON")
                {
                    return;
                }

                if (!_internetServices.IsInternetConnected())
                {
                    SignalPing.ForeColor = Color.Red;
                    SignalPing.Text = $"Error\n Sync \n{DateTime.Now:HH:mm}";
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
                    NotifyHelper.Success($"Berhasil syncron server at ${DateTime.Now:HH:mm}");
                    Whatsapp_Config whatsappConfig = new();
                    var connectionStatus = await whatsappConfig.CheckConnectionStatusAsync();
                    if (connectionStatus.Connected)
                    {
                        btnWhatsapp.Text = "Connected";
                    }
                    else
                    {
                        btnWhatsapp.Text = "Whatsapp Config";
                    }
                    await Task.Run(async () =>
                    {
                        await checkVersionAppWindows();
                        //await headerName();
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
                    SignalPing.Text = $"Error\n Sync \n{DateTime.Now:HH:mm}";
                    SignalPing.IconColor = Color.White;
                    NotifyHelper.Error($"Gagal syncron server at ${DateTime.Now:HH:mm}");

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
            _ = Process.Start(startInfo);
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
            if (isOpenNavbar)
            {
                closeNavbar();
                isOpenNavbar = false;
            }
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

        private void btnContact_Click(object sender, EventArgs e)
        {
            using (Offline_Complaint c = new())
            {
                QuestionHelper bg = new(null, null, null, null);
                Form background = bg.CreateOverlayForm();

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
            if (isOpenNavbar)
            {
                closeNavbar();
                isOpenNavbar = false;
            }
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

                LogoKasir.Image = Resources.a_2_;
                LogoKasir.Height = 103;

                MenuBtn.Text = "Menu";
                TransBtn.Text = "Transaction";
                btnShiftLaporan.Text = "Shift Report";
                BtnSettingForm.Text = "Settings";
                btnContact.Text = "Contact Us";
                btnDev.Text = "Developer";
                SignalPing.Text = "Test Ping";
            }
            else
            {
                closeNavbar();
            }
        }

        private void closeNavbar()
        {
            // Mode navbar tertutup
            gradientPanel2.Width = 50;
            LogoKasir.Width = 50;
            panel2.Width = 50;

            LogoKasir.Image = Resources.logoblack;
            LogoKasir.Height = 50;

            SignalPing.Width = 50;

            MenuBtn.Text = "";
            TransBtn.Text = "";
            btnShiftLaporan.Text = "";
            BtnSettingForm.Text = "";
            btnContact.Text = "";
            btnDev.Text = "";
            SignalPing.Text = "";
            SignalPing.ImageAlign = ContentAlignment.MiddleLeft;
            btnWhatsapp.Text = "";
            btnWhatsapp.ImageAlign = ContentAlignment.MiddleLeft;
        }

        private async void btnWhatsapp_Click(object sender, EventArgs e)
        {
            using (Whatsapp_Config wa = new())
            {
                QuestionHelper bg = new(null, null, null, null);
                Form background = bg.CreateOverlayForm();

                wa.Owner = background;
                background.Show();
                DialogResult result = wa.ShowDialog();

                background.Dispose();
            }
        }
    }
}