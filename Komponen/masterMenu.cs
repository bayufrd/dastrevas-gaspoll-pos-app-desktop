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
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
namespace KASIR.komponen
{
    public partial class masterMenu : UserControl
    {
        private ApiService apiService;
        private DataTable originalDataTable;
        private createMenuForm uu;
        private readonly ILogger _log = LoggerService.Instance._log;
        public masterMenu()
        {
            InitializeComponent();
            apiService = new ApiService();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                string response = await apiService.Get("/menu");

                GetMenuModel menuModel = JsonConvert.DeserializeObject<GetMenuModel>(response);
                List<Menu> menuList = menuModel.data.ToList();
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(int));
                dataTable.Columns.Add("Nama", typeof(string));
                dataTable.Columns.Add("Tipe", typeof(string));
                dataTable.Columns.Add("Harga", typeof(string));
                foreach (Menu menu in menuList)
                {
                    dataTable.Rows.Add(menu.id, menu.name, menu.menu_type, "Rp " + menu.price);
                }

                dataGridView1.DataSource = dataTable;
                originalDataTable = dataTable.Copy();
                dataGridView1.Columns["ID"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while retrieving data: " + ex.Message);

                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);

            }
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
           
             
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(textBox1_TextChanged));

            PerformSearch();
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

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                int id = Convert.ToInt32(selectedRow.Cells["ID"].Value);

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

                using (detailMenuForm detailForm = new detailMenuForm(id.ToString()))
                {
                    detailForm.Owner = background;

                    background.Show();

                    DialogResult dialogResult = detailForm.ShowDialog();

                    background.Dispose();

                    if (dialogResult == DialogResult.Yes && detailForm.ReloadDataInBaseForm)
                    {
                        ReloadData();
                    }
                }
            }
        }

        private void ReloadData()
        {
            LoadData();
        }

        private void masterMenu_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
    }
}
