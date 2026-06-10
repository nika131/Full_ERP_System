import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import toast from 'react-hot-toast';

interface ProtectedRouteProps {
    requiredPermission?: string;
}

export const ProtectedRoute = ({ requiredPermission }: ProtectedRouteProps) => {
    const { isAuthenticated, hasPermission } = useAuth();
    
    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    if (requiredPermission && !hasPermission(requiredPermission)) {
        toast.error(`Access Denied: Requires '${requiredPermission}'`);
        return <Navigate to="/profile" replace />;
    }

    return <Outlet />;
}