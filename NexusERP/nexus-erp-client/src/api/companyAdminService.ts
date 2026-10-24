import apiClient from './apiClient';
import type { JoinRequestSummary } from './authService';

export interface RoleOption {
    roleId: number;
    name: string;
    isSystemRole: boolean;
}

export const companyAdminService = {
    getPending: async () => {
        const { data } = await apiClient.get<{ message: string; requests: JoinRequestSummary[] }>('/company-join-requests/pending');
        return data.requests;
    },
    getRoles: async () => {
        const { data } = await apiClient.get<RoleOption[]>('/roles/lookup');
        return data;
    },
    approve: async (requestId: number, roleId: number) => {
        const { data } = await apiClient.post<{ message: string }>(`/company-join-requests/${requestId}/approve`, { roleId });
        return data;
    },
    reject: async (requestId: number) => {
        const { data } = await apiClient.post<{ message: string }>(`/company-join-requests/${requestId}/reject`);
        return data;
    },
    rotateJoinCode: async () => {
        const { data } = await apiClient.post<{ message: string; joinCode: string; expiresAt: string }>('/companies/join-code/rotate');
        return data;
    },
};
