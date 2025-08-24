using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Timers;
using KASIR.Helper;
using KASIR.Model;
using KASIR.Network;
using KASIR.OfflineMode;
using KASIR.Printer;
using KASIR.Properties;
using KASIR.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Application = System.Windows.Forms.Application;
using CheckBox = System.Windows.Forms.CheckBox;
using Color = System.Drawing.Color;
using ComboBox = System.Windows.Forms.ComboBox;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Drawing.Point;
using TextBox = System.Windows.Forms.TextBox;
namespace KASIR.Komponen
{
    public partial class Offline_settingsForm : UserControl
    {
        private readonly string baseOutlet = Settings.Default.BaseOutlet;
        private readonly string configFolderPath = "setting";
        private PrinterModel printerModel;
        private readonly InternetService _internetService;
        private readonly ImageUploadHelper _imageUploadHelper;
        private readonly UpdaterHelper _updaterHelper;

        public Offline_settingsForm()
        {
            //ControlBox = false;
            InitializeComponent();
            _imageUploadHelper = new ImageUploadHelper();

            //SetAllLabelsToBlack();
            SetAllControlStyles(
                labelForeColor: Color.Black,
                labelBackColor: Color.Transparent,
                labelFont: new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                checkBoxForeColor: Color.Black,
                checkBoxBackColor: Color.Transparent,
                checkBoxFont: new Font("Segoe UI", 9F, FontStyle.Regular),
                textBoxForeColor: Color.Black,
                textBoxBackColor: Color.White,
                textBoxFont: new Font("Segoe UI", 9F, FontStyle.Regular)
            );

            Offline_masterPos m = new();
            m.RoundedPanel(LogPanel);
            _internetService = new InternetService();
            _updaterHelper = new UpdaterHelper(_internetService);

            _ = LoadConfig();
            InitializeUpdateSettings();
            _ = LoadPrintersAndSettings().ConfigureAwait(false);
            cekUpdate();
            _ = loadLogo();
        }
        private void SetAllLabelsToBlack(Control container = null)
        {
            // If no container is provided, use the current form/control
            if (container == null)
            {
                container = this;
            }

            // Recursively find and modify all labels
            foreach (Control control in container.Controls)
            {
                // Check if the control is a Label
                if (control is Label label)
                {
                    // Set label properties
                    label.ForeColor = Color.Black;
                    label.BackColor = Color.Transparent;
                }

                // Recursively check child controls
                if (control.HasChildren)
                {
                    SetAllLabelsToBlack(control);
                }
            }
        }

        private void SetAllControlStyles(
    Color? labelForeColor = null,
    Color? labelBackColor = null,
    Font? labelFont = null,
    Color? checkBoxForeColor = null,
    Color? checkBoxBackColor = null,
    Font? checkBoxFont = null,
    Color? textBoxForeColor = null,        // Add this line
    Color? textBoxBackColor = null,        // Add this line
    Font? textBoxFont = null)              // Add this line
        {
            // Default values if not provided
            Color defaultLabelForeColor = labelForeColor ?? Color.Black;
            Color defaultLabelBackColor = labelBackColor ?? Color.Transparent;
            Font defaultLabelFont = labelFont ?? new Font("Segoe UI Semibold", 9F, FontStyle.Bold);

            Color defaultCheckBoxForeColor = checkBoxForeColor ?? Color.Black;
            Color defaultCheckBoxBackColor = checkBoxBackColor ?? Color.Transparent;
            Font defaultCheckBoxFont = checkBoxFont ?? new Font("Segoe UI", 9F, FontStyle.Regular);

            // Add default values for TextBox
            Color defaultTextBoxForeColor = textBoxForeColor ?? Color.Black;
            Color defaultTextBoxBackColor = textBoxBackColor ?? Color.White;
            Font defaultTextBoxFont = textBoxFont ?? new Font("Segoe UI", 9F, FontStyle.Regular);

            void ProcessControl(Control container)
            {
                foreach (Control control in container.Controls)
                {
                    // Style Labels
                    if (control is Label label)
                    {
                        label.ForeColor = defaultLabelForeColor;
                        label.BackColor = defaultLabelBackColor;
                        label.Font = defaultLabelFont;
                    }

                    // Style Checkboxes
                    if (control is CheckBox checkBox)
                    {
                        checkBox.ForeColor = defaultCheckBoxForeColor;
                        checkBox.BackColor = defaultCheckBoxBackColor;
                        checkBox.Font = defaultCheckBoxFont;
                    }

                    // Style TextBoxes
                    if (control is TextBox textBox)
                    {
                        textBox.ForeColor = defaultTextBoxForeColor;
                        textBox.BackColor = defaultTextBoxBackColor;
                        textBox.Font = defaultTextBoxFont;
                    }

                    // Recursively process child controls
                    if (control.HasChildren)
                    {
                        ProcessControl(control);
                    }
                }
            }

            ProcessControl(this);
        }

        private void SetAllLabelsToBlack(Control container, Color? foreColor = null, Color? backColor = null, Font? font = null)
        {
            // Default values if not provided
            Color labelForeColor = foreColor ?? Color.Black;
            Color labelBackColor = backColor ?? Color.Transparent;
            Font labelFont = font ?? new Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);

            foreach (Control control in container.Controls)
            {
                if (control is Label label)
                {
                    label.ForeColor = labelForeColor;
                    label.BackColor = labelBackColor;
                    label.Font = labelFont;
                }

                // Recursively check child controls
                if (control.HasChildren)
                {
                    SetAllLabelsToBlack(control, foreColor, backColor, font);
                }
            }
        }

        private void InitializeUpdateSettings()
        {
            lblNewVersion.Visible = true;
            lblNewVersionNow.Visible = true;
            btnUpdate.Text = "Repair";
            lblVersion.Text = Settings.Default.Version;
            printerModel = new PrinterModel(); // Initialize printerModel
        }

        // Metode untuk memuat pengaturan printer dan checkbox
        public async Task LoadPrintersAndSettings()
        {
            List<ComboBox> comboBoxes = new() { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };

            foreach (ComboBox comboBox in comboBoxes)
            {
                comboBox.Items.Clear();
                comboBox.SelectedIndexChanged -= ComboBoxPrinter_SelectedIndexChanged; // Drum handler pertama

                try
                {
                    List<PrinterItem> printers = await printerModel.GetAvailablePrinters();
                    _ = comboBox.Items.Add(new PrinterItem("Mac Address Manual", null));
                    comboBox.Items.AddRange(printers.ToArray());

                    // Set printer yang sudah disimpan
                    await SetSavedPrinterAsync(comboBox);
                    // Load the checkbox states
                    await LoadCheckBoxStates(comboBox.Name.Substring(10)); // Mengambil angka dari ComboBox
                }
                catch (Exception ex)
                {
                    HandlePrinterLoadingError(comboBox, ex);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
                finally
                {
                    comboBox.SelectedIndexChanged += ComboBoxPrinter_SelectedIndexChanged; // Restore the event handler
                }
            }
        }

        private async Task SetSavedPrinterAsync(ComboBox comboBox)
        {
            try
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
                    TextBox associatedTextBox =
                        Controls.Find($"txtPrinter{comboBoxName.Last()}", true).FirstOrDefault() as TextBox;
                    if (associatedTextBox != null)
                    {
                        associatedTextBox.Text = savedPrinterId; // Update associated TextBox
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void HandlePrinterLoadingError(ComboBox comboBox, Exception ex)
        {
            NotifyHelper.Error($"Error loading printers for {comboBox.Name}: {ex.Message}");
            LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            _ = comboBox.Items.Add(new PrinterItem("Mac Address Manual", null));
            comboBox.SelectedIndex = 0; // Set default item on error
        }

        public async Task CheckAndUpdateCheckboxStates()
        {
            try
            {
                List<ComboBox> comboBoxes = new() { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };
                List<TextBox> textBoxes = new() { txtPrinter1, txtPrinter2, txtPrinter3 };

                for (int i = 0; i < comboBoxes.Count; i++)
                {
                    ComboBox? comboBox = comboBoxes[i];
                    TextBox? textBox = textBoxes[i];
                    int printerNumber = i + 1;
                    string comboBoxName = $"inter{printerNumber}";

                    // Dapatkan daftar checkbox untuk printer ini
                    List<CheckBox> checkBoxes = new()
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
                        foreach (CheckBox checkBox in checkBoxes)
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
                        foreach (CheckBox checkBox in checkBoxes)
                        {
                            await printerModel.SaveCheckBoxSettingAsync(checkBox.Name, checkBox.Checked);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        public async Task CleanupPrinterSettings()
        {
            try
            {
                List<ComboBox> comboBoxes = new() { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };
                List<TextBox> textBoxes = new() { txtPrinter1, txtPrinter2, txtPrinter3 };

                for (int i = 0; i < comboBoxes.Count; i++)
                {
                    ComboBox comboBox = comboBoxes[i];
                    TextBox textBox = textBoxes[i];
                    int printerNumber = i + 1;
                    string comboBoxName = $"inter{printerNumber}";

                    // Dapatkan daftar checkbox untuk printer ini
                    List<CheckBox> checkBoxes = new()
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
                        foreach (CheckBox checkBox in checkBoxes)
                        {
                            checkBox.Checked = false;
                            await printerModel.SaveCheckBoxSettingAsync(checkBox.Name, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        private async Task LoadCheckBoxStates(string comboBoxName)
        {
            try
            {
                List<CheckBox> checkBoxes = new()
            {
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
                checkBoxMinumanPrinter3
            };

                foreach (CheckBox checkBox in checkBoxes)
                {
                    string checkBoxName = checkBox.Name;
                    bool isChecked = await printerModel.LoadCheckBoxSettingAsync(checkBoxName);
                    checkBox.Checked = isChecked;
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        private async void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox checkBox = (CheckBox)sender;
                string checkBoxName = checkBox.Name;
                bool isChecked = checkBox.Checked;

                // Save checkbox state
                await printerModel.SaveCheckBoxSettingAsync(checkBoxName, isChecked);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        private List<CheckBox> FindCheckBoxesByPrefix(string prefix)
        {
            List<CheckBox> checkBoxes = new()
            {
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
                checkBoxMinumanPrinter3
            };

            return checkBoxes;

        }

        private async void ComboBoxPrinter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                PrinterItem? selectedPrinter = (PrinterItem)comboBox.SelectedItem;

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
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        private void SetSelectedPrinter(ComboBox comboBox, string printerId)
        {
            try
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
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }


        private async Task savingSettings()
        {
            try
            {
                // Assuming ComboBoxPrinter and txtPrinter are arrays or lists containing the ComboBoxes and TextBoxes
                List<ComboBox> comboBoxPrinters = new() { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };
                List<TextBox> txtPrinters = new() { txtPrinter1, txtPrinter2, txtPrinter3 };

                for (int i = 0; i < comboBoxPrinters.Count; i++) // Loop through 0 to 2 for three printers
                {
                    string comboBoxName = "inter" + (i + 1); // Adjust to match naming convention if needed
                    if (comboBoxPrinters[i].SelectedItem.ToString() == "Mac Address Manual" &&
                        !string.IsNullOrEmpty(txtPrinters[i].Text))
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
                        PrinterItem? selectedPrinter = (PrinterItem)comboBoxPrinters[i].SelectedItem;

                        // Simpan pengaturan printer terpilih
                        string
                            printerId = selectedPrinter
                                ?.PrinterId; // Use null-conditional operator to prevent null reference
                        if (!string.IsNullOrEmpty(printerId) && printerId != "Mac Address Manual")
                        {
                            await printerModel.SavePrinterSettings(comboBoxName, printerId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

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
                NotifyHelper.Success("Config terbaru diSimpan.");
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                NotifyHelper.Error($"{ex}");

            }
        }

        private bool IsInternetConnected()
        {
            try
            {
                // Ping a well-known server, such as Google's public DNS server (8.8.8.8)
                Ping ping = new();
                PingReply reply = ping.Send("8.8.8.8", 1000); // 1000 milliseconds timeout

                return reply != null && reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                NotifyHelper.Error("Koneksi bermasalah");
                LoggerUtil.LogWarning("No koneksi");

                // Handle exception (no internet connection or other issues)
                return false;
            }
        }

        private async Task cekUpdateAsync()
        {
            try
            {
                string versinew = await _updaterHelper.CheckVersionNewAppAsync();
                string currentVersion = Settings.Default.Version;

                string newVersion = versinew.Replace(".", "");
                currentVersion = currentVersion.Replace(".", "");

                // Fetch the version string for display
                string displayVersion = newVersion;

                if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
                {
                    UpdateUIForNewVersion(displayVersion, "Update");
                }
                else
                {
                    UpdateUIForNewVersion(displayVersion, "Fix");
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

        private async void cekUpdate()
        {
            if (!_internetService.IsInternetConnected())
            {
                lblNewVersionNow.Text = "No Internet";
                lblVersionNow.Text = "No Internet";
                return;
            }
            await cekUpdateAsync();
        }

        public string GetOutletNameFromFile(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            JObject? outletData = JsonConvert.DeserializeObject<JObject>(fileContent);
            return outletData["data"]?["name"]?.ToString();
        }

        public async Task<string> GetOutletNameFromApi()
        {
            IApiService apiService = new ApiService();
            string response = await apiService.CekShift("/outlet/" + baseOutlet);
            if (response != null)
            {
                JObject? apiResponse = JsonConvert.DeserializeObject<JObject>(response);
                File.WriteAllText($"DT-Cache\\DataOutlet{baseOutlet}.data", JsonConvert.SerializeObject(response));

                return apiResponse["data"]?["name"]?.ToString();
            }

            return null;
        }

        public async void btnUpdate_Click(object sender, EventArgs e)
        {
            //---------------- Question Begin -----------------\\
            string titleHelper = "Perbaiki / Update Manual";
            string msgHelper = "Perbaiki / Update Manual Aplikasi ?";
            string cancelHelper = "Batal";
            string okHelper = "Update";
            QuestionHelper c = new(titleHelper, msgHelper, cancelHelper, okHelper);
            Form background = c.CreateOverlayForm();

            c.Owner = background;

            background.Show();

            DialogResult dialogResult = c.ShowDialog();
            if (dialogResult != DialogResult.OK)
            {
                return;
            }
            else
            {


                //--------------- Question Result -------------------\\

                string filePath = $"DT-Cache\\DataOutlet{baseOutlet}.data";
                string outletName = await GetOrCreateOutletName(filePath);
                Util n = new();
                n.sendLogTelegramNetworkError("Open Updater Manual" + outletName);
                btnUpdate.Enabled = false;

                try
                {
                    string oldUrl = Properties.Settings.Default.BaseAddressProd.ToString();
                    string urlVersion = RemoveApiPrefix(oldUrl);
                    Form1 d = new Form1();
                    await d.OpenUpdaterAsync(lblVersion.Text, urlVersion, lblNewVersionNow.Text);
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
        }
        private string RemoveApiPrefix(string url)
        {
            return url.Contains("api.") ? url.Replace("api.", "") : url;
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
                await File.WriteAllTextAsync(filePath,
                    JsonConvert.SerializeObject(new { data = new { name = outletName } }));
                return outletName;
            }

            return " (Hybrid)";
        }

        private async Task StartUpdaterProcess()
        {
            string updatePath = Path.Combine(Application.StartupPath, "update", "update.exe");

            try
            {
                // Validasi Ekstensi dan Keamanan File
                if (!File.Exists(updatePath))
                {
                    NotifyHelper.Error("File update tidak ditemukan");
                    LoggerUtil.LogWarning("Update file not found");
                    return;
                }

                // Tambahkan Verifikasi Tambahan
                if (!IsValidUpdateExecutable(updatePath))
                {
                    NotifyHelper.Error("File update tidak valid");
                    LoggerUtil.LogWarning("Invalid update executable");
                    return;
                }

                // Periksa Koneksi Internet dengan Timeout
                if (!await IsInternetConnectedAsync())
                {
                    NotifyHelper.Warning("Tidak ada koneksi internet");
                    return;
                }

                // Logging Detail Sebelum Proses
                LoggerUtil.LogWarning($"Memulai proses update dari: {updatePath}");

                // Proses Start dengan Konfigurasi Aman
                ProcessStartInfo startInfo = new()
                {
                    FileName = updatePath,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    Verb = "runas", // Elevated privileges
                    WorkingDirectory = Path.GetDirectoryName(updatePath)
                };

                // Keluar Aplikasi Setelah Memulai Update
                Application.Exit();
            }
            catch (Exception ex)
            {
                // Logging Komprehensif
                LoggerUtil.LogError(ex, $"Gagal memulai proses update: {ex.Message}");
                NotifyHelper.Error($"Gagal memulai update: {ex.Message}");
            }
        }

        // Metode Tambahan untuk Validasi
        private bool IsValidUpdateExecutable(string filePath)
        {
            try
            {
                FileInfo fileInfo = new(filePath);

                // Validasi Ukuran File
                if (fileInfo.Length == 0 || fileInfo.Length > 50_000_000) // Maks 50 MB
                {
                    return false;
                }

                // Validasi Ekstensi
                if (Path.GetExtension(filePath).ToLower() != ".exe")
                {
                    return false;
                }

                // Optional: Tambahkan Hash Verification
                // string fileHash = CalculateFileHash(filePath);
                // return fileHash == "ExpectedHashValue";

                return true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Validasi file update gagal");
                return false;
            }
        }

        // Metode Koneksi Internet dengan Timeout
        private async Task<bool> IsInternetConnectedAsync(int timeoutMilliseconds = 5000)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                    var response = await client.GetAsync("https://www.google.com");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        private async void TestPrinter(object sender, EventArgs e)
        {
            try
            {
                btnSave_Click(sender, null);
                if (printerModel != null)
                {
                    await printerModel.SelectAndPrintAsync();
                }
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal test " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        public async Task OpenDualMonitor()
        {
            try
            {
                string data = "ON";
                await File.WriteAllTextAsync("setting\\configDualMonitor.data", data);

                string path = Application.StartupPath + "KASIRDualMonitor\\KASIR Dual Monitor.exe";

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

        private async void radioDualMonitor_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string allSettingsData = await File.ReadAllTextAsync("setting\\configDualMonitor.data");

                if (radioDualMonitor.Checked && allSettingsData == "OFF")
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
                NotifyHelper.Error($"{ex}");
            }
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                NotifyHelper.Info("Mematikan Tampilan Dual Monitor");

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
            //---------------- Question Begin -----------------\\
            string titleHelper = "Reset Cache ?";
            string msgHelper = "Reset / Redownload Data Lokal Cache ?";
            string cancelHelper = "Batal";
            string okHelper = "Reset";
            QuestionHelper c = new(titleHelper, msgHelper, cancelHelper, okHelper);
            Form background = c.CreateOverlayForm();

            c.Owner = background;

            background.Show();

            DialogResult dialogResult = c.ShowDialog();
            if (dialogResult != DialogResult.OK)
            {
                background.Dispose();
                return;
            }
            else
            {


                //--------------- Question Result -------------------\\

                background.Dispose();
                string TypeCacheEksekusi = "Reset";
                CacheDataApp ResetForm = new(TypeCacheEksekusi);
                ResetForm.Show();
            }
        }


        private void UpdateInfo_Click(object sender, EventArgs e)
        {
            UpdateInformation u = new();
            //Close();

            u.Show();
        }


        private void CacheApp_Click(object sender, EventArgs e)
        {
            try
            {
                string TypeCacheEksekusi = "Sync";
                CacheDataApp form3 = new(TypeCacheEksekusi);
                //Close();
                form3.Show();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error($"Gagal Download Data : {ex}");
            }
        }

        public async Task LoadConfig()
        {
            if (!Directory.Exists(configFolderPath))
            {
                _ = Directory.CreateDirectory(configFolderPath);
            }

            await EnsureFileExistsAsync("setting\\FooterStruk.data", "TERIMAKASIH ATAS KUNJUNGANNYA", txtFooter);
            await EnsureFileExistsAsync("setting\\RunningText.data", "TERIMAKASIH ATAS KUNJUNGANNYA", txtRunningText);
            await EnsureFileExistsAsync("setting\\configListMenu.data", "OFF", sButtonListMenu);
            await EnsureFileExistsAsync("setting\\configDualMonitor.data", "OFF", radioDualMonitor);
            await EnsureFileExistsAsync("setting\\OfflineMode.data", "ON", sButtonOffline);
            //OFLLINE AUTO ON

            string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
            CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
            lblOutletID.Text = "ID : " + dataOutlet.data.id.ToString();
            lblOutletName.Text = "Name : " + dataOutlet.data.name.ToString();
            lblOutletAddress.Text = "Address : " + dataOutlet.data.address.ToString();
            lblOutletPhoneNumber.Text = "Phone Number : " + dataOutlet.data.phone_number.ToString();

            DisplayLogInPanel(LoggerMsg);
        }
        public async void DisplayLogInPanel(Panel logggerMsg)
        {
            try
            {
                string content = await ReadingLogAsync(); // ambil dulu konten

                // Pisah per baris
                string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // Filter hanya baris yang bukan stack trace / path file
                var filtered = lines
                    .Where(l => !l.TrimStart().StartsWith("at ")) // buang stack trace
                    .Where(l => !l.Contains(":line "))            // buang info line number
                    .Where(l => !l.Contains(@":\"))               // buang path Windows (C:\..)
                    .Where(l => !l.StartsWith("--- End of stack trace")) // buang end trace
                    .ToArray();

                // Batasi 5000 baris terakhir
                int maxLines = 5000;
                if (filtered.Length > maxLines)
                    filtered = filtered.Skip(filtered.Length - maxLines).ToArray();

                // Gabungkan dengan extra newline supaya ada jarak antar log
                string finalContent = string.Join(Environment.NewLine + Environment.NewLine, filtered);

                LoggerMsg.Controls.Clear();

                var box = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Text = finalContent,
                    BackColor = Color.White,
                    ForeColor = Color.Black,  // opsional
                    Font = new Font("Courier New", 10) // opsional biar rapih
                };

                LoggerMsg.Controls.Add(box);

                // Auto scroll ke bawah setelah isi dimasukkan
                box.SelectionStart = box.Text.Length;
                box.ScrollToCaret();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error(ex.Message);
            }
        }


        private async Task<string> ReadingLogAsync()
        {
            try
            {
                string outletName = "unknown";
                string outletID = Settings.Default.BaseOutlet;
                string cacheOutlet = $"DT-Cache\\DataOutlet{outletID}.data";
                if (File.Exists(cacheOutlet))
                {
                    string cacheData = await File.ReadAllTextAsync(cacheOutlet);
                    CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheData);

                    if (dataOutlet?.data != null)
                    {
                        outletName = dataOutlet.data.name;
                    }
                }

                string todayDate = DateTime.Now.ToString("yyyyMMdd");
                string customText = $"{outletID}_{outletName}_log{todayDate}";
                string logFilePath = $"log\\{customText}.txt";

                if (!File.Exists(logFilePath))
                {
                    return string.Empty;
                }

                using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string content = await streamReader.ReadToEndAsync();
                    return content;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
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
                NotifyHelper.Success($"Loading Config.. {filePath}\nstatus : {content}");
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
                _ = Directory.CreateDirectory(configFolderPath);
            }

            if (sButtonListMenu.Checked)
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
            if (IPAddress.TryParse(txtPrinter1.Text, out _))
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
            if (IPAddress.TryParse(txtPrinter2.Text, out _))
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
            if (IPAddress.TryParse(txtPrinter3.Text, out _))
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
            SettingsDual u = new();

            //Close();

            u.Show();
        }

        public static void OpenFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                NotifyHelper.Error("Folder tidak ditemukan.");
                return;
            }

            _ = Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true, // Perlu diaktifkan untuk menggunakan shell default sistem operasi
                Verb = "open"
            });
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            Form passwordForm = new()
            {
                Text = "Autentikasi",
                Width = 300,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent, // Center relative to parent
                MaximizeBox = false,
                TopMost = true,
                //Owner = this // Set the owner to establish parent-child relationship
            };

            Label lblStatus = new()
            {
                Text = "Masukkan Password:",
                Location = new Point(20, 20), // Sekarang menggunakan System.Drawing.Point
                Width = 260,
                TextAlign = ContentAlignment.MiddleLeft
            };

            TextBox txtPassword = new()
            {
                Location = new Point(20, 50), // Sekarang menggunakan System.Drawing.Point
                Width = 260,
                UseSystemPasswordChar = true
            };

            System.Windows.Forms.Button btnSubmit = new()
            {
                Text = "Submit",
                Location = new Point(100, 100),
                DialogResult = DialogResult.OK
            };

            Label lblError = new()
            {
                Text = "",
                Location = new Point(20, 130),
                Width = 260,
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };

            //Close();
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
                    _ = txtPassword.Focus();
                }
            };

            passwordForm.Controls.Add(lblStatus);
            passwordForm.Controls.Add(txtPassword);
            passwordForm.Controls.Add(btnSubmit);
            passwordForm.Controls.Add(lblError);
            passwordForm.AcceptButton = btnSubmit;

            if (passwordForm.ShowDialog() == DialogResult.OK)
            {
                SettingsConfig u = new();
                u.Show();
            }
        }

        private async void sButtonOffline_CheckedChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(configFolderPath))
            {
                _ = Directory.CreateDirectory(configFolderPath);
            }

            string data, Config = "setting\\OfflineMode.data";
            string allSettingsData = await File.ReadAllTextAsync(Config);

            if (sButtonOffline.Checked && allSettingsData == "OFF")
            {
                data = "ON";
                await File.WriteAllTextAsync(Config, data);

                // Memanggil UpdateContent pada Form1 yang sudah ada
                //MainForm.UpdateContent(); // Mengupdate konten pada Form1 yang sudah ada

                string TypeCacheEksekusi = "Sync";
                CacheDataApp form3 = new(TypeCacheEksekusi);
                //Close();
                form3.Show();
            }

            if (sButtonOffline.Checked == false && allSettingsData == "ON")
            {
                data = "OFF";
                await File.WriteAllTextAsync(Config, data);

                //MainForm.UpdateContent(); // Mengupdate konten pada Form1 yang sudah ada
                //Close();
            }
        }

        private async void sButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(configFolderPath))
            {
                _ = Directory.CreateDirectory(configFolderPath);
            }

            string data, Config = "setting\\QRcodeSetting.data";
            string allSettingsData = await File.ReadAllTextAsync(Config);

            if (sButton1.Checked == true && allSettingsData == "OFF")
            {
                if (picThumbnail.Image == null)
                {
                    NotifyHelper.Error("Image QRCode Not Found");
                    sButton1.Checked = false;
                    return;
                }
                data = "ON";
                await File.WriteAllTextAsync(Config, data);
            }

            if (sButton1.Checked == false && allSettingsData == "ON")
            {
                data = "OFF";
                await File.WriteAllTextAsync(Config, data);
            }
        }

        private async Task loadLogo()
        {
            string PathLogo = "icon\\QRCode.bmp";

            string Config = "setting\\QRcodeSetting.data";
            if (!File.Exists(Config))
            {
                string statusdata = "OFF";
                await File.WriteAllTextAsync(Config, statusdata);
            }
            string allSettingsData = await File.ReadAllTextAsync(Config);

            if (allSettingsData == "OFF")
            {
                radioDualMonitor.Checked = false;
            }
            else
            {
                radioDualMonitor.Checked = true;
            }

            // Menyeting ukuran dan lokasi untuk PictureBox (untuk thumbnail)
            picThumbnail.Size = new System.Drawing.Size(100, 100);  // Ukuran thumbnail
            picThumbnail.SizeMode = PictureBoxSizeMode.Zoom;  // Agar gambar ter-pastikan tidak pecah

            // Menampilkan gambar default (SambelCowek.bmp) jika tersedia
            string defaultImagePath = PathLogo;  // Sesuaikan dengan path gambar Anda
            if (File.Exists(defaultImagePath))
            {
                // Membuka file gambar menggunakan FileStream untuk memastikan file dapat diakses
                using (FileStream fs = new(defaultImagePath, FileMode.Open, FileAccess.Read))
                {
                    picThumbnail.Image = new Bitmap(fs);  // Menampilkan gambar di PictureBox
                }
            }
            else
            {
                defaultImagePath = "icon\\DT-Logo.bmp";
                if (!File.Exists(defaultImagePath))
                {
                    return;
                }
                // Membuka file gambar menggunakan FileStream untuk memastikan file dapat diakses
                using (FileStream fs = new(defaultImagePath, FileMode.Open, FileAccess.Read))
                {
                    picThumbnail.Image = new Bitmap(fs);  // Menampilkan gambar di PictureBox
                }
            }
        }

        private void picThumbnail_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Path folder untuk menyimpan logo
                string logoFolder = "icon";

                // Pastikan folder exists
                _ = Directory.CreateDirectory(logoFolder);

                // Path lengkap file logo
                string PathLogo = Path.Combine(logoFolder, "QRCode.bmp");

                // Gunakan method dari ImageUploadHelper
                _imageUploadHelper.UploadAndSaveImage(picThumbnail, PathLogo);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Terjadi kesalahan saat membuka file: " + ex.Message);
            }
        }

        private void iconButton2_Click(object sender, EventArgs e)
        {
            SettingsDual u = new();

            //Close();

            u.Show();
        }

        private void Offline_settingsForm_Load(object sender, EventArgs e)
        {

        }
    }
}