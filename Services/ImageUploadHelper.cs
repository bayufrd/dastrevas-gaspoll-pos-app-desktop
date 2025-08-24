using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KASIR.Helper;

namespace KASIR.Services
{
    public class ImageUploadHelper
    {
        // Daftar ekstensi gambar yang didukung
        private static readonly string[] SupportedImageExtensions =
        {
        ".BMP", ".PNG", ".JPG", ".JPEG", ".GIF", ".TIFF", ".WebP"
    };

        // Validasi ekstensi file
        public bool IsValidImageFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToUpper();
            return SupportedImageExtensions.Contains(extension);
        }

        // Konversi gambar ke BMP dengan kualitas optimal
        public Bitmap ConvertToBmp(string sourcePath)
        {
            try
            {
                using (var originalImage = Image.FromFile(sourcePath))
                {
                    // Buat bitmap baru dengan format piksel yang konsisten
                    Bitmap bmpImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format24bppRgb);

                    using (Graphics graphics = Graphics.FromImage(bmpImage))
                    {
                        graphics.Clear(Color.White); // Background putih jika transparan
                        graphics.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);
                    }

                    return bmpImage;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Gagal mengkonversi gambar: {ex.Message}", ex);
            }
        }

        // Validasi ukuran file
        public bool ValidateFileSize(string filePath, long maxSizeInBytes = 5 * 1024 * 1024) // Default 5MB
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length <= maxSizeInBytes;
        }

        // Resize gambar (opsional)
        public Bitmap ResizeImage(Image imgToResize, Size size)
        {
            try
            {
                Bitmap bmp = new Bitmap(size.Width, size.Height);

                using (Graphics graphic = Graphics.FromImage(bmp))
                {
                    graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphic.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }

                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception("Gagal me-resize gambar", ex);
            }
        }

        // Method utama untuk upload dan konversi
        public void UploadAndSaveImage(PictureBox picThumbnail, string destinationPath)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Filter untuk semua jenis gambar
                openFileDialog.Filter =
                    "Image Files|*.BMP;*.PNG;*.JPG;*.JPEG;*.GIF;*.TIFF;*.WebP|All Files (*.*)|*.*";
                openFileDialog.Title = "Pilih Logo";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;

                    try
                    {
                        // Validasi ekstensi
                        if (!IsValidImageFile(selectedFilePath))
                        {
                            
                            NotifyHelper.Warning("Format gambar tidak didukung.");
                            return;
                        }

                        // Validasi ukuran file
                        if (!ValidateFileSize(selectedFilePath))
                        {
                            NotifyHelper.Warning("Ukuran file terlalu besar. Maksimal 5MB.");
                            return;
                        }

                        // Pastikan direktori tujuan ada
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                        // Hapus file lama jika ada
                        if (File.Exists(destinationPath))
                        {
                            File.Delete(destinationPath);
                        }

                        // Konversi ke BMP
                        using (Bitmap bmpImage = ConvertToBmp(selectedFilePath))
                        {
                            // Simpan sebagai BMP
                            bmpImage.Save(destinationPath, ImageFormat.Bmp);

                            // Tampilkan di PictureBox
                            picThumbnail.Image = new Bitmap(bmpImage);
                        }

                        NotifyHelper.Success("Logo berhasil di-upload dan disimpan.");
                    }
                    catch (Exception ex)
                    {
                        NotifyHelper.Error($"Terjadi kesalahan: {ex.Message}");
                    }
                }
            }
        }
    }
}
