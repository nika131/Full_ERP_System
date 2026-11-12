import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { categoryService } from '../../api/categoryService';
import type { CategoryFormData } from '../../schemas/categorySchema';

export const useCategoriesQuery = (page: number, limit: number, search: string) => {
    return useQuery({
        queryKey: ['categories', { page, limit, search }],
        queryFn: ({ signal }) => categoryService.getCategories(page, limit, search, signal),
        staleTime: 5 * 60 * 1000, 
    });
};

export const useSaveCategoryMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (payload: CategoryFormData & { categoryId?: number }) => categoryService.saveCategory(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['categories'] });
            queryClient.invalidateQueries({ queryKey: ['lookupCategories'] });
        }
    });
};

export const useDeleteCategoryMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (categoryId: number) => categoryService.deleteCategory(categoryId),
        onSuccess: () => {
            queryClient.invalidateQueries(); // product classification and reports also change
        }
    });
};

export const useLookupCategoriesQuery = (enabled: boolean = true) => {
    return useQuery({
        enabled,
        queryKey: ['lookupCategories'],
        queryFn: ()  => categoryService.getLookupCategories(),
        staleTime: 5 * 60 * 1000,
        refetchOnWindowFocus: 'always',
    })
}

export const useCategoryDeletionImpact = (id: number, enabled: boolean) =>
    useQuery({
        queryKey: ['categories', 'deletionImpact', id],
        queryFn: () => categoryService.getDeletionImpact(id),
        enabled: enabled && id > 0,
        staleTime: 0
    });
