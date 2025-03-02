﻿using InTheHand.Net.Bluetooth;

using FontAwesome.Sharp;
using InTheHand.Net.Sockets;
using InTheHand.Net;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Text.Json.Nodes;
namespace KASIR.OfflineMode
{
    public partial class Offline_updatePerItemForm : Form
    {
        private readonly string baseOutlet;
        public Offline_updatePerItemForm()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet.ToString();
            InitializeComponent();
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            // KeluarButtonPrintReportShiftClicked = true;
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel13_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {

            try
            {
                if (textPin.Text.ToString() == "" || textPin.Text == null)
                {
                    MessageBox.Show("Masukan pin terlebih dahulu", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                var dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);

                if (textPin.Text.ToString() == dataOutlet.data.pin.ToString())
                {
                        cancelReason = txtReason.Text;
                        DialogResult = DialogResult.OK;
                        Close();
                }
                else
                {
                    MessageBox.Show("Password salah", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal ubah data " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                DialogResult = DialogResult.Cancel;
            }

        }
        public string cancelReason {  get; set; }

        private void txtJumlahCicil_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
        private void txtSelesaiShift_TextChanged(object sender, EventArgs e)
        {

        }


    }

}

