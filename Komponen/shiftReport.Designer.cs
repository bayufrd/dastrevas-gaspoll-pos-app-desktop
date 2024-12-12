
using FontAwesome.Sharp;
namespace KASIR.Komponen
{
    partial class shiftReport
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

        #region Component Designer generated code

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
            panel3 = new Panel();
            txtNamaKasir = new TextBox();
            panel5 = new Panel();
            label3 = new Label();
            txtActualCash = new TextBox();
            lblShiftSekarang = new Label();
            dataGridView1 = new DataGridView();
            lblNotifikasi = new Label();
            label1 = new Label();
            panel1 = new Panel();
            btnRiwayatShift = new IconButton();
            btnCetakStruk = new IconButton();
            btnPengeluaran = new IconButton();
            btnAddMenu = new Button();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel2.BackColor = Color.WhiteSmoke;
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(panel5);
            panel2.Controls.Add(lblShiftSekarang);
            panel2.Controls.Add(dataGridView1);
            panel2.Controls.Add(lblNotifikasi);
            panel2.Controls.Add(label1);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 70);
            panel2.Name = "panel2";
            panel2.Size = new Size(912, 510);
            panel2.TabIndex = 7;
            // 
            // panel3
            // 
            panel3.BackColor = Color.White;
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(txtNamaKasir);
            panel3.Location = new Point(13, 13);
            panel3.Name = "panel3";
            panel3.Size = new Size(434, 36);
            panel3.TabIndex = 47;
            // 
            // txtNamaKasir
            // 
            txtNamaKasir.BorderStyle = BorderStyle.None;
            txtNamaKasir.Location = new Point(7, 9);
            txtNamaKasir.Name = "txtNamaKasir";
            txtNamaKasir.PlaceholderText = "Nama kasir ...";
            txtNamaKasir.Size = new Size(426, 16);
            txtNamaKasir.TabIndex = 0;
            // 
            // panel5
            // 
            panel5.BackColor = Color.White;
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Controls.Add(label3);
            panel5.Controls.Add(txtActualCash);
            panel5.Location = new Point(13, 66);
            panel5.Name = "panel5";
            panel5.Size = new Size(434, 35);
            panel5.TabIndex = 46;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(3, 10);
            label3.Name = "label3";
            label3.Size = new Size(24, 15);
            label3.TabIndex = 19;
            label3.Text = "Rp.";
            // 
            // txtActualCash
            // 
            txtActualCash.BorderStyle = BorderStyle.None;
            txtActualCash.Location = new Point(33, 9);
            txtActualCash.Name = "txtActualCash";
            txtActualCash.PlaceholderText = "Masukan uang kasir ...";
            txtActualCash.Size = new Size(396, 16);
            txtActualCash.TabIndex = 0;
            txtActualCash.TextChanged += txtActualCash_TextChanged;
            // 
            // lblShiftSekarang
            // 
            lblShiftSekarang.AutoSize = true;
            lblShiftSekarang.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblShiftSekarang.Location = new Point(13, 104);
            lblShiftSekarang.Name = "lblShiftSekarang";
            lblShiftSekarang.Size = new Size(136, 15);
            lblShiftSekarang.TabIndex = 45;
            lblShiftSekarang.Text = "Laporan Shift Sekarang";
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
            dataGridView1.Location = new Point(13, 122);
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
            dataGridView1.Size = new Size(886, 385);
            dataGridView1.TabIndex = 19;
            // 
            // lblNotifikasi
            // 
            lblNotifikasi.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblNotifikasi.AutoSize = true;
            lblNotifikasi.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblNotifikasi.ForeColor = Color.DarkRed;
            lblNotifikasi.Location = new Point(465, 13);
            lblNotifikasi.Name = "lblNotifikasi";
            lblNotifikasi.Size = new Size(401, 15);
            lblNotifikasi.TabIndex = 43;
            lblNotifikasi.Text = "Tidak dapat akhiri laporan karena belum ada jarak 1 jam dari mulai shift.";
            lblNotifikasi.TextAlign = ContentAlignment.TopRight;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.BackColor = Color.DarkGray;
            label1.Location = new Point(13, 1);
            label1.Name = "label1";
            label1.Size = new Size(1573, 1);
            label1.TabIndex = 11;
            // 
            // panel1
            // 
            panel1.BackColor = Color.WhiteSmoke;
            panel1.Controls.Add(btnRiwayatShift);
            panel1.Controls.Add(btnCetakStruk);
            panel1.Controls.Add(btnPengeluaran);
            panel1.Controls.Add(btnAddMenu);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(912, 70);
            panel1.TabIndex = 6;
            // 
            // btnRiwayatShift
            // 
            btnRiwayatShift.BackColor = Color.WhiteSmoke;
            btnRiwayatShift.Cursor = Cursors.Hand;
            btnRiwayatShift.FlatAppearance.BorderSize = 0;
            btnRiwayatShift.FlatStyle = FlatStyle.Flat;
            btnRiwayatShift.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnRiwayatShift.ForeColor = Color.FromArgb(31, 30, 68);
            btnRiwayatShift.IconChar = IconChar.Rug;
            btnRiwayatShift.IconColor = Color.FromArgb(31, 30, 68);
            btnRiwayatShift.IconFont = IconFont.Auto;
            btnRiwayatShift.IconSize = 40;
            btnRiwayatShift.Location = new Point(13, 13);
            btnRiwayatShift.Name = "btnRiwayatShift";
            btnRiwayatShift.Size = new Size(136, 45);
            btnRiwayatShift.TabIndex = 43;
            btnRiwayatShift.Text = "Riwayat Shift";
            btnRiwayatShift.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnRiwayatShift.UseVisualStyleBackColor = false;
            btnRiwayatShift.Click += btnRiwayatShift_Click;
            // 
            // btnCetakStruk
            // 
            btnCetakStruk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCetakStruk.BackColor = Color.FromArgb(30, 31, 68);
            btnCetakStruk.Cursor = Cursors.Hand;
            btnCetakStruk.FlatAppearance.BorderSize = 0;
            btnCetakStruk.FlatStyle = FlatStyle.Flat;
            btnCetakStruk.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnCetakStruk.ForeColor = Color.WhiteSmoke;
            btnCetakStruk.IconChar = IconChar.Scroll;
            btnCetakStruk.IconColor = Color.WhiteSmoke;
            btnCetakStruk.IconFont = IconFont.Auto;
            btnCetakStruk.IconSize = 25;
            btnCetakStruk.Location = new Point(766, 13);
            btnCetakStruk.Name = "btnCetakStruk";
            btnCetakStruk.Size = new Size(133, 45);
            btnCetakStruk.TabIndex = 41;
            btnCetakStruk.Text = "Selesai Shift & Cetak Laporan";
            btnCetakStruk.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCetakStruk.UseVisualStyleBackColor = false;
            btnCetakStruk.Click += btnCetakStruk_Click;
            // 
            // btnPengeluaran
            // 
            btnPengeluaran.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPengeluaran.BackColor = Color.DarkRed;
            btnPengeluaran.FlatStyle = FlatStyle.Flat;
            btnPengeluaran.ForeColor = Color.White;
            btnPengeluaran.IconChar = IconChar.CashRegister;
            btnPengeluaran.IconColor = Color.White;
            btnPengeluaran.IconFont = IconFont.Auto;
            btnPengeluaran.IconSize = 25;
            btnPengeluaran.Location = new Point(633, 13);
            btnPengeluaran.Name = "btnPengeluaran";
            btnPengeluaran.Size = new Size(127, 45);
            btnPengeluaran.TabIndex = 15;
            btnPengeluaran.Text = "Pengeluaran";
            btnPengeluaran.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnPengeluaran.UseVisualStyleBackColor = false;
            btnPengeluaran.Click += btnPengeluaran_Click;
            // 
            // btnAddMenu
            // 
            btnAddMenu.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnAddMenu.AutoSize = true;
            btnAddMenu.BackColor = Color.FromArgb(31, 30, 68);
            btnAddMenu.FlatAppearance.BorderSize = 0;
            btnAddMenu.FlatStyle = FlatStyle.Flat;
            btnAddMenu.ForeColor = Color.White;
            btnAddMenu.Location = new Point(1447, 23);
            btnAddMenu.Name = "btnAddMenu";
            btnAddMenu.Size = new Size(148, 29);
            btnAddMenu.TabIndex = 11;
            btnAddMenu.Text = "Tambah Menu";
            btnAddMenu.UseVisualStyleBackColor = false;
            // 
            // shiftReport
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "shiftReport";
            Size = new Size(912, 580);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel2;
        private Label label1;
        private Panel panel1;
        private Button btnAddMenu;
        private IconButton btnPengeluaran;
        private DataGridView dataGridView1;
        private IconButton btnCetakStruk;
        private Label lblNotifikasi;
        private Label lblShiftSekarang;
        private Panel panel5;
        private Label label3;
        private TextBox txtActualCash;
        private Panel panel3;
        private TextBox txtNamaKasir;
        private IconButton btnRiwayatShift;
    }
}
