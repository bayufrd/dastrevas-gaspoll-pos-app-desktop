using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class CartDetailStrukCustomerTransaction
    {
        public int cart_detail_id { get; set; }
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public int? menu_detail_id { get; set; }
        public string? varian { get; set; }
        public int serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public int? discount_id { get; set; }
        public string? discount_code { get; set; }
        public int? discounts_value { get; set; }
        public int? discounted_price { get; set; }
        public int? discounts_is_percent { get; set; }
        public int price { get; set; }
        public int subtotal { get; set; }
        public int subtotal_price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
        public string? note_item { get; set; }
        public int? is_ordered { get; set; }
    }

    public class KitchenAndBarCartDetails
    {
        public int cart_detail_id { get; set; }
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public object menu_detail_id { get; set; }
        public string? varian { get; set; }
        public int serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public int? discount_id { get; set; }
        public string? discount_code { get; set; }
        public object discounts_value { get; set; }
        public int? discounted_price { get; set; }
        public object discounts_is_percent { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
        public object note_item { get; set; }
        public int? is_ordered { get; set; }
    }

    public class CanceledItemStrukCustomerTransaction
    {
        public int cart_detail_id { get; set; }
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public object menu_detail_id { get; set; }
        public string? varian { get; set; }
        public int serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public int? subtotal { get; set; }
        public int? subtotal_price { get; set; }
        public int? discount_id { get; set; }
        public int? is_ordered { get; set; }
        public string? discount_code { get; set; }
        public object discounts_value { get; set; }
        public int? discounted_price { get; set; }
        public object discounts_is_percent { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
        public object note_item { get; set; }
        public string? cancel_reason { get; set; }
        public int? is_printed { get; set; }
    }

    public class KitchenAndBarCanceledItems
    {
        public int cart_detail_id { get; set; }
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public object menu_detail_id { get; set; }
        public string? varian { get; set; }
        public int serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public int? discount_id { get; set; }
        public string? discount_code { get; set; }
        public object discounts_value { get; set; }
        public int? discounted_price { get; set; }
        public object discounts_is_percent { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
        public object note_item { get; set; }
        public string? cancel_reason { get; set; }
        public int? is_ordered { get; set; }
        public int? is_printed { get; set; }
    }


    public class DataStrukCustomerTransaction
    {
        public string outlet_name { get; set; }
        public string outlet_address { get; set; }
        public string outlet_phone_number { get; set; }
        public string outlet_footer { get; set; }
        public int transaction_id { get; set; }
        public string receipt_number { get; set; }
        public string customer_name { get; set; }
        public int customer_seat { get; set; }
        public string? payment_type { get; set; }
        public string delivery_type { get; set; }
        public string delivery_note { get; set; }
        public int cart_id { get; set; }
        public int subtotal { get; set; }
        public int total { get; set; }
        public int? discount_id { get; set; }
        public string? discount_code { get; set; }
        public int? discounts_value { get; set; }
        public string? discounts_is_percent { get; set; }
        public List<CartDetailStrukCustomerTransaction> cart_details { get; set; }
        public List<CanceledItemStrukCustomerTransaction> canceled_items { get; set; }
        public List<KitchenAndBarCartDetails> kitchenBarCartDetails { get; set; }
        public List<KitchenAndBarCanceledItems> kitchenBarCanceledItems { get; set; }
        public int? customer_cash { get; set; }
        public int? customer_change { get; set; }
        public string? invoice_due_date { get; set; }
        //membership
        public string? member_name { get; set; }
        public int? member_id { get; set; }
        public string? member_phone_number { get; set; }
        public string? member_email { get; set; }
        public int? member_point { get; set; }
        public int? member_use_point { get; set; }
    }

    public class GetStrukCustomerTransaction
    {
        public int code { get; set; }
        public string message { get; set; }
        public DataStrukCustomerTransaction data { get; set; }
    }


    public class CartDataCache
    {
        public string transaction_id { get; set; }
        public string receipt_number { get; set; }
        public string invoice_number { get; set; }
        public object payment_type_id { get; set; }
        public string payment_type_name { get; set; }
        public string customer_name { get; set; }
        public int customer_seat { get; set; }
        public int customer_cash { get; set; }
        public int total { get; set; }
        public int subtotal { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int is_refund { get; set; }
        public string refund_reason { get; set; }
        public int? customer_change { get; set; }
        public string deleted_at { get; set; }
        public string delivery_type { get; set; }
        public string delivery_note { get; set; }
        public int? discount_id { get; set; }
        public string discount_code { get; set; }
        public string discounts_value { get; set; }
        public string discounts_is_percent { get; set; }
        public string invoice_due_date { get; set; }
        public string member_name { get; set; }
        public string member_phone_number { get; set; }
        // Cart details for each item in the transaction
        public List<CartDetail> cart_details { get; set; }

        // Refund details
        public List<RefundDetail> refund_details { get; set; }

        // Canceled items
        public List<CanceledItem> canceled_items { get; set; }
    }

    public class RefundDetail
    {
        public int cart_detail_id { get; set; }
        public string refund_reason_item { get; set; }
        public int qty_refund_item { get; set; }
        public int total_refund_price { get; set; }
        public string payment_type_name { get; set; }
        public string payment_category_name { get; set; }
        public string menu_name { get; set; }
        public string? varian { get; set; }
        public string serving_type_name { get; set; }
        public string? discount_code { get; set; }
        public string? discounts_value { get; set; }
        public string? discounted_price { get; set; }
        public int menu_price { get; set; }
        public string? note_item { get; set; }
        public int? is_printed { get; set; }
    }

    public class CanceledItem
    {
        public string cancel_reason { get; set; }
        public int cart_detail_id { get; set; }
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public object menu_detail_id { get; set; }
        public string varian { get; set; }
        public int serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public int? discount_id { get; set; }
        public string? discount_code { get; set; }
        public object discounts_value { get; set; }
        public int? discounted_price { get; set; }
        public string discounts_is_percent { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
        public string note_item { get; set; }
        public int? is_ordered { get; set; }
        public int? is_printed { get; set; }
    }

    public class CartDetail
    {
        public string cart_detail_id { get; set; }
        public int menu_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public int menu_detail_id { get; set; }
        public string menu_detail_name { get; set; }
        public int is_ordered { get; set; }
        public int serving_type_id { get; set; }
        public string serving_type_name { get; set; }
        public int price { get; set; }
        public int qty { get; set; }
        public string note_item { get; set; }
        public string created_at { get; set; }
        public string update_at { get; set; }
        public object discount_id { get; set; }
        public object discount_code { get; set; }
        public object discounts_value { get; set; }
        public int discounted_price { get; set; }
        public object discounts_is_percent { get; set; }
        public int total_price { get; set; }
        public int subtotal { get; set; }
        public int subtotal_price { get; set; }
        public string varian { get; set; }
        public int? is_printed { get; set; }
    }
    public class TransactionCache
    {
        public List<CartDataCache> data { get; set; }
    }
    public class Data
    {
        public int id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public int pin { get; set; }
        public string phone_number { get; set; }
        public int is_kitchen_bar_merged { get; set; }
        public string footer { get; set; }
    }

    public class CartDataOutlet
    {
        public Data data { get; set; }
    }


}
