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
    public interface IProductRepository
    {
        PagedResult<Product> GetPaged(int pageNumber, int pageSize, string? searchTerm);
        void UpSert(Product products);
        void LogInventoryTransaction(int productId, int? SupplierId, int UserId, string transactionType, int quantity, decimal unitPrice, decimal totalAmount, decimal profit);
        void LogSystemAudit(int userId, string entityType, int entityId, string action, string chnagesMade);
        void Delete(int id);
        DashboardResponse GetDashboardAggregates();
        IEnumerable<Category> GetCategories();
        IEnumerable<Supplier> GetSuppliers();
    }
}
