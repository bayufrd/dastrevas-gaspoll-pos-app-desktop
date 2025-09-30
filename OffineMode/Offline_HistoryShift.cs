using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Guna.UI2.WinForms.Enums;
using Guna.UI2.WinForms;
using KASIR.Helper;
using KASIR.Model;
using KASIR.Properties;
using Newtonsoft.Json.Linq;
using FontAwesome.Sharp;
using System.Drawing.Drawing2D;

namespace KASIR.OffineMode
{
    public partial class Offline_HistoryShift : Form
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

        private readonly string baseOutlet;

        public Offline_HistoryShift()
        {
            baseOutlet = Settings.Default.BaseOutlet;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));


            Openform();
        }

        public ShiftData SelectedShift { get; private set; }

        private async void Openform()
        {
            await LoadShiftDataButtons();
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        private async Task LoadShiftDataButtons()
        {
            try
            {
                string directoryPath = @"DT-Cache\Transaction\ShiftDataTransaction";
                string shiftDataPath = @"DT-Cache\Transaction\ShiftData.data";

                // Bersihkan panel
                panelHistory.Controls.Clear();

                // 🔹 FlowLayoutPanel untuk card
                FlowLayoutPanel flowPanel = new()
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Padding = new Padding(15),   // jarak dalam
                };
                // 🔎 TextBox Search (dibungkus biar sejajar dengan card)
                Panel searchWrapper = new()
                {
                    Width = panelHistory.ClientSize.Width - 40, // sama dengan cardWidth
                    Height = 50,
                    //Margin = new Padding(0, 0, 0, 12),
                    Location = new Point(0, 0),  // Tetap di kiri atas flowPanel
                    BackColor = panelHistory.BackColor // transparan/ikut panel
                };
                TextBox txtSearch = new()
                {
                    PlaceholderText = "🔍 Cari berdasarkan tanggal/hari...",
                    Font = new Font("Segoe UI", 11F),
                    Width = searchWrapper.Width,
                    Height = 35,
                    Location = new Point(20, 7), 
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // masukkan txtSearch ke dalam wrapper
                searchWrapper.Controls.Add(txtSearch);

                // tambahkan ke flowPanel, bukan ke panelHistory langsung
                flowPanel.Controls.Add(searchWrapper);


                // Tambahkan search + flowPanel ke panelHistory
                panelHistory.Controls.Add(flowPanel);
                panelHistory.Controls.Add(txtSearch);
                panelHistory.Controls.SetChildIndex(txtSearch, 0);

                int totalWidth = panelHistory.ClientSize.Width - 40;

                // 🔹 Fungsi bantu bikin card
                Panel CreateCard(string title, string filePath)
                {
                    Panel card = new()
                    {
                        Width = totalWidth,
                        Height = 70,
                        Margin = new Padding(5),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = title // penting untuk filter
                    };

                    Label lbl = new()
                    {
                        Text = title,
                        Dock = DockStyle.Left,
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Width = card.Width - 120,
                        Padding = new Padding(10, 20, 0, 0),
                        ForeColor = Color.Black
                    };

                    IconButton btnOpen = new()
                    {
                        Text = " Open",
                        IconChar = IconChar.FolderOpen,
                        IconColor = Color.DodgerBlue,
                        IconSize = 20,
                        TextImageRelation = TextImageRelation.ImageBeforeText,
                        FlatStyle = FlatStyle.Flat,
                        Width = 90,
                        Height = 30,
                        Location = new Point(card.Width - 100, 20),
                        BackColor = Color.Gainsboro,
                        ForeColor = Color.Black
                    };
                    btnOpen.FlatAppearance.BorderSize = 0;

                    btnOpen.Click += async (s, e) =>
                    {
                        List<ShiftData> shiftDataList = await ReadShiftDataFromFile(filePath);
                        BindShiftDataToDataGridView(shiftDataList);
                    };

                    card.Controls.Add(lbl);
                    card.Controls.Add(btnOpen);

                    return card;
                }

                // 🔹 Card untuk ShiftData.data (terakhir)
                if (File.Exists(shiftDataPath))
                {
                    string shiftNow = DateTime.Now.ToString("yyyy-MM-dd");
                    string[] hariIndonesia = { "Minggu", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu" };
                    string namaHari = hariIndonesia[(int)DateTime.Now.DayOfWeek];

                    var card = CreateCard($"{shiftNow} {namaHari} | ShiftData (Terakhir)", shiftDataPath);
                    flowPanel.Controls.Add(card);
                }

                // 🔹 Card untuk history lain
                if (Directory.Exists(directoryPath))
                {
                    string[] files = Directory.GetFiles(directoryPath, "*.data");

                    foreach (string file in files)
                    {
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                        Regex regex = new(@"History_.*_DT-\d+_(\d{8})");
                        Match match = regex.Match(fileNameWithoutExtension);

                        if (match.Success)
                        {
                            string datePart = match.Groups[1].Value;

                            if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out DateTime fileDate))
                            {
                                string[] hariIndonesia = { "Minggu", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu" };
                                string namaHari = hariIndonesia[(int)fileDate.DayOfWeek];

                                var card = CreateCard(fileDate.ToString("yyyy-MM-dd") + " " + namaHari, file);
                                flowPanel.Controls.Add(card);
                            }
                        }
                    }
                }

                // 🔎 Event untuk filter pencarian
                txtSearch.TextChanged += (s, e) =>
                {
                    string keyword = txtSearch.Text.Trim().ToLower();

                    foreach (Control ctrl in flowPanel.Controls)
                    {
                        if (ctrl is Panel card && card.Tag is string title)
                        {
                            card.Visible = string.IsNullOrEmpty(keyword) ||
                                           title.ToLower().Contains(keyword);
                        }
                    }
                };
            }
            catch(Exception ex)
            {
                LoggerUtil.LogError(ex, $"Error: {ex.Message}",ex);
            }
        }

        private async Task exLoadShiftDataButtons()
        {
            string directoryPath = @"DT-Cache\Transaction\ShiftDataTransaction";
            string shiftDataPath = @"DT-Cache\Transaction\ShiftData.data"; // Path for ShiftData.data file

            // Clear the existing controls in the panel
            panelHistory.Controls.Clear();
            panelHistory.AutoScroll = true;

            // Create the FlowLayoutPanel
            FlowLayoutPanel flowPanel = new()
            {
                Dock = DockStyle.Top,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Height = panelHistory.ClientSize.Width,
                Width = panelHistory.ClientSize.Height
            };

            panelHistory.Controls.Add(flowPanel);

            int totalWidth = panelHistory.ClientSize.Width;

            // Check if the ShiftData.data file exists first
            if (File.Exists(shiftDataPath))
            {
                string shiftNow = DateTime.Now.ToString("yyyy-MM-dd");
                // Create a button for the ShiftData.data file
                Button shiftDataButton = new()
                {
                    Text = $"{shiftNow} | ShiftData (Terakhir)", // A generic name for the button
                    Width = totalWidth * 98 / 100,
                    ForeColor = Color.Black,
                    Height = 40,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                shiftDataButton.Click += async (sender, e) =>
                {
                    // Deserialize the ShiftData.data file
                    List<ShiftData> shiftDataList = await ReadShiftDataFromFile(shiftDataPath);

                    // Bind the shift data to the DataGridView
                    BindShiftDataToDataGridView(shiftDataList);
                };

                // Add the button for ShiftData.data to the FlowLayoutPanel
                flowPanel.Controls.Add(shiftDataButton);
            }
            else
            {
                NotifyHelper.Warning("The ShiftData.data file does not exist.");
            }

            // Next, check if the directory exists for other .data files
            if (Directory.Exists(directoryPath))
            {
                // Get all .data files in the directory
                string[] files = Directory.GetFiles(directoryPath, "*.data");

                foreach (string file in files)
                {
                    // Extract the file name without extension
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                    // Using regular expression to capture the date part from the filename
                    Regex regex = new(@"History_.*_DT-\d+_(\d{8})");
                    Match match = regex.Match(fileNameWithoutExtension);

                    if (match.Success)
                    {
                        // Extract the date (the part that is matched by the regular expression)
                        string datePart = match.Groups[1].Value; // This will be the date in yyyyMMdd format

                        DateTime fileDate;
                        if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out fileDate))
                        {
                            // Create a button for this file
                            Button fileButton = new()
                            {
                                Text = fileDate.ToString("yyyy-MM-dd"), // Display only the date
                                Width = totalWidth * 98 / 100,
                                ForeColor = Color.Black,
                                Height = 40,
                                FlatStyle = FlatStyle.Flat,
                                TextAlign = ContentAlignment.MiddleLeft
                            };

                            fileButton.Click += async (sender, e) =>
                            {
                                // Deserialize the .data file to ShiftData objects
                                List<ShiftData> shiftDataList = await ReadShiftDataFromFile(file);

                                // Bind the shift data to the DataGridView
                                BindShiftDataToDataGridView(shiftDataList);
                            };

                            // Add the button to the FlowLayoutPanel
                            flowPanel.Controls.Add(fileButton);
                        }
                    }
                }
            }
            else
            {
                // If the directory does not exist, create it
                Directory.CreateDirectory(directoryPath);
            }
        }


        private async Task<List<ShiftData>> ReadShiftDataFromFile(string filePath)
        {
            // Read the JSON file content
            string jsonContent = await File.ReadAllTextAsync(filePath);

            // Deserialize the JSON content into a list of ShiftData objects
            JObject jsonData = JObject.Parse(jsonContent);
            JArray dataArray = (JArray)jsonData["data"];

            List<ShiftData> shiftDataList = dataArray.Select(item => new ShiftData
            {
                CasherName = item["casher_name"]?.ToString(),
                ShiftNumber = (int)item["shift_number"],
                ActualEndingCash = decimal.Parse(item["actual_ending_cash"]?.ToString() ?? "0"),
                StartAt = item["start_at"]?.ToString(),
                EndAt = item["end_at"]?.ToString(),
                id = item["id"]?.ToString()
            }).ToList();

            return shiftDataList;
        }

        private void BindShiftDataToDataGridView(List<ShiftData> shiftDataList)
        {
            // Bersihkan panel terlebih dahulu
            panelHistory.Controls.Clear();

            // Buat FlowLayoutPanel
            FlowLayoutPanel flowPanel = new()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            panelHistory.Controls.Add(flowPanel);

            foreach (var shift in shiftDataList)
            {
                // Panel container untuk setiap shift
                Panel card = new()
                {
                    Width = panelHistory.ClientSize.Width - 25,
                    Height = 80,
                    Margin = new Padding(5),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Label tanggal / shift info
                Label lblInfo = new()
                {
                    Text = $"Shift {shift.ShiftNumber} | {shift.CasherName}\n" +
                           $"Start: {shift.StartAt} | End: {shift.EndAt}\n" +
                           $"Actual Ending Cash : {shift.ActualEndingCash}\n" +
                           $"id : #{shift.id}",
                    AutoSize = false,
                    Dock = DockStyle.Left,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = Color.Black,
                    Width = card.Width - 180
                };

                // Tombol pilih dengan icon FontAwesome
                IconButton btnPilih = new()
                {
                    Text = " Pilih",
                    IconChar = IconChar.CheckCircle,
                    IconColor = Color.Green,
                    IconSize = 20,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.LightGray,
                    ForeColor = Color.Black,
                    Width = 80,
                    Height = 30,
                    Location = new Point(card.Width - 170, 25)
                };
                btnPilih.FlatAppearance.BorderSize = 0;

                btnPilih.Click += (s, e) =>
                {
                    SelectedShift = shift;
                    DialogResult = DialogResult.OK;
                    Close();
                };

                // Tombol print dengan icon FontAwesome
                IconButton btnPrint = new()
                {
                    Text = " Print",
                    IconChar = IconChar.Print,
                    IconColor = Color.DodgerBlue,
                    IconSize = 20,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.LightGray,
                    ForeColor = Color.Black,
                    Width = 80,
                    Height = 30,
                    Location = new Point(card.Width - 85, 25)
                };
                btnPrint.FlatAppearance.BorderSize = 0;

                btnPrint.Click += (s, e) =>
                {
                    SelectedShift = shift;
                    DialogResult = DialogResult.Continue; // sesuai logic Anda
                    Close();
                };

                // Tambahkan ke panel card
                card.Controls.Add(lblInfo);
                card.Controls.Add(btnPilih);
                card.Controls.Add(btnPrint);

                // Tambahkan card ke flow panel
                flowPanel.Controls.Add(card);
            }
        }


        private void ApplyDataGridViewStyles(DataGridView dataGridView)
        {
            // Disable cell editing, resizing, and adding/deleting rows
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AllowUserToResizeColumns = false;
            dataGridView.AllowUserToResizeRows = false;

            // Set the default style for the cells
            DataGridViewCellStyle dataGridViewCellStyle1 = new()
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                ForeColor = Color.Silver,
                SelectionBackColor = Color.Transparent,
                SelectionForeColor = Color.FromArgb(15, 90, 94)
            };
            dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;

            // General DataGridView settings
            dataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Column header style
            DataGridViewCellStyle dataGridViewCellStyle2 = new()
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(15, 90, 94),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.White,
                SelectionBackColor = Color.Transparent,
                SelectionForeColor = Color.Transparent,
                WrapMode = DataGridViewTriState.True
            };
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView.ColumnHeadersHeight = 30;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.ColumnHeadersVisible = true; // Keep column headers visible

            // Cell style
            DataGridViewCellStyle dataGridViewCellStyle3 = new()
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.White,
                SelectionBackColor = Color.Gainsboro,
                SelectionForeColor = Color.Gainsboro,
                WrapMode = DataGridViewTriState.False
            };
            dataGridView.DefaultCellStyle = dataGridViewCellStyle3;

            // Row headers style
            DataGridViewCellStyle dataGridViewCellStyle4 = new()
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(15, 90, 94),
                SelectionBackColor = Color.Gainsboro,
                SelectionForeColor = Color.FromArgb(15, 90, 94),
                WrapMode = DataGridViewTriState.True
            };
            dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            // Row style
            DataGridViewCellStyle dataGridViewCellStyle5 = new()
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 90, 94),
                SelectionBackColor = Color.Gainsboro,
                SelectionForeColor = Color.FromArgb(15, 90, 94)
            };
            dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dataGridView.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            dataGridView.RowTemplate.DefaultCellStyle.ForeColor = Color.FromArgb(15, 90, 94);
            dataGridView.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            dataGridView.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 90, 94);
            dataGridView.RowTemplate.Height = 40;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Final touches for DataGridView
            dataGridView.GridColor = Color.FromArgb(15, 90, 94);
            dataGridView.ImeMode = ImeMode.NoControl;
        }
    }
}