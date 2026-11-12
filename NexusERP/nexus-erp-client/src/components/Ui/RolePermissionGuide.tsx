import { useState } from 'react';

export function RolePermissionGuide() {
    const [visible, setVisible] = useState(
        () => localStorage.getItem('tenex_role_guide_seen') !== '1'
    );

    if (!visible) return null;

    return (
        <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-950">
            <h4 className="font-bold">How employee roles work</h4>

            <p className="mt-2 leading-6">
                Roles determine which features employees can access.
                You can customize roles under Employees → Roles & Permissions.
            </p>

            <p className="mt-2 leading-6">
                For example, edit the Cashier role and remove
                <strong> View receipts</strong> to prevent cashiers
                from opening receipts in the POS.
            </p>

            <button
                type="button"
                onClick={() => {
                    localStorage.setItem('tenex_role_guide_seen', '1');
                    setVisible(false);
                }}
                className="mt-3 rounded-lg bg-emerald-700 px-3 py-1.5 font-semibold text-white hover:bg-emerald-800"
            >
                Got it
            </button>
        </div>
    );
}