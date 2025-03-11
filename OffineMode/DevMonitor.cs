
using FontAwesome.Sharp;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Globalization;
using System.Text.RegularExpressions;
using static System.Windows.Forms.DataFormats;
using System.Windows.Media;
using Color = System.Drawing.Color;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using Label = System.Windows.Forms.Label;
using UserControl = System.Windows.Forms.UserControl;  // Gunakan ini untuk WinForms
using Panel = System.Windows.Forms.Panel;
using TextBox = System.Windows.Forms.TextBox;

namespace KASIR.OfflineMode
{
    public partial class DevMonitor : UserControl
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private ApiService apiService;
        private DataTable originalDataTable;
        private readonly string baseOutlet;
        //private inputPin pinForm;

        public DevMonitor()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            apiService = new ApiService();
            loadData();
        }
        private async void loadData()
        {
            // Panggil API untuk mendapatkan data
            IApiService apiService = new ApiService();
            string response = await apiService.GetActiveCart("/update-confirm");

            // Log response untuk debugging
            Console.WriteLine(response);

            if (string.IsNullOrEmpty(response) || response.StartsWith("<"))
            {
                MessageBox.Show("API response is not valid JSON.");
                return;
            }

            try
            {
                // Mengonversi response JSON menjadi objek JObject
                var jsonResponse = JObject.Parse(response);

                // Ambil data dari field "data"
                var jsonData = jsonResponse["data"];

                if (jsonData == null || !jsonData.HasValues)
                {
                    MessageBox.Show("No data found in API response.");
                    return;
                }

                panelConfirm.Controls.Clear();
                panelConfirm.AutoScroll = true; // Membuat panel scrollable

                int totalWidth = panelConfirm.ClientSize.Width;

                // Iterasi melalui setiap item dalam jsonData
                foreach (var outlet in jsonData)
                {
                    // Membuat panel untuk setiap outlet
                    Panel itemPanel = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 120, // Menambahkan sedikit tinggi untuk label
                    };

                    // Label untuk nama outlet
                    Label nameLabel = new Label
                    {
                        Text = outlet["outlet_name"]?.ToString(),
                        Width = (int)(totalWidth),
                        TextAlign = ContentAlignment.MiddleLeft,
                    };

                    // Label untuk versi
                    Label versionLabel = new Label
                    {
                        Text = "Version: " + outlet["version"]?.ToString(),
                        Width = (int)(totalWidth),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Top = 30,
                    };

                    // Label untuk versi baru
                    Label newVersionLabel = new Label
                    {
                        Text = "New Version: " + outlet["new_version"]?.ToString().Trim(),
                        Width = (int)(totalWidth),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Top = 50,
                    };

                    // Label untuk waktu pembaruan
                    Label lastUpdatedLabel = new Label
                    {
                        Text = "Last Updated: " + outlet["last_updated"]?.ToString(),
                        Width = (int)(totalWidth),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Top = 70,
                    };

                    // Menambahkan label dan panel ke itemPanel
                    itemPanel.Controls.Add(nameLabel);
                    itemPanel.Controls.Add(versionLabel);
                    itemPanel.Controls.Add(newVersionLabel);
                    itemPanel.Controls.Add(lastUpdatedLabel);

                    // Menambahkan itemPanel ke panelConfirm
                    panelConfirm.Controls.Add(itemPanel);
                }
                loadDataComplaint();
            }
            catch (JsonReaderException ex)
            {
                MessageBox.Show($"Error parsing JSON: {ex.Message}");
            }
        }

        private async void loadDataComplaint()
        {
            // Panggil API untuk mendapatkan data
            IApiService apiService = new ApiService();
            string response = await apiService.GetActiveCart("/complaint");

            // Log response untuk debugging
            Console.WriteLine(response);

            if (string.IsNullOrEmpty(response) || response.StartsWith("<"))
            {
                MessageBox.Show("API response is not valid JSON.");
                return;
            }

            try
            {
                // Mengonversi response JSON menjadi objek JObject
                var jsonResponse = JObject.Parse(response);

                // Ambil data dari field "data"
                var jsonData = jsonResponse["data"];

                if (jsonData == null || !jsonData.HasValues)
                {
                    MessageBox.Show("No data found in API response.");
                    return;
                }

                panelComplaint.Controls.Clear();
                panelComplaint.AutoScroll = true; // Membuat panel scrollable

                // Use FlowLayoutPanel for automatic control arrangement
                FlowLayoutPanel flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,  // Enables scrolling
                    FlowDirection = FlowDirection.TopDown,  // Items arranged vertically
                    WrapContents = false  // Prevent wrapping, making sure items stay in the same column
                };

                panelComplaint.Controls.Add(flowPanel);

                int totalWidth = panelComplaint.ClientSize.Width;

                // Iterasi melalui setiap item dalam jsonData
                foreach (var outlet in jsonData)
                {
                    // Membuat button untuk setiap outlet
                    System.Windows.Forms.Button outletNameButton = new System.Windows.Forms.Button
                    {
                        Text = outlet["name"]?.ToString() +" || " +outlet["sent_at"]?.ToString(),
                        Width = totalWidth,
                        Height = 40,
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    outletNameButton.Click += (sender, e) =>
                    {
                        // Open a new form with details when clicked
                        ShowOutletDetailsForm(outlet);
                    };

                    // Menambahkan button ke FlowLayoutPanel (flowPanel)
                    flowPanel.Controls.Add(outletNameButton);
                }
            }
            catch (JsonReaderException ex)
            {
                MessageBox.Show($"Error parsing JSON: {ex.Message}");
            }
        }
        private void ShowOutletDetailsForm(JToken outlet)
        {
            Form detailsForm = new Form
            {
                Text = outlet["name"]?.ToString(),
                Width = 600,  // Mengatur lebar form ke lebar layar
                Height = 600,  // Mengatur tinggi form ke tinggi layar
                StartPosition = FormStartPosition.CenterScreen,  // Menampilkan form di tengah layar
                MaximizeBox = false,  // Menonaktifkan tombol maximize untuk menghindari form lebih besar dari layar
                FormBorderStyle = FormBorderStyle.FixedDialog  // Mengatur border agar tidak bisa diubah-ubah ukuran
            };

            // Create labels and textboxes for detailed data
            int totalWidth = detailsForm.ClientSize.Width;

            Label titleLabel = new Label
            {
                Text = "Title: " + outlet["title"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 20
            };

            Label messageLabel = new Label
            {
                Text = "Message: " + outlet["message"]?.ToString().Trim(),
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 60
            };

            Label sentAtLabel = new Label
            {
                Text = "Sent at: " + outlet["sent_at"]?.ToString().Trim(),
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 100
            };

            // Labels and Textboxes for log_last_outlet and cache transaction fields
            Label log_last_outlet = new Label
            {
                Text = "Log Last Outlet:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 140
            };
            string jsonString = outlet["log_last_outlet"]?.ToString();

            // Pastikan tidak ada literal \r dan \n
            if (jsonString != null)
            {
                // Mengganti escape sequences (\\n, \\r) dengan string kosong atau newline yang benar
                jsonString = jsonString.Replace("\\n", Environment.NewLine); // Hapus literal \n
                jsonString = jsonString.Replace("\\r", ""); // Hapus literal \r
                jsonString = jsonString.Replace("\r", "");  // Hapus karakter \r yang tidak terlihat
                jsonString = jsonString.Replace("\n", Environment.NewLine);  // Ganti \n dengan newline sistem
            }
            TextBox logLastOutlet = new TextBox
            {
                Text = jsonString.ToString(), //outlet["log_last_outlet"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 160
            };

            Label cache_transaction = new Label
            {
                Text = "Cache Transaction:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 220
            };
            TextBox cacheTransaction = new TextBox
            {
                Text = outlet["cache_transaction"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 240
            };

            Label cache_failed_transaction = new Label
            {
                Text = "Cache Failed Transaction:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 300
            };
            TextBox cacheFailedTransaction = new TextBox
            {
                Text = outlet["cache_failed_transaction"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 320
            };

            Label cache_success_transaction = new Label
            {
                Text = "Cache Success Transaction:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 380
            };
            TextBox cacheSuccessTransaction = new TextBox
            {
                Text = outlet["cache_success_transaction"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 400
            };

            Label cache_history_transaction = new Label
            {
                Text = "Cache History Transaction:",
                Width = (int)(totalWidth * 0.9),
                TextAlign = ContentAlignment.MiddleLeft,
                Top = 460
            };
            TextBox cacheHistoryTransaction = new TextBox
            {
                Text = outlet["cache_history_transaction"]?.ToString(),
                Width = (int)(totalWidth * 0.9),
                Height = 50,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Top = 480
            };

            // Add controls to the details form
            detailsForm.Controls.Add(titleLabel);
            detailsForm.Controls.Add(messageLabel);
            detailsForm.Controls.Add(sentAtLabel);
            detailsForm.Controls.Add(log_last_outlet);
            detailsForm.Controls.Add(logLastOutlet);
            detailsForm.Controls.Add(cache_transaction);
            detailsForm.Controls.Add(cacheTransaction);
            detailsForm.Controls.Add(cache_failed_transaction);
            detailsForm.Controls.Add(cacheFailedTransaction);
            detailsForm.Controls.Add(cache_success_transaction);
            detailsForm.Controls.Add(cacheSuccessTransaction);
            detailsForm.Controls.Add(cache_history_transaction);
            detailsForm.Controls.Add(cacheHistoryTransaction);

            // Show the details form
            detailsForm.ShowDialog();
        }





    }
}
