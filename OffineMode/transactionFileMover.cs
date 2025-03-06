using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using SharpCompress.Common;
using Newtonsoft.Json.Linq;
using System.Transactions;

namespace KASIR.OffineMode
{
    public class transactionFileMover
    {

        public static void MoveFilesCreatedAfter(string baseOutlet, string sourceDirectory, string destinationDirectory, TimeSpan timeSpan)
        {
            try
            {
                // Pastikan direktori tujuan ada
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }
                if (!File.Exists(sourceDirectory))
                {
                    return;
                }

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceDirectory);
                string fileExtension = Path.GetExtension(sourceDirectory);

                // Menambahkan timestamp atau informasi lainnya ke nama file
                string newFileName = $"History_{fileNameWithoutExtension}_DT-{baseOutlet}_{DateTime.Now:yyyyMMdd}{fileExtension}";

                // Tentukan path baru untuk file
                string destinationPath = Path.Combine(destinationDirectory, newFileName);

                if (File.Exists(destinationPath))
                {
                    // 1. Baca file JSON
                    string jsonData = File.ReadAllText(sourceDirectory);
                    JObject data = JObject.Parse(jsonData);

                    // 2. Dapatkan array "data"
                    JArray transactions = (JArray)data["data"];
                    string existingData = File.ReadAllText(sourceDirectory);
                    JObject existingDataJson = JObject.Parse(existingData);
                    JArray existingTransactions = (JArray)existingDataJson["data"];

                    // Menambahkan transaksi baru ke dalam data yang ada
                    existingTransactions.Add(transactions);
                    existingDataJson["data"] = existingTransactions;

                    // Simpan kembali file yang sudah diperbarui
                    File.WriteAllText(destinationPath, existingDataJson.ToString());
                    File.Delete(sourceDirectory);
                }
                else
                {
                    // Pindahkan file ke direktori tujuan
                    File.Move(sourceDirectory, destinationPath);
                }

                /*  // Dapatkan informasi file
                  FileInfo fileInfo = new FileInfo(sourceDirectory);

                  // Cek apakah file dibuat lebih dari 25 jam yang lalu
                  if (DateTime.Now - fileInfo.CreationTime > timeSpan)
                  {
                      string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceDirectory);
                      string fileExtension = Path.GetExtension(sourceDirectory);

                      // Menambahkan timestamp atau informasi lainnya ke nama file
                      string newFileName = $"History_{fileNameWithoutExtension}_DT-{baseOutlet}_{DateTime.Now:yyyyMMdd_HHmmss}{fileExtension}";

                      // Tentukan path baru untuk file
                      string destinationPath = Path.Combine(destinationDirectory, newFileName);

                      // Pindahkan file ke direktori tujuan
                      File.Move(sourceDirectory, destinationPath);
                  }
              */
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "A null reference error occurred: {ErrorMessage}", ex.Message);
            }
        }
    }
}
