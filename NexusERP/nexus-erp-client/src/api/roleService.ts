import apiClient from './apiClient';
import type { RoleResponse } from '../types/role';

export const roleService = {
    getRoles: async (signal?: AbortSignal): Promise<RoleResponse[]> => {
        const response = await apiClient.get('/roles', { signal });
        
        if (response.data && response.data.items) {
            return response.data.items;
        }
        return response.data;
    },

    getAvailablePermissions: async (signal?: AbortSignal): Promise<string[]> => {
        const response = await apiClient.get('/roles/permissions', { signal });
        
        if (response.data && response.data.items) {
            return response.data.items;
        }
        return Array.isArray(response.data) ? response.data : [];
    },

    upsertRole: async (payload: RoleResponse): Promise<void> => {
        await apiClient.post(`/Roles/upsert`, payload);
    },

    deleteRole: async (roleId: number): Promise<void> => {
        await apiClient.delete(`/roles/${roleId}`);
    }
};