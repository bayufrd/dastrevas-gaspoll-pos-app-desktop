using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using KASIR.Helper;
using KASIR.Model;

namespace KASIR.Printer
{
    public class PrinterModel
    {
        private readonly string baseDirectory;
        private readonly Dictionary<string, bool> checkBoxSettings = new();
        private readonly Dictionary<string, string> printerSettings = new();

        private string _kategori;
        private readonly int logoCredit = 75; //default 75 PrintLogo(stream, "icon\\DT-Logo.bmp", logoCredit);

        //init size logo struk
        private readonly int logoSize = 250; //default 250 PrintLogo(stream, "icon\\OutletLogo.bmp", logoSize);
        private const string SEPARATOR = "--------------------------------\n";
        private const string PRINT_POWERED_BY = "Powered By Dastrevas\n";
        private static readonly ConcurrentDictionary<string, BluetoothClient> _activeBluetoothConnections =
    new ConcurrentDictionary<string, BluetoothClient>();
        private static readonly object _connectionLock = new object();
        public PrinterModel()
        {
            baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting");
            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }
        }
        public static class PrinterConfig
        {
            private static readonly Dictionary<string, (string NormalSize, string BigSize, int Width)> _printerPresets =
        new Dictionary<string, (string, string, int)>(StringComparer.OrdinalIgnoreCase)
    {
        // Format: (Ukuran Normal, Ukuran Besar, Lebar Karakter)
        { "66:22:F7:10:58:5B", ("\x1D\x21\x11", "\x1D\x21\x22", 24) }, // Moka - double size sebagai normal
        { "moka", ("\x1D\x21\x11", "\x1D\x21\x22", 24) },               // Moka generic
        { "xprinter", ("\x1D\x21\x00", "\x1D\x21\x01", 16) },           // XPrinter - normal size
        { "default", ("\x1D\x21\x00", "\x1D\x21\x01", 16) }             // Default
    };

            public static (string NormalSize, string BigSize, int Width) GetPrinterSettings(string printerName)
            {
                // Coba cari setting berdasarkan nama/alamat lengkap
                if (_printerPresets.TryGetValue(printerName, out var settings))
                    return settings;

                // Coba cari berdasarkan substring (mis. jika nama mengandung "moka")
                foreach (var key in _printerPresets.Keys)
                {
                    if (printerName.Contains(key, StringComparison.OrdinalIgnoreCase))
                        return _printerPresets[key];
                }

                // Gunakan default jika tidak ditemukan
                return _printerPresets["default"];
            }
        }
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public async Task SavePrinterBluetoothMACSettings(string comboBoxName, string printerId)
        {
            printerSettings[comboBoxName] = printerId;
            await SaveSettingsToFileAsync("printerSettings.data", SerializePrinterSettings());
        }

        public async Task SavePrinterSettings(string comboBoxName, string printerId)
        {
            printerSettings[comboBoxName] = printerId;
            await SaveSettingsToFileAsync("printerSettings.data", SerializePrinterSettings());
        }

        public async Task DelPrinterSettings(string comboBoxName)
        {
            if (printerSettings.ContainsKey(comboBoxName))
            {
                printerSettings.Remove(comboBoxName);
                await SaveSettingsToFileAsync("printerSettings.data", SerializePrinterSettings());
            }
        }

        public async Task<string?> LoadPrinterSettingsAsync(string comboBoxName)
        {
            await LoadAllPrinterSettings();
            if (printerSettings.TryGetValue(comboBoxName, out string? printerId))
            {
                return printerId;
            }

            return null;
        }

        public async Task SaveCheckBoxSettingAsync(string checkBoxName, bool isChecked)
        {
            checkBoxSettings[checkBoxName] = isChecked;
            await SaveSettingsToFileAsync("checkBoxSettings.data", SerializeCheckBoxSettings());
        }

        public async Task<bool> LoadCheckBoxSettingAsync(string checkBoxName)
        {
            await LoadAllPrinterSettings();
            if (checkBoxSettings.TryGetValue(checkBoxName, out bool isChecked))
            {
                return isChecked;
            }

            return false;
        }

        public async Task LoadAllPrinterSettings()
        {
            await LoadPrinterSettings();
            await LoadCheckBoxSettings();
        }

        public string SerializePrinterSettings()
        {
            StringBuilder sb = new();
            foreach (KeyValuePair<string, string> kvp in printerSettings)
            {
                sb.AppendLine($"{kvp.Key}:{kvp.Value}");
            }

            return sb.ToString();
        }

        public void DeserializePrinterSettings(string data)
        {
            printerSettings.Clear();
            string[] lines = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    printerSettings[parts[0]] = parts[1];
                }
            }
        }

        public async Task<List<PrinterItem>> GetAvailablePrinters()
        {
            List<PrinterItem> availablePrinters = new();

            try
            {
                await Task.Run(() =>
                {
                    using (ManagementObjectSearcher searcher = new("SELECT * FROM Win32_Printer"))
                    {
                        foreach (ManagementObject printer in searcher.Get())
                        {
                            try
                            {
                                string printerName = printer["Name"]?.ToString();
                                string printerId = printer["DeviceID"]?.ToString();
                                int printerStatus = Convert.ToInt32(printer["PrinterStatus"] ?? 0);
                                bool workOffline = Convert.ToBoolean(printer["WorkOffline"] ?? false);

                                // Filter out printers with unavailable drivers or problematic status
                                if (!string.IsNullOrEmpty(printerName) &&
                                    !string.IsNullOrEmpty(printerId) &&
                                    printerStatus != 7 && // Assuming 7 is an error status
                                    !workOffline) // Ignore printers that are offline
                                {
                                    availablePrinters.Add(new PrinterItem(printerName, printerId));
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggerUtil.LogError(ex, "An error occurred: " + ex.Message);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: " + ex.Message);
            }

            return availablePrinters;
        }

        public async Task LoadPrinterSettings()
        {
            string filePath = Path.Combine(baseDirectory, "printerSettings.data");
            if (File.Exists(filePath))
            {
                string allSettingsData = await File.ReadAllTextAsync(filePath);
                DeserializePrinterSettings(allSettingsData);
            }
        }

        public async Task LoadCheckBoxSettings()
        {
            string filePath = Path.Combine(baseDirectory, "checkBoxSettings.data");
            if (File.Exists(filePath))
            {
                string allSettingsData = await File.ReadAllTextAsync(filePath);
                DeserializeCheckBoxSettings(allSettingsData);
            }
        }
        public async Task SaveSettingsToFileAsync(string fileName, string data)
        {
            try
            {
                string fullPath = Path.Combine(baseDirectory, fileName);

                // Pastikan direktori ada
                EnsureDirectoryExists(Path.GetDirectoryName(fullPath));

                // Tulis data ke file
                await File.WriteAllTextAsync(fullPath, data);
            }
            catch (Exception ex)
            {
                // Log error
                LoggerUtil.LogError(ex, $"Gagal menyimpan file: {fileName}");
                throw;
            }
        }

        public async Task SaveSettingsToFile(string fileName, string data)
        {
            await File.WriteAllTextAsync($"setting//{fileName}", data);
        }

        public string SerializeCheckBoxSettings()
        {
            StringBuilder sb = new();
            foreach (KeyValuePair<string, bool> kvp in checkBoxSettings)
            {
                sb.AppendLine($"{kvp.Key}:{kvp.Value}");
            }

            return sb.ToString();
        }

        private void DeserializeCheckBoxSettings(string data)
        {
            checkBoxSettings.Clear();
            string[] lines = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    if (bool.TryParse(parts[1], out bool isChecked))
                    {
                        checkBoxSettings[parts[0]] = isChecked;
                    }
                }
            }
        }

        //Printing
        public async Task LoadSettingsAsync()
        {
            await LoadPrinterSettingsAsyncatPrinting("setting\\printerSettings.data");
            await LoadCheckBoxSettingsAsync("setting\\checkBoxSettings.data");
        }

        private async Task LoadPrinterSettingsAsyncatPrinting(string filePath)
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    printerSettings[parts[0]] = parts[1];
                }
            }
        }

        private async Task LoadCheckBoxSettingsAsync(string filePath)
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2 && bool.TryParse(parts[1], out bool value))
                {
                    checkBoxSettings[parts[0]] = value;
                }
            }
        }

        //TEST PRINTER

        public async Task SelectAndPrintAsync()
        {
            try
            {
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync(); // Load additional settings

                foreach (KeyValuePair<string, string> printer in printerSettings)
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        PrintDocument printDocument = new();
                        printDocument.PrintPage += Ex_PrintDocument_PrintPage;
                        //Ex_PrintDocument_PrintPage();
                        continue;
                    }

                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        _kategori = "Kasir";
                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            continue;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new())
                        {
                            if (!printerDevice.Authenticated) // cek sudah paired?
                            {
                                if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                {
                                    NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                    continue;
                                }
                            }


                            clientSocket.Connect(endpoint);
                            Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += SEPARATOR;

                            strukText += kodeHeksadesimalSizeBesar +
                                         CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += SEPARATOR;
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine(printerName, "Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine(printerName, "Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += SEPARATOR;
                            strukText += PRINT_POWERED_BY;
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", logoCredit); // Smaller logo size

                            byte[] buffer = Encoding.UTF8.GetBytes(strukText);

                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();

                            clientSocket.GetStream().Close();
                            stream.Close();
                            clientSocket.Close();
                        }
                    }

                    if (ShouldPrint(printerId, "Checker"))
                    {
                        _kategori = "Checker";

                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            continue;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new())
                        {
                            if (!printerDevice.Authenticated) // cek sudah paired?
                            {
                                if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                {
                                    NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                    continue;
                                }
                            }

                            clientSocket.Connect(endpoint);
                            Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += SEPARATOR;

                            strukText += kodeHeksadesimalSizeBesar +
                                         CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += SEPARATOR;
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine(printerName, "Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine(printerName, "Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += SEPARATOR;
                            strukText += PRINT_POWERED_BY;
                            strukText += "--------------------------------\n\n\n\n\n";
                            //PrintLogo(stream, "icon\\DT-Logo.bmp", logoCredit); // Smaller logo size

                            byte[] buffer = Encoding.UTF8.GetBytes(strukText);

                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();

                            clientSocket.GetStream().Close();
                            stream.Close();
                            clientSocket.Close();
                        }
                    }

                    if (ShouldPrint(printerId, "Makanan"))
                    {
                        _kategori = "Makanan";

                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            continue;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new())
                        {
                            if (!printerDevice.Authenticated) // cek sudah paired?
                            {
                                if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                {
                                    NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                    continue;
                                }
                            }

                            clientSocket.Connect(endpoint);
                            Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += SEPARATOR;

                            strukText += kodeHeksadesimalSizeBesar +
                                         CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += SEPARATOR;
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine(printerName, "Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine(printerName, "Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += SEPARATOR;
                            strukText += PRINT_POWERED_BY;
                            strukText += "--------------------------------\n\n\n\n\n";
                            //PrintLogo(stream, "icon\\DT-Logo.bmp", logoCredit); // Smaller logo size

                            byte[] buffer = Encoding.UTF8.GetBytes(strukText);

                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();

                            clientSocket.GetStream().Close();
                            stream.Close();
                            clientSocket.Close();
                        }
                    }

                    if (ShouldPrint(printerId, "Minuman"))
                    {
                        _kategori = "Minuman";

                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            continue;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new())
                        {
                            if (!printerDevice.Authenticated) // cek sudah paired?
                            {
                                if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                {
                                    NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                    continue;
                                }
                            }

                            clientSocket.Connect(endpoint);
                            Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += SEPARATOR;

                            strukText += kodeHeksadesimalSizeBesar +
                                         CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += SEPARATOR;
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine(printerName, "Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine(printerName, "Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += SEPARATOR;
                            strukText += PRINT_POWERED_BY;
                            strukText += "--------------------------------\n\n\n\n\n";
                            //PrintLogo(stream, "icon\\DT-Logo.bmp", logoCredit); // Smaller logo size

                            byte[] buffer = Encoding.UTF8.GetBytes(strukText);

                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();

                            clientSocket.GetStream().Close();
                            stream.Close();
                            clientSocket.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: " + ex.Message);
            }
        }

        private bool ShouldPrint(string printerId, string printType)
        {
            return checkBoxSettings.TryGetValue($"checkBox{printType}Printer{printerId}", out bool shouldPrint) &&
                   shouldPrint;
        }

        // Bluetooth Struct Printing
        private string ConvertMacAddressFormat(string macAddress)
        {
            return macAddress.Replace("-", ":");
        }

        public bool IsBluetoothPrinter(string macAddress)
        {
            // Remove any spaces from the MAC address
            string cleanedAddress = macAddress?.Trim().Replace(" ", "");

            // Safety check for null or empty string
            if (string.IsNullOrEmpty(cleanedAddress))
            {
                return false;
            }

            // Format yang diharapkan: "00:1A:2B:3C:4D:5E" or "00-1A-2B-3C-4D-5E"
            // Pengecekan dengan ekspresi reguler (regex)
            string pattern = "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";

            // Buat objek Regex untuk memeriksa format
            Regex regex = new(pattern);

            // Lakukan pencocokan regex untuk memeriksa validitas MAC Address
            return regex.IsMatch(cleanedAddress);
        }


        private void PrintViaBluetooth(string printerName, PrintDocument printDocument)
        {
            // Get Bluetooth address from printer settings
            string bluetoothAddress = printerName;

            try
            {
                BluetoothClient client = new();

                BluetoothDeviceInfo printer = new(BluetoothAddress.Parse(bluetoothAddress));
                if (printer == null)
                {
                    return;
                }

                BluetoothEndPoint endpoint = new(printer.DeviceAddress, BluetoothService.SerialPort);

                if (printer != null)
                {
                    if (!printer.Authenticated) // cek sudah paired?
                    {
                        if (!BluetoothSecurity.PairRequest(printer.DeviceAddress, "0000"))
                        {
                            NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                            return;
                        }
                    }

                    // Connect to Bluetooth printer service (replace with appropriate UUID)
                    Guid serviceClass = BluetoothService.SerialPort;
                    client.Connect(endpoint);

                    // If connected, print using existing print document
                    if (client.Connected)
                    {
                        printDocument.PrinterSettings.PrinterName = printerName;
                        printDocument.Print();
                    }

                    // Close connection
                    client.Close();
                }
                else
                {
                    // Device not found
                    throw new Exception("Bluetooth device not found.");
                }
            }
            catch (Exception ex)
            {
                // Handle Bluetooth connection or printing errors
                LoggerUtil.LogError(ex, "An error occurred while printing via Bluetooth: {ErrorMessage}", ex.Message);
            }
        }

        public async void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                await LoadPrinterSettings();
                await LoadSettingsAsync();

                foreach (KeyValuePair<string, string> printer in printerSettings)
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");

                    // Jika bukan mac/ip address, jalankan print dokumen
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintDocument_PrintPage(sender, e);
                    }

                    // Fungsi untuk print dengan kondisi
                    async Task PrintWithCondition(string kategori, Func<string, bool> shouldPrintCondition)
                    {
                        if (shouldPrintCondition(printerId))
                        {
                            BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                            if (printerDevice == null)
                            {
                                return;
                            }

                            BluetoothClient client = new();
                            BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                            using (BluetoothClient clientSocket = new())
                            {
                                if (!printerDevice.Authenticated) // cek sudah paired?
                                {
                                    if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                    {
                                        NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                        return;
                                    }
                                }

                                clientSocket.Connect(endpoint);
                                Stream stream = clientSocket.GetStream();

                                string kodeHeksadesimalBold = "\x1B\x45\x01";
                                string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                                string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

                                string strukText = "\n" + kodeHeksadesimalBold + CenterText(kategori);
                                strukText += kodeHeksadesimalNormal;
                                strukText += SEPARATOR;

                                strukText += kodeHeksadesimalSizeBesar +
                                             CenterText("Printed Success at Mac Address" + printerName);
                                strukText += kodeHeksadesimalNormal;
                                strukText += SEPARATOR;
                                strukText += CenterText("Test Tengah");
                                strukText += FormatSimpleLine(printerName, "Test Data Kiri", "Test Data Kanan") + "\n";
                                strukText += kodeHeksadesimalBold;
                                strukText += FormatSimpleLine(printerName, "Test Data Kiri Tebal", "Bold") + "\n";

                                strukText += kodeHeksadesimalNormal;

                                strukText += SEPARATOR;
                                strukText += PRINT_POWERED_BY;
                                strukText += "--------------------------------\n\n\n\n\n";

                                byte[] buffer = Encoding.UTF8.GetBytes(strukText);

                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();

                                clientSocket.GetStream().Close();
                                stream.Close();
                                clientSocket.Close();
                            }
                        }
                    }

                    // Panggil print untuk setiap kategori
                    await PrintWithCondition("Kasir", id => ShouldPrint(id, "Kasir"));
                    await PrintWithCondition("Checker", id => ShouldPrint(id, "Checker"));
                    await PrintWithCondition("Makanan", id => ShouldPrint(id, "Makanan"));
                    await PrintWithCondition("Minuman", id => ShouldPrint(id, "Minuman"));
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: " + ex.Message);
            }
        }


        private void Ex_PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                string Kategori = _kategori;
                // Mengatur font normal dan tebal
                Font normalFont =
                    new("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                Font boldFont = new("Arial", 8, FontStyle.Bold);
                float leftMargin = 5; // Margin kiri (dalam pixel)
                float rightMargin = 5; // Margin kanan (dalam pixel)
                float topMargin = 5; // Margin atas (dalam pixel)
                float yPos = topMargin;

                // Lebar kertas thermal 58mm, sesuaikan dengan margin
                float paperWidth = 58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                float printableWidth = paperWidth - leftMargin - rightMargin;

                // Fungsi untuk format teks kiri dan kanan
                void DrawSimpleLine(string textLeft, string textRight)
                {
                    SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                    SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                    e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                    e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                        leftMargin + printableWidth - sizeRight.Width, yPos);
                    yPos += normalFont.GetHeight(e.Graphics);
                }

                // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                void DrawCenterText(string text, Font font)
                {
                    if (text == null)
                    {
                        text = string.Empty;
                    }

                    if (font == null)
                    {
                        throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                    }

                    string[] words = text.Split(' ');
                    StringBuilder currentLine = new();
                    foreach (string word in words)
                    {
                        SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                        if (size.Width > printableWidth)
                        {
                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                            SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                            yPos += font.GetHeight(e.Graphics);
                            currentLine.Clear();
                        }

                        // Tambahkan kata ke baris saat ini
                        currentLine.Append(word + " ");
                    }

                    // Gambar baris terakhir
                    if (currentLine.Length > 0)
                    {
                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                            leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                        yPos += font.GetHeight(e.Graphics);
                    }
                }

                // Fungsi untuk menggambar teks rata kiri
                void DrawLeftText(string text, Font font)
                {
                    if (text == null)
                    {
                        //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                        return;
                    }

                    e.Graphics.DrawString(text, font, Brushes.Black, leftMargin, yPos);
                    yPos += font.GetHeight(e.Graphics);
                }

                // Fungsi untuk menggambar garis pemisah
                void DrawSeparator()
                {
                    e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                    yPos += normalFont.GetHeight(e.Graphics);
                }

                // Fungsi untuk mendapatkan dan mengonversi gambar logo ke hitam dan putih
                Image GetLogoImage(string path)
                {
                    Image img = Image.FromFile(path);
                    Bitmap bmp = new(img.Width, img.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        ColorMatrix colorMatrix = new(new[]
                        {
                            new[] { 0.3f, 0.3f, 0.3f, 0, 0 }, new[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                            new[] { 0.11f, 0.11f, 0.11f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 },
                            new float[] { 0, 0, 0, 0, 1 }
                        });
                        ImageAttributes attributes = new();
                        attributes.SetColorMatrix(colorMatrix);
                        g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height,
                            GraphicsUnit.Pixel, attributes);
                    }

                    return bmp;
                }

                // Menambahkan logo dan teks "Powered by" di akhir struk
                void DrawPoweredByLogo(string path)
                {
                    if (!File.Exists(path)) { return; }

                    // Menambahkan jarak sebelum mencetak teks dan logo
                    float spaceBefore = 50; // Jarak dalam pixel, sesuaikan dengan kebutuhan
                    yPos += spaceBefore;

                    // Mengukur teks "Powered by Your Company"
                    string poweredByText = "Powered by Dastrevas";
                    SizeF textSize = e.Graphics.MeasureString(poweredByText, normalFont);

                    // Gambar teks
                    float textX = leftMargin + ((printableWidth - textSize.Width) / 2);
                    e.Graphics.DrawString(poweredByText, normalFont, Brushes.Black, textX, yPos);

                    // Sesuaikan yPos untuk logo
                    yPos += textSize.Height;

                    // Menggambar logo
                    Image logoPoweredBy = GetLogoImage(path);
                    float targetWidth = 35; // Ukuran lebar logo dalam pixel, sesuaikan dengan kebutuhan
                    float scaleFactor = targetWidth / logoPoweredBy.Width;
                    float logoHeight = logoPoweredBy.Height * scaleFactor;

                    float logoX = leftMargin + ((printableWidth - targetWidth) / 2);
                    e.Graphics.DrawImage(logoPoweredBy, logoX, yPos, targetWidth, logoHeight);

                    spaceBefore = 5;
                    yPos += spaceBefore;
                    // Sesuaikan yPos untuk elemen berikutnya
                    yPos += logoHeight;
                }


                // Path ke logo Powered by Anda
                string poweredByLogoPath = "icon\\DT-Logo.bmp"; // Ganti dengan path logo Powered by Anda

                // Menambahkan logo di bagian atas dengan ukuran yang proporsional
                string logoPath = "icon\\OutletLogo.bmp"; // Ganti dengan path logo Anda
                Image logo = GetLogoImage(logoPath);
                float logoTargetWidthMm = 25; // Lebar target logo dalam mm
                float logoTargetWidthPx = logoTargetWidthMm / 25.4f * 100; // Konversi ke pixel

                // Hitung tinggi logo berdasarkan lebar yang diinginkan dengan mempertahankan rasio aspek
                float scaleFactor = logoTargetWidthPx / logo.Width;
                float logoHeight = logo.Height * scaleFactor;
                float logoX = leftMargin + ((printableWidth - logoTargetWidthPx) / 2);

                // Gambar logo dengan ukuran yang diubah
                e.Graphics.DrawImage(logo, logoX, yPos, logoTargetWidthPx, logoHeight);
                yPos += logoHeight + 5; // Menambahkan jarak setelah logo

                // Struct template

                DrawCenterText(Kategori, boldFont);

                DrawCenterText("Test Tengah", boldFont);
                DrawSeparator();
                DrawSimpleLine("Test Data Kiri", "Test Data Kanan");
                DrawLeftText("Test Data Kiri Tebal", boldFont);

                DrawPoweredByLogo(poweredByLogoPath);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        // Struct Print Laporan Shift Cetak Laporan
        public async Task PrintModelCetakLaporanShift(DataStrukShift dataShifts,
            List<ExpenditureStrukShift> expenditures,
            List<CartDetailsSuccessStrukShift> cartDetailsSuccess,
            List<CartDetailsPendingStrukShift> cartDetailsPendings,
            List<CartDetailsCanceledStrukShift> cartDetailsCanceled,
            List<RefundDetailStrukShift> refundDetails,
            List<PaymentDetailStrukShift> paymentDetails)
        {
            try
            {
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync(); // Load additional settings
                var printTasks = new ConcurrentBag<Task>();

                foreach (KeyValuePair<string, string> printer in printerSettings)
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");
                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }

                    // Tangkap variabel untuk closure
                    var currentPrinterName = printerName;
                    var currentPrinterId = printerId;

                    // Tambahkan task paralel
                    printTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (IsNotMacAddressOrIpAddress(printerName))
                            {
                                Ex_PrintModelCetakLaporanShift(dataShifts, expenditures, cartDetailsSuccess,
                                    cartDetailsPendings, cartDetailsCanceled, refundDetails, paymentDetails,
                                    printerId, printerName
                                );
                                return;
                            }

                            if (ShouldPrint(printerId, "Kasir"))
                            {
                                await GenerateStrukTextShiftLaporan(dataShifts, expenditures,
                                    cartDetailsSuccess, cartDetailsPendings, cartDetailsCanceled, refundDetails,
                                    paymentDetails, printerName);
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerUtil.LogError(ex, $"Error printing for printer {currentPrinterName}: {ex.Message}");
                        }
                    }));
                }

                // Tunggu semua task selesai
                await Task.WhenAll(printTasks);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error printing for printing ShiftReport : {ex.Message}");
                throw;
            }
        }

        private async Task GenerateStrukTextShiftLaporan(
    DataStrukShift dataShifts,
    List<ExpenditureStrukShift> expenditures,
    List<CartDetailsSuccessStrukShift> cartDetailsSuccess,
    List<CartDetailsPendingStrukShift> cartDetailsPendings,
    List<CartDetailsCanceledStrukShift> cartDetailsCanceled,
    List<RefundDetailStrukShift> refundDetails,
    List<PaymentDetailStrukShift> paymentDetails,
    string printerName)
        {
            Stream stream = Stream.Null;

            try
            {
                stream = await EstablishPrinterConnection(printerName);

                int totalItems = cartDetailsSuccess.Count +
                                 cartDetailsPendings.Count +
                                 cartDetailsCanceled.Count +
                                 refundDetails.Count +
                                 expenditures.Count +
                                 paymentDetails.Count;
                var capacity = GetOptimalCapacity(totalItems);
                var strukBuilder = new StringBuilder(capacity);

                string kodeHeksadesimalBold = "\x1B\x45\x01";
                string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

                strukBuilder.AppendFormat("\n{0}{1}",
                    kodeHeksadesimalBold,
                    CenterText(dataShifts.outlet_name));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.Append(CenterText(dataShifts.outlet_address));
                strukBuilder.Append(CenterText(dataShifts.outlet_phone_number));

                strukBuilder.Append(SEPARATOR);

                strukBuilder.AppendFormat("{0}{1}",
                    kodeHeksadesimalSizeBesar,
                    CenterText("SHIFT PRINT"));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Start Date", dataShifts.start_date));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "End Date", dataShifts.end_date));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Casher Name", dataShifts.casher_name));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Shift Number", dataShifts.shift_number.ToString()));
                strukBuilder.Append(SEPARATOR);

                void AppendSortedDetails<T>(
                    List<T> details,
                    string sectionTitle,
                    Func<T, string> menuNameSelector,
                    Func<T, int> qtySelector,
                    Func<T, decimal> totalPriceSelector,
                    Func<T, string> varianSelector = null)
                {
                    if (!details.Any()) return;

                    strukBuilder.AppendFormat("{0}{1}", kodeHeksadesimalBold, sectionTitle + "\n");
                    strukBuilder.Append(kodeHeksadesimalNormal);

                    var sortedDetails = details
                        .OrderBy(x =>
                        {
                            var menuType = x.GetType().GetProperty("menu_type")?.GetValue(x) as string ?? "";
                            return menuType.Contains("Minuman") ? 1 :
                                   menuType.Contains("Additional Minuman") ? 2 :
                                   menuType.Contains("Makanan") ? 3 :
                                   menuType.Contains("Additional Makanan") ? 4 : 5;
                        })
                        .ThenBy(x => menuNameSelector(x));

                    foreach (var detail in sortedDetails)
                    {
                        strukBuilder.AppendFormat("{0}",
                            FormatSimpleLine(printerName,
                                $"{qtySelector(detail)} {menuNameSelector(detail)}",
                                $"{totalPriceSelector(detail):n0}"));

                        if (varianSelector != null && !string.IsNullOrEmpty(varianSelector(detail)))
                        {
                            strukBuilder.AppendFormat("{0}\n",
                                FormatDetailItemLine("Varian", varianSelector(detail)));
                        }
                    }
                }

                strukBuilder.AppendFormat("{0}{1}", kodeHeksadesimalBold, CenterText("ORDER DETAILS"));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.Append(SEPARATOR);

                strukBuilder.AppendFormat("{0}{1}\n", kodeHeksadesimalBold, "SOLD ITEMS");
                strukBuilder.Append(kodeHeksadesimalNormal);

                AppendSortedDetails(
                    cartDetailsSuccess,
                    "SOLD ITEMS",
                    x => x.menu_name,
                    x => x.qty,
                    x => x.total_price,
                    x => x.varian
                );

                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Item Sold Qty", dataShifts.totalSuccessQty.ToString()));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Item Sold Amount", $"{dataShifts.totalCartSuccessAmount:n0}"));

                if (cartDetailsPendings.Any())
                {
                    strukBuilder.Append(SEPARATOR);
                    AppendSortedDetails(
                        cartDetailsPendings,
                        "PENDING ITEMS",
                        x => x.menu_name,
                        x => x.qty,
                        x => x.total_price,
                        x => x.varian
                    );

                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Item Pending Qty", dataShifts.totalPendingQty.ToString()));
                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Item Pending Amount", $"{dataShifts.totalCartPendingAmount:n0}"));
                }

                if (cartDetailsCanceled.Any())
                {
                    strukBuilder.Append(SEPARATOR);
                    AppendSortedDetails(
                        cartDetailsCanceled,
                        "CANCEL ITEMS",
                        x => x.menu_name,
                        x => x.qty,
                        x => x.total_price,
                        x => x.varian
                    );

                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Item Cancel Qty", dataShifts.totalCanceledQty.ToString()));
                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Item Cancel Amount", $"{dataShifts.totalCartCanceledAmount:n0}"));
                }

                if (refundDetails.Any())
                {
                    strukBuilder.Append(SEPARATOR);
                    AppendSortedDetails(
                        refundDetails,
                        "REFUND ITEMS",
                        x => x.menu_name,
                        x => x.qty_refund_item,
                        x => x.total_refund_price,
                        x => x.varian
                    );

                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Item Refund Qty", dataShifts.totalRefundQty.ToString()));
                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Item Refund Amount", $"{dataShifts.totalCartRefundAmount:n0}"));
                }

                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}{1}", kodeHeksadesimalBold, CenterText("CASH MANAGEMENT"));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.Append(SEPARATOR);

                if (expenditures.Any())
                {
                    strukBuilder.AppendFormat("{0}EXPENSE\n", kodeHeksadesimalBold);
                    strukBuilder.Append(kodeHeksadesimalNormal);

                    foreach (var expense in expenditures)
                    {
                        strukBuilder.AppendFormat("{0}\n",
                            FormatSimpleLine(printerName, expense.description, $"{expense.nominal:n0}"));
                    }
                }

                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Expected Ending Cash", $"{dataShifts.ending_cash_expected:n0}"));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Actual Ending Cash", $"{dataShifts.ending_cash_actual:n0}"));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Cash Difference", $"{dataShifts.cash_difference:n0}"));

                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}DISCOUNTS\n", kodeHeksadesimalBold);
                strukBuilder.Append(kodeHeksadesimalNormal);

                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "All Discount items", $"{dataShifts.discount_amount_per_items:n0}"));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "All Discount Cart", $"{dataShifts.discount_amount_per_items:n0}"));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "TOTAL AMOUNT", $"{dataShifts.discount_total_amount:n0}"));

                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}{1}", kodeHeksadesimalBold, CenterText("PAYMENT DETAIL"));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.Append(SEPARATOR);

                foreach (var paymentDetail in paymentDetails)
                {
                    strukBuilder.AppendLine(paymentDetail.payment_category);
                    foreach (var paymentType in paymentDetail.payment_type_detail)
                    {
                        strukBuilder.AppendFormat("{0}\n",
                            FormatSimpleLine(printerName, paymentType.payment_type, $"{paymentType.total_payment:n0}"));
                    }

                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "TOTAL AMOUNT", $"{paymentDetail.total_amount:n0}"));
                    strukBuilder.Append(SEPARATOR);
                }

                strukBuilder.AppendFormat("{0}{1}\n",
                    kodeHeksadesimalBold,
                    FormatSimpleLine(printerName, "TOTAL TRANSACTION", $"{dataShifts.total_transaction:n0}"));
                strukBuilder.Append(kodeHeksadesimalNormal);

                strukBuilder.Append("--------------------------------\n\n\n\n\n");

                byte[] buffer = Encoding.UTF8.GetBytes(strukBuilder.ToString());
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error in GenerateStrukTextShiftLaporan: {ex.Message}");
                throw;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }
        }

        private string CenterText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Menggunakan perintah ESC/POS untuk rata tengah
            string centerCmd = "\x1B\x61\x01"; // ESC a 1 = center alignment
            string leftCmd = "\x1B\x61\x00";   // ESC a 0 = left alignment (reset)

            // Menambahkan perintah centering + teks + new line + kembali ke kiri
            return centerCmd + text + "\n" + leftCmd;
        }

        // Fungsi untuk memotong kata panjang menjadi beberapa bagian
        private void SplitLongWord(string word, int maxLength, StringBuilder centeredText, StringBuilder currentLine)
        {
            int startIndex = 0;
            while (startIndex < word.Length)
            {
                int length = Math.Min(maxLength, word.Length - startIndex);
                string part = word.Substring(startIndex, length);

                // Tambahkan potongan kata ke baris dengan padding
                AppendLineWithPadding(centeredText, new StringBuilder(part), maxLength);

                startIndex += length;
            }
        }

        // Fungsi untuk menambahkan baris dengan padding tengah
        private void AppendLineWithPadding(StringBuilder centeredText, StringBuilder line, int maxLength)
        {
            int spaces = (maxLength - line.Length) / 2;
            centeredText.AppendLine(new string(' ', spaces) + line.ToString().TrimEnd());
        }

        public string FormatSimpleLine(string printerIdentifier, string left, object right)
        {
            // Jika objek right null, maka atur rightString sebagai string kosong
            string rightString = right != null ? right.ToString() : string.Empty;

            // Hitung panjang teks yang seharusnya ditambahkan ke kiri
            int paddingLength = Math.Max(0, 32 - rightString.Length);

            // Format baris dengan padding dan alignment yang benar
            string formattedLine = left.PadRight(paddingLength) + rightString;

            return formattedLine;
        }

        private string FormatDetailItemLine(string label, string value)
        {
            return "  @" + label + ": " + value;
        }

        public async Task Ex_PrintModelCetakLaporanShift(DataStrukShift dataShifts,
            List<ExpenditureStrukShift> expenditures,
            List<CartDetailsSuccessStrukShift> cartDetailsSuccess,
            List<CartDetailsPendingStrukShift> cartDetailsPendings,
            List<CartDetailsCanceledStrukShift> cartDetailsCanceled,
            List<RefundDetailStrukShift> refundDetails,
            List<PaymentDetailStrukShift> paymentDetails,
            string printerId,
            string printerName)
        {
            try
            {
                Stream stream = null;

                // Koneksi printer langsung di sini
                if (IPAddress.TryParse(printerName, out _))
                {
                    TcpClient client = new(printerName, 9100);
                    stream = client.GetStream();
                }
                else
                {
                    await RetryPolicyAsync(async () =>
                    {
                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            return false;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        if (!printerDevice.Authenticated) // cek sudah paired?
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                return false;
                            }
                        }

                        client.Connect(endpoint);
                        stream = client.GetStream();
                        return true;
                    }, 3);
                }

                // Struck Checker
                if (ShouldPrint(printerId, "Kasir"))
                {
                    PrintDocument printDocument = new();
                    printDocument.PrintPage += (sender, e) =>
                    {
                        Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                        // Mengatur font normal dan tebal
                        Font normalFont =
                            new("Arial", 8,
                                FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                        Font boldFont = new("Arial", 8, FontStyle.Bold);
                        Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                        Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                        float leftMargin = 5; // Margin kiri (dalam pixel)
                        float rightMargin = 5; // Margin kanan (dalam pixel)
                        float topMargin = 5; // Margin atas (dalam pixel)
                        float yPos = topMargin;

                        // Lebar kertas thermal 58mm, sesuaikan dengan margin
                        float paperWidth =
                            58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                        float printableWidth = paperWidth - leftMargin - rightMargin;


                        // Fungsi untuk format teks kiri dan kanan
                        void DrawSimpleLine(string textLeft, string textRight)
                        {
                            SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                            SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                            e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                            e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                leftMargin + printableWidth - sizeRight.Width, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                        void DrawCenterText(string text, Font font)
                        {
                            if (text == null)
                            {
                                text = string.Empty;
                            }

                            if (font == null)
                            {
                                throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                    leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }

                        // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                        void DrawLeftText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }


                        // Fungsi untuk menggambar garis pemisah
                        void DrawSeparator()
                        {
                            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        void DrawSpace()
                        {
                            yPos += normalFont
                                .GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                        }

                        DrawCenterText(dataShifts?.outlet_name, BigboldFont);
                        DrawCenterText(dataShifts?.outlet_address, normalFont);
                        DrawCenterText(dataShifts?.outlet_phone_number, normalFont);

                        DrawSeparator();

                        DrawCenterText("SHIFT PRINT", BigboldFont);
                        DrawSeparator();
                        DrawSimpleLine("Start Date", dataShifts.start_date);
                        DrawSimpleLine("End Date", dataShifts.end_date);
                        DrawSimpleLine("Casher Name", dataShifts.casher_name);
                        DrawSimpleLine("Shift Number", dataShifts.shift_number.ToString());
                        DrawSeparator();
                        DrawCenterText("ORDER DETAILS", BigboldFont);
                        DrawSeparator();
                        DrawCenterText("SOLD ITEMS", boldFont);
                        IOrderedEnumerable<CartDetailsSuccessStrukShift> sortedcartDetailSuccess = cartDetailsSuccess
                            .OrderBy(x =>
                            {
                                if (x.menu_type.Contains("Minuman"))
                                {
                                    return 1;
                                }

                                if (x.menu_type.Contains("Additional Minuman"))
                                {
                                    return 2;
                                }

                                if (x.menu_type.Contains("Makanan"))
                                {
                                    return 3;
                                }

                                if (x.menu_type.Contains("Additional Makanan"))
                                {
                                    return 4;
                                }

                                return 5;
                            })
                            .ThenBy(x => x.menu_name);
                        //foreach (var cartDetail in cartDetailsSuccess)
                        foreach (CartDetailsSuccessStrukShift cartDetail in sortedcartDetailSuccess)
                        {
                            // Add varian to the cart detail name if it's not null
                            string displayMenuName = cartDetail.menu_name;
                            if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                            {
                                DrawSimpleLine(cartDetail.menu_name, "");
                                DrawSimpleLine(cartDetail.varian, "");
                            }
                            else
                            {
                                DrawSimpleLine(cartDetail.menu_name, "");
                            }

                            DrawSimpleLine(cartDetail.qty.ToString(), string.Format("{0:n0}", cartDetail.total_price));
                        }

                        DrawSpace();
                        DrawSimpleLine("Item Sold Qty", dataShifts.totalSuccessQty.ToString());
                        DrawSimpleLine("Item Sold Amount", string.Format("{0:n0}", dataShifts.totalCartSuccessAmount));
                        if (cartDetailsPendings.Count != 0)
                        {
                            DrawSeparator();
                            DrawCenterText("PENDING ITEMS", boldFont);
                            IOrderedEnumerable<CartDetailsPendingStrukShift> sortedcartDetailPendings =
                                cartDetailsPendings.OrderBy(x =>
                                    {
                                        if (x.menu_type.Contains("Minuman"))
                                        {
                                            return 1;
                                        }

                                        if (x.menu_type.Contains("Additional Minuman"))
                                        {
                                            return 2;
                                        }

                                        if (x.menu_type.Contains("Makanan"))
                                        {
                                            return 3;
                                        }

                                        if (x.menu_type.Contains("Additional Makanan"))
                                        {
                                            return 4;
                                        }

                                        return 5;
                                    })
                                    .ThenBy(x => x.menu_name);
                            //foreach (var cartDetail in cartDetailsPendings)
                            foreach (CartDetailsPendingStrukShift cartDetail in sortedcartDetailPendings)
                            {
                                // Add varian to the cart detail name if it's not null
                                if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                    DrawSimpleLine(cartDetail.varian, "");
                                }
                                else
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                }

                                DrawSimpleLine(cartDetail.qty.ToString(),
                                    string.Format("{0:n0}", cartDetail.total_price));
                            }

                            DrawSimpleLine("Item Pending Qty", dataShifts.totalPendingQty.ToString());
                            DrawSimpleLine("Item Pending Amount",
                                string.Format("{0:n0}", dataShifts.totalCartPendingAmount));
                        }

                        if (cartDetailsCanceled.Count != 0)
                        {
                            DrawSeparator();
                            DrawCenterText("CANCEL ITEMS", boldFont);
                            IOrderedEnumerable<CartDetailsCanceledStrukShift> sortedcartDetailCanceled =
                                cartDetailsCanceled.OrderBy(x =>
                                    {
                                        if (x.menu_type.Contains("Minuman"))
                                        {
                                            return 1;
                                        }

                                        if (x.menu_type.Contains("Additional Minuman"))
                                        {
                                            return 2;
                                        }

                                        if (x.menu_type.Contains("Makanan"))
                                        {
                                            return 3;
                                        }

                                        if (x.menu_type.Contains("Additional Makanan"))
                                        {
                                            return 4;
                                        }

                                        return 5;
                                    })
                                    .ThenBy(x => x.menu_name);
                            //foreach (var cartDetail in cartDetailsCanceled)
                            foreach (CartDetailsCanceledStrukShift cartDetail in sortedcartDetailCanceled)
                            {
                                // Add varian to the cart detail name if it's not null
                                string displayMenuName = cartDetail.menu_name;
                                if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                    DrawSimpleLine(cartDetail.varian, "");
                                }
                                else
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                }

                                DrawSimpleLine(cartDetail.qty.ToString(),
                                    string.Format("{0:n0}", cartDetail.total_price));
                            }

                            DrawSimpleLine("Item Cancel Qty", dataShifts.totalCanceledQty.ToString());
                            DrawSimpleLine("Item Cancel Amount",
                                string.Format("{0:n0}", dataShifts.totalCartCanceledAmount));
                        }

                        if (refundDetails.Count != 0)
                        {
                            DrawSeparator();
                            DrawCenterText("REFUND ITEMS", boldFont);
                            IOrderedEnumerable<RefundDetailStrukShift> sortedrefundDetails = refundDetails.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman"))
                                    {
                                        return 1;
                                    }

                                    if (x.menu_type.Contains("Additional Minuman"))
                                    {
                                        return 2;
                                    }

                                    if (x.menu_type.Contains("Makanan"))
                                    {
                                        return 3;
                                    }

                                    if (x.menu_type.Contains("Additional Makanan"))
                                    {
                                        return 4;
                                    }

                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);
                            //foreach (var cartDetail in refundDetails)
                            foreach (RefundDetailStrukShift cartDetail in sortedrefundDetails)
                            {
                                // Add varian to the cart detail name if it's not null
                                string displayMenuName = cartDetail.menu_name;
                                if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                    DrawSimpleLine(cartDetail.varian, "");
                                }
                                else
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                }

                                DrawSimpleLine(cartDetail.qty_refund_item.ToString(),
                                    string.Format("{0:n0}", cartDetail.total_refund_price));
                            }

                            DrawSimpleLine("Item Refund Qty", dataShifts.totalRefundQty.ToString());
                            DrawSimpleLine("Item Refund Amount",
                                string.Format("{0:n0}", dataShifts.totalCartRefundAmount));
                        }

                        DrawSeparator();
                        DrawCenterText("CASH MANAGEMENT", BigboldFont);
                        DrawSeparator();
                        if (expenditures.Count != 0)
                        {
                            DrawLeftText("EXPENSE", boldFont);
                            foreach (ExpenditureStrukShift expense in expenditures)
                            {
                                DrawSimpleLine(expense.description, string.Format("{0:n0}", expense.nominal));
                            }
                        }

                        DrawSimpleLine("Expected Ending Cash",
                            string.Format("{0:n0}", dataShifts.ending_cash_expected));
                        DrawSimpleLine("Actual Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_actual));
                        DrawSimpleLine("Cash Difference", string.Format("{0:n0}", dataShifts.cash_difference));
                        DrawSeparator();
                        DrawLeftText("DISCOUNTS", boldFont);
                        DrawSimpleLine("All Discount items",
                            string.Format("{0:n0}", dataShifts.discount_amount_per_items));
                        DrawSimpleLine("All Discount Cart",
                            string.Format("{0:n0}", dataShifts.discount_amount_per_items));
                        DrawSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", dataShifts.discount_total_amount));
                        DrawSeparator();
                        DrawCenterText("PAYMENT DETAIL", BigboldFont);
                        DrawSeparator();
                        foreach (PaymentDetailStrukShift paymentDetail in paymentDetails)
                        {
                            DrawLeftText(paymentDetail.payment_category, boldFont);
                            foreach (PaymentTypeDetailStrukShift paymentType in paymentDetail.payment_type_detail)
                            {
                                DrawSimpleLine(paymentType.payment_type,
                                    string.Format("{0:n0}", paymentType.total_payment));
                            }

                            DrawSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount));
                            DrawSeparator();
                        }

                        DrawSimpleLine("TOTAL TRANSACTION", string.Format("{0:n0}", dataShifts.total_transaction));
                        DrawSeparator();
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        // Struct Print Ulang Shift
        public async Task PrintModelCetakUlangShift(DataStrukShift dataShifts,
            List<ExpenditureStrukShift> expenditures,
            List<CartDetailsSuccessStrukShift> cartDetailsSuccess,
            List<CartDetailsPendingStrukShift> cartDetailsPendings,
            List<CartDetailsCanceledStrukShift> cartDetailsCanceled,
            List<RefundDetailStrukShift> refundDetails,
            List<PaymentDetailStrukShift> paymentDetails)
        {
            try
            {
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync(); // Load additional settings

                foreach (KeyValuePair<string, string> printer in printerSettings)
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }

                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintModelCetakUlangShift(dataShifts, expenditures, cartDetailsSuccess, cartDetailsPendings,
                            cartDetailsCanceled, refundDetails, paymentDetails,
                            printerId, printerName
                        );
                        continue;
                    }

                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        Stream stream = Stream.Null;

                        try
                        {
                            if (IPAddress.TryParse(printerName, out _))
                            {
                                // Connect via LAN
                                TcpClient client = new(printerName, 9100);
                                stream = client.GetStream();
                            }
                            else
                            {
                                // Connect via Bluetooth
                                BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                                if (printerDevice == null)
                                {
                                    continue;
                                }

                                BluetoothClient client = new();
                                BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress,
                                    BluetoothService.SerialPort);

                                if (!printerDevice.Authenticated) // cek sudah paired?
                                {
                                    if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                    {
                                        NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                        continue;
                                    }
                                }

                                client.Connect(endpoint);
                                stream = client.GetStream();
                            }

                            string strukText = GenerateStrukText(dataShifts, expenditures, cartDetailsSuccess,
                                cartDetailsPendings, cartDetailsCanceled, refundDetails, paymentDetails, printerName);
                            byte[] buffer = Encoding.UTF8.GetBytes(strukText);
                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
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

        private string GenerateStrukText(DataStrukShift dataShifts,
            List<ExpenditureStrukShift> expenditures,
            List<CartDetailsSuccessStrukShift> cartDetailsSuccess,
            List<CartDetailsPendingStrukShift> cartDetailsPendings,
            List<CartDetailsCanceledStrukShift> cartDetailsCanceled,
            List<RefundDetailStrukShift> refundDetails,
            List<PaymentDetailStrukShift> paymentDetails, string printerName)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            string strukText = "\n" + kodeHeksadesimalBold + CenterText(dataShifts.outlet_name);
            strukText += kodeHeksadesimalNormal;
            strukText += CenterText(dataShifts.outlet_address);
            strukText += CenterText(dataShifts.outlet_phone_number);

            strukText += SEPARATOR;

            strukText += kodeHeksadesimalSizeBesar + CenterText("SHIFT PRINT");
            strukText += kodeHeksadesimalNormal;
            strukText += SEPARATOR;
            strukText += FormatSimpleLine(printerName, "Start Date", dataShifts.start_date) + "\n";
            strukText += FormatSimpleLine(printerName, "End Date", dataShifts.end_date) + "\n";
            strukText += FormatSimpleLine(printerName, "Casher Name", dataShifts.casher_name) + "\n";
            strukText += FormatSimpleLine(printerName, "Shift Number", dataShifts.shift_number.ToString()) + "\n";
            strukText += SEPARATOR;

            strukText += kodeHeksadesimalBold + CenterText("ORDER DETAILS");
            strukText += kodeHeksadesimalNormal;
            strukText += SEPARATOR;
            strukText += kodeHeksadesimalBold + "SOLD ITEMS" + "\n";
            strukText += kodeHeksadesimalNormal;


            IOrderedEnumerable<CartDetailsSuccessStrukShift> sortedCartDetailsSuccess = cartDetailsSuccess.OrderBy(x =>
            {
                if (x.menu_type.Contains("Minuman"))
                {
                    return 1;
                }

                if (x.menu_type.Contains("Additional Minuman"))
                {
                    return 2;
                }

                if (x.menu_type.Contains("Makanan"))
                {
                    return 3;
                }

                if (x.menu_type.Contains("Additional Makanan"))
                {
                    return 4;
                }

                return 5;
            }).ThenBy(x => x.menu_name);

            foreach (CartDetailsSuccessStrukShift cartDetail in sortedCartDetailsSuccess)
            {
                strukText += FormatSimpleLine(printerName, cartDetail.qty + " " + cartDetail.menu_name,
                    string.Format("{0:n0}", cartDetail.total_price));
                if (!string.IsNullOrEmpty(cartDetail.varian))
                {
                    strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                }
            }

            strukText += FormatSimpleLine(printerName, "Item Sold Qty", dataShifts.totalSuccessQty.ToString()) + "\n";
            strukText +=
                FormatSimpleLine(printerName, "Item Sold Amount", string.Format("{0:n0}", dataShifts.totalCartSuccessAmount)) + "\n";

            if (cartDetailsPendings.Count != 0)
            {
                strukText += SEPARATOR;
                strukText += kodeHeksadesimalBold + "PENDING ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                IOrderedEnumerable<CartDetailsPendingStrukShift> sortedCartDetailsPendings = cartDetailsPendings
                    .OrderBy(x =>
                    {
                        if (x.menu_type.Contains("Minuman"))
                        {
                            return 1;
                        }

                        if (x.menu_type.Contains("Additional Minuman"))
                        {
                            return 2;
                        }

                        if (x.menu_type.Contains("Makanan"))
                        {
                            return 3;
                        }

                        if (x.menu_type.Contains("Additional Makanan"))
                        {
                            return 4;
                        }

                        return 5;
                    }).ThenBy(x => x.menu_name);

                foreach (CartDetailsPendingStrukShift cartDetail in sortedCartDetailsPendings)
                {
                    strukText += FormatSimpleLine(printerName, cartDetail.qty + " " + cartDetail.menu_name,
                        string.Format("{0:n0}", cartDetail.total_price));
                    if (!string.IsNullOrEmpty(cartDetail.varian))
                    {
                        strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                    }
                }

                strukText += FormatSimpleLine(printerName, "Item Pending Qty", dataShifts.totalPendingQty.ToString()) + "\n";
                strukText += FormatSimpleLine(printerName, "Item Pending Amount",
                    string.Format("{0:n0}", dataShifts.totalCartPendingAmount)) + "\n";
            }

            if (cartDetailsCanceled.Count != 0)
            {
                strukText += SEPARATOR;
                strukText += kodeHeksadesimalBold + "CANCEL ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                IOrderedEnumerable<CartDetailsCanceledStrukShift> sortedCartDetailsCanceled = cartDetailsCanceled
                    .OrderBy(x =>
                    {
                        if (x.menu_type.Contains("Minuman"))
                        {
                            return 1;
                        }

                        if (x.menu_type.Contains("Additional Minuman"))
                        {
                            return 2;
                        }

                        if (x.menu_type.Contains("Makanan"))
                        {
                            return 3;
                        }

                        if (x.menu_type.Contains("Additional Makanan"))
                        {
                            return 4;
                        }

                        return 5;
                    }).ThenBy(x => x.menu_name);

                foreach (CartDetailsCanceledStrukShift cartDetail in sortedCartDetailsCanceled)
                {
                    strukText += FormatSimpleLine(printerName, cartDetail.qty + " " + cartDetail.menu_name,
                        string.Format("{0:n0}", cartDetail.total_price));
                    if (!string.IsNullOrEmpty(cartDetail.varian))
                    {
                        strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                    }
                }

                strukText += FormatSimpleLine(printerName, "Item Cancel Qty", dataShifts.totalCanceledQty.ToString()) + "\n";
                strukText += FormatSimpleLine(printerName, "Item Cancel Amount",
                    string.Format("{0:n0}", dataShifts.totalCartCanceledAmount)) + "\n";
            }

            if (refundDetails.Count != 0)
            {
                strukText += SEPARATOR;
                strukText += kodeHeksadesimalBold + "REFUND ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                IOrderedEnumerable<RefundDetailStrukShift> sortedRefundDetails = refundDetails.OrderBy(x =>
                {
                    if (x.menu_type.Contains("Minuman"))
                    {
                        return 1;
                    }

                    if (x.menu_type.Contains("Additional Minuman"))
                    {
                        return 2;
                    }

                    if (x.menu_type.Contains("Makanan"))
                    {
                        return 3;
                    }

                    if (x.menu_type.Contains("Additional Makanan"))
                    {
                        return 4;
                    }

                    return 5;
                }).ThenBy(x => x.menu_name);

                foreach (RefundDetailStrukShift refundDetail in sortedRefundDetails)
                {
                    strukText += FormatSimpleLine(printerName, refundDetail.qty_refund_item + " " + refundDetail.menu_name,
                        string.Format("{0:n0}", refundDetail.total_refund_price));
                    if (!string.IsNullOrEmpty(refundDetail.varian))
                    {
                        strukText += FormatDetailItemLine("Varian", refundDetail.varian) + "\n";
                    }
                }

                strukText += FormatSimpleLine(printerName, "Item Refund Qty", dataShifts.totalRefundQty.ToString()) + "\n";
                strukText += FormatSimpleLine(printerName, "Item Refund Amount",
                    string.Format("{0:n0}", dataShifts.totalCartRefundAmount)) + "\n";
            }

            strukText += SEPARATOR;
            strukText += kodeHeksadesimalBold + CenterText("CASH MANAGEMENT");

            strukText += kodeHeksadesimalNormal;
            strukText += SEPARATOR;

            if (expenditures.Count != 0)
            {
                strukText += kodeHeksadesimalBold + "EXPENSE\n";
                strukText += kodeHeksadesimalNormal;

                foreach (ExpenditureStrukShift expense in expenditures)
                {
                    strukText += FormatSimpleLine(printerName, expense.description, string.Format("{0:n0}", expense.nominal)) + "\n";
                }
            }

            strukText +=
                FormatSimpleLine(printerName, "Expected Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_expected)) +
                "\n";
            strukText +=
                FormatSimpleLine(printerName, "Actual Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_actual)) + "\n";
            strukText += FormatSimpleLine(printerName, "Cash Difference", string.Format("{0:n0}", dataShifts.cash_difference)) +
                         "\n";

            strukText += SEPARATOR;
            strukText += kodeHeksadesimalBold + "DISCOUNTS\n";
            strukText += kodeHeksadesimalNormal;

            strukText += FormatSimpleLine(printerName, "All Discount items",
                string.Format("{0:n0}", dataShifts.discount_amount_per_items)) + "\n";
            strukText += FormatSimpleLine(printerName, "All Discount Cart",
                string.Format("{0:n0}", dataShifts.discount_amount_per_items)) + "\n";
            strukText += FormatSimpleLine(printerName, "TOTAL AMOUNT", string.Format("{0:n0}", dataShifts.discount_total_amount)) +
                         "\n";

            strukText += SEPARATOR;
            strukText += kodeHeksadesimalBold + CenterText("PAYMENT DETAIL");
            strukText += kodeHeksadesimalNormal;

            strukText += SEPARATOR;

            foreach (PaymentDetailStrukShift paymentDetail in paymentDetails)
            {
                strukText += paymentDetail.payment_category + "\n";
                foreach (PaymentTypeDetailStrukShift paymentType in paymentDetail.payment_type_detail)
                {
                    strukText += FormatSimpleLine(printerName, paymentType.payment_type,
                        string.Format("{0:n0}", paymentType.total_payment)) + "\n";
                }

                strukText += FormatSimpleLine(printerName, "TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount)) +
                             "\n";
                strukText += SEPARATOR;
            }

            strukText += kodeHeksadesimalBold +
                         FormatSimpleLine(printerName, "TOTAL TRANSACTION", string.Format("{0:n0}", dataShifts.total_transaction)) +
                         "\n";
            strukText += kodeHeksadesimalNormal;

            strukText += "--------------------------------\n\n\n\n\n";

            return strukText;
        }

        public async Task Ex_PrintModelCetakUlangShift(DataStrukShift dataShifts,
            List<ExpenditureStrukShift> expenditures,
            List<CartDetailsSuccessStrukShift> cartDetailsSuccess,
            List<CartDetailsPendingStrukShift> cartDetailsPendings,
            List<CartDetailsCanceledStrukShift> cartDetailsCanceled,
            List<RefundDetailStrukShift> refundDetails,
            List<PaymentDetailStrukShift> paymentDetails,
            string printerId,
            string printerName)
        {
            try
            {
                Stream stream = null;

                // Koneksi printer langsung di sini
                if (IPAddress.TryParse(printerName, out _))
                {
                    TcpClient client = new(printerName, 9100);
                    stream = client.GetStream();
                }
                else
                {
                    await RetryPolicyAsync(async () =>
                    {
                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            return false;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        if (!printerDevice.Authenticated) // cek sudah paired?
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                return false;
                            }
                        }

                        client.Connect(endpoint);
                        stream = client.GetStream();
                        return true;
                    }, 3);
                }

                // Struck Checker
                if (ShouldPrint(printerId, "Kasir"))
                {
                    PrintDocument printDocument = new();
                    printDocument.PrintPage += (sender, e) =>
                    {
                        Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                        // Mengatur font normal dan tebal
                        Font normalFont =
                            new("Arial", 8,
                                FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                        Font boldFont = new("Arial", 8, FontStyle.Bold);
                        Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                        Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                        float leftMargin = 5; // Margin kiri (dalam pixel)
                        float rightMargin = 5; // Margin kanan (dalam pixel)
                        float topMargin = 5; // Margin atas (dalam pixel)
                        float yPos = topMargin;

                        // Lebar kertas thermal 58mm, sesuaikan dengan margin
                        float paperWidth =
                            58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                        float printableWidth = paperWidth - leftMargin - rightMargin;


                        // Fungsi untuk format teks kiri dan kanan
                        void DrawSimpleLine(string textLeft, string textRight)
                        {
                            SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                            SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                            e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                            e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                leftMargin + printableWidth - sizeRight.Width, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                        void DrawCenterText(string text, Font font)
                        {
                            if (text == null)
                            {
                                text = string.Empty;
                            }

                            if (font == null)
                            {
                                throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                    leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }

                        // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                        void DrawLeftText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }


                        // Fungsi untuk menggambar garis pemisah
                        void DrawSeparator()
                        {
                            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        void DrawSpace()
                        {
                            yPos += normalFont
                                .GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                        }

                        DrawCenterText(dataShifts?.outlet_name, BigboldFont);
                        DrawCenterText(dataShifts?.outlet_address, normalFont);
                        DrawCenterText(dataShifts?.outlet_phone_number, normalFont);

                        DrawSeparator();

                        DrawCenterText("SHIFT PRINT", BigboldFont);
                        DrawSeparator();
                        DrawSimpleLine("Start Date", dataShifts.start_date);
                        DrawSimpleLine("End Date", dataShifts.end_date);
                        DrawSimpleLine("Casher Name", dataShifts.casher_name);
                        DrawSimpleLine("Shift Number", dataShifts.shift_number.ToString());
                        DrawSeparator();
                        DrawCenterText("ORDER DETAILS", BigboldFont);
                        DrawSeparator();
                        DrawCenterText("SOLD ITEMS", boldFont);
                        IOrderedEnumerable<CartDetailsSuccessStrukShift> sortedcartDetailSuccess = cartDetailsSuccess
                            .OrderBy(x =>
                            {
                                if (x.menu_type.Contains("Minuman"))
                                {
                                    return 1;
                                }

                                if (x.menu_type.Contains("Additional Minuman"))
                                {
                                    return 2;
                                }

                                if (x.menu_type.Contains("Makanan"))
                                {
                                    return 3;
                                }

                                if (x.menu_type.Contains("Additional Makanan"))
                                {
                                    return 4;
                                }

                                return 5;
                            })
                            .ThenBy(x => x.menu_name);
                        //foreach (var cartDetail in cartDetailsSuccess)
                        foreach (CartDetailsSuccessStrukShift cartDetail in sortedcartDetailSuccess)
                        {
                            // Add varian to the cart detail name if it's not null
                            string displayMenuName = cartDetail.menu_name;
                            if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                            {
                                DrawSimpleLine(cartDetail.menu_name, "");
                                DrawSimpleLine(cartDetail.varian, "");
                            }
                            else
                            {
                                DrawSimpleLine(cartDetail.menu_name, "");
                            }

                            DrawSimpleLine(cartDetail.qty.ToString(), string.Format("{0:n0}", cartDetail.total_price));
                        }

                        DrawSpace();
                        DrawSimpleLine("Item Sold Qty", dataShifts.totalSuccessQty.ToString());
                        DrawSimpleLine("Item Sold Amount", string.Format("{0:n0}", dataShifts.totalCartSuccessAmount));
                        if (cartDetailsPendings.Count != 0)
                        {
                            DrawSeparator();
                            DrawCenterText("PENDING ITEMS", boldFont);
                            IOrderedEnumerable<CartDetailsPendingStrukShift> sortedcartDetailPendings =
                                cartDetailsPendings.OrderBy(x =>
                                    {
                                        if (x.menu_type.Contains("Minuman"))
                                        {
                                            return 1;
                                        }

                                        if (x.menu_type.Contains("Additional Minuman"))
                                        {
                                            return 2;
                                        }

                                        if (x.menu_type.Contains("Makanan"))
                                        {
                                            return 3;
                                        }

                                        if (x.menu_type.Contains("Additional Makanan"))
                                        {
                                            return 4;
                                        }

                                        return 5;
                                    })
                                    .ThenBy(x => x.menu_name);
                            //foreach (var cartDetail in cartDetailsPendings)
                            foreach (CartDetailsPendingStrukShift cartDetail in sortedcartDetailPendings)
                            {
                                // Add varian to the cart detail name if it's not null
                                if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                    DrawSimpleLine(cartDetail.varian, "");
                                }
                                else
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                }

                                DrawSimpleLine(cartDetail.qty.ToString(),
                                    string.Format("{0:n0}", cartDetail.total_price));
                            }

                            DrawSimpleLine("Item Pending Qty", dataShifts.totalPendingQty.ToString());
                            DrawSimpleLine("Item Pending Amount",
                                string.Format("{0:n0}", dataShifts.totalCartPendingAmount));
                        }

                        if (cartDetailsCanceled.Count != 0)
                        {
                            DrawSeparator();
                            DrawCenterText("CANCEL ITEMS", boldFont);
                            IOrderedEnumerable<CartDetailsCanceledStrukShift> sortedcartDetailCanceled =
                                cartDetailsCanceled.OrderBy(x =>
                                    {
                                        if (x.menu_type.Contains("Minuman"))
                                        {
                                            return 1;
                                        }

                                        if (x.menu_type.Contains("Additional Minuman"))
                                        {
                                            return 2;
                                        }

                                        if (x.menu_type.Contains("Makanan"))
                                        {
                                            return 3;
                                        }

                                        if (x.menu_type.Contains("Additional Makanan"))
                                        {
                                            return 4;
                                        }

                                        return 5;
                                    })
                                    .ThenBy(x => x.menu_name);
                            //foreach (var cartDetail in cartDetailsCanceled)
                            foreach (CartDetailsCanceledStrukShift cartDetail in sortedcartDetailCanceled)
                            {
                                // Add varian to the cart detail name if it's not null
                                string displayMenuName = cartDetail.menu_name;
                                if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                    DrawSimpleLine(cartDetail.varian, "");
                                }
                                else
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                }

                                DrawSimpleLine(cartDetail.qty.ToString(),
                                    string.Format("{0:n0}", cartDetail.total_price));
                            }

                            DrawSimpleLine("Item Cancel Qty", dataShifts.totalCanceledQty.ToString());
                            DrawSimpleLine("Item Cancel Amount",
                                string.Format("{0:n0}", dataShifts.totalCartCanceledAmount));
                        }

                        if (refundDetails.Count != 0)
                        {
                            DrawSeparator();
                            DrawCenterText("REFUND ITEMS", boldFont);
                            IOrderedEnumerable<RefundDetailStrukShift> sortedrefundDetails = refundDetails.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman"))
                                    {
                                        return 1;
                                    }

                                    if (x.menu_type.Contains("Additional Minuman"))
                                    {
                                        return 2;
                                    }

                                    if (x.menu_type.Contains("Makanan"))
                                    {
                                        return 3;
                                    }

                                    if (x.menu_type.Contains("Additional Makanan"))
                                    {
                                        return 4;
                                    }

                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);
                            //foreach (var cartDetail in refundDetails)
                            foreach (RefundDetailStrukShift cartDetail in sortedrefundDetails)
                            {
                                // Add varian to the cart detail name if it's not null
                                string displayMenuName = cartDetail.menu_name;
                                if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                    DrawSimpleLine(cartDetail.varian, "");
                                }
                                else
                                {
                                    DrawSimpleLine(cartDetail.menu_name, "");
                                }

                                DrawSimpleLine(cartDetail.qty_refund_item.ToString(),
                                    string.Format("{0:n0}", cartDetail.total_refund_price));
                            }

                            DrawSimpleLine("Item Refund Qty", dataShifts.totalRefundQty.ToString());
                            DrawSimpleLine("Item Refund Amount",
                                string.Format("{0:n0}", dataShifts.totalCartRefundAmount));
                        }

                        DrawSeparator();
                        DrawCenterText("CASH MANAGEMENT", BigboldFont);
                        DrawSeparator();
                        if (expenditures.Count != 0)
                        {
                            DrawLeftText("EXPENSE", boldFont);
                            foreach (ExpenditureStrukShift expense in expenditures)
                            {
                                DrawSimpleLine(expense.description, string.Format("{0:n0}", expense.nominal));
                            }
                        }

                        DrawSimpleLine("Expected Ending Cash",
                            string.Format("{0:n0}", dataShifts.ending_cash_expected));
                        DrawSimpleLine("Actual Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_actual));
                        DrawSimpleLine("Cash Difference", string.Format("{0:n0}", dataShifts.cash_difference));
                        DrawSeparator();
                        DrawLeftText("DISCOUNTS", boldFont);
                        DrawSimpleLine("All Discount items",
                            string.Format("{0:n0}", dataShifts.discount_amount_per_items));
                        DrawSimpleLine("All Discount Cart",
                            string.Format("{0:n0}", dataShifts.discount_amount_per_items));
                        DrawSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", dataShifts.discount_total_amount));
                        DrawSeparator();
                        DrawCenterText("PAYMENT DETAIL", BigboldFont);
                        DrawSeparator();
                        foreach (PaymentDetailStrukShift paymentDetail in paymentDetails)
                        {
                            DrawLeftText(paymentDetail.payment_category, boldFont);
                            foreach (PaymentTypeDetailStrukShift paymentType in paymentDetail.payment_type_detail)
                            {
                                DrawSimpleLine(paymentType.payment_type,
                                    string.Format("{0:n0}", paymentType.total_payment));
                            }

                            DrawSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount));
                            DrawSeparator();
                        }

                        DrawSimpleLine("TOTAL TRANSACTION", string.Format("{0:n0}", dataShifts.total_transaction));
                        DrawSeparator();
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        // Struct Refund
        public async Task PrintModelRefund(DataRefundStruk datas,
            List<RefundDetailStruk> refundDetailStruks,
            int totalTransactions)
        {
            try
            {
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync(); // Load additional settings

                foreach (KeyValuePair<string, string> printer in printerSettings.ToList())
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintModelRefund(datas, refundDetailStruks, totalTransactions,
                            printerId, printerName);
                        continue;
                    }

                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        Stream stream = Stream.Null;

                        try
                        {
                            if (IPAddress.TryParse(printerName, out _))
                            {
                                // Connect via LAN
                                TcpClient client = new(printerName, 9100);
                                stream = client.GetStream();
                            }
                            else
                            {
                                // Connect via Bluetooth
                                BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                                if (printerDevice == null)
                                {
                                    continue;
                                }

                                BluetoothClient client = new();
                                BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress,
                                    BluetoothService.SerialPort);

                                if (!printerDevice.Authenticated) // cek sudah paired?
                                {
                                    if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                    {
                                        NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                        continue;
                                    }
                                }

                                client.Connect(endpoint);
                                stream = client.GetStream();
                            }

                            PrintRefundReceipt(stream, datas, refundDetailStruks, totalTransactions, printerName);
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
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

        private void PrintRefundReceipt(Stream stream, DataRefundStruk datas,
            List<RefundDetailStruk> refundDetailStruks, int totalTransactions, string printerName)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            byte[] InitPrinter = { 0x1B, 0x40 }; // Initialize printer
            byte[] NewLine = { 0x0A }; // New line

            // Write initialization bytes
            stream.Write(InitPrinter, 0, InitPrinter.Length);

            // Print logo (assuming logo is already in a proper format for the printer)

            // Print the rest of the receipt
            //string strukText = "\n" + kodeHeksadesimalBold + CenterText("No. " + totalTransactions.ToString()) + "\n";
            string strukText = kodeHeksadesimalNormal;
            strukText += SEPARATOR;
            strukText += kodeHeksadesimalBold + CenterText("REFUND");
            strukText += CenterText("Receipt No. " + datas?.receipt_number);
            strukText += kodeHeksadesimalNormal;
            strukText += SEPARATOR;
            if (datas?.invoice_due_date != null)
            {
                strukText += CenterText(datas?.invoice_due_date);
            }

            strukText += "Name: " + datas?.customer_name + "\n";

            IEnumerable<string> servingTypes = refundDetailStruks.Select(cd => cd.serving_type_name).Distinct();

            foreach (string servingType in servingTypes)
            {
                List<RefundDetailStruk> itemsForServingType =
                    refundDetailStruks.Where(cd => cd.serving_type_name == servingType).ToList();
                if (itemsForServingType.Count == 0)
                {
                    continue;
                }

                strukText += SEPARATOR;
                strukText += CenterText(servingType);
                strukText += "\n";

                foreach (RefundDetailStruk refundDetail in itemsForServingType)
                {
                    strukText += FormatSimpleLine(printerName, refundDetail.qty_refund_item + " " + refundDetail.menu_name,
                        string.Format("{0:n0}", refundDetail.menu_price));
                    if (!string.IsNullOrEmpty(refundDetail.varian))
                    {
                        strukText += "   Varian: " + refundDetail.varian + "\n";
                    }

                    if (!string.IsNullOrEmpty(refundDetail.note_item))
                    {
                        strukText += "   Note: " + refundDetail.note_item + "\n";
                    }

                    if (!string.IsNullOrEmpty(refundDetail.discount_code))
                    {
                        strukText += "   Discount Code: " + refundDetail.discount_code + "\n";
                    }

                    if (refundDetail.discounted_price.HasValue && refundDetail.discounted_price != 0)
                    {
                        strukText += "   Total Discount: " + (refundDetail.discounts_is_percent != "1"
                            ? string.Format("{0:n0}", refundDetail.discounts_value)
                            : refundDetail.discounts_value + " %") + "\n";
                    }

                    strukText += "   Payment Type Refund: " + refundDetail.payment_type_name + "\n";
                    strukText += "   Total Refund: " + string.Format("{0:n0}", refundDetail.total_refund_price) + "\n";
                    strukText += "   Refund Reason: " + refundDetail.refund_reason_item + "\n";
                    strukText += "\n";
                }
            }

            strukText += SEPARATOR;
            strukText += FormatSimpleLine(printerName, "Subtotal", string.Format("{0:n0}", datas.subtotal)) + "\n";
            if (!string.IsNullOrEmpty(datas.discount_code))
            {
                strukText += "Discount Code: " + datas.discount_code + "\n";
            }

            if (datas.discounts_value.HasValue && datas.discounts_value != 0)
            {
                strukText += FormatSimpleLine(printerName, "Discount Value",
                    datas.discounts_is_percent != "1"
                        ? string.Format("{0:n0}", datas.discounts_value)
                        : datas.discounts_value + " %") + "\n";
            }

            strukText += FormatSimpleLine(printerName, "Total", string.Format("{0:n0}", datas.total)) + "\n";
            strukText += "Payment Type: " + datas.payment_type + "\n";
            if (!string.IsNullOrEmpty(datas.refund_reason))
            {
                strukText += "Refund Reason: " + datas.refund_reason + "\n";
            }

            strukText += FormatSimpleLine(printerName, "Total Refund", string.Format("{0:n0}", datas.total_refund)) + "\n";
            strukText += SEPARATOR;
            strukText += CenterText("Meja No. " + datas.customer_seat);
            strukText += SEPARATOR;
            strukText += CenterText("Powered By Dastrevas");

            string NomorUrut = "\n" + kodeHeksadesimalSizeBesar + kodeHeksadesimalBold +
                               CenterText("No. " + totalTransactions) + "\n\n\n    ";

            byte[] buffer1 = Encoding.UTF8.GetBytes(NomorUrut);
            byte[] buffer = Encoding.UTF8.GetBytes(strukText);

            stream.Write(buffer1, 0, buffer1.Length);
            PrintLogo(stream, "icon\\OutletLogo.bmp", logoSize); // Smaller logo size
            stream.Write(buffer, 0, buffer.Length);
            //PrintLogo(stream, "icon\\DT-Logo.bmp", logoCredit); // Smaller logo size
            stream.Write(NewLine, 0, NewLine.Length);

            stream.Flush();
        }

        public async Task Ex_PrintModelRefund
        (DataRefundStruk datas,
            List<RefundDetailStruk> refundDetailStruks,
            int totalTransactions,
            string printerId,
            string printerName)
        {
            try
            {
                Stream stream = null;

                // Koneksi printer langsung di sini
                if (IPAddress.TryParse(printerName, out _))
                {
                    TcpClient client = new(printerName, 9100);
                    stream = client.GetStream();
                }
                else
                {
                    await RetryPolicyAsync(async () =>
                    {
                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            return false;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        if (!printerDevice.Authenticated) // cek sudah paired?
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                return false;
                            }
                        }

                        client.Connect(endpoint);
                        stream = client.GetStream();
                        return true;
                    }, 3);
                }

                // Struck Checker
                if (ShouldPrint(printerId, "Kasir"))
                {
                    PrintDocument printDocument = new();
                    printDocument.PrintPage += (sender, e) =>
                    {
                        Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                        // Mengatur font normal dan tebal
                        Font normalFont =
                            new("Arial", 8,
                                FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                        Font boldFont = new("Arial", 8, FontStyle.Bold);
                        Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                        Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                        float leftMargin = 5; // Margin kiri (dalam pixel)
                        float rightMargin = 5; // Margin kanan (dalam pixel)
                        float topMargin = 5; // Margin atas (dalam pixel)
                        float yPos = topMargin;

                        // Lebar kertas thermal 58mm, sesuaikan dengan margin
                        float paperWidth =
                            58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                        float printableWidth = paperWidth - leftMargin - rightMargin;


                        // Fungsi untuk format teks kiri dan kanan
                        void DrawSimpleLine(string textLeft, string textRight)
                        {
                            SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                            SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                            e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                            e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                leftMargin + printableWidth - sizeRight.Width, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                        void DrawCenterText(string text, Font font)
                        {
                            if (text == null)
                            {
                                text = string.Empty;
                            }

                            if (font == null)
                            {
                                throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                    leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }

                        // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                        void DrawLeftText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }


                        // Fungsi untuk menggambar garis pemisah
                        void DrawSeparator()
                        {
                            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        void DrawSpace()
                        {
                            yPos += normalFont
                                .GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                        }


                        // Fungsi untuk mendapatkan dan mengonversi gambar logo ke hitam dan putih
                        Image GetLogoImage(string path)
                        {
                            Image img = Image.FromFile(path);
                            Bitmap bmp = new(img.Width, img.Height);
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                ColorMatrix colorMatrix = new(new[]
                                {
                                    new[] { 0.3f, 0.3f, 0.3f, 0, 0 }, new[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                                    new[] { 0.11f, 0.11f, 0.11f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 },
                                    new float[] { 0, 0, 0, 0, 1 }
                                });
                                ImageAttributes attributes = new();
                                attributes.SetColorMatrix(colorMatrix);
                                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width,
                                    img.Height, GraphicsUnit.Pixel, attributes);
                            }

                            return bmp;
                        }

                        // Menambahkan logo dan teks "Powered by" di akhir struk
                        void DrawPoweredByLogo(string path)
                        {
                            if (!File.Exists(path)) { return; }

                            // Menambahkan jarak sebelum mencetak teks dan logo
                            float spaceBefore = 50; // Jarak dalam pixel, sesuaikan dengan kebutuhan
                            yPos += spaceBefore;

                            // Mengukur teks "Powered by Your Company"
                            string poweredByText = "Powered by Dastrevas";
                            SizeF textSize = e.Graphics.MeasureString(poweredByText, normalFont);

                            // Gambar teks
                            float textX = leftMargin + ((printableWidth - textSize.Width) / 2);
                            e.Graphics.DrawString(poweredByText, normalFont, Brushes.Black, textX, yPos);

                            // Sesuaikan yPos untuk logo
                            yPos += textSize.Height;

                            // Menggambar logo
                            Image logoPoweredBy = GetLogoImage(path);
                            float targetWidth = 35; // Ukuran lebar logo dalam pixel, sesuaikan dengan kebutuhan
                            float scaleFactor = targetWidth / logoPoweredBy.Width;
                            float logoHeight = logoPoweredBy.Height * scaleFactor;

                            float logoX = leftMargin + ((printableWidth - targetWidth) / 2);
                            e.Graphics.DrawImage(logoPoweredBy, logoX, yPos, targetWidth, logoHeight);

                            spaceBefore = 5;
                            yPos += spaceBefore;
                            // Sesuaikan yPos untuk elemen berikutnya
                            yPos += logoHeight;
                        }


                        DrawCenterText("No. " + totalTransactions, NomorAntrian);

                        // Path ke logo Powered by Anda
                        string poweredByLogoPath = "icon\\DT-Logo.bmp"; // Ganti dengan path logo Powered by Anda

                        // Menambahkan logo di bagian atas dengan ukuran yang proporsional
                        string logoPath = "icon\\OutletLogo.bmp"; // Ganti dengan path logo Anda
                        Image logo = GetLogoImage(logoPath);
                        float logoTargetWidthMm = 25; // Lebar target logo dalam mm
                        float logoTargetWidthPx = logoTargetWidthMm / 25.4f * 100; // Konversi ke pixel

                        // Hitung tinggi logo berdasarkan lebar yang diinginkan dengan mempertahankan rasio aspek
                        float scaleFactor = logoTargetWidthPx / logo.Width;
                        float logoHeight = logo.Height * scaleFactor;
                        float logoX = leftMargin + ((printableWidth - logoTargetWidthPx) / 2);

                        // Gambar logo dengan ukuran yang diubah
                        e.Graphics.DrawImage(logo, logoX, yPos, logoTargetWidthPx, logoHeight);
                        yPos += logoHeight + 5; // Menambahkan jarak setelah logo

                        // Path ke logo Powered by Anda

                        string nomorMeja = "Meja No." + datas?.customer_seat;
                        //DrawCenterText(datas?.outlet_name, BigboldFont);
                        //DrawCenterText(datas?.outlet_address, normalFont);
                        //DrawCenterText(datas?.outlet_phone_number, normalFont);

                        DrawSeparator();

                        //Struct Checker
                        DrawCenterText("REFUND", BigboldFont);
                        DrawCenterText("Receipt No. " + datas?.receipt_number, normalFont);
                        DrawSeparator();
                        if (datas?.invoice_due_date != null)
                        {
                            DrawCenterText(datas?.invoice_due_date, normalFont);
                        }

                        DrawSpace();
                        DrawLeftText(datas?.customer_name, normalFont);

                        // Iterate through cart details and group by serving_type_name
                        IEnumerable<string> servingTypes =
                            refundDetailStruks.Select(cd => cd.serving_type_name).Distinct();

                        foreach (string servingType in servingTypes)
                        {
                            // Filter cart details by serving_type_name
                            List<RefundDetailStruk> itemsForServingType = refundDetailStruks
                                .Where(cd => cd.serving_type_name == servingType).ToList();

                            // Skip if there are no items for this serving type
                            if (itemsForServingType.Count == 0)
                            {
                                continue;
                            }

                            // Add a section for the serving type
                            DrawSeparator();
                            DrawCenterText(servingType, normalFont);
                            DrawSpace();

                            // Iterate through items for this serving type
                            foreach (RefundDetailStruk refundDetail in itemsForServingType)
                            {
                                DrawSimpleLine(refundDetail.qty_refund_item + " " + refundDetail.menu_name,
                                    string.Format("{0:n0}", refundDetail.menu_price));
                                // Add detail items
                                if (!string.IsNullOrEmpty(refundDetail.varian))
                                {
                                    DrawLeftText("   Varian: " + refundDetail.varian, normalFont);
                                }

                                if (!string.IsNullOrEmpty(refundDetail.note_item))
                                {
                                    DrawLeftText("   Note: " + refundDetail.note_item, normalFont);
                                }

                                if (!string.IsNullOrEmpty(refundDetail.discount_code))
                                {
                                    DrawLeftText("   Discount Code: " + refundDetail.discount_code, normalFont);
                                }

                                if (refundDetail.discounted_price.HasValue && refundDetail.discounted_price != 0)
                                {
                                    DrawSimpleLine("   Total Discount",
                                        refundDetail.discounts_is_percent != "1"
                                            ? string.Format("{0:n0}", refundDetail.discounts_value)
                                            : refundDetail.discounts_value + " %");
                                }

                                DrawLeftText("   Payment Type Refund: " + refundDetail.payment_type_name, normalFont);
                                DrawSimpleLine("   Total Refund",
                                    string.Format("{0:n0}", refundDetail.total_refund_price));
                                DrawLeftText("   Refund Reason: " + refundDetail.refund_reason_item, normalFont);

                                // Add an empty line between items
                                DrawSpace();
                            }
                        }

                        DrawSeparator();
                        DrawSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal));
                        if (!string.IsNullOrEmpty(datas.discount_code))
                        {
                            DrawLeftText("Discount Code: " + datas.discount_code, normalFont);
                        }

                        if (datas.discounts_value.HasValue && datas.discounts_value != 0)
                        {
                            DrawSimpleLine("Discount Value",
                                datas.discounts_is_percent != "1"
                                    ? string.Format("{0:n0}", datas.discounts_value)
                                    : datas.discounts_value + " %");
                        }

                        DrawSimpleLine("Total", string.Format("{0:n0}", datas.total));
                        DrawLeftText("Payment Type: " + datas.payment_type, normalFont);
                        if (!string.IsNullOrEmpty(datas.refund_reason))
                        {
                            DrawLeftText("Refund Reason: " + datas.refund_reason, normalFont);
                        }

                        DrawSimpleLine("Total Refund ", string.Format("{0:n0}", datas.total_refund));
                        DrawSeparator();
                        DrawPoweredByLogo(poweredByLogoPath);
                        DrawSpace();
                        DrawCenterText(nomorMeja, NomorAntrian);
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        // Struct InputPin cetak ulang struk
        public async Task PrintModelInputPin(DataRestruk datas,
            List<CartDetailRestruk> cartDetails,
            List<RefundDetailRestruk> cartRefundDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int totalTransactions)
        {
            try
            {
                if (totalTransactions == null) { totalTransactions = 0; }
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync(); // Load additional settings

                List<KeyValuePair<string, string>> printerSettingsCopy;
                lock (printerSettings)
                {
                    printerSettingsCopy = printerSettings.ToList(); // Create a copy of the collection
                }

                foreach (KeyValuePair<string, string> printer in printerSettingsCopy)
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }

                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintModelInputPin(datas, cartDetails, cartRefundDetails, canceledItems, totalTransactions,
                            printerId, printerName
                        );
                        continue;
                    }

                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        await PrintInputPinReceipt(datas, cartDetails, cartRefundDetails, canceledItems,
                                totalTransactions, printerName);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async Task PrintInputPinReceipt(
    DataRestruk datas,
    List<CartDetailRestruk> cartDetails,
    List<RefundDetailRestruk> cartRefundDetails,
    List<CanceledItemStrukCustomerRestruk> canceledItems,
    int totalTransactions,
    string printerName)
        {
            Stream stream = Stream.Null;

            try
            {
                // Establish connection
                stream = await EstablishPrinterConnection(printerName);

                // Hitung kapasitas optimal
                int totalItems = (cartDetails?.Count ?? 0) +
                                 (canceledItems?.Count ?? 0) +
                                 (cartRefundDetails?.Count ?? 0);
                var capacity = GetOptimalCapacity(totalItems);
                var strukBuilder = new StringBuilder(capacity);

                // Kode Heksadesimal
                string kodeHeksadesimalBold = "\x1B\x45\x01";
                string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

                // Header
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}{1}",
                    kodeHeksadesimalBold,
                    CenterText("RE-PRINT STRUCT"));
                strukBuilder.Append(kodeHeksadesimalNormal);

                strukBuilder.AppendFormat("{0}",
                    CenterText($"Receipt No. {datas?.receipt_number}"));

                if (datas?.invoice_due_date != null)
                {
                    strukBuilder.AppendFormat("{0}",
                        CenterText(datas.invoice_due_date));
                }

                // Member/Customer Information
                if (datas != null && (datas.member_id != 0 || !string.IsNullOrEmpty(datas.member_name)))
                {
                    strukBuilder.AppendFormat("Name Member: {0}\n", datas.member_name ?? "-");
                    strukBuilder.AppendFormat("Number Member: {0}\n", datas.member_phone_number ?? "-");
                    strukBuilder.AppendFormat("Point Member: {0}\n",
                        datas.member_point.ToString("#,#") ?? "0");

                    if (!string.IsNullOrEmpty(datas.member_use_point.ToString()) && datas.member_use_point > 0)
                    {
                        strukBuilder.AppendFormat("Point Member Terpakai: {0}\n", datas.member_use_point);
                    }
                }
                else
                {
                    strukBuilder.AppendFormat("Name: {0}\n",
                        datas?.customer_name ?? "Walk-in Customer");
                }

                // Ordered Items
                if (cartDetails?.Any() == true)
                {
                    var servingTypes = cartDetails
                        .Select(cd => cd.serving_type_name)
                        .Distinct();

                    foreach (string servingType in servingTypes)
                    {
                        var itemsForServingType = cartDetails
                            .Where(cd => cd.serving_type_name == servingType)
                            .ToList();

                        if (!itemsForServingType.Any()) continue;

                        strukBuilder.Append(SEPARATOR);
                        strukBuilder.AppendFormat("{0}", CenterText("ORDERED"));
                        strukBuilder.AppendFormat("{0}\n", CenterText(servingType));

                        foreach (var cartDetail in itemsForServingType)
                        {
                            strukBuilder.Append(kodeHeksadesimalBold);
                            strukBuilder.AppendFormat("{0}\n", cartDetail.menu_name);
                            strukBuilder.Append(kodeHeksadesimalNormal);

                            strukBuilder.AppendFormat("{0}\n",
                                FormatSimpleLine(printerName,
                                    $"@{cartDetail.price:n0} x{cartDetail.qty}",
                                    $"{cartDetail.total_price:n0}"));

                            if (!string.IsNullOrEmpty(cartDetail.varian))
                            {
                                strukBuilder.AppendFormat("  Varian: {0}\n", cartDetail.varian);
                            }
                            if (!string.IsNullOrEmpty(cartDetail.note_item))
                            {
                                strukBuilder.AppendFormat("  Note: {0}\n", cartDetail.note_item);
                            }

                            if (!string.IsNullOrEmpty(cartDetail.discount_code))
                            {
                                strukBuilder.AppendFormat("  Discount Code: {0}\n", cartDetail.discount_code);
                            }

                            if (cartDetail.discounts_value != null && Convert.ToDecimal(cartDetail.discounts_value) != 0)
                            {
                                strukBuilder.AppendFormat("  Discount Value: {0}\n",
    cartDetail.discounts_is_percent.ToString() != "1"
        ? string.Format("{0:n0}", cartDetail.discounts_value.ToString())
        : $"{cartDetail.discounts_value} %");
                            }

                        }
                    }
                }

                // Canceled Items
                if (canceledItems?.Any() == true)
                {
                    var servingTypesCancel = canceledItems
                        .Select(cd => cd.serving_type_name)
                        .Distinct();

                    strukBuilder.Append(SEPARATOR);
                    strukBuilder.AppendFormat("{0}", CenterText("CANCELED"));

                    foreach (string servingType in servingTypesCancel)
                    {
                        var itemsForServingType = canceledItems
                            .Where(cd => cd.serving_type_name == servingType)
                            .ToList();

                        if (!itemsForServingType.Any()) continue;

                        strukBuilder.AppendFormat("{0}\n", CenterText(servingType));

                        foreach (var cancelItem in itemsForServingType)
                        {
                            strukBuilder.Append(kodeHeksadesimalBold);
                            strukBuilder.AppendFormat("{0}\n", cancelItem.menu_name);
                            strukBuilder.Append(kodeHeksadesimalNormal);

                            strukBuilder.AppendFormat("{0}\n",
                                FormatSimpleLine(printerName,
                                    $"@{cancelItem.price:n0} x{cancelItem.qty}",
                                    $"{cancelItem.total_price:n0}"));

                            if (!string.IsNullOrEmpty(cancelItem.varian))
                            {
                                strukBuilder.AppendFormat("  Varian: {0}\n", cancelItem.varian);
                            }

                            if (!string.IsNullOrEmpty(cancelItem.note_item.ToString()))
                            {
                                strukBuilder.AppendFormat("  Note: {0}\n", cancelItem.note_item);
                            }

                            if (!string.IsNullOrEmpty(cancelItem.discount_code))
                            {
                                strukBuilder.AppendFormat("  Discount Code: {0}\n", cancelItem.discount_code);
                            }

                            if (cancelItem.discounts_value != null && Convert.ToDecimal(cancelItem.discounts_value) != 0)
                            {
                                strukBuilder.AppendFormat("  Discount Value: {0}\n",
                                    cancelItem.discounts_is_percent.ToString() != "1"
                                        ? string.Format("{0:n0}", cancelItem.discounts_value.ToString())
                                        : $"{cancelItem.discounts_value} %");
                            }
                        }
                    }
                }

                // Refund Details
                if (cartRefundDetails?.Any() == true)
                {
                    var servingTypesRefund = cartRefundDetails
                        .Select(cd => cd.serving_type_name)
                        .Distinct();

                    strukBuilder.Append(SEPARATOR);
                    strukBuilder.AppendFormat("{0}", CenterText("REFUNDED"));

                    foreach (string servingType in servingTypesRefund)
                    {
                        var itemsForServingType = cartRefundDetails
                            .Where(cd => cd.serving_type_name == servingType)
                            .ToList();

                        if (!itemsForServingType.Any()) continue;

                        strukBuilder.AppendFormat("{0}\n", CenterText(servingType));

                        foreach (var cartDetail in itemsForServingType)
                        {
                            strukBuilder.Append(kodeHeksadesimalBold);
                            strukBuilder.AppendFormat("{0}\n", cartDetail.menu_name);
                            strukBuilder.Append(kodeHeksadesimalNormal);

                            strukBuilder.AppendFormat("{0}\n",
                                FormatSimpleLine(printerName,
                                    $"@{cartDetail.menu_price:n0} x{cartDetail.qty_refund_item}",
                                    $"{cartDetail.total_refund_price:n0}"));

                            if (!string.IsNullOrEmpty(cartDetail.varian))
                            {
                                strukBuilder.AppendFormat("  Varian: {0}\n", cartDetail.varian);
                            }

                            if (!string.IsNullOrEmpty(cartDetail.note_item))
                            {
                                strukBuilder.AppendFormat("  Note: {0}\n", cartDetail.note_item);
                            }

                            if (!string.IsNullOrEmpty(cartDetail.discount_code))
                            {
                                strukBuilder.AppendFormat("  Discount Code: {0}\n", cartDetail.discount_code);
                            }

                            if (cartDetail.discounted_price != null)
                            {
                                strukBuilder.AppendFormat("  Total Discount: {0:n0}\n", cartDetail.discounted_price);
                            }

                            if (!string.IsNullOrEmpty(cartDetail.payment_type_name))
                            {
                                strukBuilder.AppendFormat("  Payment Type: {0}\n", cartDetail.payment_type_name);
                            }

                            if (!string.IsNullOrEmpty(cartDetail.refund_reason_item))
                            {
                                strukBuilder.AppendFormat("  Refund Reason: {0}\n", cartDetail.refund_reason_item);
                            }
                        }
                    }
                }

                // Tambahkan detail pembayaran, total, dll setelah refund
                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Subtotal", $"{datas.subtotal:n0}"));

                if (!string.IsNullOrEmpty(datas.discount_code))
                {
                    strukBuilder.AppendFormat("Discount Code: {0}\n", datas.discount_code);
                }

                if (datas.discounts_value != null)
                {
                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Discount Value",
                            datas.discounts_is_percent != "1"
                                ? string.Format("{0:n0}", datas.discounts_value)
                                : $"{datas.discounts_value} %"));
                }

                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Total", $"{datas.total:n0}"));

                strukBuilder.AppendFormat("Payment Type: {0}\n", datas.payment_type);

                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Cash", $"{datas.customer_cash:n0}"));

                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Change", $"{datas.customer_change:n0}"));

                if (datas.total_refund != null)
                {
                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Total Refund", $"{datas.total_refund:n0}"));
                }

                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}",
                    CenterText($"Meja No. {datas.customer_seat}"));
                strukBuilder.AppendFormat("{0}",
                    CenterText("Powered By Dastrevas"));

                // Konversi dan Cetak
                string nomorUrut = $"\n{kodeHeksadesimalSizeBesar}{kodeHeksadesimalBold}" +
                                   CenterText($"No. {totalTransactions}") + "\n";

                byte[] bufferNomorUrut = Encoding.UTF8.GetBytes(nomorUrut);
                byte[] bufferStruk = Encoding.UTF8.GetBytes(strukBuilder.ToString());

                stream.Write(bufferNomorUrut, 0, bufferNomorUrut.Length);
                PrintLogo(stream, "icon\\OutletLogo.bmp", logoSize);
                stream.Write(bufferStruk, 0, bufferStruk.Length);

                // Footer
                byte[] bufferFooter = Encoding.UTF8.GetBytes("\n\n\n\n\n");
                stream.Write(bufferFooter, 0, bufferFooter.Length);

                stream.Flush();
            }
            catch (Exception ex)
            {
                // Logging dan error handling
                LoggerUtil.LogError(ex, $"Error in PrintInputPinReceipt: {ex.Message}");
                throw;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }
        }
        private void PrintLogo(Stream stream, string logoPath, int targetWidthPx)
        {
            // Validasi input dasar
            if (stream == null)
            {
                LoggerUtil.LogWarning("Stream is null. Cannot print logo.");
                return;
            }

            if (string.IsNullOrWhiteSpace(logoPath))
            {
                LoggerUtil.LogWarning("Logo path is empty or null.");
                return;
            }

            try
            {
                // Periksa keberadaan file
                if (!File.Exists(logoPath))
                {
                    LoggerUtil.LogWarning($"Logo file not found: {logoPath}");
                    return;
                }

                // Load gambar
                using (Image logo = Image.FromFile(logoPath))
                using (Bitmap bmp = new Bitmap(logo))
                {
                    // Resize bitmap
                    Bitmap resizedBitmap = ResizeBitmap(bmp, targetWidthPx);

                    // Konversi ke format printer
                    byte[] logoBytes = ConvertBitmapToRasterFormat(resizedBitmap, targetWidthPx);

                    // Tulis ke stream
                    stream.Write(logoBytes, 0, logoBytes.Length);
                }
            }
            catch (FileNotFoundException ex)
            {
                LoggerUtil.LogError(ex, $"File not found: {logoPath}");
            }
            catch (OutOfMemoryException ex)
            {
                LoggerUtil.LogError(ex, $"Invalid image file: {logoPath}");
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error printing logo: {ex.Message}");
            }
        }

        private Bitmap ResizeBitmap(Bitmap bmp, int targetWidthPx)
        {
            int targetHeightPx = (int)((float)bmp.Height / bmp.Width * targetWidthPx);
            Bitmap resizedBmp = new(targetWidthPx, targetHeightPx);
            using (Graphics g = Graphics.FromImage(resizedBmp))
            {
                g.DrawImage(bmp, 0, 0, targetWidthPx, targetHeightPx);
            }

            return resizedBmp;
        }
        private byte[] ConvertBitmapToRasterFormat(Bitmap bitmap, int targetWidthPx)
        {
            int printerWidthPx = 384; // Assuming a typical thermal printer width of 384px
            int paddingPx = (printerWidthPx - targetWidthPx) / 2;
            int width = bitmap.Width;
            int height = bitmap.Height;
            int bytesPerLine = (printerWidthPx + 7) / 8; // Adjust for total width including padding
            List<byte> imageData = new();

            // ESC/POS raster bitmap header
            imageData.Add(0x1D); // GS
            imageData.Add(0x76); // v
            imageData.Add(0x30); // 0
            imageData.Add(0x00); // m (normal mode)

            // Width and height in bytes (little-endian)
            imageData.Add((byte)(bytesPerLine % 256));
            imageData.Add((byte)(bytesPerLine / 256));
            imageData.Add((byte)(height % 256));
            imageData.Add((byte)(height / 256));

            for (int y = 0; y < height; y++)
            {
                // Add padding to the left to center the image
                for (int p = 0; p < paddingPx / 8; p++)
                {
                    imageData.Add(0x00);
                }

                for (int x = 0; x < width; x += 8)
                {
                    byte byteData = 0x00;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if (x + bit < width)
                        {
                            Color pixelColor = bitmap.GetPixel(x + bit, y);
                            if (pixelColor.GetBrightness() < 0.5)
                            {
                                byteData |= (byte)(0x80 >> bit);
                            }
                        }
                    }

                    imageData.Add(byteData);
                }

                // Add padding to the right to center the image
                for (int p = 0; p < paddingPx / 8; p++)
                {
                    imageData.Add(0x00);
                }
            }

            return imageData.ToArray();
        }


        public async Task Ex_PrintModelInputPin
        (DataRestruk datas,
            List<CartDetailRestruk> cartDetails,
            List<RefundDetailRestruk> cartRefundDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int totalTransactions,
            string printerId,
            string printerName)
        {
            try
            {
                Stream stream = null;

                // Koneksi printer langsung di sini
                if (IPAddress.TryParse(printerName, out _))
                {
                    TcpClient client = new(printerName, 9100);
                    stream = client.GetStream();
                }
                else
                {
                    await RetryPolicyAsync(async () =>
                    {
                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            return false;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        if (!printerDevice.Authenticated) // cek sudah paired?
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                return false;
                            }
                        }

                        client.Connect(endpoint);
                        stream = client.GetStream();
                        return true;
                    }, 3);
                }

                // Struck Checker
                if (ShouldPrint(printerId, "Kasir"))
                {
                    PrintDocument printDocument = new();
                    printDocument.PrintPage += (sender, e) =>
                    {
                        Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                        // Mengatur font normal dan tebal
                        Font normalFont =
                            new("Arial", 8,
                                FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                        Font boldFont = new("Arial", 8, FontStyle.Bold);
                        Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                        Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                        float leftMargin = 5; // Margin kiri (dalam pixel)
                        float rightMargin = 5; // Margin kanan (dalam pixel)
                        float topMargin = 5; // Margin atas (dalam pixel)
                        float yPos = topMargin;

                        // Lebar kertas thermal 58mm, sesuaikan dengan margin
                        float paperWidth =
                            58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                        float printableWidth = paperWidth - leftMargin - rightMargin;


                        // Fungsi untuk format teks kiri dan kanan
                        void DrawSimpleLine(string textLeft, string textRight)
                        {
                            SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                            SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                            e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                            e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                leftMargin + printableWidth - sizeRight.Width, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                        void DrawCenterText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                    leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }

                        // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                        void DrawLeftText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }


                        // Fungsi untuk menggambar garis pemisah
                        void DrawSeparator()
                        {
                            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        void DrawSpace()
                        {
                            yPos += normalFont
                                .GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                        }


                        // Fungsi untuk mendapatkan dan mengonversi gambar logo ke hitam dan putih
                        Image GetLogoImage(string path)
                        {
                            Image img = Image.FromFile(path);
                            Bitmap bmp = new(img.Width, img.Height);
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                ColorMatrix colorMatrix = new(new[]
                                {
                                    new[] { 0.3f, 0.3f, 0.3f, 0, 0 }, new[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                                    new[] { 0.11f, 0.11f, 0.11f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 },
                                    new float[] { 0, 0, 0, 0, 1 }
                                });
                                ImageAttributes attributes = new();
                                attributes.SetColorMatrix(colorMatrix);
                                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width,
                                    img.Height, GraphicsUnit.Pixel, attributes);
                            }

                            return bmp;
                        }

                        // Menambahkan logo dan teks "Powered by" di akhir struk
                        void DrawPoweredByLogo(string path)
                        {
                            if (!File.Exists(path)) { return; }

                            // Menambahkan jarak sebelum mencetak teks dan logo
                            float spaceBefore = 50; // Jarak dalam pixel, sesuaikan dengan kebutuhan
                            yPos += spaceBefore;

                            // Mengukur teks "Powered by Your Company"
                            string poweredByText = "Powered by Dastrevas";
                            SizeF textSize = e.Graphics.MeasureString(poweredByText, normalFont);

                            // Gambar teks
                            float textX = leftMargin + ((printableWidth - textSize.Width) / 2);
                            e.Graphics.DrawString(poweredByText, normalFont, Brushes.Black, textX, yPos);

                            // Sesuaikan yPos untuk logo
                            yPos += textSize.Height;

                            // Menggambar logo
                            Image logoPoweredBy = GetLogoImage(path);
                            float targetWidth = 35; // Ukuran lebar logo dalam pixel, sesuaikan dengan kebutuhan
                            float scaleFactor = targetWidth / logoPoweredBy.Width;
                            float logoHeight = logoPoweredBy.Height * scaleFactor;

                            float logoX = leftMargin + ((printableWidth - targetWidth) / 2);
                            e.Graphics.DrawImage(logoPoweredBy, logoX, yPos, targetWidth, logoHeight);

                            spaceBefore = 5;
                            yPos += spaceBefore;
                            // Sesuaikan yPos untuk elemen berikutnya
                            yPos += logoHeight;
                        }


                        DrawCenterText("No. " + totalTransactions, NomorAntrian);

                        // Path ke logo Powered by Anda
                        string poweredByLogoPath = "icon\\DT-Logo.bmp"; // Ganti dengan path logo Powered by Anda

                        // Menambahkan logo di bagian atas dengan ukuran yang proporsional
                        string logoPath = "icon\\OutletLogo.bmp"; // Ganti dengan path logo Anda
                        Image logo = GetLogoImage(logoPath);
                        float logoTargetWidthMm = 25; // Lebar target logo dalam mm
                        float logoTargetWidthPx = logoTargetWidthMm / 25.4f * 100; // Konversi ke pixel

                        // Hitung tinggi logo berdasarkan lebar yang diinginkan dengan mempertahankan rasio aspek
                        float scaleFactor = logoTargetWidthPx / logo.Width;
                        float logoHeight = logo.Height * scaleFactor;
                        float logoX = leftMargin + ((printableWidth - logoTargetWidthPx) / 2);

                        // Gambar logo dengan ukuran yang diubah
                        e.Graphics.DrawImage(logo, logoX, yPos, logoTargetWidthPx, logoHeight);
                        yPos += logoHeight + 5; // Menambahkan jarak setelah logo

                        // Path ke logo Powered by Anda

                        string nomorMeja = "Meja No." + datas?.customer_seat;
                        //DrawCenterText(datas?.outlet_name, BigboldFont);
                        //DrawCenterText(datas?.outlet_address, normalFont);
                        //DrawCenterText(datas?.outlet_phone_number, normalFont);

                        DrawSeparator();

                        //Struct Checker
                        DrawCenterText("CHECKER", BigboldFont);
                        DrawCenterText("Receipt No. " + datas?.receipt_number, normalFont);
                        DrawSeparator();
                        if (datas?.invoice_due_date != null)
                        {
                            DrawCenterText(datas?.invoice_due_date, normalFont);
                        }

                        DrawSpace();
                        DrawLeftText(datas?.customer_name, normalFont);


                        if (cartDetails.Count != 0)
                        {
                            // Iterate through cart details and group by serving_type_name
                            IEnumerable<string> servingTypes =
                                cartDetails.Select(cd => cd.serving_type_name).Distinct();

                            foreach (string servingType in servingTypes)
                            {
                                // Filter cart details by serving_type_name
                                List<CartDetailRestruk> itemsForServingType =
                                    cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                                DrawSeparator();
                                DrawCenterText("ORDERED", boldFont);

                                // Skip if there are no items for this serving type
                                if (itemsForServingType.Count == 0)
                                {
                                    continue;
                                }

                                // Add a section for the serving type
                                DrawSeparator();
                                DrawCenterText(servingType, normalFont);
                                DrawSpace();

                                // Iterate through items for this serving type
                                foreach (CartDetailRestruk cartDetail in itemsForServingType)
                                {
                                    DrawSimpleLine(cartDetail.qty + " " + cartDetail.menu_name,
                                        string.Format("{0:n0}", cartDetail.price));
                                    // Add detail items
                                    if (!string.IsNullOrEmpty(cartDetail.varian))
                                    {
                                        DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cartDetail.note_item))
                                    {
                                        DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                    {
                                        DrawLeftText("   Discount Code: " + cartDetail.discount_code, normalFont);
                                    }

                                    if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                                    {
                                        DrawSimpleLine("   Total Discount: ",
                                            string.Format("{0:n0}", cartDetail.discounted_price));
                                    }

                                    DrawSimpleLine("   Total Price:", string.Format("{0:n0}", cartDetail.total_price));
                                    DrawSpace();
                                }
                            }
                        }

                        if (canceledItems.Count != 0)
                        {
                            IEnumerable<string> servingTypesCancel =
                                canceledItems.Select(cd => cd.serving_type_name).Distinct();
                            DrawSeparator();
                            DrawCenterText("CANCELED", boldFont);

                            foreach (string servingType in servingTypesCancel)
                            {
                                List<CanceledItemStrukCustomerRestruk> itemsForServingType =
                                    canceledItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                if (itemsForServingType.Count == 0)
                                {
                                    continue;
                                }

                                DrawSeparator();
                                DrawCenterText(servingType, normalFont);
                                DrawSpace();

                                foreach (CanceledItemStrukCustomerRestruk cancelItem in itemsForServingType)
                                {
                                    DrawSimpleLine(cancelItem.qty + " " + cancelItem.menu_name,
                                        string.Format("{0:n0}", cancelItem.price));
                                    // Add detail items
                                    if (!string.IsNullOrEmpty(cancelItem.varian))
                                    {
                                        DrawLeftText("   Varian: " + cancelItem.varian, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                    {
                                        DrawLeftText("   Note: " + cancelItem.note_item, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cancelItem.discount_code))
                                    {
                                        DrawLeftText("   Discount Code: " + cancelItem.discount_code, normalFont);
                                    }

                                    if (cancelItem.discounted_price.HasValue && cancelItem.discounted_price != 0)
                                    {
                                        DrawSimpleLine("   Total Discount",
                                            string.Format("{0:n0}", cancelItem.discounted_price));
                                    }

                                    if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                    {
                                        DrawLeftText("   Reason: " + cancelItem.cancel_reason, normalFont);
                                    }

                                    // Add an empty line between items
                                    DrawSpace();
                                }
                            }
                        }

                        if (cartRefundDetails.Count != 0)
                        {
                            DrawSeparator();
                            DrawCenterText("REFUNDED", boldFont);
                            IEnumerable<string> servingTypesRefund =
                                cartRefundDetails.Select(cd => cd.serving_type_name).Distinct();
                            foreach (string servingTypeRefund in servingTypesRefund)
                            {
                                // Filter cart details by serving_type_name
                                List<RefundDetailRestruk> itemsForServingType = cartRefundDetails
                                    .Where(cd => cd.serving_type_name == servingTypeRefund).ToList();

                                // Skip if there are no items for this serving type
                                if (itemsForServingType.Count == 0)
                                {
                                    continue;
                                }

                                // Add a section for the serving type
                                DrawSeparator();
                                DrawCenterText(servingTypeRefund, normalFont);
                                DrawSpace();
                                // Iterate through items for this serving type
                                foreach (RefundDetailRestruk cartDetail in itemsForServingType)
                                {
                                    DrawSimpleLine(cartDetail.qty_refund_item + " " + cartDetail.menu_name,
                                        string.Format("{0:n0}", cartDetail.total_refund_price));
                                    // Add detail items
                                    if (!string.IsNullOrEmpty(cartDetail.varian))
                                    {
                                        DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cartDetail.note_item))
                                    {
                                        DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                    {
                                        DrawLeftText("   Discount Code: " + cartDetail.discount_code, normalFont);
                                    }

                                    if (cartDetail.discounted_price != null)
                                    {
                                        DrawSimpleLine("   Total Discount: ",
                                            string.Format("{0:n0}", cartDetail.discounted_price));
                                    }

                                    if (cartDetail.payment_type_name != null)
                                    {
                                        DrawLeftText("   Payment Type: " + cartDetail.payment_type_name, normalFont);
                                    }

                                    if (cartDetail.refund_reason_item != null)
                                    {
                                        DrawLeftText("   Refund Reason: " + cartDetail.refund_reason_item, normalFont);
                                    }

                                    // Add an empty line between items
                                    DrawSpace();
                                }
                            }
                        }

                        DrawSeparator();
                        DrawSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal));
                        if (!string.IsNullOrEmpty(datas.discount_code))
                        {
                            DrawSimpleLine("Discount Code", datas.discount_code);
                        }

                        if (datas.discounts_value != null)
                        {
                            DrawSimpleLine("Discount Value",
                                datas.discounts_is_percent != "1"
                                    ? string.Format("{0:n0}", datas.discounts_value)
                                    : datas.discounts_value + " %");
                        }

                        DrawSimpleLine("Total", string.Format("{0:n0}", datas.total));
                        DrawLeftText("Payment Type " + datas.payment_type, normalFont);
                        DrawSimpleLine("Cash", string.Format("{0:n0}", datas.customer_cash));
                        DrawSimpleLine("Change", string.Format("{0:n0}", datas.customer_change));
                        if (datas.total_refund != null)
                        {
                            DrawSimpleLine("Total Refund", string.Format("{0:n0}", datas.total_refund));
                        }

                        DrawSeparator();
                        DrawPoweredByLogo(poweredByLogoPath);
                        DrawSpace();
                        DrawCenterText(nomorMeja, NomorAntrian);
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        // Struct DataBill Simpan
        public async Task PrintModelDataBill(DataRestruk datas,
            List<CartDetailRestruk> cartDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int totalTransactions)
        {
            if (totalTransactions < 0 || totalTransactions == null)
            {
                totalTransactions = 0;
            }
            try
            {
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync(); // Load additional settings

                foreach (KeyValuePair<string, string> printer in
                         printerSettings.ToList()) // Membuat salinan dari koleksi
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }

                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintModelDataBill(datas, cartDetails, canceledItems, totalTransactions,
                            printerId, printerName
                        );
                        continue;
                    }

                    if (ShouldPrint(printerId, "Checker"))
                    {
                        await PrintDataBillReceipt(datas, cartDetails, canceledItems, totalTransactions, printerName);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async Task PrintDataBillReceipt(
    DataRestruk datas,
    List<CartDetailRestruk> cartDetails,
    List<CanceledItemStrukCustomerRestruk> canceledItems,
    int totalTransactions,
    string printerName)
        {
            Stream stream = Stream.Null;

            try
            {
                stream = await EstablishPrinterConnection(printerName);

                int totalItems = (cartDetails?.Count ?? 0) + (canceledItems?.Count ?? 0);
                var capacity = GetOptimalCapacity(totalItems);
                var strukBuilder = new StringBuilder(capacity);

                string kodeHeksadesimalBold = "\x1B\x45\x01";
                string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}{1}",
                    kodeHeksadesimalBold,
                    CenterText($"SaveBill No. {totalTransactions}"));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}{1}",
                    kodeHeksadesimalBold,
                    CenterText("CHECKER"));

                if (!string.IsNullOrEmpty(datas?.receipt_number))
                {
                    if (datas.receipt_number.Length > 32)
                    {
                        strukBuilder.AppendFormat("Receipt No.{0}\n", datas.receipt_number);
                    }
                    else
                    {
                        strukBuilder.Append(CenterText($"Receipt No.{datas.receipt_number}"));
                    }
                }
                else
                {
                    strukBuilder.Append(CenterText("Receipt No. -"));
                }

                strukBuilder.Append(kodeHeksadesimalNormal);

                if (datas?.invoice_due_date != null)
                {
                    strukBuilder.Append(CenterText(datas.invoice_due_date));
                }

                strukBuilder.AppendFormat("Name: {0}\n", datas?.customer_name ?? "");

                void AppendItemDetails<T>(
                    List<T> items,
                    string sectionTitle,
                    Func<T, string> menuNameSelector,
                    Func<T, decimal> priceSelector,
                    Func<T, int> qtySelector,
                    Func<T, decimal> totalPriceSelector,
                    Func<T, string> varianSelector = null,
                    Func<T, string> noteSelector = null,
                    Func<T, string> discountCodeSelector = null,
                    Func<T, string> discountValueSelector = null,
                    Func<T, string> additionalInfoSelector = null)
                {
                    if (items?.Any() != true) return;

                    var servingTypes = items
                        .Select(i => GetPropertyValue(i, "serving_type_name") as string)
                        .Distinct();

                    strukBuilder.Append(SEPARATOR);
                    strukBuilder.AppendFormat("{0}", CenterText(sectionTitle));

                    foreach (string servingType in servingTypes)
                    {
                        var itemsForServingType = items
                            .Where(i => GetPropertyValue(i, "serving_type_name") as string == servingType)
                            .ToList();

                        if (!itemsForServingType.Any()) continue;

                        strukBuilder.Append(CenterText(servingType));

                        foreach (var item in itemsForServingType)
                        {
                            strukBuilder.Append(kodeHeksadesimalBold);
                            strukBuilder.AppendFormat("{0}\n", menuNameSelector(item));
                            strukBuilder.Append(kodeHeksadesimalNormal);

                            strukBuilder.AppendFormat("{0}\n",
                                FormatSimpleLine(printerName,
                                    $"@{priceSelector(item):n0} x{qtySelector(item)}",
                                    $"{totalPriceSelector(item):n0}"));

                            if (varianSelector != null && !string.IsNullOrEmpty(varianSelector(item)))
                            {
                                strukBuilder.AppendFormat("   Varian: {0}\n", varianSelector(item));
                            }

                            if (noteSelector != null && !string.IsNullOrEmpty(noteSelector(item)))
                            {
                                strukBuilder.AppendFormat("   Note: {0}\n", noteSelector(item));
                            }

                            if (discountCodeSelector != null && !string.IsNullOrEmpty(discountCodeSelector(item)))
                            {
                                strukBuilder.AppendFormat("   Discount Code: {0}\n", discountCodeSelector(item));
                            }

                            if (discountValueSelector != null)
                            {
                                var discountValue = discountValueSelector(item);
                                if (!string.IsNullOrEmpty(discountValue) && Convert.ToDecimal(discountValue) != 0)
                                {
                                    strukBuilder.AppendFormat("   Discount Value: {0}\n",
                                        GetPropertyValue(item, "discounts_is_percent")?.ToString() != "1"
                                            ? $"{Convert.ToDecimal(discountValue):n0}"
                                            : $"{discountValue} %");
                                }
                            }

                            if (additionalInfoSelector != null && !string.IsNullOrEmpty(additionalInfoSelector(item)))
                            {
                                strukBuilder.AppendFormat("   Reason: {0}\n", additionalInfoSelector(item));
                            }
                        }
                    }
                }

                AppendItemDetails(
                    cartDetails,
                    "ORDERED",
                    x => x.menu_name,
                    x => x.price,
                    x => x.qty,
                    x => x.total_price,
                    x => x.varian,
                    x => x.note_item,
                    x => x.discount_code,
                    x => x.discounts_value?.ToString()
                );

                AppendItemDetails(
                    canceledItems,
                    "CANCELED",
                    x => x.menu_name,
                    x => x.price,
                    x => x.qty,
                    x => x.total_price,
                    x => x.varian,
                    x => x.note_item?.ToString(),
                    null,
                    null,
                    x => x.cancel_reason
                );

                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Subtotal", $"{datas.subtotal:n0}"));

                if (!string.IsNullOrEmpty(datas.discount_code))
                {
                    strukBuilder.AppendFormat("Discount Code: {0}\n", datas.discount_code);
                }

                if (datas?.discounts_value != null && datas.discounts_value != "")
                {
                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Discount Value",
                            datas.discounts_is_percent != "1"
                                ? $"{datas.discounts_value:n0}"
                                : $"{datas.discounts_value} %"));
                }

                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Total", $"{datas.total:n0}"));

                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}", CenterText($"Meja No. {datas?.customer_seat}"));
                strukBuilder.Append(SEPARATOR);

                byte[] buffer = Encoding.UTF8.GetBytes(strukBuilder.ToString());
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error in PrintDataBillReceipt: {ex.Message}");
                throw;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }
        }


        public async Task Ex_PrintModelDataBill
        (DataRestruk datas,
            List<CartDetailRestruk> cartDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int totalTransactions,
            string printerId,
            string printerName)
        {
            try
            {
                LoggerUtil.LogWarning("Ex_PrintModelDataBill");

                Stream stream = null;

                // Koneksi printer langsung di sini
                if (IPAddress.TryParse(printerName, out _))
                {
                    TcpClient client = new(printerName, 9100);
                    stream = client.GetStream();
                }
                else
                {
                    await RetryPolicyAsync(async () =>
                    {
                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            return false;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        if (!printerDevice.Authenticated) // cek sudah paired?
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                return false;
                            }
                        }

                        client.Connect(endpoint);
                        stream = client.GetStream();
                        return true;
                    }, 3);
                }

                // Struck Checker
                if (ShouldPrint(printerId, "Checker"))
                {
                    PrintDocument printDocument = new();
                    printDocument.PrintPage += (sender, e) =>
                    {
                        Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                        // Mengatur font normal dan tebal
                        Font normalFont =
                            new("Arial", 8,
                                FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                        Font boldFont = new("Arial", 8, FontStyle.Bold);
                        Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                        Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                        float leftMargin = 5; // Margin kiri (dalam pixel)
                        float rightMargin = 5; // Margin kanan (dalam pixel)
                        float topMargin = 5; // Margin atas (dalam pixel)
                        float yPos = topMargin;

                        // Lebar kertas thermal 58mm, sesuaikan dengan margin
                        float paperWidth =
                            58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                        float printableWidth = paperWidth - leftMargin - rightMargin;


                        // Fungsi untuk format teks kiri dan kanan
                        void DrawSimpleLine(string textLeft, string textRight)
                        {
                            SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                            SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                            e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                            e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                leftMargin + printableWidth - sizeRight.Width, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                        void DrawCenterText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                    leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }

                        // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                        void DrawLeftText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }


                        // Fungsi untuk menggambar garis pemisah
                        void DrawSeparator()
                        {
                            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        void DrawSpace()
                        {
                            yPos += normalFont
                                .GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                        }


                        DrawCenterText("SaveBill No. " + totalTransactions, NomorAntrian);
                        string nomorMeja = "Meja No." + datas?.customer_seat;
                        //DrawCenterText(datas?.outlet_name, BigboldFont);
                        //DrawCenterText(datas?.outlet_address, normalFont);
                        //DrawCenterText(datas?.outlet_phone_number, normalFont);

                        DrawSeparator();

                        //Struct Checker
                        DrawCenterText("CHECKER", BigboldFont);
                        DrawCenterText("Receipt No. " + datas?.receipt_number, normalFont);
                        DrawSeparator();
                        if (datas?.invoice_due_date != null)
                        {
                            DrawCenterText(datas?.invoice_due_date, normalFont);
                        }

                        DrawSpace();
                        DrawLeftText(datas?.customer_name, normalFont);

                        if (cartDetails.Count != 0)
                        {
                            // Iterate through cart details and group by serving_type_name
                            IEnumerable<string> servingTypes =
                                cartDetails.Select(cd => cd.serving_type_name).Distinct();

                            foreach (string servingType in servingTypes)
                            {
                                // Filter cart details by serving_type_name
                                List<CartDetailRestruk> itemsForServingType =
                                    cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                                DrawSeparator();
                                DrawCenterText("ORDERED", boldFont);

                                // Skip if there are no items for this serving type
                                if (itemsForServingType.Count == 0)
                                {
                                    continue;
                                }

                                // Add a section for the serving type
                                DrawSeparator();
                                DrawCenterText(servingType, normalFont);
                                DrawSpace();

                                // Iterate through items for this serving type
                                foreach (CartDetailRestruk cartDetail in itemsForServingType)
                                {
                                    DrawSimpleLine(cartDetail.qty + " " + cartDetail.menu_name,
                                        string.Format("{0:n0}", cartDetail.price));
                                    // Add detail items
                                    if (!string.IsNullOrEmpty(cartDetail.varian))
                                    {
                                        DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cartDetail.note_item))
                                    {
                                        DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                    {
                                        DrawLeftText("   Discount Code: " + cartDetail.discount_code, normalFont);
                                    }

                                    if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                                    {
                                        DrawSimpleLine("   Total Discount: ",
                                            string.Format("{0:n0}", cartDetail.discounted_price));
                                    }

                                    DrawSimpleLine("   Total Price:", string.Format("{0:n0}", cartDetail.total_price));
                                    DrawSpace();
                                }
                            }
                        }

                        if (canceledItems.Count != 0)
                        {
                            IEnumerable<string> servingTypesCancel =
                                canceledItems.Select(cd => cd.serving_type_name).Distinct();
                            DrawSeparator();
                            DrawCenterText("CANCELED", boldFont);

                            foreach (string servingType in servingTypesCancel)
                            {
                                List<CanceledItemStrukCustomerRestruk> itemsForServingType =
                                    canceledItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                if (itemsForServingType.Count == 0)
                                {
                                    continue;
                                }

                                DrawSeparator();
                                DrawCenterText(servingType, normalFont);
                                DrawSpace();

                                foreach (CanceledItemStrukCustomerRestruk cancelItem in itemsForServingType)
                                {
                                    DrawSimpleLine(cancelItem.qty + " " + cancelItem.menu_name,
                                        string.Format("{0:n0}", cancelItem.price));
                                    // Add detail items
                                    if (!string.IsNullOrEmpty(cancelItem.varian))
                                    {
                                        DrawLeftText("   Varian: " + cancelItem.varian, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                    {
                                        DrawLeftText("   Note: " + cancelItem.note_item, normalFont);
                                    }

                                    if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                    {
                                        DrawLeftText("   Reason: " + cancelItem.cancel_reason, normalFont);
                                    }

                                    // Add an empty line between items
                                    DrawSpace();
                                }
                            }
                        }

                        DrawSeparator();
                        DrawSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal));
                        if (!string.IsNullOrEmpty(datas.discount_code))
                        {
                            DrawLeftText("Discount Code: " + datas.discount_code, normalFont);
                        }

                        if (datas?.discounts_value != null && datas.discounts_value != "")
                        {
                            DrawSimpleLine("Discount Value: ",
                                datas.discounts_is_percent != "1"
                                    ? string.Format("{0:n0}", datas.discounts_value)
                                    : datas.discounts_value + " %");
                        }

                        DrawSimpleLine("Total", string.Format("{0:n0}", datas.total));
                        DrawSeparator();
                        DrawSpace();
                        DrawCenterText(nomorMeja, NomorAntrian);
                    };
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        // Struct Simpan bill / save bill / savebill
        public async Task PrinterModelSimpan(
            GetStrukCustomerTransaction datas,
            List<CartDetailStrukCustomerTransaction> KitchenCartDetails,
            List<CanceledItemStrukCustomerTransaction> KitchenCancelItems,
            List<CartDetailStrukCustomerTransaction> BarCartDetails,
            List<CanceledItemStrukCustomerTransaction> BarCancelItems,
            int totalTransactions)
        {
            try
            {
                if(totalTransactions == null) { totalTransactions = 0; }
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync(); // Load additional settings
                var printTasks = new ConcurrentBag<Task>();

                foreach (KeyValuePair<string, string> printer in printerSettings)
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }

                    // Tangkap variabel untuk closure
                    var currentPrinterName = printerName;
                    var currentPrinterId = printerId;

                    // Tambahkan task paralel
                    printTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            bool shouldSkip = false;
                            if (IsNotMacAddressOrIpAddress(printerName))
                            {
                                Ex_PrinterModelSimpan(datas, KitchenCartDetails, KitchenCancelItems, BarCartDetails,
                                    BarCancelItems, totalTransactions,
                                    printerId, printerName
                                );
                                shouldSkip = true;
                            }
                            if (!shouldSkip)
                            {

                                if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                                {
                                    if (ShouldPrint(printerId, "Makanan"))
                                    {
                                        await PrintSimpanReceipt(datas, KitchenCartDetails, KitchenCancelItems,
                                              BarCartDetails, BarCancelItems, totalTransactions, "Makanan", printerName);
                                    }
                                }

                                if (BarCartDetails.Any() || BarCancelItems.Any())
                                {
                                    if (ShouldPrint(printerId, "Minuman"))
                                    {
                                        await PrintSimpanReceipt(datas, KitchenCartDetails, KitchenCancelItems,
                                              BarCartDetails, BarCancelItems, totalTransactions, "Minuman", printerName);
                                    }
                                }

                                if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                                {
                                    if (ShouldPrint(printerId, "Checker"))
                                    {
                                        await PrintSimpanReceipt(datas, KitchenCartDetails, KitchenCancelItems,
                                            BarCartDetails, BarCancelItems, totalTransactions, "Checker", printerName);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerUtil.LogError(ex, $"Error printing for printer {currentPrinterName}: {ex.Message}");
                        }
                    }));
                }

                // Tunggu semua task selesai
                await Task.WhenAll(printTasks);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async Task PrintSimpanReceipt(
    GetStrukCustomerTransaction datas,
    List<CartDetailStrukCustomerTransaction> KitchenCartDetails,
    List<CanceledItemStrukCustomerTransaction> KitchenCancelItems,
    List<CartDetailStrukCustomerTransaction> BarCartDetails,
    List<CanceledItemStrukCustomerTransaction> BarCancelItems,
    int totalTransactions,
    string type,
    string printerName)
        {
            Stream stream = Stream.Null;

            try
            {
                stream = await EstablishPrinterConnection(printerName);

                int totalItems = (KitchenCartDetails?.Count ?? 0) +
                                 (KitchenCancelItems?.Count ?? 0) +
                                 (BarCartDetails?.Count ?? 0) +
                                 (BarCancelItems?.Count ?? 0);
                var capacity = GetOptimalCapacity(totalItems);
                var strukBuilder = new StringBuilder(capacity);

                string kodeHeksadesimalBold = "\x1B\x45\x01";
                string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

                strukBuilder.AppendFormat("\n{0}{1}",
                    kodeHeksadesimalBold,
                    CenterText($"SaveBill No. {totalTransactions}"));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}{1}",
                    kodeHeksadesimalBold,
                    CenterText(type.ToUpper()));
                strukBuilder.AppendFormat("{0}",
                    CenterText($"Receipt No.: {datas.data?.receipt_number}"));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}",
                    CenterText(datas.data?.invoice_due_date ?? ""));
                strukBuilder.AppendFormat("Name: {0}\n",
                    datas.data?.customer_name ?? "");

                void AppendItemDetails<T>(
                    List<T> items,
                    string sectionTitle,
                    Func<T, string> menuNameSelector,
                    Func<T, int> qtySelector,
                    Func<T, string> varianSelector = null,
                    Func<T, string> noteSelector = null,
                    Func<T, string> additionalInfoSelector = null)
                {
                    if (items?.Any() != true) return;

                    var servingTypes = items
                        .Select(i => GetPropertyValue(i, "serving_type_name") as string)
                        .Distinct();

                    strukBuilder.Append(SEPARATOR);
                    strukBuilder.AppendFormat("{0}", CenterText(sectionTitle));

                    foreach (string servingType in servingTypes)
                    {
                        var itemsForServingType = items
                            .Where(i => GetPropertyValue(i, "serving_type_name") as string == servingType)
                            .ToList();

                        if (!itemsForServingType.Any()) continue;

                        strukBuilder.Append(SEPARATOR);
                        strukBuilder.AppendFormat("{0}", CenterText(servingType));
                        strukBuilder.Append("\n");

                        foreach (var item in itemsForServingType)
                        {
                            strukBuilder.Append(kodeHeksadesimalSizeBesar + kodeHeksadesimalBold);
                            strukBuilder.AppendFormat("{0}\n",
                                FormatSimpleLine(printerName, menuNameSelector(item), qtySelector(item)));

                            if (varianSelector != null && !string.IsNullOrEmpty(varianSelector(item)))
                            {
                                strukBuilder.AppendFormat("Varian: {0}\n", varianSelector(item));
                            }

                            if (noteSelector != null && !string.IsNullOrEmpty(noteSelector(item)))
                            {
                                strukBuilder.AppendFormat("Note: {0}\n", noteSelector(item));
                            }

                            if (additionalInfoSelector != null && !string.IsNullOrEmpty(additionalInfoSelector(item)))
                            {
                                strukBuilder.AppendFormat("Additional Info: {0}\n", additionalInfoSelector(item));
                            }

                            strukBuilder.Append(kodeHeksadesimalNormal);
                            strukBuilder.Append("\n");
                        }
                    }
                }

                switch (type)
                {
                    case "Makanan":
                        AppendItemDetails(
                            KitchenCartDetails,
                            "ORDER",
                            x => x.menu_name,
                            x => x.qty,
                            x => x.varian,
                            x => x.note_item
                        );
                        AppendItemDetails(
                            KitchenCancelItems,
                            "CANCELED",
                            x => x.menu_name,
                            x => x.qty,
                            x => x.varian,
                            x => x.note_item?.ToString(),
                            x => x.cancel_reason
                        );
                        break;

                    case "Minuman":
                        AppendItemDetails(
                            BarCartDetails,
                            "ORDER",
                            x => x.menu_name,
                            x => x.qty,
                            x => x.varian,
                            x => x.note_item
                        );
                        AppendItemDetails(
                            BarCancelItems,
                            "CANCELED",
                            x => x.menu_name,
                            x => x.qty,
                            x => x.varian,
                            x => x.note_item?.ToString(),
                            x => x.cancel_reason
                        );
                        break;

                    case "Checker":
                        var allCartDetails = new List<CartDetailStrukCustomerTransaction>();
                        allCartDetails.AddRange(KitchenCartDetails);
                        allCartDetails.AddRange(BarCartDetails);

                        var allCancelItems = new List<CanceledItemStrukCustomerTransaction>();
                        allCancelItems.AddRange(KitchenCancelItems);
                        allCancelItems.AddRange(BarCancelItems);

                        var servingTypes = allCartDetails
                            .Select(x => x.serving_type_name)
                            .Distinct();

                        strukBuilder.Append(SEPARATOR);
                        strukBuilder.AppendFormat("{0}", CenterText("ORDER"));

                        foreach (string servingType in servingTypes)
                        {
                            var orderedItems = allCartDetails
                                .Where(x => x.serving_type_name == servingType)
                                .ToList();

                            strukBuilder.Append(SEPARATOR);
                            strukBuilder.AppendFormat("{0}", CenterText(servingType));
                            strukBuilder.Append("\n");

                            foreach (var item in orderedItems)
                            {
                                strukBuilder.Append(kodeHeksadesimalSizeBesar + kodeHeksadesimalBold);
                                strukBuilder.AppendFormat("{0}\n",
                                    FormatSimpleLine(printerName, item.menu_name, item.qty));

                                if (!string.IsNullOrEmpty(item.varian))
                                {
                                    strukBuilder.AppendFormat("Varian: {0}\n", item.varian);
                                }

                                if (!string.IsNullOrEmpty(item.note_item))
                                {
                                    strukBuilder.AppendFormat("Note: {0}\n", item.note_item);
                                }

                                strukBuilder.Append(kodeHeksadesimalNormal);
                                strukBuilder.Append("\n");
                            }
                        }

                        var canceledServingTypes = allCancelItems
                            .Select(x => x.serving_type_name)
                            .Distinct();

                        strukBuilder.Append(SEPARATOR);
                        strukBuilder.AppendFormat("{0}", CenterText("CANCELED"));

                        foreach (string servingType in canceledServingTypes)
                        {
                            var canceledItems = allCancelItems
                                .Where(x => x.serving_type_name == servingType)
                                .ToList();

                            strukBuilder.Append(SEPARATOR);
                            strukBuilder.AppendFormat("{0}", CenterText(servingType));
                            strukBuilder.Append("\n");

                            foreach (var item in canceledItems)
                            {
                                strukBuilder.Append(kodeHeksadesimalSizeBesar + kodeHeksadesimalBold);
                                strukBuilder.AppendFormat("{0}\n",
                                    FormatSimpleLine(printerName, item.menu_name, item.qty));

                                if (!string.IsNullOrEmpty(item.varian))
                                {
                                    strukBuilder.AppendFormat("Varian: {0}\n", item.varian);
                                }

                                if (!string.IsNullOrEmpty(item.note_item?.ToString()))
                                {
                                    strukBuilder.AppendFormat("Note: {0}\n", item.note_item);
                                }

                                if (!string.IsNullOrEmpty(item.cancel_reason))
                                {
                                    strukBuilder.AppendFormat("Canceled Reason: {0}\n", item.cancel_reason);
                                }

                                strukBuilder.Append(kodeHeksadesimalNormal);
                                strukBuilder.Append("\n");
                            }
                        }
                        break;
                }

                strukBuilder.Append("--------------------------------\n\n\n\n\n");

                byte[] buffer = Encoding.UTF8.GetBytes(strukBuilder.ToString());
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error in PrintSimpanReceipt: {ex.Message}");
                throw;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }
        }
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj?.GetType().GetProperty(propertyName)?.GetValue(obj);
        }
        public async Task Ex_PrinterModelSimpan(
            GetStrukCustomerTransaction datas,
            List<CartDetailStrukCustomerTransaction> KitchenCartDetails,
            List<CanceledItemStrukCustomerTransaction> KitchenCancelItems,
            List<CartDetailStrukCustomerTransaction> BarCartDetails,
            List<CanceledItemStrukCustomerTransaction> BarCancelItems,
            int totalTransactions,
            string printerId,
            string printerName)
        {
            try
            {
                Stream stream = null;

                // Koneksi printer langsung di sini
                if (IPAddress.TryParse(printerName, out _))
                {
                    TcpClient client = new(printerName, 9100);
                    stream = client.GetStream();
                }
                else
                {
                    await RetryPolicyAsync(async () =>
                    {
                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            return false;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        if (!printerDevice.Authenticated) // cek sudah paired?
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                return false;
                            }
                        }

                        client.Connect(endpoint);
                        stream = client.GetStream();
                        return true;
                    }, 3);
                }

                // Struct Kitchen&Bar

                if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                {
                    if (ShouldPrint(printerId, "Makanan"))
                    {
                        PrintDocument printDocument = new();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                            // Mengatur font normal dan tebal
                            Font normalFont =
                                new("Arial", 8,
                                    FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                            float leftMargin = 5; // Margin kiri (dalam pixel)
                            float rightMargin = 5; // Margin kanan (dalam pixel)
                            float topMargin = 5; // Margin atas (dalam pixel)
                            float yPos = topMargin;

                            // Lebar kertas thermal 58mm, sesuaikan dengan margin
                            float paperWidth =
                                58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                            float printableWidth = paperWidth - leftMargin - rightMargin;


                            // Fungsi untuk format teks kiri dan kanan
                            void DrawSimpleLine(string textLeft, string textRight)
                            {
                                if (textLeft == null)
                                {
                                    textLeft = string.Empty;
                                }

                                if (textRight == null)
                                {
                                    textRight = string.Empty;
                                }

                                SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                                SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                                e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                    leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null)
                                {
                                    text = string.Empty;
                                }

                                if (font == null)
                                {
                                    throw new ArgumentNullException(nameof(font),
                                        "Font is null in DrawCenterText method.");
                                }

                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                            leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                        yPos += font.GetHeight(e.Graphics);
                                        currentLine.Clear();
                                    }

                                    // Tambahkan kata ke baris saat ini
                                    currentLine.Append(word + " ");
                                }

                                // Gambar baris terakhir
                                if (currentLine.Length > 0)
                                {
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                }
                            }

                            // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                            void DrawLeftText(string text, Font font)
                            {
                                if (text == null)
                                {
                                    //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                    return;
                                }

                                if (text == null)
                                {
                                    text = string.Empty;
                                }

                                if (font == null)
                                {
                                    throw new ArgumentNullException(nameof(font),
                                        "Font is null in DrawLeftText method.");
                                }

                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                            yPos);
                                        yPos += font.GetHeight(e.Graphics);
                                        currentLine.Clear();
                                    }

                                    // Tambahkan kata ke baris saat ini
                                    currentLine.Append(word + " ");
                                }

                                // Gambar baris terakhir
                                if (currentLine.Length > 0)
                                {
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                }
                            }

                            // Fungsi untuk menggambar garis pemisah
                            void DrawSeparator()
                            {
                                e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            void DrawSpace()
                            {
                                yPos += normalFont
                                    .GetHeight(e
                                        .Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                            }


                            string nomorMeja = "Meja No." + datas.data?.customer_seat;
                            DrawCenterText("SaveBill No. " + totalTransactions, NomorAntrian);

                            // Generate struk text
                            DrawSeparator();
                            DrawCenterText("MAKANAN", BigboldFont);
                            DrawCenterText(datas.data?.receipt_number, normalFont);
                            DrawSeparator();
                            DrawCenterText(datas.data?.invoice_due_date, normalFont);
                            DrawSpace();
                            DrawSimpleLine(datas.data?.customer_name, "");

                            if (KitchenCartDetails.Count != 0)
                            {
                                IEnumerable<string> servingTypes =
                                    KitchenCartDetails.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("ORDER", boldFont);

                                foreach (string servingType in servingTypes)
                                {
                                    List<CartDetailStrukCustomerTransaction> itemsForServingType =
                                        KitchenCartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                    {
                                        continue;
                                    }

                                    DrawSeparator();
                                    DrawCenterText(servingType, normalFont);

                                    foreach (CartDetailStrukCustomerTransaction cartDetail in itemsForServingType)
                                    {
                                        string qtyMenu = "x" + cartDetail.qty;
                                        DrawCenterText(qtyMenu + " " + cartDetail.menu_name, BigboldFont);

                                        if (!string.IsNullOrEmpty(cartDetail.varian))
                                        {
                                            DrawCenterText("Varian: " + cartDetail.varian, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cartDetail.note_item))
                                        {
                                            DrawCenterText("Note: " + cartDetail.note_item, BigboldFont);
                                        }

                                        DrawSpace();
                                    }
                                }
                            }

                            if (KitchenCancelItems.Count != 0)
                            {
                                IEnumerable<string> servingTypes =
                                    KitchenCancelItems.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("CANCELED", boldFont);

                                foreach (string servingType in servingTypes)
                                {
                                    List<CanceledItemStrukCustomerTransaction> itemsForServingType =
                                        KitchenCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                    {
                                        continue;
                                    }

                                    DrawSeparator();
                                    DrawCenterText(servingType, boldFont);

                                    foreach (CanceledItemStrukCustomerTransaction cancelItem in itemsForServingType)
                                    {
                                        string qtyMenu = "x" + cancelItem.qty;
                                        DrawCenterText(qtyMenu + " " + cancelItem.menu_name, BigboldFont);

                                        if (!string.IsNullOrEmpty(cancelItem.varian))
                                        {
                                            DrawCenterText("Varian: " + cancelItem.varian, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                        {
                                            DrawCenterText("Note: " + cancelItem.note_item, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                        {
                                            DrawCenterText("Canceled Reason: " + cancelItem.cancel_reason, BigboldFont);
                                        }

                                        DrawSpace();
                                    }
                                }
                            }

                            DrawSeparator();
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);
                        };

                        if (IsBluetoothPrinter(printerName))
                        {
                            printerName = ConvertMacAddressFormat(printerName);
                            PrintViaBluetooth(printerName, printDocument);
                        }
                        else
                        {
                            printDocument.PrinterSettings.PrinterName = printerName;
                            printDocument.Print();
                        }
                    }
                }

                // Struct Kitchen&Bar
                if (BarCartDetails.Any() || BarCancelItems.Any())
                {
                    if (ShouldPrint(printerId, "Minuman"))
                    {
                        PrintDocument printDocument = new();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                            // Mengatur font normal dan tebal
                            Font normalFont =
                                new("Arial", 8,
                                    FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                            float leftMargin = 5; // Margin kiri (dalam pixel)
                            float rightMargin = 5; // Margin kanan (dalam pixel)
                            float topMargin = 5; // Margin atas (dalam pixel)
                            float yPos = topMargin;

                            // Lebar kertas thermal 58mm, sesuaikan dengan margin
                            float paperWidth =
                                58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                            float printableWidth = paperWidth - leftMargin - rightMargin;


                            // Fungsi untuk format teks kiri dan kanan
                            void DrawSimpleLine(string textLeft, string textRight)
                            {
                                if (textLeft == null)
                                {
                                    textLeft = string.Empty;
                                }

                                if (textRight == null)
                                {
                                    textRight = string.Empty;
                                }

                                SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                                SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                                e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                    leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null)
                                {
                                    text = string.Empty;
                                }

                                if (font == null)
                                {
                                    throw new ArgumentNullException(nameof(font),
                                        "Font is null in DrawCenterText method.");
                                }

                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                            leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                        yPos += font.GetHeight(e.Graphics);
                                        currentLine.Clear();
                                    }

                                    // Tambahkan kata ke baris saat ini
                                    currentLine.Append(word + " ");
                                }

                                // Gambar baris terakhir
                                if (currentLine.Length > 0)
                                {
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                }
                            }

                            // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                            void DrawLeftText(string text, Font font)
                            {
                                if (text == null)
                                {
                                    //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                    return;
                                }

                                if (text == null)
                                {
                                    text = string.Empty;
                                }

                                if (font == null)
                                {
                                    throw new ArgumentNullException(nameof(font),
                                        "Font is null in DrawLeftText method.");
                                }

                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                            yPos);
                                        yPos += font.GetHeight(e.Graphics);
                                        currentLine.Clear();
                                    }

                                    // Tambahkan kata ke baris saat ini
                                    currentLine.Append(word + " ");
                                }

                                // Gambar baris terakhir
                                if (currentLine.Length > 0)
                                {
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                }
                            }

                            // Fungsi untuk menggambar garis pemisah
                            void DrawSeparator()
                            {
                                e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            void DrawSpace()
                            {
                                yPos += normalFont
                                    .GetHeight(e
                                        .Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                            }


                            string nomorMeja = "Meja No." + datas.data?.customer_seat;
                            DrawCenterText("SaveBill No. " + totalTransactions, NomorAntrian);

                            // Generate struk text
                            DrawSeparator();
                            DrawCenterText("MINUMAN", BigboldFont);
                            DrawCenterText(datas.data?.receipt_number, normalFont);
                            DrawSeparator();
                            DrawCenterText(datas.data?.invoice_due_date, normalFont);
                            DrawSpace();
                            DrawSimpleLine(datas.data?.customer_name, "");

                            if (BarCartDetails.Count != 0)
                            {
                                IEnumerable<string> servingTypes =
                                    BarCartDetails.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("ORDER", boldFont);

                                foreach (string servingType in servingTypes)
                                {
                                    List<CartDetailStrukCustomerTransaction> itemsForServingType =
                                        BarCartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                    {
                                        continue;
                                    }

                                    DrawSeparator();
                                    DrawCenterText(servingType, normalFont);

                                    foreach (CartDetailStrukCustomerTransaction cartDetail in itemsForServingType)
                                    {
                                        string qtyMenu = "x" + cartDetail.qty;
                                        DrawCenterText(qtyMenu + " " + cartDetail.menu_name, BigboldFont);

                                        if (!string.IsNullOrEmpty(cartDetail.varian))
                                        {
                                            DrawCenterText("Varian: " + cartDetail.varian, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cartDetail.note_item))
                                        {
                                            DrawCenterText("Note: " + cartDetail.note_item, BigboldFont);
                                        }

                                        DrawSpace();
                                    }
                                }
                            }

                            if (BarCancelItems.Count != 0)
                            {
                                IEnumerable<string> servingTypes =
                                    BarCancelItems.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("CANCELED", boldFont);

                                foreach (string servingType in servingTypes)
                                {
                                    List<CanceledItemStrukCustomerTransaction> itemsForServingType =
                                        BarCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                    {
                                        continue;
                                    }

                                    DrawSeparator();
                                    DrawCenterText(servingType, boldFont);

                                    foreach (CanceledItemStrukCustomerTransaction cancelItem in itemsForServingType)
                                    {
                                        string qtyMenu = "x" + cancelItem.qty;
                                        DrawCenterText(qtyMenu + " " + cancelItem.menu_name, BigboldFont);

                                        if (!string.IsNullOrEmpty(cancelItem.varian))
                                        {
                                            DrawCenterText("Varian: " + cancelItem.varian, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                        {
                                            DrawCenterText("Note: " + cancelItem.note_item, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                        {
                                            DrawCenterText("Canceled Reason: " + cancelItem.cancel_reason, BigboldFont);
                                        }

                                        DrawSpace();
                                    }
                                }
                            }

                            DrawSeparator();
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        // Struct Payform
        public async Task PrintModelPayform(
            GetStrukCustomerTransaction datas,
            List<CartDetailStrukCustomerTransaction> cartDetails,
            List<KitchenAndBarCartDetails> KitchenCartDetails,
            List<KitchenAndBarCartDetails> BarCartDetails,
            List<KitchenAndBarCanceledItems> KitchenCancelItems,
            List<KitchenAndBarCanceledItems> BarCancelItems,
            int totalTransactions,
            string Kakimu)
        {
            try
            {
                if(totalTransactions == null) { totalTransactions = 0; }
                if (string.IsNullOrEmpty(Kakimu)) { Kakimu = ""; }
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync(); // Load additional settings
                var printTasks = new ConcurrentBag<Task>();

                foreach (KeyValuePair<string, string> printer in printerSettings)
                {
                    string printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }

                    string printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }

                    // Tangkap variabel untuk closure
                    var currentPrinterName = printerName;
                    var currentPrinterId = printerId;

                    // Tambahkan task paralel
                    printTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            bool shouldSkip = false;

                            if (IsNotMacAddressOrIpAddress(printerName))
                            {
                                await Ex_PrintModelPayform(
                                    datas, cartDetails, KitchenCartDetails, BarCartDetails,
                                    KitchenCancelItems, BarCancelItems, totalTransactions, Kakimu,
                                    printerId, printerName
                                );
                                shouldSkip = true;
                            }
                            if (!shouldSkip)
                            {
                                // Struct Customer ====
                                if (ShouldPrint(printerId, "Kasir"))
                                {
                                    // Setelah koneksi berhasil, cetak struk
                                    await PrintCustomerReceipt(datas, cartDetails, totalTransactions, Kakimu, printerName);
                                }

                                // Struct Checker
                                if (ShouldPrint(printerId, "Checker"))
                                {
                                    // Setelah koneksi berhasil, cetak struk
                                    await PrintCheckerReceipt(datas, cartDetails, totalTransactions, printerName);
                                }

                                // Struct Kitchen
                                if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                                {
                                    if (ShouldPrint(printerId, "Makanan"))
                                    {
                                        // Filter kitchen cart details yang is_ordered == 1
                                        List<KitchenAndBarCartDetails> orderedKitchenItems =
                                            KitchenCartDetails.Where(x => x.is_ordered != 1).ToList();

                                        if (orderedKitchenItems.Any())
                                        {
                                            // Setelah koneksi berhasil, cetak struk
                                            await PrintKitchenOrBarReceipt(datas, KitchenCartDetails, KitchenCancelItems,
                                                totalTransactions, "Makanan", printerName);

                                        }
                                    }
                                }
                                // Struct Bar
                                if (BarCartDetails.Any() || BarCancelItems.Any())
                                {
                                    if (ShouldPrint(printerId, "Minuman"))
                                    {
                                        // Setelah koneksi berhasil, cetak struk
                                        await PrintKitchenOrBarReceipt(datas, BarCartDetails, BarCancelItems,
                                             totalTransactions, "Minuman", printerName);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerUtil.LogError(ex, $"Error printing for printer {currentPrinterName}: {ex.Message}");
                        }
                    }));
                }

                // Tunggu semua task selesai
                await Task.WhenAll(printTasks);
            }
            catch (Exception ex)
            {
                LogContext.LogPrintOperation(
            LogContext.PrintType.Payform,
            ex,
            $"Failed for transactions: {totalTransactions}"
        );
                throw;
            }
        }
        private async Task<bool> IsQRCodeEnabledAsync()
        {
            string PathFile = "setting//QRcodeSetting.data";
            return File.Exists(PathFile) &&
                   (await File.ReadAllTextAsync(PathFile)).Trim() == "ON";
        }
        // Helper method untuk koneksi printer
        private async Task<Stream> EstablishPrinterConnection(string printerName)
        {
            Stream stream = Stream.Null;

            if (IPAddress.TryParse(printerName, out _))
            {
                // Connect via LAN
                var client = new TcpClient(printerName, 9100);
                stream = client.GetStream();
            }
            else
            {
                // Connect via Bluetooth dengan retry policy
                await RetryPolicyAsync(async () =>
                {
                    var printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                    if (printerDevice == null)
                    {
                        throw new Exception("Invalid Bluetooth Device");
                    }

                    var client = new BluetoothClient();
                    var endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                    if (!printerDevice.Authenticated) // cek sudah paired?
                    {
                        if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                        {
                            NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                            throw new Exception("Bluetooth pairing failed");
                        }
                    }

                    client.Connect(endpoint);
                    stream = client.GetStream();

                    return true;
                }, 2);
            }

            return stream;
        }
        private int GetOptimalCapacity(int itemCount)
        {
            return itemCount switch
            {
                <= 2 => 2048,     // 2 KB
                <= 5 => 4096,     // 4 KB
                <= 10 => 6144,    // 6 KB
                <= 20 => 8192,    // 8 KB
                _ => 16384        // 16 KB
            };
        }

        private async Task PrintCustomerReceipt(GetStrukCustomerTransaction datas,
    List<CartDetailStrukCustomerTransaction> cartDetails,
    int totalTransactions, string Kakimu, string printerName)
        {
            Stream stream = Stream.Null;

            try
            {
                stream = await EstablishPrinterConnection(printerName);

                var capacity = GetOptimalCapacity(cartDetails.Count);
                var strukBuilder = new StringBuilder(capacity);

                // Kode Heksadesimal  
                string kodeHeksadesimalBold = "\x1B\x45\x01";
                string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

                // Header Struk  
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}{1}", kodeHeksadesimalBold, CenterText(datas.data?.outlet_name ?? ""));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}{1}", "", CenterText(datas.data?.outlet_address ?? ""));
                strukBuilder.AppendFormat("{0}{1}", "", CenterText(datas.data?.outlet_phone_number ?? ""));
                strukBuilder.AppendFormat("{0}{1}", kodeHeksadesimalBold,
                    CenterText($"Receipt No.: {datas.data?.receipt_number ?? ""}"));
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}{1}", "", CenterText(datas.data?.invoice_due_date ?? ""));

                // Informasi Member/Customer  
                if (datas?.data != null &&
                    (datas.data.member_id != 0 || !string.IsNullOrEmpty(datas.data.member_name)))
                {
                    strukBuilder.AppendFormat("Name Member: {0}\n", datas.data.member_name ?? "-");
                    strukBuilder.AppendFormat("Number Member: {0}\n", datas.data.member_phone_number ?? "-");
                    strukBuilder.AppendFormat("Point Member: {0}\n",
                        datas.data.member_point?.ToString("#,#") ?? "0");

                    if (datas.data.member_use_point.HasValue && datas.data.member_use_point > 0)
                    {
                        strukBuilder.AppendFormat("Point Member Terpakai: {0}\n",
                            datas.data.member_use_point);
                    }
                }
                else
                {
                    strukBuilder.AppendFormat("Name: {0}\n",
                        datas?.data?.customer_name ?? "Walk-in Customer");
                }

                // Daftar Pesanan  
                if (cartDetails != null && cartDetails.Any())
                {
                    var servingTypes = cartDetails
                        .Select(cd => cd.serving_type_name)
                        .Distinct();

                    strukBuilder.Append(SEPARATOR);
                    strukBuilder.AppendFormat("{0}", CenterText("ORDER"));

                    foreach (var servingType in servingTypes)
                    {
                        var itemsForServingType = cartDetails
                            .Where(cd => cd.serving_type_name == servingType)
                            .ToList();

                        if (!itemsForServingType.Any()) continue;

                        strukBuilder.AppendFormat("{0}", CenterText(servingType));

                        foreach (var cartDetail in itemsForServingType)
                        {
                            strukBuilder.AppendFormat("{0}{1}\n",
                                kodeHeksadesimalBold, cartDetail.menu_name);
                            strukBuilder.Append(kodeHeksadesimalNormal);

                            strukBuilder.AppendFormat("{0}\n",
                                FormatSimpleLine(printerName,
                                    $"@{cartDetail.price:n0} x{cartDetail.qty}",
                                    $"{cartDetail.total_price:n0}"));

                            // Varian  
                            if (!string.IsNullOrEmpty(cartDetail.varian))
                            {
                                strukBuilder.AppendFormat("  Varian: {0}\n", cartDetail.varian);
                            }

                            // Catatan Item  
                            if (!string.IsNullOrEmpty(cartDetail.note_item))
                            {
                                strukBuilder.AppendFormat("  Note: {0}\n", cartDetail.note_item);
                            }

                            // Diskon  
                            if (!string.IsNullOrEmpty(cartDetail.discount_code))
                            {
                                strukBuilder.AppendFormat("  Discount Code: {0}\n", cartDetail.discount_code);
                            }

                            // Nilai Diskon  
                            if (cartDetail.discounts_value != null &&
                                Convert.ToDecimal(cartDetail.discounts_value) != 0)
                            {
                                var discountDisplay = cartDetail.discounts_is_percent.ToString() != "1"
                                    ? $"{cartDetail.discounts_value:n0}"
                                    : $"{cartDetail.discounts_value}%";

                                strukBuilder.AppendFormat("  Discount Value: {0}\n", discountDisplay);
                            }
                        }
                    }
                }

                // Ringkasan Transaksi  
                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Subtotal", $"{datas.data.subtotal:n0}"));

                // Diskon Transaksi  
                if (!string.IsNullOrEmpty(datas.data.discount_code))
                {
                    strukBuilder.AppendFormat("Discount Code: {0}\n", datas.data.discount_code);
                }

                if (datas.data.discounts_value.HasValue && datas.data.discounts_value != 0)
                {
                    var discountDisplay = datas.data.discounts_is_percent != "1"
                        ? $"{datas.data.discounts_value:n0}"
                        : $"{datas.data.discounts_value}%";

                    strukBuilder.AppendFormat("{0}\n",
                        FormatSimpleLine(printerName, "Discount Value: ", discountDisplay));
                }

                // Total dan Pembayaran  
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Total", $"{datas.data.total:n0}"));
                strukBuilder.AppendFormat("Payment Type: {0}\n", datas.data.payment_type);
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Cash", $"{datas.data.customer_cash:n0}"));
                strukBuilder.AppendFormat("{0}\n",
                    FormatSimpleLine(printerName, "Change", $"{datas.data.customer_change:n0}"));

                // Footer  
                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}", CenterText(datas.data.outlet_footer));
                strukBuilder.AppendFormat("{0}", CenterText(Kakimu));
                strukBuilder.AppendFormat("{0}", CenterText($"Meja No. {datas.data.customer_seat}"));
                strukBuilder.Append(SEPARATOR);
                strukBuilder.AppendFormat("{0}", CenterText("Powered By Dastrevas"));

                // Nomor Urut dengan StringBuilder
                var nomorUrutBuilder = new StringBuilder();
                nomorUrutBuilder.AppendFormat("\n{0}{1}",
                    kodeHeksadesimalSizeBesar + kodeHeksadesimalBold,
                    CenterText($"No. {totalTransactions}"));
                nomorUrutBuilder.Append("\n\n\n    ");

                // Konversi dan Cetak
                byte[] buffer1 = Encoding.UTF8.GetBytes(nomorUrutBuilder.ToString());
                byte[] buffer = Encoding.UTF8.GetBytes(strukBuilder.ToString());

                stream.Write(buffer1, 0, buffer1.Length);
                PrintLogo(stream, "icon\\OutletLogo.bmp", logoSize);
                stream.Write(buffer, 0, buffer.Length);

                // QR Code
                if (await IsQRCodeEnabledAsync())
                {
                    PrintLogo(stream, "icon\\QRcode.bmp", logoSize);
                }

                // Tambahan newline
                strukBuilder.Clear(); // Reuse StringBuilder
                strukBuilder.Append("--------------------------------\n\n\n");
                buffer = Encoding.UTF8.GetBytes(strukBuilder.ToString());
                stream.Write(buffer, 0, buffer.Length);

                stream.Flush();
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        private async Task PrintCheckerReceipt(
    GetStrukCustomerTransaction datas,
    List<CartDetailStrukCustomerTransaction> cartDetails,
    int totalTransactions, string printerName)
        {
            Stream stream = null;
            try
            {
                // Filter checker cart details yang is_ordered == 1
                List<CartDetailStrukCustomerTransaction> orderedCheckerItems =
                    cartDetails.Where(x => x.is_ordered != 1).ToList();

                if (orderedCheckerItems.Any())
                {
                    // Koneksi printer (tetap sama)
                    stream = await EstablishPrinterConnection(printerName);

                    // Gunakan kapasitas optimal
                    var capacity = GetOptimalCapacity(cartDetails.Count);
                    var strukBuilder = new StringBuilder(capacity);

                    var printerSettings = PrinterConfig.GetPrinterSettings(printerName);

                    // Kode Heksadesimal
                    string kodeHeksadesimalBold = "\x1B\x45\x01";
                    string kodeHeksadesimalSizeBesar = printerSettings.BigSize;
                    string kodeHeksadesimalNormal = "\x1B\x45\x00" + printerSettings.NormalSize;

                    // Header Checker
                    strukBuilder.Append(kodeHeksadesimalNormal);
                    strukBuilder.AppendFormat("{0}{1}",
                        kodeHeksadesimalBold,
                        CenterText($"CHECKER No. {totalTransactions}"));
                    strukBuilder.Append(kodeHeksadesimalNormal);

                    strukBuilder.AppendFormat("{0}{1}",
                        kodeHeksadesimalBold,
                        CenterText($"Receipt No.: {datas.data?.receipt_number}"));

                    strukBuilder.Append(kodeHeksadesimalNormal);
                    strukBuilder.AppendFormat("{0}", CenterText(datas.data?.invoice_due_date));

                    // Informasi Member/Customer
                    if (datas.data?.member_id != null && !string.IsNullOrEmpty(datas.data?.member_name))
                    {
                        strukBuilder.AppendFormat("Name Member: {0}\n", datas.data?.member_name);
                        strukBuilder.AppendFormat("Number Member: {0}\n", datas.data?.member_phone_number);
                        strukBuilder.AppendFormat("Point Member: {0}\n",
                            datas.data?.member_point?.ToString("#,#") ?? "0");

                        if (datas.data?.member_use_point > 0)
                        {
                            strukBuilder.AppendFormat("Point Member Terpakai: {0}\n",
                                datas.data?.member_use_point);
                        }
                    }
                    else
                    {
                        strukBuilder.AppendFormat("Name: {0}\n", datas.data?.customer_name);
                    }

                    // Filter items untuk checker
                    var checkerItems = cartDetails
                        .Where(x => x.is_ordered == 0 && !string.IsNullOrEmpty(x.menu_name))
                        .ToList();

                    // Cetak jika ada item
                    if (checkerItems.Any())
                    {
                        var servingTypes = cartDetails
                            .Select(cd => cd.serving_type_name)
                            .Distinct();

                        strukBuilder.Append(SEPARATOR);
                        strukBuilder.AppendFormat("{0}", CenterText("ORDER"));

                        foreach (string servingType in servingTypes)
                        {
                            var itemsForServingType = cartDetails
                                .Where(cd => cd.serving_type_name == servingType)
                                .ToList();

                            if (itemsForServingType.Count == 0) continue;

                            strukBuilder.AppendFormat("{0}", CenterText(servingType));

                            foreach (var cartDetail in itemsForServingType)
                            {
                                strukBuilder.Append(kodeHeksadesimalSizeBesar + kodeHeksadesimalBold);
                                strukBuilder.AppendFormat("{0}\n",
                                    FormatSimpleLine(printerName, cartDetail.menu_name, cartDetail.qty.ToString()));

                                if (!string.IsNullOrEmpty(cartDetail.varian))
                                {
                                    strukBuilder.AppendFormat("   Varian: {0}\n", cartDetail.varian);
                                }

                                if (!string.IsNullOrEmpty(cartDetail.note_item))
                                {
                                    strukBuilder.AppendFormat("   Note: {0}\n", cartDetail.note_item);
                                }

                                strukBuilder.Append(kodeHeksadesimalNormal);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("no check");
                    }

                    // Footer
                    strukBuilder.AppendFormat("{0}", CenterText($"Meja No.{datas.data?.customer_seat}"));
                    strukBuilder.Append("--------------------------------\n\n\n\n\n");

                    // Konversi dan Cetak
                    byte[] buffer = Encoding.UTF8.GetBytes(strukBuilder.ToString());
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                }
            }
            finally
            {
                // Cleanup stream
                stream?.Close();
                stream?.Dispose();

                // Optional: Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        private async Task PrintKitchenOrBarReceipt(
    GetStrukCustomerTransaction datas,
    List<KitchenAndBarCartDetails> CartDetails,
    List<KitchenAndBarCanceledItems> CancelItems,
    int totalTransactions, string type, string printerName)
        {
            Stream stream = Stream.Null;

            try
            {
                // Koneksi printer (tetap sama)
                stream = await EstablishPrinterConnection(printerName);

                // Hitung kapasitas optimal
                int totalItems = (CartDetails?.Count ?? 0) + (CancelItems?.Count ?? 0);
                var capacity = GetOptimalCapacity(totalItems);
                var strukBuilder = new StringBuilder(capacity);

                // Kode Heksadesimal
                string kodeHeksadesimalBold = "\x1B\x45\x01";
                string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

                // Header
                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}{1}",
                    kodeHeksadesimalBold,
                    CenterText($"{type.ToUpper()} No. {totalTransactions}"));

                strukBuilder.AppendFormat("{0}{1}",
                    kodeHeksadesimalBold,
                    CenterText($"Receipt No.: {datas.data?.receipt_number}"));

                strukBuilder.Append(kodeHeksadesimalNormal);
                strukBuilder.AppendFormat("{0}", CenterText(datas.data?.invoice_due_date));
                strukBuilder.AppendFormat("Name: {0}\n", datas.data?.customer_name);

                // Order Items
                if (CartDetails?.Count > 0)
                {
                    var servingTypes = CartDetails
                        .Select(cd => cd.serving_type_name)
                        .Distinct();

                    strukBuilder.Append(SEPARATOR);
                    strukBuilder.AppendFormat("{0}", CenterText("ORDER"));

                    foreach (string servingType in servingTypes)
                    {
                        var itemsForServingType = CartDetails
                            .Where(cd => cd.serving_type_name == servingType)
                            .ToList();

                        if (itemsForServingType.Count == 0) continue;

                        strukBuilder.AppendFormat("{0}\n", CenterText(servingType));

                        foreach (var cartDetail in itemsForServingType)
                        {
                            strukBuilder.Append(kodeHeksadesimalSizeBesar + kodeHeksadesimalBold);
                            strukBuilder.AppendFormat("{0}\n",
                                FormatSimpleLine(printerName, cartDetail.menu_name, cartDetail.qty.ToString()));

                            if (!string.IsNullOrEmpty(cartDetail.varian))
                            {
                                strukBuilder.AppendFormat("   Varian: {0}\n", cartDetail.varian);
                            }

                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            {
                                strukBuilder.AppendFormat("   Note: {0}\n", cartDetail.note_item);
                            }

                            strukBuilder.Append(kodeHeksadesimalNormal);
                            strukBuilder.Append("\n");
                        }
                    }
                }

                // Canceled Items
                if (CancelItems?.Count > 0)
                {
                    var servingTypes = CancelItems
                        .Select(cd => cd.serving_type_name)
                        .Distinct();

                    strukBuilder.Append(SEPARATOR);
                    strukBuilder.AppendFormat("{0}", CenterText("CANCELED"));

                    foreach (string servingType in servingTypes)
                    {
                        var itemsForServingType = CancelItems
                            .Where(cd => cd.serving_type_name == servingType)
                            .ToList();

                        if (itemsForServingType.Count == 0) continue;

                        strukBuilder.AppendFormat("{0}\n", CenterText(servingType));

                        foreach (var cancelItem in itemsForServingType)
                        {
                            strukBuilder.Append(kodeHeksadesimalSizeBesar + kodeHeksadesimalBold);
                            strukBuilder.AppendFormat("{0}\n",
                                FormatSimpleLine(printerName, cancelItem.menu_name, cancelItem.qty.ToString()));

                            if (!string.IsNullOrEmpty(cancelItem.varian))
                            {
                                strukBuilder.AppendFormat("   Varian: {0}\n", cancelItem.varian);
                            }

                            if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                            {
                                strukBuilder.AppendFormat("   Note: {0}\n", cancelItem.note_item);
                            }

                            if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                            {
                                strukBuilder.AppendFormat("   Canceled Reason: {0}\n", cancelItem.cancel_reason);
                            }

                            strukBuilder.Append(kodeHeksadesimalNormal);
                        }
                    }
                }

                // Footer
                strukBuilder.AppendFormat("{0}",
                    CenterText($"Meja No.{datas.data?.customer_seat}\n"));
                strukBuilder.Append("--------------------------------\n\n\n\n\n");

                // Konversi dan Cetak
                byte[] buffer = Encoding.UTF8.GetBytes(strukBuilder.ToString());
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            finally
            {
                // Cleanup stream
                stream?.Close();
                stream?.Dispose();

                // Optional: Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        public async Task Ex_PrintModelPayform
        (GetStrukCustomerTransaction datas,
            List<CartDetailStrukCustomerTransaction> cartDetails,
            List<KitchenAndBarCartDetails> KitchenCartDetails,
            List<KitchenAndBarCartDetails> BarCartDetails,
            List<KitchenAndBarCanceledItems> KitchenCancelItems,
            List<KitchenAndBarCanceledItems> BarCancelItems,
            int totalTransactions,
            string Kakimu,
            string printerId,
            string printerName)
        {
            try
            {
                LoggerUtil.LogWarning("Ex_PrintModelPayform");

                Stream stream = null;
                if (IPAddress.TryParse(printerName, out _))
                {
                    // Connect via LAN
                    TcpClient client = new(printerName, 9100);
                    stream = client.GetStream();
                }
                else
                {
                    // Bluetooth connection with retry policy
                    await RetryPolicyAsync(async () =>
                    {
                        BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            return false;
                        }

                        BluetoothClient client = new();
                        BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);
                        if (!printerDevice.Authenticated) // cek sudah paired?
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                NotifyHelper.Error("Pairing gagal. Pastikan printer sudah dipair manual di Windows Settings.");
                                return false;
                            }
                        }

                        client.Connect(endpoint);
                        stream = client.GetStream();
                        return true;
                    }, 3);
                }

                // Struct Customer ====
                if (ShouldPrint(printerId, "Kasir"))
                {
                    PrintDocument printDocument = new();
                    printDocument.PrintPage += (sender, e) =>
                    {
                        Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                        // Mengatur font normal dan tebal
                        Font normalFont =
                            new("Arial", 8,
                                FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                        Font boldFont = new("Arial", 8, FontStyle.Bold);
                        Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                        Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                        float leftMargin = 5; // Margin kiri (dalam pixel)
                        float rightMargin = 5; // Margin kanan (dalam pixel)
                        float topMargin = 5; // Margin atas (dalam pixel)
                        float yPos = topMargin;

                        // Lebar kertas thermal 58mm, sesuaikan dengan margin
                        float paperWidth =
                            58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                        float printableWidth = paperWidth - leftMargin - rightMargin;

                        // Fungsi untuk format teks kiri dan kanan
                        void DrawSimpleLine(string textLeft, string textRight)
                        {
                            SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                            SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                            e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                            e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                leftMargin + printableWidth - sizeRight.Width, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                        void DrawCenterText(string text, Font font)
                        {
                            if (text == null)
                            {
                                text = string.Empty;
                            }

                            if (font == null)
                            {
                                throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                    leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }

                        // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                        void DrawLeftText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }


                        // Fungsi untuk menggambar garis pemisah
                        void DrawSeparator()
                        {
                            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        // Fungsi untuk mendapatkan dan mengonversi gambar logo ke hitam dan putih
                        Image GetLogoImage(string path)
                        {
                            Image img = Image.FromFile(path);
                            Bitmap bmp = new(img.Width, img.Height);
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                ColorMatrix colorMatrix = new(new[]
                                {
                                    new[] { 0.3f, 0.3f, 0.3f, 0, 0 }, new[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                                    new[] { 0.11f, 0.11f, 0.11f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 },
                                    new float[] { 0, 0, 0, 0, 1 }
                                });
                                ImageAttributes attributes = new();
                                attributes.SetColorMatrix(colorMatrix);
                                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width,
                                    img.Height, GraphicsUnit.Pixel, attributes);
                            }

                            return bmp;
                        }

                        // Menambahkan logo dan teks "Powered by" di akhir struk
                        void DrawPoweredByLogo(string path)
                        {
                            if (!File.Exists(path)) { return; }
                            // Menambahkan jarak sebelum mencetak teks dan logo
                            float spaceBefore = 50; // Jarak dalam pixel, sesuaikan dengan kebutuhan
                            yPos += spaceBefore;

                            // Mengukur teks "Powered by Your Company"
                            string poweredByText = "Powered by Dastrevas";
                            SizeF textSize = e.Graphics.MeasureString(poweredByText, normalFont);

                            // Gambar teks
                            float textX = leftMargin + ((printableWidth - textSize.Width) / 2);
                            e.Graphics.DrawString(poweredByText, normalFont, Brushes.Black, textX, yPos);

                            // Sesuaikan yPos untuk logo
                            yPos += textSize.Height;

                            // Menggambar logo
                            Image logoPoweredBy = GetLogoImage(path);
                            float targetWidth = 35; // Ukuran lebar logo dalam pixel, sesuaikan dengan kebutuhan
                            float scaleFactor = targetWidth / logoPoweredBy.Width;
                            float logoHeight = logoPoweredBy.Height * scaleFactor;

                            float logoX = leftMargin + ((printableWidth - targetWidth) / 2);
                            e.Graphics.DrawImage(logoPoweredBy, logoX, yPos, targetWidth, logoHeight);

                            spaceBefore = 5;
                            yPos += spaceBefore;
                            // Sesuaikan yPos untuk elemen berikutnya
                            yPos += logoHeight;
                        }

                        void DrawSpace()
                        {
                            yPos += normalFont
                                .GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                        }


                        DrawCenterText("No. " + totalTransactions, NomorAntrian);

                        // Path ke logo Powered by Anda
                        string poweredByLogoPath = "icon\\DT-Logo.bmp"; // Ganti dengan path logo Powered by Anda

                        // Menambahkan logo di bagian atas dengan ukuran yang proporsional
                        string logoPath = "icon\\OutletLogo.bmp"; // Ganti dengan path logo Anda
                        Image logo = GetLogoImage(logoPath);
                        float logoTargetWidthMm = 25; // Lebar target logo dalam mm
                        float logoTargetWidthPx = logoTargetWidthMm / 25.4f * 100; // Konversi ke pixel

                        // Hitung tinggi logo berdasarkan lebar yang diinginkan dengan mempertahankan rasio aspek
                        float scaleFactor = logoTargetWidthPx / logo.Width;
                        float logoHeight = logo.Height * scaleFactor;
                        float logoX = leftMargin + ((printableWidth - logoTargetWidthPx) / 2);

                        // Gambar logo dengan ukuran yang diubah
                        e.Graphics.DrawImage(logo, logoX, yPos, logoTargetWidthPx, logoHeight);
                        yPos += logoHeight + 5; // Menambahkan jarak setelah logo

                        // Struct templateDrawCenterText(datas.data.outlet_name, BigboldFont);
                        DrawCenterText(datas.data.outlet_address, normalFont);
                        DrawCenterText(datas.data.outlet_phone_number, normalFont);
                        DrawSpace();
                        DrawCenterText("Receipt No. " + datas.data.receipt_number + "\n", normalFont);

                        DrawSeparator();
                        string nomorMeja = "Meja No." + datas.data.customer_seat;
                        DrawCenterText(datas.data.outlet_name, BigboldFont);
                        DrawCenterText(datas.data.outlet_address, normalFont);
                        DrawCenterText(datas.data.outlet_phone_number, normalFont);
                        DrawSpace();
                        DrawCenterText("Receipt No. " + datas.data.receipt_number + "\n", normalFont);

                        DrawSeparator();

                        DrawCenterText("PEMBELIAN", boldFont);
                        DrawSeparator();
                        DrawCenterText(datas.data.invoice_due_date + "\n", normalFont);
                        DrawSpace();
                        DrawSimpleLine(datas.data.customer_name, "");

                        if (datas.data.member_name != null)
                        {
                            DrawSeparator();
                            DrawSimpleLine("MEMBER: ", datas.data.member_name);
                            DrawSimpleLine("No. HP: ", datas.data.member_phone_number);
                            DrawSeparator();
                        }

                        IEnumerable<string> servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();
                        foreach (string servingType in servingTypes)
                        {
                            List<CartDetailStrukCustomerTransaction> itemsForServingType =
                                cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                            if (itemsForServingType.Count == 0)
                            {
                                continue;
                            }

                            DrawSeparator();
                            DrawCenterText(servingType, normalFont);
                            DrawSeparator();
                            foreach (CartDetailStrukCustomerTransaction cartDetail in itemsForServingType)
                            {
                                DrawSimpleLine(cartDetail.qty + " " + cartDetail.menu_name,
                                    string.Format("{0:n0}", cartDetail.price));
                                if (!string.IsNullOrEmpty(cartDetail.varian))
                                {
                                    DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                }

                                if (!string.IsNullOrEmpty(cartDetail.note_item))
                                {
                                    DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                }

                                if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                {
                                    DrawLeftText("   Discount Code: " + cartDetail.discount_code, normalFont);
                                }

                                if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                                {
                                    DrawSimpleLine("   Total Discount: ",
                                        string.Format("{0:n0}", cartDetail.discounted_price));
                                }

                                DrawSimpleLine("   Total Price:", string.Format("{0:n0}", cartDetail.total_price));
                                DrawSpace();
                            }
                        }

                        DrawSeparator();
                        DrawSimpleLine("Subtotal", string.Format("{0:n0}", datas.data.subtotal));
                        if (!string.IsNullOrEmpty(datas.data.discount_code))
                        {
                            DrawLeftText("Discount Code: " + datas.data.discount_code, normalFont);
                        }

                        if (datas.data.discounts_value.HasValue && datas.data.discounts_value != 0)
                        {
                            DrawSimpleLine("Discount Value: ",
                                datas.data.discounts_is_percent != "1"
                                    ? string.Format("{0:n0}", datas.data.discounts_value.ToString())
                                    : datas.data.discounts_value + " %");
                        }

                        DrawSimpleLine("Total", string.Format("{0:n0}", datas.data.total));
                        DrawLeftText("Payment Type: " + datas.data.payment_type, normalFont);
                        DrawSimpleLine("Cash", string.Format("{0:n0}", datas.data.customer_cash));
                        DrawSimpleLine("Change", string.Format("{0:n0}", datas.data.customer_change));
                        DrawSeparator();
                        DrawCenterText(datas.data.outlet_footer, normalFont);
                        DrawCenterText(Kakimu, normalFont);
                        DrawPoweredByLogo(poweredByLogoPath);
                        DrawSpace();
                        DrawCenterText(nomorMeja, NomorAntrian);
                    };
                }

                // Struck Checker
                if (ShouldPrint(printerId, "Checker"))
                {
                    PrintDocument printDocument = new();
                    printDocument.PrintPage += (sender, e) =>
                    {
                        Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                        // Mengatur font normal dan tebal
                        Font normalFont =
                            new("Arial", 8,
                                FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                        Font boldFont = new("Arial", 8, FontStyle.Bold);
                        Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                        Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                        float leftMargin = 5; // Margin kiri (dalam pixel)
                        float rightMargin = 5; // Margin kanan (dalam pixel)
                        float topMargin = 5; // Margin atas (dalam pixel)
                        float yPos = topMargin;

                        // Lebar kertas thermal 58mm, sesuaikan dengan margin
                        float paperWidth =
                            58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                        float printableWidth = paperWidth - leftMargin - rightMargin;


                        // Fungsi untuk format teks kiri dan kanan
                        void DrawSimpleLine(string textLeft, string textRight)
                        {
                            SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                            SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                            e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                            e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                leftMargin + printableWidth - sizeRight.Width, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                        void DrawCenterText(string text, Font font)
                        {
                            if (text == null)
                            {
                                text = string.Empty;
                            }

                            if (font == null)
                            {
                                throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                    leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }

                        // Fungsi untuk menggambar teks rata kiri dengan pemotongan otomatis
                        void DrawLeftText(string text, Font font)
                        {
                            if (text == null)
                            {
                                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                                return;
                            }

                            string[] words = text.Split(' ');
                            StringBuilder currentLine = new();
                            foreach (string word in words)
                            {
                                SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                if (size.Width > printableWidth)
                                {
                                    // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin,
                                        yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                    currentLine.Clear();
                                }

                                // Tambahkan kata ke baris saat ini
                                currentLine.Append(word + " ");
                            }

                            // Gambar baris terakhir
                            if (currentLine.Length > 0)
                            {
                                e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
                                yPos += font.GetHeight(e.Graphics);
                            }
                        }


                        // Fungsi untuk menggambar garis pemisah
                        void DrawSeparator()
                        {
                            e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                            yPos += normalFont.GetHeight(e.Graphics);
                        }

                        void DrawSpace()
                        {
                            yPos += normalFont
                                .GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                        }


                        DrawCenterText("No. " + totalTransactions, NomorAntrian);
                        string nomorMeja = "Meja No." + datas.data.customer_seat;
                        DrawSeparator();

                        //Struct Checker
                        DrawCenterText("CHECKER", BigboldFont);
                        DrawCenterText("Receipt No. " + datas.data.receipt_number, normalFont);
                        DrawSeparator();
                        DrawCenterText(datas.data.invoice_due_date, normalFont);
                        DrawSpace();
                        DrawSimpleLine(datas.data.customer_name, "");
                        // Iterate through cart details and group by serving_type_name
                        IEnumerable<string> checkerServingTypes =
                            cartDetails.Select(cd => cd.serving_type_name).Distinct();

                        foreach (string checkerServingType in checkerServingTypes)
                        {
                            // Filter cart details by serving_type_name
                            List<CartDetailStrukCustomerTransaction> itemsForServingType =
                                cartDetails.Where(cd => cd.serving_type_name == checkerServingType).ToList();

                            // Skip if there are no items for this serving type
                            if (itemsForServingType.Count == 0)
                            {
                                continue;
                            }

                            // Add a section for the serving type
                            DrawSeparator();
                            DrawCenterText(checkerServingType, normalFont);
                            DrawSeparator();

                            // Iterate through items for this serving type
                            foreach (CartDetailStrukCustomerTransaction cartDetail in itemsForServingType)
                            {
                                DrawSimpleLine(cartDetail.menu_name, cartDetail.qty.ToString());
                                // Add detail items
                                if (!string.IsNullOrEmpty(cartDetail.varian))
                                {
                                    DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                }

                                if (!string.IsNullOrEmpty(cartDetail.note_item))
                                {
                                    DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                }

                                DrawSpace();
                            }
                        }

                        DrawSeparator();
                        DrawSpace();
                        DrawCenterText(nomorMeja, NomorAntrian);
                    };
                }

                // Struct Kitchen

                if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                {
                    if (ShouldPrint(printerId, "Makanan"))
                    {
                        PrintDocument printDocument = new();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                            // Mengatur font normal dan tebal
                            Font normalFont =
                                new("Arial", 8,
                                    FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                            float leftMargin = 5; // Margin kiri (dalam pixel)
                            float rightMargin = 5; // Margin kanan (dalam pixel)
                            float topMargin = 5; // Margin atas (dalam pixel)
                            float yPos = topMargin;

                            // Lebar kertas thermal 58mm, sesuaikan dengan margin
                            float paperWidth =
                                58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                            float printableWidth = paperWidth - leftMargin - rightMargin;


                            // Fungsi untuk format teks kiri dan kanan
                            void DrawSimpleLine(string textLeft, string textRight)
                            {
                                SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                                SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                                e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                    leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null)
                                {
                                    text = string.Empty;
                                }

                                if (font == null)
                                {
                                    throw new ArgumentNullException(nameof(font),
                                        "Font is null in DrawCenterText method.");
                                }

                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                            leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                        yPos += font.GetHeight(e.Graphics);
                                        currentLine.Clear();
                                    }

                                    // Tambahkan kata ke baris saat ini
                                    currentLine.Append(word + " ");
                                }

                                // Gambar baris terakhir
                                if (currentLine.Length > 0)
                                {
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                }
                            }

                            // Fungsi untuk menggambar garis pemisah
                            void DrawSeparator()
                            {
                                e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            void DrawSpace()
                            {
                                yPos += normalFont
                                    .GetHeight(e
                                        .Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                            }


                            DrawCenterText("No. " + totalTransactions, NomorAntrian);
                            string nomorMeja = "Meja No." + datas.data.customer_seat;

                            // Generate struk text
                            DrawSeparator();
                            DrawCenterText("MAKANAN", BigboldFont);
                            DrawCenterText(datas.data.receipt_number, normalFont);
                            DrawSeparator();
                            DrawCenterText(datas.data.invoice_due_date, normalFont);
                            DrawSpace();
                            DrawSimpleLine(datas.data.customer_name, "");

                            if (KitchenCartDetails.Count != 0)
                            {
                                IEnumerable<string> servingTypes =
                                    KitchenCartDetails.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("ORDER", boldFont);

                                foreach (string servingType in servingTypes)
                                {
                                    List<KitchenAndBarCartDetails> itemsForServingType = KitchenCartDetails
                                        .Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                    {
                                        continue;
                                    }

                                    DrawSeparator();
                                    DrawCenterText(servingType, normalFont);

                                    foreach (KitchenAndBarCartDetails cartDetail in itemsForServingType)
                                    {
                                        string qtyMenu = "x" + cartDetail.qty;
                                        DrawCenterText(qtyMenu + " " + cartDetail.menu_name, BigboldFont);

                                        if (!string.IsNullOrEmpty(cartDetail.varian))
                                        {
                                            DrawCenterText("Varian: " + cartDetail.varian, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                        {
                                            DrawCenterText("Note: " + cartDetail.note_item, BigboldFont);
                                        }

                                        DrawSpace();
                                    }
                                }
                            }

                            if (KitchenCancelItems.Count != 0)
                            {
                                IEnumerable<string> servingTypes =
                                    KitchenCancelItems.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("CANCELED", boldFont);

                                foreach (string servingType in servingTypes)
                                {
                                    List<KitchenAndBarCanceledItems> itemsForServingType = KitchenCancelItems
                                        .Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                    {
                                        continue;
                                    }

                                    DrawSeparator();
                                    DrawCenterText(servingType, boldFont);

                                    foreach (KitchenAndBarCanceledItems cancelItem in itemsForServingType)
                                    {
                                        string qtyMenu = "x" + cancelItem.qty;
                                        DrawCenterText(qtyMenu + " " + cancelItem.menu_name, BigboldFont);

                                        if (!string.IsNullOrEmpty(cancelItem.varian))
                                        {
                                            DrawCenterText("Varian: " + cancelItem.varian, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                        {
                                            DrawCenterText("Note: " + cancelItem.note_item, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                        {
                                            DrawCenterText("Canceled Reason: " + cancelItem.cancel_reason, BigboldFont);
                                        }

                                        DrawSpace();
                                    }
                                }
                            }

                            DrawSeparator();
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);
                        };
                    }
                }

                // Struct Bar
                if (BarCartDetails.Any() || BarCancelItems.Any())
                {
                    if (ShouldPrint(printerId, "Minuman"))
                    {
                        PrintDocument printDocument = new();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics ?? Graphics.FromImage(new Bitmap(1, 1));

                            // Mengatur font normal dan tebal
                            Font normalFont =
                                new("Arial", 8,
                                    FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new("Arial", 12, FontStyle.Bold);

                            float leftMargin = 5; // Margin kiri (dalam pixel)
                            float rightMargin = 5; // Margin kanan (dalam pixel)
                            float topMargin = 5; // Margin atas (dalam pixel)
                            float yPos = topMargin;

                            // Lebar kertas thermal 58mm, sesuaikan dengan margin
                            float paperWidth =
                                58 / 25.4f * 85; // 58mm converted to inches and then to hundredths of an inch
                            float printableWidth = paperWidth - leftMargin - rightMargin;


                            // Fungsi untuk format teks kiri dan kanan
                            void DrawSimpleLine(string textLeft, string textRight)
                            {
                                SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                                SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                                e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black,
                                    leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null)
                                {
                                    text = string.Empty;
                                }

                                if (font == null)
                                {
                                    throw new ArgumentNullException(nameof(font),
                                        "Font is null in DrawCenterText method.");
                                }

                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                            leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                        yPos += font.GetHeight(e.Graphics);
                                        currentLine.Clear();
                                    }

                                    // Tambahkan kata ke baris saat ini
                                    currentLine.Append(word + " ");
                                }

                                // Gambar baris terakhir
                                if (currentLine.Length > 0)
                                {
                                    SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black,
                                        leftMargin + ((printableWidth - currentSize.Width) / 2), yPos);
                                    yPos += font.GetHeight(e.Graphics);
                                }
                            }

                            // Fungsi untuk menggambar garis pemisah
                            void DrawSeparator()
                            {
                                if (e.Graphics != null)
                                {
                                    e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + printableWidth, yPos);
                                    yPos += normalFont.GetHeight(e.Graphics);
                                }
                            }

                            void DrawSpace()
                            {
                                if (e.Graphics != null)
                                {
                                    yPos += normalFont.GetHeight(e.Graphics);
                                }
                            }


                            DrawCenterText("No. " + totalTransactions, NomorAntrian);
                            string nomorMeja = "Meja No." + datas.data.customer_seat;

                            // Generate struk text
                            DrawSeparator();
                            DrawCenterText("MINUMAN", BigboldFont);
                            DrawCenterText(datas.data.receipt_number, normalFont);
                            DrawSeparator();
                            DrawCenterText(datas.data.invoice_due_date, normalFont);
                            DrawSpace();
                            DrawSimpleLine(datas.data.customer_name, "");

                            if (BarCartDetails.Count != 0)
                            {
                                IEnumerable<string> servingTypes =
                                    BarCartDetails.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("ORDER", boldFont);

                                foreach (string servingType in servingTypes)
                                {
                                    List<KitchenAndBarCartDetails> itemsForServingType = BarCartDetails
                                        .Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                    {
                                        continue;
                                    }

                                    DrawSeparator();
                                    DrawCenterText(servingType, normalFont);

                                    foreach (KitchenAndBarCartDetails cartDetail in itemsForServingType)
                                    {
                                        string qtyMenu = "x" + cartDetail.qty;
                                        DrawCenterText(qtyMenu + " " + cartDetail.menu_name, BigboldFont);

                                        if (!string.IsNullOrEmpty(cartDetail.varian))
                                        {
                                            DrawCenterText("Varian: " + cartDetail.varian, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                        {
                                            DrawCenterText("Note: " + cartDetail.note_item, BigboldFont);
                                        }

                                        DrawSpace();
                                    }
                                }
                            }

                            if (BarCancelItems.Count != 0)
                            {
                                IEnumerable<string> servingTypes =
                                    BarCancelItems.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("CANCELED", boldFont);

                                foreach (string servingType in servingTypes)
                                {
                                    List<KitchenAndBarCanceledItems> itemsForServingType =
                                        BarCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                    {
                                        continue;
                                    }

                                    DrawSeparator();
                                    DrawCenterText(servingType, boldFont);

                                    foreach (KitchenAndBarCanceledItems cancelItem in itemsForServingType)
                                    {
                                        string qtyMenu = "x" + cancelItem.qty;
                                        DrawCenterText(qtyMenu + " " + cancelItem.menu_name, BigboldFont);

                                        if (!string.IsNullOrEmpty(cancelItem.varian))
                                        {
                                            DrawCenterText("Varian: " + cancelItem.varian, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                        {
                                            DrawCenterText("Note: " + cancelItem.note_item, BigboldFont);
                                        }

                                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                        {
                                            DrawCenterText("Canceled Reason: " + cancelItem.cancel_reason, BigboldFont);
                                        }

                                        DrawSpace();
                                    }
                                }
                            }

                            DrawSeparator();
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        //Retry Policy

        public async Task<bool> RetryPolicyAsync(Func<Task<bool>> action, int maxRetries)
        {
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < maxRetries)
            {
                attempt++;
                try
                {
                    if (await action())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LoggerUtil.LogError(ex, $"Attempt {attempt} failed: {ex.Message}");
                    NotifyHelper.Error($"Percobaan {attempt} gagal: {ex.Message}");
                }
                await Task.Delay(1000 * attempt);

            }

            // Log details error
            Util util = new();
            util.sendLogTelegramNetworkError($"All {maxRetries} attempts failed. Last error: {lastException}");
            NotifyHelper.Error($"Semua percobaan {maxRetries} x gagal. Error terakhir: {lastException}");

            return false;
        }
        public bool IsNotMacAddressOrIpAddress(string input)
        {
            return !IsMacAddress(input) && !IsIpAddress(input) && input.Length > 3;
        }

        // Method to check if the string is a MAC address
        private bool IsMacAddress(string input)
        {
            string macPattern = "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
            return Regex.IsMatch(input, macPattern);
        }

        // Method to check if the string is an IP address
        private bool IsIpAddress(string input)
        {
            return IPAddress.TryParse(input, out _);
        }

        // Other methods and properties of the PrinterModel class
    }
}