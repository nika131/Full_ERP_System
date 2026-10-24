import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';
import { authService } from '../api/authService';
import { tokenStorage } from '../api/tokenStorage';

interface User {
    username: string;
    role: string;
    permissions: string[];
    companyId: number;
}

interface AuthContextType {
    user: User | null;
    isAuthenticated: boolean;
    isAccountAuthenticated: boolean;
    isLoading: boolean;
    companyId: number | null;
    loginAccount: (email: string, password: string) => Promise<void>;
    selectCompany: (companyId: number) => Promise<void>;
    switchCompany: () => void;
    logout: () => Promise<void>;
    hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

function decodeCompanyUser(token: string): User | null {
    try {
        const payload = jwtDecode<Record<string, unknown>>(token);
        if (payload.token_type !== 'company') return null;

        const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? payload.role;
        const username = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? payload.unique_name;
        const raw = payload.Permission ?? [];
        const permissions = Array.isArray(raw) ? raw.map(String) : typeof raw === 'string' ? [raw] : [];
        const companyId = Number(payload.CompanyId);

        if (!Number.isInteger(companyId) || !username) return null;
        return { username: String(username), role: String(role ?? ''), permissions, companyId };
    } catch {
        return null;
    }
}

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<User | null>(null);
    const [isAccountAuthenticated, setAccountAuthenticated] = useState(false);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        let active = true;
        authService.restoreSession().then((restored) => {
            if (!active) return;
            setAccountAuthenticated(restored);
            setUser(restored && tokenStorage.getCompanyAccess() ? decodeCompanyUser(tokenStorage.getCompanyAccess()!) : null);
        }).finally(() => {
            if (active) setIsLoading(false);
        });
        return () => { active = false; };
    }, []);

    const loginAccount = async (email: string, password: string) => {
        await authService.loginAccount(email, password);
        setAccountAuthenticated(true);
        setUser(null);
    };

    const selectCompany = async (id: number) => {
        const result = await authService.selectCompany(id);
        const decoded = decodeCompanyUser(result.accessToken);
        if (!decoded) throw new Error('Invalid company token returned by server.');
        setUser(decoded);
    };

    const switchCompany = () => {
        tokenStorage.clearCompany();
        setUser(null);
    };

    const logout = async () => {
        try {
            await authService.logoutAccount();
        } finally {
            tokenStorage.clearAll();
            setAccountAuthenticated(false);
            setUser(null);
        }
    };

    const hasPermission = (permission: string) => user?.permissions.includes(permission) ?? false;

    return (
        <AuthContext.Provider value={{
            user,
            isAuthenticated: !!user,
            isAccountAuthenticated,
            isLoading,
            companyId: user?.companyId ?? tokenStorage.getCompanyId(),
            loginAccount,
            selectCompany,
            switchCompany,
            logout,
            hasPermission,
        }}>
            {isLoading ? (
                <div className="min-h-screen flex items-center justify-center text-gray-600">Loading TenexERP...</div>
            ) : children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error('useAuth must be used within an AuthProvider');
    return context;
};
