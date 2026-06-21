import { useQuery } from '@tanstack/react-query';
import { auditService } from '../../api/auditService';

export const useAuditLogsQuery = (
    limit: number,
    createdAt: string | null,
    logId: number | null,
    search: string
) => {
    return useQuery({
        queryKey: ['auditLogs', { limit, createdAt, logId, search }],
        queryFn: ({ signal }) => auditService.getLogs(limit, createdAt, logId, search, signal),
        staleTime: 0, 
    });
};