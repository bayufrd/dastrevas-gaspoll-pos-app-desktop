using FontAwesome.Sharp;
namespace KASIR.OfflineMode
{
    partial class Offline_masterPos
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle9 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            panel3 = new Panel();
            lblDetailKeranjang = new IconButton();
            iconButtonHps = new IconButton();
            listBill1 = new IconButton();
            panel6 = new Panel();
            label6 = new Label();
            iconButtonGet = new IconButton();
            label7 = new Label();
            lblDiskon1 = new Label();
            label1 = new Label();
            panel9 = new Panel();
            cmbDiskon = new ComboBox();
            lblTotal1 = new Label();
            lblSubTotal1 = new Label();
            lblTotal = new Label();
            lblSubTotal = new Label();
            dataGridView1 = new DataGridView();
            panel1 = new Panel();
            ButtonSplit = new IconButton();
            iconButton1 = new IconButton();
            iconButton3 = new IconButton();
            ButtonSimpan = new IconButton();
            panel4 = new Panel();
            label3 = new Label();
            textBox2 = new TextBox();
            button4 = new Button();
            panel7 = new Panel();
            cmbFilter = new ComboBox();
            panel5 = new Panel();
            txtCariMenuList = new TextBox();
            pictureBox2 = new PictureBox();
            txtCariMenu = new TextBox();
            panel2 = new Panel();
            dataGridView2 = new DataGridView();
            dataGridView3 = new FlowLayoutPanel();
            panel8 = new Panel();
            btnCari = new IconButton();
            lblCountingItems = new Label();
            label9 = new Label();
            label5 = new Label();
            label4 = new Label();
            iconDropDownButton1 = new IconDropDownButton();
            menuBindingSource = new BindingSource(components);
            panel3.SuspendLayout();
            panel6.SuspendLayout();
            panel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel1.SuspendLayout();
            panel4.SuspendLayout();
            panel7.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            panel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)menuBindingSource).BeginInit();
            SuspendLayout();
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panel3.BackColor = Color.WhiteSmoke;
            panel3.Controls.Add(lblDetailKeranjang);
            panel3.Controls.Add(iconButtonHps);
            panel3.Controls.Add(listBill1);
            panel3.Controls.Add(panel6);
            panel3.Controls.Add(dataGridView1);
            panel3.Controls.Add(panel1);
            panel3.Controls.Add(panel4);
            panel3.ForeColor = Color.White;
            panel3.Location = new Point(619, 9);
            panel3.Name = "panel3";
            panel3.Padding = new Padding(5);
            panel3.Size = new Size(410, 657);
            panel3.TabIndex = 8;
            panel3.Paint += panel3_Paint;
            // 
            // lblDetailKeranjang
            // 
            lblDetailKeranjang.BackColor = Color.White;
            lblDetailKeranjang.FlatStyle = FlatStyle.Flat;
            lblDetailKeranjang.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblDetailKeranjang.ForeColor = Color.FromArgb(31, 30, 68);
            lblDetailKeranjang.IconChar = IconChar.CartArrowDown;
            lblDetailKeranjang.IconColor = Color.FromArgb(31, 30, 68);
            lblDetailKeranjang.IconFont = IconFont.Auto;
            lblDetailKeranjang.IconSize = 30;
            lblDetailKeranjang.ImageAlign = ContentAlignment.MiddleRight;
            lblDetailKeranjang.Location = new Point(2, 60);
            lblDetailKeranjang.Name = "lblDetailKeranjang";
            lblDetailKeranjang.Size = new Size(402, 49);
            lblDetailKeranjang.TabIndex = 1;
            lblDetailKeranjang.Text = "Keranjang :";
            lblDetailKeranjang.TextImageRelation = TextImageRelation.ImageBeforeText;
            lblDetailKeranjang.UseVisualStyleBackColor = false;
            lblDetailKeranjang.Click += lblDetailKeranjang_Click;
            // 
            // iconButtonHps
            // 
            iconButtonHps.FlatAppearance.BorderSize = 0;
            iconButtonHps.FlatStyle = FlatStyle.Flat;
            iconButtonHps.ForeColor = Color.DarkRed;
            iconButtonHps.IconChar = IconChar.TrashRestore;
            iconButtonHps.IconColor = Color.DarkRed;
            iconButtonHps.IconFont = IconFont.Auto;
            iconButtonHps.IconSize = 30;
            iconButtonHps.ImageAlign = ContentAlignment.MiddleRight;
            iconButtonHps.Location = new Point(203, 3);
            iconButtonHps.Name = "iconButtonHps";
            iconButtonHps.Size = new Size(200, 47);
            iconButtonHps.TabIndex = 19;
            iconButtonHps.Text = "Hapus Pesanan";
            iconButtonHps.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButtonHps.UseVisualStyleBackColor = true;
            iconButtonHps.Click += button5_ClickAsync;
            // 
            // listBill1
            // 
            listBill1.Enabled = false;
            listBill1.FlatAppearance.BorderSize = 0;
            listBill1.FlatStyle = FlatStyle.Flat;
            listBill1.ForeColor = Color.FromArgb(31, 30, 68);
            listBill1.IconChar = IconChar.List;
            listBill1.IconColor = Color.FromArgb(31, 30, 68);
            listBill1.IconFont = IconFont.Auto;
            listBill1.IconSize = 30;
            listBill1.ImageAlign = ContentAlignment.MiddleRight;
            listBill1.Location = new Point(2, 3);
            listBill1.Name = "listBill1";
            listBill1.Size = new Size(201, 47);
            listBill1.TabIndex = 18;
            listBill1.Text = "List Bill (Disable)";
            listBill1.TextImageRelation = TextImageRelation.ImageBeforeText;
            listBill1.UseVisualStyleBackColor = true;
            listBill1.Click += listBill_Click;
            // 
            // panel6
            // 
            panel6.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panel6.BackColor = Color.White;
            panel6.Controls.Add(label6);
            panel6.Controls.Add(iconButtonGet);
            panel6.Controls.Add(label7);
            panel6.Controls.Add(lblDiskon1);
            panel6.Controls.Add(label1);
            panel6.Controls.Add(panel9);
            panel6.Controls.Add(lblTotal1);
            panel6.Controls.Add(lblSubTotal1);
            panel6.Controls.Add(lblTotal);
            panel6.Controls.Add(lblSubTotal);
            panel6.Location = new Point(3, 412);
            panel6.Name = "panel6";
            panel6.Size = new Size(401, 135);
            panel6.TabIndex = 15;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = Color.White;
            label6.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label6.ForeColor = Color.FromArgb(31, 30, 68);
            label6.Location = new Point(41, 22);
            label6.Name = "label6";
            label6.Size = new Size(71, 21);
            label6.TabIndex = 61;
            label6.Text = "Discount";
            // 
            // iconButtonGet
            // 
            iconButtonGet.BackColor = Color.White;
            iconButtonGet.FlatAppearance.BorderSize = 0;
            iconButtonGet.FlatStyle = FlatStyle.Flat;
            iconButtonGet.Font = new Font("Segoe UI Semibold", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
            iconButtonGet.ForeColor = Color.FromArgb(31, 30, 68);
            iconButtonGet.IconChar = IconChar.Tag;
            iconButtonGet.IconColor = Color.FromArgb(31, 30, 68);
            iconButtonGet.IconFont = IconFont.Auto;
            iconButtonGet.IconSize = 20;
            iconButtonGet.Location = new Point(284, 20);
            iconButtonGet.Name = "iconButtonGet";
            iconButtonGet.Size = new Size(113, 23);
            iconButtonGet.TabIndex = 23;
            iconButtonGet.Text = "Gunakan Disc";
            iconButtonGet.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButtonGet.UseVisualStyleBackColor = false;
            iconButtonGet.Click += btnGet_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = Color.White;
            label7.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label7.ForeColor = Color.FromArgb(31, 30, 68);
            label7.Location = new Point(3, 22);
            label7.Name = "label7";
            label7.Size = new Size(40, 21);
            label7.TabIndex = 60;
            label7.Text = "Pilih";
            // 
            // lblDiskon1
            // 
            lblDiskon1.AutoSize = true;
            lblDiskon1.BackColor = Color.White;
            lblDiskon1.ForeColor = Color.FromArgb(31, 30, 68);
            lblDiskon1.Location = new Point(272, 66);
            lblDiskon1.Name = "lblDiskon1";
            lblDiskon1.Size = new Size(51, 15);
            lblDiskon1.TabIndex = 9;
            lblDiskon1.Text = "- Diskon";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.White;
            label1.ForeColor = Color.FromArgb(31, 30, 68);
            label1.Location = new Point(3, 63);
            label1.Name = "label1";
            label1.Size = new Size(49, 15);
            label1.TabIndex = 5;
            label1.Text = "Diskon :";
            // 
            // panel9
            // 
            panel9.Controls.Add(cmbDiskon);
            panel9.Location = new Point(110, 11);
            panel9.Name = "panel9";
            panel9.Size = new Size(168, 41);
            panel9.TabIndex = 4;
            // 
            // cmbDiskon
            // 
            cmbDiskon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDiskon.FlatStyle = FlatStyle.Flat;
            cmbDiskon.FormattingEnabled = true;
            cmbDiskon.Location = new Point(0, 9);
            cmbDiskon.Name = "cmbDiskon";
            cmbDiskon.Size = new Size(168, 23);
            cmbDiskon.TabIndex = 1;
            cmbDiskon.SelectedIndexChanged += cmbDiskon_SelectedIndexChanged;
            // 
            // lblTotal1
            // 
            lblTotal1.AutoSize = true;
            lblTotal1.BackColor = Color.White;
            lblTotal1.ForeColor = Color.FromArgb(31, 30, 68);
            lblTotal1.Location = new Point(273, 112);
            lblTotal1.Name = "lblTotal1";
            lblTotal1.Size = new Size(32, 15);
            lblTotal1.TabIndex = 3;
            lblTotal1.Text = "Total";
            lblTotal1.Click += lblTotal1_Click;
            // 
            // lblSubTotal1
            // 
            lblSubTotal1.AutoSize = true;
            lblSubTotal1.BackColor = Color.White;
            lblSubTotal1.ForeColor = Color.FromArgb(31, 30, 68);
            lblSubTotal1.Location = new Point(273, 87);
            lblSubTotal1.Name = "lblSubTotal1";
            lblSubTotal1.Size = new Size(52, 15);
            lblSubTotal1.TabIndex = 2;
            lblSubTotal1.Text = "SubTotal";
            lblSubTotal1.Click += lblSubTotal1_Click;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.BackColor = Color.White;
            lblTotal.ForeColor = Color.FromArgb(31, 30, 68);
            lblTotal.Location = new Point(3, 112);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(41, 15);
            lblTotal.TabIndex = 1;
            lblTotal.Text = "Total : ";
            // 
            // lblSubTotal
            // 
            lblSubTotal.AutoSize = true;
            lblSubTotal.BackColor = Color.White;
            lblSubTotal.ForeColor = Color.FromArgb(31, 30, 68);
            lblSubTotal.Location = new Point(3, 86);
            lblSubTotal.Name = "lblSubTotal";
            lblSubTotal.Size = new Size(58, 15);
            lblSubTotal.TabIndex = 0;
            lblSubTotal.Text = "SubTotal :";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.ForeColor = Color.Silver;
            dataGridViewCellStyle1.SelectionBackColor = Color.Transparent;
            dataGridViewCellStyle1.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.Transparent;
            dataGridViewCellStyle2.SelectionForeColor = Color.Transparent;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.ColumnHeadersHeight = 30;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.ColumnHeadersVisible = false;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = Color.White;
            dataGridViewCellStyle3.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle3.SelectionForeColor = Color.Gainsboro;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.FromArgb(31, 30, 68);
            dataGridView1.ImeMode = ImeMode.NoControl;
            dataGridView1.Location = new Point(3, 115);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.White;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle4.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle4.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle5.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle5.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dataGridView1.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowTemplate.DefaultCellStyle.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(400, 291);
            dataGridView1.TabIndex = 14;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellPainting += DataGridView1_CellPainting;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panel1.Controls.Add(ButtonSplit);
            panel1.Controls.Add(iconButton1);
            panel1.Controls.Add(iconButton3);
            panel1.Controls.Add(ButtonSimpan);
            panel1.Location = new Point(3, 553);
            panel1.Name = "panel1";
            panel1.Size = new Size(401, 104);
            panel1.TabIndex = 8;
            // 
            // ButtonSplit
            // 
            ButtonSplit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ButtonSplit.Enabled = false;
            ButtonSplit.FlatAppearance.BorderSize = 0;
            ButtonSplit.FlatStyle = FlatStyle.Flat;
            ButtonSplit.ForeColor = Color.FromArgb(31, 30, 68);
            ButtonSplit.IconChar = IconChar.Receipt;
            ButtonSplit.IconColor = Color.FromArgb(31, 30, 68);
            ButtonSplit.IconFont = IconFont.Auto;
            ButtonSplit.IconSize = 25;
            ButtonSplit.ImageAlign = ContentAlignment.MiddleRight;
            ButtonSplit.Location = new Point(4, 14);
            ButtonSplit.Name = "ButtonSplit";
            ButtonSplit.Size = new Size(154, 35);
            ButtonSplit.TabIndex = 23;
            ButtonSplit.Text = "Split Bill (Disable)";
            ButtonSplit.TextImageRelation = TextImageRelation.ImageBeforeText;
            ButtonSplit.UseVisualStyleBackColor = true;
            ButtonSplit.Click += ButtonSplit_Click;
            // 
            // iconButton1
            // 
            iconButton1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            iconButton1.BackColor = Color.FromArgb(30, 31, 68);
            iconButton1.Cursor = Cursors.Hand;
            iconButton1.FlatAppearance.BorderSize = 0;
            iconButton1.FlatStyle = FlatStyle.Flat;
            iconButton1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            iconButton1.ForeColor = Color.Gainsboro;
            iconButton1.IconChar = IconChar.Donate;
            iconButton1.IconColor = Color.Gainsboro;
            iconButton1.IconFont = IconFont.Auto;
            iconButton1.IconSize = 30;
            iconButton1.ImageAlign = ContentAlignment.MiddleRight;
            iconButton1.Location = new Point(5, 52);
            iconButton1.Name = "iconButton1";
            iconButton1.Size = new Size(392, 49);
            iconButton1.TabIndex = 22;
            iconButton1.Text = "Bayar";
            iconButton1.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButton1.UseVisualStyleBackColor = false;
            iconButton1.Click += button1_Click;
            // 
            // iconButton3
            // 
            iconButton3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            iconButton3.FlatAppearance.BorderSize = 0;
            iconButton3.FlatStyle = FlatStyle.Flat;
            iconButton3.ForeColor = Color.FromArgb(31, 30, 68);
            iconButton3.IconChar = IconChar.Tags;
            iconButton3.IconColor = Color.FromArgb(31, 30, 68);
            iconButton3.IconFont = IconFont.Auto;
            iconButton3.IconSize = 30;
            iconButton3.Location = new Point(327, 14);
            iconButton3.Name = "iconButton3";
            iconButton3.Size = new Size(70, 35);
            iconButton3.TabIndex = 21;
            iconButton3.UseVisualStyleBackColor = true;
            iconButton3.Click += button3_Click;
            // 
            // ButtonSimpan
            // 
            ButtonSimpan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ButtonSimpan.Enabled = false;
            ButtonSimpan.FlatAppearance.BorderSize = 0;
            ButtonSimpan.FlatStyle = FlatStyle.Flat;
            ButtonSimpan.ForeColor = Color.FromArgb(31, 30, 68);
            ButtonSimpan.IconChar = IconChar.FileDownload;
            ButtonSimpan.IconColor = Color.FromArgb(31, 30, 68);
            ButtonSimpan.IconFont = IconFont.Auto;
            ButtonSimpan.IconSize = 25;
            ButtonSimpan.ImageAlign = ContentAlignment.MiddleRight;
            ButtonSimpan.Location = new Point(163, 14);
            ButtonSimpan.Name = "ButtonSimpan";
            ButtonSimpan.Size = new Size(160, 35);
            ButtonSimpan.TabIndex = 20;
            ButtonSimpan.Text = "Simpan Bill (Disable)";
            ButtonSimpan.TextImageRelation = TextImageRelation.ImageBeforeText;
            ButtonSimpan.UseVisualStyleBackColor = true;
            ButtonSimpan.Click += SimpanBill_Click;
            // 
            // panel4
            // 
            panel4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel4.Controls.Add(label3);
            panel4.Controls.Add(textBox2);
            panel4.Controls.Add(button4);
            panel4.Location = new Point(13, 975);
            panel4.Name = "panel4";
            panel4.Size = new Size(594, 128);
            panel4.TabIndex = 0;
            // 
            // label3
            // 
            label3.Font = new Font("Segoe UI Black", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label3.ForeColor = Color.FromArgb(31, 30, 68);
            label3.Location = new Point(4, 0);
            label3.Name = "label3";
            label3.Size = new Size(100, 23);
            label3.TabIndex = 7;
            label3.Text = "Total";
            // 
            // textBox2
            // 
            textBox2.Anchor = AnchorStyles.None;
            textBox2.AutoCompleteSource = AutoCompleteSource.FileSystem;
            textBox2.BackColor = Color.WhiteSmoke;
            textBox2.BorderStyle = BorderStyle.None;
            textBox2.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            textBox2.ForeColor = Color.FromArgb(31, 30, 68);
            textBox2.Location = new Point(419, 57);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.PlaceholderText = "Rp.0";
            textBox2.RightToLeft = RightToLeft.No;
            textBox2.Size = new Size(169, 30);
            textBox2.TabIndex = 6;
            // 
            // button4
            // 
            button4.BackColor = Color.FromArgb(31, 30, 68);
            button4.FlatAppearance.BorderSize = 0;
            button4.FlatStyle = FlatStyle.Flat;
            button4.ForeColor = Color.White;
            button4.Location = new Point(3, 79);
            button4.Name = "button4";
            button4.Size = new Size(400, 46);
            button4.TabIndex = 5;
            button4.Text = "Bayar";
            button4.UseVisualStyleBackColor = false;
            // 
            // panel7
            // 
            panel7.BackColor = Color.White;
            panel7.Controls.Add(cmbFilter);
            panel7.Location = new Point(124, 55);
            panel7.Name = "panel7";
            panel7.Size = new Size(149, 35);
            panel7.TabIndex = 16;
            // 
            // cmbFilter
            // 
            cmbFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbFilter.FlatStyle = FlatStyle.Flat;
            cmbFilter.FormattingEnabled = true;
            cmbFilter.Location = new Point(4, 5);
            cmbFilter.Name = "cmbFilter";
            cmbFilter.Size = new Size(145, 23);
            cmbFilter.TabIndex = 0;
            cmbFilter.SelectedIndexChanged += cmbFilter_SelectedIndexChanged;
            // 
            // panel5
            // 
            panel5.BackColor = Color.White;
            panel5.Controls.Add(txtCariMenuList);
            panel5.Controls.Add(pictureBox2);
            panel5.Controls.Add(txtCariMenu);
            panel5.Location = new Point(5, 8);
            panel5.Name = "panel5";
            panel5.Size = new Size(355, 31);
            panel5.TabIndex = 14;
            // 
            // txtCariMenuList
            // 
            txtCariMenuList.BackColor = Color.White;
            txtCariMenuList.BorderStyle = BorderStyle.None;
            txtCariMenuList.Cursor = Cursors.IBeam;
            txtCariMenuList.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtCariMenuList.Location = new Point(37, 10);
            txtCariMenuList.Name = "txtCariMenuList";
            txtCariMenuList.PlaceholderText = "Masukan nama menu";
            txtCariMenuList.Size = new Size(284, 16);
            txtCariMenuList.TabIndex = 15;
            txtCariMenuList.TextChanged += txtCariMenuList_TextChanged;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.White;
            pictureBox2.BackgroundImage = Properties.Resources.search_20px;
            pictureBox2.BackgroundImageLayout = ImageLayout.Center;
            pictureBox2.InitialImage = Properties.Resources.search_20px;
            pictureBox2.Location = new Point(0, 0);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(31, 31);
            pictureBox2.TabIndex = 14;
            pictureBox2.TabStop = false;
            // 
            // txtCariMenu
            // 
            txtCariMenu.BackColor = Color.White;
            txtCariMenu.BorderStyle = BorderStyle.None;
            txtCariMenu.Cursor = Cursors.IBeam;
            txtCariMenu.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtCariMenu.Location = new Point(37, 8);
            txtCariMenu.Name = "txtCariMenu";
            txtCariMenu.PlaceholderText = "Masukan nama menu";
            txtCariMenu.Size = new Size(291, 16);
            txtCariMenu.TabIndex = 0;
            txtCariMenu.TextChanged += txtCariMenu_TextChanged;
            txtCariMenu.KeyDown += txtCariMenu_KeyDown;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.WhiteSmoke;
            panel2.Controls.Add(dataGridView2);
            panel2.Controls.Add(dataGridView3);
            panel2.Controls.Add(panel8);
            panel2.Location = new Point(6, 9);
            panel2.Name = "panel2";
            panel2.Size = new Size(607, 660);
            panel2.TabIndex = 9;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.AllowUserToResizeColumns = false;
            dataGridView2.AllowUserToResizeRows = false;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = Color.White;
            dataGridViewCellStyle6.ForeColor = Color.Silver;
            dataGridViewCellStyle6.SelectionBackColor = Color.Transparent;
            dataGridViewCellStyle6.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView2.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            dataGridView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.BackgroundColor = Color.White;
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView2.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle7.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle7.ForeColor = Color.White;
            dataGridViewCellStyle7.SelectionBackColor = Color.Transparent;
            dataGridViewCellStyle7.SelectionForeColor = Color.Transparent;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.True;
            dataGridView2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            dataGridView2.ColumnHeadersHeight = 30;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView2.ColumnHeadersVisible = false;
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = Color.White;
            dataGridViewCellStyle8.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle8.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle8.SelectionForeColor = Color.Gainsboro;
            dataGridViewCellStyle8.WrapMode = DataGridViewTriState.False;
            dataGridView2.DefaultCellStyle = dataGridViewCellStyle8;
            dataGridView2.EnableHeadersVisualStyles = false;
            dataGridView2.GridColor = Color.FromArgb(31, 30, 68);
            dataGridView2.ImeMode = ImeMode.NoControl;
            dataGridView2.Location = new Point(0, 115);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.ReadOnly = true;
            dataGridView2.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = Color.White;
            dataGridViewCellStyle9.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle9.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle9.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle9.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle9.WrapMode = DataGridViewTriState.True;
            dataGridView2.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = Color.White;
            dataGridViewCellStyle10.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridViewCellStyle10.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle10.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView2.RowsDefaultCellStyle = dataGridViewCellStyle10;
            dataGridView2.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            dataGridView2.RowTemplate.DefaultCellStyle.ForeColor = Color.FromArgb(31, 30, 68);
            dataGridView2.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            dataGridView2.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.FromArgb(31, 30, 68);
            dataGridView2.RowTemplate.Height = 40;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.Size = new Size(607, 542);
            dataGridView2.TabIndex = 21;
            dataGridView2.CellContentClick += DataGridView2_CellContentClick;
            // 
            // dataGridView3
            // 
            dataGridView3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView3.AutoScroll = true;
            dataGridView3.BackColor = Color.WhiteSmoke;
            dataGridView3.Location = new Point(0, 115);
            dataGridView3.Name = "dataGridView3";
            dataGridView3.Size = new Size(607, 545);
            dataGridView3.TabIndex = 18;
            dataGridView3.Scroll += dataGridView3_Scroll;
            // 
            // panel8
            // 
            panel8.BackColor = Color.White;
            panel8.Controls.Add(btnCari);
            panel8.Controls.Add(lblCountingItems);
            panel8.Controls.Add(label9);
            panel8.Controls.Add(label5);
            panel8.Controls.Add(label4);
            panel8.Controls.Add(panel5);
            panel8.Controls.Add(panel7);
            panel8.Location = new Point(0, 0);
            panel8.Name = "panel8";
            panel8.Size = new Size(453, 109);
            panel8.TabIndex = 17;
            // 
            // btnCari
            // 
            btnCari.BackColor = Color.FromArgb(31, 30, 68);
            btnCari.FlatAppearance.BorderSize = 0;
            btnCari.FlatStyle = FlatStyle.Flat;
            btnCari.ForeColor = Color.White;
            btnCari.IconChar = IconChar.MagnifyingGlass;
            btnCari.IconColor = Color.White;
            btnCari.IconFont = IconFont.Auto;
            btnCari.IconSize = 20;
            btnCari.ImageAlign = ContentAlignment.MiddleRight;
            btnCari.Location = new Point(366, 8);
            btnCari.Name = "btnCari";
            btnCari.Size = new Size(77, 31);
            btnCari.TabIndex = 20;
            btnCari.Text = "Cari";
            btnCari.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCari.UseVisualStyleBackColor = false;
            btnCari.Click += btnCari_Click;
            // 
            // lblCountingItems
            // 
            lblCountingItems.AutoSize = true;
            lblCountingItems.BackColor = Color.White;
            lblCountingItems.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCountingItems.ForeColor = Color.FromArgb(31, 30, 68);
            lblCountingItems.Location = new Point(338, 70);
            lblCountingItems.Name = "lblCountingItems";
            lblCountingItems.Size = new Size(45, 15);
            lblCountingItems.TabIndex = 61;
            lblCountingItems.Text = "0 Items";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.BackColor = Color.White;
            label9.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label9.ForeColor = Color.FromArgb(31, 30, 68);
            label9.Location = new Point(285, 69);
            label9.Name = "label9";
            label9.Size = new Size(54, 15);
            label9.TabIndex = 60;
            label9.Text = "Showing";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.White;
            label5.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label5.ForeColor = Color.FromArgb(31, 30, 68);
            label5.Location = new Point(50, 64);
            label5.Name = "label5";
            label5.Size = new Size(73, 21);
            label5.TabIndex = 59;
            label5.Text = "Category";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.White;
            label4.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label4.ForeColor = Color.FromArgb(31, 30, 68);
            label4.Location = new Point(3, 64);
            label4.Name = "label4";
            label4.Size = new Size(52, 21);
            label4.TabIndex = 58;
            label4.Text = "Menu";
            // 
            // iconDropDownButton1
            // 
            iconDropDownButton1.IconChar = IconChar.None;
            iconDropDownButton1.IconColor = Color.Black;
            iconDropDownButton1.IconFont = IconFont.Auto;
            iconDropDownButton1.Name = "iconDropDownButton1";
            iconDropDownButton1.Size = new Size(23, 23);
            iconDropDownButton1.Text = "iconDropDownButton1";
            // 
            // Offline_masterPos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(1044, 681);
            Controls.Add(panel2);
            Controls.Add(panel3);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Offline_masterPos";
            Text = "menu";
            Load += masterPos_Load;
            panel3.ResumeLayout(false);
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            panel9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            panel1.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel7.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)menuBindingSource).EndInit();
            ResumeLayout(false);
        }


        #endregion

        private Panel panel3;
        private Panel panel4;
        private Label label3;
        private TextBox textBox2;
        private Button button4;
        private Panel panel1;
        private Panel panel5;
        private TextBox txtCariMenu;
        private DataGridView dataGridView1;
        private Panel panel6;
        private Label lblSubTotal;
        private Label lblTotal;
        private Label lblTotal1;
        private Label lblSubTotal1;
        private Panel panel7;
        private ComboBox cmbFilter;
        private Panel panel2;
        private Panel panel8;
        private Panel panel9;
        private ComboBox cmbDiskon;
        private Label label1;
        private Button button6;
        private Label lblDiskon1;
        private FlowLayoutPanel dataGridView3;
        private IconButton listBill1;
        private IconButton iconButtonHps;
        private IconButton ButtonSimpan;
        private IconButton iconButton3;
        private IconButton iconButton1;
        private IconButton iconButtonGet;
        private IconDropDownButton iconDropDownButton1;
        private IconButton ButtonSplit;
        private PictureBox pictureBox2;
        private DataGridView dataGridView2;
        private TextBox txtCariMenuList;
        private Model.SButton sButton2;
        private Label label5;
        private Label label4;
        private IconButton lblDetailKeranjang;
        private BindingSource menuBindingSource;
        private Label label6;
        private Label label7;
        private Label lblCountingItems;
        private Label label9;
        private IconButton btnCari;
    }
}