import { Permissions } from '../constants/permissions';
import { useAuth } from '../context/AuthContext';
import { CopyCodeButton } from '../components/Ui/CopyCodeButton';
import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { RefreshCw, ShieldCheck } from 'lucide-react';
import { companyAdminService, type RoleOption } from '../api/companyAdminService';
import { apiErrorMessage, type JoinRequestSummary } from '../api/authService';

export default function CompanyAdministration() {
    const { hasPermission } = useAuth();
    const canReview = hasPermission(Permissions.ManageJoinRequests);
    const canRotate = hasPermission(Permissions.ManageJoinCodes);
    const [requests, setRequests] = useState<JoinRequestSummary[]>([]);
    const [roles, setRoles] = useState<RoleOption[]>([]);
    const [selectedRoles, setSelectedRoles] = useState<Record<number, number>>({});
    const [newCode, setNewCode] = useState<{ code: string; expires: string } | null>(null);
    const [busyId, setBusyId] = useState<number | null>(null);
    const [busyRotation, setBusyRotation] = useState(false);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');

    const reload = useCallback(async () => {
        if (!canReview) return;
        try {
            const [pending, options] = await Promise.all([companyAdminService.getPending(), companyAdminService.getRoles()]);
            setRequests(pending);
            setRoles(options.filter(r => !r.isSystemRole));
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to retrieve pending requests.'));
        }
    }, [canReview]);

    useEffect(() => { void reload(); }, [reload]);

    const decide = async (id: number, action: 'approve' | 'reject') => {
        const roleId = selectedRoles[id];
        if (action === 'approve' && !roleId) return setError('Choose an employee role before approving.');
        setBusyId(id);
        setError('');
        try {
            const result = action === 'approve' ? await companyAdminService.approve(id, roleId) : await companyAdminService.reject(id);
            setMessage(result.message);
            await reload();
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to review request.'));
        } finally {
            setBusyId(null);
        }
    };

    const rotate = async () => {
        if (!window.confirm('Regenerate the join code? The previous code will stop working immediately.')) return;
        setBusyRotation(true);
        setError('');
        try {
            const result = await companyAdminService.rotateJoinCode();
            setNewCode({ code: result.joinCode, expires: result.expiresAt });
            setMessage(result.message);
        } catch (err: unknown) {
            setError(apiErrorMessage(err, 'Unable to rotate the join code.'));
        } finally {
            setBusyRotation(false);
        }
    };

    return (
        <div className="mx-auto max-w-5xl space-y-6 text-left">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Company access</h1>
                <p className="mt-1 text-sm text-gray-500">Review employees and manage your company invitation code.</p>
            </div>
            {error && <div role="alert" className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">{error}</div>}
            {message && <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-800">{message}</div>}
            {canRotate && <section className="rounded-xl border border-gray-200 bg-white p-6">
                <div className="flex flex-wrap items-center justify-between gap-3">
                    <div><h2 className="text-lg font-semibold text-gray-900">Company join code</h2><p className="mt-1 text-sm text-gray-500">Codes expire after 7 days. Rotation invalidates the previous code.</p></div>
                    <button type="button" disabled={busyRotation} onClick={() => { void rotate(); }} className="flex items-center gap-2 rounded-lg bg-gray-900 px-4 py-2 text-sm font-semibold text-white hover:bg-gray-800 disabled:opacity-50"><RefreshCw size={16} /> {busyRotation ? 'Generating...' : 'Generate new code'}</button>
                </div>
                {newCode && <div className="mt-4 rounded-lg bg-amber-50 p-4 text-amber-950">
                    <strong>Copy this code now. It cannot be retrieved later.</strong>
                    <div className="mt-2 flex flex-wrap gap-2"><code className="select-all break-all font-mono text-sm">{newCode.code}</code><CopyCodeButton code={newCode.code} /></div>
                    <p className="mt-2 text-xs">Expires: {new Date(newCode.expires).toLocaleString()}</p>
                </div>}
            </section>}
            {canReview && <section className="rounded-xl border border-gray-200 bg-white p-6">
                <div className="mb-4 flex items-center justify-between"><h2 className="text-lg font-semibold text-gray-900">Pending join requests ({requests.length})</h2><button onClick={() => void reload()} className="text-sm font-medium text-emerald-700 hover:underline">Refresh</button></div>
                {roles.length === 0 && <p className="mb-4 rounded-lg bg-amber-50 p-3 text-sm text-amber-800">Create a non-system employee role using <Link className="font-semibold underline" to="/employees">Employees / Roles</Link> before approving requests.</p>}
                {requests.length === 0 ? <p className="text-sm text-gray-500">No pending join requests.</p> : <div className="space-y-4">
                    {requests.map(req => <div key={req.joinRequestId} className="rounded-lg border border-gray-200 p-4">
                        <div className="mb-3 flex flex-wrap items-start justify-between gap-2"><div><div className="font-semibold text-gray-900">{req.fullName} <span className="font-normal text-gray-500">(@{req.username})</span></div><div className="text-sm text-gray-500">{req.accountEmail}</div></div><span className="text-xs text-gray-500">{new Date(req.createdAt).toLocaleString()}</span></div>
                        <div className="flex flex-wrap gap-2 items-center">
                            <select value={selectedRoles[req.joinRequestId] || ''} onChange={e => setSelectedRoles(prev => ({ ...prev, [req.joinRequestId]: Number(e.target.value) }))} className="rounded-lg border border-gray-300 px-3 py-2 text-sm" aria-label={`Assign role to ${req.fullName}`}>
                                <option value="">Choose employee role</option>
                                {roles.map(role => <option key={role.roleId} value={role.roleId}>{role.name}</option>)}
                            </select>
                            <button type="button" disabled={busyId !== null || !selectedRoles[req.joinRequestId]} onClick={() => void decide(req.joinRequestId, 'approve')} className="flex items-center gap-2 rounded-lg bg-emerald-600 px-3 py-2 text-sm font-semibold text-white disabled:opacity-50"><ShieldCheck size={16} /> Approve</button>
                            <button type="button" disabled={busyId !== null} onClick={() => void decide(req.joinRequestId, 'reject')} className="rounded-lg border border-red-300 px-3 py-2 text-sm font-medium text-red-700 disabled:opacity-50">Reject</button>
                        </div>
                    </div>)}
                </div>}
            </section>}
        </div>
    );
}
