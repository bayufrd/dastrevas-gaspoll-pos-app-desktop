
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Printer;
using Newtonsoft.Json.Linq;
using System.Transactions;
using System.Windows.Markup;
using System.Data.Common;
using KASIR.OffineMode;
using System.Text;
namespace KASIR.Komponen
{
    public partial class shiftReport : UserControl
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private ApiService apiService;
        private readonly string baseOutlet;

        private readonly string MacAddressKasir;
        private readonly string PinPrinterKasir;
        private readonly string BaseOutletName;
        int bedaCash = 0;
        int shiftnumber, NewDataChecker = 0;
        DateTime mulaishift, akhirshift;

        public shiftReport()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
            InitializeComponent();
            apiService = new ApiService();
            btnCetakStruk.Enabled = true;
            lblNotifikasi.Visible = false;
            //LoadData();
            lblShiftSekarang.Visible = false;
        }
        public async Task SyncDataTransactions()
        {
            try
            {
                if (TransactionSync.IsSyncing) // Check if sync is already in progress using the shared manager
                {
                    return; // If sync is already running, exit
                }
                // Coba untuk memulai sinkronisasi
                bool canSync = await TransactionSync.BeginSyncAsync();
                if (!canSync)
                {
                    return; // Keluar jika tidak berhasil mendapatkan lock
                }
                try
                {
                    string filePath = "DT-Cache\\Transaction\\transaction.data";
                    string newSyncFileTransaction = "DT-Cache\\Transaction\\SyncSuccessTransaction";
                    string destinationPath = "DT-Cache\\Transaction\\transactionSyncing.data";

                    if (File.Exists(destinationPath))
                        try { File.Delete(destinationPath); }
                        catch (Exception ex) { LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message); }

                    IApiService apiService = new ApiService();

                    // Ensure directories exist
                    string directoryPath = Path.GetDirectoryName(filePath);
                    string newFileName = "";
                    string apiUrl = "/sync-transactions-outlet?outlet_id=" + baseOutlet;
                    newFileName = $"{baseOutlet}_SyncSuccess_{DateTime.Now:yyyyMMdd}.data";

                    EnsureDirectoryExists(directoryPath);

                    string saveBillDataPath = "DT-Cache\\Transaction\\saveBill.data";
                    string saveBillDataPathClone = "DT-Cache\\Transaction\\saveBillSync.data";

                    if (File.Exists(saveBillDataPath))
                    {
                        SyncSaveBillData(saveBillDataPath, saveBillDataPathClone, apiUrl);
                    }

                    if (!File.Exists(filePath))
                    {
                        return;
                    }

                    EnsureDirectoryExists(newSyncFileTransaction);

                    // Copy file to destination for processing
                    CopyFileWithStreams(filePath, destinationPath);

                    SimplifyAndSaveData(destinationPath);

                    // Process the transactions
                    JObject data = ParseAndRepairJsonFile(destinationPath);
                    JArray transactions = GetTransactionsFromData(data);

                    if (transactions == null || !transactions.Any())
                    {
                        // Delete file if data is empty but continue process
                        try { File.Delete(destinationPath); }
                        catch (Exception ex) { LoggerUtil.LogError(ex, "Failed to delete empty file: {ErrorMessage}", ex.Message); }

                        File.WriteAllText(destinationPath, "{\"data\":[]}");
                        transactions = new JArray();
                        data["data"] = transactions;
                    }

                    // Sync with API
                    HttpResponseMessage response = await apiService.SyncTransaction(data.ToString(), apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string folderCombine = Path.Combine(newSyncFileTransaction, newFileName);

                        // Handle successful sync
                        ProcessSuccessfulSync(destinationPath, folderCombine);

                        // Notify main form that sync was successful
                        SyncCompleted?.Invoke();
                        SyncSuccess(filePath);
                        NewDataChecker = 1;
                    }
                    else
                    {
                        // Handle failed sync
                        ProcessFailedSync(destinationPath, response, baseOutlet);
                    }
                }
                finally
                {
                    // Selalu selesaikan sinkronisasi, apapun yang terjadi
                    TransactionSync.EndSync();
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private JObject ParseAndRepairJsonFile(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);
            JObject data;

            try
            {
                // Try normal parsing first
                data = JObject.Parse(jsonData);
                return data;
            }
            catch (JsonReaderException jsonEx)
            {
                LoggerUtil.LogError(jsonEx, "JSON parsing error detected, attempting repair");

                // Backup corrupted file
                BackupCorruptedFile(filePath);

                // Try repair methods in sequence
                string errorInfo = ExtractErrorInfo(jsonEx, out string errorPath, out int errorIndex);

                // Try various repair methods
                if (TryRepairByRemovingProblematicTransaction(jsonData, errorIndex, out data))
                    return data;

                if (TryRepairWithRegexExtraction(jsonData, out data))
                    return data;

                if (TryRepairUnexpectedEndOfContent(jsonData, jsonEx, out data))
                    return data;

                if (TryManualJsonPatch(jsonData, jsonEx, out data))
                    return data;

                // If all repair methods fail, create minimal valid JSON
                LoggerUtil.LogWarning("All repair methods failed, creating minimal valid JSON");
                return JObject.Parse("{\"data\":[{\"id\":\"recovery\",\"message\":\"Data recovery failed\"}]}");
            }
            catch (Exception otherEx)
            {
                // Handle non-JSON errors
                LoggerUtil.LogError(otherEx, "Non-JSON parsing error occurred");

                try
                {
                    // Try parse as plain text and wrap in valid JSON
                    return JObject.Parse("{\"data\":[{\"raw_content\":\"" +
                           jsonData.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") +
                           "\"}]}");
                }
                catch
                {
                    // Last fallback if everything fails
                    return JObject.Parse("{\"data\":[{\"id\":\"recovery_fallback\"}]}");
                }
            }
        }

        private void BackupCorruptedFile(string filePath)
        {
            try
            {
                string backupPath = $"DT-Cache\\Transaction\\corrupted_{DateTime.Now:yyyyMMddHHmmss}.data";
                File.Copy(filePath, backupPath);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to backup corrupted file");
            }
        }

        private string ExtractErrorInfo(JsonReaderException jsonEx, out string errorPath, out int errorIndex)
        {
            errorPath = "";
            errorIndex = -1;

            if (jsonEx.Message.Contains("Path '"))
            {
                errorPath = jsonEx.Message.Split('\'')[1];
                if (errorPath.StartsWith("data[") && errorPath.Contains("]"))
                {
                    string indexStr = errorPath.Substring(5, errorPath.IndexOf(']') - 5);
                    int.TryParse(indexStr, out errorIndex);
                }
            }

            return jsonEx.Message;
        }

        private bool TryRepairByRemovingProblematicTransaction(string jsonData, int errorIndex, out JObject repairedData)
        {
            repairedData = null;
            if (errorIndex < 0) return false;

            try
            {
                // Find problematic transaction
                int openBrackets = 0;
                int closeBrackets = 0;
                int transactionStart = 0;
                int currentTrans = 0;
                bool inTransaction = false;

                for (int i = 0; i < jsonData.Length; i++)
                {
                    if (jsonData[i] == '{')
                    {
                        openBrackets++;
                        if (openBrackets == 2 && !inTransaction)
                        {
                            inTransaction = true;
                            transactionStart = i;
                        }
                    }
                    else if (jsonData[i] == '}')
                    {
                        closeBrackets++;
                        if (openBrackets == closeBrackets + 1 && inTransaction)
                        {
                            inTransaction = false;
                            if (currentTrans == errorIndex)
                            {
                                // Cut JSON just before problematic transaction
                                string fixedJson = jsonData.Substring(0, transactionStart);
                                // Add closing brackets
                                fixedJson = fixedJson.TrimEnd(',') + "]}";
                                repairedData = JObject.Parse(fixedJson);
                                LoggerUtil.LogWarning($"Repaired JSON by removing problematic transaction at index {errorIndex}");
                                return true;
                            }
                            currentTrans++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Repair by removing problematic transaction failed");
            }

            return false;
        }

        private bool TryRepairWithRegexExtraction(string jsonData, out JObject repairedData)
        {
            repairedData = null;

            try
            {
                string pattern = @"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}";
                MatchCollection matches = Regex.Matches(jsonData, pattern);

                if (matches.Count > 0)
                {
                    // Get first match which should be JSON root
                    JObject rootObj = JObject.Parse(matches[0].Value);
                    // Create new data array
                    JArray newDataArray = new JArray();

                    // Loop through all matches (transactions) after root and try to parse
                    for (int i = 1; i < matches.Count; i++)
                    {
                        try
                        {
                            JObject transObj = JObject.Parse(matches[i].Value);
                            newDataArray.Add(transObj);
                        }
                        catch
                        {
                            // Skip transactions that can't be parsed
                            LoggerUtil.LogWarning($"Skipping invalid transaction match at index {i}");
                        }
                    }

                    // Replace data array in root with new array
                    rootObj["data"] = newDataArray;
                    repairedData = rootObj;

                    LoggerUtil.LogWarning($"Repaired JSON using regex extraction, recovered {newDataArray.Count} transactions");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Repair with regex extraction failed");
            }

            return false;
        }

        private bool TryRepairUnexpectedEndOfContent(string jsonData, JsonReaderException jsonEx, out JObject repairedData)
        {
            repairedData = null;

            if (!jsonEx.Message.Contains("Unexpected end of content")) return false;

            try
            {
                // Try fixing by adding closing brackets
                string fixedJson = jsonData;
                // Count opening and closing brackets
                int openCount = fixedJson.Count(c => c == '{');
                int closeCount = fixedJson.Count(c => c == '}');
                int arrayOpen = fixedJson.Count(c => c == '[');
                int arrayClose = fixedJson.Count(c => c == ']');

                // Add missing closing brackets
                for (int i = 0; i < openCount - closeCount; i++)
                {
                    fixedJson += "}";
                }

                // Add missing array closing brackets
                for (int i = 0; i < arrayOpen - arrayClose; i++)
                {
                    fixedJson += "]";
                }

                repairedData = JObject.Parse(fixedJson);
                LoggerUtil.LogWarning("Repaired JSON by adding missing closing brackets");
                return true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Repair unexpected end of content failed");
            }

            return false;
        }

        private bool TryManualJsonPatch(string jsonData, JsonReaderException jsonEx, out JObject repairedData)
        {
            repairedData = null;

            try
            {
                // Get JSON segment before error
                int errorPos = 0;
                if (jsonEx.LinePosition > 0 && jsonEx.LineNumber > 0)
                {
                    // Find error position by counting lines and position
                    string[] lines = jsonData.Split('\n');
                    for (int i = 0; i < Math.Min(jsonEx.LineNumber - 1, lines.Length); i++)
                    {
                        errorPos += lines[i].Length + 1; // +1 for newline
                    }
                    errorPos += Math.Min(jsonEx.LinePosition - 1, lines[Math.Min(jsonEx.LineNumber - 1, lines.Length - 1)].Length);
                }

                // Cut JSON before error and fix
                if (errorPos > 0)
                {
                    string partialJson = jsonData.Substring(0, errorPos);
                    // Add necessary closing brackets
                    StringBuilder sb = new StringBuilder(partialJson);

                    // Figure out how many brackets need to be closed
                    int openBraces = partialJson.Count(c => c == '{');
                    int closeBraces = partialJson.Count(c => c == '}');
                    int openBrackets = partialJson.Count(c => c == '[');
                    int closeBrackets = partialJson.Count(c => c == ']');

                    // Close JSON structure
                    for (int i = 0; i < openBraces - closeBraces; i++)
                    {
                        sb.Append("}");
                    }

                    for (int i = 0; i < openBrackets - closeBrackets; i++)
                    {
                        sb.Append("]");
                    }

                    string fixedJson = sb.ToString();
                    repairedData = JObject.Parse(fixedJson);
                    LoggerUtil.LogWarning("Repaired JSON using manual truncation and closure");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Manual JSON patch failed");
            }

            return false;
        }

        private JArray GetTransactionsFromData(JObject data)
        {
            JArray transactions = (JArray)data["data"];
            if (transactions == null)
            {
                // If data array is null, create a new one
                transactions = new JArray();
                data["data"] = transactions;
            }
            return transactions;
        }

        private void ProcessSuccessfulSync(string sourcePath, string destinationPath)
        {
            if (File.Exists(destinationPath))
            {
                // Read existing data and add new transactions
                string existingData = File.ReadAllText(destinationPath);
                JObject existingDataJson = JObject.Parse(existingData);
                JArray existingTransactions = (JArray)existingDataJson["data"];

                // Read source file
                string jsonData = File.ReadAllText(sourcePath);
                JObject data = JObject.Parse(jsonData);
                JArray transactions = (JArray)data["data"];

                // Add new transactions to existing data
                foreach (var transaction in transactions)
                {
                    existingTransactions.Add(transaction);
                }

                // Save updated file
                File.WriteAllText(destinationPath, existingDataJson.ToString());
            }
            else
            {
                // Copy file using streams
                CopyFileWithStreams(sourcePath, destinationPath);
            }
        }

        private void ProcessFailedSync(string sourcePath, HttpResponseMessage response, string outletId)
        {
            string errorMessage = $"Gagal mengirim data Transactions. API Response: {response.ReasonPhrase}";
            //MessageBox.Show(errorMessage);


            string responseBody = response.Content.ReadAsStringAsync().Result;
            string detailedErrorMessage = $"{errorMessage} || Response Body: {responseBody}";

            string folderGagal = "DT-Cache\\Transaction\\FailedSyncTransaction";
            string newFileName = $"{outletId}_SyncFailed_{DateTime.Now:yyyyMMdd}.data";
            string folderCombine = Path.Combine(folderGagal, newFileName);

            EnsureDirectoryExists(folderGagal);

            if (File.Exists(folderCombine))
            {
                // Update existing failed sync file
                UpdateExistingFailedSyncFile(sourcePath, folderCombine);
            }
            else
            {
                // Copy to failed sync folder
                CopyFileWithStreams(sourcePath, folderCombine);
            }

            throw new Exception(detailedErrorMessage);
        }

        private void UpdateExistingFailedSyncFile(string sourcePath, string destinationPath)
        {
            try
            {
                // Read existing failed sync data
                string existingData = File.ReadAllText(destinationPath);
                JObject existingDataJson = JObject.Parse(existingData);
                JArray existingTransactions = (JArray)existingDataJson["data"];

                // Read source file
                string jsonData = File.ReadAllText(sourcePath);
                JObject data = JObject.Parse(jsonData);
                JArray transactions = (JArray)data["data"];

                // Add new transactions to existing data
                foreach (var transaction in transactions)
                {
                    existingTransactions.Add(transaction);
                }

                // Save updated file
                File.WriteAllText(destinationPath, existingDataJson.ToString());
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Failed to update existing failed sync file, copying full file instead");
                CopyFileWithStreams(sourcePath, destinationPath);
            }
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void CopyFileWithStreams(string sourcePath, string destinationPath)
        {
            using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (FileStream destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                sourceStream.CopyTo(destStream);
            }
        }
        public async void SyncSaveBillData(string saveBillDataPath, string saveBillDataPathClone, string apiUrl)
        {
            try
            {
                // Copy saveBill data using streams
                using (FileStream sourceStream = new FileStream(saveBillDataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream destStream = new FileStream(saveBillDataPathClone, FileMode.Create, FileAccess.Write))
                {
                    await sourceStream.CopyToAsync(destStream);
                }

                SimplifyAndSaveData(saveBillDataPathClone);

                string jsonSavwDataSync = File.ReadAllText(saveBillDataPathClone);
                JObject dataSave = JObject.Parse(jsonSavwDataSync);

                JArray transactionsSaveBill = (JArray)dataSave["data"];
                if (transactionsSaveBill == null || !transactionsSaveBill.Any())
                {
                    File.Delete(saveBillDataPathClone);
                    return;
                }

                HttpResponseMessage savebillSync = await apiService.SyncTransaction(jsonSavwDataSync, apiUrl);
                if (savebillSync.IsSuccessStatusCode)
                {
                    SyncSuccess(saveBillDataPath);
                }
                else
                {
                    MessageBox.Show("Gagal mengirim data SaveBill.");
                    throw new Exception("Gagal mengirim data SaveBill." + savebillSync.ToString());
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        public static readonly object FileLock = new object();
        public event Action SyncCompleted;  // Event untuk memberi tahu form utama bahwa sinkronisasi berhasil

        public static void SyncSuccess(string filePath)
        {
            try
            {
                lock (FileLock) // Pastikan hanya satu thread yang menulis ke file
                {
                    // Baca file JSON
                    string jsonData = File.ReadAllText(filePath);
                    JObject data = JObject.Parse(jsonData);

                    // Dapatkan array "data"
                    JArray transactions = (JArray)data["data"];

                    // Iterasi setiap transaksi dan hanya perbarui yang statusnya is_sent_sync = 0
                    foreach (JObject transaction in transactions)
                    {
                        if (transaction["is_sent_sync"] != null && (int)transaction["is_sent_sync"] == 0)
                        {
                            // Update status menjadi 1 jika transaksi berhasil
                            transaction["is_sent_sync"] = 1;

                            // Simpan perubahan hanya untuk transaksi yang berhasil disinkronkan
                        }
                    }
                    File.WriteAllText(filePath, data.ToString());

                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred during SyncSuccess: {ErrorMessage}", ex.Message);
            }
        }

        public static void SimplifyAndSaveData(string filePath)
        {
            try
            {
                // 1. Baca file JSON
                string jsonData = File.ReadAllText(filePath);
                JObject data = JObject.Parse(jsonData);
                // 2. Dapatkan array "data"
                JArray transactions = (JArray)data["data"];
                // 3. Iterasi setiap transaksi dan hapus elemen yang tidak diperlukan
                for (int i = transactions.Count - 1; i >= 0; i--) // Iterasi mundur untuk menghindari masalah saat menghapus
                {
                    JObject transaction = (JObject)transactions[i];
                    // Hapus transaksi jika is_sent_sync = 1
                    if (transaction["is_sent_sync"] != null && (int)transaction["is_sent_sync"] == 1)
                    {
                        transactions.RemoveAt(i);
                        continue; // Lewati sisa proses untuk transaksi ini
                    }
                    // Hapus field yang tidak dibutuhkan di level transaksi
                    transaction.Remove("transaction_id");
                    transaction.Remove("payment_type_name");

                    // Proses untuk menangani canceled_items dan cart_details
                    JArray cartDetails = (JArray)transaction["cart_details"];
                    JArray canceledItems = (JArray)transaction["canceled_items"];

                    // Periksa apakah canceled_items tidak kosong
                    if (canceledItems != null && canceledItems.Count > 0)
                    {
                        foreach (JObject canceledItem in canceledItems)
                        {
                            long canceledCartDetailId = (long)canceledItem["cart_detail_id"];
                            string cancelReason = (string)canceledItem["cancel_reason"];

                            // Cari item di cart_details dengan cart_detail_id yang sama
                            for (int j = 0; j < cartDetails.Count; j++)
                            {
                                JObject cartItem = (JObject)cartDetails[j];
                                long cartDetailId = (long)cartItem["cart_detail_id"];

                                if (cartDetailId == canceledCartDetailId)
                                {
                                    // Periksa jika qty pada cart_details tidak sama dengan 0
                                    if ((int)cartItem["qty"] != 0)
                                    {
                                        // Duplikasi item dan tambahkan is_canceled=1 dan cancel_reason
                                        JObject duplicateItem = (JObject)cartItem.DeepClone();

                                        // Modifikasi cart_detail_id dengan menambahkan 1 di awal
                                        string originalDetailId = cartDetailId.ToString();
                                        string newDetailId = "1" + originalDetailId;
                                        duplicateItem["cart_detail_id"] = long.Parse(newDetailId);

                                        // Tambahkan is_canceled dan cancel_reason
                                        duplicateItem["is_canceled"] = 1;
                                        duplicateItem["cancel_reason"] = cancelReason;

                                        // Tambahkan item yang sudah diduplikasi ke cartDetails
                                        cartDetails.Add(duplicateItem);
                                    }
                                    else // Jika qty = 0 
                                    {
                                        // Modifikasi item yang ada alih-alih menduplikasi
                                        cartItem["qty"] = (int)canceledItem["qty"];
                                        cartItem["is_canceled"] = 1;
                                        cartItem["cancel_reason"] = cancelReason;
                                        cartItem["total_price"] = (int)canceledItem["total_price"];
                                        cartItem["subtotal_price"] = (int)canceledItem["subtotal_price"];
                                    }

                                    break; // Keluar dari loop setelah menemukan dan memproses item
                                }
                            }
                        }
                    }

                    // Iterasi ke cart_details untuk menghapus field yang tidak dibutuhkan
                    foreach (JObject cartItem in cartDetails)
                    {
                        cartItem.Remove("menu_name"); // Hapus serving_type_name dari cart detail
                        cartItem.Remove("menu_type");  // Hapus menu_detail_name jika tidak diperlukan
                        cartItem.Remove("menu_detail_name");  // Hapus varian jika tidak diperluka
                    }
                }

                // 4. Simpan data yang sudah disederhanakan ke file baru atau file yang sama
                // MessageBox.Show(data.ToString());
                File.WriteAllText(filePath, data.ToString());
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private async Task<string> GetShiftData(string configOfflineMode)
        {
            IApiService apiService = new ApiService();
            return await apiService.CekShift("/shift?outlet_id=" + baseOutlet);
        }
        private static bool isSyncing = false;  // Static flag to track sync status
        public async Task LoadData()
        {
            if (TransactionSync.IsSyncing)
            {
                // Jika sedang menyinkronkan, tunggu 2 detik dan panggil LoadData lagi
                await Task.Delay(5000); // Tunggu 5 detik
                await LoadData(); // Panggil LoadData lagi
                return;
            }
            btnCetakStruk.Enabled = false;
            const int maxRetryAttempts = 3;
            int retryAttempts = 0;
            bool success = false;
            string Config = "setting\\OfflineMode.data";

            while (retryAttempts < maxRetryAttempts && !success)
            {
                try
                {
                    NewDataChecker = 0;
                    // Mengecek apakah sButtonOffline dalam status checked,l=
                    string directoryPath = Path.GetDirectoryName(Config);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    // Memeriksa apakah file ada
                    if (!File.Exists(Config))
                    {
                        // Membuat file dan menulis "OFF" ke dalamnya jika file tidak ada
                        File.WriteAllText(Config, "OFF");
                    }
                    string allSettingsData = File.ReadAllText(Config); // Ambil status offline

                    // Jika status offline ON, tampilkan Offline_masterPos
                    if (!string.IsNullOrEmpty(allSettingsData) && allSettingsData == "ON")
                    {
                        await SyncDataTransactions();
                    }

                    string response = await GetShiftData(allSettingsData);
                    //File.WriteAllText($"DT-Cache\\Transaction\\ShiftRepot{baseOutlet}.data", JsonConvert.SerializeObject(response, Formatting.Indented));
                    if (response != null)
                    {
                        try
                        {
                            GetShift cekShift = JsonConvert.DeserializeObject<GetShift>(response);
                            DataShift datas = cekShift.data;
                            List<ExpenditureStrukShift> expenditures = datas.expenditures;
                            List<CartDetailsSuccessStrukShift> cartDetailsSuccess = datas.cart_details_success;
                            List<CartDetailsPendingStrukShift> cartDetailsPending = datas.cart_details_pending;
                            List<CartDetailsCanceledStrukShift> cartDetailsCanceled = datas.cart_details_canceled;
                            List<RefundDetailStrukShift> refundDetails = datas.refund_details;
                            List<PaymentDetailStrukShift> paymentDetails = datas.payment_details;
                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("ID", typeof(string)); // Add a column to avoid header error
                            dataTable.Columns.Add("DATA", typeof(string));
                            dataTable.Columns.Add("Detail", typeof(string));

                            //dataTable.Rows.Add(null, "SHIFT REPORT : -", null); // Add a separator row
                            // Panggil metode untuk menambahkan separator row
                            AddSeparatorRow(dataTable, "SHIFT REPORT", dataGridView1);
                            // Panggil metode untuk menambahkan separator row
                            // Mengonversi string ke DateTime
                            DateTime dateTime = DateTime.Parse(datas.start_date.ToString(), CultureInfo.InvariantCulture);

                            // Format tanggal dan waktu sesuai dengan yang diinginkan
                            string formattedDateTime = dateTime.ToString("dddd, dd MMMM yyyy 'Pukul' HH:mm:ss", new CultureInfo("id-ID"));

                            AddSeparatorRow(dataTable, "Start Date : " + formattedDateTime, dataGridView1);
                            // Panggil metode untuk menambahkan separator row
                            string mulai = datas.start_date.ToString();
                            string akhir = datas.end_date.ToString();
                            convertDateTime(mulai, akhir);
                            shiftnumber = datas.shift_number;
                            shiftnumber += 1;
                            lblShiftSekarang.Text += " | Shift: " + shiftnumber;
                            AddSeparatorRow(dataTable, "SHIFT NUMBER : " + shiftnumber.ToString(), dataGridView1);

                            AddSeparatorRow(dataTable, " ", dataGridView1);

                            AddSeparatorRow(dataTable, "ORDER DETAILS", dataGridView1);
                            AddSeparatorRow(dataTable, "SOLD ITEMS", dataGridView1);

                            var sortedcartDetailSuccess = cartDetailsSuccess.OrderBy(x =>
                            {
                                if (x.menu_type.Contains("Minuman")) return 1;
                                if (x.menu_type.Contains("Additional Minuman")) return 2;
                                if (x.menu_type.Contains("Makanan")) return 3;
                                if (x.menu_type.Contains("Additional Makanan")) return 4;
                                return 5;
                            })
                            .ThenBy(x => x.menu_name);

                            foreach (var cartDetail in sortedcartDetailSuccess)
                            {
                                // Add varian to the cart detail name if it's not null
                                string displayMenuName = cartDetail.menu_name;
                                if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                {
                                    displayMenuName += "\n - " + cartDetail.varian;
                                }

                                //dataTable.Rows.Add(null, displayMenuName, null);
                                dataTable.Rows.Add(null, string.Format("{0}x ", cartDetail.qty) + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                            }

                            dataTable.Rows.Add(null, "Item Sold Qty :", datas.totalSuccessQty);
                            dataTable.Rows.Add(null, "Item Sold Amount :", string.Format("{0:n0}", datas.totalCartSuccessAmount));

                            if (cartDetailsPending.Count != 0)
                            {
                                AddSeparatorRow(dataTable, "PENDING ITEMS", dataGridView1);

                                var sortedcartDetailPendings = cartDetailsPending.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);

                                foreach (var cartDetail in sortedcartDetailPendings)
                                {
                                    // Add varian to the cart detail name if it's not null
                                    string displayMenuName = cartDetail.menu_name;
                                    if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                    {
                                        displayMenuName += "\n - " + cartDetail.varian;
                                    }

                                    //dataTable.Rows.Add(null, displayMenuName, null);
                                    dataTable.Rows.Add(null, string.Format("{0}x ", cartDetail.qty) + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                                }

                                dataTable.Rows.Add(null, "Item Pending Qty :", datas.totalPendingQty);
                                dataTable.Rows.Add(null, "Item Pending Amount :", string.Format("{0:n0}", datas.totalCartPendingAmount));
                            }

                            if (cartDetailsCanceled.Count != 0)
                            {
                                AddSeparatorRow(dataTable, "CANCELED ITEMS", dataGridView1);

                                var sortedcartDetailCanceled = cartDetailsCanceled.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);

                                foreach (var cartDetail in sortedcartDetailCanceled)
                                {
                                    // Add varian to the cart detail name if it's not null
                                    string displayMenuName = cartDetail.menu_name;
                                    if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                    {
                                        displayMenuName += "\n - " + cartDetail.varian;
                                    }

                                    //dataTable.Rows.Add(null, displayMenuName, null);
                                    dataTable.Rows.Add(null, string.Format("{0}x ", cartDetail.qty) + displayMenuName, string.Format("{0:n0}", cartDetail.total_price));
                                }

                                dataTable.Rows.Add(null, "Item Cancel Qty :", datas.totalCanceledQty);
                                dataTable.Rows.Add(null, "Item Canceled Amount : ", string.Format("{0:n0}", datas.totalCartCanceledAmount));
                            }
                            if (refundDetails.Count != 0)
                            {
                                AddSeparatorRow(dataTable, "REFUNDED ITEMS", dataGridView1);

                                var sortedrefoundDetails = refundDetails.OrderBy(x =>
                                {
                                    if (x.menu_type.Contains("Minuman")) return 1;
                                    if (x.menu_type.Contains("Additional Minuman")) return 2;
                                    if (x.menu_type.Contains("Makanan")) return 3;
                                    if (x.menu_type.Contains("Additional Makanan")) return 4;
                                    return 5;
                                })
                                .ThenBy(x => x.menu_name);

                                foreach (var cartDetail in sortedrefoundDetails)
                                {
                                    // Add varian to the cart detail name if it's not null
                                    string displayMenuName = cartDetail.menu_name;
                                    if (!string.IsNullOrEmpty(cartDetail.varian) && cartDetail.varian != "null")
                                    {
                                        displayMenuName += "\n - " + cartDetail.varian;
                                    }

                                    //dataTable.Rows.Add(null, displayMenuName, null);
                                    dataTable.Rows.Add(null, string.Format("{0}x ", cartDetail.qty_refund_item) + displayMenuName, string.Format("{0:n0}", cartDetail.total_refund_price));
                                }

                                dataTable.Rows.Add(null, "Item Refund Qty :", datas.totalRefundQty);
                                dataTable.Rows.Add(null, "Item Refund Amount : ", string.Format("{0:n0}", datas.totalCartRefundAmount));
                            }
                            AddSeparatorRow(dataTable, "CASH MANAGEMENT", dataGridView1);

                            if (expenditures.Count != 0)
                            {
                                AddSeparatorRow(dataTable, "EXPENSE", dataGridView1);

                                foreach (var expense in expenditures)
                                {
                                    dataTable.Rows.Add(null, expense.description, string.Format("{0:n0}", expense.nominal));
                                }
                            }
                            dataTable.Rows.Add(null, "Expected Ending Cash", string.Format("{0:n0}", datas.ending_cash_expected));
                            dataTable.Rows.Add(null, "Actual Ending Cash", string.Format("{0:n0}", datas.ending_cash_actual));
                            dataTable.Rows.Add(null, "Cash Difference", string.Format("{0:n0}", datas.cash_difference));
                            AddSeparatorRow(dataTable, " ", dataGridView1);

                            dataTable.Rows.Add(null, "DISCOUNTS", "");
                            dataTable.Rows.Add(null, "All Discount items", string.Format("{0:n0}", datas.discount_amount_per_items));
                            dataTable.Rows.Add(null, "All Discount Cart", string.Format("{0:n0}", datas.discount_amount_transactions));
                            dataTable.Rows.Add(null, "TOTAL AMOUNT", string.Format("{0:n0}", datas.discount_total_amount));
                            AddSeparatorRow(dataTable, "PAYMENT DETAIL", dataGridView1);

                            foreach (var paymentDetail in paymentDetails)
                            {
                                AddSeparatorRow(dataTable, paymentDetail.payment_category, dataGridView1);
                                //dataTable.Rows.Add(null, paymentDetail.payment_category, "");
                                foreach (var paymentType in paymentDetail.payment_type_detail)
                                {
                                    dataTable.Rows.Add(null, paymentType.payment_type, string.Format("{0:n0}", paymentType.total_payment));
                                }
                                dataTable.Rows.Add(null, "TOTAL AMOUNT", string.Format("{0:n0}", paymentDetail.total_amount));
                                AddSeparatorRow(dataTable, " ", dataGridView1);

                            }
                            dataTable.Rows.Add(null, "TOTAL TRANSACTION", string.Format("{0:n0}", datas.total_transaction));

                            if (dataGridView1 == null)
                            {
                                ReloadDataGridView(dataTable);
                            }
                            else if (dataGridView1.Font == null)
                            {
                                // Mengatur font default jika Font null
                                dataGridView1.Font = new Font("Arial", 8.25f, FontStyle.Regular);
                                ReloadDataGridView(dataTable);
                            }
                            else
                            {
                                dataGridView1.DataSource = dataTable;

                                if (dataGridView1.Columns.Contains("DATA"))
                                {
                                    DataGridViewCellStyle boldStyle = new DataGridViewCellStyle
                                    {
                                        Font = new Font(dataGridView1.Font, FontStyle.Italic)
                                    };
                                    dataGridView1.Columns["DATA"].DefaultCellStyle = boldStyle;
                                }
                                else
                                {
                                    // Menangani situasi jika kolom "DATA" tidak ada
                                    Console.WriteLine("Kolom 'DATA' tidak ditemukan.");
                                }

                                if (dataGridView1.Columns.Contains("ID"))
                                {
                                    dataGridView1.Columns["ID"].Visible = false;
                                }
                                else
                                {
                                    // Menangani situasi jika kolom "ID" tidak ada
                                    Console.WriteLine("Kolom 'ID' tidak ditemukan.");
                                }
                            }

                            btnCetakStruk.Enabled = true;
                            bedaCash = datas.ending_cash_expected;
                            txtActualCash.Text = datas.ending_cash_expected.ToString();
                            txtNamaKasir.Text = "Dastrevas (AutoFill)";
                            bool isMoreThanOneHourAgo = IsStartShiftMoreThanOneHourAgo();
                            if (isMoreThanOneHourAgo)
                            {
                                btnCetakStruk.Enabled = true;
                            }
                            else
                            {
                                lblNotifikasi.Visible = true;
                                lblNotifikasi.Text = $"Tidak dapat akhiri laporan karena belum ada jarak 1 jam dari mulai shift.\nUntuk Cetak Ulang Laporan Shift [{datas.shift_number}] Tersedia.";
                                btnCetakStruk.Enabled = false;
                            }
                            success = true; // Successfully loaded data
                        }
                        catch (NullReferenceException ex)
                        {
                            retryAttempts++;
                            if (retryAttempts >= maxRetryAttempts)
                            {
                                //MessageBox.Show("A null reference error occurred: " + ex.Message, "Null Reference Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                LoggerUtil.LogError(ex, "A null reference error occurred: {ErrorMessage}", ex.Message);
                                CleanFormAndAddRetryButton(ex.Message);

                                break; // Stop retrying after max attempts
                            }
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show("Error: " + ex.Message);
                            LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                            CleanFormAndAddRetryButton(ex.Message);

                            break; // Stop retrying on other exceptions
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    CleanFormAndAddRetryButton(ex.Message);

                    break; // Do not retry on TaskCanceledException
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    CleanFormAndAddRetryButton(ex.Message);

                    break; // Stop retrying on other exceptions
                }
            }
        }
        private void AddSeparatorRow(DataTable dataTable, string groupKey, DataGridView dataGridView)
        {
            // Tambahkan separator row ke DataTable
            dataTable.Rows.Add(null, groupKey, null); // Add a separator row

            // Ambil indeks baris terakhir yang baru saja ditambahkan
            int lastRowIndex = dataTable.Rows.Count - 1;

            // Menambahkan row ke DataGridView
            dataGridView.DataSource = dataTable;

            // Mengatur gaya sel untuk kolom tertentu
            int[] cellIndexesToStyle = { 1, 2 }; // Indeks kolom yang ingin diatur
            SetCellStyle(dataGridView.Rows[lastRowIndex], cellIndexesToStyle, Color.WhiteSmoke, FontStyle.Bold);
        }
        private void SetCellStyle(DataGridViewRow row, int[] cellIndexes, Color backgroundColor, FontStyle fontStyle)
        {
            foreach (int index in cellIndexes)
            {
                row.Cells[index].Style.BackColor = backgroundColor;
                row.Cells[index].Style.Font = new Font(dataGridView1.Font, fontStyle);
            }
        }
        private void CleanFormAndAddRetryButton(string ex)
        {
            // Bersihkan form
            if (dataGridView1 != null && dataGridView1.DataSource != null)
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
            }

            lblShiftSekarang.Text = "Shift: ";
            txtActualCash.Clear();
            txtNamaKasir.Clear();
            panel1.Visible = false;
            panel2.Visible = false;

            // Tambahkan PictureBox untuk gambar
            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = "ErrorImg";
            pictureBox.Size = new Size(100, 100); // Sesuaikan ukuran gambar
            pictureBox.Location = new Point((this.ClientSize.Width - pictureBox.Width) / 2, (this.ClientSize.Height - pictureBox.Height) / 2 - 110); // Posisi di atas tombol
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Atur ukuran gambar agar sesuai dengan PictureBox
            using (FileStream fs = new FileStream("icon\\OutletLogo.bmp", FileMode.Open, FileAccess.Read))
            {
                pictureBox.Image = Image.FromStream(fs);
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

            // Membuat sudut membulat
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 20, 20, 180, 90);
            path.AddArc(btnRetry.Width - 20, 0, 20, 20, 270, 90);
            path.AddArc(btnRetry.Width - 20, btnRetry.Height - 20, 20, 20, 0, 90);
            path.AddArc(0, btnRetry.Height - 20, 20, 20, 90, 90);
            path.CloseFigure();
            btnRetry.Region = new Region(path);

            // Menambahkan ikon FontAwesome
            // Pastikan Anda sudah menambahkan FontAwesome ke proyek Anda
            // Misalnya, menggunakan FontAwesome.Sharp
            // Anda bisa menggunakan Label untuk menampilkan ikon
            Label lblIcon = new Label();
            lblIcon.Text = "\uf021"; // Ganti dengan kode ikon FontAwesome yang sesuai
            lblIcon.Font = new Font("FontAwesome", 14); // Pastikan font FontAwesome sudah ditambahkan
            lblIcon.ForeColor = Color.White;
            lblIcon.Location = new Point(10, 10); // Sesuaikan posisi ikon
            lblIcon.AutoSize = true;
            // Menambahkan label di bawah tombol
            Label lblInfo = new Label();
            lblInfo.Name = "ErrorMsg";
            lblInfo.Text = ex.ToString(); // Teks yang ingin ditampilkan
            lblInfo.Font = new Font("Arial", 9, FontStyle.Regular);
            lblInfo.ForeColor = Color.Black; // Warna teks label
            lblInfo.AutoSize = true; // Agar label menyesuaikan ukuran teks

            // Mengatur posisi label agar berada di tengah
            lblInfo.Location = new Point((this.ClientSize.Width - lblInfo.Width) / 4, btnRetry.Bottom + 10); // Posisi di bawah tombol

            // Menambahkan kontrol ke form
            this.Controls.Add(pictureBox); // Menambahkan PictureBox ke form
            this.Controls.Add(btnRetry);
            this.Controls.Add(lblInfo); // Menambahkan label ke form


            this.Controls.Add(btnRetry);
            btnRetry.BringToFront();
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            // Hapus tombol retry
            if (sender is Button btnRetry)
            {
                this.Controls.Remove(btnRetry);
            }

            // Hapus label informasi jika ada
            var lblInfo = this.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "ErrorMsg");
            if (lblInfo != null)
            {
                this.Controls.Remove(lblInfo);
            }

            var ErrImg = this.Controls.OfType<PictureBox>().FirstOrDefault(lbl => lbl.Name == "ErrorImg");
            if (ErrImg != null)
            {
                this.Controls.Remove(ErrImg);
            }
            // Reinisialisasi form jika diperlukan
            InitializeComponent();

            // Muat data kembali
            _ = LoadData();
        }
        private void convertDateTime(string mulai, string akhir)
        {
            // Attempt to convert the string to a DateTime object using the DateTime.ParseExact method
            try
            {
                mulaishift = DateTime.ParseExact(mulai, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
                akhirshift = DateTime.ParseExact(akhir, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        public bool IsStartShiftMoreThanOneHourAgo()
        {
            TimeSpan difference = akhirshift - mulaishift;
            if (difference.TotalHours > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ReloadDataGridView(DataTable dataTable)
        {
            try
            {
                // Attempt to reinitialize dataGridView1 if it is null
                if (dataGridView1 == null)
                {
                    dataGridView1 = new DataGridView();
                }

                // Attempt to set the Font property if it is null
                if (dataGridView1.Font == null)
                {
                    dataGridView1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                }

                // Reapply data source and styles
                dataGridView1.DataSource = dataTable; // Assuming dataTable is a class-level variable or passed as a parameter
                DataGridViewCellStyle boldStyle = new DataGridViewCellStyle
                {
                    Font = new Font(dataGridView1.Font, FontStyle.Italic)
                };
                dataGridView1.Columns["DATA"].DefaultCellStyle = boldStyle;
                dataGridView1.Columns["ID"].Visible = false;

                // Log success
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the reinitialization process
                LoggerUtil.LogError(ex, "An error occurred while reinitializing dataGridView1: {ErrorMessage}", ex.Message);
            }
        }

        private void txtActualCash_TextChanged(object sender, EventArgs e)
        {
            if (txtActualCash.Text == "" || txtActualCash.Text == "0") return;
            decimal number;
            try
            {
                number = decimal.Parse(txtActualCash.Text, System.Globalization.NumberStyles.Currency);
            }
            catch (FormatException)
            {
                // The text could not be parsed as a decimal number.
                // You can handle this exception in different ways, such as displaying a message to the user.
                MessageBox.Show("inputan hanya bisa Numeric");
                return;
            }
            txtActualCash.Text = number.ToString("#,#");
            txtActualCash.SelectionStart = txtActualCash.Text.Length;
        }

        private void btnPengeluaran_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnPengeluaran_Click));

            Form background = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = this.Location,
                ShowInTaskbar = false,
            };

            using (notifikasiPengeluaran notifikasiPengeluaran = new notifikasiPengeluaran())
            {
                notifikasiPengeluaran.Owner = background;

                background.Show();

                DialogResult dialogResult = notifikasiPengeluaran.ShowDialog();

                background.Dispose();
            }
        }

        private void btnRiwayatShift_Click(object sender, EventArgs e)
        {

            Form background = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = this.Location,
                ShowInTaskbar = false,
            };
            this.Invoke((MethodInvoker)delegate
            {
                using (printReportShift payForm = new printReportShift())
                {
                    payForm.Owner = background;

                    background.Show();

                    DialogResult dialogResult = payForm.ShowDialog();

                    background.Dispose();
                    if (dialogResult == DialogResult.OK && payForm.ReloadDataInBaseForm)
                    {
                        LoadData();

                    }
                }
            });
        }

        private async void btnCetakStruk_Click(object sender, EventArgs e)
        {
            try
            {

                if (txtNamaKasir.Text.ToString() == "" || txtNamaKasir.Text == null)
                {
                    MessageBox.Show("Nama kasir masih kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (txtActualCash.Text.ToString() == "" || txtActualCash.Text == null)
                {
                    MessageBox.Show("Uang kasir masih kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string fulus = Regex.Replace(txtActualCash.Text, "[^0-9]", "");


                DialogResult yakin = MessageBox.Show($"Melakukan End Shift {shiftnumber.ToString()} pada waktu \n{mulaishift.ToString()} sampai {akhirshift.ToString()}\nNama Kasir : {txtNamaKasir.Text}\nActual Cash : Rp.{txtActualCash.Text},- \nCash Different : {string.Format("{0:n0}", Convert.ToInt32(fulus) - bedaCash)}?", "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (yakin != DialogResult.Yes)
                {
                    MessageBox.Show("Cetak Shift diCancel");
                    return;
                }
                else
                {
                    btnCetakStruk.Enabled = false;
                    btnCetakStruk.Text = "Waiting...";
                    ////LoggerUtil.LogPrivateMethod(nameof(btnCetakStruk_Click));

                    var casherName = string.IsNullOrEmpty(txtNamaKasir.Text) ? "" : txtNamaKasir.Text;
                    var actualCash = string.IsNullOrEmpty(fulus) ? "0" : fulus;

                    var json = new
                    {
                        outlet_id = baseOutlet,
                        casher_name = casherName,
                        actual_ending_cash = actualCash
                    };
                    string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);

                    IApiService api = new ApiService();

                    string response = await api.CetakLaporanShift(jsonString, "/struct-shift");

                    if (response != null)
                    {
                        await HandleSuccessfulPrint(response);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ResetButtonState();
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal cetak laporan " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                ResetButtonState();
            }
        }

        private async Task HandleSuccessfulPrint(string response)
        {
            int retryCount = 3; // Jumlah maksimal percobaan ulang
            int delayBetweenRetries = 2000; // Jeda antara percobaan dalam milidetik
            int currentRetry = 0;

            while (currentRetry < retryCount)
            {
                try
                {
                    PrinterModel printerModel = new PrinterModel();
                    btnCetakStruk.Text = "Mencetak...";
                    btnCetakStruk.Enabled = false;

                    GetStrukShift shiftModel = JsonConvert.DeserializeObject<GetStrukShift>(response);
                    DataStrukShift datas = shiftModel.data;

                    await printerModel.PrintModelCetakLaporanShift(
                        datas,
                        datas.expenditures,
                        datas.cart_details_success,
                        datas.cart_details_pending,
                        datas.cart_details_canceled,
                        datas.refund_details,
                        datas.payment_details
                    );

                    MessageBox.Show("Cetak laporan sukses", "Gaspol", MessageBoxButtons.OK);
                    btnCetakStruk.Enabled = false;

                    // Jika sukses, keluar dari loop
                    break;
                }
                catch (Exception ex)
                {
                    currentRetry++;

                    if (currentRetry >= retryCount)
                    {
                        // Jika sudah mencapai batas retry, tampilkan pesan error
                        //MessageBox.Show("Error printing after multiple attempts: " + ex.Message);
                        LoggerUtil.LogError(ex, $"Error printing after {currentRetry}", ex);
                    }
                    else
                    {
                        // Tunda sebelum mencoba ulang
                        //MessageBox.Show($"Error printing attempt {currentRetry}: {ex.Message}, retrying...");
                        await Task.Delay(delayBetweenRetries); // Menunggu sebelum mencoba lagi
                    }
                }
            }
        }


        private void ResetButtonState()
        {
            btnCetakStruk.Enabled = true;
            btnCetakStruk.Text = "Cetak Struk";
            btnCetakStruk.BackColor = Color.FromArgb(31, 30, 68);
        }
    }
}
