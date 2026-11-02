import { useState, type FormEvent } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { ArrowRight, Boxes, Eye, EyeOff } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { apiErrorMessage } from '../api/authService';

const inputClass = 'auth-input mt-1.5 block w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10';

export default function Login() {
    const navigate = useNavigate();
    const location = useLocation();
    const { loginAccount, isAccountAuthenticated } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [error, setError] = useState('');
    const [busy, setBusy] = useState(false);

    const intended = new URLSearchParams(location.search).get('next');
    const destination = intended?.startsWith('/') && !intended.startsWith('//') && !intended.includes('\\')
        ? `/companies?next=${encodeURIComponent(intended)}` : '/companies';

    if (isAccountAuthenticated) return <Navigate to={destination} replace />;

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setError('');
        setBusy(true);
        try {
            await loginAccount(email.trim(), password);
            navigate(destination, { replace: true });
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to sign in.'));
        } finally {
            setBusy(false);
        }
    };

    const statusMessage = (location.state as { message?: string } | null)?.message;

    return (
        <main className="auth-page flex min-h-dvh justify-center bg-[#f7faf7] px-4 py-8 text-left sm:px-6 sm:py-12">
            <div className="my-auto w-full max-w-[460px]">
                <div className="mb-8 text-center">
                    <Link to="/" aria-label="TenexERP home" className="inline-flex items-center gap-2.5 text-[#163f35] transition hover:opacity-80">
                        <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#163f35] text-white"><Boxes size={21} aria-hidden="true" /></span>
                        <span className="text-2xl font-extrabold tracking-tight">Tenex<span className="text-emerald-600">ERP</span></span>
                    </Link>
                    <h1 className="mt-7 mb-0 text-center text-[1.8rem] leading-[1.15] font-extrabold tracking-tight text-slate-900 sm:text-[2rem]">
                        Welcome back
                    </h1>
                    <p className="mt-2 text-sm leading-6 text-slate-500">Sign in to your TenexERP workspace.</p>
                </div>

                <div className="rounded-[22px] border border-slate-200/90 bg-white p-6 shadow-[0_18px_55px_-25px_rgba(18,63,51,0.25)] sm:p-8">
                    {statusMessage && <p role="status" className="mb-5 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">{statusMessage}</p>}
                    {error && <p role="alert" className="mb-5 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>}
                    <form onSubmit={submit} className="space-y-5">
                        <div>
                            <label htmlFor="login-email" className="block text-sm font-semibold text-slate-700">Email address</label>
                            <input id="login-email" autoComplete="email" type="email" required value={email} onChange={e => setEmail(e.target.value)} placeholder="you@company.com" className={inputClass} />
                        </div>
                        <div>
                            <label htmlFor="login-password" className="block text-sm font-semibold text-slate-700">Password</label>
                            <div className="relative">
                                <input id="login-password" autoComplete="current-password" type={showPassword ? 'text' : 'password'} required value={password} onChange={e => setPassword(e.target.value)} className={`${inputClass} pr-12`} />
                                <button type="button" onClick={() => setShowPassword(v => !v)} aria-label={showPassword ? 'Hide password' : 'Show password'} aria-pressed={showPassword} className="absolute inset-y-0 right-1 mt-1.5 flex w-11 items-center justify-center rounded-lg text-slate-500 transition hover:text-emerald-700 focus-visible:outline-2 focus-visible:outline-offset-[-3px] focus-visible:outline-emerald-600">
                                    {showPassword ? <EyeOff size={19} aria-hidden="true" /> : <Eye size={19} aria-hidden="true" />}
                                </button>
                            </div>
                        </div>
                        <button type="submit" disabled={busy} className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#163f35] px-4 py-3.5 text-sm font-bold text-white transition hover:bg-[#215b4a] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 disabled:cursor-wait disabled:opacity-60">
                            {busy ? 'Signing in...' : 'Sign in'} {!busy && <ArrowRight size={17} aria-hidden="true" />}
                        </button>
                    </form>
                    <p className="mt-6 text-center text-sm text-slate-500">New to TenexERP? <Link to="/register" className="font-semibold text-emerald-700 hover:underline">Create an account</Link></p>
                </div>
                <p className="mt-6 text-center text-xs text-slate-500"><Link to="/privacy-policy" className="hover:underline">Privacy policy</Link> <span className="mx-2">·</span> <Link to="/terms-of-service" className="hover:underline">Terms of service</Link></p>
            </div>
        </main>
    );
}
