import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { apiErrorMessage, authService } from '../api/authService';
import { useAuth } from '../context/AuthContext';

export default function Register() {
    const navigate = useNavigate();
    const { isAccountAuthenticated } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [busy, setBusy] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [error, setError] = useState('');

    if (isAccountAuthenticated) return <Navigate to="/companies" replace />;

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setError('');
        if (password !== confirmPassword) return setError('Passwords do not match.');
        if (password.length < 8) return setError('Password must contain at least 8 characters.');
        if (new TextEncoder().encode(password).length > 72) return setError('Password must not exceed 72 UTF-8 bytes.');
        const trivial = /^(.)\1{7,}$/.test(password) || /^[0-9]+$/.test(password) ||
            /^(password|qwerty|abcdefgh|12345678|123456789|admin123|letmein)/i.test(password);
        if (trivial) return setError('Please choose a less predictable password. Avoid common words, sequences or repeated characters.');
        setBusy(true);
        try {
            const result = await authService.registerAccount(email.trim(), password);
            navigate(`/verify-email?email=${encodeURIComponent(email.trim())}`, { state: { message: result.message } });
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to submit registration.'));
        } finally {
            setBusy(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-50 flex items-center justify-center px-4 py-10">
            <div className="w-full max-w-md">
                <h1 className="text-center text-3xl font-bold text-emerald-700 mb-2">TenexERP</h1>
                <p className="text-center text-gray-500 mb-7">Create your account</p>
                <div className="rounded-2xl border border-gray-200 bg-white p-7 shadow-sm">
                    {error && <p role="alert" className="mb-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</p>}
                    <form onSubmit={submit} className="space-y-4">
                        <label className="block text-sm font-medium text-gray-700">Email address
                            <input required type="email" autoComplete="email" value={email} onChange={e => setEmail(e.target.value)} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5" />
                        </label>
                        <label className="block text-sm font-medium text-gray-700">Password
                            <input required type={showPassword ? "text" : "password"} autoComplete="new-password" minLength={8} maxLength={128} value={password} onChange={e => setPassword(e.target.value)} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5" />
                        </label>
                        <p className="mt-1 text-xs text-gray-500">At least 8 characters. A short phrase is fine; avoid passwords like 12345678 or ********.</p>
                        <label className="flex items-center gap-2 text-xs text-gray-600"><input type="checkbox" checked={showPassword} onChange={e => setShowPassword(e.target.checked)}/> Show password</label>
                        <label className="block text-sm font-medium text-gray-700">Confirm password
                            <input required type="password" autoComplete="new-password" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5" />
                        </label>
                        <button disabled={busy} className="w-full rounded-lg bg-emerald-600 px-4 py-2.5 font-semibold text-white hover:bg-emerald-700 disabled:opacity-50">{busy ? 'Submitting...' : 'Create account'}</button>
                    </form>
                    <p className="mt-5 text-center text-sm text-gray-600">Already registered? <Link className="font-medium text-emerald-700 hover:underline" to="/login">Sign in</Link></p>
                </div>
            </div>
        </div>
    );
}
