using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task ProcessTransaction(InventoryTransaction transaction, int userId, string transactionType);
    }
}
