import type { CategoryResponse } from "../types/category";
import type { PagedResult } from "../types/pagination";
import apiClient from "./apiClient";

export const categoryService = {
    getCategories: async (page: number = 1, pageSize: number = 10, search?: string, signal?: AbortSignal): Promise<PagedResult<CategoryResponse>> => {
        const params = new URLSearchParams({ page: page.toString(), pageSize: pageSize.toString() });
        if (search) params.append('search', search);
        const response = await apiClient.get(`/categories?${params.toString()}`, { signal });
        return response.data;
    },

    saveCategory: async (data: any):
        Promise<void> => apiClient.post('/categories/upsert', data),

    deleteCategory: async (id: number): 
        Promise<void> => apiClient.delete(`/categories/${id}`),
}