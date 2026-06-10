using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using System.ComponentModel;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireViewDashboard")]
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
        public async Task<IActionResult> GetDashboardStatistics()
        {
            var stats = await _Productrepository.GetDashboardAggregates();
            return Ok(stats);
        }

        [HttpGet("revenueChart")]
        public async Task<IActionResult> GetChartData()
        {
            var chartData = await _ReportRepository.GetWeeklyRevenueChart();
            return Ok(chartData);
        }

        [HttpGet("top-Products")]
        public async Task<IActionResult> GetTopProducts()
        {
            var data = await _ReportRepository.GetTopPerformingProducts();
            return Ok(data);
        }
    }
}
