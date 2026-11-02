import { Permissions } from '../constants/permissions';
import { useUrlBoolean } from '../hooks/useUrlState';
import { useAuth } from '../context/AuthContext';
import { LeaveRequestForm } from '../components/hr/LeaveRequestForm';
import { useQueryClient } from '@tanstack/react-query';
import { useMyLeaveHistoryQuery, useMySalaryHistoryQuery } from '../hooks/queries/useProfileQueries';
import { useEffect } from 'react';
import { createPortal } from 'react-dom';
import { X } from 'lucide-react';

export default function Profile() {
    const { user, hasPermission } = useAuth();
    const canViewAbsences = hasPermission(Permissions.ViewAbsences);
    const canRequestAbsences = hasPermission(Permissions.RequestAbsences);
    const queryClient = useQueryClient();
    
    const [isRequesting, setIsRequesting] = useUrlBoolean('requestLeave');

    useEffect(() => {
        if (!isRequesting) return;

        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape') {
                setIsRequesting(false);
            }
        };

        document.addEventListener('keydown', handleEscape);

        return () => {
            document.removeEventListener('keydown', handleEscape);
        };
    }, [isRequesting, setIsRequesting]);

    const { data: history = [], isLoading: isLoadingHistory } = useMyLeaveHistoryQuery(canViewAbsences);
    const { data: salaryHistory = [], isLoading: isLoadingSalary } = useMySalaryHistoryQuery();

    const pendingRequests = history.filter(h => h.status === 'Pending').length;
    const approvedLeaves = history.filter(h => h.status === 'Approved').length;

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'Approved': return 'bg-emerald-100 text-emerald-800 border-emerald-200';
            case 'Rejected': return 'bg-red-100 text-red-800 border-red-200';
            default: return 'bg-amber-100 text-amber-800 border-amber-200';
        }
    };

    return (
        <div className="space-y-6 max-w-6xl mx-auto">
            {/* Header */}
            <div className="relative flex flex-col sm:flex-row justify-center items-center w-full min-h-[40px] gap-4 sm:gap-0 mt-4 sm:mt-0">
                <div className="text-center">
                    <h2 className="text-2xl font-bold text-slate-800">My Profile</h2>
                    <p className="text-sm text-slate-500">Manage your account and time off.</p>
                </div>
                {canRequestAbsences && <button 
                    onClick={() => setIsRequesting(!isRequesting)}
                    className="sm:absolute sm:right-0 bg-slate-800 hover:bg-slate-700 text-white px-4 py-2 rounded-md text-sm font-medium transition-colors w-full sm:w-auto"
                >
                    {isRequesting ? 'Cancel Request' : '+ Request Time Off'}
                </button>}
            </div>

            {/* Statistics Row */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                    <p className="text-sm font-medium text-slate-500">Current Role</p>
                    <p className="text-2xl font-bold text-slate-800 mt-1">{user?.role || 'Unassigned'}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                    <p className="text-sm font-medium text-slate-500">Approved Absences</p>
                    <p className="text-2xl font-bold text-emerald-600 mt-1">{approvedLeaves}</p>
                </div>
                <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                    <p className="text-sm font-medium text-slate-500">Pending Requests</p>
                    <p className="text-2xl font-bold text-amber-600 mt-1">{pendingRequests}</p>
                </div>
            </div>

           
            {/* Leave Request Modal */}
            {canRequestAbsences && isRequesting && createPortal(
                <div className="fixed inset-0 z-[70] flex items-center justify-center bg-slate-950/60 p-3 sm:p-4">
                    <div
                        role="dialog"
                        aria-modal="true"
                        aria-labelledby="leave-request-title"
                        className="flex max-h-[calc(100dvh-1.5rem)] w-full max-w-lg flex-col overflow-hidden rounded-2xl bg-white text-left shadow-2xl"
                    >
                        {/* Modal Header */}
                        <div className="flex shrink-0 items-center justify-between gap-3 border-b border-slate-200 px-4 py-4 sm:px-6">
                            <div className="min-w-0">
                                <h3
                                    id="leave-request-title"
                                    className="!m-0 text-lg font-bold leading-6 text-slate-900"
                                >
                                    Request Time Off
                                </h3>
                                <p className="mt-1 text-xs text-slate-500">
                                    Select your leave type and dates.
                                </p>
                            </div>

                            <button
                                type="button"
                                onClick={() => setIsRequesting(false)}
                                aria-label="Close leave request"
                                className="shrink-0 rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                            >
                                <X size={19} />
                            </button>
                        </div>

                        {/* Scrollable Form */}
                        <div className="tenex-scroll min-h-0 flex-1 overflow-y-auto p-4 sm:p-6">
                            <LeaveRequestForm
                                onSuccess={() => {
                                    setIsRequesting(false);
                                    queryClient.invalidateQueries({
                                        queryKey: ['profile', 'leaveHistory']
                                    });
                                    queryClient.invalidateQueries({
                                        queryKey: ['pendingLeaves']
                                    });
                                }}
                                onCancel={() => setIsRequesting(false)}
                            />
                        </div>
                    </div>
                </div>,
                document.body
            )}


            {/* History Table */}
            <div className="bg-white rounded-lg shadow-sm border border-slate-200 overflow-hidden">
                <div className="p-4 border-b border-slate-200 bg-slate-50">
                    <h3 className="font-bold text-slate-800">Leave History</h3>
                </div>
                {isLoadingHistory ? (
                    <div className="p-8 text-center text-slate-500">Loading history...</div>
                ) : (
                    <div className='overflow-x-auto w-full'>
                        <table className="w-full text-left text-sm whitespace-nowrap">
                            <thead className="bg-white text-slate-500 border-b border-slate-200">
                                <tr>
                                    <th className="p-4 font-medium">Type</th>
                                    <th className="p-4 font-medium">Start Date</th>
                                    <th className="p-4 font-medium">End Date</th>
                                    <th className="p-4 font-medium">Status</th>
                                    <th className="p-4 font-medium">Manager Notes</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-100">
                                {history.map((record) => (
                                    <tr key={record.absenceId} className="hover:bg-slate-50">
                                        <td className="p-4 font-medium text-slate-800">{record.type}</td>
                                        <td className="p-4 text-slate-600">{new Date(record.startDate).toLocaleDateString()}</td>
                                        <td className="p-4 text-slate-600">{new Date(record.endDate).toLocaleDateString()}</td>
                                        <td className="p-4">
                                            <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(record.status)}`}>
                                                {record.status}
                                            </span>
                                        </td>
                                        <td className="p-4 text-slate-500 italic">
                                            {record.reviewerComments || '-'}
                                        </td>
                                    </tr>
                                ))}
                                {history.length === 0 && (
                                    <tr>
                                        <td colSpan={5} className="p-8 text-center text-slate-500">
                                            No leave history found.
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>

            {/* Salary History */}
            <div>
                <h4 className="text-sm font-bold text-slate-800 mb-2">Salary History</h4>
                {isLoadingSalary ? <p className="text-sm text-slate-500">Loading...</p> : (
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
}
