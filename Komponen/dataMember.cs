using System.Data;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.Services;
using Newtonsoft.Json;


namespace KASIR.Komponen
{
    public partial class dataMember : Form
    {
        int idMember, items;
        private DataTable listDataTable;
        string customMember;
        private IInternetService _internetServices;

        public string namaMember { get; private set; }
        public string emailMember { get; private set; }
        public string hpMember { get; private set; }
        public int SelectedId { get; private set; }
        public dataMember()
        {
            InitializeComponent();
            _internetServices = new InternetService();

            loadDataMember();
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
        private void TambahMember_Click(object sender, EventArgs e)
        {
            customMember = "Tambah";
            Form background = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = this.Location,
                ShowInTaskbar = false,
            };

            using (addMember addMember = new addMember(customMember, idMember, namaMember, hpMember, emailMember))
            {
                addMember.Owner = background;

                background.Show();

                //DialogResult dialogResult = dataBill.ShowDialog();

                //background.Dispose();
                DialogResult result = addMember.ShowDialog();

                // Handle the result if needed
                if (result == DialogResult.OK)
                {
                    loadDataMember();
                    background.Dispose();
                    // Settings were successfully updated, perform any necessary actions
                }
                else
                {
                    MessageBox.Show("Gagal Simpan, Silahkan coba lagi");
                    loadDataMember();

                    background.Dispose();
                }
            }
        }

        private void PerformSearchList()
        {
            try
            {
                if (listDataTable == null)
                    return;

                string searchTerm = txtCariMenuList.Text.ToLower();

                DataTable filteredDataTable = listDataTable.Clone();
                items = 0;
                IEnumerable<DataRow> filteredRows = listDataTable.AsEnumerable()
                    .Where(row => row.ItemArray.Any(field => field.ToString().ToLower().Contains(searchTerm)));

                foreach (DataRow row in filteredRows)
                {
                    filteredDataTable.ImportRow(row);
                    items++;
                }
                lblCountingItems.Text = $"{items} Member ditemukan.";

                dataGridView1.DataSource = filteredDataTable;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                //MessageBox.Show("Error: terlalu banyak aksi, aplikasi akan dijalankan ulang");
                //Application.Restart();
                //Environment.Exit(0);
            }
        }
        // Metode bantuan untuk inisialisasi kontrol
        private void EnsureControlsInitialized()
        {
            // Inisialisasi DataGridView jika null
            if (dataGridView1 == null)
            {
                dataGridView1 = new DataGridView();
                this.Controls.Add(dataGridView1);
            }

            // Inisialisasi label jika null
            if (lblCountingItems == null)
            {
                lblCountingItems = new Label();
                this.Controls.Add(lblCountingItems);
            }
        }

        // Metode untuk mereset DataGridView
        private void ResetDataGridView()
        {
            if (dataGridView1 != null)
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
            }
        }

        // Metode untuk membuat DataTable
        private DataTable CreateMemberDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Nama", typeof(string));
            dataTable.Columns.Add("No. HP", typeof(string));
            dataTable.Columns.Add("Email", typeof(string));
            return dataTable;
        }

        // Metode untuk menambahkan kolom button
        private void AddButtonColumns()
        {
            if (dataGridView1 == null) return;

            var buttonColumns = new[]
            {
        new { HeaderText = "Pilih", Text = "Pilih" },
        new { HeaderText = "Edit", Text = "Edit" }
    };

            foreach (var columnInfo in buttonColumns)
            {
                var buttonColumn = new DataGridViewButtonColumn
                {
                    HeaderText = columnInfo.HeaderText,
                    Text = columnInfo.Text,
                    FlatStyle = FlatStyle.Flat,
                    UseColumnTextForButtonValue = true
                };
                dataGridView1.Columns.Add(buttonColumn);
            }
        }

        // Metode untuk menangani error JSON
        private void HandleJsonDeserializationError(JsonException ex)
        {
            string errorMessage = $"Kesalahan parsing data: {ex.Message}";
            MessageBox.Show(errorMessage, "Parsing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            LoggerUtil.LogError(ex, errorMessage);
        }

        // Metode untuk menangani error umum
        private void HandleGeneralError(Exception ex)
        {
            string errorMessage = $"Gagal menampilkan data member: {ex.Message}";
            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            LoggerUtil.LogError(ex, errorMessage);
        }
        private async void loadDataMember()
        {
            try
            {
                // Pastikan kontrol diinisialisasi
                EnsureControlsInitialized();

                // Inisialisasi internet service
                _internetServices ??= new InternetService();

                // Cek koneksi internet
                if (!_internetServices.IsInternetConnected())
                {
                    // Coba load dari file cache
                    if (LoadMemberDataFromCache())
                    {
                        return; // Berhasil load dari cache
                    }

                    // Jika tidak ada cache
                    MessageBox.Show(
                        "Tidak ada koneksi internet dan tidak ada data member tersimpan.",
                        "Informasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                // Jika ada internet, proses dari API
                await LoadMemberDataFromApiAsync();
            }
            catch (Exception ex)
            {
                HandleGeneralError(ex);
            }
        }
        private string GetMemberCacheFilePath()
        {
            // Buat direktori cache jika belum ada
            string cacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DT-Cache");
            Directory.CreateDirectory(cacheDirectory);

            // Path file cache untuk member
            return Path.Combine(cacheDirectory, "MemberCache.data");
        }

        private bool LoadMemberDataFromCache()
        {
            try
            {
                string cachePath = GetMemberCacheFilePath();

                // Periksa apakah file cache ada
                if (!File.Exists(cachePath))
                {
                    return false;
                }

                // Baca isi file cache
                string cachedData = File.ReadAllText(cachePath);

                // Deserialisasi data
                var cachedMemberModel = JsonConvert.DeserializeObject<GetMemberModel>(cachedData);

                if (cachedMemberModel?.data == null || !cachedMemberModel.data.Any())
                {
                    MessageBox.Show(
                        "Data member dalam cache kosong.",
                        "Informasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return false;
                }

                // Buat DataTable dari data cache
                DataTable dataTable = CreateMemberDataTable();
                items = 0;

                foreach (Member data in cachedMemberModel.data)
                {
                    dataTable.Rows.Add(
                        data.member_id,
                        data.member_name ?? "N/A",
                        data.member_phone_number ?? "N/A",
                        data.member_email ?? "N/A",
                        data.member_points // Tambahkan points
                    );
                    items++;
                }

                // Set DataSource
                if (dataGridView1 != null)
                {
                    dataGridView1.DataSource = dataTable;
                    AddButtonColumns();

                    // Sembunyikan kolom ID
                    if (dataGridView1.Columns.Contains("ID"))
                    {
                        dataGridView1.Columns["ID"].Visible = false;
                    }
                }

                // Update label
                if (lblCountingItems != null)
                {
                    lblCountingItems.Text = $"{items} Member ditemukan (dari cache).";
                }

                // Simpan salinan data
                listDataTable = dataTable.Copy();

                return true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal memuat data member dari cache");
                return false;
            }
        }

        private async Task LoadMemberDataFromApiAsync()
        {
            try
            {
                // Reset DataGridView
                ResetDataGridView();

                // Inisialisasi ApiService
                IApiService apiService = new ApiService();

                // Ambil response
                string response = await apiService.GetMember("/membership");

                // Validasi response
                if (string.IsNullOrWhiteSpace(response))
                {
                    MessageBox.Show(
                        "Tidak ada data member yang diterima dari server.",
                        "Informasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                // Deserialisasi
                GetMemberModel member = JsonConvert.DeserializeObject<GetMemberModel>(response);

                if (member?.data == null || !member.data.Any())
                {
                    MessageBox.Show(
                        "Data member kosong.",
                        "Informasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                // Simpan ke cache
                await SaveMemberDataToCacheAsync(response);

                // Buat DataTable
                DataTable dataTable = CreateMemberDataTable();
                items = 0;

                // Tambahkan data ke DataTable
                foreach (Member data in member.data)
                {
                    dataTable.Rows.Add(
                        data.member_id,
                        data.member_name ?? "N/A",
                        data.member_phone_number ?? "N/A",
                        data.member_email ?? "N/A",
                        data.member_points // Tambahkan points
                    );
                    items++;
                }

                // Set DataSource
                if (dataGridView1 != null)
                {
                    dataGridView1.DataSource = dataTable;
                    AddButtonColumns();

                    // Sembunyikan kolom ID
                    if (dataGridView1.Columns.Contains("ID"))
                    {
                        dataGridView1.Columns["ID"].Visible = false;
                    }
                }

                // Update label
                if (lblCountingItems != null)
                {
                    lblCountingItems.Text = $"{items} Member ditemukan.";
                }

                // Simpan salinan data
                listDataTable = dataTable.Copy();
            }
            catch (JsonException jsonEx)
            {
                HandleJsonDeserializationError(jsonEx);
            }
            catch (Exception ex)
            {
                HandleGeneralError(ex);
            }
        }
        private async Task SaveMemberDataToCacheAsync(string memberData)
        {
            try
            {
                string cachePath = GetMemberCacheFilePath();

                // Simpan data ke file cache
                await File.WriteAllTextAsync(cachePath, memberData);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Gagal menyimpan data member ke cache");
            }
        }
        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Pilih" && e.RowIndex >= 0)
            {

                //int selectedId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value);
                try
                {
                    SelectedId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value);
                    namaMember = dataGridView1.Rows[e.RowIndex].Cells["Nama"].Value.ToString();
                    hpMember = dataGridView1.Rows[e.RowIndex].Cells["No. HP"].Value.ToString();
                    emailMember = dataGridView1.Rows[e.RowIndex].Cells["Email"].Value.ToString();

                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    DialogResult = DialogResult.Cancel;
                    MessageBox.Show("Gagal load Member " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    Close();
                }



            }

            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Edit" && e.RowIndex >= 0)
            {
                customMember = "Edit";

                idMember = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value);
                namaMember = dataGridView1.Rows[e.RowIndex].Cells["Nama"].Value.ToString();
                hpMember = dataGridView1.Rows[e.RowIndex].Cells["No. HP"].Value.ToString();
                emailMember = dataGridView1.Rows[e.RowIndex].Cells["Email"].Value.ToString();

                try
                {
                    Form background = new Form
                    {
                        StartPosition = FormStartPosition.Manual,
                        FormBorderStyle = FormBorderStyle.None,
                        Opacity = 0.7d,
                        BackColor = Color.Black,
                        WindowState = FormWindowState.Maximized,
                        TopMost = true,
                        Location = this.Location,
                        ShowInTaskbar = false,
                    };

                    using (addMember addMember = new addMember(customMember, idMember, namaMember, hpMember, emailMember))
                    {
                        addMember.Owner = background;

                        background.Show();

                        DialogResult result = addMember.ShowDialog();

                        // Handle the result if needed
                        if (result == DialogResult.OK)
                        {
                            background.Dispose();
                            loadDataMember();
                        }
                        else
                        {
                            loadDataMember();

                            background.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    DialogResult = DialogResult.Cancel;
                    MessageBox.Show("Gagal load Member " + ex.Message, "Gaspol");
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                    Close();
                }



            }
        }

        private void txtCariMenuList_TextChanged(object sender, EventArgs e)
        {
            PerformSearchList();
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            MessageBox.Show("Tidak ada member yang dipilih!");
            Close();
        }
    }
}
