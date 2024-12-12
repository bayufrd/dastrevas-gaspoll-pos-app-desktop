namespace KASIR.komponen
{
    partial class cicilRefund
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
            btnKeluar = new Button();
            btnSimpan = new Button();
            panel7 = new Panel();
            panel2 = new Panel();
            label4 = new Label();
            txtJumlahCicil = new TextBox();
            panel11 = new Panel();
            panel9 = new Panel();
            txtSudahBayar = new Label();
            txtBelumDibayar = new Label();
            txtTotalCart = new Label();
            panel1 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnTunai = new Button();
            panel2.SuspendLayout();
            panel11.SuspendLayout();
            panel9.SuspendLayout();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
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
            btnKeluar.Click += btnKeluar_Click;
            // 
            // btnSimpan
            // 
            btnSimpan.BackColor = Color.SteelBlue;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.ForeColor = Color.White;
            btnSimpan.Location = new Point(9, 134);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(575, 29);
            btnSimpan.TabIndex = 7;
            btnSimpan.Text = "Konfirmasi";
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
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(btnKeluar);
            panel2.Controls.Add(panel7);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(598, 70);
            panel2.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(5, 72);
            label4.Name = "label4";
            label4.Size = new Size(46, 15);
            label4.TabIndex = 1;
            label4.Text = "Jumlah";
            // 
            // txtJumlahCicil
            // 
            txtJumlahCicil.BorderStyle = BorderStyle.None;
            txtJumlahCicil.Location = new Point(11, 9);
            txtJumlahCicil.Name = "txtJumlahCicil";
            txtJumlahCicil.PlaceholderText = "Masukan jumlah ...";
            txtJumlahCicil.Size = new Size(546, 16);
            txtJumlahCicil.TabIndex = 0;
            // 
            // panel11
            // 
            panel11.BorderStyle = BorderStyle.FixedSingle;
            panel11.Controls.Add(txtJumlahCicil);
            panel11.Location = new Point(9, 92);
            panel11.Name = "panel11";
            panel11.Size = new Size(576, 36);
            panel11.TabIndex = 2;
            // 
            // panel9
            // 
            panel9.Controls.Add(txtSudahBayar);
            panel9.Controls.Add(btnSimpan);
            panel9.Controls.Add(txtBelumDibayar);
            panel9.Controls.Add(txtTotalCart);
            panel9.Controls.Add(panel11);
            panel9.Controls.Add(label4);
            panel9.Location = new Point(3, 3);
            panel9.Name = "panel9";
            panel9.Size = new Size(597, 177);
            panel9.TabIndex = 4;
            // 
            // txtSudahBayar
            // 
            txtSudahBayar.AutoSize = true;
            txtSudahBayar.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            txtSudahBayar.ForeColor = Color.Black;
            txtSudahBayar.Location = new Point(7, 47);
            txtSudahBayar.Name = "txtSudahBayar";
            txtSudahBayar.Size = new Size(68, 15);
            txtSudahBayar.TabIndex = 5;
            txtSudahBayar.Text = "sudahBayar";
            // 
            // txtBelumDibayar
            // 
            txtBelumDibayar.AutoSize = true;
            txtBelumDibayar.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            txtBelumDibayar.ForeColor = Color.Black;
            txtBelumDibayar.Location = new Point(7, 29);
            txtBelumDibayar.Name = "txtBelumDibayar";
            txtBelumDibayar.Size = new Size(70, 15);
            txtBelumDibayar.TabIndex = 4;
            txtBelumDibayar.Text = "belumBayar";
            // 
            // txtTotalCart
            // 
            txtTotalCart.AutoSize = true;
            txtTotalCart.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            txtTotalCart.ForeColor = Color.Black;
            txtTotalCart.Location = new Point(7, 10);
            txtTotalCart.Name = "txtTotalCart";
            txtTotalCart.Size = new Size(52, 15);
            txtTotalCart.TabIndex = 3;
            txtTotalCart.Text = "totalCart";
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
            // cicilRefund
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 600);
            Controls.Add(panel1);
            Controls.Add(btnTunai);
            FormBorderStyle = FormBorderStyle.None;
            Name = "cicilRefund";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "create1";
            TopMost = true;
            panel2.ResumeLayout(false);
            panel11.ResumeLayout(false);
            panel11.PerformLayout();
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            panel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button btnKeluar;
        private Button btnSimpan;
        private Panel panel7;
        private Panel panel2;
        private Label label4;
        private TextBox txtJumlahCicil;
        private Panel panel11;
        private Panel panel9;
        private Panel panel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnTunai;
        private Label txtTotalCart;
        private Label txtBelumDibayar;
        private Label txtSudahBayar;
    }
}