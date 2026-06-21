import { zodResolver } from "@hookform/resolvers/zod";
import type { EmployeeResponse } from "../../types/employee";
import { useForm } from "react-hook-form";
import toast from "react-hot-toast";
import { salaryRecordSchema, type SalaryRecordFormData } from "../../schemas/hrSchema";
import { useSalaryHistoryQuery, useAddSalaryRecordMutation } from "../../hooks/queries/useHrQueries";

export const SalaryForm = ({ employee, onSuccess }: { employee: EmployeeResponse, onSuccess: () => void }) => {
    const { data: salaryHistory = [], isLoading: isFetchingSalary } = useSalaryHistoryQuery(employee.userId);
    const addSalaryMutation = useAddSalaryRecordMutation();

    const { register, handleSubmit, reset, formState: { errors } } = useForm<SalaryRecordFormData>({
        resolver: zodResolver(salaryRecordSchema),
        defaultValues: {
            amount: 0,
            effectiveDate: new Date().toISOString().split('T')[0],
            notes: ''
        }
    });

    const onSubmit = async (data: SalaryRecordFormData) => {
        try {
            const payload = {
                ...data,
                notes: data.notes || null 
            };
            await addSalaryMutation.mutateAsync({ userId: employee.userId, payload });
            toast.success("New salary record added.");
            reset();
            onSuccess();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to add salary.");
        }
    };

    return (
        <div className="flex-1 overflow-y-auto p-6 space-y-6">
            <form onSubmit={handleSubmit(onSubmit)} className="bg-slate-50 p-4 rounded-md border border-slate-200 space-y-4">
                <h4 className="text-sm font-bold text-slate-800">Issue New Salary Contract</h4>
                <div className="grid grid-cols-2 gap-4">
                    <div>
                        <label className="block text-xs font-medium text-slate-700">New Amount ($)</label>
                        <input 
                            type="number" step="0.01" 
                            {...register('amount', { valueAsNumber: true })}
                            className={`mt-1 w-full p-2 text-sm border rounded-md outline-none ${errors.amount ? 'border-red-500' : 'border-slate-300'}`}
                        />
                        {errors.amount && <p className="text-red-500 text-xs mt-1">{errors.amount.message}</p>}
                    </div>
                    <div>
                        <label className="block text-xs font-medium text-slate-700">Effective Date</label>
                        <input 
                            type="date" 
                            {...register('effectiveDate')}
                            className={`mt-1 w-full p-2 text-sm border rounded-md outline-none ${errors.effectiveDate ? 'border-red-500' : 'border-slate-300'}`}
                        />
                        {errors.effectiveDate && <p className="text-red-500 text-xs mt-1">{errors.effectiveDate.message}</p>}
                    </div>
                </div>
                <div>
                    <label className="block text-xs font-medium text-slate-700">Reason / Notes</label>
                    <input 
                        type="text" placeholder="e.g., Annual Review, Promotion"
                        {...register('notes')}
                        className={`mt-1 w-full p-2 text-sm border rounded-md outline-none ${errors.notes ? 'border-red-500' : 'border-slate-300'}`}
                    />
                    {errors.notes && <p className="text-red-500 text-xs mt-1">{errors.notes.message}</p>}
                </div>
                <button 
                    type="submit" 
                    disabled={addSalaryMutation.isPending} 
                    className="w-full py-2 bg-slate-800 text-white rounded-md text-sm font-medium hover:bg-slate-700 disabled:opacity-50"
                >
                    {addSalaryMutation.isPending ? 'Processing...' : 'Apply Contract'}
                </button>
            </form>

            <div>
                <h4 className="text-sm font-bold text-slate-800 mb-2">Contract History</h4>
                {isFetchingSalary ? <p className="text-sm text-slate-500">Loading...</p> : (
                    <table className="w-full text-left text-sm border border-slate-200">
                        <thead className="bg-slate-100 text-slate-600">
                            <tr>
                                <th className="p-2">Amount</th>
                                <th className="p-2">Effective</th>
                                <th className="p-2">Reason</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-200">
                            {salaryHistory.map(record => (
                                <tr key={record.salaryRecordId}>
                                    <td className="p-2 font-medium text-emerald-600">${record.amount.toLocaleString()}</td>
                                    <td className="p-2">{new Date(record.effectiveDate).toLocaleDateString()}</td>
                                    <td className="p-2 text-slate-500">{record.notes || '-'}</td>
                                </tr>
                            ))}
                            {salaryHistory.length === 0 && (
                                <tr><td colSpan={3} className="p-4 text-center text-slate-500">No salary history found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>
        </div>
    );
};