using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.DTOs;
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
        public async Task<IActionResult> GetDashboardStatistics(
                                [FromQuery] DateTime? startDate,
                                [FromQuery] DateTime? endDate,
                                [FromQuery] int? storeId,
                                [FromQuery] int? categoryId,
                                [FromQuery] int? supplierId)
        {
            var stats = await _Productrepository.GetDashboardAggregates(startDate, endDate, storeId, categoryId, supplierId);
            return Ok(stats);
        }

        [HttpGet("revenueChart")]
        public async Task<IActionResult> GetChartData(
                                [FromQuery] DateTime? startDate,
                                [FromQuery] DateTime? endDate,
                                [FromQuery] int? storeId,
                                [FromQuery] int? categoryId,
                                [FromQuery] int? supplierId)
        {
            var chartData = await _ReportRepository.GetWeeklyRevenueChart(startDate, endDate, storeId, categoryId, supplierId);
            return Ok(chartData);
        }

        [HttpGet("top-Products")]
        public async Task<IActionResult> GetTopProducts(
                                [FromQuery] DateTime? startDate,
                                [FromQuery] DateTime? endDate,
                                [FromQuery] int? storeId,
                                [FromQuery] int? categoryId,
                                [FromQuery] int? supplierId)
        {
            var data = await _ReportRepository.GetTopPerformingProducts(startDate, endDate, storeId, categoryId, supplierId);
            return Ok(data);
        }
    }
}
