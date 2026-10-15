import apiClient from "./client";
import type { Product } from "../types";

export interface ProductsPage {
  items: Product[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export const productService = {
  search: async (
    searchTerm: string,
    pageSize = 30,
    excludeIds: number[] = []
  ): Promise<Product[]> => {
    const res = await apiClient.get("/products", {
      params: {
        page: 1,
        pageSize,
        searchTerm,
        excludeIds: excludeIds.length > 0 ? excludeIds.join(",") : undefined,
      },
    });
    return (res.data as ProductsPage).items;
  },

};
