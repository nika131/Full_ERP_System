import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { storeService } from '../../api/storeService';
import type { StoreFormData } from '../../schemas/storeSchema';

export const useNearbyStoresQuery = (lat: number, lon: number, radius: number) => {
    return useQuery({
        queryKey: ['stores', 'nearby', { lat, lon, radius }],
        queryFn: ({ signal }) => storeService.getNearbyStores(lat, lon, radius, signal),
        staleTime: 0, 
        enabled: !!lat && !!lon, 
    });
};

/* export const useSaveStoreMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (payload: StoreFormData & { storeId?: number }) => storeService.saveStore(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['stores', 'nearby'] });
        }
    });
};*/