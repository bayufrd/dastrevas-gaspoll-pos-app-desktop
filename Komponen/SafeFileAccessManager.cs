using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Komponen
{
    public class SafeFileAccessManager
    {
        private static readonly SemaphoreSlim _fileSemaphore = new SemaphoreSlim(1, 1);
        private static readonly object _lockObject = new object();

        public static async Task<T> ReadFileAsync<T>(string filePath, Func<string, T> deserializeFunc)
        {
            await _fileSemaphore.WaitAsync();
            try
            {
                // Gunakan FileShare untuk izinkan berbagi akses
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string content = await streamReader.ReadToEndAsync();
                    return deserializeFunc(content);
                }
            }
            catch (IOException ex)
            {
                LoggerUtil.LogError(ex, $"IO Error reading file {filePath}: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error reading file {filePath}: {ex.Message}");
                return default;
            }
            finally
            {
                _fileSemaphore.Release();
            }
        }

        public static async Task WriteFileAsync(string filePath, string content)
        {
            await _fileSemaphore.WaitAsync();
            try
            {
                // Pastikan direktori ada
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Tulis dengan mekanisme atomic
                string tempFilePath = filePath + ".tmp";

                await File.WriteAllTextAsync(tempFilePath, content);

                // Ganti file secara atomic
                File.Replace(tempFilePath, filePath, null);
            }
            catch (IOException ex)
            {
                LoggerUtil.LogError(ex, $"IO Error writing file {filePath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error writing file {filePath}: {ex.Message}");
            }
            finally
            {
                _fileSemaphore.Release();
            }
        }
    }
}
