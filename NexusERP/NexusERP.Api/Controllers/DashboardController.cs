using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using System.ComponentModel;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IProductRepository _repository;

        public DashboardController(IProductRepository repository)
        {
            _repository = repository;
        }

        /*
        [HttpGet("statistics")]
        public IActionResult GetDashboardStatistics()
        {
            try
            {
                var products = _repository.GetPaged();

                decimal totalValue = 0;
                decimal totalProfit = 0;
                decimal totalCost = 0;
                int lowStockCount = 0;

                foreach (var product in products)
                {
                    decimal price = product.Price;
                    decimal cost = product.CostPrice;
                    int qty = product.Quantity;

                    totalValue += (price * qty);
                    totalCost += (cost * qty);
                    totalProfit += (price - cost) * qty;

                    if (qty < 5)
                    {
                        lowStockCount++;
                    }

                }

                decimal margin = totalValue > 0 ? (totalProfit / totalValue) * 100 : 0;

                string inventoryHealth;
                if (margin > 30 && lowStockCount == 0)
                {
                    inventoryHealth = "EXCELLENT";
                }
                else if (lowStockCount > 0)
                {
                    inventoryHealth = "ACTION REQUIRED";
                }
                else
                {
                    inventoryHealth = "STABLE";
                }

                var response = new DashboardResponseDto
                {
                    TotalValue = totalValue,
                    TotalCost = totalCost,
                    TotalProfit = totalProfit,
                    LowStockCount = lowStockCount,
                    MarginPrecentage = margin,
                    InventoryHealth = inventoryHealth,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calculating dashboard statistics: " + ex.Message });
            }
        }*/
    }
}
