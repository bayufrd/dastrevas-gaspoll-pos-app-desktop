
using FontAwesome.Sharp;
using KASIR.komponen;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Windows.Forms;



namespace KASIR.komponen
{
    public partial class detailMenuForm : Form
    {
        private List<string> namaVarian = new List<string>();
        private List<string> hargaVarian = new List<string>();
        public string idmenu;
        private List<Panel> dynamicGroups = new List<Panel>();
        public bool ReloadDataInBaseForm { get; private set; }
        private readonly ILogger _log = LoggerService.Instance._log;
        public detailMenuForm(string id)
        {
            InitializeComponent();
            LoadTipe();

            flowVarian.FlowDirection = FlowDirection.TopDown;
            flowVarian.WrapContents = false;
            flowVarian.AutoScroll = true;
            cmbTipe.Items.Add("Silahkan pilih tipe menu");
            cmbTipe.SelectedIndex = 2;
            cmbTipe.ForeColor = Color.Gray;
            idmenu = id;
           // LoadDataAsync();
        }

        private void LoadTipe()
        {
            string[] tipe = { "Makanan", "Minuman" };

            cmbTipe.Items.AddRange(tipe);
        }

        private async void LoadDataAsync()
        {
          /*  IApiService apiService = new ApiService();
            string response = await apiService.GetMenuByID("/menu", idmenu);

            GetMenuByIdModel menuModel = JsonConvert.DeserializeObject<GetMenuByIdModel>(response);
            DataMenu data = menuModel.data;
            if (data.menu_type.Equals("Makanan"))
            {
                cmbTipe.SelectedIndex = 0;
            }
            else
            {
                cmbTipe.SelectedIndex = 1;
            }
            txtNama.Text = data.name;
            txtHarga.Text = data.price.ToString();

            for (int i = 0; i < data.menu_details.Count; i++)
            {
                Panel newGroup = new Panel
                {
                    Width = 560, 
                    Height = 90  
                };

                TextBox textBox1 = new TextBox { Width = 560, PlaceholderText = "Nama Varian ke " + (flowVarian.Controls.Count + 1) };
                TextBox textBox2 = new TextBox { Width = 560, Top = textBox1.Height + 5, PlaceholderText = "Harga Varian ke " + (flowVarian.Controls.Count + 1) };
                Button buttonRemove = new Button { Width = 560, Top = textBox1.Height + 35, Text = "Batal" };
                buttonRemove.Click += (s, ev) => RemoveGroup(newGroup);
                newGroup.Controls.Add(textBox1);
                newGroup.Controls.Add(textBox2);
                newGroup.Controls.Add(buttonRemove);
                dynamicGroups.Add(newGroup);

                textBox1.Text = data.menu_details[i].varian;
                textBox2.Text = data.menu_details[i].price.ToString();
                flowVarian.Controls.Add(newGroup);
            }*/

        }
        private void RemoveGroup(Panel newGroup)
        {
            flowVarian.Controls.Remove(newGroup);
            dynamicGroups.Remove(newGroup);
        }
        private void btnTambah_Click_1(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnTambah_Click_1));

            Panel newGroup = new Panel
            {
                Width = 560, 
                Height = 60  
            };

            TextBox textBox1 = new TextBox { Width = 560, PlaceholderText = "Nama Varian ke " + (flowVarian.Controls.Count + 1) };
            TextBox textBox2 = new TextBox { Width = 560, Top = textBox1.Height + 5, PlaceholderText = "Harga Varian ke " + (flowVarian.Controls.Count + 1) };

            newGroup.Controls.Add(textBox1);
            newGroup.Controls.Add(textBox2);

            flowVarian.Controls.Add(newGroup);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void button4_Click_1(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(button4_Click_1));

            namaVarian.Clear();
            hargaVarian.Clear();

            bool anyEmptyTextBox = false;

            foreach (Control group in flowVarian.Controls)
            {
                if (group is Panel panel && panel.Controls.Count >= 2)
                {
                    if (panel.Controls[0] is TextBox textBox1)
                    {
                        if (string.IsNullOrWhiteSpace(textBox1.Text))
                        {
                            anyEmptyTextBox = true;
                            textBox1.BackColor = System.Drawing.Color.Red;
                        }
                        else
                        {
                            namaVarian.Add(textBox1.Text);
                            textBox1.BackColor = System.Drawing.SystemColors.Window;
                        }
                    }

                    if (panel.Controls[1] is TextBox textBox2)
                    {
                        if (string.IsNullOrWhiteSpace(textBox2.Text))
                        {
                            anyEmptyTextBox = true;
                            textBox2.BackColor = System.Drawing.Color.Red;
                        }
                        else
                        {
                            hargaVarian.Add(textBox2.Text);
                            textBox2.BackColor = System.Drawing.SystemColors.Window;
                        }
                    }
                }
            }

            if (anyEmptyTextBox)
            {
                MessageBox.Show("Please fill all textboxes before proceeding.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Please fill all textboxes before proceeding.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtHarga.Text))
            {
                MessageBox.Show("Please fill all textboxes before proceeding.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbTipe.SelectedIndex == 2)
            {
                MessageBox.Show("Please select a menu type before proceeding.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<Dictionary<string, object>> menuDetailsList = new List<Dictionary<string, object>>();

            for (int i = 0; i < namaVarian.Count; i++)
            {
                string varian = namaVarian[i];
                string price = hargaVarian[i];

                Dictionary<string, object> varianPricePair = new Dictionary<string, object>
                {
                    { "varian", varian },
                    { "price", price }
                };

                menuDetailsList.Add(varianPricePair);
            }

            var json = new
            {
                name = txtNama.Text,
                menu_type = cmbTipe.Text,
                price = txtHarga.Text,
                menu_details = menuDetailsList
            };
            string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
            IApiService apiService = new ApiService();
            HttpResponseMessage response = await apiService.UpdateMenu("/menu", idmenu, jsonString);
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    DialogResult result = MessageBox.Show("Data berhasil disimpan" + response.StatusCode, "Confirmation", MessageBoxButtons.OK);

                    if (result == DialogResult.OK)
                    {
                        ReloadDataInBaseForm = true;
                    }
                    this.DialogResult = result;
                }
                else
                {
                    MessageBox.Show("Data gagal diperbaharui " + response.StatusCode);
                }
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowVarian_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void btnHapus_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(btnHapus_Click));

            IApiService apiService = new ApiService();
            HttpResponseMessage response = await apiService.DeleteMenu("/menu", idmenu);
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    DialogResult result = MessageBox.Show("Data berhasil dihapus" + response.StatusCode, "Confirmation", MessageBoxButtons.OK);

                    if (result == DialogResult.OK)
                    {
                        ReloadDataInBaseForm = true;
                    }
                    this.DialogResult = result;
                }
                else
                {
                    MessageBox.Show("Data gagal dihapus " + response.StatusCode);
                    _log.Error("data gagal dihapus");
                }
            }

        }

        private void txtNama_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
