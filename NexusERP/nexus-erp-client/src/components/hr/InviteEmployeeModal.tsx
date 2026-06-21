import toast from 'react-hot-toast';
import { z } from 'zod';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { employeeProfileSchema } from '../../schemas/hrSchema';
import { useRolesQuery, useRegisterEmployeeMutation } from '../../hooks/queries/useHrQueries';

type EmployeeProfileFormData = z.infer<typeof employeeProfileSchema>;

interface InviteModalProps {
    onClose: () => void;
    onSuccess: () => void;
}

export const InviteEmployeeModal = ({ onClose, onSuccess }: InviteModalProps) => {
    const { data: roles = [] } = useRolesQuery();
    const registerMutation = useRegisterEmployeeMutation();

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<EmployeeProfileFormData>({
        resolver: zodResolver(employeeProfileSchema),
        defaultValues: {
            fullName: '',
            username: '',
            password: '',
            roleId: 0
        }
    });

    const onSubmit = async (data: EmployeeProfileFormData) => {
        try {
            await registerMutation.mutateAsync(data);
            toast.success("Employee created successfully.");
            onSuccess();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to create employee.");
        }
    };

    return (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
                <h3 className="text-lg font-bold text-slate-800 mb-4">Invite New Employee</h3>
                
                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    {/* Form Fields */}
                    <div>
                        <label className="block text-sm font-medium text-slate-700">Full Name</label>
                        <input 
                            type="text" 
                            className={`mt-1 w-full p-2 border rounded-md outline-none focus:border-emerald-500 ${errors.fullName ? 'border-red-500' : 'border-slate-300'}`}
                            {...register('fullName')}
                        />
                        {errors.fullName && <p className="text-red-500 text-xs mt-1">{errors.fullName.message}</p>}
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-slate-700">Username</label>
                        <input 
                            type="text" 
                            className={`mt-1 w-full p-2 border rounded-md outline-none focus:border-emerald-500 ${errors.username ? 'border-red-500' : 'border-slate-300'}`}
                            {...register('username')}
                        />
                        {errors.username && <p className="text-red-500 text-xs mt-1">{errors.username.message}</p>}
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-slate-700">Password</label>
                        <input 
                            type="password" 
                            className={`mt-1 w-full p-2 border rounded-md outline-none focus:border-emerald-500 ${errors.password ? 'border-red-500' : 'border-slate-300'}`}
                            {...register('password')}
                        />
                        {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-slate-700">System Role</label>
                        <select 
                            className={`mt-1 w-full p-2 border rounded-md outline-none focus:border-emerald-500 ${errors.roleId ? 'border-red-500' : 'border-slate-300'}`}
                            {...register('roleId', { valueAsNumber: true })}
                        >
                            <option value={0}>Select a role...</option>
                            {roles.map(r => (
                                <option key={r.roleId} value={r.roleId}>{r.name}</option>
                            ))}
                        </select>
                        {errors.roleId && <p className="text-red-500 text-xs mt-1">{errors.roleId.message}</p>}
                    </div>

                    {/* Actions */}
                    <div className="flex justify-end gap-2 pt-4">
                        <button 
                            type="button" 
                            onClick={onClose} 
                            className="px-4 py-2 text-sm text-slate-600 hover:bg-slate-100 rounded-md"
                        >
                            Cancel
                        </button>
                        <button 
                            type="submit" 
                            disabled={registerMutation.isPending} 
                            className="px-4 py-2 text-sm bg-emerald-600 text-white rounded-md hover:bg-emerald-700 disabled:opacity-50"
                        >
                            {registerMutation.isPending ? 'Creating...' : 'Create Employee'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};