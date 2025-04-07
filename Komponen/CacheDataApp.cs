using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Image = System.Drawing.Image;
using Menu = KASIR.Model.Menu;

namespace KASIR.Komponen
{
    public partial class CacheDataApp : Form
    {
        ApiService apiService = new ApiService();
        private readonly string baseOutlet = Properties.Settings.Default.BaseOutlet;
        private readonly string baseUrl = Properties.Settings.Default.BaseAddress;
        Dictionary<Menu, Image> menuImageDictionary = new Dictionary<Menu, Image>();
        Util util = new Util();
        int[] menuIds = new int[0];
        string folderAddCartForm = "DT-Cache\\addCartForm";
        int items;
        string choice;

        public CacheDataApp(string TypeCacheEksekusi)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen; // Menampilkan form di tengah layar
            MaximizeBox = false;  // Menonaktifkan tombol maximize untuk menghindari form lebih besar dari layar
            FormBorderStyle = FormBorderStyle.FixedDialog; // Mengatur border agar tidak bisa diubah-ubah ukuran
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            choice = TypeCacheEksekusi.ToString();
            openForm(choice);
        }

        public async void openForm(string choice)
        {
            await LoadData(choice);
        }
        public async Task LoadData(string TypeCacheEksekusi)
        {
            if (TypeCacheEksekusi == "Sync")
            {
                //util.sendLogTelegram("Open Sync Data. Outlet ID: " + baseOutlet);
                await CompareAndWriteCacheMenuItems();
            }
            if (TypeCacheEksekusi == "Reset" || TypeCacheEksekusi != "Sync")
            {
                util.sendLogTelegramNetworkError("Open Reset Data. Outlet ID: " + baseOutlet);
                await DeleteDataCache();
            }
        }
        private void UpdateDetailLabel(string text)
        {
            if (lblDetail.InvokeRequired)
            {
                lblDetail.Invoke(new Action(() => lblDetail.Text = text));
            }
            else
            {
                lblDetail.Text = text;
            }
        }

        private void UpdateProgressLabel(string text)
        {
            if (lblProgress.InvokeRequired)
            {
                lblProgress.Invoke(new Action(() => lblProgress.Text = text));
            }
            else
            {
                lblProgress.Text = text;
            }
        }
        private void EnsureDirectoriesExist()
        {
            if (!Directory.Exists("DT-Cache"))
            {
                Directory.CreateDirectory("DT-Cache");
            }
            if (!Directory.Exists(folderAddCartForm))
            {
                Directory.CreateDirectory(folderAddCartForm);
            }
        }

        private async Task DeleteDataCache()
        {
            UpdateDetailLabel("Deleting Items dan Gambar");
            UpdateProgressLabel($"Loading Data...");
            progressBar.Value = 0;

            string cacheDirectory = "DT-Cache";
            string transactionDirectory = Path.Combine(cacheDirectory, "Transaction");

            if (Directory.Exists(cacheDirectory))
            {
                // Get all directories and files within DT-Cache except the Transaction folder
                var directories = Directory.GetDirectories(cacheDirectory)
                                           .Where(d => !d.Equals(transactionDirectory, StringComparison.OrdinalIgnoreCase));
                var files = Directory.GetFiles(cacheDirectory);

                // Delete all files and directories except the Transaction folder
                foreach (var directory in directories)
                {
                    Directory.Delete(directory, true);
                }

                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(appDirectory)?.Parent?.FullName;
            string localImagePath = Path.Combine(projectDirectory, "ImageCache");
            if (Directory.Exists(localImagePath))
            {
                Directory.Delete(localImagePath, true);
            }

            progressBar.Value = 100;
            await LoadCacheMenuItems();
        }


        private async Task CompareAndWriteCacheMenuItems()
        {
            try
            {
                UpdateDetailLabel("Memuat Menu Items dan Gambar");
                items = 0;
                progressBar.Value = items;

                EnsureDirectoriesExist();

                string cacheFilePath = $"DT-Cache\\menu_outlet_id_{baseOutlet}.data";
                string response = await apiService.Get("/menu?outlet_id=" + baseOutlet);
                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                List<Menu> menuList = menuModel.data.ToList();

                menuIds = menuList.Select(menu => menu.id).Where(id => id != 0).ToArray();

                GetMenuModel cachedMenuModel = null;
                if (File.Exists(cacheFilePath))
                {
                    string cachedData = File.ReadAllText(cacheFilePath);
                    cachedMenuModel = JsonConvert.DeserializeObject<GetMenuModel>(cachedData);
                }

                bool dataChanged = !AreMenuModelsEqual(menuModel, cachedMenuModel);

                if (dataChanged || cachedMenuModel == null)
                {
                    File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(menuModel));
                }
                await UpdateUIWithMenuData(menuModel);

                await CompareAndWriteServingTypeItems();
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

        private async Task UpdateUIWithMenuData(GetMenuModel menuModel)
        {
            items = 0;
            progressBar.Value = items;
            foreach (Menu menu in menuModel.data)
            {
                items++;
                int x = 100 / menuModel.data.Count;
                PictureBox pictureBox = new PictureBox();
                UpdateProgressLabel($"Mengunduh Data...[{items} / {menuModel.data.Count}]");

                UpdateDetailLabel("Checking Image...");
                await LoadImageToPictureBox(pictureBox, menu);
                progressBar.Value = x * items;

            }
        }

        private async Task CompareAndWriteServingTypeItems()
        {
            try
            {
                items = 0;
                progressBar.Value = items;
                UpdateDetailLabel("Memuat Serving Type Items");

                EnsureDirectoriesExist();

                foreach (int menuId in menuIds)
                {
                    items++;
                    string idmenu = menuId.ToString();
                    string apiResponse = await apiService.GetMenuByID("/menu", idmenu);
                    var apiMenuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(apiResponse);

                    string cacheFilePath = $"{folderAddCartForm}\\LoadDataServingType_{menuId}_Outlet_{baseOutlet}.data";
                    var cachedMenuModel = LoadCachedMenuByIdModel(cacheFilePath);

                    bool dataChanged = !AreMenuByIdModelsEqual(apiMenuModel, cachedMenuModel);
                    UpdateProgressLabel($"Membandingkan...");


                    if (dataChanged || cachedMenuModel == null)
                    {
                        UpdateProgressLabel($"Mendownload...");

                        File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(apiMenuModel));
                    }
                    // Update progress bar after processing each item
                    UpdateProgressBar(items, menuIds.Length);
                }

                await CompareAndWriteVarianItems();
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

        private async Task CompareAndWriteVarianItems()
        {
            try
            {
                items = 0;
                UpdateDetailLabel("Memuat Data Varian Items dan Membandingkan");


                EnsureDirectoriesExist();

                foreach (int menuId in menuIds)
                {
                    items++;
                    string idmenu = menuId.ToString();
                    string apiResponse = await apiService.GetMenuDetailByID("/menu-detail", idmenu);
                    var apiMenuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(apiResponse);

                    string cacheFilePath = $"{folderAddCartForm}\\LoadDataVarian_{idmenu}_Outlet_{baseOutlet}.data";
                    var cachedMenuModel = LoadCachedMenuDetailCartModel(cacheFilePath);

                    bool dataChanged = !AreMenuDetailCartModelsEqual(apiMenuModel, cachedMenuModel);
                    UpdateProgressLabel($"Membandingkan...");


                    if (dataChanged || cachedMenuModel == null)
                    {
                        UpdateProgressLabel($"Mendownload...");


                        File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(apiMenuModel));
                        UpdateProgressBar(items, menuIds.Length);
                    }
                }

                await CompareAndWriteDiscountPeritems();
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

        private async Task CompareAndWriteDiscountPeritems()
        {
            try
            {
                UpdateDetailLabel("Memuat Diskon Per Item");

                items = 0;
                UpdateProgressLabel($"Mengunduh Data...");

                progressBar.Value = items;

                EnsureDirectoriesExist();

                string apiResponse = await apiService.GetDiscount($"/discount?outlet_id={baseOutlet}&is_discount_cart=", "0");
                var apiDiscountModel = JsonConvert.DeserializeObject<DiscountCartModel>(apiResponse);

                string cacheFilePath = $"{folderAddCartForm}\\LoadDataDiscountItem_Outlet_{baseOutlet}.data";
                var cachedDiscountModel = LoadCachedDiscountModel(cacheFilePath);

                bool dataChanged = !AreDiscountCartModelsEqual(apiDiscountModel, cachedDiscountModel);
                UpdateProgressLabel($"Membandingkan...");

                if (dataChanged || cachedDiscountModel == null)
                {
                    UpdateProgressLabel("Mendowload...");

                    File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(apiDiscountModel));
                }

                progressBar.Value = 100;
                await CompareAndWritePayform();
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

        private async Task CompareAndWritePayform()
        {
            try
            {
                UpdateDetailLabel("Memuat Data Payform");

                items = 0;
                UpdateProgressLabel($"Mengunduh Data...");

                progressBar.Value = items;

                EnsureDirectoriesExist();

                string apiResponse = await apiService.GetPaymentType("/payment-type");
                var apiPaymentModel = JsonConvert.DeserializeObject<PaymentTypeModel>(apiResponse);

                string cacheFilePath = $"DT-Cache\\LoadDataPayment_Outlet_{baseOutlet}.data";
                var cachedPaymentModel = LoadCachedPaymentTypeModel(cacheFilePath);

                bool dataChanged = !ArePaymentTypeModelsEqual(apiPaymentModel, cachedPaymentModel);
                UpdateProgressLabel($"Membandingkan...");

                if (dataChanged || cachedPaymentModel == null)
                {
                    UpdateProgressLabel($"Mendownload...");

                    File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(apiPaymentModel));
                }

                progressBar.Value = 100;
                await CompareAndWriteOutletData();

                await CompareAndWriteDiscountPerCart();
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
        private async Task CompareAndWriteOutletData()
        {
            try
            {
                // Fetch outlet data from API
                string outletApiResponse = await apiService.CekShift("/outlet/" + baseOutlet);
                var apiOutletModel = JsonConvert.DeserializeObject<CartDataOutlet>(outletApiResponse);

                // Define cache file path for outlet data
                string outletCacheFilePath = $"DT-Cache\\DataOutlet{baseOutlet}.data";

                // Load cached outlet data
                var cachedOutletModel = LoadCachedOutletData(outletCacheFilePath);

                // Compare data
                bool dataChanged = !AreOutletModelsEqual(apiOutletModel.data, cachedOutletModel?.data);

                if (dataChanged || cachedOutletModel == null)
                {
                    UpdateProgressLabel($"Mendownload Outlet Data...");
                    // Write updated outlet data to the cache file
                    File.WriteAllText(outletCacheFilePath, JsonConvert.SerializeObject(apiOutletModel));
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred while comparing and writing outlet data: {ErrorMessage}", ex.Message);
            }
        }

        private bool AreOutletModelsEqual(Data model1, Data model2)
        {
            if (model1 == null || model2 == null)
            {
                return false;
            }

            return model1.id == model2.id
                && model1.name == model2.name
                && model1.address == model2.address
                && model1.pin == model2.pin
                && model1.phone_number == model2.phone_number
                && model1.is_kitchen_bar_merged == model2.is_kitchen_bar_merged
                && model1.footer == model2.footer;
        }

        private CartDataOutlet LoadCachedOutletData(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cachedData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<CartDataOutlet>(cachedData);
            }
            return null;
        }
        private async Task CompareAndWriteDiscountPerCart()
        {
            try
            {
                UpdateDetailLabel("Memuat Data Diskon Keranjang");

                items = 0;
                lblProgress.Text = $"Mengunduh Data...";
                UpdateProgressLabel($"Mendownload...");

                progressBar.Value = items;

                EnsureDirectoriesExist();

                string cacheDirectory = "DT-Cache";
                if (!Directory.Exists(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                }

                string apiResponse = await apiService.GetDiscount($"/discount?outlet_id={baseOutlet}&is_discount_cart=", "1");
                DiscountCartModel apiCartModel = JsonConvert.DeserializeObject<DiscountCartModel>(apiResponse);

                string cacheFilePath = $"{cacheDirectory}\\LoadDiscountPerCart_Outlet_{baseOutlet}.data";
                DiscountCartModel cachedCartModel = LoadCachedDiscountModel(cacheFilePath);

                bool dataChanged = !AreDiscountCartModelsEqual(apiCartModel, cachedCartModel);
                UpdateProgressLabel($"Membandingkan...");

                if (dataChanged || cachedCartModel == null)
                {
                    UpdateProgressLabel($"Mendownload...");

                    File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(apiCartModel));
                }

                progressBar.Value = 100;
                //util.sendLogTelegram("Selesai Sinkronisasi Data. Outlet ID: " + baseOutlet);
                this.Close();
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal Mengunduh data " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private GetMenuByIdModel LoadCachedMenuByIdModel(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cachedData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<GetMenuByIdModel>(cachedData);
            }
            return null;
        }

        private GetMenuDetailCartModel LoadCachedMenuDetailCartModel(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cachedData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<GetMenuDetailCartModel>(cachedData);
            }
            return null;
        }

        private DiscountCartModel LoadCachedDiscountModel(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cachedData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<DiscountCartModel>(cachedData);
            }
            return null;
        }

        private PaymentTypeModel LoadCachedPaymentTypeModel(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cachedData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<PaymentTypeModel>(cachedData);
            }
            return null;
        }

        private GetCartModel LoadCachedCartModel(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cachedData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<GetCartModel>(cachedData);
            }
            return null;
        }
        private void UpdateProgressBar(int currentItem, int totalCount)
        {
            if (totalCount > 0)
            {
                // Hitung kemajuan sebagai persentase (0 hingga 100)
                int progress = (int)((double)currentItem / totalCount * 100);

                // Pastikan nilai kemajuan berada dalam rentang yang valid
                progress = Math.Max(progress, 0); // Pastikan tidak kurang dari 0
                progress = Math.Min(progress, 100); // Pastikan tidak lebih dari 100

                // Memperbarui ProgressBar di thread UI
                if (progressBar.InvokeRequired)
                {
                    // Jika kita tidak berada di thread UI, panggil metode ini di thread UI
                    progressBar.Invoke(new Action(() => UpdateProgressBar(currentItem, totalCount)));
                }
                else
                {
                    // Jika kita berada di thread UI, langsung perbarui ProgressBar
                    progressBar.Value = progress;
                }
            }
        }
        private bool AreMenuModelsEqual(GetMenuModel model1, GetMenuModel model2)
        {
            if (model1 == null || model2 == null)
            {
                return false;
            }

            if (model1.data.Count != model2.data.Count)
            {
                return false;
            }

            for (int i = 0; i < model1.data.Count; i++)
            {
                if (!AreMenusEqual(model1.data[i], model2.data[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreMenusEqual(Menu menu1, Menu menu2)
        {
            return menu1.id == menu2.id &&
                   menu1.name == menu2.name &&
                   menu1.menu_type == menu2.menu_type &&
                   menu1.price == menu2.price &&
                   menu1.image_url == menu2.image_url &&
                   menu1.receipt_number == menu2.receipt_number &&
                   menu1.outlet_id == menu2.outlet_id &&
                   menu1.cart_id == menu2.cart_id &&
                   menu1.customer_name == menu2.customer_name &&
                   menu1.customer_seat == menu2.customer_seat &&
                   menu1.payment_type == menu2.payment_type &&
                   menu1.payment_category == menu2.payment_category &&
                   menu1.customer_cash == menu2.customer_cash &&
                   menu1.customer_change == menu2.customer_change &&
                   menu1.invoice_due_date == menu2.invoice_due_date;
        }

        private bool AreMenuByIdModelsEqual(GetMenuByIdModel model1, GetMenuByIdModel model2)
        {
            if (model1 == null || model2 == null)
            {
                return false;
            }

            return model1.data.id == model2.data.id &&
                   model1.data.name == model2.data.name &&
                   model1.data.menu_type == model2.data.menu_type &&
                   model1.data.image_url == model2.data.image_url &&
                   AreMenuPricesEqual(model1.data.menu_prices, model2.data.menu_prices) &&
                   AreServingTypesEqual(model1.data.serving_types, model2.data.serving_types) &&
                   AreMenuDetailsEqual(model1.data.menu_details, model2.data.menu_details);
        }

        private bool AreMenuPricesEqual(List<MenuPrice> prices1, List<MenuPrice> prices2)
        {
            if (prices1 == null || prices2 == null || prices1.Count != prices2.Count)
            {
                return false;
            }

            for (int i = 0; i < prices1.Count; i++)
            {
                if (prices1[i].price != prices2[i].price || prices1[i].serving_type_id != prices2[i].serving_type_id)
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreServingTypesEqual(List<ServingType> types1, List<ServingType> types2)
        {
            if (types1 == null || types2 == null || types1.Count != types2.Count)
            {
                return false;
            }

            for (int i = 0; i < types1.Count; i++)
            {
                if (types1[i].id != types2[i].id || types1[i].name != types2[i].name)
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreMenuDetailsEqual(List<MenuDetailS> details1, List<MenuDetailS> details2)
        {
            if (details1 == null || details2 == null || details1.Count != details2.Count)
            {
                return false;
            }

            for (int i = 0; i < details1.Count; i++)
            {
                if (details1[i].menu_detail_id != details2[i].menu_detail_id || details1[i].varian != details2[i].varian || !AreMenuPricesEqual(details1[i].menu_prices, details2[i].menu_prices))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreMenuDetailCartModelsEqual(GetMenuDetailCartModel model1, GetMenuDetailCartModel model2)
        {
            if (model1 == null || model2 == null)
            {
                return false;
            }

            return model1.data.id == model2.data.id &&
                   model1.data.menu_type == model2.data.menu_type &&
                   AreMenuDetailDataCartListsEqual(model1.data.menu_details, model2.data.menu_details);
        }

        private bool AreMenuDetailDataCartListsEqual(List<MenuDetailDataCart> list1, List<MenuDetailDataCart> list2)
        {
            // Compare counts of MenuDetailDataCart lists
            if (list1 == null || list2 == null || list1.Count != list2.Count)
            {
                return false;
            }

            // Compare each MenuDetailDataCart object in the lists
            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i].menu_detail_id != list2[i].menu_detail_id || list1[i].varian != list2[i].varian || !AreServingTypesListsEqual(list1[i].serving_types, list2[i].serving_types))
                {
                    return false;
                }
            }

            return true;
        }

        // Method to compare two lists of ServingTypes objects
        private bool AreServingTypesListsEqual(List<ServingTypes> types1, List<ServingTypes> types2)
        {
            // Compare counts of ServingTypes lists
            if (types1 == null || types2 == null || types1.Count != types2.Count)
            {
                return false;
            }

            // Compare each ServingTypes object in the lists
            for (int i = 0; i < types1.Count; i++)
            {
                if (types1[i].id != types2[i].id || types1[i].name != types2[i].name || types1[i].price != types2[i].price)
                {
                    return false;
                }
            }

            return true;
        }

        // Method to compare two DiscountCartModel instances
        private bool AreDiscountCartModelsEqual(DiscountCartModel model1, DiscountCartModel model2)
        {
            if (model1 == null || model2 == null)
            {
                return false; // If any model is null, they are not equal
            }

            // Compare counts of DataDiscountCart lists
            if (model1.data == null || model2.data == null || model1.data.Count != model2.data.Count)
            {
                return false;
            }

            // Compare each DataDiscountCart object in the lists
            foreach (var item1 in model1.data)
            {
                bool found = false;
                foreach (var item2 in model2.data)
                {
                    if (item1.id == item2.id &&
                        item1.code == item2.code &&
                        item1.is_percent == item2.is_percent &&
                        item1.is_discount_cart == item2.is_discount_cart &&
                        item1.value == item2.value &&
                        item1.start_date == item2.start_date &&
                        item1.end_date == item2.end_date &&
                        item1.min_purchase == item2.min_purchase &&
                        item1.max_discount == item2.max_discount)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        // Method to compare two PaymentTypeModel instances
        private bool ArePaymentTypeModelsEqual(PaymentTypeModel model1, PaymentTypeModel model2)
        {
            if (model1 == null || model2 == null)
            {
                return false; // If any model is null, they are not equal
            }

            // Compare counts of PaymentType lists
            if (model1.data == null || model2.data == null || model1.data.Count != model2.data.Count)
            {
                return false;
            }

            // Compare each PaymentType object in the lists
            foreach (var item1 in model1.data)
            {
                bool found = false;
                foreach (var item2 in model2.data)
                {
                    if (item1.id == item2.id && item1.name == item2.name)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        // Method to compare two GetCartModel instances
        private bool AreCartModelsEqual(GetCartModel model1, GetCartModel model2)
        {
            // Check if both models are null
            if (model1 == null && model2 == null)
            {
                return true;
            }

            // Check if either model is null
            if (model1 == null || model2 == null)
            {
                return false;
            }

            // Check if both data properties are null
            if (model1.data == null && model2.data == null)
            {
                return true;
            }

            // Check if either data property is null
            if (model1.data == null || model2.data == null)
            {
                return false;
            }
            // Compare properties of DataCart objects
            return model1.data.customer_name == model2.data.customer_name &&
                   model1.data.customer_seat == model2.data.customer_seat &&
                   model1.data.cart_id == model2.data.cart_id &&
                   model1.data.subtotal == model2.data.subtotal &&
                   model1.data.total == model2.data.total &&
                   model1.data.is_splitted == model2.data.is_splitted &&
                   model1.data.discount_id == model2.data.discount_id &&
                   AreDetailCartListsEqual(model1.data.cart_details, model2.data.cart_details);
        }

        // Method to compare two lists of DetailCart objects
        private bool AreDetailCartListsEqual(List<DetailCart> list1, List<DetailCart> list2)
        {
            // Compare counts of DetailCart lists
            if (list1 == null || list2 == null || list1.Count != list2.Count)
            {
                return false;
            }

            // Compare each DetailCart object in the lists
            for (int i = 0; i < list1.Count; i++)
            {
                if (!AreDetailCartObjectsEqual(list1[i], list2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Method to compare two DetailCart objects
        private bool AreDetailCartObjectsEqual(DetailCart obj1, DetailCart obj2)
        {
            // Compare properties of DetailCart objects
            return obj1.cart_detail_id == obj2.cart_detail_id &&
                   obj1.menu_id == obj2.menu_id &&
                   obj1.menu_name == obj2.menu_name &&
                   obj1.menu_type == obj2.menu_type &&
                   obj1.menu_detail_id == obj2.menu_detail_id &&
                   obj1.is_ordered == obj2.is_ordered &&
                   obj1.varian == obj2.varian &&
                   obj1.serving_type_id == obj2.serving_type_id &&
                   obj1.serving_type_name == obj2.serving_type_name &&
                   obj1.discount_id == obj2.discount_id &&
                   obj1.discount_code == obj2.discount_code &&
                   obj1.discounts_value == obj2.discounts_value &&
                   obj1.discounted_price == obj2.discounted_price &&
                   obj1.discounts_is_percent == obj2.discounts_is_percent &&
                   obj1.price == obj2.price &&
                   obj1.total_price == obj2.total_price &&
                   obj1.qty == obj2.qty &&
                   obj1.note_item == obj2.note_item;
        }


        private async Task LoadCacheMenuItems()
        {
            try
            {
                UpdateDetailLabel("Load Menu Items dan Gambar");
                items = 0;
                progressBar.Value = items;

                if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }
                string response = await apiService.Get("/menu?outlet_id=" + baseOutlet);

                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                List<Menu> menuList = menuModel.data.ToList();

                // Save the menu data to a local file
                File.WriteAllText($"DT-Cache\\menu_outlet_id_{baseOutlet}.data", JsonConvert.SerializeObject(menuModel));
                // Using Select method to get the id numbers
                menuIds = menuList.Select(menu => menu.id).Where(id => id != 0).ToArray();
                foreach (Menu menu in menuList)
                {
                    items++;
                    int x = 100 / menuModel.data.Count;

                    PictureBox pictureBox = new PictureBox();
                    UpdateDetailLabel($"Downloading Data...[{items} / {menuModel.data.Count}]");


                    // Load gambar dari cache atau unduh jika tidak ada di cache
                    await LoadImageToPictureBox(pictureBox, menu);
                    progressBar.Value = x * items;

                }
                await LoadServingTypeItems();

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

        private async Task LoadServingTypeItems()
        {
            try
            {
                items = 0;
                progressBar.Value = items;
                UpdateDetailLabel("Load Serving Type Items");


                if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }
                if (!Directory.Exists(folderAddCartForm)) { Directory.CreateDirectory(folderAddCartForm); }

                // Open each menu by its id one by one

                foreach (int menuId in menuIds)
                {
                    items++;

                    string idmenu = menuId.ToString();
                    string response = await apiService.GetMenuByID("/menu", idmenu);
                    GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(response);
                    // Save the menu data to a local file
                    File.WriteAllText(folderAddCartForm + "\\LoadDataServingType_" + menuId + "_Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(menuModel));
                    int count = menuIds.Where(id => id != 0).Count();
                    UpdateProgressLabel($"Downloading Data...[{items} / {count}]");

                    int x = 100 / count;
                    progressBar.Value = x * items;
                }
                await LoadDataVarianItems();

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

        private async Task LoadDataVarianItems()
        {
            try
            {
                items = 0;
                UpdateDetailLabel("Load Data Varian Items");


                if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }
                if (!Directory.Exists(folderAddCartForm)) { Directory.CreateDirectory(folderAddCartForm); }

                // Open each menu by its id one by one

                foreach (int menuId in menuIds)
                {
                    items++;

                    string idmenu = menuId.ToString();
                    string response = await apiService.GetMenuDetailByID("/menu-detail", idmenu);
                    GetMenuDetailCartModel menuModel = JsonConvert.DeserializeObject<GetMenuDetailCartModel>(response);
                    // Save the menu data to a local file
                    File.WriteAllText(folderAddCartForm + "\\LoadDataVarian_" + idmenu + "_Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(menuModel));

                    int count = menuIds.Where(id => id != 0).Count();
                    UpdateProgressLabel($"Downloading Data...[{items} / {count}]");
                    int x = 100 / count;
                    progressBar.Value = x * items;
                }
                await LoadDiscountPeritems();

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


        private async Task LoadDiscountPeritems()
        {
            try
            {
                UpdateDetailLabel("Load Discount PerItems");


                items = 0;
                UpdateProgressLabel($"Downloading Data...");

                progressBar.Value = items;
                if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }
                if (!Directory.Exists(folderAddCartForm)) { Directory.CreateDirectory(folderAddCartForm); }

                string response = await apiService.GetDiscount($"/discount?outlet_id={baseOutlet}&is_discount_cart=", "0");
                DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(response);
                File.WriteAllText(folderAddCartForm + "\\LoadDataDiscountItem_" + "Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(menuModel));
                progressBar.Value = 100;
                await LoadPayform();

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


        private async Task LoadPayform()
        {
            try
            {
                UpdateDetailLabel("Load Data Payform");


                items = 0;
                UpdateProgressLabel($"Downloading Data...");

                progressBar.Value = items;

                if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }

                string response = await apiService.GetPaymentType("/payment-type");
                PaymentTypeModel payment = JsonConvert.DeserializeObject<PaymentTypeModel>(response);
                File.WriteAllText("DT-Cache" + "\\LoadDataPayment_" + "Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(payment));

                progressBar.Value = 100;
                await LoadDiscountPerCart();

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

        private async Task LoadDiscountPerCart()
        {
            try
            {
                UpdateDetailLabel("Load Data Discount Cart");


                items = 0;
                UpdateProgressLabel($"Downloading Data...");

                progressBar.Value = items;

                if (!Directory.Exists("DT-Cache")) { Directory.CreateDirectory("DT-Cache"); }

                string response = await apiService.GetDiscount($"/discount?outlet_id={baseOutlet}&is_discount_cart=", "1");
                DiscountCartModel menuModel = JsonConvert.DeserializeObject<DiscountCartModel>(response);

                File.WriteAllText("DT-Cache" + "\\LoadDiscountPerCart_" + "Outlet_" + baseOutlet + ".data", JsonConvert.SerializeObject(menuModel));

                progressBar.Value = 100;
                //util.sendLogTelegram("Selesai Sync Data. Outlet ID: " + baseOutlet);

                this.Close();

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


        // Fungsi untuk memuat gambar ke PictureBox dengan mengambil dari cache atau unduh jika tidak ada di cache
        private async Task LoadImageToPictureBox(PictureBox pictureBox, Menu menu)
        {

            if (!menuImageDictionary.ContainsKey(menu))
            {
                menuImageDictionary.Add(menu, LoadPlaceholderImage(70, 70)); // Placeholder image initially

                Image cachedImage = await LoadImageFromCache(menu.id.ToString());

                if (cachedImage != null)
                {

                    try
                    {


                        // Create a rounded rectangle for the PictureBox
                        using (Graphics graphics = Graphics.FromHwnd(pictureBox.Handle))
                        {
                            Rectangle rect = new Rectangle(0, 0, pictureBox.Width, pictureBox.Height);
                            graphics.FillRectangle(new SolidBrush(Color.White), rect);
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            // Create a GraphicsPath object for the rounded rectangle
                            GraphicsPath path = new GraphicsPath();
                            path.AddArc(rect.X, rect.Y, rect.Width / 2, rect.Height / 2, 180, 90);
                            path.AddArc(rect.Right - rect.Width / 2, rect.Y, rect.Width / 2, rect.Height / 2, 270, 90);
                            path.AddArc(rect.Right - rect.Width / 2, rect.Bottom - rect.Height / 2, rect.Width / 2, rect.Height / 2, 0, 90);
                            path.AddArc(rect.X, rect.Bottom - rect.Height / 2, rect.Width / 2, rect.Height / 2, 90, 90);
                            path.CloseFigure();

                            // Create a new Region object based on the GraphicsPath object
                            Region region = new Region(path);

                            // Set the PictureBox region to the rounded rectangle
                            pictureBox.Region = region;
                        }

                        // Set the PictureBox image and size
                        pictureBox.Image = cachedImage;
                        //pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle the error by displaying a message or logging the error
                        Console.WriteLine($"Error: {ex.Message}");
                        LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                    }
                }
                else
                {
                    try
                    {
                        Image downloadedImage = await LoadImageAsync(menu);
                        // Resize the image before adding it to the dictionary
                        //agar tidak out of memory saat load gambar dengan menggunakan method resize

                        //downloadedImage = ResizeImage(downloadedImage, 70);
                        ResizeImage(downloadedImage, 70);
                        // Check if the image size is within the limits of the PictureBox
                        CheckImageSize(downloadedImage);
                        try
                        {
                            //hapus sampe sini jika msh error
                            menuImageDictionary[menu] = downloadedImage;

                            if (pictureBox.InvokeRequired)
                            {
                                pictureBox.Invoke((MethodInvoker)delegate
                                {
                                    pictureBox.Image = downloadedImage;
                                });
                            }
                            else
                            {
                                pictureBox.Image = downloadedImage;
                            }
                        }
                        catch (ArgumentException ex)
                        {

                            // Handle the error by displaying a message or logging the error
                            Console.WriteLine($"Error: {ex.Message}");
                            LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                        }
                        // Save the downloaded image to cache
                        SaveImageToCache(downloadedImage, menu.id.ToString());
                    }
                    catch (ArgumentException ex)
                    {
                        //Console.WriteLine($"Error loading image: {ex.Message}");
                        LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    }
                }
            }
            else
            {
                pictureBox.Image = menuImageDictionary[menu];
            }
        }

        public static Image ResizeImage(Image image, int size)
        {
            // Create a new Bitmap with white background
            //perhitungan manual
            int deviden;
            int width, height;
            deviden = image.Height / size;
            height = image.Height * deviden;
            width = image.Width * deviden;

            var finalImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(finalImage))
            {
                // Set the background color to white
                graphics.Clear(Color.White);

                // Maintain aspect ratio while resizing
                float ratio = Math.Min((float)width / image.Width, (float)height / image.Height);
                int newWidth = (int)(image.Width * ratio);
                int newHeight = (int)(image.Height * ratio);

                // Calculate the position to center the image on the white background
                int posX = (width - newWidth) / 2;
                int posY = (height - newHeight) / 2;

                // Draw the image onto the white background
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, posX, posY, newWidth, newHeight);
            }

            return finalImage;
        }

        private void CheckImageSize(Image image)
        {
            // Get the size of the image
            Size imageSize = image.Size;

            // Check if the image size is within the limits of the PictureBox
            if (imageSize.Width > 150 || imageSize.Height > 150)
            {
                throw new ArgumentException("The image size is too large for the PictureBox.");

            }
        }
        //======================================resize image


        // Fungsi untuk memuat gambar dari cache lokal
        private async Task<Image> LoadImageFromCache(string imageName)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(appDirectory)?.Parent?.Parent?.FullName;
            string localImagePath = Path.Combine(projectDirectory, "ImageCache", imageName + ".Jpeg");

            if (File.Exists(localImagePath))
            {
                try
                {
                    using (FileStream stream = new FileStream(localImagePath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(stream);
                    }
                }
                catch (ArgumentException ex)
                {
                    // Log the error and return null if the image file is not valid
                    //Console.WriteLine($"Error loading image from cache: {ex.Message}");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

                    return null;
                }
            }

            return null;
        }

        // Fungsi untuk menyimpan gambar ke cache lokal
        //mengganti jpg to png >>>> If possible, store images in a lossless format, such as PNG, to reduce memory usage.
        private async void SaveImageToCache(Image image, string imageName)
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Directory.GetParent(appDirectory)?.Parent?.Parent?.FullName;
                string localDirectory = Path.Combine(projectDirectory, "ImageCache");
                Directory.CreateDirectory(localDirectory);

                string localImagePath = Path.Combine(localDirectory, imageName + ".Jpeg"); //before jpg
                                                                                           // Generate a random pastel color

                // Generate a random pastel color
                Random rand = new Random();
                int r = rand.Next(200, 255);
                int g = rand.Next(200, 255);
                int b = rand.Next(200, 255);
                Color pastelColor = Color.FromArgb(r, g, b);

                // Create a new bitmap with the pastel color background
                Bitmap bmp = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics graphic = Graphics.FromImage(bmp))
                {
                    graphic.Clear(pastelColor);
                    graphic.DrawImage(image, new Rectangle(new Point(), image.Size), new Rectangle(new Point(), image.Size), GraphicsUnit.Pixel);
                }

                // Save the new image
                using (FileStream stream = new FileStream(localImagePath, FileMode.Create, FileAccess.Write))
                {
                    bmp.Save(stream, ImageFormat.Jpeg);
                }
                /*
                using (FileStream stream = new FileStream(localImagePath, FileMode.Create, FileAccess.Write))
                {
                    image.Save(stream, ImageFormat.Jpeg);
                }
                */
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error saving image to cache: {ex.Message}");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }
        private async Task<Image> LoadImageAsync(Menu menu)
        {
            try
            {
                //start replace
                string imageUrl = baseUrl + "/" + menu.image_url;


                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                /*
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    return Image.FromStream(stream);
                }
                */

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    Image originalImage = Image.FromStream(stream);

                    // Generate a random pastel color
                    Random rand = new Random();
                    int r = rand.Next(200, 255);
                    int g = rand.Next(200, 255);
                    int b = rand.Next(200, 255);

                    Color pastelColor = Color.FromArgb(r, g, b);
                    // Create a new bitmap with the pastel color background
                    int width = Math.Min(70, originalImage.Width);
                    int height = Math.Min(70, originalImage.Height);
                    int left = (70 - width) / 2;
                    int top = (70 - height) / 2;
                    Bitmap bmp = new Bitmap(70, 70, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics graphic = Graphics.FromImage(bmp))
                    {
                        graphic.Clear(pastelColor);
                        graphic.DrawImage(originalImage, left, top, width, height);
                    }

                    return bmp;

                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error downloading image: {ex.Message}");
                return LoadPlaceholderImage(70, 70);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }
        private Image LoadPlaceholderImage(int width, int height)
        {
            // Path to the placeholder image
            string startupPath = AppDomain.CurrentDomain.BaseDirectory; // Get the startup folder
            string placeholderImagePath = Path.Combine(startupPath, "icon", "ImageNotFound.png");

            try
            {
                // Check if the file exists
                if (File.Exists(placeholderImagePath))
                {
                    // Load the image from file
                    Image placeholderImage = Image.FromFile(placeholderImagePath);

                    // Resize the image to the specified width and height
                    Image resizedPlaceholder = new Bitmap(placeholderImage, new Size(width, height));
                    return resizedPlaceholder;
                }
                else
                {
                    Console.WriteLine("Placeholder image not found at: " + placeholderImagePath);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception if necessary
                Console.WriteLine("Error loading placeholder image: " + ex.Message);
            }

            // If the image file is not found or an error occurs, use the generated placeholder logic
            Bitmap placeholder = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(placeholder))
            {
                // Generate a random pastel color
                Random Rand = new Random();
                int r = Rand.Next(128, 255); // Random red value between 128 and 255
                int g = Rand.Next(128, 255); // Random green value between 128 and 255
                int b = Rand.Next(128, 255); // Random blue value between 128 and 255
                Color pastelColor = Color.FromArgb(r, g, b);

                graphics.FillRectangle(new SolidBrush(pastelColor), 0, 0, width, height);

                // Calculate the center point of the image
                int x = (width - 70) / 2;
                int y = (height - 35) / 2;

                // Draw the placeholder text at the center point
                graphics.DrawString("Image\nTidak\nDiUpload", SystemFonts.DefaultFont, new SolidBrush(Color.FromArgb(30, 31, 68)), new PointF(x, y));
            }
            return placeholder;
        }


        // Method to asynchronously load the image
        /*
        private Image LoadPlaceholderImage(int width, int height)
        {
            // Replace this with your placeholder image logic
            // For example:

            string ex = "error download image";
            Bitmap placeholder = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(placeholder))
            {
                // Generate a random pastel color
                Random Rand = new Random();
                int r = Rand.Next(128, 255); // Random red value between 128 and 255
                int g = Rand.Next(128, 255); // Random green value between 128 and 255
                int b = Rand.Next(128, 255); // Random blue value between 128 and 255
                Color pastelColor = Color.FromArgb(r, g, b);

                graphics.FillRectangle(new SolidBrush(pastelColor), 0, 0, width, height);

                // Calculate the center point of the image
                int x = (width - 70) / 2;
                int y = (height - 35) / 2;

                // Draw the placeholder text at the center point
                // Draw the placeholder text at the center point
                graphics.DrawString("Image\nTidak\nDiUpload", SystemFonts.DefaultFont, new SolidBrush(Color.FromArgb(30, 31, 68)), new PointF(x, y));
            }
            return placeholder;
        }
        */



    }



}
