
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

        public CompaniesController(ICompanyRegistrationService companyRegistrationService)
        {
            _companyRegistrationService = companyRegistrationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequestDto request)
        {
            string? accountIdClaim = User.FindFirst("account_id")?.Value;

            if (!int.TryParse(accountIdClaim, out int accountId))
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
    }
}
