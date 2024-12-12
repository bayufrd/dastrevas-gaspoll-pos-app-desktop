using System;
using System.Collections.Generic;
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


                string response = await apiService.CekShift("/shift?outlet_id=" + baseOutlet);
                if (response != null)
                {
                    /* if (response.IsSuccessStatusCode)
                     {
     */


                    GetShift cekShift = JsonConvert.DeserializeObject<GetShift>(response);

                    DataShift datas = cekShift.data;
                    outletName = datas.outlet_name.ToString();


                }
                else
                {
                    outletName = Properties.Settings.Default.BaseOutletName.ToString();
                }
                string outletID = Properties.Settings.Default.BaseOutlet.ToString();

                string customText = outletID + "_" + outletName + "_";
                _log = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "{yyyy-MM-dd HH:mm:ss} [{Level:u3}]\r\n {Message} \n\r{PropertyName}{NewLine}\n\r{Exception}")
                    .WriteTo.File($"log\\{customText}log.txt", rollingInterval: RollingInterval.Day)

                    .CreateLogger();
            }catch(Exception ex)
            {
                return;
            }
            }
    }
}
