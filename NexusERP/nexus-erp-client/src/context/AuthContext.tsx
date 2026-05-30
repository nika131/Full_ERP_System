import { createContext, useContext, useEffect, type ReactNode, useState } from 'react';
import { jwtDecode } from 'jwt-decode';
import { authService } from '../api/authService';

interface User {
    username: string;
    role: string;
}

interface AuthContextType {
    user: User | null;
    isAuthenticated: boolean;
    login: (token: string) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<User | null>(null);
    
    useEffect(() => {
        const token = localStorage.getItem('jwt_token');
        if (token) {
            decodeAndSetUser(token);
        }
    }, []);

    const decodeAndSetUser = (token: string) => {
        try {
            const decoded: any = jwtDecode(token);
            const role = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role;
            const username = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || decoded.unique_name;
            
            setUser({ username, role });
        }
        catch (error) {
            console.error("Invalid token format");
            authService.logout();
        }
    };

    const login = (token: string) => {
        decodeAndSetUser(token);
    };

    const logout = () => {
        setUser(null);
        authService.logout();
    };

    return (
        <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error("useAuth must be used within an AuthProvider");
    return context;
}