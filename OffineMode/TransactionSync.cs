using KASIR.Network;
using Newtonsoft.Json.Linq;
using KASIR.Helper;

namespace KASIR.OffineMode
{
    // Define a class where your methods will reside
    public class TransactionSync
    {
        private readonly string baseOutlet;
        // Gunakan static SemaphoreSlim sebagai flag global yang dapat diakses semua kelas
        private static SemaphoreSlim syncSemaphore = new SemaphoreSlim(1, 1);
        public TransactionSync()
        {
            // Assign the value of baseOutlet in the constructor
            baseOutlet = Properties.Settings.Default.BaseOutlet;
        }
        // Property untuk mengecek status sinkronisasi di seluruh aplikasi
        public static bool IsSyncing
        {
            get => syncSemaphore.CurrentCount == 0;
        }

        // Method untuk memulai sinkronisasi - mengembalikan true jika berhasil mendapatkan lock
        public static async Task<bool> BeginSyncAsync(int maxRetries = 3, int delayBetweenRetries = 1000)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Tunggu dengan timeout untuk mendapatkan lock
                    bool lockAcquired = await syncSemaphore.WaitAsync(TimeSpan.FromSeconds(5));
                    if (lockAcquired)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogWarning($"Sync attempt {attempt + 1} failed: {ex.Message}");
                }

                // Jeda antara percobaan
                await Task.Delay(delayBetweenRetries);
            }

            LoggerUtil.LogWarning("Failed to acquire sync lock after multiple attempts");
            return false;
        }
        // Method untuk menyelesaikan sinkronisasi
        public static void EndSync()
        {
            try
            {
                // Hanya release jika semaphore sedang dikunci
                if (syncSemaphore.CurrentCount == 0)
                {
                    syncSemaphore.Release();
                }
            }
            catch (SemaphoreFullException)
            {
                // Abaikan jika semaphore sudah di-release sebelumnya
                LoggerUtil.LogWarning("Attempted to release already released semaphore");
            }
            catch (Exception ex)
            {
                // Log exception lain yang mungkin terjadi
                LoggerUtil.LogError(ex, "Unexpected error releasing sync lock: {ErrorMessage}", ex.Message);
            }
        }

        // Method async untuk memulai sinkronisasi dalam operasi async
        public static async Task<bool> BeginSyncAsync()
        {
            return await syncSemaphore.WaitAsync(0);
        }

        public async Task SyncIndividualTransactions()
        {

            try
            {
                string filePath = "DT-Cache\\Transaction\\transaction.data";
                string newSyncFileTransaction = "DT-Cache\\Transaction\\SyncSuccessTransaction";
                string failedSyncFolder = "DT-Cache\\Transaction\\FailedSyncTransaction";

                // Memastikan direktori tujuan ada
                if (!Directory.Exists(newSyncFileTransaction))
                {
                    Directory.CreateDirectory(newSyncFileTransaction);
                }
                if (!Directory.Exists(failedSyncFolder))
                {
                    Directory.CreateDirectory(failedSyncFolder);
                }

                // Membaca konten file .data yang berisi JSON
                if (!File.Exists(filePath))
                {
                    return;
                }
                // Membuat path lengkap untuk file baru
                string newsynfile = "DT-Cache\\Transaction\\transactionSyncing.data";
                // Memindahkan file dan mengganti nama, paksa overwrite jika sudah ada file dengan nama yang sama
                File.Copy(filePath, newsynfile, true);  // true untuk overwrite file yang sudah ada

                // Baca file JSON
                string jsonData = File.ReadAllText(newsynfile);
                JObject data = JObject.Parse(jsonData);

                // Dapatkan array "data" (transaksi)
                JArray transactions = (JArray)data["data"];
                if (transactions == null || transactions.Count == 0)
                {
                    return; // Jika tidak ada transaksi
                }

                // Membuat array untuk menyimpan transaksi yang disederhanakan
                JArray simplifiedTransactions = new JArray();
                bool success = false;
                // Memproses setiap transaksi secara individual
                foreach (JToken transaction in transactions) // Use JToken instead of JObject
                {
                    JObject transactionObject = (JObject)transaction; // Cast JToken to JObject

                    // Jika is_sent_sync masih 0, kirimkan transaksi ini ke API
                    if (transaction["is_sent_sync"] != null && (int)transaction["is_sent_sync"] == 0)
                    {
                        JObject simplifiedTransaction = SimplifyTransactionData(transaction); // Simplify the transaction data

                        simplifiedTransactions.Add(simplifiedTransaction); // Add the simplified transaction to the array
                                                                           // Simpan transaksi yang berhasil disinkronkan ke file SyncSuccessTransaction
                        JObject newData1 = new JObject();
                        newData1["data"] = simplifiedTransactions; // Wrap simplified transactions in "data"
                        // Lakukan pengiriman transaksi ke API dan handle response
                        string apiUrl = "/sync-transactions-outlet?outlet_id=" + baseOutlet;
                        IApiService apiService = new ApiService();
                        HttpResponseMessage response = await apiService.SyncTransaction(newData1.ToString(), apiUrl);
                        NotifyHelper.Error(newData1.ToString());



                        // Log pengiriman sukses atau gagal
                        if (response.IsSuccessStatusCode)
                        {
                            // Jika berhasil, perbarui status is_sent_sync menjadi 1
                            transaction["is_sent_sync"] = 1;
                            //SyncSuccess(filePath);
                            success = true;
                            // Buat path folder dan file untuk SyncSuccess
                            string newFileName = $"{baseOutlet}_SyncSuccess_{DateTime.Now:yyyyMMdd}.data";
                            string destinationPath = Path.Combine(newSyncFileTransaction, newFileName);

                            // Cek apakah file sudah ada, jika ada, baca file dan tambahkan transaksi baru
                            if (File.Exists(destinationPath))
                            {
                                string existingData = File.ReadAllText(destinationPath);
                                JObject existingDataJson = JObject.Parse(existingData);
                                JArray existingTransactions = (JArray)existingDataJson["data"];

                                // Menambahkan transaksi baru ke dalam data yang ada
                                existingTransactions.Add(transaction);
                                existingDataJson["data"] = existingTransactions;

                                // Simpan kembali file yang sudah diperbarui
                                File.WriteAllText(destinationPath, existingDataJson.ToString());
                            }
                            else
                            {
                                // Jika file belum ada, buat file baru dengan transaksi
                                JObject newData = new JObject();
                                newData["data"] = new JArray(transaction);

                                // Simpan data ke file baru
                                File.WriteAllText(destinationPath, newData.ToString());
                            }

                        }
                        else
                        {
                            NotifyHelper.Error(response.ToString());
                            // Jika gagal, pindahkan ke folder FailedSyncTransaction
                            string failedFileName = $"{baseOutlet}_FailedSync_{DateTime.Now:yyyyMMdd}.data";
                            string failedPath = Path.Combine(failedSyncFolder, failedFileName);

                            // Cek apakah file sudah ada, jika ada, baca file dan tambahkan transaksi baru
                            if (File.Exists(failedPath))
                            {
                                string existingFailedData = File.ReadAllText(failedPath);
                                JObject existingFailedDataJson = JObject.Parse(existingFailedData);
                                JArray failedTransactions = (JArray)existingFailedDataJson["data"];

                                // Menambahkan transaksi gagal ke dalam data yang ada
                                failedTransactions.Add(transaction);
                                existingFailedDataJson["data"] = failedTransactions;

                                // Simpan kembali file yang sudah diperbarui
                                File.WriteAllText(failedPath, existingFailedDataJson.ToString());
                            }
                            else
                            {
                                // Jika file gagal belum ada, buat file baru dengan transaksi gagal
                                JObject newFailedData = new JObject();
                                newFailedData["data"] = new JArray(transaction);

                                // Simpan data ke file baru
                                File.WriteAllText(failedPath, newFailedData.ToString());
                            }
                        }
                    }
                }
                if (success == true)
                {
                    SyncSuccess(filePath);

                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred during transaction synchronization: {ErrorMessage}", ex.Message);
            }
        }

        public static JObject SimplifyTransactionData(JToken transaction)
        {
            // Simplify data to match the required format for the API
            JObject simplifiedTransaction = new JObject();

            // Add only necessary fields for the API request
            simplifiedTransaction["transaction_id"] = transaction["transaction_id"];
            simplifiedTransaction["receipt_number"] = transaction["receipt_number"];
            simplifiedTransaction["transaction_ref"] = transaction["transaction_ref"];
            simplifiedTransaction["invoice_number"] = transaction["invoice_number"];
            simplifiedTransaction["invoice_due_date"] = transaction["invoice_due_date"];
            simplifiedTransaction["payment_type_id"] = transaction["payment_type_id"];
            simplifiedTransaction["payment_type_name"] = transaction["payment_type_name"];
            simplifiedTransaction["customer_name"] = transaction["customer_name"];
            simplifiedTransaction["customer_seat"] = transaction["customer_seat"];
            simplifiedTransaction["customer_cash"] = transaction["customer_cash"];
            simplifiedTransaction["customer_change"] = transaction["customer_change"];
            simplifiedTransaction["total"] = transaction["total"];
            simplifiedTransaction["subtotal"] = transaction["subtotal"];
            simplifiedTransaction["created_at"] = transaction["created_at"];
            simplifiedTransaction["updated_at"] = transaction["updated_at"];
            simplifiedTransaction["deleted_at"] = transaction["deleted_at"];
            simplifiedTransaction["is_refund"] = transaction["is_refund"];
            simplifiedTransaction["refund_reason"] = transaction["refund_reason"];
            simplifiedTransaction["delivery_type"] = transaction["delivery_type"];
            simplifiedTransaction["delivery_note"] = transaction["delivery_note"];
            simplifiedTransaction["discount_id"] = transaction["discount_id"];
            simplifiedTransaction["discount_code"] = transaction["discount_code"];
            simplifiedTransaction["discounts_value"] = transaction["discounts_value"];
            simplifiedTransaction["discounts_is_percent"] = transaction["discounts_is_percent"];
            simplifiedTransaction["member_name"] = transaction["member_name"];
            simplifiedTransaction["member_phone_number"] = transaction["member_phone_number"];
            simplifiedTransaction["is_refund_all"] = transaction["is_refund_all"];
            simplifiedTransaction["refund_reason_all"] = transaction["refund_reason_all"];
            simplifiedTransaction["refund_payment_id_all"] = transaction["refund_payment_id_all"];
            simplifiedTransaction["refund_created_at_all"] = transaction["refund_created_at_all"];
            simplifiedTransaction["total_refund"] = transaction["total_refund"];
            simplifiedTransaction["refund_payment_name_all"] = transaction["refund_payment_name_all"];
            simplifiedTransaction["is_edited_sync"] = transaction["is_edited_sync"];
            simplifiedTransaction["is_sent_sync"] = transaction["is_sent_sync"];

            // You can modify this part to add or remove more fields based on what the API expects

            // Handle cart_details and refund_details
            JArray cartDetails = (JArray)transaction["cart_details"];
            JArray simplifiedCartDetails = new JArray();
            foreach (JObject cartItem in cartDetails)
            {
                JObject simplifiedCartItem = new JObject
                {
                    ["cart_detail_id"] = cartItem["cart_detail_id"],
                    ["menu_id"] = cartItem["menu_id"],
                    ["menu_name"] = cartItem["menu_name"],
                    ["price"] = cartItem["price"],
                    ["qty"] = cartItem["qty"],
                    ["subtotal_price"] = cartItem["subtotal_price"],
                    ["total_price"] = cartItem["total_price"]
                };
                simplifiedCartDetails.Add(simplifiedCartItem);
            }
            simplifiedTransaction["cart_details"] = simplifiedCartDetails;

            return simplifiedTransaction;
        }

        public static void SyncSuccess(string filePath)
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

                    if (transaction["is_sent_sync"] != null && (int)transaction["is_sent_sync"] == 0)
                    {
                        transaction["is_sent_sync"] = 1;
                    }
                }

                // 4. Simpan data yang sudah disederhanakan ke file baru atau file yang sama
                File.WriteAllText(filePath, data.ToString());
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred during SyncSuccess: {ErrorMessage}", ex.Message);
            }
        }
    }
}
