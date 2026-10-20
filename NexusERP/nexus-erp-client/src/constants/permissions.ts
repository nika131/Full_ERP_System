export const Permissions = {
  ViewProducts: 'Products.View', CreateProducts: 'Products.Create', EditProducts: 'Products.Edit', DeleteProducts: 'Products.Delete', AdjustStock: 'Products.AdjustStock',
  ViewCategories: 'Categories.View', CreateCategories: 'Categories.Create', EditCategories: 'Categories.Edit', DeleteCategories: 'Categories.Delete',
  ViewSuppliers: 'Suppliers.View', CreateSuppliers: 'Suppliers.Create', EditSuppliers: 'Suppliers.Edit', DeleteSuppliers: 'Suppliers.Delete',
  ViewStores: 'Stores.View', CreateStores: 'Stores.Create', EditStores: 'Stores.Edit',
  ViewDashboard: 'Dashboard.View',
  ViewOwnTransactions: 'Reports.ViewOwn', ViewAllTransactions: 'Reports.ViewAll', ExportExcelTransactions: 'Reports.ExportExcel', ViewShifts: 'Shifts.View', ViewReceipts: 'Receipts.View',
  ViewUsers: 'Users.View', CreateUsers: 'Users.Create', EditUsers: 'Users.Edit', DeleteUsers: 'Users.Delete', ManageRoles: 'Users.ManageRoles', AssignRoles: 'Users.AssignRoles', ManageJoinRequests: 'Users.ManageJoinRequests', ManageJoinCodes: 'Companies.ManageJoinCodes',
  ViewSalary: 'Salary.View', ManageSalary: 'Salary.Manage',
  ViewAuditLogs: 'AuditLogs.View', ViewAbsences: 'Absences.ViewOwn', RequestAbsences: 'Absences.Request', ManageAbsences: 'Absences.Manage',
  ViewSettings: 'Settings.View', ManageSettings: 'Settings.Manage',
  PerformSales: 'Transactions.PerformSale', PerformInboundTransactions: 'Transactions.PerformInbound',
  ApplyItemDiscount: 'Transactions.ApplyItemDiscount', ApplyCartDiscount: 'Transactions.ApplyCartDiscount',
  EditTransaction: 'Transactions.Edit', DeleteTransaction: 'Transactions.Delete',
  OpenCloseShift: 'Shifts.OpenClose', PerformCashMovements: 'Shifts.CashMovements', ManageShifts: 'Shifts.Manage',
  ViewPos: 'POS.View', ManagePosLayout: 'POS.ManageLayout',
} as const;

// Names shown to admins; the database persists only the stable strings above.
export const PermissionGroups: Record<string, { permission: string; label: string }[]> = {
  Products: [['ViewProducts','View products'],['CreateProducts','Create products'],['EditProducts','Edit products'],['DeleteProducts','Delete products'],['AdjustStock','Adjust stock']].map(([key,label])=>({permission:Permissions[key as keyof typeof Permissions],label})),
  Categories: [['ViewCategories','View categories'],['CreateCategories','Create categories'],['EditCategories','Edit categories'],['DeleteCategories','Delete categories']].map(([key,label])=>({permission:Permissions[key as keyof typeof Permissions],label})),
  Suppliers: [['ViewSuppliers','View suppliers'],['CreateSuppliers','Create suppliers'],['EditSuppliers','Edit suppliers'],['DeleteSuppliers','Delete suppliers']].map(([key,label])=>({permission:Permissions[key as keyof typeof Permissions],label})),
  Stores: [['ViewStores','View stores'],['CreateStores','Create stores'],['EditStores','Edit stores']].map(([key,label])=>({permission:Permissions[key as keyof typeof Permissions],label})),
  Dashboard: [{permission:Permissions.ViewDashboard,label:'View overview, statistics and charts'}],
  'Reports & Shifts': [['ViewOwnTransactions','View own transactions'],['ViewAllTransactions','View all transactions'],['ViewShifts','View shift history'],['ViewReceipts','View receipts'],['ExportExcelTransactions','Export reports'],['OpenCloseShift','Open and close shifts'],['PerformCashMovements','Cash movements'],['ManageShifts','Manage shifts']].map(([key,label])=>({permission:Permissions[key as keyof typeof Permissions],label})),
  'Employees & Access': [['ViewUsers','View employees'],['CreateUsers','Create employees'],['EditUsers','Edit employees'],['DeleteUsers','Remove employees'],['ManageRoles','Create, edit and delete roles'],['AssignRoles','Assign non-system roles to employees'],['ManageJoinRequests','Review join requests'],['ManageJoinCodes','Regenerate join codes'],['ViewSalary','View employee salaries'],['ManageSalary','Manage employee salaries']].map(([key,label])=>({permission:Permissions[key as keyof typeof Permissions],label})),
  'POS & Inventory': [['ViewPos','View POS'],['PerformSales','Perform sales'],['PerformInboundTransactions','Record inbound transactions'],['ApplyItemDiscount','Apply item discounts'],['ApplyCartDiscount','Apply cart discounts'],['ManagePosLayout','Edit POS layout']].map(([key,label])=>({permission:Permissions[key as keyof typeof Permissions],label})),
  'Absences & Settings': [['ViewAbsences','View own absences'],['RequestAbsences','Request absences'],['ManageAbsences','Review absences'],['ViewSettings','Read system settings'],['ManageSettings','Change system settings'],['ViewAuditLogs','View audit logs']].map(([key,label])=>({permission:Permissions[key as keyof typeof Permissions],label})),
};
