import apiClient from './apiClient';

export interface StoreResponse {
    storeId: number;
    name: string;
    latitude: number;
    longitude: number;
    address: string;
    isActive: boolean;
}

export const storeService = {
    getNearbyStores: async (lat: number, lon: number, radius: number, signal?: AbortSignal) => {
        const response = await apiClient.get<StoreResponse[]>(
            `/stores/nearby?latitude=${lat}&longitude=${lon}&radiusInMeters=${radius}`,
            { signal }
        );
        return response.data;
    }
};