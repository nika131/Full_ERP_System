using System.Collections.Generic;
using NexusERP.Domain.Entities;

namespace NexusERP.Domain.Constants
{
    public static class RoleTemplates
    {
        // Editable company-specific starter roles. Do not overwrite an existing role's permissions.
        public static List<Role> CreateDefaultRoles(int companyId)
        {
            return new List<Role>
            {
                Make("Store Manager", companyId, Permissions.ViewDashboard, Permissions.ViewProducts, Permissions.CreateProducts, Permissions.EditProducts, Permissions.AdjustStock, Permissions.ViewCategories, Permissions.CreateCategories, Permissions.ViewSuppliers, Permissions.ViewStores, Permissions.CreateStores, Permissions.EditStores, Permissions.ViewAllTransactions, Permissions.ViewShifts, Permissions.ViewReceipts, Permissions.ViewPos, Permissions.PerformSales, Permissions.PerformInboundTransactions, Permissions.OpenCloseShift, Permissions.PerformCashMovements, Permissions.ViewSettings),
                Make("Inventory Clerk", companyId, Permissions.ViewProducts, Permissions.CreateProducts, Permissions.EditProducts, Permissions.AdjustStock, Permissions.ViewCategories, Permissions.ViewSuppliers, Permissions.ViewStores, Permissions.PerformInboundTransactions, Permissions.ViewAbsences, Permissions.RequestAbsences),
                Make("Cashier", companyId, Permissions.ViewProducts, Permissions.ViewCategories, Permissions.ViewStores, Permissions.ViewPos, Permissions.PerformSales, Permissions.OpenCloseShift, Permissions.ViewReceipts, Permissions.ViewAbsences, Permissions.RequestAbsences),
                Make("HR Officer", companyId, Permissions.ViewUsers, Permissions.CreateUsers, Permissions.EditUsers, Permissions.AssignRoles, Permissions.ViewSalary, Permissions.ManageSalary, Permissions.ManageAbsences, Permissions.ViewAbsences, Permissions.RequestAbsences),
                Make("Read Only", companyId, Permissions.ViewDashboard, Permissions.ViewProducts, Permissions.ViewCategories, Permissions.ViewSuppliers, Permissions.ViewStores, Permissions.ViewOwnTransactions)
            };
        }

        private static Role Make(string name, int companyId, params string[] permissions)
        {
            return new Role
            {
                CompanyId = companyId,
                Name = name,
                Permissions = new List<string>(permissions),
                IsSystemRole = false,
                IsActive = true
            };
        }
    }
}
