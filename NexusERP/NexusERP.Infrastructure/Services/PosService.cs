using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;
using System.Data;

namespace NexusERP.Infrastructure.Services
{
    public class PosService : IPosService
    {
        private class CheckoutCalculation
        {
            public decimal SubTotal { get; set; }
            public decimal DiscountedSubtotal { get; set; }
            public decimal FinalTotal { get; set; }

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

        private readonly ApplicationDbContext _context;

        public PosService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Shift> OpenShiftAsync(int userId, OpenShiftDto dto)
        {
            var existingShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == ShiftStatus.Open);


            if (existingShift != null)
                throw new AppException("You already have open shift. Close it before opening a new one. ");

            var shift = new Shift
            {
                UserId = userId,
                StoreId = dto.StoreId,
                StartDate = DateTime.UtcNow,
                StartingCash = dto.StartingCash,
                ExpectedEndingCash = dto.StartingCash,
                Status = ShiftStatus.Open,
                Notes = dto.Note
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task<Shift> CloseShiftAsync(int shiftId, int userId, CloseShiftDto dto)
        {
            var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.ShiftId == shiftId && s.UserId == userId);

            if (shift == null) throw new AppException("Shift not found.");
            if (shift.Status == ShiftStatus.Closed) throw new AppException("Shift is closed");

            shift.EndDate = DateTime.UtcNow;
            shift.ActualEndingCash = dto.ActualEndingCash;
            shift.Status = ShiftStatus.Closed;

            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                shift.Notes = string.IsNullOrWhiteSpace(shift.Notes) ? dto.Note : $"{shift.Notes} | {dto.Note}";
            }

            _context.Shifts.Update(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

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

        public async Task<CashMovement> AddCashMovementAsync(int shiftId, int userId, CashMovementDto dto)
        {
            var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.ShiftId == shiftId && s.Status == ShiftStatus.Open);
            if (shift == null) throw new AppException("Active shift not found for this user.");

            var movement = new CashMovement
            {
                ShiftId = shiftId,
                UserId = userId,
                StoreId = dto.StoreId,
                MovementType = dto.MovementType,
                Amount = dto.Amount,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow,
            };

            if (dto.MovementType == CashMovementType.PayIn) shift.ExpectedEndingCash += dto.Amount;
            else if (dto.MovementType == CashMovementType.PayOut) shift.ExpectedEndingCash -= dto.Amount;

            _context.CashMovements.Add(movement);
            _context.Shifts.Update(shift);
            await _context.SaveChangesAsync();

            return movement;
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

        private async Task<CheckoutValidationResult> ValidateCheckoutAsync(int userId, CheckoutRequestDto cart)
        {
            if (cart.Items == null || cart.Items.Count == 0)
                throw new AppException("Cart is empty.");

            // 1. Active Shift
            var activeShift = await _context.Shifts.FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.StoreId == cart.StoreId &&
                s.Status == ShiftStatus.Open);

            if (activeShift == null)
                throw new AppException("You must open shift before processing sales.");

            // group every product to count total quantity if product repities multiple times in the list
            var qtyByProduct = cart.Items.GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            // 2. Store
            var store = await _context.Stores.FindAsync(cart.StoreId);

            if (store == null)
                throw new AppException("Invalid store.");

            // 3. Cart discount
            if (cart.CartDiscountPercentage < 0 || cart.CartDiscountPercentage > 100)
                throw new AppException("Cart discount percentage must be between 0% and 100%");

            if (cart.CartDiscountPercentage > store.MaxCartDiscountPercentage)
                throw new AppException($"Cart discount exceeds the store maximum of {store.MaxCartDiscountPercentage}%");

            // 4. System settings 
            var negativeSetting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.SettingKey == "AllowNegativeInventory");

            bool allowNegative = negativeSetting != null && bool.TryParse(negativeSetting.SettingValue, out bool parsedVal) && parsedVal;

            var discountSetting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.SettingKey == "DiscountPolicy");

            string discountPolicy =
                discountSetting?.SettingValue ?? "Enabled";

            // 5. Discount policy
            bool hasManualDiscounts =
                cart.CartDiscountPercentage > 0 || cart.Items.Any(i => i.ManualItemDiscountPercentage > 0);

            if (hasManualDiscounts && discountPolicy != "Enabled")
            {
                if (discountPolicy == "Disabled")
                {
                    throw new AppException("Manual discounts are currently disabled globally by system settings.");
                }

                if (discountPolicy == "AdminOnly")
                {
                    var user = await _context.Users.Include(
                        u => u.Role)
                        .FirstOrDefaultAsync(u => u.UserId == userId);

                    if (user?.Role?.Name != "Admin")
                        throw new AppException("System settings currently restrict manual discounts to Administrator only.");
                }
            }

            // 6. Validate every cart item
            var products = new List<Product>();

            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null || !product.IsActive)
                    throw new AppException("Invalid Product in cart");

                // Quantity being sold must always be positive.
                if (item.Quantity <= 0)
                    throw new AppException($"Invalid quantity for {product.Name}.");

                // Prevent selling more stock than available.
                if (!allowNegative && product.Quantity < qtyByProduct[item.ProductId])
                    throw new AppException($"Insufficient stock for {product.Name}.");
                
                // Product price itself must never be negative.
                if (product.Price < 0)
                    throw new AppException($"Price for {product.Name} cannot be negative.");

                // Market discount must be safe.
                if (product.MarketDiscountRate < 0 || product.MarketDiscountRate > 100)
                    throw new AppException(
                        $"Market discount for {product.Name} must be between 0% and 100%.");

                // Manual discount must be safe.
                if (item.ManualItemDiscountPercentage < 0 || item.ManualItemDiscountPercentage > 100)
                    throw new AppException(
                        $"Manual discount for {product.Name} must be between 0% and 100%.");

                // Manual discount must also respect the product-specific maximum.
                if (item.ManualItemDiscountPercentage > product.MaxDiscountPercentage)
                    throw new AppException(
                        $"Discount for {product.Name} exceeds the maximum allowed {product.MaxDiscountPercentage}%.");

                // Vart rate check
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

                //original price
                decimal itemSubtotal = product.Price * item.Quantity;

                // 1. Market discount
                decimal afterMarketDiscount = itemSubtotal * (1m - product.MarketDiscountRate / 100m);

                // 2. Manual item discount
                decimal afterManualDiscount = afterMarketDiscount * (1m - item.ManualItemDiscountPercentage / 100m);

                result.SubTotal += itemSubtotal;
                result.DiscountedSubtotal += afterManualDiscount;

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

            // 3. Cart discount
            result.FinalTotal = result.DiscountedSubtotal * (1m - cart.CartDiscountPercentage / 100m);
            
            return result;
        }

        public async Task<Receipt> ProcessCheckoutAsync(int userId, CheckoutRequestDto cart)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction =
                    await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    var validation = await ValidateCheckoutAsync(userId, cart);

                    var activeShift = validation.ActiveShift;
                    var products = validation.Products;

                    // payment method validation
                    if (!Enum.TryParse<PaymentMethod>(cart.PaymentMethod, true, out var paymentMethod))
                    {
                        throw new AppException(
                            $"Invalid payment method: {cart.PaymentMethod}.");
                    }

                    var receipt = new Receipt
                    {
                        UserId = userId,
                        StoreId = cart.StoreId,
                        ShiftId = activeShift.ShiftId,
                        CartDiscountPercentage = cart.CartDiscountPercentage,
                        PaymentMethod = paymentMethod,
                        CreatedAt = DateTime.UtcNow,
                    };

                    decimal totalCostForProfit = 0;

                    var calculation = CalculateCheckoutHelper(cart, products);

                    receipt.SubTotal = calculation.SubTotal;
                    receipt.FinalTotal = calculation.FinalTotal;

                    foreach (var line in calculation.Lines)
                    {
                        var product = line.Product;

                        product.Quantity -= line.Quantity;

                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
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
                            ProductId = product.ProductId,
                            Quantity = line.Quantity,
                            UnitPrice = product.Price,
                            CostPrice = product.CostPrice,
                            VatRate = product.VatRate,
                            MarketDiscountPercentage = product.MarketDiscountRate,
                            ManualItemDiscountPercentage = line.ManualItemDiscountPercentage,
                            LineTotal = line.FinalLineTotal,
                        });
                    }

                    receipt.TotalVatAmount =
                        receipt.Lines.Sum(l =>
                        {
                            decimal lineAfterItemDiscounts =
                                l.LineTotal;

                            decimal lineAfterCartDiscount =
                                lineAfterItemDiscounts *
                                (1m - receipt.CartDiscountPercentage / 100m);

                            return lineAfterCartDiscount *
                                (l.VatRate / (100m + l.VatRate));
                        });

                    activeShift.TotalSales += receipt.FinalTotal;

                    activeShift.TotalProfit +=
                        receipt.FinalTotal - totalCostForProfit - receipt.TotalVatAmount;

                    if (receipt.PaymentMethod == PaymentMethod.Cash)
                    {
                        activeShift.ExpectedEndingCash += receipt.FinalTotal;
                    }

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

        public async Task<CheckoutQuoteDto> GetCheckoutQuoteAsync(int userId, CheckoutRequestDto cart)
        {
            var validation = await ValidateCheckoutAsync(userId, cart);
            var products = validation.Products;

            var calculation = CalculateCheckoutHelper(cart, products);

            return new CheckoutQuoteDto
            {
                FinalTotal = calculation.FinalTotal
            };
        }

    }
}
