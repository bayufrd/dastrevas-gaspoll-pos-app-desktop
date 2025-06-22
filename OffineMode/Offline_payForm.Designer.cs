
using FontAwesome.Sharp;
namespace KASIR.OfflineMode
{
    partial class Offline_payForm
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
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel9 = new Panel();
            panel10 = new Panel();
            txtSeat = new TextBox();
            label5 = new Label();
            panel11 = new Panel();
            txtNama = new TextBox();
            label4 = new Label();
            panel3 = new Panel();
            lblKembalian = new Label();
            btnSetPrice3 = new Button();
            btnSetPrice2 = new Button();
            btnSetPrice1 = new Button();
            txtJumlahPembayaran = new Label();
            panel5 = new Panel();
            label3 = new Label();
            txtCash = new TextBox();
            label1 = new Label();
            panel6 = new Panel();
            panel12 = new Panel();
            cmbPayform = new ComboBox();
            label6 = new Label();
            panel4 = new Panel();
            lblUsePoint = new Label();
            ButtonSwitchUsePoint = new Model.SButton();
            lblPoint = new Label();
            btnDataMember = new IconButton();
            panel14 = new Panel();
            lblHPMember = new Label();
            panel13 = new Panel();
            lblEmailMember = new Label();
            panel8 = new Panel();
            lblNamaMember = new Label();
            sButton1 = new Model.SButton();
            label7 = new Label();
            panel2 = new Panel();
            btnSimpan = new IconButton();
            btnKeluar = new IconButton();
            panel7 = new Panel();
            btnTunai = new Button();
            label2 = new Label();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            panel9.SuspendLayout();
            panel10.SuspendLayout();
            panel11.SuspendLayout();
            panel3.SuspendLayout();
            panel5.SuspendLayout();
            panel6.SuspendLayout();
            panel12.SuspendLayout();
            panel4.SuspendLayout();
            panel14.SuspendLayout();
            panel13.SuspendLayout();
            panel8.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(flowLayoutPanel1);
            panel1.Controls.Add(panel2);
            panel1.Dock = DockStyle.Fill;
            panel1.ForeColor = Color.SteelBlue;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(600, 530);
            panel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.BackColor = Color.White;
            flowLayoutPanel1.Controls.Add(panel9);
            flowLayoutPanel1.Controls.Add(panel3);
            flowLayoutPanel1.Controls.Add(panel6);
            flowLayoutPanel1.Controls.Add(panel4);
            flowLayoutPanel1.Location = new Point(0, 70);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(598, 455);
            flowLayoutPanel1.TabIndex = 3;
            // 
            // panel9
            // 
            panel9.Controls.Add(panel10);
            panel9.Controls.Add(label5);
            panel9.Controls.Add(panel11);
            panel9.Controls.Add(label4);
            panel9.Location = new Point(3, 3);
            panel9.Name = "panel9";
            panel9.Size = new Size(597, 74);
            panel9.TabIndex = 4;
            // 
            // panel10
            // 
            panel10.BorderStyle = BorderStyle.FixedSingle;
            panel10.Controls.Add(txtSeat);
            panel10.Location = new Point(313, 22);
            panel10.Name = "panel10";
            panel10.Size = new Size(271, 36);
            panel10.TabIndex = 4;
            // 
            // txtSeat
            // 
            txtSeat.BorderStyle = BorderStyle.None;
            txtSeat.Location = new Point(11, 9);
            txtSeat.Name = "txtSeat";
            txtSeat.PlaceholderText = "Masukan Nomor Seat ...";
            txtSeat.Size = new Size(246, 16);
            txtSeat.TabIndex = 0;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(310, 4);
            label5.Name = "label5";
            label5.Size = new Size(34, 15);
            label5.TabIndex = 3;
            label5.Text = "SEAT";
            // 
            // panel11
            // 
            panel11.BorderStyle = BorderStyle.FixedSingle;
            panel11.Controls.Add(txtNama);
            panel11.Location = new Point(8, 23);
            panel11.Name = "panel11";
            panel11.Size = new Size(270, 36);
            panel11.TabIndex = 2;
            // 
            // txtNama
            // 
            txtNama.BorderStyle = BorderStyle.None;
            txtNama.Location = new Point(11, 9);
            txtNama.Name = "txtNama";
            txtNama.PlaceholderText = "Masukan Nama ...";
            txtNama.Size = new Size(246, 16);
            txtNama.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(5, 5);
            label4.Name = "label4";
            label4.Size = new Size(43, 15);
            label4.TabIndex = 1;
            label4.Text = "NAMA";
            // 
            // panel3
            // 
            panel3.Controls.Add(lblKembalian);
            panel3.Controls.Add(btnSetPrice3);
            panel3.Controls.Add(btnSetPrice2);
            panel3.Controls.Add(btnSetPrice1);
            panel3.Controls.Add(txtJumlahPembayaran);
            panel3.Controls.Add(panel5);
            panel3.Controls.Add(label1);
            panel3.Location = new Point(3, 83);
            panel3.Name = "panel3";
            panel3.Size = new Size(597, 176);
            panel3.TabIndex = 2;
            // 
            // lblKembalian
            // 
            lblKembalian.AutoSize = true;
            lblKembalian.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblKembalian.ForeColor = Color.Black;
            lblKembalian.Location = new Point(313, 5);
            lblKembalian.Name = "lblKembalian";
            lblKembalian.Size = new Size(61, 15);
            lblKembalian.TabIndex = 9;
            lblKembalian.Text = "CHANGES";
            // 
            // btnSetPrice3
            // 
            btnSetPrice3.FlatStyle = FlatStyle.Flat;
            btnSetPrice3.ForeColor = Color.Black;
            btnSetPrice3.Location = new Point(411, 70);
            btnSetPrice3.Name = "btnSetPrice3";
            btnSetPrice3.Size = new Size(174, 36);
            btnSetPrice3.TabIndex = 8;
            btnSetPrice3.Text = "Rp. 1000000";
            btnSetPrice3.UseVisualStyleBackColor = true;
            btnSetPrice3.Click += btnSetPrice3_Click;
            // 
            // btnSetPrice2
            // 
            btnSetPrice2.FlatStyle = FlatStyle.Flat;
            btnSetPrice2.ForeColor = Color.Black;
            btnSetPrice2.Location = new Point(211, 70);
            btnSetPrice2.Name = "btnSetPrice2";
            btnSetPrice2.Size = new Size(174, 36);
            btnSetPrice2.TabIndex = 7;
            btnSetPrice2.Text = "Rp. 500000";
            btnSetPrice2.UseVisualStyleBackColor = true;
            btnSetPrice2.Click += btnSetPrice2_Click_1;
            // 
            // btnSetPrice1
            // 
            btnSetPrice1.FlatStyle = FlatStyle.Flat;
            btnSetPrice1.ForeColor = Color.Black;
            btnSetPrice1.Location = new Point(9, 71);
            btnSetPrice1.Name = "btnSetPrice1";
            btnSetPrice1.Size = new Size(174, 36);
            btnSetPrice1.TabIndex = 6;
            btnSetPrice1.Text = "Rp. 30000";
            btnSetPrice1.UseVisualStyleBackColor = true;
            btnSetPrice1.Click += btnSetPrice1_Click;
            // 
            // txtJumlahPembayaran
            // 
            txtJumlahPembayaran.AutoSize = true;
            txtJumlahPembayaran.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            txtJumlahPembayaran.ForeColor = Color.Black;
            txtJumlahPembayaran.Location = new Point(5, 30);
            txtJumlahPembayaran.Name = "txtJumlahPembayaran";
            txtJumlahPembayaran.Size = new Size(0, 15);
            txtJumlahPembayaran.TabIndex = 3;
            // 
            // panel5
            // 
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Controls.Add(label3);
            panel5.Controls.Add(txtCash);
            panel5.Location = new Point(9, 123);
            panel5.Name = "panel5";
            panel5.Size = new Size(576, 36);
            panel5.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(3, 9);
            label3.Name = "label3";
            label3.Size = new Size(24, 15);
            label3.TabIndex = 9;
            label3.Text = "Rp.";
            // 
            // txtCash
            // 
            txtCash.BorderStyle = BorderStyle.None;
            txtCash.Location = new Point(29, 9);
            txtCash.Name = "txtCash";
            txtCash.PlaceholderText = "Nominal";
            txtCash.Size = new Size(533, 16);
            txtCash.TabIndex = 1;
            txtCash.TextChanged += txtCash_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(5, 5);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 1;
            label1.Text = "CASH";
            // 
            // panel6
            // 
            panel6.Controls.Add(panel12);
            panel6.Controls.Add(label6);
            panel6.Location = new Point(3, 265);
            panel6.Name = "panel6";
            panel6.Size = new Size(597, 74);
            panel6.TabIndex = 11;
            // 
            // panel12
            // 
            panel12.BackColor = SystemColors.ControlLightLight;
            panel12.BorderStyle = BorderStyle.FixedSingle;
            panel12.Controls.Add(cmbPayform);
            panel12.Location = new Point(8, 24);
            panel12.Name = "panel12";
            panel12.Size = new Size(576, 38);
            panel12.TabIndex = 5;
            // 
            // cmbPayform
            // 
            cmbPayform.BackColor = Color.White;
            cmbPayform.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayform.FlatStyle = FlatStyle.Flat;
            cmbPayform.FormattingEnabled = true;
            cmbPayform.Location = new Point(3, 4);
            cmbPayform.Name = "cmbPayform";
            cmbPayform.Size = new Size(568, 23);
            cmbPayform.TabIndex = 0;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label6.ForeColor = Color.Black;
            label6.Location = new Point(4, 6);
            label6.Name = "label6";
            label6.Size = new Size(110, 15);
            label6.TabIndex = 4;
            label6.Text = "TIPE PEMBAYARAN";
            // 
            // panel4
            // 
            panel4.Controls.Add(lblUsePoint);
            panel4.Controls.Add(ButtonSwitchUsePoint);
            panel4.Controls.Add(lblPoint);
            panel4.Controls.Add(btnDataMember);
            panel4.Controls.Add(panel14);
            panel4.Controls.Add(panel13);
            panel4.Controls.Add(panel8);
            panel4.Controls.Add(sButton1);
            panel4.Controls.Add(label7);
            panel4.Location = new Point(3, 345);
            panel4.Name = "panel4";
            panel4.Size = new Size(592, 102);
            panel4.TabIndex = 12;
            // 
            // lblUsePoint
            // 
            lblUsePoint.AutoSize = true;
            lblUsePoint.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblUsePoint.ForeColor = Color.Black;
            lblUsePoint.Location = new Point(466, 66);
            lblUsePoint.Name = "lblUsePoint";
            lblUsePoint.Size = new Size(67, 15);
            lblUsePoint.TabIndex = 31;
            lblUsePoint.Text = "USE POINT";
            lblUsePoint.Visible = false;
            // 
            // ButtonSwitchUsePoint
            // 
            ButtonSwitchUsePoint.AutoSize = true;
            ButtonSwitchUsePoint.Location = new Point(539, 63);
            ButtonSwitchUsePoint.MinimumSize = new Size(45, 22);
            ButtonSwitchUsePoint.Name = "ButtonSwitchUsePoint";
            ButtonSwitchUsePoint.OffBackColor = Color.Gray;
            ButtonSwitchUsePoint.OffToggleColor = Color.Gainsboro;
            ButtonSwitchUsePoint.OnBackColor = Color.MediumSlateBlue;
            ButtonSwitchUsePoint.OnToggleColor = Color.WhiteSmoke;
            ButtonSwitchUsePoint.Size = new Size(45, 22);
            ButtonSwitchUsePoint.TabIndex = 30;
            ButtonSwitchUsePoint.UseVisualStyleBackColor = true;
            ButtonSwitchUsePoint.Visible = false;
            ButtonSwitchUsePoint.CheckedChanged += ButtonSwitchUsePoint_CheckedChanged;
            // 
            // lblPoint
            // 
            lblPoint.AutoSize = true;
            lblPoint.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            lblPoint.ForeColor = Color.Black;
            lblPoint.Location = new Point(276, 66);
            lblPoint.Name = "lblPoint";
            lblPoint.Size = new Size(95, 15);
            lblPoint.TabIndex = 29;
            lblPoint.Text = "Total Point : 0,-";
            // 
            // btnDataMember
            // 
            btnDataMember.BackColor = Color.FromArgb(30, 31, 68);
            btnDataMember.Cursor = Cursors.Hand;
            btnDataMember.FlatAppearance.BorderSize = 0;
            btnDataMember.FlatStyle = FlatStyle.Flat;
            btnDataMember.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnDataMember.ForeColor = Color.WhiteSmoke;
            btnDataMember.IconChar = IconChar.ListCheck;
            btnDataMember.IconColor = Color.WhiteSmoke;
            btnDataMember.IconFont = IconFont.Auto;
            btnDataMember.IconSize = 25;
            btnDataMember.Location = new Point(535, 31);
            btnDataMember.Name = "btnDataMember";
            btnDataMember.Size = new Size(50, 24);
            btnDataMember.TabIndex = 25;
            btnDataMember.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnDataMember.UseVisualStyleBackColor = false;
            btnDataMember.Click += btnDataMember_Click;
            // 
            // panel14
            // 
            panel14.BorderStyle = BorderStyle.FixedSingle;
            panel14.Controls.Add(lblHPMember);
            panel14.Location = new Point(272, 31);
            panel14.Name = "panel14";
            panel14.Size = new Size(257, 24);
            panel14.TabIndex = 5;
            // 
            // lblHPMember
            // 
            lblHPMember.AutoSize = true;
            lblHPMember.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            lblHPMember.ForeColor = Color.Black;
            lblHPMember.Location = new Point(3, 3);
            lblHPMember.Name = "lblHPMember";
            lblHPMember.Size = new Size(101, 15);
            lblHPMember.TabIndex = 28;
            lblHPMember.Text = "No. HP Member :";
            // 
            // panel13
            // 
            panel13.BorderStyle = BorderStyle.FixedSingle;
            panel13.Controls.Add(lblEmailMember);
            panel13.Location = new Point(9, 61);
            panel13.Name = "panel13";
            panel13.Size = new Size(257, 24);
            panel13.TabIndex = 4;
            // 
            // lblEmailMember
            // 
            lblEmailMember.AutoSize = true;
            lblEmailMember.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            lblEmailMember.ForeColor = Color.Black;
            lblEmailMember.Location = new Point(4, 4);
            lblEmailMember.Name = "lblEmailMember";
            lblEmailMember.Size = new Size(93, 15);
            lblEmailMember.TabIndex = 27;
            lblEmailMember.Text = "Email Member :";
            // 
            // panel8
            // 
            panel8.BorderStyle = BorderStyle.FixedSingle;
            panel8.Controls.Add(lblNamaMember);
            panel8.Location = new Point(9, 31);
            panel8.Name = "panel8";
            panel8.Size = new Size(257, 24);
            panel8.TabIndex = 3;
            // 
            // lblNamaMember
            // 
            lblNamaMember.AutoSize = true;
            lblNamaMember.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            lblNamaMember.ForeColor = Color.Black;
            lblNamaMember.Location = new Point(4, 4);
            lblNamaMember.Name = "lblNamaMember";
            lblNamaMember.Size = new Size(96, 15);
            lblNamaMember.TabIndex = 26;
            lblNamaMember.Text = "Nama Member :";
            // 
            // sButton1
            // 
            sButton1.AutoSize = true;
            sButton1.Location = new Point(8, 3);
            sButton1.MinimumSize = new Size(45, 22);
            sButton1.Name = "sButton1";
            sButton1.OffBackColor = Color.Gray;
            sButton1.OffToggleColor = Color.Gainsboro;
            sButton1.OnBackColor = Color.MediumSlateBlue;
            sButton1.OnToggleColor = Color.WhiteSmoke;
            sButton1.Size = new Size(45, 22);
            sButton1.TabIndex = 5;
            sButton1.UseVisualStyleBackColor = true;
            sButton1.CheckedChanged += sButton1_CheckedChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label7.ForeColor = Color.Black;
            label7.Location = new Point(60, 8);
            label7.Name = "label7";
            label7.Size = new Size(114, 15);
            label7.TabIndex = 4;
            label7.Text = "MEMBERSHIP AREA";
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(btnSimpan);
            panel2.Controls.Add(btnKeluar);
            panel2.Controls.Add(panel7);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(598, 70);
            panel2.TabIndex = 2;
            // 
            // btnSimpan
            // 
            btnSimpan.BackColor = Color.FromArgb(30, 31, 68);
            btnSimpan.Cursor = Cursors.Hand;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnSimpan.ForeColor = Color.WhiteSmoke;
            btnSimpan.IconChar = IconChar.CheckCircle;
            btnSimpan.IconColor = Color.WhiteSmoke;
            btnSimpan.IconFont = IconFont.Auto;
            btnSimpan.IconSize = 25;
            btnSimpan.Location = new Point(479, 21);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(109, 30);
            btnSimpan.TabIndex = 24;
            btnSimpan.Text = "Konfirmasi";
            btnSimpan.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSimpan.UseVisualStyleBackColor = false;
            btnSimpan.Click += btnSimpan_Click;
            // 
            // btnKeluar
            // 
            btnKeluar.BackColor = Color.WhiteSmoke;
            btnKeluar.Cursor = Cursors.Hand;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.Flip = FlipOrientation.Horizontal;
            btnKeluar.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnKeluar.ForeColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconChar = IconChar.CircleChevronRight;
            btnKeluar.IconColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconFont = IconFont.Auto;
            btnKeluar.IconSize = 25;
            btnKeluar.Location = new Point(12, 21);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(88, 30);
            btnKeluar.TabIndex = 23;
            btnKeluar.Text = "Keluar";
            btnKeluar.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += btnKeluar_Click;
            // 
            // panel7
            // 
            panel7.Location = new Point(3, 70);
            panel7.Name = "panel7";
            panel7.Size = new Size(585, 10);
            panel7.TabIndex = 6;
            // 
            // btnTunai
            // 
            btnTunai.BackColor = SystemColors.ControlDark;
            btnTunai.FlatStyle = FlatStyle.Flat;
            btnTunai.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            btnTunai.ForeColor = Color.White;
            btnTunai.Location = new Point(8, 23);
            btnTunai.Name = "btnTunai";
            btnTunai.Size = new Size(270, 36);
            btnTunai.TabIndex = 2;
            btnTunai.Text = "Tunai";
            btnTunai.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(4, 5);
            label2.Name = "label2";
            label2.Size = new Size(110, 15);
            label2.TabIndex = 1;
            label2.Text = "TIPE PEMBAYARAN";
            // 
            // Offline_payForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 530);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximumSize = new Size(600, 530);
            Name = "Offline_payForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "create1";
            TopMost = true;
            panel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            panel10.ResumeLayout(false);
            panel10.PerformLayout();
            panel11.ResumeLayout(false);
            panel11.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            panel12.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel14.ResumeLayout(false);
            panel14.PerformLayout();
            panel13.ResumeLayout(false);
            panel13.PerformLayout();
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panel1;
        private Panel panel2;
        private Panel panel7;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel9;
        private Panel panel10;
        private TextBox txtSeat;
        private Label label5;
        private Panel panel11;
        private TextBox txtNama;
        private Label label4;
        private Panel panel3;
        private Panel panel5;
        private TextBox txtKuantitas;
        private Label label1;
        private Button btnTunai;
        private Label label2;
        private TextBox txtCash;
        private Panel panel6;
        private Panel panel12;
        private ComboBox cmbPayform;
        private Label label6;
        private Label txtJumlahPembayaran;
        private Button btnSetPrice1;
        private Button btnSetPrice2;
        private Button btnSetPrice3;
        private FontAwesome.Sharp.IconButton btnKeluar;
        private FontAwesome.Sharp.IconButton btnSimpan;
        private Label label3;
        private Panel panel4;
        private Model.SButton sButton1;
        private Label label7;
        private IconButton btnDataMember;
        private Panel panel14;
        private Label lblHPMember;
        private Panel panel13;
        private Label lblEmailMember;
        private Panel panel8;
        private Label lblNamaMember;
        private Label lblPoint;
        private Label lblKembalian;
        private Label lblUsePoint;
        private Model.SButton ButtonSwitchUsePoint;
    }
}