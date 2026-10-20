import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Building2, Plus, UserRoundPlus, ArrowRight, LogOut } from 'lucide-react';
import { apiErrorMessage, authService, type CompanySummary, type CreatedCompany, type JoinRequestSummary } from '../api/authService';
import { useAuth } from '../context/AuthContext';

export default function Companies() {
    const navigate = useNavigate();
    const { selectCompany, switchCompany, logout } = useAuth();
    const [companies, setCompanies] = useState<CompanySummary[]>([]);
    const [requests, setRequests] = useState<JoinRequestSummary[]>([]);
    const [view, setView] = useState<'list' | 'create' | 'join'>('list');
    const [companyName, setCompanyName] = useState('');
    const [fullName, setFullName] = useState('');
    const [username, setUsername] = useState('');
    const [posPin, setPosPin] = useState('');
    const [joinCode, setJoinCode] = useState('');
    const [created, setCreated] = useState<CreatedCompany | null>(null);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);

    const load = useCallback(async () => {
        setError('');
        try {
            const [c, r] = await Promise.all([authService.getMyCompanies(), authService.getMyJoinRequests()]);
            setCompanies(c.companies);
            setRequests(r.requests);
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Could not load your companies.'));
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { void load(); }, [load]);

    const enter = async (companyId: number) => {
        setBusy(true);
        setError('');
        try {
            await selectCompany(companyId);
            navigate('/profile');
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to enter this company.'));
        } finally {
            setBusy(false);
        }
    };

    const create = async (event: FormEvent) => {
        event.preventDefault();
        setBusy(true);
        setError('');
        try {
            const result = await authService.createCompany({ companyName, fullName, username, posPin: posPin || undefined });
            setCreated(result);
            setMessage(result.message);
            setView('list');
            await load();
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Company creation failed.'));
        } finally {
            setBusy(false);
        }
    };

    const join = async (event: FormEvent) => {
        event.preventDefault();
        setBusy(true);
        setError('');
        try {
            const result = await authService.submitJoinRequest({ joinCode, fullName, username });
            setMessage(result.message);
            setView('list');
            setJoinCode('');
            await load();
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to submit join request.'));
        } finally {
            setBusy(false);
        }
    };

    const signOut = async () => {
        setBusy(true);
        try {
            await logout();
            navigate('/login');
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Signed out locally, but server logout could not be confirmed.'));
            navigate('/login');
        } finally {
            setBusy(false);
        }
    };

    const fieldClass = 'mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5 focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500';
    const buttonClass = 'rounded-lg bg-emerald-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-emerald-700 disabled:opacity-50';

    return (
        <div className="min-h-screen bg-slate-50 px-4 py-8 sm:px-8">
            <div className="mx-auto max-w-5xl">
                <header className="mb-8 flex flex-wrap items-center justify-between gap-3">
                    <div>
                        <h1 className="text-2xl font-bold text-emerald-700">TenexERP</h1>
                        <p className="mt-1 text-sm text-gray-500">Choose a company or join a team</p>
                    </div>
                    <button type="button" onClick={() => { void signOut(); }} className="flex items-center gap-2 text-sm text-gray-600 hover:text-gray-900"><LogOut size={16} /> Sign out</button>
                </header>

                {error && <div role="alert" className="mb-5 rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">{error}</div>}
                {message && <div className="mb-5 rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-800">{message}</div>}
                {created && <div className="mb-5 rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-950">
                    <strong>Save this join code now — it is shown only once.</strong>
                    <div className="mt-2 flex flex-wrap items-center gap-3"><code className="break-all select-all font-mono text-sm">{created.joinCode}</code><button type="button" className="rounded bg-amber-200 px-2 py-1 text-xs" onClick={() => void navigator.clipboard.writeText(created.joinCode)}>Copy</button></div>
                    <p className="mt-2 text-xs">The code expires after 7 days, or immediately when an administrator rotates it.</p>
                </div>}

                <div className="mb-5 flex flex-wrap items-center gap-2">
                    <button onClick={() => { setView('list'); setError(''); }} className={`rounded-lg px-4 py-2 text-sm font-medium ${view === 'list' ? 'bg-gray-900 text-white' : 'bg-white border border-gray-200 text-gray-700'}`}>My companies</button>
                    <button onClick={() => { setView('create'); setError(''); }} className={`flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium ${view === 'create' ? 'bg-gray-900 text-white' : 'bg-white border border-gray-200 text-gray-700'}`}><Plus size={16} /> Create company</button>
                    <button onClick={() => { setView('join'); setError(''); }} className={`flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium ${view === 'join' ? 'bg-gray-900 text-white' : 'bg-white border border-gray-200 text-gray-700'}`}><UserRoundPlus size={16} /> Join company</button>
                </div>

                {view === 'list' && <>
                    <section className="grid gap-4 sm:grid-cols-2">
                        {loading ? <p className="text-gray-600">Loading companies...</p> : companies.length === 0 ?
                            <div className="col-span-full rounded-xl border border-dashed border-gray-300 bg-white p-8 text-center text-gray-600">You don't belong to a company yet. Create one or request to join an existing team.</div> :
                            companies.map(c => <div key={c.companyId} className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
                                <Building2 className="mb-3 text-emerald-600" size={24} />
                                <h2 className="text-lg font-semibold text-gray-900">{c.companyName}</h2>
                                <p className="mt-1 text-sm text-gray-500">{c.roleName} · @{c.username}</p>
                                <button disabled={busy} onClick={() => { switchCompany(); void enter(c.companyId); }} className={`mt-5 flex w-full items-center justify-center gap-2 ${buttonClass}`}>Enter ERP <ArrowRight size={16} /></button>
                            </div>)}
                    </section>
                    {requests.length > 0 && <section className="mt-8 rounded-xl border border-gray-200 bg-white p-5">
                        <h2 className="mb-4 text-lg font-semibold text-gray-900">My join requests</h2>
                        <div className="space-y-3">{requests.map(r => <div key={r.joinRequestId} className="flex flex-wrap items-center justify-between gap-2 border-b border-gray-100 pb-3 text-sm">
                            <div><strong>{r.companyName}</strong><p className="text-gray-500">Submitted {new Date(r.createdAt).toLocaleDateString()}</p></div>
                            <span className="rounded-lg bg-gray-100 px-3 py-1 text-gray-700">{typeof r.status === 'number' ? ['Pending', 'Approved', 'Rejected'][r.status] : r.status}</span>
                        </div>)}</div>
                    </section>}
                </>}

                {view === 'create' && <section className="max-w-xl rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
                    <h2 className="text-lg font-semibold text-gray-900">Create a new company</h2>
                    <p className="mt-1 mb-5 text-sm text-gray-500">You'll be its first administrator.</p>
                    <form onSubmit={create} className="space-y-4">
                        <label className="block text-sm font-medium">Company name<input required minLength={2} maxLength={150} value={companyName} onChange={e => setCompanyName(e.target.value)} className={fieldClass} /></label>
                        <label className="block text-sm font-medium">Your full name<input required minLength={2} maxLength={150} value={fullName} onChange={e => setFullName(e.target.value)} className={fieldClass} /></label>
                        <label className="block text-sm font-medium">ERP username<input required minLength={2} maxLength={100} value={username} onChange={e => setUsername(e.target.value)} className={fieldClass} /></label>
                        <label className="block text-sm font-medium">Optional POS PIN<input inputMode="numeric" pattern="[0-9]{4}" maxLength={4} value={posPin} onChange={e => setPosPin(e.target.value.replace(/\D/g, ''))} className={fieldClass} placeholder="Four digits" /></label>
                        <button disabled={busy} className={buttonClass}>{busy ? 'Creating...' : 'Create company'}</button>
                    </form>
                </section>}

                {view === 'join' && <section className="max-w-xl rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
                    <h2 className="text-lg font-semibold text-gray-900">Request to join a company</h2>
                    <p className="mt-1 mb-5 text-sm text-gray-500">An administrator must approve your request before you can enter the ERP.</p>
                    <form onSubmit={join} className="space-y-4">
                        <label className="block text-sm font-medium">Company join code<input required pattern="[A-Fa-f0-9]{32}" maxLength={32} value={joinCode} onChange={e => setJoinCode(e.target.value.trim().toUpperCase())} className={fieldClass + ' font-mono'} placeholder="32-character code" /></label>
                        <label className="block text-sm font-medium">Your full name<input required minLength={2} maxLength={150} value={fullName} onChange={e => setFullName(e.target.value)} className={fieldClass} /></label>
                        <label className="block text-sm font-medium">Preferred ERP username<input required minLength={2} maxLength={100} value={username} onChange={e => setUsername(e.target.value)} className={fieldClass} /></label>
                        <button disabled={busy} className={buttonClass}>{busy ? 'Submitting...' : 'Submit join request'}</button>
                    </form>
                </section>}
            </div>
        </div>
    );
}
