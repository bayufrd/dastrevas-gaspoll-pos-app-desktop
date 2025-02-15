namespace KASIR.OfflineMode
{
    partial class Offline_updateCartForm
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
            panel1 = new Panel();
            lblNameCart = new FontAwesome.Sharp.IconButton();
            btnHapus = new FontAwesome.Sharp.IconButton();
            btnKeluar = new FontAwesome.Sharp.IconButton();
            btnSimpan = new FontAwesome.Sharp.IconButton();
            panel7 = new Panel();
            panel6 = new Panel();
            label3 = new Label();
            txtNotes = new TextBox();
            panel5 = new Panel();
            panel12 = new Panel();
            comboBox1 = new ComboBox();
            lblTipe = new Label();
            label2 = new Label();
            panel2 = new Panel();
            panel4 = new Panel();
            btnKurang = new FontAwesome.Sharp.IconButton();
            btnTambah = new FontAwesome.Sharp.IconButton();
            panel3 = new Panel();
            txtKuantitas = new TextBox();
            label1 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel10 = new Panel();
            lblDiscount = new Label();
            panel11 = new Panel();
            cmbDiskon = new ComboBox();
            label5 = new Label();
            panel8 = new Panel();
            lblVarian = new Label();
            panel9 = new Panel();
            cmbVarian = new ComboBox();
            label4 = new Label();
            panel1.SuspendLayout();
            panel6.SuspendLayout();
            panel5.SuspendLayout();
            panel12.SuspendLayout();
            panel2.SuspendLayout();
            panel4.SuspendLayout();
            panel3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            panel10.SuspendLayout();
            panel11.SuspendLayout();
            panel8.SuspendLayout();
            panel9.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Window;
            panel1.Controls.Add(lblNameCart);
            panel1.Controls.Add(btnHapus);
            panel1.Controls.Add(btnKeluar);
            panel1.Controls.Add(btnSimpan);
            panel1.Controls.Add(panel7);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(600, 70);
            panel1.TabIndex = 4;
            panel1.Paint += panel1_Paint;
            // 
            // lblNameCart
            // 
            lblNameCart.BackColor = Color.White;
            lblNameCart.Cursor = Cursors.Cross;
            lblNameCart.FlatAppearance.BorderSize = 0;
            lblNameCart.FlatStyle = FlatStyle.Flat;
            lblNameCart.Font = new Font("Segoe UI", 10F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            lblNameCart.ForeColor = Color.FromArgb(30, 31, 68);
            lblNameCart.IconChar = FontAwesome.Sharp.IconChar.Carrot;
            lblNameCart.IconColor = Color.FromArgb(30, 31, 68);
            lblNameCart.IconFont = FontAwesome.Sharp.IconFont.Auto;
            lblNameCart.IconSize = 25;
            lblNameCart.ImageAlign = ContentAlignment.MiddleRight;
            lblNameCart.Location = new Point(106, 19);
            lblNameCart.Name = "lblNameCart";
            lblNameCart.Size = new Size(294, 30);
            lblNameCart.TabIndex = 27;
            lblNameCart.Text = "Konfirmasi";
            lblNameCart.TextImageRelation = TextImageRelation.ImageBeforeText;
            lblNameCart.UseVisualStyleBackColor = false;
            // 
            // btnHapus
            // 
            btnHapus.BackColor = Color.DarkRed;
            btnHapus.Cursor = Cursors.Hand;
            btnHapus.FlatAppearance.BorderSize = 0;
            btnHapus.FlatStyle = FlatStyle.Flat;
            btnHapus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnHapus.ForeColor = Color.WhiteSmoke;
            btnHapus.IconChar = FontAwesome.Sharp.IconChar.TrashRestore;
            btnHapus.IconColor = Color.WhiteSmoke;
            btnHapus.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnHapus.IconSize = 25;
            btnHapus.Location = new Point(406, 20);
            btnHapus.Name = "btnHapus";
            btnHapus.Size = new Size(88, 30);
            btnHapus.TabIndex = 30;
            btnHapus.Text = "Hapus";
            btnHapus.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnHapus.UseVisualStyleBackColor = false;
            btnHapus.Click += btnHapus_Click;
            // 
            // btnKeluar
            // 
            btnKeluar.BackColor = Color.WhiteSmoke;
            btnKeluar.Cursor = Cursors.Hand;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.Flip = FontAwesome.Sharp.FlipOrientation.Horizontal;
            btnKeluar.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnKeluar.ForeColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconChar = FontAwesome.Sharp.IconChar.CircleChevronRight;
            btnKeluar.IconColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnKeluar.IconSize = 25;
            btnKeluar.Location = new Point(12, 20);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(88, 30);
            btnKeluar.TabIndex = 28;
            btnKeluar.Text = "Keluar";
            btnKeluar.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += btnKeluar_Click;
            // 
            // btnSimpan
            // 
            btnSimpan.BackColor = Color.FromArgb(30, 31, 68);
            btnSimpan.Cursor = Cursors.Hand;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnSimpan.ForeColor = Color.WhiteSmoke;
            btnSimpan.IconChar = FontAwesome.Sharp.IconChar.CheckCircle;
            btnSimpan.IconColor = Color.WhiteSmoke;
            btnSimpan.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnSimpan.IconSize = 25;
            btnSimpan.Location = new Point(500, 20);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(88, 30);
            btnSimpan.TabIndex = 29;
            btnSimpan.Text = "Simpan";
            btnSimpan.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSimpan.UseVisualStyleBackColor = false;
            btnSimpan.Click += btnSimpan_Click;
            // 
            // panel7
            // 
            panel7.Location = new Point(3, 70);
            panel7.Name = "panel7";
            panel7.Size = new Size(585, 10);
            panel7.TabIndex = 6;
            // 
            // panel6
            // 
            panel6.Controls.Add(label3);
            panel6.Controls.Add(txtNotes);
            panel6.Location = new Point(3, 378);
            panel6.Name = "panel6";
            panel6.Size = new Size(585, 100);
            panel6.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(6, 5);
            label3.Name = "label3";
            label3.Size = new Size(57, 15);
            label3.TabIndex = 8;
            label3.Text = "CATATAN";
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(6, 25);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.PlaceholderText = "Catatan ...";
            txtNotes.Size = new Size(577, 67);
            txtNotes.TabIndex = 1;
            // 
            // panel5
            // 
            panel5.Controls.Add(panel12);
            panel5.Controls.Add(lblTipe);
            panel5.Controls.Add(label2);
            panel5.Location = new Point(3, 283);
            panel5.Name = "panel5";
            panel5.Size = new Size(597, 89);
            panel5.TabIndex = 4;
            // 
            // panel12
            // 
            panel12.BackColor = SystemColors.ControlLightLight;
            panel12.BorderStyle = BorderStyle.FixedSingle;
            panel12.Controls.Add(comboBox1);
            panel12.Location = new Point(7, 44);
            panel12.Name = "panel12";
            panel12.Size = new Size(576, 38);
            panel12.TabIndex = 9;
            // 
            // comboBox1
            // 
            comboBox1.BackColor = Color.White;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FlatStyle = FlatStyle.Flat;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(3, 5);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(568, 23);
            comboBox1.TabIndex = 0;
            // 
            // lblTipe
            // 
            lblTipe.AutoSize = true;
            lblTipe.Location = new Point(5, 26);
            lblTipe.Name = "lblTipe";
            lblTipe.Size = new Size(29, 15);
            lblTipe.TabIndex = 8;
            lblTipe.Text = "Tipe";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(4, 5);
            label2.Name = "label2";
            label2.Size = new Size(100, 15);
            label2.TabIndex = 1;
            label2.Text = "TIPE PENJUALAN";
            // 
            // panel2
            // 
            panel2.Controls.Add(panel4);
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(label1);
            panel2.Location = new Point(3, 206);
            panel2.Name = "panel2";
            panel2.Size = new Size(597, 71);
            panel2.TabIndex = 2;
            // 
            // panel4
            // 
            panel4.Controls.Add(btnKurang);
            panel4.Controls.Add(btnTambah);
            panel4.Location = new Point(314, 29);
            panel4.Name = "panel4";
            panel4.Size = new Size(270, 36);
            panel4.TabIndex = 3;
            // 
            // btnKurang
            // 
            btnKurang.BackColor = Color.WhiteSmoke;
            btnKurang.Cursor = Cursors.Hand;
            btnKurang.FlatAppearance.BorderSize = 0;
            btnKurang.FlatStyle = FlatStyle.Popup;
            btnKurang.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnKurang.ForeColor = Color.FromArgb(30, 31, 68);
            btnKurang.IconChar = FontAwesome.Sharp.IconChar.CircleMinus;
            btnKurang.IconColor = Color.FromArgb(30, 31, 68);
            btnKurang.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnKurang.IconSize = 25;
            btnKurang.Location = new Point(0, 0);
            btnKurang.Name = "btnKurang";
            btnKurang.Size = new Size(136, 36);
            btnKurang.TabIndex = 30;
            btnKurang.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnKurang.UseVisualStyleBackColor = false;
            btnKurang.Click += btnKurang_Click_1;
            // 
            // btnTambah
            // 
            btnTambah.BackColor = Color.WhiteSmoke;
            btnTambah.Cursor = Cursors.Hand;
            btnTambah.FlatAppearance.BorderSize = 0;
            btnTambah.FlatStyle = FlatStyle.Popup;
            btnTambah.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnTambah.ForeColor = Color.FromArgb(30, 31, 68);
            btnTambah.IconChar = FontAwesome.Sharp.IconChar.PlusCircle;
            btnTambah.IconColor = Color.FromArgb(30, 31, 68);
            btnTambah.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnTambah.IconSize = 25;
            btnTambah.Location = new Point(134, 0);
            btnTambah.Name = "btnTambah";
            btnTambah.Size = new Size(136, 36);
            btnTambah.TabIndex = 29;
            btnTambah.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnTambah.UseVisualStyleBackColor = false;
            btnTambah.Click += btnTambah_Click_1;
            // 
            // panel3
            // 
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(txtKuantitas);
            panel3.Location = new Point(8, 29);
            panel3.Name = "panel3";
            panel3.Size = new Size(270, 36);
            panel3.TabIndex = 2;
            // 
            // txtKuantitas
            // 
            txtKuantitas.BorderStyle = BorderStyle.None;
            txtKuantitas.Location = new Point(1, 8);
            txtKuantitas.Name = "txtKuantitas";
            txtKuantitas.PlaceholderText = "Masukan kuantitas ...";
            txtKuantitas.Size = new Size(246, 16);
            txtKuantitas.TabIndex = 0;
            txtKuantitas.TextAlign = HorizontalAlignment.Right;
            txtKuantitas.TextChanged += txtKuantitas_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(5, 5);
            label1.Name = "label1";
            label1.Size = new Size(71, 15);
            label1.TabIndex = 1;
            label1.Text = "KUANTITAS";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.BackColor = Color.White;
            flowLayoutPanel1.Controls.Add(panel10);
            flowLayoutPanel1.Controls.Add(panel8);
            flowLayoutPanel1.Controls.Add(panel2);
            flowLayoutPanel1.Controls.Add(panel5);
            flowLayoutPanel1.Controls.Add(panel6);
            flowLayoutPanel1.Location = new Point(0, 70);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(600, 530);
            flowLayoutPanel1.TabIndex = 3;
            // 
            // panel10
            // 
            panel10.Controls.Add(lblDiscount);
            panel10.Controls.Add(panel11);
            panel10.Controls.Add(label5);
            panel10.Location = new Point(3, 3);
            panel10.Name = "panel10";
            panel10.Size = new Size(597, 101);
            panel10.TabIndex = 11;
            // 
            // lblDiscount
            // 
            lblDiscount.AutoSize = true;
            lblDiscount.Location = new Point(5, 24);
            lblDiscount.Name = "lblDiscount";
            lblDiscount.Size = new Size(46, 15);
            lblDiscount.TabIndex = 7;
            lblDiscount.Text = "Diskon ";
            // 
            // panel11
            // 
            panel11.BackColor = SystemColors.ControlLightLight;
            panel11.BorderStyle = BorderStyle.FixedSingle;
            panel11.Controls.Add(cmbDiskon);
            panel11.Location = new Point(8, 42);
            panel11.Name = "panel11";
            panel11.Size = new Size(576, 38);
            panel11.TabIndex = 5;
            // 
            // cmbDiskon
            // 
            cmbDiskon.BackColor = Color.White;
            cmbDiskon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDiskon.Enabled = false;
            cmbDiskon.FlatStyle = FlatStyle.Flat;
            cmbDiskon.FormattingEnabled = true;
            cmbDiskon.Location = new Point(3, 7);
            cmbDiskon.Name = "cmbDiskon";
            cmbDiskon.Size = new Size(568, 23);
            cmbDiskon.TabIndex = 0;
            cmbDiskon.SelectedIndexChanged += cmbDiskon_SelectedIndexChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.Location = new Point(4, 6);
            label5.Name = "label5";
            label5.Size = new Size(109, 15);
            label5.TabIndex = 4;
            label5.Text = "DISKON (Disabled)";
            // 
            // panel8
            // 
            panel8.Controls.Add(lblVarian);
            panel8.Controls.Add(panel9);
            panel8.Controls.Add(label4);
            panel8.Location = new Point(3, 110);
            panel8.Name = "panel8";
            panel8.Size = new Size(597, 90);
            panel8.TabIndex = 10;
            // 
            // lblVarian
            // 
            lblVarian.AutoSize = true;
            lblVarian.Location = new Point(5, 24);
            lblVarian.Name = "lblVarian";
            lblVarian.Size = new Size(39, 15);
            lblVarian.TabIndex = 6;
            lblVarian.Text = "Varian";
            // 
            // panel9
            // 
            panel9.BackColor = SystemColors.ControlLightLight;
            panel9.BorderStyle = BorderStyle.FixedSingle;
            panel9.Controls.Add(cmbVarian);
            panel9.Location = new Point(8, 47);
            panel9.Name = "panel9";
            panel9.Size = new Size(576, 38);
            panel9.TabIndex = 5;
            // 
            // cmbVarian
            // 
            cmbVarian.BackColor = Color.White;
            cmbVarian.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVarian.FlatStyle = FlatStyle.Flat;
            cmbVarian.FormattingEnabled = true;
            cmbVarian.Location = new Point(3, 3);
            cmbVarian.Name = "cmbVarian";
            cmbVarian.Size = new Size(568, 23);
            cmbVarian.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(4, 6);
            label4.Name = "label4";
            label4.Size = new Size(50, 15);
            label4.TabIndex = 4;
            label4.Text = "VARIAN";
            // 
            // Offline_updateCartForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(600, 600);
            Controls.Add(panel1);
            Controls.Add(flowLayoutPanel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximumSize = new Size(600, 600);
            Name = "Offline_updateCartForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "updateCart";
            TopMost = true;
            panel1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            panel12.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel4.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            panel10.ResumeLayout(false);
            panel10.PerformLayout();
            panel11.ResumeLayout(false);
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            panel9.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panel1;
        private Panel panel7;
        private Panel panel6;
        private Label label3;
        private TextBox txtNotes;
        private Panel panel5;
        private Label lblTipe;
        private Label label2;
        private Panel panel2;
        private Panel panel4;
        private Panel panel3;
        private TextBox txtKuantitas;
        private Label label1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel8;
        private Panel panel9;
        private ComboBox cmbVarian;
        private Label label4;
        private Label lblVarian;
        private Panel panel10;
        private Panel panel11;
        private ComboBox cmbDiskon;
        private Label label5;
        private Label lblDiscount;
        private Panel panel12;
        private ComboBox comboBox1;
        private FontAwesome.Sharp.IconButton btnKeluar;
        private FontAwesome.Sharp.IconButton btnSimpan;
        private FontAwesome.Sharp.IconButton btnHapus;
        private FontAwesome.Sharp.IconButton lblNameCart;
        private FontAwesome.Sharp.IconButton btnKurang;
        private FontAwesome.Sharp.IconButton btnTambah;
    }
}