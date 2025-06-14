using KASIR.Model;
using KASIR.Network;
using KASIR.Properties;
using Newtonsoft.Json;
using Serilog;

namespace KASIR
{
    public class LoggerService
    {
        private readonly string baseOutlet = Settings.Default.BaseOutlet;

        private LoggerService()
        {
            ConfigureLogger();
        }

        public static LoggerService Instance { get; } = new();

        public ILogger _log { get; private set; }

        private async void ConfigureLogger()
        {
            IApiService apiService = new ApiService();
            try
            {
                // Default value for outletName
                string outletName = "unknown";

                // Membaca file JSON dan mendeserialize jika file ada
                string cacheOutlet = $"DT-Cache\\DataOutlet{baseOutlet}.data";

                // Cek jika file ada sebelum membacanya
                if (File.Exists(cacheOutlet))
                {
                    // Membaca file JSON
                    string cacheData = File.ReadAllText(cacheOutlet);

                    // Mend deserialisasi JSON ke objek CartDataOutlet
                    CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheData);

                    if (dataOutlet != null && dataOutlet.data != null)
                    {
                        outletName = dataOutlet.data.name; // Mengambil nama outlet dari data yang deserialized
                    }
                }

                // Mendapatkan outletID dari settings
                string outletID = Settings.Default.BaseOutlet;

                // Menggabungkan outletID dan outletName
                string
                    customText =
                        $"{outletID}_{outletName}_"; // Menggunakan interpolasi string untuk format yang lebih bersih

                _log = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate:
                        "{yyyy-MM-dd HH:mm:ss} [{Level:u3}]\r\n {Message} \n\r{PropertyName}{NewLine}\n\r{Exception}")
                    .WriteTo.File($"log\\{customText}log.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
                //File.Copy(LogCacheData, $"log\\{baseOutlet}_{outletName}_log{formatDate}CloningSent.txt");
            }
            catch (Exception)
            {
            }
        }
    }
}