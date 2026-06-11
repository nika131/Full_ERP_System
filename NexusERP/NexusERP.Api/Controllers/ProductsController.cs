using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Constants;
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

        [HttpGet]
        public async Task<IActionResult> GetProducts(
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 10,
                    [FromQuery] string? searchTerm = null)
        {
            if (pageSize > 100) pageSize = 100;

            var result = await _repository.GetPaged(page, pageSize, searchTerm);

            var responseItems = result.Items.Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "Uncategorized",
                SupplierId = p.SupplierId,
                CompanyName = p.Supplier?.CompanyName ?? "No Supplier",
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
        [Authorize(Policy = "RequireProductUpsert")]
        public async Task<IActionResult> SaveProduct([FromBody] ProductUpsertDto dto)
        {
            var product = new Product
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                SupplierId = dto.SupplierId
            };

            await _repository.Upsert(product, User.GetCurrentUserId());

            return Ok(new { message = "Product saved successfully." });
        }

        [HttpPost("transaction")]
        [Authorize]
        public async Task<IActionResult> MakeTransaction([FromBody] TransactionRequestDto dto)
        {
            if (dto.TransactionType != "Sale" && !User.HasPermission(Permissions.PerformSales))
            {
                return Forbid("Missing Perform Sales permission.");
            }

            if (dto.TransactionType != "Sale" && !User.HasPermission(Permissions.PerformInboundTransactions))
            {
                return Forbid("Missing Inbound Inventory permission.");
            }

            decimal unitPrice = dto.TransactionType == "Sale" ? dto.ProductPrice : dto.CostPrice;
            decimal totalAmount = unitPrice * dto.Quantity;
            decimal profit = dto.TransactionType == "Sale" ? totalAmount - (dto.CostPrice * dto.Quantity) : 0;

            var transactionEntity = new InventoryTransaction
            {
                ProductId = dto.ProductId,
                SupplierId = dto.SupplierId > 0 ? dto.SupplierId : null,
                TransactionType = Enum.Parse<TransactionAction>(dto.TransactionType),
                Quantity = dto.Quantity,
                UnitPrice = unitPrice,
                TotalAmount = totalAmount,
                Profit = profit
            };

            await _repository.LogInventoryTransaction(transactionEntity, User.GetCurrentUserId(), dto.TransactionType);

            return Ok(new { message = $"Transaction ({dto.TransactionType}) logged successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireProductDelete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _repository.Delete(id, User.GetCurrentUserId());
            return Ok(new { message = "Product deleted successfully." });
        }
    }
}
