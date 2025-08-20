using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;
using Serilog;
using Serilog.Events;
using KASIR.Model;
using KASIR.Properties;
using Serilog.Parsing;

namespace KASIR
{
    public static class LoggerUtil
    {
        private static readonly Util util = new();
        private static readonly ILogger _log = LoggerService.Instance._log;

        public static void LogError(Exception ex, string message, params object[] properties)
        {
            if (_log == null)
            {
                return;
            }

            _log.Error(ex, message, properties);

            //var payload = CreatePayload(
            //    new LogEvent(DateTimeOffset.Now, LogEventLevel.Error, ex, new MessageTemplate(message, new List<MessageTemplateToken>()), new List<LogEventProperty>()),
            //    logCode
            //);

            //_ = SendLogAsync(payload);

            if (ex is SocketException socketEx)
            {
                if (socketEx.ErrorCode == 10048 || socketEx.ErrorCode == 10060 || socketEx.ErrorCode == 10049)
                {
                    _log.Error($"Ignored SocketException(Bluetooth Connection) ({socketEx.ErrorCode}): {ex}", message,
                        properties);
                    return;
                }
            }

            util.sendLogTelegramBy(ex, message, properties);
        }

        public static void LogPrivateMethod(string methodName)
        {
            if (_log == null)
            {
                return;
            }

            _log.Information(methodName);
        }

        public static void LogWarning(string message)
        {
            if (_log == null)
            {
                return;
            }

            _log.Warning(message);
        }

        public static void LogNetwork(string message)
        {
            if (_log == null)
            {
                return;
            }

            _log.Warning(message);
            util.sendLogTelegramNetworkError(message);
        }
        public class LogPayload
        {
            public string OutletId { get; set; }
            public string OutletName { get; set; }
            public string LogCode { get; set; }
            public string LogLevel { get; set; }
            public string Message { get; set; }
            public string Exception { get; set; }
            public string Source { get; set; }
            public string AdditionalInfo { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        private static LogPayload CreatePayload(LogEvent logEvent, string logCode = "GENERIC")
        {
            string exception = logEvent.Exception?.ToString();
            string message = logEvent.RenderMessage();
            string logLevel = logEvent.Level.ToString().ToUpper();

            // Ambil source class/method dari exception stacktrace
            string source = null;
            if (logEvent.Exception != null)
            {
                var trace = new System.Diagnostics.StackTrace(logEvent.Exception, true);
                var frame = trace.GetFrames()?.FirstOrDefault(f => f.GetMethod()?.DeclaringType?.Namespace?.StartsWith("KASIR") ?? false);
                source = frame?.GetMethod()?.DeclaringType?.FullName;
            }

            // AdditionalInfo sederhana, misal ambil terakhir dari pesan
            string additionalInfo = null;
            if (!string.IsNullOrEmpty(message) && message.Contains(":"))
            {
                var parts = message.Split(' ');
                additionalInfo = parts.Length > 0 ? parts[^1] : null;
            }

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


            return new LogPayload
            {
                OutletId = outletID,
                OutletName = outletName,
                LogCode = logCode,
                LogLevel = logLevel,
                Message = message,
                Exception = exception,
                Source = source,
                AdditionalInfo = additionalInfo,
                CreatedAt = logEvent.Timestamp.UtcDateTime
            };
        }

        private static async Task SendLogAsync(LogPayload payload)
        {
            try
            {
                using var client = new HttpClient();
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //var response = await client.PostAsync(_apiEndpoint, content);

                //if (!response.IsSuccessStatusCode)
                //{
                //    _log.Warning($"Failed to send log to server: {response.StatusCode}");
                //}
            }
            catch (Exception ex)
            {
                _log.Warning($"Error sending log: {ex.Message}");
            }
        }
    }
}