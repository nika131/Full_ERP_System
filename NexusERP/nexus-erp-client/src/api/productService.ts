import apiClient from "./apiClient";
import { type Product } from "../types/product";
import { type PagedResult } from "../types/pagination";

export interface TransactionPayLoad {
    productId: number;
    supplierId?: number | null; 
    transactionType: string;
    quantity: number;          
    productPrice?: number;      
    costPrice?: number;         
}

export const productService = {
    getProducts: async (
        page: number = 1, 
        pageSize: number = 10, 
        searchTerm?: string,
        categoryName?: string,
        supplierName?: string,
        signal?: AbortSignal
    ): Promise<PagedResult<Product>> => {
        const params = new URLSearchParams();
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        if (searchTerm) params.append('searchTerm', searchTerm);
        if(categoryName) params.append('categoryName', categoryName);
        if(supplierName) params.append('supplierName', supplierName);

        const response = await apiClient.get(`/products?${params.toString()}`, { signal });
        return response.data;
    },

    saveProduct: async (productData: any): Promise<void> => {
        await apiClient.post('/products/upsert', productData);
    },

    deleteProduct: async (productId: number): Promise<void> => {
        await apiClient.delete(`/products/${productId}`);
    },

    makeTransaction: async (payload: TransactionPayLoad): Promise<void> => {
        await apiClient.post('/products/transaction', payload);
    },

};