using Microsoft.EntityFrameworkCore;
using NexusERP.Infrastructure.Services;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CursorPagedResult<InventoryTransaction>> GetPagedTransactionsOptimized(
            int companyId, 
            int pageSize, 
            int currentUserId, 
            bool canViewAll, 
            string typeFilter, 
            string? searchTerm, 
            DateTime? lastCreatedAt, 
            int? lastTransactionId, 
            int? productId, 
            int? supplierId, 
            int? storeId = null, 
            int? categoryId = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null,
            TimeSpan? startHour = null,
            TimeSpan? endHour = null,
            int[]? storeIds = null,
            int[]? categoryIds = null,
            int[]? supplierIds = null,
            int[]? employeeIds = null,
            string timeZoneId = "UTC"
        )
        {
            var baseQuery = _context.InventoryTransactions
                .IgnoreQueryFilters()
                .Include(t => t.Product)
                .ThenInclude(p => p!.Supplier)
                .Include(t => t.User)
                .Where(t => t.CompanyId == companyId && t.TransactionType != TransactionAction.Sale)
                .AsNoTracking();

            if (!canViewAll)
                baseQuery = baseQuery.Where(t => t.UserId == currentUserId);

            if (typeFilter != "All" && Enum.TryParse<TransactionAction>(typeFilter, true, out var action))
                baseQuery = baseQuery.Where(t => t.TransactionType == action);

            if (productId.HasValue)
                baseQuery = baseQuery.Where(t => t.ProductId == productId.Value);

            if (supplierId.HasValue)
                baseQuery = baseQuery.Where(t => t.Product != null && t.Product.SupplierId == supplierId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (int.TryParse(searchTerm, out int numericSearchId))
                    baseQuery = baseQuery.Where(t => t.TransactionId == numericSearchId);
                else
                    baseQuery = baseQuery.Where(t => t.Product != null && t.Product.Name.Contains(searchTerm));
            }

            if (storeId.HasValue)
                baseQuery = baseQuery.Where(t => t.StoreId == storeId.Value);

            if (categoryId.HasValue)
                baseQuery = baseQuery.Where(t => t.Product != null && t.Product.CategoryId == categoryId.Value);

            var reportZone = ReportingTime.Resolve(timeZoneId);
            var reportStart = ReportingTime.StartUtc(startDate, reportZone);
            var reportEnd = ReportingTime.EndUtcExclusive(endDate, reportZone);
            var reportSqlZone = ReportingTime.SqlZone(reportZone);
            if (reportStart.HasValue)
                baseQuery = baseQuery.Where(t => t.CreatedAt >= reportStart.Value);
            if (reportEnd.HasValue)
                baseQuery = baseQuery.Where(t => t.CreatedAt < reportEnd.Value);
            if (startHour.HasValue)
                baseQuery = baseQuery.Where(t =>
                    EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(t.CreatedAt, "UTC"), reportSqlZone).TimeOfDay >= startHour.Value);
            if (endHour.HasValue)
                baseQuery = baseQuery.Where(t =>
                    EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(t.CreatedAt, "UTC"), reportSqlZone).TimeOfDay <= endHour.Value);

            if (storeIds is { Length: > 0 })
            {
                baseQuery = baseQuery.Where(t =>
                    t.StoreId.HasValue &&
                    storeIds.Contains(t.StoreId.Value));
            }

            if (categoryIds is { Length: > 0 })
            {
                baseQuery = baseQuery.Where(t =>
                    t.Product != null &&
                    t.Product.CategoryId.HasValue &&
                    categoryIds.Contains(t.Product.CategoryId.Value));
            }

            if (supplierIds is { Length: > 0 })
            {
                baseQuery = baseQuery.Where(t =>
                    t.Product != null &&
                    t.Product.SupplierId.HasValue &&
                    supplierIds.Contains(t.Product.SupplierId.Value));
            }

            if (employeeIds is { Length: > 0 })
            {
                baseQuery = baseQuery.Where(t =>
                    employeeIds.Contains(t.UserId));
            }


            if (lastCreatedAt.HasValue && lastTransactionId.HasValue)
            {
                baseQuery = baseQuery.Where(t =>
                    t.CreatedAt < lastCreatedAt.Value ||
                    (t.CreatedAt == lastCreatedAt.Value && t.TransactionId < lastTransactionId.Value)
                );
            }

            int fetchCount = pageSize + 1;

            var items = await baseQuery
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TransactionId)
                .Take(fetchCount)
                .ToListAsync();

            bool hasMorePages = items.Count == fetchCount;

            DateTime? nextCreatedAtCursor = null;
            int? nextTransactionIdCursor = null;

            if (hasMorePages)
            {
                var lastValidItem = items[pageSize - 1];

                nextCreatedAtCursor = lastValidItem.CreatedAt;
                nextTransactionIdCursor = lastValidItem.TransactionId;

                items.RemoveAt(items.Count - 1);
            }

            return new CursorPagedResult<InventoryTransaction>
            {
                Items = items,
                NextCreatedAt = nextCreatedAtCursor,
                NextId = nextTransactionIdCursor,
                PageSize = pageSize
            };
        }

        public async Task<InventoryTransaction?> GetById(int companyId, int transactionId)
        {
            return await _context.InventoryTransactions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.CompanyId == companyId &&
                    t.TransactionId == transactionId
                );
        }

        public async Task<List<RevenueChartData>> GetWeeklyRevenueChart(int companyId, DashboardFilterRequest dto)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);

            var zone = ReportingTime.Resolve(dto.TimeZoneId);
            var today = ReportingTime.LocalTime(DateTime.UtcNow, zone).Date;
            var actualStart = dto.StartDate?.Date ?? today.AddDays(-6);
            var actualEnd = dto.EndDate?.Date ?? today;
            var startUtc = ReportingTime.ToUtcMidnight(actualStart, zone);
            var endUtc = ReportingTime.ToUtcMidnight(actualEnd.AddDays(1), zone);
            var sqlZone = ReportingTime.SqlZone(zone);

            var query = _context.ReceiptItems
                .IgnoreQueryFilters()
                .Include(ri => ri.Receipt)
                .Include(ri => ri.Product)
                .Where(ri =>
                    ri.CompanyId == companyId &&
                    ri.Receipt != null &&
                    ri.Receipt.CompanyId == companyId &&
                    ri.Receipt.IsActive &&
                    ri.Receipt.CreatedAt >= startUtc &&
                    ri.Receipt.CreatedAt < endUtc
                )
                .AsQueryable();

            if (dto.StartHour.HasValue)
                query = query.Where(ri =>
                    EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(ri.Receipt!.CreatedAt, "UTC"), sqlZone).TimeOfDay >= dto.StartHour.Value);

            if (dto.EndHour.HasValue)
                query = query.Where(ri =>
                    EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(ri.Receipt!.CreatedAt, "UTC"), sqlZone).TimeOfDay <= dto.EndHour.Value);

            if (dto.StoreIds != null && dto.StoreIds.Any())
                query = query.Where(ri => dto.StoreIds.Contains(ri.Receipt!.StoreId));

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
                query = query.Where(ri => ri.Product!.CategoryId.HasValue && dto.CategoryIds.Contains(ri.Product.CategoryId.Value));

            if (dto.SupplierIds != null && dto.SupplierIds.Any())
                query = query.Where(ri => ri.Product!.SupplierId.HasValue && dto.SupplierIds.Contains(ri.Product.SupplierId.Value));

            if (dto.EmployeeIds != null && dto.EmployeeIds.Any())
                query = query.Where(ri => dto.EmployeeIds.Contains(ri.Receipt!.UserId));

            var chartData = new List<RevenueChartData>();

            if (actualStart.Date == actualEnd.Date)
            {
                var rawData = await query
                    .GroupBy(ri => EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(ri.Receipt!.CreatedAt, "UTC"), sqlZone).Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        Revenue = g.Sum(ri => ri.FinalAmount),
                        Profit = g.Sum(ri => ri.FinalAmount - ri.CostPrice * ri.Quantity - ri.AllocatedVatAmount)
                    })
                    .ToListAsync();

                for (int i = 0; i <= 23; i++)
                {
                    var hourData = rawData.FirstOrDefault(d => d.Hour == i);

                    chartData.Add(new RevenueChartData
                    {
                        Date = $"{i:D2}:00",
                        Revenue = hourData?.Revenue ?? 0,
                        Profit = hourData?.Profit ?? 0
                    });
                }
            }
            else
            {
                var rawData = await query
                    .GroupBy(ri => EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(ri.Receipt!.CreatedAt, "UTC"), sqlZone).Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Revenue = g.Sum(ri => ri.FinalAmount),
                        Profit = g.Sum(ri => ri.FinalAmount - ri.CostPrice * ri.Quantity - ri.AllocatedVatAmount)
                    })
                    .ToListAsync();

                int totalDays = (int)(actualEnd.Date - actualStart.Date).TotalDays;

                for (int i = 0; i <= totalDays; i++)
                {
                    var targetDate = actualStart.Date.AddDays(i);
                    var dayData = rawData.FirstOrDefault(d => d.Date == targetDate);

                    chartData.Add(new RevenueChartData
                    {
                        Date = targetDate.ToString("MMM dd"),
                        Revenue = dayData?.Revenue ?? 0,
                        Profit = dayData?.Profit ?? 0
                    });
                }
            }

            return chartData;
        }

        public async Task<List<TopProductChartData>> GetTopPerformingProducts(int companyId, DashboardFilterRequest dto)
        {
            var query = _context.ReceiptItems
                .IgnoreQueryFilters()
                .Include(ri => ri.Receipt)
                .Include(ri => ri.Product)
                .Where(ri =>
                    ri.CompanyId == companyId &&
                    ri.Receipt != null &&
                    ri.Receipt.CompanyId == companyId &&
                    ri.Receipt.IsActive
                )
                .AsQueryable();

            var zone = ReportingTime.Resolve(dto.TimeZoneId);
            var startUtc = ReportingTime.StartUtc(dto.StartDate, zone);
            var endUtc = ReportingTime.EndUtcExclusive(dto.EndDate, zone);
            var sqlZone = ReportingTime.SqlZone(zone);
            if (startUtc.HasValue)
                query = query.Where(ri => ri.Receipt!.CreatedAt >= startUtc.Value);
            if (endUtc.HasValue)
                query = query.Where(ri => ri.Receipt!.CreatedAt < endUtc.Value);
            if (dto.StartHour.HasValue)
                query = query.Where(ri =>
                    EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(ri.Receipt!.CreatedAt, "UTC"), sqlZone).TimeOfDay >= dto.StartHour.Value);
            if (dto.EndHour.HasValue)
                query = query.Where(ri =>
                    EF.Functions.AtTimeZone(EF.Functions.AtTimeZone(ri.Receipt!.CreatedAt, "UTC"), sqlZone).TimeOfDay <= dto.EndHour.Value);

            if (dto.StoreIds != null && dto.StoreIds.Any())
                query = query.Where(ri => dto.StoreIds.Contains(ri.Receipt!.StoreId));

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
                query = query.Where(ri => ri.Product!.CategoryId.HasValue && dto.CategoryIds.Contains(ri.Product.CategoryId.Value));

            if (dto.SupplierIds != null && dto.SupplierIds.Any())
                query = query.Where(ri => ri.Product!.SupplierId.HasValue && dto.SupplierIds.Contains(ri.Product.SupplierId.Value));

            if (dto.EmployeeIds != null && dto.EmployeeIds.Any())
                query = query.Where(ri => dto.EmployeeIds.Contains(ri.Receipt!.UserId));

            var cutoff = DateTime.UtcNow.AddDays(-30);
            return await query
                .GroupBy(ri => new
                {
                    ri.ProductId,
                    Name = ri.Product != null &&
                        (ri.Product.IsActive || (ri.Product.DeletedAt.HasValue && ri.Product.DeletedAt > cutoff))
                        ? ri.Product.Name : "Deleted product"
                })
                .Select(g => new TopProductChartData
                {
                    ProductName = g.Key.Name ?? "Deleted product",
                    Revenue = g.Sum(ri => ri.FinalAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PagedResult<ShiftAuditDto>> GetPagedShiftsAsync(int companyId, int pageNumber, int pageSize, int? storeId)
        {
            var query = _context.Shifts
                .IgnoreQueryFilters()
                .Include(s => s.User)
                .Include(s => s.Store)
                .Where(s => s.CompanyId == companyId)
                .AsNoTracking();

            var cutoff = DateTime.UtcNow.AddDays(-30);

            if (storeId.HasValue)
                query = query.Where(s => s.StoreId == storeId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShiftAuditDto
                {
                    ShiftId = s.ShiftId,
                    StoreName = s.Store!.Name,
                    CashierName = s.User != null &&
                        (s.User.IsActive ||
                         (s.User.DeletedAt.HasValue && s.User.DeletedAt > cutoff))
                        ? s.User.Username : "Deleted user",

                    CashierDeletedRecently = s.User != null &&
                        !s.User.IsActive &&
                        s.User.DeletedAt.HasValue &&
                        s.User.DeletedAt > cutoff,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    StartingCash = s.StartingCash,
                    ExpectedEndingCash = s.ExpectedEndingCash,
                    ActualEndingCash = s.ActualEndingCash,
                    TotalSales = s.TotalSales,
                    Status = s.Status.ToString()
                })
                .ToListAsync();

            return new PagedResult<ShiftAuditDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<ReceiptAuditDto>> GetPagedReceiptsAsync(int companyId, int pageNumber, int pageSize, string? receiptNumber)
        {
            var query = _context.Receipts
                .IgnoreQueryFilters()
                .Include(r => r.User)
                .Include(r => r.Store)
                .Where(r => r.CompanyId == companyId)
                .AsNoTracking();

            var cutoff = DateTime.UtcNow.AddDays(-30);

            if (!string.IsNullOrWhiteSpace(receiptNumber))
                query = query.Where(r => r.ReceiptNumber.Contains(receiptNumber));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReceiptAuditDto
                {
                    ReceiptId = r.ReceiptId,
                    ReceiptNumber = r.ReceiptNumber,
                    StoreName = r.Store!.Name,
                    CashierName = r.User != null &&
                        (r.User.IsActive ||
                         (r.User.DeletedAt.HasValue && r.User.DeletedAt > cutoff))
                        ? r.User.Username : "Deleted user",
                    CashierDeletedRecently = r.User != null &&
                        !r.User.IsActive &&
                        r.User.DeletedAt.HasValue &&
                        r.User.DeletedAt > cutoff,
                    CreatedAt = r.CreatedAt,
                    SubTotal = r.SubTotal,
                    CartDiscountPercentage = r.CartDiscountPercentage,
                    TotalVatAmount = r.TotalVatAmount,
                    FinalTotal = r.FinalTotal,
                    PaymentMethod = r.PaymentMethod.ToString()
                })
                .ToListAsync();

            return new PagedResult<ReceiptAuditDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<ShiftDetailsDto?> GetShiftDetailsAsync(
            int companyId, int shiftId)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);

            var shift = await _context.Shifts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(s => s.CompanyId == companyId &&
                            s.ShiftId == shiftId)
                .Select(s => new
                {
                    s.ShiftId,
                    s.StartDate,
                    s.EndDate,
                    s.StartingCash,
                    s.ActualEndingCash,
                    s.Notes,
                    Status = s.Status.ToString(),
                    StoreName = s.Store != null
                        ? s.Store.Name : "Unknown store",
                    CashierName = s.User != null &&
                        (s.User.IsActive ||
                         (s.User.DeletedAt.HasValue && s.User.DeletedAt > cutoff))
                        ? s.User.FullName : "Deleted user",
                    CashierDeletedRecently = s.User != null &&
                        !s.User.IsActive &&
                        s.User.DeletedAt.HasValue &&
                        s.User.DeletedAt > cutoff
                })
                .FirstOrDefaultAsync();

            if (shift == null) return null;

            // Same receipt calculations used by the POS shift summary.
            var sales = await _context.Receipts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(r => r.CompanyId == companyId &&
                            r.ShiftId == shiftId &&
                            r.IsActive)
                .GroupBy(r => 1)
                .Select(g => new
                {
                    Total = g.Sum(r => r.FinalTotal),
                    Cost = g.Sum(r => r.TotalCostAmount),
                    Vat = g.Sum(r => r.TotalVatAmount),
                    Cash = g.Sum(r => r.PaymentMethod == PaymentMethod.Cash
                        ? r.FinalTotal : 0m),
                    Card = g.Sum(r => r.PaymentMethod == PaymentMethod.Card
                        ? r.FinalTotal : 0m),
                    Voucher = g.Sum(r => r.PaymentMethod == PaymentMethod.Voucher
                        ? r.FinalTotal : 0m),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            // Every recorded active cash movement, including the employee.
            var movements = await _context.CashMovements
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(m => m.CompanyId == companyId &&
                            m.ShiftId == shiftId &&
                            m.IsActive)
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.MovementId)
                .Select(m => new ShiftCashMovementDto
                {
                    MovementId = m.MovementId,
                    UserId = m.UserId,
                    UserName = m.User != null &&
                        (m.User.IsActive ||
                         (m.User.DeletedAt.HasValue && m.User.DeletedAt > cutoff))
                        ? m.User.FullName : "Deleted user",
                    UserDeletedRecently = m.User != null &&
                        !m.User.IsActive &&
                        m.User.DeletedAt.HasValue &&
                        m.User.DeletedAt > cutoff,
                    MovementType = m.MovementType,
                    Amount = m.Amount,
                    Reason = m.Reason,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            var payIns = movements
                .Where(m => m.MovementType == CashMovementType.PayIn)
                .Sum(m => m.Amount);

            var payOuts = movements
                .Where(m => m.MovementType == CashMovementType.PayOut)
                .Sum(m => m.Amount);

            var cashSales = sales?.Cash ?? 0m;

            var expectedCash =
                shift.StartingCash + cashSales + payIns - payOuts;

            return new ShiftDetailsDto
            {
                ShiftId = shift.ShiftId,
                StoreName = shift.StoreName,
                CashierName = shift.CashierName,
                Status = shift.Status,
                StartDate = shift.StartDate,
                CashierDeletedRecently = shift.CashierDeletedRecently,
                EndDate = shift.EndDate,
                StartingCash = shift.StartingCash,
                ExpectedEndingCash = expectedCash,
                ActualEndingCash = shift.ActualEndingCash,
                Variance = shift.ActualEndingCash.HasValue
                    ? shift.ActualEndingCash.Value - expectedCash
                    : null,
                TotalSales = sales?.Total ?? 0m,
                TotalProfit = (sales?.Total ?? 0m)
                    - (sales?.Cost ?? 0m)
                    - (sales?.Vat ?? 0m),
                CashSales = cashSales,
                CardSales = sales?.Card ?? 0m,
                VoucherSales = sales?.Voucher ?? 0m,
                TotalPayIns = payIns,
                TotalPayOuts = payOuts,
                ReceiptCount = sales?.Count ?? 0,
                Notes = shift.Notes,
                Movements = movements
            };
        }

    }
}