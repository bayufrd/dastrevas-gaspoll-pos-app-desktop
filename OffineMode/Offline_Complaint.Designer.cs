
using FontAwesome.Sharp;
namespace KASIR.OffineMode
{
    partial class Offline_Complaint
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
            LogPanel = new Panel();
            label6 = new Label();
            LoggerMsg = new Panel();
            panel5 = new Panel();
            lblNama = new TextBox();
            label2 = new Label();
            label5 = new Label();
            panel4 = new Panel();
            txtNotes = new TextBox();
            panel2 = new Panel();
            lblNameCart = new IconButton();
            button1 = new IconButton();
            button2 = new IconButton();
            btnSimpan = new Button();
            btnKeluar = new Button();
            panel1.SuspendLayout();
            LogPanel.SuspendLayout();
            panel5.SuspendLayout();
            panel4.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(LogPanel);
            panel1.Controls.Add(panel5);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(panel4);
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(btnSimpan);
            panel1.Controls.Add(btnKeluar);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(600, 530);
            panel1.TabIndex = 0;
            // 
            // LogPanel
            // 
            LogPanel.BackColor = SystemColors.Control;
            LogPanel.Controls.Add(label6);
            LogPanel.Controls.Add(LoggerMsg);
            LogPanel.ForeColor = SystemColors.Control;
            LogPanel.Location = new Point(12, 394);
            LogPanel.Name = "LogPanel";
            LogPanel.Size = new Size(576, 133);
            LogPanel.TabIndex = 109;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Font = new Font("Courier New", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label6.ForeColor = SystemColors.ActiveCaptionText;
            label6.Location = new Point(127, 8);
            label6.Name = "label6";
            label6.Size = new Size(322, 16);
            label6.TabIndex = 1;
            label6.Text = ">_ Log Aktifitas Aplikasi ( Auto dikirimkan )";
            // 
            // LoggerMsg
            // 
            LoggerMsg.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LoggerMsg.BackColor = Color.White;
            LoggerMsg.Location = new Point(8, 27);
            LoggerMsg.Name = "LoggerMsg";
            LoggerMsg.Size = new Size(560, 106);
            LoggerMsg.TabIndex = 0;
            // 
            // panel5
            // 
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Controls.Add(lblNama);
            panel5.Location = new Point(12, 105);
            panel5.Name = "panel5";
            panel5.Size = new Size(576, 36);
            panel5.TabIndex = 17;
            // 
            // lblNama
            // 
            lblNama.BorderStyle = BorderStyle.None;
            lblNama.Location = new Point(8, 3);
            lblNama.Multiline = true;
            lblNama.Name = "lblNama";
            lblNama.PlaceholderText = "Dastrevas (AutoFill)";
            lblNama.Size = new Size(559, 28);
            lblNama.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(9, 156);
            label2.Name = "label2";
            label2.Size = new Size(108, 15);
            label2.TabIndex = 16;
            label2.Text = "Deskripsi Masalah :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.Location = new Point(8, 77);
            label5.Name = "label5";
            label5.Size = new Size(73, 15);
            label5.TabIndex = 15;
            label5.Text = "Nama Kasir :";
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(txtNotes);
            panel4.Location = new Point(12, 183);
            panel4.Name = "panel4";
            panel4.Size = new Size(576, 205);
            panel4.TabIndex = 14;
            // 
            // txtNotes
            // 
            txtNotes.BorderStyle = BorderStyle.None;
            txtNotes.Location = new Point(8, 7);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.PlaceholderText = "Deskripsi masalah";
            txtNotes.Size = new Size(559, 182);
            txtNotes.TabIndex = 2;
            txtNotes.TextChanged += txtNotes_TextChanged;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(lblNameCart);
            panel2.Controls.Add(button1);
            panel2.Controls.Add(button2);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(600, 70);
            panel2.TabIndex = 12;
            // 
            // lblNameCart
            // 
            lblNameCart.BackColor = Color.White;
            lblNameCart.Cursor = Cursors.Cross;
            lblNameCart.FlatAppearance.BorderSize = 0;
            lblNameCart.FlatStyle = FlatStyle.Flat;
            lblNameCart.Font = new Font("Segoe UI", 10F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            lblNameCart.ForeColor = Color.FromArgb(30, 31, 68);
            lblNameCart.IconChar = IconChar.Heart;
            lblNameCart.IconColor = Color.FromArgb(30, 31, 68);
            lblNameCart.IconFont = IconFont.Auto;
            lblNameCart.IconSize = 25;
            lblNameCart.ImageAlign = ContentAlignment.MiddleRight;
            lblNameCart.Location = new Point(106, 12);
            lblNameCart.Name = "lblNameCart";
            lblNameCart.Size = new Size(367, 30);
            lblNameCart.TabIndex = 28;
            lblNameCart.Text = "Contact IT Support";
            lblNameCart.TextImageRelation = TextImageRelation.ImageBeforeText;
            lblNameCart.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            button1.BackColor = Color.WhiteSmoke;
            button1.Cursor = Cursors.Hand;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Flip = FlipOrientation.Horizontal;
            button1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            button1.ForeColor = Color.FromArgb(30, 31, 68);
            button1.IconChar = IconChar.CircleChevronRight;
            button1.IconColor = Color.FromArgb(30, 31, 68);
            button1.IconFont = IconFont.Auto;
            button1.IconSize = 25;
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(88, 30);
            button1.TabIndex = 26;
            button1.Text = "Keluar";
            button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.BackColor = Color.FromArgb(30, 31, 68);
            button2.Cursor = Cursors.Hand;
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            button2.ForeColor = Color.WhiteSmoke;
            button2.IconChar = IconChar.CheckCircle;
            button2.IconColor = Color.WhiteSmoke;
            button2.IconFont = IconFont.Auto;
            button2.IconSize = 25;
            button2.Location = new Point(479, 12);
            button2.Name = "button2";
            button2.Size = new Size(109, 30);
            button2.TabIndex = 27;
            button2.Text = "Kirim";
            button2.TextImageRelation = TextImageRelation.ImageBeforeText;
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // btnSimpan
            // 
            btnSimpan.BackColor = Color.SteelBlue;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.ForeColor = Color.White;
            btnSimpan.Location = new Point(500, 12);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(88, 29);
            btnSimpan.TabIndex = 10;
            btnSimpan.Text = "Refund";
            btnSimpan.UseVisualStyleBackColor = false;
            // 
            // btnKeluar
            // 
            btnKeluar.BackColor = Color.WhiteSmoke;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.ForeColor = Color.SteelBlue;
            btnKeluar.Location = new Point(12, 12);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(88, 29);
            btnKeluar.TabIndex = 9;
            btnKeluar.Text = "Batal";
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += btnKeluar_Click;
            // 
            // Offline_Complaint
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(600, 530);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximumSize = new Size(600, 530);
            Name = "Offline_Complaint";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "refund";
            TopMost = true;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            LogPanel.ResumeLayout(false);
            LogPanel.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void txtSelesaiShift_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            // Allow only numbers and colon (:)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ':')
            {
                e.Handled = true;
            }

            // Automatically insert colon after the second digit for the hour
            if (char.IsDigit(e.KeyChar) && textBox.TextLength == 2 && !textBox.Text.Contains(':'))
            {
                textBox.Text += ":";
                textBox.SelectionStart = textBox.Text.Length; // Place cursor at the end
            }

            // Prevent typing after 5 characters (HH:mm format)
            if (textBox.Text.Length >= 5 && e.KeyChar != '\b') // '\b' is Backspace key
            {
                e.Handled = true;
            }
        }


        private void txtMulaiShift_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            // Allow only numbers and colon (:)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ':')
            {
                e.Handled = true;
            }

            // Allow a colon only if no colon exists, and the length is 2 or 3 (for HH:mm format)
            if (e.KeyChar == ':' && (textBox.Text.IndexOf(':') > -1 || textBox.Text.Length >= 3))
            {
                e.Handled = true;
            }
        }


        #endregion

        private Panel panel1;
        private Button btnKeluar;
        private Button btnSimpan;
        private Panel panel2;
        private Panel panel4;
        private Label label2;
        private Label label5;
        private Panel panel5;
        private TextBox txtNotes;
        private FontAwesome.Sharp.IconButton button1;
        private FontAwesome.Sharp.IconButton button2;
        private TextBox lblNama;
        private IconButton lblNameCart;
        private Panel LogPanel;
        private Label label6;
        private Panel LoggerMsg;
    }
}