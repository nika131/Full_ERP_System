import { useState, type FormEvent } from 'react';
import { KeyRound, RefreshCw, ShieldCheck, X } from 'lucide-react';
import toast from 'react-hot-toast';
import { useRolesQuery, useRegisterEmployeeMutation } from '../../hooks/queries/useHrQueries';

interface InviteModalProps { onClose: () => void; onSuccess: () => void; }

export const InviteEmployeeModal = ({ onClose, onSuccess }: InviteModalProps) => {
    const { data: roles = [] } = useRolesQuery();
    const mutation = useRegisterEmployeeMutation();
    const [fullName, setFullName] = useState('');
    const [username, setUsername] = useState('');
    const [posPin, setPosPin] = useState('');
    const [roleId, setRoleId] = useState(0);
    const [error, setError] = useState('');
    const availableRoles = roles.filter(role => role.name !== 'Admin' && role.permissions.includes('POS.View'));

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setError('');
        if (fullName.trim().length < 2 || username.trim().length < 3) return setError('Enter a valid full name and username.');
        if (!/^\d{4}$/.test(posPin)) return setError('POS PIN must be exactly four digits.');
        if (!availableRoles.some(r => r.roleId === roleId)) return setError('Select a POS-enabled employee role.');
        try {
            await mutation.mutateAsync({ fullName: fullName.trim(), username: username.trim(), posPin, roleId });
            toast.success('POS-only employee created. They can sign in on an activated POS terminal using their PIN.');
            onSuccess();
        } catch (err: unknown) {
            const message = err && typeof err === 'object' && 'response' in err
                ? (err as { response?: { data?: { message?: string } } }).response?.data?.message
                : undefined;
            setError(message || 'Could not create employee. Please try again.');
        }
    };

    return <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/60 p-4" role="presentation">
        <div role="dialog" aria-modal="true" aria-labelledby="pos-employee-title" className="w-full max-w-lg rounded-2xl bg-white p-6 text-left shadow-2xl">
            <div className="flex items-start justify-between gap-4">
                <div className="flex items-center gap-3"><span className="rounded-xl bg-emerald-100 p-3 text-emerald-700"><KeyRound size={22}/></span><div><h2 id="pos-employee-title" className="text-xl font-bold text-slate-900">Add POS cashier</h2><p className="text-sm text-slate-500">Quick account for in-store staff</p></div></div>
                <button type="button" onClick={onClose} className="rounded-lg p-2 text-slate-400 hover:bg-slate-100" aria-label="Close"><X size={19}/></button>
            </div>
            <div className="mt-5 rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-sm leading-6 text-emerald-950">
                <div className="mb-1 flex items-center gap-2 font-semibold"><ShieldCheck size={17}/> How this works</div>
                This creates a <strong>POS-only employee</strong> with a required four-digit PIN. They can switch into the POS app on a company-activated terminal. They <strong>cannot sign into the ERP website</strong>. For employees who need website access, ask them to register with email and join through Company access.
            </div>
            {error && <p role="alert" className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</p>}
            <form onSubmit={submit} className="mt-5 space-y-4">
                <label className="block text-sm font-semibold text-slate-700">Full name
                    <input value={fullName} onChange={e=>setFullName(e.target.value)} required minLength={2} maxLength={150} className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2.5 font-normal outline-none focus:border-emerald-500" autoComplete="off" />
                </label>
                <label className="block text-sm font-semibold text-slate-700">Username
                    <input value={username} onChange={e=>setUsername(e.target.value)} required minLength={3} maxLength={100} className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2.5 font-normal outline-none focus:border-emerald-500" autoComplete="off" />
                </label>
                <label className="block text-sm font-semibold text-slate-700">POS PIN (required)
                    <span className="mt-1 flex gap-2"><input value={posPin} onChange={e=>setPosPin(e.target.value.replace(/\D/g,'').slice(0,4))} required inputMode="numeric" pattern="[0-9]{4}" maxLength={4} className="min-w-0 flex-1 rounded-lg border border-slate-300 px-3 py-2.5 font-mono tracking-widest outline-none focus:border-emerald-500" placeholder="4 digits" autoComplete="off" />
                    <button type="button" onClick={() => setPosPin(String(Math.floor(1000 + Math.random() * 9000)))} className="flex items-center gap-2 rounded-lg border border-slate-300 bg-slate-50 px-3 text-sm font-medium text-slate-700 hover:bg-slate-100"><RefreshCw size={15}/> Generate</button></span>
                </label>
                <label className="block text-sm font-semibold text-slate-700">POS role
                    <select value={roleId} onChange={e=>setRoleId(Number(e.target.value))} required className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2.5 font-normal outline-none focus:border-emerald-500">
                        <option value={0}>Choose a POS role</option>
                        {availableRoles.map(role=><option key={role.roleId} value={role.roleId}>{role.name}</option>)}
                    </select>
                </label>
                {availableRoles.length === 0 && <p className="rounded-lg bg-amber-50 p-3 text-sm text-amber-900">No editable POS roles available. Create a role with POS.View first.</p>}
                <div className="flex justify-end gap-2 border-t border-slate-100 pt-4"><button type="button" onClick={onClose} className="rounded-lg px-4 py-2 text-slate-600 hover:bg-slate-100">Cancel</button><button type="submit" disabled={mutation.isPending || availableRoles.length === 0} className="rounded-lg bg-emerald-600 px-5 py-2 font-semibold text-white hover:bg-emerald-700 disabled:opacity-50">{mutation.isPending?'Creating...':'Create POS cashier'}</button></div>
            </form>
        </div>
    </div>;
};
