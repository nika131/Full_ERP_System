import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { categoryService } from '../../api/categoryService';
import type { CategoryFormData } from '../../schemas/categorySchema';
import { Signal } from 'lucide-react';

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
        }
    });
};

export const useDeleteCategoryMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (categoryId: number) => categoryService.deleteCategory(categoryId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['categories'] });
        }
    });
};

export const useLookupCategoriesQuery = () => {
    return useQuery({
        queryKey: ['lookupCategories'],
        queryFn: ()  => categoryService.getLookupCategories(),
        staleTime: 5 * 60 * 1000,
    })
}