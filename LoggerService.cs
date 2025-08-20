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
            baseOutlet = Settings.Default.BaseOutlet;
            ConfigureLogger();
        }

        public static LoggerService Instance { get; } = new();

        public ILogger _log { get; private set; }

        private async void ConfigureLogger()
        {
            IApiService apiService = new ApiService();
            try
            {
                string outletName = "unknown";
                string outletID = Settings.Default.BaseOutlet;
                string cacheOutlet = $"DT-Cache\\DataOutlet{outletID}.data";
                if (File.Exists(cacheOutlet))
                {
                    string cacheData = File.ReadAllText(cacheOutlet);
                    CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheData);
                    if (dataOutlet != null && dataOutlet.data != null)
                    {
                        outletName = dataOutlet.data.name;
                    }
                }

                string
                    customText =
                        $"{outletID}_{outletName}_"; 

                _log = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate:
                        "{yyyy-MM-dd HH:mm:ss} [{Level:u3}]\r\n {Message} \n\r{PropertyName}{NewLine}\n\r{Exception}")
                    .WriteTo.File($"log\\{customText}log.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
            catch (Exception)
            {
            }
        }
    }
}