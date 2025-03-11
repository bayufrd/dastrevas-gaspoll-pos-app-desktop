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
            SyncTimer = new System.Windows.Forms.Timer(components);
            gradientPanel2 = new GradientPanel();
            btnContact = new IconButton();
            SignalPing = new IconButton();
            lblPing = new Label();
            btnShiftLaporan = new IconButton();
            iconButton1 = new IconButton();
            iconButton2 = new IconButton();
            panel3 = new Panel();
            button2 = new PictureBox();
            Setting = new IconButton();
            panel2 = new Panel();
            btnDev = new IconButton();
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
            resources.ApplyResources(button1, "button1");
            button1.Name = "button1";
            // 
            // button6
            // 
            resources.ApplyResources(button6, "button6");
            button6.Name = "button6";
            // 
            // panel1
            // 
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(panelTitleBar);
            resources.ApplyResources(panel1, "panel1");
            panel1.Name = "panel1";
            panel1.Paint += panel1_Paint;
            // 
            // panelTitleBar
            // 
            resources.ApplyResources(panelTitleBar, "panelTitleBar");
            panelTitleBar.BackColor = Color.FromArgb(30, 31, 68);
            panelTitleBar.Controls.Add(btnMin);
            panelTitleBar.Controls.Add(btnMax);
            panelTitleBar.Controls.Add(btnExit);
            panelTitleBar.Controls.Add(gradientPanel1);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.MouseDown += panelTitleBar_MouseDown;
            // 
            // btnMin
            // 
            btnMin.BackColor = Color.Transparent;
            resources.ApplyResources(btnMin, "btnMin");
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.ForeColor = Color.Wheat;
            btnMin.IconChar = IconChar.CircleMinus;
            btnMin.IconColor = Color.DarkSeaGreen;
            btnMin.IconFont = IconFont.Auto;
            btnMin.IconSize = 25;
            btnMin.Name = "btnMin";
            btnMin.UseVisualStyleBackColor = false;
            btnMin.Click += btnMinimize_Click;
            // 
            // btnMax
            // 
            btnMax.BackColor = Color.Transparent;
            resources.ApplyResources(btnMax, "btnMax");
            btnMax.FlatAppearance.BorderSize = 0;
            btnMax.ForeColor = Color.Transparent;
            btnMax.IconChar = IconChar.Maximize;
            btnMax.IconColor = Color.PaleGoldenrod;
            btnMax.IconFont = IconFont.Auto;
            btnMax.IconSize = 25;
            btnMax.Name = "btnMax";
            btnMax.UseVisualStyleBackColor = false;
            btnMax.Click += btnMaximize_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.Transparent;
            resources.ApplyResources(btnExit, "btnExit");
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.ForeColor = Color.Transparent;
            btnExit.IconChar = IconChar.CircleMinus;
            btnExit.IconColor = Color.IndianRed;
            btnExit.IconFont = IconFont.Auto;
            btnExit.IconSize = 25;
            btnExit.Name = "btnExit";
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
            resources.ApplyResources(gradientPanel1, "gradientPanel1");
            gradientPanel1.Name = "gradientPanel1";
            gradientPanel1.TopColor = Color.Black;
            gradientPanel1.MouseDown += panelTitleBar_MouseDown;
            // 
            // progressBar
            // 
            progressBar.ForeColor = SystemColors.ControlText;
            resources.ApplyResources(progressBar, "progressBar");
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Step = 100;
            progressBar.Style = ProgressBarStyle.Continuous;
            // 
            // lblDetail
            // 
            resources.ApplyResources(lblDetail, "lblDetail");
            lblDetail.BackColor = Color.Transparent;
            lblDetail.ForeColor = Color.Gainsboro;
            lblDetail.Name = "lblDetail";
            // 
            // lblNamaOutlet
            // 
            resources.ApplyResources(lblNamaOutlet, "lblNamaOutlet");
            lblNamaOutlet.BackColor = Color.Transparent;
            lblNamaOutlet.ForeColor = Color.Gainsboro;
            lblNamaOutlet.Name = "lblNamaOutlet";
            // 
            // iconCurrentChildForm
            // 
            iconCurrentChildForm.BackColor = Color.Transparent;
            iconCurrentChildForm.ForeColor = Color.MediumPurple;
            iconCurrentChildForm.IconChar = IconChar.House;
            iconCurrentChildForm.IconColor = Color.MediumPurple;
            iconCurrentChildForm.IconFont = IconFont.Auto;
            iconCurrentChildForm.IconSize = 37;
            resources.ApplyResources(iconCurrentChildForm, "iconCurrentChildForm");
            iconCurrentChildForm.Name = "iconCurrentChildForm";
            iconCurrentChildForm.TabStop = false;
            // 
            // lblTitleChildForm
            // 
            resources.ApplyResources(lblTitleChildForm, "lblTitleChildForm");
            lblTitleChildForm.BackColor = Color.Transparent;
            lblTitleChildForm.ForeColor = Color.Gainsboro;
            lblTitleChildForm.Name = "lblTitleChildForm";
            // 
            // SyncTimer
            // 
            SyncTimer.Enabled = true;
            SyncTimer.Interval = 1800000;
            SyncTimer.Tick += timer1_Tick;
            // 
            // gradientPanel2
            // 
            gradientPanel2.Angle = 90F;
            gradientPanel2.BottomColor = Color.FromArgb(31, 30, 68);
            gradientPanel2.Controls.Add(btnDev);
            gradientPanel2.Controls.Add(btnContact);
            gradientPanel2.Controls.Add(SignalPing);
            gradientPanel2.Controls.Add(lblPing);
            gradientPanel2.Controls.Add(btnShiftLaporan);
            gradientPanel2.Controls.Add(iconButton1);
            gradientPanel2.Controls.Add(iconButton2);
            gradientPanel2.Controls.Add(panel3);
            gradientPanel2.Controls.Add(button2);
            resources.ApplyResources(gradientPanel2, "gradientPanel2");
            gradientPanel2.Name = "gradientPanel2";
            gradientPanel2.TopColor = Color.Black;
            // 
            // btnContact
            // 
            btnContact.BackColor = Color.Transparent;
            btnContact.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnContact, "btnContact");
            btnContact.ForeColor = Color.Gainsboro;
            btnContact.IconChar = IconChar.HandHoldingHeart;
            btnContact.IconColor = Color.Gainsboro;
            btnContact.IconFont = IconFont.Auto;
            btnContact.IconSize = 25;
            btnContact.Name = "btnContact";
            btnContact.UseVisualStyleBackColor = false;
            btnContact.Click += btnContact_Click;
            // 
            // SignalPing
            // 
            resources.ApplyResources(SignalPing, "SignalPing");
            SignalPing.FlatAppearance.BorderSize = 0;
            SignalPing.IconChar = IconChar.WifiStrong;
            SignalPing.IconColor = Color.WhiteSmoke;
            SignalPing.IconFont = IconFont.Auto;
            SignalPing.IconSize = 30;
            SignalPing.Name = "SignalPing";
            SignalPing.UseVisualStyleBackColor = true;
            SignalPing.Click += btnTestSpeed_Click;
            // 
            // lblPing
            // 
            resources.ApplyResources(lblPing, "lblPing");
            lblPing.ForeColor = Color.Lime;
            lblPing.Name = "lblPing";
            // 
            // btnShiftLaporan
            // 
            btnShiftLaporan.BackColor = Color.Transparent;
            btnShiftLaporan.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnShiftLaporan, "btnShiftLaporan");
            btnShiftLaporan.ForeColor = Color.Gainsboro;
            btnShiftLaporan.IconChar = IconChar.UserEdit;
            btnShiftLaporan.IconColor = Color.Gainsboro;
            btnShiftLaporan.IconFont = IconFont.Auto;
            btnShiftLaporan.IconSize = 25;
            btnShiftLaporan.Name = "btnShiftLaporan";
            btnShiftLaporan.UseVisualStyleBackColor = false;
            btnShiftLaporan.Click += btnShiftLaporan_Click;
            // 
            // iconButton1
            // 
            iconButton1.BackColor = Color.Transparent;
            iconButton1.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(iconButton1, "iconButton1");
            iconButton1.ForeColor = Color.Gainsboro;
            iconButton1.IconChar = IconChar.HomeLg;
            iconButton1.IconColor = Color.Gainsboro;
            iconButton1.IconFont = IconFont.Auto;
            iconButton1.IconSize = 25;
            iconButton1.Name = "iconButton1";
            iconButton1.UseVisualStyleBackColor = false;
            iconButton1.Click += button6_Click;
            // 
            // iconButton2
            // 
            iconButton2.BackColor = Color.Transparent;
            iconButton2.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(iconButton2, "iconButton2");
            iconButton2.ForeColor = Color.Gainsboro;
            iconButton2.IconChar = IconChar.Book;
            iconButton2.IconColor = Color.Gainsboro;
            iconButton2.IconFont = IconFont.Auto;
            iconButton2.IconSize = 25;
            iconButton2.Name = "iconButton2";
            iconButton2.UseVisualStyleBackColor = false;
            iconButton2.Click += button1_Click;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Transparent;
            resources.ApplyResources(panel3, "panel3");
            panel3.Name = "panel3";
            // 
            // button2
            // 
            button2.BackColor = Color.Transparent;
            button2.Cursor = Cursors.Help;
            button2.Image = Properties.Resources.a_2_;
            resources.ApplyResources(button2, "button2");
            button2.Name = "button2";
            button2.TabStop = false;
            button2.Click += link_Click;
            // 
            // Setting
            // 
            resources.ApplyResources(Setting, "Setting");
            Setting.FlatAppearance.BorderSize = 0;
            Setting.IconChar = IconChar.Gear;
            Setting.IconColor = Color.WhiteSmoke;
            Setting.IconFont = IconFont.Auto;
            Setting.IconSize = 20;
            Setting.Name = "Setting";
            Setting.UseVisualStyleBackColor = true;
            Setting.Click += btnEditSettings_Click;
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(31, 30, 68);
            panel2.Controls.Add(Setting);
            panel2.Controls.Add(gradientPanel2);
            resources.ApplyResources(panel2, "panel2");
            panel2.Name = "panel2";
            panel2.Paint += panel2_Paint;
            // 
            // btnDev
            // 
            btnDev.BackColor = Color.Transparent;
            btnDev.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(btnDev, "btnDev");
            btnDev.ForeColor = Color.Gainsboro;
            btnDev.IconChar = IconChar.GetPocket;
            btnDev.IconColor = Color.Gainsboro;
            btnDev.IconFont = IconFont.Auto;
            btnDev.IconSize = 25;
            btnDev.Name = "btnDev";
            btnDev.UseVisualStyleBackColor = false;
            btnDev.Click += btnDev_Click;
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(panel1);
            Controls.Add(panel2);
            ForeColor = Color.Maroon;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            HelpButton = true;
            Name = "Form1";
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
        private System.Windows.Forms.Timer SyncTimer;
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
        private IconButton btnContact;
        private IconButton btnDev;
    }
}