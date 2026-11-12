import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useRecentLeavesQuery, useReviewLeaveMutation } from '../hooks/queries/usePendingLeavesQueries';
import { DataTable, type ColumnDef } from '../components/Ui/DataTable';
import type { LeaveResponse } from '../types/absence';

export default function PendingLeaves() {
    const { data: requests = [], isLoading } = useRecentLeavesQuery();
    const reviewMutation = useReviewLeaveMutation();

    const handleAction = (id: number, status: 'Approved' | 'Rejected') => {
        reviewMutation.mutate({ id, status });
    };

    const { user } = useAuth();

    const isOwnRequest = (requesterId: number) =>
        user?.userId != null && user.userId === requesterId;


    const [statusFilter, setStatusFilter] = useState('All');
    const [page, setPage] = useState(1);

    const filteredRequests = requests.filter(r =>
        statusFilter === 'All' || r.status === statusFilter
    );

    const pageSize = 10;
    const totalPages = Math.max(
        1,
        Math.ceil(filteredRequests.length / pageSize)
    );
    const currentPage = Math.min(page, totalPages);

    const pageRequests = filteredRequests.slice(
        (currentPage - 1) * pageSize,
        currentPage * pageSize
    );

    const columns: ColumnDef<LeaveResponse>[] = [
        { header: 'Employee', accessor: 'employeeName' },
        { header: 'Type', accessor: 'type' },
        {
            header: 'Dates',
            accessor: 'startDate',
            render: r => `${new Date(r.startDate).toLocaleDateString()} - ${new Date(r.endDate).toLocaleDateString()}`
        },
        { header: 'Status', accessor: 'status' },
        {
            header: 'Actions',
            accessor: 'absenceId',
            render: r => r.status === 'Pending' ? (
                <div className="flex gap-3">
                    <button
                        onClick={() => handleAction(r.absenceId, 'Approved')}
                        disabled={reviewMutation.isPending || isOwnRequest(r.userId)}
                        className="text-emerald-600 disabled:opacity-40"
                    >
                        Approve
                    </button>
                    <button
                        onClick={() => handleAction(r.absenceId, 'Rejected')}
                        disabled={reviewMutation.isPending || isOwnRequest(r.userId)}
                        className="text-red-600 disabled:opacity-40"
                    >
                        Reject
                    </button>
                </div>
            ) : null
        }
    ];

    return (
        <div className="max-w-4xl mx-auto p-6">
            {/* Header */}
            <h2 className="text-2xl font-bold text-slate-800 mb-6">Pending Leave Approvals</h2>
            
            <select
                value={statusFilter}
                onChange={e => {
                    setStatusFilter(e.target.value);
                    setPage(1);
                }}
                className="rounded border border-slate-300 p-2 text-sm"
            >
                <option value="All">All requests</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
            </select>

            {/* Data Table */}
            <DataTable
                data={pageRequests}
                columns={columns}
                isLoading={isLoading}
                page={currentPage}
                totalPages={totalPages}
                totalCount={filteredRequests.length}
                onPageChange={setPage}
            />
        </div>
    );
}