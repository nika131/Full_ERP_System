using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.DTOs;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
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
        private readonly IInventoryService _inventoryService;
        
        public ProductsController(IProductRepository repository, IInventoryService inventoryService)
        {
            _repository = repository;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 10,
                    [FromQuery] string? searchTerm = null,
                    [FromQuery] string? categoryName = null,
                    [FromQuery] string? supplierName = null,
                    [FromQuery] bool lowStockOnly = false)
        {
            if (pageSize > 100) pageSize = 100;

            var (result, totalValue) = await _repository.GetPaged(page, pageSize, searchTerm, categoryName, supplierName, lowStockOnly);

            var responseItems = result.Items.Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "Uncategorized",
                SupplierId = p.SupplierId,
                CompanyName = p.Supplier?.CompanyName ?? "No Supplier",
                Quantity = p.Quantity,
                LowStockThreshold = p.LowStockThreshold,
                Price = p.Price,
                CostPrice = p.CostPrice,
                VatRate = p.VatRate,
                MarketDiscountRate = p.MarketDiscountRate,
                MaxDiscountPercentage = p.MaxDiscountPercentage,
                Barcode = p.Barcode,
                ImageUrl = p.ImageUrl,
                ShapeType = p.ShapeType,
                ShapeColor = p.ShapeColor,
                ShapeText = p.ShapeText
            }).ToList();

            return Ok(new
            {
                items = responseItems,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalValue = totalValue
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
                SupplierId = dto.SupplierId,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                Quantity = dto.Quantity,
                LowStockThreshold = dto.LowStockThreshold,
                VatRate = dto.VatRate,
                MarketDiscountRate = dto.MarketDiscountRate,
                MaxDiscountPercentage = dto.MaxDiscountPercentage,
                Barcode = dto.Barcode,
                ImageUrl = dto.ImageUrl,
                ShapeType = dto.ShapeType,
                ShapeColor = dto.ShapeColor,
                ShapeText = dto.ShapeText
            }; 

            await _repository.Upsert(product, User.GetCurrentUserId());

            return Ok(new { message = "Product saved successfully." });
        }

        [HttpPost("transaction")]
        [Authorize]
        public async Task<IActionResult> MakeTransaction([FromBody] TransactionRequestDto dto)
        {
            if (!Enum.TryParse<TransactionAction>(dto.TransactionType, true, out var parsedAction))
            {
                return BadRequest(new { message = $"Invalid transaction type: '{dto.TransactionType}'." });
            }

            if (parsedAction == TransactionAction.Sale)
            {
                return BadRequest(new { message = "Sales must now be processed through the POS Checkout system to generate a valid Receipt." });
            }

            if (!User.HasPermission(Permissions.PerformInboundTransactions))
            {
                return StatusCode(403, "Missing Inbound Inventory permission.");
            }

            var transactionEntity = new InventoryTransaction
            {
                ProductId = dto.ProductId,
                TransactionType = parsedAction,
                Quantity = dto.Quantity
            };

            await _inventoryService.ProcessTransaction(transactionEntity, User.GetCurrentUserId(), dto.TransactionType);

            return Ok(new { message = $"Transaction ({dto.TransactionType}) logged successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireProductDelete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _repository.Delete(id, User.GetCurrentUserId());
            return Ok(new { message = "Product deleted successfully." });
        }

        [HttpGet("lowOnStock")]
        public async Task<IActionResult> GetProductsLowOnStock()
        {
            var products = await _repository.GetProductsLowOnStock();

            return Ok(new { products });
        }
    }
}
