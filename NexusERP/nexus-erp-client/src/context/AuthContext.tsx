import { createContext, useContext, useEffect, type ReactNode, useState } from 'react';
import { jwtDecode } from 'jwt-decode';
import { authService } from '../api/authService';

interface User {
    username: string;
    role: string;
    permissions: string[];
}

interface AuthContextType {
    user: User | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (token: string) => void;
    logout: () => void;
    hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<User | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    
    useEffect(() => {
        const token = localStorage.getItem('jwt_token');
        if (token) {
            decodeAndSetUser(token);
        } else {
            setIsLoading(false);
        }
    }, []);

    const decodeAndSetUser = (token: string) => {
        try {
            const decoded: any = jwtDecode(token);
            const role = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role;
            const username = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || decoded.unique_name;
            
            const rawPermissions = decoded["Permission"] || decoded.permissions || [];
            let permissions: string[] = [];

            if (Array.isArray(rawPermissions)) {
                permissions = rawPermissions;
            } else if (typeof rawPermissions === 'string') {
                try {
                    permissions = JSON.parse(rawPermissions);
                } catch {
                    permissions = [rawPermissions];
                }
            }

            setUser({ username, role, permissions });
        }
        catch (error) {
            console.error("Invalid token format");
            authService.logout();
        } finally {
            setIsLoading(false);
        }
    };

    const login = (token: string) => {
        localStorage.setItem('jwt_token', token);
        decodeAndSetUser(token);
    };

    const logout = () => {
        setUser(null);
        authService.logout();
    };

    const hasPermission = (requiredPermission: string): boolean => {
        return user?.permissions.includes(requiredPermission) || false;
    };

    return (
        <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout, hasPermission }}>
            {!isLoading ? children : <div className="h-screen w-screen flex items-center justify-center">Loading Application...</div>}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error("useAuth must be used within an AuthProvider");
    return context;
}