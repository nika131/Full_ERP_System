import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { ArrowRight, Boxes, CheckCircle2, Eye, EyeOff, Gift, Info } from 'lucide-react';
import { apiErrorMessage, authService } from '../api/authService';
import { useAuth } from '../context/AuthContext';
import { startVerificationCooldown } from '../utils/emailVerificationCooldown';

const inputClass = 'auth-input mt-1.5 block w-full rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10';

function passwordProblem(password: string): string | null {
    if (password.length < 8) return 'Use at least 8 characters.';
    if (new TextEncoder().encode(password).length > 72) return 'Password is too long (maximum 72 UTF-8 bytes).';
    const isPredictable = /^(.)\1{7,}$/.test(password) || /^[0-9]+$/.test(password) ||
        /^(password|qwerty|abcdefgh|12345678|123456789|admin123|letmein)/i.test(password);
    if (isPredictable) return 'Avoid common words, number-only passwords and repeated characters.';
    return null;
}

export default function Register() {
    const navigate = useNavigate();
    const { isAccountAuthenticated } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmation, setShowConfirmation] = useState(false);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState('');

    if (isAccountAuthenticated) return <Navigate to="/companies" replace />;

    const passwordIssue = password ? passwordProblem(password) : null;
    const confirmationMismatch = confirmPassword.length > 0 && password !== confirmPassword;

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setError('');
        if (password !== confirmPassword) return setError('Passwords do not match.');
        const issue = passwordProblem(password);
        if (issue) return setError(issue);
        setBusy(true);
        try {
            const normalizedEmail = email.trim();
            const result = await authService.registerAccount(normalizedEmail, password);
            startVerificationCooldown(normalizedEmail);
            navigate(`/verify-email?email=${encodeURIComponent(normalizedEmail)}`, {
                state: { message: result.message },
            });
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to submit registration.'));
        } finally {
            setBusy(false);
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
                    <h1 className="mt-7 mb-0 text-center text-[1.8rem] leading-[1.15] font-extrabold tracking-tight text-slate-900 sm:text-[2rem]">
                        Create your account
                    </h1>
                    <p className="mt-2 text-sm leading-6 text-slate-500">Get your business organized, all in one place.</p>
                </div>

                <div className="mb-5 flex items-start gap-3 rounded-2xl border border-emerald-200/80 bg-[#eaf6e8] px-4 py-4">
                    <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-white text-emerald-700"><Gift size={20} aria-hidden="true" /></span>
                    <div>
                        <p className="text-sm font-bold text-[#164737]">Your first 14 days: full feature access</p>
                        <p className="mt-1 text-xs leading-5 text-[#456556]">Explore inventory, POS, sales insights and team management in one workspace.</p>
                    </div>
                </div>

                <div className="rounded-[22px] border border-slate-200/90 bg-white p-6 shadow-[0_18px_55px_-25px_rgba(18,63,51,0.25)] sm:p-8">
                    {error && <p role="alert" className="mb-5 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>}
                    <form onSubmit={submit} className="space-y-5">
                        <div>
                            <label htmlFor="register-email" className="block text-sm font-semibold text-slate-700">Email address</label>
                            <input id="register-email" required type="email" autoComplete="email" maxLength={320} value={email} onChange={e => setEmail(e.target.value)} placeholder="you@company.com" className={inputClass} />
                        </div>
                        <div>
                            <label htmlFor="register-password" className="block text-sm font-semibold text-slate-700">Password</label>
                            <div className="relative">
                                <input id="register-password" required type={showPassword ? 'text' : 'password'} autoComplete="new-password" minLength={8} maxLength={128} value={password} onChange={e => setPassword(e.target.value)} aria-describedby="register-password-help" aria-invalid={!!passwordIssue} className={`${inputClass} pr-12`} />
                                <button type="button" onClick={() => setShowPassword(v => !v)} aria-label={showPassword ? 'Hide password' : 'Show password'} aria-pressed={showPassword} className="absolute inset-y-0 right-1 mt-1.5 flex w-11 items-center justify-center rounded-lg text-slate-500 transition hover:text-emerald-700 focus-visible:outline-2 focus-visible:outline-offset-[-3px] focus-visible:outline-emerald-600">
                                    {showPassword ? <EyeOff size={19} aria-hidden="true" /> : <Eye size={19} aria-hidden="true" />}
                                </button>
                            </div>
                            <div id="register-password-help" className={`mt-2 flex items-start gap-1.5 text-xs leading-5 ${password && !passwordIssue ? 'text-emerald-700' : 'text-slate-500'}`}>
                                {password && !passwordIssue ? <CheckCircle2 size={15} className="mt-0.5 shrink-0" aria-hidden="true" /> : <Info size={15} className="mt-0.5 shrink-0" aria-hidden="true" />}
                                <span>{password ? passwordIssue ?? 'Password meets the basic requirements.' : 'At least 8 characters; avoid common or easily guessed passwords.'}</span>
                            </div>
                        </div>
                        <div>
                            <label htmlFor="register-confirm-password" className="block text-sm font-semibold text-slate-700">Confirm password</label>
                            <div className="relative">
                                <input id="register-confirm-password" required type={showConfirmation ? 'text' : 'password'} autoComplete="new-password" maxLength={128} value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} aria-invalid={confirmationMismatch} aria-describedby={confirmationMismatch ? 'register-confirm-error' : undefined} className={`${inputClass} pr-12 ${confirmationMismatch ? 'border-red-300' : ''}`} />
                                <button type="button" onClick={() => setShowConfirmation(v => !v)} aria-label={showConfirmation ? 'Hide confirmation password' : 'Show confirmation password'} aria-pressed={showConfirmation} className="absolute inset-y-0 right-1 mt-1.5 flex w-11 items-center justify-center rounded-lg text-slate-500 transition hover:text-emerald-700 focus-visible:outline-2 focus-visible:outline-offset-[-3px] focus-visible:outline-emerald-600">
                                    {showConfirmation ? <EyeOff size={19} aria-hidden="true" /> : <Eye size={19} aria-hidden="true" />}
                                </button>
                            </div>
                            {confirmationMismatch && <p id="register-confirm-error" className="mt-1.5 text-xs text-red-600">Passwords do not match yet.</p>}
                        </div>
                        <button type="submit" disabled={busy} className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#163f35] px-4 py-3.5 text-sm font-bold text-white transition hover:bg-[#215b4a] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 disabled:cursor-wait disabled:opacity-60">
                            {busy ? 'Creating account...' : 'Create account'} {!busy && <ArrowRight size={17} aria-hidden="true" />}
                        </button>
                    </form>
                    <p className="mt-6 text-center text-sm text-slate-500">Already have an account? <Link className="font-semibold text-emerald-700 hover:underline" to="/login">Sign in</Link></p>
                </div>
                <p className="mt-6 text-center text-xs text-slate-500"><Link to="/privacy-policy" className="hover:underline">Privacy policy</Link> <span className="mx-2">·</span> <Link to="/terms-of-service" className="hover:underline">Terms of service</Link></p>
            </div>
        </main>
    );
}
