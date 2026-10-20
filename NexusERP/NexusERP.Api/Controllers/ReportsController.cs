using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IExcelExportService _excelService;
        private readonly IPdfExportService _pdfService;

        public ReportsController(IReportRepository repository, IUserRepository userRepository, IExcelExportService excelService, IPdfExportService pdfService)
        {
            _repository = repository;
            _userRepository = userRepository;
            _excelService = excelService;
            _pdfService = pdfService;
        }

        [HttpGet]
        [Authorize(Policy = "RequireReadTransactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int pageSize = 50, [FromQuery] DateTime? lastCreatedAt = null, [FromQuery] int? lastTransactionId = null, [FromQuery] int? productId = null, [FromQuery] int? supplierId = null, [FromQuery] int? storeId = null, [FromQuery] int? categoryId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? searchTerm = null, [FromQuery] string typeFilter = "All")
        {
            if (pageSize > 100) pageSize = 100;

            bool canViewAll = User.HasPermission(Permissions.ViewAllTransactions);

            var secureData = await _repository.GetPagedTransactionsOptimized(
                User.GetCurrentCompanyId(),
                pageSize,
                User.GetCurrentUserId(),
                canViewAll,
                typeFilter,
                searchTerm,
                lastCreatedAt,
                lastTransactionId,
                productId,
                supplierId,
                storeId,
                categoryId,
                startDate,
                endDate
            );

            var responseItems = secureData.Items.Select(t => new TransactionResponseDto
            {
                TransactionId = t.TransactionId,
                ProductId = t.ProductId,
                ProductName = t.Product?.Name ?? "Unknown",
                SupplierName = t.Product?.Supplier?.ContactName ?? "N/A",
                TransactionType = t.TransactionType.ToString(),
                Quantity = t.Quantity,
                TotalAmount = 0,
                Profit = 0,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(new
            {
                items = responseItems,
                nextCreatedAt = secureData.NextCreatedAt,
                nextTransactionId = secureData.NextId,
                pageSize = secureData.PageSize,
                hasMorePages = secureData.HasMorePages
            });
        }

        [HttpGet("shifts")]
        [Authorize(Policy = Permissions.ViewShifts)]
        public async Task<IActionResult> GetShifts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? storeId = null)
        {
            if (pageSize > 100) pageSize = 100;

            return Ok(await _repository.GetPagedShiftsAsync(User.GetCurrentCompanyId(), page, pageSize, storeId));
        }

        [HttpGet("receipts")]
        [Authorize(Policy = Permissions.ViewReceipts)]
        public async Task<IActionResult> GetReceipts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;

            return Ok(await _repository.GetPagedReceiptsAsync(User.GetCurrentCompanyId(), page, pageSize, search));
        }
    }
}