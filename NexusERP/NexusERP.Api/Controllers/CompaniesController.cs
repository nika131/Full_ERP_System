
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/companies")]
    [Authorize(AuthenticationSchemes = AccountJwtConstants.AuthenticationScheme)]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRegistrationService _companyRegistrationService;
        private readonly ICompanyAccessService _companyAccessService;

        public CompaniesController(ICompanyRegistrationService companyRegistrationService, ICompanyAccessService companyAccessService)
        {
            _companyRegistrationService = companyRegistrationService;
            _companyAccessService = companyAccessService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequestDto request)
        {
            if (!int.TryParse(User.FindFirst("account_id")?.Value, out int accountId))
                return Unauthorized(new { message = "Invalid account identity. Please log in again." });

            var result = await _companyRegistrationService.CreateCompanyAsync(accountId, request);

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Company created successfully.",
                companyId = result.CompanyId,
                userId = result.UserId,
                roleId = result.RoleId,
                joinCode = result.JoinCode
            });
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyCompanies()
        {
            if (!int.TryParse(User.FindFirst("account_id")?.Value, out int accountId))
                return Unauthorized(new { message = "Invalid account identity. Please log in again." });

            var companies = await _companyAccessService.GetMyCompaniesAsync(accountId);

            return Ok(new
            {
                message = "Companies retrieved successfully.",
                companies
            });
        }

        [HttpPost("{companyId:int}/select")]
        public async Task<IActionResult> SelectCompany(int companyId)
        {
            if (!int.TryParse(User.FindFirst("account_id")?.Value, out int accountId) ||
                !Guid.TryParse(User.FindFirst("session_id")?.Value, out Guid sessionId))
                return Unauthorized(new { message = "Invalid account session. Please log in again." });

            var result = await _companyAccessService.SelectCompanyAsync(accountId, sessionId, companyId);

            return Ok(result);
        }
    }
}
