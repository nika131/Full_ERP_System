using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<InventoryTransaction> GetAll()
        {
            var query = from t in _context.InventoryTransactions.AsNoTracking()

                        join p in _context.Products on t.ProductId equals p.ProductId into pJoin
                        from p in pJoin.DefaultIfEmpty()

                        join s in _context.Suppliers on t.SupplierId equals s.SupplierId into sJoin
                        from s in sJoin.DefaultIfEmpty()

                        orderby t.CreatedAt descending

                        select new InventoryTransaction
                        {
                            TransactionId = t.TransactionId,
                            ProductId = t.ProductId,
                            SupplierId = t.SupplierId,
                            UserId = t.UserId,
                            TransactionType = t.TransactionType,
                            Quantity = t.Quantity,
                            UnitPrice = t.UnitPrice,
                            TotalAmount = t.TotalAmount,
                            Profit = t.Profit,
                            CreatedAt = t.CreatedAt,

                            ProductName = p != null ? p.Name : string.Empty,
                            SupplierName = s != null ? s.CompanyName : string.Empty
                        };

            return query.ToList();
        }

        public IEnumerable<InventoryTransaction> Search(string keyword)
        {
            var baseQuery = from t in _context.InventoryTransactions.AsNoTracking()
                            join p in _context.Products on t.ProductId equals p.ProductId into pJoin
                            from p in pJoin.DefaultIfEmpty()
                            join s in _context.Suppliers on t.SupplierId equals s.SupplierId into sJoin
                            from s in sJoin.DefaultIfEmpty()
                            select new { t, p, s }; 


            var filteredQuery = from item in baseQuery
                                where (item.s != null && item.s.CompanyName.Contains(keyword)) ||
                                      (item.p != null && item.p.Name.Contains(keyword)) ||
                                      item.t.TransactionId.ToString().Contains(keyword)
                                orderby item.t.CreatedAt descending
                                select new InventoryTransaction
                                {
                                    TransactionId = item.t.TransactionId,
                                    ProductId = item.t.ProductId,
                                    SupplierId = item.t.SupplierId,
                                    UserId = item.t.UserId,
                                    TransactionType = item.t.TransactionType,
                                    Quantity = item.t.Quantity,
                                    UnitPrice = item.t.UnitPrice,
                                    TotalAmount = item.t.TotalAmount,
                                    Profit = item.t.Profit,
                                    CreatedAt = item.t.CreatedAt,
                                    ProductName = item.p != null ? item.p.Name : string.Empty,
                                    SupplierName = item.s != null ? item.s.CompanyName : string.Empty
                                };

            return filteredQuery.ToList();
        }
    }
}
