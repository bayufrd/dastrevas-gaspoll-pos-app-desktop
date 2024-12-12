
using FontAwesome.Sharp;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Microsoft.VisualBasic.ApplicationServices;
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
namespace KASIR.Komponen
{
    public partial class SettingsApp : UserControl
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private ApiService apiService;
        private DataTable originalDataTable;
        private readonly string baseOutlet;
        private inputPin pinForm;

        public SettingsApp()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            apiService = new ApiService();

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
        private void OpenRefundForm(string transaksiId)
        {
            refund refundForm = new refund(transaksiId);
            refundForm.RefundSuccessful += OnRefundSuccess;
            if (pinForm != null && !pinForm.IsDisposed)
            {
                pinForm.Close();
            }
            refundForm.ShowDialog();
        }

        private void OnRefundSuccess(object sender, EventArgs e)
        {
            // Refresh data in successTransaction form
            LoadData();
        }
        public async void LoadData()
        {
            try
            {
                string response = await apiService.Get("/transaction?outlet_id=" + baseOutlet + "&is_success=true");

                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                List<Menu> menuList = menuModel.data.ToList();

                // Begin Counting Transaction Queue
                int numberQueue = menuList.Count + 1; // Start queue number

                // End Counting
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(int));
                dataTable.Columns.Add("Receipt Number", typeof(string));
                dataTable.Columns.Add("ID Outlet", typeof(int));
                dataTable.Columns.Add("ID Cart", typeof(int));
                dataTable.Columns.Add("Customer Name", typeof(string));
                dataTable.Columns.Add("Customer Seat", typeof(string));
                dataTable.Columns.Add("Total Cart", typeof(string));
                dataTable.Columns.Add("Payment Type", typeof(string));
                dataTable.Columns.Add("Transaction Time", typeof(string));
                dataTable.Columns.Add("NumberQueue", typeof(int)); // Add this line
                string format = "dddd, dd MMMM yyyy - HH:mm";

                foreach (Menu menu in menuList)
                {
                    numberQueue -= 1; // Decrease number for the next entry
                    float total = menu.customer_cash - menu.customer_change;
                    DateTime updatedAt;
                    // Parsing tanggal menggunakan format yang sesuai
                    if (DateTime.TryParseExact(menu.invoice_due_date, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out updatedAt))
                    {
                        // Format ulang menjadi lebih singkat, misalnya "19 Oct 2024, 10:38"
                        string formattedDate = updatedAt.ToString("dd MMM yyyy, HH:mm");
                        dataTable.Rows.Add(menu.id, numberQueue + ". " + menu.receipt_number, menu.outlet_id, menu.cart_id, menu.customer_name, menu.customer_seat ?? "0", string.Format("Rp. {0:n0},-", total), menu.payment_type, formattedDate, numberQueue); // Add the queue number
                    }
                    else
                    {
                        // Jika parsing gagal, tampilkan seperti apa adanya
                        dataTable.Rows.Add(menu.id, numberQueue + ". " + menu.receipt_number, menu.outlet_id, menu.cart_id, menu.customer_name, menu.customer_seat ?? "0", string.Format("Rp. {0:n0},-", total), menu.payment_type, updatedAt, numberQueue); // Add the queue number
                    }
                }

                dataGridView1.DataSource = dataTable;
                originalDataTable = dataTable.Copy();
                dataGridView1.Columns["ID"].Visible = false;
                dataGridView1.Columns["ID Outlet"].Visible = false;
                dataGridView1.Columns["ID Cart"].Visible = false;
                dataGridView1.Columns["NumberQueue"].Visible = false; // Optionally hide if not needed
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while retrieving data: " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }

        private void PerformSearch()
        {
            if (originalDataTable == null)
                return;

            string searchTerm = textBox1.Text.ToLower();

            DataTable filteredDataTable = originalDataTable.Clone();

            IEnumerable<DataRow> filteredRows = originalDataTable.AsEnumerable()
                .Where(row => row.ItemArray.Any(field => field.ToString().ToLower().Contains(searchTerm)));

            foreach (DataRow row in filteredRows)
            {
                filteredDataTable.ImportRow(row);
            }
            dataGridView1.DataSource = filteredDataTable;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                int id = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                int urutanRiwayat = Convert.ToInt32(selectedRow.Cells["NumberQueue"].Value); // Access the NumberQueue here

                LoadPin(id, urutanRiwayat);
                //  OpenRefundForm(id.ToString());
                /* using (refund refund = new refund(id.ToString()))
                 {
                     refund.Owner = background;

                     background.Show();

                     DialogResult dialogResult = refund.ShowDialog();

                     background.Dispose();

                     if (dialogResult == DialogResult.Yes && refund.ReloadDataInBaseForm)
                     {
                         ReloadData();
                     }
                 }*/
            }
        }

        private void LoadPin(int id, int urutanRiwayat)
        {
            Form background = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = this.Location,
                ShowInTaskbar = false,
            };
            using (inputPin pinForm = new inputPin(id, urutanRiwayat))
            {
                pinForm.Owner = background;

                background.Show();

                DialogResult dialogResult = pinForm.ShowDialog();

                background.Dispose();


            }
        }

        private void ReloadData()
        {
            LoadData();
        }

        private void btnReportShift_Click(object sender, EventArgs e)
        {
            //LoggerUtil.LogPrivateMethod(nameof(btnReportShift_Click));

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
            this.Invoke((MethodInvoker)delegate
            {
                using (printReportShift payForm = new printReportShift())
                {
                    payForm.Owner = background;

                    background.Show();

                    DialogResult dialogResult = payForm.ShowDialog();

                    background.Dispose();

                    /*if (printReportShift.KeluarButtonPrintReportShiftClicked)
                    {
                        LoadData(); 
                    }
    */
                    if (dialogResult == DialogResult.OK && payForm.ReloadDataInBaseForm)
                    {
                        LoadData();

                    }
                }
            });
        }

        private void btnNotifikasiPengeluaran_Click(object sender, EventArgs e)
        {
            //LoggerUtil.LogPrivateMethod(nameof(btnNotifikasiPengeluaran_Click));

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

            using (notifikasiPengeluaran notifikasiPengeluaran = new notifikasiPengeluaran())
            {
                notifikasiPengeluaran.Owner = background;

                background.Show();

                DialogResult dialogResult = notifikasiPengeluaran.ShowDialog();

                background.Dispose();

                /*if (printReportShift.KeluarButtonPrintReportShiftClicked)
                {
                    LoadData(); 
                }
*/
                if (dialogResult == DialogResult.OK && notifikasiPengeluaran.ReloadDataInBaseForm)
                {
                    LoadData();
                }
            }
        }
    }
}
