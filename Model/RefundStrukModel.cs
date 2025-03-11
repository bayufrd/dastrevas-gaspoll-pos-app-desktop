using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class RefundStrukModel
    {
        public DataRefundStruk Data { get; set; }
        public string Message { get; set; }
    }

    public class RefundDetailStruk
    {
        public int cart_detail_id { get; set; }
        public string refund_reason_item { get; set; }
        public string payment_type_name { get; set; }
        public int qty_refund_item { get; set; }
        public int total_refund_price { get; set; }
        public string menu_name { get; set; }
        public string varian { get; set; }
        public string serving_type_name { get; set; }
        public string? discount_code { get; set; }
        public int? discounts_value { get; set; }
        public int? discounted_price { get; set; }
        public int? discounted_item_price { get; set; }
        public string discounts_is_percent { get; set; }
        public int menu_price { get; set; }
        public string note_item { get; set; }
    }

    public class DataRefundStruk
    {
        public int transaction_id { get; set; }
        public string receipt_number { get; set; }
        public string customer_name { get; set; }
        public int customer_seat { get; set; }
        public string payment_type { get; set; }
        public string delivery_type { get; set; }
        public string delivery_note { get; set; }
        public int cart_id { get; set; }
        public int subtotal { get; set; }
        public int total { get; set; }
        public int? discount_id { get; set; }
        public string? discount_code { get; set; }
        public int? discounts_value { get; set; }
        public string? discounts_is_percent { get; set; }
        public List<CartDetailRefundStruk> cart_details { get; set; }
        public int customer_cash { get; set; }
        public int customer_change { get; set; }
        public string invoice_due_date { get; set; }
        public int? is_refund_all { get; set; }
        public string? refund_reason { get; set; }
        public int total_refund { get; set; }
        public List<RefundDetailStruk> refund_details { get; set; }
    }

    public class CartDetailRefundStruk
    {
        public int cart_detail_id { get; set; }
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public string? menu_detail_id { get; set; }
        public string? varian { get; set; }
        public int serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public string? discount_id { get; set; }
        public string? discount_code { get; set; }
        public string? discounts_value { get; set; }
        public int? discounted_price { get; set; }
        public string? discounts_is_percent { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
        public string? note_item { get; set; }
    }
}