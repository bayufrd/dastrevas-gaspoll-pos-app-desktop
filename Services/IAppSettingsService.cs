using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Services
{
    public interface IAppSettingsService
    {
        Task<AppSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
        Task<T> GetSettingValueAsync<T>(string key);
        Task SaveSettingValueAsync<T>(string key, T value);
    }
}
