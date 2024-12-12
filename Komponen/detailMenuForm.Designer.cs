
using FontAwesome.Sharp;
namespace KASIR.komponen
{
    partial class detailMenuForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(detailMenuForm));
            panel1 = new Panel();
            btnHapus = new Button();
            btnKeluar = new Button();
            btnSimpan = new Button();
            panel2 = new Panel();
            txtHarga = new TextBox();
            btnTambah = new Button();
            cmbTipe = new ComboBox();
            txtNama = new TextBox();
            button5 = new Button();
            label2 = new Label();
            label1 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowVarian = new FlowLayoutPanel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Window;
            panel1.Controls.Add(btnHapus);
            panel1.Controls.Add(btnKeluar);
            panel1.Controls.Add(btnSimpan);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(1, 354);
            panel1.Name = "panel1";
            panel1.Size = new Size(598, 45);
            panel1.TabIndex = 0;
            // 
            // btnHapus
            // 
            btnHapus.BackColor = Color.FromArgb(255, 192, 192);
            btnHapus.FlatAppearance.BorderSize = 0;
            btnHapus.FlatStyle = FlatStyle.Flat;
            btnHapus.ForeColor = Color.Red;
            btnHapus.Location = new Point(406, 7);
            btnHapus.Name = "btnHapus";
            btnHapus.Size = new Size(88, 29);
            btnHapus.TabIndex = 10;
            btnHapus.Text = "Hapus";
            btnHapus.UseVisualStyleBackColor = false;
            btnHapus.Click += btnHapus_Click;
            // 
            // btnKeluar
            // 
            btnKeluar.BackColor = Color.WhiteSmoke;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.ForeColor = Color.SteelBlue;
            btnKeluar.Location = new Point(12, 7);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(88, 29);
            btnKeluar.TabIndex = 8;
            btnKeluar.Text = "Batal";
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += button1_Click_1;
            // 
            // btnSimpan
            // 
            btnSimpan.BackColor = Color.SteelBlue;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.ForeColor = Color.White;
            btnSimpan.Location = new Point(500, 7);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(88, 29);
            btnSimpan.TabIndex = 7;
            btnSimpan.Text = "Update";
            btnSimpan.UseVisualStyleBackColor = false;
            btnSimpan.Click += button4_Click_1;
            // 
            // panel2
            // 
            panel2.Controls.Add(txtHarga);
            panel2.Controls.Add(btnTambah);
            panel2.Controls.Add(cmbTipe);
            panel2.Controls.Add(txtNama);
            panel2.Controls.Add(button5);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(label1);
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(585, 151);
            panel2.TabIndex = 19;
            // 
            // txtHarga
            // 
            txtHarga.BorderStyle = BorderStyle.None;
            txtHarga.Location = new Point(122, 87);
            txtHarga.Name = "txtHarga";
            txtHarga.PlaceholderText = "Harga Menu";
            txtHarga.Size = new Size(471, 16);
            txtHarga.TabIndex = 20;
            // 
            // btnTambah
            // 
            btnTambah.BackColor = Color.SteelBlue;
            btnTambah.FlatAppearance.BorderSize = 0;
            btnTambah.FlatStyle = FlatStyle.Flat;
            btnTambah.ForeColor = Color.White;
            btnTambah.Location = new Point(9, 119);
            btnTambah.Name = "btnTambah";
            btnTambah.Size = new Size(576, 29);
            btnTambah.TabIndex = 10;
            btnTambah.Text = "Tambah Varian";
            btnTambah.UseVisualStyleBackColor = false;
            btnTambah.Click += btnTambah_Click_1;
            // 
            // cmbTipe
            // 
            cmbTipe.FormattingEnabled = true;
            cmbTipe.Location = new Point(114, 53);
            cmbTipe.Name = "cmbTipe";
            cmbTipe.Size = new Size(471, 23);
            cmbTipe.TabIndex = 19;
            // 
            // txtNama
            // 
            txtNama.BorderStyle = BorderStyle.None;
            txtNama.Location = new Point(122, 26);
            txtNama.Name = "txtNama";
            txtNama.PlaceholderText = "Nama Menu";
            txtNama.Size = new Size(459, 16);
            txtNama.TabIndex = 18;
            txtNama.TextChanged += txtNama_TextChanged;
            // 
            // button5
            // 
            button5.AutoSize = true;
            button5.BackColor = Color.Transparent;
            button5.BackgroundImage = (Image)resources.GetObject("button5.BackgroundImage");
            button5.BackgroundImageLayout = ImageLayout.Zoom;
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.ForeColor = Color.Black;
            button5.ImageAlign = ContentAlignment.TopLeft;
            button5.Location = new Point(9, -16);
            button5.Name = "button5";
            button5.Size = new Size(99, 164);
            button5.TabIndex = 17;
            button5.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            label2.BackColor = Color.White;
            label2.ForeColor = Color.Black;
            label2.Location = new Point(114, 18);
            label2.Name = "label2";
            label2.Size = new Size(471, 30);
            label2.TabIndex = 22;
            // 
            // label1
            // 
            label1.BackColor = Color.White;
            label1.ForeColor = Color.Black;
            label1.Location = new Point(114, 81);
            label1.Name = "label1";
            label1.Size = new Size(471, 30);
            label1.TabIndex = 23;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = Color.WhiteSmoke;
            flowLayoutPanel1.Controls.Add(panel2);
            flowLayoutPanel1.Controls.Add(flowVarian);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(1, 1);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(598, 353);
            flowLayoutPanel1.TabIndex = 1;
            flowLayoutPanel1.Paint += flowLayoutPanel1_Paint;
            // 
            // flowVarian
            // 
            flowVarian.FlowDirection = FlowDirection.TopDown;
            flowVarian.Location = new Point(3, 160);
            flowVarian.Name = "flowVarian";
            flowVarian.Padding = new Padding(5, 0, 0, 0);
            flowVarian.Size = new Size(585, 165);
            flowVarian.TabIndex = 20;
            flowVarian.Paint += flowVarian_Paint;
            // 
            // detailMenuForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 400);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            Name = "detailMenuForm";
            Padding = new Padding(1);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "create1";
            TopMost = true;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void label1_Click_1(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private Panel panel1;
        private Button btnSimpan;
        private Button btnKeluar;
        private Panel panel2;
        private TextBox txtNama;
        private Button button5;
        private FlowLayoutPanel flowLayoutPanel1;
        private ComboBox cmbTipe;
        private Button btnTambah;
        private TextBox txtHarga;
        private FlowLayoutPanel flowVarian;
        private Button btnHapus;
        private Label label2;
        private Label label1;
    }
}