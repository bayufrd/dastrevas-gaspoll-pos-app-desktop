
using FontAwesome.Sharp;
namespace KASIR.OfflineMode
{
    partial class Offline_updatePerItemForm
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
            label2 = new Label();
            label1 = new Label();
            panel3 = new Panel();
            textPin = new MaskedTextBox();
            panel2 = new Panel();
            button1 = new IconButton();
            button2 = new IconButton();
            btnSimpan = new Button();
            btnKeluar = new Button();
            panel4 = new Panel();
            txtReason = new TextBox();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel4.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(panel4);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(panel3);
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(btnSimpan);
            panel1.Controls.Add(btnKeluar);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(493, 243);
            panel1.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(13, 144);
            label2.Name = "label2";
            label2.Size = new Size(42, 15);
            label2.TabIndex = 21;
            label2.Text = "Alasan";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(12, 73);
            label1.Name = "label1";
            label1.Size = new Size(24, 15);
            label1.TabIndex = 20;
            label1.Text = "Pin";
            // 
            // panel3
            // 
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(textPin);
            panel3.Location = new Point(12, 91);
            panel3.Name = "panel3";
            panel3.Size = new Size(461, 36);
            panel3.TabIndex = 19;
            // 
            // textPin
            // 
            textPin.BorderStyle = BorderStyle.None;
            textPin.Location = new Point(0, 9);
            textPin.Name = "textPin";
            textPin.PasswordChar = '*';
            textPin.Size = new Size(460, 16);
            textPin.TabIndex = 10;
            textPin.TextAlign = HorizontalAlignment.Center;
            textPin.UseSystemPasswordChar = true;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(button1);
            panel2.Controls.Add(button2);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(493, 70);
            panel2.TabIndex = 12;
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
            button2.Location = new Point(364, 12);
            button2.Name = "button2";
            button2.Size = new Size(109, 30);
            button2.TabIndex = 27;
            button2.Text = "Konfirmasi";
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
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(txtReason);
            panel4.Location = new Point(13, 162);
            panel4.Name = "panel4";
            panel4.Size = new Size(461, 67);
            panel4.TabIndex = 22;
            // 
            // txtReason
            // 
            txtReason.BorderStyle = BorderStyle.None;
            txtReason.Location = new Point(8, 8);
            txtReason.Multiline = true;
            txtReason.Name = "txtReason";
            txtReason.PlaceholderText = "Masukkan alasan ...";
            txtReason.Size = new Size(438, 54);
            txtReason.TabIndex = 2;
            // 
            // Offline_updatePerItemForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(493, 241);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Offline_updatePerItemForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "refund";
            TopMost = true;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
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
        private FontAwesome.Sharp.IconButton button1;
        private FontAwesome.Sharp.IconButton button2;
        private Label label1;
        private Panel panel3;
        private MaskedTextBox textPin;
        private Label label2;
        private Panel panel4;
        private TextBox txtReason;
    }
}