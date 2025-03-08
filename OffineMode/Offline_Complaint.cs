
using FontAwesome.Sharp;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Globalization;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Text.RegularExpressions;
using System.Windows.Markup;
namespace KASIR.OffineMode
{
    public partial class Offline_Complaint : Form
    {
        private readonly string BaseOutletName;
        private readonly string Version;
        public bool ReloadDataInBaseForm { get; private set; }
        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        private readonly string baseOutlet;
        GetTransactionDetail dataTransaction;
        private readonly ILogger _log = LoggerService.Instance._log;
        public Offline_Complaint()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
            Version = Properties.Settings.Default.Version;

            InitializeComponent();
            txtNotes.PlaceholderText = "Deskripsi Masalah?";
            txtNotes.PlaceholderText += "\nKronologi : ";
            txtNotes.PlaceholderText += "\nSaat Melakukan/Mengakses tombol : ";
            txtNotes.PlaceholderText += "\nKesalahan yang terjadi : ";
            txtNotes.PlaceholderText += "\nSeharusnya yang terjadi : ";
            txtNotes.PlaceholderText += "\n\nMohon maaf atas ketidak nyamanannya dan \nTerimakasih, dengan support ini akan membantu agar lebih maju \ndan berkembang kedepannya. \n\nGaspoll Management. x Dastrevas";



        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
            this.Close();
        }
        private void AddItem(string name, string amount)
        {


        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel13_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblNama.Text == null || lblNama.Text == "")
                {
                    MessageBox.Show("Format nama kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (txtNotes.Text == null || txtNotes.ToString() == "")
                {
                    MessageBox.Show("Format notes kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }


                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");

                // Deserialize JSON ke object CartDataCache
                var dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                string BaseOutletNameID = dataOutlet?.data?.name?.ToString() ?? "";

                string formatDate = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

                // Mengubah string formatDate ke DateTime
                DateTime date = DateTime.ParseExact(formatDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                // Mengurangi satu hari
                DateTime previousDay = date.AddDays(-1);

                // Mengonversi kembali ke format string
                string previousDayString = previousDay.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

                // Folder untuk cache dan log
                string FolderCache = "DT-Cache\\Transaction";
                string FolderLogCacheData = $"log\\{baseOutlet}_{BaseOutletName}_log{formatDate}.txt";

                // Path untuk berbagai file log cache
                string FolderLogCache_transaction = Path.Combine(FolderCache, "transaction.data");
                string FolderLogCache_failed_transaction = Path.Combine(FolderCache, "FailedSyncTransaction", $"{baseOutlet}_FailedSync_{formatDate}.data");
                string FolderLogCache_success_transaction = Path.Combine(FolderCache, "SyncSuccessTransaction", $"{baseOutlet}_SyncSuccess_{formatDate}.data");
                string FolderLogCache_history_transaction = Path.Combine(FolderCache, "HistoryTransaction", $"History_transaction_DT-{baseOutlet}_{previousDayString}.data");

                // Membaca isi file log cache atau mengembalikan "{}" jika file tidak ada
                string LogCacheData = ReadFileContentAsSingleLine(FolderLogCacheData);
                string Cache_transaction = ReadFileContentAsSingleLine(FolderLogCache_transaction);
                string Cache_failed_transaction = ReadFileContentAsSingleLine(FolderLogCache_failed_transaction);
                string Cache_success_transaction = ReadFileContentAsSingleLine(FolderLogCache_success_transaction);
                string Cache_history_transaction = ReadFileContentAsSingleLine(FolderLogCache_history_transaction);

                string nameID = $"{baseOutlet}_{DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture)}_{BaseOutletNameID}_{Version}_{lblNama.Text.ToString()}";

                var json = new
                {
                     name = nameID,
                     title =  "ada ada aja",
                     message =  txtNotes.Text.ToString(),
                     sent_at =  DateTime.Now.ToString("yyyy-MM-dd HH=mm=ss", CultureInfo.InvariantCulture) ,
                     log_last_outlet = LogCacheData,
                     cache_transaction = Cache_transaction,
                     cache_failed_transaction = Cache_failed_transaction,
                     cache_success_transaction = Cache_success_transaction,
                     cache_history_transaction = Cache_history_transaction
                };
                //string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);

                // Mengubah objek menjadi string JSON
                string jsonString = JsonConvert.SerializeObject(json, Formatting.None); // Tidak ada indentasi
                IApiService apiService = new ApiService();

                HttpResponseMessage response = await apiService.notifikasiPengeluaran(jsonString, $"/complaint?outlet_id={baseOutlet}");

                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        DialogResult result = MessageBox.Show("Berhasil dikirim, Terimakasih", "Gaspol", MessageBoxButtons.OK);
                        if (result == DialogResult.OK)
                        {
                            this.Close(); // Close the payForm
                        }
                        this.DialogResult = result;
                    }
                    else
                    {
                        MessageBox.Show("Gagal dikirim, Coba cek koneksi internet ulang " + response.StatusCode);
                        _log.Error("Gagal dikirim, Coba cek koneksi internet ulang ");
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }

        }
        private static string XReadFileContentAsSingleLine(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);

                // Menghapus escape backslash secara eksplisit
                jsonContent = jsonContent.Replace("\\", ""); 
                jsonContent = jsonContent.Replace("\"", "");



                // Deserialisasi dan serialisasi untuk memastikan JSON valid
                var parsedJson = JsonConvert.DeserializeObject(jsonContent);

                // Serialize kembali tanpa escape characters
                return JsonConvert.SerializeObject(parsedJson, Formatting.None);
            }
            return "{}";  // Kembalikan "{}" jika file tidak ada
        }

        private static string ReadFileContentAsSingleLine(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);

                // Deserialisasi JSON dan serialisasi kembali
                var parsedJson = JsonConvert.DeserializeObject(jsonContent);

                // Serialize kembali dengan pengaturan escape handling
                return JsonConvert.SerializeObject(parsedJson, Formatting.None, new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml
                });
            }
            return "{}";  // Kembalikan "{}" jika file tidak ada
        }


        private void txtJumlahCicil_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
        private void txtSelesaiShift_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNominal_TextChanged(object sender, EventArgs e)
        {
        }
    }

}

