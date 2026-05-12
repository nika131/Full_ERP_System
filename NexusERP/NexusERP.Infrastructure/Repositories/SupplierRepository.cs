using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        public IEnumerable<Supplier> GetAllSuppliers()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetSuppliers");
            var suppliers = new List<Supplier>();

            foreach (DataRow row in dt.Rows)
            {
                suppliers.Add(new Supplier
                {
                    SupplierId = Convert.ToInt32(row["SupplierId"]),
                    CompanyName = row["CompanyName"] == DBNull.Value ? string.Empty : row["CompanyName"].ToString()!,
                    ContactName = row["ContactName"] == DBNull.Value ? string.Empty : row["ContactName"].ToString()!,
                    Phone = row["Phone"] == DBNull.Value ? string.Empty : row["Phone"].ToString()!,
                    Email = row["Email"] == DBNull.Value ? string.Empty : row["Email"].ToString()!
                });
            }
            return suppliers;
        }

        public IEnumerable<Supplier> SearchSuppliers(string Keyword)
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_SearchSuppliers", new Dictionary<string, object> { { "@Keyword", Keyword } });
            var suppliers = new List<Supplier>();

            foreach (DataRow row in dt.Rows)
            {
                suppliers.Add(new Supplier
                {
                    SupplierId = Convert.ToInt32(row["SupplierId"]),
                    CompanyName = row["CompanyName"] == DBNull.Value ? string.Empty : row["CompanyName"].ToString()!,
                    ContactName = row["ContactName"] == DBNull.Value ? string.Empty : row["ContactName"].ToString()!,
                    Phone = row["Phone"] == DBNull.Value ? string.Empty : row["Phone"].ToString()!,
                    Email = row["Email"] == DBNull.Value ? string.Empty : row["Email"].ToString()!
                });
            }
            return suppliers;
        }
            

        public void UpsertSuppliers(Supplier supplier)
        {
            var args = new Dictionary<string, object>
            {
                { "@supplierId", supplier.SupplierId },
                { "@companyName", supplier.CompanyName },
                { "@contactName", supplier.ContactName },
                { "@phone", supplier.Phone },
                { "@email", supplier.Email }
            };
            DatabaseHelper.ExecuteNonQuery("sp_UsertSupplier", args);
        }

        public void DeleteSupplier(int id)
        {
            DatabaseHelper.ExecuteNonQuery("sp_DeleteSupplier", new Dictionary<string, object> { { "@supplierId", id } });
        }
    }
}
