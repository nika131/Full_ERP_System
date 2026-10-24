import axios from 'axios';
import { expiresSoon, tokenStorage, type SessionTokens } from './tokenStorage';

const accountApi = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    headers: { 'Content-Type': 'application/json' },
});

export interface CompanySummary {
    companyId: number;
    companyName: string;
    userId: number;
    fullName: string;
    username: string;
    roleId: number;
    roleName: string;
}

export interface JoinRequestSummary {
    joinRequestId: number;
    companyId: number;
    companyName: string;
    accountEmail: string;
    fullName: string;
    username: string;
    status: 'Pending' | 'Approved' | 'Rejected' | 0 | 1 | 2;
    createdAt: string;
    reviewedAt: string | null;
    approvedRoleId: number | null;
}

export interface CreatedCompany {
    message: string;
    companyId: number;
    userId: number;
    roleId: number;
    joinCode: string;
}

let refreshPromise: Promise<void> | null = null;
let restorePromise: Promise<boolean> | null = null;

async function rotateAccountTokens() {
    const refreshToken = tokenStorage.getRefresh();
    if (!refreshToken) throw new Error('Your account session has ended. Please sign in.');

    if (!refreshPromise) {
        refreshPromise = accountApi.post<SessionTokens>('/account-auth/refresh', { refreshToken })
            .then(({ data }) => tokenStorage.setAccount(data))
            .catch((error: unknown) => {
                tokenStorage.clearAll();
                throw error;
            })
            .finally(() => { refreshPromise = null; });
    }

    await refreshPromise;
}

async function accountAccessToken() {
    let token = tokenStorage.getAccountAccess();
    if (!token || expiresSoon(token)) {
        await rotateAccountTokens();
        token = tokenStorage.getAccountAccess();
    }
    if (!token) throw new Error('Please sign in again.');
    return token;
}

async function authorizationHeader() {
    return { Authorization: `Bearer ${await accountAccessToken()}` };
}

async function doRestoreSession() {
    if (!tokenStorage.getRefresh()) {
        tokenStorage.clearAll();
        return false;
    }

    try {
        await rotateAccountTokens();
        const selectedCompany = tokenStorage.getCompanyId();
        if (selectedCompany) {
            try {
                await authService.selectCompany(selectedCompany);
            } catch {
                tokenStorage.clearCompany();
            }
        }
        return true;
    } catch {
        tokenStorage.clearAll();
        return false;
    }
}

export const authService = {
    registerAccount: async (email: string, password: string) => {
        const { data } = await accountApi.post<{ message: string }>('/account-auth/register', { email, password });
        return data;
    },

    verifyEmail: async (email: string, code: string) => {
        const { data } = await accountApi.post<{ message: string }>('/account-auth/verify-email', { email, code });
        return data;
    },

    resendCode: async (email: string) => {
        const { data } = await accountApi.post<{ message: string }>('/account-auth/resend-code', { email });
        return data;
    },

    loginAccount: async (email: string, password: string) => {
        const { data } = await accountApi.post<SessionTokens & { message: string }>('/account-auth/login', { email, password });
        tokenStorage.clearAll();
        tokenStorage.setAccount(data);
        return data;
    },

    restoreSession: () => {
        if (!restorePromise)
            restorePromise = doRestoreSession().finally(() => { restorePromise = null; });
        return restorePromise;
    },

    getMyCompanies: async () => {
        const { data } = await accountApi.get<{ message: string; companies: CompanySummary[] }>('/companies/mine', {
            headers: await authorizationHeader(),
        });
        return data;
    },

    createCompany: async (request: { companyName: string; fullName: string; username: string; posPin?: string }) => {
        const { data } = await accountApi.post<CreatedCompany>('/companies', request, {
            headers: await authorizationHeader(),
        });
        return data;
    },

    selectCompany: async (companyId: number) => {
        const { data } = await accountApi.post<{ message: string; accessToken: string; companyId: number; companyName: string; userId: number; roleName: string }>(
            `/companies/${companyId}/select`, {}, { headers: await authorizationHeader() });
        tokenStorage.setCompany(companyId, data.accessToken);
        return data;
    },

    refreshCompanyAccess: async () => {
        const companyId = tokenStorage.getCompanyId();
        if (!companyId) throw new Error('Select a company to continue.');
        const result = await authService.selectCompany(companyId);
        return result.accessToken;
    },

    submitJoinRequest: async (request: { joinCode: string; fullName: string; username: string }) => {
        const { data } = await accountApi.post<{ message: string; joinRequestId: number }>('/companies/join', request, {
            headers: await authorizationHeader(),
        });
        return data;
    },

    getMyJoinRequests: async () => {
        const { data } = await accountApi.get<{ message: string; requests: JoinRequestSummary[] }>('/companies/join-requests/mine', {
            headers: await authorizationHeader(),
        });
        return data;
    },

    logoutAccount: async () => {
        try {
            const token = await accountAccessToken();
            await accountApi.post('/account-auth/logout', {}, { headers: { Authorization: `Bearer ${token}` } });
        } finally {
            tokenStorage.clearAll();
        }
    },

    // Compatibility: existing employee-management form still uses this endpoint.
    register: async (data: { fullName: string; username: string; password: string; roleId: number; posPin?: string }) => {
        let erpToken = tokenStorage.getCompanyAccess();
        if (erpToken && expiresSoon(erpToken)) erpToken = await authService.refreshCompanyAccess();
        if (!erpToken) throw new Error('Select a company first.');
        const response = await accountApi.post('/auth/register', data, {
            headers: { Authorization: `Bearer ${erpToken}` },
        });
        return response.data;
    },
};

export function apiErrorMessage(error: unknown, fallback = 'Something went wrong. Please try again.') {
    if (axios.isAxiosError<{ message?: string; Message?: string }>(error))
        return error.response?.data?.message ?? error.response?.data?.Message ?? fallback;
    return error instanceof Error ? error.message : fallback;
}
