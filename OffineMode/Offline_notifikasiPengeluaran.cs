using System.Data;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;
using KASIR.Model;
using KASIR.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using KASIR.Helper;

namespace KASIR.Komponen
{
    public partial class Offline_notifikasiPengeluaran : Form
    {

        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        private readonly string baseOutlet;

        public Offline_notifikasiPengeluaran()
        {
            baseOutlet = Settings.Default.BaseOutlet;
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
                List<ExpenditureStrukShift>? expenditures =
                    ReadExpendituresFromFile("DT-Cache\\Transaction\\expenditure.data");

                DataTable dataTable = new();
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
                        dataTable.Rows.Add(null, expense?.description ?? "No description",
                            $"Rp. {expense?.nominal:n0},-", expense?.created_at ?? "NoTime");
                    }
                }

                dataGridView1.DataSource = dataTable;

                DataGridViewCellStyle boldStyle = new() { Font = new Font(dataGridView1.Font, FontStyle.Italic) };
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
                string fileContent = File.ReadAllText(filePath);
                ExpenditureData? expenditureData = JsonConvert.DeserializeObject<ExpenditureData>(fileContent);
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
            try
            {
                // Bersihkan form
                if (dataGridView1 != null && dataGridView1.DataSource != null)
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.Rows.Clear();
                }

                // Tambahkan PictureBox untuk gambar
                PictureBox pictureBox = new();
                pictureBox.Name = "ErrorImg";
                pictureBox.Size = new Size(100, 100); // Sesuaikan ukuran gambar
                pictureBox.Location = new Point((ClientSize.Width - pictureBox.Width) / 2,
                    ((ClientSize.Height - pictureBox.Height) / 2) - 110); // Posisi di atas tombol
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Atur ukuran gambar agar sesuai dengan PictureBox
                try
                {
                    using (FileStream fs = new("icon\\OutletLogo.bmp", FileMode.Open, FileAccess.Read))
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
                Button btnRetry = new();
                btnRetry.Name = "btnRetry";
                btnRetry.Text = "Retry Load Data\nOut of Service";
                btnRetry.Size = new Size(190, 60);
                btnRetry.Location = new Point((ClientSize.Width - btnRetry.Width) / 2,
                    (ClientSize.Height - btnRetry.Height) / 2);
                btnRetry.BackColor = Color.FromArgb(30, 31, 68);
                btnRetry.FlatStyle = FlatStyle.Flat;
                btnRetry.Font = new Font("Arial", 10, FontStyle.Bold);
                btnRetry.ForeColor = Color.White; // Mengatur warna teks tombol menjadi putih
                btnRetry.Click += BtnRetry_Click;

                // Membuat sudut membulat
                GraphicsPath path = new();
                path.AddArc(0, 0, 20, 20, 180, 90);
                path.AddArc(btnRetry.Width - 20, 0, 20, 20, 270, 90);
                path.AddArc(btnRetry.Width - 20, btnRetry.Height - 20, 20, 20, 0, 90);
                path.AddArc(0, btnRetry.Height - 20, 20, 20, 90, 90);
                path.CloseFigure();
                btnRetry.Region = new Region(path);

                // Menambahkan label di bawah tombol
                Label lblInfo = new();
                lblInfo.Name = "ErrorMsg";
                lblInfo.Text = ex; // Teks yang ingin ditampilkan
                lblInfo.Font = new Font("Arial", 9, FontStyle.Regular);
                lblInfo.ForeColor = Color.Black; // Warna teks label
                lblInfo.AutoSize = true; // Agar label menyesuaikan ukuran teks

                // Mengatur posisi label agar berada di tengah
                lblInfo.Location =
                    new Point((ClientSize.Width - lblInfo.Width) / 4, btnRetry.Bottom + 10); // Posisi di bawah tombol

                // Menambahkan kontrol ke form
                Controls.Add(pictureBox); // Menambahkan PictureBox ke form
                Controls.Add(btnRetry);
                Controls.Add(lblInfo); // Menambahkan label ke form

                btnRetry.BringToFront();
            }
            catch (Exception ez)
            {
                LoggerUtil.LogError(ez, "Error: {ErrorMessage}", ez.Message);
            }
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            try
            {
                // Hapus tombol retry
                if (sender is Button btnRetry)
                {
                    Controls.Remove(btnRetry);
                }

                // Hapus label informasi jika ada
                Label? lblInfo = Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Name == "ErrorMsg");
                if (lblInfo != null)
                {
                    Controls.Remove(lblInfo);
                }

                PictureBox? ErrImg = Controls.OfType<PictureBox>().FirstOrDefault(lbl => lbl.Name == "ErrorImg");
                if (ErrImg != null)
                {
                    Controls.Remove(ErrImg);
                }

                // Muat data kembali
                LoadData();
            }
            catch (Exception ez)
            {
                LoggerUtil.LogError(ez, "Error: {ErrorMessage}", ez.Message);
            }
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
            Close();
        }
        private async void btnSaveExpenditure(object sender, EventArgs e)
        {
            string fulus = Regex.Replace(txtNominal.Text, "[^0-9]", "");
            try
            {
                if (fulus == null || fulus == "")
                {
                    NotifyHelper.Warning("Format nominal kurang tepat");
                    return;
                }

                if (txtNotes.Text == null || txtNotes.ToString() == "")
                {
                    NotifyHelper.Warning("Format notes kurang tepat");
                    return;
                }

                DialogResult yakin =
                    MessageBox.Show(
                        $"Yakin menambahkan Expenditure Rp.{txtNominal.Text},- dengan tujuan \n {txtNotes.Text} ?",
                        "KONFIRMASI", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (yakin != DialogResult.Yes)
                {
                    NotifyHelper.Error("Penambahan Expenditure Shift diCancel");
                    return;
                    Close();
                }

                var expenditureData = new
                {
                    nominal = int.Parse(fulus),
                    description = txtNotes.Text,
                    outlet_id = baseOutlet,

                    //adding for cache details

                    created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    is_sync = 0
                };

                // Save transaction data to transaction.data
                string expenditureDataPath = "DT-Cache\\Transaction\\expenditure.data";
                JArray expenditureDataArray = new();
                if (File.Exists(expenditureDataPath))
                {
                    // If the transaction file exists, read and append the new transaction
                    string existingData = File.ReadAllText(expenditureDataPath);
                    JObject? existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                    expenditureDataArray = existingTransactions["data"] as JArray ?? new JArray();
                }

                // Add new transaction
                expenditureDataArray.Add(JToken.FromObject(expenditureData));

                // Serialize and save back to transaction.data
                JObject newTransactionData = new() { { "data", expenditureDataArray } };
                File.WriteAllText(expenditureDataPath,
                    JsonConvert.SerializeObject(newTransactionData, Formatting.Indented));

                DialogResult result = DialogResult.OK;
                if (result == DialogResult.OK)
                {
                    Close(); // Close the payForm
                }

                DialogResult = result;
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

        private void txtNominal_TextChanged(object sender, EventArgs e)
        {
            if (txtNominal.Text == "" || txtNominal.Text == "0")
            {
                return;
            }

            decimal number;
            try
            {
                number = decimal.Parse(txtNominal.Text, NumberStyles.Currency);
            }
            catch (FormatException)
            {
                // The text could not be parsed as a decimal number.
                // You can handle this exception in different ways, such as displaying a message to the user.
                NotifyHelper.Error("inputan hanya bisa Numeric");
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