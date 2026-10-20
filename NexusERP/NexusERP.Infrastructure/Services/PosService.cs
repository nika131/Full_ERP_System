using NexusERP.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;
using System.Data;
using System.Text.Json;

namespace NexusERP.Infrastructure.Services
{
    public class PosService : IPosService
    {
        private const int MaxPosLayoutProducts = 2000;

        private class CheckoutCalculation
        {
            public decimal SubTotal { get; set; }
            public decimal DiscountedSubtotal { get; set; }
            public decimal FinalTotal { get; set; }
            public decimal TotalVatAmount { get; set; }
            public List<CalculatedCheckoutLine> Lines { get; set; } = new();
        }

        private class CalculatedCheckoutLine
        {
            public Product Product { get; set; } = null!;
            public int Quantity { get; set; }
            public decimal ItemSubtotal { get; set; }
            public decimal AfterMarketDiscount { get; set; }
            public decimal AfterManualDiscount { get; set; }
            public decimal FinalLineTotal { get; set; }
            public decimal ManualItemDiscountPercentage { get; set; }
        }

        private class CheckoutValidationResult
        {
            public Shift ActiveShift { get; set; } = null!;
            public Store Store { get; set; } = null!;
            public List<Product> Products { get; set; } = new();
        }

        private class ShiftCalculationResult
        {
            public decimal CashPayments { get; set; }
            public decimal CardPayments { get; set; }
            public decimal VoucherPayments { get; set; }
            public decimal PayIn { get; set; }
            public decimal PayOut { get; set; }
            public int ReceiptCount { get; set; }
        }

        private readonly ApplicationDbContext _context;

        public PosService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Shift> OpenShiftAsync(int companyId, int userId, OpenShiftDto dto)
        {
            if (dto.StartingCash < 0)
                throw new AppException("Starting amount cant be less than zero.");

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    bool userExists = await _context.Users.AnyAsync(u =>
                        u.CompanyId == companyId &&
                        u.UserId == userId
                    );

                    if (!userExists)
                        throw new AppException("User does not belong to this company.");

                    var existingShift = await _context.Shifts.FirstOrDefaultAsync(s =>
                        s.CompanyId == companyId &&
                        s.UserId == userId &&
                        s.Status == ShiftStatus.Open
                    );

                    if (existingShift != null)
                        throw new AppException("You already have open shift. Close it before opening a new one.");

                    var store = await _context.Stores.FirstOrDefaultAsync(s =>
                        s.CompanyId == companyId &&
                        s.StoreId == dto.StoreId &&
                        s.IsActive
                    );

                    if (store == null)
                        throw new AppException("Store does not exist.");

                    var shift = new Shift
                    {
                        CompanyId = companyId,
                        UserId = userId,
                        StoreId = store.StoreId,
                        StartDate = DateTime.UtcNow,
                        StartingCash = dto.StartingCash,
                        ExpectedEndingCash = dto.StartingCash,
                        Status = ShiftStatus.Open,
                        Notes = dto.Note
                    };

                    _context.Shifts.Add(shift);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return shift;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<Shift> CloseShiftAsync(int companyId, int shiftId, int userId, CloseShiftDto dto)
        {
            if (dto.ActualEndingCash < 0)
                throw new AppException("Actual ending cash cannot be less than zero.");

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    var shift = await _context.Shifts.FirstOrDefaultAsync(s =>
                        s.CompanyId == companyId &&
                        s.ShiftId == shiftId &&
                        s.UserId == userId
                    );

                    if (shift == null)
                        throw new AppException("Shift not found.");

                    if (shift.Status == ShiftStatus.Closed)
                        throw new AppException("Shift is closed.");

                    await InitializeShiftAsync(shift);

                    shift.EndDate = DateTime.UtcNow;
                    shift.ActualEndingCash = dto.ActualEndingCash;
                    shift.Status = ShiftStatus.Closed;

                    if (!string.IsNullOrWhiteSpace(dto.Note))
                        shift.Notes = string.IsNullOrWhiteSpace(shift.Notes) ? dto.Note : $"{shift.Notes} | {dto.Note}";

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return shift;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static decimal RoundToTwoDecimalPlaces(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private async Task<ShiftCalculationResult> InitializeShiftAsync(Shift shift)
        {
            var receiptValues = await _context.Receipts
                .Where(r =>
                    r.CompanyId == shift.CompanyId &&
                    r.ShiftId == shift.ShiftId &&
                    r.IsActive
                )
                .GroupBy(r => 1)
                .Select(g => new
                {
                    TotalSales = g.Sum(r => r.FinalTotal),
                    TotalCost = g.Sum(r => r.TotalCostAmount),
                    TotalVat = g.Sum(r => r.TotalVatAmount),

                    CashSales = g.Sum(r =>
                        r.PaymentMethod == PaymentMethod.Cash
                            ? r.FinalTotal
                            : 0m
                    ),

                    CardSales = g.Sum(r =>
                        r.PaymentMethod == PaymentMethod.Card
                            ? r.FinalTotal
                            : 0m
                    ),

                    VoucherSales = g.Sum(r =>
                        r.PaymentMethod == PaymentMethod.Voucher
                            ? r.FinalTotal
                            : 0m
                    ),

                    ReceiptCount = g.Count()
                })
                .FirstOrDefaultAsync();

            var movementValues = await _context.CashMovements
                .Where(c =>
                    c.CompanyId == shift.CompanyId &&
                    c.ShiftId == shift.ShiftId &&
                    c.IsActive
                )
                .GroupBy(c => 1)
                .Select(g => new
                {
                    PayIns = g.Sum(c =>
                        c.MovementType == CashMovementType.PayIn
                            ? c.Amount
                            : 0m
                    ),

                    PayOuts = g.Sum(c =>
                        c.MovementType == CashMovementType.PayOut
                            ? c.Amount
                            : 0m
                    )
                })
                .FirstOrDefaultAsync();

            var totalSales = receiptValues?.TotalSales ?? 0m;
            var totalCost = receiptValues?.TotalCost ?? 0m;
            var totalVat = receiptValues?.TotalVat ?? 0m;

            var cashSales = receiptValues?.CashSales ?? 0m;
            var cardSales = receiptValues?.CardSales ?? 0m;
            var voucherSales = receiptValues?.VoucherSales ?? 0m;

            var payIns = movementValues?.PayIns ?? 0m;
            var payOuts = movementValues?.PayOuts ?? 0m;

            shift.TotalSales = totalSales;
            shift.TotalProfit = totalSales - totalCost - totalVat;
            shift.ExpectedEndingCash = shift.StartingCash + cashSales + payIns - payOuts;

            await _context.SaveChangesAsync();

            return new ShiftCalculationResult
            {
                CashPayments = cashSales,
                CardPayments = cardSales,
                VoucherPayments = voucherSales,
                PayIn = payIns,
                PayOut = payOuts,
                ReceiptCount = receiptValues?.ReceiptCount ?? 0
            };
        }

        public async Task<CurrentShiftDto?> GetCurrentShiftAsync(int companyId, int userId, int storeId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    var shift = await _context.Shifts
                        .Include(s => s.Store)
                        .FirstOrDefaultAsync(s =>
                            s.CompanyId == companyId &&
                            s.UserId == userId &&
                            s.StoreId == storeId &&
                            s.Status == ShiftStatus.Open
                        );

                    if (shift == null) return null;

                    var calculation = await InitializeShiftAsync(shift);

                    var result = new CurrentShiftDto
                    {
                        ShiftId = shift.ShiftId,
                        StoreId = shift.StoreId,
                        StoreName = shift.Store?.Name ?? string.Empty,
                        StartDate = shift.StartDate,
                        StartingCash = shift.StartingCash,
                        ExpectedEndingCash = shift.ExpectedEndingCash,
                        TotalSales = shift.TotalSales,
                        TotalProfit = shift.TotalProfit,
                        CashSales = calculation.CashPayments,
                        CardSales = calculation.CardPayments,
                        VoucherSales = calculation.VoucherPayments,
                        TotalPayIns = calculation.PayIn,
                        TotalPayOuts = calculation.PayOut,
                        ReceiptCount = calculation.ReceiptCount,
                        Notes = shift.Notes
                    };

                    await transaction.CommitAsync();

                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<ShiftHistoryItemDto>> GetShiftHistoryAsync(int companyId, int userId, int storeId)
        {
            return await _context.Shifts
                .Where(s =>
                    s.CompanyId == companyId &&
                    s.UserId == userId &&
                    s.StoreId == storeId &&
                    s.Status == ShiftStatus.Closed
                )
                .OrderByDescending(s => s.StartDate)
                .Take(30)
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

        public async Task<CashMovementResultDto> AddCashMovementAsync(int companyId, int shiftId, int userId, CashMovementDto dto)
        {
            if (dto.Amount <= 0)
                throw new AppException("Amount must be greater than zero.");

            if (dto.MovementType != CashMovementType.PayIn && dto.MovementType != CashMovementType.PayOut)
                throw new AppException("Invalid movement type.");

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    var shift = await _context.Shifts.FirstOrDefaultAsync(s =>
                        s.CompanyId == companyId &&
                        s.ShiftId == shiftId &&
                        s.Status == ShiftStatus.Open &&
                        s.UserId == userId
                    );

                    if (shift == null)
                        throw new AppException("Active shift not found for this user.");

                    bool hasWarning = false;
                    string? warning = null;

                    if (dto.MovementType == CashMovementType.PayOut)
                    {
                        await InitializeShiftAsync(shift);

                        if (dto.Amount > shift.ExpectedEndingCash)
                        {
                            hasWarning = true;
                            warning = $"PayOut exceeds expected register cash by {dto.Amount - shift.ExpectedEndingCash:C}.";
                        }
                    }

                    var movement = new CashMovement
                    {
                        CompanyId = companyId,
                        ShiftId = shiftId,
                        UserId = userId,
                        StoreId = shift.StoreId,
                        MovementType = dto.MovementType,
                        Amount = dto.Amount,
                        Reason = dto.Reason,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.CashMovements.Add(movement);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new CashMovementResultDto
                    {
                        Movement = movement,
                        HasWarning = hasWarning,
                        Warning = warning
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<ShiftReceiptSummaryDto>> GetShiftReceiptsAsync(int companyId, int shiftId, int userId)
        {
            var shift = await _context.Shifts
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.CompanyId == companyId &&
                    s.ShiftId == shiftId &&
                    s.UserId == userId
                );

            if (shift == null)
                throw new AppException("Shift not found or does not belong to you.");

            return await _context.Receipts
                .Where(r =>
                    r.CompanyId == companyId &&
                    r.ShiftId == shiftId &&
                    r.IsActive
                )
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

        public async Task<ReceiptDetailDto> GetReceiptDetailAsync(int companyId, int receiptId, int userId)
        {
            var receipt = await _context.Receipts
                .Include(r => r.Lines)
                    .ThenInclude(l => l.Product)
                .Include(r => r.User)
                .Include(r => r.Store)
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.CompanyId == companyId &&
                    r.ReceiptId == receiptId &&
                    r.UserId == userId
                );

            if (receipt == null)
                throw new AppException("Receipt not found.");

            return new ReceiptDetailDto
            {
                ReceiptId = receipt.ReceiptId,
                ReceiptNumber = receipt.ReceiptNumber,
                CreatedAt = receipt.CreatedAt,
                CashierName = receipt.User?.FullName ?? "Unknown",
                StoreName = receipt.Store?.Name ?? string.Empty,
                SubTotal = receipt.SubTotal,
                CartDiscountPercentage = receipt.CartDiscountPercentage,
                TotalVatAmount = receipt.TotalVatAmount,
                FinalTotal = receipt.FinalTotal,
                PaymentMethod = receipt.PaymentMethod.ToString(),

                Lines = receipt.Lines.Select(l => new ReceiptLineDto
                {
                    ProductName = l.Product?.Name ?? "Unknown Product",
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    MarketDiscountPercentage = l.MarketDiscountPercentage,
                    ManualItemDiscountPercentage = l.ManualItemDiscountPercentage,
                    LineTotal = l.LineTotal
                }).ToList()
            };
        }

        private async Task<CheckoutValidationResult> ValidateCheckoutAsync(int companyId, int userId, CheckoutRequestDto cart)
        {
            if (cart.Items == null || cart.Items.Count == 0)
                throw new AppException("Cart is empty.");

            var activeShift = await _context.Shifts.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.UserId == userId &&
                s.StoreId == cart.StoreId &&
                s.Status == ShiftStatus.Open
            );

            if (activeShift == null)
                throw new AppException("You must open shift before processing sales.");

            var qtyByProduct = cart.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var store = await _context.Stores.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.StoreId == cart.StoreId &&
                s.IsActive
            );

            if (store == null)
                throw new AppException("Invalid store.");

            if (cart.CartDiscountPercentage < 0 || cart.CartDiscountPercentage > 100)
                throw new AppException("Cart discount percentage must be between 0% and 100%.");

            if (cart.CartDiscountPercentage > store.MaxCartDiscountPercentage)
                throw new AppException($"Cart discount exceeds the store maximum of {store.MaxCartDiscountPercentage}%.");

            var negativeSetting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.SettingKey == "AllowNegativeInventory"
            );

            bool allowNegative = negativeSetting != null &&
                                 bool.TryParse(negativeSetting.SettingValue, out bool parsedVal) &&
                                 parsedVal;

            // Both quote and final checkout pass this validation; do not authorize only at the controller.
            if (cart.CartDiscountPercentage > 0 || cart.Items.Any(i => i.ManualItemDiscountPercentage > 0))
            {
                var cashier = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u =>
                    u.CompanyId == companyId && u.UserId == userId && u.IsActive && u.Role.IsActive &&
                    u.Role.CompanyId == companyId);
                if (cashier == null)
                    throw new UnauthorizedAccessException("Cashier does not have an active company role.");
                if (cart.CartDiscountPercentage > 0 && !cashier.Role.Permissions.Contains(Permissions.ApplyCartDiscount))
                    throw new AppException("You do not have permission to apply cart discounts.");
                if (cart.Items.Any(i => i.ManualItemDiscountPercentage > 0) && !cashier.Role.Permissions.Contains(Permissions.ApplyItemDiscount))
                    throw new AppException("You do not have permission to apply item discounts.");
            }

            var discountSetting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.SettingKey == "DiscountPolicy"
            );

            string discountPolicy = discountSetting?.SettingValue ?? "Enabled";

            bool hasManualDiscounts =
                cart.CartDiscountPercentage > 0 ||
                cart.Items.Any(i => i.ManualItemDiscountPercentage > 0);

            if (hasManualDiscounts && discountPolicy != "Enabled")
            {
                if (discountPolicy == "Disabled")
                    throw new AppException("Manual discounts are currently disabled by system settings.");

                if (discountPolicy == "AdminOnly")
                {
                    var user = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u =>
                            u.CompanyId == companyId &&
                            u.UserId == userId
                        );

                    if (user?.Role?.Name != "Admin")
                        throw new AppException("System settings currently restrict manual discounts to Administrator only.");
                }
            }

            var products = new List<Product>();

            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p =>
                    p.CompanyId == companyId &&
                    p.ProductId == item.ProductId &&
                    p.IsActive
                );

                if (product == null)
                    throw new AppException("Invalid product in cart.");

                if (item.Quantity <= 0)
                    throw new AppException($"Invalid quantity for {product.Name}.");

                if (!allowNegative && product.Quantity < qtyByProduct[item.ProductId])
                    throw new AppException($"Insufficient stock for {product.Name}.");

                if (product.Price < 0)
                    throw new AppException($"Price for {product.Name} cannot be negative.");

                if (product.MarketDiscountRate < 0 || product.MarketDiscountRate > 100)
                    throw new AppException($"Market discount for {product.Name} must be between 0% and 100%.");

                if (item.ManualItemDiscountPercentage < 0 || item.ManualItemDiscountPercentage > 100)
                    throw new AppException($"Manual discount for {product.Name} must be between 0% and 100%.");

                if (item.ManualItemDiscountPercentage > product.MaxDiscountPercentage)
                    throw new AppException($"Discount for {product.Name} exceeds the maximum allowed {product.MaxDiscountPercentage}%.");

                if (product.VatRate < 0 || product.VatRate > 100)
                    throw new AppException($"VAT rate for {product.Name} must be between 0% and 100%.");

                products.Add(product);
            }

            return new CheckoutValidationResult
            {
                ActiveShift = activeShift,
                Store = store,
                Products = products
            };
        }

        private CheckoutCalculation CalculateCheckoutHelper(CheckoutRequestDto cart, List<Product> products)
        {
            var result = new CheckoutCalculation();

            foreach (var item in cart.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);

                decimal itemSubtotal = product.Price * item.Quantity;

                decimal afterMarketDiscount =
                    itemSubtotal * (1m - product.MarketDiscountRate / 100m);

                decimal afterManualDiscount =
                    afterMarketDiscount * (1m - item.ManualItemDiscountPercentage / 100m);

                decimal afterCartDiscount =
                    afterManualDiscount * (1m - cart.CartDiscountPercentage / 100m);

                decimal vatAmount =
                    afterCartDiscount * (product.VatRate / (100m + product.VatRate));

                result.SubTotal += itemSubtotal;
                result.DiscountedSubtotal += afterManualDiscount;
                result.FinalTotal += afterCartDiscount;
                result.TotalVatAmount += vatAmount;

                result.Lines.Add(new CalculatedCheckoutLine
                {
                    Product = product,
                    Quantity = item.Quantity,
                    ItemSubtotal = itemSubtotal,
                    AfterMarketDiscount = afterMarketDiscount,
                    AfterManualDiscount = afterManualDiscount,
                    FinalLineTotal = afterManualDiscount,
                    ManualItemDiscountPercentage = item.ManualItemDiscountPercentage
                });
            }

            result.SubTotal = RoundToTwoDecimalPlaces(result.SubTotal);
            result.FinalTotal = RoundToTwoDecimalPlaces(result.FinalTotal);
            result.TotalVatAmount = RoundToTwoDecimalPlaces(result.TotalVatAmount);

            return result;
        }

        public async Task<Receipt> ProcessCheckoutAsync(int companyId, int userId, CheckoutRequestDto cart)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    var validation = await ValidateCheckoutAsync(companyId, userId, cart);

                    var activeShift = validation.ActiveShift;
                    var products = validation.Products;

                    if (!Enum.TryParse<PaymentMethod>(cart.PaymentMethod, true, out var paymentMethod))
                        throw new AppException($"Invalid payment method: {cart.PaymentMethod}.");

                    var receipt = new Receipt
                    {
                        CompanyId = companyId,
                        UserId = userId,
                        StoreId = cart.StoreId,
                        ShiftId = activeShift.ShiftId,
                        CartDiscountPercentage = cart.CartDiscountPercentage,
                        PaymentMethod = paymentMethod,
                        CreatedAt = DateTime.UtcNow
                    };

                    decimal totalCostForProfit = 0;

                    var calculation = CalculateCheckoutHelper(cart, products);

                    receipt.SubTotal = calculation.SubTotal;
                    receipt.FinalTotal = calculation.FinalTotal;
                    receipt.TotalVatAmount = calculation.TotalVatAmount;

                    foreach (var line in calculation.Lines)
                    {
                        var product = line.Product;

                        product.Quantity -= line.Quantity;

                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            CompanyId = companyId,
                            ProductId = product.ProductId,
                            UserId = userId,
                            StoreId = cart.StoreId,
                            TransactionType = TransactionAction.Sale,
                            Quantity = line.Quantity,
                            CreatedAt = DateTime.UtcNow
                        });

                        totalCostForProfit += product.CostPrice * line.Quantity;

                        receipt.Lines.Add(new ReceiptItem
                        {
                            CompanyId = companyId,
                            ProductId = product.ProductId,
                            Quantity = line.Quantity,
                            UnitPrice = product.Price,
                            CostPrice = product.CostPrice,
                            VatRate = product.VatRate,
                            MarketDiscountPercentage = product.MarketDiscountRate,
                            ManualItemDiscountPercentage = line.ManualItemDiscountPercentage,
                            LineTotal = line.FinalLineTotal
                        });
                    }

                    receipt.TotalCostAmount = totalCostForProfit;

                    _context.Receipts.Add(receipt);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return receipt;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<CheckoutQuoteDto> GetCheckoutQuoteAsync(int companyId, int userId, CheckoutRequestDto cart)
        {
            var validation = await ValidateCheckoutAsync(companyId, userId, cart);
            var calculation = CalculateCheckoutHelper(cart, validation.Products);

            return new CheckoutQuoteDto
            {
                FinalTotal = calculation.FinalTotal
            };
        }

        private async Task<List<ProductResponseDto>> GetPosLayoutProductsAsync(int companyId, List<int> productIds)
        {
            if (productIds.Count == 0)
                return new List<ProductResponseDto>();

            return await _context.Products
                .AsNoTracking()
                .Where(p =>
                    p.CompanyId == companyId &&
                    productIds.Contains(p.ProductId)
                )
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,

                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null
                        ? p.Category.CategoryName
                        : "Uncategorized",

                    SupplierId = p.SupplierId,
                    CompanyName = p.Supplier != null
                        ? p.Supplier.CompanyName
                        : "No Supplier",

                    Quantity = p.Quantity,
                    LowStockThreshold = p.LowStockThreshold,

                    Price = p.Price,
                    CostPrice = p.CostPrice,

                    VatRate = p.VatRate,
                    MarketDiscountRate = p.MarketDiscountRate,
                    MaxDiscountPercentage = p.MaxDiscountPercentage,

                    Barcode = p.Barcode,

                    ImageUrl = p.ImageUrl,
                    ShapeType = p.ShapeType,
                    ShapeColor = p.ShapeColor,
                    ShapeText = p.ShapeText,
                    DisplayMode = p.DisplayMode
                })
                .ToListAsync();
        }

        public async Task<PosLayoutDto> GetPosLayoutAsync(int companyId, int storeId)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.CompanyId == companyId &&
                    s.StoreId == storeId
                );

            if (store == null)
                throw new AppException("Store not found.");

            if (string.IsNullOrWhiteSpace(store.PosLayoutJson))
            {
                return new PosLayoutDto
                {
                    StoreId = storeId,
                    Slots = new List<PosLayoutSlotDto>(),
                    Products = new List<ProductResponseDto>()
                };
            }

            try
            {
                var slots = JsonSerializer.Deserialize<List<PosLayoutSlotDto>>(store.PosLayoutJson)
                            ?? new List<PosLayoutSlotDto>();

                if (slots.Count > MaxPosLayoutProducts)
                    throw new AppException($"POS layout cannot contain more than {MaxPosLayoutProducts} products.");

                if (slots.Any(s => s.SlotIndex < 0 || s.ProductId <= 0))
                    throw new AppException("Saved POS layout contains invalid data.");

                if (slots.GroupBy(s => s.SlotIndex).Any(group => group.Count() > 1))
                    throw new AppException("Saved POS layout contains duplicate slot positions.");

                if (slots.GroupBy(s => s.ProductId).Any(group => group.Count() > 1))
                    throw new AppException("Saved POS layout contains the same product more than once.");

                var orderedSlots = slots
                    .OrderBy(s => s.SlotIndex)
                    .ToList();

                var productIds = orderedSlots
                    .Select(s => s.ProductId)
                    .Distinct()
                    .ToList();

                var products = await GetPosLayoutProductsAsync(companyId, productIds);

                var validProductIds = products
                    .Select(p => p.ProductId)
                    .ToHashSet();

                var cleanSlots = orderedSlots
                    .Where(s => validProductIds.Contains(s.ProductId))
                    .ToList();

                return new PosLayoutDto
                {
                    StoreId = storeId,
                    Slots = cleanSlots,
                    Products = products
                };
            }
            catch (JsonException)
            {
                throw new AppException("Invalid POS layout data.");
            }
        }

        public async Task<PosLayoutDto> SavePosLayoutAsync(int companyId, int storeId, UpdatePosLayoutDto dto)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.StoreId == storeId
            );

            if (store == null)
                throw new AppException("Store not found.");

            var slots = dto.Slots ?? new List<PosLayoutSlotDto>();

            if (slots.Count > MaxPosLayoutProducts)
                throw new AppException($"POS layout cannot contain more than {MaxPosLayoutProducts} products.");

            if (slots.Any(s => s.SlotIndex < 0))
                throw new AppException("Slot index cannot be negative.");

            if (slots.Any(s => s.ProductId <= 0))
                throw new AppException("POS layout contains an invalid product.");

            if (slots.GroupBy(s => s.SlotIndex).Any(group => group.Count() > 1))
                throw new AppException("POS layout contains duplicate slot indices.");

            if (slots.GroupBy(s => s.ProductId).Any(group => group.Count() > 1))
                throw new AppException("A product cannot appear more than once in the POS layout.");

            var productIds = slots
                .Select(s => s.ProductId)
                .Distinct()
                .ToList();

            var products = await GetPosLayoutProductsAsync(companyId, productIds);

            if (products.Count != productIds.Count)
                throw new AppException("POS layout contains an invalid or inactive product.");

            var cleanSlots = slots
                .OrderBy(s => s.SlotIndex)
                .Select(s => new PosLayoutSlotDto
                {
                    SlotIndex = s.SlotIndex,
                    ProductId = s.ProductId
                })
                .ToList();

            store.PosLayoutJson = JsonSerializer.Serialize(cleanSlots);
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PosLayoutDto
            {
                StoreId = storeId,
                Slots = cleanSlots,
                Products = products
            };
        }
    }
}