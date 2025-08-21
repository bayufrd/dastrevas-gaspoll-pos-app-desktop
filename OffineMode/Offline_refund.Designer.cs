
using FontAwesome.Sharp;
namespace KASIR.OfflineMode
{
    partial class Offline_refund
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
            lblDetailPayment = new Label();
            panel4 = new Panel();
            label1 = new Label();
            cmbRefundType = new ComboBox();
            panel12 = new Panel();
            label2 = new Label();
            cmbPayform = new ComboBox();
            panel13 = new Panel();
            panel3 = new Panel();
            lblCustomerName = new Label();
            panel2 = new Panel();
            btnRefund = new IconButton();
            button1 = new IconButton();
            panel6 = new Panel();
            txtNotes = new TextBox();
            label3 = new Label();
            btnSimpan = new Button();
            btnKeluar = new Button();
            panel1.SuspendLayout();
            panel4.SuspendLayout();
            panel12.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(lblDetailPayment);
            panel1.Controls.Add(panel4);
            panel1.Controls.Add(panel12);
            panel1.Controls.Add(panel13);
            panel1.Controls.Add(panel3);
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(panel6);
            panel1.Controls.Add(btnSimpan);
            panel1.Controls.Add(btnKeluar);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(600, 530);
            panel1.TabIndex = 0;
            // 
            // lblDetailPayment
            // 
            lblDetailPayment.AutoSize = true;
            lblDetailPayment.Location = new Point(12, 164);
            lblDetailPayment.Name = "lblDetailPayment";
            lblDetailPayment.Size = new Size(130, 15);
            lblDetailPayment.TabIndex = 1;
            lblDetailPayment.Text = "Payment sebelumnya : ";
            // 
            // panel4
            // 
            panel4.BackColor = SystemColors.ControlLightLight;
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(label1);
            panel4.Controls.Add(cmbRefundType);
            panel4.Location = new Point(8, 118);
            panel4.Name = "panel4";
            panel4.Size = new Size(584, 38);
            panel4.TabIndex = 18;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(3, 13);
            label1.Name = "label1";
            label1.Size = new Size(79, 15);
            label1.TabIndex = 1;
            label1.Text = "Type Refund :";
            // 
            // cmbRefundType
            // 
            cmbRefundType.BackColor = Color.White;
            cmbRefundType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRefundType.FlatStyle = FlatStyle.Flat;
            cmbRefundType.FormattingEnabled = true;
            cmbRefundType.Location = new Point(121, 5);
            cmbRefundType.Name = "cmbRefundType";
            cmbRefundType.Size = new Size(456, 23);
            cmbRefundType.TabIndex = 0;
            cmbRefundType.SelectedIndexChanged += cmbRefundType_SelectedIndexChanged;
            // 
            // panel12
            // 
            panel12.BackColor = SystemColors.ControlLightLight;
            panel12.BorderStyle = BorderStyle.FixedSingle;
            panel12.Controls.Add(label2);
            panel12.Controls.Add(cmbPayform);
            panel12.Location = new Point(8, 182);
            panel12.Name = "panel12";
            panel12.Size = new Size(584, 38);
            panel12.TabIndex = 17;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(3, 13);
            label2.Name = "label2";
            label2.Size = new Size(101, 15);
            label2.TabIndex = 2;
            label2.Text = "Payment Refund :";
            // 
            // cmbPayform
            // 
            cmbPayform.BackColor = Color.White;
            cmbPayform.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayform.FlatStyle = FlatStyle.Flat;
            cmbPayform.FormattingEnabled = true;
            cmbPayform.Location = new Point(121, 5);
            cmbPayform.Name = "cmbPayform";
            cmbPayform.Size = new Size(456, 23);
            cmbPayform.TabIndex = 0;
            // 
            // panel13
            // 
            panel13.AutoScroll = true;
            panel13.BorderStyle = BorderStyle.FixedSingle;
            panel13.Location = new Point(8, 226);
            panel13.Name = "panel13";
            panel13.Size = new Size(584, 177);
            panel13.TabIndex = 16;
            // 
            // panel3
            // 
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(lblCustomerName);
            panel3.Location = new Point(8, 76);
            panel3.Name = "panel3";
            panel3.Size = new Size(584, 36);
            panel3.TabIndex = 13;
            // 
            // lblCustomerName
            // 
            lblCustomerName.AutoSize = true;
            lblCustomerName.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblCustomerName.Location = new Point(3, 10);
            lblCustomerName.Name = "lblCustomerName";
            lblCustomerName.Size = new Size(93, 15);
            lblCustomerName.TabIndex = 0;
            lblCustomerName.Text = "Customer Name";
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(btnRefund);
            panel2.Controls.Add(button1);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(600, 70);
            panel2.TabIndex = 12;
            // 
            // btnRefund
            // 
            btnRefund.BackColor = Color.FromArgb(15, 90, 94);
            btnRefund.Cursor = Cursors.Hand;
            btnRefund.FlatAppearance.BorderSize = 0;
            btnRefund.FlatStyle = FlatStyle.Flat;
            btnRefund.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnRefund.ForeColor = Color.WhiteSmoke;
            btnRefund.IconChar = IconChar.MoneyCheckDollar;
            btnRefund.IconColor = Color.WhiteSmoke;
            btnRefund.IconFont = IconFont.Auto;
            btnRefund.IconSize = 25;
            btnRefund.Location = new Point(504, 20);
            btnRefund.Name = "btnRefund";
            btnRefund.Size = new Size(88, 30);
            btnRefund.TabIndex = 33;
            btnRefund.Text = "Refund";
            btnRefund.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnRefund.UseVisualStyleBackColor = false;
            btnRefund.Click += Refundbutton_Click;
            // 
            // button1
            // 
            button1.BackColor = Color.White;
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
            button1.ImageAlign = ContentAlignment.MiddleLeft;
            button1.Location = new Point(8, 20);
            button1.Name = "button1";
            button1.Size = new Size(88, 30);
            button1.TabIndex = 31;
            button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // panel6
            // 
            panel6.Controls.Add(txtNotes);
            panel6.Controls.Add(label3);
            panel6.Location = new Point(8, 409);
            panel6.Name = "panel6";
            panel6.Size = new Size(584, 100);
            panel6.TabIndex = 11;
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(2, 25);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.PlaceholderText = "Alasan refund ...";
            txtNotes.Size = new Size(575, 72);
            txtNotes.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(2, 5);
            label3.Name = "label3";
            label3.Size = new Size(53, 15);
            label3.TabIndex = 8;
            label3.Text = "REASON";
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
            // Offline_refund
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(600, 530);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximumSize = new Size(600, 530);
            Name = "Offline_refund";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "refund";
            TopMost = true;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel12.ResumeLayout(false);
            panel12.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnKeluar;
        private Button btnSimpan;
        private Panel panel6;
        private TextBox txtNotes;
        private Label label3;
        private Panel panel3;
        private Panel panel2;
        private Panel panel13;
        private Label lblCustomerName;
        private Panel panel12;
        private ComboBox cmbPayform;
        private Panel panel4;
        private ComboBox cmbRefundType;
        private FontAwesome.Sharp.IconButton button1;
        private FontAwesome.Sharp.IconButton btnRefund;
        private Label lblDetailPayment;
        private Label label1;
        private Label label2;
    }
}