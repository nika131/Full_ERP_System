import type { ChartData } from "recharts/types/state/chartDataSlice";
import type { DashbaordStats, TopProduct } from "../types/dashboard";
import type { CursorPagedResult } from "../types/pagination";
import type { Transaction } from "../types/transaction";
import apiClient from "./apiClient";
import type { DashboardFilters } from "../pages/Dashboard";

export const dashboaredService = {
    getStatistics: async (filters: DashboardFilters): Promise<DashbaordStats> => {
        const response = await apiClient.get('/Dashboard/statistics', {
            params: filters,
            paramsSerializer: {indexes: null }
        });
        return response.data;
    },

    getTransactions: async (
        pageSize: number = 10,
        lastCreatedAt: string | null = null,
        lastTransactionId: number | null = null,
        searchTerm: string | null = null,
        filters: DashboardFilters,
        signal?: AbortSignal
    ): Promise<CursorPagedResult<Transaction>> => {
        
        const params = new URLSearchParams();
        params.append('pageSize', pageSize.toString());
        
        if (lastCreatedAt) params.append('lastCreatedAt', lastCreatedAt);
        if (lastTransactionId !== null) params.append('lastTransactionId', lastTransactionId.toString());
        if (searchTerm !== null) params.append('searchTerm', searchTerm);

        if (filters.startDate) params.append('startDate', filters.startDate);
        if (filters.endDate) params.append('endDate', filters.endDate);

        if (filters.startHour) params.append('startHour', filters.startHour);
        if (filters.endHour) params.append('endHour', filters.endHour);

        if (filters.storeIds?.length) {
            filters.storeIds.forEach(id => params.append('storeIds', String(id)));
        }

        if (filters.categoryIds?.length) {
            filters.categoryIds.forEach(id => params.append('categoryIds', String(id)));
        }

        if (filters.supplierIds?.length) {
            filters.supplierIds.forEach(id => params.append('supplierIds', String(id)));
        }

        if (filters.employeeIds?.length) {
            filters.employeeIds.forEach(id => params.append('employeeIds', String(id)));
        }

        const response = await apiClient.get(`/reports`, { params, signal });
        return response.data;
    },

    getChartData: async (filters: DashboardFilters): Promise<ChartData[]> => {
        const response = await apiClient.get('/Dashboard/revenueChart', { 
            params: filters, 
            paramsSerializer: { indexes: null }
        });
        return response.data;
    },

    getTopProducts: async (filters: DashboardFilters): Promise<TopProduct[]> => {
        const response = await apiClient.get('/Dashboard/top-Products', { 
            params: filters, 
            paramsSerializer: { indexes: null }
        });
        return response.data;
    },
};