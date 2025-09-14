using System.Net.Sockets;
using System.Text;
using KASIR.Model;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace KASIR
{
    public static class LoggerUtil
    {
        private static readonly Util util = new();
        private static readonly ILogger _log = LoggerService.Instance._log;

        public static void LogError(Exception ex, string message, params object[] properties)
        {
            if (_log == null) return;

            string logCode = GenerateLogCode(ex, message);

            if (ex is SocketException socketEx)
            {
                if (socketEx.ErrorCode == 10048 || socketEx.ErrorCode == 10060 || socketEx.ErrorCode == 10049)
                {
                    _log.Error(
                        $"Ignored SocketException(Bluetooth Connection) ({socketEx.ErrorCode}): {ex}", message,
                        properties);
                    return;
                }
            }

            var payload = CreatePayload(
               new LogEvent(DateTimeOffset.Now, LogEventLevel.Error, ex,
                   new MessageTemplate(message, new List<MessageTemplateToken>()),
                   new List<LogEventProperty>()),
               logCode
           );


            _log.Error(ex, message, properties);

            if(payload.OutletId == "4") { return; }
            _ = SendLogAsync(payload);

        }
        public static void LogInfo(string message)
        {
            if (_log == null) return;

            var payload = CreatePayload(
                new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
                    new MessageTemplate(message, new List<MessageTemplateToken>()),
                    new List<LogEventProperty>()),
                "WA_LOG"
            );

            // override level jadi "INFO" karena backend tidak support "INFORMATION"
            payload.LogLevel = "WARN";

            _log.Information(message);

            _ = SendLogAsync(payload);
        }

        public static void LogPrivateMethod(string methodName)
        {
            if (_log == null) return;
            _log.Information(methodName);
        }

        public static void LogWarning(string message)
        {
            if (_log == null) return;
            _log.Warning(message);
        }

        public static void LogNetwork(string message)
        {
            if (_log == null) return;
            _log.Warning(message);
            util.sendLogTelegramNetworkError(message);
        }

        private static string GenerateLogCode(Exception ex, string message)
        {
            string[] keywords =
            {
                "printer", "network", "connection", "file", "io", "socket",
                "bluetooth", "report", "savebill", "splitbill", "transaction",
                "cart", "masterpos", "regex", "sync", "whatsapp",
            };

            foreach (var keyword in keywords)
            {
                if (message.ToLower().Contains(keyword))
                {
                    return $"{keyword.ToUpper()}_ERROR";
                }
            }

            if (ex != null)
            {
                string exceptionType = ex.GetType().Name.Replace("Exception", "").ToUpper();
                return $"{exceptionType}_ERROR";
            }

            try
            {
                var stackTrace = new System.Diagnostics.StackTrace(ex, true);
                var frame = stackTrace.GetFrames()?.FirstOrDefault(
                    f => f.GetMethod()?.DeclaringType?.Namespace?.StartsWith("KASIR") ?? false
                );

                if (frame != null)
                {
                    string className = frame.GetMethod()?.DeclaringType?.Name ?? "UNKNOWN";
                    return $"{className.ToUpper()}_ERROR";
                }
            }
            catch { }

            return "GENERIC_ERROR";
        }

        public class LogPayload
        {
            [JsonProperty("outlet_id")]
            public string OutletId { get; set; } = "UNKNOWN";

            [JsonProperty("outlet_name")]
            public string OutletName { get; set; } = "UNKNOWN_OUTLET";

            [JsonProperty("log_code")]
            public string LogCode { get; set; } = "GENERIC_ERROR";

            [JsonProperty("log_level")]
            public string LogLevel { get; set; } = "ERROR";

            [JsonProperty("message")]
            public string Message { get; set; } = "No specific message";

            [JsonProperty("exception")]
            public string Exception { get; set; } = string.Empty;

            [JsonProperty("source")]
            public string Source { get; set; } = string.Empty;

            [JsonProperty("additional_info")]
            public string AdditionalInfo { get; set; } = string.Empty;

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

        private static LogPayload CreatePayload(LogEvent logEvent, string logCode = "GENERIC_ERROR")
        {
            string exception = logEvent.Exception?.ToString() ?? string.Empty;

            string message = logEvent.RenderMessage();
            if (string.IsNullOrWhiteSpace(message))
                message = logEvent.Exception?.Message ?? "No specific message";

            string logLevel = logEvent.Level.ToString().ToUpper();
            if (string.IsNullOrWhiteSpace(logLevel))
                logLevel = "ERROR";

            string source = "UNKNOWN_SOURCE";
            if (logEvent.Exception != null)
            {
                var trace = new System.Diagnostics.StackTrace(logEvent.Exception, true);
                var frame = trace.GetFrames()?.FirstOrDefault(
                    f => f.GetMethod()?.DeclaringType?.Namespace?.StartsWith("KASIR") ?? false
                );
                source = frame?.GetMethod()?.DeclaringType?.FullName ?? "UNKNOWN_SOURCE";
            }

            string additionalInfo = string.Empty;
            if (!string.IsNullOrEmpty(message) && message.Contains(":"))
            {
                var parts = message.Split(' ');
                additionalInfo = parts.Length > 0 ? parts[^1] : string.Empty;
            }

            string outletName = "UNKNOWN_OUTLET";
            string outletID = "UNKNOWN";
            try
            {
                outletID = Settings.Default.BaseOutlet ?? "UNKNOWN";
                string cacheOutlet = $"DT-Cache\\DataOutlet{outletID}.data";
                if (File.Exists(cacheOutlet))
                {
                    string cacheData = File.ReadAllText(cacheOutlet);
                    CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheData);
                    if (dataOutlet?.data != null)
                    {
                        outletName = dataOutlet.data.name ?? "UNKNOWN_OUTLET";
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"Error getting outlet info: {ex.Message}");
            }

            var payload = new LogPayload
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

            ValidatePayload(payload);
            return payload;
        }

        private static async Task SendLogAsync(LogPayload payload)
        {
            try
            {
                string baseUri = Properties.Settings.Default.BaseAddress;
                using var client = new HttpClient();

                // langsung pakai JsonProperty (tanpa SnakeCaseNamingStrategy)
                var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                _log.Warning($"Sending Log Payload: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUri + "/logs", content);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _log.Warning($"Failed to send log to server: {response.StatusCode}");
                    _log.Warning($"Response Body: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"Error sending log: {ex.Message}");
                _log.Warning($"Stack Trace: {ex.StackTrace}");
            }
        }

        private static void ValidatePayload(LogPayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload.OutletId))
                payload.OutletId = "UNKNOWN";

            if (string.IsNullOrWhiteSpace(payload.OutletName))
                payload.OutletName = "UNKNOWN_OUTLET";

            if (string.IsNullOrWhiteSpace(payload.LogCode))
                payload.LogCode = "GENERIC_ERROR";

            if (string.IsNullOrWhiteSpace(payload.LogLevel))
                payload.LogLevel = "ERROR";

            if (string.IsNullOrWhiteSpace(payload.Message))
                payload.Message = "No specific message";

            if (payload.CreatedAt == default)
                payload.CreatedAt = DateTime.UtcNow;

            // truncate panjang string
            payload.Message = TruncateString(payload.Message, 1000);
            payload.Exception = TruncateString(payload.Exception, 2000);
            payload.Source = TruncateString(payload.Source, 500);
            payload.AdditionalInfo = TruncateString(payload.AdditionalInfo, 500);
        }

        private static string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }
    }
}
