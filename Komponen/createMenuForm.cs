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
using System.Windows.Forms;



namespace KASIR.komponen
{
    public partial class createMenuForm : Form
    {
        private List<string> namaVarian = new List<string>();
        private List<string> hargaVarian = new List<string>();
        private List<Panel> dynamicGroups = new List<Panel>();

        private const int PaddingWidth = 100;
        private const int PaddingHeight = 100;
        public bool ReloadDataInBaseForm { get; private set; }
        public createMenuForm()
        {
            InitializeComponent();
            LoadTipe();
            flowVarian.FlowDirection = FlowDirection.TopDown;
            flowVarian.WrapContents = false;
            flowVarian.AutoScroll = true;
            cmbTipe.Items.Add("Silahkan pilih tipe menu");
            cmbTipe.SelectedIndex = 2;
            cmbTipe.ForeColor = Color.Gray;
            StartPosition = FormStartPosition.CenterParent;


            // Set the initial form size


            int newHeight = Screen.PrimaryScreen.WorkingArea.Height - 100;

            //int availableHeight = Screen.PrimaryScreen.WorkingArea.Height;
            //int maxHeight = availableHeight - (2 * PaddingHeight);
            //int newHeight = Math.Min(maxHeight, Height);
            //Size = new Size(Width, newHeight);

            Height = newHeight;
            //Point mainFormLocation = Owner.Location;
            //Location = new Point(mainFormLocation.X, mainFormLocation.Y + PaddingHeight);
        }

        private void LoadTipe()
        {
            string[] tipe = { "Makanan", "Minuman" };

            cmbTipe.Items.AddRange(tipe);
        }


        private void btnTambah_Click_1(object sender, EventArgs e)
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
            flowVarian.Controls.Add(newGroup);
        }

        private void RemoveGroup(Panel newGroup)
        {
            flowVarian.Controls.Remove(newGroup);

            dynamicGroups.Remove(newGroup);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void button4_Click_1(object sender, EventArgs e)
        {
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

            HttpResponseMessage response = await apiService.PostAddMenu(jsonString, "/menu");

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
                    MessageBox.Show("Data gagal disimpan  " + response.StatusCode);
                }
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowVarian_Paint(object sender, PaintEventArgs e)
        {

        }

        private void createMenuForm_Resize(object sender, EventArgs e)
        {
            AdjustFormSize();
        }

        private void AdjustFormSize()
        {
            //int newHeight = Screen.GetBounds(Point.Empty).Height - (5 * PaddingHeight);
            //int newHeight = Screen.PrimaryScreen.WorkingArea.Height - (5 * PaddingHeight);
            // Update the form size
            //Size = new Size(Width, newHeight);
        }

        private void cmbTipe_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
