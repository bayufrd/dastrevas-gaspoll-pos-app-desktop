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
    public partial class SettingsDual : Form
    {
        ApiService apiService = new ApiService();
        private readonly string ID = Properties.Settings.Default.BaseOutlet;
        private readonly string BaseAddress = Properties.Settings.Default.BaseAddress;
        private readonly string API = Properties.Settings.Default.BaseAddressDev;
        private readonly string VersionAddress = Properties.Settings.Default.BaseAddressVersion;
        string PathLogo = "KASIRDualMonitor\\imageDualMonitor";

        public SettingsDual()
        {
            InitializeComponent();
            loadImageDual();
        }
        private async void loadImageDual()
        {
            // Array untuk menampung PictureBox yang akan digunakan
            PictureBox[] pictureBoxes = new PictureBox[] { picThumbnail1, picThumbnail2, picThumbnail3, picThumbnail4, picThumbnail5 };

            // Loop untuk memuat gambar 1 hingga 5
            for (int i = 0; i < 5; i++)
            {
                // Menyeting ukuran dan lokasi untuk PictureBox (untuk thumbnail)
                pictureBoxes[i].Size = new Size(100, 100);  // Ukuran thumbnail
                pictureBoxes[i].SizeMode = PictureBoxSizeMode.Zoom;  // Agar gambar ter-pastikan tidak pecah

                // Membaca gambar dari path yang sesuai (1.jpg, 2.jpg, dll.)
                string imagePath = PathLogo + "\\" + (i + 1) + ".jpg";  // Path gambar berdasarkan index
                if (File.Exists(imagePath))
                {
                    // Membuka file gambar menggunakan FileStream untuk memastikan file dapat diakses
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        pictureBoxes[i].Image = new Bitmap(fs);  // Menampilkan gambar di PictureBox
                    }
                }
                else
                {
                    // Jika gambar tidak ditemukan, tampilkan pesan error
                    MessageBox.Show($"Gambar default {i + 1}.jpg tidak ditemukan.");
                }
            }
            // Menyeting ukuran dan lokasi untuk PictureBox (untuk thumbnail)
            picThumbnailL.Size = new Size(100, 100);  // Ukuran thumbnail
            picThumbnailL.SizeMode = PictureBoxSizeMode.Zoom;  // Agar gambar ter-pastikan tidak pecah

            // Menampilkan gambar default (SambelCowek.bmp) jika tersedia
            string defaultImagePath = PathLogo + "\\6.jpg";  // Sesuaikan dengan path gambar Anda
            if (File.Exists(defaultImagePath))
            {
                // Membuka file gambar menggunakan FileStream untuk memastikan file dapat diakses
                using (FileStream fs = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                {
                    picThumbnailL.Image = new Bitmap(fs);  // Menampilkan gambar di PictureBox
                }
            }
            else
            {
                MessageBox.Show("Gambar default tidak ditemukan.");
            }
            // Menyeting ukuran dan lokasi untuk PictureBox (untuk thumbnail)
            picThumbnailM.Size = new Size(100, 100);  // Ukuran thumbnail
            picThumbnailM.SizeMode = PictureBoxSizeMode.Zoom;  // Agar gambar ter-pastikan tidak pecah

            // Menampilkan gambar default (SambelCowek.bmp) jika tersedia
            defaultImagePath = PathLogo + "\\7.jpg";  // Sesuaikan dengan path gambar Anda
            if (File.Exists(defaultImagePath))
            {
                // Membuka file gambar menggunakan FileStream untuk memastikan file dapat diakses
                using (FileStream fs = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                {
                    picThumbnailM.Image = new Bitmap(fs);  // Menampilkan gambar di PictureBox
                }
            }
            else
            {
                MessageBox.Show("Gambar default tidak ditemukan.");
            }
            // Menyeting ukuran dan lokasi untuk PictureBox (untuk thumbnail)
            picThumbnailR.Size = new Size(100, 100);  // Ukuran thumbnail
            picThumbnailR.SizeMode = PictureBoxSizeMode.Zoom;  // Agar gambar ter-pastikan tidak pecah

            // Menampilkan gambar default (SambelCowek.bmp) jika tersedia
            defaultImagePath = PathLogo + "\\8.jpg";  // Sesuaikan dengan path gambar Anda
            if (File.Exists(defaultImagePath))
            {
                // Membuka file gambar menggunakan FileStream untuk memastikan file dapat diakses
                using (FileStream fs = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                {
                    picThumbnailR.Image = new Bitmap(fs);  // Menampilkan gambar di PictureBox
                }
            }
            else
            {
                MessageBox.Show("Gambar default tidak ditemukan.");
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail1.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo + "\\1.jpg";  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail1.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                }
            }
        }

        private void btnUpload2_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail1.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo + "\\2.jpg";  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail1.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                }
            }
        }

        private void btnUpload3_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail1.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo + "\\3.jpg";  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail1.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                }
            }
        }

        private void btnUpload4_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail1.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo + "\\4.jpg";  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail1.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                }
            }
        }

        private void picThumbnail5_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail1.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo + "\\5.jpg";  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail1.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                }
            }
        }

        private void picThumbnailL_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail1.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo + "\\6.jpg";  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail1.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                }
            }
        }

        private void picThumbnailM_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail1.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo + "\\7.jpg";  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail1.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

                    MessageBox.Show("Logo berhasil di-upload dan disimpan.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat membuka file: " + ex.Message);
                }
            }
        }

        private void picThumbnailR_Click(object sender, EventArgs e)
        {
            // Membuat instance dari OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Logo";

            // Menampilkan OpenFileDialog dan mengecek jika file dipilih
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Mendapatkan path file yang dipilih
                    string selectedFilePath = openFileDialog.FileName;

                    // Menampilkan gambar yang dipilih di PictureBox
                    picThumbnail1.Image = new Bitmap(selectedFilePath); // Menampilkan gambar di PictureBox

                    string destinationPath = PathLogo + "\\8.jpg";  // Lokasi dan nama file tujuan

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);  // Menghapus file lama (jika ada)
                    }

                    File.Copy(selectedFilePath, destinationPath);

                    // Refresh PictureBox dengan gambar yang baru saja disalin
                    picThumbnail1.Image = new Bitmap(destinationPath);  // Muat ulang gambar dari path baru

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




