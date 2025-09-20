
using FontAwesome.Sharp;
namespace KASIR.OfflineMode
{
    partial class DevMonitor
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
            components = new System.ComponentModel.Container();
            panel2 = new Panel();
            panelComplaint = new Panel();
            panelConfirm = new Panel();
            panel3 = new Panel();
            pictureBox1 = new PictureBox();
            textBox1 = new TextBox();
            label1 = new Label();
            panel1 = new Panel();
            timer1 = new System.Windows.Forms.Timer(components);
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel2.BackColor = Color.WhiteSmoke;
            panel2.Controls.Add(panelComplaint);
            panel2.Controls.Add(panelConfirm);
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(label1);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 70);
            panel2.Name = "panel2";
            panel2.Size = new Size(912, 510);
            panel2.TabIndex = 7;
            // 
            // panelComplaint
            // 
            panelComplaint.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelComplaint.Location = new Point(304, 40);
            panelComplaint.Name = "panelComplaint";
            panelComplaint.Size = new Size(586, 467);
            panelComplaint.TabIndex = 15;
            // 
            // panelConfirm
            // 
            panelConfirm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panelConfirm.Location = new Point(13, 40);
            panelConfirm.Name = "panelConfirm";
            panelConfirm.Size = new Size(285, 467);
            panelConfirm.TabIndex = 14;
            // 
            // panel3
            // 
            panel3.BackColor = Color.WhiteSmoke;
            panel3.Controls.Add(pictureBox1);
            panel3.Controls.Add(textBox1);
            panel3.Location = new Point(13, 5);
            panel3.Name = "panel3";
            panel3.Size = new Size(608, 29);
            panel3.TabIndex = 13;
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = Properties.Resources.search_20px;
            pictureBox1.BackgroundImageLayout = ImageLayout.Center;
            pictureBox1.InitialImage = Properties.Resources.search_20px;
            pictureBox1.Location = new Point(0, 1);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(30, 28);
            pictureBox1.TabIndex = 13;
            pictureBox1.TabStop = false;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.WhiteSmoke;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Location = new Point(37, 5);
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "Cari transaksi ...";
            textBox1.Size = new Size(568, 16);
            textBox1.TabIndex = 12;
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
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(912, 70);
            panel1.TabIndex = 6;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 2000;
            // 
            // DevMonitor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "DevMonitor";
            Size = new Size(912, 580);
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel2;
        private Label label1;
        private Panel panel1;
        private Panel panel3;
        private PictureBox pictureBox1;
        private TextBox textBox1;
        private System.Windows.Forms.Timer timer1;
        private Panel panelComplaint;
        private Panel panelConfirm;
    }
}
