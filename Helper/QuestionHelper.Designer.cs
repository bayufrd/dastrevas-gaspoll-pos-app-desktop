namespace KASIR.Helper
{
    partial class QuestionHelper
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            btnConfirm = new Guna.UI2.WinForms.Guna2Button();
            btnCancel = new Guna.UI2.WinForms.Guna2Button();
            PanelText = new Panel();
            lblDisc = new Label();
            lblTitle = new Label();
            PanelText.SuspendLayout();
            SuspendLayout();
            // 
            // btnConfirm
            // 
            btnConfirm.BackColor = Color.Transparent;
            btnConfirm.BorderColor = Color.FromArgb(15, 90, 94);
            btnConfirm.BorderRadius = 8;
            btnConfirm.BorderThickness = 2;
            btnConfirm.Cursor = Cursors.Hand;
            btnConfirm.CustomizableEdges = customizableEdges1;
            btnConfirm.DisabledState.BorderColor = Color.DarkGray;
            btnConfirm.DisabledState.CustomBorderColor = Color.DarkGray;
            btnConfirm.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnConfirm.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnConfirm.FillColor = Color.FromArgb(15, 90, 94);
            btnConfirm.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnConfirm.ForeColor = Color.White;
            btnConfirm.Location = new Point(215, 127);
            btnConfirm.Margin = new Padding(3, 3, 3, 20);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.ShadowDecoration.BorderRadius = 8;
            customizableEdges2.TopLeft = false;
            customizableEdges2.TopRight = false;
            btnConfirm.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnConfirm.ShadowDecoration.Depth = 20;
            btnConfirm.ShadowDecoration.Enabled = true;
            btnConfirm.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            btnConfirm.Size = new Size(105, 34);
            btnConfirm.TabIndex = 3;
            btnConfirm.Text = "Konfirmasi";
            btnConfirm.Click += btnConfirm_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.Transparent;
            btnCancel.BorderRadius = 8;
            btnCancel.BorderThickness = 2;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.CustomizableEdges = customizableEdges3;
            btnCancel.DisabledState.BorderColor = Color.DarkGray;
            btnCancel.DisabledState.CustomBorderColor = Color.DarkGray;
            btnCancel.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnCancel.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnCancel.FillColor = Color.White;
            btnCancel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnCancel.ForeColor = Color.Black;
            btnCancel.Location = new Point(25, 127);
            btnCancel.Margin = new Padding(3, 3, 3, 20);
            btnCancel.Name = "btnCancel";
            btnCancel.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnCancel.ShadowDecoration.Depth = 8;
            btnCancel.ShadowDecoration.Enabled = true;
            btnCancel.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            btnCancel.Size = new Size(105, 34);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Batal";
            btnCancel.Click += btnCancel_Click;
            // 
            // PanelText
            // 
            PanelText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PanelText.AutoSize = true;
            PanelText.Controls.Add(lblDisc);
            PanelText.Controls.Add(lblTitle);
            PanelText.Location = new Point(25, 16);
            PanelText.Name = "PanelText";
            PanelText.Size = new Size(295, 105);
            PanelText.TabIndex = 4;
            PanelText.Paint += PanelText_Paint;
            // 
            // lblDisc
            // 
            lblDisc.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblDisc.Location = new Point(0, 31);
            lblDisc.Margin = new Padding(0);
            lblDisc.Name = "lblDisc";
            lblDisc.Size = new Size(295, 65);
            lblDisc.TabIndex = 1;
            lblDisc.Text = "Hapus Keranjang ini ?";
            lblDisc.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Location = new Point(80, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(138, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Hapus Keranjang ?";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // QuestionHelper
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.White;
            ClientSize = new Size(350, 184);
            ControlBox = false;
            Controls.Add(PanelText);
            Controls.Add(btnConfirm);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MdiChildrenMinimizedAnchorBottom = false;
            MinimizeBox = false;
            Name = "QuestionHelper";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Question";
            TopMost = true;
            PanelText.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel PanelText;
        public Label lblDisc;
        public Label lblTitle;
        public Guna.UI2.WinForms.Guna2Button btnConfirm;
        public Guna.UI2.WinForms.Guna2Button btnCancel;
    }
}