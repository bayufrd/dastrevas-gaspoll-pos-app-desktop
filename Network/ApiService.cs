using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Polly;
using MessageBox = System.Windows.MessageBox;

namespace KASIR.Network
{
    public class ApiService : IApiService
    {
        private readonly HttpClient httpClient;
        private readonly string baseAddress;
        private readonly IAsyncPolicy<HttpResponseMessage> retryPolicy;
        private readonly IAsyncPolicy<HttpResponseMessage> timeoutPolicy;
        private readonly IAsyncPolicy<HttpResponseMessage> combinedPolicy;

        public ApiService()
        {
            baseAddress = Properties.Settings.Default.BaseAddress;
            httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(300) // 5 menit = 300 detik
            };
            httpClient.BaseAddress = new Uri(baseAddress); // Replace with your API base URL
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(3);

            timeoutPolicy = Policy
                .TimeoutAsync<HttpResponseMessage>(300); // Set the timeout duration to 10 seconds, adjust as needed

            combinedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);
        }

        // Utility method to send requests with retry and timeout policy
        private async Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc)
        {
            try
            {
                var response = await combinedPolicy.ExecuteAsync(requestFunc);
                //response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException ex)
            {
                LoggerUtil.LogError(ex, "Terjadi kesalahan jaringan: {ErrorMessage}", ex.Message);
                throw; // rethrow the exception to maintain the original behavior
            }
            catch (TaskCanceledException ex)
            {
                // Handle TaskCanceledException, e.g., notify the user

                if (ex.CancellationToken.IsCancellationRequested)
                {
                    LoggerUtil.LogError(ex, "Operasi dibatalkan, ada yang salah saat penginputan: {ErrorMessage} - Canceled", ex.Message);
                }
                else
                {
                    //MessageBox.Show("Waktu koneksi berakhir, telah dicoba sebanyak 3x. Silakan periksa koneksi internet Anda dan coba lagi.", "Timeout Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoggerUtil.LogError(ex, "Waktu koneksi berakhir, telah dicoba sebanyak 3x: {ErrorMessage} - Timeout", ex.Message);
                }
                throw; // rethrow the exception to maintain the original behavior
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException && ((HttpRequestException)ex).StatusCode == HttpStatusCode.InternalServerError)
                {
                    MessageBoxResult result = MessageBox.Show("Terjadi kesalahan pada server (500 Internal Server Error). Apakah Anda ingin mencoba lagi?", "Server Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Retry logic
                        try
                        {
                            var response = await combinedPolicy.ExecuteAsync(requestFunc);
                            return response;
                        }
                        catch (Exception retryEx)
                        {
                            LoggerUtil.LogError(retryEx, "Retry attempt failed: {Message}", retryEx.Message);
                            throw;
                        }
                    }
                    else
                    {
                        throw; // rethrow the exception to maintain the original behavior
                    }
                }
                else
                {
                    throw; // rethrow the exception to maintain the original behavior
                }
            }
        }

        //Menu
        public async Task<string> Get(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetMenuByID(string url, string id)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url + "/" + id));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CheckIsOrdered(string url, string id)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url + "" + id));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> PostAddMenu(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<HttpResponseMessage> UpdateMenu(string url, string id, string jsonString)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PatchAsync(url + "/" + id, content));
            return response;
        }

        public async Task<HttpResponseMessage> DeleteMenu(string url, string id)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.DeleteAsync(url + "/" + id));
            return response;
        }

        public async Task<HttpResponseMessage> DeleteCart(string url, string id)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.DeleteAsync(url + "&" + id));
            return response;
        }

        //Transaksi
        public async Task<string> GetListMenu(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> CreateCart(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<string> GetCart(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetItemOnCart(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> DeleteCart(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.DeleteAsync(url));
            return response;
        }

        public async Task<HttpResponseMessage> PayBill(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<string> PayBillTransaction(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SaveBill(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetMenuDetailByID(string url, string id)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url + "/" + id));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> UpdateCart(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PatchAsync(url, content));
            return response;
        }

        public async Task<string> GetDiscount(string url, string id)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url + id));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> UseDiscount(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<string> GetListBill(string url, string id)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url + id));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetActiveCart(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetTransactionRefund(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> Refund(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> inputPin(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<string> GetCicilDetail(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> cicilRefund(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<string> CetakLaporanShift(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CekShift(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetPaymentType(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        //Membership
        public async Task<string> GetMember(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> CreateMember(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<HttpResponseMessage> EditMember(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PatchAsync(url, content));
            return response;
        }

        public async Task<HttpResponseMessage> DeleteMember(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.DeleteAsync(url));
            return response;
        }

        public async Task<HttpResponseMessage> notifikasiPengeluaran(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<HttpResponseMessage> deleteCart(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<string> Restruk(string url)
        {
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.GetAsync(url));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SplitBill(string jsonString, string url)
        {
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<HttpResponseMessage> SyncTransaction(string jsonString, string url)
        {
            try
            {
                // Log detail payload
                LoggerUtil.LogWarning($"Sync Transaction Payload Size: {jsonString.Length} bytes, Timestamp: {DateTime.Now}");

                StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));

                // Log response details
                LoggerUtil.LogWarning($"Sync Transaction Response Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");

                return response;
            }
            catch (Exception ex)
            {
                // Log comprehensive error details
                LoggerUtil.LogError(ex, $"Sync Transaction Error: {ex.Message}", new
                {
                    ExceptionType = ex.GetType().Name,
                    InnerExceptionMessage = ex.InnerException?.Message
                });

                throw;
            }
        }
    }
}
