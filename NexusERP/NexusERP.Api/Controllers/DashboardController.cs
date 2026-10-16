using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireViewDashboard")]
    public class DashboardController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IReportRepository _reportRepository;

        public DashboardController(IProductRepository productRepository, IReportRepository reportRepository)
        {
            _productRepository = productRepository;
            _reportRepository = reportRepository;
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetDashboardStatistics([FromQuery] DashboardFilterRequest request)
        {
            var stats = await _productRepository.GetDashboardAggregates(User.GetCurrentCompanyId(), request);

            return Ok(stats);
        }

        [HttpGet("revenueChart")]
        public async Task<IActionResult> GetChartData([FromQuery] DashboardFilterRequest request)
        {
            var chartData = await _reportRepository.GetWeeklyRevenueChart(User.GetCurrentCompanyId(), request);

            return Ok(chartData);
        }

        [HttpGet("top-Products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] DashboardFilterRequest request)
        {
            var data = await _reportRepository.GetTopPerformingProducts(User.GetCurrentCompanyId(), request);

            return Ok(data);
        }
    }
}