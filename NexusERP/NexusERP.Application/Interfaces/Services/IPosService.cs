using NexusERP.Application.DTOs;
using NexusERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IPosService
    {
        Task<Shift> OpenShiftAsync(int userId, OpenShiftDto dto);
        Task<Shift> CloseShiftAsync(int shiftId, int userId, CloseShiftDto dto);
        Task<CashMovementResultDto> AddCashMovementAsync(int shiftId, int userId, CashMovementDto dto);
        Task<Receipt> ProcessCheckoutAsync(int userId, CheckoutRequestDto cart);
        Task<CurrentShiftDto?> GetCurrentShiftAsync(int userId, int storeId);
        Task<List<ShiftHistoryItemDto>> GetShiftHistoryAsync(int storeId, int take = 20);
        Task<List<ShiftReceiptSummaryDto>> GetShiftReceiptsAsync(int shiftId, int userId);
        Task<ReceiptDetailDto> GetReceiptDetailAsync(int receiptId, int userId);
        Task<CheckoutQuoteDto> GetCheckoutQuoteAsync(int UserId, CheckoutRequestDto cart);
    }
}
