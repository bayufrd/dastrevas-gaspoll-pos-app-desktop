
using FontAwesome.Sharp;
namespace KASIR.Komponen
{
    partial class SettingsForm
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
            Button2 = new IconButton();
            Button1 = new IconButton();
            textBox3 = new TextBox();
            panel1 = new Panel();
            textBox6 = new TextBox();
            panel6 = new Panel();
            btnUpdate = new IconButton();
            lblVersionNow = new Label();
            lblVersion = new Label();
            lblNewVersion = new Label();
            lblNewVersionNow = new Label();
            TestKitchen = new IconButton();
            TestKasir = new IconButton();
            TestBar = new IconButton();
            label7 = new Label();
            panel7 = new Panel();
            txtFooter = new TextBox();
            gradientPanel1 = new Model.GradientPanel();
            panel14 = new Panel();
            txtRunningText = new TextBox();
            label12 = new Label();
            panelPrinterOpt = new Panel();
            lblBluetooth3 = new Label();
            lblBluetooth2 = new Label();
            lblBluetooth1 = new Label();
            panel13 = new Panel();
            txtPrinter3 = new TextBox();
            panel12 = new Panel();
            txtPrinter2 = new TextBox();
            panel11 = new Panel();
            txtPrinter1 = new TextBox();
            label5 = new Label();
            label4 = new Label();
            label2 = new Label();
            panel10 = new Panel();
            checkBoxCheckerPrinter3 = new CheckBox();
            checkBoxMakananPrinter3 = new CheckBox();
            checkBoxMinumanPrinter3 = new CheckBox();
            checkBoxKasirPrinter3 = new CheckBox();
            panel9 = new Panel();
            checkBoxCheckerPrinter2 = new CheckBox();
            checkBoxMakananPrinter2 = new CheckBox();
            checkBoxMinumanPrinter2 = new CheckBox();
            checkBoxKasirPrinter2 = new CheckBox();
            panel8 = new Panel();
            checkBoxCheckerPrinter1 = new CheckBox();
            checkBoxMakananPrinter1 = new CheckBox();
            checkBoxMinumanPrinter1 = new CheckBox();
            checkBoxKasirPrinter1 = new CheckBox();
            ComboBoxPrinter3 = new ComboBox();
            ComboBoxPrinter2 = new ComboBox();
            ComboBoxPrinter1 = new ComboBox();
            ListMenu = new IconButton();
            sButtonListMenu = new Model.SButton();
            CacheApp = new IconButton();
            UpdateInfo = new IconButton();
            Redownload = new IconButton();
            iconDual = new IconButton();
            panel3 = new Panel();
            textBox4 = new TextBox();
            panel2 = new Panel();
            textBox1 = new TextBox();
            panel4 = new Panel();
            textBox5 = new TextBox();
            radioDualMonitor = new Model.SButton();
            label8 = new Label();
            panel5 = new Panel();
            textBox2 = new TextBox();
            lblKitchen = new Label();
            lblKasir = new Label();
            lblBar = new Label();
            label1 = new Label();
            label3 = new Label();
            label6 = new Label();
            iconButton1 = new IconButton();
            panel1.SuspendLayout();
            panel6.SuspendLayout();
            panel7.SuspendLayout();
            gradientPanel1.SuspendLayout();
            panel14.SuspendLayout();
            panelPrinterOpt.SuspendLayout();
            panel13.SuspendLayout();
            panel12.SuspendLayout();
            panel11.SuspendLayout();
            panel10.SuspendLayout();
            panel9.SuspendLayout();
            panel8.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel4.SuspendLayout();
            panel5.SuspendLayout();
            SuspendLayout();
            // 
            // Button2
            // 
            Button2.BackColor = Color.FromArgb(31, 30, 68);
            Button2.Cursor = Cursors.Hand;
            Button2.FlatAppearance.BorderSize = 0;
            Button2.FlatStyle = FlatStyle.Flat;
            Button2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Button2.ForeColor = Color.WhiteSmoke;
            Button2.IconChar = IconChar.CheckCircle;
            Button2.IconColor = Color.WhiteSmoke;
            Button2.IconFont = IconFont.Auto;
            Button2.IconSize = 25;
            Button2.Location = new Point(372, 12);
            Button2.Name = "Button2";
            Button2.Size = new Size(88, 30);
            Button2.TabIndex = 27;
            Button2.Text = "Simpan";
            Button2.TextImageRelation = TextImageRelation.ImageBeforeText;
            Button2.UseVisualStyleBackColor = false;
            Button2.Click += btnSave_Click;
            // 
            // Button1
            // 
            Button1.BackColor = Color.WhiteSmoke;
            Button1.Cursor = Cursors.Hand;
            Button1.FlatAppearance.BorderSize = 0;
            Button1.FlatStyle = FlatStyle.Flat;
            Button1.Flip = FlipOrientation.Horizontal;
            Button1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Button1.ForeColor = Color.FromArgb(30, 31, 68);
            Button1.IconChar = IconChar.CircleChevronRight;
            Button1.IconColor = Color.FromArgb(30, 31, 68);
            Button1.IconFont = IconFont.Auto;
            Button1.IconSize = 25;
            Button1.Location = new Point(28, 12);
            Button1.Name = "Button1";
            Button1.Size = new Size(88, 30);
            Button1.TabIndex = 26;
            Button1.Text = "Keluar";
            Button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            Button1.UseVisualStyleBackColor = false;
            Button1.Click += btnCancel_Click;
            // 
            // textBox3
            // 
            textBox3.BackColor = Color.White;
            textBox3.BorderStyle = BorderStyle.None;
            textBox3.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox3.ForeColor = Color.Black;
            textBox3.Location = new Point(3, 9);
            textBox3.MaxLength = 40;
            textBox3.Name = "textBox3";
            textBox3.PlaceholderText = "Masukkan MAC Bar";
            textBox3.Size = new Size(234, 22);
            textBox3.TabIndex = 1;
            textBox3.TextAlign = HorizontalAlignment.Center;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(textBox3);
            panel1.ForeColor = Color.White;
            panel1.Location = new Point(28, 233);
            panel1.Name = "panel1";
            panel1.Size = new Size(242, 36);
            panel1.TabIndex = 36;
            // 
            // textBox6
            // 
            textBox6.BackColor = Color.White;
            textBox6.BorderStyle = BorderStyle.None;
            textBox6.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox6.ForeColor = Color.Black;
            textBox6.Location = new Point(3, 3);
            textBox6.MaxLength = 10;
            textBox6.Name = "textBox6";
            textBox6.PlaceholderText = "Pin";
            textBox6.Size = new Size(93, 22);
            textBox6.TabIndex = 1;
            textBox6.TextAlign = HorizontalAlignment.Center;
            // 
            // panel6
            // 
            panel6.BackColor = Color.White;
            panel6.BorderStyle = BorderStyle.FixedSingle;
            panel6.Controls.Add(textBox6);
            panel6.ForeColor = Color.White;
            panel6.Location = new Point(275, 233);
            panel6.Name = "panel6";
            panel6.Size = new Size(104, 36);
            panel6.TabIndex = 43;
            // 
            // btnUpdate
            // 
            btnUpdate.BackColor = Color.FromArgb(31, 30, 68);
            btnUpdate.Cursor = Cursors.Hand;
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnUpdate.ForeColor = Color.WhiteSmoke;
            btnUpdate.IconChar = IconChar.Unsplash;
            btnUpdate.IconColor = Color.WhiteSmoke;
            btnUpdate.IconFont = IconFont.Auto;
            btnUpdate.IconSize = 25;
            btnUpdate.Location = new Point(372, 57);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(88, 30);
            btnUpdate.TabIndex = 44;
            btnUpdate.Text = "Update";
            btnUpdate.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnUpdate.UseVisualStyleBackColor = false;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // lblVersionNow
            // 
            lblVersionNow.AutoSize = true;
            lblVersionNow.BackColor = Color.Transparent;
            lblVersionNow.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblVersionNow.Location = new Point(32, 57);
            lblVersionNow.Name = "lblVersionNow";
            lblVersionNow.Size = new Size(73, 15);
            lblVersionNow.TabIndex = 45;
            lblVersionNow.Text = "Your Version";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.BackColor = Color.Transparent;
            lblVersion.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblVersion.Location = new Point(32, 72);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(73, 15);
            lblVersion.TabIndex = 46;
            lblVersion.Text = "Your Version";
            // 
            // lblNewVersion
            // 
            lblNewVersion.AutoSize = true;
            lblNewVersion.BackColor = Color.Transparent;
            lblNewVersion.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblNewVersion.Location = new Point(198, 57);
            lblNewVersion.Name = "lblNewVersion";
            lblNewVersion.Size = new Size(73, 15);
            lblNewVersion.TabIndex = 47;
            lblNewVersion.Text = "New Version";
            // 
            // lblNewVersionNow
            // 
            lblNewVersionNow.AutoSize = true;
            lblNewVersionNow.BackColor = Color.Transparent;
            lblNewVersionNow.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblNewVersionNow.Location = new Point(198, 72);
            lblNewVersionNow.Name = "lblNewVersionNow";
            lblNewVersionNow.Size = new Size(46, 15);
            lblNewVersionNow.TabIndex = 48;
            lblNewVersionNow.Text = "Version";
            // 
            // TestKitchen
            // 
            TestKitchen.BackColor = Color.White;
            TestKitchen.Cursor = Cursors.Hand;
            TestKitchen.FlatAppearance.BorderSize = 0;
            TestKitchen.FlatStyle = FlatStyle.Flat;
            TestKitchen.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            TestKitchen.ForeColor = Color.FromArgb(30, 31, 68);
            TestKitchen.IconChar = IconChar.Bluetooth;
            TestKitchen.IconColor = Color.FromArgb(30, 31, 68);
            TestKitchen.IconFont = IconFont.Auto;
            TestKitchen.IconSize = 25;
            TestKitchen.Location = new Point(355, 113);
            TestKitchen.Name = "TestKitchen";
            TestKitchen.Size = new Size(67, 36);
            TestKitchen.TabIndex = 49;
            TestKitchen.Text = "Test";
            TestKitchen.TextImageRelation = TextImageRelation.ImageBeforeText;
            TestKitchen.UseVisualStyleBackColor = false;
            TestKitchen.Visible = false;
            // 
            // TestKasir
            // 
            TestKasir.BackColor = Color.White;
            TestKasir.Cursor = Cursors.Hand;
            TestKasir.FlatAppearance.BorderSize = 0;
            TestKasir.FlatStyle = FlatStyle.Flat;
            TestKasir.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            TestKasir.ForeColor = Color.FromArgb(30, 31, 68);
            TestKasir.IconChar = IconChar.Bluetooth;
            TestKasir.IconColor = Color.FromArgb(30, 31, 68);
            TestKasir.IconFont = IconFont.Auto;
            TestKasir.IconSize = 25;
            TestKasir.Location = new Point(355, 159);
            TestKasir.Name = "TestKasir";
            TestKasir.Size = new Size(67, 36);
            TestKasir.TabIndex = 50;
            TestKasir.Text = "Test";
            TestKasir.TextImageRelation = TextImageRelation.ImageBeforeText;
            TestKasir.UseVisualStyleBackColor = false;
            TestKasir.Click += TestPrinter;
            // 
            // TestBar
            // 
            TestBar.BackColor = Color.White;
            TestBar.Cursor = Cursors.Hand;
            TestBar.FlatAppearance.BorderSize = 0;
            TestBar.FlatStyle = FlatStyle.Flat;
            TestBar.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            TestBar.ForeColor = Color.FromArgb(30, 31, 68);
            TestBar.IconChar = IconChar.Bluetooth;
            TestBar.IconColor = Color.FromArgb(30, 31, 68);
            TestBar.IconFont = IconFont.Auto;
            TestBar.IconSize = 25;
            TestBar.Location = new Point(355, 223);
            TestBar.Name = "TestBar";
            TestBar.Size = new Size(67, 36);
            TestBar.TabIndex = 51;
            TestBar.Text = "Test";
            TestBar.TextImageRelation = TextImageRelation.ImageBeforeText;
            TestBar.UseVisualStyleBackColor = false;
            TestBar.Visible = false;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = Color.Transparent;
            label7.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label7.Location = new Point(138, 438);
            label7.Name = "label7";
            label7.Size = new Size(198, 15);
            label7.TabIndex = 52;
            label7.Text = "Footer Struk ( Catatan Bawah Struk )";
            // 
            // panel7
            // 
            panel7.BorderStyle = BorderStyle.FixedSingle;
            panel7.Controls.Add(txtFooter);
            panel7.Location = new Point(16, 456);
            panel7.Name = "panel7";
            panel7.Size = new Size(444, 40);
            panel7.TabIndex = 38;
            // 
            // txtFooter
            // 
            txtFooter.BorderStyle = BorderStyle.None;
            txtFooter.Location = new Point(3, 3);
            txtFooter.Multiline = true;
            txtFooter.Name = "txtFooter";
            txtFooter.Size = new Size(436, 32);
            txtFooter.TabIndex = 3;
            txtFooter.TextAlign = HorizontalAlignment.Center;
            // 
            // gradientPanel1
            // 
            gradientPanel1.Angle = 90F;
            gradientPanel1.BottomColor = Color.White;
            gradientPanel1.Controls.Add(iconButton1);
            gradientPanel1.Controls.Add(panel14);
            gradientPanel1.Controls.Add(label12);
            gradientPanel1.Controls.Add(panel7);
            gradientPanel1.Controls.Add(panelPrinterOpt);
            gradientPanel1.Controls.Add(ListMenu);
            gradientPanel1.Controls.Add(sButtonListMenu);
            gradientPanel1.Controls.Add(CacheApp);
            gradientPanel1.Controls.Add(UpdateInfo);
            gradientPanel1.Controls.Add(Redownload);
            gradientPanel1.Controls.Add(iconDual);
            gradientPanel1.Controls.Add(panel3);
            gradientPanel1.Controls.Add(panel2);
            gradientPanel1.Controls.Add(panel4);
            gradientPanel1.Controls.Add(radioDualMonitor);
            gradientPanel1.Controls.Add(label8);
            gradientPanel1.Controls.Add(label7);
            gradientPanel1.Controls.Add(panel5);
            gradientPanel1.Controls.Add(lblVersion);
            gradientPanel1.Controls.Add(lblKitchen);
            gradientPanel1.Controls.Add(lblKasir);
            gradientPanel1.Controls.Add(lblNewVersionNow);
            gradientPanel1.Controls.Add(lblBar);
            gradientPanel1.Controls.Add(lblNewVersion);
            gradientPanel1.Controls.Add(label1);
            gradientPanel1.Controls.Add(label3);
            gradientPanel1.Controls.Add(lblVersionNow);
            gradientPanel1.Controls.Add(label6);
            gradientPanel1.Location = new Point(0, 0);
            gradientPanel1.Name = "gradientPanel1";
            gradientPanel1.Size = new Size(482, 639);
            gradientPanel1.TabIndex = 53;
            gradientPanel1.TopColor = Color.Gainsboro;
            // 
            // panel14
            // 
            panel14.BorderStyle = BorderStyle.FixedSingle;
            panel14.Controls.Add(txtRunningText);
            panel14.Location = new Point(16, 517);
            panel14.Name = "panel14";
            panel14.Size = new Size(444, 40);
            panel14.TabIndex = 53;
            // 
            // txtRunningText
            // 
            txtRunningText.BorderStyle = BorderStyle.None;
            txtRunningText.Location = new Point(3, 3);
            txtRunningText.Multiline = true;
            txtRunningText.Name = "txtRunningText";
            txtRunningText.Size = new Size(436, 32);
            txtRunningText.TabIndex = 3;
            txtRunningText.TextAlign = HorizontalAlignment.Center;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.BackColor = Color.Transparent;
            label12.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label12.Location = new Point(170, 499);
            label12.Name = "label12";
            label12.Size = new Size(148, 15);
            label12.TabIndex = 54;
            label12.Text = "RunningText Dual Monitor";
            // 
            // panelPrinterOpt
            // 
            panelPrinterOpt.BackColor = SystemColors.Control;
            panelPrinterOpt.BorderStyle = BorderStyle.FixedSingle;
            panelPrinterOpt.Controls.Add(lblBluetooth3);
            panelPrinterOpt.Controls.Add(lblBluetooth2);
            panelPrinterOpt.Controls.Add(lblBluetooth1);
            panelPrinterOpt.Controls.Add(panel13);
            panelPrinterOpt.Controls.Add(panel12);
            panelPrinterOpt.Controls.Add(panel11);
            panelPrinterOpt.Controls.Add(label5);
            panelPrinterOpt.Controls.Add(label4);
            panelPrinterOpt.Controls.Add(label2);
            panelPrinterOpt.Controls.Add(TestBar);
            panelPrinterOpt.Controls.Add(panel10);
            panelPrinterOpt.Controls.Add(panel9);
            panelPrinterOpt.Controls.Add(panel8);
            panelPrinterOpt.Controls.Add(ComboBoxPrinter3);
            panelPrinterOpt.Controls.Add(ComboBoxPrinter2);
            panelPrinterOpt.Controls.Add(TestKasir);
            panelPrinterOpt.Controls.Add(TestKitchen);
            panelPrinterOpt.Controls.Add(ComboBoxPrinter1);
            panelPrinterOpt.Location = new Point(16, 101);
            panelPrinterOpt.Name = "panelPrinterOpt";
            panelPrinterOpt.Size = new Size(444, 334);
            panelPrinterOpt.TabIndex = 62;
            // 
            // lblBluetooth3
            // 
            lblBluetooth3.AutoSize = true;
            lblBluetooth3.BackColor = Color.Transparent;
            lblBluetooth3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblBluetooth3.Location = new Point(15, 257);
            lblBluetooth3.Name = "lblBluetooth3";
            lblBluetooth3.Size = new Size(102, 15);
            lblBluetooth3.TabIndex = 71;
            lblBluetooth3.Text = "Bluetooth Manual";
            // 
            // lblBluetooth2
            // 
            lblBluetooth2.AutoSize = true;
            lblBluetooth2.BackColor = Color.Transparent;
            lblBluetooth2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblBluetooth2.Location = new Point(15, 152);
            lblBluetooth2.Name = "lblBluetooth2";
            lblBluetooth2.Size = new Size(102, 15);
            lblBluetooth2.TabIndex = 70;
            lblBluetooth2.Text = "Bluetooth Manual";
            // 
            // lblBluetooth1
            // 
            lblBluetooth1.AutoSize = true;
            lblBluetooth1.BackColor = Color.Transparent;
            lblBluetooth1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblBluetooth1.Location = new Point(12, 48);
            lblBluetooth1.Name = "lblBluetooth1";
            lblBluetooth1.Size = new Size(102, 15);
            lblBluetooth1.TabIndex = 69;
            lblBluetooth1.Text = "Bluetooth Manual";
            // 
            // panel13
            // 
            panel13.BorderStyle = BorderStyle.FixedSingle;
            panel13.Controls.Add(txtPrinter3);
            panel13.Location = new Point(12, 277);
            panel13.Name = "panel13";
            panel13.Size = new Size(183, 36);
            panel13.TabIndex = 68;
            // 
            // txtPrinter3
            // 
            txtPrinter3.BackColor = Color.WhiteSmoke;
            txtPrinter3.BorderStyle = BorderStyle.None;
            txtPrinter3.Location = new Point(-1, 9);
            txtPrinter3.Name = "txtPrinter3";
            txtPrinter3.PlaceholderText = "MacAddressBluetooth";
            txtPrinter3.Size = new Size(182, 16);
            txtPrinter3.TabIndex = 0;
            txtPrinter3.TextAlign = HorizontalAlignment.Center;
            txtPrinter3.TextChanged += txtPrinter3_TextChanged;
            // 
            // panel12
            // 
            panel12.BorderStyle = BorderStyle.FixedSingle;
            panel12.Controls.Add(txtPrinter2);
            panel12.Location = new Point(12, 169);
            panel12.Name = "panel12";
            panel12.Size = new Size(183, 36);
            panel12.TabIndex = 67;
            // 
            // txtPrinter2
            // 
            txtPrinter2.BackColor = Color.WhiteSmoke;
            txtPrinter2.BorderStyle = BorderStyle.None;
            txtPrinter2.Location = new Point(-1, 9);
            txtPrinter2.Name = "txtPrinter2";
            txtPrinter2.PlaceholderText = "MacAddressBluetooth";
            txtPrinter2.Size = new Size(182, 16);
            txtPrinter2.TabIndex = 0;
            txtPrinter2.TextAlign = HorizontalAlignment.Center;
            txtPrinter2.TextChanged += txtPrinter2_TextChanged;
            // 
            // panel11
            // 
            panel11.BorderStyle = BorderStyle.FixedSingle;
            panel11.Controls.Add(txtPrinter1);
            panel11.Location = new Point(13, 65);
            panel11.Name = "panel11";
            panel11.Size = new Size(183, 36);
            panel11.TabIndex = 66;
            // 
            // txtPrinter1
            // 
            txtPrinter1.BackColor = Color.WhiteSmoke;
            txtPrinter1.BorderStyle = BorderStyle.None;
            txtPrinter1.Location = new Point(-1, 9);
            txtPrinter1.Name = "txtPrinter1";
            txtPrinter1.PlaceholderText = "MacAddressBluetooth";
            txtPrinter1.Size = new Size(182, 16);
            txtPrinter1.TabIndex = 0;
            txtPrinter1.TextAlign = HorizontalAlignment.Center;
            txtPrinter1.TextChanged += txtPrinter1_TextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.Transparent;
            label5.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.Location = new Point(13, 213);
            label5.Name = "label5";
            label5.Size = new Size(52, 15);
            label5.TabIndex = 65;
            label5.Text = "Printer 3";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(12, 107);
            label4.Name = "label4";
            label4.Size = new Size(52, 15);
            label4.TabIndex = 64;
            label4.Text = "Printer 2";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(12, 4);
            label2.Name = "label2";
            label2.Size = new Size(180, 15);
            label2.TabIndex = 63;
            label2.Text = "Printer 1( Pilih atas jika tidak ada)";
            // 
            // panel10
            // 
            panel10.BorderStyle = BorderStyle.FixedSingle;
            panel10.Controls.Add(checkBoxCheckerPrinter3);
            panel10.Controls.Add(checkBoxMakananPrinter3);
            panel10.Controls.Add(checkBoxMinumanPrinter3);
            panel10.Controls.Add(checkBoxKasirPrinter3);
            panel10.Location = new Point(213, 223);
            panel10.Name = "panel10";
            panel10.Size = new Size(134, 102);
            panel10.TabIndex = 8;
            // 
            // checkBoxCheckerPrinter3
            // 
            checkBoxCheckerPrinter3.AutoSize = true;
            checkBoxCheckerPrinter3.Location = new Point(9, 30);
            checkBoxCheckerPrinter3.Name = "checkBoxCheckerPrinter3";
            checkBoxCheckerPrinter3.Size = new Size(69, 19);
            checkBoxCheckerPrinter3.TabIndex = 7;
            checkBoxCheckerPrinter3.Text = "Checker";
            checkBoxCheckerPrinter3.UseVisualStyleBackColor = true;
            // 
            // checkBoxMakananPrinter3
            // 
            checkBoxMakananPrinter3.AutoSize = true;
            checkBoxMakananPrinter3.Location = new Point(9, 53);
            checkBoxMakananPrinter3.Name = "checkBoxMakananPrinter3";
            checkBoxMakananPrinter3.Size = new Size(75, 19);
            checkBoxMakananPrinter3.TabIndex = 4;
            checkBoxMakananPrinter3.Text = "Makanan";
            checkBoxMakananPrinter3.UseVisualStyleBackColor = true;
            // 
            // checkBoxMinumanPrinter3
            // 
            checkBoxMinumanPrinter3.AutoSize = true;
            checkBoxMinumanPrinter3.Location = new Point(9, 75);
            checkBoxMinumanPrinter3.Name = "checkBoxMinumanPrinter3";
            checkBoxMinumanPrinter3.Size = new Size(78, 19);
            checkBoxMinumanPrinter3.TabIndex = 5;
            checkBoxMinumanPrinter3.Text = "Minuman";
            checkBoxMinumanPrinter3.UseVisualStyleBackColor = true;
            // 
            // checkBoxKasirPrinter3
            // 
            checkBoxKasirPrinter3.AutoSize = true;
            checkBoxKasirPrinter3.Location = new Point(9, 8);
            checkBoxKasirPrinter3.Name = "checkBoxKasirPrinter3";
            checkBoxKasirPrinter3.Size = new Size(51, 19);
            checkBoxKasirPrinter3.TabIndex = 3;
            checkBoxKasirPrinter3.Text = "Kasir";
            checkBoxKasirPrinter3.UseVisualStyleBackColor = true;
            // 
            // panel9
            // 
            panel9.BorderStyle = BorderStyle.FixedSingle;
            panel9.Controls.Add(checkBoxCheckerPrinter2);
            panel9.Controls.Add(checkBoxMakananPrinter2);
            panel9.Controls.Add(checkBoxMinumanPrinter2);
            panel9.Controls.Add(checkBoxKasirPrinter2);
            panel9.Location = new Point(213, 117);
            panel9.Name = "panel9";
            panel9.Size = new Size(134, 100);
            panel9.TabIndex = 7;
            // 
            // checkBoxCheckerPrinter2
            // 
            checkBoxCheckerPrinter2.AutoSize = true;
            checkBoxCheckerPrinter2.Location = new Point(9, 30);
            checkBoxCheckerPrinter2.Name = "checkBoxCheckerPrinter2";
            checkBoxCheckerPrinter2.Size = new Size(69, 19);
            checkBoxCheckerPrinter2.TabIndex = 6;
            checkBoxCheckerPrinter2.Text = "Checker";
            checkBoxCheckerPrinter2.UseVisualStyleBackColor = true;
            // 
            // checkBoxMakananPrinter2
            // 
            checkBoxMakananPrinter2.AutoSize = true;
            checkBoxMakananPrinter2.Location = new Point(9, 51);
            checkBoxMakananPrinter2.Name = "checkBoxMakananPrinter2";
            checkBoxMakananPrinter2.Size = new Size(75, 19);
            checkBoxMakananPrinter2.TabIndex = 4;
            checkBoxMakananPrinter2.Text = "Makanan";
            checkBoxMakananPrinter2.UseVisualStyleBackColor = true;
            // 
            // checkBoxMinumanPrinter2
            // 
            checkBoxMinumanPrinter2.AutoSize = true;
            checkBoxMinumanPrinter2.Location = new Point(9, 73);
            checkBoxMinumanPrinter2.Name = "checkBoxMinumanPrinter2";
            checkBoxMinumanPrinter2.Size = new Size(78, 19);
            checkBoxMinumanPrinter2.TabIndex = 5;
            checkBoxMinumanPrinter2.Text = "Minuman";
            checkBoxMinumanPrinter2.UseVisualStyleBackColor = true;
            // 
            // checkBoxKasirPrinter2
            // 
            checkBoxKasirPrinter2.AutoSize = true;
            checkBoxKasirPrinter2.Location = new Point(9, 8);
            checkBoxKasirPrinter2.Name = "checkBoxKasirPrinter2";
            checkBoxKasirPrinter2.Size = new Size(51, 19);
            checkBoxKasirPrinter2.TabIndex = 3;
            checkBoxKasirPrinter2.Text = "Kasir";
            checkBoxKasirPrinter2.UseVisualStyleBackColor = true;
            // 
            // panel8
            // 
            panel8.BorderStyle = BorderStyle.FixedSingle;
            panel8.Controls.Add(checkBoxCheckerPrinter1);
            panel8.Controls.Add(checkBoxMakananPrinter1);
            panel8.Controls.Add(checkBoxMinumanPrinter1);
            panel8.Controls.Add(checkBoxKasirPrinter1);
            panel8.Location = new Point(213, 13);
            panel8.Name = "panel8";
            panel8.Size = new Size(134, 97);
            panel8.TabIndex = 6;
            // 
            // checkBoxCheckerPrinter1
            // 
            checkBoxCheckerPrinter1.AutoSize = true;
            checkBoxCheckerPrinter1.Location = new Point(9, 28);
            checkBoxCheckerPrinter1.Name = "checkBoxCheckerPrinter1";
            checkBoxCheckerPrinter1.Size = new Size(69, 19);
            checkBoxCheckerPrinter1.TabIndex = 7;
            checkBoxCheckerPrinter1.Text = "Checker";
            checkBoxCheckerPrinter1.UseVisualStyleBackColor = true;
            // 
            // checkBoxMakananPrinter1
            // 
            checkBoxMakananPrinter1.AutoSize = true;
            checkBoxMakananPrinter1.Location = new Point(9, 48);
            checkBoxMakananPrinter1.Name = "checkBoxMakananPrinter1";
            checkBoxMakananPrinter1.Size = new Size(75, 19);
            checkBoxMakananPrinter1.TabIndex = 4;
            checkBoxMakananPrinter1.Text = "Makanan";
            checkBoxMakananPrinter1.UseVisualStyleBackColor = true;
            // 
            // checkBoxMinumanPrinter1
            // 
            checkBoxMinumanPrinter1.AutoSize = true;
            checkBoxMinumanPrinter1.Location = new Point(9, 70);
            checkBoxMinumanPrinter1.Name = "checkBoxMinumanPrinter1";
            checkBoxMinumanPrinter1.Size = new Size(78, 19);
            checkBoxMinumanPrinter1.TabIndex = 5;
            checkBoxMinumanPrinter1.Text = "Minuman";
            checkBoxMinumanPrinter1.UseVisualStyleBackColor = true;
            // 
            // checkBoxKasirPrinter1
            // 
            checkBoxKasirPrinter1.AutoSize = true;
            checkBoxKasirPrinter1.Location = new Point(9, 8);
            checkBoxKasirPrinter1.Name = "checkBoxKasirPrinter1";
            checkBoxKasirPrinter1.Size = new Size(51, 19);
            checkBoxKasirPrinter1.TabIndex = 3;
            checkBoxKasirPrinter1.Text = "Kasir";
            checkBoxKasirPrinter1.UseVisualStyleBackColor = true;
            // 
            // ComboBoxPrinter3
            // 
            ComboBoxPrinter3.FormattingEnabled = true;
            ComboBoxPrinter3.Location = new Point(12, 231);
            ComboBoxPrinter3.Name = "ComboBoxPrinter3";
            ComboBoxPrinter3.Size = new Size(183, 23);
            ComboBoxPrinter3.TabIndex = 2;
            // 
            // ComboBoxPrinter2
            // 
            ComboBoxPrinter2.FormattingEnabled = true;
            ComboBoxPrinter2.Location = new Point(12, 125);
            ComboBoxPrinter2.Name = "ComboBoxPrinter2";
            ComboBoxPrinter2.Size = new Size(183, 23);
            ComboBoxPrinter2.TabIndex = 1;
            // 
            // ComboBoxPrinter1
            // 
            ComboBoxPrinter1.FormattingEnabled = true;
            ComboBoxPrinter1.Location = new Point(13, 22);
            ComboBoxPrinter1.Name = "ComboBoxPrinter1";
            ComboBoxPrinter1.Size = new Size(183, 23);
            ComboBoxPrinter1.TabIndex = 0;
            // 
            // ListMenu
            // 
            ListMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ListMenu.AutoSize = true;
            ListMenu.BackColor = Color.Transparent;
            ListMenu.BackgroundImageLayout = ImageLayout.None;
            ListMenu.FlatAppearance.BorderSize = 0;
            ListMenu.FlatStyle = FlatStyle.Flat;
            ListMenu.ForeColor = Color.Transparent;
            ListMenu.IconChar = IconChar.List;
            ListMenu.IconColor = Color.FromArgb(31, 30, 68);
            ListMenu.IconFont = IconFont.Auto;
            ListMenu.IconSize = 30;
            ListMenu.Location = new Point(109, 574);
            ListMenu.Name = "ListMenu";
            ListMenu.Size = new Size(36, 36);
            ListMenu.TabIndex = 58;
            ListMenu.UseVisualStyleBackColor = false;
            // 
            // sButtonListMenu
            // 
            sButtonListMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sButtonListMenu.AutoSize = true;
            sButtonListMenu.BackColor = Color.White;
            sButtonListMenu.FlatStyle = FlatStyle.Flat;
            sButtonListMenu.Location = new Point(105, 612);
            sButtonListMenu.MinimumSize = new Size(45, 22);
            sButtonListMenu.Name = "sButtonListMenu";
            sButtonListMenu.OffBackColor = Color.Gray;
            sButtonListMenu.OffToggleColor = Color.Gainsboro;
            sButtonListMenu.OnBackColor = Color.FromArgb(31, 30, 68);
            sButtonListMenu.OnToggleColor = Color.WhiteSmoke;
            sButtonListMenu.Size = new Size(45, 22);
            sButtonListMenu.TabIndex = 57;
            sButtonListMenu.UseVisualStyleBackColor = false;
            sButtonListMenu.CheckedChanged += sButtonListMenu_CheckedChanged;
            // 
            // CacheApp
            // 
            CacheApp.BackColor = Color.Transparent;
            CacheApp.Cursor = Cursors.Hand;
            CacheApp.FlatAppearance.BorderSize = 0;
            CacheApp.FlatStyle = FlatStyle.Flat;
            CacheApp.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CacheApp.ForeColor = Color.FromArgb(31, 30, 68);
            CacheApp.IconChar = IconChar.Skyatlas;
            CacheApp.IconColor = Color.FromArgb(31, 30, 68);
            CacheApp.IconFont = IconFont.Auto;
            CacheApp.IconSize = 30;
            CacheApp.Location = new Point(156, 574);
            CacheApp.Name = "CacheApp";
            CacheApp.Size = new Size(52, 54);
            CacheApp.TabIndex = 56;
            CacheApp.Text = "Sync";
            CacheApp.TextImageRelation = TextImageRelation.ImageAboveText;
            CacheApp.UseVisualStyleBackColor = false;
            CacheApp.Click += CacheApp_Click;
            // 
            // UpdateInfo
            // 
            UpdateInfo.BackColor = Color.White;
            UpdateInfo.Cursor = Cursors.Hand;
            UpdateInfo.FlatAppearance.BorderSize = 0;
            UpdateInfo.FlatStyle = FlatStyle.Popup;
            UpdateInfo.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            UpdateInfo.ForeColor = Color.WhiteSmoke;
            UpdateInfo.IconChar = IconChar.IceCream;
            UpdateInfo.IconColor = Color.FromArgb(31, 30, 68);
            UpdateInfo.IconFont = IconFont.Auto;
            UpdateInfo.IconSize = 20;
            UpdateInfo.Location = new Point(330, 57);
            UpdateInfo.Name = "UpdateInfo";
            UpdateInfo.Size = new Size(33, 30);
            UpdateInfo.TabIndex = 54;
            UpdateInfo.TextImageRelation = TextImageRelation.ImageBeforeText;
            UpdateInfo.UseVisualStyleBackColor = false;
            UpdateInfo.Click += UpdateInfo_Click;
            // 
            // Redownload
            // 
            Redownload.BackColor = Color.Transparent;
            Redownload.Cursor = Cursors.Hand;
            Redownload.FlatAppearance.BorderSize = 0;
            Redownload.FlatStyle = FlatStyle.Flat;
            Redownload.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Redownload.ForeColor = Color.FromArgb(31, 30, 68);
            Redownload.IconChar = IconChar.ArrowRightRotate;
            Redownload.IconColor = Color.FromArgb(31, 30, 68);
            Redownload.IconFont = IconFont.Auto;
            Redownload.IconSize = 30;
            Redownload.Location = new Point(214, 574);
            Redownload.Name = "Redownload";
            Redownload.Size = new Size(52, 54);
            Redownload.TabIndex = 54;
            Redownload.Text = "Reset";
            Redownload.TextImageRelation = TextImageRelation.ImageAboveText;
            Redownload.UseVisualStyleBackColor = false;
            Redownload.Click += Redownload_Click;
            // 
            // iconDual
            // 
            iconDual.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iconDual.AutoSize = true;
            iconDual.BackColor = Color.Transparent;
            iconDual.BackgroundImageLayout = ImageLayout.None;
            iconDual.FlatAppearance.BorderSize = 0;
            iconDual.FlatStyle = FlatStyle.Flat;
            iconDual.ForeColor = Color.Transparent;
            iconDual.IconChar = IconChar.DesktopAlt;
            iconDual.IconColor = Color.FromArgb(31, 30, 68);
            iconDual.IconFont = IconFont.Auto;
            iconDual.IconSize = 30;
            iconDual.Location = new Point(286, 574);
            iconDual.Name = "iconDual";
            iconDual.Size = new Size(36, 36);
            iconDual.TabIndex = 55;
            iconDual.UseVisualStyleBackColor = false;
            iconDual.Click += iconDual_Click;
            // 
            // panel3
            // 
            panel3.BackColor = Color.White;
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(textBox4);
            panel3.ForeColor = Color.White;
            panel3.Location = new Point(275, 176);
            panel3.Name = "panel3";
            panel3.Size = new Size(104, 36);
            panel3.TabIndex = 39;
            // 
            // textBox4
            // 
            textBox4.BackColor = Color.White;
            textBox4.BorderStyle = BorderStyle.None;
            textBox4.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox4.ForeColor = Color.Black;
            textBox4.Location = new Point(3, 3);
            textBox4.MaxLength = 10;
            textBox4.Name = "textBox4";
            textBox4.PlaceholderText = "Pin";
            textBox4.Size = new Size(93, 22);
            textBox4.TabIndex = 1;
            textBox4.TextAlign = HorizontalAlignment.Center;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(textBox1);
            panel2.ForeColor = Color.White;
            panel2.Location = new Point(28, 176);
            panel2.Name = "panel2";
            panel2.Size = new Size(242, 36);
            panel2.TabIndex = 37;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.White;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox1.ForeColor = Color.Black;
            textBox1.Location = new Point(3, 3);
            textBox1.MaxLength = 40;
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "Masukkan MAC Kitchen";
            textBox1.Size = new Size(234, 22);
            textBox1.TabIndex = 1;
            textBox1.TextAlign = HorizontalAlignment.Center;
            // 
            // panel4
            // 
            panel4.BackColor = Color.White;
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(textBox5);
            panel4.ForeColor = Color.White;
            panel4.Location = new Point(275, 119);
            panel4.Name = "panel4";
            panel4.Size = new Size(104, 36);
            panel4.TabIndex = 41;
            // 
            // textBox5
            // 
            textBox5.BackColor = Color.White;
            textBox5.BorderStyle = BorderStyle.None;
            textBox5.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox5.ForeColor = Color.Black;
            textBox5.Location = new Point(3, 3);
            textBox5.MaxLength = 10;
            textBox5.Name = "textBox5";
            textBox5.PlaceholderText = "Pin";
            textBox5.Size = new Size(93, 22);
            textBox5.TabIndex = 1;
            textBox5.TextAlign = HorizontalAlignment.Center;
            // 
            // radioDualMonitor
            // 
            radioDualMonitor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            radioDualMonitor.AutoSize = true;
            radioDualMonitor.BackColor = Color.White;
            radioDualMonitor.FlatStyle = FlatStyle.Flat;
            radioDualMonitor.Location = new Point(282, 612);
            radioDualMonitor.MinimumSize = new Size(45, 22);
            radioDualMonitor.Name = "radioDualMonitor";
            radioDualMonitor.OffBackColor = Color.Gray;
            radioDualMonitor.OffToggleColor = Color.Gainsboro;
            radioDualMonitor.OnBackColor = Color.FromArgb(31, 30, 68);
            radioDualMonitor.OnToggleColor = Color.WhiteSmoke;
            radioDualMonitor.Size = new Size(45, 22);
            radioDualMonitor.TabIndex = 54;
            radioDualMonitor.UseVisualStyleBackColor = false;
            radioDualMonitor.CheckedChanged += radioDualMonitor_CheckedChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.BackColor = Color.Transparent;
            label8.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label8.Location = new Point(214, 556);
            label8.Name = "label8";
            label8.Size = new Size(50, 15);
            label8.TabIndex = 53;
            label8.Text = "Settings";
            // 
            // panel5
            // 
            panel5.BackColor = Color.White;
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Controls.Add(textBox2);
            panel5.ForeColor = Color.White;
            panel5.Location = new Point(28, 119);
            panel5.Name = "panel5";
            panel5.Size = new Size(242, 36);
            panel5.TabIndex = 35;
            // 
            // textBox2
            // 
            textBox2.BackColor = Color.White;
            textBox2.BorderStyle = BorderStyle.None;
            textBox2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox2.ForeColor = Color.Black;
            textBox2.Location = new Point(3, 3);
            textBox2.MaxLength = 40;
            textBox2.Name = "textBox2";
            textBox2.PlaceholderText = "Masukkan MAC Kasir";
            textBox2.Size = new Size(234, 22);
            textBox2.TabIndex = 1;
            textBox2.TextAlign = HorizontalAlignment.Center;
            // 
            // lblKitchen
            // 
            lblKitchen.AutoSize = true;
            lblKitchen.BackColor = Color.Transparent;
            lblKitchen.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblKitchen.Location = new Point(32, 158);
            lblKitchen.Name = "lblKitchen";
            lblKitchen.Size = new Size(221, 15);
            lblKitchen.TabIndex = 28;
            lblKitchen.Text = "MAC Address Kitchen ( Hanya Makanan)";
            // 
            // lblKasir
            // 
            lblKasir.AutoSize = true;
            lblKasir.BackColor = Color.Transparent;
            lblKasir.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblKasir.Location = new Point(32, 101);
            lblKasir.Name = "lblKasir";
            lblKasir.Size = new Size(205, 15);
            lblKasir.TabIndex = 30;
            lblKasir.Text = "MAC Address Kasir ( Struk && Checker)";
            // 
            // lblBar
            // 
            lblBar.AutoSize = true;
            lblBar.BackColor = Color.Transparent;
            lblBar.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblBar.Location = new Point(28, 215);
            lblBar.Name = "lblBar";
            lblBar.Size = new Size(201, 15);
            lblBar.TabIndex = 32;
            lblBar.Text = "MAC Address Bar ( Hanya Minuman)";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(275, 101);
            label1.Name = "label1";
            label1.Size = new Size(108, 15);
            label1.TabIndex = 38;
            label1.Text = "Pin (Default: 0000)";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(275, 158);
            label3.Name = "label3";
            label3.Size = new Size(108, 15);
            label3.TabIndex = 40;
            label3.Text = "Pin (Default: 0000)";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = Color.Transparent;
            label6.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label6.Location = new Point(275, 215);
            label6.Name = "label6";
            label6.Size = new Size(108, 15);
            label6.TabIndex = 42;
            label6.Text = "Pin (Default: 0000)";
            // 
            // iconButton1
            // 
            iconButton1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iconButton1.AutoSize = true;
            iconButton1.BackColor = Color.Transparent;
            iconButton1.Cursor = Cursors.No;
            iconButton1.FlatAppearance.BorderSize = 0;
            iconButton1.FlatStyle = FlatStyle.Flat;
            iconButton1.ForeColor = Color.FromArgb(31, 30, 68);
            iconButton1.IconChar = IconChar.Gear;
            iconButton1.IconColor = Color.FromArgb(31, 30, 68);
            iconButton1.IconFont = IconFont.Auto;
            iconButton1.IconSize = 30;
            iconButton1.Location = new Point(343, 573);
            iconButton1.Name = "iconButton1";
            iconButton1.Size = new Size(53, 55);
            iconButton1.TabIndex = 63;
            iconButton1.Text = "Config";
            iconButton1.TextImageRelation = TextImageRelation.ImageAboveText;
            iconButton1.UseVisualStyleBackColor = false;
            iconButton1.Click += iconButton1_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(482, 635);
            ControlBox = false;
            Controls.Add(btnUpdate);
            Controls.Add(Button2);
            Controls.Add(Button1);
            Controls.Add(gradientPanel1);
            Controls.Add(panel1);
            Controls.Add(panel6);
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DT-Setting";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            gradientPanel1.ResumeLayout(false);
            gradientPanel1.PerformLayout();
            panel14.ResumeLayout(false);
            panel14.PerformLayout();
            panelPrinterOpt.ResumeLayout(false);
            panelPrinterOpt.PerformLayout();
            panel13.ResumeLayout(false);
            panel13.PerformLayout();
            panel12.ResumeLayout(false);
            panel12.PerformLayout();
            panel11.ResumeLayout(false);
            panel11.PerformLayout();
            panel10.ResumeLayout(false);
            panel10.PerformLayout();
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private FontAwesome.Sharp.IconButton Button2;
        private FontAwesome.Sharp.IconButton Button1;
        private TextBox textBox3;
        private Panel panel1;
        private TextBox textBox6;
        private Panel panel6;
        private IconButton btnUpdate;
        private Label lblVersionNow;
        private Label lblVersion;
        private Label lblNewVersion;
        private Label lblNewVersionNow;
        private IconButton TestKitchen;
        private IconButton TestKasir;
        private IconButton TestBar;
        private Label label7;
        private Panel panel7;
        private TextBox txtFooter;
        private Model.GradientPanel gradientPanel1;
        private Label label8;
        private IconButton iconDual;
        private Model.SButton radioDualMonitor;
        private IconButton Redownload;
        private IconButton UpdateInfo;
        private IconButton CacheApp;
        private IconButton ListMenu;
        private Model.SButton sButtonListMenu;
        private Panel panelPrinterOpt;
        private ComboBox ComboBoxPrinter3;
        private ComboBox ComboBoxPrinter2;
        private ComboBox ComboBoxPrinter1;
        private Panel panel3;
        private TextBox textBox4;
        private Panel panel2;
        private TextBox textBox1;
        private Panel panel4;
        private TextBox textBox5;
        private Panel panel5;
        private TextBox textBox2;
        private Label lblKitchen;
        private Label lblKasir;
        private Label lblBar;
        private Label label1;
        private Label label3;
        private Label label6;
        private Panel panel8;
        private CheckBox checkBoxMakananPrinter1;
        private CheckBox checkBoxMinumanPrinter1;
        private CheckBox checkBoxKasirPrinter1;
        private Panel panel9;
        private CheckBox checkBoxMakananPrinter2;
        private CheckBox checkBoxMinumanPrinter2;
        private CheckBox checkBoxKasirPrinter2;
        private Panel panel10;
        private CheckBox checkBoxMakananPrinter3;
        private CheckBox checkBoxMinumanPrinter3;
        private CheckBox checkBoxKasirPrinter3;
        private Label label5;
        private Label label4;
        private Label label2;
        private CheckBox checkBoxCheckerPrinter3;
        private CheckBox checkBoxCheckerPrinter2;
        private CheckBox checkBoxCheckerPrinter1;
        private Label lblBluetooth1;
        private Panel panel13;
        private TextBox txtPrinter3;
        private Panel panel12;
        private TextBox txtPrinter2;
        private Panel panel11;
        private TextBox txtPrinter1;
        private Label lblBluetooth3;
        private Label lblBluetooth2;
        private Panel panel14;
        private TextBox txtRunningText;
        private Label label12;
        private IconButton iconButton1;
    }
}