using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NexusERP.Domain.Constants
{
    public static class Permissions
    {
        // Fine-grained privileges. Keep values identical to web constants/permissions.ts.
        public const string ViewProducts = "Products.View";
        public const string CreateProducts = "Products.Create";
        public const string EditProducts = "Products.Edit";
        public const string DeleteProducts = "Products.Delete";
        public const string AdjustStock = "Products.AdjustStock";
        public const string ViewCategories = "Categories.View";
        public const string CreateCategories = "Categories.Create";
        public const string EditCategories = "Categories.Edit";
        public const string DeleteCategories = "Categories.Delete";
        public const string ViewSuppliers = "Suppliers.View";
        public const string CreateSuppliers = "Suppliers.Create";
        public const string EditSuppliers = "Suppliers.Edit";
        public const string DeleteSuppliers = "Suppliers.Delete";
        public const string ViewStores = "Stores.View";
        public const string CreateStores = "Stores.Create";
        public const string EditStores = "Stores.Edit";
        public const string ViewDashboard = "Dashboard.View";
        public const string ViewOwnTransactions = "Reports.ViewOwn";
        public const string ViewAllTransactions = "Reports.ViewAll";
        public const string ExportExcelTransactions = "Reports.ExportExcel";
        public const string ViewShifts = "Shifts.View";
        public const string ViewReceipts = "Receipts.View";
        public const string ViewUsers = "Users.View";
        public const string CreateUsers = "Users.Create";
        public const string EditUsers = "Users.Edit";
        public const string DeleteUsers = "Users.Delete";
        public const string ManageRoles = "Users.ManageRoles";
        public const string AssignRoles = "Users.AssignRoles";
        public const string ManageJoinRequests = "Users.ManageJoinRequests";
        public const string ManageJoinCodes = "Companies.ManageJoinCodes";
        public const string ViewSalary = "Salary.View";
        public const string ManageSalary = "Salary.Manage";
        public const string ViewAuditLogs = "AuditLogs.View";
        public const string ViewAbsences = "Absences.ViewOwn";
        public const string RequestAbsences = "Absences.Request";
        public const string ManageAbsences = "Absences.Manage";
        public const string ViewSettings = "Settings.View";
        public const string ManageSettings = "Settings.Manage";
        public const string PerformSales = "Transactions.PerformSale";
        public const string PerformInboundTransactions = "Transactions.PerformInbound";
        public const string ApplyItemDiscount = "Transactions.ApplyItemDiscount";
        public const string ApplyCartDiscount = "Transactions.ApplyCartDiscount";
        public const string EditTransaction = "Transactions.Edit";
        public const string DeleteTransaction = "Transactions.Delete";
        public const string OpenCloseShift = "Shifts.OpenClose";
        public const string PerformCashMovements = "Shifts.CashMovements";
        public const string ManageShifts = "Shifts.Manage";
        public const string ViewPos = "POS.View";
        public const string ManagePosLayout = "POS.ManageLayout";

        // Legacy strings stay temporarily so old code still compiles; NOT available to newly configured roles.
        public const string UpsertProducts = "Products.Upsert";
        public const string ManageCategories = "Categories.Manage";
        public const string ManageSuppliers = "Suppliers.Manage";
        public const string ManageUsers = "Users.Manage";
        public const string editTransaction = EditTransaction;
        public const string deleteTransaction = DeleteTransaction;

        public static List<string> GetAllPermissions()
        {
            var excluded = new HashSet<string>(StringComparer.Ordinal)
            {
                nameof(UpsertProducts), nameof(ManageCategories), nameof(ManageSuppliers), nameof(ManageUsers),
                nameof(editTransaction), nameof(deleteTransaction)
            };

            return typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string) && !excluded.Contains(f.Name))
                .Select(f => (string)f.GetRawConstantValue()!)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
    }
}
