import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { absenceService } from '../../api/absenceService';
import toast from 'react-hot-toast';

export const usePendingLeavesQuery = () => {
    return useQuery({
        queryKey: ['pendingLeaves'],
        queryFn: ({ signal }) => absenceService.getPendingRequests(signal),
        staleTime: 0, 
    });
};

export const useReviewLeaveMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, status }: { id: number, status: 'Approved' | 'Rejected' }) => 
            absenceService.reviewLeave(id, { status, reviewerComments: "" }),
        onSuccess: (_, variables) => {
            toast.success(`Request ${variables.status} successfully.`);
            queryClient.invalidateQueries({ queryKey: ['pendingLeaves'] });
            queryClient.invalidateQueries({ queryKey: ['profile', 'leaveHistory'] });
        },
        onError: () => {
            toast.error("Action failed.");
        }
    });
};