﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class CartDetailsPendingStrukShift
    {
        public int cart_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public string varian { get; set; }
        public string serving_type_name { get; set; }
        public string discount_code { get; set; }
        public int discounted_price { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
    }

    public class CartDetailsCanceledStrukShift
    {
        public int cart_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public string varian { get; set; }
        public string serving_type_name { get; set; }
        public string discount_code { get; set; }
        public int discounted_price { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
    }

    public class CartDetailsSuccessStrukShift
    {
        public int cart_id { get; set; }
        public string menu_name { get; set; }
        public string menu_type { get; set; }
        public string varian { get; set; }
        public string serving_type_name { get; set; }
        public string discount_code { get; set; }
        public int discounted_price { get; set; }
        public int price { get; set; }
        public int total_price { get; set; }
        public int qty { get; set; }
    }

    public class DataStrukShift
    {
        public string outlet_name { get; set; }
        public string outlet_address { get; set; }
        public string outlet_phone_number { get; set; }
        public string casher_name { get; set; }
        public int shift_number { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public List<ExpenditureStrukShift> expenditures { get; set; }
        public int expenditures_total { get; set; }
        public int ending_cash_expected { get; set; }
        public int ending_cash_actual { get; set; }
        public int cash_difference { get; set; }
        public List<CartDetailsSuccessStrukShift> cart_details_success { get; set; }
        public int totalSuccessQty { get; set; }
        public int totalCartSuccessAmount { get; set; }
        public List<CartDetailsPendingStrukShift> cart_details_pending { get; set; }
        public int totalPendingQty { get; set; }
        public int totalCartPendingAmount { get; set; }
        public List<CartDetailsCanceledStrukShift> cart_details_canceled { get; set; }
        public int totalCanceledQty { get; set; }
        public int totalCartCanceledAmount { get; set; }
        public List<RefundDetailStrukShift> refund_details { get; set; }
        public int totalRefundQty { get; set; }
        public int totalCartRefundAmount { get; set; }
        public List<PaymentDetailStrukShift> payment_details { get; set; }
        public long total_transaction { get; set; }
        public int discount_amount_transactions { get; set; }
        public int discount_amount_per_items { get; set; }
        public int discount_total_amount { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Outlet: {outlet_name}");
            sb.AppendLine($"Shift: {shift_number}");
            sb.AppendLine($"Start: {start_date}");
            sb.AppendLine($"End: {end_date}");

            // Expenditures
            sb.AppendLine("Expenditures:");
            foreach (var expenditure in expenditures)
            {
                sb.AppendLine($"- {expenditure.description}: {expenditure.nominal}");
            }

            // Cart Details Success
            sb.AppendLine("Cart Details Success:");
            foreach (var cartSuccess in cart_details_success)
            {
                sb.AppendLine($"- {cartSuccess.menu_name} ({cartSuccess.qty}): {cartSuccess.total_price}");
            }

            // Cart Details Pending
            sb.AppendLine("Cart Details Pending:");
            foreach (var cartPending in cart_details_pending)
            {
                sb.AppendLine($"- {cartPending.menu_name} ({cartPending.qty}): {cartPending.total_price}");
            }

            // Cart Details Canceled
            sb.AppendLine("Cart Details Canceled:");
            foreach (var cartCanceled in cart_details_canceled)
            {
                sb.AppendLine($"- {cartCanceled.menu_name} ({cartCanceled.qty}): {cartCanceled.total_price}");
            }

            // Refund Details
            sb.AppendLine("Refund Details:");
            foreach (var refundDetail in refund_details)
            {
                sb.AppendLine($"- {refundDetail.menu_name} ({refundDetail.qty_refund_item}): {refundDetail.total_refund_price}");
            }

            // Payment Details
            sb.AppendLine("Payment Details:");
            foreach (var paymentDetail in payment_details)
            {
                sb.AppendLine($"- {paymentDetail.payment_category}: {paymentDetail.total_amount}");
            }

            return sb.ToString();
        }
    }

    public class ExpenditureStrukShift
    {
        public string description { get; set; }
        public int nominal { get; set; }
    }

    public class PaymentDetailStrukShift
    {
        public string payment_category { get; set; }
        public List<PaymentTypeDetailStrukShift> payment_type_detail { get; set; }
        public int total_amount { get; set; }
    }

    public class PaymentTypeDetailStrukShift
    {
        public string payment_type { get; set; }
        public int total_payment { get; set; }
    }

    public class RefundDetailStrukShift
    {
        public int qty_refund_item { get; set; }
        public int payment_type_id { get; set; }
        public int total_refund_price { get; set; }
        public string menu_name { get; set; }
        public string varian { get; set; }
        public string serving_type_name { get; set; }
        public string discount_code { get; set; }
        public int discounted_price { get; set; }
        public int price { get; set; }
        public string? menu_type { get; set; }
    }

    public class GetStrukShift
    {
        public int code { get; set; }
        public string message { get; set; }
        public DataStrukShift data { get; set; }
    }

    public class GetShift
    {
        public int code { get; set; }
        public string message { get; set; }
        public DataShift data { get; set; }
    }

    public class DataShift
    {
        public string outlet_name { get; set; }
        public string outlet_address { get; set; }
        public string outlet_phone_number { get; set; }
        public string casher_name { get; set; }
        public int shift_number { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public List<ExpenditureStrukShift> expenditures { get; set; }
        public int expenditures_total { get; set; }
        public int ending_cash_expected { get; set; }
        public int ending_cash_actual { get; set; }
        public int cash_difference { get; set; }
        public List<CartDetailsSuccessStrukShift> cart_details_success { get; set; }
        public int totalSuccessQty { get; set; }
        public int totalCartSuccessAmount { get; set; }
        public List<CartDetailsPendingStrukShift> cart_details_pending { get; set; }
        public int totalPendingQty { get; set; }
        public int totalCartPendingAmount { get; set; }
        public List<CartDetailsCanceledStrukShift> cart_details_canceled { get; set; }
        public int totalCanceledQty { get; set; }
        public int totalCartCanceledAmount { get; set; }
        public List<RefundDetailStrukShift> refund_details { get; set; }
        public int totalRefundQty { get; set; }
        public int totalCartRefundAmount { get; set; }
        public List<PaymentDetailStrukShift> payment_details { get; set; }
        public long total_transaction { get; set; }
        public int discount_amount_transactions { get; set; }
        public int discount_amount_per_items { get; set; }
        public int discount_total_amount { get; set; }
    }


}