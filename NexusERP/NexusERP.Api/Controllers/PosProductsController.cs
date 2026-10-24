using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusERP.Api.Extensions;
using NexusERP.Domain.Constants;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/pos/products")]
    [Authorize(Policy = Permissions.ViewPos)]
    public class PosProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PosProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? searchTerm = null, [FromQuery] int pageSize = 30, [FromQuery] string? excludeIds = null)
        {
            int companyId = User.GetCurrentCompanyId();
            pageSize = Math.Clamp(pageSize, 1, 50);
            var excluded = (excludeIds ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out int id) ? id : 0).Where(id => id > 0).ToArray();

            var query = _db.Products.AsNoTracking().Where(p => p.CompanyId == companyId && p.IsActive && !excluded.Contains(p.ProductId));
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string search = searchTerm.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.ProductId.ToString() == search);
            }

            // Do not send purchase cost or profit data to cashiers.
            var items = await query.OrderBy(p => p.Name).Take(pageSize).Select(p => new
            {
                p.ProductId, p.Name, p.CategoryId,
                CategoryName = p.Category != null ? p.Category.CategoryName : "Uncategorized",
                p.SupplierId, CompanyName = p.Supplier != null ? p.Supplier.CompanyName : "",
                p.Quantity, p.LowStockThreshold, p.Price, CostPrice = 0m,
                p.VatRate, p.MarketDiscountRate, p.MaxDiscountPercentage,
                p.Barcode, p.ImageUrl, p.ShapeType, p.ShapeColor, p.ShapeText, p.DisplayMode
            }).ToListAsync();

            return Ok(new { items });
        }
    }
}
