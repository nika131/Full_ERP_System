import { usePendingLeavesQuery, useReviewLeaveMutation } from '../hooks/queries/usePendingLeavesQueries';

export default function PendingLeaves() {
    const { data: requests = [], isLoading } = usePendingLeavesQuery();
    const reviewMutation = useReviewLeaveMutation();

    const handleAction = (id: number, status: 'Approved' | 'Rejected') => {
        reviewMutation.mutate({ id, status });
    };

    return (
        <div className="max-w-4xl mx-auto p-6">
            {/* Header */}
            <h2 className="text-2xl font-bold text-slate-800 mb-6">Pending Leave Approvals</h2>
            
            {/* Data Table */}
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
                        {isLoading ? (
                            <tr>
                                <td colSpan={4} className="p-8 text-center text-slate-500">Loading requests...</td>
                            </tr>
                        ) : requests.length === 0 ? (
                            <tr>
                                <td colSpan={4} className="p-8 text-center text-slate-500">No pending requests found.</td>
                            </tr>
                        ) : (
                            requests.map(r => (
                                <tr key={r.absenceId}>
                                    <td className="p-4 font-medium text-slate-800">{r.employeeName || 'N/A'}</td>
                                    <td className="p-4 text-slate-600">{r.type}</td>
                                    <td className="p-4 text-slate-600">
                                        {new Date(r.startDate).toLocaleDateString()} - {new Date(r.endDate).toLocaleDateString()}
                                    </td>
                                    <td className="p-4 text-right space-x-3">
                                        <button 
                                            onClick={() => handleAction(r.absenceId, 'Approved')} 
                                            disabled={reviewMutation.isPending}
                                            className="text-emerald-600 hover:text-emerald-800 font-medium disabled:opacity-50"
                                        >
                                            Approve
                                        </button>
                                        <button 
                                            onClick={() => handleAction(r.absenceId, 'Rejected')} 
                                            disabled={reviewMutation.isPending}
                                            className="text-red-600 hover:text-red-800 font-medium disabled:opacity-50"
                                        >
                                            Reject
                                        </button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}