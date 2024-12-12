using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KASIR.Komponen
{
    public partial class KembalianForm : Form
    {
        public KembalianForm(string message)
        {
            InitializeComponent();
            labelKembalian.Text = message;
        }
        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static void Show(string message)
        {
            KembalianForm kembalianForm = new KembalianForm(message);
            kembalianForm.ShowDialog();
        }
    }
}
