using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Constants
{
    public class Permissions
    {
        // Products
        public const string ViewProducts = "Products.View";
        public const string UpsertProducts = "Products.Upsert";
        public const string DeleteProducts = "Products.Delete";

        // Categories & Suppliers
        public const string ManageCategories = "Categories.Manage";
        public const string ManageSuppliers = "Suppliers.Manage";

        // Transactions 
        public const string PerformSales = "Transactions.PerformSale";
        public const string PerformInboundTransactions = "Transactions.PerformInbound";

        // Reports
        public const string ViewOwnTransactions = "Reports.ViewOwn";
        public const string ViewAllTransactions = "Reports.ViewAll";
        public const string ExportExcelTransactions = "Reports.ExportExcel";

        // Admin
        public const string ManageUsers = "Users.Manage";
        public const string ViewAuditLogs = "AuditLogs.View";
        public const string ViewDashboard = "Dashboard.View";

        public const string ManageAbsences = "Absences.Manage";
    }
}
