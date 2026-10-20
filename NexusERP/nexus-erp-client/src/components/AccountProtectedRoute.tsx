import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function AccountProtectedRoute() {
    const { isLoading, isAccountAuthenticated } = useAuth();
    if (isLoading) return null;
    return isAccountAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}
