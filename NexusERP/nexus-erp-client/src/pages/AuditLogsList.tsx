import { useEffect, useMemo, useState } from "react";
import type { AuditLog } from "../types/auditLog";
import { auditService } from "../api/auditService";
import { DataTable, type ColumnDef } from "../components/Ui/DataTable";


export default function AuditLogsList() {
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [searchTerm, setSearchTerm] = useState('');

    useEffect(() => {
        const controller = new AbortController();
        const timer = setTimeout(() => {
            loadLogs(controller.signal)
        }, 300);
        return () => { clearTimeout(timer); controller.abort(); };
    }, [page, searchTerm]);

    const loadLogs = async (signal: AbortSignal) => {
        try {
            setIsLoading(true);
            const data = await auditService.getLogs(page, 10, searchTerm, signal);
            setLogs(data.items);
            setTotalPages(data.totalPages);
            setTotalCount(data.totalCount);
        } catch (err: any) {
            if (err.name !== 'CanceledError') console.error(err);
        } finally {
            setIsLoading(false);
        }
    }

    const columns = useMemo<ColumnDef<AuditLog>[]>(() => [
    { header: 'Date', accessor: 'createdAt', render: (l) => new Date(l.createdAt).toLocaleString() },
    { header: 'User', accessor: 'userId' },
    { header: 'Entity', accessor: 'entityType' },
    { header: 'Action', accessor: 'action', render: (l) => (
        <span className={`px-2 py-1 rounded text-xs font-semibold ${l.action === 'Delete' ? 'bg-red-100 text-red-700' : 'bg-blue-100 text-blue-700'}`}>
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
        onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
      />
      <DataTable 
        data={logs}
        columns={columns}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        totalCount={totalCount}
        onPageChange={setPage}
      />
    </div>
  );
};