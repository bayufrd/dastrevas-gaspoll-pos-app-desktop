using System.Globalization;
using KASIR.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace KASIR.OfflineMode
{
    public partial class Offline_deletePerItemForm : Form
    {
        //private successTransaction SuccessTransaction { get; set; }
        private List<CartDetailTransaction> item = new List<CartDetailTransaction>();
        private List<RefundModel> refundItems = new List<RefundModel>();
        string cart_detail;
        public Offline_deletePerItemForm(string cartDetail)
        {
            cart_detail = cartDetail;
            InitializeComponent();
        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
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
            if (txtReason.Text == null || txtReason.Text.ToString() == "")
            {
                MessageBox.Show("Masukkan alasan hapus item", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (textPin.Text.ToString() == "" || textPin.Text == null)
            {
                MessageBox.Show("Masukan pin terlebih dahulu", "Gaspol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string configCart = "DT-Cache\\Transaction\\Cart.data";

                if (File.Exists(configCart))
                {
                    // Read the cart file content as a string
                    string json = File.ReadAllText(configCart);

                    // Parse the JSON string into a JObject (dynamic)
                    JObject cartData = JObject.Parse(json);

                    // Find and remove the item with the matching cart_detail_id
                    JArray cartDetails = (JArray)cartData["cart_details"];
                    var itemToRemove = cartDetails.FirstOrDefault(item => item["cart_detail_id"].ToString() == cart_detail);

                    if (itemToRemove != null)
                    {

                        itemToRemove["edited_reason"] = txtReason.Text.ToString() ?? "";

                        // Check if canceled_items exists, if not create it
                        if (cartData["canceled_items"] == null)
                        {
                            cartData["canceled_items"] = new JArray();
                        }
                        var cancelDetails = cartData["canceled_items"] as JArray;

                        int discounted_priceFix = int.Parse(itemToRemove["price"].ToString()) - int.Parse(itemToRemove["discounted_item_price"].ToString());
                        int total_priceCanceled = discounted_priceFix * int.Parse(itemToRemove["qty"].ToString());

                        cancelDetails.Add(new JObject
                        {
                            ["cart_detail_id"] = itemToRemove["cart_detail_id"],
                            ["menu_id"] = itemToRemove["menu_id"],
                            ["menu_name"] = itemToRemove["menu_name"],
                            ["menu_type"] = itemToRemove["menu_type"],
                            ["menu_detail_id"] = itemToRemove["menu_detail_id"],
                            ["menu_detail_name"] = itemToRemove["menu_detail_name"],
                            ["varian"] = itemToRemove["varian"],
                            ["is_ordered"] = 1,
                            ["serving_type_id"] = itemToRemove["serving_type_id"],
                            ["serving_type_name"] = itemToRemove["serving_type_name"],
                            ["price"] = itemToRemove["price"],
                            ["qty"] = itemToRemove["qty"],
                            ["note_item"] = itemToRemove["note_item"],
                            ["created_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            ["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            ["discount_id"] = int.Parse(itemToRemove["discount_id"]?.ToString()),
                            ["discount_code"] = itemToRemove["discount_code"]?.ToString() ?? null,
                            ["discounts_value"] = int.Parse(itemToRemove["discounts_value"]?.ToString()),
                            ["discounted_price"] = int.Parse(itemToRemove["discounted_price"]?.ToString()),
                            ["discounts_is_percent"] = int.Parse(itemToRemove["discounts_is_percent"]?.ToString()),
                            ["discounted_item_price"] = int.Parse(itemToRemove["discounted_item_price"]?.ToString()),
                            ["cancel_reason"] = txtReason.Text.ToString(),
                            ["subtotal_price"] = itemToRemove["subtotal_price"],
                            ["total_price"] = itemToRemove["total_price"]
                        });

                        itemToRemove["deleted_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        itemToRemove["qty"] = 0;
                        // Recalculate subtotal and total
                        int subtotal = 0;
                        foreach (var item in cartDetails)
                        {
                            subtotal += (int)item["total_price"];
                        }

                        // Update the cart totals
                        cartData["subtotal"] = subtotal;
                        cartData["subtotal_price"] = subtotal;
                        cartData["total"] = subtotal;
                        cartData["is_sent_sync"] = 0;

                        // Save the updated cart data back to the file
                        File.WriteAllText(configCart, cartData.ToString(Formatting.Indented));
                        DialogResult = DialogResult.OK;

                        this.Close();

                    }
                    else
                    {
                        Console.WriteLine("Item not found in the cart.");
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.CancellationToken.IsCancellationRequested)
                {
                    MessageBox.Show("PIN salah atau koneksi tidak stabil. Silakan coba beberapa saat lagi.", "Timeout/Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Koneksi tidak stabil. Coba beberapa saat lagi.", "Timeout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal hapus data {ex.ToString()}" + ex.Message);
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

