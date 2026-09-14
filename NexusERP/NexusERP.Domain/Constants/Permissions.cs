using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public const string ApplyItemDiscount = "Transactions.ApplyItemDiscount";
        public const string ApplyCartDiscount = "Transactions.ApplyCartDiscount";
        public const string editTransaction = "Transactions.Edit";
        public const string deleteTransaction = "Transactions.Delete";

        public const string ManageShifts = "Shifts.Manage";
        public const string OpenCloseShift = "Shifts.OpenClose";
        public const string PerformCashMovements = "Shifts.CashMovements";

        public static List<string> GetAllPermissions()
        {
            return typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!)
                .ToList();
        }
    }
}
