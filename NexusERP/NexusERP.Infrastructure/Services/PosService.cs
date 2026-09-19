using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Services
{
    public class PosService : IPosService
    {
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

        public async Task<Receipt> ProcessCheckoutAsync(int userId, CheckoutRequestDto cart)
        {
            var activeShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.StoreId == cart.StoreId && s.Status == ShiftStatus.Open);

            if (activeShift == null) throw new AppException("You must open shift before processing sales. ");

            var store = await _context.Stores.FindAsync(cart.StoreId);
            if (store == null) throw new AppException("Invalid store. ");

            var negativeSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "AllowNegativeInventory");
            bool allowNegative = negativeSetting != null && bool.TryParse(negativeSetting.SettingValue, out bool parsedVal) && parsedVal;

            var discountSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "DiscountPolicy");
            string discountPolicy = discountSetting?.SettingValue ?? "Enabled";

            bool hasManualDiscounts = cart.CartDiscountAmount > 0 || cart.Items.Any(i => i.ManualItemDiscount > 0);

            if (hasManualDiscounts && discountPolicy != "Enabled")
            {
                if (discountPolicy == "Disabled")
                    throw new AppException("Manual discounts are currently disabled globally by system settings.");

                if (discountPolicy == "AdminOnly")
                {
                    var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user?.Role?.Name != "Admin")
                        throw new AppException("System settings currently restrict manual discounts to Administrators only.");
                }
            }

            var receipt = new Receipt
            {
                UserId = userId,
                StoreId = cart.StoreId,
                ShiftId = activeShift.ShiftId,
                CartDiscountAmount = cart.CartDiscountAmount,
                PaymentMethod = Enum.Parse<PaymentMethod>(cart.PaymentMethod),
                CreatedAt = DateTime.UtcNow,
            };

            decimal totalCostForProfit = 0;
            decimal calculatedSubtotal = 0;

            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || !product.IsActive) throw new AppException("Invalid Product in cart");

                if (!allowNegative && product.Quantity < item.Quantity) 
                    throw new AppException($"Insufficient stock for {product.Name}.");

                decimal maxAllowedItemDiscount = (product.MaxDiscountPercentage / 100m ) * (product.Price * item.Quantity);
                if (item.ManualItemDiscount > maxAllowedItemDiscount)
                    throw new AppException($"Discount for {product.Name} exceeds the maximum allowed {product.MaxDiscountPercentage}%.");


                product.Quantity -= item.Quantity;
                _context.InventoryTransactions.Add(new InventoryTransaction
                {
                    ProductId = product.ProductId,
                    UserId = userId,
                    StoreId = cart.StoreId,
                    TransactionType = TransactionAction.Sale,
                    Quantity = item.Quantity,
                    CreatedAt = DateTime.UtcNow
                });

                decimal itemSubtotal = product.Price * item.Quantity;
                decimal marketDiscount = (product.MarketDiscountRate / 100m) * itemSubtotal;
                decimal lineTotal = itemSubtotal - marketDiscount - item.ManualItemDiscount;

                calculatedSubtotal += itemSubtotal;
                receipt.TotalVatAmount += lineTotal * (product.VatRate / 100m);
                totalCostForProfit += (product.CostPrice * item.Quantity);

                receipt.Lines.Add(new ReceiptItem
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    CostPrice = product.CostPrice,
                    VatRate = product.VatRate,
                    MarketDiscountAmount = marketDiscount,
                    ManualItemDiscountAmount = item.ManualItemDiscount,
                    LineTotal = lineTotal,
                });
            }

            receipt.SubTotal = calculatedSubtotal;

            decimal maxAllowedCartDiscount = (store.MaxCartDiscountPercentage / 100m) * receipt.SubTotal;
            if (cart.CartDiscountAmount > maxAllowedCartDiscount)
                throw new AppException($"Cart discount exceeds the store maximum of {store.MaxCartDiscountPercentage}%");

            receipt.FinalTotal = receipt.SubTotal - receipt.CartDiscountAmount - receipt.Lines.Sum(l => l.MarketDiscountAmount + l.ManualItemDiscountAmount);

            activeShift.TotalSales += receipt.FinalTotal;
            activeShift.TotalProfit += (receipt.FinalTotal - totalCostForProfit);

            if (receipt.PaymentMethod == PaymentMethod.Cash)
            {
                activeShift.ExpectedEndingCash += receipt.FinalTotal;
            }

            _context.Receipts.Add( receipt );
            await _context.SaveChangesAsync();

            return receipt;
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
    }
}
