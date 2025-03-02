using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using KASIR.Model;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Polly.Caching;
using Newtonsoft.Json;
using KASIR.Komponen;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Printing;
using System.Net;

namespace KASIR.Printer
{
    public class PrinterModel
    {
        private readonly string baseDirectory;
        //private readonly Dictionary<string, string> printerSettings = new Dictionary<string, string>();
        private readonly Dictionary<string, string> printerSettings = new Dictionary<string, string>();
        private readonly Dictionary<string, bool> checkBoxSettings = new Dictionary<string, bool>();
        private readonly Graphics graphics;

        // Member variable to store the Kategori value
        private string _kategori;

        public PrinterModel()
        {

            baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting");
            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }
        }
        public async Task SavePrinterBluetoothMACSettings(string comboBoxName, string printerId)
        {
            printerSettings[comboBoxName] = printerId;
            await SaveSettingsToFile("printerSettings.data", SerializePrinterSettings());
        }

        public async Task SavePrinterSettings(string comboBoxName, string printerId)
        {
            printerSettings[comboBoxName] = printerId;
            await SaveSettingsToFile("printerSettings.data", SerializePrinterSettings());
        }
        public async Task DelPrinterSettings(string comboBoxName)
        {
            if (printerSettings.ContainsKey(comboBoxName))
            {
                printerSettings.Remove(comboBoxName);
                await SaveSettingsToFile("printerSettings.data", SerializePrinterSettings());
            }
        }

        public async Task<string> LoadPrinterSettingsAsync(string comboBoxName)
        {
            await LoadAllPrinterSettings();
            if (printerSettings.TryGetValue(comboBoxName, out string printerId))
            {
                return printerId;
            }
            return null;
        }

        public async Task SaveCheckBoxSettingAsync(string checkBoxName, bool isChecked)
        {
            checkBoxSettings[checkBoxName] = isChecked;
            await SaveSettingsToFile("checkBoxSettings.data", SerializeCheckBoxSettings());
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
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in printerSettings)
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
            List<PrinterItem> availablePrinters = new List<PrinterItem>();

            try
            {
                await Task.Run(() =>
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
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
                            catch (Exception innerEx)
                            {
                                // Log individual printer query errors
                                //MessageBox.Show($"Error processing printer: {innerEx.Message}");
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // Log general errors
                //MessageBox.Show($"Error retrieving printers: {ex.Message}");
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

        public async Task SaveSettingsToFile(string fileName, string data)
        {
            string filePath = Path.Combine(baseDirectory, fileName);
            await File.WriteAllTextAsync(filePath, data);
        }

        public string SerializeCheckBoxSettings()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in checkBoxSettings)
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
            var lines = await File.ReadAllLinesAsync(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    printerSettings[parts[0]] = parts[1];
                }
            }
        }

        private async Task LoadCheckBoxSettingsAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && bool.TryParse(parts[1], out var value))
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
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += Ex_PrintDocument_PrintPage;
                        //Ex_PrintDocument_PrintPage();
                        continue;
                    }
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        _kategori = "Kasir";
                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                            continue;
                        }

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                continue;
                            }
                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";

                            strukText += kodeHeksadesimalSizeBesar + CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine("Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine("Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += "--------------------------------\n";
                            strukText += "Powered by Dastrevas\n";
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

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

                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                            continue;
                        }

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                continue;
                            }
                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";

                            strukText += kodeHeksadesimalSizeBesar + CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine("Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine("Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += "--------------------------------\n";
                            strukText += "Powered by Dastrevas\n";
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

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

                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                            continue;
                        }

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                continue;
                            }
                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";

                            strukText += kodeHeksadesimalSizeBesar + CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine("Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine("Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += "--------------------------------\n";
                            strukText += "Powered by Dastrevas\n";
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

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

                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                            continue;
                        }

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                continue;
                            }
                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";

                            strukText += kodeHeksadesimalSizeBesar + CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine("Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine("Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += "--------------------------------\n";
                            strukText += "Powered by Dastrevas\n";
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

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
            return checkBoxSettings.TryGetValue($"checkBox{printType}Printer{printerId}", out var shouldPrint) && shouldPrint;
        }
        private void Print(string printerName, string Kategori)
        {
            _kategori = Kategori;
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            try
            {
                if (IsBluetoothPrinter(printerName))
                {
                    printerName = ConvertMacAddressFormat(printerName);
                    // Inisialisasi PrintDocument

                    // Menambahkan event handler PrintPage ke event PrintPage dari PrintDocument
                    printDocument.PrintPage += PrintDocument_PrintPage;

                    // Memanggil method untuk memulai proses pencetakan
                    printDocument.Print();
                    //PrintViaBluetooth(printerName, printDocument);
                }
                else
                {
                    printDocument.PrinterSettings.PrinterName = printerName;
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                //MessageBox.Show("An error occurred while printing: " + ex.Message);
            }
        }


        // Bluetooth Struct Printing
        private string ConvertMacAddressFormat(string macAddress)
        {
            return macAddress.Replace("-", ":");
        }

        public bool IsBluetoothPrinter(string macAddress)
        {
            // Format yang diharapkan: "00:1A:2B:3C:4D:5E"
            // Pengecekan dengan ekspresi reguler (regex)
            string pattern = "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";

            // Buat objek Regex untuk memeriksa format
            Regex regex = new Regex(pattern);

            // Lakukan pencocokan regex untuk memeriksa validitas MAC Address
            return regex.IsMatch(macAddress);
        }


        private void PrintViaBluetooth(string printerName, PrintDocument printDocument)
        {

            // Get Bluetooth address from printer settings
            string bluetoothAddress = printerName;

            try
            {
                BluetoothClient client = new BluetoothClient();

                BluetoothDeviceInfo printer = new BluetoothDeviceInfo(BluetoothAddress.Parse(bluetoothAddress));
                if (printer == null)
                {
                    //MessageBox.Show("Printer" + bluetoothAddress + " not found", "Gaspol");
                    return;
                }

                BluetoothEndPoint endpoint = new BluetoothEndPoint(printer.DeviceAddress, BluetoothService.SerialPort);

                if (printer != null)
                {

                    if (!BluetoothSecurity.PairRequest(printer.DeviceAddress, "0000"))
                    {
                        //MessageBox.Show("Pairing failed to " + bluetoothAddress, "Gaspol");
                        return;
                    }

                    // Pair request with PIN 0000
                    //BluetoothSecurity.PairRequest(device.DeviceAddress, "0000");

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
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintDocument_PrintPage(sender, e);
                        continue;
                    }
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                            continue;
                        }

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                continue;
                            }
                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori) + "\n";
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";

                            strukText += kodeHeksadesimalSizeBesar + CenterText("Printed Success at Mac Address" + printerName) + "\n";
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";
                            strukText += CenterText("Test Tengah") + "\n";
                            strukText += FormatSimpleLine("Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine("Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += "--------------------------------\n";
                            strukText += "Powered by Dastrevas\n";
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();

                            clientSocket.GetStream().Close();
                            stream.Close();
                            clientSocket.Close();
                        }
                    }
                    if (ShouldPrint(printerId, "Checker"))
                    {
                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                            continue;
                        }

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                continue;
                            }
                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";

                            strukText += kodeHeksadesimalSizeBesar + CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine("Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine("Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += "--------------------------------\n";
                            strukText += "Powered by Dastrevas\n";
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();

                            clientSocket.GetStream().Close();
                            stream.Close();
                            clientSocket.Close();
                        }
                    }
                    if (ShouldPrint(printerId, "Makanan"))
                    {
                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                            continue;
                        }

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                continue;
                            }
                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";

                            strukText += kodeHeksadesimalSizeBesar + CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine("Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine("Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += "--------------------------------\n";
                            strukText += "Powered by Dastrevas\n";
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

                            stream.Write(buffer, 0, buffer.Length);
                            stream.Flush();

                            clientSocket.GetStream().Close();
                            stream.Close();
                            clientSocket.Close();
                        }
                    }
                    if (ShouldPrint(printerId, "Minuman"))
                    {
                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null)
                        {
                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                            continue;
                        }

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                        using (BluetoothClient clientSocket = new BluetoothClient())
                        {
                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                            {
                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                continue;
                            }
                            clientSocket.Connect(endpoint);
                            System.IO.Stream stream = clientSocket.GetStream();

                            string kodeHeksadesimalBold = "\x1B\x45\x01";
                            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
                            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";
                            string Kategori = _kategori;

                            string strukText = "\n" + kodeHeksadesimalBold + CenterText(Kategori);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";

                            strukText += kodeHeksadesimalSizeBesar + CenterText("Printed Success at Mac Address" + printerName);
                            strukText += kodeHeksadesimalNormal;
                            strukText += "--------------------------------\n";
                            strukText += CenterText("Test Tengah");
                            strukText += FormatSimpleLine("Test Data Kiri", "Test Data Kanan") + "\n";
                            strukText += kodeHeksadesimalBold;
                            strukText += FormatSimpleLine("Test Data Kiri Tebal", "Bold") + "\n";

                            strukText += kodeHeksadesimalNormal;

                            strukText += "--------------------------------\n";
                            strukText += "Powered by Dastrevas\n";
                            strukText += "--------------------------------\n\n\n\n\n";
                            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

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



        private void Ex_PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                string Kategori = _kategori;
                // Mengatur font normal dan tebal
                Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                Font boldFont = new Font("Arial", 8, FontStyle.Bold);
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
                    e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                    yPos += normalFont.GetHeight(e.Graphics);
                }

                // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                void DrawCenterText(string text, Font font)
                {
                    if (text == null) text = string.Empty;
                    if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                    string[] words = text.Split(' ');
                    StringBuilder currentLine = new StringBuilder();
                    foreach (string word in words)
                    {
                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                        if (size.Width > printableWidth)
                        {
                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                            SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                    Bitmap bmp = new Bitmap(img.Width, img.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                        {
                new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
                        });
                        ImageAttributes attributes = new ImageAttributes();
                        attributes.SetColorMatrix(colorMatrix);
                        g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attributes);
                    }
                    return bmp;
                }

                // Menambahkan logo dan teks "Powered by" di akhir struk
                void DrawPoweredByLogo(string path)
                {
                    // Menambahkan jarak sebelum mencetak teks dan logo
                    float spaceBefore = 50; // Jarak dalam pixel, sesuaikan dengan kebutuhan
                    yPos += spaceBefore;

                    // Mengukur teks "Powered by Your Company"
                    string poweredByText = "Powered by Dastrevas";
                    SizeF textSize = e.Graphics.MeasureString(poweredByText, normalFont);

                    // Gambar teks
                    float textX = leftMargin + (printableWidth - textSize.Width) / 2;
                    e.Graphics.DrawString(poweredByText, normalFont, Brushes.Black, textX, yPos);

                    // Sesuaikan yPos untuk logo
                    yPos += textSize.Height;

                    // Menggambar logo
                    Image logoPoweredBy = GetLogoImage(path);
                    float targetWidth = 35; // Ukuran lebar logo dalam pixel, sesuaikan dengan kebutuhan
                    float scaleFactor = targetWidth / logoPoweredBy.Width;
                    float logoHeight = logoPoweredBy.Height * scaleFactor;

                    float logoX = leftMargin + (printableWidth - targetWidth) / 2;
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
                float logoX = leftMargin + (printableWidth - logoTargetWidthPx) / 2;

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
        // Struct Print Laporan Shift
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
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintModelCetakLaporanShift(dataShifts, expenditures, cartDetailsSuccess, cartDetailsPendings, cartDetailsCanceled, refundDetails, paymentDetails);
                        continue;
                    }
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        System.IO.Stream stream = null;

                        try
                        {
                            if (System.Net.IPAddress.TryParse(printerName, out _))
                            {
                                // Connect via LAN
                                var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                stream = client.GetStream();
                            }
                            else
                            {
                                // Connect via Bluetooth dengan retry policy
                                if (!await RetryPolicyAsync(async () =>
                                {
                                    // Connect via Bluetooth
                                    BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                    if (printerDevice == null)
                                    {
                                        //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                        return false;
                                    }

                                    BluetoothClient client = new BluetoothClient();
                                    BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                    if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                    {
                                        //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                        return false;
                                    }

                                    client.Connect(endpoint);
                                    stream = client.GetStream();

                                    return true;
                                }, maxRetries: 3))
                                {
                                    continue;
                                }
                            }

                            string strukText = GenerateStrukTextShiftLaporan(dataShifts, expenditures, cartDetailsSuccess, cartDetailsPendings, cartDetailsCanceled, refundDetails, paymentDetails);
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);
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

        private string GenerateStrukTextShiftLaporan(DataStrukShift dataShifts,
            List<ExpenditureStrukShift> expenditures,
            List<CartDetailsSuccessStrukShift> cartDetailsSuccess,
            List<CartDetailsPendingStrukShift> cartDetailsPendings,
            List<CartDetailsCanceledStrukShift> cartDetailsCanceled,
            List<RefundDetailStrukShift> refundDetails,
            List<PaymentDetailStrukShift> paymentDetails)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            string strukText = "\n" + kodeHeksadesimalBold + CenterText(dataShifts.outlet_name);
            strukText += kodeHeksadesimalNormal;
            strukText += CenterText(dataShifts.outlet_address);
            strukText += CenterText(dataShifts.outlet_phone_number);

            strukText += "--------------------------------\n";

            strukText += kodeHeksadesimalSizeBesar + CenterText("SHIFT PRINT");
            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";
            strukText += FormatSimpleLine("Start Date", dataShifts.start_date) + "\n";
            strukText += FormatSimpleLine("End Date", dataShifts.end_date) + "\n";
            strukText += FormatSimpleLine("Casher Name", dataShifts.casher_name) + "\n";
            strukText += FormatSimpleLine("Shift Number", dataShifts.shift_number.ToString()) + "\n";
            strukText += "--------------------------------\n";

            strukText += kodeHeksadesimalBold + CenterText("ORDER DETAILS");
            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + "SOLD ITEMS" + "\n";
            strukText += kodeHeksadesimalNormal;


            var sortedCartDetailsSuccess = cartDetailsSuccess.OrderBy(x =>
            {
                if (x.menu_type.Contains("Minuman")) return 1;
                if (x.menu_type.Contains("Additional Minuman")) return 2;
                if (x.menu_type.Contains("Makanan")) return 3;
                if (x.menu_type.Contains("Additional Makanan")) return 4;
                return 5;
            }).ThenBy(x => x.menu_name);

            foreach (var cartDetail in sortedCartDetailsSuccess)
            {
                strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.total_price));
                if (!string.IsNullOrEmpty(cartDetail.varian))
                    strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
            }
            strukText += FormatSimpleLine("Item Sold Qty", dataShifts.totalSuccessQty.ToString()) + "\n";
            strukText += FormatSimpleLine("Item Sold Amount", string.Format("{0:n0}", dataShifts.totalCartSuccessAmount)) + "\n";

            if (cartDetailsPendings.Count != 0)
            {
                strukText += "--------------------------------\n";
                strukText += kodeHeksadesimalBold + "PENDING ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                var sortedCartDetailsPendings = cartDetailsPendings.OrderBy(x =>
                {
                    if (x.menu_type.Contains("Minuman")) return 1;
                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                    if (x.menu_type.Contains("Makanan")) return 3;
                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                    return 5;
                }).ThenBy(x => x.menu_name);

                foreach (var cartDetail in sortedCartDetailsPendings)
                {
                    strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.total_price));
                    if (!string.IsNullOrEmpty(cartDetail.varian))
                        strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                }
                strukText += FormatSimpleLine("Item Pending Qty", dataShifts.totalPendingQty.ToString()) + "\n";
                strukText += FormatSimpleLine("Item Pending Amount", string.Format("{0:n0}", dataShifts.totalCartPendingAmount)) + "\n";
            }

            if (cartDetailsCanceled.Count != 0)
            {
                strukText += "--------------------------------\n";
                strukText += kodeHeksadesimalBold + "CANCEL ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                var sortedCartDetailsCanceled = cartDetailsCanceled.OrderBy(x =>
                {
                    if (x.menu_type.Contains("Minuman")) return 1;
                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                    if (x.menu_type.Contains("Makanan")) return 3;
                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                    return 5;
                }).ThenBy(x => x.menu_name);

                foreach (var cartDetail in sortedCartDetailsCanceled)
                {
                    strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.total_price));
                    if (!string.IsNullOrEmpty(cartDetail.varian))
                        strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                }
                strukText += FormatSimpleLine("Item Cancel Qty", dataShifts.totalCanceledQty.ToString()) + "\n";
                strukText += FormatSimpleLine("Item Cancel Amount", string.Format("{0:n0}", dataShifts.totalCartCanceledAmount)) + "\n";
            }

            if (refundDetails.Count != 0)
            {
                strukText += "--------------------------------\n";
                strukText += kodeHeksadesimalBold + "REFUND ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                var sortedRefundDetails = refundDetails.OrderBy(x =>
                {
                    if (x.menu_type.Contains("Minuman")) return 1;
                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                    if (x.menu_type.Contains("Makanan")) return 3;
                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                    return 5;
                }).ThenBy(x => x.menu_name);

                foreach (var refundDetail in sortedRefundDetails)
                {
                    strukText += FormatSimpleLine(refundDetail.qty_refund_item + " " + refundDetail.menu_name, string.Format("{0:n0}", refundDetail.total_refund_price));
                    if (!string.IsNullOrEmpty(refundDetail.varian))
                        strukText += FormatDetailItemLine("Varian", refundDetail.varian) + "\n";
                }
                strukText += FormatSimpleLine("Item Refund Qty", dataShifts.totalRefundQty.ToString()) + "\n";
                strukText += FormatSimpleLine("Item Refund Amount", string.Format("{0:n0}", dataShifts.totalCartRefundAmount)) + "\n";
            }

            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText("CASH MANAGEMENT");

            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";

            if (expenditures.Count != 0)
            {
                strukText += kodeHeksadesimalBold + "EXPENSE\n";
                strukText += kodeHeksadesimalNormal;

                foreach (var expense in expenditures)
                {
                    strukText += FormatSimpleLine(expense.description, string.Format("{0:n0}", expense.nominal)) + "\n";
                }
            }

            strukText += FormatSimpleLine("Expected Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_expected)) + "\n";
            strukText += FormatSimpleLine("Actual Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_actual)) + "\n";
            strukText += FormatSimpleLine("Cash Difference", string.Format("{0:n0}", dataShifts.cash_difference)) + "\n";

            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + "DISCOUNTS\n";
            strukText += kodeHeksadesimalNormal;

            strukText += FormatSimpleLine("All Discount items", string.Format("{0:n0}", dataShifts.discount_amount_per_items)) + "\n";
            strukText += FormatSimpleLine("All Discount Cart", string.Format("{0:n0}", dataShifts.discount_amount_per_items)) + "\n";
            strukText += FormatSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", dataShifts.discount_total_amount)) + "\n";

            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText("PAYMENT DETAIL");
            strukText += kodeHeksadesimalNormal;

            strukText += "--------------------------------\n";

            foreach (var paymentDetail in paymentDetails)
            {
                strukText += paymentDetail.payment_category + "\n";
                foreach (var paymentType in paymentDetail.payment_type_detail)
                {
                    strukText += FormatSimpleLine(paymentType.payment_type, string.Format("{0:n0}", paymentType.total_payment)) + "\n";
                }
                strukText += FormatSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount)) + "\n";
                strukText += "--------------------------------\n";
            }

            strukText += kodeHeksadesimalBold + FormatSimpleLine("TOTAL TRANSACTION", string.Format("{0:n0}", dataShifts.total_transaction)) + "\n";
            strukText += kodeHeksadesimalNormal;

            strukText += "--------------------------------\n\n\n\n\n";

            return strukText;
        }

        // Fungsi untuk memformat baris dengan tiga kolom (Item, Kuantitas, Harga)
        private string FormatItemLine(string item, object quantity, object price)
        {
            int column1Width = 20; // Adjust as needed
            int column2Width = 6;  // Adjust as needed

            string quantityString = quantity.ToString() + "x ";
            string priceString = price.ToString();

            // Format the line with padding and alignment
            string formattedLine = "\x1B\x45\x01" + item.PadRight(column1Width) + "\x1B\x45\x00" +
                                   quantityString.PadLeft(column2Width) +
                                   priceString.PadLeft(priceString.Length);
            return formattedLine;
        }

        // Fungsi untuk memformat baris dengan dua kolom detail item
        private string FormatDetailItemLine(string column1, object column2)
        {
            // Gabungkan kolom menjadi satu string dengan format "column1: column2"
            string combinedColumns = column1 + ": " + column2.ToString();

            // Jika panjang kombinasi kolom lebih dari 32 karakter
            if (combinedColumns.Length > 32)
            {
                // Inisialisasi string untuk menyimpan hasil yang akan dikembalikan
                string formattedLine = "";

                // Hitung berapa karakter yang masih dapat dimasukkan ke baris ini
                int charactersToFitInLine = 32;

                // Indeks untuk memulai bagian berikutnya dari teks yang akan diproses
                int startIndex = 0;

                while (startIndex < combinedColumns.Length)
                {
                    // Bagian berikutnya dari teks yang akan diproses
                    string nextPart = combinedColumns.Substring(startIndex, Math.Min(charactersToFitInLine, combinedColumns.Length - startIndex));

                    // Tambahkan ke baris yang akan dikembalikan
                    formattedLine += nextPart;

                    // Periksa apakah masih ada lebih banyak teks yang harus diproses
                    if (startIndex + nextPart.Length < combinedColumns.Length)
                    {
                        // Tambahkan newline (\n) jika masih ada teks yang harus diproses
                        formattedLine += "\n";

                        // Sisakan karakter yang dapat dimasukkan ke baris berikutnya
                        charactersToFitInLine = 32;
                    }

                    // Perbarui indeks untuk memulai bagian berikutnya
                    startIndex += nextPart.Length;
                }

                return formattedLine;
            }
            else
            {
                // Jika panjang tidak melebihi 32 karakter, langsung lakukan padding
                int paddingSpaces = 32 - combinedColumns.Length;
                string formattedLine = "".PadLeft(paddingSpaces) + combinedColumns;
                return formattedLine;
            }
        }

        private string Ex_CenterText(string text)
        {
            if (text == null)
            {
                LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                return string.Empty; // or return new string(' ', 32); to keep the same line length
            }

            int maxLength = 32; // Maximum length of a line on the receipt
            StringBuilder centeredText = new StringBuilder();
            string[] words = text.Split(' ');

            StringBuilder currentLine = new StringBuilder();
            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 > maxLength)
                {
                    int spaces = (maxLength - currentLine.Length) / 2;
                    centeredText.AppendLine(new string(' ', spaces) + currentLine.ToString().TrimEnd());
                    currentLine.Clear();
                }
                currentLine.Append(word + " ");
            }

            if (currentLine.Length > 0)
            {
                int spaces = (maxLength - currentLine.Length) / 2;
                centeredText.AppendLine(new string(' ', spaces) + currentLine.ToString().TrimEnd());
            }

            return centeredText.ToString();
        }

        private string CenterText(string text)
        {
            if (text == null)
            {
                //LoggerUtil.LogError(new NullReferenceException(), "Text parameter is null");
                return string.Empty;
            }

            int maxLength = 32; // Maksimal panjang karakter dalam satu baris
            StringBuilder centeredText = new StringBuilder();
            string[] words = text.Split(' ');

            StringBuilder currentLine = new StringBuilder();

            foreach (var word in words)
            {
                // Jika satu kata lebih panjang dari maxLength, maka perlu dipotong
                if (word.Length > maxLength)
                {
                    SplitLongWord(word, maxLength, centeredText, currentLine);
                }
                else
                {
                    if (currentLine.Length + word.Length + 1 > maxLength)
                    {
                        AppendLineWithPadding(centeredText, currentLine, maxLength);
                        currentLine.Clear();
                    }
                    currentLine.Append(word + " ");
                }
            }

            // Tambahkan sisa baris yang belum diproses
            if (currentLine.Length > 0)
            {
                AppendLineWithPadding(centeredText, currentLine, maxLength);
            }

            return centeredText.ToString();
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


        private string FormatSimpleLine(string left, object right)
        {
            // Jika objek right null, maka atur rightString sebagai string kosong
            string rightString = right != null ? right.ToString() : string.Empty;

            // Hitung panjang teks yang seharusnya ditambahkan ke kiri
            int paddingLength = Math.Max(0, 32 - rightString.Length);

            // Format baris dengan padding dan alignment yang benar
            string formattedLine = left.PadRight(paddingLength) + rightString;

            return formattedLine;
        }
        /*
                private string FormatSimpleLine(string left, string right)
                {
                    if (left == null)
                    {
                        left = string.Empty;
                    }
                    if (right == null)
                    {
                        right = string.Empty;
                    }

                    int maxLength = 32; // Maximum length of a line on the receipt
                    StringBuilder formattedText = new StringBuilder();
                    string[] leftWords = left.Split(' ');

                    StringBuilder currentLine = new StringBuilder();
                    int currentLineLength = 0;

                    // Add left words to the line, ensuring no word is cut off
                    foreach (var word in leftWords)
                    {
                        if (currentLineLength + word.Length + 1 > maxLength - right.Length - 1)
                        {
                            formattedText.Append(currentLine.ToString().TrimEnd() + "\n");
                            currentLine.Clear();
                            currentLineLength = 0;
                        }
                        currentLine.Append(word + " ");
                        currentLineLength += word.Length + 1;
                    }

                    // Ensure the current line is added if there's remaining text
                    if (currentLine.Length > 0)
                    {
                        // Calculate remaining spaces after left text
                        int spaces = maxLength - currentLine.Length - right.Length;
                        spaces = spaces < 0 ? 0 : spaces;

                        formattedText.Append(currentLine.ToString().TrimEnd() + new string(' ', spaces) + right);
                    }
                    else
                    {
                        // If current line is empty, add the right text directly
                        formattedText.Append(new string(' ', maxLength - right.Length) + right);
                    }

                    return formattedText.ToString();
                }
        */


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
            List<PaymentDetailStrukShift> paymentDetails)
        {
            try
            {

                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");


                    // Struck Checker
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics;

                            // Mengatur font normal dan tebal
                            Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null) text = string.Empty;
                                if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
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
                            var sortedcartDetailSuccess = cartDetailsSuccess.OrderBy(x =>
                            {
                                if (x.menu_type.Contains("Minuman")) return 1;
                                if (x.menu_type.Contains("Additional Minuman")) return 2;
                                if (x.menu_type.Contains("Makanan")) return 3;
                                if (x.menu_type.Contains("Additional Makanan")) return 4;
                                return 5;
                            })
                            .ThenBy(x => x.menu_name);
                            //foreach (var cartDetail in cartDetailsSuccess)
                            foreach (var cartDetail in sortedcartDetailSuccess)
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
                                var sortedcartDetailPendings = cartDetailsPendings.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);
                                //foreach (var cartDetail in cartDetailsPendings)
                                foreach (var cartDetail in sortedcartDetailPendings)
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

                                    DrawSimpleLine(cartDetail.qty.ToString(), string.Format("{0:n0}", cartDetail.total_price));
                                }
                                DrawSimpleLine("Item Pending Qty", dataShifts.totalPendingQty.ToString());
                                DrawSimpleLine("Item Pending Amount", string.Format("{0:n0}", dataShifts.totalCartPendingAmount));
                            }
                            if (cartDetailsCanceled.Count != 0)
                            {
                                DrawSeparator();
                                DrawCenterText("CANCEL ITEMS", boldFont);
                                var sortedcartDetailCanceled = cartDetailsCanceled.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);
                                //foreach (var cartDetail in cartDetailsCanceled)
                                foreach (var cartDetail in sortedcartDetailCanceled)
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
                                DrawSimpleLine("Item Cancel Qty", dataShifts.totalCanceledQty.ToString());
                                DrawSimpleLine("Item Cancel Amount", string.Format("{0:n0}", dataShifts.totalCartCanceledAmount));
                            }
                            if (refundDetails.Count != 0)
                            {
                                DrawSeparator();
                                DrawCenterText("REFUND ITEMS", boldFont);
                                var sortedrefundDetails = refundDetails.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);
                                //foreach (var cartDetail in refundDetails)
                                foreach (var cartDetail in sortedrefundDetails)
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
                                    DrawSimpleLine(cartDetail.qty_refund_item.ToString(), string.Format("{0:n0}", cartDetail.total_refund_price));
                                }
                                DrawSimpleLine("Item Refund Qty", dataShifts.totalRefundQty.ToString());
                                DrawSimpleLine("Item Refund Amount", string.Format("{0:n0}", dataShifts.totalCartRefundAmount));
                            }
                            DrawSeparator();
                            DrawCenterText("CASH MANAGEMENT", BigboldFont);
                            DrawSeparator();
                            if (expenditures.Count != 0)
                            {
                                DrawLeftText("EXPENSE", boldFont);
                                foreach (var expense in expenditures)
                                {
                                    DrawSimpleLine(expense.description, string.Format("{0:n0}", expense.nominal));
                                }
                            }
                            DrawSimpleLine("Expected Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_expected));
                            DrawSimpleLine("Actual Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_actual));
                            DrawSimpleLine("Cash Difference", string.Format("{0:n0}", dataShifts.cash_difference));
                            DrawSeparator();
                            DrawLeftText("DISCOUNTS", boldFont);
                            DrawSimpleLine("All Discount items", string.Format("{0:n0}", dataShifts.discount_amount_per_items));
                            DrawSimpleLine("All Discount Cart", string.Format("{0:n0}", dataShifts.discount_amount_per_items));
                            DrawSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", dataShifts.discount_total_amount));
                            DrawSeparator();
                            DrawCenterText("PAYMENT DETAIL", BigboldFont);
                            DrawSeparator();
                            foreach (var paymentDetail in paymentDetails)
                            {
                                DrawLeftText(paymentDetail.payment_category, boldFont);
                                foreach (var paymentType in paymentDetail.payment_type_detail)
                                {
                                    DrawSimpleLine(paymentType.payment_type, string.Format("{0:n0}", paymentType.total_payment));
                                }
                                DrawSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount));
                                DrawSeparator();
                            }
                            DrawSimpleLine("TOTAL TRANSACTION", string.Format("{0:n0}", dataShifts.total_transaction));
                            DrawSeparator();
                        };
                        /*
                        if (IsBluetoothPrinter(printerName))
                        {
                            printerName = ConvertMacAddressFormat(printerName);
                            PrintViaBluetooth(printerName, printDocument);
                        }
                        else
                        {
                            printDocument.PrinterSettings.PrinterName = printerName;
                            printDocument.Print();
                        }// Langsung melakukan pencetakan
                    */
                    }
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
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintModelCetakUlangShift(dataShifts, expenditures, cartDetailsSuccess, cartDetailsPendings, cartDetailsCanceled, refundDetails, paymentDetails);
                        continue;
                    }
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        System.IO.Stream stream = null;

                        try
                        {
                            if (System.Net.IPAddress.TryParse(printerName, out _))
                            {
                                // Connect via LAN
                                var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                stream = client.GetStream();
                            }
                            else
                            {
                                // Connect via Bluetooth
                                BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                if (printerDevice == null)
                                {
                                    //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                    continue;
                                }

                                BluetoothClient client = new BluetoothClient();
                                BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                {
                                    //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                    continue;
                                }

                                client.Connect(endpoint);
                                stream = client.GetStream();
                            }

                            string strukText = GenerateStrukText(dataShifts, expenditures, cartDetailsSuccess, cartDetailsPendings, cartDetailsCanceled, refundDetails, paymentDetails);
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);
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
            List<PaymentDetailStrukShift> paymentDetails)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            string strukText = "\n" + kodeHeksadesimalBold + CenterText(dataShifts.outlet_name);
            strukText += kodeHeksadesimalNormal;
            strukText += CenterText(dataShifts.outlet_address);
            strukText += CenterText(dataShifts.outlet_phone_number);

            strukText += "--------------------------------\n";

            strukText += kodeHeksadesimalSizeBesar + CenterText("SHIFT PRINT");
            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";
            strukText += FormatSimpleLine("Start Date", dataShifts.start_date) + "\n";
            strukText += FormatSimpleLine("End Date", dataShifts.end_date) + "\n";
            strukText += FormatSimpleLine("Casher Name", dataShifts.casher_name) + "\n";
            strukText += FormatSimpleLine("Shift Number", dataShifts.shift_number.ToString()) + "\n";
            strukText += "--------------------------------\n";

            strukText += kodeHeksadesimalBold + CenterText("ORDER DETAILS");
            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + "SOLD ITEMS" + "\n";
            strukText += kodeHeksadesimalNormal;


            var sortedCartDetailsSuccess = cartDetailsSuccess.OrderBy(x =>
            {
                if (x.menu_type.Contains("Minuman")) return 1;
                if (x.menu_type.Contains("Additional Minuman")) return 2;
                if (x.menu_type.Contains("Makanan")) return 3;
                if (x.menu_type.Contains("Additional Makanan")) return 4;
                return 5;
            }).ThenBy(x => x.menu_name);

            foreach (var cartDetail in sortedCartDetailsSuccess)
            {
                strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.total_price));
                if (!string.IsNullOrEmpty(cartDetail.varian))
                    strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
            }
            strukText += FormatSimpleLine("Item Sold Qty", dataShifts.totalSuccessQty.ToString()) + "\n";
            strukText += FormatSimpleLine("Item Sold Amount", string.Format("{0:n0}", dataShifts.totalCartSuccessAmount)) + "\n";

            if (cartDetailsPendings.Count != 0)
            {
                strukText += "--------------------------------\n";
                strukText += kodeHeksadesimalBold + "PENDING ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                var sortedCartDetailsPendings = cartDetailsPendings.OrderBy(x =>
                {
                    if (x.menu_type.Contains("Minuman")) return 1;
                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                    if (x.menu_type.Contains("Makanan")) return 3;
                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                    return 5;
                }).ThenBy(x => x.menu_name);

                foreach (var cartDetail in sortedCartDetailsPendings)
                {
                    strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.total_price));
                    if (!string.IsNullOrEmpty(cartDetail.varian))
                        strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                }
                strukText += FormatSimpleLine("Item Pending Qty", dataShifts.totalPendingQty.ToString()) + "\n";
                strukText += FormatSimpleLine("Item Pending Amount", string.Format("{0:n0}", dataShifts.totalCartPendingAmount)) + "\n";
            }

            if (cartDetailsCanceled.Count != 0)
            {
                strukText += "--------------------------------\n";
                strukText += kodeHeksadesimalBold + "CANCEL ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                var sortedCartDetailsCanceled = cartDetailsCanceled.OrderBy(x =>
                {
                    if (x.menu_type.Contains("Minuman")) return 1;
                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                    if (x.menu_type.Contains("Makanan")) return 3;
                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                    return 5;
                }).ThenBy(x => x.menu_name);

                foreach (var cartDetail in sortedCartDetailsCanceled)
                {
                    strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.total_price));
                    if (!string.IsNullOrEmpty(cartDetail.varian))
                        strukText += FormatDetailItemLine("Varian", cartDetail.varian) + "\n";
                }
                strukText += FormatSimpleLine("Item Cancel Qty", dataShifts.totalCanceledQty.ToString()) + "\n";
                strukText += FormatSimpleLine("Item Cancel Amount", string.Format("{0:n0}", dataShifts.totalCartCanceledAmount)) + "\n";
            }

            if (refundDetails.Count != 0)
            {
                strukText += "--------------------------------\n";
                strukText += kodeHeksadesimalBold + "REFUND ITEMS" + "\n";

                strukText += kodeHeksadesimalNormal;
                var sortedRefundDetails = refundDetails.OrderBy(x =>
                {
                    if (x.menu_type.Contains("Minuman")) return 1;
                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                    if (x.menu_type.Contains("Makanan")) return 3;
                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                    return 5;
                }).ThenBy(x => x.menu_name);

                foreach (var refundDetail in sortedRefundDetails)
                {
                    strukText += FormatSimpleLine(refundDetail.qty_refund_item + " " + refundDetail.menu_name, string.Format("{0:n0}", refundDetail.total_refund_price));
                    if (!string.IsNullOrEmpty(refundDetail.varian))
                        strukText += FormatDetailItemLine("Varian", refundDetail.varian) + "\n";
                }
                strukText += FormatSimpleLine("Item Refund Qty", dataShifts.totalRefundQty.ToString()) + "\n";
                strukText += FormatSimpleLine("Item Refund Amount", string.Format("{0:n0}", dataShifts.totalCartRefundAmount)) + "\n";
            }

            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText("CASH MANAGEMENT");

            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";

            if (expenditures.Count != 0)
            {
                strukText += kodeHeksadesimalBold + "EXPENSE\n";
                strukText += kodeHeksadesimalNormal;

                foreach (var expense in expenditures)
                {
                    strukText += FormatSimpleLine(expense.description, string.Format("{0:n0}", expense.nominal)) + "\n";
                }
            }

            strukText += FormatSimpleLine("Expected Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_expected)) + "\n";
            strukText += FormatSimpleLine("Actual Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_actual)) + "\n";
            strukText += FormatSimpleLine("Cash Difference", string.Format("{0:n0}", dataShifts.cash_difference)) + "\n";

            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + "DISCOUNTS\n";
            strukText += kodeHeksadesimalNormal;

            strukText += FormatSimpleLine("All Discount items", string.Format("{0:n0}", dataShifts.discount_amount_per_items)) + "\n";
            strukText += FormatSimpleLine("All Discount Cart", string.Format("{0:n0}", dataShifts.discount_amount_per_items)) + "\n";
            strukText += FormatSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", dataShifts.discount_total_amount)) + "\n";

            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText("PAYMENT DETAIL");
            strukText += kodeHeksadesimalNormal;

            strukText += "--------------------------------\n";

            foreach (var paymentDetail in paymentDetails)
            {
                strukText += paymentDetail.payment_category + "\n";
                foreach (var paymentType in paymentDetail.payment_type_detail)
                {
                    strukText += FormatSimpleLine(paymentType.payment_type, string.Format("{0:n0}", paymentType.total_payment)) + "\n";
                }
                strukText += FormatSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount)) + "\n";
                strukText += "--------------------------------\n";
            }

            strukText += kodeHeksadesimalBold + FormatSimpleLine("TOTAL TRANSACTION", string.Format("{0:n0}", dataShifts.total_transaction)) + "\n";
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
            List<PaymentDetailStrukShift> paymentDetails)
        {
            try
            {

                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");


                    // Struck Checker
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics;

                            // Mengatur font normal dan tebal
                            Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null) text = string.Empty;
                                if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
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
                            var sortedcartDetailSuccess = cartDetailsSuccess.OrderBy(x =>
                            {
                                if (x.menu_type.Contains("Minuman")) return 1;
                                if (x.menu_type.Contains("Additional Minuman")) return 2;
                                if (x.menu_type.Contains("Makanan")) return 3;
                                if (x.menu_type.Contains("Additional Makanan")) return 4;
                                return 5;
                            })
                            .ThenBy(x => x.menu_name);
                            //foreach (var cartDetail in cartDetailsSuccess)
                            foreach (var cartDetail in sortedcartDetailSuccess)
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
                                var sortedcartDetailPendings = cartDetailsPendings.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);
                                //foreach (var cartDetail in cartDetailsPendings)
                                foreach (var cartDetail in sortedcartDetailPendings)
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

                                    DrawSimpleLine(cartDetail.qty.ToString(), string.Format("{0:n0}", cartDetail.total_price));
                                }
                                DrawSimpleLine("Item Pending Qty", dataShifts.totalPendingQty.ToString());
                                DrawSimpleLine("Item Pending Amount", string.Format("{0:n0}", dataShifts.totalCartPendingAmount));
                            }
                            if (cartDetailsCanceled.Count != 0)
                            {
                                DrawSeparator();
                                DrawCenterText("CANCEL ITEMS", boldFont);
                                var sortedcartDetailCanceled = cartDetailsCanceled.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);
                                //foreach (var cartDetail in cartDetailsCanceled)
                                foreach (var cartDetail in sortedcartDetailCanceled)
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
                                DrawSimpleLine("Item Cancel Qty", dataShifts.totalCanceledQty.ToString());
                                DrawSimpleLine("Item Cancel Amount", string.Format("{0:n0}", dataShifts.totalCartCanceledAmount));
                            }
                            if (refundDetails.Count != 0)
                            {
                                DrawSeparator();
                                DrawCenterText("REFUND ITEMS", boldFont);
                                var sortedrefundDetails = refundDetails.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);
                                //foreach (var cartDetail in refundDetails)
                                foreach (var cartDetail in sortedrefundDetails)
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
                                    DrawSimpleLine(cartDetail.qty_refund_item.ToString(), string.Format("{0:n0}", cartDetail.total_refund_price));
                                }
                                DrawSimpleLine("Item Refund Qty", dataShifts.totalRefundQty.ToString());
                                DrawSimpleLine("Item Refund Amount", string.Format("{0:n0}", dataShifts.totalCartRefundAmount));
                            }
                            DrawSeparator();
                            DrawCenterText("CASH MANAGEMENT", BigboldFont);
                            DrawSeparator();
                            if (expenditures.Count != 0)
                            {
                                DrawLeftText("EXPENSE", boldFont);
                                foreach (var expense in expenditures)
                                {
                                    DrawSimpleLine(expense.description, string.Format("{0:n0}", expense.nominal));
                                }
                            }
                            DrawSimpleLine("Expected Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_expected));
                            DrawSimpleLine("Actual Ending Cash", string.Format("{0:n0}", dataShifts.ending_cash_actual));
                            DrawSimpleLine("Cash Difference", string.Format("{0:n0}", dataShifts.cash_difference));
                            DrawSeparator();
                            DrawLeftText("DISCOUNTS", boldFont);
                            DrawSimpleLine("All Discount items", string.Format("{0:n0}", dataShifts.discount_amount_per_items));
                            DrawSimpleLine("All Discount Cart", string.Format("{0:n0}", dataShifts.discount_amount_per_items));
                            DrawSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", dataShifts.discount_total_amount));
                            DrawSeparator();
                            DrawCenterText("PAYMENT DETAIL", BigboldFont);
                            DrawSeparator();
                            foreach (var paymentDetail in paymentDetails)
                            {
                                DrawLeftText(paymentDetail.payment_category, boldFont);
                                foreach (var paymentType in paymentDetail.payment_type_detail)
                                {
                                    DrawSimpleLine(paymentType.payment_type, string.Format("{0:n0}", paymentType.total_payment));
                                }
                                DrawSimpleLine("TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount));
                                DrawSeparator();
                            }
                            DrawSimpleLine("TOTAL TRANSACTION", string.Format("{0:n0}", dataShifts.total_transaction));
                            DrawSeparator();
                        };
                        /*
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
                        */
                    }
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
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings.ToList())
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintModelRefund(datas, refundDetailStruks, totalTransactions);
                        continue;
                    }
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        System.IO.Stream stream = null;

                        try
                        {
                            if (System.Net.IPAddress.TryParse(printerName, out _))
                            {
                                // Connect via LAN
                                var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                stream = client.GetStream();
                            }
                            else
                            {
                                // Connect via Bluetooth
                                BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                if (printerDevice == null)
                                {
                                    //.Show("Printer " + printerName + " not found", "Error");
                                    continue;
                                }

                                BluetoothClient client = new BluetoothClient();
                                BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                {
                                    //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                    continue;
                                }

                                client.Connect(endpoint);
                                stream = client.GetStream();
                            }

                            PrintRefundReceipt(stream, datas, refundDetailStruks, totalTransactions);
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

        private void PrintRefundReceipt(System.IO.Stream stream, DataRefundStruk datas,
            List<RefundDetailStruk> refundDetailStruks, int totalTransactions)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            byte[] InitPrinter = new byte[] { 0x1B, 0x40 }; // Initialize printer
            byte[] NewLine = new byte[] { 0x0A }; // New line

            // Write initialization bytes
            stream.Write(InitPrinter, 0, InitPrinter.Length);

            // Print logo (assuming logo is already in a proper format for the printer)

            // Print the rest of the receipt
            //string strukText = "\n" + kodeHeksadesimalBold + CenterText("No. " + totalTransactions.ToString()) + "\n";
            string strukText = kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText("REFUND");
            strukText += CenterText("Receipt No. " + datas?.receipt_number);
            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";
            if (datas?.invoice_due_date != null)
                strukText += CenterText(datas?.invoice_due_date);
            strukText += "Name: " + datas?.customer_name + "\n";

            var servingTypes = refundDetailStruks.Select(cd => cd.serving_type_name).Distinct();

            foreach (var servingType in servingTypes)
            {
                var itemsForServingType = refundDetailStruks.Where(cd => cd.serving_type_name == servingType).ToList();
                if (itemsForServingType.Count == 0)
                    continue;

                strukText += "--------------------------------\n";
                strukText += CenterText(servingType);
                strukText += "\n";

                foreach (var refundDetail in itemsForServingType)
                {
                    strukText += FormatSimpleLine(refundDetail.qty_refund_item + " " + refundDetail.menu_name, string.Format("{0:n0}", refundDetail.menu_price));
                    if (!string.IsNullOrEmpty(refundDetail.varian))
                        strukText += "   Varian: " + refundDetail.varian + "\n";
                    if (!string.IsNullOrEmpty(refundDetail.note_item?.ToString()))
                        strukText += "   Note: " + refundDetail.note_item + "\n";
                    if (!string.IsNullOrEmpty(refundDetail.discount_code))
                        strukText += "   Discount Code: " + refundDetail.discount_code + "\n";
                    if (refundDetail.discounted_price.HasValue && refundDetail.discounted_price != 0)
                        strukText += "   Total Discount: " + (refundDetail.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", refundDetail.discounts_value) : refundDetail.discounts_value + " %") + "\n";
                    strukText += "   Payment Type Refund: " + refundDetail.payment_type_name + "\n";
                    strukText += "   Total Refund: " + string.Format("{0:n0}", refundDetail.total_refund_price) + "\n";
                    strukText += "   Refund Reason: " + refundDetail.refund_reason_item + "\n";
                    strukText += "\n";
                }
            }

            strukText += "--------------------------------\n";
            strukText += FormatSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal)) + "\n";
            if (!string.IsNullOrEmpty(datas.discount_code))
                strukText += "Discount Code: " + datas.discount_code + "\n";
            if (datas.discounts_value.HasValue && datas.discounts_value != 0)
                strukText += FormatSimpleLine("Discount Value", (datas.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", datas.discounts_value) : datas.discounts_value + " %")) + "\n";

            strukText += FormatSimpleLine("Total", string.Format("{0:n0}", datas.total)) + "\n";
            strukText += "Payment Type: " + datas.payment_type + "\n";
            if (!string.IsNullOrEmpty(datas.refund_reason))
                strukText += "Refund Reason: " + datas.refund_reason + "\n";
            strukText += FormatSimpleLine("Total Refund", string.Format("{0:n0}", datas.total_refund)) + "\n";
            strukText += "--------------------------------\n";
            strukText += CenterText("Meja No. " + datas.customer_seat.ToString());
            strukText += "--------------------------------\n";
            strukText += CenterText("Powered By");

            string NomorUrut = "\n" + kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText("No. " + totalTransactions.ToString()) + "\n\n\n    ";

            byte[] buffer1 = System.Text.Encoding.UTF8.GetBytes(NomorUrut);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

            stream.Write(buffer1, 0, buffer1.Length);
            PrintLogo(stream, "icon\\OutletLogo.bmp", 250); // Smaller logo size
            stream.Write(buffer, 0, buffer.Length);
            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size
            stream.Write(NewLine, 0, NewLine.Length);

            stream.Flush();
        }

        public async Task Ex_PrintModelRefund
          (DataRefundStruk datas,
            List<RefundDetailStruk> refundDetailStruks,
           int totalTransactions)
        {
            try
            {

                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");


                    // Struck Checker
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics;

                            // Mengatur font normal dan tebal
                            Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null) text = string.Empty;
                                if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                            }



                            // Fungsi untuk mendapatkan dan mengonversi gambar logo ke hitam dan putih
                            Image GetLogoImage(string path)
                            {
                                Image img = Image.FromFile(path);
                                Bitmap bmp = new Bitmap(img.Width, img.Height);
                                using (Graphics g = Graphics.FromImage(bmp))
                                {
                                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                    {
                                        new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                                        new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                                        new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                                        new float[] { 0, 0, 0, 1, 0 },
                                        new float[] { 0, 0, 0, 0, 1 }
                                    });
                                    ImageAttributes attributes = new ImageAttributes();
                                    attributes.SetColorMatrix(colorMatrix);
                                    g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attributes);
                                }
                                return bmp;
                            }

                            // Menambahkan logo dan teks "Powered by" di akhir struk
                            void DrawPoweredByLogo(string path)
                            {
                                // Menambahkan jarak sebelum mencetak teks dan logo
                                float spaceBefore = 50; // Jarak dalam pixel, sesuaikan dengan kebutuhan
                                yPos += spaceBefore;

                                // Mengukur teks "Powered by Your Company"
                                string poweredByText = "Powered by Dastrevas";
                                SizeF textSize = e.Graphics.MeasureString(poweredByText, normalFont);

                                // Gambar teks
                                float textX = leftMargin + (printableWidth - textSize.Width) / 2;
                                e.Graphics.DrawString(poweredByText, normalFont, Brushes.Black, textX, yPos);

                                // Sesuaikan yPos untuk logo
                                yPos += textSize.Height;

                                // Menggambar logo
                                Image logoPoweredBy = GetLogoImage(path);
                                float targetWidth = 35; // Ukuran lebar logo dalam pixel, sesuaikan dengan kebutuhan
                                float scaleFactor = targetWidth / logoPoweredBy.Width;
                                float logoHeight = logoPoweredBy.Height * scaleFactor;

                                float logoX = leftMargin + (printableWidth - targetWidth) / 2;
                                e.Graphics.DrawImage(logoPoweredBy, logoX, yPos, targetWidth, logoHeight);

                                spaceBefore = 5;
                                yPos += spaceBefore;
                                // Sesuaikan yPos untuk elemen berikutnya
                                yPos += logoHeight;
                            }


                            DrawCenterText("No. " + totalTransactions.ToString(), NomorAntrian);

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
                            float logoX = leftMargin + (printableWidth - logoTargetWidthPx) / 2;

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
                                DrawCenterText(datas?.invoice_due_date, normalFont);
                            DrawSpace();
                            DrawLeftText(datas?.customer_name, normalFont);

                            // Iterate through cart details and group by serving_type_name
                            var servingTypes = refundDetailStruks.Select(cd => cd.serving_type_name).Distinct();

                            foreach (var servingType in servingTypes)
                            {
                                // Filter cart details by serving_type_name
                                var itemsForServingType = refundDetailStruks.Where(cd => cd.serving_type_name == servingType).ToList();

                                // Skip if there are no items for this serving type
                                if (itemsForServingType.Count == 0)
                                    continue;

                                // Add a section for the serving type
                                DrawSeparator();
                                DrawCenterText(servingType, normalFont);
                                DrawSpace();

                                // Iterate through items for this serving type
                                foreach (var refundDetail in itemsForServingType)
                                {
                                    DrawSimpleLine(refundDetail.qty_refund_item + " " + refundDetail.menu_name, string.Format("{0:n0}", refundDetail.menu_price));
                                    // Add detail items
                                    if (!string.IsNullOrEmpty(refundDetail.varian))
                                        DrawLeftText("   Varian: " + refundDetail.varian, normalFont);
                                    if (!string.IsNullOrEmpty(refundDetail.note_item?.ToString()))
                                        DrawLeftText("   Note: " + refundDetail.note_item, normalFont);
                                    if (!string.IsNullOrEmpty(refundDetail.discount_code))
                                        DrawLeftText("   Discount Code: " + refundDetail.discount_code, normalFont);
                                    if (refundDetail.discounted_price.HasValue && refundDetail.discounted_price != 0)
                                        DrawSimpleLine("   Total Discount", (refundDetail.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", refundDetail.discounts_value) : refundDetail.discounts_value + " %"));
                                    DrawLeftText("   Payment Type Refund: " + refundDetail.payment_type_name, normalFont);
                                    DrawSimpleLine("   Total Refund", string.Format("{0:n0}", refundDetail.total_refund_price));
                                    DrawLeftText("   Refund Reason: " + refundDetail.refund_reason_item, normalFont);

                                    // Add an empty line between items
                                    DrawSpace();
                                }
                            }
                            DrawSeparator();
                            DrawSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal));
                            if (!string.IsNullOrEmpty(datas.discount_code))
                                DrawLeftText("Discount Code: " + datas.discount_code, normalFont);
                            if (datas.discounts_value.HasValue && datas.discounts_value != 0)
                                DrawSimpleLine("Discount Value", (datas.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", datas.discounts_value) : datas.discounts_value + " %"));

                            DrawSimpleLine("Total", string.Format("{0:n0}", datas.total));
                            DrawLeftText("Payment Type: " + datas.payment_type, normalFont);
                            if (!string.IsNullOrEmpty(datas.refund_reason))
                                DrawLeftText("Refund Reason: " + datas.refund_reason, normalFont);
                            DrawSimpleLine("Total Refund ", string.Format("{0:n0}", datas.total_refund));
                            DrawSeparator();
                            DrawPoweredByLogo(poweredByLogoPath);
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);

                        };
                        /*
                        if (IsBluetoothPrinter(printerName))
                        {
                            PrintViaBluetooth(printerName, printDocument);
                        }
                        else
                        {
                            printDocument.PrinterSettings.PrinterName = printerName;
                            printDocument.Print();
                        }
                        */
                    }
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
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings
                
                List<KeyValuePair<string, string>> printerSettingsCopy;
                lock (printerSettings)
                {
                    printerSettingsCopy = printerSettings.ToList(); // Create a copy of the collection
                }

                foreach (var printer in printerSettingsCopy)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {

                        Ex_PrintModelInputPin(datas, cartDetails, cartRefundDetails, canceledItems, totalTransactions);
                        continue;
                    }
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        System.IO.Stream stream = null;

                        try
                        {
                            if (System.Net.IPAddress.TryParse(printerName, out _))
                            {

                                // Connect via LAN
                                var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                stream = client.GetStream();
                            }
                            else
                            {

                                // Connect via Bluetooth
                                BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                if (printerDevice == null)
                                {
                                    //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                    continue;
                                }

                                BluetoothClient client = new BluetoothClient();
                                BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                {
                                    //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                    continue;
                                }

                                client.Connect(endpoint);
                                stream = client.GetStream();
                            }

                            PrintInputPinReceipt(stream, datas, cartDetails, cartRefundDetails, canceledItems, totalTransactions);
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

        private void PrintInputPinReceipt(System.IO.Stream stream, DataRestruk datas,
            List<CartDetailRestruk> cartDetails,
            List<RefundDetailRestruk> cartRefundDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int totalTransactions)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            byte[] InitPrinter = new byte[] { 0x1B, 0x40 }; // Initialize printer
            byte[] NewLine = new byte[] { 0x0A }; // New line
/*
            // Set Line Spacing (perintah ESC/POS untuk line spacing, misalnya 30 dots)
            byte[] setLineSpacing = new byte[] { 0x1B, 0x33, 1 }; // Ganti 30 sesuai kebutuhan
            stream.Write(setLineSpacing, 0, setLineSpacing.Length); // Mengirim perintah untuk line spacing
*/
            // Write initialization bytes
            stream.Write(InitPrinter, 0, InitPrinter.Length);

            // Print logo (assuming logo is already in a proper format for the printer)

            // Print the rest of the receipt

            //string strukText = "\n" + kodeHeksadesimalBold + CenterText("No. " + totalTransactions.ToString()) + "\n";
            string strukText = kodeHeksadesimalNormal;
            //strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText("RE-PRINT STRUCT");
            strukText += kodeHeksadesimalNormal;

            strukText += CenterText("Receipt No. " + datas?.receipt_number);
            //strukText += "--------------------------------\n";

            if (datas?.invoice_due_date != null)
                strukText += CenterText(datas?.invoice_due_date);
            strukText += "Name: " + datas?.customer_name + "\n";

            if (cartDetails.Count != 0)
            {
                var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                    strukText += "--------------------------------\n";
                    strukText += CenterText("ORDERED");

                    if (itemsForServingType.Count == 0)
                        continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingType) + "\n";
                    //strukText += "\n";

                    foreach (var cartDetail in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalBold + $"{cartDetail.menu_name}" + "\n";
                        strukText += kodeHeksadesimalNormal;
                        strukText += FormatSimpleLine("@" + string.Format("{0:n0}", cartDetail.price) + " x" + cartDetail.qty, string.Format("{0:n0}", cartDetail.total_price)) + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "  Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "  Note: " + cartDetail.note_item + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.discount_code))
                            strukText += "  Discount Code: " + cartDetail.discount_code + "\n";
                        if (cartDetail.discounts_value != null && Convert.ToDecimal(cartDetail.discounts_value) != 0)
                            strukText += "  Discount Value: " + (cartDetail.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", cartDetail.discounts_value.ToString()) : cartDetail.discounts_value.ToString() + " %") + "\n";
/*
                        strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.price)) + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "   Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "   Note: " + cartDetail.note_item + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.discount_code))
                            strukText += "   Discount Code: " + cartDetail.discount_code + "\n";
                        if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                            strukText += "   Total Discount: " + string.Format("{0:n0}", cartDetail.discounted_price) + "\n";
                        strukText += "   Total Price: " + string.Format("{0:n0}", cartDetail.total_price) + "\n";*/
                        //strukText += "\n";
                    }
                }
            }

            if (canceledItems.Count != 0)
            {
                var servingTypesCancel = canceledItems.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("CANCELED");

                foreach (var servingType in servingTypesCancel)
                {
                    var itemsForServingType = canceledItems.Where(cd => cd.serving_type_name == servingType).ToList();

                    if (itemsForServingType.Count == 0)
                        continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    //strukText += "\n";

                    foreach (var cancelItem in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalBold + $"{cancelItem.menu_name}" + "\n";
                        strukText += kodeHeksadesimalNormal;
                        strukText += FormatSimpleLine("@" + string.Format("{0:n0}", cancelItem.price) + " x" + cancelItem.qty, string.Format("{0:n0}", cancelItem.total_price)) + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.varian))
                            strukText += "  Varian: " + cancelItem.varian + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                            strukText += "  Note: " + cancelItem.note_item + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.discount_code))
                            strukText += "  Discount Code: " + cancelItem.discount_code + "\n";
                        if (cancelItem.discounts_value != null && Convert.ToDecimal(cancelItem.discounts_value) != 0)
                            strukText += "  Discount Value: " + (cancelItem.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", cancelItem.discounts_value.ToString()) : cancelItem.discounts_value.ToString() + " %") + "\n";

                        /*strukText += FormatSimpleLine(cancelItem.qty + " " + cancelItem.menu_name, string.Format("{0:n0}", cancelItem.price)) + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.varian))
                            strukText += "   Varian: " + cancelItem.varian + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                            strukText += "   Note: " + cancelItem.note_item + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.discount_code))
                            strukText += "   Discount Code: " + cancelItem.discount_code + "\n";
                        if (cancelItem.discounted_price.HasValue && cancelItem.discounted_price != 0)
                            strukText += "   Total Discount: " + string.Format("{0:n0}", cancelItem.discounted_price) + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                            strukText += "   Reason: " + cancelItem.cancel_reason + "\n";*/
                        //strukText += "\n";
                    }
                }
            }

            if (cartRefundDetails.Count != 0)
            {
                strukText += "--------------------------------\n";
                strukText += CenterText("REFUNDED");
                var servingTypesRefund = cartRefundDetails.Select(cd => cd.serving_type_name).Distinct();
                foreach (var servingTypeRefund in servingTypesRefund)
                {
                    var itemsForServingType = cartRefundDetails.Where(cd => cd.serving_type_name == servingTypeRefund).ToList();

                    if (itemsForServingType.Count == 0)
                        continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingTypeRefund);
                    //strukText += "\n";

                    foreach (var cartDetail in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalBold + $"{cartDetail.menu_name}" + "\n";
                        strukText += kodeHeksadesimalNormal;
                        strukText += FormatSimpleLine("@" + string.Format("{0:n0}", cartDetail.menu_price) + " x" + cartDetail.qty_refund_item, string.Format("{0:n0}", cartDetail.total_refund_price)) + "\n";
                        //strukText += FormatSimpleLine(cartDetail.qty_refund_item + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.total_refund_price)) + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "   Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "   Note: " + cartDetail.note_item + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.discount_code))
                            strukText += "   Discount Code: " + cartDetail.discount_code + "\n";
                        if (cartDetail.discounted_price != null)
                            strukText += "   Total Discount: " + string.Format("{0:n0}", cartDetail.discounted_price) + "\n";
                        if (cartDetail.payment_type_name != null)
                            strukText += "   Payment Type: " + cartDetail.payment_type_name + "\n";
                        if (cartDetail.refund_reason_item != null)
                            strukText += "   Refund Reason: " + cartDetail.refund_reason_item + "\n";
                        //strukText += "\n";
                    }
                }
            }

            strukText += "--------------------------------\n";
            strukText += FormatSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal)) + "\n";
            if (!string.IsNullOrEmpty(datas.discount_code))
                strukText += "Discount Code: " + datas.discount_code + "\n";
            if (datas.discounts_value != null)
                strukText += FormatSimpleLine("Discount Value", (datas.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", datas.discounts_value.ToString()) : datas.discounts_value.ToString() + " %")) + "\n";

            strukText += FormatSimpleLine("Total", string.Format("{0:n0}", datas.total)) + "\n";
            strukText += "Payment Type: " + datas.payment_type + "\n";
            strukText += FormatSimpleLine("Cash", string.Format("{0:n0}", datas.customer_cash)) + "\n";
            strukText += FormatSimpleLine("Change", string.Format("{0:n0}", datas.customer_change)) + "\n";
            if (datas.total_refund != null)
                strukText += FormatSimpleLine("Total Refund", string.Format("{0:n0}", datas.total_refund)) + "\n";
            strukText += "--------------------------------\n";

            strukText += CenterText("Meja No. " + datas.customer_seat.ToString());
            //strukText += "--------------------------------\n\n";
            strukText += CenterText("Powered By");

            string NomorUrut = "\n" + kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText("No. " + totalTransactions.ToString()) + "\n";

            byte[] buffer1 = System.Text.Encoding.UTF8.GetBytes(NomorUrut);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

            stream.Write(buffer1, 0, buffer1.Length);
            PrintLogo(stream, "icon\\OutletLogo.bmp", 250); // Smaller logo size
            stream.Write(buffer, 0, buffer.Length);
            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size
            strukText = "\n\n\n\n\n";
            buffer = System.Text.Encoding.UTF8.GetBytes(strukText);
            stream.Write(buffer, 0, buffer.Length);
            stream.Write(NewLine, 0, NewLine.Length);

            stream.Flush();
        }
        private void PrintLogo(System.IO.Stream stream, string logoPath, int targetWidthPx)
        {
            Image logo = Image.FromFile(logoPath);
            Bitmap bmp = new Bitmap(logo);

            // Resize the bitmap to the target width
            Bitmap resizedBitmap = ResizeBitmap(bmp, targetWidthPx);

            // Convert to printer format and center
            byte[] logoBytes = ConvertBitmapToRasterFormat(resizedBitmap, targetWidthPx);
            stream.Write(logoBytes, 0, logoBytes.Length);
        }

        private Bitmap ResizeBitmap(Bitmap bmp, int targetWidthPx)
        {
            int targetHeightPx = (int)((float)bmp.Height / bmp.Width * targetWidthPx);
            Bitmap resizedBmp = new Bitmap(targetWidthPx, targetHeightPx);
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
            List<byte> imageData = new List<byte>();

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
            int totalTransactions)
        {
            try
            {

                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");


                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }

                    // Struck Checker
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics;

                            // Mengatur font normal dan tebal
                            Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                            }



                            // Fungsi untuk mendapatkan dan mengonversi gambar logo ke hitam dan putih
                            Image GetLogoImage(string path)
                            {
                                Image img = Image.FromFile(path);
                                Bitmap bmp = new Bitmap(img.Width, img.Height);
                                using (Graphics g = Graphics.FromImage(bmp))
                                {
                                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                    {
                                        new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                                        new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                                        new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                                        new float[] { 0, 0, 0, 1, 0 },
                                        new float[] { 0, 0, 0, 0, 1 }
                                    });
                                    ImageAttributes attributes = new ImageAttributes();
                                    attributes.SetColorMatrix(colorMatrix);
                                    g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attributes);
                                }
                                return bmp;
                            }

                            // Menambahkan logo dan teks "Powered by" di akhir struk
                            void DrawPoweredByLogo(string path)
                            {
                                // Menambahkan jarak sebelum mencetak teks dan logo
                                float spaceBefore = 50; // Jarak dalam pixel, sesuaikan dengan kebutuhan
                                yPos += spaceBefore;

                                // Mengukur teks "Powered by Your Company"
                                string poweredByText = "Powered by Dastrevas";
                                SizeF textSize = e.Graphics.MeasureString(poweredByText, normalFont);

                                // Gambar teks
                                float textX = leftMargin + (printableWidth - textSize.Width) / 2;
                                e.Graphics.DrawString(poweredByText, normalFont, Brushes.Black, textX, yPos);

                                // Sesuaikan yPos untuk logo
                                yPos += textSize.Height;

                                // Menggambar logo
                                Image logoPoweredBy = GetLogoImage(path);
                                float targetWidth = 35; // Ukuran lebar logo dalam pixel, sesuaikan dengan kebutuhan
                                float scaleFactor = targetWidth / logoPoweredBy.Width;
                                float logoHeight = logoPoweredBy.Height * scaleFactor;

                                float logoX = leftMargin + (printableWidth - targetWidth) / 2;
                                e.Graphics.DrawImage(logoPoweredBy, logoX, yPos, targetWidth, logoHeight);

                                spaceBefore = 5;
                                yPos += spaceBefore;
                                // Sesuaikan yPos untuk elemen berikutnya
                                yPos += logoHeight;
                            }


                            DrawCenterText("No. " + totalTransactions.ToString(), NomorAntrian);

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
                            float logoX = leftMargin + (printableWidth - logoTargetWidthPx) / 2;

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
                                DrawCenterText(datas?.invoice_due_date, normalFont);
                            DrawSpace();
                            DrawLeftText(datas?.customer_name, normalFont);


                            if (cartDetails.Count != 0)
                            {
                                // Iterate through cart details and group by serving_type_name
                                var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();

                                foreach (var servingType in servingTypes)
                                {
                                    // Filter cart details by serving_type_name
                                    var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                                    DrawSeparator();
                                    DrawCenterText("ORDERED", boldFont);

                                    // Skip if there are no items for this serving type
                                    if (itemsForServingType.Count == 0)
                                        continue;

                                    // Add a section for the serving type
                                    DrawSeparator();
                                    DrawCenterText(servingType, normalFont);
                                    DrawSpace();

                                    // Iterate through items for this serving type
                                    foreach (var cartDetail in itemsForServingType)
                                    {
                                        DrawSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.price));
                                        // Add detail items
                                        if (!string.IsNullOrEmpty(cartDetail.varian))
                                            DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                            DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                        if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                            DrawLeftText("   Discount Code: " + cartDetail.discount_code, normalFont);
                                        if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                                            DrawSimpleLine("   Total Discount: ", string.Format("{0:n0}", cartDetail.discounted_price));
                                        DrawSimpleLine("   Total Price:", string.Format("{0:n0}", cartDetail.total_price));
                                        DrawSpace();
                                    }
                                }
                            }

                            if (canceledItems.Count != 0)
                            {
                                var servingTypesCancel = canceledItems.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("CANCELED", boldFont);

                                foreach (var servingType in servingTypesCancel)
                                {
                                    var itemsForServingType = canceledItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                        continue;

                                    DrawSeparator();
                                    DrawCenterText(servingType, normalFont);
                                    DrawSpace();

                                    foreach (var cancelItem in itemsForServingType)
                                    {
                                        DrawSimpleLine(cancelItem.qty + " " + cancelItem.menu_name, string.Format("{0:n0}", cancelItem.price));
                                        // Add detail items
                                        if (!string.IsNullOrEmpty(cancelItem.varian))
                                            DrawLeftText("   Varian: " + cancelItem.varian, normalFont);
                                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                            DrawLeftText("   Note: " + cancelItem.note_item, normalFont);
                                        if (!string.IsNullOrEmpty(cancelItem.discount_code))
                                            DrawLeftText("   Discount Code: " + cancelItem.discount_code, normalFont);
                                        if (cancelItem.discounted_price.HasValue && cancelItem.discounted_price != 0)
                                            DrawSimpleLine("   Total Discount", string.Format("{0:n0}", cancelItem.discounted_price));
                                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                            DrawLeftText("   Reason: " + cancelItem.cancel_reason, normalFont);
                                        // Add an empty line between items
                                        DrawSpace();
                                    }
                                }
                            }
                            if (cartRefundDetails.Count != 0)
                            {

                                DrawSeparator();
                                DrawCenterText("REFUNDED", boldFont);
                                var servingTypesRefund = cartRefundDetails.Select(cd => cd.serving_type_name).Distinct();
                                foreach (var servingTypeRefund in servingTypesRefund)
                                {
                                    // Filter cart details by serving_type_name
                                    var itemsForServingType = cartRefundDetails.Where(cd => cd.serving_type_name == servingTypeRefund).ToList();

                                    // Skip if there are no items for this serving type
                                    if (itemsForServingType.Count == 0)
                                        continue;

                                    // Add a section for the serving type
                                    DrawSeparator();
                                    DrawCenterText(servingTypeRefund, normalFont);
                                    DrawSpace();
                                    // Iterate through items for this serving type
                                    foreach (var cartDetail in itemsForServingType)
                                    {
                                        DrawSimpleLine(cartDetail.qty_refund_item + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.total_refund_price));
                                        // Add detail items
                                        if (!string.IsNullOrEmpty(cartDetail.varian))
                                            DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                            DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                        if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                            DrawLeftText("   Discount Code: " + cartDetail.discount_code, normalFont);
                                        if (cartDetail.discounted_price != null)
                                            DrawSimpleLine("   Total Discount: ", string.Format("{0:n0}", cartDetail.discounted_price));
                                        if (cartDetail.payment_type_name != null)
                                            DrawLeftText("   Payment Type: " + cartDetail.payment_type_name, normalFont);
                                        if (cartDetail.refund_reason_item != null)
                                            DrawLeftText("   Refund Reason: " + cartDetail.refund_reason_item, normalFont);
                                        // Add an empty line between items
                                        DrawSpace();
                                    }
                                }
                            }
                            DrawSeparator();
                            DrawSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal));
                            if (!string.IsNullOrEmpty(datas.discount_code))
                                DrawSimpleLine("Discount Code", datas.discount_code);
                            if (datas.discounts_value != null)
                                DrawSimpleLine("Discount Value", (datas.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", datas.discounts_value.ToString()) : datas.discounts_value.ToString() + " %"));
                            DrawSimpleLine("Total", string.Format("{0:n0}", datas.total));
                            DrawLeftText("Payment Type " + datas.payment_type, normalFont);
                            DrawSimpleLine("Cash", string.Format("{0:n0}", datas.customer_cash));
                            DrawSimpleLine("Change", string.Format("{0:n0}", datas.customer_change));
                            if (datas.total_refund != null)
                                DrawSimpleLine("Total Refund", string.Format("{0:n0}", datas.total_refund));
                            DrawSeparator();
                            DrawPoweredByLogo(poweredByLogoPath);
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);
                        };
                        /*
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
                        */
                    }
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
                totalTransactions = 0;
            try
            {
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings.ToList()) // Membuat salinan dari koleksi
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrintModelDataBill(datas, cartDetails, canceledItems, totalTransactions);
                        continue;
                    }
                    if (ShouldPrint(printerId, "Checker"))
                    {
                        System.IO.Stream stream = null;

                        try
                        {
                            if (System.Net.IPAddress.TryParse(printerName, out _))
                            {
                                // Connect via LAN
                                var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                stream = client.GetStream();
                            }
                            else
                            {
                                // Connect via Bluetooth dengan retry policy
                                if (!await RetryPolicyAsync(async () =>
                                {
                                    // Connect via Bluetooth
                                    BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                    if (printerDevice == null)
                                    {
                                        //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                        return false;
                                    }

                                    BluetoothClient client = new BluetoothClient();
                                    BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                    if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                    {
                                        ////MessageBox.Show("Pairing failed to " + printerName, "Error");
                                        return false;
                                    }

                                    client.Connect(endpoint);
                                    stream = client.GetStream();

                                    return true;
                                }, maxRetries: 3))
                                {
                                    continue;
                                }
                            }

                            // Setelah koneksi berhasil, cetak struk
                            PrintDataBillReceipt(stream, datas, cartDetails, canceledItems, totalTransactions);

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

        private void PrintDataBillReceipt(System.IO.Stream stream, DataRestruk datas,
            List<CartDetailRestruk> cartDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
            int totalTransactions)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            byte[] NewLine = new byte[] { 0x0A }; // New line

            // Construct receipt text
            string strukText = kodeHeksadesimalBold + CenterText("SaveBill No. " + totalTransactions.ToString());
            strukText += kodeHeksadesimalNormal;
            //strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText("CHECKER");
            if (!string.IsNullOrEmpty(datas?.receipt_number))
            {
                // Periksa apakah panjang receipt_number lebih dari 32 karakter
                if (datas.receipt_number.Length > 32)
                {
                    // Jika lebih dari 32 karakter, gunakan teks biasa
                    strukText += "Receipt No." + datas.receipt_number.ToString() + "\n";
                }
                else
                {
                    // Jika tidak lebih dari 32 karakter, gunakan CenterText
                    strukText += CenterText("Receipt No." + datas.receipt_number.ToString());
                }
            }
            else
            {
                // Jika receipt_number null atau kosong, cetak pesan alternatif
                strukText += CenterText("Receipt No. Tidak Tersedia");
            }


            strukText += kodeHeksadesimalNormal;
            //strukText += "--------------------------------\n";

            if (datas?.invoice_due_date != null)
                strukText += CenterText(datas?.invoice_due_date);

            strukText += "Name: " + datas?.customer_name + "\n";
            
            // Ordered items
            if (cartDetails.Count > 0 && cartDetails != null)
            {
                var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                    strukText += "--------------------------------\n";
                    strukText += CenterText("ORDERED");

                    if (itemsForServingType.Count == 0)
                        continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    //strukText += "\n";

                    foreach (var cartDetail in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalBold + $"{cartDetail.menu_name}" + "\n";
                        strukText += kodeHeksadesimalNormal;
                        strukText += FormatSimpleLine("@" + string.Format("{0:n0}", cartDetail.price) + " x" + cartDetail.qty, string.Format("{0:n0}", cartDetail.total_price)) + "\n";
                        //strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.price)) + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "   Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "   Note: " + cartDetail.note_item + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.discount_code))
                            strukText += "   Discount Code: " + cartDetail.discount_code + "\n";
                        if (cartDetail.discounts_value != null && Convert.ToDecimal(cartDetail.discounts_value) != 0)
                            strukText += "   Discount Value: " + (cartDetail.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", cartDetail.discounts_value.ToString()) : cartDetail.discounts_value.ToString() + " %") + "\n";
                        //if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                        //    strukText += "   Total Discount: " + string.Format("{0:n0}", cartDetail.discounted_price) + "\n";
                        //strukText += "   Total Price: " + string.Format("{0:n0}", cartDetail.total_price) + "\n";
                        //strukText += "\n";
                    }
                }
            }

            // Canceled items
            if (canceledItems.Count > 0 && canceledItems != null)
            {
                var servingTypesCancel = canceledItems.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("CANCELED");

                foreach (var servingType in servingTypesCancel)
                {
                    var itemsForServingType = canceledItems.Where(cd => cd.serving_type_name == servingType).ToList();

                    if (itemsForServingType.Count == 0)
                        continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    //strukText += "\n";

                    foreach (var cancelItem in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalBold + $"{cancelItem.menu_name}" + "\n";
                        strukText += kodeHeksadesimalNormal;
                        strukText += FormatSimpleLine("@" + string.Format("{0:n0}", cancelItem.price) + " x" + cancelItem.qty, string.Format("{0:n0}", cancelItem.total_price)) + "\n";
                        //strukText += FormatSimpleLine(cancelItem.qty + " " + cancelItem.menu_name, string.Format("{0:n0}", cancelItem.price)) + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.varian))
                            strukText += "   Varian: " + cancelItem.varian + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                            strukText += "   Note: " + cancelItem.note_item + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                            strukText += "   Reason: " + cancelItem.cancel_reason + "\n";
                        //strukText += "\n";
                    }
                }
            }
            
            // Subtotal, discount, and total
            strukText += "--------------------------------\n";
            strukText += FormatSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal)) + "\n";
            if (!string.IsNullOrEmpty(datas.discount_code))
                strukText += "Discount Code: " + datas.discount_code + "\n";
            if (datas?.discounts_value != null && datas.discounts_value != "")
                strukText += FormatSimpleLine("Discount Value: ", (datas.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", datas.discounts_value.ToString()) : datas.discounts_value.ToString() + " %")) + "\n";
            strukText += FormatSimpleLine("Total", string.Format("{0:n0}", datas.total)) + "\n";

            // Add the "Meja No." at the bottom
            strukText += kodeHeksadesimalNormal + CenterText("Meja No. " + datas?.customer_seat); 
            strukText += "--------------------------------\n";
            //strukText += "--------------------------------\n";

            // Convert the final string to bytes and send to the printer
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);
            stream.Write(buffer, 0, buffer.Length);
            stream.Write(NewLine, 0, NewLine.Length);

            // Flush the stream to ensure everything is sent
            stream.Flush();

        }

        public async Task Ex_PrintModelDataBill
           (DataRestruk datas,
            List<CartDetailRestruk> cartDetails,
            List<CanceledItemStrukCustomerRestruk> canceledItems,
           int totalTransactions)
        {
            try
            {

                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");


                    // Struck Checker
                    if (ShouldPrint(printerId, "Checker"))
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics;

                            // Mengatur font normal dan tebal
                            Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                            }


                            DrawCenterText("SaveBill No. " + totalTransactions.ToString(), NomorAntrian);
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
                                DrawCenterText(datas?.invoice_due_date, normalFont);
                            DrawSpace();
                            DrawLeftText(datas?.customer_name, normalFont);

                            if (cartDetails.Count != 0)
                            {
                                // Iterate through cart details and group by serving_type_name
                                var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();

                                foreach (var servingType in servingTypes)
                                {
                                    // Filter cart details by serving_type_name
                                    var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                                    DrawSeparator();
                                    DrawCenterText("ORDERED", boldFont);

                                    // Skip if there are no items for this serving type
                                    if (itemsForServingType.Count == 0)
                                        continue;

                                    // Add a section for the serving type
                                    DrawSeparator();
                                    DrawCenterText(servingType, normalFont);
                                    DrawSpace();

                                    // Iterate through items for this serving type
                                    foreach (var cartDetail in itemsForServingType)
                                    {
                                        DrawSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.price));
                                        // Add detail items
                                        if (!string.IsNullOrEmpty(cartDetail.varian))
                                            DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                            DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                        if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                            DrawLeftText("   Discount Code: " + cartDetail.discount_code, normalFont);
                                        if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                                            DrawSimpleLine("   Total Discount: ", string.Format("{0:n0}", cartDetail.discounted_price));
                                        DrawSimpleLine("   Total Price:", string.Format("{0:n0}", cartDetail.total_price));
                                        DrawSpace();
                                    }
                                }
                            }

                            if (canceledItems.Count != 0)
                            {
                                var servingTypesCancel = canceledItems.Select(cd => cd.serving_type_name).Distinct();
                                DrawSeparator();
                                DrawCenterText("CANCELED", boldFont);

                                foreach (var servingType in servingTypesCancel)
                                {
                                    var itemsForServingType = canceledItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                    if (itemsForServingType.Count == 0)
                                        continue;

                                    DrawSeparator();
                                    DrawCenterText(servingType, normalFont);
                                    DrawSpace();

                                    foreach (var cancelItem in itemsForServingType)
                                    {
                                        DrawSimpleLine(cancelItem.qty + " " + cancelItem.menu_name, string.Format("{0:n0}", cancelItem.price));
                                        // Add detail items
                                        if (!string.IsNullOrEmpty(cancelItem.varian))
                                            DrawLeftText("   Varian: " + cancelItem.varian, normalFont);
                                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                            DrawLeftText("   Note: " + cancelItem.note_item, normalFont);
                                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                            DrawLeftText("   Reason: " + cancelItem.cancel_reason, normalFont);
                                        // Add an empty line between items
                                        DrawSpace();
                                    }
                                }
                            }
                            DrawSeparator();
                            DrawSimpleLine("Subtotal", string.Format("{0:n0}", datas.subtotal));
                            if (!string.IsNullOrEmpty(datas.discount_code))
                                DrawLeftText("Discount Code: " + datas.discount_code, normalFont);
                            if (datas?.discounts_value != null && datas.discounts_value != "")
                                DrawSimpleLine("Discount Value: ", (datas.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", datas.discounts_value.ToString()) : datas.discounts_value.ToString() + " %"));
                            DrawSimpleLine("Total", string.Format("{0:n0}", datas.total));
                            DrawSeparator();
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);
                        };
                        /*
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
                          */
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        // Struct Simpan bill / save bill
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
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                List<KeyValuePair<string, string>> printerSettingsCopy;
                lock (printerSettings)
                {
                    printerSettingsCopy = printerSettings.ToList(); // Create a copy of the collection
                }

                foreach (var printer in printerSettingsCopy)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        Ex_PrinterModelSimpan(datas, KitchenCartDetails, KitchenCancelItems, BarCartDetails, BarCancelItems, totalTransactions);
                        continue;
                    }
                    if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                    {
                        if (ShouldPrint(printerId, "Makanan"))
                        {
                            System.IO.Stream stream = null;

                            try
                            {
                                if (System.Net.IPAddress.TryParse(printerName, out _))
                                {
                                    // Connect via LAN
                                    var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                    stream = client.GetStream();
                                }
                                else
                                {
                                    // Connect via Bluetooth dengan retry policy
                                    if (!await RetryPolicyAsync(async () =>
                                    {
                                        // Connect via Bluetooth
                                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                        if (printerDevice == null)
                                        {
                                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                            return false;
                                        }

                                        BluetoothClient client = new BluetoothClient();
                                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                        if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                        {
                                            //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                            return false;
                                        }

                                        client.Connect(endpoint);
                                        stream = client.GetStream();

                                        return true;
                                    }, maxRetries: 3))
                                    {
                                        continue;
                                    }
                                }

                                PrintSimpanReceipt(stream, datas, KitchenCartDetails, KitchenCancelItems, BarCartDetails, BarCancelItems, totalTransactions, "Makanan");
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

                    if (BarCartDetails.Any() || BarCancelItems.Any())
                    {
                        if (ShouldPrint(printerId, "Minuman"))
                        {
                            System.IO.Stream stream = null;

                            try
                            {
                                if (System.Net.IPAddress.TryParse(printerName, out _))
                                {
                                    // Connect via LAN
                                    var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                    stream = client.GetStream();
                                }
                                else
                                {
                                    // Connect via Bluetooth dengan retry policy
                                    if (!await RetryPolicyAsync(async () =>
                                    {
                                        // Connect via Bluetooth
                                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                        if (printerDevice == null)
                                        {
                                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                            return false;
                                        }

                                        BluetoothClient client = new BluetoothClient();
                                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                        if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                        {
                                            //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                            return false;
                                        }

                                        client.Connect(endpoint);
                                        stream = client.GetStream();

                                        return true;
                                    }, maxRetries: 3))
                                    {
                                        continue;
                                    }
                                }

                                PrintSimpanReceipt(stream, datas, KitchenCartDetails, KitchenCancelItems, BarCartDetails, BarCancelItems, totalTransactions, "Minuman");
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
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void PrintSimpanReceipt(System.IO.Stream stream, GetStrukCustomerTransaction datas,
            List<CartDetailStrukCustomerTransaction> KitchenCartDetails,
            List<CanceledItemStrukCustomerTransaction> KitchenCancelItems,
            List<CartDetailStrukCustomerTransaction> BarCartDetails,
            List<CanceledItemStrukCustomerTransaction> BarCancelItems,
            int totalTransactions, string type)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            byte[] InitPrinter = new byte[] { 0x1B, 0x40 }; // Initialize printer
            byte[] NewLine = new byte[] { 0x0A }; // New line

            // Write initialization bytes
            stream.Write(InitPrinter, 0, InitPrinter.Length);

            // Print logo (assuming logo is already in a proper format for the printer)
            //PrintLogo(stream, "icon\\OutletLogo.bmp", 50); // Larger logo size

            // Print the rest of the receipt
            string strukText = "\n" + kodeHeksadesimalBold + CenterText("SaveBill No. " + totalTransactions.ToString());
            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText(type.ToUpper());
            strukText += CenterText("Receipt No.: " + datas.data?.receipt_number);
            strukText += kodeHeksadesimalNormal;
            strukText += "--------------------------------\n";
            strukText += CenterText(datas.data?.invoice_due_date);
            strukText += "Name: " + datas.data?.customer_name + "\n";

            if (type == "Makanan" && KitchenCartDetails.Count != 0)
            {
                var servingTypes = KitchenCartDetails.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("ORDER");

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = KitchenCartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                    if (itemsForServingType.Count == 0) continue;

                    strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    strukText += "\n";

                    foreach (var cartDetail in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold;

                        strukText += FormatSimpleLine(cartDetail.menu_name, cartDetail.qty ) + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "Note: " + cartDetail.note_item + "\n";

                        strukText += kodeHeksadesimalNormal;
                        strukText += "\n";
                    }
                }
            }

            if (type == "Makanan" && KitchenCancelItems.Count != 0)
            {
                var servingTypes = KitchenCancelItems.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("CANCELED");

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = KitchenCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();
                    if (itemsForServingType.Count == 0) continue;

                    strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    strukText += "\n";

                    foreach (var cancelItem in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold;

                        strukText += FormatSimpleLine(cancelItem.menu_name, cancelItem.qty) + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.varian))
                            strukText += "Varian: " + cancelItem.varian + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                            strukText += "Note: " + cancelItem.note_item + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                            strukText += "Canceled Reason: " + cancelItem.cancel_reason + "\n";
                        strukText += kodeHeksadesimalNormal;

                        strukText += "\n";
                    }
                }
            }

            if (type == "Minuman" && BarCartDetails.Count != 0)
            {
                var servingTypes = BarCartDetails.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("ORDER");

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = BarCartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                    if (itemsForServingType.Count == 0) continue;

                    strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    strukText += "\n";

                    foreach (var cartDetail in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold;

                        strukText += FormatSimpleLine("x" + cartDetail.qty + " " + cartDetail.menu_name, "") + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "Note: " + cartDetail.note_item + "\n";
                        strukText += kodeHeksadesimalNormal;

                        strukText += "\n";
                    }
                }
            }

            if (type == "Minuman" && BarCancelItems.Count != 0)
            {
                var servingTypes = BarCancelItems.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("CANCELED");

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = BarCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();
                    if (itemsForServingType.Count == 0) continue;

                    strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    strukText += "\n";

                    foreach (var cancelItem in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold;

                        strukText += FormatSimpleLine("x" + cancelItem.qty + " " + cancelItem.menu_name, "") + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.varian))
                            strukText += "Varian: " + cancelItem.varian + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                            strukText += "Note: " + cancelItem.note_item + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                            strukText += "Canceled Reason: " + cancelItem.cancel_reason + "\n";
                        strukText += kodeHeksadesimalNormal;

                        strukText += "\n";
                    }
                }
            }

            strukText += "--------------------------------\n\n\n\n\n";

            // Encode your text into bytes (you might need to adjust the encoding)
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

            // Send the text to the printer
            stream.Write(buffer, 0, buffer.Length);

            // Flush the stream to ensure all data is sent to the printer
            stream.Flush();
        }


        public async Task Ex_PrinterModelSimpan(
            GetStrukCustomerTransaction datas,
            List<CartDetailStrukCustomerTransaction> KitchenCartDetails,
            List<CanceledItemStrukCustomerTransaction> KitchenCancelItems,
            List<CartDetailStrukCustomerTransaction> BarCartDetails,
            List<CanceledItemStrukCustomerTransaction> BarCancelItems,
            int totalTransactions)
        {
            try
            {
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }

                    // Struct Kitchen&Bar

                    if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                    {

                        if (ShouldPrint(printerId, "Makanan"))
                        {
                            PrintDocument printDocument = new PrintDocument();
                            printDocument.PrintPage += (sender, e) =>
                            {
                                Graphics graphics = e.Graphics;

                                // Mengatur font normal dan tebal
                                Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                                Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                                Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                                Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                    if (textLeft == null) textLeft = string.Empty;
                                    if (textRight == null) textRight = string.Empty;

                                    SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                                    SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                                    e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                                    e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                    yPos += normalFont.GetHeight(e.Graphics);
                                }

                                // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                                void DrawCenterText(string text, Font font)
                                {
                                    if (text == null) text = string.Empty;
                                    if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");

                                    string[] words = text.Split(' ');
                                    StringBuilder currentLine = new StringBuilder();
                                    foreach (string word in words)
                                    {
                                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                        if (size.Width > printableWidth)
                                        {
                                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                            SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    if (text == null) text = string.Empty;
                                    if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawLeftText method.");

                                    string[] words = text.Split(' ');
                                    StringBuilder currentLine = new StringBuilder();
                                    foreach (string word in words)
                                    {
                                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                        if (size.Width > printableWidth)
                                        {
                                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                    yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                                }


                                string nomorMeja = "Meja No." + datas.data?.customer_seat;
                                DrawCenterText("SaveBill No. " + totalTransactions.ToString(), NomorAntrian);

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
                                    var servingTypes = KitchenCartDetails.Select(cd => cd.serving_type_name).Distinct();
                                    DrawSeparator();
                                    DrawCenterText("ORDER", boldFont);

                                    foreach (var servingType in servingTypes)
                                    {
                                        var itemsForServingType = KitchenCartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                                        if (itemsForServingType.Count == 0)
                                            continue;

                                        DrawSeparator();
                                        DrawCenterText(servingType, normalFont);

                                        foreach (var cartDetail in itemsForServingType)
                                        {
                                            string qtyMenu = "x" + cartDetail.qty.ToString();
                                            DrawCenterText(qtyMenu + " " + cartDetail.menu_name, BigboldFont);

                                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                                DrawCenterText("Varian: " + cartDetail.varian, BigboldFont);
                                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                                DrawCenterText("Note: " + cartDetail.note_item, BigboldFont);
                                            DrawSpace();
                                        }
                                    }
                                }

                                if (KitchenCancelItems.Count != 0)
                                {
                                    var servingTypes = KitchenCancelItems.Select(cd => cd.serving_type_name).Distinct();
                                    DrawSeparator();
                                    DrawCenterText("CANCELED", boldFont);

                                    foreach (var servingType in servingTypes)
                                    {
                                        var itemsForServingType = KitchenCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                        if (itemsForServingType.Count == 0)
                                            continue;

                                        DrawSeparator();
                                        DrawCenterText(servingType, boldFont);

                                        foreach (var cancelItem in itemsForServingType)
                                        {
                                            string qtyMenu = "x" + cancelItem.qty.ToString();
                                            DrawCenterText(qtyMenu + " " + cancelItem.menu_name, BigboldFont);

                                            if (!string.IsNullOrEmpty(cancelItem.varian))
                                                DrawCenterText("Varian: " + cancelItem.varian, BigboldFont);
                                            if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                                DrawCenterText("Note: " + cancelItem.note_item?.ToString(), BigboldFont);
                                            if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                                DrawCenterText("Canceled Reason: " + cancelItem.cancel_reason, BigboldFont);
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
                            PrintDocument printDocument = new PrintDocument();
                            printDocument.PrintPage += (sender, e) =>
                            {
                                Graphics graphics = e.Graphics;

                                // Mengatur font normal dan tebal
                                Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                                Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                                Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                                Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                    if (textLeft == null) textLeft = string.Empty;
                                    if (textRight == null) textRight = string.Empty;

                                    SizeF sizeLeft = e.Graphics.MeasureString(textLeft, normalFont);
                                    SizeF sizeRight = e.Graphics.MeasureString(textRight, normalFont);
                                    e.Graphics.DrawString(textLeft, normalFont, Brushes.Black, leftMargin, yPos);
                                    e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                    yPos += normalFont.GetHeight(e.Graphics);
                                }

                                // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                                void DrawCenterText(string text, Font font)
                                {
                                    if (text == null) text = string.Empty;
                                    if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");

                                    string[] words = text.Split(' ');
                                    StringBuilder currentLine = new StringBuilder();
                                    foreach (string word in words)
                                    {
                                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                        if (size.Width > printableWidth)
                                        {
                                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                            SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    if (text == null) text = string.Empty;
                                    if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawLeftText method.");

                                    string[] words = text.Split(' ');
                                    StringBuilder currentLine = new StringBuilder();
                                    foreach (string word in words)
                                    {
                                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                        if (size.Width > printableWidth)
                                        {
                                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                    yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                                }


                                string nomorMeja = "Meja No." + datas.data?.customer_seat;
                                DrawCenterText("SaveBill No. " + totalTransactions.ToString(), NomorAntrian);

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
                                    var servingTypes = BarCartDetails.Select(cd => cd.serving_type_name).Distinct();
                                    DrawSeparator();
                                    DrawCenterText("ORDER", boldFont);

                                    foreach (var servingType in servingTypes)
                                    {
                                        var itemsForServingType = BarCartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                                        if (itemsForServingType.Count == 0)
                                            continue;

                                        DrawSeparator();
                                        DrawCenterText(servingType, normalFont);

                                        foreach (var cartDetail in itemsForServingType)
                                        {
                                            string qtyMenu = "x" + cartDetail.qty.ToString();
                                            DrawCenterText(qtyMenu + " " + cartDetail.menu_name, BigboldFont);

                                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                                DrawCenterText("Varian: " + cartDetail.varian, BigboldFont);
                                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                                DrawCenterText("Note: " + cartDetail.note_item, BigboldFont);
                                            DrawSpace();
                                        }
                                    }
                                }

                                if (BarCancelItems.Count != 0)
                                {
                                    var servingTypes = BarCancelItems.Select(cd => cd.serving_type_name).Distinct();
                                    DrawSeparator();
                                    DrawCenterText("CANCELED", boldFont);

                                    foreach (var servingType in servingTypes)
                                    {
                                        var itemsForServingType = BarCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                        if (itemsForServingType.Count == 0)
                                            continue;

                                        DrawSeparator();
                                        DrawCenterText(servingType, boldFont);

                                        foreach (var cancelItem in itemsForServingType)
                                        {
                                            string qtyMenu = "x" + cancelItem.qty.ToString();
                                            DrawCenterText(qtyMenu + " " + cancelItem.menu_name, BigboldFont);

                                            if (!string.IsNullOrEmpty(cancelItem.varian))
                                                DrawCenterText("Varian: " + cancelItem.varian, BigboldFont);
                                            if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                                DrawCenterText("Note: " + cancelItem.note_item?.ToString(), BigboldFont);
                                            if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                                DrawCenterText("Canceled Reason: " + cancelItem.cancel_reason, BigboldFont);
                                            DrawSpace();
                                        }
                                    }
                                }
                                DrawSeparator();
                                DrawSpace();
                                DrawCenterText(nomorMeja, NomorAntrian);
                            };
                            /*
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
                            */

                        }
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
                await LoadPrinterSettings(); // Load printer settings
                await LoadSettingsAsync();   // Load additional settings

                foreach (var printer in printerSettings)
                {
                    var printerName = printer.Value;
                    if (IsBluetoothPrinter(printerName))
                    {
                        printerName = ConvertMacAddressFormat(printerName);
                    }
                    var printerId = printer.Key.Replace("inter", "");

                    if (string.IsNullOrWhiteSpace(printerName) || printerName.Length < 3)
                    {
                        continue;
                    }
                    if (IsNotMacAddressOrIpAddress(printerName))
                    {
                        await Ex_PrintModelPayform(
                    datas, cartDetails, KitchenCartDetails, BarCartDetails,
                    KitchenCancelItems, BarCancelItems, totalTransactions, Kakimu,
                    printerId, printerName
                );
                        continue;
                    }
                    // Struct Customer ====
                    if (ShouldPrint(printerId, "Kasir"))
                    {
                        System.IO.Stream stream = null;

                        try
                        {
                            if (System.Net.IPAddress.TryParse(printerName, out _))
                            {
                                // Connect via LAN
                                var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                stream = client.GetStream();
                            }
                            else
                            {
                                // Connect via Bluetooth dengan retry policy
                                if (!await RetryPolicyAsync(async () =>
                                {
                                    // Connect via Bluetooth
                                    BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                    if (printerDevice == null)
                                    {
                                        //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                        return false;
                                    }

                                    BluetoothClient client = new BluetoothClient();
                                    BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                    if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                    {
                                        //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                        return false;
                                    }

                                    client.Connect(endpoint);
                                    stream = client.GetStream();

                                    return true;
                                }, maxRetries: 3))
                                {
                                    continue;
                                }
                            }

                            // Setelah koneksi berhasil, cetak struk
                            PrintCustomerReceipt(stream, datas, cartDetails, totalTransactions, Kakimu);

                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
                            }
                        }
                    }

                    // Struct Checker
                    if (ShouldPrint(printerId, "Checker"))
                    {
                        System.IO.Stream stream = null;

                        try
                        {
                            // Filter checker cart details yang is_ordered == 1
                            var orderedCheckerItems = cartDetails.Where(x => x.is_ordered != 1).ToList();

                            if (orderedCheckerItems.Any())
                            {
                                if (System.Net.IPAddress.TryParse(printerName, out _))
                                {
                                    // Connect via LAN
                                    var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                    stream = client.GetStream();
                                }
                                else
                                {
                                    // Connect via Bluetooth dengan retry policy
                                    if (!await RetryPolicyAsync(async () =>
                                    {
                                        // Connect via Bluetooth
                                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                        if (printerDevice == null)
                                        {
                                            //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                            return false;
                                        }

                                        BluetoothClient client = new BluetoothClient();
                                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                        if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                        {
                                            //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                            return false;
                                        }

                                        client.Connect(endpoint);
                                        stream = client.GetStream();

                                        return true;
                                    }, maxRetries: 3))
                                    {
                                        continue;
                                    }
                                }

                                // Setelah koneksi berhasil, cetak struk
                                PrintCheckerReceipt(stream, datas, cartDetails, totalTransactions);

                            }
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
                            }
                        }
                    }

                    // Struct Kitchen
                    if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                    {
                        if (ShouldPrint(printerId, "Makanan"))
                        {
                            System.IO.Stream stream = null;

                            try
                            {
                                // Filter kitchen cart details yang is_ordered == 1
                                var orderedKitchenItems = KitchenCartDetails.Where(x => x.is_ordered != 1).ToList();

                                if (orderedKitchenItems.Any())
                                {
                                    if (System.Net.IPAddress.TryParse(printerName, out _))
                                    {
                                        // Connect via LAN
                                        var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                        stream = client.GetStream();
                                    }
                                    else
                                    {
                                        // Connect via Bluetooth dengan retry policy
                                        if (!await RetryPolicyAsync(async () =>
                                        {
                                            // Connect via Bluetooth
                                            BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                            if (printerDevice == null)
                                            {
                                                //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                                return false;
                                            }

                                            BluetoothClient client = new BluetoothClient();
                                            BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                            {
                                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                                return false;
                                            }

                                            client.Connect(endpoint);
                                            stream = client.GetStream();

                                            return true;
                                        }, maxRetries: 3))
                                        {
                                            continue;
                                        }
                                    }

                                    // Setelah koneksi berhasil, cetak struk
                                    PrintKitchenOrBarReceipt(stream, datas, KitchenCartDetails, KitchenCancelItems, totalTransactions, "Makanan");

                                }
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

                    // Struct Bar
                    if (BarCartDetails.Any() || BarCancelItems.Any())
                    {
                        if (ShouldPrint(printerId, "Minuman"))
                        {
                            System.IO.Stream stream = null;

                            try
                            {
                                // Filter bar cart details yang is_ordered == 1
                                var orderedBarItems = BarCartDetails.Where(x => x.is_ordered != 1).ToList();

                                if (orderedBarItems.Any())
                                {
                                    if (System.Net.IPAddress.TryParse(printerName, out _))
                                    {
                                        // Connect via LAN
                                        var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                                        stream = client.GetStream();
                                    }
                                    else
                                    {
                                        // Connect via Bluetooth dengan retry policy
                                        if (!await RetryPolicyAsync(async () =>
                                        {
                                            // Connect via Bluetooth
                                            BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                                            if (printerDevice == null)
                                            {
                                                //MessageBox.Show("Printer " + printerName + " not found", "Error");
                                                return false;
                                            }

                                            BluetoothClient client = new BluetoothClient();
                                            BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                                            if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                                            {
                                                //MessageBox.Show("Pairing failed to " + printerName, "Error");
                                                return false;
                                            }

                                            client.Connect(endpoint);
                                            stream = client.GetStream();

                                            return true;
                                        }, maxRetries: 3))
                                        {
                                            continue;
                                        }
                                    }

                                    // Setelah koneksi berhasil, cetak struk
                                    PrintKitchenOrBarReceipt(stream, datas, BarCartDetails, BarCancelItems, totalTransactions, "Minuman");

                                }
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
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void PrintCustomerReceipt(System.IO.Stream stream, GetStrukCustomerTransaction datas,
            List<CartDetailStrukCustomerTransaction> cartDetails,
            int totalTransactions, string Kakimu)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            byte[] InitPrinter = new byte[] { 0x1B, 0x40 }; // Initialize printer
            byte[] NewLine = new byte[] { 0x0A }; // New line
/*
            // Set Line Spacing (perintah ESC/POS untuk line spacing, misalnya 30 dots)
            byte[] setLineSpacing = new byte[] { 0x1B, 0x33, 1 }; // Ganti 30 sesuai kebutuhan
            stream.Write(setLineSpacing, 0, setLineSpacing.Length); // Mengirim perintah untuk line spacing
*/
            // Write initialization bytes
            stream.Write(InitPrinter, 0, InitPrinter.Length);

            string strukText = kodeHeksadesimalNormal;
            strukText += kodeHeksadesimalBold + CenterText(datas.data?.outlet_name);
            strukText += kodeHeksadesimalNormal;
            strukText += CenterText(datas.data?.outlet_address);
            strukText += CenterText(datas.data?.outlet_phone_number);
            //strukText += "--------------------------------\n";
            strukText += CenterText("Receipt No.: " + datas.data?.receipt_number);
            //strukText += "--------------------------------\n";
            strukText += CenterText(datas.data?.invoice_due_date);
            strukText += "Name: " + datas.data?.customer_name + "\n";

            if (cartDetails.Count != 0)
            {
                var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("ORDER");

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                    if (itemsForServingType.Count == 0) continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    //strukText += "\n";

                    foreach (var cartDetail in itemsForServingType)
                    {
                        //strukText += FormatSimpleLine(cartDetail.qty + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.price)) + "\n";
                        strukText += kodeHeksadesimalBold + $"{cartDetail.menu_name}" + "\n";
                        strukText += kodeHeksadesimalNormal;
                        strukText += FormatSimpleLine("@" + string.Format("{0:n0}", cartDetail.price) + " x" + cartDetail.qty, string.Format("{0:n0}", cartDetail.total_price)) + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "  Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "  Note: " + cartDetail.note_item + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.discount_code))
                            strukText += "  Discount Code: " + cartDetail.discount_code + "\n";
                        if (cartDetail.discounts_value != null && Convert.ToDecimal(cartDetail.discounts_value) != 0)
                            strukText += "  Discount Value: " + (cartDetail.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", cartDetail.discounts_value.ToString()) : cartDetail.discounts_value.ToString() + " %") + "\n";
                        //if (cartDetail.discounts_value.HasValue && cartDetail.discounts_value != 0)
                        //    strukText += "  Total Discount: " + string.Format("Rp. {0:n0},-", cartDetail.discounted_price) + "\n";
                        //if (cartDetail.qty > 1)
                        //  strukText += "  Total Price: " + string.Format("{0:n0}", cartDetail.total_price) + "\n";

                        //strukText += "\n";
                    }
                }
            }

            strukText += "--------------------------------\n";
            strukText += FormatSimpleLine("Subtotal", string.Format("{0:n0}", datas.data.subtotal)) + "\n";
            if (!string.IsNullOrEmpty(datas.data.discount_code))
                strukText += "Discount Code: " + datas.data.discount_code + "\n";
            if (datas.data.discounts_value.HasValue && datas.data.discounts_value != 0)
                strukText += FormatSimpleLine("Discount Value: ", (datas.data.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", datas.data.discounts_value.ToString()) : datas.data.discounts_value.ToString() + " %")) + "\n";
            strukText += FormatSimpleLine("Total", string.Format("{0:n0}", datas.data.total)) + "\n";
            strukText += "Payment Type: " + datas.data.payment_type + "\n";
            strukText += FormatSimpleLine("Cash", string.Format("{0:n0}", datas.data.customer_cash)) + "\n";
            strukText += FormatSimpleLine("Change", string.Format("{0:n0}", datas.data.customer_change)) + "\n";
            strukText += "--------------------------------\n";
            strukText += CenterText(datas.data.outlet_footer);
            strukText += CenterText(Kakimu.ToString());
            strukText += CenterText("Meja No. " + datas.data.customer_seat.ToString());
            strukText += "--------------------------------\n";
            strukText += CenterText("Powered By");

            string NomorUrut = "\n" + kodeHeksadesimalSizeBesar + kodeHeksadesimalBold + CenterText("No. " + totalTransactions.ToString()) + "\n\n\n    ";

            byte[] buffer1 = System.Text.Encoding.UTF8.GetBytes(NomorUrut);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

            stream.Write(buffer1, 0, buffer1.Length);
            PrintLogo(stream, "icon\\OutletLogo.bmp", 250); // Smaller logo size
            stream.Write(buffer, 0, buffer.Length);
            PrintLogo(stream, "icon\\DT-Logo.bmp", 75); // Smaller logo size
            //strukText = "\n\n\n\n\n";
            stream.Write(NewLine, 0, NewLine.Length);
            strukText = "--------------------------------";

            buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

            stream.Write(buffer, 0, buffer.Length);

            stream.Flush();
        }

        private void PrintCheckerReceipt(System.IO.Stream stream, GetStrukCustomerTransaction datas,
            List<CartDetailStrukCustomerTransaction> cartDetails,
            int totalTransactions)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            byte[] InitPrinter = new byte[] { 0x1B, 0x40 }; // Initialize printer
            byte[] NewLine = new byte[] { 0x0A }; // New line

            /*// Set Line Spacing (perintah ESC/POS untuk line spacing, misalnya 30 dots)
            byte[] setLineSpacing = new byte[] { 0x1B, 0x33, 1 }; // Ganti 30 sesuai kebutuhan
            stream.Write(setLineSpacing, 0, setLineSpacing.Length); // Mengirim perintah untuk line spacing*/
            // Write initialization bytes
            stream.Write(InitPrinter, 0, InitPrinter.Length);
            stream.Write(NewLine, 0, NewLine.Length);

            //string strukText = kodeHeksadesimalNormal + "--------------------------------\n";
            string strukText = kodeHeksadesimalNormal;
            strukText += kodeHeksadesimalBold + CenterText("CHECKER No. " + totalTransactions.ToString());
            strukText += kodeHeksadesimalNormal;
            //strukText += "--------------------------------\n";
            strukText += CenterText("Receipt No.: " + datas.data?.receipt_number);
            //strukText += "--------------------------------\n";
            strukText += CenterText(datas.data?.invoice_due_date);
            strukText += "Name: " + datas.data?.customer_name + "\n";

            if (cartDetails.Count != 0)
            {
                var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("ORDER");

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                    if (itemsForServingType.Count == 0) continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    //strukText += "\n";

                    foreach (var cartDetail in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold;
                        strukText += FormatSimpleLine(cartDetail.menu_name, cartDetail.qty.ToString()) + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "   Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "   Note: " + cartDetail.note_item + "\n";
                        strukText += kodeHeksadesimalNormal;

                        //strukText += "\n";
                    }
                }
            }
            strukText += CenterText("Meja No." + datas.data?.customer_seat);
            strukText += "--------------------------------\n\n\n\n\n";

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        private void PrintKitchenOrBarReceipt(System.IO.Stream stream, GetStrukCustomerTransaction datas,
            List<KitchenAndBarCartDetails> CartDetails,
            List<KitchenAndBarCanceledItems> CancelItems,
            int totalTransactions, string type)
        {
            string kodeHeksadesimalBold = "\x1B\x45\x01";
            string kodeHeksadesimalSizeBesar = "\x1D\x21\x01";
            string kodeHeksadesimalNormal = "\x1B\x45\x00" + "\x1D\x21\x00";

            byte[] InitPrinter = new byte[] { 0x1B, 0x40 }; // Initialize printer
            byte[] NewLine = new byte[] { 0x0A }; // New line

            // Write initialization bytes
            stream.Write(InitPrinter, 0, InitPrinter.Length);

            //string strukText = "\n" + kodeHeksadesimalBold + CenterText("No. " + totalTransactions.ToString()) + "\n";
            string strukText = kodeHeksadesimalNormal;
            //strukText += "--------------------------------\n";
            strukText += kodeHeksadesimalBold + CenterText(type.ToUpper() + " No. " + totalTransactions.ToString());
            strukText += CenterText("Receipt No.: " + datas.data?.receipt_number);
            strukText += kodeHeksadesimalNormal;
            //strukText += "--------------------------------\n";
            strukText += CenterText(datas.data?.invoice_due_date);
            strukText += "Name: " + datas.data?.customer_name + "\n";

            if (CartDetails.Count > 0 && CartDetails != null)
            {
                var servingTypes = CartDetails.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("ORDER");

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = CartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                    if (itemsForServingType.Count == 0) continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    strukText += "\n";

                    foreach (var cartDetail in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold;
                        strukText += FormatSimpleLine(cartDetail.menu_name, cartDetail.qty.ToString()) + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.varian))
                            strukText += "   Varian: " + cartDetail.varian + "\n";
                        if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                            strukText += "   Note: " + cartDetail.note_item + "\n";
                        strukText += kodeHeksadesimalNormal;

                        strukText += "\n";
                    }
                }
            }

            if (CancelItems.Count > 0 && CancelItems != null)
            {
                var servingTypes = CancelItems.Select(cd => cd.serving_type_name).Distinct();
                strukText += "--------------------------------\n";
                strukText += CenterText("CANCELED");

                foreach (var servingType in servingTypes)
                {
                    var itemsForServingType = CancelItems.Where(cd => cd.serving_type_name == servingType).ToList();
                    if (itemsForServingType.Count == 0) continue;

                    //strukText += "--------------------------------\n";
                    strukText += CenterText(servingType);
                    strukText += "\n";

                    foreach (var cancelItem in itemsForServingType)
                    {
                        strukText += kodeHeksadesimalSizeBesar + kodeHeksadesimalBold;
                        strukText += FormatSimpleLine(cancelItem.menu_name, cancelItem.qty.ToString()) + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.varian))
                            strukText += "   Varian: " + cancelItem.varian + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                            strukText += "   Note: " + cancelItem.note_item + "\n";
                        if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                            strukText += "   Canceled Reason: " + cancelItem.cancel_reason + "\n";
                        strukText += kodeHeksadesimalNormal;

                        //strukText += "\n";
                    }
                }
            }
            strukText += CenterText("Meja No." + datas.data?.customer_seat + "\n");
            strukText += "--------------------------------\n\n\n\n\n";

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strukText);

            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
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

                System.IO.Stream stream = null;
                if (System.Net.IPAddress.TryParse(printerName, out _))
                {
                    // Connect via LAN
                    var client = new System.Net.Sockets.TcpClient(printerName, 9100);
                    stream = client.GetStream();
                }
                else
                {
                    // Bluetooth connection with retry policy
                    await RetryPolicyAsync(async () =>
                    {
                        BluetoothDeviceInfo printerDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(printerName));
                        if (printerDevice == null) return false;

                        BluetoothClient client = new BluetoothClient();
                        BluetoothEndPoint endpoint = new BluetoothEndPoint(printerDevice.DeviceAddress, BluetoothService.SerialPort);
                        if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000")) return false;

                        client.Connect(endpoint);
                        stream = client.GetStream();
                        return true;
                    }, maxRetries: 3);
                }
                // Struct Customer ====
                if (ShouldPrint(printerId, "Kasir"))
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics;

                            // Mengatur font normal dan tebal
                            Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null) text = string.Empty;
                                if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                Bitmap bmp = new Bitmap(img.Width, img.Height);
                                using (Graphics g = Graphics.FromImage(bmp))
                                {
                                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                    {
                                        new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                                        new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                                        new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                                        new float[] { 0, 0, 0, 1, 0 },
                                        new float[] { 0, 0, 0, 0, 1 }
                                    });
                                    ImageAttributes attributes = new ImageAttributes();
                                    attributes.SetColorMatrix(colorMatrix);
                                    g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attributes);
                                }
                                return bmp;
                            }

                            // Menambahkan logo dan teks "Powered by" di akhir struk
                            void DrawPoweredByLogo(string path)
                            {
                                // Menambahkan jarak sebelum mencetak teks dan logo
                                float spaceBefore = 50; // Jarak dalam pixel, sesuaikan dengan kebutuhan
                                yPos += spaceBefore;

                                // Mengukur teks "Powered by Your Company"
                                string poweredByText = "Powered by Dastrevas";
                                SizeF textSize = e.Graphics.MeasureString(poweredByText, normalFont);

                                // Gambar teks
                                float textX = leftMargin + (printableWidth - textSize.Width) / 2;
                                e.Graphics.DrawString(poweredByText, normalFont, Brushes.Black, textX, yPos);

                                // Sesuaikan yPos untuk logo
                                yPos += textSize.Height;

                                // Menggambar logo
                                Image logoPoweredBy = GetLogoImage(path);
                                float targetWidth = 35; // Ukuran lebar logo dalam pixel, sesuaikan dengan kebutuhan
                                float scaleFactor = targetWidth / logoPoweredBy.Width;
                                float logoHeight = logoPoweredBy.Height * scaleFactor;

                                float logoX = leftMargin + (printableWidth - targetWidth) / 2;
                                e.Graphics.DrawImage(logoPoweredBy, logoX, yPos, targetWidth, logoHeight);

                                spaceBefore = 5;
                                yPos += spaceBefore;
                                // Sesuaikan yPos untuk elemen berikutnya
                                yPos += logoHeight;
                            }
                            void DrawSpace()
                            {
                                yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                            }


                            DrawCenterText("No. " + totalTransactions.ToString(), NomorAntrian);

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
                            float logoX = leftMargin + (printableWidth - logoTargetWidthPx) / 2;

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

                            var servingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();
                            foreach (var servingType in servingTypes)
                            {
                                var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == servingType).ToList();
                                if (itemsForServingType.Count == 0)
                                    continue;
                                DrawSeparator();
                                DrawCenterText(servingType, normalFont);
                                DrawSeparator();
                                foreach (var cartDetail in itemsForServingType)
                                {
                                    DrawSimpleLine(cartDetail.qty.ToString() + " " + cartDetail.menu_name, string.Format("{0:n0}", cartDetail.price));
                                    if (!string.IsNullOrEmpty(cartDetail.varian))
                                        DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                    if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                        DrawLeftText("   Note: " + cartDetail.note_item, normalFont);
                                    if (!string.IsNullOrEmpty(cartDetail.discount_code))
                                        DrawLeftText("   Discount Code: " + cartDetail.discount_code, normalFont);
                                    if (cartDetail.discounted_price.HasValue && cartDetail.discounted_price != 0)
                                        DrawSimpleLine("   Total Discount: ", string.Format("{0:n0}", cartDetail.discounted_price));
                                    DrawSimpleLine("   Total Price:", string.Format("{0:n0}", cartDetail.total_price));
                                    DrawSpace();
                                }
                            }

                            DrawSeparator();
                            DrawSimpleLine("Subtotal", string.Format("{0:n0}", datas.data.subtotal));
                            if (!string.IsNullOrEmpty(datas.data.discount_code))
                                DrawLeftText("Discount Code: " + datas.data.discount_code, normalFont);
                            if (datas.data.discounts_value.HasValue && datas.data.discounts_value != 0)
                                DrawSimpleLine("Discount Value: ", (datas.data.discounts_is_percent.ToString() != "1" ? string.Format("{0:n0}", datas.data.discounts_value.ToString()) : datas.data.discounts_value.ToString() + " %"));
                            DrawSimpleLine("Total", string.Format("{0:n0}", datas.data.total));
                            DrawLeftText("Payment Type: " + datas.data.payment_type, normalFont);
                            DrawSimpleLine("Cash", string.Format("{0:n0}", datas.data.customer_cash));
                            DrawSimpleLine("Change", string.Format("{0:n0}", datas.data.customer_change));
                            DrawSeparator();
                            DrawCenterText(datas.data.outlet_footer, normalFont);
                            DrawCenterText(Kakimu.ToString(), normalFont);
                            DrawPoweredByLogo(poweredByLogoPath);
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);
                        };
                        /*
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
                        */
                    }

                    // Struck Checker
                    if (ShouldPrint(printerId, "Checker"))
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += (sender, e) =>
                        {
                            Graphics graphics = e.Graphics;

                            // Mengatur font normal dan tebal
                            Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                            Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                            Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                            Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                yPos += normalFont.GetHeight(e.Graphics);
                            }

                            // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                            void DrawCenterText(string text, Font font)
                            {
                                if (text == null) text = string.Empty;
                                if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                                string[] words = text.Split(' ');
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                StringBuilder currentLine = new StringBuilder();
                                foreach (string word in words)
                                {
                                    SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                    if (size.Width > printableWidth)
                                    {
                                        // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                            }


                            DrawCenterText("No. " + totalTransactions.ToString(), NomorAntrian);
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
                            var checkerServingTypes = cartDetails.Select(cd => cd.serving_type_name).Distinct();

                            foreach (var checkerServingType in checkerServingTypes)
                            {
                                // Filter cart details by serving_type_name
                                var itemsForServingType = cartDetails.Where(cd => cd.serving_type_name == checkerServingType).ToList();

                                // Skip if there are no items for this serving type
                                if (itemsForServingType.Count == 0)
                                    continue;

                                // Add a section for the serving type
                                DrawSeparator();
                                DrawCenterText(checkerServingType, normalFont);
                                DrawSeparator();

                                // Iterate through items for this serving type
                                foreach (var cartDetail in itemsForServingType)
                                {
                                    DrawSimpleLine(cartDetail.menu_name, cartDetail.qty.ToString());
                                    // Add detail items
                                    if (!string.IsNullOrEmpty(cartDetail.varian))
                                        DrawLeftText("   Varian: " + cartDetail.varian, normalFont);
                                    if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                        DrawLeftText("   Note: " + cartDetail.note_item.ToString(), normalFont);
                                    DrawSpace();
                                }
                            }
                            DrawSeparator();
                            DrawSpace();
                            DrawCenterText(nomorMeja, NomorAntrian);
                        };
                        /*
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
                        */
                    }

                    // Struct Kitchen

                    if (KitchenCartDetails.Any() || KitchenCancelItems.Any())
                    {

                        if (ShouldPrint(printerId, "Makanan"))
                        {
                            PrintDocument printDocument = new PrintDocument();
                            printDocument.PrintPage += (sender, e) =>
                            {
                                Graphics graphics = e.Graphics;

                                // Mengatur font normal dan tebal
                                Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                                Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                                Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                                Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                    e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                    yPos += normalFont.GetHeight(e.Graphics);
                                }

                                // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                                void DrawCenterText(string text, Font font)
                                {
                                    if (text == null) text = string.Empty;
                                    if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                                    string[] words = text.Split(' ');
                                    StringBuilder currentLine = new StringBuilder();
                                    foreach (string word in words)
                                    {
                                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                        if (size.Width > printableWidth)
                                        {
                                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                            SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    StringBuilder currentLine = new StringBuilder();
                                    foreach (string word in words)
                                    {
                                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                        if (size.Width > printableWidth)
                                        {
                                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                    yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                                }


                                DrawCenterText("No. " + totalTransactions.ToString(), NomorAntrian);
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
                                    var servingTypes = KitchenCartDetails.Select(cd => cd.serving_type_name).Distinct();
                                    DrawSeparator();
                                    DrawCenterText("ORDER", boldFont);

                                    foreach (var servingType in servingTypes)
                                    {
                                        var itemsForServingType = KitchenCartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                                        if (itemsForServingType.Count == 0)
                                            continue;

                                        DrawSeparator();
                                        DrawCenterText(servingType, normalFont);

                                        foreach (var cartDetail in itemsForServingType)
                                        {
                                            string qtyMenu = "x" + cartDetail.qty.ToString();
                                            DrawCenterText(qtyMenu + " " + cartDetail.menu_name, BigboldFont);

                                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                                DrawCenterText("Varian: " + cartDetail.varian, BigboldFont);
                                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                                DrawCenterText("Note: " + cartDetail.note_item, BigboldFont);
                                            DrawSpace();
                                        }
                                    }
                                }

                                if (KitchenCancelItems.Count != 0)
                                {
                                    var servingTypes = KitchenCancelItems.Select(cd => cd.serving_type_name).Distinct();
                                    DrawSeparator();
                                    DrawCenterText("CANCELED", boldFont);

                                    foreach (var servingType in servingTypes)
                                    {
                                        var itemsForServingType = KitchenCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                        if (itemsForServingType.Count == 0)
                                            continue;

                                        DrawSeparator();
                                        DrawCenterText(servingType, boldFont);

                                        foreach (var cancelItem in itemsForServingType)
                                        {
                                            string qtyMenu = "x" + cancelItem.qty.ToString();
                                            DrawCenterText(qtyMenu + " " + cancelItem.menu_name, BigboldFont);

                                            if (!string.IsNullOrEmpty(cancelItem.varian))
                                                DrawCenterText("Varian: " + cancelItem.varian, BigboldFont);
                                            if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                                DrawCenterText("Note: " + cancelItem.note_item?.ToString(), BigboldFont);
                                            if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                                DrawCenterText("Canceled Reason: " + cancelItem.cancel_reason, BigboldFont);
                                            DrawSpace();
                                        }
                                    }
                                }
                                DrawSeparator();
                                DrawSpace();
                                DrawCenterText(nomorMeja, NomorAntrian);
                            };
                            /*
                            if (IsBluetoothPrinter(printerName))
                            {
                                printerName = ConvertMacAddressFormat(printerName);
                                PrintViaBluetooth(printerName, printDocument);
                            }
                            if(printerName.Length > 3 && printerName != null && printerName != "" && printerName != " ")
                            {
                                printDocument.PrinterSettings.PrinterName = printerName;
                                printDocument.Print();
                            }
                            */
                        }
                    }

                    // Struct Bar
                    if (BarCartDetails.Any() || BarCancelItems.Any())
                    {
                        if (ShouldPrint(printerId, "Minuman"))
                        {
                            PrintDocument printDocument = new PrintDocument();
                            printDocument.PrintPage += (sender, e) =>
                            {
                                Graphics graphics = e.Graphics;

                                // Mengatur font normal dan tebal
                                Font normalFont = new Font("Arial", 8, FontStyle.Regular); // Ukuran font lebih kecil untuk mencocokkan lebar kertas
                                Font boldFont = new Font("Arial", 8, FontStyle.Bold);
                                Font BigboldFont = new Font("Arial", 10, FontStyle.Bold);
                                Font NomorAntrian = new Font("Arial", 12, FontStyle.Bold);

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
                                    e.Graphics.DrawString(textRight, normalFont, Brushes.Black, leftMargin + printableWidth - sizeRight.Width, yPos);
                                    yPos += normalFont.GetHeight(e.Graphics);
                                }

                                // Fungsi untuk menggambar teks terpusat dengan pemotongan otomatis
                                void DrawCenterText(string text, Font font)
                                {
                                    if (text == null) text = string.Empty;
                                    if (font == null) throw new ArgumentNullException(nameof(font), "Font is null in DrawCenterText method.");
                                    string[] words = text.Split(' ');
                                    StringBuilder currentLine = new StringBuilder();
                                    foreach (string word in words)
                                    {
                                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                        if (size.Width > printableWidth)
                                        {
                                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                            SizeF currentSize = e.Graphics.MeasureString(currentLine.ToString(), font);
                                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                        e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin + (printableWidth - currentSize.Width) / 2, yPos);
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
                                    StringBuilder currentLine = new StringBuilder();
                                    foreach (string word in words)
                                    {
                                        SizeF size = e.Graphics.MeasureString(currentLine.ToString() + word, font);
                                        if (size.Width > printableWidth)
                                        {
                                            // Jika ukuran melebihi lebar, gambar teks yang sudah ada dan reset baris saat ini
                                            e.Graphics.DrawString(currentLine.ToString(), font, Brushes.Black, leftMargin, yPos);
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
                                    yPos += normalFont.GetHeight(e.Graphics); // Menambahkan satu baris spasi berdasarkan tinggi font normal
                                }


                                DrawCenterText("No. " + totalTransactions.ToString(), NomorAntrian);
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
                                    var servingTypes = BarCartDetails.Select(cd => cd.serving_type_name).Distinct();
                                    DrawSeparator();
                                    DrawCenterText("ORDER", boldFont);

                                    foreach (var servingType in servingTypes)
                                    {
                                        var itemsForServingType = BarCartDetails.Where(cd => cd.serving_type_name == servingType).ToList();

                                        if (itemsForServingType.Count == 0)
                                            continue;

                                        DrawSeparator();
                                        DrawCenterText(servingType, normalFont);

                                        foreach (var cartDetail in itemsForServingType)
                                        {
                                            string qtyMenu = "x" + cartDetail.qty.ToString();
                                            DrawCenterText(qtyMenu + " " + cartDetail.menu_name, BigboldFont);

                                            if (!string.IsNullOrEmpty(cartDetail.varian))
                                                DrawCenterText("Varian: " + cartDetail.varian, BigboldFont);
                                            if (!string.IsNullOrEmpty(cartDetail.note_item?.ToString()))
                                                DrawCenterText("Note: " + cartDetail.note_item, BigboldFont);
                                            DrawSpace();
                                        }
                                    }
                                }

                                if (BarCancelItems.Count != 0)
                                {
                                    var servingTypes = BarCancelItems.Select(cd => cd.serving_type_name).Distinct();
                                    DrawSeparator();
                                    DrawCenterText("CANCELED", boldFont);

                                    foreach (var servingType in servingTypes)
                                    {
                                        var itemsForServingType = BarCancelItems.Where(cd => cd.serving_type_name == servingType).ToList();

                                        if (itemsForServingType.Count == 0)
                                            continue;

                                        DrawSeparator();
                                        DrawCenterText(servingType, boldFont);

                                        foreach (var cancelItem in itemsForServingType)
                                        {
                                            string qtyMenu = "x" + cancelItem.qty.ToString();
                                            DrawCenterText(qtyMenu + " " + cancelItem.menu_name, BigboldFont);

                                            if (!string.IsNullOrEmpty(cancelItem.varian))
                                                DrawCenterText("Varian: " + cancelItem.varian, BigboldFont);
                                            if (!string.IsNullOrEmpty(cancelItem.note_item?.ToString()))
                                                DrawCenterText("Note: " + cancelItem.note_item?.ToString(), BigboldFont);
                                            if (!string.IsNullOrEmpty(cancelItem.cancel_reason))
                                                DrawCenterText("Canceled Reason: " + cancelItem.cancel_reason, BigboldFont);
                                            DrawSpace();
                                        }
                                    }
                                }
                                DrawSeparator();
                                DrawSpace();
                                DrawCenterText(nomorMeja, NomorAntrian);
                            };
                            /*
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
                            */

                        }
                    }
                
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        // Retry Policy

        public async Task<bool> RetryPolicyAsync(Func<Task<bool>> action, int maxRetries)
        {
            int attempt = 2;
            Exception lastException = null; // Simpan pengecualian terakhir jika terjadi error

            while (attempt < maxRetries)
            {
                attempt++;
                try
                {
                    // Lakukan action dan cek apakah berhasil
                    if (await action())
                    {
                        return true; // Berhasil pada percobaan attempt
                    }
                }
                catch (Exception ex)
                {
                    // Simpan exception terakhir untuk dilaporkan nanti jika semua percobaan gagal
                    lastException = ex;
                }
            }

            // Jika mencapai percobaan maksimal dan semua percobaan gagal
            Util util = new Util();
            if (lastException != null)
            {
                // Jika gagal karena exception
                util.sendLogTelegramNetworkError($"Semua percobaan ({maxRetries}x) gagal dengan error: {lastException.Message}.");
            }
            else
            {
                // Jika gagal tanpa exception, hanya action() yang mengembalikan false
                util.sendLogTelegramNetworkError($"Semua percobaan ({maxRetries}x) gagal.");
            }

            return false;
        }


        // Method to validate if the string is not a MAC address or IP address
        public bool IsNotMacAddressOrIpAddress(string input)
        {
            return !IsMacAddress(input) && !IsIpAddress(input);
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
