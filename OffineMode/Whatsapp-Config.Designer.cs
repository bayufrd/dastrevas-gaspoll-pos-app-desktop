namespace KASIR.OffineMode
{
    partial class Whatsapp_Config
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
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            QRPictureBox = new Guna.UI2.WinForms.Guna2PictureBox();
            btnResetQR = new Guna.UI2.WinForms.Guna2Button();
            LogPanel = new Panel();
            label6 = new Label();
            LoggerMsg = new Panel();
            Button1 = new FontAwesome.Sharp.IconButton();
            btnReload = new FontAwesome.Sharp.IconButton();
            lblStatus = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)QRPictureBox).BeginInit();
            LogPanel.SuspendLayout();
            SuspendLayout();
            // 
            // QRPictureBox
            // 
            QRPictureBox.CustomizableEdges = customizableEdges1;
            QRPictureBox.ImageRotate = 0F;
            QRPictureBox.Location = new Point(99, 60);
            QRPictureBox.Name = "QRPictureBox";
            QRPictureBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            QRPictureBox.ShadowDecoration.Enabled = true;
            QRPictureBox.Size = new Size(200, 200);
            QRPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            QRPictureBox.TabIndex = 0;
            QRPictureBox.TabStop = false;
            // 
            // btnResetQR
            // 
            btnResetQR.BackColor = Color.Transparent;
            btnResetQR.BorderColor = Color.DarkRed;
            btnResetQR.BorderRadius = 8;
            btnResetQR.BorderThickness = 2;
            btnResetQR.Cursor = Cursors.Hand;
            btnResetQR.CustomizableEdges = customizableEdges3;
            btnResetQR.DisabledState.BorderColor = Color.DarkGray;
            btnResetQR.DisabledState.CustomBorderColor = Color.DarkGray;
            btnResetQR.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnResetQR.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnResetQR.FillColor = Color.DarkRed;
            btnResetQR.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnResetQR.ForeColor = Color.White;
            btnResetQR.Location = new Point(271, 9);
            btnResetQR.Margin = new Padding(3, 3, 3, 20);
            btnResetQR.Name = "btnResetQR";
            btnResetQR.PressedColor = Color.DarkRed;
            btnResetQR.ShadowDecoration.BorderRadius = 8;
            customizableEdges4.TopLeft = false;
            customizableEdges4.TopRight = false;
            btnResetQR.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnResetQR.ShadowDecoration.Depth = 20;
            btnResetQR.ShadowDecoration.Enabled = true;
            btnResetQR.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            btnResetQR.Size = new Size(117, 45);
            btnResetQR.TabIndex = 26;
            btnResetQR.Text = "Reset Login";
            btnResetQR.Click += btnResetQR_Click;
            // 
            // LogPanel
            // 
            LogPanel.BackColor = SystemColors.Control;
            LogPanel.Controls.Add(label6);
            LogPanel.Controls.Add(LoggerMsg);
            LogPanel.ForeColor = SystemColors.Control;
            LogPanel.Location = new Point(12, 286);
            LogPanel.Name = "LogPanel";
            LogPanel.Size = new Size(376, 133);
            LogPanel.TabIndex = 110;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Font = new Font("Courier New", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label6.ForeColor = SystemColors.ActiveCaptionText;
            label6.Location = new Point(33, 8);
            label6.Name = "label6";
            label6.Size = new Size(287, 16);
            label6.TabIndex = 1;
            label6.Text = ">_ Log Aktifitas Whatsapp( Auto Update )";
            // 
            // LoggerMsg
            // 
            LoggerMsg.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LoggerMsg.BackColor = Color.White;
            LoggerMsg.Location = new Point(3, 27);
            LoggerMsg.Name = "LoggerMsg";
            LoggerMsg.Size = new Size(370, 106);
            LoggerMsg.TabIndex = 0;
            // 
            // Button1
            // 
            Button1.BackColor = Color.Transparent;
            Button1.Cursor = Cursors.Hand;
            Button1.FlatAppearance.BorderSize = 0;
            Button1.FlatStyle = FlatStyle.Flat;
            Button1.Flip = FontAwesome.Sharp.FlipOrientation.Horizontal;
            Button1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Button1.ForeColor = Color.Black;
            Button1.IconChar = FontAwesome.Sharp.IconChar.CircleChevronRight;
            Button1.IconColor = Color.FromArgb(30, 31, 68);
            Button1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            Button1.IconSize = 25;
            Button1.ImageAlign = ContentAlignment.MiddleLeft;
            Button1.Location = new Point(12, 12);
            Button1.Name = "Button1";
            Button1.Size = new Size(49, 30);
            Button1.TabIndex = 111;
            Button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            Button1.UseVisualStyleBackColor = false;
            Button1.Click += Button1_Click;
            // 
            // btnReload
            // 
            btnReload.BackColor = Color.Transparent;
            btnReload.Cursor = Cursors.Hand;
            btnReload.FlatAppearance.BorderSize = 0;
            btnReload.FlatStyle = FlatStyle.Flat;
            btnReload.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnReload.ForeColor = Color.Black;
            btnReload.IconChar = FontAwesome.Sharp.IconChar.Rotate;
            btnReload.IconColor = Color.Black;
            btnReload.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnReload.IconSize = 20;
            btnReload.Location = new Point(12, 60);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(83, 31);
            btnReload.TabIndex = 112;
            btnReload.Text = "Refresh";
            btnReload.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnReload.UseVisualStyleBackColor = false;
            btnReload.Click += btnReload_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Courier New", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblStatus.ForeColor = Color.LimeGreen;
            lblStatus.Location = new Point(99, 12);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(70, 16);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "lblStatus";
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 10000;
            timer1.Tick += timer1_Tick;
            // 
            // Whatsapp_Config
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 431);
            ControlBox = false;
            Controls.Add(lblStatus);
            Controls.Add(btnReload);
            Controls.Add(Button1);
            Controls.Add(LogPanel);
            Controls.Add(btnResetQR);
            Controls.Add(QRPictureBox);
            ForeColor = Color.DarkRed;
            MaximizeBox = false;
            MdiChildrenMinimizedAnchorBottom = false;
            MinimizeBox = false;
            Name = "Whatsapp_Config";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Whatsapp_Config";
            TopMost = true;
            ((System.ComponentModel.ISupportInitialize)QRPictureBox).EndInit();
            LogPanel.ResumeLayout(false);
            LogPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2PictureBox QRPictureBox;
        public Guna.UI2.WinForms.Guna2Button btnResetQR;
        private Panel LogPanel;
        private Label label6;
        private Panel LoggerMsg;
        private FontAwesome.Sharp.IconButton Button1;
        private FontAwesome.Sharp.IconButton btnReload;
        private Label lblStatus;
        private System.Windows.Forms.Timer timer1;
    }
}