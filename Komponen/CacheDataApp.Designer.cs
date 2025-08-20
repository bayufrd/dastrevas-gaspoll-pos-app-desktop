namespace KASIR.Komponen
{
    partial class CacheDataApp
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
            lblProgress = new FontAwesome.Sharp.IconButton();
            lblDetail = new FontAwesome.Sharp.IconButton();
            progressBar = new ProgressBar();
            lblTextInformation = new Label();
            gradientPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // gradientPanel2
            // 
            gradientPanel2.Angle = 90F;
            gradientPanel2.BottomColor = Color.Gray;
            gradientPanel2.Controls.Add(lblProgress);
            gradientPanel2.Controls.Add(lblDetail);
            gradientPanel2.Controls.Add(progressBar);
            gradientPanel2.Controls.Add(lblTextInformation);
            gradientPanel2.Dock = DockStyle.Fill;
            gradientPanel2.Location = new Point(0, 0);
            gradientPanel2.Name = "gradientPanel2";
            gradientPanel2.Size = new Size(307, 83);
            gradientPanel2.TabIndex = 3;
            gradientPanel2.TopColor = Color.Gainsboro;
            // 
            // lblProgress
            // 
            lblProgress.BackColor = Color.FromArgb(15, 90, 94);
            lblProgress.Cursor = Cursors.Hand;
            lblProgress.FlatAppearance.BorderSize = 0;
            lblProgress.FlatStyle = FlatStyle.Flat;
            lblProgress.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblProgress.ForeColor = Color.WhiteSmoke;
            lblProgress.IconChar = FontAwesome.Sharp.IconChar.Database;
            lblProgress.IconColor = Color.WhiteSmoke;
            lblProgress.IconFont = FontAwesome.Sharp.IconFont.Auto;
            lblProgress.IconSize = 20;
            lblProgress.ImageAlign = ContentAlignment.MiddleRight;
            lblProgress.Location = new Point(0, 30);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(307, 30);
            lblProgress.TabIndex = 52;
            lblProgress.Text = "lblProgress";
            lblProgress.TextImageRelation = TextImageRelation.ImageBeforeText;
            lblProgress.UseVisualStyleBackColor = false;
            // 
            // lblDetail
            // 
            lblDetail.BackColor = Color.FromArgb(15, 90, 94);
            lblDetail.Cursor = Cursors.Hand;
            lblDetail.FlatAppearance.BorderSize = 0;
            lblDetail.FlatStyle = FlatStyle.Flat;
            lblDetail.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblDetail.ForeColor = Color.WhiteSmoke;
            lblDetail.IconChar = FontAwesome.Sharp.IconChar.Upload;
            lblDetail.IconColor = Color.WhiteSmoke;
            lblDetail.IconFont = FontAwesome.Sharp.IconFont.Auto;
            lblDetail.IconSize = 20;
            lblDetail.ImageAlign = ContentAlignment.MiddleRight;
            lblDetail.Location = new Point(0, 0);
            lblDetail.Name = "lblDetail";
            lblDetail.Size = new Size(307, 30);
            lblDetail.TabIndex = 51;
            lblDetail.Text = "Progress";
            lblDetail.TextImageRelation = TextImageRelation.ImageBeforeText;
            lblDetail.UseVisualStyleBackColor = false;
            // 
            // progressBar
            // 
            progressBar.ForeColor = SystemColors.ControlText;
            progressBar.Location = new Point(0, 59);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(307, 25);
            progressBar.Step = 100;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 50;
            // 
            // lblTextInformation
            // 
            lblTextInformation.AutoSize = true;
            lblTextInformation.BackColor = Color.Transparent;
            lblTextInformation.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblTextInformation.ForeColor = Color.White;
            lblTextInformation.Location = new Point(12, 9);
            lblTextInformation.Name = "lblTextInformation";
            lblTextInformation.Size = new Size(0, 15);
            lblTextInformation.TabIndex = 48;
            // 
            // CacheDataApp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(307, 83);
            Controls.Add(gradientPanel2);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximumSize = new Size(323, 122);
            Name = "CacheDataApp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DT-Downloading...";
            gradientPanel2.ResumeLayout(false);
            gradientPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Model.GradientPanel gradientPanel2;
        private Label lblTextInformation;
        private ProgressBar progressBar;
        private FontAwesome.Sharp.IconButton lblDetail;
        private FontAwesome.Sharp.IconButton lblProgress;
    }
}