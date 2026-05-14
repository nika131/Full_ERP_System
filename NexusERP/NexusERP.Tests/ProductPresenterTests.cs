using Moq;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Application.Presenters;
using NexusERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Tests
{
    public class ProductPresenterTests
    {
        [Fact]
        public void SaveProduct_WhenNameIsEmpty_ShowErrorMessageAndDoseNotSave()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockView = new Mock<IProductView>();

            mockView.Setup(v => v.ViewProductName).Returns("");

            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);


            presenter.SaveProduct();


            mockView.Verify(v => v.ShowMessage("Product Name is Required"), Times.Once);

            mockRepo.Verify(r => r.UpSert(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public void SaveProduct_WithValidData_CallsUpSertAndRefreshesData()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockView = new Mock<IProductView>();

            mockView.Setup(v => v.ProductId).Returns(0);
            mockView.Setup(v => v.ViewProductName).Returns("Gaming Mouse");
            mockView.Setup(v => v.CategoryId).Returns(1);
            mockView.Setup(v => v.ProductPrice).Returns(50m);
            mockView.Setup(v => v.CostPrice).Returns(20m);
            mockView.Setup(v => v.ProductQuantity).Returns(100);
            mockView.Setup(v => v.SupplierId).Returns(2);

            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);


            presenter.SaveProduct();


            mockRepo.Verify(r => r.UpSert(It.Is<Product>(p =>
                p.ProductId == 0 &&
                p.ProductName == "Gaming Mouse" &&
                p.ProductCategoryId == 1 &&
                p.ProductPrice == 50m &&
                p.ProductCostPrice == 20m &&
                p.Quantity == 100 &&
                p.SupplierId == 2
            )), Times.Once);

            mockView.Verify(v => v.ShowMessage("Product saved successfully."), Times.Once);
        }

        [Fact]
        public void SaveProduct_ShouldShowErrorMessage_WhenDatabaseFails()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockView = new Mock<IProductView>();

            mockView.Setup(v => v.ViewProductName).Returns("name");
            mockView.Setup(v => v.ProductId).Returns(1);
            mockView.Setup(v => v.CategoryId).Returns(1);
            mockView.Setup(v => v.ProductPrice).Returns(1);

            mockRepo.Setup(r => r.UpSert(It.IsAny<Product>())).Throws(new Exception("Database timeout"));

            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);

            presenter.SaveProduct();

            mockView.Verify(v => v.ShowMessage("Database timeout"), Times.Once);
        }

        [Fact]
        public void MakeTransaction_WhenSoldQtyIsZeroAndOtherValuesAreCorrect_ShowErrorMessageAndDoseNotSave()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockView = new Mock<IProductView>();

            mockView.Setup(v => v.ProductId).Returns(5);
            mockView.Setup(v => v.TransactionType).Returns("IN");
            mockView.Setup(v => v.SoldQty).Returns(0);

            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);

            presenter.MakeTransaction();

            mockView.Verify(v => v.ShowMessage("Sold quantity must be greater than zero."), Times.Once);
            mockRepo.Verify(v => v.MakeTransaction(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>()
                ), Times.Never);
        }

        [Fact]
        public void MakeTransaction_WhenOUTUsingValidvalues_ReturnCorrectValues()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockView = new Mock<IProductView>();

            mockView.Setup(v => v.ProductId).Returns(5);
            mockView.Setup(v => v.ProductPrice).Returns(20);
            mockView.Setup(v => v.TransactionType).Returns("OUT");
            mockView.Setup(v => v.SoldQty).Returns(5);

            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);

            presenter.MakeTransaction();

            mockView.Verify(v => v.ShowMessage("Transaction logged successfully."), Times.Once);
            mockRepo.Verify(v => v.MakeTransaction(
                It.IsAny<int>(),
                It.IsAny<int>(),
                "OUT",
                -5,
                100,
                It.IsAny<decimal>()
                ), Times.Once);
        }

        [Fact]
        public void MakeTransaction_WhenINUsingValidvalues_ReturnCorrectValues()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockView = new Mock<IProductView>();

            mockView.Setup(v => v.CostPrice).Returns(5);
            mockView.Setup(v => v.TransactionType).Returns("IN");
            mockView.Setup(v => v.SoldQty).Returns(10);
            mockView.Setup(v => v.ProductId).Returns(1);

            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);

            presenter.MakeTransaction();

            mockView.Verify(v => v.ShowMessage("Transaction logged successfully."), Times.Once);
            mockRepo.Verify(v => v.MakeTransaction(
                It.IsAny<int>(),
                It.IsAny<int>(),
                "IN",
                10,
                50,
                It.IsAny<decimal>()
                ), Times.Once);
        }

        [Fact]
        public void UpdateCalculations_WhenAllFieldsAreEmpty()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockView = new Mock<IProductView>();

            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);

            var emptyList = new List<Product>();

            presenter.UpdateCalculations(emptyList);

            mockView.VerifySet(v => v.TotalItemsText = "Total Products: 0");
            mockView.VerifySet(v => v.TotalValueText = "Total Value: $0.00");
            mockView.VerifySet(v => v.TotalProfitText = "Total Potential Profit: $0.00");
            mockView.VerifySet(v => v.LowStockText = "Low Stock Alerts: 0");
        }

        [Fact]
        public void UpdateCalculations_WhenAllFieldsArecorrect()
        {
            var Products = new List<Product>
            {
                new Product{ ProductCostPrice = 15.50m, Quantity = 35, ProductPrice = 55.5m },
                new Product{ ProductCostPrice = 10.99m, Quantity = 3, ProductPrice = 39.99m },
                new Product{ ProductCostPrice = 5.75m, Quantity = 25, ProductPrice = 14.99m }
            };

            var mockRepo = new Mock<IProductRepository>();
            var mockView = new Mock<IProductView>();

            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);

            presenter.UpdateCalculations(Products);


            mockView.VerifySet(v => v.TotalItemsText = "Total Products: 3");
            mockView.VerifySet(v => v.TotalValueText = It.Is<String>(s => s.Contains("2,437.22")));
            mockView.VerifySet(v => v.TotalProfitText = It.Is<String>(s => s.Contains("1,718.00")));
            mockView.VerifySet(v => v.LowStockText = "Low Stock Alerts: 1");
        }

        [Fact]
        public void Delete_WhenDeletionWasCanceled_RepositoryNeverGetsCalled()
        {
            var mockView = new Mock<IProductView>();
            var mockRepo = new Mock<IProductRepository>();

            mockView.Setup(v => v.ConfirmDelete(It.IsAny<String>())).Returns(false);


            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);

            presenter.DeleteProduct(1, "Test Product");


            mockRepo.Verify(v => v.Delete(1), Times.Never);
        }

        [Fact]
        public void Delete_WhenDeletionWasConfirmed_RepositoryGetsCalledAndRefreshData()
        {
            var mockView = new Mock<IProductView>();
            var mockRepo = new Mock<IProductRepository>();

            mockView.Setup(v => v.ConfirmDelete(It.IsAny<String>())).Returns(true);


            var presenter = new ProductPresenter(mockRepo.Object);
            presenter.SetView(mockView.Object);

            presenter.DeleteProduct(1, "Test Product");


            mockRepo.Verify(v => v.Delete(It.IsAny<int>()), Times.Once);
            mockRepo.Verify(v => v.GetAll(), Times.Once);
        }

    }
}
