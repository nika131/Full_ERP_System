import { useEffect, useState } from 'react';
import { absenceService } from '../api/absenceService';
import type { LeaveResponse } from '../types/absence';
import toast from 'react-hot-toast';

export default function PendingLeaves() {
    const [requests, setRequests] = useState<LeaveResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => { loadRequests(); }, []);

    const loadRequests = async () => {
        setIsLoading(true);
        const data = await absenceService.getPendingRequests();
        setRequests(data);
        setIsLoading(false);
    };

    const handleAction = async (id: number, status: 'Approved' | 'Rejected') => {
        try {
            await absenceService.reviewLeave(id, { status, reviewerComments: "" });
            toast.success(`Request ${status} successfully.`);
            loadRequests();
        } catch (e) { toast.error("Action failed."); }
    };

    return (
        <div className="max-w-4xl mx-auto p-6">
            <h2 className="text-2xl font-bold text-slate-800 mb-6">Pending Leave Approvals</h2>
            <div className="bg-white rounded-lg shadow-sm border border-slate-200 overflow-hidden">
                <table className="w-full text-sm">
                    <thead className="bg-slate-50 border-b border-slate-200">
                        <tr>
                            <th className="p-4 text-left">Employee</th>
                            <th className="p-4 text-left">Type</th>
                            <th className="p-4 text-left">Dates</th>
                            <th className="p-4 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {requests.map(r => (
                            <tr key={r.absenceId}>
                                <td className="p-4">{r.employeeName || 'N/A'}</td>
                                <td className="p-4">{r.type}</td>
                                <td className="p-4">{new Date(r.startDate).toLocaleDateString()} - {new Date(r.endDate).toLocaleDateString()}</td>
                                <td className="p-4 text-right space-x-2">
                                    <button onClick={() => handleAction(r.absenceId, 'Approved')} className="text-emerald-600 hover:text-emerald-800 font-medium">Approve</button>
                                    <button onClick={() => handleAction(r.absenceId, 'Rejected')} className="text-red-600 hover:text-red-800 font-medium">Reject</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}