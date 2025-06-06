

using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using KASIR.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace KASIR.OffineMode
{
    public partial class Offline_HistoryShift : Form
    {
        private readonly string baseOutlet;
        public ShiftData SelectedShift { get; private set; } // Property to hold the selected shift data

        public Offline_HistoryShift()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();

            Openform();
        }

        private async void Openform()
        {
            await LoadShiftDataButtons();
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            this.Close();
        }
        private async Task LoadShiftDataButtons()
        {
            string directoryPath = @"DT-Cache\Transaction\ShiftDataTransaction";

            // Clear the existing controls in the panel
            panelHistory.Controls.Clear();
            panelHistory.AutoScroll = true;

            // Create the FlowLayoutPanel
            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            panelHistory.Controls.Add(flowPanel);

            int totalWidth = panelHistory.ClientSize.Width;

            // Check if the directory exists
            if (Directory.Exists(directoryPath))
            {
                // Get all .data files in the directory
                string[] files = Directory.GetFiles(directoryPath, "*.data");

                foreach (var file in files)
                {
                    // Extract the file name without extension
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                    // Using regular expression to capture the date part from the filename
                    var regex = new Regex(@"History_.*_DT-\d+_(\d{8})");
                    var match = regex.Match(fileNameWithoutExtension);

                    if (match.Success)
                    {
                        // Extract the date (the part that is matched by the regular expression)
                        string datePart = match.Groups[1].Value; // This will be the date in yyyyMMdd format

                        DateTime fileDate;
                        if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                        {
                            // Create a button for this file
                            Button fileButton = new Button
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
                                var shiftDataList = await ReadShiftDataFromFile(file);

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
                MessageBox.Show("The specified directory does not exist.");
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
                EndAt = item["end_at"]?.ToString()
            }).ToList();

            return shiftDataList;
        }
        private void BindShiftDataToDataGridView(List<ShiftData> shiftDataList)
        {
            // Clear the existing controls in the panel
            panelHistory.Controls.Clear();

            // Create the DataGridView for displaying shift data
            DataGridView dataGridView = new DataGridView
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
                Name = "ShiftNumber",
                HeaderText = "Shift Number",
                DataPropertyName = "ShiftNumber",
                Width = 100
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CasherName",
                HeaderText = "Name",
                DataPropertyName = "CasherName",
                Width = 150
            });

            // Add a button column for "Pilih"
            DataGridViewButtonColumn pilihColumn = new DataGridViewButtonColumn
            {
                Name = "Pilih",
                HeaderText = "Pilih",
                Text = "Pilih",
                UseColumnTextForButtonValue = true
            };
            dataGridView.Columns.Add(pilihColumn);

            // Add a button column for "Print"
            DataGridViewButtonColumn printColumn = new DataGridViewButtonColumn
            {
                Name = "Print",
                HeaderText = "Print",
                Text = "Print",
                UseColumnTextForButtonValue = true
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
                    var selectedRow = dataGridView.Rows[e.RowIndex];

                    if (e.ColumnIndex == dataGridView.Columns["Pilih"].Index)
                    {
                        // Get the shift data directly from the shiftDataList (corresponding to the selected row)
                        ShiftData selectedShift = shiftDataList[e.RowIndex];

                        // Set the SelectedShift property to the selected ShiftData
                        SelectedShift = selectedShift;

                        // Close the form and pass back the data
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else if (e.ColumnIndex == dataGridView.Columns["Print"].Index)
                    {
                        // Handle the "Print" button click
                        MessageBox.Show($"Print button clicked for shift {shiftDataList[e.RowIndex].ShiftNumber}");
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                ForeColor = Color.Silver,
                SelectionBackColor = Color.Transparent,
                SelectionForeColor = Color.FromArgb(31, 30, 68)
            };
            dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;

            // General DataGridView settings
            dataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Column header style
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(31, 30, 68),
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
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle
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
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(31, 30, 68),
                SelectionBackColor = Color.Gainsboro,
                SelectionForeColor = Color.FromArgb(31, 30, 68),
                WrapMode = DataGridViewTriState.True
            };
            dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            // Row style
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(31, 30, 68),
                SelectionBackColor = Color.Gainsboro,
                SelectionForeColor = Color.FromArgb(31, 30, 68)
            };
            dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dataGridView.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            dataGridView.RowTemplate.DefaultCellStyle.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridView.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            dataGridView.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView.RowTemplate.Height = 40;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Final touches for DataGridView
            dataGridView.GridColor = Color.FromArgb(31, 30, 68);
            dataGridView.ImeMode = ImeMode.NoControl;
        }


    }
}
