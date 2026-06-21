import { useMemo, useState } from "react";
import { DataTable, type ColumnDef } from "../components/Ui/DataTable";
import { SlideOver } from "../components/Ui/SlideOver";
import { ConfirmDialog } from "../components/Ui/ConfirmDialog";
import type { EmployeeResponse } from "../types/employee";
import { EmployeeManagerForm } from "../components/hr/EmployeeManagerForm";
import { RolesManager } from "../components/hr/RolesManager";
import { useNavigate } from "react-router-dom";
import { InviteEmployeeModal } from "../components/hr/InviteEmployeeModal";
import { useQueryClient } from "@tanstack/react-query";
import { useEmployeesQuery, useRolesQuery, usePendingLeavQuery, useDeleteEmployeeMutation } from "../hooks/queries/useHrQueries";

export default function EmployeeList() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    const [page, setPage] = useState(1);
    const [searchTerm, setSearchTerm] = useState('');
    const [roleFilter, setRoleFilter] = useState('All');
    const [activeView, setActiveView] = useState<'Directory' | 'Roles'>('Directory');
    
    const [isSlideOverOpen, setIsSlideOverOpen] = useState(false);
    const [selectedEmployee, setSelectedEmployee] = useState<EmployeeResponse | null>(null);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);

    const { data: employeesData, isLoading: isLoadingEmployees } = useEmployeesQuery(page, 10, searchTerm, roleFilter);
    const { data: roles = [] } = useRolesQuery();
    const { data: pendingLeaves = [] } = usePendingLeavQuery();
    const deleteEmployeeMutation = useDeleteEmployeeMutation();

    const employees = employeesData?.items || [];
    const totalPages = employeesData?.totalPages || 1;
    const totalCount = employeesData?.totalCount || 0;
    const pendingLeavesCount = pendingLeaves.length;

    const handleConfirmDelete = async () => {
        if (!selectedEmployee) return;
        try {
            await deleteEmployeeMutation.mutateAsync(selectedEmployee.userId);
            setIsDeleteDialogOpen(false);
            setSelectedEmployee(null);
        } catch (err) {
            console.error("Deletion failed", err);
        }
    };

    const columns = useMemo<ColumnDef<EmployeeResponse>[]>(() => [
        { header: 'ID', accessor: 'userId', className: 'w-16' },
        { header: 'Full Name', accessor: 'fullName', className: 'font-bold text-slate-800' },
        { header: 'Username', accessor: 'username', className: 'text-slate-500' },
        { 
            header: 'Role', 
            accessor: 'roleName',
            render: (item) => (
                <span className="px-2 py-1 bg-blue-50 text-emerald-700 rounded text-xs font-medium border border-blue-200">
                    {item.roleName}
                </span>
            )
        },
        { 
            header: 'Current Salary', 
            accessor: 'salary',
            render: (item) => item.salary ? <span className="text-emerald-700 font-medium">${item.salary.toLocaleString(undefined, { minimumFractionDigits: 2 })}</span> : <span className="text-slate-400">Not Set</span>
        },
        {
            header: 'Actions', accessor: 'actions', className: 'text-right w-40',
            render: (item) => (
                <div className="flex justify-end gap-3">
                    <button onClick={() => { setSelectedEmployee(item); setIsSlideOverOpen(true); }} className="text-emerald-600 hover:text-emerald-800 font-medium">
                        Manage
                    </button>
                    <button onClick={() => { setSelectedEmployee(item); setIsDeleteDialogOpen(true); }} className="text-red-600 hover:text-red-800 font-medium">
                        Revoke
                    </button>
                </div>
            )
        }
    ], []);

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-slate-800">Employee Directory</h2>
                <div className="flex justify-between items-center border-b border-slate-200 pb-4">
                    <button 
                        onClick={() => setIsInviteModalOpen(true)}
                        className="bg-emerald-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-emerald-700"
                    >
                        + Invite Employee
                    </button>

                    {isInviteModalOpen && (
                        <InviteEmployeeModal 
                            onClose={() => setIsInviteModalOpen(false)} 
                            onSuccess={() => { 
                                setIsInviteModalOpen(false); 
                                queryClient.invalidateQueries({ queryKey: ['employees'] });
                            }} 
                        />
                    )}
                </div>
                <div className="flex bg-slate-100 p-1 rounded-lg">
                    <button 
                        onClick={() => setActiveView('Directory')}
                        className={`px-4 py-2 text-sm font-medium rounded-md transition-all ${activeView === 'Directory' ? 'bg-white shadow-sm text-emerald-600' : 'text-slate-500 hover:text-slate-700'}`}
                    >
                        Employee Directory
                    </button>
                    <button 
                        onClick={() => setActiveView('Roles')}
                        className={`px-4 py-2 text-sm font-medium rounded-md transition-all ${activeView === 'Roles' ? 'bg-white shadow-sm text-emerald-600' : 'text-slate-500 hover:text-slate-700'}`}
                    >
                        Roles & Permissions
                    </button>
                </div>
            </div>

            {activeView === 'Directory' ? (
                <>
                    {/* HR Statistics Row */}
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                            <p className="text-sm font-medium text-slate-500">Total Active Employees</p>
                            <p className="text-2xl font-bold text-slate-800 mt-1">{totalCount}</p>
                        </div>
                        <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                            <p className="text-sm font-medium text-slate-500">Pending Leave Approvals</p>
                            <div className="flex items-center gap-2 mt-1">
                                <p className={`text-2xl font-bold ${pendingLeavesCount > 0 ? 'text-amber-600' : 'text-slate-800'}`}>
                                    {pendingLeavesCount}
                                </p>
                                {pendingLeavesCount > 0 && (
                                    <span className="flex h-3 w-3 relative">
                                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-amber-400 opacity-75"></span>
                                        <span className="relative inline-flex rounded-full h-3 w-3 bg-amber-500"></span>
                                    </span>
                                )}
                            </div>
                        </div>
                        <div className="bg-emerald-700 p-4 rounded-lg shadow-sm border border-emerald-600 text-white">
                            <p className="text-sm font-medium text-slate-300">Quick Actions</p>
                            <button 
                                onClick={() => navigate('/pending-leaves')}
                            className="mt-2 text-sm font-medium text-white-300 hover:text-emerald-200">
                                View Pending Leaves →
                            </button>
                        </div>
                    </div>

                    {/* Filters */}
                    <div className="flex gap-4 max-w-2xl">
                        <div className="flex-1 bg-white p-1 rounded-md shadow-sm border border-slate-200">
                            <input 
                                type="text" 
                                placeholder="Search employees..." 
                                value={searchTerm} 
                                onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }} 
                                className="w-full px-3 py-2 outline-none text-sm bg-transparent" 
                            />
                        </div>
                        <select 
                            className="bg-white px-3 py-2 rounded-md shadow-sm border border-slate-200 text-sm outline-none" 
                            value={roleFilter} 
                            onChange={(e) => { setRoleFilter(e.target.value); setPage(1); }}
                        >
                            <option value="All">All Roles</option>
                            {roles.map((role) => (
                                <option key={role.roleId} value={role.name}>
                                    {role.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    <DataTable 
                        data={employees} 
                        columns={columns} 
                        isLoading={isLoadingEmployees} 
                        page={page} 
                        totalPages={totalPages} 
                        totalCount={totalCount} 
                        onPageChange={setPage} 
                    />
                    
                    <SlideOver isOpen={isSlideOverOpen} onClose={() => setIsSlideOverOpen(false)} title={`Manage ${selectedEmployee?.fullName || 'Employee'}`}>
                        {selectedEmployee && (
                            <EmployeeManagerForm 
                                employee={selectedEmployee} 
                                onSuccess={() => { 
                                    setIsSlideOverOpen(false); 
                                    queryClient.invalidateQueries({ queryKey: ['employees'] });
                                }} 
                                onCancel={() => setIsSlideOverOpen(false)} 
                            />
                        )}
                    </SlideOver>

                    <ConfirmDialog 
                        isOpen={isDeleteDialogOpen} 
                        title="Revoke Access" 
                        message={`Are you sure you want to permanently revoke access for "${selectedEmployee?.fullName}"?`} 
                        onConfirm={handleConfirmDelete} 
                        onCancel={() => { setIsDeleteDialogOpen(false); setSelectedEmployee(null); }} 
                        isProcessing={deleteEmployeeMutation.isPending} 
                    />
                </>
            ) : (
                <RolesManager />
            )}
        </div>
    );
}