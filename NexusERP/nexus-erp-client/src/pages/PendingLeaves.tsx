import { useAuth } from '../context/AuthContext';
import { usePendingLeavesQuery, useReviewLeaveMutation } from '../hooks/queries/usePendingLeavesQueries';

export default function PendingLeaves() {
    const { data: requests = [], isLoading } = usePendingLeavesQuery();
    const reviewMutation = useReviewLeaveMutation();

    const handleAction = (id: number, status: 'Approved' | 'Rejected') => {
        reviewMutation.mutate({ id, status });
    };

    const { user } = useAuth();

    const isOwnRequest = (requesterId: number) =>
        user?.userId != null && user.userId === requesterId;

    return (
        <div className="max-w-4xl mx-auto p-6">
            {/* Header */}
            <h2 className="text-2xl font-bold text-slate-800 mb-6">Pending Leave Approvals</h2>
            
            {/* Data Table */}
            <div className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
                <div className="tenex-scroll overflow-x-auto">
                    <table className="w-full min-w-[620px] text-sm">
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
                                        
                                        <td className="p-4 text-right">
                                            <div className="flex flex-col items-end gap-2">
                                                <div className="flex items-center justify-end gap-3">
                                                    <button
                                                        type="button"
                                                        onClick={() => handleAction(r.absenceId, 'Approved')}
                                                        disabled={
                                                            reviewMutation.isPending ||
                                                            isOwnRequest(r.userId)
                                                        }
                                                        title={
                                                            isOwnRequest(r.userId)
                                                                ? 'You cannot approve your own leave request.'
                                                                : undefined
                                                        }
                                                        className="text-sm font-semibold text-emerald-600 hover:text-emerald-800 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:text-emerald-600"
                                                    >
                                                        Approve
                                                    </button>

                                                    <button
                                                        type="button"
                                                        onClick={() => handleAction(r.absenceId, 'Rejected')}
                                                        disabled={
                                                            reviewMutation.isPending ||
                                                            isOwnRequest(r.userId)
                                                        }
                                                        title={
                                                            isOwnRequest(r.userId)
                                                                ? 'You cannot reject your own leave request.'
                                                                : undefined
                                                        }
                                                        className="text-sm font-semibold text-red-600 hover:text-red-800 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:text-red-600"
                                                    >
                                                        Reject
                                                    </button>
                                                </div>

                                                {isOwnRequest(r.userId) && (
                                                    <span className="text-xs leading-5 text-slate-500">
                                                        You cannot review your own request.
                                                    </span>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
}