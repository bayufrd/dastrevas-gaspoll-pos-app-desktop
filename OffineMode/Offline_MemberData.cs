using System.Data;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using KASIR.Helper;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using KASIR.Properties;
using KASIR.Services;
using Newtonsoft.Json;
using Serilog;

namespace KASIR.Komponen
{
    public partial class Offline_MemberData : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        private string customMember;
        private DataTable listDataTable;
        private readonly string baseOutlet;
        private IInternetService _internetServices;

        public Offline_MemberData()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            _internetServices = new InternetService();

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
                    QuestionHelper bg = new(null, null, null, null);
                    Form background = bg.CreateOverlayForm();

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
                        NotifyHelper.Error("Gagal Simpan, Silahkan coba lagi");
                        background.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyHelper.Error(ex.ToString());
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

        private async Task loadDataMember()
        {
            try
            {
                string cacheFilePath = $"DT-Cache\\DataMember_Outlet{baseOutlet}.data";
                dataGridView1.DataSource = null;

                if (!_internetServices.IsInternetConnected())
                {
                    if (File.Exists(cacheFilePath))
                    {
                        string cachedJson = File.ReadAllText(cacheFilePath);
                        var cachedMembers = JsonConvert.DeserializeObject<GetMemberModel>(cachedJson)?.data;

                        UpdateDataGridView(cachedMembers);
                        return;
                    }
                    else
                    {
                        NotifyHelper.Error("No Connection Internet to Fetch Membership");
                    }
                    return;
                }

                // Get data from API
                IApiService apiService = new ApiService();
                string response = await apiService.GetMember("/membership");
                var apiResponse = JsonConvert.DeserializeObject<GetMemberModel>(response);

                if (apiResponse?.data == null)
                {
                    NotifyHelper.Error("Failed to retrieve member data from API");
                    return;
                }

                List<Member> finalMemberList = new List<Member>();
                PopulateMemberList(apiResponse.data, finalMemberList, cacheFilePath);

                // Always update cache with latest API data
                File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(new GetMemberModel { data = finalMemberList }, Formatting.Indented));

                UpdateDataGridView(finalMemberList);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error("Failed to display member data: " + ex.Message);
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
                            // Compare updated_at and choose the most recent record
                            DateTime apiUpdatedAt = ParseDateTime(apiMember.updated_at);
                            DateTime cachedUpdatedAt = ParseDateTime(cachedMember.updated_at);

                            Member memberToAdd;
                            if (apiUpdatedAt > cachedUpdatedAt)
                            {
                                // API data is more recent
                                memberToAdd = apiMember;

                                // Preserve points from cached data if API doesn't provide points
                                if (memberToAdd.member_points == 0)
                                {
                                    memberToAdd.member_points = cachedMember.member_points;
                                }
                            }
                            else if (apiUpdatedAt < cachedUpdatedAt)
                            {
                                // Cached data is more recent
                                memberToAdd = cachedMember;

                                // Preserve points from API if cached points are 0
                                if (memberToAdd.member_points == 0 && apiMember.member_points > 0)
                                {
                                    memberToAdd.member_points = apiMember.member_points;
                                }
                            }
                            else // Equal timestamps
                            {
                                // Choose the member with more points
                                memberToAdd = apiMember.member_points > cachedMember.member_points
                                    ? apiMember
                                    : cachedMember;
                            }

                            finalMemberList.Add(memberToAdd);
                        }
                        else
                        {
                            // New member from API
                            finalMemberList.Add(apiMember);
                        }
                        addedMemberIds.Add(apiMember.member_id);
                    }
                }
            }
            else
            {
                foreach (var apiMember in apiMemberList)
                {
                    if (!finalMemberList.Any(m => m.member_id == apiMember.member_id)) // Check for existing member
                    {
                        finalMemberList.Add(apiMember);
                    }
                }
            }
        }

        // Helper method to parse datetime safely
        private DateTime ParseDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return DateTime.MinValue;

            if (DateTime.TryParse(dateTimeString, out DateTime parsedDate))
            {
                return parsedDate;
            }

            return DateTime.MinValue;
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
            listDataTable = dataTable.Copy();
        }
        private void AddButtonColumns()
        {
            if (dataGridView1 == null) return;

            // Konfigurasi kolom untuk points
            if (dataGridView1.Columns.Contains("Points"))
            {
                dataGridView1.Columns["Points"].HeaderText = "Points";
                dataGridView1.Columns["Points"].DisplayIndex = dataGridView1.ColumnCount - 2; // Atur posisi
            }

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
                    NotifyHelper.Error("Gagal load Member " + ex.Message);
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
                    using (Offline_MemberCustom editMemberForm = new(customMember, SelectedMember.member_id, SelectedMember.member_name, SelectedMember.member_phone_number, SelectedMember.member_email))
                    {
                        QuestionHelper bg = new(null, null, null, null);
                        Form background = bg.CreateOverlayForm();

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
                    NotifyHelper.Error("Gagal load Member " + ex.Message);
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
            NotifyHelper.Error("Tidak ada member yang dipilih!");
            Close();
        }
    }
}