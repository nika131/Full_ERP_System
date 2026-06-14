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
    }
}
