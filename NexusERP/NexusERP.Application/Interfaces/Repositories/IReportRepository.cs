using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IReportRepository
    {
        PagedResult<InventoryTransaction> GetPagedTransactions(int pageNumber, int pageSize, string? searchTerm, int currentUserId, string currentUserRole, string typeFilter);
        InventoryTransaction? GetById(int transactionId);
    }
}
