using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.DTOs
{
    public class OpenShiftDto
    {
        public int StoreId { get; set; }
        public decimal StartingCash { get; set; }
        public string? Note { get; set; } = string.Empty;
    }

    public class CloseShiftDto
    {
        public decimal ActualEndingCash { get; set; }
        public string? Note { get; set; } = string.Empty;
    }

    public class CashMovementDto
    {
        public int StoreId { get; set; }
        public CashMovementType MovementType { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; } = string.Empty;
    }

    public class CheckoutRequestDto
    {
        public int StoreId { get; set; }
        public decimal CartDiscountAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public List<CheckoutItemDto> Items { get; set; } = new();
    }

    public class CheckoutItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ManualItemDiscount { get; set; }
    }

    public class ShiftAuditDto
    {
        public int ShiftId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal StartingCash { get; set; }
        public decimal ExpectedEndingCash { get; set; }
        public decimal? ActualEndingCash { get; set; }
        public decimal Variance => (ActualEndingCash ?? ExpectedEndingCash) - ExpectedEndingCash;
        public decimal TotalSales { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ReceiptAuditDto
    {
        public int ReceiptId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal CartDiscountAmount { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal FinalTotal { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
