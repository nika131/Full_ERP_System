import apiClient from './apiClient';
import type { 
    LeaveRequestPayload, 
    LeaveReviewPayload, 
    LeaveResponse 
} from '../types/absence';

export const absenceService = {
    requestLeave: async (payload: LeaveRequestPayload): Promise<void> => {
        await apiClient.post('/absences/request', payload);
    },

    getMyHistory: async (signal?: AbortSignal): Promise<LeaveResponse[]> => {
        const response = await apiClient.get('/absences/my-history', { signal });
        return response.data;
    },

    getPendingRequests: async (signal?: AbortSignal): Promise<LeaveResponse[]> => {
        const response = await apiClient.get('/Absences/pending', { signal });
        return response.data;
    },

    reviewLeave: async (absenceId: number, payload: LeaveReviewPayload): Promise<void> => {
        await apiClient.put(`/absences/${absenceId}/review`, payload);
    }
};