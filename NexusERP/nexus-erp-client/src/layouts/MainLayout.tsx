import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function MainLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const isActive = (path: string) => location.pathname.startsWith(path);

  return (
    <div className="flex h-screen bg-gray-50">
    
        <aside className="w-64 bg-gray-900 text-white flex flex-col">
            <div className="p-4 bg-gray-950 border-b border-gray-800">
            <h2 className="text-xxl font-bold tracking-wider text-emerald-400! hover:text-emerald-600!">NEXUS ERP</h2>
            </div>
            
            <nav className="flex-1 p-4 space-y-2">
                <Link 
                    to="/dashboard" 
                    className={`block px-4 py-2 rounded transition-colors ${isActive('/dashboard') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}
                >
                    Dashboard
                </Link>
                
                <Link 
                    to="/profile" 
                    className={`block px-4 py-2 rounded transition-colors ${isActive('/profile') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}
                >
                    My Profile
                </Link>

                <Link 
                    to="/inventory" 
                    className={`block px-4 py-2 rounded transition-colors ${isActive('/inventory') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}
                >
                    Inventory
                </Link>
                <Link 
                    to="/suppliers" 
                    className={`block px-4 py-2 rounded transition-colors ${isActive('/suppliers') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}
                >
                    Suppliers
                </Link>
                <Link 
                    to="/categories" 
                    className={`block px-4 py-2 rounded transition-colors ${isActive('/categories') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}
                >
                    Categories
                </Link>
                
                <Link 
                    to="/employees" 
                    className={`block px-4 py-2 rounded transition-colors ${isActive('/employees') ? 'bg-emerald-600' : 'hover:bg-gray-800'}`}
                >
                    Employees
                </Link>
            
                <Link 
                to="/logs" 
                className={`block px-4 py-2 rounded transition-colors mt-8 ${isActive('/logs') ? 'bg-emerald-600' : 'hover:bg-gray-800 text-gray-400'}`}
                >
                System Logs
                </Link>
            </nav>
        </aside>

        <div className="flex-1 flex flex-col overflow-hidden">
            {/* Header */}
            <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 shadow-sm z-10">
                <div className="text-lg font-semibold text-gray-700">
                </div>
                <div className="flex items-center space-x-4">
                    <div className="text-right">
                        <div className="text-sm font-medium text-gray-900">{user?.username}</div>
                        <div className="text-xs font-semibold text-emerald-600 uppercase tracking-wide">{user?.role}</div>
                    </div>
                    <button 
                    onClick={handleLogout}
                    className="px-3 py-1.5 text-sm bg-gray-100 text-gray-700 rounded hover:bg-gray-200 transition-colors border border-gray-300"
                    >
                    Logout
                    </button>
                </div>
            </header>

            <main className="flex-1 overflow-x-hidden overflow-y-auto bg-gray-50 p-6">
                <Outlet />
            </main>
      </div>
    </div>
  );
}