import { useUrlString, useUrlQuery } from '../hooks/useUrlState';
import { useMemo } from "react";
import type { AuditLog } from "../types/auditLog";
import { CursorDataTable, type ColumnDef } from "../components/Ui/CursorDataTable";
import { useAuditLogsQuery } from "../hooks/queries/useAuditQueries";

import DatePicker from "react-datepicker";
import { useUrlCursor } from '../hooks/useUrlCursor';
import "react-datepicker/dist/react-datepicker.css";

export default function AuditLogsList() {
    const [searchTerm] = useUrlString('search');

    const [from] = useUrlString('from');
    const [to] = useUrlString('to');
    const startDate = /^\d{4}-\d{2}-\d{2}$/.test(from) ? new Date(from+'T12:00:00') : null;
    const endDate = /^\d{4}-\d{2}-\d{2}$/.test(to) ? new Date(to+'T12:00:00') : null;

    const { patch } = useUrlQuery();
    const cursor = useUrlCursor();
    const currentCursor = { createdAt: cursor.cursor.createdAt, logId: cursor.cursor.id };

    const formattedStart = startDate ? startDate.toISOString().split('T')[0] : '';
    const formattedEnd = endDate ? endDate.toISOString().split('T')[0] : '';

    const { data, isLoading } = useAuditLogsQuery(
        10,
        currentCursor.createdAt,
        currentCursor.logId,
        searchTerm,
        formattedStart,
        formattedEnd
    );

    const logs = data?.items || [];
    const hasMorePages = data?.hasMorePages || false;

    const handleSearchChange = (value: string) => patch({ search: value, cursorTrail: null, cursorPage: null });
    const handleNext = () => cursor.next({ createdAt: data?.nextCreatedAt ?? null, id: data?.nextLogId ?? null });
    const handlePrevious = () => cursor.previous();

    const columns = useMemo<ColumnDef<AuditLog>[]>(() => [
        { header: 'Date', accessor: 'createdAt', render: (l) => new Date(l.createdAt).toLocaleString() },
        { header: 'UserId', accessor: 'userId' },
        { header: 'UserName', accessor: 'username'},
        { header: 'Entity', accessor: 'entityType' },
        { header: 'Action', accessor: 'action', render: (l) => (
            <span className={`px-2 py-1 rounded text-xs font-semibold ${l.action === 'Delete' ? 'bg-red-100 text-red-700' : l.action === 'Create' ? 'bg-blue-100 text-blue-700' : 'bg-emerald-100 text-emerald-700'}`}>
                {l.action}
            </span>
        )},
        { header: 'Details', accessor: 'changesMade', className: 'max-w-xs truncate' }
    ], []);

    return (
        <div className="space-y-6">
            <div className="mb-6">
                <h2 className="text-2xl font-bold text-slate-800">System Audit Logs</h2>
                <p className="text-sm text-slate-500">Track system events, user activity, and security alerts.</p>
            </div>

            {/* Filter Controls */}
            <div className="flex flex-col sm:flex-row gap-4 md-4">
                <input 
                    className="border border-slate-300 rounded p-2 text-sm w-full max-w-sm"
                    placeholder="Filter by action or user..."
                    value={searchTerm}
                    onChange={(e) => handleSearchChange(e.target.value)}
                />

                <div className="border border-slate-300 rounded bg-white">
                    <DatePicker
                        selectsRange={true}
                        startDate={startDate}
                        endDate={endDate}
                        onChange={(update: [Date | null, Date | null]) => {
                            patch({ from: update[0] ? update[0].toISOString().slice(0,10) : null, to: update[1] ? update[1].toISOString().slice(0,10) : null, cursorTrail: null, cursorPage: null });
                        }}
                        maxDate={new Date()}
                        placeholderText="Select date range..."
                        className="p-2 text-sm outline-none w-64"
                        isClearable={true}
                    />
                </div>
            </div>

            <CursorDataTable 
                data={logs}
                columns={columns}
                isLoading={isLoading}
                hasMorePages={hasMorePages}
                isFirstPage={cursor.isFirstPage}
                onNext={handleNext}
                onPrevious={handlePrevious}
            />
        </div>
    );
}
