// ============================================================================
// ADD these actions into: NexusERP.Api.Controllers.PosController
// (inside the existing class, alongside OpenShift/CloseShift/etc.)
// ============================================================================

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
