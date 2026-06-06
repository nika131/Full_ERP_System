import type { AuditLog } from "../types/auditLog";
import type { PagedResult } from "../types/pagination"
import apiClient from "./apiClient";

export const auditService = {
    getLogs: async (page: number = 1, pageSize: number = 10, search?: string, signal?: AbortSignal): Promise<PagedResult<AuditLog>> => {
        const params = new URLSearchParams({
            pageNumber: page.toString(),
            pageSize: pageSize.toString(),
            ...(search && { searchTerm: search })
        });

        const response = await apiClient.get(`/auditLogs?${params.toString()}`, { signal });
        return response.data;
    } 
};