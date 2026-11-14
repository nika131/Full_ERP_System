using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusERP.Api.Extensions;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using NexusERP.Infrastructure.Database;
using NexusERP.Domain.Helpers;

namespace NexusERP.Api.Controllers;

public sealed class SalesLedgerQuery
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtcExclusive { get; set; }
    public TimeSpan? StartHour { get; set; }
    public TimeSpan? EndHour { get; set; }
    public int UtcOffsetMinutes { get; set; }
    public int[]? StoreIds { get; set; }
    public int[]? EmployeeIds { get; set; }
    public int[]? CategoryIds { get; set; }
    public int[]? SupplierIds { get; set; }
    public int[]? ProductIds { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Sales reporting from Receipts and ReceiptItems, never InventoryTransactions.
/// Existing stock/transaction endpoints and POS checkout are unchanged.
/// </summary>
[ApiController]
[Route("api/sales-ledger")]
[Authorize(Policy = "RequireReadTransactions")]
public sealed class SalesLedgerController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public SalesLedgerController(ApplicationDbContext db) => _db = db;

    private IQueryable<Receipt> VisibleReceipts()
    {
        int companyId = User.GetCurrentCompanyId();
        int userId = User.GetCurrentUserId();
        bool canViewAll = User.HasPermission(Permissions.ViewAllTransactions);

        // Ignore filters for renamed/deactivated employees/stores in historical reports,
        // then reapply strict company and active-receipt restrictions explicitly.
        var receipts = _db.Receipts.IgnoreQueryFilters().AsNoTracking()
            .Where(r => r.CompanyId == companyId && r.IsActive);
        return canViewAll ? receipts : receipts.Where(r => r.UserId == userId);
    }

    private static IQueryable<Receipt> ApplyReceiptFilters(
        IQueryable<Receipt> receipts, SalesLedgerQuery filter)
    {
        if (filter.FromUtc.HasValue)
            receipts = receipts.Where(r => r.CreatedAt >= filter.FromUtc.Value);
        if (filter.ToUtcExclusive.HasValue)
            receipts = receipts.Where(r => r.CreatedAt < filter.ToUtcExclusive.Value);
        if (filter.StoreIds is { Length: > 0 })
            receipts = receipts.Where(r => filter.StoreIds.Contains(r.StoreId));
        if (filter.EmployeeIds is { Length: > 0 })
            receipts = receipts.Where(r => filter.EmployeeIds.Contains(r.UserId));

        // The UI sends JS Date.getTimezoneOffset(): minutes to add to local time for UTC.
        // Filter hours in the user's selected local clock, not the server's time zone.
        int offset = Math.Clamp(filter.UtcOffsetMinutes, -840, 840);
        if (filter.StartHour.HasValue)
            receipts = receipts.Where(r =>
                r.CreatedAt.AddMinutes(-offset).TimeOfDay >= filter.StartHour.Value);
        if (filter.EndHour.HasValue)
            receipts = receipts.Where(r =>
                r.CreatedAt.AddMinutes(-offset).TimeOfDay <= filter.EndHour.Value);

        return receipts;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SalesLedgerQuery filter)
    {
        int companyId = User.GetCurrentCompanyId();
        int page = Math.Clamp(filter.Page, 1, 100000);
        int pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var receipts = ApplyReceiptFilters(VisibleReceipts(), filter);
        var search = filter.Search?.Trim();
        if (search?.Length > 100) return BadRequest(new { message = "Search is too long." });

        bool productMode =
            filter.ProductIds is { Length: > 0 } ||
            filter.CategoryIds is { Length: > 0 } ||
            filter.SupplierIds is { Length: > 0 };

        var userCutoff = DateTime.UtcNow.AddDays(-30);

        if (!productMode)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                int.TryParse(search, out int receiptId);
                receipts = receipts.Where(r =>
                    r.ReceiptNumber.Contains(search) ||
                    (receiptId > 0 && r.ReceiptId == receiptId));
            }

            var raw = await receipts
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.ReceiptId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .Select(r => new
                {
                    r.ReceiptId, r.ReceiptNumber, r.CreatedAt,
                    StoreName = r.Store != null ? r.Store.Name : "Unknown store",
                    CashierName = r.User != null &&
                        (r.User.IsActive ||
                         (r.User.DeletedAt.HasValue &&
                          r.User.DeletedAt > userCutoff))
                        ? r.User.FullName
                        : "Deleted user",
                    CashierDeletedRecently =
                        r.User != null &&
                        !r.User.IsActive &&
                        r.User.DeletedAt.HasValue &&
                        r.User.DeletedAt > userCutoff,
                    r.PaymentMethod, r.FinalTotal, r.TotalCostAmount, r.TotalVatAmount
                })
                .ToListAsync();

            var items = raw.Take(pageSize).Select(r => new
            {
                r.ReceiptId, 
                r.ReceiptNumber,
                CreatedAt = AsUtc(r.CreatedAt),
                r.StoreName, 
                r.CashierName,
                r.CashierDeletedRecently,
                PaymentMethod = r.PaymentMethod.ToString(),
                Total = r.FinalTotal,
                Profit = r.FinalTotal - r.TotalCostAmount - r.TotalVatAmount
            }).ToList();
            return Ok(new { mode = "receipts", items, page, hasMorePages = raw.Count > pageSize });
        }

        // Filter actual receipt lines, not whole receipts. One line = one sale item.
        // Ignore Product.IsActive filter so a deactivated product still appears in history.
        var lines = _db.ReceiptItems.IgnoreQueryFilters().AsNoTracking()
            .Where(l => l.CompanyId == companyId)
            .Join(receipts, l => l.ReceiptId, r => r.ReceiptId,
                (line, receipt) => new { line, receipt });

        if (filter.ProductIds is { Length: > 0 })
            lines = lines.Where(x => x.line.ProductId.HasValue && filter.ProductIds.Contains(x.line.ProductId.Value));
        if (filter.CategoryIds is { Length: > 0 })
            lines = lines.Where(x => x.line.Product != null &&
                x.line.Product.CategoryId.HasValue &&
                filter.CategoryIds.Contains(x.line.Product.CategoryId.Value));
        if (filter.SupplierIds is { Length: > 0 })
            lines = lines.Where(x => x.line.Product != null &&
                x.line.Product.SupplierId.HasValue &&
                filter.SupplierIds.Contains(x.line.Product.SupplierId.Value));

        if (!string.IsNullOrWhiteSpace(search))
        {
            int.TryParse(search, out int receiptId);
            lines = lines.Where(x =>
                (x.line.Product != null && x.line.Product.Name.Contains(search)) ||
                x.receipt.ReceiptNumber.Contains(search) ||
                (receiptId > 0 && x.receipt.ReceiptId == receiptId));
        }

        var deletionCutoff = DateTime.UtcNow.AddDays(-30);
        var rawLines = await lines
            .OrderByDescending(x => x.receipt.CreatedAt)
            .ThenByDescending(x => x.receipt.ReceiptId)
            .ThenBy(x => x.line.ItemId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .Select(x => new
            {
                x.line.ItemId, x.line.ReceiptId, x.receipt.ReceiptNumber,
                x.receipt.CreatedAt, x.line.ProductId,
                ProductName = x.line.Product != null &&
                    (x.line.Product.IsActive || (x.line.Product.DeletedAt.HasValue && x.line.Product.DeletedAt > deletionCutoff))
                    ? x.line.Product.Name : "Deleted product",
                ProductDeletedRecently = x.line.Product != null && !x.line.Product.IsActive &&
                    x.line.Product.DeletedAt.HasValue && x.line.Product.DeletedAt > deletionCutoff,
                x.line.Quantity, x.line.UnitPrice
            })
            .ToListAsync();

        var visibleLines = rawLines.Take(pageSize).ToList();
        var receiptIds = visibleLines.Select(x => x.ReceiptId).Distinct().ToArray();
        var receiptDetails = await _db.Receipts.IgnoreQueryFilters().AsNoTracking()
            .Include(r => r.Lines)
            .Where(r => r.CompanyId == companyId && r.IsActive && receiptIds.Contains(r.ReceiptId))
            .ToListAsync();

        // Allocate the final receipt amount and VAT across ALL lines before choosing
        // displayed filtered lines. This preserves receipt totals after cart discounts.
        var allocations = receiptDetails.ToDictionary(r => r.ReceiptId, AllocateLines);
        var productItems = visibleLines.Select(x =>
        {
            var amount = allocations[x.ReceiptId][x.ItemId];
            return new
            {
                x.ItemId, x.ReceiptId, x.ReceiptNumber,
                CreatedAt = AsUtc(x.CreatedAt),
                x.ProductId, x.ProductName, x.ProductDeletedRecently, x.Quantity, x.UnitPrice,
                Total = amount.Total, Profit = amount.Profit
            };
        }).ToList();

        return Ok(new { mode = "products", items = productItems, page, hasMorePages = rawLines.Count > pageSize });
    }

    [HttpGet("products")]
    public async Task<IActionResult> SearchSoldProducts([FromQuery] string? search)
    {
        if (search?.Length > 100) return BadRequest(new { message = "Search is too long." });
        int companyId = User.GetCurrentCompanyId();
        var receipts = VisibleReceipts();
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var rows = _db.ReceiptItems.IgnoreQueryFilters().AsNoTracking()
            .Where(l => l.CompanyId == companyId && l.ProductId.HasValue && l.Product != null &&
                (l.Product.IsActive || (l.Product.DeletedAt.HasValue && l.Product.DeletedAt > cutoff)))
            .Join(receipts, l => l.ReceiptId, r => r.ReceiptId,
                (l, r) => new { ProductId = l.ProductId!.Value, Name = l.Product!.Name });
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            rows = rows.Where(x => x.Name.Contains(term));
        }
        return Ok(await rows.Distinct().OrderBy(x => x.Name).Take(30).ToListAsync());
    }

    [HttpGet("receipts/{id:int}")]
    public async Task<IActionResult> GetReceipt(int id)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var r = await VisibleReceipts()
            .Where(r => r.ReceiptId == id)
            .Include(r => r.Lines).ThenInclude(l => l.Product)
            .Include(r => r.User)
            .Include(r => r.Store)
            .AsSplitQuery()
            .FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "Receipt not found." });
        return Ok(new
        {
            r.ReceiptId, r.ReceiptNumber, CreatedAt = AsUtc(r.CreatedAt),
            StoreName = r.Store?.Name ?? "Unknown store",
            CashierName = DeletedUserDisplay.GetName(r.User, cutoff),
            CashierDeletedRecently = DeletedUserDisplay.IsRecentlyDeleted(r.User, cutoff),
            PaymentMethod = r.PaymentMethod.ToString(),
            r.SubTotal, r.CartDiscountPercentage, r.TotalVatAmount,
            r.FinalTotal,
            Profit = r.FinalTotal - r.TotalCostAmount - r.TotalVatAmount,
            Lines = r.Lines.OrderBy(l => l.ItemId).Select(l => new
            {
                l.ItemId, l.ProductId,
                ProductName = l.Product != null &&
                    (l.Product.IsActive || (l.Product.DeletedAt.HasValue && l.Product.DeletedAt > cutoff))
                    ? l.Product.Name : "Deleted product",
                ProductDeletedRecently = l.Product != null && !l.Product.IsActive &&
                    l.Product.DeletedAt.HasValue && l.Product.DeletedAt > cutoff,
                l.Quantity, l.UnitPrice, l.MarketDiscountPercentage,
                l.ManualItemDiscountPercentage, l.LineTotal
            })
        });
    }

    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private sealed record LineAllocation(decimal Total, decimal Profit);

    private static Dictionary<int, LineAllocation> AllocateLines(Receipt receipt)
    {
        var lines = receipt.Lines.OrderBy(x => x.ItemId).ToArray();
        var result = new Dictionary<int, LineAllocation>();
        if (lines.Length == 0) return result;

        decimal subtotalAfterItemDiscounts = lines.Sum(x => x.LineTotal);
        decimal remaining = receipt.FinalTotal;
        var totals = new decimal[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            totals[i] = i == lines.Length - 1
                ? remaining
                : subtotalAfterItemDiscounts > 0
                    ? Math.Round(receipt.FinalTotal * lines[i].LineTotal / subtotalAfterItemDiscounts,
                        2, MidpointRounding.AwayFromZero)
                    : 0m;
            remaining -= totals[i];
        }

        // Allocate stored receipt VAT proportionally to each line's VAT contribution.
        // All rounding residuals go to the final line so their sums equal the receipt.
        var vatWeights = lines.Select((l, i) =>
            l.VatRate > 0 ? totals[i] * l.VatRate / (100m + l.VatRate) : 0m).ToArray();
        decimal vatWeightSum = vatWeights.Sum();
        decimal remainingVat = receipt.TotalVatAmount;
        int lastTaxableLine = Array.FindLastIndex(vatWeights, weight => weight > 0);
        if (lastTaxableLine < 0) lastTaxableLine = lines.Length - 1;
        for (int i = 0; i < lines.Length; i++)
        {
            decimal vat = i == lastTaxableLine ? remainingVat
                : i > lastTaxableLine || vatWeightSum <= 0 ? 0m
                : Math.Round(receipt.TotalVatAmount * vatWeights[i] / vatWeightSum,
                    2, MidpointRounding.AwayFromZero);
            remainingVat -= vat;
            decimal cost = lines[i].CostPrice * lines[i].Quantity;
            result[lines[i].ItemId] = new LineAllocation(totals[i], totals[i] - cost - vat);
        }
        return result;
    }
}
