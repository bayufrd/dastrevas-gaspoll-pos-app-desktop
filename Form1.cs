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
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;
using KASIR.Network;
using Newtonsoft.Json;
using System.Windows.Forms.Design;
using Menu = KASIR.Model.Menu;
using System.Drawing;
using System.Windows.Documents;
using Path = System.IO.Path;
using System.Xml;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Windows.Markup;
using System.Windows.Media;
using DrawingColor = System.Drawing.Color;
using Color = System.Drawing.Color;




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
            headerOutletName("");
            panel2.Controls.Add(leftBorderBtn);
            masterPos m = new masterPos();
            m.TopLevel = false;
            m.Dock = DockStyle.Fill;
            panel1.Controls.Add(m);
            m.BringToFront();
            m.Show();
            this.Height += 100;
            button2.Visible = true;
            lblDetail.Visible = false;
            progressBar.Visible = false;
            StarterApp();
            //checkCacheData();
            initPingTest();
            //headerName();

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
                string TypeCacheEksekusi = "Sync";
                await Task.Delay(500);

                CacheDataApp form3 = new CacheDataApp(TypeCacheEksekusi);

                // Show the new form before closing the current one
                form3.LoadData(TypeCacheEksekusi);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Gagal Download Data : {ex}");
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
            await Task.Run(async () =>
            {
                await DualMonitorChecker();
                await cekLastUpdaterApp();
                await cekVersionAndData();
                //await cekCacheData();
                await checkCacheData();
                await headerName();

            });
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
        public static void DuplicateAndRenameToOutletLogo(string file2Path)
        {
            // Path untuk OutletLogo.bmp
            string outletLogoPath = "icon\\OutletLogo.bmp";
            string tempCopyPath = "icon\\TempCopy.bmp";

            try
            {
                // Cek apakah file OutletLogo.bmp sudah ada
                if (File.Exists(outletLogoPath))
                {
                    // Hapus file OutletLogo.bmp jika sudah ada
                    File.Delete(outletLogoPath);
                }

                // Duplikasi file original (file2) ke file sementara (TempCopy.bmp)
                File.Copy(file2Path, tempCopyPath);

                // Rename TempCopy.bmp menjadi OutletLogo.bmp
                File.Move(tempCopyPath, outletLogoPath);
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

        static void YourFunctionToAddNewID()
        {
            // Fungsi Anda untuk menangani penambahan ID baru yang ditemukan di API
            // Anda bisa menambahkan log, pembaruan, atau tindakan lain sesuai kebutuhan
            string msg = "Ada Penambahan atau Perubahan Item Menu! \n\nSinkronasi Menu Sekarang?";
            MessageBoxIcon icon = MessageBoxIcon.Information;
            DialogResult result = MessageBox.Show(msg, "PEMBAHARUAN! - GASPOL | DTCoding", MessageBoxButtons.YesNo, icon);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string TypeCacheEksekusi = "Sync";
                    CacheDataApp sinkronasi = new CacheDataApp(TypeCacheEksekusi);
                    sinkronasi.Show();
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                // User clicked "No", so the program will not continue.
                // Add your code here.
            }
        }


        static string FindLinkFolderBeforeStartupPath(string startupPath, string linkFolderName)
        {
            string currentPath = Path.GetDirectoryName(startupPath);

            while (currentPath != null)
            {
                string[] directories = Directory.GetDirectories(currentPath, linkFolderName, SearchOption.AllDirectories);

                if (directories.Length > 0)
                {
                    return directories[0];
                }

                currentPath = Path.GetDirectoryName(currentPath);
            }

            return null;
        }
        private async Task cekVersionAndData()
        {
            try
            {
                await headerOutletName("Checking Kasir Version...");

                var urlVersion = Properties.Settings.Default.BaseAddressVersion.ToString();
                var newVersion = (new WebClient().DownloadString(urlVersion));
                string currentVersion = Properties.Settings.Default.Version.ToString();
                await headerOutletName($"Current Version is {currentVersion}, New is {newVersion}");

                newVersion = newVersion.Replace(".", "");
                currentVersion = currentVersion.Replace(".", "");


                if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
                {
                    urlVersion = Properties.Settings.Default.BaseAddressVersion.ToString();
                    newVersion = (new WebClient().DownloadString(urlVersion));
                    currentVersion = Properties.Settings.Default.Version.ToString();

                    Util n = new Util();
                    n.sendLogTelegramNetworkError("Open Updater");
                    await headerOutletName("Opening Kasir Updater...");

                    OpenUpdaterExe();
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }

        }

        private async void OpenUpdaterExe()
        {
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "update\\update.exe"))
            {
                DeathTimeBegin();
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

        private async Task headerName()
        {
            try
            {
                IApiService apiService = new ApiService();

                string response = await apiService.CekShift("/shift?outlet_id=" + baseOutlet);
                if (response != null)
                {
                    GetShift cekShift = JsonConvert.DeserializeObject<GetShift>(response);

                    DataShift datas = cekShift.data;
                    lblNamaOutlet.Text = datas.outlet_name.ToString();
                    baseOutletName = lblNamaOutlet.Text.ToString();
                }
                else
                {
                    lblNamaOutlet.Text = namaOutlet.ToString().Replace("&", "&&");
                    baseOutletName = lblNamaOutlet.Text.ToString();

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

        private void button6_Click(object sender, EventArgs e)
        {
            Color randomColor = PickRandomColor();
            ActivateButton(sender, randomColor);
            try
            {
                masterPos m = new masterPos();
                if (m == null)
                {
                    MessageBox.Show("Terjadi kesalahan cek koneksi anda", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                m.TopLevel = false;
                m.Dock = DockStyle.Fill;
                panel1.Controls.Add(m);
                m.BringToFront();
                m.Show();
                lblTitleChildForm.Text = "Menu - Semua Transaksi";
                // Misalkan 'obj' adalah objek yang mungkin null



                // Lakukan operasi dengan 'obj'
                // ...
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Log error jika diperlukan
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
        private void button1_Click(object sender, EventArgs e)
        {
            Color randomColor = PickRandomColor();
            ActivateButton(sender, randomColor);

            try
            {
                successTransaction c = new successTransaction();

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
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Log error jika diperlukan
            }
            //====by


            /* inputPin payForm = new inputPin();
             background.Show();
             payForm.Owner = background;
             payForm.ShowDialog();
             background.Dispose();*/
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

            await DeathTimeBegin();
            Application.Exit();
        }
        private async Task DeathTimeBegin()
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName.Equals("KASIR Dual Monitor", StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"{ex}");
            }
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
            using (SettingsForm settingsForm = new SettingsForm())
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
            await CheckConnection();
        }

        private async Task CheckConnection()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface firstInterface = interfaces.FirstOrDefault(i => i.OperationalStatus == OperationalStatus.Up);

            if (firstInterface == null)
            {
                UpdatePingLabel(Color.Red, "Tidak ada akses \nInternet");
                return;
            }
            string server = "www.google.com"; // Ubah ke server Anda jika diperlukan, misalnya "yourserver.com"

            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply reply = await ping.SendPingAsync(server, 1000);

                    if (reply.Status == IPStatus.Success)
                    {
                        if (reply.RoundtripTime <= 50)
                        {
                            UpdatePingLabel(Color.Lime, $"{reply.RoundtripTime} ms");
                        }
                        else if (reply.RoundtripTime <= 100)
                        {
                            UpdatePingLabel(Color.Yellow, $"{reply.RoundtripTime} ms");
                        }
                        else
                        {
                            UpdatePingLabel(Color.Red, $"{reply.RoundtripTime} ms");
                        }
                    }
                    else
                    {
                        UpdatePingLabel(Color.Red, "Ping Gagal :(");
                    }
                }
                catch (PingException ex)
                {
                    // Log error here if needed
                    UpdatePingLabel(Color.Red, "Tidak ada akses \nInternet");
                }
            }
        }
        private async void btnTestSpeed_Click(object sender, EventArgs e)
        {
            SignalPing.Text = "Testing...";
            SignalPing.ForeColor = Color.White; // Default warna saat testing

            try
            {
                //string baseAddress = Properties.Settings.Default.BaseAddressDev.ToString();
                // 1. Uji kecepatan internet
                //double speed = await TestInternetSpeed();
                //SignalPing.Text = $"{speed:0.00} Mbps";
                //UpdateSpeedColor(speed); // Perbarui warna berdasarkan hasil kecepatan

                // 2. Tunggu 5 detik, lalu lakukan ping
                //await Task.Delay(5000);

                int ping = await TestPing("8.8.8.8"); // Ping ke Google DNS
                UpdatePingColor(ping); // Perbarui warna berdasarkan nilai ping

                SignalPing.Text = $"{ping} ms";
                await Task.Delay(2000);
                UpdatePingLabel(Color.White, "Test Ping");

            }
            catch (Exception ex)
            {
                UpdatePingLabel(Color.Red, $"Error: {ex.Message}");
            }
        }

        private async Task<double> TestInternetSpeed()
        {
            string[] urls = new string[]
            {
        "http://speedtest.tele2.net/10MB.zip",
        "http://ipv4.download.thinkbroadband.com/10MB.zip",
        "http://speed.cloudflare.com/__down?bytes=10000000"
            };

            HttpClient client = new HttpClient();
            byte[] data = null;
            Stopwatch stopwatch = new Stopwatch();

            foreach (var url in urls)
            {
                try
                {
                    stopwatch.Start();
                    data = await client.GetByteArrayAsync(url);
                    stopwatch.Stop();
                    break; // Keluar dari loop jika berhasil mengunduh
                }
                catch (Exception)
                {
                    stopwatch.Reset();
                }
            }

            if (data == null)
            {
                throw new Exception("Failed to download file from all URLs.");
            }

            double timeTakenInSeconds = stopwatch.Elapsed.TotalSeconds;
            double fileSizeInBits = data.Length * 8;
            double speedInBps = fileSizeInBits / timeTakenInSeconds;
            double speedInMbps = speedInBps / (1024 * 1024);

            return speedInMbps;
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

        private void UpdateSpeedColor(double speed)
        {
            if (speed > 50) // Jika speed di atas 50 Mbps, warna hijau
            {
                UpdatePingLabel(Color.Green, $"{speed:0.00} Mbps - Good");
            }
            else if (speed >= 10 && speed <= 50) // Jika speed antara 10 dan 50 Mbps, warna kuning
            {
                UpdatePingLabel(Color.Yellow, $"{speed:0.00} Mbps - Moderate");
            }
            else // Jika speed di bawah 10 Mbps, warna merah
            {
                UpdatePingLabel(Color.Red, $"{speed:0.00} Mbps - Poor");
            }
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

        private void btnShiftLaporan_Click(object sender, EventArgs e)
        {

            //====by
            ActivateButton(sender, RGBColors.color4);
            try
            {
                shiftReport c = new shiftReport();

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
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Log error jika diperlukan
            }

        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            //settingsButton
            //====by
            ActivateButton(sender, RGBColors.color4);
            try
            {
                SettingsForm c = new SettingsForm();

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
            }
        }
    }
}