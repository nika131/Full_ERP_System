
import { jwtDecode } from 'jwt-decode';

const COMPANY_ID_KEY = 'tenex_company_id';

// Access tokens exist only in memory.
let accountAccess: string | null = null;
let companyAccess: string | null = null;

export type SessionTokens = {
    accessToken: string;
};

// Remove tokens stored by the previous authentication system.
const oldTokenKeys = [
    'tenex_account_access',
    'tenex_account_refresh',
    'tenex_company_access',
    'jwt_token'
];

oldTokenKeys.forEach(key => {
    sessionStorage.removeItem(key);
    localStorage.removeItem(key);
});

export const tokenStorage = {
    getAccountAccess: () => accountAccess,

    getCompanyAccess: () => companyAccess,

    getCompanyId: () => {
        const value =
            sessionStorage.getItem(COMPANY_ID_KEY) ??
            localStorage.getItem(COMPANY_ID_KEY);
        const id = value ? Number(value) : NaN;

        return Number.isInteger(id) && id > 0 ? id : null;
    },

    setAccount: (tokens: SessionTokens) => {
        accountAccess = tokens.accessToken;
    },

    setCompany: (companyId: number, token: string) => {
        sessionStorage.setItem(COMPANY_ID_KEY, String(companyId));
        localStorage.setItem(COMPANY_ID_KEY, String(companyId));
        companyAccess = token;
    },

    clearCompany: () => {
        companyAccess = null;
        sessionStorage.removeItem(COMPANY_ID_KEY);
        localStorage.removeItem(COMPANY_ID_KEY);
    },

    clearAll: () => {
        accountAccess = null;
        companyAccess = null;
        sessionStorage.removeItem(COMPANY_ID_KEY);
        localStorage.removeItem(COMPANY_ID_KEY);
        oldTokenKeys.forEach(key => {
            sessionStorage.removeItem(key);
            localStorage.removeItem(key);
        });
    }
};

export const expiresSoon = (token: string, seconds = 45) => {
    try {
        const { exp } = jwtDecode<{ exp?: number }>(token);
        return !exp || exp * 1000 <= Date.now() + seconds * 1000;
    } catch {
        return true;
    }
};
