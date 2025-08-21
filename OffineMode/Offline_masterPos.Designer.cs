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
            panelCartArea = new Panel();
            lblDeleteCart = new Label();
            label2 = new Label();
            lblDetailKeranjang = new IconButton();
            listBill1 = new IconButton();
            PanelDetailTotal = new Panel();
            cmbDiskon = new ComboBox();
            label6 = new Label();
            iconButtonGet = new IconButton();
            label7 = new Label();
            lblDiskon1 = new Label();
            label1 = new Label();
            lblTotal1 = new Label();
            lblSubTotal1 = new Label();
            lblTotal = new Label();
            lblSubTotal = new Label();
            dataGridView1 = new DataGridView();
            panel1 = new Panel();
            ButtonSplit = new IconButton();
            buttonPayment = new IconButton();
            iconButton3 = new IconButton();
            ButtonSimpan = new IconButton();
            panel4 = new Panel();
            label3 = new Label();
            textBox2 = new TextBox();
            button4 = new Button();
            panelSearchBox = new Panel();
            txtCariMenuList = new TextBox();
            pictureBox2 = new PictureBox();
            txtCariMenu = new TextBox();
            panel2 = new Panel();
            dataGridView2 = new DataGridView();
            dataGridView3 = new FlowLayoutPanel();
            panel8 = new Panel();
            btnReload = new IconButton();
            btnListView = new IconButton();
            btnGridView = new IconButton();
            label9 = new Label();
            btnCategoryMin = new IconButton();
            btnCategoryMkn = new IconButton();
            btnCategoryAll = new IconButton();
            btnCari = new IconButton();
            lblCountingItems = new Label();
            label5 = new Label();
            label4 = new Label();
            iconDropDownButton1 = new IconDropDownButton();
            menuBindingSource = new BindingSource(components);
            panelCartArea.SuspendLayout();
            PanelDetailTotal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel1.SuspendLayout();
            panel4.SuspendLayout();
            panelSearchBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            panel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)menuBindingSource).BeginInit();
            SuspendLayout();
            // 
            // panelCartArea
            // 
            panelCartArea.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panelCartArea.BackColor = Color.White;
            panelCartArea.Controls.Add(lblDeleteCart);
            panelCartArea.Controls.Add(label2);
            panelCartArea.Controls.Add(lblDetailKeranjang);
            panelCartArea.Controls.Add(listBill1);
            panelCartArea.Controls.Add(PanelDetailTotal);
            panelCartArea.Controls.Add(dataGridView1);
            panelCartArea.Controls.Add(panel1);
            panelCartArea.Controls.Add(panel4);
            panelCartArea.ForeColor = Color.White;
            panelCartArea.Location = new Point(619, 9);
            panelCartArea.Name = "panelCartArea";
            panelCartArea.Padding = new Padding(5);
            panelCartArea.Size = new Size(410, 657);
            panelCartArea.TabIndex = 8;
            // 
            // lblDeleteCart
            // 
            lblDeleteCart.AutoSize = true;
            lblDeleteCart.BackColor = Color.Transparent;
            lblDeleteCart.Cursor = Cursors.Hand;
            lblDeleteCart.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblDeleteCart.ForeColor = Color.DarkRed;
            lblDeleteCart.Location = new Point(304, 42);
            lblDeleteCart.Name = "lblDeleteCart";
            lblDeleteCart.Size = new Size(97, 15);
            lblDeleteCart.TabIndex = 63;
            lblDeleteCart.Text = "Hapus Keranjang";
            lblDeleteCart.Click += lblDeleteCart_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(8, 42);
            label2.Name = "label2";
            label2.Size = new Size(101, 21);
            label2.TabIndex = 62;
            label2.Text = "Order items ";
            // 
            // lblDetailKeranjang
            // 
            lblDetailKeranjang.BackColor = Color.White;
            lblDetailKeranjang.Enabled = false;
            lblDetailKeranjang.FlatAppearance.BorderSize = 0;
            lblDetailKeranjang.FlatStyle = FlatStyle.Flat;
            lblDetailKeranjang.Font = new Font("Segoe UI Semibold", 7.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblDetailKeranjang.ForeColor = Color.Black;
            lblDetailKeranjang.IconChar = IconChar.CartArrowDown;
            lblDetailKeranjang.IconColor = Color.Black;
            lblDetailKeranjang.IconFont = IconFont.Auto;
            lblDetailKeranjang.IconSize = 20;
            lblDetailKeranjang.ImageAlign = ContentAlignment.MiddleRight;
            lblDetailKeranjang.Location = new Point(52, 6);
            lblDetailKeranjang.Name = "lblDetailKeranjang";
            lblDetailKeranjang.Size = new Size(352, 33);
            lblDetailKeranjang.TabIndex = 1;
            lblDetailKeranjang.Text = "Keranjang :";
            lblDetailKeranjang.TextImageRelation = TextImageRelation.ImageBeforeText;
            lblDetailKeranjang.UseVisualStyleBackColor = false;
            lblDetailKeranjang.Click += lblDetailKeranjang_Click;
            // 
            // listBill1
            // 
            listBill1.Cursor = Cursors.Hand;
            listBill1.FlatAppearance.BorderSize = 0;
            listBill1.FlatStyle = FlatStyle.Flat;
            listBill1.ForeColor = Color.Black;
            listBill1.IconChar = IconChar.List;
            listBill1.IconColor = Color.Black;
            listBill1.IconFont = IconFont.Auto;
            listBill1.IconSize = 30;
            listBill1.ImageAlign = ContentAlignment.MiddleRight;
            listBill1.Location = new Point(6, 5);
            listBill1.Name = "listBill1";
            listBill1.Size = new Size(40, 34);
            listBill1.TabIndex = 18;
            listBill1.TextImageRelation = TextImageRelation.ImageBeforeText;
            listBill1.UseVisualStyleBackColor = true;
            listBill1.Click += listBill_Click;
            // 
            // PanelDetailTotal
            // 
            PanelDetailTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            PanelDetailTotal.BackColor = Color.WhiteSmoke;
            PanelDetailTotal.Controls.Add(cmbDiskon);
            PanelDetailTotal.Controls.Add(label6);
            PanelDetailTotal.Controls.Add(iconButtonGet);
            PanelDetailTotal.Controls.Add(label7);
            PanelDetailTotal.Controls.Add(lblDiskon1);
            PanelDetailTotal.Controls.Add(label1);
            PanelDetailTotal.Controls.Add(lblTotal1);
            PanelDetailTotal.Controls.Add(lblSubTotal1);
            PanelDetailTotal.Controls.Add(lblTotal);
            PanelDetailTotal.Controls.Add(lblSubTotal);
            PanelDetailTotal.Location = new Point(8, 412);
            PanelDetailTotal.Name = "PanelDetailTotal";
            PanelDetailTotal.Size = new Size(392, 135);
            PanelDetailTotal.TabIndex = 15;
            // 
            // cmbDiskon
            // 
            cmbDiskon.Cursor = Cursors.Hand;
            cmbDiskon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDiskon.FlatStyle = FlatStyle.Flat;
            cmbDiskon.FormattingEnabled = true;
            cmbDiskon.Location = new Point(126, 98);
            cmbDiskon.Name = "cmbDiskon";
            cmbDiskon.Size = new Size(149, 23);
            cmbDiskon.TabIndex = 1;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = Color.Transparent;
            label6.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label6.ForeColor = Color.Black;
            label6.Location = new Point(44, 99);
            label6.Name = "label6";
            label6.Size = new Size(71, 21);
            label6.TabIndex = 61;
            label6.Text = "Discount";
            // 
            // iconButtonGet
            // 
            iconButtonGet.BackColor = Color.White;
            iconButtonGet.Cursor = Cursors.Hand;
            iconButtonGet.FlatAppearance.BorderSize = 0;
            iconButtonGet.FlatStyle = FlatStyle.Flat;
            iconButtonGet.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            iconButtonGet.ForeColor = Color.Black;
            iconButtonGet.IconChar = IconChar.None;
            iconButtonGet.IconColor = Color.FromArgb(15, 90, 94);
            iconButtonGet.IconFont = IconFont.Auto;
            iconButtonGet.IconSize = 20;
            iconButtonGet.Location = new Point(287, 97);
            iconButtonGet.Name = "iconButtonGet";
            iconButtonGet.Size = new Size(93, 23);
            iconButtonGet.TabIndex = 23;
            iconButtonGet.Text = "Pakai";
            iconButtonGet.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButtonGet.UseVisualStyleBackColor = false;
            iconButtonGet.Click += btnGet_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = Color.Transparent;
            label7.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label7.ForeColor = Color.Black;
            label7.Location = new Point(10, 99);
            label7.Name = "label7";
            label7.Size = new Size(40, 21);
            label7.TabIndex = 60;
            label7.Text = "Pilih";
            // 
            // lblDiskon1
            // 
            lblDiskon1.AutoSize = true;
            lblDiskon1.BackColor = Color.Transparent;
            lblDiskon1.ForeColor = Color.Black;
            lblDiskon1.Location = new Point(278, 11);
            lblDiskon1.Name = "lblDiskon1";
            lblDiskon1.Size = new Size(51, 15);
            lblDiskon1.TabIndex = 9;
            lblDiskon1.Text = "- Diskon";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.ForeColor = Color.Black;
            label1.Location = new Point(9, 11);
            label1.Name = "label1";
            label1.Size = new Size(49, 15);
            label1.TabIndex = 5;
            label1.Text = "Diskon :";
            // 
            // lblTotal1
            // 
            lblTotal1.AutoSize = true;
            lblTotal1.BackColor = Color.Transparent;
            lblTotal1.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblTotal1.ForeColor = Color.Black;
            lblTotal1.Location = new Point(279, 60);
            lblTotal1.Name = "lblTotal1";
            lblTotal1.Size = new Size(45, 21);
            lblTotal1.TabIndex = 3;
            lblTotal1.Text = "Total";
            // 
            // lblSubTotal1
            // 
            lblSubTotal1.AutoSize = true;
            lblSubTotal1.BackColor = Color.Transparent;
            lblSubTotal1.ForeColor = Color.Black;
            lblSubTotal1.Location = new Point(279, 34);
            lblSubTotal1.Name = "lblSubTotal1";
            lblSubTotal1.Size = new Size(52, 15);
            lblSubTotal1.TabIndex = 2;
            lblSubTotal1.Text = "SubTotal";
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.BackColor = Color.Transparent;
            lblTotal.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblTotal.ForeColor = Color.Black;
            lblTotal.Location = new Point(9, 60);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(57, 21);
            lblTotal.TabIndex = 1;
            lblTotal.Text = "Total : ";
            // 
            // lblSubTotal
            // 
            lblSubTotal.AutoSize = true;
            lblSubTotal.BackColor = Color.Transparent;
            lblSubTotal.ForeColor = Color.Black;
            lblSubTotal.Location = new Point(9, 34);
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
            dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(15, 90, 94);
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
            dataGridView1.GridColor = Color.Black;
            dataGridView1.ImeMode = ImeMode.NoControl;
            dataGridView1.Location = new Point(8, 66);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.White;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle4.ForeColor = Color.Black;
            dataGridViewCellStyle4.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle4.SelectionForeColor = Color.Black;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.ForeColor = Color.Black;
            dataGridViewCellStyle5.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle5.SelectionForeColor = Color.Black;
            dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dataGridView1.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowTemplate.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(392, 340);
            dataGridView1.TabIndex = 14;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panel1.Controls.Add(ButtonSplit);
            panel1.Controls.Add(buttonPayment);
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
            ButtonSplit.Cursor = Cursors.Hand;
            ButtonSplit.FlatAppearance.BorderSize = 0;
            ButtonSplit.FlatStyle = FlatStyle.Flat;
            ButtonSplit.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            ButtonSplit.ForeColor = Color.Black;
            ButtonSplit.IconChar = IconChar.None;
            ButtonSplit.IconColor = Color.FromArgb(15, 90, 94);
            ButtonSplit.IconFont = IconFont.Auto;
            ButtonSplit.IconSize = 25;
            ButtonSplit.ImageAlign = ContentAlignment.MiddleRight;
            ButtonSplit.Location = new Point(8, 7);
            ButtonSplit.Name = "ButtonSplit";
            ButtonSplit.Size = new Size(155, 35);
            ButtonSplit.TabIndex = 23;
            ButtonSplit.Text = "Split Bill";
            ButtonSplit.UseVisualStyleBackColor = true;
            ButtonSplit.Click += ButtonSplit_Click;
            // 
            // buttonPayment
            // 
            buttonPayment.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonPayment.BackColor = Color.FromArgb(15, 90, 94);
            buttonPayment.Cursor = Cursors.Hand;
            buttonPayment.FlatAppearance.BorderSize = 0;
            buttonPayment.FlatStyle = FlatStyle.Flat;
            buttonPayment.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            buttonPayment.ForeColor = Color.Gainsboro;
            buttonPayment.IconChar = IconChar.None;
            buttonPayment.IconColor = Color.Gainsboro;
            buttonPayment.IconFont = IconFont.Auto;
            buttonPayment.IconSize = 30;
            buttonPayment.ImageAlign = ContentAlignment.MiddleRight;
            buttonPayment.Location = new Point(5, 47);
            buttonPayment.Name = "buttonPayment";
            buttonPayment.Size = new Size(392, 49);
            buttonPayment.TabIndex = 22;
            buttonPayment.Text = "Proses Pembayaran";
            buttonPayment.UseVisualStyleBackColor = false;
            buttonPayment.Click += buttonPayment_Click;
            // 
            // iconButton3
            // 
            iconButton3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            iconButton3.Cursor = Cursors.Hand;
            iconButton3.FlatAppearance.BorderSize = 0;
            iconButton3.FlatStyle = FlatStyle.Flat;
            iconButton3.ForeColor = Color.FromArgb(15, 90, 94);
            iconButton3.IconChar = IconChar.Tags;
            iconButton3.IconColor = Color.FromArgb(15, 90, 94);
            iconButton3.IconFont = IconFont.Auto;
            iconButton3.IconSize = 30;
            iconButton3.Location = new Point(328, 7);
            iconButton3.Name = "iconButton3";
            iconButton3.Size = new Size(69, 35);
            iconButton3.TabIndex = 21;
            iconButton3.UseVisualStyleBackColor = true;
            iconButton3.Click += button3_Click;
            // 
            // ButtonSimpan
            // 
            ButtonSimpan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ButtonSimpan.Cursor = Cursors.Hand;
            ButtonSimpan.FlatAppearance.BorderSize = 0;
            ButtonSimpan.FlatStyle = FlatStyle.Flat;
            ButtonSimpan.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            ButtonSimpan.ForeColor = Color.Black;
            ButtonSimpan.IconChar = IconChar.None;
            ButtonSimpan.IconColor = Color.FromArgb(15, 90, 94);
            ButtonSimpan.IconFont = IconFont.Auto;
            ButtonSimpan.IconSize = 25;
            ButtonSimpan.ImageAlign = ContentAlignment.MiddleRight;
            ButtonSimpan.Location = new Point(166, 7);
            ButtonSimpan.Name = "ButtonSimpan";
            ButtonSimpan.Size = new Size(155, 35);
            ButtonSimpan.TabIndex = 20;
            ButtonSimpan.Text = "Simpan Bill";
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
            label3.ForeColor = Color.FromArgb(15, 90, 94);
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
            textBox2.ForeColor = Color.FromArgb(15, 90, 94);
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
            button4.BackColor = Color.FromArgb(15, 90, 94);
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
            // panelSearchBox
            // 
            panelSearchBox.BackColor = Color.WhiteSmoke;
            panelSearchBox.Controls.Add(txtCariMenuList);
            panelSearchBox.Controls.Add(pictureBox2);
            panelSearchBox.Controls.Add(txtCariMenu);
            panelSearchBox.Location = new Point(5, 8);
            panelSearchBox.Name = "panelSearchBox";
            panelSearchBox.Size = new Size(266, 31);
            panelSearchBox.TabIndex = 14;
            // 
            // txtCariMenuList
            // 
            txtCariMenuList.BackColor = Color.WhiteSmoke;
            txtCariMenuList.BorderStyle = BorderStyle.None;
            txtCariMenuList.Cursor = Cursors.IBeam;
            txtCariMenuList.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtCariMenuList.Location = new Point(37, 10);
            txtCariMenuList.Name = "txtCariMenuList";
            txtCariMenuList.PlaceholderText = "Masukan nama menu";
            txtCariMenuList.Size = new Size(226, 16);
            txtCariMenuList.TabIndex = 15;
            txtCariMenuList.TextChanged += txtCariMenuList_TextChanged;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.WhiteSmoke;
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
            txtCariMenu.BackColor = Color.WhiteSmoke;
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
            panel2.Size = new Size(607, 657);
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
            dataGridViewCellStyle6.SelectionForeColor = Color.Black;
            dataGridView2.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            dataGridView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.BackgroundColor = Color.White;
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView2.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = Color.Black;
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
            dataGridView2.GridColor = Color.Black;
            dataGridView2.ImeMode = ImeMode.NoControl;
            dataGridView2.Location = new Point(0, 115);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.ReadOnly = true;
            dataGridView2.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = Color.White;
            dataGridViewCellStyle9.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle9.ForeColor = Color.FromArgb(15, 90, 94);
            dataGridViewCellStyle9.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle9.SelectionForeColor = Color.Black;
            dataGridViewCellStyle9.WrapMode = DataGridViewTriState.True;
            dataGridView2.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = Color.White;
            dataGridViewCellStyle10.ForeColor = Color.Black;
            dataGridViewCellStyle10.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle10.SelectionForeColor = Color.Black;
            dataGridView2.RowsDefaultCellStyle = dataGridViewCellStyle10;
            dataGridView2.RowTemplate.DefaultCellStyle.BackColor = Color.White;
            dataGridView2.RowTemplate.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView2.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            dataGridView2.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView2.RowTemplate.Height = 40;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.Size = new Size(607, 539);
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
            dataGridView3.Size = new Size(607, 542);
            dataGridView3.TabIndex = 18;
            dataGridView3.Scroll += dataGridView3_Scroll;
            // 
            // panel8
            // 
            panel8.BackColor = Color.White;
            panel8.Controls.Add(btnReload);
            panel8.Controls.Add(btnListView);
            panel8.Controls.Add(btnGridView);
            panel8.Controls.Add(label9);
            panel8.Controls.Add(btnCategoryMin);
            panel8.Controls.Add(btnCategoryMkn);
            panel8.Controls.Add(btnCategoryAll);
            panel8.Controls.Add(btnCari);
            panel8.Controls.Add(lblCountingItems);
            panel8.Controls.Add(label5);
            panel8.Controls.Add(label4);
            panel8.Controls.Add(panelSearchBox);
            panel8.Location = new Point(0, 0);
            panel8.Name = "panel8";
            panel8.Size = new Size(465, 109);
            panel8.TabIndex = 17;
            // 
            // btnReload
            // 
            btnReload.BackColor = Color.White;
            btnReload.Cursor = Cursors.Hand;
            btnReload.FlatAppearance.BorderSize = 0;
            btnReload.FlatStyle = FlatStyle.Flat;
            btnReload.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnReload.ForeColor = Color.Black;
            btnReload.IconChar = IconChar.Rotate;
            btnReload.IconColor = Color.Black;
            btnReload.IconFont = IconFont.Auto;
            btnReload.IconSize = 20;
            btnReload.Location = new Point(427, 8);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(35, 31);
            btnReload.TabIndex = 67;
            btnReload.UseVisualStyleBackColor = false;
            btnReload.Click += btnReload_Click;
            // 
            // btnListView
            // 
            btnListView.BackColor = Color.White;
            btnListView.Cursor = Cursors.Hand;
            btnListView.FlatAppearance.BorderSize = 0;
            btnListView.FlatStyle = FlatStyle.Flat;
            btnListView.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnListView.ForeColor = Color.Black;
            btnListView.IconChar = IconChar.ListCheck;
            btnListView.IconColor = Color.Black;
            btnListView.IconFont = IconFont.Auto;
            btnListView.IconSize = 20;
            btnListView.Location = new Point(386, 8);
            btnListView.Name = "btnListView";
            btnListView.Size = new Size(35, 31);
            btnListView.TabIndex = 66;
            btnListView.UseVisualStyleBackColor = false;
            btnListView.Click += btnListView_Click;
            // 
            // btnGridView
            // 
            btnGridView.BackColor = Color.White;
            btnGridView.Cursor = Cursors.Hand;
            btnGridView.FlatAppearance.BorderSize = 0;
            btnGridView.FlatStyle = FlatStyle.Flat;
            btnGridView.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnGridView.ForeColor = Color.Black;
            btnGridView.IconChar = IconChar.BorderAll;
            btnGridView.IconColor = Color.Black;
            btnGridView.IconFont = IconFont.Auto;
            btnGridView.IconSize = 20;
            btnGridView.Location = new Point(345, 8);
            btnGridView.Name = "btnGridView";
            btnGridView.Size = new Size(35, 31);
            btnGridView.TabIndex = 65;
            btnGridView.UseVisualStyleBackColor = false;
            btnGridView.Click += btnGridView_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.BackColor = Color.White;
            label9.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label9.ForeColor = Color.Black;
            label9.Location = new Point(345, 86);
            label9.Name = "label9";
            label9.Size = new Size(54, 15);
            label9.TabIndex = 60;
            label9.Text = "Showing";
            // 
            // btnCategoryMin
            // 
            btnCategoryMin.BackColor = Color.White;
            btnCategoryMin.Cursor = Cursors.Hand;
            btnCategoryMin.FlatAppearance.BorderSize = 0;
            btnCategoryMin.FlatStyle = FlatStyle.Flat;
            btnCategoryMin.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnCategoryMin.ForeColor = Color.Black;
            btnCategoryMin.IconChar = IconChar.WineGlass;
            btnCategoryMin.IconColor = Color.Black;
            btnCategoryMin.IconFont = IconFont.Auto;
            btnCategoryMin.IconSize = 20;
            btnCategoryMin.ImageAlign = ContentAlignment.MiddleRight;
            btnCategoryMin.Location = new Point(211, 66);
            btnCategoryMin.Name = "btnCategoryMin";
            btnCategoryMin.Size = new Size(114, 31);
            btnCategoryMin.TabIndex = 64;
            btnCategoryMin.Text = "MINUMAN";
            btnCategoryMin.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCategoryMin.UseVisualStyleBackColor = false;
            btnCategoryMin.Click += btnCategoryMin_Click;
            // 
            // btnCategoryMkn
            // 
            btnCategoryMkn.BackColor = Color.White;
            btnCategoryMkn.Cursor = Cursors.Hand;
            btnCategoryMkn.FlatAppearance.BorderSize = 0;
            btnCategoryMkn.FlatStyle = FlatStyle.Flat;
            btnCategoryMkn.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnCategoryMkn.ForeColor = Color.Black;
            btnCategoryMkn.IconChar = IconChar.BowlRice;
            btnCategoryMkn.IconColor = Color.Black;
            btnCategoryMkn.IconFont = IconFont.Auto;
            btnCategoryMkn.IconSize = 20;
            btnCategoryMkn.ImageAlign = ContentAlignment.MiddleRight;
            btnCategoryMkn.Location = new Point(91, 66);
            btnCategoryMkn.Name = "btnCategoryMkn";
            btnCategoryMkn.Size = new Size(114, 31);
            btnCategoryMkn.TabIndex = 63;
            btnCategoryMkn.Text = "MAKANAN";
            btnCategoryMkn.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCategoryMkn.UseVisualStyleBackColor = false;
            btnCategoryMkn.Click += iconButton1_Click;
            // 
            // btnCategoryAll
            // 
            btnCategoryAll.BackColor = Color.White;
            btnCategoryAll.Cursor = Cursors.Hand;
            btnCategoryAll.FlatAppearance.BorderSize = 0;
            btnCategoryAll.FlatStyle = FlatStyle.Flat;
            btnCategoryAll.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnCategoryAll.ForeColor = Color.Black;
            btnCategoryAll.IconChar = IconChar.Utensils;
            btnCategoryAll.IconColor = Color.Black;
            btnCategoryAll.IconFont = IconFont.Auto;
            btnCategoryAll.IconSize = 20;
            btnCategoryAll.ImageAlign = ContentAlignment.MiddleRight;
            btnCategoryAll.Location = new Point(8, 67);
            btnCategoryAll.Name = "btnCategoryAll";
            btnCategoryAll.Size = new Size(77, 31);
            btnCategoryAll.TabIndex = 62;
            btnCategoryAll.Text = "ALL";
            btnCategoryAll.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCategoryAll.UseVisualStyleBackColor = false;
            btnCategoryAll.Click += btnCategoryAll_Click;
            // 
            // btnCari
            // 
            btnCari.BackColor = Color.White;
            btnCari.Cursor = Cursors.Hand;
            btnCari.FlatAppearance.BorderSize = 0;
            btnCari.FlatStyle = FlatStyle.Flat;
            btnCari.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnCari.ForeColor = Color.Black;
            btnCari.IconChar = IconChar.MagnifyingGlass;
            btnCari.IconColor = Color.Black;
            btnCari.IconFont = IconFont.Auto;
            btnCari.IconSize = 20;
            btnCari.ImageAlign = ContentAlignment.MiddleRight;
            btnCari.Location = new Point(279, 8);
            btnCari.Name = "btnCari";
            btnCari.Size = new Size(57, 31);
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
            lblCountingItems.ForeColor = Color.Black;
            lblCountingItems.Location = new Point(398, 87);
            lblCountingItems.Name = "lblCountingItems";
            lblCountingItems.Size = new Size(45, 15);
            lblCountingItems.TabIndex = 61;
            lblCountingItems.Text = "0 Items";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.White;
            label5.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(142, 42);
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
            label4.ForeColor = Color.Black;
            label4.Location = new Point(95, 42);
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
            Controls.Add(panelCartArea);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Offline_masterPos";
            Text = "menu";
            panelCartArea.ResumeLayout(false);
            panelCartArea.PerformLayout();
            PanelDetailTotal.ResumeLayout(false);
            PanelDetailTotal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            panel1.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panelSearchBox.ResumeLayout(false);
            panelSearchBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)menuBindingSource).EndInit();
            ResumeLayout(false);
        }


        #endregion

        private Panel panelCartArea;
        private Panel panel4;
        private Label label3;
        private TextBox textBox2;
        private Button button4;
        private Panel panel1;
        private Panel panelSearchBox;
        private TextBox txtCariMenu;
        private DataGridView dataGridView1;
        private Panel panel2;
        private Panel panel8;
        private FlowLayoutPanel dataGridView3;
        private IconButton listBill1;
        private IconButton ButtonSimpan;
        private IconButton iconButton3;
        private IconButton buttonPayment;
        private IconDropDownButton iconDropDownButton1;
        private IconButton ButtonSplit;
        private PictureBox pictureBox2;
        private DataGridView dataGridView2;
        private TextBox txtCariMenuList;
        private Label label5;
        private Label label4;
        private IconButton lblDetailKeranjang;
        private BindingSource menuBindingSource;
        private Label lblCountingItems;
        private Label label9;
        private IconButton btnCari;
        private Panel PanelDetailTotal;
        private Label label6;
        private IconButton iconButtonGet;
        private Label label7;
        private Label lblDiskon1;
        private Label label1;
        private ComboBox cmbDiskon;
        private Label lblTotal1;
        private Label lblSubTotal1;
        private Label lblTotal;
        private Label lblSubTotal;
        private Label label2;
        private Label lblDeleteCart;
        private IconButton btnCategoryAll;
        private IconButton btnCategoryMin;
        private IconButton btnCategoryMkn;
        private IconButton btnGridView;
        private IconButton btnListView;
        private IconButton btnReload;
    }
}