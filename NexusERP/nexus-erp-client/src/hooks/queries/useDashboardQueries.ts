import { useQuery } from '@tanstack/react-query';
import { dashboaredService } from '../../api/dashboardService';

export const useDashboardStatsQuery = () => {
    return useQuery({
        queryKey: ['dashboard', 'stats'],
        queryFn: () => dashboaredService.getStatistics(),
        staleTime: 0, 
    });
};

export const useChartDataQuery = () => {
    return useQuery({
        queryKey: ['dashboard', 'chartData'],
        queryFn: () => dashboaredService.getChartData(),
        staleTime: 60 * 1000, 
    });
};

export const useTopProductsQuery = () => {
    return useQuery({
        queryKey: ['dashboard', 'topProducts'],
        queryFn: () => dashboaredService.getTopProducts(),
        staleTime: 60 * 1000, 
    });
};

export const useTransactionsQuery = (
    limit: number, 
    createdAt: string | null, 
    transactionId: number | null, 
    numericSearchId: number | null
) => {
    return useQuery({
        queryKey: ['transactions', { limit, createdAt, transactionId, numericSearchId }],
        queryFn: ({ signal }) => dashboaredService.getTransactions(
            limit, 
            createdAt, 
            transactionId, 
            null, 
            null, 
            numericSearchId, 
            "All", 
            signal
        ),
        staleTime: 0, 
    });
};