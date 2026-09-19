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
        [Authorize]
        public async Task<IActionResult> OpenShift([FromBody] OpenShiftDto dto)
        {
            var shift = await _posService.OpenShiftAsync(User.GetCurrentUserId(), dto);
            return Ok(new { message = "Shift opened successfully.", shiftId = shift.ShiftId });
        }

        [HttpPost("shift/{shiftId}/close")]
        [Authorize]
        public async Task<IActionResult> CloseShift(int shiftId, [FromBody] CloseShiftDto dto)
        {
            var shift = await _posService.CloseShiftAsync(shiftId, User.GetCurrentUserId(), dto);
            return Ok(new { message = "Shift closed successfully.", variance = shift.ActualEndingCash - shift.ExpectedEndingCash });
        }

        [HttpPost("shift/{shiftId}/cash-movement")]
        [Authorize]
        public async Task<IActionResult> AddCashMovement(int shiftId, [FromBody] CashMovementDto dto)
        {
            await _posService.AddCashMovementAsync(shiftId, User.GetCurrentUserId(), dto);
            return Ok(new { message = "Cash movement recorded." });
        }

        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto dto)
        {
            var receipt = await _posService.ProcessCheckoutAsync(User.GetCurrentUserId(), dto);
            return Ok(new { message = "Checkout successful.", receiptNumber = receipt.ReceiptNumber, total = receipt.FinalTotal });
        }

        [HttpPost("verify-pin")]
        [Authorize]
        public async Task<IActionResult> VerifyPosPin([FromBody] VerifyPinDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Pin) || dto.Pin.Length != 4)
                return BadRequest(new { message = "PIN must be exactly 4 digits." });

            int currentUserId = User.GetCurrentUserId();

            bool isValid = await _userRepository.VerifyPosPinAsync(currentUserId, dto.Pin);

            if (!isValid)
                return Unauthorized(new { message = "Invalid POS PIN." });

            return Ok(new { message = "PIN verified. POS unlocked." });
        }

        [HttpGet("shift/current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentShift([FromQuery] int storeId)
        {
            var shift = await _posService.GetCurrentShiftAsync(User.GetCurrentUserId(), storeId);

            if (shift == null)
                return Ok(new { hasOpenShift = false, shift = (object?)null });

            return Ok(new { hasOpenShift = true, shift });
        }

        [HttpGet("shift/history")]
        [Authorize]
        public async Task<IActionResult> GetShiftHistory([FromQuery] int storeId, [FromQuery] int take = 20)
        {
            if (take > 50) take = 50;
            var history = await _posService.GetShiftHistoryAsync(storeId, take);
            return Ok(history);
        }

        [HttpGet("shift/{shiftId}/receipts")]
        [Authorize]
        public async Task<IActionResult> GetShiftReceipts(int shiftId)
        {
            var receipts = await _posService.GetShiftReceiptsAsync(shiftId, User.GetCurrentUserId());
            return Ok(receipts);
        }

        [HttpGet("receipts/{receiptId}")]
        [Authorize]
        public async Task<IActionResult> GetReceiptDetail(int receiptId)
        {
            var detail = await _posService.GetReceiptDetailAsync(receiptId, User.GetCurrentUserId());
            return Ok(detail);
        }

        [HttpGet("switchable-users")]
        [Authorize]
        public async Task<IActionResult> GetSwitchableUsers()
        {
            var users = await _userRepository.GetLookupUsersAsync();
            return Ok(users);
        }

    }
}
