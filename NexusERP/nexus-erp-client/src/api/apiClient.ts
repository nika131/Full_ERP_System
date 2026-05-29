import axios from 'axios';

const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('jwt_token');
        if(token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

apiClient.interceptors.response.use(
    (response) => {
        return response;
    },
    (error) => {
        if (error.response ) {
            if (error.response.status === 401) {
                localStorage.removeItem('jwt_token');
                window.location.href = '/login';
            }

            if (error.response.status === 403) {
                console.error('Security Violation: Access denied');
                window.location.href = '/dashboard';
            }
        }
        return Promise.reject(error);
    }
);

export default apiClient;