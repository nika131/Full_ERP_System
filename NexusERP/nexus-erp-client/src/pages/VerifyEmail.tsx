import { useState, type FormEvent } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { apiErrorMessage, authService } from '../api/authService';

export default function VerifyEmail() {
    const [params] = useSearchParams();
    const navigate = useNavigate();
    const location = useLocation();
    const [email, setEmail] = useState(params.get('email') ?? '');
    const [code, setCode] = useState('');
    const [busy, setBusy] = useState(false);
    const [resendBusy, setResendBusy] = useState(false);
    const [message, setMessage] = useState((location.state as { message?: string } | null)?.message ?? 'Enter the six-digit code sent to your email address.');
    const [error, setError] = useState('');

    const verify = async (event: FormEvent) => {
        event.preventDefault();
        setError('');
        setBusy(true);
        try {
            await authService.verifyEmail(email.trim(), code.trim());
            navigate('/login', { replace: true, state: { message: 'Email verified successfully. You can now sign in.' } });
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to verify this code.'));
        } finally {
            setBusy(false);
        }
    };

    const resend = async () => {
        if (!email.trim()) return setError('Please enter your email address.');
        setError('');
        setResendBusy(true);
        try {
            const result = await authService.resendCode(email.trim());
            setMessage(result.message);
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to request another code.'));
        } finally {
            setResendBusy(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-50 flex items-center justify-center px-4 py-10">
            <div className="w-full max-w-md rounded-2xl border border-gray-200 bg-white p-7 shadow-sm">
                <h1 className="text-2xl font-bold text-gray-900">Verify your email</h1>
                <p className="mt-2 mb-5 text-sm text-gray-600">{message}</p>
                {error && <p role="alert" className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</p>}
                <form onSubmit={verify} className="space-y-4">
                    <label className="block text-sm font-medium text-gray-700">Email address
                        <input required type="email" value={email} onChange={e => setEmail(e.target.value)} className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5" />
                    </label>
                    <label className="block text-sm font-medium text-gray-700">Verification code
                        <input required inputMode="numeric" autoComplete="one-time-code" pattern="[0-9]{6}" maxLength={6} value={code} onChange={e => setCode(e.target.value.replace(/\D/g, ''))} placeholder="000000" className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5 tracking-widest" />
                    </label>
                    <button disabled={busy} className="w-full rounded-lg bg-emerald-600 py-2.5 font-semibold text-white hover:bg-emerald-700 disabled:opacity-50">{busy ? 'Verifying...' : 'Verify email'}</button>
                </form>
                <button type="button" onClick={resend} disabled={resendBusy} className="mt-4 w-full text-sm font-medium text-emerald-700 hover:underline disabled:opacity-50">{resendBusy ? 'Requesting...' : 'Resend code'}</button>
                <p className="mt-4 text-center text-sm text-gray-500"><Link to="/login" className="hover:underline">Back to sign in</Link></p>
            </div>
        </div>
    );
}
