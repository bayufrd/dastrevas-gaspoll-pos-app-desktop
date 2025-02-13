
using FontAwesome.Sharp;
namespace KASIR.OfflineMode
{
    partial class Offline_deletePerItemForm
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
            panel4 = new Panel();
            txtReason = new TextBox();
            panel2 = new Panel();
            Button2 = new IconButton();
            Button1 = new IconButton();
            btnSimpan = new Button();
            btnKeluar = new Button();
            panel3 = new Panel();
            textPin = new MaskedTextBox();
            label1 = new Label();
            panel1.SuspendLayout();
            panel4.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Controls.Add(panel3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(panel4);
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(btnSimpan);
            panel1.Controls.Add(btnKeluar);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(493, 294);
            panel1.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(12, 131);
            label2.Name = "label2";
            label2.Size = new Size(42, 15);
            label2.TabIndex = 16;
            label2.Text = "Alasan";
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(txtReason);
            panel4.Location = new Point(12, 149);
            panel4.Name = "panel4";
            panel4.Size = new Size(461, 133);
            panel4.TabIndex = 14;
            // 
            // txtReason
            // 
            txtReason.BorderStyle = BorderStyle.None;
            txtReason.Location = new Point(8, 8);
            txtReason.Multiline = true;
            txtReason.Name = "txtReason";
            txtReason.PlaceholderText = "Masukan alasan hapus ...";
            txtReason.Size = new Size(438, 109);
            txtReason.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(Button2);
            panel2.Controls.Add(Button1);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(493, 70);
            panel2.TabIndex = 12;
            // 
            // Button2
            // 
            Button2.BackColor = Color.DarkRed;
            Button2.Cursor = Cursors.Hand;
            Button2.FlatAppearance.BorderSize = 0;
            Button2.FlatStyle = FlatStyle.Flat;
            Button2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Button2.ForeColor = Color.WhiteSmoke;
            Button2.IconChar = IconChar.TrashRestore;
            Button2.IconColor = Color.WhiteSmoke;
            Button2.IconFont = IconFont.Auto;
            Button2.IconSize = 25;
            Button2.Location = new Point(385, 21);
            Button2.Name = "Button2";
            Button2.Size = new Size(88, 30);
            Button2.TabIndex = 27;
            Button2.Text = "Hapus";
            Button2.TextImageRelation = TextImageRelation.ImageBeforeText;
            Button2.UseVisualStyleBackColor = false;
            Button2.Click += button2_Click;
            // 
            // Button1
            // 
            Button1.BackColor = Color.WhiteSmoke;
            Button1.Cursor = Cursors.Hand;
            Button1.FlatAppearance.BorderSize = 0;
            Button1.FlatStyle = FlatStyle.Flat;
            Button1.Flip = FlipOrientation.Horizontal;
            Button1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Button1.ForeColor = Color.FromArgb(30, 31, 68);
            Button1.IconChar = IconChar.CircleChevronRight;
            Button1.IconColor = Color.FromArgb(30, 31, 68);
            Button1.IconFont = IconFont.Auto;
            Button1.IconSize = 25;
            Button1.Location = new Point(12, 21);
            Button1.Name = "Button1";
            Button1.Size = new Size(88, 30);
            Button1.TabIndex = 26;
            Button1.Text = "Keluar";
            Button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            Button1.UseVisualStyleBackColor = false;
            Button1.Click += button1_Click;
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
            // panel3
            // 
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(textPin);
            panel3.Location = new Point(12, 92);
            panel3.Name = "panel3";
            panel3.Size = new Size(461, 36);
            panel3.TabIndex = 17;
            // 
            // textPin
            // 
            textPin.BorderStyle = BorderStyle.None;
            textPin.Location = new Point(0, 15);
            textPin.Name = "textPin";
            textPin.PasswordChar = '*';
            textPin.Size = new Size(460, 16);
            textPin.TabIndex = 5;
            textPin.TextAlign = HorizontalAlignment.Center;
            textPin.UseSystemPasswordChar = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(12, 74);
            label1.Name = "label1";
            label1.Size = new Size(24, 15);
            label1.TabIndex = 18;
            label1.Text = "Pin";
            // 
            // deletePerItemForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(493, 294);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "deletePerItemForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "refund";
            TopMost = true;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
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
        private TextBox txtReason;
        private FontAwesome.Sharp.IconButton Button2;
        private FontAwesome.Sharp.IconButton Button1;
        private Panel panel3;
        private MaskedTextBox textPin;
        private Label label1;
    }
}