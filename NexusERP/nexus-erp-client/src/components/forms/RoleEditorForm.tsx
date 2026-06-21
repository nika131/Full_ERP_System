import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import toast from 'react-hot-toast';
import { roleSchema, type RoleFormData } from '../../schemas/hrSchema';
import type { RoleResponse } from '../../types/role';
import { useUpsertRoleMutation } from '../../hooks/queries/useHrQueries';

interface RoleEditorFormProps {
    role: RoleResponse | null;
    availablePermissions: string[];
    onSuccess: () => void;
    onCancel: () => void;
}

export const RoleEditorForm = ({ role, availablePermissions, onSuccess, onCancel }: RoleEditorFormProps) => {
    const isAdmin = role?.name === 'Admin';
    const upsertMutation = useUpsertRoleMutation();

    const { register, handleSubmit, formState: { errors } } = useForm<RoleFormData>({
        resolver: zodResolver(roleSchema),
        defaultValues: {
            name: role?.name || '',
            permissions: role?.permissions || []
        }
    });

    const onSubmit = async (data: RoleFormData) => {
        try {
            const payload = {
                roleId: role?.roleId || 0,
                name: data.name,
                permissions: data.permissions
            };
            
            await upsertMutation.mutateAsync(payload);
            toast.success(payload.roleId === 0 ? "Role created." : "Role updated.");
            onSuccess();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to save role.");
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 flex flex-col h-full">
            <div className="flex justify-between items-center border-b border-slate-200 pb-4">
                <h3 className="text-lg font-bold text-slate-800">
                    {role ? `Edit Role: ${role.name}` : 'Create New Role'}
                </h3>
                <div className="flex gap-2">
                    <button type="button" onClick={onCancel} className="px-4 py-2 text-sm text-slate-600 hover:bg-slate-100 rounded-md">
                        Cancel
                    </button>
                    <button 
                        type="submit" 
                        disabled={upsertMutation.isPending || isAdmin} 
                        className="px-4 py-2 text-sm text-white bg-emerald-600 hover:bg-emerald-700 rounded-md disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {upsertMutation.isPending ? 'Saving...' : 'Save Role'}
                    </button>
                </div>
            </div>

            <div className="flex-1 overflow-y-auto pr-2 space-y-6">
                <div>
                    <label className="block text-sm font-medium text-slate-700 mb-1">Role Name</label>
                    <input 
                        type="text" 
                        {...register('name')}
                        disabled={isAdmin}
                        className={`w-full max-w-md p-2 border rounded-md outline-none transition-colors ${errors.name ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'} disabled:bg-slate-100`}
                        placeholder="e.g., Senior Manager"
                    />
                    {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name.message}</p>}
                </div>

                <div>
                    <div className="flex justify-between items-center mb-3">
                        <h4 className="text-sm font-medium text-slate-700">System Permissions</h4>
                        {errors.permissions && <span className="text-red-500 text-xs font-medium">{errors.permissions.message}</span>}
                    </div>
                    
                    <div className={`grid grid-cols-1 sm:grid-cols-2 gap-3 bg-slate-50 p-4 rounded-md border ${errors.permissions ? 'border-red-300' : 'border-slate-200'}`}>
                        {availablePermissions.map(perm => (
                            <label key={perm} className={`flex items-center space-x-3 p-2 bg-white border border-slate-200 rounded transition-colors ${isAdmin ? 'opacity-70 cursor-not-allowed' : 'cursor-pointer hover:border-blue-400'}`}>
                                <input 
                                    type="checkbox" 
                                    value={perm}
                                    {...register('permissions')}
                                    disabled={isAdmin}
                                    className="h-4 w-4 text-emerald-600 rounded border-slate-300 focus:ring-emerald-500 disabled:opacity-50"
                                />
                                <span className="text-sm text-slate-700 font-medium">{perm}</span>
                            </label>
                        ))}
                    </div>
                    {isAdmin && (
                        <p className="text-xs text-amber-600 mt-3 font-medium bg-amber-50 p-2 rounded border border-amber-200">
                            System Lock: The core Admin role is protected and cannot be modified or duplicated via the interface.
                        </p>
                    )}
                </div>
            </div>
        </form>
    );
};