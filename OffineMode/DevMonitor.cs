using System.Data;
using System.Diagnostics;
using KASIR.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Button = System.Windows.Forms.Button;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Label = System.Windows.Forms.Label;
using Panel = System.Windows.Forms.Panel;
using TextBox = System.Windows.Forms.TextBox;
using UserControl = System.Windows.Forms.UserControl;

namespace KASIR.OfflineMode
{
    public partial class DevMonitor : UserControl
    {
        private ApiService apiService;
        private readonly string baseOutlet;

        // Komponen UI untuk menampilkan loading progress
        private Panel loadingPanel;
        private Label loadingLabel;
        private ProgressBar progressBar;
        private CancellationTokenSource cts;
        private int currentProgress = 0;

        public DevMonitor()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            apiService = new ApiService();
            InitializeLoadingUI();

            // Pastikan untuk membersihkan resource saat form ditutup
            this.Disposed += (s, e) =>
            {
                cts?.Cancel();
                cts?.Dispose();
            };

            LoadData();
        }
        // Timer untuk mengukur waktu loading
        private Stopwatch loadingTimer;

        // Label untuk menampilkan waktu loading dan ukuran data
        private Label timerLabel;
        private Label dataSizeLabel;

        private void InitializeLoadingUI()
        {
            try
            {
                // Modifikasi existing loading panel setup
                loadingPanel = new Panel
                {
                    BackColor = Color.FromArgb(30, 31, 68), // Dark blue background
                    BorderStyle = BorderStyle.Fixed3D,
                    Size = new Size(400, 180), // Increased size to accommodate new labels
                    Location = new Point((this.ClientSize.Width - 400) / 2, (this.ClientSize.Height - 200) / 2),
                    Visible = false
                };

                // Label untuk loading text
                loadingLabel = new Label
                {
                    Text = "Mengambil data dari server...",
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = Color.White,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(380, 30),
                    Location = new Point(10, 20)
                };

                // Label untuk timer
                timerLabel = new Label
                {
                    Text = "Waktu: 0.00 detik",
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.White,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(380, 25),
                    Location = new Point(10, 55)
                };

                // Label untuk ukuran data
                dataSizeLabel = new Label
                {
                    Text = "Ukuran data: 0.00 MB",
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.White,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(380, 25),
                    Location = new Point(10, 85)
                };

                // Progress bar
                progressBar = new ProgressBar
                {
                    Style = ProgressBarStyle.Continuous,
                    Size = new Size(380, 25),
                    Location = new Point(10, 120),
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0
                };

                // Add components to panel
                loadingPanel.Controls.Add(loadingLabel);
                loadingPanel.Controls.Add(timerLabel);
                loadingPanel.Controls.Add(dataSizeLabel);
                loadingPanel.Controls.Add(progressBar);

                // Add panel to form
                this.Controls.Add(loadingPanel);
                loadingPanel.BringToFront();

                // Initialize stopwatch
                loadingTimer = new Stopwatch();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error initializing loading UI: {ErrorMessage}", ex.Message);
            }
        }

        private void ShowLoading()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowLoading));
                return;
            }

            loadingPanel.Visible = true;
            progressBar.Value = 0;
            loadingLabel.Text = "Mengambil data dari server...";
        }

        private void HideLoading()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HideLoading));
                return;
            }

            loadingPanel.Visible = false;
        }

        private void UpdateProgress(int percentage, string message = null)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int, string>(UpdateProgress), percentage, message);
                return;
            }

            if (percentage >= 0 && percentage <= 100)
            {
                progressBar.Value = percentage;
            }

            if (!string.IsNullOrEmpty(message))
            {
                loadingLabel.Text = message;
            }
        }

        private void StartLoadingTimer()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(StartLoadingTimer));
                return;
            }

            // Reset dan mulai timer
            loadingTimer.Restart();
        }

        private void StopLoadingTimer(int dataLength)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(StopLoadingTimer), dataLength);
                return;
            }

            // Hentikan timer
            loadingTimer.Stop();

            // Hitung waktu dalam detik
            double seconds = loadingTimer.ElapsedMilliseconds / 1000.0;

            // Hitung ukuran data dalam MB
            double dataSizeMB = dataLength / (1024.0 * 1024.0);

            // Update label-label
            timerLabel.Text = $"Waktu: {seconds:F2} detik";
            dataSizeLabel.Text = $"Ukuran data: {dataSizeMB:F2} MB";
        }

        private async Task LoadData()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            cts = new CancellationTokenSource();
            var token = cts.Token;

            try
            {
                // Hapus kontrol retry button jika ada
                RemoveRetryControls();

                // Tampilkan loading dan mulai timer
                ShowLoading();
                StartLoadingTimer();
                UpdateProgress(10, "Menghubungi server...");

                // Menggunakan Task.Run untuk simulasi progress
                currentProgress = 10;
                var progressTask = Task.Run(async () =>
                {
                    while (currentProgress < 90 && !token.IsCancellationRequested)
                    {
                        await Task.Delay(500, token);
                        currentProgress += 5;
                        UpdateProgress(currentProgress, $"Mengambil data... ({currentProgress}%)");
                    }
                }, token);

                // Panggil API untuk mendapatkan data
                IApiService apiService = new ApiService();
                string response = await apiService.GetActiveCart("/update-confirm");

                // Hentikan timer dan update UI
                StopLoadingTimer(response.Length);
                // Log response untuk debugging
                Console.WriteLine(response);

                // Update progress setelah response diterima
                UpdateProgress(95, "Memproses data...");

                if (string.IsNullOrEmpty(response) || response.StartsWith("<"))
                {
                    HideLoading();
                    MessageBox.Show("API response is not valid JSON.");
                    CleanFormAndAddRetryButton("API response is not valid JSON.");
                    return;
                }

                await Task.Delay(300);
                UpdateProgress(98, "Menampilkan data...");

                // Mengonversi response JSON menjadi objek JObject
                var jsonResponse = JObject.Parse(response);

                // Ambil data dari field "data"
                var jsonData = jsonResponse["data"];

                if (jsonData == null || !jsonData.HasValues)
                {
                    HideLoading();
                    MessageBox.Show("No data found in API response.");
                    CleanFormAndAddRetryButton("No data found in API response.");
                    return;
                }

                await Task.Delay(200);
                UpdateProgress(100, "Selesai!");

                // Setelah delay singkat, sembunyikan loading
                await Task.Delay(300);

                panelConfirm.Controls.Clear();
                panelConfirm.AutoScroll = true; // Membuat panel scrollable

                int totalWidth = panelConfirm.ClientSize.Width;

                // Iterasi melalui setiap item dalam jsonData
                foreach (var outlet in jsonData)
                {
                    // Membuat panel untuk setiap outlet
                    Panel itemPanel = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 120, // Menambahkan sedikit tinggi untuk label
                    };

                    // Label untuk nama outlet
                    Label nameLabel = new Label
                    {
                        Text = outlet["outlet_name"]?.ToString(),
                        Width = totalWidth,
                        ForeColor = Color.Black,
                        TextAlign = ContentAlignment.MiddleLeft,
                    };

                    // Label untuk versi
                    Label versionLabel = new Label
                    {
                        Text = "Version: " + outlet["version"]?.ToString(),
                        Width = totalWidth,
                        ForeColor = Color.Black,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Top = 30,
                    };

                    // Label untuk versi baru
                    Label newVersionLabel = new Label
                    {
                        Text = "New Version: " + outlet["new_version"]?.ToString()?.Trim(),
                        Width = totalWidth,
                        TextAlign = ContentAlignment.MiddleLeft,
                        ForeColor = Color.Black,
                        Top = 50,
                    };

                    // Label untuk waktu pembaruan
                    Label lastUpdatedLabel = new Label
                    {
                        Text = "Last Updated: " + outlet["last_updated"]?.ToString(),
                        Width = totalWidth,
                        TextAlign = ContentAlignment.MiddleLeft,
                        ForeColor = Color.Black,
                        Top = 70,
                    };

                    // Menambahkan label dan panel ke itemPanel
                    itemPanel.Controls.Add(nameLabel);
                    itemPanel.Controls.Add(versionLabel);
                    itemPanel.Controls.Add(newVersionLabel);
                    itemPanel.Controls.Add(lastUpdatedLabel);

                    // Menambahkan itemPanel ke panelConfirm
                    panelConfirm.Controls.Add(itemPanel);
                }
                await LoadDataComplaint();
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "TaskCanceledException: {ErrorMessage}", ex.Message);
                HideLoading();
                CleanFormAndAddRetryButton("Koneksi terputus atau timeout. Detail: " + ex.Message);
            }
            catch (JsonReaderException ex)
            {
                LoggerUtil.LogError(ex, "JsonReaderException: {ErrorMessage}", ex.Message);
                HideLoading();
                CleanFormAndAddRetryButton("Error parsing JSON: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Hentikan timer jika terjadi error
                StopLoadingTimer(0);

                LoggerUtil.LogError(ex, "Unexpected error: {ErrorMessage}", ex.Message);
                HideLoading();
                CleanFormAndAddRetryButton("Terjadi kesalahan: " + ex.Message);
            }
        }
        // Perbarui LoadDataComplaint untuk menggunakan metode baru
        private async Task LoadDataComplaint()
        {
            try
            {
                // Pastikan token masih valid sebelum memulai
                if (cts.Token.IsCancellationRequested)
                    return;

                // Update progress untuk API komplain
                currentProgress = 10;
                UpdateProgress(currentProgress, "Mengambil data komplain...");

                // Restart timer untuk mengukur waktu loading
                if (loadingTimer == null)
                    loadingTimer = new Stopwatch();
                loadingTimer.Restart();

                // Task untuk simulasi progress API kedua
                var progressTask = Task.Run(async () =>
                {
                    while (currentProgress < 90 && !cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(300, cts.Token);
                        currentProgress += 5;
                        UpdateProgress(currentProgress, $"Mengambil data komplain... ({currentProgress}%)");
                    }
                }, cts.Token);

                // Panggil API untuk mendapatkan data komplain dengan ukuran
                var (response, dataSize) = await GetActiveCartWithSize("/complaint");

                // Log raw response size for debugging
                Console.WriteLine($"Raw Response Length: {response?.Length ?? 0}, Data Size: {dataSize:F4} MB");

                // Hentikan timer dan dapatkan ukuran data
                if (this.InvokeRequired)
                {
                    // Jika metode dipanggil dari thread yang berbeda, gunakan Invoke
                    this.Invoke(new Action(() => UpdateTimerAndDataSize(response)));
                }
                else
                {
                    // Jika sudah di UI thread, panggil langsung
                    UpdateTimerAndDataSize(response);
                }


                UpdateProgress(95, "Memproses data komplain...");

                if (string.IsNullOrEmpty(response) || response.StartsWith("<"))
                {
                    HideLoading();
                    MessageBox.Show("API response is not valid JSON.");
                    CleanFormAndAddRetryButton("API response is not valid JSON.");
                    return;
                }

                // Mengonversi response JSON menjadi objek JObject
                var jsonResponse = JObject.Parse(response);

                UpdateProgress(98, "Menampilkan data komplain...");

                // Ambil data dari field "data"
                var jsonData = jsonResponse["data"];

                if (jsonData == null || !jsonData.HasValues)
                {
                    HideLoading();
                    MessageBox.Show("No data found in API response.");
                    CleanFormAndAddRetryButton("No data found in API response.");
                    return;
                }

                await Task.Delay(200);
                UpdateProgress(100, "Selesai!");
                await Task.Delay(300);
                HideLoading();

                // Bersihkan dan persiapkan panel untuk menampilkan data
                panelComplaint.Controls.Clear();
                panelComplaint.AutoScroll = true;

                // Use FlowLayoutPanel for automatic control arrangement
                FlowLayoutPanel flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false
                };

                panelComplaint.Controls.Add(flowPanel);

                int totalWidth = panelComplaint.ClientSize.Width;

                // Iterasi melalui setiap item dalam jsonData
                foreach (var outlet in jsonData)
                {
                    // Membuat button untuk setiap outlet
                    System.Windows.Forms.Button outletNameButton = new System.Windows.Forms.Button
                    {
                        Text = outlet["name"]?.ToString() + " || " + outlet["sent_at"]?.ToString(),
                        Width = totalWidth,
                        ForeColor = Color.Black,
                        Height = 40,
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    outletNameButton.Click += (sender, e) =>
                    {
                        // Open a new form with details when clicked
                        ShowOutletDetailsForm(outlet);
                    };

                    // Menambahkan button ke FlowLayoutPanel (flowPanel)
                    flowPanel.Controls.Add(outletNameButton);
                }
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "TaskCanceledException in LoadDataComplaint: {ErrorMessage}", ex.Message);
                HideLoading();
                UpdateTimerAndDataSize(null); // Reset timer jika terjadi cancel
                CleanFormAndAddRetryButton("Koneksi terputus atau timeout saat memuat data complaint. Detail: " + ex.Message);
            }
            catch (JsonReaderException ex)
            {
                LoggerUtil.LogError(ex, "JsonReaderException in LoadDataComplaint: {ErrorMessage}", ex.Message);
                HideLoading();
                UpdateTimerAndDataSize(null); // Reset timer jika terjadi error parsing
                CleanFormAndAddRetryButton("Error parsing JSON in complaint data: " + ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Unexpected error in LoadDataComplaint: {ErrorMessage}", ex.Message);
                HideLoading();
                UpdateTimerAndDataSize(null); // Reset timer jika terjadi error
                CleanFormAndAddRetryButton("Terjadi kesalahan saat memuat data complaint: " + ex.Message);
            }
        }

        private void UpdateTimerAndDataSize(string response)
        {
            // Pastikan method dipanggil di UI thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateTimerAndDataSize), response);
                return;
            }

            // Hentikan timer
            loadingTimer.Stop();

            // Hitung waktu dalam detik
            double seconds = loadingTimer.ElapsedMilliseconds / 1000.0;

            // Hitung ukuran data dalam MB
            double dataSizeMB = 0;
            if (response != null)
            {
                // Konversi string ke byte array untuk mendapatkan ukuran yang akurat
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(response);
                dataSizeMB = byteArray.Length / (1024.0 * 1024.0);
            }

            // Coba alternatif lain jika masih 0
            if (dataSizeMB == 0)
            {
                try
                {
                    // Jika response adalah string JSON, coba parse dan dapatkan ukuran
                    if (!string.IsNullOrEmpty(response))
                    {
                        var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(response);
                        string jsonString = jsonObject.ToString(Newtonsoft.Json.Formatting.None);
                        byte[] jsonByteArray = System.Text.Encoding.UTF8.GetBytes(jsonString);
                        dataSizeMB = jsonByteArray.Length / (1024.0 * 1024.0);
                    }
                }
                catch (Exception ex)
                {
                    // Log error jika parsing gagal
                    Console.WriteLine($"Error parsing JSON for size calculation: {ex.Message}");
                }
            }

            // Update label-label dengan format yang lebih presisi
            timerLabel.Text = $"Waktu: {seconds:F2} detik";
            dataSizeLabel.Text = $"Ukuran data: {dataSizeMB:F4} MB";

            // Debug logging dengan informasi tambahan
            Console.WriteLine($"Debug - Response Length: {response?.Length ?? 0}, " +
                              $"Byte Array Size: {dataSizeMB:F4} MB, " +
                              $"Raw Response: {response?.Substring(0, Math.Min(response?.Length ?? 0, 100))}...");
        }

        // Tambahkan metode bantuan untuk mendapatkan ukuran data dari response HTTP
        private double GetResponseDataSize(HttpResponseMessage response)
        {
            try
            {
                // Coba dapatkan Content-Length header
                var contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue)
                {
                    return contentLength.Value / (1024.0 * 1024.0);
                }

                // Jika Content-Length tidak tersedia, baca konten
                var content = response.Content.ReadAsByteArrayAsync().Result;
                return content.Length / (1024.0 * 1024.0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting response size: {ex.Message}");
                return 0;
            }
        }

        // Modifikasi metode API untuk mengembalikan ukuran data
        private async Task<(string response, double dataSize)> GetActiveCartWithSize(string endpoint)
        {
            IApiService apiService = new ApiService();
            string response = await apiService.GetActiveCart(endpoint);

            // Hitung ukuran data
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(response);
            double dataSizeMB = byteArray.Length / (1024.0 * 1024.0);

            return (response, dataSizeMB);
        }
        private void ShowOutletDetailsForm(JToken outlet)
        {
            Form detailsForm = new Form
            {
                Text = outlet["name"]?.ToString(),
                Width = 600,  // Mengatur lebar form ke lebar layar
                Height = 600,  // Mengatur tinggi form ke tinggi layar
                StartPosition = FormStartPosition.CenterScreen,  // Menampilkan form di tengah layar
                MaximizeBox = false,  // Menonaktifkan tombol maximize untuk menghindari form lebih besar dari layar
                FormBorderStyle = FormBorderStyle.FixedDialog  // Mengatur border agar tidak bisa diubah-ubah ukuran
            };

            // Create labels and textboxes for detailed data
            int totalWidth = detailsForm.ClientSize.Width;

            Label titleLabel = new Label
            {
                Text = "Title: " + outlet["title"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 20
            };

            Label messageLabel = new Label
            {
                Text = "Message: " + outlet["message"]?.ToString()?.Trim(),
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 60
            };

            Label sentAtLabel = new Label
            {
                Text = "Sent at: " + outlet["sent_at"]?.ToString()?.Trim(),
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 100
            };

            // Labels and Textboxes for log_last_outlet and cache transaction fields
            Label log_last_outlet = new Label
            {
                Text = "Log Last Outlet:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 140
            };
            string jsonString = outlet["log_last_outlet"]?.ToString();

            // Pastikan tidak ada literal \r dan \n
            if (jsonString != null)
            {
                // Mengganti escape sequences (\\n, \\r) dengan string kosong atau newline yang benar
                jsonString = jsonString.Replace("\\n", Environment.NewLine); // Hapus literal \n
                jsonString = jsonString.Replace("\\r", ""); // Hapus literal \r
                jsonString = jsonString.Replace("\r", "");  // Hapus karakter \r yang tidak terlihat
                jsonString = jsonString.Replace("\n", Environment.NewLine);  // Ganti \n dengan newline sistem
            }
            TextBox logLastOutlet = new TextBox
            {
                Text = jsonString?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 160
            };

            Label cache_transaction = new Label
            {
                Text = "Cache Transaction:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 220
            };
            TextBox cacheTransaction = new TextBox
            {
                Text = outlet["cache_transaction"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 240
            };

            Label cache_failed_transaction = new Label
            {
                Text = "Cache Failed Transaction:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 300
            };
            TextBox cacheFailedTransaction = new TextBox
            {
                Text = outlet["cache_failed_transaction"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 320
            };

            Label cache_success_transaction = new Label
            {
                Text = "Cache Success Transaction:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 380
            };
            TextBox cacheSuccessTransaction = new TextBox
            {
                Text = outlet["cache_success_transaction"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 400
            };

            Label cache_history_transaction = new Label
            {
                Text = "Cache History Transaction:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 460
            };
            TextBox cacheHistoryTransaction = new TextBox
            {
                Text = outlet["cache_history_transaction"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 480
            };

            // Add controls to the details form
            detailsForm.Controls.Add(titleLabel);
            detailsForm.Controls.Add(messageLabel);
            detailsForm.Controls.Add(sentAtLabel);
            detailsForm.Controls.Add(log_last_outlet);
            detailsForm.Controls.Add(logLastOutlet);
            detailsForm.Controls.Add(cache_transaction);
            detailsForm.Controls.Add(cacheTransaction);
            detailsForm.Controls.Add(cache_failed_transaction);
            detailsForm.Controls.Add(cacheFailedTransaction);
            detailsForm.Controls.Add(cache_success_transaction);
            detailsForm.Controls.Add(cacheSuccessTransaction);
            detailsForm.Controls.Add(cache_history_transaction);
            detailsForm.Controls.Add(cacheHistoryTransaction);

            // Show the details form
            detailsForm.ShowDialog();
        }
        // Method untuk menambahkan tombol retry ke form
        private void CleanFormAndAddRetryButton(string errorMessage)
        {
            // Bersihkan panel
            panelConfirm.Controls.Clear();
            panelComplaint.Controls.Clear();

            // Tambahkan PictureBox untuk gambar
            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = "ErrorImg";
            pictureBox.Size = new Size(100, 100); // Sesuaikan ukuran gambar
            pictureBox.Location = new Point((this.ClientSize.Width - pictureBox.Width) / 2, (this.ClientSize.Height - pictureBox.Height) / 2 - 110); // Posisi di atas tombol
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Atur ukuran gambar agar sesuai dengan PictureBox

            try
            {
                using (FileStream fs = new FileStream("icon\\OutletLogo.bmp", FileMode.Open, FileAccess.Read))
                {
                    pictureBox.Image = Image.FromStream(fs);
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error loading logo image: {ErrorMessage}", ex.Message);
                // Jika gagal memuat gambar, jangan tampilkan PictureBox
                pictureBox = null;
            }

            // Tambahkan tombol retry
            Button btnRetry = new Button();
            btnRetry.Name = "btnRetry";
            btnRetry.Text = "Retry Load Data\nOut of Service";
            btnRetry.Size = new Size(190, 60);
            btnRetry.Location = new Point((this.ClientSize.Width - btnRetry.Width) / 2, (this.ClientSize.Height - btnRetry.Height) / 2);
            btnRetry.BackColor = Color.FromArgb(30, 31, 68);
            btnRetry.FlatStyle = FlatStyle.Flat;
            btnRetry.Font = new Font("Arial", 10, FontStyle.Bold);
            btnRetry.ForeColor = Color.White; // Mengatur warna teks tombol menjadi putih
            btnRetry.Click += new EventHandler(BtnRetry_Click);

            try
            {
                // Membuat sudut membulat
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, 20, 20, 180, 90);
                path.AddArc(btnRetry.Width - 20, 0, 20, 20, 270, 90);
                path.AddArc(btnRetry.Width - 20, btnRetry.Height - 20, 20, 20, 0, 90);
                path.AddArc(0, btnRetry.Height - 20, 20, 20, 90, 90);
                path.CloseFigure();
                btnRetry.Region = new Region(path);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error creating rounded button: {ErrorMessage}", ex.Message);
                // Jika gagal membuat sudut membulat, lanjutkan tanpa efek visual
            }

            // Menambahkan label di bawah tombol
            Label lblInfo = new Label();
            lblInfo.Name = "ErrorMsg";
            lblInfo.Text = errorMessage; // Teks yang ingin ditampilkan
            lblInfo.Font = new Font("Arial", 9, FontStyle.Regular);
            lblInfo.ForeColor = Color.Black; // Warna teks label
            lblInfo.AutoSize = true; // Agar label menyesuaikan ukuran teks
            lblInfo.MaximumSize = new Size(this.ClientSize.Width - 40, 0); // Batasi lebar teks

            // Mengatur posisi label agar berada di tengah
            lblInfo.Location = new Point((this.ClientSize.Width - lblInfo.Width) / 2, btnRetry.Bottom + 10); // Posisi di bawah tombol

            // Menambahkan kontrol ke form
            if (pictureBox != null)
            {
                this.Controls.Add(pictureBox); // Menambahkan PictureBox ke form
            }

            this.Controls.Add(btnRetry);
            this.Controls.Add(lblInfo); // Menambahkan label ke form

            btnRetry.BringToFront();
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            // Hapus retry UI
            RemoveRetryControls();

            // Muat data kembali
            _ = LoadData();
        }

        private void RemoveRetryControls()
        {
            // Hapus tombol retry
            var btnRetry = this.Controls.OfType<Button>().FirstOrDefault(btn => btn.Name == "btnRetry");
            if (btnRetry != null)
            {
                this.Controls.Remove(btnRetry);
            }

            // Hapus label informasi jika ada
            var lblInfo = this.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "ErrorMsg");
            if (lblInfo != null)
            {
                this.Controls.Remove(lblInfo);
            }

            // Hapus gambar error jika ada
            var errImg = this.Controls.OfType<PictureBox>().FirstOrDefault(pic => pic.Name == "ErrorImg");
            if (errImg != null)
            {
                this.Controls.Remove(errImg);
            }
        }
    }
}