import { useQuery } from '@tanstack/react-query';
import { auditService } from '../../api/auditService';

export const useAuditLogsQuery = (
    limit: number,
    createdAt: string | null,
    logId: number | null,
    search: string,
    startDate: string,
    endDate: string
) => {
    return useQuery({
        queryKey: ['auditLogs', { limit, createdAt, logId, search, startDate, endDate }],
        queryFn: ({ signal }) => auditService.getLogs(limit, createdAt, logId, search, startDate, endDate, signal),
        staleTime: 0, 
    });
};