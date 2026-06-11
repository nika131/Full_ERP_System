import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import { employeeService } from '../../api/employeeService';
import type { EmployeeResponse, SalaryRecordResponse } from '../../types/employee';
import apiClient from '../../api/apiClient'; 

interface EmployeeManagerFormProps {
    employee: EmployeeResponse;
    onSuccess: () => void;
    onCancel: () => void;
}

export const EmployeeManagerForm = ({ employee, onSuccess, onCancel }: EmployeeManagerFormProps) => {
    const [activeTab, setActiveTab] = useState<'Role' | 'Salary'>('Role');
    const [isLoading, setIsLoading] = useState(false);

    const [roles, setRoles] = useState<{ roleId: number, name: string }[]>([]);
    const [roleForm, setRoleForm] = useState({
        fullName: employee.fullName,
        username: employee.username,
        roleId: employee.roleId
    });

    const [salaryHistory, setSalaryHistory] = useState<SalaryRecordResponse[]>([]);
    const [isFetchingSalary, setIsFetchingSalary] = useState(false);
    const [salaryForm, setSalaryForm] = useState({
        amount: '',
        effectiveDate: new Date().toISOString().split('T')[0], 
        notes: ''
    });

    useEffect(() => {
        apiClient.get('/roles/lookup').then(res => setRoles(res.data)).catch(console.error);
    }, []);

    useEffect(() => {
        if (activeTab === 'Salary') {
            loadSalaryHistory();
        }
    }, [activeTab, employee.userId]);

    const loadSalaryHistory = async () => {
        try {
            setIsFetchingSalary(true);
            const data = await employeeService.getSalaryHistory(employee.userId);
            setSalaryHistory(data);
        } catch (error) {
            toast.error("Failed to load salary history.");
        } finally {
            setIsFetchingSalary(false);
        }
    };

    const handleRoleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            await employeeService.updateEmployee(employee.userId, { 
                ...roleForm, 
                salary: employee.salary 
            });
            toast.success("Employee profile updated.");
            onSuccess();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to update employee.");
        } finally {
            setIsLoading(false);
        }
    };

    const handleSalarySubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            await employeeService.addSalaryRecord(employee.userId, {
                amount: parseFloat(salaryForm.amount),
                effectiveDate: salaryForm.effectiveDate,
                notes: salaryForm.notes || null
            });
            toast.success("New salary record added.");
            setSalaryForm({ amount: '', effectiveDate: new Date().toISOString().split('T')[0], notes: '' });
            loadSalaryHistory(); 
            onSuccess();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to add salary.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex flex-col h-full">
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

            <div className="flex-1 overflow-y-auto p-6">
                {activeTab === 'Role' ? (
                    <form id="role-form" onSubmit={handleRoleSubmit} className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-slate-700">Full Name</label>
                            <input 
                                type="text" required
                                className="mt-1 w-full p-2 border border-slate-300 rounded-md outline-none focus:border-blue-500"
                                value={roleForm.fullName}
                                onChange={e => setRoleForm({...roleForm, fullName: e.target.value})}
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-slate-700">Username</label>
                            <input 
                                type="text" required
                                className="mt-1 w-full p-2 border border-slate-300 rounded-md outline-none focus:border-blue-500"
                                value={roleForm.username}
                                onChange={e => setRoleForm({...roleForm, username: e.target.value})}
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-slate-700">System Role</label>
                            <select 
                                className="mt-1 w-full p-2 border border-slate-300 rounded-md outline-none focus:border-blue-500"
                                value={roleForm.roleId}
                                onChange={e => setRoleForm({...roleForm, roleId: Number(e.target.value)})}
                            >
                                {roles.map(r => (
                                    <option key={r.roleId} value={r.roleId}>{r.name}</option>
                                ))}
                            </select>
                        </div>
                    </form>
                ) : (
                    <div className="space-y-6">
                        {/* New Salary Form */}
                        <form id="salary-form" onSubmit={handleSalarySubmit} className="bg-slate-50 p-4 rounded-md border border-slate-200 space-y-4">
                            <h4 className="text-sm font-bold text-slate-800">Issue New Salary Contract</h4>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-xs font-medium text-slate-700">New Amount ($)</label>
                                    <input 
                                        type="number" min="0" step="0.01" required
                                        className="mt-1 w-full p-2 text-sm border border-slate-300 rounded-md outline-none focus:border-blue-500"
                                        value={salaryForm.amount}
                                        onChange={e => setSalaryForm({...salaryForm, amount: e.target.value})}
                                    />
                                </div>
                                <div>
                                    <label className="block text-xs font-medium text-slate-700">Effective Date</label>
                                    <input 
                                        type="date" required
                                        className="mt-1 w-full p-2 text-sm border border-slate-300 rounded-md outline-none focus:border-blue-500"
                                        value={salaryForm.effectiveDate}
                                        onChange={e => setSalaryForm({...salaryForm, effectiveDate: e.target.value})}
                                    />
                                </div>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-slate-700">Reason / Notes</label>
                                <input 
                                    type="text" required placeholder="e.g., Annual Review, Promotion"
                                    className="mt-1 w-full p-2 text-sm border border-slate-300 rounded-md outline-none focus:border-blue-500"
                                    value={salaryForm.notes}
                                    onChange={e => setSalaryForm({...salaryForm, notes: e.target.value})}
                                />
                            </div>
                            <button type="submit" disabled={isLoading} className="w-full py-2 bg-slate-800 text-white rounded-md text-sm font-medium hover:bg-slate-700">
                                Apply Contract
                            </button>
                        </form>

                        {/* Salary History Table */}
                        <div>
                            <h4 className="text-sm font-bold text-slate-800 mb-2">Contract History</h4>
                            {isFetchingSalary ? <p className="text-sm text-slate-500">Loading...</p> : (
                                <table className="w-full text-left text-sm border border-slate-200">
                                    <thead className="bg-slate-100 text-slate-600">
                                        <tr>
                                            <th className="p-2">Amount</th>
                                            <th className="p-2">Effective</th>
                                            <th className="p-2">Reason</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-slate-200">
                                        {salaryHistory.map(record => (
                                            <tr key={record.salaryRecordId}>
                                                <td className="p-2 font-medium text-emerald-600">${record.amount.toLocaleString()}</td>
                                                <td className="p-2">{new Date(record.effectiveDate).toLocaleDateString()}</td>
                                                <td className="p-2 text-slate-500">{record.notes}</td>
                                            </tr>
                                        ))}
                                        {salaryHistory.length === 0 && (
                                            <tr><td colSpan={3} className="p-4 text-center text-slate-500">No salary history found.</td></tr>
                                        )}
                                    </tbody>
                                </table>
                            )}
                        </div>
                    </div>
                )}
            </div>

            {/* Footer Actions */}
            <div className="p-6 border-t border-slate-200 flex justify-end gap-3 bg-slate-50">
                <button type="button" onClick={onCancel} className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-md hover:bg-slate-50">
                    Cancel
                </button>
                {activeTab === 'Role' && (
                    <button type="submit" form="role-form" disabled={isLoading} className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700">
                        {isLoading ? 'Saving...' : 'Save Profile'}
                    </button>
                )}
            </div>
        </div>
    );
};