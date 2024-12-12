namespace KASIR.komponen
{
    partial class update
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(update));
            panel1 = new Panel();
            panel2 = new Panel();
            label6 = new Label();
            pictureBox2 = new PictureBox();
            label1 = new Label();
            panel4 = new Panel();
            comboBox1 = new ComboBox();
            label8 = new Label();
            textBox3 = new TextBox();
            label5 = new Label();
            textBox2 = new TextBox();
            label3 = new Label();
            label4 = new Label();
            button5 = new Button();
            panel3 = new Panel();
            label7 = new Label();
            button1 = new Button();
            button4 = new Button();
            textBox1 = new TextBox();
            label2 = new Label();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel4.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(panel4);
            panel1.Dock = DockStyle.Fill;
            panel1.ForeColor = Color.SteelBlue;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(490, 346);
            panel1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(label6);
            panel2.Controls.Add(pictureBox2);
            panel2.Controls.Add(label1);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(488, 66);
            panel2.TabIndex = 6;
            // 
            // label6
            // 
            label6.BackColor = Color.DarkGray;
            label6.Location = new Point(20, 65);
            label6.Name = "label6";
            label6.Size = new Size(450, 1);
            label6.TabIndex = 9;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.BackgroundImage = (Image)resources.GetObject("pictureBox2.BackgroundImage");
            pictureBox2.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox2.Location = new Point(18, 8);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(50, 50);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.FlatStyle = FlatStyle.Flat;
            label1.Font = new Font("Segoe UI", 19F, FontStyle.Regular, GraphicsUnit.Point);
            label1.ForeColor = Color.SteelBlue;
            label1.Location = new Point(172, 14);
            label1.Name = "label1";
            label1.Size = new Size(147, 36);
            label1.TabIndex = 0;
            label1.Text = "Edit Produk";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            panel4.BackColor = Color.WhiteSmoke;
            panel4.Controls.Add(comboBox1);
            panel4.Controls.Add(label8);
            panel4.Controls.Add(textBox3);
            panel4.Controls.Add(label5);
            panel4.Controls.Add(textBox2);
            panel4.Controls.Add(label3);
            panel4.Controls.Add(label4);
            panel4.Controls.Add(button5);
            panel4.Controls.Add(panel3);
            panel4.Controls.Add(textBox1);
            panel4.Controls.Add(label2);
            panel4.Dock = DockStyle.Fill;
            panel4.ForeColor = Color.Transparent;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Size = new Size(488, 344);
            panel4.TabIndex = 7;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Makanan", "Minuman" });
            comboBox1.Location = new Point(194, 225);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(274, 23);
            comboBox1.TabIndex = 23;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.BackColor = Color.Transparent;
            label8.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            label8.ForeColor = Color.Black;
            label8.Location = new Point(191, 206);
            label8.Name = "label8";
            label8.Size = new Size(82, 19);
            label8.TabIndex = 22;
            label8.Text = "Tipe Produk";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(194, 171);
            textBox3.Name = "textBox3";
            textBox3.PlaceholderText = "Rp.0";
            textBox3.Size = new Size(276, 23);
            textBox3.TabIndex = 21;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.Transparent;
            label5.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(191, 152);
            label5.Name = "label5";
            label5.Size = new Size(94, 19);
            label5.TabIndex = 20;
            label5.Text = "Harga Produk";
            // 
            // textBox2
            // 
            textBox2.BorderStyle = BorderStyle.FixedSingle;
            textBox2.Location = new Point(194, 119);
            textBox2.Name = "textBox2";
            textBox2.PlaceholderText = "Masukkan nama produk";
            textBox2.Size = new Size(276, 23);
            textBox2.TabIndex = 19;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(191, 100);
            label3.Name = "label3";
            label3.Size = new Size(93, 19);
            label3.TabIndex = 18;
            label3.Text = "Nama Produk";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(30, 233);
            label4.Name = "label4";
            label4.Size = new Size(125, 19);
            label4.TabIndex = 17;
            label4.Text = "Masukkan Gambar";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button5
            // 
            button5.AutoSize = true;
            button5.BackColor = Color.Transparent;
            button5.BackgroundImage = (Image)resources.GetObject("button5.BackgroundImage");
            button5.BackgroundImageLayout = ImageLayout.Zoom;
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.ForeColor = Color.Black;
            button5.ImageAlign = ContentAlignment.TopLeft;
            button5.Location = new Point(18, 95);
            button5.Name = "button5";
            button5.Size = new Size(148, 135);
            button5.TabIndex = 16;
            button5.UseVisualStyleBackColor = false;
            // 
            // panel3
            // 
            panel3.BackColor = Color.White;
            panel3.Controls.Add(label7);
            panel3.Controls.Add(button1);
            panel3.Controls.Add(button4);
            panel3.Dock = DockStyle.Bottom;
            panel3.ForeColor = Color.Transparent;
            panel3.Location = new Point(0, 270);
            panel3.Name = "panel3";
            panel3.Size = new Size(488, 74);
            panel3.TabIndex = 9;
            // 
            // label7
            // 
            label7.BackColor = Color.DarkGray;
            label7.Location = new Point(20, 1);
            label7.Name = "label7";
            label7.Size = new Size(450, 1);
            label7.TabIndex = 9;
            // 
            // button1
            // 
            button1.BackColor = Color.WhiteSmoke;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.ForeColor = Color.SteelBlue;
            button1.Location = new Point(384, 16);
            button1.Name = "button1";
            button1.Size = new Size(88, 46);
            button1.TabIndex = 7;
            button1.Text = "Keluar";
            button1.UseVisualStyleBackColor = false;
            // 
            // button4
            // 
            button4.BackColor = Color.SteelBlue;
            button4.FlatAppearance.BorderSize = 0;
            button4.FlatStyle = FlatStyle.Flat;
            button4.ForeColor = Color.White;
            button4.Location = new Point(275, 16);
            button4.Name = "button4";
            button4.Size = new Size(88, 46);
            button4.TabIndex = 6;
            button4.Text = "Simpan";
            button4.UseVisualStyleBackColor = false;
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Location = new Point(12, 41);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(140, 23);
            textBox1.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(8, 14);
            label2.Name = "label2";
            label2.Size = new Size(93, 19);
            label2.TabIndex = 1;
            label2.Text = "Nama Produk";
            // 
            // update
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(490, 346);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "update";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "update";
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private Label label6;
        private PictureBox pictureBox2;
        private Label label1;
        private Panel panel4;
        private ComboBox comboBox1;
        private Label label8;
        private TextBox textBox3;
        private Label label5;
        private TextBox textBox2;
        private Label label3;
        private Label label4;
        private Button button5;
        private Panel panel3;
        private Label label7;
        private Button button1;
        private Button button4;
        private TextBox textBox1;
        private Label label2;
    }
}