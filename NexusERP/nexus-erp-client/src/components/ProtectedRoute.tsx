import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
    requiredPermission?: string;
}

export const ProtectedRoute = ({ requiredPermission }: ProtectedRouteProps) => {
    const { isAuthenticated, isAccountAuthenticated, isLoading, hasPermission } = useAuth();

    if (isLoading) return null;
    if (!isAccountAuthenticated) return <Navigate to="/login" replace />;
    if (!isAuthenticated) return <Navigate to="/companies" replace />;
    if (requiredPermission && !hasPermission(requiredPermission)) return <Navigate to="/profile" replace />;
    return <Outlet />;
};
