import { zodResolver } from "@hookform/resolvers/zod";
import type { EmployeeResponse } from "../../types/employee";
import { useForm } from "react-hook-form";
import { employeeProfileSchema, type EmployeeProfileFormData } from "../../schemas/hrSchema";
import toast from "react-hot-toast";
import { useRolesQuery, useUpdateEmployeeMutation } from "../../hooks/queries/useHrQueries";

export const RoleForm = ({ employee, onSuccess }: { employee: EmployeeResponse, onSuccess: () => void }) => {
    const { data: roles = [], isLoading: isLoadingRoles } = useRolesQuery();
    const updateMutation = useUpdateEmployeeMutation();

    const { register, handleSubmit, formState: { errors } } = useForm<EmployeeProfileFormData>({
        resolver: zodResolver(employeeProfileSchema),
        defaultValues: {
            fullName: employee.fullName,
            username: employee.username,
            roleId: employee.roleId
        }
    });

    const onSubmit = async (data: EmployeeProfileFormData) => {
        try {
            await updateMutation.mutateAsync({
                userId: employee.userId,
                data: { ...data, salary: employee.salary }
            });
            toast.success("Employee profile updated.");
            onSuccess();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to update employee.");
        }
    };

    return (
        <form id="role-form" onSubmit={handleSubmit(onSubmit)} className="space-y-4 flex flex-col h-full">
            <div className="flex-1 overflow-y-auto p-6 space-y-4">
                
                {/* Form Fields */}
                <div>
                    <label className="block text-sm font-medium text-slate-700">Full Name</label>
                    <input 
                        type="text" 
                        {...register('fullName')}
                        className={`mt-1 w-full p-2 border rounded-md outline-none ${errors.fullName ? 'border-red-500' : 'border-slate-300 focus:border-blue-500'}`}
                    />
                    {errors.fullName && <p className="text-red-500 text-xs mt-1">{errors.fullName.message}</p>}
                </div>
                <div>
                    <label className="block text-sm font-medium text-slate-700">Username</label>
                    <input 
                        type="text" 
                        {...register('username')}
                        className={`mt-1 w-full p-2 border rounded-md outline-none ${errors.username ? 'border-red-500' : 'border-slate-300 focus:border-blue-500'}`}
                    />
                    {errors.username && <p className="text-red-500 text-xs mt-1">{errors.username.message}</p>}
                </div>
                <div>
                    <label className="block text-sm font-medium text-slate-700">System Role</label>
                    <select 
                        {...register('roleId', { valueAsNumber: true })}
                        disabled={isLoadingRoles}
                        className={`mt-1 w-full p-2 border rounded-md outline-none ${errors.roleId ? 'border-red-500' : 'border-slate-300 focus:border-blue-500'}`}
                    >
                        {roles.map(r => (
                            <option key={r.roleId} value={r.roleId}>{r.name}</option>
                        ))}
                    </select>
                    {errors.roleId && <p className="text-red-500 text-xs mt-1">{errors.roleId.message}</p>}
                </div>
            </div>
            
            {/* Embedded Action Footer */}
            <div className="p-6 border-t border-slate-200 flex justify-end gap-3 bg-slate-50 mt-auto">
                <button 
                    type="submit" 
                    disabled={updateMutation.isPending} 
                    className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
                >
                    {updateMutation.isPending ? 'Saving...' : 'Save Profile'}
                </button>
            </div>
        </form>
    );
};