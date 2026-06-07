import type { PagedResult } from "../types/pagination";
import type { Supplier } from "../types/supplier";
import apiClient from "./apiClient";


export const supplierService = {
    getSuppliers: async (
        page: number = 1,
        pageSize: number = 10,
        search?: string,
        signal?: AbortSignal
    ): Promise<PagedResult<Supplier>> => {
        const params = new URLSearchParams();
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        if (search) params.append('search', search);

        const response = await apiClient.get(`/suppliers?${params.toString()}`, { signal });
        return response.data;
    },

    saveSupplier: async (supplierData: any): Promise<void> => {
        await apiClient.post('/suppliers/upsert', supplierData);
    },

    deleteSupplier: async (supplierId: number): Promise<void> => {
        await apiClient.delete(`/suppliers/${supplierId}`);
    }
};