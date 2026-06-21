import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { employeeService } from "../../api/employeeService";
import { roleService } from "../../api/roleService";
import { absenceService } from "../../api/absenceService";
import type { EmployeeProfileFormData } from "../../schemas/hrSchema";
import { authService } from "../../api/authService";

export const useEmployeesQuery = (page: number, limit: number, search: string, roleFilter: string) => {
    return useQuery({
        queryKey: ['employees', { page, limit, search, roleFilter }],
        queryFn: ({ signal }) => employeeService.getEmployees(page, limit, search, roleFilter, signal),
        staleTime: 60 * 1000,
    });
};

export const useRolesQuery = () => {
    return useQuery({
        queryKey: ['roles'],
        queryFn: () => roleService.getRoles(),
        staleTime: Infinity,
    });
};

export const usePendingLeavQuery = () => {
    return useQuery({
        queryKey: ['pendingLeaves'],
        queryFn: ({ signal }) => absenceService.getPendingRequests(signal),
        staleTime: 0,
    });
};

export const useDeleteEmployeeMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (userId: number) => employeeService.deleteEmployee(userId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['employees'] });
        }
    });
};

export const usePermissionsQuery = () => {
    return useQuery({
        queryKey: ['permissions'],
        queryFn: () => roleService.getAvailablePermissions(),
        staleTime: Infinity, 
    });
};

export const useRegisterEmployeeMutation = () => {
    return useMutation({
        mutationFn: (data: EmployeeProfileFormData) => authService.register(data),
    });
};

export const useUpdateEmployeeMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ userId, data }: { userId: number, data: any }) => 
            employeeService.updateEmployee(userId, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['employees'] });
        }
    });
};

export const useUpsertRoleMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (payload: any) => roleService.upsertRole(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['roles'] });
        }
    });
};

export const useSalaryHistoryQuery = (userId: number) => {
    return useQuery({
        queryKey: ['employees', userId, 'salaryHistory'],
        queryFn: () => employeeService.getSalaryHistory(userId),
        staleTime: 0, 
    });
};

export const useAddSalaryRecordMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ userId, payload }: { userId: number, payload: any }) => 
            employeeService.addSalaryRecord(userId, payload),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: ['employees', variables.userId, 'salaryHistory'] });
            queryClient.invalidateQueries({ queryKey: ['employees'] });
        }
    });
};