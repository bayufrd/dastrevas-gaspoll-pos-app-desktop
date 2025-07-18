using System.Data;
using System.Text.RegularExpressions;
using KASIR.Model;
using KASIR.Network;
using KASIR.Printer;
using Newtonsoft.Json;
using Serilog;


namespace KASIR.Komponen
{
    public partial class dataBill : Form
    {
         

        private readonly string baseOutlet;
        private readonly string MacAddressKasir;
        private readonly string MacAddressKitchen;
        private readonly string MacAddressBar;
        private readonly string PinPrinterKasir;
        private readonly string PinPrinterKitchen;
        private readonly string PinPrinterBar;
        private readonly string BaseOutletName;

        int nomor = 0;

        public bool ReloadDataInBaseForm { get; private set; }
        public dataBill()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            MacAddressKitchen = Properties.Settings.Default.MacAddressKitchen;
            MacAddressBar = Properties.Settings.Default.MacAddressBar;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            PinPrinterKitchen = Properties.Settings.Default.PinPrinterKitchen;
            PinPrinterBar = Properties.Settings.Default.PinPrinterBar;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
            LoadData();
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;

        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0) // Memastikan kita hanya memformat sel data
            {
                if (e.RowIndex % 2 == 0) // Baris genap
                {
                    e.CellStyle.BackColor = Color.White; // Warna untuk baris genap
                }
                else // Baris ganjil
                {
                    e.CellStyle.BackColor = Color.WhiteSmoke; // Warna untuk baris ganjil
                }
            }
        }
        private async void LoadData()
        {
            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.GetListBill("/transaction?outlet_id=", baseOutlet);

                ListBillModel menuModel = JsonConvert.DeserializeObject<ListBillModel>(response);
                List<ListBill> menuList = menuModel.data.ToList();
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(int));
                dataTable.Columns.Add("Nama", typeof(string));
                dataTable.Columns.Add("Terakhir Update", typeof(string));
                dataTable.Columns.Add("Seat", typeof(string));

                string format = "dddd, dd MMMM yyyy - HH:mm";

                foreach (ListBill menu in menuList)
                {
                    nomor++;
                    // Parsing tanggal dari string API dan format ulang
                    DateTime updatedAt;
                    // Parsing tanggal menggunakan format yang sesuai
                    if (DateTime.TryParseExact(menu.updated_at, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out updatedAt))
                    {
                        // Format ulang menjadi lebih singkat, misalnya "19 Oct 2024, 10:38"
                        string formattedDate = updatedAt.ToString("dd MMM yyyy, HH:mm");
                        dataTable.Rows.Add(menu.id, nomor.ToString() + "." + menu.customer_name, formattedDate, menu.customer_seat.ToString());
                    }
                    else
                    {
                        // Jika parsing gagal, tampilkan seperti apa adanya
                        dataTable.Rows.Add(menu.id, nomor.ToString() + "." + menu.customer_name, menu.updated_at, menu.customer_seat.ToString());
                    }
                }

                dataGridView1.DataSource = dataTable;
                DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
                buttonColumn.HeaderText = "Pilih Bill";
                buttonColumn.Text = "Pilih";
                buttonColumn.FlatStyle = FlatStyle.Flat;
                buttonColumn.UseColumnTextForButtonValue = true; // Displays the "Add to Cart" text on the button
                DataGridViewButtonColumn buttonColumn1 = new DataGridViewButtonColumn();
                buttonColumn1.HeaderText = "Struk";
                buttonColumn1.Text = "Cetak";
                buttonColumn1.FlatStyle = FlatStyle.Flat;
                buttonColumn1.UseColumnTextForButtonValue = true;
                dataGridView1.Columns.Add(buttonColumn);
                dataGridView1.Columns.Add(buttonColumn1);
                if (dataGridView1.DataSource != null)
                {
                    // Pastikan kolom "ID" ada dalam DataGridView sebelum mencoba mengaksesnya
                    if (dataGridView1.Columns.Contains("ID"))
                    {
                        dataGridView1.Columns["ID"].Visible = false;
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
                MessageBox.Show("Gagal tampil data bill  " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1 == null || dataGridView1.Columns.Count <= e.ColumnIndex || e.RowIndex < 0)
            {
                MessageBox.Show("Invalid selection", "Error");
                return;
            }

            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Pilih Bill" && e.RowIndex >= 0)
            {
                var cellValue = dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
                if (cellValue == null)
                {
                    MessageBox.Show("Invalid cell value", "Error");
                    return;
                }

                int selectedId = Convert.ToInt32(cellValue);
                try
                {
                    IApiService apiService = new ApiService();
                    string response = await apiService.GetActiveCart("/transaction/" + selectedId + "?outlet_id=" + baseOutlet);
                    if (response != null)
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    DialogResult = DialogResult.Cancel;
                    MessageBox.Show("Gagal load keranjang " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    Close();
                }
            }
            else if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Struk" && e.RowIndex >= 0)
            {
                PrinterModel printerModel = new PrinterModel();
                var cellValue = dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
                if (cellValue == null)
                {
                    MessageBox.Show("Invalid cell value", "Error");
                    return;
                }

                int selectedId = Convert.ToInt32(cellValue);

                var antrianCell = dataGridView1.Rows[e.RowIndex].Cells["Nama"].Value;
                int AntrianSaveBill = 0;

                if (antrianCell != null)
                {
                    // Mencari angka sebelum titik menggunakan regex
                    Regex regex = new Regex(@"^\d+"); // Mencocokkan angka di awal string
                    Match match = regex.Match(antrianCell.ToString());

                    if (match.Success)
                    {
                        AntrianSaveBill = int.Parse(match.Value); // Ambil angka yang ditemukan
                    }
                }

                try
                {
                    IApiService apiService = new ApiService();
                    string response = await apiService.Restruk("/transaction/" + selectedId + "?outlet_id=" + baseOutlet + "&is_struct=1");
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        await HandleSuccessfulTransaction(response, AntrianSaveBill);
                    }
                    else
                    {
                        MessageBox.Show("Gagal memproses transaksi. Silahkan coba lagi.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch (TaskCanceledException ex)
                {
                    MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal cetak ulang struk " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
        }

        private async Task HandleSuccessfulTransaction(string response, int AntrianSaveBill)
        {
            try
            {
                PrinterModel printerModel = new PrinterModel();

                RestrukModel restrukModel = JsonConvert.DeserializeObject<RestrukModel>(response);

                if (restrukModel == null)
                {
                    throw new InvalidOperationException("Deserialization failed: restrukModel is null");
                }
                DataRestruk data = restrukModel.data;

                if (data == null)
                {
                    throw new InvalidOperationException("Deserialization failed: data is null");
                }
                List<CartDetailRestruk> listCart = data.cart_details;
                List<CanceledItemStrukCustomerRestruk> listCancel = data.canceled_items;

                DataRestruk datas = restrukModel.data;
                List<CartDetailRestruk> cartDetails = datas.cart_details;
                List<CanceledItemStrukCustomerRestruk> canceledItems = datas.canceled_items;

                if (printerModel != null)
                {
                    await Task.Run(() =>
                    {
                        printerModel.PrintModelDataBill(datas, cartDetails, canceledItems, AntrianSaveBill);
                    });
                }
                else
                {
                    throw new InvalidOperationException("printerModel is null");
                }
                DialogResult = DialogResult.OK;

                Close();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
    }
}
