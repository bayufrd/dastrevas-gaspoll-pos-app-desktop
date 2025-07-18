using System.Net.NetworkInformation;
using KASIR.Model;
using KASIR.Properties;
using Newtonsoft.Json;

namespace KASIR.Services
{
    public class AppSettingsService
    {
        private const string SETTINGS_FILE_PATH = "setting\\app_settings.data";

        public async Task<AppSettings> LoadSettingsAsync()
        {
            try
            {
                // Jika file tidak ada, buat default settings
                if (!File.Exists(SETTINGS_FILE_PATH))
                {
                    var defaultSettings = new AppSettings
                    {
                        BaseOutlet = Settings.Default.BaseOutlet,
                        BaseOutletName = Settings.Default.BaseOutletName,
                        Version = Settings.Default.Version
                    };
                    await SaveSettingsAsync(defaultSettings);
                    return defaultSettings;
                }

                // Baca file settings
                string jsonContent = await File.ReadAllTextAsync(SETTINGS_FILE_PATH);
                var settings = JsonConvert.DeserializeObject<AppSettings>(jsonContent);

                return settings ?? CreateDefaultSettings();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal memuat pengaturan");
                return CreateDefaultSettings();
            }
        }
        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                // Pastikan direktori ada
                Directory.CreateDirectory(Path.GetDirectoryName(SETTINGS_FILE_PATH));

                // Simpan sebagai JSON
                string jsonContent = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await File.WriteAllTextAsync(SETTINGS_FILE_PATH, jsonContent);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal menyimpan pengaturan");
                throw;
            }
        }

        private AppSettings CreateDefaultSettings()
        {
            return new AppSettings
            {
                BaseOutlet = Settings.Default.BaseOutlet,
                BaseOutletName = Settings.Default.BaseOutletName,
                Version = Settings.Default.Version
            };
        }

        // Metode utilitas untuk mengupdate setting tertentu
        public async Task UpdateSettingAsync<T>(string propertyName, T value)
        {
            var settings = await LoadSettingsAsync();
            var property = typeof(AppSettings).GetProperty(propertyName);

            if (property == null)
                throw new ArgumentException($"Properti '{propertyName}' tidak ditemukan");

            property.SetValue(settings, value);
            await SaveSettingsAsync(settings);
        }
    }
}