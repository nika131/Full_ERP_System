import type { StoreFormData } from '../schemas/storeSchema';
import apiClient from './apiClient';
import type { PagedResult } from '../types/pagination';

export interface StoreResponse {
    storeId: number;
    name: string;
    latitude: number;
    longitude: number;
    address: string;
    isActive: boolean;
}

export const storeService = {
    getPagedStores: async (
        page: number,
        pageSize: number,
        searchTerm?: string,
        signal?: AbortSignal
    ): Promise<PagedResult<StoreResponse>> => {
        const params = new URLSearchParams();
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        if (searchTerm) params.append('searchTerm', searchTerm);

        const response = await apiClient.get<PagedResult<StoreResponse>>('/stores/paged', { params, signal });
        return response.data;
    },

    getAllStores: async (): Promise<StoreResponse[]> => {
        const response = await apiClient.get<StoreResponse[]>(`/stores`);
        return response.data;
    },

    getStoreById: async (id: number): Promise<StoreResponse> => {
        const response = await apiClient.get<StoreResponse>(`/stores/${id}`);
        return response.data;
    },

    createStore: async (data: StoreFormData): Promise<StoreResponse> => {
        const response = await apiClient.post<StoreResponse>(`/stores`, data);
        return response.data;
    },

    updateStore: async (id: number, data: StoreFormData): Promise<void> => {
        await apiClient.put(`/stores/${id}`, data);
    },

    getNearbyStores: async (lat: number, lon: number, radius: number, signal?: AbortSignal) => {
        const response = await apiClient.get<StoreResponse[]>(
            `/stores/nearby?latitude=${lat}&longitude=${lon}&radiusInMeters=${radius}`,
            { signal }
        );
        return response.data;
    },

    getLookupStores: async (): Promise<StoreResponse[]> => {
        const response = await apiClient.get<StoreResponse[]>(`/stores`);
        return response.data;
    }
};

