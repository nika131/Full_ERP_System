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
                    throw new AppException($"Sales must now be processed through the POS Checkout system to generate a valid Receipt.");

                case TransactionAction.Loss:
                    if (product.Quantity < qty)
                        throw new AppException($"Cannot deduct {qty}. Only {product.Quantity} available.");

                    product.Quantity -= qty;
                    break;

                case TransactionAction.Damage:
                    if (product.Quantity < qty)
                        throw new AppException($"Cannot deduct {qty}. Only {product.Quantity} available.");

                    product.Quantity -= qty;
                    break;

                case TransactionAction.Restock:
                    var oldQuantity = product.Quantity;
                    var oldTotalValue = oldQuantity * product.CostPrice;

                    product.Quantity += qty;
                    break;

                default:
                    throw new AppException("Unsupported transaction operation.");
            }

            string changes = $"{transactionType} product '{product.Name}' quantity {qty}.";
            var audit = new SystemAuditLog
            {
                UserId = userId,
                EntityType = "Product",
                EntityId = product.ProductId,
                Action = transactionType,
                ChangesMade = changes,
                CreatedAt = DateTime.UtcNow
            };
            
            await _productRepository.SaveTransaction(transaction, product, audit);
        }
    }
}
