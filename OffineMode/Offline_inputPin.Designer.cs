
using FontAwesome.Sharp;
namespace KASIR.OfflineMode
{
    partial class Offline_inputPin
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
            panel2 = new Panel();
            btnCetakStruk = new IconButton();
            btnKeluar = new IconButton();
            btnSimpan = new IconButton();
            panel3 = new Panel();
            textPin = new MaskedTextBox();
            lblKonfirmasi = new Label();
            panel1 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.Controls.Add(btnCetakStruk);
            panel2.Controls.Add(btnKeluar);
            panel2.Controls.Add(btnSimpan);
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(lblKonfirmasi);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(600, 141);
            panel2.TabIndex = 0;
            // 
            // btnCetakStruk
            // 
            btnCetakStruk.BackColor = Color.FromArgb(15, 90, 94);
            btnCetakStruk.Cursor = Cursors.Hand;
            btnCetakStruk.FlatAppearance.BorderSize = 0;
            btnCetakStruk.FlatStyle = FlatStyle.Flat;
            btnCetakStruk.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnCetakStruk.ForeColor = Color.WhiteSmoke;
            btnCetakStruk.IconChar = IconChar.Scroll;
            btnCetakStruk.IconColor = Color.WhiteSmoke;
            btnCetakStruk.IconFont = IconFont.Auto;
            btnCetakStruk.IconSize = 25;
            btnCetakStruk.Location = new Point(450, 12);
            btnCetakStruk.Name = "btnCetakStruk";
            btnCetakStruk.Size = new Size(109, 30);
            btnCetakStruk.TabIndex = 29;
            btnCetakStruk.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCetakStruk.UseVisualStyleBackColor = false;
            btnCetakStruk.Click += btnCetakStruk_Click;
            // 
            // btnKeluar
            // 
            btnKeluar.BackColor = Color.Transparent;
            btnKeluar.Cursor = Cursors.Hand;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.Flip = FlipOrientation.Horizontal;
            btnKeluar.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnKeluar.ForeColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconChar = IconChar.CircleChevronRight;
            btnKeluar.IconColor = Color.Black;
            btnKeluar.IconFont = IconFont.Auto;
            btnKeluar.IconSize = 25;
            btnKeluar.ImageAlign = ContentAlignment.MiddleLeft;
            btnKeluar.Location = new Point(38, 12);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(88, 30);
            btnKeluar.TabIndex = 26;
            btnKeluar.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += btnKeluar_Click;
            // 
            // btnSimpan
            // 
            btnSimpan.BackColor = Color.FromArgb(15, 90, 94);
            btnSimpan.Cursor = Cursors.Hand;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnSimpan.ForeColor = Color.WhiteSmoke;
            btnSimpan.IconChar = IconChar.CheckCircle;
            btnSimpan.IconColor = Color.WhiteSmoke;
            btnSimpan.IconFont = IconFont.Auto;
            btnSimpan.IconSize = 25;
            btnSimpan.ImageAlign = ContentAlignment.MiddleRight;
            btnSimpan.Location = new Point(38, 106);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(521, 30);
            btnSimpan.TabIndex = 27;
            btnSimpan.Text = "Konfirmasi";
            btnSimpan.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSimpan.UseVisualStyleBackColor = false;
            btnSimpan.Click += btnKonfirmasi_Click;
            // 
            // panel3
            // 
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(textPin);
            panel3.Location = new Point(38, 64);
            panel3.Name = "panel3";
            panel3.Size = new Size(521, 36);
            panel3.TabIndex = 16;
            // 
            // textPin
            // 
            textPin.BorderStyle = BorderStyle.None;
            textPin.Location = new Point(-1, 15);
            textPin.Name = "textPin";
            textPin.PasswordChar = '*';
            textPin.Size = new Size(521, 16);
            textPin.TabIndex = 5;
            textPin.Text = "@@@";
            textPin.TextAlign = HorizontalAlignment.Center;
            textPin.UseSystemPasswordChar = true;
            textPin.MaskInputRejected += texttPin_MaskInputRejected;
            // 
            // lblKonfirmasi
            // 
            lblKonfirmasi.AutoSize = true;
            lblKonfirmasi.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblKonfirmasi.Location = new Point(244, 20);
            lblKonfirmasi.Name = "lblKonfirmasi";
            lblKonfirmasi.Size = new Size(115, 15);
            lblKonfirmasi.TabIndex = 0;
            lblKonfirmasi.Text = "Masukkan Kode Pin";
            // 
            // panel1
            // 
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(flowLayoutPanel1);
            panel1.Controls.Add(panel2);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(600, 537);
            panel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = Color.White;
            flowLayoutPanel1.Location = new Point(38, 147);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(521, 378);
            flowLayoutPanel1.TabIndex = 35;
            // 
            // Offline_inputPin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(600, 537);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximumSize = new Size(600, 537);
            Name = "Offline_inputPin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "inputPin";
            TopMost = true;
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel2;
        private Label lblKonfirmasi;
        private Panel panel1;
        private Panel panel3;
        private FontAwesome.Sharp.IconButton btnKeluar;
        private FontAwesome.Sharp.IconButton btnSimpan;
        private FontAwesome.Sharp.IconButton btnCetakStruk;
        private MaskedTextBox textPin;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}