
using FontAwesome.Sharp;
namespace KASIR.komponen
{
    partial class saveBill
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
            panel7 = new Panel();
            panel2 = new Panel();
            btnKeluar = new IconButton();
            btnSimpan = new IconButton();
            label4 = new Label();
            txtNama = new TextBox();
            label5 = new Label();
            panel11 = new Panel();
            txtSeat = new TextBox();
            panel10 = new Panel();
            panel9 = new Panel();
            panel1 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnTunai = new Button();
            panel2.SuspendLayout();
            panel11.SuspendLayout();
            panel10.SuspendLayout();
            panel9.SuspendLayout();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel7
            // 
            panel7.Location = new Point(3, 70);
            panel7.Name = "panel7";
            panel7.Size = new Size(585, 10);
            panel7.TabIndex = 6;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(btnKeluar);
            panel2.Controls.Add(panel7);
            panel2.Controls.Add(btnSimpan);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(598, 70);
            panel2.TabIndex = 2;
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
            btnKeluar.Location = new Point(11, 22);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(100, 30);
            btnKeluar.TabIndex = 26;
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
            btnSimpan.IconChar = IconChar.CheckCircle;
            btnSimpan.IconColor = Color.WhiteSmoke;
            btnSimpan.IconFont = IconFont.Auto;
            btnSimpan.IconSize = 25;
            btnSimpan.Location = new Point(485, 22);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(100, 30);
            btnSimpan.TabIndex = 27;
            btnSimpan.Text = "Simpan";
            btnSimpan.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSimpan.UseVisualStyleBackColor = false;
            btnSimpan.Click += btnSimpan_Click;
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
            // txtNama
            // 
            txtNama.BorderStyle = BorderStyle.None;
            txtNama.Location = new Point(11, 9);
            txtNama.Name = "txtNama";
            txtNama.PlaceholderText = "Masukan Nama ...";
            txtNama.Size = new Size(246, 16);
            txtNama.TabIndex = 0;
            txtNama.Click += txtNama_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(5, 66);
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
            panel11.Size = new Size(576, 36);
            panel11.TabIndex = 2;
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
            // panel10
            // 
            panel10.BorderStyle = BorderStyle.FixedSingle;
            panel10.Controls.Add(txtSeat);
            panel10.Location = new Point(8, 84);
            panel10.Name = "panel10";
            panel10.Size = new Size(576, 36);
            panel10.TabIndex = 4;
            // 
            // panel9
            // 
            panel9.Controls.Add(panel10);
            panel9.Controls.Add(label5);
            panel9.Controls.Add(panel11);
            panel9.Controls.Add(label4);
            panel9.Location = new Point(3, 3);
            panel9.Name = "panel9";
            panel9.Size = new Size(597, 133);
            panel9.TabIndex = 4;
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
            panel1.Size = new Size(600, 600);
            panel1.TabIndex = 4;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.BackColor = Color.White;
            flowLayoutPanel1.Controls.Add(panel9);
            flowLayoutPanel1.Location = new Point(0, 70);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(598, 525);
            flowLayoutPanel1.TabIndex = 3;
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
            btnTunai.TabIndex = 5;
            btnTunai.Text = "Tunai";
            btnTunai.UseVisualStyleBackColor = false;
            // 
            // saveBill
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 600);
            Controls.Add(panel1);
            Controls.Add(btnTunai);
            FormBorderStyle = FormBorderStyle.None;
            Name = "saveBill";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "create1";
            TopMost = true;
            panel2.ResumeLayout(false);
            panel11.ResumeLayout(false);
            panel11.PerformLayout();
            panel10.ResumeLayout(false);
            panel10.PerformLayout();
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            panel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panel7;
        private Panel panel2;
        private Label label4;
        private TextBox txtNama;
        private Label label5;
        private Panel panel11;
        private TextBox txtSeat;
        private Panel panel10;
        private Panel panel9;
        private Panel panel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnTunai;
        private FontAwesome.Sharp.IconButton btnKeluar;
        private FontAwesome.Sharp.IconButton btnSimpan;
    }
}