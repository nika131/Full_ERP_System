
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Services;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/companies/join-code")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "RequireManageUsers")]
    public class CompanyJoinCodeController : ControllerBase
    {
        private readonly ICompanyJoinCodeService _service;

        public CompanyJoinCodeController(ICompanyJoinCodeService service)
        {
            _service = service;
        }

        [HttpPost("rotate")]
        public async Task<IActionResult> Rotate()
        {
            var result = await _service.RotateAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId());

            return Ok(result);
        }
    }
}
