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
        void MakeTransaction(int productId, int SupplierId, string transactionType, int qty, decimal Amount, decimal profit);
        void Delete(int id);
        IEnumerable<Category> GetCategories();
        IEnumerable<Supplier> GetSuppliers();
    }
}
