import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function AccountProtectedRoute() {
    const { isLoading, isAccountAuthenticated } = useAuth();
    const location = useLocation();
    if (isLoading) return null;
    return isAccountAuthenticated ? <Outlet /> : <Navigate to={`/login?next=${encodeURIComponent(location.pathname + location.search + location.hash)}`} replace />;
}
