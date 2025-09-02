using KASIR.Helper;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;

namespace KASIR.OffineMode
{
    public partial class Whatsapp_Config : Form
    {
        private readonly HttpClient httpClient;
        private string QR_API_URL = "http://localhost:1234/get-qr";
        private string RESET_API_URL = "http://localhost:1234/reset";
        private string CONNECTION_STATUS_URL = "http://localhost:1234/wa-connection-status";
        private string oldUrl = Properties.Settings.Default.BaseAddressProd.ToString();
        private string BASEURL = "";
        private RichTextBox logRichTextBox;

        public Whatsapp_Config()
        {
            InitializeComponent();
            InitializeLoggerMsg();

            // Inisialisasi HttpClient
            httpClient = new HttpClient();

            BASEURL = RemoveApiPrefix(oldUrl);
            QR_API_URL = $"{BASEURL}/get-qr";
            RESET_API_URL = $"{BASEURL}/reset";
            CONNECTION_STATUS_URL = $"{BASEURL}/wa-connection-status";

            // Panggil async method dengan ConfigureAwait
            InitializeFormAsync();
        }

        private void InitializeLoggerMsg()
        {
            // Hapus kode sebelumnya untuk Label

            logRichTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            LoggerMsg.Controls.Add(logRichTextBox);
        }

        /// <summary>
        /// Menghapus prefix API dari URL
        /// </summary>
        private string RemoveApiPrefix(string url)
        {
            return url.Contains("api.") ? url.Replace("api.", "whatsapp.") : url;
        }

        /// <summary>
        /// Proses update aplikasi
        /// </summary>
        // Method async untuk inisialisasi
        private async void InitializeFormAsync()
        {
            try
            {
                FetchLogsFromApi();
                // Panggil method check status
                var status = await CheckConnectionStatusAsync();

                // Update UI berdasarkan status
                UpdateUIBasedOnStatus(status);

                // Muat QR Code
                await LoadQRCodeAsync();

            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal inisialisasi form");
                NotifyHelper.Error("Gagal memuat form: " + ex.Message);
            }
        }
        private async Task FetchLogsFromApi()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"{BASEURL}/logs");
                    var logContent = await response.Content.ReadAsStringAsync();

                    // Parse JSON
                    var logs = JsonConvert.DeserializeObject<List<string>>(logContent);

                    // Filter dan format logs
                    var formattedLogs = FormatLogs(logs);

                    // Pastikan update UI di main thread
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate {
                            UpdateLogDisplay(formattedLogs);
                        });
                    }
                    else
                    {
                        UpdateLogDisplay(formattedLogs);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Gagal mengambil logs: {ex.Message}");
            }
        }

        private List<string> FormatLogs(List<string> rawLogs)
        {
            var formattedLogs = new List<string>();
            var currentGroup = new List<string>();

            foreach (var log in rawLogs.Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                // Kelompokkan log berdasarkan blok QR Code atau pesan penting
                if (log.Contains("===== QR CODE ====="))
                {
                    // Simpan kelompok sebelumnya jika ada
                    if (currentGroup.Any())
                    {
                        formattedLogs.AddRange(currentGroup);
                        currentGroup.Clear();
                    }

                    // Tambahkan header QR Code
                    formattedLogs.Add("🔍 [QR CODE]");
                }
                else if (log.Contains("Caranya:"))
                {
                    // Abaikan baris "Caranya:"
                    continue;
                }
                else if (log.Contains("Scan QR Code di bawah ini:"))
                {
                    // Abaikan
                    continue;
                }
                else if (log.Contains("QR Code Generated"))
                {
                    formattedLogs.Add("🟢 QR Code Baru Dibuat");
                }
                else if (log.Contains("Koneksi terputus"))
                {
                    formattedLogs.Add("🔴 " + log.Substring(24).Trim());
                }
                else
                {
                    // Parsing timestamp
                    if (DateTime.TryParse(log.Substring(0, 24), out DateTime timestamp))
                    {
                        string message = log.Substring(24).Trim();
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            formattedLogs.Add($"[{timestamp:HH:mm:ss}] {message}");
                        }
                    }
                    else
                    {
                        // Jika tidak bisa parsing timestamp, tambahkan langsung
                        formattedLogs.Add(log);
                    }
                }
            }

            return formattedLogs;
        }

        private void UpdateLogDisplay(List<string> logs)
        {
            // Gabungkan logs dengan enter
            string logText = string.Join(Environment.NewLine, logs);

            logRichTextBox.Text = logText;

            // Scroll ke bawah
            logRichTextBox.SelectionStart = logRichTextBox.Text.Length;
            logRichTextBox.ScrollToCaret();
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
                }
            }
            catch (Exception ex)
            {
                //NotifyHelper.Error("Error: " + ex.Message);
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
                lblStatus.Text = $"Status: \nConnected";
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
                btnResetQR.Text = "Connected, Relogin?";
            }
        }

        // Event handler untuk reset QR
        private async void btnResetQR_Click(object sender, EventArgs e)
        {
            try
            {
                // Nonaktifkan tombol selama proses reset untuk mencegah multi-klik  
                btnResetQR.Enabled = false;
                NotifyHelper.Success($"Memanggil URL: {RESET_API_URL}");

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
                NotifyHelper.Success($"Sukses mereset QR: {ex.Message}");
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

                return CurrentConnectionStatus;
            }
            catch (OperationCanceledException)
            {
                return new ConnectionStatus
                {
                    Connected = false,
                    Status = "Timeout: Gagal mendapatkan status",
                    QRAvailable = false
                };
            }
            catch (HttpRequestException httpEx)
            {
                //LoggerUtil.LogError(httpEx, "Kesalahan HTTP saat mengambil status koneksi");

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
                //LoggerUtil.LogError(ex, "Kesalahan saat mengambil status koneksi");

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
                LoadQRCodeAsync();
                FetchLogsFromApi();
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
                FetchLogsFromApi();
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
        private string strukMessage = "Test from POS";

        private async Task SendWhatsAppReceiptIfEligible(string phoneNumber)
        {
            try
            {
                // Cek apakah koneksi WhatsApp tersedia dan terhubung
                var connectionStatus = await CheckConnectionStatusAsync();

                string qrCodePath = FindQRCodePath();
                if (IsValidPhoneNumber(phoneNumber))
                {
                    //await SendWhatsAppMessage(phoneNumber, strukMessage);
                    await SendWhatsAppMessageWithAttachment(phoneNumber, strukMessage, qrCodePath);
                }

                return;
            }
            catch (Exception ex)
            {
                SendWhatsAppMessage(phoneNumber, strukMessage);
                // Log error tanpa menghentikan proses utama
                LoggerUtil.LogError(ex, "Gagal mengirim struk via WhatsApp: {ErrorMessage}", ex.Message);
            }
        }

        // Metode validasi nomor telepon
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Bersihkan nomor dari karakter non-digit
            string cleanedNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Validasi panjang dan awalan
            return cleanedNumber.Length >= 10 &&
                   (cleanedNumber.StartsWith("62") ||
                    cleanedNumber.StartsWith("0"));
        }

        // Metode pencarian file QR Code
        private string FindQRCodePath()
        {
            // Daftar lokasi kemungkinan file QR Code
            string[] possiblePaths = new[]
            {
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon", "QRcode.bmp"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRcode.bmp"),
        Path.Combine(Application.StartupPath, "icon", "QRcode.bmp"),
        Path.Combine(Application.StartupPath, "QRcode.bmp"),
        "QRcode.bmp"
    };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    LoggerUtil.LogWarning($"QR Code ditemukan di: {path}");
                    return path;
                }
            }

            LoggerUtil.LogWarning("Tidak dapat menemukan file QR Code");
            return null;
        }
        private async Task SendWhatsAppMessageWithAttachment(string phoneNumber, string message, string attachmentPath)
        {
            try
            {
                // Validasi file lampiran
                if (!File.Exists(attachmentPath))
                {
                    LoggerUtil.LogWarning($"File lampiran tidak ditemukan: {attachmentPath}");
                    return;
                }

                // Bersihkan nomor telepon dari karakter non-digit
                phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

                // Tambahkan kode negara jika tidak ada
                if (!phoneNumber.StartsWith("62"))
                {
                    phoneNumber = phoneNumber.StartsWith("0")
                        ? "62" + phoneNumber.Substring(1)
                        : "62" + phoneNumber;
                }

                // Konversi gambar ke base64 di compress

                string compressedImagePath = CompressImage(attachmentPath);


                byte[] imageBytes = await File.ReadAllBytesAsync(compressedImagePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                // Siapkan data untuk dikirim
                var requestData = new Dictionary<string, string>
                {
                    ["nomor"] = phoneNumber,
                    ["pesan"] = $"*Form untuk Kritik dan Saran*\n\n{message}",
                    ["lampiran"] = base64Image
                };

                // Kirim pesan via API
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                // Logging sebelum mengirim
                LoggerUtil.LogWarning($"Mencoba mengirim pesan ke {phoneNumber}");
                LoggerUtil.LogWarning($"Panjang lampiran: {base64Image.Length} karakter");

                var response = await httpClient.PostAsync(
                    $"{BASEURL}/kirim-lampiran",
                    content,
                    cancellationTokenSource.Token
                );

                // Periksa response
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    LoggerUtil.LogWarning($"Struk berhasil dikirim ke {phoneNumber}");
                    LoggerUtil.LogWarning($"Detail Pengiriman: {responseObject}");
                }
                else
                {
                    LoggerUtil.LogWarning($"Gagal mengirim struk ke {phoneNumber}");
                    LoggerUtil.LogWarning($"Error Response: {responseContent}");
                }
            }
            catch (OperationCanceledException)
            {
                LoggerUtil.LogWarning("Waktu tunggu habis saat mengirim pesan");
            }
            catch (ArgumentException ex)
            {
                LoggerUtil.LogError(ex, $"Kesalahan parameter: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                LoggerUtil.LogError(ex, $"Kesalahan jaringan saat mengirim pesan ke {phoneNumber}");
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Kesalahan saat mengirim pesan ke {phoneNumber}");

                // Tambahkan detail error untuk debugging
                LoggerUtil.LogError(ex, $"Detail Error: {ex.GetType().Name} - {ex.Message}");
                LoggerUtil.LogError(ex, $"Stack Trace: {ex.StackTrace}");
            }
        }

        private string CompressImage(string inputPath)
        {
            try
            {
                string outputPath = Path.Combine(
                    Path.GetTempPath(),
                    $"compressed_{Guid.NewGuid()}.jpg"
                );

                using (var image = Image.FromFile(inputPath))
                {
                    // Kompresi lebih agresif
                    int maxWidth = 400;   // Kurangi ukuran
                    int maxHeight = 300;  // Kurangi ukuran

                    float ratioX = (float)maxWidth / image.Width;
                    float ratioY = (float)maxHeight / image.Height;
                    float ratio = Math.Min(ratioX, ratioY);

                    int newWidth = (int)(image.Width * ratio);
                    int newHeight = (int)(image.Height * ratio);

                    using (var resizedImage = new Bitmap(newWidth, newHeight))
                    {
                        using (var graphics = Graphics.FromImage(resizedImage))
                        {
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                        }

                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(
                            System.Drawing.Imaging.Encoder.Quality,
                            (long)30  // Turunkan kualitas
                        );

                        var codec = GetEncoderInfo("image/jpeg");
                        resizedImage.Save(outputPath, codec, encoderParameters);
                    }
                }

                LoggerUtil.LogWarning($"Gambar dikompres: {outputPath}");
                return outputPath;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal mengkompresi gambar");
                return inputPath;
            }
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            return ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(t => t.MimeType == mimeType);
        }
        private async Task SendWhatsAppMessage(string phoneNumber, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    LoggerUtil.LogWarning("Nomor telepon kosong");
                    return;
                }

                // Bersihkan nomor telepon dari karakter non-digit
                phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

                // Tambahkan kode negara jika tidak ada
                if (!phoneNumber.StartsWith("62"))
                {
                    phoneNumber = phoneNumber.StartsWith("0")
                        ? "62" + phoneNumber.Substring(1)
                        : "62" + phoneNumber;
                }

                using (var httpClient = new HttpClient())
                {
                    // 🔥 Gunakan JSON, bukan FormUrlEncoded
                    var payload = new
                    {
                        nomor = phoneNumber,
                        pesan = message
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync($"{BASEURL}/kirim-pesan", content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        LoggerUtil.LogWarning($"Struk berhasil dikirim ke {phoneNumber}");
                        LoggerUtil.LogWarning($"Response: {responseContent}");
                    }
                    else
                    {
                        LoggerUtil.LogWarning($"Gagal mengirim struk ke {phoneNumber}");
                        LoggerUtil.LogWarning($"Error Response: {responseContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Kesalahan saat mengirim pesan ke {phoneNumber}");
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                SendWhatsAppReceiptIfEligible(numberTesting.Text);
            }
            catch(Exception ex)
            {
                NotifyHelper.Error(ex.Message);
            }
        }
    }
}