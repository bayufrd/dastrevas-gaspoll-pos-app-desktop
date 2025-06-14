using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using KASIR.Network;
using KASIR.Printer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CheckBox = System.Windows.Forms.CheckBox;
using Color = System.Drawing.Color;
using ComboBox = System.Windows.Forms.ComboBox;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;
using TextBox = System.Windows.Forms.TextBox;

namespace KASIR.Komponen
{
    public partial class SettingsForm : Form
    {
        public Form1 MainForm { get; set; }
        private PrinterModel printerModel;
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet;
        private string configFolderPath = "setting";
        public SettingsForm(Form1 mainForm)
        {
            this.ControlBox = false;
            InitializeComponent();
            MainForm = mainForm;
            LoadConfig();
            InitializeUpdateSettings();
            LoadPrintersAndSettings().ConfigureAwait(false);
            cekUpdate();
        }

        private void InitializeUpdateSettings()
        {
            lblNewVersion.Visible = true;
            lblNewVersionNow.Visible = true;
            btnUpdate.Text = "Repair";
            lblVersion.Text = Properties.Settings.Default.Version.ToString();
            printerModel = new PrinterModel(); // Initialize printerModel
        }
        // Metode untuk memuat pengaturan printer dan checkbox
        public async Task LoadPrintersAndSettings()
        {
            List<ComboBox> comboBoxes = new List<ComboBox> { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };

            foreach (var comboBox in comboBoxes)
            {
                comboBox.Items.Clear();
                comboBox.SelectedIndexChanged -= ComboBoxPrinter_SelectedIndexChanged; // Drum handler pertama

                try
                {
                    var printers = await printerModel.GetAvailablePrinters();
                    comboBox.Items.Add(new PrinterItem("Mac Address Manual", null));
                    comboBox.Items.AddRange(printers.ToArray());

                    // Set printer yang sudah disimpan
                    await SetSavedPrinterAsync(comboBox);
                    // Load the checkbox states
                    await LoadCheckBoxStates(comboBox.Name.Substring(10)); // Mengambil angka dari ComboBox
                }
                catch (Exception ex)
                {
                    HandlePrinterLoadingError(comboBox, ex);
                }
                finally
                {
                    comboBox.SelectedIndexChanged += ComboBoxPrinter_SelectedIndexChanged; // Restore the event handler
                }
            }
        }

        private async Task SetSavedPrinterAsync(ComboBox comboBox)
        {
            string comboBoxName = comboBox.Name.Substring(10); // Mengambil angka dari nama ComboBox
            string savedPrinterId = await printerModel.LoadPrinterSettingsAsync(comboBoxName);

            if (string.IsNullOrEmpty(savedPrinterId))
            {
                comboBox.SelectedIndex = 0; // Select default item
            }
            else
            {
                SetSelectedPrinter(comboBox, savedPrinterId);
                TextBox associatedTextBox = Controls.Find($"txtPrinter{comboBoxName.Last()}", true).FirstOrDefault() as TextBox;
                if (associatedTextBox != null)
                {
                    associatedTextBox.Text = savedPrinterId; // Update associated TextBox
                }
            }
        }

        private void HandlePrinterLoadingError(ComboBox comboBox, Exception ex)
        {
            MessageBox.Show($"Error loading printers for {comboBox.Name}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            comboBox.Items.Add(new PrinterItem("Mac Address Manual", null));
            comboBox.SelectedIndex = 0; // Set default item on error
        }

        public async Task CheckAndUpdateCheckboxStates()
        {
            List<ComboBox> comboBoxes = new List<ComboBox> { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };
            List<TextBox> textBoxes = new List<TextBox> { txtPrinter1, txtPrinter2, txtPrinter3 };

            for (int i = 0; i < comboBoxes.Count; i++)
            {
                var comboBox = comboBoxes[i];
                var textBox = textBoxes[i];
                int printerNumber = i + 1;
                string comboBoxName = $"inter{printerNumber}";

                // Dapatkan daftar checkbox untuk printer ini
                List<CheckBox> checkBoxes = new List<CheckBox>
        {
            Controls.Find($"checkBoxKasirPrinter{printerNumber}", true).FirstOrDefault() as CheckBox,
            Controls.Find($"checkBoxCheckerPrinter{printerNumber}", true).FirstOrDefault() as CheckBox,
            Controls.Find($"checkBoxMakananPrinter{printerNumber}", true).FirstOrDefault() as CheckBox,
            Controls.Find($"checkBoxMinumanPrinter{printerNumber}", true).FirstOrDefault() as CheckBox
        };

                // Filter out null checkboxes
                checkBoxes = checkBoxes.Where(cb => cb != null).ToList();

                // Periksa apakah ada checkbox yang dicentang
                bool anyCheckboxChecked = checkBoxes.Any(cb => cb.Checked);

                // Pengecekan tambahan untuk mencegah error
                if (comboBox == null || textBox == null || checkBoxes.Count == 0)
                {
                    continue; // Lewati iterasi jika ada kontrol yang null
                }

                // Jika ComboBox adalah "Mac Address Manual" dan TextBox kosong
                if (comboBox.Items.Count > 0 &&
                    comboBox.Items[0].ToString() == "Mac Address Manual" &&
                    comboBox.SelectedIndex == 0 &&
                    string.IsNullOrEmpty(textBox.Text))
                {
                    // Uncheck semua checkbox untuk printer ini
                    foreach (var checkBox in checkBoxes)
                    {
                        checkBox.Checked = false;
                        await printerModel.SaveCheckBoxSettingAsync(checkBox.Name, false);
                    }

                    // Hapus pengaturan printer
                    await printerModel.DelPrinterSettings(comboBoxName);
                }
                // Jika tidak ada checkbox yang dicentang, kosongkan TextBox
                else if (!anyCheckboxChecked)
                {
                    textBox.Text = string.Empty;

                    // Cek apakah "Mac Address Manual" ada di ComboBox sebelum mengatur SelectedIndex
                    if (comboBox.Items.Count > 0 &&
                        comboBox.Items[0].ToString() == "Mac Address Manual")
                    {
                        comboBox.SelectedIndex = 0;
                    }

                    // Hapus pengaturan printer
                    await printerModel.DelPrinterSettings(comboBoxName);
                }
                else
                {
                    // Jika ada checkbox yang dicentang, simpan MAC Address atau printer
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        // Simpan MAC Address Bluetooth
                        await printerModel.SavePrinterBluetoothMACSettings(comboBoxName, textBox.Text);
                    }
                    else if (comboBox.SelectedItem is PrinterItem selectedPrinter &&
                             selectedPrinter.ToString() != "Mac Address Manual")
                    {
                        // Simpan printer dari daftar
                        await printerModel.SavePrinterSettings(comboBoxName, selectedPrinter.PrinterId);
                    }

                    // Simpan status checkbox
                    foreach (var checkBox in checkBoxes)
                    {
                        await printerModel.SaveCheckBoxSettingAsync(checkBox.Name, checkBox.Checked);
                    }
                }
            }
        }
        public async Task CleanupPrinterSettings()
        {
            List<ComboBox> comboBoxes = new List<ComboBox> { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };
            List<TextBox> textBoxes = new List<TextBox> { txtPrinter1, txtPrinter2, txtPrinter3 };

            for (int i = 0; i < comboBoxes.Count; i++)
            {
                var comboBox = comboBoxes[i];
                var textBox = textBoxes[i];
                int printerNumber = i + 1;
                string comboBoxName = $"inter{printerNumber}";

                // Dapatkan daftar checkbox untuk printer ini
                List<CheckBox> checkBoxes = new List<CheckBox>
        {
            Controls.Find($"checkBoxKasirPrinter{printerNumber}", true).FirstOrDefault() as CheckBox,
            Controls.Find($"checkBoxCheckerPrinter{printerNumber}", true).FirstOrDefault() as CheckBox,
            Controls.Find($"checkBoxMakananPrinter{printerNumber}", true).FirstOrDefault() as CheckBox,
            Controls.Find($"checkBoxMinumanPrinter{printerNumber}", true).FirstOrDefault() as CheckBox
        };

                // Filter out null checkboxes
                checkBoxes = checkBoxes.Where(cb => cb != null).ToList();

                // Periksa apakah ada checkbox yang dicentang
                bool anyCheckboxChecked = checkBoxes.Any(cb => cb.Checked);

                // Jika ComboBox adalah "Mac Address Manual" dan tidak ada checkbox yang dicentang
                if (comboBox.Items.Count > 0 &&
                    comboBox.Items[0].ToString() == "Mac Address Manual" &&
                    comboBox.SelectedIndex == 0 &&
                    !anyCheckboxChecked)
                {
                    // Kosongkan TextBox
                    textBox.Text = string.Empty;
                }
                // Jika ada checkbox yang dicentang tapi TextBox kosong
                else if (anyCheckboxChecked && string.IsNullOrEmpty(textBox.Text))
                {
                    // Uncheck semua checkbox
                    foreach (var checkBox in checkBoxes)
                    {
                        checkBox.Checked = false;
                        await printerModel.SaveCheckBoxSettingAsync(checkBox.Name, false);
                    }
                }
            }
        }
        private async Task LoadCheckBoxStates(string comboBoxName)
        {
            List<CheckBox> checkBoxes = new List<CheckBox> {
                checkBoxKasirPrinter1,
                checkBoxCheckerPrinter1,
                checkBoxMakananPrinter1,
                checkBoxMinumanPrinter1,
                checkBoxKasirPrinter2,
                checkBoxCheckerPrinter2,
                checkBoxMakananPrinter2,
                checkBoxMinumanPrinter2,
                checkBoxKasirPrinter3,
                checkBoxCheckerPrinter3,
                checkBoxMakananPrinter3,
                checkBoxMinumanPrinter3,
            };

            foreach (var checkBox in checkBoxes)
            {
                string checkBoxName = checkBox.Name;
                bool isChecked = await printerModel.LoadCheckBoxSettingAsync(checkBoxName);
                checkBox.Checked = isChecked;
            }
        }

        private async void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox)sender;
            string checkBoxName = checkBox.Name;
            bool isChecked = checkBox.Checked;

            // Save checkbox state
            await printerModel.SaveCheckBoxSettingAsync(checkBoxName, isChecked);
        }

        private List<CheckBox> FindCheckBoxesByPrefix(string prefix)
        {
            List<CheckBox> checkBoxes = new List<CheckBox> {
                checkBoxKasirPrinter1,
                checkBoxCheckerPrinter1,
                checkBoxMakananPrinter1,
                checkBoxMinumanPrinter1,
                checkBoxKasirPrinter2,
                checkBoxCheckerPrinter2,
                checkBoxMakananPrinter2,
                checkBoxMinumanPrinter2,
                checkBoxKasirPrinter3,
                checkBoxCheckerPrinter3,
                checkBoxMakananPrinter3,
                checkBoxMinumanPrinter3,
            };

            return checkBoxes;
        }

        private async void ComboBoxPrinter_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedPrinter = (PrinterItem)comboBox.SelectedItem;

            if (selectedPrinter.ToString() == "Mac Address Manual")
            {

                string comboBoxName = comboBox.Name.Substring(10); // Mengambil angka dari nama comboBox
                await printerModel.DelPrinterSettings(comboBoxName);

                string numberString = comboBoxName.Substring(5); // Mengambil angka setelah "inter"
                if (int.TryParse(numberString, out int printerNumber))
                {
                    // Bentuk nama TextBox berdasarkan nomor printer
                    string textBoxName = $"txtPrinter{printerNumber}";

                    // Cari kontrol TextBox yang sesuai
                    TextBox textBox = Controls.Find(textBoxName, true).FirstOrDefault() as TextBox;
                    if (textBox != null)
                    {
                        string printerId = textBox.Text;
                        await printerModel.SavePrinterBluetoothMACSettings(comboBoxName, printerId);
                    }
                }
            }
            else if (selectedPrinter != null && selectedPrinter.ToString() != "Mac Address Manual")
            {
                string comboBoxName = comboBox.Name.Substring(10); // Mengambil angka dari nama comboBox

                // Simpan pengaturan printer terpilih
                string printerId = selectedPrinter.PrinterId;
                if (printerId != null && printerId != "Mac Address Manual")
                {
                    await printerModel.SavePrinterSettings(comboBoxName, printerId);
                    string numberString = comboBoxName.Substring(5); // Mengambil angka setelah "inter"

                    if (int.TryParse(numberString, out int printerNumber))
                    {
                        // Bentuk nama TextBox berdasarkan nomor printer
                        string textBoxName = $"txtPrinter{printerNumber}";

                        // Cari kontrol TextBox yang sesuai
                        TextBox textBox = Controls.Find(textBoxName, true).FirstOrDefault() as TextBox;
                        if (textBox != null)
                        {
                            textBox.Text = "";
                        }
                    }
                }
            }
        }

        private void SetSelectedPrinter(ComboBox comboBox, string printerId)
        {
            foreach (PrinterItem item in comboBox.Items)
            {
                if (item.PrinterId == printerId)
                {
                    comboBox.SelectedItem = item;
                    return; // Exit as soon as we find and set the item
                }
            }
            // If no match found, select the default item
            comboBox.SelectedIndex = 0;
        }
        private async Task savingSettings()
        {
            // Assuming ComboBoxPrinter and txtPrinter are arrays or lists containing the ComboBoxes and TextBoxes
            List<ComboBox> comboBoxPrinters = new List<ComboBox> { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };
            List<TextBox> txtPrinters = new List<TextBox> { txtPrinter1, txtPrinter2, txtPrinter3 };

            for (int i = 0; i < comboBoxPrinters.Count; i++) // Loop through 0 to 2 for three printers
            {
                string comboBoxName = "inter" + (i + 1); // Adjust to match naming convention if needed
                if (comboBoxPrinters[i].SelectedItem.ToString() == "Mac Address Manual" && !string.IsNullOrEmpty(txtPrinters[i].Text))
                {
                    string printerId = txtPrinters[i].Text;
                    if (printerModel.IsBluetoothPrinter(txtPrinters[i].Text))
                    {
                        printerId = ConvertMacAddressFormat(txtPrinters[i].Text);
                        await printerModel.SavePrinterBluetoothMACSettings(comboBoxName, printerId);
                    }
                    else
                    {
                        await printerModel.SavePrinterBluetoothMACSettings(comboBoxName, printerId);
                    }
                }
                else
                {
                    var selectedPrinter = (PrinterItem)comboBoxPrinters[i].SelectedItem;

                    // Simpan pengaturan printer terpilih
                    string printerId = selectedPrinter?.PrinterId; // Use null-conditional operator to prevent null reference
                    if (!string.IsNullOrEmpty(printerId) && printerId != "Mac Address Manual")
                    {
                        await printerModel.SavePrinterSettings(comboBoxName, printerId);
                    }
                }
            }
        }
        public string ConvertMacAddressFormat(string macAddress)
        {
            // Use Regex to replace all colons with hyphens
            return Regex.Replace(macAddress, ":", "-");
        }

        public async Task SaveFooterStruk(string data)
        {
            await File.WriteAllTextAsync("setting\\FooterStruk.data", data);
        }

        public async Task SaveRunningText(string data)
        {
            await File.WriteAllTextAsync("setting\\RunningText.data", data);
        }
        private async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                await CheckAndUpdateCheckboxStates();

                await savingSettings();
                if (!string.IsNullOrEmpty(txtFooter.Text))
                {
                    string data = txtFooter.Text;
                    await SaveFooterStruk(data);
                }
                if (!string.IsNullOrEmpty(txtRunningText.Text))
                {
                    string data = txtRunningText.Text;
                    await SaveRunningText(data);
                }
                string allSettingsData = await File.ReadAllTextAsync("setting\\configDualMonitor.data");
                if (allSettingsData == "ON")
                {
                    radioDualMonitor.Checked = false;
                    Thread.Sleep(3000);
                    radioDualMonitor.Checked = true;
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Close the form with DialogResult.Cancel
            DialogResult = DialogResult.Cancel;
            Close();
        }



        private bool IsInternetConnected()
        {
            try
            {
                // Ping a well-known server, such as Google's public DNS server (8.8.8.8)
                Ping ping = new Ping();
                PingReply reply = ping.Send("8.8.8.8", 1000); // 1000 milliseconds timeout

                return reply != null && reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                System.Windows.Forms.MessageBox.Show("Koneksi bermasalah", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogWarning("No koneksi");

                // Handle exception (no internet connection or other issues)
                return false;
            }
        }

        private async Task cekUpdateAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var urlVersion = Properties.Settings.Default.BaseAddressVersion.ToString();
                    var newVersion = await httpClient.GetStringAsync(urlVersion);
                    string currentVersion = Properties.Settings.Default.Version.ToString();

                    newVersion = newVersion.Replace(".", "");
                    currentVersion = currentVersion.Replace(".", "");

                    // Fetch the version string for display
                    var displayVersion = await httpClient.GetStringAsync(urlVersion);

                    if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
                    {
                        UpdateUIForNewVersion(displayVersion, "Update");
                    }
                    else
                    {
                        UpdateUIForNewVersion(displayVersion, "Fix");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void UpdateUIForNewVersion(string versionText, string buttonText)
        {
            lblNewVersion.Visible = true;
            lblNewVersionNow.Visible = true;
            lblNewVersionNow.Text = versionText;
            btnUpdate.Text = buttonText;
        }

        // If you need to call this from an event handler that expects void
        private async void cekUpdate()
        {
            await cekUpdateAsync();
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
        public async void btnUpdate_Click(object sender, EventArgs e)
        {
            string filePath = $"DT-Cache\\DataOutlet{baseOutlet}.data";
            string outletName = await GetOrCreateOutletName(filePath);
            Util n = new Util();
            n.sendLogTelegramNetworkError("Open Updater Manual" + outletName);
            btnUpdate.Enabled = false;

            try
            {
                await StartUpdaterProcess();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async Task<string> GetOrCreateOutletName(string filePath)
        {
            if (File.Exists(filePath))
            {
                return GetOutletNameFromFile(filePath);
            }
            string outletName = await GetOutletNameFromApi();
            if (outletName != null)
            {
                // Simpan data ke file jika berhasil mendapatkan nama outlet
                await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(new { data = new { name = outletName } }));
                return outletName;
            }
            return " (Hybrid)";
        }

        private async Task StartUpdaterProcess()
        {
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "update\\update.exe"))
            {
                if (IsInternetConnected())
                {
                    LoggerUtil.LogWarning("Open Update.exe");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = System.Windows.Forms.Application.StartupPath + "update\\update.exe",
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        Verb = "runas"
                    });
                    System.Windows.Forms.Application.Exit();
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("File tidak ditemukan", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogWarning("File not found");
            }
        }

        private async void TestPrinter(object sender, EventArgs e)
        {
            try
            {
                if (printerModel != null)
                {
                    await printerModel.SelectAndPrintAsync();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Gagal test " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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
        private async void radioDualMonitor_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string allSettingsData = await File.ReadAllTextAsync("setting\\configDualMonitor.data");

                if (radioDualMonitor.Checked == true && allSettingsData == "OFF")
                {
                    await OpenDualMonitor();
                }
                if (radioDualMonitor.Checked == false && allSettingsData == "ON")
                {
                    string data = "OFF";
                    await File.WriteAllTextAsync("setting\\configDualMonitor.data", data);
                    /*DeathTimeBegin();*/
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}");
            }
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                MessageBox.Show("Mematikan Tampilan Dual Monitor");

                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName.Equals("KASIR Dual Monitor.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                        break;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void Redownload_Click(object sender, EventArgs e)
        {
            DialogResult yakin = System.Windows.Forms.MessageBox.Show("Reset / Redownload Data lokal Cache ?", "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (yakin != DialogResult.Yes)
            {
                return;
            }
            else
            {
                string TypeCacheEksekusi = "Reset";
                CacheDataApp form3 = new CacheDataApp(TypeCacheEksekusi);
                this.Close();
                form3.Show();
            }
        }


        private void UpdateInfo_Click(object sender, EventArgs e)
        {


            UpdateInformation u = new UpdateInformation();
            this.Close();

            u.Show();

        }



        private void CacheApp_Click(object sender, EventArgs e)
        {
            try
            {
                string TypeCacheEksekusi = "Sync";
                CacheDataApp form3 = new CacheDataApp(TypeCacheEksekusi);
                this.Close();
                form3.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Gagal Download Data : {ex}");
            }
        }
        public async Task LoadConfig()
        {
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }

            await EnsureFileExistsAsync("setting\\FooterStruk.data", "TERIMAKASIH ATAS KUNJUNGANNYA", txtFooter);
            await EnsureFileExistsAsync("setting\\RunningText.data", "TERIMAKASIH ATAS KUNJUNGANNYA", txtRunningText);
            await EnsureFileExistsAsync("setting\\configListMenu.data", "OFF", sButtonListMenu);
            await EnsureFileExistsAsync("setting\\configDualMonitor.data", "OFF", radioDualMonitor);
            await EnsureFileExistsAsync("setting\\OfflineMode.data", "OFF", sButtonOffline);
        }

        private async Task EnsureFileExistsAsync(string filePath, string defaultContent, Control controlToUpdate)
        {
            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, defaultContent);
            }
            else
            {
                string content = await File.ReadAllTextAsync(filePath);
                if (!string.IsNullOrEmpty(content))
                {
                    if (controlToUpdate is TextBox textBox)
                    {
                        textBox.Text = content;
                    }
                    else if (controlToUpdate is CheckBox checkBox)
                    {
                        checkBox.Checked = content == "ON";
                    }
                    else if (controlToUpdate is RadioButton radioButton)
                    {
                        radioButton.Checked = content == "ON";
                    }
                    // Tambah jenis kontrol lain jika diperlukan
                }
            }
        }
        private async void sButtonListMenu_CheckedChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }
            if (sButtonListMenu.Checked == true)
            {
                string data = "ON";
                await File.WriteAllTextAsync("setting\\configListMenu.data", data);
            }
            else
            {
                string data = "OFF";
                await File.WriteAllTextAsync("setting\\configListMenu.data", data);
            }
        }

        private void txtPrinter1_TextChanged(object sender, EventArgs e)
        {
            if (System.Net.IPAddress.TryParse(txtPrinter1.Text, out _))
            {
                // Connect via LAN
                lblBluetooth1.Text = "IP Address Detected";
            }
            else
            {
                lblBluetooth1.Text = "Mac Address Bluetooth";
            }
        }

        private void txtPrinter2_TextChanged(object sender, EventArgs e)
        {
            if (System.Net.IPAddress.TryParse(txtPrinter2.Text, out _))
            {
                // Connect via LAN
                lblBluetooth2.Text = "IP Address Detected";
            }
            else
            {
                lblBluetooth2.Text = "Mac Address Bluetooth";
            }
        }

        private void txtPrinter3_TextChanged(object sender, EventArgs e)
        {
            if (System.Net.IPAddress.TryParse(txtPrinter3.Text, out _))
            {
                // Connect via LAN
                lblBluetooth3.Text = "IP Address Detected";
            }
            else
            {
                lblBluetooth3.Text = "Mac Address Bluetooth";
            }
        }

        private void iconDual_Click(object sender, EventArgs e)
        {

            SettingsDual u = new SettingsDual();

            this.Close();

            u.Show();
        }
        public static void OpenFolder(string folderPath)
        {
            if (!System.IO.Directory.Exists(folderPath))
            {
                MessageBox.Show("Folder tidak ditemukan.");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true, // Perlu diaktifkan untuk menggunakan shell default sistem operasi
                Verb = "open"
            });
        }
        private void iconButton1_Click(object sender, EventArgs e)
        {
            Form passwordForm = new Form
            {
                Text = "Autentikasi",
                Width = 300,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent, // Center relative to parent
                MaximizeBox = false,
                TopMost = true,
                Owner = this // Set the owner to establish parent-child relationship
            };

            Label lblStatus = new Label
            {
                Text = "Masukkan Password:",
                Location = new Point(20, 20), // Sekarang menggunakan System.Drawing.Point
                Width = 260,
                TextAlign = ContentAlignment.MiddleLeft
            };

            TextBox txtPassword = new TextBox
            {
                Location = new Point(20, 50), // Sekarang menggunakan System.Drawing.Point
                Width = 260,
                UseSystemPasswordChar = true
            };

            Button btnSubmit = new Button
            {
                Text = "Submit",
                Location = new Point(100, 100),
                DialogResult = DialogResult.OK
            };

            Label lblError = new Label
            {
                Text = "",
                Location = new Point(20, 130),
                Width = 260,
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Close();
            //this.Hide(); // Hide rather than close immediately
            btnSubmit.Click += (s, ev) =>
            {
                if (txtPassword.Text == "GMC1234")
                {
                    passwordForm.DialogResult = DialogResult.OK;
                    passwordForm.Close();
                }
                else
                {
                    lblError.Text = "Password Salah!";
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            };

            passwordForm.Controls.Add(lblStatus);
            passwordForm.Controls.Add(txtPassword);
            passwordForm.Controls.Add(btnSubmit);
            passwordForm.Controls.Add(lblError);
            passwordForm.AcceptButton = btnSubmit;

            if (passwordForm.ShowDialog() == DialogResult.OK)
            {
                SettingsConfig u = new SettingsConfig();
                u.Show();
            }
        }

        private async void sButtonOffline_CheckedChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }
            string data, Config = "setting\\OfflineMode.data";
            string allSettingsData = await File.ReadAllTextAsync(Config);

            if (sButtonOffline.Checked == true && allSettingsData == "OFF")
            {
                data = "ON";
                await File.WriteAllTextAsync(Config, data);

                // Memanggil UpdateContent pada Form1 yang sudah ada
                MainForm.UpdateContent(); // Mengupdate konten pada Form1 yang sudah ada

                string TypeCacheEksekusi = "Sync";
                CacheDataApp form3 = new CacheDataApp(TypeCacheEksekusi);
                this.Close();
                form3.Show();
            }

            if (sButtonOffline.Checked == false && allSettingsData == "ON")
            {
                data = "OFF";
                await File.WriteAllTextAsync(Config, data);

                MainForm.UpdateContent(); // Mengupdate konten pada Form1 yang sudah ada
                this.Close();

            }
        }

    }
}
