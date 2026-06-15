using Moq;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Tests
{
    public class InventoryServiceTests
    {

        [Fact]
        public async Task ProcessTransaction_sale_DeductsQuantityAndCalculatesProfit()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var fakeProduct = new Product
            {
                ProductId = 1,
                Name = "Test Server",
                IsActive = true,
                Quantity = 10,
                CostPrice = 500m,
                Price = 800m
            };

            var saleTransaction = new InventoryTransaction
            {
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 800m
            };

            mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(fakeProduct);

            await service.ProcessTransaction(saleTransaction, userId: 99, "Sale");

            Assert.Equal(8, fakeProduct.Quantity);

            Assert.Equal(1600m, saleTransaction.TotalAmount);

            Assert.Equal(600m, saleTransaction.Profit);

            mockRepo.Verify(repo => repo.SaveTransaction(saleTransaction, fakeProduct), Times.Once);
        }

        [Fact]
        public async Task ProcessTransaction_Loss_DeductsQuantityAndCalculatesNegativeProfit()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var fakeProduct = new Product
            {
                ProductId = 2,
                Name = "Damaged Monitor",
                IsActive = true,
                Quantity = 5,
                CostPrice = 200m
            };

            var lossTransaction = new InventoryTransaction
            {
                ProductId = 2,
                Quantity = 3,
            };

            mockRepo.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(fakeProduct);

            await service.ProcessTransaction(lossTransaction, userId: 99, "Loss");

            Assert.Equal(2, fakeProduct.Quantity);
            Assert.Equal(0m, lossTransaction.TotalAmount);
            Assert.Equal(-600m, lossTransaction.Profit);

            mockRepo.Verify(repo => repo.SaveTransaction(lossTransaction, fakeProduct), Times.Once);
        }

        [Fact]
        public async Task ProcessTransaction_Sale_InsufficientStock_ThrowsAppException()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var fakeProduct = new Product
            {
                ProductId = 3,
                IsActive = true,
                Quantity = 5
            };

            var overSaleTransaction = new InventoryTransaction
            {
                ProductId = 3,
                Quantity = 10,
                UnitPrice = 100m
            };

            mockRepo.Setup(repo => repo.GetByIdAsync(3)).ReturnsAsync(fakeProduct);

            var exception = await Assert.ThrowsAnyAsync<NexusERP.Domain.Exceptions.AppException>(() =>
                service.ProcessTransaction(overSaleTransaction, userId: 99, "Sale"));

            Assert.Contains("Insufficient stock", exception.Message);

            mockRepo.Verify(repo => repo.SaveTransaction(It.IsAny<InventoryTransaction>(), It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTransaction_Restock_CalculatesMovingAverageCost()
        {
            var mockRepo = new Mock<IProductRepository>();
            var service = new InventoryService(mockRepo.Object);

            var fakeProduct = new Product
            {
                ProductId = 4,
                IsActive = true,
                Quantity = 10,
                CostPrice = 100m
            };

            var restockTransaction = new InventoryTransaction
            {
                ProductId = 4,
                Quantity = 10,
                UnitPrice = 150m 
            };

            mockRepo.Setup(repo => repo.GetByIdAsync(4)).ReturnsAsync(fakeProduct);

            await service.ProcessTransaction(restockTransaction, userId: 99, "Restock");

            Assert.Equal(20, fakeProduct.Quantity); 
            Assert.Equal(1500m, restockTransaction.TotalAmount);
            Assert.Equal(0m, restockTransaction.Profit);

            Assert.Equal(125m, fakeProduct.CostPrice);

            mockRepo.Verify(repo => repo.SaveTransaction(restockTransaction, fakeProduct), Times.Once);
        }
    }
}
