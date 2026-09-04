import { useQuery } from '@tanstack/react-query';
import { dashboaredService } from '../../api/dashboardService';
import type { DashboardFilters } from '../../pages/Dashboard';

export const useDashboardStatsQuery = (filters: DashboardFilters) => {
    return useQuery({
        queryKey: ['dashboard', 'stats', filters],
        queryFn: () => dashboaredService.getStatistics(filters),
        staleTime: 0, 
    });
};

export const useChartDataQuery = (filters: DashboardFilters) => {
    return useQuery({
        queryKey: ['dashboard', 'chartData', filters],
        queryFn: () => dashboaredService.getChartData(filters),
        staleTime: 60 * 1000, 
    });
};

export const useTopProductsQuery = (filters: DashboardFilters) => {
    return useQuery({
        queryKey: ['dashboard', 'topProducts', filters],
        queryFn: () => dashboaredService.getTopProducts(filters),
        staleTime: 60 * 1000, 
    });
};

export const useTransactionsQuery = (
    limit: number, 
    createdAt: string | null, 
    transactionId: number | null, 
    searchTerm: string | null,
    filters: DashboardFilters,
) => {
    return useQuery({
        queryKey: ['transactions', { limit, createdAt, transactionId, searchTerm, filters }],
        queryFn: ({ signal }) => dashboaredService.getTransactions(
            limit, 
            createdAt, 
            transactionId, 
            searchTerm, 
            filters,
            signal
        ),
        staleTime: 0, 
    });
};