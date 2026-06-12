import { useState } from 'react';
import type { EmployeeResponse } from '../../types/employee';
import { RoleForm } from '../forms/RoleProfileForm';
import { SalaryForm } from '../forms/SalaryLedgerForm';

interface EmployeeManagerFormProps {
    employee: EmployeeResponse;
    onSuccess: () => void;
    onCancel: () => void;
}

export const EmployeeManagerForm = ({ employee, onSuccess, onCancel }: EmployeeManagerFormProps) => {
    const [activeTab, setActiveTab] = useState<'Role' | 'Salary'>('Role');

    return (
        <div className="flex flex-col h-full bg-white relative">
            {/* Global Cancel Button*/}
            <div className="absolute top-4 right-6 z-10">
                <button type="button" onClick={onCancel} className="text-slate-400 hover:text-slate-600">
                    ✕
                </button>
            </div>

            {/* Tabs */}
            <div className="flex border-b border-slate-200 px-6 mt-4">
                <button 
                    onClick={() => setActiveTab('Role')}
                    className={`pb-3 px-4 font-medium text-sm ${activeTab === 'Role' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-slate-500 hover:text-slate-700'}`}
                >
                    Profile & Role
                </button>
                <button 
                    onClick={() => setActiveTab('Salary')}
                    className={`pb-3 px-4 font-medium text-sm ${activeTab === 'Salary' ? 'border-b-2 border-blue-600 text-blue-600' : 'text-slate-500 hover:text-slate-700'}`}
                >
                    Salary Ledger
                </button>
            </div>

            {/* View Port */}
            {activeTab === 'Role' ? (
                <RoleForm employee={employee} onSuccess={onSuccess} />
            ) : (
                <SalaryForm employee={employee} onSuccess={onSuccess} />
            )}
        </div>
    );
};