using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using System.Security.Claims;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controler]")]
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _repository;
        private readonly IExcelExportService _excelService;
        private readonly IPdfExportService _pdfService;

        public ReportsController(IReportRepository repository, IExcelExportService excelService, IPdfExportService pdfService)
        {
            _repository = repository;
            _excelService = excelService;
            _pdfService = pdfService;
        }

        private int GetCurrentuserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private IEnumerable<InventoryTransaction> ApplyRbacRestrictions(IEnumerable<InventoryTransaction> rawData)
        {
            var dataList = rawData?.ToList() ?? new List<InventoryTransaction>();

            if (GetCurrentUserRole() == "Cashier")
            {
                int currentUserID = GetCurrentuserId();
                return dataList
                    .Where(t => t.TransactionType == Domain.Enums.TransactionAction.Sale && t.UserId == currentUserID)
                    .ToList();
            }

            return dataList;
        }

        [HttpGet]
        public IActionResult GetTransactions([FromQuery] string? keyword, [FromQuery] string typeFilter = "All")
        {
            try
            {
                var transactions = string.IsNullOrWhiteSpace(keyword)
                    ? _repository.GetAll()
                    : _repository.Search(keyword);

                var secureData = ApplyRbacRestrictions(transactions);

                if (typeFilter != "All" && Enum.TryParse(typeFilter, true, out TransactionAction action))
                {
                    secureData = secureData.Where(t => t.TransactionType == action).ToList();
                }

                return Ok(secureData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet("export/excel")]
        public IActionResult ExportExcel([FromQuery] string? keyword, [FromQuery] string typeFilter = "All")
        {
            try
            {
                var transactions = string.IsNullOrWhiteSpace(keyword)
                    ? _repository.GetAll()
                    : _repository.Search(keyword);

                var secureData = ApplyRbacRestrictions(transactions);

                if (typeFilter != "All" && Enum.TryParse(typeFilter, true, out TransactionAction action))
                {
                    secureData = secureData.Where(t => t.TransactionType == action).ToList();
                }

                byte[] fileContents = _excelService.ExcelTransactions(secureData, "Transactions");

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InventoryReport.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating Excel: " + ex.Message });
            }
        }

        [HttpGet("export/pdf/{transactionId}")]
        public IActionResult ExportPdf(int transactionId)
        {
            try
            {
                var transaction = _repository.GetAll().FirstOrDefault(t => t.TransactionId == transactionId);

                if (transaction == null) return NotFound("Transaction not found.");

                if (GetCurrentUserRole() == "Cashier" && transaction.UserId != GetCurrentuserId())
                {
                    return Forbid();
                }

                byte[] fileContents = _pdfService.GenerateInvoice(transaction);

                return File(fileContents, "application/pdf", $"Invoice_{transactionId}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF: " + ex.Message });
            }
        }
    }
}
