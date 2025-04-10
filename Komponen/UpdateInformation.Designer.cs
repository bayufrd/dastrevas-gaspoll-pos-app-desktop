namespace KASIR.Komponen
{
    partial class UpdateInformation
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
            lblTextInformation = new Label();
            gradientPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // gradientPanel2
            // 
            gradientPanel2.Angle = 90F;
            gradientPanel2.BottomColor = Color.Gray;
            gradientPanel2.Controls.Add(lblTextInformation);
            gradientPanel2.Dock = DockStyle.Fill;
            gradientPanel2.Location = new Point(0, 0);
            gradientPanel2.Name = "gradientPanel2";
            gradientPanel2.Size = new Size(584, 227);
            gradientPanel2.TabIndex = 2;
            gradientPanel2.TopColor = Color.Gainsboro;
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
            // UpdateInformation
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 227);
            Controls.Add(gradientPanel2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateInformation";
            Opacity = 0.8D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Update Information";
            gradientPanel2.ResumeLayout(false);
            gradientPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Model.GradientPanel gradientPanel2;
        private Label lblTextInformation;
    }
}