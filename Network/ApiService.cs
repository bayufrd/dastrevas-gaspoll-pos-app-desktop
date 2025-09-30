using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using KASIR.Helper;
using KASIR.Properties;
using Polly;
using MessageBox = System.Windows.MessageBox;

namespace KASIR.Network
{
    public class ApiService : IApiService
    {
        private readonly string baseAddress;
        private readonly IAsyncPolicy<HttpResponseMessage> combinedPolicy;
        private readonly HttpClient httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> retryPolicy;
        private readonly IAsyncPolicy<HttpResponseMessage> timeoutPolicy;

        public ApiService()
        {
            baseAddress = Settings.Default.BaseAddress;
            httpClient = new HttpClient
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<HttpResponseMessage> UpdateMenu(string url, string id, string jsonString)
        {
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<string> PayBillTransaction(string jsonString, string url)
        {
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SaveBill(string jsonString, string url)
        {
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> inputPin(string jsonString, string url)
        {
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<string> CetakLaporanShift(string jsonString, string url)
        {
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<HttpResponseMessage> EditMember(string jsonString, string url)
        {
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return response;
        }

        public async Task<HttpResponseMessage> deleteCart(string jsonString, string url)
        {
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
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
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> SyncTransaction(string jsonString, string url)
        {
            try
            {
                StringContent content = new(jsonString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await SendRequestAsync(() => httpClient.PostAsync(url, content));

                // Log response details
                LoggerUtil.LogWarning(
                    $"Sync Transaction Response Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");

                return response;
            }
            catch (Exception ex)
            {
                // Log comprehensive error details
                LoggerUtil.LogError(ex, $"Sync Transaction Error: {ex.Message}",
                    new { ExceptionType = ex.GetType().Name, InnerExceptionMessage = ex.InnerException?.Message });

                throw;
            }
        }

        // Utility method to send requests with retry and timeout policy
        private async Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc)
        {
            try
            {
                HttpResponseMessage? response = await combinedPolicy.ExecuteAsync(requestFunc);
                //response.EnsureSuccessStatusCode();  // Uncomment jika ingin auto-throw untuk non-2xx
                return response;
            }
            catch (HttpRequestException ex)
            {
                // MODIFIED: Bedakan client-side vs server-side
                if (ex.StatusCode.HasValue && ex.StatusCode.Value >= HttpStatusCode.InternalServerError)  // 5xx: Critical, log ke server
                {
                    LoggerUtil.LogError(ex, $"Kesalahan server (5xx): {ex.Message}", ex.Message);
                    NotifyHelper.Error($"Kesalahan pada server: {ex.Message}. Hubungi admin jika berlanjut.");
                }
                else  // 4xx atau network error (client-side): Hanya notify, tidak log ke server
                {
                    // Contoh 404, 400, DNS error, dll.
                    NotifyHelper.Error("Masalah koneksi atau data tidak ditemukan. Periksa internet dan coba lagi.");
                }
                throw; // Rethrow agar caller bisa handle (misalnya fallback offline)
            }
            catch (TaskCanceledException ex)
            {
                // MODIFIED: Hanya notify user, tidak log ke server (clean log)
                if (ex.CancellationToken.IsCancellationRequested)
                {
                    // Pembatalan manual (misalnya user cancel button)
                    NotifyHelper.Error("Operasi dibatalkan. Silakan coba lagi jika diperlukan.");
                }
                else
                {
                    // Timeout (waktu habis setelah retry Polly)
                    NotifyHelper.Error("Koneksi timeout setelah 5 menit. Periksa internet Anda dan coba lagi.");
                }
                throw; // Rethrow untuk propagate ke caller
            }
            catch (Exception ex)
            {
                // MODIFIED: Handle unexpected/critical (termasuk 500 jika bukan HttpRequestException)
                bool isServerError = ex is HttpRequestException httpEx && httpEx.StatusCode == HttpStatusCode.InternalServerError;

                if (isServerError)
                {
                    // Critical: Log ke server DAN notify user
                    LoggerUtil.LogError(ex, $"Kesalahan server internal (500): {ex.Message}", ex.Message);
                    NotifyHelper.Error("Terjadi kesalahan pada server. Silakan coba lagi.");

                    // MODIFIED: Uncomment dan perbaiki dialog retry untuk UX lebih baik (opsional, tapi recommended)
                    //---------------- Question Begin -----------------\\
                    //string titleQuest = "Kesalahan Server";
                    //string msgQuest = "Terjadi kesalahan pada server (500). Apakah Anda ingin mencoba lagi?";
                    //string cancelQuest = "Batal";
                    //string okQuest = "Coba Lagi";

                    //QuestionHelper c = new(titleQuest, msgQuest, cancelQuest, okQuest);
                    //Form background = c.CreateOverlayForm();
                    //c.Owner = background;
                    //background.Show();
                    //DialogResult dialogResult = c.ShowDialog();

                    //if (dialogResult == DialogResult.OK)
                    //{
                    //    try
                    //    {
                    //        // Retry otomatis via Polly
                    //        HttpResponseMessage? response = await combinedPolicy.ExecuteAsync(requestFunc);
                    //        background.Dispose();
                    //        return response;  // Sukses retry, return response
                    //    }
                    //    catch (Exception retryEx)
                    //    {
                    //        // Retry gagal: Log dan throw
                    //        LoggerUtil.LogError(retryEx, "Retry attempt setelah dialog gagal: {Message}", retryEx.Message);
                    //        background.Dispose();
                    //        throw;
                    //    }
                    //}

                    //background.Dispose();
                    //--------------- Question End -------------------\\

                    throw; // User batal, throw ulang
                }
                else
                {
                    // Unexpected exception (bukan network): Critical, log ke server
                    LoggerUtil.LogError(ex, $"Error tak terduga di API: {ex.Message}", ex.Message);
                    NotifyHelper.Error("Terjadi kesalahan tak terduga. Hubungi admin.");
                    throw;
                }
            }
        }

    }
}