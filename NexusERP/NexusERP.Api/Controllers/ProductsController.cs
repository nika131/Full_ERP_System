using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using System.Security.Claims;
using System.Transactions;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("Api/[Controller]")]
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repository;
        
        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        [HttpGet]
        public IActionResult GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageSize > 100) pageSize = 100;

            var result = _repository.GetPaged(page, pageSize, searchTerm);
            
            var responseItems = result.Items.Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "Uncategorized",
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier?.ContactName ?? "No Supplier",
                Quantity = p.Quantity,
                Price = p.Price,
                CostPrice = p.CostPrice
            }).ToList();

            return Ok(new
            {
                items = responseItems,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
            });
        }

        [HttpPost("upsert")]
        public IActionResult SaveProduct([FromBody] ProductUpsertDto dto)
        {
            if (GetCurrentUserRole() == "Cashier") return Forbid();

            var product = new Product
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                SupplierId = dto.SupplierId
            };

            _repository.UpSert(product, GetCurrentUserId());

            return Ok(new { message = "Product saved successfully." });
        }

        [HttpPost("transaction")]
        public IActionResult MakeTransaction([FromBody] TransactionRequestDto dto)
        {
            if (GetCurrentUserRole() == "Cashier" && dto.TransactionType != "Sale")
            {
                return Forbid("Security Violation: Cashier are restricted to outbound sales only.");
            }

            decimal unitPrice = dto.TransactionType == "Sale" ? dto.ProductPrice : dto.CostPrice;
            decimal totalAmount = unitPrice * dto.SoldQty;
            decimal profit = dto.TransactionType == "Sale" ? totalAmount - (dto.CostPrice * dto.SoldQty) : 0;

            var transactionEntity = new InventoryTransaction
            {
                ProductId = dto.ProductId,
                SupplierId = dto.SupplierId > 0 ? dto.SupplierId : null,
                TransactionType = Enum.Parse<TransactionAction>(dto.TransactionType),
                Quantity = dto.SoldQty,
                UnitPrice = unitPrice,
                TotalAmount = totalAmount,
                Profit = profit
            };

            _repository.LogInventoryTransaction(transactionEntity, GetCurrentUserId(), dto.TransactionType);

            return Ok(new { message = $"Transaction ({dto.TransactionType}) logged successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            if (GetCurrentUserRole() == "Cashier")
            {
                return Forbid("Security Violation: Cashier cannot delete products.");
            }

            _repository.Delete(id);
            return Ok(new { message = "Product deleted successfully." });
        }
    }
}
