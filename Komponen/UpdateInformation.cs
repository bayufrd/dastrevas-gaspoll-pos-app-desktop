using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace KASIR.Komponen
{
    public partial class UpdateInformation : Form
    {
        public UpdateInformation()
        {
            InitializeComponent();
            string info;
            string fileUrl = "https://raw.githubusercontent.com/bayufrd/update/main/update.txt"; // replace with the URL of your file

            using (WebClient client = new WebClient())
            {
                string fileContent = client.DownloadString(fileUrl);
                info = fileContent;
            }
            System.Windows.Forms.Label strukLabel = new System.Windows.Forms.Label();
            strukLabel.Text = info;
            strukLabel.BackColor = Color.Transparent;
            strukLabel.ForeColor = Color.Black;
            strukLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            strukLabel.AutoSize = true;
            strukLabel.Font = new System.Drawing.Font(strukLabel.Font.FontFamily, strukLabel.Font.Size, System.Drawing.FontStyle.Bold);
            gradientPanel2.AutoScroll = true;
            gradientPanel2.Controls.Add(strukLabel);
        }
    }
}
