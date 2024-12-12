using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Menu = KASIR.Model.Menu;
using Image = System.Drawing.Image;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace KASIR.Komponen
{
    public partial class SettingsConfig : Form
    {
        ApiService apiService = new ApiService();
        private readonly string ID = Properties.Settings.Default.BaseOutlet;
        private readonly string BaseAddress = Properties.Settings.Default.BaseAddress;
        private readonly string API = Properties.Settings.Default.BaseAddressDev;
        private readonly string VersionAddress = Properties.Settings.Default.BaseAddressVersion;
        string PathLogo = "icon\\OutletLogo.bmp";

        public SettingsConfig()
        {
            InitializeComponent();
            loadConfig();
            // Menyeting ukuran dan lokasi untuk PictureBox (untuk thumbnail)
            picThumbnail.Size = new Size(100, 100);  // Ukuran thumbnail
            picThumbnail.SizeMode = PictureBoxSizeMode.Zoom;  // Agar gambar ter-pastikan tidak pecah

            // Menampilkan gambar default (SambelCowek.bmp) jika tersedia
            string defaultImagePath = PathLogo;  // Sesuaikan dengan path gambar Anda
            if (File.Exists(defaultImagePath))
            {
                // Membuka file gambar menggunakan FileStream untuk memastikan file dapat diakses
                using (FileStream fs = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                {
                    picThumbnail.Image = new Bitmap(fs);  // Menampilkan gambar di PictureBox
                }
            }
            else
            {
                MessageBox.Show("Gambar default tidak ditemukan.");
            }
        }

        private async void loadConfig()
        {
            textBoxID.Text = ID.ToString();
            textBoxBaseAddress.Text = BaseAddress.ToString();
            textBoxAPI.Text = API.ToString();
            textBoxVersion.Text = VersionAddress.ToString();
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                //MAIN APP
                var kasirConfigPath = "KASIR.dll.config";
                var doc = new XmlDocument();
                doc.Load(kasirConfigPath);

                var newID = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseOutlet']/value");
                newID.InnerText = textBoxID.Text.ToString();

                var newBaseAddress = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseAddress']/value");
                newBaseAddress.InnerText = textBoxBaseAddress.Text.ToString();

                var newAPI = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseAddressDev']/value");
                newAPI.InnerText = textBoxAPI.Text.ToString();

                var newAPI2 = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseAddressProd']/value");
                newAPI2.InnerText = textBoxAPI.Text.ToString();

                var newVersion = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseAddressVersion']/value");
                newVersion.InnerText = textBoxVersion.Text.ToString();

                doc.Save(kasirConfigPath);

                //UPDATE APP
                kasirConfigPath = "update\\update.dll.config";
                var doc1 = new XmlDocument();
                doc1.Load(kasirConfigPath);

                var newVersion1 = doc1.SelectSingleNode("//applicationSettings/update.Properties.Settings/setting[@name='BaseAddressVersion']/value");
                newVersion1.InnerText = textBoxVersion.Text.ToString();

                doc1.Save(kasirConfigPath);

                //DUAL MONITOR APP
                kasirConfigPath = "KASIRDualMonitor\\KASIR Dual Monitor.dll.config";
                var doc2 = new XmlDocument();
                doc2.Load(kasirConfigPath);

                var newID2 = doc2.SelectSingleNode("//applicationSettings/KASIR_Dual_Monitor.Properties.Settings/setting[@name='BaseOutlet']/value");
                newID2.InnerText = textBoxID.Text.ToString();

                var newBaseAddress2 = doc2.SelectSingleNode("//applicationSettings/KASIR_Dual_Monitor.Properties.Settings/setting[@name='BaseAddress']/value");
                newBaseAddress2.InnerText = textBoxBaseAddress.Text.ToString();

                doc2.Save(kasirConfigPath);

                MessageBox.Show("Berhasil Di Ubah, aplikasi akan dijalankan ulang");
                Application.Restart();
                Environment.Exit(0);

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo;  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                }
            }
        }
    }
}




