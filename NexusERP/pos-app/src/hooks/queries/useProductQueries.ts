import { useQuery } from "@tanstack/react-query";
import { productService } from "../../api/productService";
import { authService } from "../../api/authService";

export const useProductSearchQuery = (searchTerm: string, excludeIds: number[] = []) => {
  const excludeKey = [...excludeIds].sort((a, b) => a - b);
 
  return useQuery({
    queryKey: ["products", "search", searchTerm, excludeKey],
    queryFn: () =>
      productService.search(searchTerm, searchTerm.trim().length === 0 ? 10 : 30, excludeIds),
  });
};

export const useProductsByIdsQuery = (ids: number[]) =>
  useQuery({
    queryKey: ["products", "byIds", [...ids].sort()],
    queryFn: () => productService.getByIds(ids),
    enabled: ids.length > 0,
  });

export const useSwitchableUsersQuery = (enabled: boolean) =>
  useQuery({
    queryKey: ["auth", "switchableUsers"],
    queryFn: () => authService.getSwitchableUsers(),
    enabled,
  });
