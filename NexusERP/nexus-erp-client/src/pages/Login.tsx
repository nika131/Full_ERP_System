import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiErrorMessage } from '../api/authService';

export default function Login() {
    const navigate = useNavigate();
    const location = useLocation();
    const { loginAccount, isAccountAuthenticated } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [busy, setBusy] = useState(false);

    if (isAccountAuthenticated) return <Navigate to="/companies" replace />;

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setError('');
        setBusy(true);
        try {
            await loginAccount(email.trim(), password);
            navigate('/companies', { replace: true });
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to sign in.'));
        } finally {
            setBusy(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-50 flex items-center justify-center px-4 py-10">
            <div className="w-full max-w-md">
                <div className="text-center mb-7">
                    <div className="text-3xl font-bold tracking-tight text-emerald-700">TenexERP</div>
                    <p className="mt-2 text-gray-500">Sign in to manage your companies</p>
                </div>
                <div className="bg-white shadow-sm border border-gray-200 rounded-2xl p-7">
                    <h1 className="text-xl font-semibold text-gray-900 mb-5">Welcome back</h1>
                    {(location.state as { message?: string } | null)?.message && <p role="status" className="mb-4 rounded-lg bg-emerald-50 border border-emerald-200 p-3 text-sm text-emerald-800">{(location.state as { message?: string }).message}</p>}
                    {error && <p role="alert" className="mb-4 rounded-lg bg-red-50 border border-red-200 p-3 text-sm text-red-700">{error}</p>}
                    <form onSubmit={submit} className="space-y-4">
                        <label className="block text-sm font-medium text-gray-700">Email address
                            <input autoComplete="email" type="email" required value={email} onChange={e => setEmail(e.target.value)} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                        </label>
                        <label className="block text-sm font-medium text-gray-700">Password
                            <input autoComplete="current-password" type="password" required value={password} onChange={e => setPassword(e.target.value)} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                        </label>
                        <button disabled={busy} className="w-full rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white font-semibold py-2.5 disabled:opacity-50">
                            {busy ? 'Signing in...' : 'Sign in'}
                        </button>
                    </form>
                    <p className="mt-5 text-center text-sm text-gray-600">New to TenexERP? <Link to="/register" className="font-medium text-emerald-700 hover:underline">Create an account</Link></p>
                </div>
                <p className="mt-5 text-center text-sm text-gray-500"><Link to="/privacy-policy" className="hover:underline">Privacy policy</Link> · <Link to="/terms-of-service" className="hover:underline">Terms of service</Link></p>
            </div>
        </div>
    );
}
