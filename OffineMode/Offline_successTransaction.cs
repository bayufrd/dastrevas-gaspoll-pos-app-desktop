using System.Data;
using KASIR.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Color = System.Drawing.Color;
namespace KASIR.OfflineMode
{
    public partial class Offline_successTransaction : UserControl
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private ApiService apiService;
        private DataTable originalDataTable;
        private readonly string baseOutlet;
        //private inputPin pinForm;

        public Offline_successTransaction()
        {
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            apiService = new ApiService();

            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0) // Memastikan kita hanya memformat sel data
            {
                if (e.RowIndex % 2 == 0) // Baris genap
                {
                    e.CellStyle.BackColor = Color.White; // Warna untuk baris genap
                }
                else // Baris ganjil
                {
                    e.CellStyle.BackColor = Color.WhiteSmoke; // Warna untuk baris ganjil
                }
            }
        }

        private void OnRefundSuccess(object sender, EventArgs e)
        {
            // Refresh data in successTransaction form
            LoadData();
        }
        public async Task LoadData()
        {
            try
            {
                // Path untuk file transaction.data
                string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";

                // Cek apakah file transaction.data ada
                if (File.Exists(transactionDataPath))
                {
                    // Membaca isi file transaction.data
                    string transactionJson = File.ReadAllText(transactionDataPath);
                    var transactionData = JsonConvert.DeserializeObject<JObject>(transactionJson);

                    // Ambil array data transaksi
                    var transactionDetails = transactionData["data"] as JArray;

                    // Begin Counting Transaction Queue
                    int numberQueue = transactionDetails.Count + 1; // Start queue number

                    // Prepare DataTable to display the data
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Transaction ID", typeof(string));
                    dataTable.Columns.Add("NumberQueue", typeof(int));
                    dataTable.Columns.Add("Receipt Number", typeof(string));
                    dataTable.Columns.Add("Customer Name", typeof(string));
                    dataTable.Columns.Add("Customer Seat", typeof(string));
                    dataTable.Columns.Add("Total", typeof(string));
                    dataTable.Columns.Add("Payment Type", typeof(string));
                    dataTable.Columns.Add("Transaction Time", typeof(string));

                    // Reverse the transactionDetails array to load from bottom to top
                    var reversedTransactionDetails = transactionDetails.Reverse().ToList();

                    // Loop through each transaction to fill the DataTable
                    foreach (var transaction in reversedTransactionDetails)
                    {
                        numberQueue -= 1; // Decrease number for the next entry

                        // Format total price and other values
                        decimal total = transaction["total"] != null ? decimal.Parse(transaction["total"].ToString()) : 0;
                        string customerName = transaction["customer_name"]?.ToString() ?? "-";
                        string customerSeat = transaction["customer_seat"]?.ToString() ?? "0";
                        string paymentType = transaction["payment_type_name"]?.ToString() ?? "-";

                        DateTime transactionTime;
                        if (transaction["is_canceled"]?.ToString() == "1")
                        {
                            continue;
                        }
                        string refundRemind = "";
                        if (transaction["is_refund"]?.ToString() == "1")
                        {
                            refundRemind = " [Refunded]";
                            if(transaction["is_refund_all"]?.ToString() == "1")
                            {
                                total = 0;
                            }
                        }
                        // Parse the created_at field and format it
                        if (DateTime.TryParse(transaction["created_at"]?.ToString(), out transactionTime))
                        {
                            string formattedDate = transactionTime.ToString("dd MMM yyyy, HH:mm");
                            dataTable.Rows.Add(
                                transaction["transaction_id"]?.ToString(), numberQueue,
                                numberQueue + ". " + transaction["receipt_number"]?.ToString(),
                                customerName,
                                customerSeat,
                                string.Format("Rp. {0:n0},-", total) + refundRemind,
                                paymentType,
                                formattedDate
                            );
                        }
                        else
                        {
                            // If parsing fails, show original date
                            dataTable.Rows.Add(
                                transaction["transaction_id"]?.ToString(), numberQueue,
                                numberQueue + ". " + transaction["receipt_number"]?.ToString(),
                                customerName,
                                customerSeat,
                                string.Format("Rp. {0:n0},-", total) + refundRemind,
                                paymentType,
                                transaction["created_at"]?.ToString()
                            );
                        }
                    }

                    // Bind the data to DataGridView
                    dataGridView1.DataSource = dataTable;
                    originalDataTable = dataTable.Copy();

                    // Optional: Hide other columns as needed
                    dataGridView1.Columns["Transaction ID"].Visible = false;
                    dataGridView1.Columns["NumberQueue"].Visible = false;
                }
                else
                {
                    /* MessageBox.Show("File transaction.data tidak ditemukan.", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);*/
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while reading the transaction file: " + ex.Message, "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void PerformSearch()
        {
            if (originalDataTable == null)
                return;

            string searchTerm = textBox1.Text.ToLower();

            DataTable filteredDataTable = originalDataTable.Clone();

            IEnumerable<DataRow> filteredRows = originalDataTable.AsEnumerable()
                .Where(row => row.ItemArray.Any(field => field.ToString().ToLower().Contains(searchTerm)));

            foreach (DataRow row in filteredRows)
            {
                filteredDataTable.ImportRow(row);
            }
            dataGridView1.DataSource = filteredDataTable;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                string id = selectedRow.Cells["Transaction ID"].Value.ToString();
                int urutanRiwayat = Convert.ToInt32(selectedRow.Cells["NumberQueue"].Value); // Access the NumberQueue here

                LoadPin(id, urutanRiwayat);
            }
        }

        private void LoadPin(string id, int urutanRiwayat)
        {
            Form background = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.7d,
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                Location = this.Location,
                ShowInTaskbar = false,
            };
            using (Offline_inputPin pinForm = new Offline_inputPin(id, urutanRiwayat))
            {
                pinForm.Owner = background;

                background.Show();

                DialogResult dialogResult = pinForm.ShowDialog();

                background.Dispose();
                LoadData();
            }
        }

        private void ReloadData()
        {
            LoadData();
        }





    }
}
