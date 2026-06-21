import { useMutation, useQuery } from '@tanstack/react-query';
import { absenceService } from '../../api/absenceService';
import { employeeService } from '../../api/employeeService';

export const useMyLeaveHistoryQuery = () => {
    return useQuery({
        queryKey: ['profile', 'leaveHistory'],
        queryFn: () => absenceService.getMyHistory(),
        staleTime: 5 * 60 * 1000, 
    });
};

export const useMySalaryHistoryQuery = () => {
    return useQuery({
        queryKey: ['profile', 'salaryHistory'],
        queryFn: () => employeeService.getMySalaryHistory(),
        staleTime: 5 * 60 * 1000, 
    });
};

export const useRequestLeaveMutation = () => {
    return useMutation({
        mutationFn: (payload: any) => absenceService.requestLeave(payload),
    });
};