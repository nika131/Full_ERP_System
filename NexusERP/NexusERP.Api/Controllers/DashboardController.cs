using DocumentFormat.OpenXml.Office2016.Excel;
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
        public async Task<IActionResult> GetDashboardStatistics([FromQuery] DashboardFilterRequest request)
        {
            var stats = await _Productrepository.GetDashboardAggregates(request);
            return Ok(stats);
        }

        [HttpGet("revenueChart")]
        public async Task<IActionResult> GetChartData([FromQuery] DashboardFilterRequest request)
        {
            var chartData = await _ReportRepository.GetWeeklyRevenueChart(request);
            return Ok(chartData);
        }

        [HttpGet("top-Products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] DashboardFilterRequest request)
        {
            var data = await _ReportRepository.GetTopPerformingProducts(request);
            return Ok(data);
        }
    }
}
