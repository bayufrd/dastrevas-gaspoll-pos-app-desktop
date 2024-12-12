namespace KASIR.Komponen
{
    partial class KembalianForm
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
        /// 
        private Label labelKembalian;
        private Button buttonOK;
        private void InitializeComponent()
        {
            this.labelKembalian = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelKembalian
            // 
            this.labelKembalian.AutoSize = true;
            this.labelKembalian.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F); // Set font size to 24
            this.labelKembalian.Location = new System.Drawing.Point(12, 9);
            this.labelKembalian.MaximumSize = new System.Drawing.Size(500, 0);
            this.labelKembalian.Name = "labelKembalian";
            this.labelKembalian.Size = new System.Drawing.Size(170, 37);
            this.labelKembalian.TabIndex = 0;
            this.labelKembalian.Text = "label1";
            // 
            // buttonOK
            // 
            this.buttonOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F); // Set font size to 12
            this.buttonOK.Location = new System.Drawing.Point(160, 80);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 30);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // KembalianForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 120);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelKembalian);
            this.Name = "KembalianForm";
            this.Text = "Bayar Selesai";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}