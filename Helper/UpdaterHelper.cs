
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using KASIR.Services;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace KASIR.Helper
{
    public class UpdaterHelper
    {
        private readonly IInternetService _internetService;
        private string _versionUpdaterApp;
        private string _pathKasir;

        public UpdaterHelper(IInternetService internetService)
        {
            _internetService = internetService;
        }
        public async Task<string> CheckVersionNewAppAsync()
        {
            try
            {
                // Langkah 1: Validasi Koneksi Internet
                if (!_internetService.IsInternetConnected())
                {
                    return "1.0.0.0";
                }

                using (HttpClient httpClient = new())
                {
                    string oldUrl = Properties.Settings.Default.BaseAddressProd.ToString();
                    string urlVersion = RemoveApiPrefix(oldUrl);

                    // Gunakan await untuk mendapatkan versi
                    string newVersion = await httpClient.GetStringAsync($"{urlVersion}/server/version.txt");

                    // Bersihkan dan kembalikan versi
                    return newVersion.Trim();
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Kesalahan umum dalam proses update");
                return "1.0.0.0"; // Nilai default jika terjadi kesalahan
            }
        }
        public async Task CheckLastUpdaterAppAsync()
        {
            try
            {
                if (!_internetService.IsInternetConnected())
                {
                    return;
                }

                string directoryPath = "update";
                string filePath = Path.Combine(directoryPath, "versionUpdater.txt");

                // Pastikan direktori ada
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Buat file versi default jika tidak ada
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "1.0.0.1");
                }

                _versionUpdaterApp = File.ReadAllText(filePath).Trim();

                await DownloadUpdaterAppAsync();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Kesalahan saat memeriksa pembaruan: {ErrorMessage}", ex.Message);
            }
        }

        private async Task DownloadUpdaterAppAsync()
        {
            try
            {

                using (var httpClient = new HttpClient())
                {
                    // URL untuk memeriksa versi - sesuaikan dengan kebutuhan Anda
                    string oldUrl = Properties.Settings.Default.BaseAddressProd.ToString();
                    string urlVersion = RemoveApiPrefix(oldUrl);
                    
                    string newVersion = await httpClient.GetStringAsync(urlVersion + "/server/updaterVersion.txt");

                    if (string.IsNullOrWhiteSpace(newVersion))
                    {
                        throw new Exception("Data versi kosong.");
                    }

                    newVersion = newVersion.Trim();

                    // Bandingkan versi
                    if (IsNewVersionAvailable(newVersion, _versionUpdaterApp))
                    {
                        // URL file unduhan - sesuaikan dengan kebutuhan Anda
                        string fileUrl = urlVersion + "/server/Dastrevas.zip";
                        string destinationPath = Path.Combine("update" , "Dastrevas.rar");

                        await DownloadFileAsync(httpClient, fileUrl, destinationPath);
                        await ExtractAndUpdateAsync(destinationPath, newVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Kesalahan saat mengunduh pembaruan: {ErrorMessage}", ex.Message);
            }
        }

        private string RemoveApiPrefix(string url)
        {
            return url.Contains("api.") ? url.Replace("api.", "") : url;
        }

        private bool IsNewVersionAvailable(string newVersion, string currentVersion)
        {
            try
            {
                var newParts = newVersion.Split('.').Select(int.Parse).ToArray();
                var currentParts = currentVersion.Split('.').Select(int.Parse).ToArray();

                for (int i = 0; i < Math.Min(newParts.Length, currentParts.Length); i++)
                {
                    if (newParts[i] > currentParts[i]) return true;
                    if (newParts[i] < currentParts[i]) return false;
                }

                return newParts.Length > currentParts.Length;
            }
            catch
            {
                return false;
            }
        }

        private async Task DownloadFileAsync(HttpClient httpClient, string fileUrl, string destinationPath)
        {
            try
            {
                // Pastikan direktori tujuan ada
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                // Unduh file
                byte[] fileData = await httpClient.GetByteArrayAsync(fileUrl);
                await File.WriteAllBytesAsync(destinationPath, fileData);

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Kesalahan pengunduhan: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task ExtractAndUpdateAsync(string rarFilePath, string newVersion)
        {
            try
            {
                string extractDirectory = Path.Combine(_pathKasir ?? Application.StartupPath, "update");
                Directory.CreateDirectory(extractDirectory);

                // Ekstrak arsip
                using (var archive = ArchiveFactory.Open(rarFilePath))
                {
                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entry.WriteToDirectory(extractDirectory, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }

                // Perbarui file versi
                string versionFilePath = Path.Combine(extractDirectory, "versionUpdater.txt");
                await File.WriteAllTextAsync(versionFilePath, newVersion);

                // Tunggu sebentar sebelum membuka updater
                await Task.Delay(3000);
                await OpenUpdaterExeAsync();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal mengekstrak: {ErrorMessage}", ex.Message);
            }
        }

        private async Task OpenUpdaterExeAsync()
        {
            try
            {
                string updatePath = Path.Combine(
                    Application.StartupPath,
                    "update",
                    "update.exe"
                );

                if (!File.Exists(updatePath))
                {
                    LoggerUtil.LogWarning($"File pembaruan tidak ditemukan: {updatePath}");
                    return;
                }

                if (!_internetService.IsInternetConnected())
                {
                    LoggerUtil.LogWarning("Tidak ada koneksi internet");
                    return;
                }

                ProcessStartInfo startInfo = new()
                {
                    FileName = updatePath,
                    Arguments = "",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);

                await Task.Delay(1000);
                Application.Exit();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal membuka pembarui");
            }
        }
    }
}
