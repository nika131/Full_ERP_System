import apiClient from './apiClient';

export const authService = {
    login: async (credentials: { username: string; password: string}) => {
        const response = await apiClient.post('/auth/login', credentials);
        const token = response.data.token;

        localStorage.setItem('jwt_token', token);
        return response;
    },

    logout: () => {
        localStorage.removeItem('jwt_token');
        window.location.href = '/login';
    },

    isAuthenticated: () => {
        return !!localStorage.getItem('jwt_token');
    },

    register: async (data: { 
        fullName: string; 
        username: string; 
        password: string; 
        roleId: number 
    }) => {
        const response = await apiClient.post('/auth/register', data);
        return response.data;
    }
};