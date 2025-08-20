using System.Data;
using System.Windows.Forms;
using KASIR.Helper;
using KASIR.Model;
using KASIR.Network;
using KASIR.Properties;
using Newtonsoft.Json;
using Serilog;

namespace KASIR.OfflineMode
{
    public partial class Offline_dataDiskon : Form
    {
        private const string CacheFolder = "DT-Cache";
         
        private readonly string baseOutlet = Settings.Default.BaseOutlet;
        private readonly string CacheFileName;

        public Offline_dataDiskon()
        {
            InitializeComponent();
            CacheFileName = "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data";
            LoadData();
        }

        public bool ReloadDataInBaseForm { get; private set; }

        private async void LoadData()
        {
            try
            {
                string cacheFilePath = Path.Combine(CacheFolder, string.Format(CacheFileName, baseOutlet));

                DiscountCartModel menuModel = await ReadFromCache(cacheFilePath);
                if (menuModel == null)
                {
                    menuModel = await FetchFromApiAndCache(cacheFilePath);
                }

                if (menuModel == null || menuModel.data == null)
                {
                    NotifyHelper.Error("Data diskon tidak dapat diproses. Periksa koneksi lagi");
                    return;
                }

                PopulateFlowLayout(menuModel);
            }
            catch (JsonException jsonEx)
            {
                NotifyHelper.Error("Gagal memproses data dari API: " + jsonEx.Message);
                LoggerUtil.LogError(jsonEx, "An error occurred during JSON deserialization: {ErrorMessage}",
                    jsonEx.Message);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Gagal tampil data diskon: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private async Task<DiscountCartModel> ReadFromCache(string cacheFilePath)
        {
            if (File.Exists(cacheFilePath))
            {
                try
                {
                    string cachedData = await File.ReadAllTextAsync(cacheFilePath);
                    return JsonConvert.DeserializeObject<DiscountCartModel>(cachedData);
                }
                catch (Exception ex)
                {
                    NotifyHelper.Error($"Gagal load dari lokal, silahkan syncron ulang");
                    LoggerUtil.LogError(ex, "Failed to read from cache: {ErrorMessage}", ex.Message);
                }
            }

            return null;
        }

        private async Task<DiscountCartModel> FetchFromApiAndCache(string cacheFilePath)
        {
            IApiService apiService = new ApiService();
            string response = await apiService.GetDiscount($"/discount?outlet_id={baseOutlet}", "");

            if (string.IsNullOrEmpty(response))
            {
                NotifyHelper.Error("Respons dari API kosong.");
                return null;
            }

            DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(response);

            if (menuModel != null)
            {
                try
                {
                    Directory.CreateDirectory(CacheFolder);
                    await File.WriteAllTextAsync(cacheFilePath, JsonConvert.SerializeObject(menuModel));
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, "Failed to write to cache: {ErrorMessage}", ex.Message);
                }
            }

            return menuModel;
        }
        private void PopulateFlowLayout(DiscountCartModel menuModel)
        {
            // pastikan flowLayoutPanel1 ada di form
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;

            List<DataDiscountCart> menuList = menuModel.data.ToList();

            foreach (DataDiscountCart menu in menuList)
            {
                // Panel kartu
                Panel card = new Panel
                {
                    Width = 200,
                    Height = 130,
                    BackColor = Color.White,
                    Margin = new Padding(10),
                    Padding = new Padding(10),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Judul (kode diskon)
                Label lblCode = new Label
                {
                    Text = $"Kode: {menu.code}",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    ForeColor = Color.Black,
                    Location = new Point(5, 5)
                };
                card.Controls.Add(lblCode);

                // Nilai
                Label lblValue = new Label
                {
                    Text = menu.is_percent == 0 ? $"Nominal: Rp {menu.value:n0}" : $"Diskon: {menu.value}%",
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    AutoSize = true,
                    Location = new Point(5, 35)
                };
                card.Controls.Add(lblValue);

                // Tipe
                Label lblType = new Label
                {
                    Text = $"Tipe: {(menu.is_percent == 0 ? "Nominal" : "Percent")}",
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    Location = new Point(5, 55)
                };
                card.Controls.Add(lblType);

                // Jenis discount
                Label lblJenis = new Label
                {
                    Text = $"Jenis: {(menu.is_discount_cart == 0 ? "Per Item" : "Keranjang")}",
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    Location = new Point(5, 75)
                };
                card.Controls.Add(lblJenis);

                // Durasi
                Label lblDurasi = new Label
                {
                    Text = $"Durasi: {menu.start_date:yyyy-MM-dd} - {menu.end_date:yyyy-MM-dd}",
                    Font = new Font("Segoe UI", 8, FontStyle.Italic),
                    AutoSize = true,
                    ForeColor = Color.Gray,
                    Location = new Point(5, 95)
                };
                card.Controls.Add(lblDurasi);


                // Tambah ke flowLayout
                flowLayoutPanel1.Controls.Add(card);
            }
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}