using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Helper;
using KASIR.Properties;
using Newtonsoft.Json.Linq;

namespace KASIR.OffineMode
{
    public class transactionFileMover
    {
        private readonly string baseOutlet = Settings.Default.BaseOutlet;

        public async Task refreshCacheTransaction()
        {
            string transactionSource = "DT-Cache\\Transaction\\transaction.data";
            string shiftSource = "DT-Cache\\Transaction\\shiftData.data";
            string expenditureSource = "DT-Cache\\Transaction\\expenditure.data";

            string transactionHistoryDir = "DT-Cache\\Transaction\\HistoryTransaction";
            string shiftHistoryDir = "DT-Cache\\Transaction\\ShiftDataTransaction";
            string expenditureHistoryDir = "DT-Cache\\Transaction\\ExpendituresTransaction";

            try
            {
                SyncHelper c = new();
                c.IsBackgroundOperation = true;

                await c.SyncDataTransactions();

                // ================= TRANSACTION =================
                if (File.Exists(transactionSource))
                {
                    string jsonData = await ReadJsonFileAsync(transactionSource);
                    JObject data = JObject.Parse(jsonData);
                    JArray transactions = (JArray)data["data"];

                    if (transactions != null && transactions.Count > 0)
                    {
                        DateTime? firstTransactionDate = GetFirstTransactionDate(transactions);
                        DateTime currentDateTime = DateTime.Now;

                        if (firstTransactionDate.HasValue &&
                            firstTransactionDate.Value.Date != currentDateTime.Date &&
                            currentDateTime.Hour >= 6)
                        {
                            // Archive transaction
                            await ArchiveTransactionData(transactions, transactionHistoryDir, jsonData);

                            // Clear transaction file
                            await ClearSourceFile(transactionSource);

                            // Archive shift data & expenditure data
                            //await ArchiveShiftData(shiftSource, shiftHistoryDir); // max 3 shift otomatis
                            await ArchiveData(shiftSource, shiftHistoryDir);
                            await ArchiveData(expenditureSource, expenditureHistoryDir);
                            await ClearSourceFile(shiftSource);

                            // Clear expenditure file
                            await ClearSourceFile(expenditureSource);
                        }
                    }
                }
                else
                {
                    if (File.Exists(shiftSource))
                        await ArchiveShiftData(shiftSource, shiftHistoryDir);
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Unexpected error in refreshCacheTransaction: {ErrorMessage}", ex.Message);
            }
        }


        public async Task<string> ReadJsonFileAsync(string filePath)
        {
            using (FileStream sourceStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new(sourceStream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public DateTime? GetFirstTransactionDate(JArray transactions)
        {
            JObject firstTransaction = (JObject)transactions[0];
            if (firstTransaction["created_at"] != null)
            {
                string createdAt = firstTransaction["created_at"].ToString();
                createdAt = Regex.Replace(createdAt, @"(\d)\.(\d)", "$1:$2"); // Replace dot with colon for time format

                if (DateTime.TryParseExact(createdAt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
            }

            return null;
        }

        private async Task ArchiveTransactionData(JArray transactions, string historyDirectory, string jsonData)
        {
            DateTime previousDay = DateTime.Now.AddDays(-1);

            string fileNameWithoutExtension =
                Path.GetFileNameWithoutExtension("DT-Cache\\Transaction\\transaction.data");
            string fileExtension = Path.GetExtension("DT-Cache\\Transaction\\transaction.data");
            string newFileName =
                $"History_{fileNameWithoutExtension}_DT-{baseOutlet}_{previousDay:yyyyMMdd}{fileExtension}";
            string destinationPath = Path.Combine(historyDirectory, newFileName);

            if (!Directory.Exists(historyDirectory))
            {
                _ = Directory.CreateDirectory(historyDirectory);
            }

            if (File.Exists(destinationPath))
            {
                await MergeTransactionsToFile(destinationPath, transactions);
            }
            else
            {
                await WriteJsonToFile(destinationPath, jsonData);
            }
        }

        private async Task MergeTransactionsToFile(string destinationPath, JArray transactions)
        {
            string existingData = await ReadJsonFileAsync(destinationPath);
            JObject destinationJson = JObject.Parse(existingData);
            JArray destinationTransactions = (JArray)destinationJson["data"];

            foreach (JObject trans in transactions)
            {
                destinationTransactions.Add(trans);
            }

            await WriteJsonToFile(destinationPath, destinationJson.ToString());
        }

        public async Task WriteJsonToFile(string filePath, string jsonData)
        {
            using (FileStream writeStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new(writeStream))
            {
                await writer.WriteAsync(jsonData);
            }
        }

        public async Task ClearSourceFile(string sourcePath)
        {
            JObject emptyJson = new();
            emptyJson["data"] = new JArray();

            try
            {
                await WriteJsonToFile(sourcePath, emptyJson.ToString());
            }
            catch (IOException ex)
            {
                LoggerUtil.LogError(ex, $"Could not clear the source file: {ex.Message}");
            }
        }
        public async Task ArchiveShiftData(string sourceFilePath, string archiveDirectory, int maxShifts = 3)
        {
            if (!Directory.Exists(archiveDirectory))
                _ = Directory.CreateDirectory(archiveDirectory);

            DateTime previousDay = DateTime.Now.AddDays(-1);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            string fileExtension = Path.GetExtension(sourceFilePath);
            string newFileName =
                $"History_{fileNameWithoutExtension}_DT-{baseOutlet}_{previousDay:yyyyMMdd}{fileExtension}";
            string destinationPath = Path.Combine(archiveDirectory, newFileName);

            // Baca source file
            string sourceData = await ReadJsonFileAsync(sourceFilePath);
            JObject sourceJson = JObject.Parse(sourceData);
            JArray sourceArray = (JArray)sourceJson["data"];

            if (sourceArray.Count > maxShifts)
            {
                // Ambil shift yang lebih lama untuk di-archive
                var shiftsToArchive = sourceArray.Take(sourceArray.Count - maxShifts).ToList();

                // Hapus shift yang di-archive dari sourceArray
                foreach (var shift in shiftsToArchive)
                    _ = sourceArray.Remove(shift);

                // Merge ke file archive jika ada
                if (File.Exists(destinationPath))
                {
                    string existingData = await ReadJsonFileAsync(destinationPath);
                    JObject destinationJson = JObject.Parse(existingData);
                    JArray destinationArray = (JArray)destinationJson["data"];

                    foreach (var shift in shiftsToArchive)
                        destinationArray.Add(shift);

                    await WriteJsonToFile(destinationPath, destinationJson.ToString());
                }
                else
                {
                    JObject newArchive = new();
                    newArchive["data"] = new JArray(shiftsToArchive);
                    await WriteJsonToFile(destinationPath, newArchive.ToString());
                }
            }

            // Tulis kembali source file (hanya shift terakhir maxShifts)
            await WriteJsonToFile(sourceFilePath, sourceJson.ToString());
        }


        public async Task ArchiveData(string sourceFilePath, string archiveDirectory)
        {
            try
            {
                if (!Directory.Exists(archiveDirectory))
                {
                    _ = Directory.CreateDirectory(archiveDirectory);
                }

                DateTime previousDay = DateTime.Now.AddDays(-1);

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
                string fileExtension = Path.GetExtension(sourceFilePath);
                string newFileName =
                    $"History_{fileNameWithoutExtension}_DT-{baseOutlet}_{previousDay:yyyyMMdd}{fileExtension}";
                string destinationPath = Path.Combine(archiveDirectory, newFileName);

                if (File.Exists(destinationPath))
                {
                    string existingData = await ReadJsonFileAsync(destinationPath);
                    JObject destinationJson = JObject.Parse(existingData);
                    JArray destinationData = (JArray)destinationJson["data"];

                    string sourceData = await ReadJsonFileAsync(sourceFilePath);
                    JObject sourceJson = JObject.Parse(sourceData);
                    JArray sourceDataArray = (JArray)sourceJson["data"];

                    foreach (JObject trans in sourceDataArray)
                    {
                        destinationData.Add(trans);
                    }

                    await WriteJsonToFile(destinationPath, destinationJson.ToString());
                }
                else
                {
                    string sourceData = await ReadJsonFileAsync(sourceFilePath);
                    await WriteJsonToFile(destinationPath, sourceData);
                }
            }
            catch(Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error Shift : ", ex.Message);
            }
        }
    }
}