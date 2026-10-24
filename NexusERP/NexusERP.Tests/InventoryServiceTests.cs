using Moq;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Services;

namespace NexusERP.Tests
{
    public class InventoryServiceTests
    {
        private const int CompanyId = 1;
        private const int UserId = 99;

        [Fact]
        public async Task ProcessTransaction_Sale_ThrowsAppException()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var product = new Product
            {
                ProductId = 1,
                CompanyId = CompanyId,
                Name = "Test Product",
                IsActive = true,
                Quantity = 10
            };

            mockRepo.Setup(r => r.GetByIdAsync(CompanyId, 1)).ReturnsAsync(product);

            var transaction = new InventoryTransaction
            {
                ProductId = 1,
                Quantity = 2
            };

            var exception = await Assert.ThrowsAsync<AppException>(() =>
                service.ProcessTransaction(transaction, UserId, CompanyId, "Sale"));

            Assert.Contains("POS Checkout", exception.Message);
        }

        [Fact]
        public async Task ProcessTransaction_Loss_DeductsQuantity()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var product = new Product
            {
                ProductId = 2,
                CompanyId = CompanyId,
                Name = "Damaged Monitor",
                IsActive = true,
                Quantity = 5
            };

            mockRepo.Setup(r => r.GetByIdAsync(CompanyId, 2)).ReturnsAsync(product);
            mockRepo.Setup(r => r.IsNegativeInventoryAllowedAsync(CompanyId)).ReturnsAsync(false);

            var transaction = new InventoryTransaction
            {
                ProductId = 2,
                Quantity = 3
            };

            await service.ProcessTransaction(transaction, UserId, CompanyId, "Loss");

            Assert.Equal(2, product.Quantity);
            Assert.Equal(CompanyId, transaction.CompanyId);
        }

        [Fact]
        public async Task ProcessTransaction_Loss_InsufficientStock_ThrowsAppException()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var product = new Product
            {
                ProductId = 3,
                CompanyId = CompanyId,
                IsActive = true,
                Quantity = 5
            };

            mockRepo.Setup(r => r.GetByIdAsync(CompanyId, 3)).ReturnsAsync(product);
            mockRepo.Setup(r => r.IsNegativeInventoryAllowedAsync(CompanyId)).ReturnsAsync(false);

            var transaction = new InventoryTransaction
            {
                ProductId = 3,
                Quantity = 10
            };

            var exception = await Assert.ThrowsAsync<AppException>(() =>
                service.ProcessTransaction(transaction, UserId, CompanyId, "Loss"));

            Assert.Contains("Only 5 available", exception.Message);
        }

        [Fact]
        public async Task ProcessTransaction_Restock_IncreasesQuantity()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var product = new Product
            {
                ProductId = 4,
                CompanyId = CompanyId,
                IsActive = true,
                Quantity = 10,
                CostPrice = 100m
            };

            mockRepo.Setup(r => r.GetByIdAsync(CompanyId, 4)).ReturnsAsync(product);
            mockRepo.Setup(r => r.IsNegativeInventoryAllowedAsync(CompanyId)).ReturnsAsync(false);

            var transaction = new InventoryTransaction
            {
                ProductId = 4,
                Quantity = 10
            };

            await service.ProcessTransaction(transaction, UserId, CompanyId, "Restock");

            Assert.Equal(20, product.Quantity);
            Assert.Equal(100m, product.CostPrice);
        }

        [Fact]
        public async Task ProcessTransaction_ProductInactive_ThrowsAppException()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var product = new Product
            {
                ProductId = 5,
                CompanyId = CompanyId,
                IsActive = false,
                Quantity = 100
            };

            mockRepo.Setup(r => r.GetByIdAsync(CompanyId, 5)).ReturnsAsync(product);

            var transaction = new InventoryTransaction
            {
                ProductId = 5,
                Quantity = 10
            };

            var exception = await Assert.ThrowsAsync<AppException>(() =>
                service.ProcessTransaction(transaction, UserId, CompanyId, "Loss"));

            Assert.Contains("deleted product", exception.Message);
        }

        [Fact]
        public async Task ProcessTransaction_InvalidTransactionType_ThrowsAppException()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var transaction = new InventoryTransaction
            {
                ProductId = 1,
                Quantity = 5
            };

            var exception = await Assert.ThrowsAsync<AppException>(() =>
                service.ProcessTransaction(transaction, UserId, CompanyId, "Steal"));

            Assert.Contains("Invalid transaction type", exception.Message);
        }

        [Fact]
        public async Task ProcessTransaction_NegativeQuantityInput_ConvertsToAbsoluteValue()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var product = new Product
            {
                ProductId = 6,
                CompanyId = CompanyId,
                IsActive = true,
                Quantity = 10
            };

            mockRepo.Setup(r => r.GetByIdAsync(CompanyId, 6)).ReturnsAsync(product);
            mockRepo.Setup(r => r.IsNegativeInventoryAllowedAsync(CompanyId)).ReturnsAsync(false);

            var transaction = new InventoryTransaction
            {
                ProductId = 6,
                Quantity = -4
            };

            await service.ProcessTransaction(transaction, UserId, CompanyId, "Loss");

            Assert.Equal(6, product.Quantity);
            Assert.Equal(4, transaction.Quantity);
        }
    }
}