
using FontAwesome.Sharp;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using KASIR.komponen;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Globalization;
using Serilog;
using Newtonsoft.Json.Linq;
using System.Transactions;

namespace KASIR.OfflineMode
{
    public partial class Offline_deleteForm : Form
    {
        //private successTransaction SuccessTransaction { get; set; }
        private List<CartDetailTransaction> item = new List<CartDetailTransaction>();
        private List<RefundModel> refundItems = new List<RefundModel>();
        private readonly string MacAddressKasir;
        private readonly string PinPrinterKasir;
        private readonly string BaseOutletName;
        public bool ReloadDataInBaseForm { get; private set; }
        //public bool KeluarButtonPrintReportShiftClicked { get; private set; }
        private readonly string baseOutlet;
        GetTransactionDetail dataTransaction;
        string cart_id;
        private readonly ILogger _log = LoggerService.Instance._log;
        public Offline_deleteForm(string cartId)
        {
            cart_id = cartId;

            PinPrinterKasir = Properties.Settings.Default.PinPrinterKasir;
            MacAddressKasir = Properties.Settings.Default.MacAddressKasir;
            baseOutlet = Properties.Settings.Default.BaseOutlet;
            BaseOutletName = Properties.Settings.Default.BaseOutletName;

            InitializeComponent();


        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
            ////LoggerUtil.LogPrivateMethod(nameof(button1_Click));
            DialogResult = DialogResult.OK;

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

        private async void button2_Click(object sender, EventArgs e)
        {
            ////LoggerUtil.LogPrivateMethod(nameof(button2_Click));
            try
            {
                if (txtPin.Text == null || txtPin.Text.ToString() == "")
                {
                    MessageBox.Show("Pin salah atau format kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (txtReason.Text == null || txtReason.Text.ToString() == "")
                {
                    MessageBox.Show("Format alasan kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string cacheOutlet = File.ReadAllText($"DT-Cache\\DataOutlet{baseOutlet}.data");
                // Deserialize JSON ke object CartDataCache
                var dataOutlet = JsonConvert.DeserializeObject<CartDataOutlet>(cacheOutlet);
                if (txtPin.Text.ToString() != dataOutlet.data.pin.ToString())
                {
                    MessageBox.Show("Pin salah atau format kurang tepat", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string configCart = "DT-Cache\\Transaction\\Cart.data";
                // Read the cart file content as a string
                string json = File.ReadAllText(configCart);

                // Parse the JSON string into a JObject (dynamic)
                JObject cartData = JObject.Parse(json);

                // Find all items in cart_details and set their qty and price to 0
                JArray cartDetails = (JArray)cartData["cart_details"];

                // Check if canceled_items exists, if not create it
                if (cartData["canceled_items"] == null)
                {
                    cartData["canceled_items"] = new JArray();
                }
                var cancelDetails = cartData["canceled_items"] as JArray;

                foreach (var item in cartDetails)
                {
                    // Calculate the canceled item's total price
                    int discounted_priceFix = int.Parse(item["price"].ToString()) - int.Parse(item["discounted_item_price"].ToString());
                    int total_priceCanceled = discounted_priceFix * int.Parse(item["qty"].ToString());

                    cancelDetails.Add(new JObject
                    {
                        ["cart_detail_id"] = item["cart_detail_id"],
                        ["menu_id"] = item["menu_id"],
                        ["menu_name"] = item["menu_name"],
                        ["menu_type"] = item["menu_type"],
                        ["menu_detail_id"] = item["menu_detail_id"],
                        ["menu_detail_name"] = item["menu_detail_name"],
                        ["varian"] = item["varian"],
                        ["is_ordered"] = 1,
                        ["serving_type_id"] = item["serving_type_id"],
                        ["serving_type_name"] = item["serving_type_name"],
                        ["price"] = item["price"],
                        ["qty"] = item["qty"],
                        ["note_item"] = item["note_item"],
                        ["created_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        ["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        ["discount_id"] = int.Parse(item["discount_id"]?.ToString()),
                        ["discount_code"] = item["discount_code"]?.ToString() ?? (string)null,
                        ["discounts_value"] = int.Parse(item["discounts_value"]?.ToString()),
                        ["discounted_price"] = int.Parse(item["discounted_price"]?.ToString()),
                        ["discounts_is_percent"] = int.Parse(item["discounts_is_percent"]?.ToString()),
                        ["discounted_item_price"] = int.Parse(item["discounted_item_price"]?.ToString()),
                        ["cancel_reason"] = txtReason.Text.ToString(),
                        ["subtotal_price"] = item["subtotal_price"],
                        ["total_price"] = item["total_price"]
                    });

                    // Set the quantity and price to 0 for each item in cart_details
                    item["qty"] = 0;
                    item["total_price"] = 0;
                    item["subtotal_price"] = 0;
                }

                // Recalculate subtotal and total
                int subtotal = 0;
                foreach (var item in cartDetails)
                {
                    subtotal += (int)item["total_price"];
                }

                // Update the cart totals
                cartData["subtotal"] = 0;
                cartData["total"] = 0;
                cartData["is_sent_sync"] = 0;
                cartData["is_canceled"] = 1;

                int edited_sync = 0;
                if(int.Parse(cartData["discount_id"]?.ToString()) == 1) { edited_sync = 1; }
                // Prepare transaction data
                var transactionData = new
                {
                    transaction_id = int.TryParse(cartData["transaction_id"]?.ToString(), out var tempTransactionId) ? tempTransactionId : 0,
                    receipt_number = cartData["receipt_number"]?.ToString(),
                    transaction_ref = cartData["transaction_ref"]?.ToString(),
                    transaction_ref_split = (string)null,
                    invoice_number = (string)null,  // Custom invoice number with formatted date
                    invoice_due_date = (string)null, // Adjust due date as needed
                    payment_type_id = int.TryParse(cartData["payment_type_id"]?.ToString(), out var tempPaymentTypeId) ? tempPaymentTypeId : 0,
                    payment_type_name = (string)null, // No need for .ToString() if paymentTypeName is already a string
                    customer_name = cartData["customer_name"]?.ToString(),
                    customer_seat = int.TryParse(cartData["customer_seat"]?.ToString(), out var tempCustomerSeat) ? tempCustomerSeat : 0,
                    customer_cash = 0,
                    customer_change = 0,
                    total = 0,
                    subtotal = 0, // You can replace this with actual subtotal if available
                    created_at = cartData["created_at"]?.ToString(),
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    deleted_at = (string)null, // Ensure deleted_at is null, not a string "null"
                    is_refund = 0,
                    refund_reason = (string)null, // Null if no refund reason
                    delivery_type = (string)null, // Null value for delivery_type
                    delivery_note = (string)null, // Null value for delivery_note
                    discount_id = int.TryParse(cartData["discount_id"]?.ToString(), out var tempDiscountId) ? tempDiscountId : 0,
                    discount_code = cartData["discount_code"]?.ToString(),
                    discounts_value = cartData["discounts_value"]?.ToString(),
                    discounts_is_percent = cartData["discounts_is_percent"]?.ToString(),
                    discounted_price = int.TryParse(cartData["discounted_price"]?.ToString(), out var tempDiscountedPrice) ? tempDiscountedPrice : 0,
                    discounted_peritem_price = int.TryParse(cartData["discounted_peritem_price"]?.ToString(), out var tempDiscountedPeritemPrice) ? tempDiscountedPeritemPrice : 0,
                    member_name = (string)null, // Null if no member name
                    member_phone_number = (string)null, // Null if no member phone number
                    is_refund_all = 0,
                    refund_reason_all = (string)null,
                    refund_payment_id_all = 0,
                    refund_created_at_all = (string)null,
                    total_refund = 0,
                    refund_payment_name_all = (string)null,
                    is_edited_sync = edited_sync,
                    is_sent_sync = 0,
                    is_canceled = 1,
                    is_savebill = int.TryParse(cartData["is_savebill"]?.ToString(), out var tempIsSavebill) ? tempIsSavebill : 0,
                    cart_details = cartDetails,
                    refund_details = new JArray(), // Empty array for refund_details
                    canceled_items = cancelDetails // Empty array for canceled_items
                };

                // Save transaction data to transaction.data
                string transactionDataPath = "DT-Cache\\Transaction\\transaction.data";
                JArray transactionDataArray = new JArray();
                if (File.Exists(transactionDataPath))
                {
                    // If the transaction file exists, read and append the new transaction
                    string existingData = File.ReadAllText(transactionDataPath);
                    var existingTransactions = JsonConvert.DeserializeObject<JObject>(existingData);
                    transactionDataArray = existingTransactions["data"] as JArray ?? new JArray();
                }

                // Add new transaction
                transactionDataArray.Add(JToken.FromObject(transactionData));

                // Serialize and save back to transaction.data
                var newTransactionData = new JObject { { "data", transactionDataArray } };
                File.WriteAllText(transactionDataPath, JsonConvert.SerializeObject(newTransactionData, Formatting.Indented));
                // Tentukan lokasi path file yang ingin dihapus
                string filePath = "DT-Cache\\Transaction\\Cart.data";

                // Cek apakah file tersebut ada
                if (File.Exists(filePath))
                {
                    // Hapus file
                    File.Delete(filePath);
                }
                DialogResult = DialogResult.OK;

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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


    }

}

