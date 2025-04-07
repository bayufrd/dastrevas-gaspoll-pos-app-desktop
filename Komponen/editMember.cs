

using FontAwesome.Sharp;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Windows.Forms.VisualStyles;
using System.Windows.Controls;
using System.Net.NetworkInformation;
namespace KASIR.komponen
{
    public partial class editMember : Form
    {
        private masterPos _masterPos;
        private masterPos MasterPosForm { get; set; }
        private List<System.Windows.Forms.Button> radioButtonsList = new List<System.Windows.Forms.Button>();
        public string btnPayType;
        string outletID, cartID, totalCart, ttl2;
        private readonly string baseOutlet;
        public bool KeluarButtonClicked { get; private set; }

        public bool ReloadDataInBaseForm { get; private set; }


        public editMember()
        {
            InitializeComponent();

            btnSimpan.Enabled = false;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
           
        }

        private async void btnSimpan_Click(object sender, EventArgs e)
        {
           
        }

        private void OnSimpanSuccess()
        {
            // Refresh or reload the cart in the MasterPos form
            if (Application.OpenForms["masterPos"] is masterPos masterPosForm)
            {
                // Call a method in the MasterPos form to refresh the cart
                masterPosForm.LoadCart();
            }
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            //KeluarButtonClicked = true;
            DialogResult = DialogResult.OK;

            this.Close();
        }
}
}