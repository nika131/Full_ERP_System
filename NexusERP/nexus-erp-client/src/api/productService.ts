import apiClient from "./apiClient";
import { type Product } from "../types/product";
import { type PagedResult } from "../types/pagination";
import type { Category } from "../types/category";
import type { Supplier, SupplierLookup } from "../types/supplier";
import type { ChartData } from "recharts/types/state/chartDataSlice";
import type { TopProduct } from "../types/dashboard";

export interface TransactionPayLoad {
    productId: number;
    supplierId: number | null;
    transactionType: string;
    soldQty: number;
    productPrice: number;
    costPrice: number;
}

export const productService = {
    getProducts: async (
        page: number = 1, 
        pageSize: number = 10, 
        search?: string,
        signal?: AbortSignal
    ): Promise<PagedResult<Product>> => {
        const params = new URLSearchParams();
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        if (search) {
            params.append('search', search);
        }

        const response = await apiClient.get(`/products?${params.toString()}`, { signal });
        return response.data;
    },

    saveProduct: async (productData: any): Promise<void> => {
        if (productData.productId) {
            await apiClient.put(`/products/${productData.productId}`, productData);
        } else {
            await apiClient.post('/products', productData);
        }
    },

    getCategories: async (): Promise<Category[]> => {
        const response = await apiClient.get('/categories');
        return response.data;
    },

    getSuppliers: async (): Promise<SupplierLookup[]> => {
        const response = await apiClient.get('/suppliers/lookup');
        return response.data;
    },

    deleteProduct: async (productId: number): Promise<void> => {
        await apiClient.delete(`/products/${productId}`);
    },

    makeTransaction: async (payload: TransactionPayLoad): Promise<void> => {
        await apiClient.post('/products/transaction', payload);
    },


};