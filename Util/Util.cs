using KASIR.Komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace KASIR
{
    public class Util
    {
        // Your other utility functions here
        blur background = new blur();
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet;
        string outletName;

        public void ShowBlurredDialog(Form formToShow)
        {
            background.StartPosition = FormStartPosition.CenterScreen;
            background.FormBorderStyle = FormBorderStyle.None;
            background.Opacity = 0.7d;
            background.BackColor = Color.Black;
            background.WindowState = FormWindowState.Maximized;
            background.TopMost = true;
            background.Size = formToShow.Size;
            background.Location = formToShow.Location;
            background.ShowInTaskbar = false;
            background.Show();
            formToShow.Owner = background;
            formToShow.ShowDialog();
            background.Dispose();
        }
        public string GetOutletNameFromFile(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            var outletData = JsonConvert.DeserializeObject<JObject>(fileContent);
            return outletData["data"]?["name"]?.ToString();
        }
        public async Task<string> GetOutletNameFromApi()
        {
            IApiService apiService = new ApiService();
            string response = await apiService.CekShift("/outlet/" + baseOutlet);
            if (response != null)
            {
                var apiResponse = JsonConvert.DeserializeObject<JObject>(response);
                File.WriteAllText($"DT-Cache\\DataOutlet{baseOutlet}.data", JsonConvert.SerializeObject(response));

                return apiResponse["data"]?["name"]?.ToString();
            }
            return null;
        }

        public async void sendLogTelegramBy(Exception ex, string message, params object[] properties)
        {

            IApiService apiService = new ApiService();


            string filePath = $"DT-Cache\\DataOutlet{baseOutlet}.data";
            string outletName;

            // Cek apakah file ada dan baca data dari file atau API
            if (File.Exists(filePath))
            {
                outletName = GetOutletNameFromFile(filePath);
            }
            else
            {
                outletName = await GetOutletNameFromApi();
                if (outletName != null)
                {
                    // Simpan data ke file jika berhasil mendapatkan nama outlet
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(new { data = new { name = outletName } }));
                }
                else
                {
                    outletName = Properties.Settings.Default.BaseOutletName.ToString();
                }
            }


            string outletID = Properties.Settings.Default.BaseOutlet.ToString();

            using (var client = new HttpClient())
            {
                string botToken = "6909601463:AAHnKWEKqlpL1NGRkzRpXVnDgHoVtJtrqo0";
                DateTime currentDateTime = DateTime.Now;
                string datetimeStamp = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string messageWithDatetime = $"Error Outlet ID: {outletID}\nOurlet Name: {outletName}\n[{datetimeStamp}] \n{message} \n{properties} \n{ex}";

                // Replace these with your actual chat IDs
                long chatId1 = 6668065856;
                long chatId2 = 1546898379;
                long chatId3 = 5421340211;

                // Send to the first chat ID
                await SendMessageToTelegram(client, botToken, chatId1, messageWithDatetime);

                // Send to the second chat ID
                await SendMessageToTelegram(client, botToken, chatId2, messageWithDatetime);

                // Send to the third chat ID
                await SendMessageToTelegram(client, botToken, chatId3, messageWithDatetime);
            }

        }/*
        public async void sendLogTelegram(string message)
        {
            IApiService apiService = new ApiService();

            string response = await apiService.CekShift("/shift?outlet_id=" + baseOutlet);
            if (response != null)
            {


                GetShift cekShift = JsonConvert.DeserializeObject<GetShift>(response);

                DataShift datas = cekShift.data;
                outletName = datas.outlet_name.ToString();


            }
            else
            {
                outletName = Properties.Settings.Default.BaseOutlet.ToString();
            }
            using (var client = new HttpClient())
            {
                string botToken = "6909601463:AAHnKWEKqlpL1NGRkzRpXVnDgHoVtJtrqo0";
                DateTime currentDateTime = DateTime.Now;
                string datetimeStamp = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string messageWithDatetime = $"OUTLET : {outletName}\n[{datetimeStamp}] {message}";

                // Replace these with your actual chat IDs
                long chatId1 = 6668065856;
                long chatId2 = 1546898379;
                long chatId3 = 5421340211;

                // Send to the first chat ID
                await SendMessageToTelegram(client, botToken, chatId1, messageWithDatetime);

                // Send to the second chat ID
                await SendMessageToTelegram(client, botToken, chatId2, messageWithDatetime);

                // Send to the second chat ID
                await SendMessageToTelegram(client, botToken, chatId3, messageWithDatetime);
            }
        }*/

        private async Task SendMessageToTelegram(HttpClient client, string botToken, long chatId, string message)
        {
            try
            {
                // Build the API URL
                string apiUrl = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(message)}";

                // Create an HttpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Send the request
                var response = await client.SendAsync(request);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Print the response content
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            catch (Exception)
            {
                // kadang ada error di response kalo gada internet/down jadi crash
                LoggerUtil.LogWarning("bad Connection gagal mengirim data ke telegram, tersimpan dalam log :) ");
            }
        }

        public async void sendLogTelegramNetworkError(string message)
        {

            IApiService apiService = new ApiService();

            string response = await apiService.CekShift("/shift?outlet_id=" + baseOutlet);
            if (response != null)
            {


                GetShift cekShift = JsonConvert.DeserializeObject<GetShift>(response);

                DataShift datas = cekShift.data;
                outletName = datas.outlet_name.ToString();


            }
            else
            {
                outletName = Properties.Settings.Default.BaseOutletName.ToString();
            }
            string outletID = Properties.Settings.Default.BaseOutlet.ToString();

            using (var client = new HttpClient())
            {
                string botToken = "6909601463:AAHnKWEKqlpL1NGRkzRpXVnDgHoVtJtrqo0";
                DateTime currentDateTime = DateTime.Now;
                string datetimeStamp = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string messageWithDatetime = $"Error Network Outlet ID: {outletID}\nOurlet Name: {outletName}\n[{datetimeStamp}] \n{message}";

                // Replace these with your actual chat IDs
                long chatId1 = 6668065856;
                long chatId2 = 1546898379;
                long chatId3 = 5421340211;

                // Send to the first chat ID
                await SendMessageToTelegram(client, botToken, chatId1, messageWithDatetime);

                // Send to the second chat ID
                await SendMessageToTelegram(client, botToken, chatId2, messageWithDatetime);

                // Send to the third chat ID
                await SendMessageToTelegram(client, botToken, chatId3, messageWithDatetime);
            }

        }



    }
}