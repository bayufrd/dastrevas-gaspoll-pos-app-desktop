namespace KASIR.Helper
{
    partial class QuestionEndShift
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            btnConfirm = new Guna.UI2.WinForms.Guna2Button();
            btnCancel = new Guna.UI2.WinForms.Guna2Button();
            PanelText = new Panel();
            shift_number = new Guna.UI2.WinForms.Guna2TextBox();
            lblDisc = new Label();
            lblTitle = new Label();
            start_shift = new Guna.UI2.WinForms.Guna2TextBox();
            end_shift = new Guna.UI2.WinForms.Guna2TextBox();
            label1 = new Label();
            label2 = new Label();
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
            btnConfirm.Location = new Point(215, 217);
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
            btnCancel.Location = new Point(25, 217);
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
            PanelText.Controls.Add(shift_number);
            PanelText.Controls.Add(lblDisc);
            PanelText.Controls.Add(lblTitle);
            PanelText.Location = new Point(25, 16);
            PanelText.Name = "PanelText";
            PanelText.Size = new Size(298, 126);
            PanelText.TabIndex = 4;
            PanelText.Paint += PanelText_Paint;
            // 
            // shift_number
            // 
            shift_number.BorderRadius = 8;
            shift_number.CustomizableEdges = customizableEdges5;
            shift_number.DefaultText = "";
            shift_number.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            shift_number.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            shift_number.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            shift_number.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            shift_number.FocusedState.BorderColor = Color.FromArgb(15, 90, 94);
            shift_number.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            shift_number.ForeColor = Color.Black;
            shift_number.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            shift_number.Location = new Point(158, 10);
            shift_number.Name = "shift_number";
            shift_number.PlaceholderForeColor = Color.Gray;
            shift_number.PlaceholderText = "Masukkan jam mulai";
            shift_number.SelectedText = "";
            shift_number.ShadowDecoration.CustomizableEdges = customizableEdges6;
            shift_number.Size = new Size(43, 32);
            shift_number.TabIndex = 123;
            shift_number.TextAlign = HorizontalAlignment.Center;
            // 
            // lblDisc
            // 
            lblDisc.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblDisc.Location = new Point(0, 45);
            lblDisc.Margin = new Padding(0);
            lblDisc.Name = "lblDisc";
            lblDisc.Size = new Size(295, 68);
            lblDisc.TabIndex = 1;
            lblDisc.Text = "Hapus Keranjang ini ?";
            lblDisc.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Location = new Point(80, 4);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(72, 41);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "End Shift";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // start_shift
            // 
            start_shift.BorderRadius = 8;
            start_shift.CustomizableEdges = customizableEdges7;
            start_shift.DefaultText = "";
            start_shift.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            start_shift.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            start_shift.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            start_shift.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            start_shift.FocusedState.BorderColor = Color.FromArgb(15, 90, 94);
            start_shift.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            start_shift.ForeColor = Color.Black;
            start_shift.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            start_shift.Location = new Point(105, 136);
            start_shift.Name = "start_shift";
            start_shift.PlaceholderForeColor = Color.Gray;
            start_shift.PlaceholderText = "Masukkan jam mulai";
            start_shift.SelectedText = "";
            start_shift.ShadowDecoration.CustomizableEdges = customizableEdges8;
            start_shift.Size = new Size(213, 32);
            start_shift.TabIndex = 120;
            start_shift.TextAlign = HorizontalAlignment.Center;
            // 
            // end_shift
            // 
            end_shift.BorderRadius = 8;
            end_shift.CustomizableEdges = customizableEdges9;
            end_shift.DefaultText = "";
            end_shift.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            end_shift.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            end_shift.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            end_shift.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            end_shift.FocusedState.BorderColor = Color.FromArgb(15, 90, 94);
            end_shift.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            end_shift.ForeColor = Color.Black;
            end_shift.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            end_shift.Location = new Point(105, 174);
            end_shift.Name = "end_shift";
            end_shift.PlaceholderForeColor = Color.Gray;
            end_shift.PlaceholderText = "Masukkan Jam Selesai";
            end_shift.SelectedText = "";
            end_shift.ShadowDecoration.CustomizableEdges = customizableEdges10;
            end_shift.Size = new Size(213, 32);
            end_shift.TabIndex = 121;
            end_shift.TextAlign = HorizontalAlignment.Center;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(25, 139);
            label1.Name = "label1";
            label1.Size = new Size(74, 29);
            label1.TabIndex = 2;
            label1.Text = "Mulai";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label2.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(25, 174);
            label2.Name = "label2";
            label2.Size = new Size(74, 29);
            label2.TabIndex = 122;
            label2.Text = "Selesai";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // QuestionEndShift
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.White;
            ClientSize = new Size(350, 280);
            ControlBox = false;
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(end_shift);
            Controls.Add(start_shift);
            Controls.Add(PanelText);
            Controls.Add(btnConfirm);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MdiChildrenMinimizedAnchorBottom = false;
            MinimizeBox = false;
            Name = "QuestionEndShift";
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
        private Guna.UI2.WinForms.Guna2TextBox start_shift;
        private Guna.UI2.WinForms.Guna2TextBox end_shift;
        public Label label1;
        public Label label2;
        private Guna.UI2.WinForms.Guna2TextBox shift_number;
    }
}