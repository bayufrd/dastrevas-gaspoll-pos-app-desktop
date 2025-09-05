using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using FontAwesome.Sharp;
using Guna.UI2.WinForms;
using KASIR.Helper;
using KASIR.Model;
using KASIR.Properties;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KASIR.OfflineMode
{
    public partial class Dashboard : UserControl
    {
        private readonly List<ShiftReportData> shiftReports = new List<ShiftReportData>();

        public Dashboard()
        {
            InitializeComponent();
            this.Load += Dashboard_Load;
        }

        private async void Dashboard_Load(object sender, EventArgs e)
        {
            LoadShiftReports();
        }

        private async void LoadShiftReports()
        {
            string folderPath = @"DT-Cache\Transaction\ShiftReport";

            // Penanganan jika folder tidak ada
            if (!Directory.Exists(folderPath))
            {
                NotifyHelper.Warning("Folder shift report tidak ditemukan.");
                return;
            }

            // Ambil 10 file terbaru berdasarkan waktu modifikasi
            var files = Directory.GetFiles(folderPath, "*.data")
                                 .Select(f => new FileInfo(f))
                                 .OrderByDescending(f => f.LastWriteTime)
                                 .Take(14)
                                 .Select(f => f.FullName)
                                 .ToList();

            // Penanganan jika tidak ada file
            if (!files.Any())
            {
                NotifyHelper.Warning("Tidak ada file shift report yang ditemukan.");
                return;
            }

            try
            {
                var parsedFiles = files
                    .Select(File.ReadAllText)
                    .Select(JObject.Parse)
                    .ToList();

                shiftReports.Clear();
                shiftReports.AddRange(parsedFiles.Select(json => new ShiftReportData
                {
                    OutletName = json["data"]["outlet_name"]?.ToString(),
                    TotalTransaction = Convert.ToDecimal(json["data"]["total_Transaction"]),
                    SuccessQty = Convert.ToInt32(json["data"]["totalSuccessQty"]),
                    PendingQty = Convert.ToInt32(json["data"]["totalPendingQty"]),
                    CanceledQty = Convert.ToInt32(json["data"]["totalCanceledQty"]),
                    RefundQty = Convert.ToInt32(json["data"]["totalRefundQty"]),
                    StartDate = Convert.ToDateTime(json["data"]["start_date"]),
                    PaymentDetails = json["data"]["payment_details"]
                        .Select(p => new PaymentDetail
                        {
                            Category = p["payment_category"]?.ToString(),
                            TotalAmount = Convert.ToDecimal(p["total_amount"])
                        }).ToList()
                }));
                InitializeDashboard();
            }
            catch (Exception ex)
            {
                NotifyHelper.Error($"Error membaca file shift report: {ex.Message}");
            }
        }

        private async void InitializeDashboard()
        {
            try
            {
                string outletName = "unknown";
                string outletID = Settings.Default.BaseOutlet;
                string cacheOutlet = $"DT-Cache\\DataOutlet{outletID}.data";
                if (File.Exists(cacheOutlet))
                {
                    string cacheData = File.ReadAllText(cacheOutlet);
                    CartDataOutlet? dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheData);
                    if (dataOutlet != null && dataOutlet.data != null)
                    {
                        outletName = dataOutlet.data.name;
                    }
                }

                this.Controls.Clear();

                var mainPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(15),
                    ColumnCount = 2,
                    RowCount = 2
                };
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220)); // kiri stat card
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // kanan chart
                mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // header
                mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));        // isi

                // ===== HEADER =====
                var header = new Label
                {
                    Text = $"Dashboard Transaksi {outletName}",
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 90, 94),
                    Dock = DockStyle.Top,
                    Height = 60,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                mainPanel.Controls.Add(header, 0, 0);
                mainPanel.SetColumnSpan(header, 2);

                // ===== STAT CARDS (VERTIKAL) =====
                var statPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Padding = new Padding(5),
                    Margin = new Padding(5)
                };

                statPanel.Controls.Add(CreateStatCard("Total",
                    string.Format("Rp. {0:N0}", shiftReports.Sum(r => r.TotalTransaction)),
                    IconChar.MoneyBillWave));
                statPanel.Controls.Add(CreateStatCard("Sukses", shiftReports.Sum(r => r.SuccessQty).ToString(), IconChar.CheckCircle));
                statPanel.Controls.Add(CreateStatCard("Pending", shiftReports.Sum(r => r.PendingQty).ToString(), IconChar.HourglassHalf));
                statPanel.Controls.Add(CreateStatCard("Batal", shiftReports.Sum(r => r.CanceledQty).ToString(), IconChar.TimesCircle));
                statPanel.Controls.Add(CreateStatCard("Refund", shiftReports.Sum(r => r.RefundQty).ToString(), IconChar.UndoAlt));

                mainPanel.Controls.Add(statPanel, 0, 1);

                // ===== CHARTS AREA =====
                var chartPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    Padding = new Padding(10)
                };
                chartPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65)); // Line chart besar
                chartPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Pie & Bar kecil
                chartPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));       // line atas
                chartPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35));       // pie + bar bawah

                var lineChart = CreatePaymentPerDayLineChart(); // diperbesar
                var pieChart = CreatePaymentPieChart();         // diperkecil
                var barChart = CreateTransactionStatusBarChart(); // diperkecil

                chartPanel.Controls.Add(lineChart, 0, 0);
                chartPanel.SetRowSpan(lineChart, 2); // line chart ambil 2 baris (tinggi besar)
                chartPanel.Controls.Add(pieChart, 1, 0);
                chartPanel.Controls.Add(barChart, 1, 1);

                mainPanel.Controls.Add(chartPanel, 1, 1);

                this.Controls.Add(mainPanel);
            }
            catch(Exception ex)
            {
                NotifyHelper.Error($"Error membuat UI: {ex.Message}");
            }
        }

        private Guna2Panel CreateStatCard(string title, string value, IconChar icon)
        {
            var baseColor = Color.FromArgb(15, 90, 94);
            var panel = new Guna2Panel
            {
                Width = 180,
                Height = 90,
                BorderRadius = 10,
                FillColor = baseColor,
                Margin = new Padding(6),
                ShadowDecoration = { Enabled = true, Depth = 6 }
            };

            var iconBox = new IconPictureBox
            {
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 22,
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.Transparent,
                Padding = new Padding(6),
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblValue);
            panel.Controls.Add(iconBox);

            return panel;
        }

        private Control CreatePaymentPieChart()
        {
            var baseColor = Color.FromArgb(15, 90, 94);
            var panel = new Guna2Panel
            {
                Width = 260,
                Height = 200,
                BorderRadius = 10,
                FillColor = baseColor,
                Margin = new Padding(6),
                ShadowDecoration = { Enabled = true, Depth = 6 }
            };

            var chart = new PieChart
            {
                LegendLocation = LegendLocation.Right,
                Width = 250,
                Height = 190,
                InnerRadius = 50
            };

            var series = new SeriesCollection();
            var colors = new[]
            {
        System.Windows.Media.Color.FromRgb(255, 99, 132),
        System.Windows.Media.Color.FromRgb(54, 162, 235),
        System.Windows.Media.Color.FromRgb(255, 206, 86),
        System.Windows.Media.Color.FromRgb(75, 192, 192),
        System.Windows.Media.Color.FromRgb(153, 102, 255),
        System.Windows.Media.Color.FromRgb(255, 159, 64)
    };

            int colorIndex = 0;
            var grouped = shiftReports.SelectMany(x => x.PaymentDetails)
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(x => x.TotalAmount) })
                .ToList(); // Tambahkan .ToList()

            foreach (var item in grouped)
            {
                var color = colors[colorIndex % colors.Length];
                series.Add(new PieSeries
                {
                    Title = item.Category,
                    Values = new ChartValues<decimal> { item.Total },
                    DataLabels = true,
                    PushOut = 3,
                    Fill = new System.Windows.Media.SolidColorBrush(color),
                    LabelPoint = chartPoint => $"{chartPoint.Participation:P1}"
                });
                colorIndex++;
            }

            chart.Series = series;

            var elementHost = new ElementHost
            {
                Child = chart,
                Dock = DockStyle.Fill,
                Margin = new Padding(2)
            };

            panel.Controls.Add(elementHost);

            return panel;
        }

        private Control CreateTransactionStatusBarChart()
        {
            var chart = new CartesianChart
            {
                LegendLocation = LegendLocation.None,
                Width = 260,
                Height = 200
            };

            var values = new ChartValues<int>
            {
                shiftReports.Sum(r => r.SuccessQty),
                shiftReports.Sum(r => r.PendingQty),
                shiftReports.Sum(r => r.CanceledQty),
                shiftReports.Sum(r => r.RefundQty)
            };

            chart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "",
                    Values = values,
                    DataLabels = true,
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(54, 162, 235))
                }
            };

            chart.AxisX.Add(new Axis
            {
                Labels = new[] { "Sukses", "Pending", "Batal", "Refund" },
                FontSize = 10,
                Separator = new Separator { Step = 1 }
            });
            chart.AxisY.Add(new Axis
            {
                FontSize = 10,
                LabelFormatter = value => value.ToString()
        });

            return new ElementHost
            {
                Child = chart,
                Width = 260,
                Height = 200,
                Margin = new Padding(5)
            };
        }

        private Control CreatePaymentPerDayLineChart()
        {
            var chart = new CartesianChart
            {
                LegendLocation = LegendLocation.Bottom,
                Width = 700,
                Height = 400
            };

            var ordered = shiftReports.Take(10).ToList();
            var categories = ordered.SelectMany(r => r.PaymentDetails)
                                    .Select(p => p.Category)
                                    .Distinct()
                                    .ToList();

            var colorPalette = new[]
            {
                System.Windows.Media.Color.FromRgb(255, 99, 132),
                System.Windows.Media.Color.FromRgb(54, 162, 235),
                System.Windows.Media.Color.FromRgb(255, 206, 86),
                System.Windows.Media.Color.FromRgb(75, 192, 192),
                System.Windows.Media.Color.FromRgb(153, 102, 255),
                System.Windows.Media.Color.FromRgb(255, 159, 64)
            };

            var seriesCollection = new SeriesCollection();
            int colorIndex = 0;

            foreach (var cat in categories)
            {
                var vals = new ChartValues<decimal>();
                foreach (var rep in ordered)
                {
                    var pay = rep.PaymentDetails.FirstOrDefault(p => p.Category == cat);
                    vals.Add(pay?.TotalAmount ?? 0);
                }

                var color = colorPalette[colorIndex % colorPalette.Length];
                seriesCollection.Add(new LineSeries
                {
                    Title = cat,
                    Values = vals,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 6,
                    Stroke = new System.Windows.Media.SolidColorBrush(color),
                    Fill = System.Windows.Media.Brushes.Transparent,
                    DataLabels = false
                });
                colorIndex++;
            }

            var labels = ordered.Select(r => r.StartDate.ToString("dd MMM")).ToList();
            chart.Series = seriesCollection;
            chart.AxisX.Add(new Axis { Title = "Tanggal", Labels = labels, FontSize = 12 });
            chart.AxisY.Add(new Axis { Title = "Nominal", LabelFormatter = val => string.Format("Rp. {0:N0}", val), FontSize = 12 });

            return new ElementHost
            {
                Child = chart,
                Margin = new Padding(10),
                Dock = DockStyle.Fill
            };
        }
    }

    public class ShiftReportData
    {
        public string OutletName { get; set; }
        public decimal TotalTransaction { get; set; }
        public int SuccessQty { get; set; }
        public int PendingQty { get; set; }
        public int CanceledQty { get; set; }
        public int RefundQty { get; set; }
        public DateTime StartDate { get; set; }
        public List<PaymentDetail> PaymentDetails { get; set; }
    }

    public class PaymentDetail
    {
        public string Category { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
