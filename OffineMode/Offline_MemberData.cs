using System.Data;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.Properties;
using Newtonsoft.Json;
using Serilog;

namespace KASIR.Komponen
{
    public partial class Offline_MemberData : Form
    {
        private string customMember;
        private DataTable listDataTable;
        private readonly string baseOutlet;

        public Offline_MemberData()
        {
            InitializeComponent();
            baseOutlet = Settings.Default.BaseOutlet;
            loadDataMember();
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        // Single Member property to store selected member
        public Member SelectedMember { get; private set; }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0) 
            {
                if (e.RowIndex % 2 == 0) 
                {
                    e.CellStyle.BackColor = Color.White; 
                }
                else 
                {
                    e.CellStyle.BackColor = Color.WhiteSmoke; 
                }
            }
        }

        private void TambahMember_Click(object sender, EventArgs e)
        {
            try
            {
                customMember = "Tambah";
                Form background = new()
                {
                    StartPosition = FormStartPosition.Manual,
                    FormBorderStyle = FormBorderStyle.None,
                    Opacity = 0.7d,
                    BackColor = Color.Black,
                    WindowState = FormWindowState.Maximized,
                    TopMost = true,
                    Location = Location,
                    ShowInTaskbar = false
                };
                SelectedMember = new Member
                {
                    member_id = 0,
                    member_name = "add new member",
                    member_phone_number = "08121",
                    member_email = "addmember@gmail.com",
                    member_points = 0
                };
                using (Offline_MemberCustom addMember = new(customMember, SelectedMember.member_id,
                           SelectedMember.member_name, SelectedMember.member_phone_number, SelectedMember.member_email))
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
                        background.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void PerformSearchList()
        {
            try
            {
                if (listDataTable == null)
                {
                    return;
                }
                string searchTerm = txtCariMenuList.Text.ToLower();

                DataTable filteredDataTable = listDataTable.Clone();
                int items = 0;
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
                // Only clear DataGridView DataSource when necessary
                string cacheFilePath = $"DT-Cache\\DataMember_Outlet{baseOutlet}.data";
                // Clear previous data
                dataGridView1.DataSource = null;
                // Check if network is available
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    if (File.Exists(cacheFilePath))
                    {
                        string cachedJson = File.ReadAllText(cacheFilePath);
                        var cachedMembers = JsonConvert.DeserializeObject<GetMemberModel>(cachedJson)?.data;

                                UpdateDataGridView(cachedMembers);
                                return; // Exit as data is already up-to-date
                    }
                    else
                    {
                        MessageBox.Show("No Connection Internet to Fetch Membership");
                    }
                    return;
                }
                // Check if the cache file exists
                if (File.Exists(cacheFilePath))
                {
                    // Load data from cache
                    string cachedJson = File.ReadAllText(cacheFilePath);
                    var cachedMembers = JsonConvert.DeserializeObject<GetMemberModel>(cachedJson)?.data;

                    // Get data from API
                    IApiService apiService = new ApiService();
                    string response = await apiService.GetMember("/membership");
                    var apiResponse = JsonConvert.DeserializeObject<GetMemberModel>(response);

                    if (apiResponse != null && cachedMembers != null)
                    {
                        bool isEqual = cachedMembers.Count() == apiResponse.data.Count() &&
                                       cachedMembers.All(m => apiResponse.data.Any(a => a.member_id == m.member_id &&
                                                                                        a.member_name == m.member_name &&
                                                                                        a.member_email == m.member_email &&
                                                                                        a.member_phone_number == m.member_phone_number));

                        if (isEqual)
                        {
                            UpdateDataGridView(cachedMembers);
                            return; // Exit as data is already up-to-date
                        }
                    }
                }


                // Fetch from API if cache is not available or needs updating
                IApiService apiServiceNew = new ApiService();
                string apiResponseNew = await apiServiceNew.GetMember("/membership");

                var apiMemberList = JsonConvert.DeserializeObject<GetMemberModel>(apiResponseNew)?.data;

                if (apiMemberList != null)
                {
                    List<Member> finalMemberList = new List<Member>();
                    PopulateMemberList(apiMemberList, finalMemberList, cacheFilePath);

                    File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(new GetMemberModel { data = finalMemberList }));

                    UpdateDataGridView(finalMemberList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display member data: " + ex.Message, "Error");
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }


        public void PopulateMemberList(IEnumerable<Member> apiMemberList, List<Member> finalMemberList, string cacheFilePath)
        {
            if (File.Exists(cacheFilePath))
            {
                string cachedJson = File.ReadAllText(cacheFilePath);
                var cachedMembers = JsonConvert.DeserializeObject<GetMemberModel>(cachedJson)?.data;

                var cachedDict = cachedMembers.ToDictionary(m => m.member_id);

                HashSet<int> addedMemberIds = new HashSet<int>(); // To track added members

                foreach (var apiMember in apiMemberList)
                {
                    // Check if this member id has already been added
                    if (!addedMemberIds.Contains(apiMember.member_id))
                    {
                        if (cachedDict.TryGetValue(apiMember.member_id, out var cachedMember))
                        {
                            apiMember.member_points = cachedMember.member_points; // Keep the points from the cache
                        }
                        else
                        {
                            apiMember.member_points = 0; // New member
                        }

                        finalMemberList.Add(apiMember);
                        addedMemberIds.Add(apiMember.member_id); // Mark this member as added
                    }
                }
            }
            else
            {
                foreach (var apiMember in apiMemberList)
                {
                    if (!finalMemberList.Any(m => m.member_id == apiMember.member_id)) // Check for existing member
                    {
                        apiMember.member_points = 0;
                        finalMemberList.Add(apiMember);
                    }
                }
            }
        }

        private void UpdateDataGridView(IEnumerable<Member> memberList)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();  // Clear all existing columns

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Nama", typeof(string));
            dataTable.Columns.Add("No. HP", typeof(string));
            dataTable.Columns.Add("Email", typeof(string));
            dataTable.Columns.Add("Points", typeof(string));

            foreach (Member member in memberList)
            {
                dataTable.Rows.Add(member.member_id, member.member_name, member.member_phone_number, member.member_email, member.member_points.ToString("#,0"));
            }

            dataGridView1.DataSource = dataTable;

            AddButtonColumns();

            dataGridView1.Columns["ID"].Visible = false;

            lblCountingItems.Text = $"{memberList.Count()} Member ditemukan.";
            listDataTable = dataTable.Copy();  // Store the current data for further operations
        }
        private void RemoveButtonColumns()
        {
            if (dataGridView1.Columns.Contains("Pilih"))
            {
                dataGridView1.Columns.Remove("Pilih");
            }
            if (dataGridView1.Columns.Contains("Edit"))
            {
                dataGridView1.Columns.Remove("Edit");
            }
        }

        private void AddButtonColumns()
        {
            if (!dataGridView1.Columns.Contains("Pilih"))
            {
                DataGridViewButtonColumn buttonColumn = new()
                {
                    HeaderText = "Pilih",
                    Text = "Pilih",
                    FlatStyle = FlatStyle.Flat,
                    UseColumnTextForButtonValue = true
                };
                dataGridView1.Columns.Add(buttonColumn);
            }

            if (!dataGridView1.Columns.Contains("Edit"))
            {
                DataGridViewButtonColumn buttonColumn1 = new()
                {
                    HeaderText = "Edit",
                    Text = "Edit",
                    FlatStyle = FlatStyle.Flat,
                    UseColumnTextForButtonValue = true
                };
                dataGridView1.Columns.Add(buttonColumn1);
            }
        }
        private string CleanInput(string input)
        {
            return Regex.Replace(input, "[^0-9]", "");
        }
        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Pilih" && e.RowIndex >= 0)
            {
                try
                {
                    // Create a new Member instance from the selected row
                    SelectedMember = new Member
                    {
                        member_id = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value),
                        member_name = dataGridView1.Rows[e.RowIndex].Cells["Nama"].Value.ToString(),
                        member_phone_number = dataGridView1.Rows[e.RowIndex].Cells["No. HP"].Value.ToString(),
                        member_email = dataGridView1.Rows[e.RowIndex].Cells["Email"].Value.ToString(),
                        member_points = int.Parse(CleanInput(dataGridView1.Rows[e.RowIndex].Cells["Points"].Value.ToString()))
                    };

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

                // Create a new Member instance from the selected row
                SelectedMember = new Member
                {
                    member_id = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value),
                    member_name = dataGridView1.Rows[e.RowIndex].Cells["Nama"].Value.ToString(),
                    member_phone_number = dataGridView1.Rows[e.RowIndex].Cells["No. HP"].Value.ToString(),
                    member_email = dataGridView1.Rows[e.RowIndex].Cells["Email"].Value.ToString(),
                    member_points = int.Parse(CleanInput(dataGridView1.Rows[e.RowIndex].Cells["Points"].Value.ToString()))
                };
                try
                {
                    Form background = new()
                    {
                        StartPosition = FormStartPosition.Manual,
                        FormBorderStyle = FormBorderStyle.None,
                        Opacity = 0.7d,
                        BackColor = Color.Black,
                        WindowState = FormWindowState.Maximized,
                        TopMost = true,
                        Location = Location,
                        ShowInTaskbar = false
                    };

                    using (Offline_MemberCustom editMemberForm = new(customMember, SelectedMember.member_id, SelectedMember.member_name, SelectedMember.member_phone_number, SelectedMember.member_email))
                    {
                        editMemberForm.Owner = background;

                        background.Show();

                        DialogResult result = editMemberForm.ShowDialog();

                        // Handle the result if needed
                        if (result == DialogResult.OK)
                        {
                            background.Dispose();
                            loadDataMember();
                        }
                        else
                        {
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