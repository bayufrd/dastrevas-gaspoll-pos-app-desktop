namespace KASIR.komponen
{
    partial class createMenuForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(createMenuForm));
            panel1 = new Panel();
            btnKeluar = new Button();
            btnSimpan = new Button();
            panel2 = new Panel();
            panel3 = new Panel();
            cmbTipe = new ComboBox();
            txtHarga = new TextBox();
            btnTambah = new Button();
            txtNama = new TextBox();
            button5 = new Button();
            label2 = new Label();
            label3 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowVarian = new FlowLayoutPanel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Window;
            panel1.Controls.Add(btnKeluar);
            panel1.Controls.Add(btnSimpan);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(600, 70);
            panel1.TabIndex = 0;
            // 
            // btnKeluar
            // 
            btnKeluar.BackColor = Color.WhiteSmoke;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.ForeColor = Color.SteelBlue;
            btnKeluar.Location = new Point(12, 21);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(88, 30);
            btnKeluar.TabIndex = 8;
            btnKeluar.Text = "Keluar";
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += button1_Click_1;
            // 
            // btnSimpan
            // 
            btnSimpan.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnSimpan.BackColor = Color.SteelBlue;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.ForeColor = Color.White;
            btnSimpan.Location = new Point(500, 21);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(88, 29);
            btnSimpan.TabIndex = 7;
            btnSimpan.Text = "Simpan";
            btnSimpan.UseVisualStyleBackColor = false;
            btnSimpan.Click += button4_Click_1;
            // 
            // panel2
            // 
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(txtHarga);
            panel2.Controls.Add(btnTambah);
            panel2.Controls.Add(txtNama);
            panel2.Controls.Add(button5);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(label3);
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(585, 151);
            panel2.TabIndex = 19;
            // 
            // panel3
            // 
            panel3.BackColor = Color.White;
            panel3.Controls.Add(cmbTipe);
            panel3.Location = new Point(114, 51);
            panel3.Name = "panel3";
            panel3.Size = new Size(471, 28);
            panel3.TabIndex = 23;
            // 
            // cmbTipe
            // 
            cmbTipe.FlatStyle = FlatStyle.Flat;
            cmbTipe.FormattingEnabled = true;
            cmbTipe.Location = new Point(17, 3);
            cmbTipe.Name = "cmbTipe";
            cmbTipe.Size = new Size(451, 23);
            cmbTipe.TabIndex = 19;
            cmbTipe.SelectedIndexChanged += cmbTipe_SelectedIndexChanged;
            // 
            // txtHarga
            // 
            txtHarga.BorderStyle = BorderStyle.None;
            txtHarga.Location = new Point(132, 89);
            txtHarga.Name = "txtHarga";
            txtHarga.PlaceholderText = "Harga Menu";
            txtHarga.Size = new Size(440, 16);
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
            // txtNama
            // 
            txtNama.BorderStyle = BorderStyle.None;
            txtNama.Location = new Point(131, 23);
            txtNama.Name = "txtNama";
            txtNama.PlaceholderText = "Nama Menu";
            txtNama.Size = new Size(451, 16);
            txtNama.TabIndex = 18;
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
            button5.Location = new Point(9, -34);
            button5.Name = "button5";
            button5.Size = new Size(99, 182);
            button5.TabIndex = 17;
            button5.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            label2.BackColor = Color.White;
            label2.ForeColor = Color.Black;
            label2.Location = new Point(114, 16);
            label2.Name = "label2";
            label2.Size = new Size(471, 30);
            label2.TabIndex = 21;
            // 
            // label3
            // 
            label3.BackColor = Color.White;
            label3.ForeColor = Color.Black;
            label3.Location = new Point(114, 82);
            label3.Name = "label3";
            label3.Size = new Size(471, 30);
            label3.TabIndex = 22;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = Color.WhiteSmoke;
            flowLayoutPanel1.Controls.Add(panel2);
            flowLayoutPanel1.Controls.Add(flowVarian);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 70);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(600, 330);
            flowLayoutPanel1.TabIndex = 1;
            flowLayoutPanel1.Paint += flowLayoutPanel1_Paint;
            // 
            // flowVarian
            // 
            flowVarian.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowVarian.FlowDirection = FlowDirection.TopDown;
            flowVarian.Location = new Point(3, 160);
            flowVarian.Name = "flowVarian";
            flowVarian.Padding = new Padding(5, 0, 0, 0);
            flowVarian.Size = new Size(585, 170);
            flowVarian.TabIndex = 20;
            flowVarian.Paint += flowVarian_Paint;
            // 
            // createMenuForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = SystemColors.Control;
            ClientSize = new Size(600, 400);
            ControlBox = false;
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "createMenuForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "create1";
            TopMost = true;
            Resize += createMenuForm_Resize;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
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
        private Label label2;
        private Label label3;
        private Panel panel3;
    }
}