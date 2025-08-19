using System.Globalization;
using KASIR.Helper;
using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.Properties;
using Newtonsoft.Json;
using Serilog;

namespace KASIR.OffineMode
{
    public partial class Offline_Complaint : Form
    {
         

        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        private readonly string baseOutlet;
        private readonly string BaseOutletName;
        private readonly string Version;

        public Offline_Complaint()
        {
            baseOutlet = Settings.Default.BaseOutlet;
            BaseOutletName = Settings.Default.BaseOutletName;
            Version = Settings.Default.Version;

            InitializeComponent();
            DisplayLogInPanel(LoggerMsg);
            txtNotes.PlaceholderText = "Deskripsi Masalah?";
            txtNotes.PlaceholderText += "\nKronologi : ";
            txtNotes.PlaceholderText += "\nSaat Melakukan/Mengakses tombol : ";
            txtNotes.PlaceholderText += "\nKesalahan yang terjadi : ";
            txtNotes.PlaceholderText += "\nSeharusnya yang terjadi : ";
            txtNotes.PlaceholderText +=
                "\n\nMohon maaf atas ketidak nyamanannya dan \nTerimakasih, dengan support ini akan membantu agar lebih maju \ndan berkembang kedepannya. \n\nGaspoll Management. x Dastrevas";
        }
        public async void DisplayLogInPanel(Panel logggerMsg)
        {
            try
            {
                string content = await ReadingLogAsync(); // ambil dulu konten

                // Pisah per baris
                string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // Filter hanya baris yang bukan stack trace / path file
                var filtered = lines
                    .Where(l => !l.TrimStart().StartsWith("at ")) // buang stack trace
                    .Where(l => !l.Contains(":line "))            // buang info line number
                    .Where(l => !l.Contains(@":\"))               // buang path Windows (C:\..)
                    .Where(l => !l.StartsWith("--- End of stack trace")) // buang end trace
                    .ToArray();

                // Batasi 5000 baris terakhir
                int maxLines = 5000;
                if (filtered.Length > maxLines)
                    filtered = filtered.Skip(filtered.Length - maxLines).ToArray();

                // Gabungkan dengan extra newline supaya ada jarak antar log
                string finalContent = string.Join(Environment.NewLine + Environment.NewLine, filtered);

                LoggerMsg.Controls.Clear();

                var box = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Text = finalContent,
                    BackColor = Color.White,
                    ForeColor = Color.Black,  // opsional
                    Font = new Font("Courier New", 10) // opsional biar rapih
                };

                LoggerMsg.Controls.Add(box);

                // Auto scroll ke bawah setelah isi dimasukkan
                box.SelectionStart = box.Text.Length;
                box.ScrollToCaret();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error(ex.Message);
            }
        }


        private async Task<string> ReadingLogAsync()
        {
            try
            {
                string outletName = "unknown";
                string outletID = Settings.Default.BaseOutlet;
                string cacheOutlet = $"DT-Cache\\DataOutlet{outletID}.data";
                if (File.Exists(cacheOutlet))
                {
                    string cacheData = await File.ReadAllTextAsync(cacheOutlet);
                    CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheData);

                    if (dataOutlet?.data != null)
                    {
                        outletName = dataOutlet.data.name;
                    }
                }

                string todayDate = DateTime.Now.ToString("yyyyMMdd");
                string customText = $"{outletID}_{outletName}_log{todayDate}";
                string logFilePath = $"log\\{customText}.txt";

                if (!File.Exists(logFilePath))
                {
                    return string.Empty;
                }

                using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string content = await streamReader.ReadToEndAsync();
                    return content;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        public bool ReloadDataInBaseForm { get; private set; }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
            Close();
        }

        private void AddItem(string name, string amount)
        {
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
        }

        private void panel13_Paint(object sender, PaintEventArgs e)
        {
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            btnSimpan.Enabled = false;
            try
            {
                if (lblNama.Text == null || lblNama.Text == "")
                {
                    MessageBox.Show("Format nama kurang tepat", "Gaspol", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (txtNotes.Text == null || txtNotes.ToString() == "")
                {
                    MessageBox.Show("Format notes kurang tepat", "Gaspol", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string NameKasir = lblNama.Text;
                string messagesKasir = txtNotes.Text;
                SendingComplaint(NameKasir, messagesKasir);
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                btnSimpan.Enabled = true;
            }
            finally
            {
                btnSimpan.Enabled = true;
            }
        }

        public async void SendingComplaint(string NameKasir, string messagesKasir)
        {
            try
            {
                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");

                // Deserialize JSON ke object CartDataCache
                CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                string BaseOutletNameID = dataOutlet?.data?.name ?? "";

                string formatDate = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

                // Mengubah string formatDate ke DateTime
                DateTime date = DateTime.ParseExact(formatDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                // Mengurangi satu hari
                DateTime previousDay = date.AddDays(-1);

                // Mengonversi kembali ke format string
                string previousDayString = previousDay.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

                // Folder untuk cache dan log
                string FolderCache = "DT-Cache\\Transaction";
                string FolderLogCacheData = $"log\\{baseOutlet}_{BaseOutletName}_log{formatDate}.txt";

                // Path untuk berbagai file log cache
                string FolderLogCache_transaction = Path.Combine(FolderCache, "transaction.data");
                string FolderLogCache_failed_transaction = Path.Combine(FolderCache, "FailedSyncTransaction",
                    $"{baseOutlet}_SyncFailed_{formatDate}.data");
                string FolderLogCache_success_transaction = Path.Combine(FolderCache, "SyncSuccessTransaction",
                    $"{baseOutlet}_SyncSuccess_{formatDate}.data");
                string FolderLogCache_history_transaction = Path.Combine(FolderCache, "HistoryTransaction",
                    $"History_transaction_DT-{baseOutlet}_{previousDayString}.data");

                // Membaca isi file log cache atau mengembalikan "{}" jika file tidak ada

                string LogCacheData =
                    ReadFileContentAsPlainText(FolderLogCacheData) != "{}"
                        ? ReadFileContentAsPlainText(FolderLogCacheData)
                        : ReadFileContentAsPlainText($"log\\{baseOutlet}_unknown_log{formatDate}.txt");
                string Cache_transaction = ReadFileContentWithRetry(FolderLogCache_transaction);
                string Cache_failed_transaction = ReadFileContentWithRetry(FolderLogCache_failed_transaction);
                string Cache_success_transaction = ReadFileContentWithRetry(FolderLogCache_success_transaction);
                string Cache_history_transaction = ReadFileContentWithRetry(FolderLogCache_history_transaction);

                string nameID =
                    $"{baseOutlet}_{DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture)}_{BaseOutletNameID}_{Version}_{NameKasir}";

                var json = new
                {
                    name = nameID,
                    title = "ada ada aja",
                    message = messagesKasir,
                    sent_at = DateTime.Now.ToString("yyyy-MM-dd HH=mm=ss", CultureInfo.InvariantCulture),
                    log_last_outlet = LogCacheData,
                    cache_transaction = Cache_transaction,
                    cache_failed_transaction = Cache_failed_transaction,
                    cache_success_transaction = Cache_success_transaction,
                    cache_history_transaction = Cache_history_transaction
                };
                //string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);

                // Mengubah objek menjadi string JSON
                string jsonString = JsonConvert.SerializeObject(json, Formatting.None); // Tidak ada indentasi
                IApiService apiService = new ApiService();

                HttpResponseMessage response =
                    await apiService.notifikasiPengeluaran(jsonString, $"/complaint?outlet_id={baseOutlet}");

                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        DialogResult result = MessageBox.Show("Berhasil dikirim", "Gaspol", MessageBoxButtons.OK);
                        if (result == DialogResult.OK)
                        {
                            Close(); // Close the payForm
                        }

                        DialogResult = result;
                    }
                    else
                    {
                        MessageBox.Show("Gagal dikirim, Coba cek koneksi internet ulang " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private static string ReadFileContentAsPlainText(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new(fs))
                    {
                        string content = reader.ReadToEnd();

                        if (string.IsNullOrEmpty(content))
                        {
                            return "{}"; // Mengembalikan objek JSON kosong jika input null atau kosong
                        }

                        // Mengonversi string ke objek JSON
                        var jsonObject = new { Content = content };
                        string json = JsonConvert.SerializeObject(jsonObject);
                        return json; // Mengembalikan string JSON
                    }
                }
                catch (IOException ioEx)
                {
                    // Log kesalahan jika terjadi
                    LoggerUtil.LogError(ioEx, "Error accessing file {FilePath}", filePath);
                    return "{}"; // Mengembalikan objek JSON kosong dalam kasus kesalahan
                }
                catch (JsonException jsonEx)
                {
                    // Log kesalahan jika terjadi
                    LoggerUtil.LogError(jsonEx, "Error converting string to JSON");
                    return "{}"; // Mengembalikan objek JSON kosong dalam kasus kesalahan
                }
            }

            return "{}"; // Mengembalikan objek JSON kosong jika file tidak ada
        }

        private static string ReadFileContentWithRetry(string filePath)
        {
            int maxRetries = 3;
            int retries = 0;
            while (retries < maxRetries)
            {
                try
                {
                    return ReadFileContentAsSingleLine(filePath);
                }
                catch (IOException)
                {
                    retries++;
                    Thread.Sleep(500); // Wait for 500 ms before retrying
                }
            }

            return "{}"; // Return empty JSON if all retries fail
        }

        private static string ReadFileContentAsSingleLine(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new(fs))
                    {
                        string jsonContent = reader.ReadToEnd();
                        // Deserialisasi JSON dan serialisasi kembali
                        object? parsedJson = JsonConvert.DeserializeObject(jsonContent);
                        return JsonConvert.SerializeObject(parsedJson, Formatting.None,
                            new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
                    }
                }
                catch (IOException ioEx)
                {
                    // Log the error (you can add more specific error messages if needed)
                    LoggerUtil.LogError(ioEx, "Error accessing file {FilePath}", filePath);
                    return "{}"; // Return empty JSON in case of failure
                }
            }

            return "{}"; // Return "{}" if file doesn't exist
        }

        private void txtJumlahCicil_TextChanged(object sender, EventArgs e)
        {
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void txtSelesaiShift_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtNominal_TextChanged(object sender, EventArgs e)
        {
        }
    }
}