import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import toast from 'react-hot-toast';
import { useEffect } from 'react';

interface ProtectedRouteProps {
    requiredPermission?: string;
}

export const ProtectedRoute = ({ requiredPermission }: ProtectedRouteProps) => {
    const { isAuthenticated, isLoading, hasPermission } = useAuth();
    
    useEffect(() => {
        if (!isLoading && isAuthenticated && requiredPermission && !hasPermission(requiredPermission)){
            toast.error(`Access Denied: Requires '${requiredPermission}'`);
        }
    }, [isLoading, isAuthenticated, requiredPermission, hasPermission]);

    if (isLoading) {
        return null;
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    if (requiredPermission && !hasPermission(requiredPermission)) {
        return <Navigate to="/dashboared" replace />;
    }

    return <Outlet />;
}