using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.File;

namespace KASIR
{
    public class LoggerService
    {
        private static readonly LoggerService _instance = new LoggerService();
        public static LoggerService Instance => _instance;
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet;
        string outletName;

        public ILogger _log { get; private set; }

        private LoggerService()
        {
            ConfigureLogger();
        }

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
                    var cacheData = File.ReadAllText(cacheOutlet);

                    // Mend deserialisasi JSON ke objek CartDataOutlet
                    var dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheData);

                    if (dataOutlet != null && dataOutlet.data != null)
                    {
                        outletName = dataOutlet.data.name;  // Mengambil nama outlet dari data yang deserialized
                    }
                }

                // Mendapatkan outletID dari settings
                string outletID = Properties.Settings.Default.BaseOutlet.ToString();

                // Menggabungkan outletID dan outletName
                string customText = $"{outletID}_{outletName}_";  // Menggunakan interpolasi string untuk format yang lebih bersih

                _log = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "{yyyy-MM-dd HH:mm:ss} [{Level:u3}]\r\n {Message} \n\r{PropertyName}{NewLine}\n\r{Exception}")
                    .WriteTo.File($"log\\{customText}log.txt", rollingInterval: RollingInterval.Day)

                    .CreateLogger();
                //File.Copy(LogCacheData, $"log\\{baseOutlet}_{outletName}_log{formatDate}CloningSent.txt");
            }catch(Exception ex)
            {
                return;
            }
            }
    }
}
