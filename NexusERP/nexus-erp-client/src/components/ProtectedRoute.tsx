import { useEffect } from 'react';
import { Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
    requiredPermission?: string;
    requiredAnyPermissions?: string[];
}

/** All URL-based navigation still goes through account, company and action permissions. */
export const ProtectedRoute = ({ requiredPermission, requiredAnyPermissions }: ProtectedRouteProps) => {
    const { isAuthenticated, isAccountAuthenticated, isLoading, hasPermission, companyId } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const intended = location.pathname + location.search + location.hash;
    const requestedCompany = new URLSearchParams(location.search).get('company');

    useEffect(() => {
        if (isLoading || !isAccountAuthenticated || !isAuthenticated || !companyId || requestedCompany) return;
        const query = new URLSearchParams(location.search);
        query.set('company', String(companyId));
        navigate(`${location.pathname}?${query.toString()}${location.hash}`, { replace: true, preventScrollReset: true });
    }, [isLoading, isAccountAuthenticated, isAuthenticated, companyId, requestedCompany, location.pathname, location.search, location.hash, navigate]);

    if (isLoading) return null;
    if (!isAccountAuthenticated) return <Navigate to={`/login?next=${encodeURIComponent(intended)}`} replace />;
    if (!isAuthenticated || (requestedCompany && String(companyId) !== requestedCompany))
        return <Navigate to={`/companies?next=${encodeURIComponent(intended)}`} replace />;
    if (requiredAnyPermissions?.length && !requiredAnyPermissions.some(hasPermission)) return <Navigate to="/profile" replace />;
    if (requiredPermission && !hasPermission(requiredPermission)) return <Navigate to="/profile" replace />;
    return <Outlet />;
};
