import apiClient from "./apiClient";
import { type Product } from "../types/product";
import { type PageResult } from "../types/pagination";


export const productService = {
    getProducts: async (page: number = 1, pageSize: number = 10, search?: string): Promise<PageResult<Product>> => {
        const params = new URLSearchParams();
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        if (search) {
            params.append('search', search);
        }

        const response = await apiClient.get(`/products?${params.toString()}`)
        return response.data;
    }
};