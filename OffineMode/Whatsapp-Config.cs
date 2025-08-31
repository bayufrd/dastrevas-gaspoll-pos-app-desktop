using KASIR.Helper;
using Newtonsoft.Json.Linq;

namespace KASIR.OffineMode
{
    public partial class Whatsapp_Config : Form
    {
        private readonly HttpClient httpClient;
        private const string QR_API_URL = "http://localhost:1234/get-qr";
        private const string RESET_API_URL = "http://localhost:1234/get-qr";
        private const string CONNECTION_STATUS_URL = "http://localhost:1234/wa-connection-status";

        public Whatsapp_Config()
        {
            InitializeComponent();

            // Inisialisasi HttpClient
            httpClient = new HttpClient();

            // Panggil async method dengan ConfigureAwait
            InitializeFormAsync();
        }

        // Method async untuk inisialisasi
        private async void InitializeFormAsync()
        {
            try
            {
                // Periksa status koneksi terlebih dahulu
                _ = await CheckConnectionStatusAsync();

                // Muat QR Code
                await LoadQRCodeAsync();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal inisialisasi form");
                NotifyHelper.Error("Gagal memuat form: " + ex.Message);
            }
        }

        // Ubah method menjadi async dengan nama yang jelas
        private async Task LoadQRCodeAsync()
        {
            try
            {
                // Tambahkan timestamp untuk menghindari cache
                var response = await httpClient.GetStringAsync($"{QR_API_URL}?t={DateTime.Now.Ticks}");

                // Parse JSON
                var jsonResponse = JObject.Parse(response);

                if (jsonResponse["success"].ToObject<bool>())
                {
                    string base64QR = jsonResponse["qr"].ToString();

                    // Pastikan format base64 lengkap
                    if (!base64QR.Contains("base64,"))
                    {
                        base64QR = "data:image/png;base64," + base64QR;
                    }

                    // Konversi base64 ke gambar
                    byte[] imageBytes = Convert.FromBase64String(base64QR.Split(',')[1]);
                    using (var ms = new System.IO.MemoryStream(imageBytes))
                    {
                        QRPictureBox.Image = Image.FromStream(ms);
                    }

                    NotifyHelper.Success("Silakan scan QR Code");
                    lblStatus.Text = "Silakan scan QR Code";
                    lblStatus.BackColor = Color.Transparent;
                }
                else
                {
                    lblStatus.Text = "Gagal memuat QR: " + jsonResponse["message"];
                    lblStatus.BackColor = Color.Transparent;

                    NotifyHelper.Error("Gagal memuat QR: " + jsonResponse["message"]);
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal memuat QR Code");
                NotifyHelper.Error("Error: " + ex.Message);
            }
        }

        // Event handler untuk load status
        private async void btnLoadStatus_Click(object sender, EventArgs e)
        {

        }

        // Method untuk update UI berdasarkan status
        private void UpdateUIBasedOnStatus(ConnectionStatus status)
        {
            if (status.Connected)
            {
                lblStatus.Text = $"Status: \n{status.Status}";
                lblStatus.BackColor = Color.Transparent;
                lblStatus.ForeColor = Color.Black;
            }
            else
            {
                lblStatus.Text = $"Status: \n{status.Status}";
                lblStatus.BackColor = Color.Transparent;
                lblStatus.ForeColor = Color.Black;
            }

            // Update QR availability
            if (status.QRAvailable)
            {
                btnResetQR.Enabled = true;
                btnResetQR.Text = "Muat Ulang QR";
            }
            else
            {
                btnResetQR.Text = "QR Tidak Tersedia\nReset?";
            }
        }

        // Event handler untuk reset QR
        private async void btnResetQR_Click(object sender, EventArgs e)
        {
            try
            {
                // Nonaktifkan tombol selama proses reset untuk mencegah multi-klik  
                btnResetQR.Enabled = false;

                // Kirim permintaan reset ke API  
                var apiResponse = await httpClient.GetStringAsync($"{RESET_API_URL}");

                // Parse respons JSON  
                var jsonResponse = JObject.Parse(apiResponse);

                // Periksa status respons  
                if (jsonResponse["status"]?.ToString() == "success")
                {
                    // Log keberhasilan reset  
                    LoggerUtil.LogWarning("QR Code berhasil direset");

                    // Muat ulang QR Code  
                    await LoadQRCodeAsync();

                    // Tampilkan notifikasi sukses  
                    NotifyHelper.Success(jsonResponse["message"]?.ToString() ?? "QR Code berhasil direset");
                }
                else
                {
                    // Tangani respons tidak sukses  
                    string errorMessage = jsonResponse["message"]?.ToString() ?? "Sukses mereset QR Code";
                    LoggerUtil.LogWarning(errorMessage);
                    NotifyHelper.Error(errorMessage);
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Tangani kesalahan koneksi jaringan  
                LoggerUtil.LogError(httpEx, "Kesalahan jaringan saat mereset QR");
                NotifyHelper.Error($"Kesalahan jaringan: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                // Tangani kesalahan umum  
                LoggerUtil.LogError(ex, "Sukses mereset QR");
                NotifyHelper.Error($"Sukses mereset QR: {ex.Message}");
            }
            finally
            {
                // Pastikan tombol dapat diklik kembali  
                btnResetQR.Enabled = true;
            }
        }

        // Tutup form
        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Gunakan HttpClient statis atau buat factory method
        private static readonly HttpClient sharedHttpClient;

        // Inisialisasi statis dengan konfigurasi default
        static Whatsapp_Config()
        {
            sharedHttpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10) // Set timeout saat inisialisasi
            };
        }

        // Method publik untuk mengecek status koneksi
        public async Task<ConnectionStatus> CheckConnectionStatusAsync()
        {
            try
            {
                // Gunakan HttpClient statis
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                // Kirim request GET dengan token pembatalan
                var response = await sharedHttpClient.GetStringAsync(
                    $"{CONNECTION_STATUS_URL}?t={DateTime.Now.Ticks}",
                    cancellationTokenSource.Token
                );

                // Parse JSON response
                var jsonResponse = JObject.Parse(response);

                // Update dan simpan status koneksi
                CurrentConnectionStatus = new ConnectionStatus
                {
                    Connected = jsonResponse["connected"].ToObject<bool>(),
                    Status = jsonResponse["status"].ToString(),
                    QRAvailable = jsonResponse["qrAvailable"].ToObject<bool>()
                };

                // Log status untuk debugging
                LoggerUtil.LogWarning($"Status Koneksi: {CurrentConnectionStatus}");

                return CurrentConnectionStatus;
            }
            catch (OperationCanceledException)
            {
                // Tangani timeout
                LoggerUtil.LogWarning("Waktu tunggu habis saat mengambil status koneksi");

                return new ConnectionStatus
                {
                    Connected = false,
                    Status = "Timeout: Gagal mendapatkan status",
                    QRAvailable = false
                };
            }
            catch (HttpRequestException httpEx)
            {
                // Tangani kesalahan koneksi HTTP
                LoggerUtil.LogError(httpEx, "Kesalahan HTTP saat mengambil status koneksi");

                return new ConnectionStatus
                {
                    Connected = false,
                    Status = "Tidak Terhubung (Jaringan Error)",
                    QRAvailable = false
                };
            }
            catch (Exception ex)
            {
                // Tangani kesalahan umum
                LoggerUtil.LogError(ex, "Kesalahan saat mengambil status koneksi");

                return new ConnectionStatus
                {
                    Connected = false,
                    Status = "Tidak Terhubung (Error)",
                    QRAvailable = false
                };
            }
        }

        // Public property untuk menyimpan status koneksi
        public ConnectionStatus CurrentConnectionStatus { get; private set; }

        // Kelas publik untuk status koneksi
        public class ConnectionStatus
        {
            public bool Connected { get; set; }
            public string Status { get; set; }
            public bool QRAvailable { get; set; }

            // Konstruktor default
            public ConnectionStatus()
            {
                Connected = false;
                Status = "Belum diinisialisasi";
                QRAvailable = false;
            }

            // Method untuk mengecek apakah koneksi valid
            public bool IsValidConnection()
            {
                return Connected && !string.IsNullOrEmpty(Status);
            }

            // Override ToString untuk debugging
            public override string ToString()
            {
                return $"Connected: {Connected}, Status: {Status}, QR Available: {QRAvailable}";
            }
        }

        private async void btnReload_Click(object sender, EventArgs e)
        {
            try
            {
                // Panggil method check status
                var status = await CheckConnectionStatusAsync();

                // Update UI berdasarkan status
                UpdateUIBasedOnStatus(status);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal memeriksa status");
                NotifyHelper.Error("Gagal memeriksa status: " + ex.Message);
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                // Panggil method check status
                var status = await CheckConnectionStatusAsync();

                // Update UI berdasarkan status
                UpdateUIBasedOnStatus(status);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal memeriksa status");
                NotifyHelper.Error("Gagal memeriksa status: " + ex.Message);
            }
        }
    }
}