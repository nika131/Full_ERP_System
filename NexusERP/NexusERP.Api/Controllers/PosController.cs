using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PosController : Controller
    {
        private readonly IPosService _posService;
        private readonly IUserRepository _userRepository;

        public PosController(IPosService posService, IUserRepository userRepository)
        {
            _posService = posService;
            _userRepository = userRepository;
        }

        [HttpPost("shift/open")]
        public async Task<IActionResult> OpenShift([FromBody] OpenShiftDto dto)
        {
            var shift = await _posService.OpenShiftAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId(), dto);

            return Ok(new
            {
                message = "Shift opened successfully.",
                shiftId = shift.ShiftId
            });
        }

        [HttpPost("shift/{shiftId}/close")]
        public async Task<IActionResult> CloseShift(int shiftId, [FromBody] CloseShiftDto dto)
        {
            var shift = await _posService.CloseShiftAsync(User.GetCurrentCompanyId(), shiftId, User.GetCurrentUserId(), dto);

            return Ok(new
            {
                message = "Shift closed successfully.",
                variance = shift.ActualEndingCash - shift.ExpectedEndingCash
            });
        }

        [HttpPost("shift/{shiftId}/cash-movement")]
        public async Task<IActionResult> AddCashMovement(int shiftId, [FromBody] CashMovementDto dto)
        {
            await _posService.AddCashMovementAsync(User.GetCurrentCompanyId(), shiftId, User.GetCurrentUserId(), dto);

            return Ok(new { message = "Cash movement recorded." });
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto dto)
        {
            var receipt = await _posService.ProcessCheckoutAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId(), dto);

            return Ok(new
            {
                message = "Checkout successful.",
                receiptNumber = receipt.ReceiptNumber,
                total = receipt.FinalTotal
            });
        }

        [HttpPost("verify-pin")]
        public async Task<IActionResult> VerifyPosPin([FromBody] VerifyPinDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Pin) || dto.Pin.Length != 4)
                return BadRequest(new { message = "PIN must be exactly 4 digits." });

            int companyId = User.GetCurrentCompanyId();
            int currentUserId = User.GetCurrentUserId();

            bool isValid = await _userRepository.VerifyPosPinAsync(companyId, currentUserId, dto.Pin);

            if (!isValid)
                return Unauthorized(new { message = "Invalid POS PIN." });

            return Ok(new { message = "PIN verified. POS unlocked." });
        }

        [HttpGet("shift/current")]
        public async Task<IActionResult> GetCurrentShift([FromQuery] int storeId)
        {
            var shift = await _posService.GetCurrentShiftAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId(), storeId);

            if (shift == null)
                return Ok(new { hasOpenShift = false, shift = (object?)null });

            return Ok(new
            {
                hasOpenShift = true,
                shift
            });
        }

        [HttpGet("shift/history")]
        public async Task<IActionResult> GetShiftHistory([FromQuery] int storeId)
        {
            var history = await _posService.GetShiftHistoryAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId(), storeId);

            return Ok(history);
        }

        [HttpGet("shift/{shiftId}/receipts")]
        public async Task<IActionResult> GetShiftReceipts(int shiftId)
        {
            var receipts = await _posService.GetShiftReceiptsAsync(User.GetCurrentCompanyId(), shiftId, User.GetCurrentUserId());

            return Ok(receipts);
        }

        [HttpGet("receipts/{receiptId}")]
        public async Task<IActionResult> GetReceiptDetail(int receiptId)
        {
            var detail = await _posService.GetReceiptDetailAsync(User.GetCurrentCompanyId(), receiptId, User.GetCurrentUserId());

            return Ok(detail);
        }

        [HttpGet("switchable-users")]
        public async Task<IActionResult> GetSwitchableUsers()
        {
            var users = await _userRepository.GetLookupUsersAsync(User.GetCurrentCompanyId());

            return Ok(users);
        }

        [HttpPost("checkout/quote")]
        public async Task<IActionResult> GetCheckoutQuote([FromBody] CheckoutRequestDto dto)
        {
            var quote = await _posService.GetCheckoutQuoteAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId(), dto);

            return Ok(quote);
        }

        [HttpGet("layout")]
        public async Task<IActionResult> GetPosLayout([FromQuery] int storeId)
        {
            var layout = await _posService.GetPosLayoutAsync(User.GetCurrentCompanyId(), storeId);

            return Ok(layout);
        }

        [HttpPut("layout/{storeId:int}")]
        public async Task<IActionResult> SavePosLayout(int storeId, [FromBody] UpdatePosLayoutDto dto)
        {
            var layout = await _posService.SavePosLayoutAsync(User.GetCurrentCompanyId(), storeId, dto);

            return Ok(layout);
        }
    }
}