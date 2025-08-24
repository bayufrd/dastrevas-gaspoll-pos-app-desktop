using System.Globalization;
using KASIR.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KASIR.Helper;
using System.Runtime.InteropServices;

namespace KASIR.OfflineMode
{
    public partial class Offline_deletePerItemForm : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        private readonly string cart_detail;

        private List<CartDetailTransaction> item = new();
        private List<RefundModel> refundItems = new();

        public Offline_deletePerItemForm(string cartDetail)
        {
            cart_detail = cartDetail;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

        }


        private void btnKeluar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // KeluarButtonPrintReportShiftClicked = true;
            DialogResult = DialogResult.OK;

            Close();
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            if (txtReason.Text == null || txtReason.Text == "")
            {
                NotifyHelper.Warning("Masukkan alasan hapus item");
                return;
            }

            if (textPin.Text == "" || textPin.Text == null)
            {
                NotifyHelper.Warning("Masukan pin terlebih dahulu");
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
                    JToken? itemToRemove =
                        cartDetails.FirstOrDefault(item => item["cart_detail_id"].ToString() == cart_detail);

                    if (itemToRemove != null)
                    {
                        itemToRemove["edited_reason"] = txtReason.Text ?? "";

                        // Check if canceled_items exists, if not create it
                        if (cartData["canceled_items"] == null)
                        {
                            cartData["canceled_items"] = new JArray();
                        }

                        JArray? cancelDetails = cartData["canceled_items"] as JArray;

                        int discounted_priceFix = int.Parse(itemToRemove["price"].ToString()) -
                                                  int.Parse(itemToRemove["discounted_item_price"].ToString());
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
                            ["created_at"] =
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            ["updated_at"] =
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            ["discount_id"] = int.Parse(itemToRemove["discount_id"]?.ToString()),
                            ["discount_code"] = itemToRemove["discount_code"]?.ToString() ?? null,
                            ["discounts_value"] = int.Parse(itemToRemove["discounts_value"]?.ToString()),
                            ["discounted_price"] = int.Parse(itemToRemove["discounted_price"]?.ToString()),
                            ["discounts_is_percent"] = int.Parse(itemToRemove["discounts_is_percent"]?.ToString()),
                            ["discounted_item_price"] =
                                int.Parse(itemToRemove["discounted_item_price"]?.ToString()),
                            ["cancel_reason"] = txtReason.Text,
                            ["subtotal_price"] = itemToRemove["subtotal_price"],
                            ["total_price"] = itemToRemove["total_price"]
                        });

                        itemToRemove["deleted_at"] =
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        itemToRemove["qty"] = 0;
                        // Recalculate subtotal and total
                        int subtotal = 0;
                        foreach (JToken item in cartDetails)
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

                        Close();
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
                    NotifyHelper.Error("PIN salah atau koneksi tidak stabil. Silakan coba beberapa saat lagi."+ex.Message);
                }
                else
                {
                    NotifyHelper.Error("Koneksi tidak stabil. Coba beberapa saat lagi."+ex.Message);
                }

                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                NotifyHelper.Error($"Gagal hapus data {ex}" + ex.Message);
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
    }
}