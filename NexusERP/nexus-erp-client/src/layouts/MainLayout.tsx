import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Permissions } from '../constants/permissions';
import { useState } from 'react';
import { Menu, Settings, Bug, Lightbulb, Boxes, UserRound, LayoutDashboard, Package, Truck, Tags, ShieldCheck, UsersRound, Store, CalendarClock, ScrollText } from 'lucide-react';
import FeedbackDialog from '../components/FeedbackDialog';

export default function MainLayout() {
  const { user, logout, switchCompany, companyId } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [isSidebarOpen, setIsSidebarOpen] = useState(true);
  const [feedbackKind, setFeedbackKind] = useState<'bug' | 'idea' | null>(null);

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
    
        {/* SIDEBAR: one consistent visual system for every navigation item. */}
        <aside className={`bg-gray-900 text-white flex flex-col transition-all duration-300 ease-in-out shrink-0 z-50 h-full fixed md:relative
            ${isSidebarOpen
                ? 'translate-x-0 w-64 opacity-100'
                : '-translate-x-full w-64 md:translate-x-0 md:w-0 overflow-hidden opacity-0'
        }`}>
            <Link to="/profile" onClick={() => setIsSidebarOpen(false)} aria-label="TenexERP workspace" className="flex h-16 shrink-0 items-center gap-3 border-b border-gray-800 bg-gray-950 px-4 transition hover:bg-gray-900">
                <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-emerald-500/15 text-emerald-400 ring-1 ring-emerald-500/20">
                    <Boxes size={22} strokeWidth={2.2} aria-hidden="true" />
                </span>
                <span className="min-w-0 text-[20px] font-extrabold tracking-[-.055em] leading-none text-white">Tenex<span className="text-emerald-400">ERP</span></span>
            </Link>

            <nav aria-label="Workspace navigation" className="min-h-0 flex-1 space-y-1.5 overflow-y-auto px-3 py-5">
                {[
                    { path: '/profile', label: 'My Profile', icon: UserRound, visible: true },
                    { path: '/dashboard', label: 'Dashboard', icon: LayoutDashboard, visible: !!canAccess(Permissions.ViewDashboard) },
                    { path: '/inventory', label: 'Inventory', icon: Package, visible: !!canAccess(Permissions.ViewProducts) },
                    { path: '/suppliers', label: 'Suppliers', icon: Truck, visible: !!canAccess(Permissions.ViewSuppliers) },
                    { path: '/categories', label: 'Categories', icon: Tags, visible: !!canAccess(Permissions.ViewCategories) },
                    { path: '/company-admin', label: 'Company access', icon: ShieldCheck, visible: !!(canAccess(Permissions.ManageJoinRequests) || canAccess(Permissions.ManageJoinCodes)) },
                    { path: '/employees', label: 'Employees', icon: UsersRound, visible: !!(canAccess(Permissions.ViewUsers) || canAccess(Permissions.ManageRoles)) },
                    { path: '/stores', label: 'Stores', icon: Store, visible: !!canAccess(Permissions.ViewStores) },
                    { path: '/shiftAuditList', label: 'Shifts', icon: CalendarClock, visible: !!canAccess(Permissions.ViewShifts) },
                    { path: '/logs', label: 'System Logs', icon: ScrollText, visible: !!canAccess(Permissions.ViewAuditLogs) },
                ].filter(item => item.visible).map(({ path, label, icon: Icon }) => (
                    <Link
                        key={path}
                        to={path}
                        onClick={() => setIsSidebarOpen(false)}
                        aria-current={isActive(path) ? 'page' : undefined}
                        className={`group flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold transition-colors focus-visible:outline-2 focus-visible:outline-offset-[-2px] focus-visible:outline-emerald-400 ${isActive(path) ? 'bg-emerald-600 text-white shadow-sm' : 'text-slate-300 hover:bg-gray-800 hover:text-white'}`}
                    >
                        <Icon size={18} className="shrink-0" aria-hidden="true" />
                        <span className="min-w-0 truncate">{label}</span>
                    </Link>
                ))}
            </nav>

            {/* Settings lives at the bottom, visually identical to page entries. */}
            <div className="shrink-0 border-t border-gray-800 px-3 py-3">
                <Link
                    to="/settings"
                    onClick={() => setIsSidebarOpen(false)}
                    aria-current={isActive('/settings') ? 'page' : undefined}
                    className={`flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold transition-colors focus-visible:outline-2 focus-visible:outline-offset-[-2px] focus-visible:outline-emerald-400 ${isActive('/settings') ? 'bg-emerald-600 text-white shadow-sm' : 'text-slate-300 hover:bg-gray-800 hover:text-white'}`}
                >
                    <Settings size={18} className="shrink-0" aria-hidden="true" />
                    <span className="truncate">Settings &amp; support</span>
                </Link>
            </div>
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

                {/* Right side: fast feedback and user menu */}
                <div className="flex items-center gap-2 sm:gap-4">
                    <button type="button" onClick={() => setFeedbackKind('bug')} title="Report a problem" aria-label="Report a problem" className="rounded-lg p-2 text-gray-500 hover:bg-rose-50 hover:text-rose-700"><Bug size={19}/></button>
                    <button type="button" onClick={() => setFeedbackKind('idea')} title="Suggest a feature" aria-label="Suggest a feature" className="rounded-lg p-2 text-gray-500 hover:bg-amber-50 hover:text-amber-700"><Lightbulb size={19}/></button>
                    <Link to="/settings" title="Settings" aria-label="Settings" className="rounded-lg p-2 text-gray-500 hover:bg-emerald-50 hover:text-emerald-700"><Settings size={19}/></Link>
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
    {feedbackKind && <FeedbackDialog kind={feedbackKind} onClose={() => setFeedbackKind(null)}/> }
    </div>
  );
}
