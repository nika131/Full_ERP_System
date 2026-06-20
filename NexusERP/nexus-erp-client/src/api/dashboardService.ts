import type { ChartData } from "recharts/types/state/chartDataSlice";
import type { DashbaordStats, TopProduct } from "../types/dashboard";
import type { CursorPagedResult } from "../types/pagination";
import type { Transaction } from "../types/transaction";
import apiClient from "./apiClient";

export const dashboaredService = {
    getStatistics: async (): Promise<DashbaordStats> => {
        const response = await apiClient.get('/Dashboard/statistics');
        return response.data;
    },

    getTransactions: async (
        pageSize: number = 10,
        lastCreatedAt: string | null = null,
        lastTransactionId: number | null = null,
        productId: number | null = null,
        supplierId: number | null = null,
        searchTransactionId: number | null = null,
        typeFilter: string = "All",
        signal?: AbortSignal
    ): Promise<CursorPagedResult<Transaction>> => {
        
        const params = new URLSearchParams();
        params.append('pageSize', pageSize.toString());
        params.append('typeFilter', typeFilter);
        
        if (lastCreatedAt) params.append('lastCreatedAt', lastCreatedAt);
        if (lastTransactionId !== null) params.append('lastTransactionId', lastTransactionId.toString());
        
        if (productId !== null) params.append('productId', productId.toString());
        if (supplierId !== null) params.append('supplierId', supplierId.toString());
        if (searchTransactionId !== null) params.append('searchTransactionId', searchTransactionId.toString());

        const response = await apiClient.get(`/reports`, { params, signal });
        return response.data;
    },

    getChartData: async (): Promise<ChartData[]> => {
        const response = await apiClient.get('/Dashboard/revenueChart');
        return response.data;
    },

    getTopProducts: async (): Promise<TopProduct[]> => {
        const response = await apiClient.get('/Dashboard/top-Products');
        return response.data;
    },
};