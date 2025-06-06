using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
namespace KASIR.Komponen
{
    public partial class Offline_notifikasiPengeluaran : Form
    {
        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        private readonly string baseOutlet;
        private readonly ILogger _log = LoggerService.Instance._log;
        public Offline_notifikasiPengeluaran()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            txtNotes.PlaceholderText = "Tujuan Pengeluaran?";
            button2.Enabled = false;
            LoadData();
        }

        // Modifikasi method LoadData() untuk menambahkan handling error
        private async void LoadData()
        {
            try
            {
                // Read the expenditure data from the file
                var expenditures = ReadExpendituresFromFile("DT-Cache\\Transaction\\expenditure.data");

                var dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(string));
                dataTable.Columns.Add("DATA", typeof(string));
                dataTable.Columns.Add("Detail", typeof(string));
                dataTable.Columns.Add("Created", typeof(string));

                // Ensure safe access to outlet_name

                if (expenditures != null && expenditures.Any())
                {
                    dataTable.Rows.Add(null, "EXPENSE", null, null);
                    foreach (var expense in expenditures)
                    {
                        dataTable.Rows.Add(null, expense?.description ?? "No description", $"Rp. {expense?.nominal:n0},-", expense?.created_at.ToString() ?? "NoTime");
                    }
                }

                dataGridView1.DataSource = dataTable;

                var boldStyle = new DataGridViewCellStyle
                {
                    Font = new Font(dataGridView1.Font, FontStyle.Italic)
                };
                dataGridView1.Columns["DATA"].DefaultCellStyle = boldStyle;
                dataGridView1.Columns["ID"].Visible = false;

                button2.Enabled = true;
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "TaskCanceledException: {ErrorMessage}", ex.Message);
                CleanFormAndAddRetryButton("Koneksi tidak stabil: " + ex.Message);
            }
            catch (JsonSerializationException ex)
            {
                LoggerUtil.LogError(ex, "Deserialization error: {ErrorMessage}", ex.Message);
                CleanFormAndAddRetryButton("Gagal memproses data: " + ex.Message);
            }
            catch (NullReferenceException ex)
            {
                LoggerUtil.LogError(ex, "Null reference error: {ErrorMessage}", ex.Message);
                CleanFormAndAddRetryButton("Data referensi tidak ditemukan: " + ex.Message);
                button2.Enabled = false;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Unexpected error: {ErrorMessage}", ex.Message);
                CleanFormAndAddRetryButton("Terjadi kesalahan: " + ex.Message);
                button2.Enabled = false;
            }
        }

        // Method to read expenditures data from file
        private List<ExpenditureStrukShift> ReadExpendituresFromFile(string filePath)
        {
            try
            {
                var fileContent = File.ReadAllText(filePath);
                var expenditureData = JsonConvert.DeserializeObject<ExpenditureData>(fileContent);
                return expenditureData?.data ?? new List<ExpenditureStrukShift>();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "Error reading or deserializing file: {ErrorMessage}", ex.Message);
                return new List<ExpenditureStrukShift>();
            }
        }


        // Tambahkan method CleanFormAndAddRetryButton dari contoh yang diberikan
        private void CleanFormAndAddRetryButton(string ex)
        {
            // Bersihkan form
            if (dataGridView1 != null && dataGridView1.DataSource != null)
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
            }

            // Tambahkan PictureBox untuk gambar
            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = "ErrorImg";
            pictureBox.Size = new Size(100, 100); // Sesuaikan ukuran gambar
            pictureBox.Location = new Point((this.ClientSize.Width - pictureBox.Width) / 2, (this.ClientSize.Height - pictureBox.Height) / 2 - 110); // Posisi di atas tombol
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Atur ukuran gambar agar sesuai dengan PictureBox
            try
            {
                using (FileStream fs = new FileStream("icon\\OutletLogo.bmp", FileMode.Open, FileAccess.Read))
                {
                    pictureBox.Image = Image.FromStream(fs);
                }
            }
            catch
            {
                // Jika gagal memuat gambar, jangan tampilkan PictureBox
                pictureBox.Visible = false;
            }

            // Tambahkan tombol retry
            System.Windows.Forms.Button btnRetry = new System.Windows.Forms.Button();
            btnRetry.Name = "btnRetry";
            btnRetry.Text = "Retry Load Data\nOut of Service";
            btnRetry.Size = new Size(190, 60);
            btnRetry.Location = new Point((this.ClientSize.Width - btnRetry.Width) / 2, (this.ClientSize.Height - btnRetry.Height) / 2);
            btnRetry.BackColor = Color.FromArgb(30, 31, 68);
            btnRetry.FlatStyle = FlatStyle.Flat;
            btnRetry.Font = new Font("Arial", 10, FontStyle.Bold);
            btnRetry.ForeColor = Color.White; // Mengatur warna teks tombol menjadi putih
            btnRetry.Click += new EventHandler(BtnRetry_Click);

            // Membuat sudut membulat
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 20, 20, 180, 90);
            path.AddArc(btnRetry.Width - 20, 0, 20, 20, 270, 90);
            path.AddArc(btnRetry.Width - 20, btnRetry.Height - 20, 20, 20, 0, 90);
            path.AddArc(0, btnRetry.Height - 20, 20, 20, 90, 90);
            path.CloseFigure();
            btnRetry.Region = new Region(path);

            // Menambahkan label di bawah tombol
            Label lblInfo = new Label();
            lblInfo.Name = "ErrorMsg";
            lblInfo.Text = ex.ToString(); // Teks yang ingin ditampilkan
            lblInfo.Font = new Font("Arial", 9, FontStyle.Regular);
            lblInfo.ForeColor = Color.Black; // Warna teks label
            lblInfo.AutoSize = true; // Agar label menyesuaikan ukuran teks

            // Mengatur posisi label agar berada di tengah
            lblInfo.Location = new Point((this.ClientSize.Width - lblInfo.Width) / 4, btnRetry.Bottom + 10); // Posisi di bawah tombol

            // Menambahkan kontrol ke form
            this.Controls.Add(pictureBox); // Menambahkan PictureBox ke form
            this.Controls.Add(btnRetry);
            this.Controls.Add(lblInfo); // Menambahkan label ke form

            btnRetry.BringToFront();
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            // Hapus tombol retry
            if (sender is System.Windows.Forms.Button btnRetry)
            {
                this.Controls.Remove(btnRetry);
            }

            // Hapus label informasi jika ada
            var lblInfo = this.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "ErrorMsg");
            if (lblInfo != null)
            {
                this.Controls.Remove(lblInfo);
            }

            var ErrImg = this.Controls.OfType<PictureBox>().FirstOrDefault(lbl => lbl.Name == "ErrorImg");
            if (ErrImg != null)
            {
                this.Controls.Remove(ErrImg);
            }

            // Muat data kembali
            LoadData();
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
            this.Close();
        }
        private void AddItem(string name, string amount)
        {


        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel13_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void btnSaveExpenditure(object sender, EventArgs e)
        {
            string fulus = Regex.Replace(txtNominal.Text, "[^0-9]", "");
            try
            {
                if (fulus == null || fulus.ToString() == "")
                {
                    MessageBox.Show("Format nominal kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (txtNotes.Text == null || txtNotes.ToString() == "")
                {
                    MessageBox.Show("Format notes kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                DialogResult yakin = MessageBox.Show($"Yakin menambahkan Expenditure Rp.{txtNominal.Text},- dengan tujuan \n {txtNotes.Text} ?", "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (yakin != DialogResult.Yes)
                {
                    MessageBox.Show("Penambahan Expenditure Shift diCancel");
                    return;
                    this.Close();
                }
                var expenditureData = new
                {
                    nominal = int.Parse(fulus.ToString()),
                    description = txtNotes.Text.ToString(),
                    outlet_id = baseOutlet,
                    
                    //adding for cache details

                    created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    is_sync = 0
                };

                // Save transaction data to transaction.data
                string expenditureDataPath = "DT-Cache\\Transaction\\expenditure.data";
                JArray expenditureDataArray = new JArray();
                if (File.Exists(expenditureDataPath))
                {
                    // If the transaction file exists, read and append the new transaction
                    string existingData = File.ReadAllText(expenditureDataPath);
                    var existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                    expenditureDataArray = existingTransactions["data"] as JArray ?? new JArray();
                }

                // Add new transaction
                expenditureDataArray.Add(JToken.FromObject(expenditureData));

                // Serialize and save back to transaction.data
                var newTransactionData = new JObject { { "data", expenditureDataArray } };
                File.WriteAllText(expenditureDataPath, JsonConvert.SerializeObject(newTransactionData, Formatting.Indented));

                DialogResult result = MessageBox.Show("Input notifikasi pengeluaran berhasil", "Gaspol", MessageBoxButtons.OK);
                if (result == DialogResult.OK)
                {
                    this.Close(); // Close the payForm
                }
                this.DialogResult = result;
            }
            catch (TaskCanceledException ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }

        }

        private void txtJumlahCicil_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
        private void txtSelesaiShift_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNominal_TextChanged(object sender, EventArgs e)
        {
            if (txtNominal.Text == "" || txtNominal.Text == "0") return;
            decimal number;
            try
            {
                number = decimal.Parse(txtNominal.Text, System.Globalization.NumberStyles.Currency);
            }
            catch (FormatException)
            {
                // The text could not be parsed as a decimal number.
                // You can handle this exception in different ways, such as displaying a message to the user.
                MessageBox.Show("inputan hanya bisa Numeric");
                if (txtNominal.Text.Length > 0)
                {
                    txtNominal.Text = txtNominal.Text.Substring(0, txtNominal.Text.Length - 1);
                    txtNominal.SelectionStart = txtNominal.Text.Length; // Move the cursor to the end
                }

                return;
            }
            txtNominal.Text = number.ToString("#,#");
            txtNominal.SelectionStart = txtNominal.Text.Length;
        }
    }

}

