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
                if (product.Quantity < item.Quantity) throw new AppException($"Insufficient stock for {product.Name}.");

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
    }
}
