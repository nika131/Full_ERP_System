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
        Task<CashMovement> AddCashMovementAsync(int shiftId, int userId, CashMovementDto dto);
        Task<Receipt> ProcessCheckoutAsync(int userId, CheckoutRequestDto cart);
    }
}
