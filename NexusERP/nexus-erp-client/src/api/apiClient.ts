import axios, { AxiosError, AxiosHeaders } from 'axios';
import toast from 'react-hot-toast';
import { authService } from './authService';
import { expiresSoon, tokenStorage } from './tokenStorage';

const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    headers: { 'Content-Type': 'application/json' },
});

apiClient.interceptors.request.use(async (config) => {
    let token = tokenStorage.getCompanyAccess();
    if (token && expiresSoon(token)) token = await authService.refreshCompanyAccess();
    if (token) {
        config.headers = AxiosHeaders.from(config.headers);
        config.headers.set('Authorization', `Bearer ${token}`);
    }
    return config;
});

apiClient.interceptors.response.use(
    (response) => response,
    async (error: AxiosError<{ message?: string; Message?: string }>) => {
        const original = error.config as (typeof error.config & { _retried?: boolean }) | undefined;
        if (error.response?.status === 401 && original && !original._retried && tokenStorage.getCompanyId()) {
            original._retried = true;
            try {
                const fresh = await authService.refreshCompanyAccess();
                original.headers = AxiosHeaders.from(original.headers);
                original.headers.set('Authorization', `Bearer ${fresh}`);
                return await apiClient(original);
            } catch {
                tokenStorage.clearCompany();
            }
        }

        // React Query cancels unused requests during navigation/refetch. This is not a network outage.
        if (axios.isCancel(error) || error.code === 'ERR_CANCELED' || error.config?.signal?.aborted)
            return Promise.reject(error);

        const status = error.response?.status;
        const message = error.response?.data?.message ?? error.response?.data?.Message ?? 'An unexpected error occurred.';
        if (status === 401) {
            tokenStorage.clearCompany();
            toast.error('ERP access expired. Please select your company again.');
            window.location.assign('/companies');
        } else if (status === 403) {
            if (error.config?.method?.toLowerCase() !== 'get') toast.error(message);
        }
        else if (status === 429) toast.error('Rate limit exceeded. Please try again later.');
        else if (status && error.config?.method?.toLowerCase() !== 'get') toast.error(message);
        else if (!status && error.code === 'ERR_NETWORK' && error.config?.method?.toLowerCase() !== 'get')
            toast.error('Cannot connect to the server. Check your connection.');

        return Promise.reject(error);
    }
);

export default apiClient;
