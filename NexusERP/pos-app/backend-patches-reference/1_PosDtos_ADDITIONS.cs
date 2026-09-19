// ============================================================================
// ADD these classes into: NexusERP.Application/DTOs/PosDtos.cs
// (inside the existing `namespace NexusERP.Application.DTOs { ... }` block)
// ============================================================================

public class CurrentShiftDto
{
    public int ShiftId { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public decimal StartingCash { get; set; }
    public decimal ExpectedEndingCash { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal CashSales { get; set; }
    public decimal CardSales { get; set; }
    public decimal VoucherSales { get; set; }
    public decimal TotalPayIns { get; set; }
    public decimal TotalPayOuts { get; set; }
    public int ReceiptCount { get; set; }
    public string? Notes { get; set; }
}

public class ShiftHistoryItemDto
{
    public int ShiftId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal StartingCash { get; set; }
    public decimal? ActualEndingCash { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ShiftReceiptSummaryDto
{
    public int ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal FinalTotal { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class ReceiptDetailDto
{
    public int ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal CartDiscountAmount { get; set; }
    public decimal TotalVatAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public List<ReceiptLineDto> Lines { get; set; } = new();
}

public class ReceiptLineDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal MarketDiscountAmount { get; set; }
    public decimal ManualItemDiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class PinLoginDto
{
    public int UserId { get; set; }
    public string Pin { get; set; } = string.Empty;
}
