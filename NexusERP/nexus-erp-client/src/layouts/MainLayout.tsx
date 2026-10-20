import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Permissions } from '../constants/permissions';
import { useState } from 'react';
import { Menu } from 'lucide-react';

export default function MainLayout() {
  const { user, logout, switchCompany, companyId } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [isSidebarOpen, setIsSidebarOpen] = useState(true);

  const handleLogout = async () => {
    try {
      await logout();
    } catch {
      // The Account context clears local tokens even if remote revocation fails.
    }
    navigate('/login');
  };

  const handleSwitchCompany = () => {
    switchCompany();
    navigate('/companies');
  };

  const canAccess = (permission: string) => user?.permissions?.includes(permission);

  const isActive = (path: string) => location.pathname.startsWith(path);

  return (
    <div className="flex h-screen bg-gray-50 relative overflow-hidden">

        {/* MOBILE OVERLAY BACKDROP */}
        {isSidebarOpen && (
            <div
                className='fixed inset-0 bg-gray-900/50 z-40 md:hidden'
                onClick={() => setIsSidebarOpen(false)}
            ></div>
        )}
    
        {/* SIDEBAR */}
        <aside className={`bg-gray-900 text-white flex flex-col transition-all duration-300 ease-in-out shrink-0 z-50 h-full fixed md:relative
            ${isSidebarOpen 
                ? 'translate-x-0 w-64 opacity-100' 
                : '-translate-x-full w-64 md:translate-x-0 md:w-0 overflow-hidden opacity-0'
        }`}>

            <div className="p-4 bg-gray-950 border-b border-gray-800 h-16 flex items-center shrink-0">
                <h2 className="text-xl font-bold tracking-wider text-emerald-400">NEXUS ERP</h2>
            </div>
            
            <nav className="flex-1 p-4 space-y-2 overflow-y-auto whitespace-nowrap">
                <Link to="/profile" 
                    onClick={() => setIsSidebarOpen(false)}
                    className={`block px-4 py-2 rounded transition-colors ${isActive('/profile') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`} >
                    My Profile
                </Link>

                {canAccess(Permissions.ViewDashboard) && (
                    <Link to="/dashboard" 
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/dashboard') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}>
                        Dashboard
                    </Link>
                )}

                {/* Conditional Navigation Links */}
                {canAccess(Permissions.ViewProducts) && (
                    <Link to="/inventory" 
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/inventory') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}
                    >    
                        Inventory
                    </Link>
                )}

                {canAccess(Permissions.ViewSuppliers) && (
                    <Link to="/suppliers" 
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/suppliers') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}>
                        Suppliers
                    </Link>
                )}

                {canAccess(Permissions.ViewCategories) && (
                    <Link to="/categories" 
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/categories') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}>
                        Categories
                    </Link>
                )}
                
                {(canAccess(Permissions.ManageJoinRequests) || canAccess(Permissions.ManageJoinCodes)) && (
                    <Link to="/company-admin"
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/company-admin') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}>
                        Company access
                    </Link>
                )}

                {(canAccess(Permissions.ViewUsers) || canAccess(Permissions.ManageRoles)) && (
                    <Link to="/employees" 
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/employees') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}>
                        Employees
                    </Link>
                )}

                {canAccess(Permissions.ViewStores) && (
                    <Link to="/stores" 
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/stores') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}>
                        Stores
                    </Link>
                )}

                {canAccess(Permissions.ViewShifts) && (
                    <Link to="/shiftAuditList" 
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/shiftAuditList') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}>
                        shifts
                    </Link>
                )}
            
                {canAccess(Permissions.ManageAbsences) && (
                    <Link to="/pending-leaves" onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors ${isActive('/pending-leaves') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}>
                        Pending Leave Requests
                    </Link>
                )}

                {canAccess(Permissions.ViewAuditLogs) && (
                    <Link to="/logs" 
                        onClick={() => setIsSidebarOpen(false)}
                        className={`block px-4 py-2 rounded transition-colors mt-8 ${isActive('/logs') ? 'bg-emerald-600' : 'hover:bg-gray-800 text-gray-400'}`}>
                        System Logs
                    </Link>
                )}

            </nav>
        </aside>

        <div className="flex-1 flex flex-col overflow-hidden">
            {/* HEADER */}
            <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 shadow-sm z-10 shrink-0">
                
                {/* Left side: Toggle Button */}
                <div className="flex items-center space-x-4">
                    <button 
                        onClick={() => setIsSidebarOpen(!isSidebarOpen)}
                        className="p-1 rounded-md text-gray-500 hover:text-gray-700 hover:bg-gray-100 transition-colors focus:outline-none"
                    >
                        <Menu size={24} />
                    </button>
                </div>

                {/* Right side: User & Logout */}
                <div className="flex items-center space-x-4">
                    <div className="text-right">
                        <div className="text-sm font-medium text-gray-900">{user?.username}</div>
                        <div className="text-xs font-semibold text-emerald-600 uppercase tracking-wide">{user?.role}</div>
                    </div>
                    <button
                        onClick={handleSwitchCompany}
                        className="px-3 py-1.5 text-sm bg-emerald-50 text-emerald-800 rounded hover:bg-emerald-100 transition-colors border border-emerald-200"
                        title={companyId ? `Current company: ${companyId}` : 'Switch company'}
                    >
                        Switch company
                    </button>
                    <button
                        onClick={() => { void handleLogout(); }}
                        className="px-3 py-1.5 text-sm bg-gray-100 text-gray-700 rounded hover:bg-gray-200 transition-colors border border-gray-300"
                    >
                        Logout
                    </button>
                </div>
            </header>

            <main className="flex-1 overflow-x-hidden overflow-y-auto bg-gray-50 p-6">
                <Outlet />

                <footer className="mt-8 border-t border-gray-200 pt-5 pb-2 text-sm text-gray-500">
                    <div className="flex flex-wrap items-center gap-x-5 gap-y-2">
                        <span>
                            © {new Date().getFullYear()} TenexERP
                        </span>

                        <Link to="/privacy-policy" className="hover:text-emerald-600">
                            Privacy Policy
                        </Link>

                        <Link to="/terms-of-service" className="hover:text-emerald-600">
                            Terms of Service
                        </Link>

                        <a href="mailto:support@tenexerp.com" className="hover:text-emerald-600">
                            Support
                        </a>
                    </div>
                </footer>
            </main>
      </div>
    </div>
  );
}
