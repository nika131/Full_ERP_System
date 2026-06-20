import type { AuditLog } from "../types/auditLog";
import type { CursorPagedResult } from "../types/pagination";
import apiClient from "./apiClient";

export const auditService = {
    getLogs: async (
        pageSize: number = 10,
        lastCreatedAt: string | null = null,
        lastLogId: number | null = null,
        search?: string,
        signal?: AbortSignal
    ): Promise<CursorPagedResult<AuditLog>> => {
        const params = new URLSearchParams({
            pageSize: pageSize.toString()
        });
        
        if (lastCreatedAt) params.append('lastCreatedAt', lastCreatedAt);
        if (lastLogId !== null) params.append('lastLogId', lastLogId.toString());
        if (search) params.append('searchTerm', search);

        const response = await apiClient.get(`/auditLogs?${params.toString()}`, { signal });
        return response.data;
    } 
};