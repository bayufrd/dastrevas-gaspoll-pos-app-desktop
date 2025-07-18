using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json;


namespace KASIR.Services
{
    public class AppSettings
    {
        [JsonProperty("baseOutlet")]
        public string BaseOutlet { get; set; }

        [JsonProperty("baseOutletName")]
        public string BaseOutletName { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("footerText")]
        public string FooterText { get; set; } = "TERIMA KASIH ATAS KUNJUNGANNYA";

        [JsonProperty("runningText")]
        public string RunningText { get; set; } = "TERIMA KASIH ATAS KUNJUNGANNYA";

        [JsonProperty("dualMonitorEnabled")]
        public bool DualMonitorEnabled { get; set; } = false;

        [JsonProperty("offlineModeEnabled")]
        public bool OfflineModeEnabled { get; set; } = false;

        [JsonProperty("listMenuEnabled")]
        public bool ListMenuEnabled { get; set; } = false;

        [JsonProperty("printers")]
        public List<PrinterConfig> Printers { get; set; } = new List<PrinterConfig>();
    }

    public class PrinterConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("macAddress")]
        public string MacAddress { get; set; }

        [JsonProperty("isKasirPrinterEnabled")]
        public bool IsKasirPrinterEnabled { get; set; }

        [JsonProperty("isCheckerPrinterEnabled")]
        public bool IsCheckerPrinterEnabled { get; set; }

        [JsonProperty("isMakananPrinterEnabled")]
        public bool IsMakananPrinterEnabled { get; set; }

        [JsonProperty("isMinumanPrinterEnabled")]
        public bool IsMinumanPrinterEnabled { get; set; }
    }
}
