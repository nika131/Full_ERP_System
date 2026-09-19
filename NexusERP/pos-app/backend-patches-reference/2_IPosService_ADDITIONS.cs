// ============================================================================
// ADD these method signatures into: NexusERP.Application/Interfaces/Services/IPosService.cs
// (inside the existing `IPosService` interface)
// ============================================================================

Task<CurrentShiftDto?> GetCurrentShiftAsync(int userId, int storeId);
Task<List<ShiftHistoryItemDto>> GetShiftHistoryAsync(int storeId, int take = 20);
Task<List<ShiftReceiptSummaryDto>> GetShiftReceiptsAsync(int shiftId, int userId);
Task<ReceiptDetailDto> GetReceiptDetailAsync(int receiptId, int userId);
