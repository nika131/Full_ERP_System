import { zodResolver } from "@hookform/resolvers/zod";
import type { EmployeeResponse } from "../../types/employee";
import { useForm } from "react-hook-form";
import { employeeUpdateSchema, type EmployeeUpdateFormSchema } from "../../schemas/hrSchema";
import toast from "react-hot-toast";
import { useRolesQuery, useUpdateEmployeeMutation } from "../../hooks/queries/useHrQueries";
import { RefreshCw } from "lucide-react";

export const RoleForm = ({ employee, onSuccess }: { employee: EmployeeResponse, onSuccess: () => void }) => {
    const { data: roles = [], isLoading: isLoadingRoles } = useRolesQuery();
    const updateMutation = useUpdateEmployeeMutation();

    const { 
        register, 
        handleSubmit, 
        setValue, 
        formState: { errors } 
    } = useForm<EmployeeUpdateFormSchema>({
        resolver: zodResolver(employeeUpdateSchema),
        defaultValues: {
            fullName: employee.fullName,
            username: employee.username,
            roleId: employee.roleId,
            posPin: employee.posPin ?? '' 
        }
    });

    const generatePin = () => {
        const randomPin = Math.floor(1000 + Math.random() * 9000).toString();
        setValue('posPin', randomPin, { shouldValidate: true, shouldDirty: true });
    };

    const onSubmit = async (data: EmployeeUpdateFormSchema) => {
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

    const onError = (errors: any) => {
        console.error("Zod Validation Failed:", errors);
    };

    return (
        <form id="role-form" onSubmit={handleSubmit(onSubmit, onError)} className="space-y-4 flex flex-col h-full">
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

                {/* POS PIN Field with Generate Button */}
                <div>
                    <label className="block text-sm font-medium text-slate-700">POS Access PIN</label>
                    <div className="flex gap-2 mt-1">
                        <input
                            type="text"
                            maxLength={4}
                            placeholder="e.g. 1234"
                            {...register('posPin')}
                            className={`flex-1 p-2 border rounded-md outline-none transition-colors tracking-widest font-mono text-center ${errors.posPin ? 'border-red-500 focus:border-red-500' : 'border-slate-300 focus:border-blue-500'}`}
                            onKeyPress={(e) => {
                                if (!/[0-9]/.test(e.key)) {
                                    e.preventDefault();
                                }
                            }}
                        />
                        <button
                            type="button"
                            onClick={generatePin}
                            className="px-3 py-2 bg-slate-100 text-slate-600 border border-slate-300 rounded-md hover:bg-slate-200 transition-colors flex items-center justify-center gap-1.5 text-sm font-medium whitespace-nowrap shrink-0"
                        >
                            <RefreshCw size={16} className="text-slate-500" />
                            Generate
                        </button>
                    </div>
                    {errors.posPin && <p className="text-red-500 text-xs mt-1">{errors.posPin.message as string}</p>}
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