
using FontAwesome.Sharp;
namespace KASIR.OffineMode
{
    partial class Offline_saveBill
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
            panelButton = new Panel();
            btnKeluar = new IconButton();
            btnSimpan = new IconButton();
            panelForm = new Panel();
            panelEditUser = new Panel();
            panelSeat = new Panel();
            txtSeat = new TextBox();
            lblTitleSeat = new Label();
            panelNama = new Panel();
            txtNama = new TextBox();
            lblTitleNama = new Label();
            panelButton.SuspendLayout();
            panelForm.SuspendLayout();
            panelEditUser.SuspendLayout();
            panelSeat.SuspendLayout();
            panelNama.SuspendLayout();
            SuspendLayout();
            // 
            // panelButton
            // 
            panelButton.BackColor = SystemColors.Window;
            panelButton.Controls.Add(btnKeluar);
            panelButton.Controls.Add(btnSimpan);
            panelButton.Dock = DockStyle.Top;
            panelButton.Location = new Point(0, 0);
            panelButton.Name = "panelButton";
            panelButton.Size = new Size(598, 70);
            panelButton.TabIndex = 2;
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
            btnKeluar.Location = new Point(11, 22);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(100, 30);
            btnKeluar.TabIndex = 26;
            btnKeluar.TextAlign = ContentAlignment.MiddleLeft;
            btnKeluar.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += btnKeluar_Click;
            // 
            // btnSimpan
            // 
            btnSimpan.BackColor = Color.White;
            btnSimpan.Cursor = Cursors.Hand;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnSimpan.ForeColor = Color.White;
            btnSimpan.IconChar = IconChar.CheckCircle;
            btnSimpan.IconColor = Color.Black;
            btnSimpan.IconFont = IconFont.Auto;
            btnSimpan.IconSize = 25;
            btnSimpan.ImageAlign = ContentAlignment.MiddleRight;
            btnSimpan.Location = new Point(485, 22);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(100, 30);
            btnSimpan.TabIndex = 27;
            btnSimpan.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSimpan.UseVisualStyleBackColor = false;
            btnSimpan.Click += btnSimpan_Click;
            // 
            // panelForm
            // 
            panelForm.BackColor = Color.White;
            panelForm.BorderStyle = BorderStyle.FixedSingle;
            panelForm.Controls.Add(panelEditUser);
            panelForm.Controls.Add(panelButton);
            panelForm.Dock = DockStyle.Fill;
            panelForm.ForeColor = Color.SteelBlue;
            panelForm.Location = new Point(0, 0);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(600, 228);
            panelForm.TabIndex = 4;
            // 
            // panelEditUser
            // 
            panelEditUser.Controls.Add(panelSeat);
            panelEditUser.Controls.Add(lblTitleSeat);
            panelEditUser.Controls.Add(panelNama);
            panelEditUser.Controls.Add(lblTitleNama);
            panelEditUser.Location = new Point(0, 90);
            panelEditUser.Name = "panelEditUser";
            panelEditUser.Size = new Size(596, 133);
            panelEditUser.TabIndex = 4;
            // 
            // panelSeat
            // 
            panelSeat.BorderStyle = BorderStyle.FixedSingle;
            panelSeat.Controls.Add(txtSeat);
            panelSeat.Location = new Point(8, 84);
            panelSeat.Name = "panelSeat";
            panelSeat.Size = new Size(576, 36);
            panelSeat.TabIndex = 4;
            // 
            // txtSeat
            // 
            txtSeat.BorderStyle = BorderStyle.None;
            txtSeat.Location = new Point(11, 9);
            txtSeat.Name = "txtSeat";
            txtSeat.PlaceholderText = "Masukan Nomor Seat ...";
            txtSeat.Size = new Size(560, 16);
            txtSeat.TabIndex = 0;
            // 
            // lblTitleSeat
            // 
            lblTitleSeat.AutoSize = true;
            lblTitleSeat.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitleSeat.ForeColor = Color.Black;
            lblTitleSeat.Location = new Point(5, 66);
            lblTitleSeat.Name = "lblTitleSeat";
            lblTitleSeat.Size = new Size(85, 15);
            lblTitleSeat.TabIndex = 3;
            lblTitleSeat.Text = "NOMOR MEJA";
            // 
            // panelNama
            // 
            panelNama.BorderStyle = BorderStyle.FixedSingle;
            panelNama.Controls.Add(txtNama);
            panelNama.Location = new Point(8, 23);
            panelNama.Name = "panelNama";
            panelNama.Size = new Size(576, 36);
            panelNama.TabIndex = 2;
            // 
            // txtNama
            // 
            txtNama.BorderStyle = BorderStyle.None;
            txtNama.Location = new Point(11, 9);
            txtNama.Name = "txtNama";
            txtNama.PlaceholderText = "Masukan Nama ...";
            txtNama.Size = new Size(560, 16);
            txtNama.TabIndex = 0;
            txtNama.Click += txtNama_Click;
            // 
            // lblTitleNama
            // 
            lblTitleNama.AutoSize = true;
            lblTitleNama.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitleNama.ForeColor = Color.Black;
            lblTitleNama.Location = new Point(5, 5);
            lblTitleNama.Name = "lblTitleNama";
            lblTitleNama.Size = new Size(43, 15);
            lblTitleNama.TabIndex = 1;
            lblTitleNama.Text = "NAMA";
            // 
            // Offline_saveBill
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 228);
            Controls.Add(panelForm);
            FormBorderStyle = FormBorderStyle.None;
            MaximumSize = new Size(600, 600);
            Name = "Offline_saveBill";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "create1";
            TopMost = true;
            panelButton.ResumeLayout(false);
            panelForm.ResumeLayout(false);
            panelEditUser.ResumeLayout(false);
            panelEditUser.PerformLayout();
            panelSeat.ResumeLayout(false);
            panelSeat.PerformLayout();
            panelNama.ResumeLayout(false);
            panelNama.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panelButton;
        private Panel panelForm;
        private FontAwesome.Sharp.IconButton btnKeluar;
        private FontAwesome.Sharp.IconButton btnSimpan;
        private Panel panelEditUser;
        private Panel panelSeat;
        private TextBox txtSeat;
        private Label lblTitleSeat;
        private Panel panelNama;
        private TextBox txtNama;
        private Label lblTitleNama;
    }
}