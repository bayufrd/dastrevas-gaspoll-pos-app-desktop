using System.Data;
using System.Net.NetworkInformation;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;


namespace KASIR.Komponen
{
    public partial class Offline_MemberData : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        int idMember, items, idid;
        private DataTable originalDataTable, listDataTable;
        string customMember;

        public string namaMember { get; private set; }
        public string emailMember { get; private set; }
        public string hpMember { get; private set; }
        public int SelectedId { get; private set; }
        public Offline_MemberData()
        {
            InitializeComponent();
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

            using (Offline_MemberCustom addMember = new Offline_MemberCustom(customMember, idMember, namaMember, hpMember, emailMember))
            {
                addMember.Owner = background;
                background.Show();
                DialogResult result = addMember.ShowDialog();
                if (result == DialogResult.OK)
                {
                    loadDataMember();
                    background.Dispose();
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
            }
        }

        private async void loadDataMember()
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    MessageBox.Show("No network connection available. Please check your internet connection and try again.", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();

                IApiService apiService = new ApiService();
                string response = await apiService.GetMember("/membership");

                GetMemberModel member = JsonConvert.DeserializeObject<GetMemberModel>(response);
                List<Member> memberList = member.data.ToList();

                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(int));
                dataTable.Columns.Add("Nama", typeof(string));
                dataTable.Columns.Add("No. HP", typeof(string));
                dataTable.Columns.Add("Email", typeof(string));
                items = 0;




                foreach (Member data in memberList)
                {
                    dataTable.Rows.Add(data.member_id, data.member_name, data.member_phone_number, data.member_email);
                    items++;
                }

                dataGridView1.DataSource = dataTable;
                DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
                buttonColumn.HeaderText = "Pilih";
                buttonColumn.Text = "Pilih";
                buttonColumn.FlatStyle = FlatStyle.Flat;
                buttonColumn.UseColumnTextForButtonValue = true; // Displays the "Add to Cart" text on the button
                DataGridViewButtonColumn buttonColumn1 = new DataGridViewButtonColumn();
                buttonColumn1.HeaderText = "Edit";
                buttonColumn1.Text = "Edit";
                buttonColumn1.FlatStyle = FlatStyle.Flat;
                buttonColumn1.UseColumnTextForButtonValue = true; // Displays the "Add to Cart" text on the button
                dataGridView1.Columns.Add(buttonColumn);
                dataGridView1.Columns.Add(buttonColumn1);

                if (dataGridView1.DataSource != null)
                {
                    dataGridView1.Columns["ID"].Visible = false;
                }
                lblCountingItems.Text = $"{items} Member ditemukan.";

                dataGridView1.DataSource = dataTable;
                listDataTable = dataTable.Copy();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal tampil data bill  " + ex.Message, "Gaspol");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
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

                    using (Offline_MemberCustom addMember = new Offline_MemberCustom(customMember, idMember, namaMember, hpMember, emailMember))
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
