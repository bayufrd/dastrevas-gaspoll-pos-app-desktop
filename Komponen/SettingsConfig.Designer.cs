namespace KASIR.Komponen
{
    partial class SettingsConfig
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
            gradientPanel2 = new Model.GradientPanel();
            panel5 = new Panel();
            picThumbnail = new PictureBox();
            btnUpload = new FontAwesome.Sharp.IconButton();
            label4 = new Label();
            panel4 = new Panel();
            textBoxVersion = new TextBox();
            label3 = new Label();
            panel3 = new Panel();
            textBoxAPI = new TextBox();
            label2 = new Label();
            panel2 = new Panel();
            textBoxBaseAddress = new TextBox();
            label1 = new Label();
            panel1 = new Panel();
            textBoxID = new TextBox();
            lblVersionNow = new Label();
            Button2 = new FontAwesome.Sharp.IconButton();
            Button1 = new FontAwesome.Sharp.IconButton();
            gradientPanel2.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picThumbnail).BeginInit();
            panel4.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // gradientPanel2
            // 
            gradientPanel2.Angle = 90F;
            gradientPanel2.BottomColor = Color.White;
            gradientPanel2.Controls.Add(panel5);
            gradientPanel2.Controls.Add(panel4);
            gradientPanel2.Controls.Add(panel3);
            gradientPanel2.Controls.Add(panel2);
            gradientPanel2.Controls.Add(panel1);
            gradientPanel2.Controls.Add(Button2);
            gradientPanel2.Controls.Add(Button1);
            gradientPanel2.Dock = DockStyle.Fill;
            gradientPanel2.Location = new Point(0, 0);
            gradientPanel2.Name = "gradientPanel2";
            gradientPanel2.Size = new Size(645, 310);
            gradientPanel2.TabIndex = 3;
            gradientPanel2.TopColor = Color.GhostWhite;
            // 
            // panel5
            // 
            panel5.BackColor = Color.White;
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Controls.Add(picThumbnail);
            panel5.Controls.Add(btnUpload);
            panel5.Controls.Add(label4);
            panel5.Location = new Point(12, 204);
            panel5.Name = "panel5";
            panel5.Size = new Size(404, 97);
            panel5.TabIndex = 51;
            // 
            // picThumbnail
            // 
            picThumbnail.BackColor = Color.Transparent;
            picThumbnail.Cursor = Cursors.Help;
            picThumbnail.Location = new Point(108, 5);
            picThumbnail.Name = "picThumbnail";
            picThumbnail.Size = new Size(112, 87);
            picThumbnail.SizeMode = PictureBoxSizeMode.Zoom;
            picThumbnail.TabIndex = 20;
            picThumbnail.TabStop = false;
            // 
            // btnUpload
            // 
            btnUpload.BackColor = Color.WhiteSmoke;
            btnUpload.Cursor = Cursors.Hand;
            btnUpload.FlatAppearance.BorderSize = 0;
            btnUpload.FlatStyle = FlatStyle.Flat;
            btnUpload.Flip = FontAwesome.Sharp.FlipOrientation.Horizontal;
            btnUpload.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnUpload.ForeColor = Color.FromArgb(30, 31, 68);
            btnUpload.IconChar = FontAwesome.Sharp.IconChar.Upload;
            btnUpload.IconColor = Color.FromArgb(30, 31, 68);
            btnUpload.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnUpload.IconSize = 25;
            btnUpload.Location = new Point(226, 33);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(88, 30);
            btnUpload.TabIndex = 52;
            btnUpload.Text = "Upload";
            btnUpload.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnUpload.UseVisualStyleBackColor = false;
            btnUpload.Click += btnUpload_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(5, 8);
            label4.Name = "label4";
            label4.Size = new Size(76, 15);
            label4.TabIndex = 46;
            label4.Text = "Logo Outlet :";
            // 
            // panel4
            // 
            panel4.BackColor = Color.White;
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(textBoxVersion);
            panel4.Controls.Add(label3);
            panel4.Location = new Point(12, 165);
            panel4.Name = "panel4";
            panel4.Size = new Size(404, 33);
            panel4.TabIndex = 50;
            // 
            // textBoxVersion
            // 
            textBoxVersion.Location = new Point(108, 5);
            textBoxVersion.Name = "textBoxVersion";
            textBoxVersion.Size = new Size(291, 23);
            textBoxVersion.TabIndex = 47;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(5, 8);
            label3.Name = "label3";
            label3.Size = new Size(97, 15);
            label3.TabIndex = 46;
            label3.Text = "Version Address :";
            // 
            // panel3
            // 
            panel3.BackColor = Color.White;
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(textBoxAPI);
            panel3.Controls.Add(label2);
            panel3.Location = new Point(12, 126);
            panel3.Name = "panel3";
            panel3.Size = new Size(404, 33);
            panel3.TabIndex = 49;
            // 
            // textBoxAPI
            // 
            textBoxAPI.Location = new Point(108, 5);
            textBoxAPI.Name = "textBoxAPI";
            textBoxAPI.Size = new Size(291, 23);
            textBoxAPI.TabIndex = 47;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(5, 8);
            label2.Name = "label2";
            label2.Size = new Size(32, 15);
            label2.TabIndex = 46;
            label2.Text = "API :";
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(textBoxBaseAddress);
            panel2.Controls.Add(label1);
            panel2.Location = new Point(12, 87);
            panel2.Name = "panel2";
            panel2.Size = new Size(404, 33);
            panel2.TabIndex = 48;
            // 
            // textBoxBaseAddress
            // 
            textBoxBaseAddress.Location = new Point(108, 5);
            textBoxBaseAddress.Name = "textBoxBaseAddress";
            textBoxBaseAddress.Size = new Size(291, 23);
            textBoxBaseAddress.TabIndex = 47;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(5, 8);
            label1.Name = "label1";
            label1.Size = new Size(85, 15);
            label1.TabIndex = 46;
            label1.Text = "Base Address : ";
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(textBoxID);
            panel1.Controls.Add(lblVersionNow);
            panel1.Location = new Point(12, 48);
            panel1.Name = "panel1";
            panel1.Size = new Size(404, 33);
            panel1.TabIndex = 30;
            // 
            // textBoxID
            // 
            textBoxID.Location = new Point(108, 5);
            textBoxID.Name = "textBoxID";
            textBoxID.Size = new Size(291, 23);
            textBoxID.TabIndex = 47;
            // 
            // lblVersionNow
            // 
            lblVersionNow.AutoSize = true;
            lblVersionNow.BackColor = Color.Transparent;
            lblVersionNow.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblVersionNow.Location = new Point(5, 8);
            lblVersionNow.Name = "lblVersionNow";
            lblVersionNow.Size = new Size(26, 15);
            lblVersionNow.TabIndex = 46;
            lblVersionNow.Text = "ID :";
            // 
            // Button2
            // 
            Button2.BackColor = Color.DarkRed;
            Button2.Cursor = Cursors.Hand;
            Button2.FlatAppearance.BorderSize = 0;
            Button2.FlatStyle = FlatStyle.Flat;
            Button2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Button2.ForeColor = Color.WhiteSmoke;
            Button2.IconChar = FontAwesome.Sharp.IconChar.CheckCircle;
            Button2.IconColor = Color.WhiteSmoke;
            Button2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            Button2.IconSize = 25;
            Button2.Location = new Point(328, 12);
            Button2.Name = "Button2";
            Button2.Size = new Size(88, 30);
            Button2.TabIndex = 29;
            Button2.Text = "Simpan";
            Button2.TextImageRelation = TextImageRelation.ImageBeforeText;
            Button2.UseVisualStyleBackColor = false;
            Button2.Click += Button2_Click;
            // 
            // Button1
            // 
            Button1.BackColor = Color.WhiteSmoke;
            Button1.Cursor = Cursors.Hand;
            Button1.FlatAppearance.BorderSize = 0;
            Button1.FlatStyle = FlatStyle.Flat;
            Button1.Flip = FontAwesome.Sharp.FlipOrientation.Horizontal;
            Button1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Button1.ForeColor = Color.FromArgb(30, 31, 68);
            Button1.IconChar = FontAwesome.Sharp.IconChar.CircleChevronRight;
            Button1.IconColor = Color.FromArgb(30, 31, 68);
            Button1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            Button1.IconSize = 25;
            Button1.Location = new Point(12, 12);
            Button1.Name = "Button1";
            Button1.Size = new Size(88, 30);
            Button1.TabIndex = 28;
            Button1.Text = "Keluar";
            Button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            Button1.UseVisualStyleBackColor = false;
            Button1.Click += Button1_Click;
            // 
            // SettingsConfig
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(428, 310);
            ControlBox = false;
            Controls.Add(gradientPanel2);
            Name = "SettingsConfig";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DT-SettingsConfig";
            gradientPanel2.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picThumbnail).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Model.GradientPanel gradientPanel2;
        private FontAwesome.Sharp.IconButton Button2;
        private FontAwesome.Sharp.IconButton Button1;
        private Panel panel1;
        private Label lblVersionNow;
        private Panel panel2;
        private TextBox textBoxBaseAddress;
        private Label label1;
        private TextBox textBoxID;
        private Panel panel4;
        private TextBox textBoxVersion;
        private Label label3;
        private Panel panel3;
        private TextBox textBoxAPI;
        private Label label2;
        private Panel panel5;
        private FontAwesome.Sharp.IconButton btnUpload;
        private Label label4;
        private PictureBox picThumbnail;
    }
}