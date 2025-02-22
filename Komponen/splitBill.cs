
using FontAwesome.Sharp;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using KASIR.komponen;
using Serilog;

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
namespace KASIR.Komponen
{
    public partial class splitBill : Form
    {
        private readonly ILogger _log = LoggerService.Instance._log;
        private readonly string baseOutlet;
        private readonly string MacAddressKasir;
        private readonly string MacAddressKitchen;
        private readonly string MacAddressBar;
        private readonly string PinPrinterKasir;
        private readonly string PinPrinterKitchen;
        private readonly string PinPrinterBar;
        private readonly string BaseOutletName;
        private DataTable dataTable2;
        private Dictionary<int, int> originalQuantities = new Dictionary<int, int>();
        List<RequestCartModel> cartDetails = new List<RequestCartModel>();
        public bool ReloadDataInBaseForm { get; private set; }
        string cart_id;
        public splitBill(string cartID)
        {
            cart_id = cartID;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            InitializeComponent();
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            MacAddressKitchen = Properties.Settings.Default.MacAddressKitchen;
            MacAddressBar = Properties.Settings.Default.MacAddressBar;
            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            PinPrinterKitchen = Properties.Settings.Default.PinPrinterKitchen;
            PinPrinterBar = Properties.Settings.Default.PinPrinterBar;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;
            Openform();
        }

        private async void Openform()
        {
            btnSimpan.Enabled = false;
            btnKeluar.Enabled = false;
            await LoadCart();
            btnSimpan.Enabled = true;
            btnKeluar.Enabled = true;
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }
        public async Task LoadCart()
        {
            int retryCount = 0;
            bool isSuccess = false;

            while (retryCount < 3 && !isSuccess)
            {
                try
                {
                    if (dataGridView1 == null)
                    {
                        // Log or handle the error here
                        return;
                    }

                    // Ensure baseOutlet is not null or empty
                    if (string.IsNullOrEmpty(baseOutlet))
                    {
                        /*MessageBox.Show("Outlet ID is not valid.");*/
                        return;
                    }
                    IApiService apiService = new ApiService();
                    string response = await apiService.Get("/cart?outlet_id=" + baseOutlet);

                    // Check if the response is not empty or null
                    if (!string.IsNullOrEmpty(response))
                    {
                        try
                        {
                            // Attempt to deserialize the response
                            GetCartModel dataModel = JsonConvert.DeserializeObject<GetCartModel>(response);

                            // Ensure dataModel and its properties are not null
                            if (dataModel != null && dataModel.data != null && dataModel.data.cart_details != null)
                            {
                                List<DetailCart> cartList = dataModel.data.cart_details;
                                if (cartList == null || !cartList.Any())
                                {
                                    MessageBox.Show("Cart details are empty.");
                                    return;
                                }
                                // Initialize the DataTable for the DataGridView
                                DataTable dataTable = new DataTable();
                                dataTable.Columns.Add("MenuID", typeof(string));
                                dataTable.Columns.Add("CartDetailID", typeof(int));
                                dataTable.Columns.Add("Jenis", typeof(string));
                                dataTable.Columns.Add("Menu", typeof(string));
                                dataTable.Columns.Add("Jumlah", typeof(string));
                                dataTable.Columns.Add("Total Harga", typeof(string));
                                dataTable.Columns.Add("Note", typeof(string));
                                dataTable.Columns.Add("Minus", typeof(string));
                                dataTable.Columns.Add("Hasil", typeof(string));
                                dataTable.Columns.Add("Plus", typeof(string));

                                // Fill the dataTable with the cart details
                                foreach (DetailCart menu in cartList)
                                {
                                    dataTable.Rows.Add(
                                        menu.menu_id,
                                        menu.cart_detail_id,
                                        menu.serving_type_name,
                                        menu.menu_name + " " + menu.varian,
                                        "x" + menu.qty,
                                        "Rp " + menu.total_price,
                                        null,
                                        "-",
                                        "0",
                                        "+");

                                    if (!string.IsNullOrEmpty(menu.note_item))
                                    {
                                        dataTable.Rows.Add(null, null, null, "*catatan : " + menu.note_item, null, null, null, null, null, null);
                                    }
                                }

                                // Check if dataGridView1 is initialized
                                if (dataGridView1 != null)
                                {
                                    dataGridView1.DataSource = dataTable;
                                    dataTable2 = dataTable.Copy();

                                    // Check if the columns exist before trying to access them
                                    if (dataGridView1.Columns.Contains("MenuID"))
                                    {
                                        dataGridView1.Columns["MenuID"].Visible = false;
                                    }
                                    if (dataGridView1.Columns.Contains("CartDetailID"))
                                    {
                                        dataGridView1.Columns["CartDetailID"].Visible = false;
                                    }
                                    if (dataGridView1.Columns.Contains("Jenis"))
                                    {
                                        dataGridView1.Columns["Jenis"].Visible = false;
                                    }
                                    if (dataGridView1.Columns.Contains("Note"))
                                    {
                                        dataGridView1.Columns["Note"].Visible = false;
                                    }

                                    int minusColumn = dataGridView1.Columns["Minus"].Index;
                                    int plusColumn = dataGridView1.Columns["Plus"].Index;

                                    foreach (DataGridViewRow row in dataGridView1.Rows)
                                    {
                                        if (row.Cells["Jenis"].Value != null) // Check if the row is not a separator row
                                        {
                                            DataGridViewTextBoxCell minusButtonCell = new DataGridViewTextBoxCell();
                                            minusButtonCell.Value = "-";
                                            minusButtonCell.Style.Font = new Font("Arial", 10, FontStyle.Bold);
                                            minusButtonCell.Style.ForeColor = Color.Red;

                                            row.Cells[minusColumn] = minusButtonCell;

                                            DataGridViewTextBoxCell plusButtonCell = new DataGridViewTextBoxCell();
                                            plusButtonCell.Value = "+";
                                            plusButtonCell.Style.Font = new Font("Arial", 10, FontStyle.Bold);
                                            plusButtonCell.Style.ForeColor = Color.Green;

                                            row.Cells[plusColumn] = plusButtonCell;
                                        }
                                    }

                                    for (int rowIndex = 0; rowIndex < dataGridView1.Rows.Count; rowIndex++)
                                    {
                                        var menuValue = dataGridView1.Rows[rowIndex].Cells[3].Value?.ToString();

                                        if (menuValue != null && (menuValue.EndsWith("s") || menuValue.StartsWith("*")))
                                        {
                                            dataGridView1.Rows[rowIndex].Cells[minusColumn].Value = "";
                                            dataGridView1.Rows[rowIndex].Cells[plusColumn].Value = "";
                                        }
                                    }

                                    dataGridView1.ColumnHeadersVisible = false;

                                    dataGridView1.Columns["Minus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                    dataGridView1.Columns["Minus"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                                    dataGridView1.Columns["Plus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                    dataGridView1.Columns["Plus"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                                    dataGridView1.Columns["Hasil"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                    dataGridView1.Columns["Hasil"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                                    isSuccess = true; // Mark as successful if everything goes fine
                                }
                            }
                            else
                            {
                                // Log or handle the case where cart_details is null or dataModel is invalid
                                /*MessageBox.Show("Data not found or in unexpected format.");*/
                                return; // Exit the function as the data format is incorrect
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            /*MessageBox.Show("Error parsing the response data.");*/
                            LoggerUtil.LogError(jsonEx, "Error deserializing API response: {ErrorMessage}", jsonEx.Message);
                            return; // Exit the function if parsing fails
                        }
                    }
                    else
                    {
                        MessageBox.Show("No data received from the server.");
                    }
                }
                catch (TaskCanceledException ex)
                {
                    retryCount++;
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
                catch (Exception ex)
                {
                    retryCount++;
                   
                    LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                }
            }
        }

        public async void Ex_LoadCart()
        {
            try
            {
                IApiService apiService = new ApiService();
                string response = await apiService.Get("/cart?outlet_id=" + baseOutlet);
                if (!string.IsNullOrEmpty(response))
                {
                    GetCartModel dataModel = JsonConvert.DeserializeObject<GetCartModel>(response);
                    List<DetailCart> cartList = dataModel.data.cart_details;

                    Dictionary<string, List<DetailCart>> menuGroups = new Dictionary<string, List<DetailCart>>();

                    foreach (DetailCart menu in cartList)
                    {
                        if (!menuGroups.ContainsKey(menu.serving_type_name))
                        {
                            menuGroups[menu.serving_type_name] = new List<DetailCart>();
                        }
                        menuGroups[menu.serving_type_name].Add(menu);
                    }

                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("MenuID", typeof(string));
                    dataTable.Columns.Add("CartDetailID", typeof(int));
                    dataTable.Columns.Add("Jenis", typeof(string));
                    dataTable.Columns.Add("Menu", typeof(string));
                    dataTable.Columns.Add("Jumlah", typeof(string));
                    dataTable.Columns.Add("Total Harga", typeof(string));
                    dataTable.Columns.Add("Note", typeof(string));
                    dataTable.Columns.Add("Minus", typeof(string));
                    dataTable.Columns.Add("Hasil", typeof(string));
                    dataTable.Columns.Add("Plus", typeof(string));
                    string currentJenis = null;

                    foreach (var group in menuGroups)
                    {
                        dataTable.Rows.Add(null, null, null, group.Key + "s", null, null, null, null,null,null); // Add a separator row
                        foreach (DetailCart menu in group.Value)
                        {
                            dataTable.Rows.Add(menu.menu_id, menu.cart_detail_id, menu.serving_type_name, menu.menu_name + " " + menu.varian, "x" + menu.qty, "Rp " + menu.total_price, null, "-","0","+");
                            if (!string.IsNullOrEmpty(menu.note_item))
                            {
                                dataTable.Rows.Add(null, null, null, "*catatan : " + menu.note_item, null, null, null, null,null,null);
                            }
                        }
                    }

                    dataGridView1.DataSource = dataTable;
                    dataTable2 = dataTable.Copy();
                    dataGridView1.Columns["MenuID"].Visible = false;
                    dataGridView1.Columns["CartDetailID"].Visible = false;
                    dataGridView1.Columns["Jenis"].Visible = false;
                    dataGridView1.Columns["Note"].Visible = false;

                    int minusColumn = dataGridView1.Columns["Minus"].Index;
                    int plusColumn = dataGridView1.Columns["Plus"].Index;
                    // Loop through the rows and replace cells in the "Split" column with buttons
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells["Jenis"].Value != null) // Check if the row is not a separator row
                        {
                            DataGridViewTextBoxCell minusButtonCell = new DataGridViewTextBoxCell();
                            minusButtonCell.Value = "-";
                            minusButtonCell.Style.Font = new Font("Arial", 10, FontStyle.Bold); // Example: Set font and style
                            minusButtonCell.Style.ForeColor = Color.Red; // Example: Set color
                                                                         // Set other properties to resemble a button-like appearance

                            row.Cells[minusColumn] = minusButtonCell;

                            DataGridViewTextBoxCell plusButtonCell = new DataGridViewTextBoxCell();
                            plusButtonCell.Value = "+";
                            plusButtonCell.Style.Font = new Font("Arial", 10, FontStyle.Bold); // Example: Set font and style
                            plusButtonCell.Style.ForeColor = Color.Green; // Example: Set color
                                                                          // Set other properties to resemble a button-like appearance

                            row.Cells[plusColumn] = plusButtonCell;
                        }
                    }
                    for (int rowIndex = 0; rowIndex < dataGridView1.Rows.Count; rowIndex++)
                    {
                        var menuValue = dataGridView1.Rows[rowIndex].Cells[3].Value?.ToString(); // Assuming "Menu" column is at index 3

                        if (menuValue != null && (menuValue.EndsWith("s") || menuValue.StartsWith("*")))
                        {
                            dataGridView1.Rows[rowIndex].Cells[minusColumn].Value = ""; // Remove content in "Minus" column cell
                            dataGridView1.Rows[rowIndex].Cells[plusColumn].Value = ""; // Remove content in "Plus" column cell
                        }
                    }
                    dataGridView1.ColumnHeadersVisible = false;

                    // Set column width to wrap cell content for Minus and Plus columns
                    dataGridView1.Columns["Minus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns["Minus"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                    dataGridView1.Columns["Plus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns["Plus"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                    dataGridView1.Columns["Hasil"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns["Hasil"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }
                else
                {
                    // Handle the 404 error here, for example:
                    MessageBox.Show("Data tidak ditemukan");
                }

            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gagal tampil data " + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);   
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == dataGridView1.Columns["Minus"].Index || e.ColumnIndex == dataGridView1.Columns["Plus"].Index))
            {
                var cartDetailId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["CartDetailID"].Value);
                var qtyToSplitCell = dataGridView1.Rows[e.RowIndex].Cells["Hasil"]; // Access the QtyToSplit column
                var jumlahCell = dataGridView1.Rows[e.RowIndex].Cells["Jumlah"]; // Access the Jumlah column

                if (!originalQuantities.ContainsKey(cartDetailId))
                {
                    originalQuantities[cartDetailId] = 1;
                }

                int originalQuantity = originalQuantities[cartDetailId];

                // Get the maximum quantity allowed from the Jumlah column
                int maxQuantity = int.Parse(jumlahCell.Value.ToString().Replace("x", ""));

                int currentQuantity;
                if (int.TryParse(qtyToSplitCell.Value?.ToString(), out currentQuantity))
                {
                    if (e.ColumnIndex == dataGridView1.Columns["Minus"].Index)
                    {
                        if (currentQuantity > 0)
                        {
                            currentQuantity--;
                        }
                        else
                        {
                            MessageBox.Show("Kuantitas telah mencapai batas minimum");
                            return;
                        }
                    }
                    else if (e.ColumnIndex == dataGridView1.Columns["Plus"].Index)
                    {
                        if (currentQuantity < maxQuantity)
                        {
                            currentQuantity++;
                        }
                        else
                        {
                            MessageBox.Show("Kuantitas telah mencapai batas maksimal");
                            return;
                        }
                    }

                    qtyToSplitCell.Value = currentQuantity.ToString();

                    // Update or add the item to the cartDetails list
                    var existingItem = cartDetails.FirstOrDefault(item => int.Parse(item.cart_detail_id) == cartDetailId);
                    if (existingItem != null)
                    {
                        existingItem.qty_to_split = currentQuantity.ToString();
                    }
                    else
                    {
                        cartDetails.Add(new RequestCartModel
                        {
                            cart_detail_id = cartDetailId.ToString(),
                            qty_to_split = currentQuantity.ToString()
                        });
                    }
                }
                else
                {
                    MessageBox.Show("Kuantitas Invalid");
                }
            }
        }



        private void UpdateDataTable(int rowIndex, int newQuantity)
        {
            if (rowIndex >= 0 && rowIndex < dataTable2.Rows.Count)
            {
                // Update the DataTable with the new quantity
                dataTable2.Rows[rowIndex]["Jumlah"] = "x" + newQuantity;
                dataGridView1.Refresh();
            }
        }

        private async void btnSimpanSplit_ClickAsync(object sender, EventArgs e)
        {
                if (cartDetails.Count == 0)
                {
                    MessageBox.Show("Kuantitas item yang ingin di split masih nol", "Split Bill", MessageBoxButtons.OK);
                    return;
                }

                ////LoggerUtil.LogPrivateMethod(nameof(btnSimpanSplit_ClickAsync));

                Dictionary<string, object> json = new Dictionary<string, object>
                {
                    { "outlet_id", baseOutlet.ToString() },
                    { "cart_id", cart_id },
                    { "cart_details", cartDetails },
                };
                string jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
                IApiService apiService = new ApiService();
                string response = await apiService.SplitBill(jsonString, "/split-cart");
                if (response != null)
                {
                DialogResult = DialogResult.OK;
                Close();
                /*
                MessageModel message = JsonConvert.DeserializeObject<MessageModel>(response);
                    if (message.message.Contains("Cart berhasil dipisah!"))
                    {
                        if (Application.OpenForms["masterPos"] is masterPos masterPosForm)
                        {
                            // Call a method in the MasterPos form to refresh the cart
                            masterPosForm.LoadCart(); // You'll need to define this method in MasterPos
                            

                    }
                    DialogResult result = MessageBox.Show("Split Bill Berhasil", "Split Bill", MessageBoxButtons.OK);

                        if (result == DialogResult.OK)
                        {
                            //ReloadDataInBaseForm = true;
                            masterPos MasterposInstance = new masterPos();
                            MasterposInstance.LoadCart();

                        }
                        this.DialogResult = result;
                        */
                    }
                    else
                    {
                DialogResult = DialogResult.Cancel;
                Close();
                MessageBox.Show("Split Bill Gagal", "Split Bill", MessageBoxButtons.OK);
                    }



                
  
        }
    }
}
