import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import { DataTable, type ColumnDef } from '../components/Ui/DataTable';

interface ShiftAudit {
    shiftId: number;
    storeName: string;
    cashierName: string;
    startTime: string;
    endTime: string | null;
    startingCash: number;
    expectedEndingCash: number;
    actualEndingCash: number | null;
    variance: number;
    totalSales: number;
    status: string;
}

export default function ShiftAuditList() {
    const [page, setPage] = useState(1);

    const { data, isLoading } = useQuery({
        queryKey: ['shift-audits', page],
        queryFn: async () => {
            const res = await apiClient.get(`/reports/shifts?page=${page}&pageSize=15`);
            return res.data;
        }
    });

    const columns = useMemo<ColumnDef<ShiftAudit>[]>(() => [
        { header: 'ID', accessor: 'shiftId', className: 'w-16' },
        { header: 'Store', accessor: 'storeName', className: 'font-medium' },
        { header: 'Cashier', accessor: 'cashierName' },
        { 
            header: 'Start Time', 
            accessor: 'startTime',
            render: (item) => new Date(item.startTime).toLocaleString()
        },
        { 
            header: 'Status', 
            accessor: 'status',
            render: (item) => (
                <span className={`px-2 py-1 rounded text-xs font-semibold ${item.status === 'Open' ? 'bg-blue-100 text-blue-700' : 'bg-slate-100 text-slate-700'}`}>
                    {item.status}
                </span>
            )
        },
        { 
            header: 'Sales', 
            accessor: 'totalSales',
            className: 'text-right font-medium',
            render: (item) => `$${item.totalSales.toFixed(2)}`
        },
        { 
            header: 'Variance', 
            accessor: 'variance',
            className: 'text-right font-bold',
            render: (item) => (
                <span className={item.variance < 0 ? 'text-red-600' : item.variance > 0 ? 'text-amber-600' : 'text-emerald-600'}>
                    ${item.variance.toFixed(2)}
                </span>
            )
        }
    ], []);

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-2xl font-bold text-slate-800">Shift Audits</h2>
                <p className="text-sm text-slate-500">Monitor register sessions, cash discrepancies, and cashier performance.</p>
            </div>

            <div className="bg-white rounded-lg shadow-sm border border-slate-200">
                <DataTable 
                    data={data?.items || []}
                    columns={columns}
                    isLoading={isLoading}
                    page={page}
                    totalPages={data?.totalPages || 1}
                    totalCount={data?.totalCount || 0}
                    onPageChange={setPage}
                />
            </div>
        </div>
    );
}