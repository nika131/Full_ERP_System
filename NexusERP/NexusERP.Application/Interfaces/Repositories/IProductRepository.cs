using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAll();
        IEnumerable<Product> Search(string keyword);
        void UpSert(Product products);
        void LogInventoryTransaction(int productId, int? SupplierId, int UserId, string transactionType, int quantity, decimal unitPrice, decimal totalAmount, decimal profit);
        void LogSystemAudit(int userId, string entityType, int entityId, string action, string chnagesMade);
        void Delete(int id);
        IEnumerable<Category> GetCategories();
        IEnumerable<Supplier> GetSuppliers();
    }
}
