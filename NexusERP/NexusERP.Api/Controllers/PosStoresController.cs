
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusERP.Api.Extensions;
using NexusERP.Domain.Constants;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/pos/stores")]
    [Authorize(Policy = Permissions.ViewPos)]
    public class PosStoresController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PosStoresController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetStores()
        {
            int companyId = User.GetCurrentCompanyId();

            var stores = await _db.Stores
                .AsNoTracking()
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.StoreId,
                    s.Name,
                    s.MaxCartDiscountPercentage
                })
                .ToListAsync();

            return Ok(stores);
        }
    }
}
