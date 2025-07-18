using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KASIR.Network;
using KASIR.Properties;
using Newtonsoft.Json;
using Serilog;

namespace KASIR.Services
{
    public class OutletService
    {
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet.ToString();
        private readonly IInternetService _internetServices;
        private readonly IApiService _apiService;
        private readonly ILogger _logger;

        public OutletService(
            string baseOutlet,
            IInternetService internetServices,
            IApiService apiService,
            ILogger logger)
        {
            this.baseOutlet = baseOutlet;
            _internetServices = internetServices;
            _apiService = apiService;
            _logger = logger;
        }

        private string GetCacheFilePath() =>
            $"DT-Cache\\DataOutlet{baseOutlet}.data";

        public async Task<string> GetOutletNameAsync()
        {
            try
            {
                string cacheFilePath = GetCacheFilePath();

                // 1. Try to get from cache first
                if (File.Exists(cacheFilePath))
                {
                    string cachedName = TryGetOutletNameFromFile(cacheFilePath);
                    if (!string.IsNullOrWhiteSpace(cachedName))
                    {
                        return cachedName;
                    }
                }

                // 2. Check internet connection
                if (!_internetServices.IsInternetConnected())
                {
                    return GetDefaultOutletName();
                }

                // 3. Try to get from API
                string response = await _apiService.CekShift($"/outlet/{baseOutlet}");

                if (!string.IsNullOrWhiteSpace(response))
                {
                    // Save full response to cache
                    await SaveOutletNameToCacheAsync(response);

                    // Extract and return name
                    var outletData = JsonConvert.DeserializeObject<OutletApiResponse>(response, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });

                    return outletData?.data?.name ?? GetDefaultOutletName();
                }

                // 4. Fallback to default
                return GetDefaultOutletName();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get outlet name: {ErrorMessage}", ex.Message);
                return GetDefaultOutletName();
            }
        }

        private string TryGetOutletNameFromFile(string filePath)
        {
            try
            {
                string fileContent = File.ReadAllText(filePath);
                var outletData = JsonConvert.DeserializeObject<OutletApiResponse>(fileContent);

                return outletData?.data?.name ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Gagal membaca nama outlet dari file cache", ex.Message);
                return string.Empty;
            }
        }

        private async Task<string> TryGetOutletNameFromApiAsync()
        {
            try
            {
                string endpoint = $"/outlet/{baseOutlet}";
                string response = await _apiService.CekShift(endpoint);

                if (string.IsNullOrWhiteSpace(response))
                {
                    _logger.Warning("Empty response received from outlet API");
                    return string.Empty;
                }

                var outletData = JsonConvert.DeserializeObject<OutletApiResponse>(response, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });

                if (outletData?.data == null)
                {
                    _logger.Warning("Unable to parse outlet data from response");
                    return string.Empty;
                }

                return outletData.data.name ?? string.Empty;
            }
            catch (JsonException jsonEx)
            {
                _logger.Error(jsonEx, "JSON parsing error when fetching outlet name: {ErrorMessage}", jsonEx.Message);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to retrieve outlet name from API: {ErrorMessage}", ex.Message);
                return string.Empty;
            }
        }

        private async Task SaveOutletNameToCacheAsync(string response)
        {
            try
            {
                string cacheFilePath = GetCacheFilePath();

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(cacheFilePath));

                // Parse the full API response and save it completely
                var outletData = JsonConvert.DeserializeObject<OutletApiResponse>(response, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });

                if (outletData?.data == null)
                {
                    _logger.Warning("No outlet data to save to cache");
                    return;
                }

                await File.WriteAllTextAsync(
                    cacheFilePath,
                    JsonConvert.SerializeObject(outletData, Formatting.Indented)
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save outlet data to cache: {ErrorMessage}", ex.Message);
            }
        }

        private string GetDefaultOutletName()
        {
            // Gunakan BaseOutletName dari Settings
            return Settings.Default.BaseOutletName ?? "Outlet Tidak Dikenal";
        }

        // Model untuk deserialize
        private class OutletApiResponse
        {
            public OutletData data { get; set; }
        }

        private class OutletData
        {
            public int? id { get; set; }
            public string name { get; set; }
            public string address { get; set; }
            public int? pin { get; set; }
            public string phone_number { get; set; }
            public int? is_kitchen_bar_merged { get; set; }
            public string footer { get; set; }
        }
    }
}
