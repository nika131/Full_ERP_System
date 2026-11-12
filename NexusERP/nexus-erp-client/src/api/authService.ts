import axios from 'axios';
import { expiresSoon, tokenStorage, type SessionTokens } from './tokenStorage';

const accountApi = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    withCredentials: true,
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
    if (!refreshPromise) {
        const refresh = async () => {
            const { data } = await accountApi.post<SessionTokens>(
                '/account-auth/refresh',
                {}
            );

            tokenStorage.setAccount(data);
        };

        refreshPromise = (async () => {
            // Prevent concurrent refresh requests across browser tabs.
            if (navigator.locks) {
                await navigator.locks.request(
                    'tenexerp-auth-refresh',
                    { mode: 'exclusive' },
                    refresh
                );
            } else {
                await refresh();
            }
        })().finally(() => {
            refreshPromise = null;
        });
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

async function doRestoreSession(): Promise<boolean> {
    try {
        await rotateAccountTokens();
    } catch (error) {
        // Only an explicitly invalid session means logged out.
        if (
            axios.isAxiosError(error) &&
            error.response?.status === 401
        ) {
            tokenStorage.clearAll();
            return false;
        }

        // Network errors, rate limits and server failures
        // must not destroy a potentially valid session.
        throw error;
    }

    const selectedCompany = tokenStorage.getCompanyId();

    if (selectedCompany) {
        try {
            await authService.selectCompany(selectedCompany);
        } catch (error) {
            if (
                axios.isAxiosError(error) &&
                error.response &&
                [400, 401, 403, 404].includes(error.response.status)
            ) {
                tokenStorage.clearCompany();
            } else {
                throw error;
            }
        }
    }

    return true;
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
        const { data } = await accountApi.post<SessionTokens>('/account-auth/login', { email, password });
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
        const performLogout = async () => {
            await accountApi.post('/account-auth/logout', {});

            tokenStorage.clearAll();

            // Notify other tabs without sharing credentials.
            localStorage.setItem(
                'tenex_logout_event',
                `${Date.now()}-${crypto.randomUUID()}`
            );
        };

        if (navigator.locks) {
            await navigator.locks.request(
                'tenexerp-auth-refresh',
                performLogout
            );
        } else {
            await performLogout();
        }
    },
};

export function apiErrorMessage(error: unknown, fallback = 'Something went wrong. Please try again.') {
    if (axios.isAxiosError<{ message?: string; Message?: string }>(error))
        return error.response?.data?.message ?? error.response?.data?.Message ?? fallback;
    return error instanceof Error ? error.message : fallback;
}
