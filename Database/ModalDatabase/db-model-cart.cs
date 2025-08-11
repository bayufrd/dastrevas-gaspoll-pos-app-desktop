using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Database.ModalDatabase
{
    public class dbCartDetails
    {
        public int Id { get; set; } // Primary key (ID)
        public int CartDetailId { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuType { get; set; }
        public int? MenuDetailId { get; set; } = null;
        public string? MenuDetailName { get; set; }
        public int ServingTypeId { get; set; }
        public string ServingTypeName { get; set; }
        public int Price { get; set; }
        public int Qty { get; set; }
        public string? NoteItem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
        public int? DiscountId { get; set; }
        public string? DiscountCode { get; set; }
        public int? DiscountsValue { get; set; }
        public int? DiscountedPrice { get; set; }
        public int? DiscountedPerItemPrice { get; set; }
        public int? DiscountsIsPercent { get; set; }
        public int SubtotalPrice { get; set; }
        public int TotalPrice { get; set; }
    }
    public class dbCartModal
    {
        public int Id { get; set; } // Primary key (ID)
        public string TransactionRef { get; set; }
        public int Subtotal { get; set; } = 0;
        public int Total { get; set; } = 0;
        public int DiscountId { get; set; } = 0;
        public string? DiscountCode { get; set; } = null;
        public int? DiscountsValue { get; set; } = null;
        public int? DiscountsIsPercent { get; set; } = 0;
        public int? DiscountedPrice { get; set; } = 0;
        public DateTime? DeletedAt { get; set; } = null;
        public List<dbCartDetails> CartDetails { get; set; } 
    }
    public class dbRefundDetails
    {
        public int Id { get; set; } 
        public int CartDetailId { get; set; } 
        public decimal RefundAmount { get; set; } 
        public DateTime RefundDate { get; set; } 
        public string RefundReason { get; set; } 
    }
}
