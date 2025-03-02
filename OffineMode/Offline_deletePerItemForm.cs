
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

using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using Newtonsoft.Json.Linq;
using System.Windows.Markup;
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

                        // Process the refund details
                        var refundDetails = cartData["canceled_items"] as JArray;

                        // Add the refund item to the refund details array
                        refundDetails.Add(new JObject
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
                            ["created_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            ["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            ["discount_id"] = null,
                            ["discount_code"] = null,
                            ["discounts_value"] = null,
                            ["discounted_price"] = 0,
                            ["discounts_is_percent"] = null,
                            ["cancel_reason"] = txtReason.Text.ToString(),
                            ["subtotal_price"] = itemToRemove["subtotal_price"],
                            ["total_price"] = itemToRemove["total_price"]
                        });

                        itemToRemove["deleted_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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

