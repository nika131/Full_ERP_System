import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { supplierService } from '../../api/supplierService';
import type { SupplierFormData } from '../../schemas/supplierSchema';

export const useSuppliersQuery = (page: number, limit: number, search: string) => {
    return useQuery({
        queryKey: ['suppliers', { page, limit, search }],
        queryFn: ({ signal }) => supplierService.getSuppliers(page, limit, search, signal),
        staleTime: 5 * 60 * 1000, 
    });
};

export const useSaveSupplierMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (payload: SupplierFormData & { supplierId?: number }) => supplierService.saveSupplier(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['suppliers'] });
        }
    });
};

export const useDeleteSupplierMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (supplierId: number) => supplierService.deleteSupplier(supplierId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['suppliers'] });
        }
    });
};