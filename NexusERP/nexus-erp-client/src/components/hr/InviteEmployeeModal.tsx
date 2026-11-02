
import { useState, type FormEvent } from 'react';
import { ChevronDown, KeyRound, RefreshCw, X } from 'lucide-react';
import toast from 'react-hot-toast';
import {
  useRolesQuery,
  useRegisterEmployeeMutation,
} from '../../hooks/queries/useHrQueries';

interface InviteModalProps {
  onClose: () => void;
  onSuccess: () => void;
}

export const InviteEmployeeModal = ({
  onClose,
  onSuccess,
}: InviteModalProps) => {
  const { data: roles = [] } = useRolesQuery();
  const mutation = useRegisterEmployeeMutation();

  const [fullName, setFullName] = useState('');
  const [username, setUsername] = useState('');
  const [posPin, setPosPin] = useState('');
  const [roleId, setRoleId] = useState(0);
  const [error, setError] = useState('');
  const [showAccessInfo, setShowAccessInfo] = useState(false);

  const availableRoles = roles.filter(
    role => role.name !== 'Admin' && role.permissions.includes('POS.View')
  );

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setError('');

    if (fullName.trim().length < 2 || username.trim().length < 3) {
      return setError('Enter a valid full name and username.');
    }

    if (!/^\d{4}$/.test(posPin)) {
      return setError('POS PIN must be exactly four digits.');
    }

    if (!availableRoles.some(role => role.roleId === roleId)) {
      return setError('Select a POS-enabled employee role.');
    }

    try {
      await mutation.mutateAsync({
        fullName: fullName.trim(),
        username: username.trim(),
        posPin,
        roleId,
      });

      toast.success(
        'POS-only employee created. They can sign in on an activated POS terminal using their PIN.'
      );

      onSuccess();
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'response' in err
          ? (
              err as {
                response?: { data?: { message?: string } };
              }
            ).response?.data?.message
          : undefined;

      setError(
        message || 'Could not create employee. Please try again.'
      );
    }
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/60 p-3 sm:p-4"
      role="presentation"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="pos-employee-title"
        className="flex max-h-[calc(100dvh-1.5rem)] w-full max-w-lg flex-col overflow-hidden rounded-2xl bg-white text-left shadow-2xl sm:max-h-[calc(100dvh-2rem)]"
      >
        {/* Header — stays visible */}
        <div className="flex shrink-0 items-start justify-between gap-3 border-b border-slate-100 px-4 py-4 sm:px-6">
          <div className="flex min-w-0 items-center gap-3">
            <span className="shrink-0 rounded-xl bg-emerald-100 p-2.5 text-emerald-700">
              <KeyRound size={21} aria-hidden="true" />
            </span>

            <div className="min-w-0">
              <div
                id="pos-employee-title"
                className="text-lg font-bold leading-6 text-slate-900"
              >
                Create POS employee
              </div>

              <p className="mt-1 text-xs leading-5 text-slate-500">
                PIN access for in-store staff — no website login
              </p>
            </div>
          </div>

          <button
            type="button"
            onClick={onClose}
            aria-label="Close employee form"
            className="shrink-0 rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
          >
            <X size={19} />
          </button>
        </div>

        {/* Only this section scrolls */}
        <div className="tenex-scroll min-h-0 flex-1 overflow-y-auto px-4 py-4 sm:px-6">
          {/* Expandable explanation */}
          <div className="rounded-xl border border-slate-200 bg-slate-50">
            <button
              type="button"
              onClick={() => setShowAccessInfo(value => !value)}
              aria-expanded={showAccessInfo}
              aria-controls="pos-employee-access-info"
              className="flex w-full items-center justify-between gap-3 px-3 py-2.5 text-left text-sm font-medium text-slate-700 hover:bg-slate-100"
            >
              <span>How does employee access work?</span>

              <ChevronDown
                size={17}
                className={`shrink-0 transition-transform ${
                  showAccessInfo ? 'rotate-180' : ''
                }`}
                aria-hidden="true"
              />
            </button>

            {showAccessInfo && (
              <div
                id="pos-employee-access-info"
                className="border-t border-slate-200 px-3 py-3 text-xs leading-6 text-slate-600"
              >
                <p>
                  <strong>POS employees</strong> use a four-digit PIN
                  to access an activated company POS terminal.
                  They cannot log into the ERP website.
                </p>

                <p className="mt-2">
                  <strong>Website employees</strong> create their own
                  account using an email address, submit a company
                  join request with a join code, and wait for
                  administrator approval. And they can access Both the ERP website and POS terminals.
                </p>
              </div>
            )}
          </div>

          {error && (
            <p
              role="alert"
              className="mt-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700"
            >
              {error}
            </p>
          )}

          <form
            id="create-pos-employee-form"
            onSubmit={submit}
            className="mt-4 space-y-4"
          >
            <div>
              <label
                htmlFor="pos-employee-name"
                className="block text-sm font-semibold text-slate-700"
              >
                Full name
              </label>

              <input
                id="pos-employee-name"
                value={fullName}
                onChange={event => setFullName(event.target.value)}
                required
                minLength={2}
                maxLength={150}
                autoComplete="off"
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-500"
              />
            </div>

            <div>
              <label
                htmlFor="pos-employee-username"
                className="block text-sm font-semibold text-slate-700"
              >
                Username
              </label>

              <input
                id="pos-employee-username"
                value={username}
                onChange={event => setUsername(event.target.value)}
                required
                minLength={3}
                maxLength={100}
                autoComplete="off"
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-500"
              />
            </div>

            <div>
              <label
                htmlFor="pos-employee-pin"
                className="block text-sm font-semibold text-slate-700"
              >
                POS PIN (required)
              </label>

              <div className="mt-1 flex min-w-0 gap-2">
                <input
                  id="pos-employee-pin"
                  value={posPin}
                  onChange={event =>
                    setPosPin(
                      event.target.value.replace(/\D/g, '').slice(0, 4)
                    )
                  }
                  required
                  inputMode="numeric"
                  pattern="[0-9]{4}"
                  maxLength={4}
                  autoComplete="off"
                  placeholder="4 digits"
                  className="min-w-0 flex-1 rounded-lg border border-slate-300 px-3 py-2.5 font-mono text-sm tracking-widest outline-none focus:border-emerald-500"
                />

                <button
                  type="button"
                  onClick={() =>
                    setPosPin(
                      String(
                        Math.floor(1000 + Math.random() * 9000)
                      )
                    )
                  }
                  className="inline-flex shrink-0 items-center gap-1.5 rounded-lg border border-slate-300 bg-slate-50 px-3 py-2 text-xs font-semibold text-slate-700 hover:bg-slate-100 sm:text-sm"
                >
                  <RefreshCw size={15} aria-hidden="true" />
                  Generate
                </button>
              </div>
            </div>

            <div>
              <label
                htmlFor="pos-employee-role"
                className="block text-sm font-semibold text-slate-700"
              >
                POS role
              </label>

              <select
                id="pos-employee-role"
                value={roleId}
                onChange={event =>
                  setRoleId(Number(event.target.value))
                }
                required
                className="mt-1 w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-500"
              >
                <option value={0}>Choose a POS role</option>

                {availableRoles.map(role => (
                  <option key={role.roleId} value={role.roleId}>
                    {role.name}
                  </option>
                ))}
              </select>
            </div>

            {availableRoles.length === 0 && (
              <p className="rounded-lg bg-amber-50 p-3 text-sm text-amber-900">
                No editable POS roles available. Create a role
                with POS.View permission first.
              </p>
            )}
          </form>
        </div>

        {/* Footer — always accessible */}
        <div className="flex shrink-0 items-center justify-end gap-2 border-t border-slate-100 bg-white px-4 py-3 sm:px-6">
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg px-4 py-2.5 text-sm font-medium text-slate-600 hover:bg-slate-100"
          >
            Cancel
          </button>

          <button
            type="submit"
            form="create-pos-employee-form"
            disabled={mutation.isPending || availableRoles.length === 0}
            className="rounded-lg bg-emerald-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-emerald-700 disabled:opacity-50"
          >
            {mutation.isPending
              ? 'Creating...'
              : 'Create POS employee'}
          </button>
        </div>
      </div>
    </div>
  );
};
