// ============================================================================
// ADD these methods into: NexusERP.Infrastructure/Services/PosService.cs
// (inside the existing `PosService` class, and add
//  `using NexusERP.Application.DTOs;` to the usings at the top if not already present)
// ============================================================================

public async Task<CurrentShiftDto?> GetCurrentShiftAsync(int userId, int storeId)
{
    var shift = await _context.Shifts
        .Include(s => s.Store)
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.UserId == userId && s.StoreId == storeId && s.Status == ShiftStatus.Open);

    if (shift == null) return null;

    var receipts = await _context.Receipts
        .Where(r => r.ShiftId == shift.ShiftId && r.IsActive)
        .AsNoTracking()
        .ToListAsync();

    var payIns = await _context.CashMovements
        .Where(m => m.ShiftId == shift.ShiftId && m.MovementType == CashMovementType.PayIn && m.IsActive)
        .SumAsync(m => (decimal?)m.Amount) ?? 0;

    var payOuts = await _context.CashMovements
        .Where(m => m.ShiftId == shift.ShiftId && m.MovementType == CashMovementType.PayOut && m.IsActive)
        .SumAsync(m => (decimal?)m.Amount) ?? 0;

    return new CurrentShiftDto
    {
        ShiftId = shift.ShiftId,
        StoreId = shift.StoreId,
        StoreName = shift.Store?.Name ?? string.Empty,
        StartDate = shift.StartDate,
        StartingCash = shift.StartingCash,
        ExpectedEndingCash = shift.ExpectedEndingCash,
        TotalSales = shift.TotalSales,
        TotalProfit = shift.TotalProfit,
        CashSales = receipts.Where(r => r.PaymentMethod == PaymentMethod.Cash).Sum(r => r.FinalTotal),
        CardSales = receipts.Where(r => r.PaymentMethod == PaymentMethod.Card).Sum(r => r.FinalTotal),
        VoucherSales = receipts.Where(r => r.PaymentMethod == PaymentMethod.Voucher).Sum(r => r.FinalTotal),
        TotalPayIns = payIns,
        TotalPayOuts = payOuts,
        ReceiptCount = receipts.Count,
        Notes = shift.Notes
    };
}

public async Task<List<ShiftHistoryItemDto>> GetShiftHistoryAsync(int storeId, int take = 20)
{
    return await _context.Shifts
        .Include(s => s.User)
        .Where(s => s.StoreId == storeId)
        .OrderByDescending(s => s.StartDate)
        .Take(take)
        .AsNoTracking()
        .Select(s => new ShiftHistoryItemDto
        {
            ShiftId = s.ShiftId,
            CashierName = s.User != null ? s.User.FullName : "Unknown",
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            TotalSales = s.TotalSales,
            StartingCash = s.StartingCash,
            ActualEndingCash = s.ActualEndingCash,
            Status = s.Status.ToString(),
            Notes = s.Notes
        })
        .ToListAsync();
}

public async Task<List<ShiftReceiptSummaryDto>> GetShiftReceiptsAsync(int shiftId, int userId)
{
    var shift = await _context.Shifts.AsNoTracking()
        .FirstOrDefaultAsync(s => s.ShiftId == shiftId && s.UserId == userId);

    if (shift == null) throw new AppException("Shift not found or does not belong to you.");

    return await _context.Receipts
        .Include(r => r.Lines)
        .Where(r => r.ShiftId == shiftId && r.IsActive)
        .OrderByDescending(r => r.CreatedAt)
        .AsNoTracking()
        .Select(r => new ShiftReceiptSummaryDto
        {
            ReceiptId = r.ReceiptId,
            ReceiptNumber = r.ReceiptNumber,
            CreatedAt = r.CreatedAt,
            FinalTotal = r.FinalTotal,
            PaymentMethod = r.PaymentMethod.ToString(),
            ItemCount = r.Lines.Count
        })
        .ToListAsync();
}

public async Task<ReceiptDetailDto> GetReceiptDetailAsync(int receiptId, int userId)
{
    var receipt = await _context.Receipts
        .Include(r => r.Lines).ThenInclude(l => l.Product)
        .Include(r => r.User)
        .Include(r => r.Store)
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.ReceiptId == receiptId && r.UserId == userId);

    if (receipt == null) throw new AppException("Receipt not found.");

    return new ReceiptDetailDto
    {
        ReceiptId = receipt.ReceiptId,
        ReceiptNumber = receipt.ReceiptNumber,
        CreatedAt = receipt.CreatedAt,
        CashierName = receipt.User?.FullName ?? "Unknown",
        StoreName = receipt.Store?.Name ?? string.Empty,
        SubTotal = receipt.SubTotal,
        CartDiscountAmount = receipt.CartDiscountAmount,
        TotalVatAmount = receipt.TotalVatAmount,
        FinalTotal = receipt.FinalTotal,
        PaymentMethod = receipt.PaymentMethod.ToString(),
        Lines = receipt.Lines.Select(l => new ReceiptLineDto
        {
            ProductName = l.Product?.Name ?? "Unknown Product",
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            MarketDiscountAmount = l.MarketDiscountAmount,
            ManualItemDiscountAmount = l.ManualItemDiscountAmount,
            LineTotal = l.LineTotal
        }).ToList()
    };
}
