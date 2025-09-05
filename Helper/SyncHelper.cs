using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using KASIR.Model;
using KASIR.Network;
using KASIR.OffineMode;
using KASIR.Printer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using KASIR.Helper;

namespace KASIR.Helper
{
    public class SyncHelper
    {
        private ApiService apiService = new ApiService();
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet;
        int bedaCash = 0;
        int shiftnumber;
        DateTime mulaishift, akhirshift;
        public bool IsBackgroundOperation { get; set; } = false;
        public async Task SyncDataTransactions(bool isBackground = false)
        {
            try
            {
                if (TransactionSync.IsSyncing) // Check if sync is already in progress using the shared manager
                {
                    return; // If sync is already running, exit
                }

                IsBackgroundOperation = isBackground;

                bool canSync = await TransactionSync.BeginSyncAsync();
                if (!canSync)
                {
                    return; // Exit if unable to get lock
                }

                try
                {
                    string filePath = "DT-Cache\\Transaction\\transaction.data";
                    string newSyncFileTransaction = "DT-Cache\\Transaction\\SyncSuccessTransaction";
                    string destinationPath = "DT-Cache\\Transaction\\transactionSyncing.data";

                    if (File.Exists(destinationPath))
                        try { ClearCacheData(destinationPath); }
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
                    string expenditureDataPath = "DT-Cache\\Transaction\\expenditure.data";
                    if (File.Exists(expenditureDataPath))
                    {
                        SyncExpenditureData(expenditureDataPath);
                    }
                    else
                    {
                        LoggerUtil.LogWarning("Skip Expenditure Offline sync");
                    }

                    string shiftDataPath = "DT-Cache\\Transaction\\shiftData.data";
                    if (File.Exists(shiftDataPath))
                    {
                        SyncShiftData(shiftDataPath);
                    }
                    else
                    {
                        LoggerUtil.LogWarning("Skip ShiftData Offline sync");
                    }

                    string membershipDataPath = "DT-Cache\\Transaction\\membershipSyncPoint.data";
                    if (File.Exists(membershipDataPath))
                    {
                        SyncmembershipData(membershipDataPath);
                    }
                    else
                    {
                        LoggerUtil.LogWarning("Skip membership sync");
                    }
                    if (!File.Exists(filePath))
                    {
                        return;
                    }

                    EnsureDirectoryExists(newSyncFileTransaction);

                    // Copy file to destination for processing
                    CopyFileWithStreams(filePath, destinationPath);

                    SimplifyAndSaveData(destinationPath);

                    // When preparing data for sync, create a copy of transaction IDs being synced
                    List<string> transactionIdsBeingSynced = new List<string>();

                    // Process the transactions
                    JObject data = ParseAndRepairJsonFile(destinationPath);
                    JArray transactions = GetTransactionsFromData(data);

                    if (transactions == null || !transactions.Any())
                    {
                        // Delete file if data is empty but continue process
                        try { ClearCacheData(destinationPath); }
                        catch (Exception ex) { LoggerUtil.LogError(ex, "Failed to delete empty file: {ErrorMessage}", ex.Message); }

                        File.WriteAllText(destinationPath, "{\"data\":[]}");
                        transactions = new JArray();
                        data["data"] = transactions;
                        return;
                    }


                    // After parsing the transactions from the JSON file
                    foreach (var transaction in transactions)
                    {
                        // Track which transactions we're syncing
                        transactionIdsBeingSynced.Add(transaction["transaction_ref"].ToString());
                    }
                    // Sync with API
                    HttpResponseMessage response = await apiService.SyncTransaction(data.ToString(), apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        LoggerUtil.LogWarning(
                        $"Sync Transaction Payload Size: {data.ToString().Length} bytes, Timestamp: {DateTime.Now}");
                        string folderCombine = Path.Combine(newSyncFileTransaction, newFileName);

                        //SyncSuccess(destinationPath);
                        SyncSpecificTransactions(destinationPath, transactionIdsBeingSynced);

                        ProcessSuccessfulSync(destinationPath, folderCombine);

                        // Notify main form that sync was successful
                        SyncCompleted?.Invoke();
                        // Update only the transactions we tracked for this sync operation
                        SyncSpecificTransactions(filePath, transactionIdsBeingSynced);
                        //SyncSuccess(filePath);
                    }
                    else
                    {
                        ProcessFailedSync(destinationPath, response, baseOutlet);
                    }
                }
                finally
                {

                    TransactionSync.EndSync();
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
        private void ClearCacheData(string destinationPath)
        {
            try
            {
                if (File.Exists(destinationPath))
                {
                    string currentContent = File.ReadAllText(destinationPath);

                    if (string.IsNullOrWhiteSpace(currentContent) || currentContent != "{\"data\":[]}")
                    {
                        File.WriteAllText(destinationPath, "{\"data\":[]}");
                    }
                }
                else
                {
                    File.WriteAllText(destinationPath, "{\"data\":[]}");
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred while clearing cache data: {ErrorMessage}", ex.Message);
            }
        }

        public async Task SyncmembershipData(string membershipPathFile)
        {
            try
            {
                var memberships = ReadmembershipFromFile(membershipPathFile);

                if (memberships == null || !memberships.Any())
                {
                    return;
                }

                foreach (var membership in memberships)
                {
                    if (membership.is_sync == 0)  // Only send if is_sync is 0
                    {
                        string api = $"/membership-point/{membership.id}";
                        var payload = new
                        {
                            points = membership.points,
                            updated_at = membership.updated_at,
                            transaction_ref = membership.transaction_ref
                        };

                        // Step 3: Send the data to the API
                        var isSuccess = await SendToApiMembers(payload, api, "membership-point");

                        if (isSuccess)
                        {
                            membership.is_sync = 1;
                            //memberships.Remove(membership);
                        }
                    }
                }

                SavemembershipsToFile(memberships, membershipPathFile);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error while sending memberships to API: {ErrorMessage}", ex.Message);
            }
        }
        private async Task SyncShiftData(string ShiftPathFile)
        {
            try
            {
                var Shifts = ReadShiftFromFile(ShiftPathFile);

                if (Shifts == null || !Shifts.Any())
                {
                    return;
                }

                foreach (var Shift in Shifts)
                {
                    if (Shift.is_sync == 0)  // Only send if is_sync is 0
                    {
                        string api = "/struct-shift-sync";
                        var payload = new
                        {
                            outlet_id = Shift.outlet_id,
                            actual_ending_cash = Shift.actual_ending_cash,
                            cash_difference = Shift.cash_difference,
                            start_date = Shift.start_date,
                            end_date = Shift.end_date,
                            created_at = Shift.created_at,
                            updated_at = Shift.updated_at,
                            expected_ending_cash = Shift.expected_ending_cash,
                            total_amount = Shift.total_amount,
                            total_discount = Shift.total_discount,
                            shift_number = Shift.shift_number,
                            casher_name = Shift.casher_name
                        };

                        // Step 3: Send the data to the API
                        var isSuccess = await SendToApiPOST(payload, api, "ShiftData Offline");

                        if (isSuccess)
                        {
                            // If the request was successful, update is_sync to 1
                            Shift.is_sync = 1;
                        }
                    }
                }

                SaveShiftsToFile(Shifts, ShiftPathFile);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error while sending Shifts to API: {ErrorMessage}", ex.Message);
            }
        }
        private async Task SyncExpenditureData(string expenditurePathFile)
        {
            try
            {
                var expenditures = ReadExpendituresFromFile(expenditurePathFile);

                if (expenditures == null || !expenditures.Any())
                {
                    return;
                }

                foreach (var expenditure in expenditures)
                {
                    if (expenditure.is_sync == 0)  // Only send if is_sync is 0
                    {
                        string api = "/expenditure-sync";
                        var payload = new
                        {
                            nominal = expenditure.nominal,
                            description = expenditure.description.ToString(),
                            outlet_id = expenditure.outlet_id,
                            created_at = expenditure.created_at.ToString()
                        };

                        // Step 3: Send the data to the API
                        var isSuccess = await SendToApiPOST(payload, api, "expenditures");

                        if (isSuccess)
                        {
                            // If the request was successful, update is_sync to 1
                            expenditure.is_sync = 1;
                        }
                    }
                }

                SaveExpendituresToFile(expenditures, expenditurePathFile);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error while sending expenditures to API: {ErrorMessage}", ex.Message);
            }
        }
        private async Task<bool> SendToApiMembers(object payload, string api, string DetailsSync)
        {
            try
            {
                IApiService apiService = new ApiService();
                string jsonString = JsonConvert.SerializeObject(payload, Formatting.Indented);

                // Perform the POST request with the payload
                HttpResponseMessage response = await apiService.EditMember(jsonString, api);

                if (response != null)
                {
                    LoggerUtil.LogWarning(
                        $"API Response - Status Code: {response.StatusCode}, " +
                        $"Sync {DetailsSync}, " +
                        $"Payload Size: {jsonString.Length} bytes, " +
                        $"Timestamp: {DateTime.Now}");

                    // Check for specific status codes
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK: // 200
                            return true;
                        case HttpStatusCode.Created: // 201
                            return true;
                        case HttpStatusCode.BadRequest: // 400
                                                        // Optional: Read response content for more details
                            string responseBody = await response.Content.ReadAsStringAsync();
                            LoggerUtil.LogWarning(
                                $"Bad Request Details for {DetailsSync}: {responseBody}");

                            return true;
                        case HttpStatusCode.Unauthorized: // 401
                            LoggerUtil.LogWarning($"Unauthorized access for {DetailsSync}");
                            return false;
                        case HttpStatusCode.Forbidden: // 403
                            LoggerUtil.LogWarning($"Forbidden access for {DetailsSync}");
                            return false;
                        default:
                            LoggerUtil.LogWarning(
                                $"Unhandled status code {response.StatusCode} for {DetailsSync}");
                            return false;
                    }
                }

                LoggerUtil.LogWarning($"No response received for {DetailsSync}");
                return false;
            }
            catch (HttpRequestException ex)
            {
                LoggerUtil.LogError(ex,
                    $"HTTP Request Error while sending {DetailsSync}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex,
                    $"Unexpected error while sending {DetailsSync}: {ex.Message}");
                return false;
            }
        }
        private async Task<bool> SendToApiPOST(object payload, string api, string DetailsSync)
        {
            try
            {
                IApiService apiService = new ApiService();
                string jsonString = JsonConvert.SerializeObject(payload, Formatting.Indented);

                // Perform the POST request with the payload
                HttpResponseMessage response = await apiService.CreateMember(jsonString, api);

                if (response != null)
                {
                    // Log the status code for diagnostic purposes
                    LoggerUtil.LogWarning(
                        $"API Response - Status Code: {response.StatusCode}, " +
                        $"Sync {DetailsSync}, " +
                        $"Payload Size: {jsonString.Length} bytes, " +
                        $"Timestamp: {DateTime.Now}");

                    // Check for specific status codes
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK: // 200
                            return true;
                        case HttpStatusCode.Created: // 201
                            return true;
                        case HttpStatusCode.BadRequest: // 400
                            string responseBody = await response.Content.ReadAsStringAsync();
                            LoggerUtil.LogWarning(
                                $"Bad Request Details for {DetailsSync}: {responseBody}");
                            return false;
                        case HttpStatusCode.Unauthorized: // 401
                            LoggerUtil.LogWarning($"Unauthorized access for {DetailsSync}");
                            return false;
                        case HttpStatusCode.Forbidden: // 403
                            LoggerUtil.LogWarning($"Forbidden access for {DetailsSync}");
                            return false;
                        default:
                            LoggerUtil.LogWarning(
                                $"Unhandled status code {response.StatusCode} for {DetailsSync}");
                            return false;
                    }
                }

                LoggerUtil.LogWarning($"No response received for {DetailsSync}");
                return false;
            }
            catch (HttpRequestException ex)
            {
                LoggerUtil.LogError(ex,
                    $"HTTP Request Error while sending {DetailsSync}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex,
                    $"Unexpected error while sending {DetailsSync}: {ex.Message}");
                return false;
            }
        }
        private List<MemberPointModel> ReadmembershipFromFile(string filePath)
        {
            try
            {
                var fileContent = File.ReadAllText(filePath);
                var MembershipData = JsonConvert.DeserializeObject<DataStructMemberPoint>(fileContent);
                return MembershipData?.data ?? new List<MemberPointModel>();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error reading or deserializing file: {ErrorMessage}", ex.Message);
                return new List<MemberPointModel>();
            }
        }
        private List<ShiftReportData> ReadShiftFromFile(string filePath)
        {
            try
            {
                var fileContent = File.ReadAllText(filePath);
                var ShiftData = JsonConvert.DeserializeObject<DataStructShiftOffline>(fileContent);
                return ShiftData?.data ?? new List<ShiftReportData>();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error reading or deserializing file: {ErrorMessage}", ex.Message);
                return new List<ShiftReportData>();
            }
        }
        private List<ExpenditureStrukShift> ReadExpendituresFromFile(string filePath)
        {
            try
            {
                var fileContent = File.ReadAllText(filePath);
                var expenditureData = JsonConvert.DeserializeObject<ExpenditureData>(fileContent);
                return expenditureData?.data ?? new List<ExpenditureStrukShift>();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error reading or deserializing file: {ErrorMessage}", ex.Message);
                return new List<ExpenditureStrukShift>();
            }
        }
        private void SavemembershipsToFile(List<MemberPointModel> memberships, string membershipPathFile)
        {
            try
            {
                // Hanya simpan data yang belum tersinkronisasi
                var unsyncedMemberships = memberships
                    .Where(m => m.is_sync != 1)
                    .ToList();

                var membershipData = new DataStructMemberPoint
                {
                    data = unsyncedMemberships
                };

                string json = JsonConvert.SerializeObject(membershipData, Formatting.Indented);
                File.WriteAllText(membershipPathFile, json);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error saving memberships to file: {ErrorMessage}", ex.Message);
            }
        }

        private void SaveShiftsToFile(List<ShiftReportData> Shifts, string ShiftPathFile)
        {
            try
            {
                var ShiftData = new DataStructShiftOffline
                {
                    data = Shifts
                };

                string json = JsonConvert.SerializeObject(ShiftData, Formatting.Indented);
                File.WriteAllText(ShiftPathFile, json);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error saving Shifts to file: {ErrorMessage}", ex.Message);
            }
        }
        private void SaveExpendituresToFile(List<ExpenditureStrukShift> expenditures, string expenditurePathFile)
        {
            try
            {
                var expenditureData = new ExpenditureData
                {
                    data = expenditures
                };

                string json = JsonConvert.SerializeObject(expenditureData, Formatting.Indented);
                File.WriteAllText(expenditurePathFile, json);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error saving expenditures to file: {ErrorMessage}", ex.Message);
            }
        }

        private void SyncSpecificTransactions(string filePath, List<string> transactionIds)
        {
            const int maxRetries = 5;
            const int retryDelayMs = 1000; // 1 second delay between retries
            int attemptCount = 0;
            bool success = false;

            while (!success && attemptCount < maxRetries)
            {
                try
                {
                    if (attemptCount > 0)
                    {
                        // Wait before retrying
                        Thread.Sleep(retryDelayMs);
                    }

                    lock (FileLock)
                    {
                        // Read file JSON
                        string jsonData = File.ReadAllText(filePath);
                        JObject data = JObject.Parse(jsonData);
                        // Get "data" array
                        JArray transactions = (JArray)data["data"];
                        // Only mark transactions in our tracked list as synced
                        foreach (JObject transaction in transactions)
                        {
                            if (transaction["transaction_ref"] != null && transactionIds.Contains(transaction["transaction_ref"].ToString()))
                            {
                                transaction["is_sent_sync"] = 1;
                            }
                        }
                        // Save changes
                        File.WriteAllText(filePath, data.ToString());
                    }

                    // If we got here without exception, operation was successful
                    success = true;
                }
                catch (IOException ex) when (IsFileLockException(ex))
                {
                    // Specific handling for file lock issues
                    attemptCount++;
                    LoggerUtil.LogWarning($"File is in use, waiting to retry ({attemptCount}/{maxRetries}): {ex.Message}");

                    // If we've reached max retries, log an error
                    if (attemptCount >= maxRetries)
                    {
                        LoggerUtil.LogWarning(ex + "Maximum retries reached. Failed to sync specific transactions due to file access issues: {ErrorMessage}" + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    // For other exceptions, log and break the retry loop
                    LoggerUtil.LogWarning(ex + "Error updating specific transactions: {ErrorMessage}" + ex.Message);
                    break; // Don't retry on non-file-lock exceptions
                }
            }
        }

        // Helper method to determine if an exception is related to file locking
        private bool IsFileLockException(Exception ex)
        {
            // Common error messages or HResults that indicate file is locked or in use
            return ex.Message.Contains("being used by another process") ||
                   ex.Message.Contains("access is denied") ||
                   ex.HResult == -2147024864 || // 0x80070020 - ERROR_SHARING_VIOLATION
                   ex.HResult == -2147024891;   // 0x80070005 - ERROR_ACCESS_DENIED
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
            try
            {
                // Check if source file exists before attempting to read/copy it
                if (!File.Exists(sourcePath))
                {
                    LoggerUtil.LogWarning($"Source file not found: {sourcePath}");
                    return;
                }

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
                    // Create directory if it doesn't exist
                    string directoryPath = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Copy file using streams
                    CopyFileWithStreams(sourcePath, destinationPath);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                LoggerUtil.LogError(ex, "Error in ProcessSuccessfulSync: {ErrorMessage}", ex.Message);
            }
        }

        private void ProcessFailedSync(string sourcePath, HttpResponseMessage response, string outletId)
        {
            string errorMessage = $"Gagal mengirim data Transactions. API Response: {response.ReasonPhrase}";
            //NotifyHelper.Error(errorMessage);


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

                // Parse dan perbaiki file JSON
                JObject data = ParseAndRepairJsonFile(saveBillDataPathClone);
                JArray transactions = GetTransactionsFromData(data);

                // Cek apakah transaksi kosong
                if (transactions == null || !transactions.Any())
                {
                    // Hapus file jika data kosong
                    try
                    {
                        ClearCacheData(saveBillDataPathClone);
                    }
                    catch (Exception ex)
                    {
                        LoggerUtil.LogError(ex, "Failed to delete empty SaveBill file: {ErrorMessage}", ex.Message);
                    }

                    // Tulis file kosong
                    File.WriteAllText(saveBillDataPathClone, "{\"data\":[]}");
                    transactions = new JArray();
                    data["data"] = transactions;
                    return;
                }

                // When preparing data for sync, create a copy of transaction IDs being synced
                List<string> transactionIdsBeingSynced = transactions
                    .Select(t => t["transaction_ref"]?.ToString())
                    .Where(tr => !string.IsNullOrEmpty(tr))
                    .ToList();

                HttpResponseMessage savebillSync = await apiService.SyncTransaction(data.ToString(), apiUrl);

                if (savebillSync.IsSuccessStatusCode)
                {
                    LoggerUtil.LogWarning(
                    $"Sync SaveBillSync Payload Size: {data.ToString().Length} bytes, Timestamp: {DateTime.Now}");
                    SyncSpecificTransactions(saveBillDataPath, transactionIdsBeingSynced);

                    // Hapus file SaveBill setelah sinkronisasi berhasil
                    try
                    {
                        ClearCacheData(saveBillDataPathClone);
                    }
                    catch (Exception ex)
                    {
                        LoggerUtil.LogError(ex, "Failed to delete SaveBill files after sync: {ErrorMessage}", ex.Message);
                    }
                }
                else
                {
                    // Log detailed error information without throwing an exception
                    var responseBody = await savebillSync.Content.ReadAsStringAsync();
                    LoggerUtil.LogWarning($"SaveBill Sync Failed. " +
                        $"Status: {savebillSync.StatusCode}, " +
                        $"Reason: {savebillSync.ReasonPhrase}, " +
                        $"Response Body: {responseBody}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Log network-related errors
                LoggerUtil.LogError(ex,
                    $"Network error during SaveBill sync: {ex.Message}",
                    new
                    {
                        SaveBillDataPath = saveBillDataPath,
                        ApiUrl = apiUrl
                    });
            }
            catch (Exception ex)
            {
                // Log any other unexpected errors
                LoggerUtil.LogError(ex,
                    $"Unexpected error during SaveBill sync: {ex.Message}",
                    new
                    {
                        SaveBillDataPath = saveBillDataPath,
                        ApiUrl = apiUrl
                    });
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
            int maxRetries = 3;
            int delayBetweenRetries = 500; // milliseconds

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Use FileShare to allow other processes to read the file
                    using (FileStream fileStream = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite))
                    {
                        // 1. Baca file JSON
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            string jsonData = reader.ReadToEnd();
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

                            // 4. Simpan data yang sudah disederhanakan
                            fileStream.SetLength(0); // Truncate the file
                            fileStream.Position = 0;

                            using (StreamWriter writer = new StreamWriter(fileStream))
                            {
                                writer.Write(data.ToString());
                            }
                        }
                    }

                    // If successful, break out of retry loop
                    break;
                }
                catch (IOException ex)
                {
                    // Log the specific attempt failure
                    LoggerUtil.LogError(ex, $"File access attempt {attempt + 1} failed: {ex.Message}");

                    // If it's the last attempt, rethrow the exception
                    if (attempt == maxRetries - 1)
                    {
                        throw;
                    }

                    // Wait before retrying
                    System.Threading.Thread.Sleep(delayBetweenRetries);
                }
                catch (Exception ex)
                {
                    // For non-IO exceptions, log and rethrow immediately
                    LoggerUtil.LogError(ex, "An unexpected error occurred: {ErrorMessage}", ex.Message);
                    throw;
                }
            }
        }
        private async Task<string> GetShiftData(string configOfflineMode)
        {
            IApiService apiService = new ApiService();
            return await apiService.CekShift("/shift?outlet_id=" + baseOutlet);
        }
    }
}
