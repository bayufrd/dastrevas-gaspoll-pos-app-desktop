using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace KASIR.Komponen
{

    //public partial class PrinterSelectionForm : Form
    //{
    //    private BluetoothPrinterManager _printerManager;
    //    private List<BluetoothDeviceInfo> _availableDevices;

    //    public PrinterSelectionForm()
    //    {
    //        InitializeComponent();
    //        _printerManager = new BluetoothPrinterManager();
    //        InitializePrinterSelection();
    //    }

    //    private async void InitializePrinterSelection()
    //    {
    //        try
    //        {
    //            // Tampilkan loading
    //            lblStatus.Text = "Mencari printer Bluetooth...";
    //            btnConnect.Enabled = false;

    //            // Scan perangkat
    //            _availableDevices = await _printerManager.ScanBluetoothDevices();

    //            // Kosongkan combo box
    //            cboPrinters.Items.Clear();

    //            // Tambahkan nama printer ke ComboBox
    //            foreach (var device in _availableDevices)
    //            {
    //                cboPrinters.Items.Add(new PrinterItem
    //                {
    //                    DeviceName = device.DeviceName,
    //                    DeviceAddress = device.DeviceAddress
    //                });
    //            }

    //            // Update status
    //            lblStatus.Text = $"Ditemukan {_availableDevices.Count} printer";
    //            btnConnect.Enabled = _availableDevices.Count > 0;
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show($"Error scanning devices: {ex.Message}",
    //                "Kesalahan Pencarian",
    //                MessageBoxButtons.OK,
    //                MessageBoxIcon.Error);
    //        }
    //    }

    //    // Custom class untuk item ComboBox
    //    public class PrinterItem
    //    {
    //        public string DeviceName { get; set; }
    //        public BluetoothAddress DeviceAddress { get; set; }

    //        public override string ToString()
    //        {
    //            return DeviceName;
    //        }
    //    }

    //    private async void btnConnect_Click(object sender, EventArgs e)
    //    {
    //        if (cboPrinters.SelectedItem is PrinterItem selectedPrinter)
    //        {
    //            try
    //            {
    //                // Disable tombol selama proses
    //                btnConnect.Enabled = false;
    //                lblStatus.Text = "Menghubungkan...";

    //                // Cari device yang sesuai
    //                var deviceToConnect = _availableDevices
    //                    .FirstOrDefault(d => d.DeviceAddress == selectedPrinter.DeviceAddress);

    //                if (deviceToConnect != null)
    //                {
    //                    bool connected = await _printerManager.ConnectToPrinter(deviceToConnect);

    //                    if (connected)
    //                    {
    //                        MessageBox.Show($"Berhasil terhubung ke {selectedPrinter.DeviceName}",
    //                            "Koneksi Berhasil",
    //                            MessageBoxButtons.OK,
    //                            MessageBoxIcon.Information);

    //                        // Simpan preferensi printer
    //                        SavePrinterPreference(selectedPrinter);
    //                    }
    //                    else
    //                    {
    //                        MessageBox.Show("Gagal terhubung ke printer",
    //                            "Kesalahan Koneksi",
    //                            MessageBoxButtons.OK,
    //                            MessageBoxIcon.Warning);
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                MessageBox.Show($"Error: {ex.Message}",
    //                    "Kesalahan",
    //                    MessageBoxButtons.OK,
    //                    MessageBoxIcon.Error);
    //            }
    //            finally
    //            {
    //                // Aktifkan kembali tombol
    //                btnConnect.Enabled = true;
    //                lblStatus.Text = "Siap";
    //            }
    //        }
    //        else
    //        {
    //            MessageBox.Show("Pilih printer terlebih dahulu",
    //                "Peringatan",
    //                MessageBoxButtons.OK,
    //                MessageBoxIcon.Warning);
    //        }
    //    }

    //    private void SavePrinterPreference(PrinterItem printer)
    //    {
    //        // Simpan preferensi printer di setting aplikasi
    //        Properties.Settings.Default.LastUsedPrinterName = printer.DeviceName;
    //        Properties.Settings.Default.LastUsedPrinterAddress = printer.DeviceAddress.ToString();
    //        Properties.Settings.Default.Save();
    //    }

    //    private void btnRescan_Click(object sender, EventArgs e)
    //    {
    //        InitializePrinterSelection();
    //    }
    //}

    //// Form Designer Code (Pseudocode)
    //partial class PrinterSelectionForm
    //{
    //    private void InitializeComponent()
    //    {
    //        // ComboBox untuk pilih printer
    //        cboPrinters = new ComboBox
    //        {
    //            Location = new Point(50, 50),
    //            Width = 300
    //        };

    //        // Label status
    //        lblStatus = new Label
    //        {
    //            Location = new Point(50, 100),
    //            Width = 300
    //        };

    //        // Tombol Connect
    //        btnConnect = new Button
    //        {
    //            Text = "Hubungkan",
    //            Location = new Point(50, 150)
    //        };
    //        btnConnect.Click += btnConnect_Click;

    //        // Tombol Scan Ulang
    //        btnRescan = new Button
    //        {
    //            Text = "Scan Ulang",
    //            Location = new Point(200, 150)
    //        };
    //        btnRescan.Click += btnRescan_Click;
    //    }
    //}

}
