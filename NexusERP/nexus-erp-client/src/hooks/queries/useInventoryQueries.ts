import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productService, type TransactionPayLoad } from '../../api/productService';
import type { ProductFormData } from '../../schemas/productSchema';

export const useProductsQuery = (page: number, limit: number, search: string, category: string, supplier: string) => {
    return useQuery({
        queryKey: ['products', { page, limit, search, category, supplier }],
        queryFn: ({ signal }) => productService.getProducts(page, limit, search, category, supplier, signal),
        staleTime: 60 * 1000, 
    });
};

export const useSaveProductMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (payload: ProductFormData & { productId?: number }) => productService.saveProduct(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['products'] });
        }
    });
};

export const useDeleteProductMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (productId: number) => productService.deleteProduct(productId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['products'] });
        }
    });
};

export const useTransactionMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (payload: TransactionPayLoad) => productService.makeTransaction(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['products'] });
            queryClient.invalidateQueries({ queryKey: ['dashboard'] });
            queryClient.invalidateQueries({ queryKey: ['transactions'] });
        }
    });
};



