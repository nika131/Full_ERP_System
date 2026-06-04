using DocumentFormat.OpenXml.Wordprocessing;
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

        private (int UserId, string Role) GetIdentity()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            return (int.TryParse(idClaim, out int id) ? id : 0, role);
        }


        [HttpGet]
        public IActionResult GetTransactions(
            [FromQuery] string? searchTerm = null, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string typeFilter = "All")
        {
            if (pageSize > 100) pageSize = 100;
            var identity = GetIdentity();

            var secureData = _repository.GetPagedTransactions(pageNumber, pageSize, searchTerm, identity.UserId, identity.Role, typeFilter);
            return Ok(secureData);
        }

        [HttpGet("export/excel")]
        public IActionResult ExportExcel([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? keyword, [FromQuery] string typeFilter = "All")
        {
            var identity = GetIdentity();

            var data = _repository.GetPagedTransactions(pageNumber, pageSize, keyword, identity.UserId, identity.Role, typeFilter).Items;
            
            byte[] fileContents = _excelService.ExcelTransactions(data, "Transactions");
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InventoryReport.xlsx");
        }

        [HttpGet("export/pdf/{transactionId}")]
        public IActionResult ExportPdf(int transactionId)
        {
            var identity = GetIdentity();
            var transaction = _repository.GetById(transactionId);

            if (transaction == null) return NotFound("Transaction not found.");

            if (identity.Role == "Admin")
            {
                
            }
            else if (identity.Role == "Manager")
            {
                 if (transaction.UserId != identity.UserId)
                {
                    var targetUser = _userRepository.GetAllUsers().FirstOrDefault(u => u.UserId == transaction.UserId);
                    if (targetUser == null || targetUser.Role != UserRole.Cashier)
                    {
                        return Forbid();
                    }
                }
            }
            else if (identity.Role == "Cashier")
            {
                if (transaction.UserId != identity.UserId || transaction.TransactionType != TransactionAction.Sale)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            byte[] fileContents = _pdfService.GenerateInvoice(transaction);
            return File(fileContents, "application/pdf", $"Invoice_{transactionId}.pdf");
        }
    }
}
