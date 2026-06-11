import apiClient from './apiClient';
import type { PagedResult } from '../types/pagination';
import type { 
    EmployeeResponse, 
    EmployeeUpdatePayload, 
    SalaryRecordResponse, 
    SalaryRecordCreatePayload 
} from '../types/employee';

export const employeeService = {
    getEmployees: async (
        page: number = 1, 
        pageSize: number = 10, 
        searchTerm?: string,
        roleFilter: string = "All",
        signal?: AbortSignal
    ): Promise<PagedResult<EmployeeResponse>> => {
        const params = new URLSearchParams({
            page: page.toString(),
            pageSize: pageSize.toString(),
            roleFilter
        });
        if (searchTerm) params.append('searchTerm', searchTerm);

        const response = await apiClient.get(`/employees?${params.toString()}`, { signal });
        return response.data;
    },

    updateEmployee: async (userId: number, payload: EmployeeUpdatePayload): Promise<void> => {
        await apiClient.put(`/employees/${userId}`, payload);
    },

    deleteEmployee: async (userId: number): Promise<void> => {
        await apiClient.delete(`/employees/${userId}`);
    },

    getSalaryHistory: async (userId: number, signal?: AbortSignal): Promise<SalaryRecordResponse[]> => {
        const response = await apiClient.get(`/employees/${userId}/salary`, { signal });
        return response.data;
    },

    addSalaryRecord: async (userId: number, payload: SalaryRecordCreatePayload): Promise<void> => {
        await apiClient.post(`/employees/${userId}/salary`, payload);
    }
};