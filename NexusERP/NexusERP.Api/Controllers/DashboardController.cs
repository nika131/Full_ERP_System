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
                var stats = _Productrepository.GetDashboardAggregates();
                return Ok(stats);
        }

        [HttpGet("revenueChart")]
        public IActionResult GetChartData()
        {
                var chartData = _ReportRepository.GetWeeklyRevenueChart();
                return Ok(chartData);
        }

        [HttpGet("top-Products")]
        public IActionResult GetTopProducts()
        {
            var data = _ReportRepository.GetTopPerformingProducts();
            return Ok(data);
        }
    }
}
