using NexusERP.Application.DTOs;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IPosService
    {
        Task<Shift> OpenShiftAsync(int companyId, int userId, OpenShiftDto dto);
        Task<Shift> CloseShiftAsync(int companyId, int shiftId, int userId, CloseShiftDto dto);
        Task<CashMovementResultDto> AddCashMovementAsync(int companyId, int shiftId, int userId, CashMovementDto dto);
        Task<Receipt> ProcessCheckoutAsync(int companyId, int userId, CheckoutRequestDto cart);
        Task<CurrentShiftDto?> GetCurrentShiftAsync(int companyId, int userId, int storeId);
        Task<List<ShiftHistoryItemDto>> GetShiftHistoryAsync(int companyId, int userId, int storeId);
        Task<List<ShiftReceiptSummaryDto>> GetShiftReceiptsAsync(int companyId, int shiftId, int userId);
        Task<ReceiptDetailDto> GetReceiptDetailAsync(int companyId, int receiptId, int userId);
        Task<CheckoutQuoteDto> GetCheckoutQuoteAsync(int companyId, int userId, CheckoutRequestDto cart);
        Task<PosLayoutDto> GetPosLayoutAsync(int companyId, int storeId);
        Task<PosLayoutDto> SavePosLayoutAsync(int companyId, int storeId, UpdatePosLayoutDto layout);
    }
}