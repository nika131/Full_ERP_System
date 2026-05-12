using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        IEnumerable<Supplier> GetAllSuppliers();
        IEnumerable<Supplier> SearchSuppliers(string Keyword);
        void UpsertSuppliers(Supplier supplier);
        void DeleteSupplier(int id);

    }
}
