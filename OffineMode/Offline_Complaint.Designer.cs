
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            panel1 = new Panel();
            panelComplaint = new Model.GradientPanel();
            dataGridView1 = new DataGridView();
            label4 = new Label();
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
            panelComplaint.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel5.SuspendLayout();
            panel4.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(panelComplaint);
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
            // panelComplaint
            // 
            panelComplaint.Angle = 45F;
            panelComplaint.AutoScroll = true;
            panelComplaint.BorderStyle = BorderStyle.FixedSingle;
            panelComplaint.BottomColor = Color.Gainsboro;
            panelComplaint.Controls.Add(dataGridView1);
            panelComplaint.Controls.Add(label4);
            panelComplaint.Location = new Point(13, 394);
            panelComplaint.Name = "panelComplaint";
            panelComplaint.Size = new Size(575, 112);
            panelComplaint.TabIndex = 39;
            panelComplaint.TopColor = Color.White;
            panelComplaint.Visible = false;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.ForeColor = Color.Silver;
            dataGridViewCellStyle1.SelectionBackColor = Color.Transparent;
            dataGridViewCellStyle1.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.Transparent;
            dataGridViewCellStyle2.SelectionForeColor = Color.Transparent;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.ColumnHeadersHeight = 30;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = Color.White;
            dataGridViewCellStyle3.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle3.SelectionForeColor = Color.Gainsboro;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.FromArgb(31, 30, 68);
            dataGridView1.ImeMode = ImeMode.NoControl;
            dataGridView1.Location = new Point(3, 3);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.White;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle4.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle4.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle5.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle5.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dataGridView1.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowTemplate.DefaultCellStyle.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(567, 104);
            dataGridView1.TabIndex = 16;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(285, 1);
            label4.Name = "label4";
            label4.Size = new Size(0, 15);
            label4.TabIndex = 0;
            label4.TextAlign = ContentAlignment.TopCenter;
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
            label2.Size = new Size(61, 15);
            label2.TabIndex = 16;
            label2.Text = "Deskripsi :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.Location = new Point(8, 77);
            label5.Name = "label5";
            label5.Size = new Size(45, 15);
            label5.TabIndex = 15;
            label5.Text = "Nama :";
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
            panelComplaint.ResumeLayout(false);
            panelComplaint.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
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
        private Model.GradientPanel panelComplaint;
        private DataGridView dataGridView1;
        private Label label4;
        private TextBox lblNama;
        private IconButton lblNameCart;
    }
}