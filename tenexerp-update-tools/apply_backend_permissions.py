#!/usr/bin/env python3
"""Apply audited multi-tenant permission policy changes on feature/company-auth.
Runs from repository root (or NexusERP solution folder). All file edits are atomic after validation.
Does not run migrations or touch database. Review git diff after applying.
"""
from pathlib import Path
import re,sys
base=Path.cwd()
if (base/'NexusERP'/'NexusERP.Api'/'Program.cs').exists(): base=base/'NexusERP'
if not (base/'NexusERP.Api'/'Program.cs').exists():
    sys.exit('Run this script from the repository root or NexusERP solution directory.')
changes={}

def path(p): return base/p

def write(p,c):
    file=path(p);old=file.read_text(encoding='utf-8-sig')
    if old!=c:changes[p]=(old,c)

def read(p): return path(p).read_text(encoding='utf-8-sig')

def must(s,old,new,minimum=1):
    count=s.count(old)
    if count<minimum:raise RuntimeError(f'Expected anchor not found: {old[:110]!r} (found {count})')
    return s.replace(old,new)

def using(s,name):
    if f'using {name};' not in s: return f'using {name};\n'+s
    return s

def action(s,method,policy=None,guard=None):
    # Applies to a method with its existing Http attribute(s). Keep old Authorize only if no policy requested.
    patt=r'(?P<attrs>\s*\[Http(?:Get|Put|Post|Delete|Patch)(?:\([^\n]*\))?\](?:\s*\[Authorize\([^\n]*\)\])?)\s*(?P<sig>public[^\n]*?\b'+re.escape(method)+r'\s*\()'
    def new(m):
        attrs=re.sub(r'\s*\[Authorize\([^\n]*\)\]','',m.group('attrs'))
        return attrs+(('\n        [Authorize(Policy = '+policy+')]') if policy else '')+'\n        '+m.group('sig')
    s,count=re.subn(patt,new,s,count=1)
    if count!=1:raise RuntimeError('Missing action '+method)
    if guard:
        # Find exact beginning of method body after signature; signatures might be long but end before first '{'.
        begin=s.index('public ',s.index(' '+method+'(')-100) if False else 0
        match=re.search(r'(public[^\n]*?\b'+re.escape(method)+r'\s*\([^)]*\)\s*\{)',s)
        if not match:raise RuntimeError('No body for '+method)
        index=match.end()
        s=s[:index]+'\n'+guard+s[index:]
    return s

# Program.cs dynamic single source of truth; preserve existing auth configuration.
p='NexusERP.Api/Program.cs';s=read(p)
s=must(s,'    options.AddPolicy("RequirePerformSales", policy => policy.RequireClaim("Permission", Permissions.PerformSales));', '''    options.AddPolicy("RequirePerformSales", policy => policy.RequireClaim("Permission", Permissions.PerformSales));
    options.AddPolicy("RequireReadTransactions", policy => policy.RequireAssertion(ctx =>
        ctx.User.HasClaim("Permission", Permissions.ViewOwnTransactions) ||
        ctx.User.HasClaim("Permission", Permissions.ViewAllTransactions)));
    options.AddPolicy("RequireViewAllTransactions", policy => policy.RequireClaim("Permission", Permissions.ViewAllTransactions));
    options.AddPolicy("RequireCategoryLookup", policy => policy.RequireAssertion(ctx =>
        ctx.User.HasClaim("Permission", Permissions.ViewCategories) ||
        ctx.User.HasClaim("Permission", Permissions.ViewProducts) ||
        ctx.User.HasClaim("Permission", Permissions.ViewDashboard)));
    options.AddPolicy("RequireSupplierLookup", policy => policy.RequireAssertion(ctx =>
        ctx.User.HasClaim("Permission", Permissions.ViewSuppliers) ||
        ctx.User.HasClaim("Permission", Permissions.ViewProducts) ||
        ctx.User.HasClaim("Permission", Permissions.ViewDashboard)));
    options.AddPolicy("RequireStoreLookup", policy => policy.RequireAssertion(ctx =>
        ctx.User.HasClaim("Permission", Permissions.ViewStores) ||
        ctx.User.HasClaim("Permission", Permissions.ViewDashboard)));
    options.AddPolicy("RequireEmployeeLookup", policy => policy.RequireAssertion(ctx =>
        ctx.User.HasClaim("Permission", Permissions.ViewUsers) ||
        ctx.User.HasClaim("Permission", Permissions.ViewDashboard)));

    foreach (var permission in Permissions.GetAllPermissions())
        options.AddPolicy(permission, policy => policy.RequireClaim("Permission", permission));''')
# Remove first-Admin-only permissions sync: sync all *system* Admin roles, seed editable roles only where name never existed.
start=s.rfind('using (var scope = app.Services.CreateScope())')
end=s.index('app.Run();',start)
if start==-1 or end==-1:raise RuntimeError('Cannot locate startup role synchronization')
replacement='''using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var canonicalPermissions = Permissions.GetAllPermissions();
    var admins = dbContext.Roles.Where(r => r.IsSystemRole && r.Name == "Admin").ToList();

    foreach (var admin in admins)
    {
        if (!admin.Permissions.ToHashSet(StringComparer.Ordinal).SetEquals(canonicalPermissions))
            admin.Permissions = canonicalPermissions.ToList();
    }

    // Migrate old broad privileges to their equivalent granular privileges, without editing role names.
    var legacyMap = new Dictionary<string, string[]>
    {
        [Permissions.UpsertProducts] = new[] { Permissions.CreateProducts, Permissions.EditProducts },
        [Permissions.ManageCategories] = new[] { Permissions.ViewCategories, Permissions.CreateCategories, Permissions.EditCategories, Permissions.DeleteCategories },
        [Permissions.ManageSuppliers] = new[] { Permissions.ViewSuppliers, Permissions.CreateSuppliers, Permissions.EditSuppliers, Permissions.DeleteSuppliers },
        [Permissions.ManageUsers] = new[] { Permissions.ViewUsers, Permissions.CreateUsers, Permissions.EditUsers, Permissions.DeleteUsers, Permissions.ManageRoles, Permissions.AssignRoles, Permissions.ManageJoinRequests, Permissions.ManageJoinCodes, Permissions.ViewSalary, Permissions.ManageSalary },
    };
    foreach (var role in dbContext.Roles.Where(r => !r.IsSystemRole && r.IsActive).ToList())
    {
        var updated = role.Permissions.ToHashSet(StringComparer.Ordinal);
        foreach (var entry in legacyMap)
        {
            if (updated.Remove(entry.Key))
                updated.UnionWith(entry.Value);
        }
        // Pre-existing roles remain useful with the newly separated read/view policies.
        if (updated.Contains(Permissions.PerformSales)) updated.Add(Permissions.ViewPos);
        if (updated.Contains(Permissions.ManageShifts) || updated.Contains(Permissions.OpenCloseShift))
        {
            updated.Add(Permissions.ViewShifts);
            updated.Add(Permissions.ViewPos);
        }
        if (updated.Contains(Permissions.ViewAllTransactions)) updated.Add(Permissions.ViewReceipts);
        if (updated.Overlaps(new[] { Permissions.CreateProducts, Permissions.EditProducts, Permissions.DeleteProducts }))
            updated.Add(Permissions.ViewProducts);
        if (!role.Permissions.ToHashSet(StringComparer.Ordinal).SetEquals(updated))
            role.Permissions = updated.OrderBy(p => p).ToList();
    }

    // Seed missing editable starter roles once; do not overwrite customized roles.
    foreach (int companyId in dbContext.Companies.Select(c => c.CompanyId).ToList())
    {
        var existingNames = dbContext.Roles.Where(r => r.CompanyId == companyId)
            .Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var template in RoleTemplates.CreateDefaultRoles(companyId))
        {
            if (!existingNames.Contains(template.Name))
                dbContext.Roles.Add(template);
        }
    }
    dbContext.SaveChanges();
}

'''
s=s[:start]+replacement+s[end:]
write(p,s)

# New companies get same editable roles inside company creation transaction.
p='NexusERP.Infrastructure/Services/CompanyRegistrationService.cs';s=read(p)
s=must(s,'_context.Roles.Add(adminRole);\n                await _context.SaveChangesAsync();', '''_context.Roles.Add(adminRole);
                _context.Roles.AddRange(RoleTemplates.CreateDefaultRoles(company.CompanyId));
                await _context.SaveChangesAsync();''')
write(p,s)

# Password strength enforced server side as well as frontend.
p='NexusERP.Infrastructure/Services/AccountRegistrationService.cs';s=read(p)
needle='''            bool exists = await _context.Accounts.AnyAsync(a => a.NormalizedEmail == normalizedEmail);'''
s=must(s,needle,'''            if (dto.Password.Distinct().Count() == 1 || dto.Password.All(char.IsDigit) ||
                new[] { "password", "qwerty", "abcdefgh", "12345678", "admin123", "letmein" }
                    .Any(weak => dto.Password.StartsWith(weak, StringComparison.OrdinalIgnoreCase)))
                throw new AppException("Choose a less predictable password. Avoid common words, sequences and repeated characters.");

'''+needle)
write(p,s)

# Basic role escalation hardening. Role creation/edit/delete business behavior otherwise unchanged.
p='NexusERP.Infrastructure/Repositories/RoleRepository.cs';s=using(read(p),'NexusERP.Domain.Constants')
needle='''                    bool isNew = role.RoleId == 0;'''
s=must(s,needle,'''                    var actor = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u =>
                        u.UserId == userId && u.CompanyId == companyId && u.IsActive && u.Role.IsActive);

                    if (actor == null || !actor.Role.Permissions.Contains(Permissions.ManageRoles))
                        throw new UnauthorizedAccessException("You do not have permission to manage roles.");

                    if (string.IsNullOrWhiteSpace(role.Name) || role.Name.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase))
                        throw new AppException("The protected Admin role cannot be created or modified.");

                    if (role.Permissions == null || role.Permissions.Count == 0 ||
                        role.Permissions.Except(Permissions.GetAllPermissions(), StringComparer.Ordinal).Any())
                        throw new AppException("Select one or more valid permissions.");

                    if (role.Permissions.Except(actor.Role.Permissions, StringComparer.Ordinal).Any())
                        throw new AppException("You cannot grant permissions you do not possess.");

'''+needle)
# Existing role may be system role, already protected by repository, retained.
write(p,s)

# Employee creation must not allow CreateUsers holders to assign the protected Admin role or escalate privileges.
p='NexusERP.Infrastructure/Repositories/UserRepository.cs';s=read(p)
needle='''                    if (!roleExists)
                        throw new AppException("The selected role does not belong to this company.");'''
s=must(s,needle,needle+'''

                    var selectedRole = await _context.Roles.AsNoTracking().FirstAsync(r => r.RoleId == user.RoleId && r.CompanyId == companyId);
                    var actor = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == actorUserId && u.CompanyId == companyId && u.IsActive);
                    if (actor == null || !actor.Role.IsActive || !actor.Role.Permissions.Contains(Permissions.CreateUsers))
                        throw new UnauthorizedAccessException("You do not have permission to create employees.");
                    if (!actor.Role.Permissions.Contains(Permissions.AssignRoles))
                        throw new UnauthorizedAccessException("You do not have permission to assign employee roles.");
                    if (selectedRole.IsSystemRole || selectedRole.Permissions.Except(actor.Role.Permissions).Any())
                        throw new AppException("You cannot assign this role.");''')
# UpdateUser is a separate role-assignment path; do not allow privilege escalation there.
needle='''            if (!roleExists)
                throw new AppException("The selected role does not belong to this company.");'''
s=must(s,needle,needle+'''

            var selectedRoleForUpdate = await _context.Roles.AsNoTracking().FirstAsync(r => r.RoleId == user.RoleId && r.CompanyId == companyId);
            var updater = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == actorUserId && u.CompanyId == companyId && u.IsActive);
            if (updater == null || !updater.Role.IsActive || !updater.Role.Permissions.Contains(Permissions.EditUsers))
                throw new UnauthorizedAccessException("You do not have permission to edit employees.");
            if (existingUser.RoleId != user.RoleId)
            {
                if (!updater.Role.Permissions.Contains(Permissions.AssignRoles))
                    throw new UnauthorizedAccessException("You do not have permission to assign employee roles.");
                if (selectedRoleForUpdate.IsSystemRole || selectedRoleForUpdate.Permissions.Except(updater.Role.Permissions).Any())
                    throw new AppException("You cannot assign this role.");
            }''')
s=using(s,'NexusERP.Domain.Constants');write(p,s)

# Checkout discounts must be authorized by item/cart permission, including quote calculation.
p='NexusERP.Infrastructure/Services/PosService.cs';s=using(read(p),'NexusERP.Domain.Constants')
needle='            var discountSetting = await _context.SystemSettings.FirstOrDefaultAsync(s =>'
s=must(s,needle,'''            // Both quote and final checkout pass this validation; do not authorize only at the controller.
            if (cart.CartDiscountPercentage > 0 || cart.Items.Any(i => i.ManualItemDiscountPercentage > 0))
            {
                var cashier = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u =>
                    u.CompanyId == companyId && u.UserId == userId && u.IsActive && u.Role.IsActive &&
                    u.Role.CompanyId == companyId);
                if (cashier == null)
                    throw new UnauthorizedAccessException("Cashier does not have an active company role.");
                if (cart.CartDiscountPercentage > 0 && !cashier.Role.Permissions.Contains(Permissions.ApplyCartDiscount))
                    throw new AppException("You do not have permission to apply cart discounts.");
                if (cart.Items.Any(i => i.ManualItemDiscountPercentage > 0) && !cashier.Role.Permissions.Contains(Permissions.ApplyItemDiscount))
                    throw new AppException("You do not have permission to apply item discounts.");
            }

            var discountSetting = await _context.SystemSettings.FirstOrDefaultAsync(s =>''')
write(p,s)

# Role endpoints use Users.ManageRoles, not the old sweeping Users.Manage.
p='NexusERP.Api/Controllers/RolesController.cs';s=read(p)
s=s.replace('[Authorize(Policy = "RequireManageUsers")]','[Authorize(Policy = Permissions.ManageRoles)]')
s,count=re.subn(r'var permissions = typeof\(Permissions\)[\s\S]*?\.ToList\(\);', 'var permissions = Permissions.GetAllPermissions();', s, count=1)
if count != 1: raise RuntimeError('RolesController GetAvailablePermissions block changed')
write(p,s)

# Legacy employee create endpoint respects the new granular permission.
p='NexusERP.Api/Controllers/AuthController.cs';s=using(read(p),'NexusERP.Domain.Constants')
s=must(s,'[Authorize(Policy = "RequireManageUsers")]','[Authorize(Policy = Permissions.CreateUsers)]')
write(p,s)

# Employee endpoints are independent read/write/salary operations.
p='NexusERP.Api/Controllers/EmployeesController.cs';s=using(read(p),'NexusERP.Domain.Constants')
s=s.replace('[Authorize(Policy = "RequireManageUsers")]','[Authorize]')
for method,policy in [('GetEmployees','Permissions.ViewUsers'),('UpdateEmployee','Permissions.EditUsers'),('DeleteEmployee','Permissions.DeleteUsers'),('GetSalaryHistory','Permissions.ViewSalary'),('AddSalaryRecord','Permissions.ManageSalary'),('GetLookupUsers','"RequireEmployeeLookup"')]:
 s=action(s,method,policy)
# Only authorized employee editors may receive the legacy POS PIN value.
s=must(s,'PosPin = u.PosPin,', 'PosPin = User.HasPermission(Permissions.EditUsers) ? u.PosPin : null,')
write(p,s)

# Product reads, independent create/edit/delete, and stock movement; no sweeping write policy.
p='NexusERP.Api/Controllers/ProductsController.cs';s=read(p)
s=action(s,'GetProducts','Permissions.ViewProducts')
s=action(s,'SaveProduct',None,'''            var requiredPermission = dto.ProductId == 0 ? Permissions.CreateProducts : Permissions.EditProducts;
            if (!User.HasPermission(requiredPermission))
                return StatusCode(403, new { message = "You do not have permission to create or edit products." });
''')
s=action(s,'DeleteProduct','Permissions.DeleteProducts')
s=action(s,'GetProductsLowOnStock','Permissions.ViewProducts')
s=action(s,'UploadImage',None,'''            if (!User.HasPermission(Permissions.CreateProducts) && !User.HasPermission(Permissions.EditProducts))
                return StatusCode(403, new { message = "You do not have permission to upload product images." });
''')
s=must(s,'if (!User.HasPermission(Permissions.PerformInboundTransactions))','if (!User.HasPermission(Permissions.PerformInboundTransactions) && !User.HasPermission(Permissions.AdjustStock))')
s=must(s,'return StatusCode(403, "Missing Inbound Inventory permission.");',
       'return StatusCode(403, new { message = "You do not have permission to adjust inventory stock." });')
write(p,s)

# Category/supplier lookup gets read-only permissions, creation and update are separate checks.
for kind in ['Categories','Suppliers']:
 p=f'NexusERP.Api/Controllers/{kind}Controller.cs';s=using(read(p),'NexusERP.Domain.Constants')
 s=s.replace('[Authorize(Policy = "RequireManage'+kind+'")]','')
 singular=kind[:-3]+'y' if kind.endswith('ies') else kind[:-1]
 nameGet='GetPaged' if kind=='Categories' else 'GetPagedSuppliers'
 methodSave='SaveCategory' if kind=='Categories' else 'SaveSuppliers'
 methodDelete='DeleteCategory' if kind=='Categories' else 'DeleteSupplier'
 field='CategoryId' if kind=='Categories' else 'SupplierId'
 s=action(s,nameGet,f'Permissions.View{kind}')
 s=action(s,'GetLookupList','"RequireCategoryLookup"' if kind=='Categories' else '"RequireSupplierLookup"')
 s=action(s,methodSave,None,'''            string requiredPermission = dto.'''+field+f''' == 0 ? Permissions.Create{kind} : Permissions.Edit{kind};
            if (!User.HasPermission(requiredPermission))
                return StatusCode(403, new {{ message = "You do not have permission to create or edit {kind.lower()}." }});
''')
 s=action(s,methodDelete,f'Permissions.Delete{kind}')
 write(p,s)

# Stores: separate browse/create/edit, no delete operation exists.
p='NexusERP.Api/Controllers/StoresController.cs';s=using(read(p),'NexusERP.Domain.Constants')
for method,permission in [('GetPagedStores','ViewStores'),('GetAllStores','ViewStores'),('GetStoreById','ViewStores'),('GetStoresNearby','ViewStores'),('CreateStore','CreateStores'),('UpdateStore','EditStores')]:
 s=action(s,method,'"RequireStoreLookup"' if method=='GetAllStores' else 'Permissions.'+permission)
write(p,s)

# Dashboard is a complete-page permission; reports ledger separately requested only with explicit permission.
p='NexusERP.Api/Controllers/ReportsController.cs';s=using(read(p),'NexusERP.Domain.Constants')
for method,policy in [('GetTransactions','"RequireReadTransactions"'),('GetShifts','Permissions.ViewShifts'),('GetReceipts','Permissions.ViewReceipts')]:
 s=action(s,method,policy)
write(p,s)

# Settings previously allowed anyone with a valid JWT to PUT any company setting.
p='NexusERP.Api/Controllers/SettingsController.cs';s=using(read(p),'NexusERP.Domain.Constants')
for method,policy in [('GetSettings','Permissions.ViewSettings'),('UpdateSetting','Permissions.ManageSettings')]:s=action(s,method,policy)
write(p,s)

# POS endpoints: per-action policies (existing service validation retained).
p='NexusERP.Api/Controllers/PosController.cs';s=using(read(p),'NexusERP.Domain.Constants')
for method,permission in [('OpenShift','OpenCloseShift'),('CloseShift','OpenCloseShift'),('AddCashMovement','PerformCashMovements'),('ProcessCheckout','PerformSales'),('VerifyPosPin','ViewPos'),('GetCurrentShift','ViewPos'),('GetShiftHistory','ViewShifts'),('GetShiftReceipts','ViewReceipts'),('GetReceiptDetail','ViewReceipts'),('GetSwitchableUsers','ViewPos'),('GetCheckoutQuote','PerformSales'),('GetPosLayout','ViewPos'),('SavePosLayout','ManagePosLayout')]:s=action(s,method,'Permissions.'+permission)
write(p,s)

# Absence service view/request methods. Any employee can view own salary via /employees/mysalary.
p='NexusERP.Api/Controllers/AbsencesController.cs';s=using(read(p),'NexusERP.Domain.Constants')
for method,permission in [('RequestLeave','RequestAbsences'),('GetMyHistory','ViewAbsences')]:
 try:s=action(s,method,'Permissions.'+permission)
 except RuntimeError:
  # Actual service names can vary; inspect failing anchors instead of pretending changes applied.
  print('WARNING: absence action not found',method)
write(p,s)

# Approval and join code services must use new permissions in DB checks, not only endpoint attributes.
for p,old,new in [
 ('NexusERP.Infrastructure/Services/CompanyJoinRequestService.cs','Permissions.ManageUsers','Permissions.ManageJoinRequests'),
 ('NexusERP.Infrastructure/Services/CompanyJoinCodeService.cs','Permissions.ManageUsers','Permissions.ManageJoinCodes')]:
 s=read(p)
 if old not in s:raise RuntimeError(f'Missing {old} in {p}')
 write(p,s.replace(old,new))
for p,constant in [('NexusERP.Api/Controllers/CompanyJoinRequestsController.cs','ManageJoinRequests'),('NexusERP.Api/Controllers/CompanyJoinCodeController.cs','ManageJoinCodes')]:
 s=using(read(p),'NexusERP.Domain.Constants')
 s=must(s,'Policy = "RequireManageUsers"',f'Policy = Permissions.{constant}')
 write(p,s)

# Atomic write after all source files have passed anchor validation.
for name,(old,new) in changes.items():
 p=path(name);p.write_text(new,encoding='utf-8')
 print('UPDATED',name)
print(f'Updated {len(changes)} backend files. Review git diff and run dotnet build.')
