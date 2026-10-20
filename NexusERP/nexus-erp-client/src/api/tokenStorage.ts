import { jwtDecode } from 'jwt-decode';

const KEYS = {
    account: 'tenex_account_access',
    refresh: 'tenex_account_refresh',
    company: 'tenex_company_access',
    companyId: 'tenex_company_id',
} as const;

export type SessionTokens = {
    accessToken: string;
    refreshToken: string;
};

export const tokenStorage = {
    getAccountAccess: () => sessionStorage.getItem(KEYS.account),
    getRefresh: () => sessionStorage.getItem(KEYS.refresh),
    getCompanyAccess: () => sessionStorage.getItem(KEYS.company),
    getCompanyId: () => {
        const value = sessionStorage.getItem(KEYS.companyId);
        const id = value ? Number(value) : NaN;
        return Number.isInteger(id) && id > 0 ? id : null;
    },
    setAccount: (tokens: SessionTokens) => {
        sessionStorage.setItem(KEYS.account, tokens.accessToken);
        sessionStorage.setItem(KEYS.refresh, tokens.refreshToken);
    },
    setCompany: (companyId: number, token: string) => {
        sessionStorage.setItem(KEYS.companyId, String(companyId));
        sessionStorage.setItem(KEYS.company, token);
    },
    clearCompany: () => {
        sessionStorage.removeItem(KEYS.company);
        sessionStorage.removeItem(KEYS.companyId);
    },
    clearAll: () => {
        Object.values(KEYS).forEach((key) => sessionStorage.removeItem(key));
        localStorage.removeItem('jwt_token'); // Remove the old website token.
    },
};

export const expiresSoon = (token: string, seconds = 45) => {
    try {
        const { exp } = jwtDecode<{ exp?: number }>(token);
        return !exp || exp * 1000 <= Date.now() + seconds * 1000;
    } catch {
        return true;
    }
};
