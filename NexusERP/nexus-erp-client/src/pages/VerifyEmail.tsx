import { useEffect, useState, type FormEvent } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { ArrowRight, Boxes, MailCheck } from 'lucide-react';
import { apiErrorMessage, authService } from '../api/authService';
import { getVerificationCooldownUntil, startVerificationCooldown } from '../utils/emailVerificationCooldown';

const inputClass = 'mt-1.5 block w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10';

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
    const [cooldownUntil, setCooldownUntil] = useState(() => getVerificationCooldownUntil(params.get('email') ?? ''));
    const [now, setNow] = useState(() => Date.now());

    useEffect(() => {
        if (cooldownUntil <= Date.now()) return;
        const interval = window.setInterval(() => {
            const currentTime = Date.now();
            setNow(currentTime);
            if (currentTime >= cooldownUntil) window.clearInterval(interval);
        }, 1000);
        return () => window.clearInterval(interval);
    }, [cooldownUntil]);

    const secondsRemaining = Math.max(0, Math.ceil((cooldownUntil - now) / 1000));
    const resendLabel = secondsRemaining > 0
        ? `Resend code in ${Math.floor(secondsRemaining / 60)}:${String(secondsRemaining % 60).padStart(2, '0')}`
        : 'Resend verification code';

    const updateEmail = (value: string) => {
        setEmail(value);
        setCooldownUntil(getVerificationCooldownUntil(value));
        setNow(Date.now());
    };

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
        if (resendBusy || secondsRemaining > 0 || busy) return;
        const address = email.trim();
        if (!address) return setError('Please enter your email address.');
        setError('');
        setResendBusy(true);
        try {
            const result = await authService.resendCode(address);
            setMessage(result.message);
            setCooldownUntil(startVerificationCooldown(address));
            setNow(Date.now());
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to request another code.'));
        } finally {
            setResendBusy(false);
        }
    };

    return (
       <main className="auth-page flex min-h-dvh justify-center bg-[#f7faf7] px-4 py-8 text-left sm:px-6 sm:py-12">
            <div className="my-auto w-full max-w-[460px]">
                <div className="mb-8 text-center">
                    <Link to="/" aria-label="TenexERP home" className="inline-flex items-center gap-2.5 text-[#163f35] transition hover:opacity-80">
                        <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#163f35] text-white"><Boxes size={21} aria-hidden="true" /></span>
                        <span className="text-2xl font-extrabold tracking-tight">Tenex<span className="text-emerald-600">ERP</span></span>
                    </Link>
                    <div className="mx-auto mt-7 flex h-12 w-12 items-center justify-center rounded-2xl bg-emerald-100 text-emerald-700"><MailCheck size={25} aria-hidden="true" /></div>
                    <h1 className="mt-4 mb-0 text-center text-[1.8rem] leading-[1.2] font-extrabold tracking-tight text-slate-900 sm:text-[2rem]">
                        Verify your email
                    </h1>
                    <p role="status" className="mt-2 text-sm leading-6 text-slate-500">{message}</p>
                </div>

                <div className="rounded-[22px] border border-slate-200/90 bg-white p-6 shadow-[0_18px_55px_-25px_rgba(18,63,51,0.25)] sm:p-8">
                    {error && <p role="alert" className="mb-5 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>}
                    <form onSubmit={verify} className="space-y-5">
                        <div>
                            <label htmlFor="verify-email" className="block text-sm font-semibold text-slate-700">Email address</label>
                            <input id="verify-email" required type="email" autoComplete="email" value={email} onChange={e => updateEmail(e.target.value)} disabled={busy || resendBusy} className={inputClass} />
                        </div>
                        <div>
                            <label htmlFor="verify-code" className="block text-sm font-semibold text-slate-700">Verification code</label>
                            <input id="verify-code" required inputMode="numeric" autoComplete="one-time-code" pattern="[0-9]{6}" maxLength={6} value={code} onChange={e => setCode(e.target.value.replace(/\D/g, ''))} placeholder="000000" className={`${inputClass} text-center text-lg font-semibold tracking-[0.3em]`} />
                            <p className="mt-2 text-xs text-slate-500">Check your inbox and spam folder for the six-digit code.</p>
                        </div>
                        <button type="submit" disabled={busy || resendBusy} className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#163f35] px-4 py-3.5 text-sm font-bold text-white transition hover:bg-[#215b4a] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 disabled:cursor-wait disabled:opacity-60">
                            {busy ? 'Verifying...' : 'Verify email'} {!busy && <ArrowRight size={17} aria-hidden="true" />}
                        </button>
                    </form>
                    <div className="mt-6 border-t border-slate-100 pt-5 text-center">
                        <p className="mb-2 text-xs text-slate-500">Didn't receive the email?</p>
                        <button type="button" onClick={resend} disabled={resendBusy || busy || secondsRemaining > 0} className="rounded-lg px-3 py-2 text-sm font-semibold text-emerald-700 transition hover:bg-emerald-50 focus-visible:outline-2 focus-visible:outline-emerald-600 disabled:cursor-not-allowed disabled:text-slate-400 disabled:hover:bg-transparent">
                            {resendBusy ? 'Requesting...' : resendLabel}
                        </button>
                        {secondsRemaining > 0 && <p className="mt-1 text-xs text-slate-500">You can request another code when the timer ends.</p>}
                    </div>
                    <p className="mt-5 text-center text-sm text-slate-500"><Link to="/login" className="font-medium hover:text-emerald-700 hover:underline">Back to sign in</Link></p>
                </div>
            </div>
        </main>
    );
}
