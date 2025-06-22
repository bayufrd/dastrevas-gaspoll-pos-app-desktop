
using FontAwesome.Sharp;
namespace KASIR.komponen
{
    partial class addMember
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
            panel3 = new Panel();
            txtEmail = new TextBox();
            label1 = new Label();
            panel10 = new Panel();
            txtPhone = new TextBox();
            label5 = new Label();
            panel11 = new Panel();
            txtNama = new TextBox();
            label4 = new Label();
            panel2 = new Panel();
            hapusButton = new IconButton();
            btnSimpan = new IconButton();
            btnKeluar = new IconButton();
            panel7 = new Panel();
            btnTunai = new Button();
            label2 = new Label();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            panel9.SuspendLayout();
            panel3.SuspendLayout();
            panel10.SuspendLayout();
            panel11.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(flowLayoutPanel1);
            panel1.Controls.Add(panel2);
            panel1.ForeColor = Color.SteelBlue;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(600, 227);
            panel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.BackColor = Color.White;
            flowLayoutPanel1.Controls.Add(panel9);
            flowLayoutPanel1.Location = new Point(0, 70);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(598, 152);
            flowLayoutPanel1.TabIndex = 3;
            // 
            // panel9
            // 
            panel9.Controls.Add(panel3);
            panel9.Controls.Add(label1);
            panel9.Controls.Add(panel10);
            panel9.Controls.Add(label5);
            panel9.Controls.Add(panel11);
            panel9.Controls.Add(label4);
            panel9.Location = new Point(3, 3);
            panel9.Name = "panel9";
            panel9.Size = new Size(597, 146);
            panel9.TabIndex = 4;
            // 
            // panel3
            // 
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(txtEmail);
            panel3.Location = new Point(8, 91);
            panel3.Name = "panel3";
            panel3.Size = new Size(270, 36);
            panel3.TabIndex = 4;
            // 
            // txtEmail
            // 
            txtEmail.BorderStyle = BorderStyle.None;
            txtEmail.Location = new Point(11, 9);
            txtEmail.Name = "txtEmail";
            txtEmail.PlaceholderText = "Masukan Alamat Email ...";
            txtEmail.Size = new Size(246, 16);
            txtEmail.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(5, 73);
            label1.Name = "label1";
            label1.Size = new Size(92, 15);
            label1.TabIndex = 3;
            label1.Text = "ALAMAT EMAIL";
            // 
            // panel10
            // 
            panel10.BorderStyle = BorderStyle.FixedSingle;
            panel10.Controls.Add(txtPhone);
            panel10.Location = new Point(313, 22);
            panel10.Name = "panel10";
            panel10.Size = new Size(271, 36);
            panel10.TabIndex = 4;
            // 
            // txtPhone
            // 
            txtPhone.BorderStyle = BorderStyle.None;
            txtPhone.Location = new Point(11, 9);
            txtPhone.Name = "txtPhone";
            txtPhone.PlaceholderText = "Masukan Nomo Handphone ...";
            txtPhone.Size = new Size(246, 16);
            txtPhone.TabIndex = 0;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(310, 4);
            label5.Name = "label5";
            label5.Size = new Size(130, 15);
            label5.TabIndex = 3;
            label5.Text = "NOMOR HANDPHONE";
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
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(hapusButton);
            panel2.Controls.Add(btnSimpan);
            panel2.Controls.Add(btnKeluar);
            panel2.Controls.Add(panel7);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(598, 70);
            panel2.TabIndex = 2;
            // 
            // hapusButton
            // 
            hapusButton.BackColor = Color.DarkRed;
            hapusButton.Cursor = Cursors.Hand;
            hapusButton.FlatAppearance.BorderSize = 0;
            hapusButton.FlatStyle = FlatStyle.Flat;
            hapusButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            hapusButton.ForeColor = Color.WhiteSmoke;
            hapusButton.IconChar = IconChar.UserAltSlash;
            hapusButton.IconColor = Color.WhiteSmoke;
            hapusButton.IconFont = IconFont.Auto;
            hapusButton.IconSize = 25;
            hapusButton.Location = new Point(364, 21);
            hapusButton.Name = "hapusButton";
            hapusButton.Size = new Size(109, 30);
            hapusButton.TabIndex = 26;
            hapusButton.Text = "Hapus";
            hapusButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            hapusButton.UseVisualStyleBackColor = false;
            hapusButton.Click += hapusButton_Click;
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
            btnSimpan.Text = "Tambah";
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
            // addMember
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 224);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "addMember";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "create1";
            TopMost = true;
            panel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel10.ResumeLayout(false);
            panel10.PerformLayout();
            panel11.ResumeLayout(false);
            panel11.PerformLayout();
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
        private TextBox txtPhone;
        private Label label5;
        private Panel panel11;
        private TextBox txtNama;
        private Label label4;
        private Button btnKurang;
        private Button btnTunai;
        private Label label2;
        private TextBox txtNotes;
        private FontAwesome.Sharp.IconButton btnKeluar;
        private FontAwesome.Sharp.IconButton btnSimpan;
        private Panel panel3;
        private TextBox txtEmail;
        private Label label1;
        private IconButton hapusButton;
    }
}