import { useEffect, useMemo, useState } from "react";
import type { AuditLog } from "../types/auditLog";
import { CursorDataTable, type ColumnDef } from "../components/Ui/CursorDataTable";
import { useAuditLogsQuery } from "../hooks/queries/useAuditQueries";

import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";

type CursorState = {
    createdAt: string | null;
    logId: number | null;
};

export default function AuditLogsList() {
    const [searchTerm, setSearchTerm] = useState('');

    const [dateRange, setDateRange] = useState<[Date | null, Date | null]>([null, null]);
    const [startDate, endDate] = dateRange;

    const [cursorHistory, setCursorHistory] = useState<CursorState[]>([{ createdAt: null, logId: null }]);
    const [currentIndex, setCurrentIndex] = useState(0);

    const currentCursor = cursorHistory[currentIndex];

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

    useEffect(() => {
        if (data?.hasMorePages && cursorHistory.length === currentIndex + 1) {
            setCursorHistory(prev => [
                ...prev, 
                { 
                    createdAt: data.nextCreatedAt ?? null, 
                    logId: data.nextLogId ?? null 
                }
            ]);
        }
    }, [data, currentIndex, cursorHistory.length]);

    const handleSearchChange = (val: string) => {
        setSearchTerm(val);
        setCursorHistory([{ createdAt: null, logId: null }]);
        setCurrentIndex(0); 
    };

    const handleNext = () => setCurrentIndex(prev => prev + 1);
    const handlePrevious = () => setCurrentIndex(prev => prev - 1);

    const resetFilters = () => {
        setCursorHistory([{ createdAt: null, logId: null }]);
        setCurrentIndex(0);
    };

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
            <h2 className="text-2xl font-bold text-slate-800">System Audit Logs</h2>

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
                            setDateRange(update);
                            resetFilters();
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
                isFirstPage={currentIndex === 0}
                onNext={handleNext}
                onPrevious={handlePrevious}
            />
        </div>
    );
}