using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IProductRepository _productRepository;

        public InventoryService (IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task ProcessTransaction(InventoryTransaction transaction, int userId, string transactionType)
        {
            if (!Enum.TryParse<TransactionAction>(transactionType, true, out var actionEnum))
                throw new AppException("Invalid transaction type.");

            var product = await _productRepository.GetByIdAsync(transaction.ProductId);

            if (product == null) throw new AppException("Product not found.");
            if (!product.IsActive) throw new AppException("Cannot process transactions for a deleted product.");

            var qty = Math.Abs(transaction.Quantity);
            transaction.Quantity = qty;
            transaction.UserId = userId;
            transaction.TransactionType = actionEnum;

            switch (actionEnum)
            {
                case TransactionAction.Sale:
                    if (product.Quantity < qty)
                        throw new AppException($"Insufficient stock. Only {product.Quantity} available.");

                    product.Quantity -= qty;
                    transaction.UnitPrice = transaction.UnitPrice > 0 ? transaction.UnitPrice : product.Price;
                    transaction.TotalAmount = transaction.UnitPrice * qty;
                    transaction.Profit = transaction.TotalAmount - (product.CostPrice * qty);
                    break;

                case TransactionAction.Loss:
                    if (product.Quantity < qty)
                        throw new AppException($"Cannot deduct {qty}. Only {product.Quantity} available.");

                    product.Quantity -= qty;
                    transaction.UnitPrice = 0;
                    transaction.TotalAmount = 0;
                    transaction.Profit = -(product.CostPrice * qty); 
                    break;

                case TransactionAction.Damage:
                    if (product.Quantity < qty)
                        throw new AppException($"Cannot deduct {qty}. Only {product.Quantity} available.");

                    product.Quantity -= qty;
                    transaction.UnitPrice = 0;
                    transaction.TotalAmount = 0;
                    transaction.Profit = -(product.CostPrice * qty); 
                    break;

                case TransactionAction.Restock:
                    var oldQuantity = product.Quantity;
                    var oldTotalValue = oldQuantity * product.CostPrice;

                    transaction.UnitPrice = transaction.UnitPrice > 0 ? transaction.UnitPrice : product.CostPrice;
                    var incomingTotalValue = transaction.UnitPrice * qty;

                    product.Quantity += qty;
                    transaction.TotalAmount = incomingTotalValue;
                    transaction.Profit = 0;

                    if (product.Quantity > 0)
                    {
                        product.CostPrice = (oldTotalValue + incomingTotalValue) / product.Quantity;
                    }
                    break;

                default:
                    throw new AppException("Unsupported transaction operation.");
            }
            
            await _productRepository.SaveTransaction(transaction, product);
        }
    }
}
