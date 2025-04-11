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
            string destinationDirectory = "DT-Cache\\Transaction\\HistoryTransaction"; // Path to destination

            try
            {
                // 1. Check if source file exists
                if (!File.Exists(sourceDirectory))
                {
                    return; // Exit if the file does not exist
                }

                // 2. Read JSON file with proper file sharing
                string jsonData;
                using (FileStream sourceStream = new FileStream(sourceDirectory, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(sourceStream))
                {
                    jsonData = await reader.ReadToEndAsync();
                }

                JObject data = JObject.Parse(jsonData);

                // 3. Get the "data" array
                JArray transactions = (JArray)data["data"];
                if (transactions == null || transactions.Count == 0)
                {
                    return; // Exit if there are no transactions
                }

                // 4. Get the first transaction and check created_at date
                JObject firstTransaction = (JObject)transactions[0];
                DateTime? firstTransactionDate = null;

                if (firstTransaction["created_at"] != null)
                {
                    string createdAt = firstTransaction["created_at"].ToString();
                    createdAt = Regex.Replace(createdAt, @"(\d)\.(\d)", "$1:$2");

                    if (DateTime.TryParseExact(createdAt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime parsedDate))
                    {
                        firstTransactionDate = parsedDate;
                    }
                }

                // 5. If first transaction date is not today and it's after 6 AM
                DateTime currentDateTime = DateTime.Now;

                if (firstTransactionDate.HasValue &&
                    firstTransactionDate.Value.Date != currentDateTime.Date &&
                    currentDateTime.Hour >= 6)
                {
                    // Ensure destination directory exists
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    // Create destination filename
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceDirectory);
                    string fileExtension = Path.GetExtension(sourceDirectory);
                    string newFileName = $"History_{fileNameWithoutExtension}_DT-{baseOutlet}_{DateTime.Now:yyyyMMdd}{fileExtension}";
                    string destinationPath = Path.Combine(destinationDirectory, newFileName);

                    // If destination exists, merge, otherwise create new file
                    if (File.Exists(destinationPath))
                    {
                        // Read destination file
                        string destinationData;
                        using (FileStream destStream = new FileStream(destinationPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (StreamReader destReader = new StreamReader(destStream))
                        {
                            destinationData = await destReader.ReadToEndAsync();
                        }

                        JObject destinationJson = JObject.Parse(destinationData);
                        JArray destinationTransactions = (JArray)destinationJson["data"];

                        // Merge transactions
                        foreach (JObject trans in transactions)
                        {
                            destinationTransactions.Add(trans);
                        }

                        // Write merged data back
                        using (FileStream writeStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        using (StreamWriter writer = new StreamWriter(writeStream))
                        {
                            await writer.WriteAsync(destinationJson.ToString());
                        }
                    }
                    else
                    {
                        // Write to new destination file
                        using (FileStream writeStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        using (StreamWriter writer = new StreamWriter(writeStream))
                        {
                            await writer.WriteAsync(jsonData);
                        }
                    }

                    // Clear the source file by writing an empty array
                    JObject emptyJson = new JObject();
                    emptyJson["data"] = new JArray();

                    try
                    {
                        // Try to write the empty array to the source file
                        using (FileStream writeStream = new FileStream(sourceDirectory, FileMode.Create, FileAccess.Write, FileShare.None))
                        using (StreamWriter writer = new StreamWriter(writeStream))
                        {
                            await writer.WriteAsync(emptyJson.ToString());
                        }
                    }
                    catch (IOException ex)
                    {
                        LoggerUtil.LogError(ex, $"Could not clear the source file: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Unexpected error in refreshCacheTransaction: {ErrorMessage}", ex.Message);
            }
        }
    }
}
