namespace KASIR.Komponen
{
    partial class Check_For_Update
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
            btnUpdate = new FontAwesome.Sharp.IconButton();
            btnKeluar = new FontAwesome.Sharp.IconButton();
            lbnew = new Label();
            labelnew = new Label();
            lbheader = new Label();
            lbcurrent = new Label();
            label1 = new Label();
            bw_updatechecker = new System.ComponentModel.BackgroundWorker();
            SuspendLayout();
            // 
            // btnUpdate
            // 
            btnUpdate.BackColor = Color.FromArgb(31, 30, 68);
            btnUpdate.Cursor = Cursors.Hand;
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnUpdate.ForeColor = Color.WhiteSmoke;
            btnUpdate.IconChar = FontAwesome.Sharp.IconChar.Upload;
            btnUpdate.IconColor = Color.WhiteSmoke;
            btnUpdate.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnUpdate.IconSize = 25;
            btnUpdate.Location = new Point(101, 200);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(88, 30);
            btnUpdate.TabIndex = 29;
            btnUpdate.Text = "Update";
            btnUpdate.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnUpdate.UseVisualStyleBackColor = false;
            // 
            // btnKeluar
            // 
            btnKeluar.BackColor = Color.WhiteSmoke;
            btnKeluar.Cursor = Cursors.Hand;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.Flip = FontAwesome.Sharp.FlipOrientation.Horizontal;
            btnKeluar.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnKeluar.ForeColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconChar = FontAwesome.Sharp.IconChar.CircleChevronRight;
            btnKeluar.IconColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnKeluar.IconSize = 25;
            btnKeluar.Location = new Point(21, 12);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(88, 30);
            btnKeluar.TabIndex = 28;
            btnKeluar.Text = "Keluar";
            btnKeluar.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += BtnKeluar_Click;
            // 
            // lbnew
            // 
            lbnew.AutoSize = true;
            lbnew.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbnew.Location = new Point(206, 158);
            lbnew.Name = "lbnew";
            lbnew.Size = new Size(39, 15);
            lbnew.TabIndex = 30;
            lbnew.Text = "lbnew";
            // 
            // labelnew
            // 
            labelnew.AutoSize = true;
            labelnew.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelnew.Location = new Point(21, 158);
            labelnew.Name = "labelnew";
            labelnew.Size = new Size(127, 15);
            labelnew.TabIndex = 31;
            labelnew.Text = "New Version Available:";
            // 
            // lbheader
            // 
            lbheader.AutoSize = true;
            lbheader.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lbheader.Location = new Point(21, 56);
            lbheader.Name = "lbheader";
            lbheader.Size = new Size(189, 21);
            lbheader.TabIndex = 32;
            lbheader.Text = "The version is up to date";
            // 
            // lbcurrent
            // 
            lbcurrent.AutoSize = true;
            lbcurrent.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbcurrent.Location = new Point(206, 121);
            lbcurrent.Name = "lbcurrent";
            lbcurrent.Size = new Size(37, 15);
            lbcurrent.TabIndex = 33;
            lbcurrent.Text = "label1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(21, 121);
            label1.Name = "label1";
            label1.Size = new Size(91, 15);
            label1.TabIndex = 34;
            label1.Text = "Current Version:";
            // 
            // bw_updatechecker
            // 
            bw_updatechecker.DoWork += bw_updatechecker_DoWork;
            bw_updatechecker.RunWorkerCompleted += bw_updatechecker_RunWorkerCompleted;
            // 
            // Check_For_Update
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(298, 281);
            ControlBox = false;
            Controls.Add(label1);
            Controls.Add(lbcurrent);
            Controls.Add(lbheader);
            Controls.Add(labelnew);
            Controls.Add(lbnew);
            Controls.Add(btnUpdate);
            Controls.Add(btnKeluar);
            Name = "Check_For_Update";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Check For Update";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FontAwesome.Sharp.IconButton btnUpdate;
        private FontAwesome.Sharp.IconButton btnKeluar;
        private Label lbnew;
        private Label labelnew;
        private Label lbheader;
        private Label lbcurrent;
        private Label label1;
        private System.ComponentModel.BackgroundWorker bw_updatechecker;
    }
}