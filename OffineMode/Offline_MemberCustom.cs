﻿using KASIR.Network;
using KASIR.Properties;
using Newtonsoft.Json;
using Serilog;

namespace KASIR.komponen
{
    public partial class Offline_MemberCustom : Form
    {
         
        private readonly string baseOutlet;
        public string btnPayType;
        private readonly int idid;
        private readonly string Options;
        private List<Button> radioButtonsList = new();


        public Offline_MemberCustom(string customMember, int idMember, string namaMember, string hpMember,
            string emailMember)
        {
            InitializeComponent();
            btnSimpan.Enabled = false;
            hapusButton.Visible = false;
            baseOutlet = Settings.Default.BaseOutlet;
            Options = customMember;
            if (Options == "Edit")
            {
                hapusButton.Visible = Enabled;
                hapusButton.Enabled = true;
                btnSimpan.Enabled = true;
                btnSimpan.Text = "Edit";
                idid = idMember;
                txtNama.Text = namaMember;
                txtEmail.Text = hpMember;
                txtPhone.Text = emailMember;
            }
            else
            {
                btnSimpan.Enabled = true;
                btnSimpan.Text = "Tambah";
            }
        }

        private masterPos MasterPosForm { get; set; }
        public bool KeluarButtonClicked { get; private set; }

        public bool ReloadDataInBaseForm { get; private set; }

        private async void btnSimpan_Click(object sender, EventArgs e)
        {
            if (txtNama.Text == null || txtPhone.Text == null)
            {
                MessageBox.Show("Nama / Nomor Handphone masih kosong!");
                return;
            }

            if (Options == "Tambah")
            {
                Dictionary<string, object> json = new()
                {
                    { "name", txtNama.Text },
                    { "phone_number", txtPhone.Text },
                    { "outlet_id", baseOutlet },
                    { "email", txtEmail.Text }
                };

                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                IApiService apiService = new ApiService();
                HttpResponseMessage response = await apiService.CreateMember(jsonString, "/membership");

                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        DialogResult = DialogResult.OK;
                        MessageBox.Show("Data berhasil diTambah");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Member gagal ditambahkan silahkan coba ulang", "Gaspol");
                        DialogResult = DialogResult.Cancel;
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("Gagal tambah data silahkan coba ulang" + response, "Gaspol");
                    DialogResult = DialogResult.Cancel;
                }
            }
            else
            {
                IApiService apiService = new ApiService();

                string patchUrl = $"/membership/{idid}";
                Dictionary<string, object> json = new()
                {
                    { "name", txtNama.Text },
                    { "phone_number", txtPhone.Text },
                    { "outlet_id", baseOutlet },
                    { "email", txtEmail.Text }
                };

                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                HttpResponseMessage response = await apiService.EditMember(jsonString, $"/membership/{idid}");
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        DialogResult = DialogResult.OK;
                        MessageBox.Show("Data berhasil diEdit");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Member gagal diperbarui, silahkan coba ulang", "Gaspol");
                        DialogResult = DialogResult.Cancel;
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("Gagal memperbarui data, silahkan coba ulang" + response, "Gaspol");
                    DialogResult = DialogResult.Cancel;
                }
            }
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            //KeluarButtonClicked = true;
            DialogResult = DialogResult.OK;

            Close();
        }

        private async void hapusButton_Click(object sender, EventArgs e)
        {
            IApiService apiService = new ApiService();

            HttpResponseMessage responseMessage = await apiService.DeleteMember("/membership/" + idid);

            if (responseMessage.IsSuccessStatusCode)
            {
                DialogResult = DialogResult.OK;
                MessageBox.Show("Data berhasil diHapus");

                Close();
            }
            else
            {
                MessageBox.Show("Member gagal dihapus, silahkan coba ulang", "Gaspol");
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}