import { useEffect, useMemo, useState } from "react";
import type { AuditLog } from "../types/auditLog";
import { auditService } from "../api/auditService";
import { CursorDataTable, type ColumnDef } from "../components/Ui/CursorDataTable";

type CursorState = {
    createdAt: string | null;
    logId: number | null;
};

export default function AuditLogsList() {
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    
    const [cursorHistory, setCursorHistory] = useState<CursorState[]>([{ createdAt: null, logId: null }]);
    const [currentIndex, setCurrentIndex] = useState(0);
    const [hasMorePages, setHasMorePages] = useState(false);

    useEffect(() => {
        const controller = new AbortController();
        const timer = setTimeout(() => {
            loadLogs(controller.signal);
        }, 300);
        return () => { clearTimeout(timer); controller.abort(); };
    }, [currentIndex, searchTerm]); 

    const loadLogs = async (signal: AbortSignal) => {
        try {
            setIsLoading(true);
            const currentCursor = cursorHistory[currentIndex];
            
            const data = await auditService.getLogs(
                10, 
                currentCursor.createdAt, 
                currentCursor.logId, 
                searchTerm, 
                signal
            );
            
            if (!signal.aborted) {
                setLogs(data.items);
                setHasMorePages(data.hasMorePages);
                
                if (data.hasMorePages && cursorHistory.length === currentIndex + 1) {
                    setCursorHistory(prev => [
                        ...prev, 
                        { 
                            createdAt: data.nextCreatedAt ?? null, 
                            logId: data.nextLogId ?? null 
                        }
                    ]);
                }
            }
        } catch (err: any) {
            if (!signal.aborted && err.name !== 'CanceledError') console.error(err);
        } finally {
            if (!signal.aborted) setIsLoading(false);
        }
    };

    const handleSearchChange = (val: string) => {
        setSearchTerm(val);
        setCursorHistory([{ createdAt: null, logId: null }]);
        setCurrentIndex(0); 
    };

    const handleNext = () => setCurrentIndex(prev => prev + 1);
    const handlePrevious = () => setCurrentIndex(prev => prev - 1);

    const columns = useMemo<ColumnDef<AuditLog>[]>(() => [
        { header: 'Date', accessor: 'createdAt', render: (l) => new Date(l.createdAt).toLocaleString() },
        { header: 'User', accessor: 'userId' },
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
            <input 
                className="border border-slate-300 rounded p-2 text-sm w-full max-w-sm"
                placeholder="Filter by action or user..."
                value={searchTerm}
                onChange={(e) => handleSearchChange(e.target.value)}
            />
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
};