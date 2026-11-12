import { useAuth } from '../context/AuthContext';
import { Permissions } from '../constants/permissions';
import { useMemo, useState } from "react";
import { DataTable, type ColumnDef } from "../components/Ui/DataTable";
import { SlideOver } from "../components/Ui/SlideOver";
import { ConfirmDialog } from "../components/Ui/ConfirmDialog";
import type { EmployeeResponse } from "../types/employee";
import { EmployeeManagerForm } from "../components/hr/EmployeeManagerForm";
import { RolesManager } from "../components/hr/RolesManager";
import PendingLeaves from './PendingLeaves';
import { useUrlNumber, useUrlString, useUrlQuery } from '../hooks/useUrlState';
import { InviteEmployeeModal } from "../components/hr/InviteEmployeeModal";
import { useQueryClient } from "@tanstack/react-query";
import { useEmployeesQuery, useRoleLookupQuery, usePendingLeavQuery, useDeleteEmployeeMutation } from "../hooks/queries/useHrQueries";
import { apiErrorMessage } from '../api/authService';
import { companyAdminService } from '../api/companyAdminService';
import { CopyCodeButton } from '../components/Ui/CopyCodeButton';
import { Link } from "react-router-dom";
import { RefreshCw, X } from 'lucide-react';

export default function EmployeeList() {
    const { hasPermission } = useAuth();
    const canRoles = hasPermission(Permissions.ManageRoles);
    const canCreate = hasPermission(Permissions.CreateUsers);
    const canManageJoinCodes = hasPermission(Permissions.ManageJoinCodes);
    const canReviewJoinRequests = hasPermission(Permissions.ManageJoinRequests);
    const canEdit = hasPermission(Permissions.EditUsers);
    const canDelete = hasPermission(Permissions.DeleteUsers);
    const canManageAbsences = hasPermission(Permissions.ManageAbsences);
    const canViewSalary = hasPermission(Permissions.ViewSalary);
    const canManageSalary = hasPermission(Permissions.ManageSalary);
    const queryClient = useQueryClient();

    
    const [showEmployeeGuide, setShowEmployeeGuide] = useState(true);
    const [generatedCode, setGeneratedCode] = useState<{
        code: string;
        expiresAt: string;
    } | null>(null);
    const [generatingCode, setGeneratingCode] = useState(false);
    const [joinCodeError, setJoinCodeError] = useState('');

    const generateJoinCode = async () => {
        if (generatingCode || !canManageJoinCodes) return;

        if (!window.confirm(
            'Generate a new company join code? The previous code will immediately stop working.'
        )) return;

        setGeneratingCode(true);
        setJoinCodeError('');

        try {
            const result = await companyAdminService.rotateJoinCode();

            setGeneratedCode({
                code: result.joinCode,
                expiresAt: result.expiresAt,
            });
        } catch (err: unknown) {
            setJoinCodeError(
                apiErrorMessage(err, 'Could not generate a company join code.')
            );
        } finally {
            setGeneratingCode(false);
        }
    };


    const { patch } = useUrlQuery();
    const [page, setPage] = useUrlNumber('page', 1);
    const [searchTerm] = useUrlString('search');
    const [roleFilter] = useUrlString('role', 'All');
    const [viewParam, setViewParam] = useUrlString('tab', hasPermission(Permissions.ViewUsers) ? 'directory' : hasPermission(Permissions.ManageRoles) ? 'roles' : 'leaves');
    const activeView: 'Directory' | 'Roles' | 'Leaves' = viewParam === 'roles' && canRoles ? 'Roles' : viewParam === 'leaves' && canManageAbsences ? 'Leaves' : hasPermission(Permissions.ViewUsers) ? 'Directory' : canRoles ? 'Roles' : 'Leaves';
    const setActiveView = (value: 'Directory' | 'Roles' | 'Leaves') => setViewParam(value.toLowerCase());
    
    const [dialog, setDialog] = useUrlString('dialog');
    const isInviteModalOpen = dialog === 'invite' && canCreate;
    const setIsInviteModalOpen = (open: boolean) => setDialog(open ? 'invite' : '');

    const { data: employeesData, isLoading: isLoadingEmployees } = useEmployeesQuery(page, 10, searchTerm, roleFilter, hasPermission(Permissions.ViewUsers));
    const { data: roles = [] } = useRoleLookupQuery();
    const { data: pendingLeaves = [] } = usePendingLeavQuery(canManageAbsences);
    const deleteEmployeeMutation = useDeleteEmployeeMutation();

    const employees = employeesData?.items || [];
    const [dialogAction, rawId] = dialog.split(':');
    const requestedEmployeeId = Number(rawId);
    const selectedEmployee = employees.find(e => e.userId === requestedEmployeeId) ?? null;
    const isSlideOverOpen = dialogAction === 'manage' && !!selectedEmployee && (canEdit || canViewSalary || canManageSalary);
    const isDeleteDialogOpen = dialogAction === 'delete' && !!selectedEmployee && canDelete;
    const totalPages = employeesData?.totalPages || 1;
    const totalCount = employeesData?.totalCount || 0;
    const pendingLeavesCount = pendingLeaves.length;


    const handleConfirmDelete = async () => {
        if (!selectedEmployee) return;
        try {
            await deleteEmployeeMutation.mutateAsync(selectedEmployee.userId);
            setDialog('');
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
            header: canViewSalary ? 'Current Salary' : '', 
            accessor: 'salary',
            render: (item) => !canViewSalary ? null : item.salary ? <span className="text-emerald-700 font-medium">${item.salary.toLocaleString(undefined, { minimumFractionDigits: 2 })}</span> : <span className="text-slate-400">Not Set</span>
        },
        {
            header: 'Actions', accessor: 'actions', className: 'text-right w-40',
            render: (item) => (
                <div className="flex justify-end gap-3">
                    {(canEdit || canViewSalary || canManageSalary) && <button onClick={() => { setDialog(`manage:${item.userId}`); }} className="text-emerald-600 hover:text-emerald-800 font-medium">
                        Manage
                    </button>}
                    {canDelete && <button onClick={() => { setDialog(`delete:${item.userId}`); }} className="text-red-600 hover:text-red-800 font-medium">
                        Revoke
                    </button>}
                </div>
            )
        }
    ], [canEdit, canDelete, canViewSalary, canManageSalary]);

    return (
        <div className="space-y-6">

            {/* HEADER */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 pb-4 border-b border-slate-200">
                <h2 className="text-2xl font-bold text-slate-800">Employee Directory</h2>
                
                <div className="flex bg-slate-100 p-1 rounded-lg w-full sm:w-auto">
                    {hasPermission(Permissions.ViewUsers) && <button
                        onClick={() => setActiveView('Directory')}
                        className={`flex-1 sm:flex-none px-4 py-2 text-sm font-medium rounded-md transition-all ${activeView === 'Directory' ? 'bg-white shadow-sm text-emerald-600' : 'text-slate-500 hover:text-slate-700'}`}
                    >
                        Employee Directory
                    </button>}
                    {canRoles && <button 
                        onClick={() => setActiveView('Roles')}
                        className={`flex-1 sm:flex-none px-4 py-2 text-sm font-medium rounded-md transition-all ${activeView === 'Roles' ? 'bg-white shadow-sm text-emerald-600' : 'text-slate-500 hover:text-slate-700'}`}
                    >
                        Roles & Permissions
                    </button>}
                    {canManageAbsences && <button
                        type="button"
                        onClick={() => setActiveView('Leaves')}
                        className={`flex-1 sm:flex-none px-4 py-2 text-sm font-medium rounded-md transition-all ${activeView === 'Leaves' ? 'bg-white shadow-sm text-emerald-600' : 'text-slate-500 hover:text-slate-700'}`}
                    >
                        Leave requests {pendingLeavesCount>0?`(${pendingLeavesCount})`:''}
                    </button>}
                </div>
            </div>

            {activeView === 'Directory' && hasPermission(Permissions.ViewUsers) ? (
                <>
                    
                {/* Employee onboarding and company access */}
                {(showEmployeeGuide || canManageJoinCodes || canReviewJoinRequests) && (
                    <section className="rounded-xl border border-slate-200 bg-white p-4 text-left shadow-sm sm:p-5">
                        <div className="flex items-start justify-between gap-3">
                            <div className="min-w-0">
                                <div className="text-sm font-bold text-slate-900">
                                    Employee access
                                </div>

                                {showEmployeeGuide && (
                                    <p className="mt-2 text-sm leading-6 text-slate-600">
                                        <strong>POS employees</strong> use a PIN on an activated
                                        terminal. <strong>Website employees</strong> register
                                        with their own email, request to join with a company
                                        code, and are approved by an administrator.
                                    </p>
                                )}
                            </div>

                            {showEmployeeGuide && (
                                <button
                                    type="button"
                                    onClick={() => setShowEmployeeGuide(false)}
                                    aria-label="Dismiss employee setup explanation"
                                    className="shrink-0 rounded-lg p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                                >
                                    <X size={17} aria-hidden="true" />
                                </button>
                            )}
                        </div>

                        {(canManageJoinCodes || canReviewJoinRequests) && (
                            <div className="mt-3 flex flex-wrap items-center gap-2">
                                {canManageJoinCodes && (
                                    <button
                                        type="button"
                                        disabled={generatingCode}
                                        onClick={() => void generateJoinCode()}
                                        className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-50"
                                    >
                                        <RefreshCw size={15} aria-hidden="true" />
                                        {generatingCode
                                            ? 'Generating...'
                                            : 'Generate company join code'}
                                    </button>
                                )}

                                <Link
                                    to="/company-admin"
                                    className="inline-flex items-center rounded-lg px-3 py-2 text-sm font-semibold text-emerald-700 hover:bg-emerald-50"
                                >
                                    {canReviewJoinRequests
                                        ? 'Review join requests'
                                        : 'Company access'}
                                </Link>
                            </div>
                        )}

                        {joinCodeError && (
                            <p
                                role="alert"
                                className="mt-3 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
                            >
                                {joinCodeError}
                            </p>
                        )}

                        {generatedCode && (
                            <div className="mt-4 rounded-xl border border-amber-200 bg-amber-50 p-3">
                                <div className="text-sm font-bold text-amber-950">
                                    Save this code now — it is shown only once
                                </div>

                                <p className="mt-1 text-xs leading-5 text-amber-900">
                                    Share it with employees who need website access.
                                    Generating another code will invalidate this one.
                                </p>

                                <div className="mt-3 flex flex-wrap items-center gap-2">
                                    <span className="max-w-full select-all break-all rounded-lg bg-white px-3 py-2 font-mono text-xs font-semibold text-slate-900 sm:text-sm">
                                        {generatedCode.code}
                                    </span>

                                    <CopyCodeButton code={generatedCode.code} />
                                </div>

                                <p className="mt-2 text-xs text-amber-800">
                                    Expires: {new Date(generatedCode.expiresAt).toLocaleString()}
                                </p>
                            </div>
                        )}
                    </section>
                )}

                    {/* HR Statistics Row */}
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                            <p className="text-sm font-medium text-slate-500">Total Active Employees</p>
                            <p className="text-2xl font-bold text-slate-800 mt-1">{totalCount}</p>
                        </div>
                        {canManageAbsences && <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                            <p className="text-sm font-medium text-slate-500">Pending Leave Approvals</p>
                            <div className="flex justify-center items-center gap-2 mt-2">
                                <p className={`text-2xl font-bold leading-none ${pendingLeavesCount > 0 ? 'text-amber-600' : 'text-slate-800'}`}>
                                    {pendingLeavesCount}
                                </p>
                                
                                {pendingLeavesCount > 0 && (
                                    <span className="flex h-3 w-3 relative">
                                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-amber-400 opacity-75"></span>
                                        <span className="relative inline-flex rounded-full h-3 w-3 bg-amber-500"></span>
                                    </span>
                                )}
                            </div>
                        </div>}
                        {canManageAbsences && <div className="bg-emerald-700 p-4 rounded-lg shadow-sm border border-emerald-600 text-white">
                            <p className="text-sm font-medium text-slate-300">Quick Actions</p>
                            <button 
                                onClick={() => setActiveView('Leaves')}
                            className="mt-2 text-sm font-medium text-white-300 hover:text-emerald-200">
                                View Pending Leaves →
                            </button>
                        </div>}
                    </div>

                    {/* Filters */}
                    <div className="flex flex-col md:flex-row gap-4 w-full md:items-center">
                        
                        <div className="w-full md:flex-1 md:max-w-md bg-white p-1 rounded-md shadow-sm border border-slate-200">
                            <input 
                                type="text" 
                                placeholder="Search employees..." 
                                value={searchTerm} 
                                onChange={(e) => { patch({ search: e.target.value, page: null }); }} 
                                className="w-full px-3 py-2 outline-none text-sm bg-transparent" 
                            />
                        </div>

                        <div className="flex flex-col min-[350px]:flex-row gap-4 w-full md:w-auto md:ml-auto">
                            <select 
                                className="flex-1 md:flex-none bg-white px-3 py-2 rounded-md shadow-sm border border-slate-200 text-sm outline-none" 
                                value={roleFilter} 
                                onChange={(e) => { patch({ role: e.target.value === 'All' ? null : e.target.value, page: null }); }}
                            >
                                <option value="All">All Roles</option>
                                {roles.map((role) => (
                                    <option key={role.roleId} value={role.name}>
                                        {role.name}
                                    </option>
                                ))}
                            </select>
                            
                            {canCreate && <button 
                                onClick={() => setIsInviteModalOpen(true)}
                                className="flex-1 md:flex-none whitespace-nowrap bg-emerald-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-emerald-700"
                            >
                                + Create Employee
                            </button>}
                        </div>

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
                    
                    <SlideOver isOpen={isSlideOverOpen} onClose={() => { setDialog(''); }} title={`Manage ${selectedEmployee?.fullName || 'Employee'}`}>
                        {selectedEmployee && (
                            <EmployeeManagerForm 
                                employee={selectedEmployee} 
                                onSuccess={() => { 
                                    setDialog(''); 
                                    queryClient.invalidateQueries({ queryKey: ['employees'] });
                                }} 
                                onCancel={() => { setDialog(''); }} 
                            />
                        )}
                    </SlideOver>

                    <ConfirmDialog 
                        isOpen={isDeleteDialogOpen} 
                        title="Revoke Access" 
                        message={`Are you sure you want to permanently revoke access for "${selectedEmployee?.fullName}"?`} 
                        onConfirm={handleConfirmDelete} 
                        onCancel={() => { setDialog(''); }} 
                        isProcessing={deleteEmployeeMutation.isPending} 
                    />
                </>
            ) : (
                activeView === 'Leaves' && canManageAbsences ? <PendingLeaves /> : canRoles ? <RolesManager /> : null
            )}

            
            <div className="flex justify-between items-center border-b border-slate-200 pb-4">
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
        </div>
    );
}
