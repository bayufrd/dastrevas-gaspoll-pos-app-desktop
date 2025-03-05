
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Color = System.Drawing.Color;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using System.Runtime.InteropServices;
using System.Net;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Linq.Expressions;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Xml;
using System.Reflection;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Printing;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using System.IO.Ports;
using KASIR.Model;
using System.Windows.Markup;
using System.Net.Sockets;
using System.Drawing.Printing;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Timers;
using KASIR.komponen;
using static System.Windows.Forms.DataFormats;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using Brushes = System.Drawing.Brushes;
using FontStyle = System.Drawing.FontStyle;
using KASIR.Printer;
using ComboBox = System.Windows.Forms.ComboBox;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Management;
using Image = System.Drawing.Image;
using System.Drawing.Imaging;
using MessageBox = System.Windows.MessageBox;
using Rectangle = System.Drawing.Rectangle;
using CheckBox = System.Windows.Forms.CheckBox;
using Control = System.Windows.Forms.Control;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using TextBox = System.Windows.Forms.TextBox;
using SharpCompress.Common;
using SharpCompress.Compressors.Xz;
using KASIR.OfflineMode;

namespace KASIR.Komponen
{
    public partial class SettingsForm : Form
    {
        public Form1 MainForm { get; set; }
        private PrinterItem selectedPrinterItem;
        //private IPrinterModel printerModel;
        private PrinterModel printerModel; // Pastikan ini telah diinisialisasi dengan benar
        string macKitchen, macKasir, macBar;
        string pinKitchen, pinKasir, pinBar, struk;
        //private readonly PrinterModel printerModel = new PrinterModel();
        private readonly string baseOutlet;
        private readonly string MacAddressKasir;
        private readonly string MacAddressKitchen;
        private readonly string MacAddressBar;
        private readonly string PinPrinterKasir;
        private readonly string PinPrinterKitchen;
        private readonly string PinPrinterBar;
        private readonly string BaseOutletName;

        int maxAttempts = 4;
        int currentAttempt = 1;
        int retryDelayMilliseconds = 10000;
        private string configFolderPath = "setting";
        private string configFilePath = "setting\\configListMenu.txt";
        public SettingsForm(Form1 mainForm)
        {
            this.ControlBox = false;
            InitializeComponent();
            MainForm = mainForm;
            LoadConfig();
            lblNewVersion.Visible = true;
            lblNewVersionNow.Visible = true;
            btnUpdate.Text = "Repair";
            lblVersion.Text = Properties.Settings.Default.Version.ToString();
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            MacAddressKitchen = Properties.Settings.Default.MacAddressKitchen;
            MacAddressBar = Properties.Settings.Default.MacAddressBar;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            PinPrinterKitchen = Properties.Settings.Default.PinPrinterKitchen;
            PinPrinterBar = Properties.Settings.Default.PinPrinterBar;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;


            // test printer melalui pencarian. ( untuk check list )
            printerModel = new PrinterModel(); // Initialize printerModel
                                               // Load printers when the form loads or as appropriate
            LoadPrintersAndSettings().ConfigureAwait(false);

            //
            cekUpdate();

            initAwalOfflineTrial();
        }
        private async void initAwalOfflineTrial()
        {
            if (int.Parse(baseOutlet.ToString()) != 1)
            {
                sButtonOffline.Enabled = true;
            }
            else
            {
                sButtonOffline.Enabled = false;
            }
        }
        // Metode untuk memuat pengaturan printer dan checkbox
        private async Task LoadPrintersAndSettings()
        {

            PrinterModel printerModel = new PrinterModel();
            List<ComboBox> comboBoxes = new List<ComboBox> { ComboBoxPrinter1, ComboBoxPrinter2, ComboBoxPrinter3 };

            foreach (var comboBox in comboBoxes)
            {
                comboBox.Items.Clear();
                comboBox.SelectedIndexChanged -= ComboBoxPrinter_SelectedIndexChanged; // Remove handler first

                try
                {
                    var printers = await printerModel.GetAvailablePrinters();

                    // Add the default item first
                    comboBox.Items.Add(new PrinterItem("Mac Address Manual", null));

                    foreach (var printer in printers)
                    {
                        comboBox.Items.Add(printer);
                    }

                    string comboBoxName = comboBox.Name.Substring(10); // Extract the number from comboBox name
                    string savedPrinterId = await printerModel.LoadPrinterSettingsAsync(comboBoxName);
                    if (!string.IsNullOrEmpty(savedPrinterId) && !printerModel.IsNotMacAddressOrIpAddress(savedPrinterId))
                    {
                        comboBox.SelectedIndex = 0;
                        string numberString = comboBoxName.Substring(5); // Mengambil angka setelah "inter"
                        if (int.TryParse(numberString, out int printerNumber))
                        {
                            // Bentuk nama TextBox berdasarkan nomor printer
                            string textBoxName = $"txtPrinter{printerNumber}";

                            // Cari kontrol TextBox yang sesuai
                            TextBox textBox = Controls.Find(textBoxName, true).FirstOrDefault() as TextBox;
                            if (textBox != null)
                            {
                                textBox.Text = savedPrinterId;
                            }
                        }

                    }
                    else
                    {
                        if (string.IsNullOrEmpty(savedPrinterId))
                        {
                            // If no saved printer, select default item
                            comboBox.SelectedIndex = 0;

                        }
                        else
                        {
                            if (!printerModel.IsBluetoothPrinter(savedPrinterId))
                                SetSelectedPrinter(comboBox, savedPrinterId);
                        }
                    }


                    // Load and set checkbox states
                    await LoadCheckBoxStates(comboBoxName);

                    comboBox.SelectedIndexChanged += ComboBoxPrinter_SelectedIndexChanged; // Add handler back

                    // Hook up events for checkboxes dynamically
                    string checkBoxPrefix = "checkBoxKasir";
                    List<CheckBox> checkBoxes = FindCheckBoxesByPrefix(checkBoxPrefix + comboBoxName);


                    foreach (var checkBox in checkBoxes)
                    {
                        checkBox.CheckedChanged -= CheckBox_CheckedChanged; // Remove handler first (optional)
                        checkBox.CheckedChanged += CheckBox_CheckedChanged; // Add handler
                    }

                }
                catch (Exception ex)
                {
                    // Handle any exceptions, log or show an error message
                    MessageBox.Show($"Error loading printers for {comboBox.Name}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                    // Set default item if there's an error
                    comboBox.Items.Add(new PrinterItem("Mac Address Manual", null));
                    comboBox.SelectedIndex = 0;
                }

                //}
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


        private async Task<bool> LoadCheckBoxSettingAsync(string checkBoxName)
        {
            // Load checkbox state based on its name
            // Example implementation:
            bool isChecked = await printerModel.LoadCheckBoxSettingAsync(checkBoxName);
            return isChecked;
        }

        private async void SaveCheckBoxSettingAsync(string checkBoxName, bool isChecked)
        {
            // Save checkbox state based on its name
            // Example implementation:
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

        private async Task<bool> LoadCheckBoxSetting(string checkBoxName)
        {
            // Load checkbox state based on its name
            // Example implementation:
            bool isChecked = await printerModel.LoadCheckBoxSettingAsync(checkBoxName);
            return isChecked;
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

        private async void cekUpdate()
        {
            try
            {
                var urlVersion = Properties.Settings.Default.BaseAddressVersion.ToString();
                var newVersion = (new WebClient().DownloadString(urlVersion));
                string currentVersion = Properties.Settings.Default.Version.ToString();
                newVersion = newVersion.Replace(".", "");
                currentVersion = currentVersion.Replace(".", "");

                if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
                {
                    lblNewVersion.Visible = true;
                    lblNewVersionNow.Visible = true;
                    lblNewVersionNow.Text = (new WebClient().DownloadString(urlVersion));
                    btnUpdate.Text = "Update";
                }
                else
                {
                    lblNewVersion.Visible = true;
                    lblNewVersionNow.Visible = true;
                    lblNewVersionNow.Text = (new WebClient().DownloadString(urlVersion));

                    btnUpdate.Text = "Fix";
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        public void btnUpdate_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnUpdate_Click));
            Util n = new Util();
            n.sendLogTelegramNetworkError("Open Updater Manual" + BaseOutletName);
            btnUpdate.Enabled = false;

            try
            {
                if (File.Exists(System.Windows.Forms.Application.StartupPath + "update\\update.exe"))
                {
                    if (IsInternetConnected())
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
                }
                else
                {

                    System.Windows.Forms.MessageBox.Show("File tidak ditemukan", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    LoggerUtil.LogWarning("File not found");

                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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
            catch (Exception ex)
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
        private async void LoadConfig()
        {
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }

            // load Footer
            if (!File.Exists("setting\\FooterStruk.data"))
            {
                string data = "TERIMAKASIH ATAS KUNJUNGANNYA";
                await File.WriteAllTextAsync("setting\\FooterStruk.data", data);
            }
            else
            {
                string allSettingsData = await File.ReadAllTextAsync("setting\\FooterStruk.data");
                if (!string.IsNullOrEmpty(allSettingsData))
                    txtFooter.Text = allSettingsData;
            }
            //load running text
            if (!File.Exists("setting\\RunningText.data"))
            {
                string data = "TERIMAKASIH ATAS KUNJUNGANNYA";
                await File.WriteAllTextAsync("setting\\RunningText.data", data);
            }
            else
            {
                string allSettingsData = await File.ReadAllTextAsync("setting\\RunningText.data");
                if (!string.IsNullOrEmpty(allSettingsData))
                {
                    txtRunningText.Text = allSettingsData;
                }
                else
                {
                    string data = "TERIMAKASIH ATAS KUNJUNGANNYA";
                    await File.WriteAllTextAsync("setting\\RunningText.data", data);
                }
            }
            //load ListMenu
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
                        sButtonListMenu.Checked = true;
                    }
                    else
                    {
                        sButtonListMenu.Checked = false;
                    }
                }
                else
                {
                    sButtonListMenu.Checked = false;
                    string data = "OFF";
                    await File.WriteAllTextAsync("setting\\configListMenu.data", data);
                }
            }
            //load dual monitor
            if (!File.Exists("setting\\configDualMonitor.data"))
            {
                string data = "OFF";
                await File.WriteAllTextAsync("setting\\configDualMonitor.data", data);
            }
            else
            {
                string allSettingsData = await File.ReadAllTextAsync("setting\\configDualMonitor.data");

                if (!string.IsNullOrEmpty(allSettingsData))
                {
                    if (allSettingsData == "ON")
                    {
                        radioDualMonitor.Checked = true;
                    }
                    else
                    {
                        radioDualMonitor.Checked = false;
                    }

                }
                else
                {
                    radioDualMonitor.Checked = false;
                    string data = "OFF";
                    await File.WriteAllTextAsync("setting\\configDualMonitor.data", data);
                }
            }

            //loadOfflinemode
            string Configflie = "setting\\OfflineMode.data";
            if (!File.Exists(Configflie))
            {
                string data = "OFF";
                await File.WriteAllTextAsync(Configflie, data);
            }
            else
            {
                string allSettingsData = await File.ReadAllTextAsync(Configflie);

                if (!string.IsNullOrEmpty(allSettingsData))
                {
                    if (allSettingsData == "ON")
                    {
                        sButtonOffline.Checked = true;
                    }
                    else
                    {
                        sButtonOffline.Checked = false;
                    }
                }
                else
                {
                    sButtonOffline.Checked = false;
                    string data = "OFF";
                    await File.WriteAllTextAsync(Configflie, data);
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

            //Console.WriteLine($"Folder {folderPath} telah dibuka.");
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            SettingsConfig u = new SettingsConfig();
            this.Close();

            u.Show();
        }

        private async void sButtonOffline_CheckedChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }
            string data, Config = "setting\\OfflineMode.data";
            string allSettingsData = await File.ReadAllTextAsync(Config);

            // Cek apakah Offline mode diaktifkan
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

            // Cek apakah Offline mode dimatikan
            if (sButtonOffline.Checked == false && allSettingsData == "ON")
            {
                data = "OFF";
                await File.WriteAllTextAsync(Config, data);

                // Memanggil UpdateContent pada Form1 yang sudah ada
                MainForm.UpdateContent(); // Mengupdate konten pada Form1 yang sudah ada
                this.Close();

            }
        }

    }
}
