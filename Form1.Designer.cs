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
            NavBarBtn = new PictureBox();
            lblNamaOutlet = new Label();
            iconCurrentChildForm = new IconPictureBox();
            lblTitleChildForm = new Label();
            SyncTimer = new System.Windows.Forms.Timer(components);
            gradientPanel2 = new GradientPanel();
            BtnSettingForm = new IconButton();
            btnDev = new IconButton();
            btnContact = new IconButton();
            SignalPing = new IconButton();
            lblPing = new Label();
            btnShiftLaporan = new IconButton();
            MenuBtn = new IconButton();
            TransBtn = new IconButton();
            LogoKasir = new PictureBox();
            Setting = new IconButton();
            panel2 = new Panel();
            panel1.SuspendLayout();
            panelTitleBar.SuspendLayout();
            gradientPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NavBarBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)iconCurrentChildForm).BeginInit();
            gradientPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogoKasir).BeginInit();
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
            btnMin.BackColor = Color.FromArgb(15, 90, 94);
            resources.ApplyResources(btnMin, "btnMin");
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.ForeColor = Color.Transparent;
            btnMin.IconChar = IconChar.Minus;
            btnMin.IconColor = Color.White;
            btnMin.IconFont = IconFont.Auto;
            btnMin.IconSize = 25;
            btnMin.Name = "btnMin";
            btnMin.UseVisualStyleBackColor = false;
            btnMin.Click += btnMinimize_Click;
            // 
            // btnMax
            // 
            btnMax.BackColor = Color.FromArgb(15, 90, 94);
            resources.ApplyResources(btnMax, "btnMax");
            btnMax.FlatAppearance.BorderSize = 0;
            btnMax.ForeColor = Color.Transparent;
            btnMax.IconChar = IconChar.Square;
            btnMax.IconColor = Color.White;
            btnMax.IconFont = IconFont.Auto;
            btnMax.IconSize = 25;
            btnMax.Name = "btnMax";
            btnMax.UseVisualStyleBackColor = false;
            btnMax.Click += btnMaximize_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.FromArgb(15, 90, 94);
            resources.ApplyResources(btnExit, "btnExit");
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.ForeColor = Color.Transparent;
            btnExit.IconChar = IconChar.Xmark;
            btnExit.IconColor = Color.White;
            btnExit.IconFont = IconFont.Auto;
            btnExit.IconSize = 25;
            btnExit.Name = "btnExit";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // gradientPanel1
            // 
            resources.ApplyResources(gradientPanel1, "gradientPanel1");
            gradientPanel1.Angle = 0F;
            gradientPanel1.BackColor = Color.FromArgb(15, 90, 94);
            gradientPanel1.BottomColor = Color.FromArgb(15, 90, 94);
            gradientPanel1.Controls.Add(NavBarBtn);
            gradientPanel1.Controls.Add(lblNamaOutlet);
            gradientPanel1.Controls.Add(iconCurrentChildForm);
            gradientPanel1.Controls.Add(lblTitleChildForm);
            gradientPanel1.Name = "gradientPanel1";
            gradientPanel1.TopColor = Color.Black;
            gradientPanel1.MouseDown += panelTitleBar_MouseDown;
            // 
            // NavBarBtn
            // 
            NavBarBtn.BackColor = Color.Transparent;
            NavBarBtn.Cursor = Cursors.Hand;
            NavBarBtn.Image = Properties.Resources.Menu;
            resources.ApplyResources(NavBarBtn, "NavBarBtn");
            NavBarBtn.Name = "NavBarBtn";
            NavBarBtn.TabStop = false;
            NavBarBtn.Click += NavBarBtn_Click;
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
            iconCurrentChildForm.IconSize = 29;
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
            SyncTimer.Interval = 300000;
            SyncTimer.Tick += timer1_Tick;
            // 
            // gradientPanel2
            // 
            resources.ApplyResources(gradientPanel2, "gradientPanel2");
            gradientPanel2.Angle = 90F;
            gradientPanel2.BackColor = Color.FromArgb(15, 90, 94);
            gradientPanel2.BottomColor = Color.FromArgb(15, 90, 94);
            gradientPanel2.Controls.Add(BtnSettingForm);
            gradientPanel2.Controls.Add(btnDev);
            gradientPanel2.Controls.Add(btnContact);
            gradientPanel2.Controls.Add(SignalPing);
            gradientPanel2.Controls.Add(lblPing);
            gradientPanel2.Controls.Add(btnShiftLaporan);
            gradientPanel2.Controls.Add(MenuBtn);
            gradientPanel2.Controls.Add(TransBtn);
            gradientPanel2.Controls.Add(LogoKasir);
            gradientPanel2.Name = "gradientPanel2";
            gradientPanel2.TopColor = Color.Black;
            // 
            // BtnSettingForm
            // 
            BtnSettingForm.BackColor = Color.Transparent;
            BtnSettingForm.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(BtnSettingForm, "BtnSettingForm");
            BtnSettingForm.ForeColor = Color.Gainsboro;
            BtnSettingForm.IconChar = IconChar.Gear;
            BtnSettingForm.IconColor = Color.Gainsboro;
            BtnSettingForm.IconFont = IconFont.Auto;
            BtnSettingForm.IconSize = 25;
            BtnSettingForm.Name = "BtnSettingForm";
            BtnSettingForm.UseVisualStyleBackColor = false;
            BtnSettingForm.Click += BtnSettingForm_Click;
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
            SignalPing.BackColor = Color.Transparent;
            SignalPing.FlatAppearance.BorderSize = 0;
            SignalPing.IconChar = IconChar.WifiStrong;
            SignalPing.IconColor = Color.WhiteSmoke;
            SignalPing.IconFont = IconFont.Auto;
            SignalPing.IconSize = 30;
            SignalPing.Name = "SignalPing";
            SignalPing.UseVisualStyleBackColor = false;
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
            // MenuBtn
            // 
            MenuBtn.BackColor = Color.Transparent;
            MenuBtn.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(MenuBtn, "MenuBtn");
            MenuBtn.ForeColor = Color.Gainsboro;
            MenuBtn.IconChar = IconChar.HomeLg;
            MenuBtn.IconColor = Color.Gainsboro;
            MenuBtn.IconFont = IconFont.Auto;
            MenuBtn.IconSize = 25;
            MenuBtn.Name = "MenuBtn";
            MenuBtn.UseVisualStyleBackColor = false;
            MenuBtn.Click += button6_Click;
            // 
            // TransBtn
            // 
            TransBtn.BackColor = Color.Transparent;
            TransBtn.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(TransBtn, "TransBtn");
            TransBtn.ForeColor = Color.Gainsboro;
            TransBtn.IconChar = IconChar.Book;
            TransBtn.IconColor = Color.Gainsboro;
            TransBtn.IconFont = IconFont.Auto;
            TransBtn.IconSize = 25;
            TransBtn.Name = "TransBtn";
            TransBtn.UseVisualStyleBackColor = false;
            TransBtn.Click += buttonHistoryTransaction;
            // 
            // LogoKasir
            // 
            LogoKasir.BackColor = Color.Transparent;
            LogoKasir.Cursor = Cursors.Hand;
            LogoKasir.Image = Properties.Resources.a_2_1;
            resources.ApplyResources(LogoKasir, "LogoKasir");
            LogoKasir.Name = "LogoKasir";
            LogoKasir.TabStop = false;
            LogoKasir.Click += link_Click;
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
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panelTitleBar.ResumeLayout(false);
            gradientPanel1.ResumeLayout(false);
            gradientPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NavBarBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)iconCurrentChildForm).EndInit();
            gradientPanel2.ResumeLayout(false);
            gradientPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)LogoKasir).EndInit();
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
        private IconButton BtnSettingForm;
        private IconButton btnDev;
        private IconButton btnContact;
        private IconButton SignalPing;
        private Label lblPing;
        private IconButton btnShiftLaporan;
        private IconButton MenuBtn;
        private IconButton TransBtn;
        private PictureBox LogoKasir;
        private IconButton Setting;
        private Panel panel2;
        private PictureBox NavBarBtn;
    }
}