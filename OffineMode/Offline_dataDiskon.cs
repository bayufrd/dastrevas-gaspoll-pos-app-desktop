using System.Data;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
namespace KASIR.OfflineMode
{
    public partial class Offline_dataDiskon : Form
    {
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet;
        private readonly ILogger _log = LoggerService.Instance._log;
        public bool ReloadDataInBaseForm { get; private set; }
        private const string CacheFolder = "DT-Cache";
        string CacheFileName;

        public Offline_dataDiskon()
        {
            InitializeComponent();
            CacheFileName = "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data";
            LoadData();

        }

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
                    MessageBox.Show("Data diskon tidak dapat diproses. Periksa koneksi lagi", "Gaspol");
                    return;
                }

                PopulateDataGridView(menuModel);
            }
            catch (JsonException jsonEx)
            {
                MessageBox.Show("Gagal memproses data dari API: " + jsonEx.Message, "Gaspol");
                LoggerUtil.LogError(jsonEx, "An error occurred during JSON deserialization: {ErrorMessage}", jsonEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal tampil data diskon: " + ex.Message, "Gaspol");
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
                MessageBox.Show("Respons dari API kosong.", "Gaspol");
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

        private void PopulateDataGridView(DiscountCartModel menuModel)
        {
            List<DataDiscountCart> menuList = menuModel.data.ToList();
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Kode", typeof(string));
            dataTable.Columns.Add("Nilai", typeof(string));
            dataTable.Columns.Add("Tipe discount", typeof(string));
            dataTable.Columns.Add("Jenis discount", typeof(string));
            dataTable.Columns.Add("Minimum", typeof(string));
            dataTable.Columns.Add("Maximum", typeof(string));
            dataTable.Columns.Add("Durasi", typeof(string));

            foreach (DataDiscountCart menu in menuList)
            {
                dataTable.Rows.Add(
                    menu.id,
                    menu.code,
                    menu.is_percent == 0 ? string.Format("Rp. {0:n0},-", menu.value) : menu.value + " %",
                    menu.is_percent == 0 ? "Nominal" : "Percent",
                    menu.is_discount_cart == 0 ? "Peritem" : "Keranjang",
                    string.Format("Rp. {0:n0},-", menu.min_purchase),
                    string.Format("Rp. {0:n0},-", menu.max_discount),
                    menu.start_date.ToString("yyyy-MM-dd") + " - " + menu.end_date.ToString("yyyy-MM-dd")
                );
            }

            dataGridView1.DataSource = dataTable;
            dataGridView1.Columns["ID"].Visible = false;
        }




        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
