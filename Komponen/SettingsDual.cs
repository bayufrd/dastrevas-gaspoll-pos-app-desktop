using KASIR.Network;
using KASIR.Properties;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KASIR.Helper;

namespace KASIR.Komponen
{
    public partial class SettingsDual : Form
    {
        private readonly string PathLogo = "KASIRDualMonitor\\imageDualMonitor";

        public SettingsDual()
        {
            InitializeComponent();
            EnsureDirectoryExists(PathLogo);
            LoadImages();
        }

        private void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private async void LoadImages()
        {
            PictureBox[] pictureBoxes = { picThumbnail1, picThumbnail2, picThumbnail3, picThumbnail4, picThumbnail5, picThumbnailL, picThumbnailM, picThumbnailR };
            string[] imageNames = { "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg", "6.jpg", "7.jpg", "8.jpg" };

            for (int i = 0; i < imageNames.Length; i++)
            {
                if (File.Exists($"{PathLogo}\\{imageNames[i]}"))
                {
                    LoadImageIntoPictureBox(pictureBoxes[i], $"{PathLogo}\\{imageNames[i]}");
                }
                else
                {
                    NotifyHelper.Error($"Gambar default {imageNames[i]} tidak ditemukan.");
                }
            }
        }

        private void LoadImageIntoPictureBox(PictureBox pictureBox, string imagePath)
        {
            using (FileStream fs = new(imagePath, FileMode.Open, FileAccess.Read))
            {
                pictureBox.Size = new Size(100, 100);
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Image = new Bitmap(fs);
            }
        }

        private void UploadImage(PictureBox pictureBox, string imageName)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Bitmap Files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            openFileDialog.Title = "Pilih Foto";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string selectedFilePath = openFileDialog.FileName;
                    string destinationPath = Path.Combine(PathLogo, imageName);

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }

                    File.Copy(selectedFilePath, destinationPath);
                    pictureBox.Image = new Bitmap(destinationPath);
                }
                catch (Exception ex)
                {
                    NotifyHelper.Error($"Terjadi kesalahan saat membuka file: {ex.Message}");
                }
            }
        }
        // Event Handlers for uploading images
        private void btnUpload_Click(object sender, EventArgs e) => UploadImage(picThumbnail1, "1.jpg");
        private void btnUpload2_Click(object sender, EventArgs e) => UploadImage(picThumbnail2, "2.jpg");
        private void btnUpload3_Click(object sender, EventArgs e) => UploadImage(picThumbnail3, "3.jpg");
        private void btnUpload4_Click(object sender, EventArgs e) => UploadImage(picThumbnail4, "4.jpg");
        private void btnUpload5_Click(object sender, EventArgs e) => UploadImage(picThumbnail5, "5.jpg");
        private void btnUploadL_Click(object sender, EventArgs e) => UploadImage(picThumbnailL, "6.jpg");
        private void btnUploadM_Click(object sender, EventArgs e) => UploadImage(picThumbnailM, "7.jpg");
        private void btnUploadR_Click(object sender, EventArgs e) => UploadImage(picThumbnailR, "8.jpg");
        private void Button1_Click(object sender, EventArgs e) => Close();
        private void Button2_Click(object sender, EventArgs e) => Close();
    }
}