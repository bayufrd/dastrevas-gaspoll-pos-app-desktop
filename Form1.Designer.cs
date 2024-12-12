using FontAwesome.Sharp;
using KASIR.Model;
namespace KASIR
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            button1 = new Button();
            button6 = new Button();
            panel1 = new Panel();
            panelTitleBar = new Panel();
            btnMin = new IconButton();
            btnMax = new IconButton();
            btnExit = new IconButton();
            gradientPanel1 = new GradientPanel();
            progressBar = new ProgressBar();
            lblDetail = new Label();
            lblNamaOutlet = new Label();
            iconCurrentChildForm = new IconPictureBox();
            lblTitleChildForm = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            gradientPanel2 = new GradientPanel();
            SignalPing = new IconButton();
            lblPing = new Label();
            btnShiftLaporan = new IconButton();
            iconButton1 = new IconButton();
            iconButton2 = new IconButton();
            panel3 = new Panel();
            button2 = new PictureBox();
            Setting = new IconButton();
            panel2 = new Panel();
            panel1.SuspendLayout();
            panelTitleBar.SuspendLayout();
            gradientPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)iconCurrentChildForm).BeginInit();
            gradientPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)button2).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(0, 0);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            // 
            // button6
            // 
            button6.Location = new Point(0, 0);
            button6.Name = "button6";
            button6.Size = new Size(75, 23);
            button6.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(panelTitleBar);
            panel1.Dock = DockStyle.Fill;
            panel1.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            panel1.Location = new Point(106, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(906, 580);
            panel1.TabIndex = 4;
            panel1.Paint += panel1_Paint;
            // 
            // panelTitleBar
            // 
            panelTitleBar.AutoSize = true;
            panelTitleBar.BackColor = Color.FromArgb(30, 31, 68);
            panelTitleBar.Controls.Add(btnMin);
            panelTitleBar.Controls.Add(btnMax);
            panelTitleBar.Controls.Add(btnExit);
            panelTitleBar.Controls.Add(gradientPanel1);
            panelTitleBar.Dock = DockStyle.Top;
            panelTitleBar.Location = new Point(0, 0);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.Size = new Size(906, 48);
            panelTitleBar.TabIndex = 0;
            panelTitleBar.MouseDown += panelTitleBar_MouseDown;
            // 
            // btnMin
            // 
            btnMin.BackColor = Color.Transparent;
            btnMin.BackgroundImageLayout = ImageLayout.None;
            btnMin.Dock = DockStyle.Right;
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.FlatStyle = FlatStyle.Flat;
            btnMin.ForeColor = Color.Wheat;
            btnMin.IconChar = IconChar.CircleMinus;
            btnMin.IconColor = Color.DarkSeaGreen;
            btnMin.IconFont = IconFont.Auto;
            btnMin.IconSize = 25;
            btnMin.Location = new Point(807, 0);
            btnMin.Margin = new Padding(0);
            btnMin.Name = "btnMin";
            btnMin.Size = new Size(33, 48);
            btnMin.TabIndex = 4;
            btnMin.TextAlign = ContentAlignment.TopRight;
            btnMin.UseVisualStyleBackColor = false;
            btnMin.Click += btnMinimize_Click;
            // 
            // btnMax
            // 
            btnMax.BackColor = Color.Transparent;
            btnMax.BackgroundImageLayout = ImageLayout.None;
            btnMax.Dock = DockStyle.Right;
            btnMax.FlatAppearance.BorderSize = 0;
            btnMax.FlatStyle = FlatStyle.Flat;
            btnMax.ForeColor = Color.Transparent;
            btnMax.IconChar = IconChar.Maximize;
            btnMax.IconColor = Color.PaleGoldenrod;
            btnMax.IconFont = IconFont.Auto;
            btnMax.IconSize = 25;
            btnMax.Location = new Point(840, 0);
            btnMax.Margin = new Padding(0);
            btnMax.Name = "btnMax";
            btnMax.Size = new Size(33, 48);
            btnMax.TabIndex = 3;
            btnMax.TextAlign = ContentAlignment.TopRight;
            btnMax.UseVisualStyleBackColor = false;
            btnMax.Click += btnMaximize_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.Transparent;
            btnExit.BackgroundImageLayout = ImageLayout.None;
            btnExit.Dock = DockStyle.Right;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.ForeColor = Color.Transparent;
            btnExit.IconChar = IconChar.CircleMinus;
            btnExit.IconColor = Color.IndianRed;
            btnExit.IconFont = IconFont.Auto;
            btnExit.IconSize = 25;
            btnExit.Location = new Point(873, 0);
            btnExit.Margin = new Padding(0);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(33, 48);
            btnExit.TabIndex = 2;
            btnExit.TextAlign = ContentAlignment.TopRight;
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // gradientPanel1
            // 
            gradientPanel1.Angle = 0F;
            gradientPanel1.BottomColor = Color.FromArgb(30, 31, 68);
            gradientPanel1.Controls.Add(progressBar);
            gradientPanel1.Controls.Add(lblDetail);
            gradientPanel1.Controls.Add(lblNamaOutlet);
            gradientPanel1.Controls.Add(iconCurrentChildForm);
            gradientPanel1.Controls.Add(lblTitleChildForm);
            gradientPanel1.Location = new Point(-109, 0);
            gradientPanel1.Name = "gradientPanel1";
            gradientPanel1.Size = new Size(1015, 45);
            gradientPanel1.TabIndex = 5;
            gradientPanel1.TopColor = Color.Black;
            gradientPanel1.MouseDown += panelTitleBar_MouseDown;
            // 
            // progressBar
            // 
            progressBar.ForeColor = SystemColors.ControlText;
            progressBar.Location = new Point(552, 28);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(112, 10);
            progressBar.Step = 100;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 51;
            // 
            // lblDetail
            // 
            lblDetail.AutoSize = true;
            lblDetail.BackColor = Color.Transparent;
            lblDetail.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
            lblDetail.ForeColor = Color.Gainsboro;
            lblDetail.Location = new Point(551, 7);
            lblDetail.Name = "lblDetail";
            lblDetail.Size = new Size(37, 15);
            lblDetail.TabIndex = 3;
            lblDetail.Text = "Menu";
            // 
            // lblNamaOutlet
            // 
            lblNamaOutlet.AutoSize = true;
            lblNamaOutlet.BackColor = Color.Transparent;
            lblNamaOutlet.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Italic, GraphicsUnit.Point);
            lblNamaOutlet.ForeColor = Color.Gainsboro;
            lblNamaOutlet.Location = new Point(167, 9);
            lblNamaOutlet.Name = "lblNamaOutlet";
            lblNamaOutlet.Size = new Size(44, 19);
            lblNamaOutlet.TabIndex = 2;
            lblNamaOutlet.Text = "Menu";
            // 
            // iconCurrentChildForm
            // 
            iconCurrentChildForm.BackColor = Color.Transparent;
            iconCurrentChildForm.ForeColor = Color.MediumPurple;
            iconCurrentChildForm.IconChar = IconChar.House;
            iconCurrentChildForm.IconColor = Color.MediumPurple;
            iconCurrentChildForm.IconFont = IconFont.Auto;
            iconCurrentChildForm.IconSize = 37;
            iconCurrentChildForm.Location = new Point(115, 3);
            iconCurrentChildForm.Name = "iconCurrentChildForm";
            iconCurrentChildForm.Size = new Size(46, 37);
            iconCurrentChildForm.TabIndex = 0;
            iconCurrentChildForm.TabStop = false;
            // 
            // lblTitleChildForm
            // 
            lblTitleChildForm.AutoSize = true;
            lblTitleChildForm.BackColor = Color.Transparent;
            lblTitleChildForm.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitleChildForm.ForeColor = Color.Gainsboro;
            lblTitleChildForm.Location = new Point(256, 13);
            lblTitleChildForm.Name = "lblTitleChildForm";
            lblTitleChildForm.Size = new Size(38, 15);
            lblTitleChildForm.TabIndex = 1;
            lblTitleChildForm.Text = "Menu";
            lblTitleChildForm.Visible = false;
            // 
            // timer1
            // 
            timer1.Interval = 5000;
            timer1.Tick += timer1_Tick;
            // 
            // gradientPanel2
            // 
            gradientPanel2.Angle = 90F;
            gradientPanel2.BottomColor = Color.FromArgb(31, 30, 68);
            gradientPanel2.Controls.Add(SignalPing);
            gradientPanel2.Controls.Add(lblPing);
            gradientPanel2.Controls.Add(btnShiftLaporan);
            gradientPanel2.Controls.Add(iconButton1);
            gradientPanel2.Controls.Add(iconButton2);
            gradientPanel2.Controls.Add(panel3);
            gradientPanel2.Controls.Add(button2);
            gradientPanel2.Location = new Point(-3, -4);
            gradientPanel2.Name = "gradientPanel2";
            gradientPanel2.Size = new Size(112, 584);
            gradientPanel2.TabIndex = 1;
            gradientPanel2.TopColor = Color.Black;
            // 
            // SignalPing
            // 
            SignalPing.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            SignalPing.AutoSize = true;
            SignalPing.FlatAppearance.BorderSize = 0;
            SignalPing.FlatStyle = FlatStyle.Flat;
            SignalPing.IconChar = IconChar.WifiStrong;
            SignalPing.IconColor = Color.WhiteSmoke;
            SignalPing.IconFont = IconFont.Auto;
            SignalPing.IconSize = 30;
            SignalPing.ImageAlign = ContentAlignment.MiddleLeft;
            SignalPing.Location = new Point(6, 509);
            SignalPing.Name = "SignalPing";
            SignalPing.Size = new Size(103, 42);
            SignalPing.TabIndex = 6;
            SignalPing.Text = "Ping";
            SignalPing.TextImageRelation = TextImageRelation.ImageBeforeText;
            SignalPing.UseVisualStyleBackColor = true;
            SignalPing.Click += btnTestSpeed_Click;
            // 
            // lblPing
            // 
            lblPing.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblPing.AutoSize = true;
            lblPing.Font = new Font("Consolas", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
            lblPing.ForeColor = Color.Lime;
            lblPing.Location = new Point(3, 464);
            lblPing.Name = "lblPing";
            lblPing.Size = new Size(37, 13);
            lblPing.TabIndex = 20;
            lblPing.Text = "Ping:";
            lblPing.Visible = false;
            // 
            // btnShiftLaporan
            // 
            btnShiftLaporan.BackColor = Color.Transparent;
            btnShiftLaporan.FlatAppearance.BorderSize = 0;
            btnShiftLaporan.FlatStyle = FlatStyle.Flat;
            btnShiftLaporan.ForeColor = Color.Gainsboro;
            btnShiftLaporan.IconChar = IconChar.UserEdit;
            btnShiftLaporan.IconColor = Color.Gainsboro;
            btnShiftLaporan.IconFont = IconFont.Auto;
            btnShiftLaporan.IconSize = 25;
            btnShiftLaporan.ImageAlign = ContentAlignment.MiddleLeft;
            btnShiftLaporan.Location = new Point(3, 257);
            btnShiftLaporan.Name = "btnShiftLaporan";
            btnShiftLaporan.Size = new Size(106, 60);
            btnShiftLaporan.TabIndex = 20;
            btnShiftLaporan.Text = "Shift Report";
            btnShiftLaporan.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnShiftLaporan.UseVisualStyleBackColor = false;
            btnShiftLaporan.Click += btnShiftLaporan_Click;
            // 
            // iconButton1
            // 
            iconButton1.BackColor = Color.Transparent;
            iconButton1.FlatAppearance.BorderSize = 0;
            iconButton1.FlatStyle = FlatStyle.Flat;
            iconButton1.ForeColor = Color.Gainsboro;
            iconButton1.IconChar = IconChar.HomeLg;
            iconButton1.IconColor = Color.Gainsboro;
            iconButton1.IconFont = IconFont.Auto;
            iconButton1.IconSize = 25;
            iconButton1.ImageAlign = ContentAlignment.MiddleLeft;
            iconButton1.Location = new Point(3, 125);
            iconButton1.Name = "iconButton1";
            iconButton1.Size = new Size(106, 60);
            iconButton1.TabIndex = 17;
            iconButton1.Text = "Menu";
            iconButton1.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButton1.UseVisualStyleBackColor = false;
            iconButton1.Click += button6_Click;
            // 
            // iconButton2
            // 
            iconButton2.BackColor = Color.Transparent;
            iconButton2.FlatAppearance.BorderSize = 0;
            iconButton2.FlatStyle = FlatStyle.Flat;
            iconButton2.ForeColor = Color.Gainsboro;
            iconButton2.IconChar = IconChar.Book;
            iconButton2.IconColor = Color.Gainsboro;
            iconButton2.IconFont = IconFont.Auto;
            iconButton2.IconSize = 25;
            iconButton2.ImageAlign = ContentAlignment.MiddleLeft;
            iconButton2.Location = new Point(3, 191);
            iconButton2.Name = "iconButton2";
            iconButton2.Size = new Size(106, 60);
            iconButton2.TabIndex = 18;
            iconButton2.Text = "Transactions";
            iconButton2.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButton2.UseVisualStyleBackColor = false;
            iconButton2.Click += button1_Click;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Transparent;
            panel3.Location = new Point(30, 93);
            panel3.Name = "panel3";
            panel3.Size = new Size(10, 66);
            panel3.TabIndex = 0;
            // 
            // button2
            // 
            button2.BackColor = Color.Transparent;
            button2.Cursor = Cursors.Help;
            button2.Image = Properties.Resources.a_2_;
            button2.Location = new Point(0, -9);
            button2.Name = "button2";
            button2.Size = new Size(112, 87);
            button2.SizeMode = PictureBoxSizeMode.Zoom;
            button2.TabIndex = 19;
            button2.TabStop = false;
            button2.Click += link_Click;
            // 
            // Setting
            // 
            Setting.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Setting.FlatAppearance.BorderSize = 0;
            Setting.FlatStyle = FlatStyle.Flat;
            Setting.IconChar = IconChar.Gear;
            Setting.IconColor = Color.WhiteSmoke;
            Setting.IconFont = IconFont.Auto;
            Setting.IconSize = 20;
            Setting.Location = new Point(0, 553);
            Setting.Name = "Setting";
            Setting.Size = new Size(23, 27);
            Setting.TabIndex = 5;
            Setting.UseVisualStyleBackColor = true;
            Setting.Click += btnEditSettings_Click;
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(31, 30, 68);
            panel2.Controls.Add(Setting);
            panel2.Controls.Add(gradientPanel2);
            panel2.Dock = DockStyle.Left;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(106, 580);
            panel2.TabIndex = 3;
            panel2.Paint += panel2_Paint;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1012, 580);
            Controls.Add(panel1);
            Controls.Add(panel2);
            ForeColor = Color.Maroon;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            HelpButton = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Kasir";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panelTitleBar.ResumeLayout(false);
            gradientPanel1.ResumeLayout(false);
            gradientPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)iconCurrentChildForm).EndInit();
            gradientPanel2.ResumeLayout(false);
            gradientPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)button2).EndInit();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void BtnAutoUpdate_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

#endregion
        private Button button6;
        private Panel panel1;
        private Panel panelTitleBar;
        private Button button1;
        private IconPictureBox iconCurrentChildForm;
        private Label lblTitleChildForm;
        private IconButton btnExit;
        private IconButton btnMax;
        private IconButton btnMin;
        private System.Windows.Forms.Timer timer1;
        private GradientPanel gradientPanel1;
        private Label lblNamaOutlet;
        private GradientPanel gradientPanel2;
        private IconButton iconButton1;
        private IconButton iconButton2;
        private Panel panel3;
        private PictureBox button2;
        private IconButton Setting;
        private Label lblPing;
        private Panel panel2;
        private Label lblDetail;
        private ProgressBar progressBar;
        private IconButton btnShiftLaporan;
        private IconButton SignalPing;
    }
}