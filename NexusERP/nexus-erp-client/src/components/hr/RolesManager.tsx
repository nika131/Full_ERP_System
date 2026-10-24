import { roleService } from '../../api/roleService';
import { useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { useUrlString } from '../../hooks/useUrlState';
import { User } from 'lucide-react';
import type { RoleResponse } from '../../types/role';
import { RoleEditorForm } from '../forms/RoleEditorForm';
import { useRolesQuery, usePermissionsQuery } from '../../hooks/queries/useHrQueries';

export const RolesManager = () => {
    const { data: roles = [], isLoading: isLoadingRoles } = useRolesQuery();
    const queryClient = useQueryClient();
    const { data: availablePermissions = [], isLoading: isLoadingPerms } = usePermissionsQuery();

    const [roleParam, setRoleParam] = useUrlString('roleEditor');
    const selectedRole: RoleResponse | null = roles.find(role => String(role.roleId) === roleParam) ?? null;
    const isEditing = roleParam === 'new' || !!selectedRole;

    const handleCreateNew = () => {
        setRoleParam('new');
    };

    const handleEdit = (role: RoleResponse) => {
        setRoleParam(String(role.roleId));
    };

    const handleSuccess = () => {
        setRoleParam('');
    };

    const handleDelete = async (role: RoleResponse) => {
        if (role.isSystemRole || !window.confirm(`Delete role "${role.name}"? Employees using it must be reassigned first.`)) return;
        try {
            await roleService.deleteRole(role.roleId);
            await queryClient.invalidateQueries({ queryKey: ['roles'] });
            setRoleParam('');
            toast.success('Role deleted.');
        } catch { /* API client displays the server's error message. */ }
    };

    if (isLoadingRoles || isLoadingPerms) return <div className="p-8 text-center text-slate-500">Loading roles...</div>;

    return (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 h-[calc(100vh-8rem)]">
            {/* Left Column: Role List */}
            <div className="col-span-1 bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden flex flex-col h-full">
                <div className="p-4 border-b border-slate-200 flex justify-between items-center bg-slate-50 shrink-0">
                    <h3 className="font-bold text-slate-800">Company Roles</h3>
                    <button onClick={handleCreateNew} className="text-sm bg-emerald-600 text-white px-3 py-1.5 rounded hover:bg-emerald-700">
                        + New Role
                    </button>
                </div>
                <ul className="divide-y divide-slate-100 overflow-y-auto flex-1">
                    {roles.map(role => (
                        <li 
                            key={role.roleId} 
                            onClick={() => handleEdit(role)}
                            className={`p-4 cursor-pointer hover:bg-emerald-50 transition-colors ${selectedRole?.roleId === role.roleId && isEditing ? 'bg-emerald-50 border-l-4 border-emerald-600' : 'border-l-4 border-transparent'}`}
                        >
                            <div className="flex items-center justify-between gap-2">
                                <p className="font-bold text-slate-800">{role.name}</p>
                                {!role.isSystemRole && <button type="button" onClick={event => { event.stopPropagation(); void handleDelete(role); }}
                                    className="rounded px-2 py-1 text-xs font-semibold text-red-600 hover:bg-red-50">Delete</button>}
                            </div>
                            <p className="text-xs text-slate-500 mt-1">{role.permissions.length} permissions {role.isSystemRole ? '· Protected' : '· Editable'}</p>
                        </li>
                    ))}
                </ul>
            </div>

            {/* Right Column: Editor Sandbox */}
            <div className="col-span-2 bg-white border border-slate-200 rounded-lg shadow-sm p-6 overflow-hidden">
                {!isEditing ? (
                    <div className="h-full flex flex-col items-center justify-center bg-slate-50 rounded-lg border-2 border-dashed border-slate-200">
                        <User className="w-16 h-16 text-slate-300 mb-4" />
                        <h3 className="text-lg font-medium text-slate-700">No Role Selected</h3>
                        <p className="text-sm text-slate-500 mt-2 max-w-sm text-center">
                            Select a role to review or edit it, or create a new role. System roles are protected from changes.
                        </p>
                    </div>
                ) : (
                    <RoleEditorForm 
                        key={selectedRole ? selectedRole.roleId : 'new-role'} 
                        role={selectedRole} 
                        availablePermissions={availablePermissions}
                        onSuccess={handleSuccess}
                        onCancel={() => setRoleParam('')}
                    />
                )}
            </div>
        </div>
    );
};
