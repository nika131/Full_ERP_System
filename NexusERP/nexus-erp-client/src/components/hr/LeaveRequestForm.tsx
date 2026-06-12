import toast from 'react-hot-toast';
import { absenceService } from '../../api/absenceService';
import { leaveRequestSchema, type LeaveRequestFormData } from '../../schemas/hrSchema';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';

interface LeaveRequestFormProps {
    onSuccess: () => void;
    onCancel: () => void;
}

export const LeaveRequestForm = ({ onSuccess, onCancel }: LeaveRequestFormProps) => {
    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting }
    } = useForm<LeaveRequestFormData>({
        resolver: zodResolver(leaveRequestSchema),
        defaultValues: {
            type: 'Vacation',
            startDate: '',
            endDate: '',
            notes: ''
        }
    });

    const onSubmit = async (data: LeaveRequestFormData) => {
        try {
            const payload = {
                ...data,
                notes: data.notes ?? null 
            };
            await absenceService.requestLeave(payload);
            toast.success("Leave request submitted successfully.");
            onSuccess();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to submit request.");
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            
            {/* Absence Type Select */}
            <div>
                <label className="block text-sm font-medium text-slate-700">Absence Type</label>
                <select 
                    {...register('type')}
                    className={`w-full px-3 py-2 border rounded outline-none bg-white transition-colors ${
                            errors.type ? 'border-red-500' : 'border-slate-300 focus:border-blue-500'
                        }`}
                >
                    <option value="Vacation">Vacation</option>
                    <option value="Sick">Sick Leave</option>
                    <option value="Personal">Personal Reason</option>
                    <option value="Unpaid">Unpaid Leave</option>
                </select>
                {errors.type && <p className="text-red-500 text-xs mt-1">{errors.type.message}</p>}
            </div>

            {/* Date Inputs Grid */}
            <div className="grid grid-cols-2 gap-4">
                <div>
                    <label className="block text-sm font-medium text-slate-700">Start Date</label>
                    <input 
                        type="date" 
                        {...register('startDate')}
                        className={`w-full px-3 py-2 border rounded outline-none transition-colors ${
                            errors.startDate ? 'border-red-500' : 'border-slate-300 focus:border-blue-500'
                        }`}
                    />
                    {errors.startDate && <p className="text-red-500 text-xs mt-1">{errors.startDate.message}</p>}
                </div>
                <div>
                    <label className="block text-sm font-medium text-slate-700">End Date</label>
                    <input 
                        type="date" 
                        {...register('endDate')}
                        className={`w-full px-3 py-2 border rounded outline-none transition-colors ${
                            errors.endDate ? 'border-red-500' : 'border-slate-300 focus:border-blue-500'
                        }`}
                    />
                    {errors.endDate && <p className="text-red-500 text-xs mt-1">{errors.endDate.message}</p>}
                </div>
            </div>

            {/* Notes Textarea */}
            <div>
                <label className="block text-sm font-medium text-slate-700">Notes (Optional)</label>
                <textarea 
                    rows={3}
                    placeholder="Provide a brief reason if necessary..."
                    {...register('notes')}
                    className={`w-full px-3 py-2 border rounded outline-none transition-colors ${
                        errors.notes ? 'border-red-500' : 'border-slate-300 focus:border-blue-500'
                    }`}
                />
                {errors.notes && <p className="text-red-500 text-xs mt-1">{errors.notes.message}</p>}
            </div>

            {/* Actions */}
            <div className="flex justify-end gap-2 pt-4">
                <button 
                    type="button" 
                    onClick={onCancel}
                    className="px-4 py-2 text-sm font-medium text-slate-700 bg-slate-100 rounded-md hover:bg-slate-200"
                >
                    Cancel
                </button>
                <button 
                    type="submit" 
                    disabled={isSubmitting}
                    className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
                >
                    {isSubmitting ? 'Submitting...' : 'Submit Request'}
                </button>
            </div>
        </form>
    );
};