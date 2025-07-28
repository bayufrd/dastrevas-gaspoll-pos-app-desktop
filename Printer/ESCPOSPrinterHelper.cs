using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace KASIR.Printer
{
    /// <summary>
    /// Simple printer helper that works directly with ESC/POS commands
    /// </summary>
    public class ESCPOSPrinterHelper
    {
        // Common ESC/POS Commands
        private static readonly byte[] ESC_INIT = { 0x1B, 0x40 };               // Initialize printer
        private static readonly byte[] ESC_ALIGN_LEFT = { 0x1B, 0x61, 0x00 };   // Left align
        private static readonly byte[] ESC_ALIGN_CENTER = { 0x1B, 0x61, 0x01 }; // Center align
        private static readonly byte[] ESC_ALIGN_RIGHT = { 0x1B, 0x61, 0x02 };  // Right align
        private static readonly byte[] ESC_BOLD_ON = { 0x1B, 0x45, 0x01 };      // Bold on
        private static readonly byte[] ESC_BOLD_OFF = { 0x1B, 0x45, 0x00 };     // Bold off
        private static readonly byte[] ESC_DOUBLE_HEIGHT = { 0x1B, 0x21, 0x10 }; // Double height
        private static readonly byte[] ESC_NORMAL_SIZE = { 0x1D, 0x21, 0x00 };  // Normal size
        private static readonly byte[] ESC_CUT = { 0x1D, 0x56, 0x41, 0x10 };    // Cut paper
        private static readonly byte[] ESC_FEED = { 0x1B, 0x64, 0x03 };         // Feed 3 lines

        // Common separators
        public static readonly string SEPARATOR = "--------------------------------";

        private readonly int _printerWidth;

        /// <summary>
        /// Create a printer helper for a specific printer type
        /// </summary>
        public ESCPOSPrinterHelper(string printerIdentifier)
        {
        }

        /// <summary>
        /// Centers text for the receipt
        /// </summary>
        public byte[] CenterText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<byte>();

            byte[] textBytes = Encoding.UTF8.GetBytes(text + "\n");
            return Combine(ESC_ALIGN_CENTER, textBytes, ESC_ALIGN_LEFT);
        }

        /// <summary>
        /// Format a line with left and right justified content
        /// </summary>
        public byte[] FormatSimpleLine(string left, object right)
        {
            string rightText = right?.ToString() ?? string.Empty;

            // Ensure we have a valid printer width
            // If _printerWidth isn't set, use a standard default (32 for 58mm, 48 for 80mm receipt)
            int lineWidth = _printerWidth > 0 ? _printerWidth : 32;

            // Ensure left text doesn't exceed available space
            // Reserve at least 8 characters for the right value if left is too long
            int maxLeftWidth = lineWidth - rightText.Length - 1;
            int minRightSpace = Math.Min(8, rightText.Length);

            if (maxLeftWidth < minRightSpace)
                maxLeftWidth = lineWidth - minRightSpace - 1;

            string leftTextTrimmed = left;
            if (left.Length > maxLeftWidth)
            {
                leftTextTrimmed = left.Substring(0, maxLeftWidth - 3) + "...";
            }

            // Calculate exact spaces needed to push the right text to the edge
            int spacesNeeded = lineWidth - leftTextTrimmed.Length - rightText.Length;
            spacesNeeded = Math.Max(0, spacesNeeded);

            // Format with proper spacing
            string line = leftTextTrimmed + new string(' ', spacesNeeded) + rightText + "\n";
            return Encoding.UTF8.GetBytes(line);
        }

        /// <summary>
        /// Creates bold text
        /// </summary>
        public byte[] BoldText(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text + "\n");
            return Combine(ESC_BOLD_ON, textBytes, ESC_BOLD_OFF);
        }

        /// <summary>
        /// Creates bold centered text
        /// </summary>
        public byte[] BoldCenterText(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text + "\n");
            return Combine(ESC_ALIGN_CENTER, ESC_BOLD_ON, textBytes, ESC_BOLD_OFF, ESC_ALIGN_LEFT);
        }

        /// <summary>
        /// Creates large text
        /// </summary>
        public byte[] LargeText(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text + "\n");
            return Combine(ESC_DOUBLE_HEIGHT, textBytes, ESC_NORMAL_SIZE);
        }

        /// <summary>
        /// Creates large bold text
        /// </summary>
        public byte[] LargeBoldText(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text + "\n");
            return Combine(ESC_DOUBLE_HEIGHT, ESC_BOLD_ON, textBytes, ESC_BOLD_OFF, ESC_NORMAL_SIZE);
        }

        /// <summary>
        /// Creates large bold centered text
        /// </summary>
        public byte[] LargeBoldCenterText(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text + "\n");
            return Combine(
                ESC_ALIGN_CENTER,
                ESC_DOUBLE_HEIGHT,
                ESC_BOLD_ON,
                textBytes,
                ESC_BOLD_OFF,
                ESC_NORMAL_SIZE,
                ESC_ALIGN_LEFT
            );
        }

        /// <summary>
        /// Prints separator line
        /// </summary>
        public byte[] PrintSeparator()
        {
            return Encoding.UTF8.GetBytes(SEPARATOR + "\n");
        }

        /// <summary>
        /// Creates footer with line feeds and cut command
        /// </summary>
        public byte[] CreateFooter()
        {
            return Combine(ESC_FEED, ESC_CUT);
        }

        /// <summary>
        /// Combine multiple byte arrays
        /// </summary>
        private byte[] Combine(params byte[][] arrays)
        {
            int totalLength = 0;
            foreach (byte[] array in arrays)
            {
                totalLength += array.Length;
            }

            byte[] result = new byte[totalLength];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        /// <summary>
        /// Print a bitmap image
        /// </summary>
        public byte[] PrintImage(Bitmap bitmap)
        {
            // This is a simplified implementation
            // For real bitmap printing, you'd need to implement a proper ESC/POS bitmap converter
            // or use the library's function if available

            // Return initialization for now - replace with actual implementation
            return ESC_INIT;
        }
    }

    /// <summary>
    /// Thermal printer that uses direct stream access
    /// </summary>
    public class ThermalPrinter : IDisposable
    {
        private Stream _stream;
        private bool _isNetworkPrinter;
        private TcpClient _tcpClient;
        private BluetoothClient _bluetoothClient;
        private readonly string _printerAddress;
        private readonly int _port;

        public ThermalPrinter(string printerAddress, int port = 9100)
        {
            _printerAddress = printerAddress;
            _port = port;
            _isNetworkPrinter = IPAddress.TryParse(printerAddress, out _);
        }

        public void Connect()
        {
            if (_stream != null)
                return;

            if (_isNetworkPrinter)
            {
                _tcpClient = new TcpClient(_printerAddress, _port);
                _stream = _tcpClient.GetStream();
            }
            else
            {
                // Try to connect via Bluetooth
                try
                {
                    BluetoothDeviceInfo printerDevice = new(BluetoothAddress.Parse(_printerAddress));
                    _bluetoothClient = new BluetoothClient();
                    BluetoothEndPoint endpoint = new(printerDevice.DeviceAddress, BluetoothService.SerialPort);

                    if (!BluetoothSecurity.PairRequest(printerDevice.DeviceAddress, "0000"))
                        throw new Exception($"Failed to pair with device at {_printerAddress}");

                    _bluetoothClient.Connect(endpoint);
                    _stream = _bluetoothClient.GetStream();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to connect to Bluetooth printer: {ex.Message}", ex);
                }
            }
        }

        public void Write(byte[] data)
        {
            try
            {
                if (_stream == null)
                    Connect();

                _stream.Write(data, 0, data.Length);
                _stream.Flush();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing to printer: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _tcpClient?.Dispose();
            _bluetoothClient?.Dispose();
        }

        public async Task<bool> RetryPolicyAsync(Func<Task<bool>> action, int maxRetries)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                if (await action())
                    return true;

                retryCount++;
                await Task.Delay(500 * retryCount); // Exponential backoff
            }
            return false;
        }
    }
}