using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using System.Security.Claims;

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
        public IActionResult GetTransactions(
            [FromQuery] string? searchTerm = null, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string typeFilter = "All")
        {
            if (pageSize > 100) pageSize = 100;

            bool canViewAll = User.HasPermission(Permissions.ViewAllTransactions);

            var secureData = _repository.GetPagedTransactions(pageNumber, pageSize, searchTerm, User.GetCurrentUserId(), canViewAll, typeFilter);
            
            var responseItems = secureData.Items.Select( t => new TransactionResponseDto
            {
                TransactionId = t.TransactionId,
                ProductId = t.ProductId,
                ProductName = t.Product?.Name ?? "Unknown",
                SupplierName = t.Supplier?.ContactName ?? "N/A",
                TransactionType = t.TransactionType.ToString(),
                Quantity = t.Quantity,
                TotalAmount = t.TotalAmount,
                Profit = t.Profit,
                CreatedAt = t.CreatedAt,
            }).ToList();
            
            return Ok(new
            {
                items = responseItems,
                totalCount = secureData.TotalCount,
                pageNumber = secureData.PageNumber,
                pageSize = secureData.PageSize,
            });
        }

        [HttpGet("export/excel")]
        [Authorize(Policy = "RequireExportExcel")]
        public IActionResult ExportExcel([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? keyword, [FromQuery] string typeFilter = "All")
        {
            var data = _repository.GetPagedTransactions(pageNumber, pageSize, keyword, User.GetCurrentUserId(), true, typeFilter).Items;
            
            byte[] fileContents = _excelService.ExcelTransactions(data, "Transactions");
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InventoryReport.xlsx");
        }

        [HttpGet("export/pdf/{transactionId}")]
        public IActionResult ExportPdf(int transactionId)
        {
            var transaction = _repository.GetById(transactionId);
            if (transaction == null) return NotFound("Transaction not found.");

            bool canViewAll = User.HasPermission(Permissions.ViewAllTransactions);

            if (!canViewAll && transaction.UserId != User.GetCurrentUserId())
            {
                return Forbid("You do not have Permission to view others' invoices.");
            }

            byte[] fileContents = _pdfService.GenerateInvoice(transaction);
            return File(fileContents, "application/pdf", $"Invoice_{transactionId}.pdf");
        }
    }
}
