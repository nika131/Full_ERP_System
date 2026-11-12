import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Permissions } from '../constants/permissions';
import { useEffect, useState } from 'react';
import { Menu, Settings, Bug, Lightbulb, Boxes, UserRound, LayoutDashboard, Package, Truck, Tags, ShieldCheck, UsersRound, Store, CalendarClock, ScrollText, Link2, Check, MoreHorizontal, LogOut } from 'lucide-react';
import FeedbackDialog from '../components/FeedbackDialog';
import { useUrlString } from '../hooks/useUrlState';
import toast from 'react-hot-toast';

export default function MainLayout() {
  const { user, logout, companyId } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [isSidebarOpen, setIsSidebarOpen] = useState(() => window.innerWidth >= 768);
  const [feedbackParam, setFeedbackParam] = useUrlString('feedback');
  const feedbackKind = feedbackParam === 'bug' || feedbackParam === 'idea' ? feedbackParam : null;
  const setFeedbackKind = (value: 'bug' | 'idea' | null) => setFeedbackParam(value ?? '');
  const [copied, setCopied] = useState(false);

  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const handleLogout = async () => {
        try {
            await logout();
            navigate('/login');
        } catch {
            toast.error('Could not sign out. Please try again.');
        }
    };

  const handleSwitchCompany = () => {
    navigate('/companies');
  };

  const canAccess = (permission: string) => user?.permissions?.includes(permission);

  const isActive = (path: string) => location.pathname.startsWith(path);
  // Reset the copy confirmation when the address changes.
  const currentPage = location.pathname + location.search;
  useEffect(() => setCopied(false), [currentPage]);

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
            <Link to="/" onClick={() => setIsSidebarOpen(false)} aria-label="TenexERP home" className="flex h-16 shrink-0 items-center gap-3 border-b border-gray-800 bg-gray-950 px-4 transition hover:bg-gray-900">
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
            <header className="relative z-20 flex h-16 shrink-0 items-center justify-between border-b border-gray-200 bg-white px-3 shadow-sm sm:px-4 lg:px-6">
                
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
                <div className="hidden items-center gap-2 lg:flex lg:gap-3">
                    <button type="button" onClick={async () => {
                      try {
                        await navigator.clipboard.writeText(window.location.href);
                        setCopied(true);
                      } catch { setCopied(false); toast.error('Could not copy this page link.'); }
                    }} title={copied ? 'Page link copied' : 'Copy page link'} aria-label={copied ? 'Page link copied' : 'Copy page link'} className={`rounded-lg p-2 ${copied ? 'bg-emerald-50 text-emerald-700' : 'text-gray-500 hover:bg-emerald-50 hover:text-emerald-700'}`}>
                      {copied ? <Check size={19}/> : <Link2 size={19}/>}
                    </button>
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
                        className="inline-flex items-center gap-2 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-100"
                    >
                        Logout
                    </button>
                </div>

                {/* Mobile menu dropdown */}
                <div className="relative lg:hidden">
                    <button
                        type="button"
                        onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
                        aria-label="More options"
                        aria-expanded={isMobileMenuOpen}
                        className="rounded-lg p-2 text-gray-600 hover:bg-gray-100"
                    >
                        <MoreHorizontal size={24} />
                    </button>

                    {isMobileMenuOpen && (
                        <div className="absolute right-0 top-full z-30 mt-2 w-56 rounded-xl border border-gray-200 bg-white p-2 shadow-xl">
                            <div className="border-b border-gray-100 px-3 py-2">
                                <p className="text-sm font-semibold text-gray-900">
                                    {user?.username}
                                </p>
                                <p className="text-xs text-emerald-600">
                                    {user?.role}
                                </p>
                            </div>

                            <button
                                onClick={async () => {
                                    try {
                                        await navigator.clipboard.writeText(window.location.href);
                                        setCopied(true);
                                    } catch {
                                        toast.error('Could not copy link.');
                                    }
                                    setIsMobileMenuOpen(false);
                                }}
                                className="w-full rounded-lg px-3 py-2 text-left text-sm hover:bg-gray-100"
                            >
                                Copy page link
                            </button>

                            <button
                                onClick={() => {
                                    setFeedbackKind('bug');
                                    setIsMobileMenuOpen(false);
                                }}
                                className="w-full rounded-lg px-3 py-2 text-left text-sm hover:bg-gray-100"
                            >
                                Report a problem
                            </button>

                            <button
                                onClick={() => {
                                    setFeedbackKind('idea');
                                    setIsMobileMenuOpen(false);
                                }}
                                className="w-full rounded-lg px-3 py-2 text-left text-sm hover:bg-gray-100"
                            >
                                Suggest a feature
                            </button>

                            <Link
                                to="/settings"
                                onClick={() => setIsMobileMenuOpen(false)}
                                className="block w-full rounded-lg px-3 py-2 text-left text-sm hover:bg-gray-100"
                            >
                                Settings
                            </Link>

                            <button
                                onClick={() => {
                                    setIsMobileMenuOpen(false);
                                    handleSwitchCompany();
                                }}
                                className="w-full rounded-lg px-3 py-2 text-left text-sm hover:bg-gray-100"
                            >
                                Switch company
                            </button>

                            <button
                                onClick={() => {
                                    setIsMobileMenuOpen(false);
                                    void handleLogout();
                                }}
                                className="mt-1 flex w-full items-center gap-2 rounded-lg border-t border-gray-100 px-3 py-2 text-left text-sm font-semibold text-red-600 hover:bg-red-50"
                            >
                                <LogOut size={16} />
                                Log out
                            </button>
                        </div>
                    )}
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
