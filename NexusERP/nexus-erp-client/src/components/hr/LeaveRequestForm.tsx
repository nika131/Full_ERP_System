import { useState } from 'react';
import toast from 'react-hot-toast';
import { absenceService } from '../../api/absenceService';

interface LeaveRequestFormProps {
    onSuccess: () => void;
    onCancel: () => void;
}

export const LeaveRequestForm = ({ onSuccess, onCancel }: LeaveRequestFormProps) => {
    const [isLoading, setIsLoading] = useState(false);
    const [formData, setFormData] = useState({
        type: 'Vacation',
        startDate: '',
        endDate: '',
        notes: ''
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);

        try {
            if (new Date(formData.endDate) < new Date(formData.startDate)) {
                toast.error("End date cannot be before start date.");
                setIsLoading(false);
                return;
            }

            await absenceService.requestLeave(formData);
            toast.success("Leave request submitted successfully.");
            onSuccess(); 
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to submit request.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div>
                <label className="block text-sm font-medium text-slate-700">Absence Type</label>
                <select 
                    className="mt-1 block w-full rounded-md border border-slate-300 p-2"
                    value={formData.type}
                    onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                >
                    <option value="Vacation">Vacation</option>
                    <option value="Sick">Sick Leave</option>
                    <option value="Personal">Personal Reason</option>
                    <option value="Unpaid">Unpaid Leave</option>
                </select>
            </div>

            <div className="grid grid-cols-2 gap-4">
                <div>
                    <label className="block text-sm font-medium text-slate-700">Start Date</label>
                    <input 
                        type="date" 
                        required 
                        className="mt-1 block w-full rounded-md border border-slate-300 p-2"
                        value={formData.startDate}
                        onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-slate-700">End Date</label>
                    <input 
                        type="date" 
                        required 
                        className="mt-1 block w-full rounded-md border border-slate-300 p-2"
                        value={formData.endDate}
                        onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                    />
                </div>
            </div>

            <div>
                <label className="block text-sm font-medium text-slate-700">Notes (Optional)</label>
                <textarea 
                    className="mt-1 block w-full rounded-md border border-slate-300 p-2"
                    rows={3}
                    placeholder="Provide a brief reason if necessary..."
                    value={formData.notes}
                    onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                />
            </div>

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
                    disabled={isLoading}
                    className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
                >
                    {isLoading ? 'Submitting...' : 'Submit Request'}
                </button>
            </div>
        </form>
    );
};