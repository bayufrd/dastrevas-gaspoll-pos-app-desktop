using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using SharpCompress.Common;

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

                    // Dapatkan informasi file
                    FileInfo fileInfo = new FileInfo(sourceDirectory);

                    // Cek apakah file dibuat lebih dari 25 jam yang lalu
                    if (DateTime.Now - fileInfo.CreationTime > timeSpan)
                    {
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceDirectory);
                        string fileExtension = Path.GetExtension(sourceDirectory);

                        // Menambahkan timestamp atau informasi lainnya ke nama file
                        string newFileName = $"Success_{fileNameWithoutExtension}_DT-{baseOutlet}_{DateTime.Now:yyyyMMdd_HHmmss}{fileExtension}";

                        // Tentukan path baru untuk file
                        string destinationPath = Path.Combine(destinationDirectory, newFileName);

                        // Pindahkan file ke direktori tujuan
                        File.Move(sourceDirectory, destinationPath);
                    }
                
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "A null reference error occurred: {ErrorMessage}", ex.Message);
            }
        }
    }
}
