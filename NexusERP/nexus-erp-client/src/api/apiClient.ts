import axios from 'axios';
import toast from 'react-hot-toast';

const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
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
            const status = error.response.status;
            const message = error.response.data?.message || 'An unexpected error occurred.';

            switch (status) {
                case 400:
                    toast.error(message);
                    break;
                case 401:
                    toast.error('Session expired. Please log in again');
                    localStorage.removeItem('jwt_token');
                    window.location.href = '/login';
                    break;
                case 403:
                    toast.error('Access Denied: You do not have permission for this action.');
                    break;
                case 409:
                    toast.error(`Data Conflict: ${message}`);
                    break;
                case 500:
                    toast.error('A critical server error occurred.')
                    break;
                default:
                    toast.error(message);
            }
        } else if (error.request) {
            toast.error('Cannot connect to the server. Check your connection.');
        }

        return Promise.reject(error);
    }
);

export default apiClient;