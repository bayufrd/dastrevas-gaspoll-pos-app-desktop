using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using KASIR.Helper;
using KASIR.Model;
using KASIR.Properties;
using Newtonsoft.Json.Linq;

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
                // Create a button for the ShiftData.data file
                Button shiftDataButton = new()
                {
                    Text = "ShiftData (Latest)", // A generic name for the button
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
            // Clear the existing controls in the panel
            panelHistory.Controls.Clear();

            // Create the DataGridView for displaying shift data
            DataGridView dataGridView = new()
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false, // Don't allow the user to add new rows directly
                ReadOnly = true,
                Size = new Size(886, 500) // Set the size to be larger (you can adjust as needed)
            };

            // Add DataGridView columns (excluding the first column which is to be removed)
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Data",
                HeaderText = "Data",
                DataPropertyName = "StartAt", // Bind the StartAt property of ShiftData
                Width = 150
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ShiftNumber", HeaderText = "Shift Number", DataPropertyName = "ShiftNumber", Width = 100
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CasherName", HeaderText = "Name", DataPropertyName = "CasherName", Width = 150
            });

            // Add a button column for "Pilih"
            DataGridViewButtonColumn pilihColumn = new()
            {
                Name = "Pilih", HeaderText = "Pilih", Text = "Pilih", UseColumnTextForButtonValue = true
            };
            dataGridView.Columns.Add(pilihColumn);

            // Add a button column for "Print"
            DataGridViewButtonColumn printColumn = new()
            {
                Name = "Print", HeaderText = "Print", Text = "Print", UseColumnTextForButtonValue = true
            };
            dataGridView.Columns.Add(printColumn);

            // Bind the data to the DataGridView
            dataGridView.DataSource = shiftDataList;

            // Add the DataGridView to the panel
            panelHistory.Controls.Add(dataGridView);

            // Apply custom DataGridView styles
            ApplyDataGridViewStyles(dataGridView);

            // Handle button click for "Pilih" and "Print"
            dataGridView.CellContentClick += (sender, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow selectedRow = dataGridView.Rows[e.RowIndex];

                    if (e.ColumnIndex == dataGridView.Columns["Pilih"].Index)
                    {
                        // Get the shift data directly from the shiftDataList (corresponding to the selected row)
                        ShiftData selectedShift = shiftDataList[e.RowIndex];

                        // Set the SelectedShift property to the selected ShiftData
                        SelectedShift = selectedShift;

                        // Close the form and pass back the data
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else if (e.ColumnIndex == dataGridView.Columns["Print"].Index)
                    {
                        // Get the shift data directly from the shiftDataList (corresponding to the selected row)
                        ShiftData selectedShift = shiftDataList[e.RowIndex];

                        // Set the SelectedShift property to the selected ShiftData
                        SelectedShift = selectedShift;

                        // Close the form and pass back the data
                        DialogResult = DialogResult.Continue;
                        Close();
                    }
                }
            };
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