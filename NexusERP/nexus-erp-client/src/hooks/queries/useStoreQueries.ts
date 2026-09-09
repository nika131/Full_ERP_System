import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { storeService } from '../../api/storeService';
import type { StoreFormData } from '../../schemas/storeSchema';

export const usePagedStoresQuery = (page: number, limit: number, search: string) => {
    return useQuery({
        queryKey: ['stores', 'paged', { page, limit, search }],
        queryFn: ({ signal }) => storeService.getPagedStores(page, limit, search, signal),
        staleTime: 60 * 1000,
    })
}

export const useAllStoresQuery = () => {
    return useQuery({
        queryKey: ['stores', 'all'],
        queryFn: () => storeService.getAllStores(),
        staleTime: 60 * 1000,
    });
};

export const useCreateStoreMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (payload: StoreFormData) => storeService.createStore(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['stores'] });
            queryClient.invalidateQueries({ queryKey: ['lookupStores'] });
        }
    })
};

export const useUpdateStoreMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, payload }: { id: number, payload: StoreFormData }) => storeService.updateStore(id, payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['stores'] });
            queryClient.invalidateQueries({ queryKey: ['lookupStores'] });
        }
    })
}

export const useNearbyStoresQuery = (lat: number, lon: number, radius: number) => {
    return useQuery({
        queryKey: ['stores', 'nearby', { lat, lon, radius }],
        queryFn: ({ signal }) => storeService.getNearbyStores(lat, lon, radius, signal),
        staleTime: 0, 
        enabled: !!lat && !!lon, 
    });
};

export const useLookupStoresQuery = () => {
    return useQuery({
        queryKey: ['lookupStores'],
        queryFn: ()  => storeService.getLookupStores(),
        staleTime: 5 * 60 * 1000,
    })
}
