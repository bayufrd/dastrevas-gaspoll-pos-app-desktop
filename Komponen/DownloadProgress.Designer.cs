namespace KASIR.Komponen
{
    partial class DownloadProgress
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
            labelStatus = new Label();
            progressBar = new ProgressBar();
            lbDownload = new Label();
            progressBarlucu = new ProgressBar();
            intall = new Label();
            SuspendLayout();
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelStatus.Location = new Point(126, 9);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(86, 15);
            labelStatus.TabIndex = 35;
            labelStatus.Text = "Processing ... 0";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 27);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(333, 23);
            progressBar.TabIndex = 36;
            // 
            // lbDownload
            // 
            lbDownload.AutoSize = true;
            lbDownload.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbDownload.Location = new Point(12, 61);
            lbDownload.Name = "lbDownload";
            lbDownload.Size = new Size(91, 15);
            lbDownload.TabIndex = 37;
            lbDownload.Text = "Current Version:";
            // 
            // progressBarlucu
            // 
            progressBarlucu.Location = new Point(12, 95);
            progressBarlucu.Name = "progressBarlucu";
            progressBarlucu.Size = new Size(333, 23);
            progressBarlucu.TabIndex = 38;
            // 
            // intall
            // 
            intall.AutoSize = true;
            intall.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            intall.Location = new Point(126, 95);
            intall.Name = "intall";
            intall.Size = new Size(86, 15);
            intall.TabIndex = 39;
            intall.Text = "Processing ... 0";
            // 
            // DownloadProgress
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(357, 138);
            Controls.Add(intall);
            Controls.Add(progressBarlucu);
            Controls.Add(lbDownload);
            Controls.Add(progressBar);
            Controls.Add(labelStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DownloadProgress";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Downloading";
            ResumeLayout(false);
            PerformLayout();
        }

        private void BtnCancel_Click1(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

#endregion
        private Label labelStatus;
        private ProgressBar progressBar;
        private Label lbDownload;
        private ProgressBar progressBarlucu;
        private Label intall;
    }
}