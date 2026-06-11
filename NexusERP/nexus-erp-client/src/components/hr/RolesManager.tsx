import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import { roleService } from '../../api/roleService';
import type { RoleResponse } from '../../types/role';
import { User } from 'lucide-react';

export const RolesManager = () => {
    const [roles, setRoles] = useState<RoleResponse[]>([]);
    const [availablePermissions, setAvailablePermissions] = useState<string[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const [selectedRole, setSelectedRole] = useState<RoleResponse | null>(null);
    const [isEditing, setIsEditing] = useState(false);
    
    const [roleName, setRoleName] = useState('');
    const [roleId, setRoleId] = useState(0);
    const [selectedPermissions, setSelectedPermissions] = useState<Set<string>>(new Set());

    const loadData = async (signal?: AbortSignal) => {
        try {
            setIsLoading(true);
            const [rolesData, permsData] = await Promise.all([
                roleService.getRoles(signal),
                roleService.getAvailablePermissions(signal)
            ]);
            setRoles(rolesData);
            setAvailablePermissions(permsData);
        } catch (error: any) {
            if (error.name !== 'CanceledError') toast.error("Failed to load roles.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        const controller = new AbortController();
        loadData(controller.signal);
        return () => controller.abort();
    }, []);

    const handleCreateNew = () => {
        setSelectedRole(null);
        setRoleName('');
        setRoleId(0)
        setSelectedPermissions(new Set());
        setIsEditing(true);
    };

    const handleEdit = (role: RoleResponse) => {
        setSelectedRole(role);
        setRoleName(role.name);
        setRoleId(role.roleId)
        setSelectedPermissions(new Set(role.permissions));
        setIsEditing(true);
    };

    const togglePermission = (perm: string) => {
        const next = new Set(selectedPermissions);
        if (next.has(perm)) next.delete(perm);
        else next.add(perm);
        setSelectedPermissions(next);
    };

    const handleSave = async () => {
        if (!roleName.trim()) return toast.error("Role name is required.");
        
        try {
            const payload = { roleId: roleId, name: roleName, permissions: Array.from(selectedPermissions) };
            await roleService.upsertRole(payload);
            toast.success(roleId === 0 ? "Role updated." : "Role created.");
            setIsEditing(false);
            loadData();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to save role.");
        }
    };

    if (isLoading) return <div className="p-8 text-center text-slate-500">Loading roles...</div>;

    return (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* Left Column: Role List */}
            <div className="col-span-1 bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
                <div className="p-4 border-b border-slate-200 flex justify-between items-center bg-slate-50">
                    <h3 className="font-bold text-slate-800">System Roles</h3>
                    <button onClick={handleCreateNew} className="text-sm bg-emerald-600 text-white px-3 py-1.5 rounded hover:bg-emerald-700">
                        + New Role
                    </button>
                </div>
                <ul className="divide-y divide-slate-100">
                    {roles.map(role => (
                        <li 
                            key={role.roleId} 
                            onClick={() => handleEdit(role)}
                            className={`p-4 cursor-pointer hover:bg-emerald-50 transition-colors ${selectedRole?.roleId === role.roleId ? 'bg-emerald-50 border-l-4 border-emerald-600' : 'border-l-4 border-transparent'}`}
                        >
                            <p className="font-bold text-slate-800">{role.name}</p>
                            <p className="text-xs text-slate-500 mt-1">{role.permissions.length} permissions</p>
                        </li>
                    ))}
                </ul>
            </div>

            {/* Right Column: Editor */}
            <div className="col-span-2 bg-white border border-slate-200 rounded-lg shadow-sm p-6">
                {!isEditing ? (
                    <div className="h-full min-h-100 flex flex-col items-center justify-center bg-slate-50 rounded-lg border-2 border-dashed border-slate-200">
                        <User className="w-16 h-16 text-slate-300 mb-4" />
                        <h3 className="text-lg font-medium text-slate-700">No Role Selected</h3>
                        <p className="text-sm text-slate-500 mt-2 max-w-sm text-center">
                            Select a system role from the list to modify its permissions, or create a brand new role for your employees.
                        </p>
                    </div>
                ) : (
                    <div className="space-y-6">
                        <div className="flex justify-between items-center border-b border-slate-200 pb-4">
                            <h3 className="text-lg font-bold text-slate-800">
                                {selectedRole ? `Edit Role: ${selectedRole.name}` : 'Create New Role'}
                            </h3>
                            <div className="flex gap-2">
                                <button onClick={() => setIsEditing(false)} className="px-4 py-2 text-sm text-slate-600 hover:bg-slate-100 rounded-md">Cancel</button>
                                <button onClick={handleSave} className="px-4 py-2 text-sm text-white bg-emerald-600 hover:bg-emerald-700 rounded-md">Save Role</button>
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-slate-700 mb-1">Role Name</label>
                            <input 
                                type="text" 
                                className="w-full max-w-md p-2 border border-slate-300 rounded-md outline-none focus:border-emerald-500"
                                value={roleName}
                                onChange={e => setRoleName(e.target.value)}
                                placeholder="e.g., Senior Manager"
                                disabled={selectedRole?.name === 'Admin'} 
                            />
                        </div>

                        <div>
                            <h4 className="text-sm font-medium text-slate-700 mb-3">Permissions ({selectedPermissions.size} selected)</h4>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 bg-slate-50 p-4 rounded-md border border-slate-200 h-96 overflow-y-auto">
                                {availablePermissions.map(perm => (
                                    <label key={perm} className="flex items-center space-x-3 p-2 bg-white border border-slate-200 rounded cursor-pointer hover:border-blue-400 transition-colors">
                                        <input 
                                            type="checkbox" 
                                            className="h-4 w-4 text-emerald-600 rounded border-slate-300 focus:ring-emerald-500"
                                            checked={selectedPermissions.has(perm)}
                                            onChange={() => togglePermission(perm)}
                                            disabled={selectedRole?.name === 'Admin'} 
                                        />
                                        <span className="text-sm text-slate-700 font-medium">{perm}</span>
                                    </label>
                                ))}
                            </div>
                            {selectedRole?.name === 'Admin' && (
                                <p className="text-xs text-amber-600 mt-2">The core Admin role permissions cannot be modified.</p>
                            )}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};