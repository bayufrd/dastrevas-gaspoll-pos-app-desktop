
using FontAwesome.Sharp;
namespace KASIR.Komponen
{
    partial class Offline_MemberData
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            panel2 = new Panel();
            btnKeluar = new IconButton();
            panel7 = new Panel();
            TambahMember = new IconButton();
            panel4 = new Panel();
            lblCountingItems = new Label();
            dataGridView1 = new DataGridView();
            panel8 = new Panel();
            txtCariMenuList = new TextBox();
            pictureBox2 = new PictureBox();
            label7 = new Label();
            panel2.SuspendLayout();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.Window;
            panel2.Controls.Add(btnKeluar);
            panel2.Controls.Add(panel7);
            panel2.Controls.Add(TambahMember);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(600, 70);
            panel2.TabIndex = 3;
            // 
            // btnKeluar
            // 
            btnKeluar.BackColor = Color.WhiteSmoke;
            btnKeluar.Cursor = Cursors.Hand;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.Flip = FlipOrientation.Horizontal;
            btnKeluar.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnKeluar.ForeColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconChar = IconChar.CircleChevronRight;
            btnKeluar.IconColor = Color.FromArgb(30, 31, 68);
            btnKeluar.IconFont = IconFont.Auto;
            btnKeluar.IconSize = 25;
            btnKeluar.Location = new Point(12, 21);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(88, 30);
            btnKeluar.TabIndex = 24;
            btnKeluar.Text = "Keluar";
            btnKeluar.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnKeluar.UseVisualStyleBackColor = false;
            btnKeluar.Click += btnKeluar_Click;
            // 
            // panel7
            // 
            panel7.Location = new Point(3, 70);
            panel7.Name = "panel7";
            panel7.Size = new Size(585, 10);
            panel7.TabIndex = 6;
            // 
            // TambahMember
            // 
            TambahMember.BackColor = Color.FromArgb(30, 31, 68);
            TambahMember.Cursor = Cursors.Hand;
            TambahMember.FlatAppearance.BorderSize = 0;
            TambahMember.FlatStyle = FlatStyle.Flat;
            TambahMember.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            TambahMember.ForeColor = Color.WhiteSmoke;
            TambahMember.IconChar = IconChar.UserPlus;
            TambahMember.IconColor = Color.WhiteSmoke;
            TambahMember.IconFont = IconFont.Auto;
            TambahMember.IconSize = 18;
            TambahMember.Location = new Point(500, 21);
            TambahMember.Name = "TambahMember";
            TambahMember.Size = new Size(88, 30);
            TambahMember.TabIndex = 25;
            TambahMember.Text = "Tambah";
            TambahMember.TextImageRelation = TextImageRelation.ImageBeforeText;
            TambahMember.UseVisualStyleBackColor = false;
            TambahMember.Click += TambahMember_Click;
            // 
            // panel4
            // 
            panel4.Controls.Add(lblCountingItems);
            panel4.Controls.Add(dataGridView1);
            panel4.Controls.Add(panel8);
            panel4.Controls.Add(label7);
            panel4.Location = new Point(3, 70);
            panel4.Name = "panel4";
            panel4.Size = new Size(592, 518);
            panel4.TabIndex = 13;
            // 
            // lblCountingItems
            // 
            lblCountingItems.AutoSize = true;
            lblCountingItems.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCountingItems.ForeColor = Color.FromArgb(30, 31, 68);
            lblCountingItems.Location = new Point(242, 33);
            lblCountingItems.Name = "lblCountingItems";
            lblCountingItems.Size = new Size(93, 15);
            lblCountingItems.TabIndex = 27;
            lblCountingItems.Text = "Jumlah Member";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.White;
            dataGridViewCellStyle1.ForeColor = Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(30, 31, 68);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.Transparent;
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.ColumnHeadersHeight = 30;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = Color.SteelBlue;
            dataGridViewCellStyle3.SelectionBackColor = Color.Gainsboro;
            dataGridViewCellStyle3.SelectionForeColor = Color.Gainsboro;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.Black;
            dataGridView1.ImeMode = ImeMode.NoControl;
            dataGridView1.Location = new Point(9, 55);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
            dataGridView1.Size = new Size(576, 462);
            dataGridView1.TabIndex = 26;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // panel8
            // 
            panel8.BackColor = Color.White;
            panel8.BorderStyle = BorderStyle.FixedSingle;
            panel8.Controls.Add(txtCariMenuList);
            panel8.Controls.Add(pictureBox2);
            panel8.Location = new Point(180, 3);
            panel8.Name = "panel8";
            panel8.Size = new Size(318, 27);
            panel8.TabIndex = 15;
            // 
            // txtCariMenuList
            // 
            txtCariMenuList.BackColor = Color.White;
            txtCariMenuList.BorderStyle = BorderStyle.None;
            txtCariMenuList.Cursor = Cursors.IBeam;
            txtCariMenuList.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtCariMenuList.Location = new Point(23, 5);
            txtCariMenuList.Name = "txtCariMenuList";
            txtCariMenuList.PlaceholderText = "Pencarian Member ...";
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
            pictureBox2.Location = new Point(0, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(21, 20);
            pictureBox2.TabIndex = 14;
            pictureBox2.TabStop = false;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label7.ForeColor = Color.Black;
            label7.Location = new Point(9, 7);
            label7.Name = "label7";
            label7.Size = new Size(114, 15);
            label7.TabIndex = 4;
            label7.Text = "MEMBERSHIP AREA";
            // 
            // dataMember
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(600, 600);
            Controls.Add(panel4);
            Controls.Add(panel2);
            FormBorderStyle = FormBorderStyle.None;
            Name = "dataMember";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "dataBill";
            TopMost = true;
            panel2.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }



        #endregion

        private Panel panel2;
        private Panel panel7;
        private FontAwesome.Sharp.IconButton btnKeluar;
        private Panel panel4;
        private Label lblCountingItems;
        private DataGridView dataGridView1;
        private IconButton TambahMember;
        private Panel panel8;
        private TextBox txtCariMenuList;
        private PictureBox pictureBox2;
        private Label label7;
    }
}