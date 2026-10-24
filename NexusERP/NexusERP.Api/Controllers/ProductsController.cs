using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;

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
        [Authorize(Policy = Permissions.ViewProducts)]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] string? categoryName = null, [FromQuery] string? supplierName = null, [FromQuery] bool lowStockOnly = false, [FromQuery] string? excludeIds = null)
        {
            if (pageSize > 100) pageSize = 100;

            List<int>? excludeList = null;

            if (!string.IsNullOrWhiteSpace(excludeIds))
            {
                excludeList = excludeIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();
            }

            var (result, totalValue) = await _repository.GetPaged(
                User.GetCurrentCompanyId(),
                page,
                pageSize,
                searchTerm,
                categoryName,
                supplierName,
                lowStockOnly,
                excludeList
            );

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
                ShapeText = p.ShapeText,
                DisplayMode = p.DisplayMode
            }).ToList();

            return Ok(new
            {
                items = responseItems,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalValue
            });
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> SaveProduct([FromBody] ProductUpsertDto dto)
        {
            var requiredPermission = dto.ProductId == 0 ? Permissions.CreateProducts : Permissions.EditProducts;
            if (!User.HasPermission(requiredPermission))
                return StatusCode(403, new { message = "You do not have permission to create or edit products." });

            var product = new Product
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                CategoryId = dto.CategoryId == 0 ? null : dto.CategoryId,
                SupplierId = dto.SupplierId == 0 ? null : dto.SupplierId,
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
                ShapeText = dto.ShapeText,
                DisplayMode = dto.DisplayMode
            };

            await _repository.Upsert(product, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Product saved successfully." });
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> MakeTransaction([FromBody] TransactionRequestDto dto)
        {
            if (!Enum.TryParse<TransactionAction>(dto.TransactionType, true, out var parsedAction))
                return BadRequest(new { message = $"Invalid transaction type: '{dto.TransactionType}'." });

            if (parsedAction == TransactionAction.Sale)
                return BadRequest(new { message = "Sales must now be processed through the POS Checkout system to generate a valid Receipt." });

            if (!User.HasPermission(Permissions.PerformInboundTransactions) && !User.HasPermission(Permissions.AdjustStock))
                return StatusCode(403, new { message = "You do not have permission to adjust inventory stock." });

            var transactionEntity = new InventoryTransaction
            {
                ProductId = dto.ProductId,
                TransactionType = parsedAction,
                Quantity = dto.Quantity
            };

            await _inventoryService.ProcessTransaction(
                transactionEntity,
                User.GetCurrentUserId(),
                User.GetCurrentCompanyId(),
                dto.TransactionType
            );

            return Ok(new { message = $"Transaction ({dto.TransactionType}) logged successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.DeleteProducts)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _repository.Delete(id, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Product deleted successfully." });
        }

        [HttpGet("lowOnStock")]
        [Authorize(Policy = Permissions.ViewProducts)]
        public async Task<IActionResult> GetProductsLowOnStock()
        {
            var products = await _repository.GetProductsLowOnStock(User.GetCurrentCompanyId());

            return Ok(new { products });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (!User.HasPermission(Permissions.CreateProducts) && !User.HasPermission(Permissions.EditProducts))
                return StatusCode(403, new { message = "You do not have permission to upload product images." });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Image size cannot exceed 5MB." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Invalid image format. Only JPG, PNG, and WEBP are allowed." });

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { url = $"/uploads/products/{fileName}" });
        }
    }
}