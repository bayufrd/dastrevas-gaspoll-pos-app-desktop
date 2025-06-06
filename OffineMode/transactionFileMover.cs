using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace KASIR.OffineMode
{
    public class transactionFileMover
    {
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet;

        public async Task refreshCacheTransaction()
        {
            string sourceDirectory = "DT-Cache\\Transaction\\transaction.data"; // Path to source
            string transactionHistoryDirectory = "DT-Cache\\Transaction\\HistoryTransaction"; // Path to transaction history

            try
            {
                // 1. Check if the source file exists
                if (!File.Exists(sourceDirectory))
                {
                    return; // Exit if the file does not exist
                }

                // 2. Read the source file content
                string jsonData = await ReadJsonFileAsync(sourceDirectory);
                JObject data = JObject.Parse(jsonData);

                // 3. Get the "data" array of transactions
                JArray transactions = (JArray)data["data"];
                if (transactions == null || transactions.Count == 0)
                {
                    return; // Exit if no transactions exist
                }

                // 4. Check if the first transaction date is not today's date (and it's after 6 AM)
                DateTime? firstTransactionDate = GetFirstTransactionDate(transactions);
                DateTime currentDateTime = DateTime.Now;

                if (firstTransactionDate.HasValue &&
                    firstTransactionDate.Value.Date != currentDateTime.Date &&
                    currentDateTime.Hour >= 6)
                {
                    // 5. Archive transaction data
                    await ArchiveTransactionData(transactions, transactionHistoryDirectory, jsonData);

                    // 6. Clear the source file by writing an empty array
                    await ClearSourceFile(sourceDirectory);

                    // 7. Archive shift data
                    await ArchiveData("DT-Cache\\Transaction\\shiftData.data", "DT-Cache\\Transaction\\ShiftDataTransaction");

                    // 8. Archive expenditure data
                    await ArchiveData("DT-Cache\\Transaction\\expenditure.data", "DT-Cache\\Transaction\\ExpendituresTransaction");

                    await ClearSourceFile("DT-Cache\\Transaction\\shiftData.data");

                    await ClearSourceFile("DT-Cache\\Transaction\\expenditure.data");

                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Unexpected error in refreshCacheTransaction: {ErrorMessage}", ex.Message);
            }
        }

        public async Task<string> ReadJsonFileAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(sourceStream))
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
                createdAt = Regex.Replace(createdAt, @"(\d)\.(\d)", "$1:$2");  // Replace dot with colon for time format

                if (DateTime.TryParseExact(createdAt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
            }
            return null;
        }

        private async Task ArchiveTransactionData(JArray transactions, string historyDirectory, string jsonData)
        {
            DateTime previousDay = DateTime.Now.AddDays(-1);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension("DT-Cache\\Transaction\\transaction.data");
            string fileExtension = Path.GetExtension("DT-Cache\\Transaction\\transaction.data");
            string newFileName = $"History_{fileNameWithoutExtension}_DT-{baseOutlet}_{previousDay:yyyyMMdd}{fileExtension}";
            string destinationPath = Path.Combine(historyDirectory, newFileName);

            // Ensure destination directory exists
            if (!Directory.Exists(historyDirectory))
            {
                Directory.CreateDirectory(historyDirectory);
            }

            if (File.Exists(destinationPath))
            {
                // Merge transactions into the existing file
                await MergeTransactionsToFile(destinationPath, transactions);
            }
            else
            {
                // Create a new file with the transactions
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
            using (FileStream writeStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(writeStream))
            {
                await writer.WriteAsync(jsonData);
            }
        }

        public async Task ClearSourceFile(string sourcePath)
        {
            JObject emptyJson = new JObject();
            emptyJson["data"] = new JArray();

            try
            {
                // Try to write the empty array to the source file
                await WriteJsonToFile(sourcePath, emptyJson.ToString());
            }
            catch (IOException ex)
            {
                LoggerUtil.LogError(ex, $"Could not clear the source file: {ex.Message}");
            }
        }

        public async Task ArchiveData(string sourceFilePath, string archiveDirectory)
        {
            // Ensure destination directory exists
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }
            // Get current date and subtract one day
            DateTime previousDay = DateTime.Now.AddDays(-1);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            string fileExtension = Path.GetExtension(sourceFilePath);
            string newFileName = $"History_{fileNameWithoutExtension}_DT-{baseOutlet}_{previousDay:yyyyMMdd}{fileExtension}";
            string destinationPath = Path.Combine(archiveDirectory, newFileName);

            if (File.Exists(destinationPath))
            {
                // Merge data into the existing file
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
                // Write the data to a new file
                string sourceData = await ReadJsonFileAsync(sourceFilePath);
                await WriteJsonToFile(destinationPath, sourceData);
            }
        }

    }
}
