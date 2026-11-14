import type { AuditLog } from "../types/auditLog";
import type { CursorPagedResult } from "../types/pagination";
import apiClient from "./apiClient";
import { deviceTimeZone } from "../utils/dateTime";

export const auditService = {
    getLogs: async (
        pageSize: number = 10,
        lastCreatedAt: string | null = null,
        lastLogId: number | null = null,
        search?: string,
        startDate?: string,
        endDate?: string,
        signal?: AbortSignal
    ): Promise<CursorPagedResult<AuditLog>> => {
        const params = new URLSearchParams({
            pageSize: pageSize.toString()
        });
        
        if (lastCreatedAt) params.append('lastCreatedAt', lastCreatedAt);
        if (lastLogId !== null) params.append('lastLogId', lastLogId.toString());
        if (search) params.append('searchTerm', search);
        params.append('timeZoneId', deviceTimeZone());
        if (startDate) params.append('startDate', startDate)
        if (endDate) params.append('endDate', endDate)

        const response = await apiClient.get(`/auditLogs?${params.toString()}`, { signal });
        return response.data;
    } 
};