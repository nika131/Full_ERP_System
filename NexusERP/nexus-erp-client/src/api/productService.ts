import apiClient from "./apiClient";
import { type Product } from "../types/product";

export const productService = {
    getAll: async (): Promise<Product[]> => {
        const response = await apiClient.get('/products');
        return response.data;
    },

    search: async (keyword: string): Promise<Product[]> => {
        const response = await apiClient.get(`/products/search?keyword=${keyword}`);
        return response.data;
    }
}