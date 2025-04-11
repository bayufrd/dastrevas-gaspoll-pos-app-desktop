using System.Xml;
using KASIR.Network;

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
        int status = 0;

        public SettingsConfig()
        {
            InitializeComponent();
            loadConfig();
            loadLogo();
        }

        private void loadLogo()
        {
            string PathLogo = "icon\\OutletLogo.bmp";

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
                    status = 1;
                    lblStatus.Text = "Gambar sukses terload";
                    lblStatus.ForeColor = Color.Green;
                }
            }
            else
            {
                defaultImagePath = "icon\\DT-Logo.bmp";
                lblStatus.Text = "Gambar outlet tidak ditemukan. diubah ke Default";
                lblStatus.ForeColor = Color.Red;
                // Membuka file gambar menggunakan FileStream untuk memastikan file dapat diakses
                using (FileStream fs = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                {
                    picThumbnail.Image = new Bitmap(fs);  // Menampilkan gambar di PictureBox
                    status = 1;
                    lblStatus.Text = "Gambar sukses terload";
                    lblStatus.ForeColor = Color.Green;
                }
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
            lblStatus.ForeColor = Color.LightGreen;

            if (status != 1)
            {
                lblStatus.Text = "Masukkan logo yang valid dahulu.";
                lblStatus.ForeColor = Color.Red;
                return;
            }
            try
            {
                lblStatus.Text = "Menyimpan...";
                //MAIN APP
                var kasirConfigPath = "KASIR.dll.config";
                if (File.Exists(kasirConfigPath))  // Check if the file exists
                {
                    var doc = new XmlDocument();
                    doc.Load(kasirConfigPath);

                    // Check and update BaseOutlet
                    var newID = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseOutlet']/value");
                    if (newID != null)
                    {
                        newID.InnerText = textBoxID.Text.ToString();
                    }

                    // Check and update BaseAddress
                    var newBaseAddress = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseAddress']/value");
                    if (newBaseAddress != null)
                    {
                        newBaseAddress.InnerText = textBoxBaseAddress.Text.ToString();
                    }

                    // Check and update BaseAddressDev
                    var newAPI = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseAddressDev']/value");
                    if (newAPI != null)
                    {
                        newAPI.InnerText = textBoxAPI.Text.ToString();
                    }

                    // Check and update BaseAddressProd
                    var newAPI2 = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseAddressProd']/value");
                    if (newAPI2 != null)
                    {
                        newAPI2.InnerText = textBoxAPI.Text.ToString();
                    }

                    // Check and update BaseAddressVersion
                    var newVersion = doc.SelectSingleNode("//applicationSettings/KASIR.Properties.Settings/setting[@name='BaseAddressVersion']/value");
                    if (newVersion != null)
                    {
                        newVersion.InnerText = textBoxVersion.Text.ToString();
                    }

                    doc.Save(kasirConfigPath);
                }

                // UPDATE APP
                kasirConfigPath = "update\\update.dll.config";
                if (File.Exists(kasirConfigPath))  // Check if the file exists
                {
                    var doc1 = new XmlDocument();
                    doc1.Load(kasirConfigPath);

                    // Check and update BaseAddressVersion in update config
                    var newVersion1 = doc1.SelectSingleNode("//applicationSettings/update.Properties.Settings/setting[@name='BaseAddressVersion']/value");
                    if (newVersion1 != null)
                    {
                        newVersion1.InnerText = textBoxVersion.Text.ToString();
                    }

                    doc1.Save(kasirConfigPath);
                }

                // DUAL MONITOR APP
                kasirConfigPath = "KASIRDualMonitor\\KASIR Dual Monitor.dll.config";
                if (File.Exists(kasirConfigPath))  // Check if the file exists
                {
                    var doc2 = new XmlDocument();
                    doc2.Load(kasirConfigPath);

                    // Check and update BaseOutlet for Dual Monitor
                    var newID2 = doc2.SelectSingleNode("//applicationSettings/KASIR_Dual_Monitor.Properties.Settings/setting[@name='BaseOutlet']/value");
                    if (newID2 != null)
                    {
                        newID2.InnerText = textBoxID.Text.ToString();
                    }

                    // Check and update BaseAddress for Dual Monitor
                    var newBaseAddress2 = doc2.SelectSingleNode("//applicationSettings/KASIR_Dual_Monitor.Properties.Settings/setting[@name='BaseAddress']/value");
                    if (newBaseAddress2 != null)
                    {
                        newBaseAddress2.InnerText = textBoxBaseAddress.Text.ToString();
                    }

                    doc2.Save(kasirConfigPath);
                }

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
                    status = 1;
                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                    status = 0;
                }
            }
        }

        private void textBoxAPI_TextChanged(object sender, EventArgs e)
        {

        }
    }
}




