using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Services
{
    public class InternetService : IInternetService
    {
        public async Task<bool> IsInternetConnectedAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetAsync("https://www.google.com");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IsInternetConnected()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var result = ping.Send("8.8.8.8", 1000);
                    return result.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
