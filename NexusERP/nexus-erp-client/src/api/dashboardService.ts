import type { ChartData } from "recharts/types/state/chartDataSlice";
import type { DashbaordStats, TopProduct } from "../types/dashboard";
import type { PagedResult } from "../types/pagination";
import type { Transaction } from "../types/transaction";
import apiClient from "./apiClient";

export const dashboaredService = {
    getStatistics: async (): Promise<DashbaordStats> => {
        const response = await apiClient.get('/Dashboared/statistics');
        return response.data;
    },

    getTransactions: async (
        pageNumber: number = 1,
        pageSize: number = 10,
        searchTerm?: string,
        typeFilter: string = "All",
        signal?: AbortSignal
    ): Promise<PagedResult<Transaction>> => {
        const params = new URLSearchParams();
        params.append('pageNumber', pageNumber.toString());
        params.append('pageSize', pageSize.toString());
        params.append('typeFillter', typeFilter);
        if (searchTerm) params.append('searchTerm', searchTerm);

        const response = await apiClient.get(`/reports`, { params, signal });
        return response.data;
    },

    getChartData: async (): Promise<ChartData[]> => {
        const response = await apiClient.get('/dashboard/chart');
        return response.data;
    },

    getTopProducts: async (): Promise<TopProduct[]> => {
        const response = await apiClient.get('/dashboard/top-products');
        return response.data;
    },
};