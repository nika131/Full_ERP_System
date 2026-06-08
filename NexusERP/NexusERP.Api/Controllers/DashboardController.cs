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
        private readonly IProductRepository _Productrepository;
        private readonly IReportRepository _ReportRepository;

        public DashboardController(IProductRepository productRepository, IReportRepository reportRepository)
        {
            _Productrepository = productRepository;
            _ReportRepository = reportRepository;
        }

        
        [HttpGet("statistics")]
        public IActionResult GetDashboardStatistics()
        {
            try
            {
                var stats = _Productrepository.GetDashboardAggregates();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calculating dashboard statistics."});
            }
        }

        [HttpGet("revenueChart")]
        public IActionResult GetChartData()
        {
            try
            {
                var chartData = _ReportRepository.GetWeeklyRevenueChart();
                return Ok(chartData);
            } 
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading chart data." });
            }
        }

        [HttpGet("top-Products")]
        public IActionResult GettopProducts()
        {
            try
            {
                var data = _ReportRepository.GetTopPerformingProducts();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading top Products." });
            }
        }
    }
}
