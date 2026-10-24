import { useUrlNumber, useUrlString, useUrlQuery } from '../hooks/useUrlState';
import { useAuth } from '../context/AuthContext';
import { Permissions } from '../constants/permissions';
import { useMemo } from 'react';
import { type SupplierResponse } from '../types/supplier';
import { DataTable, type ColumnDef } from '../components/Ui/DataTable';
import { SlideOver } from '../components/Ui/SlideOver';
import { SupplierForm } from '../components/forms/SupplierForm';
import { ConfirmDialog } from '../components/Ui/ConfirmDialog';
import type { SupplierFormData } from '../schemas/supplierSchema';
import { useSuppliersQuery, useSaveSupplierMutation, useDeleteSupplierMutation } from '../hooks/queries/useSupplierQueries';

export default function SupplierList() {
    const { hasPermission } = useAuth();
    const canCreate = hasPermission(Permissions.CreateSuppliers);
    const canEdit = hasPermission(Permissions.EditSuppliers);
    const canDelete = hasPermission(Permissions.DeleteSuppliers);
    const { patch } = useUrlQuery();
    const [page, setPage] = useUrlNumber('page');
    const [searchTerm] = useUrlString('search');

    const [dialog, setDialog] = useUrlString('dialog');
    const itemId = Number(dialog.split(':')[1]) || 0;
    const setIsSlideOverOpen = (open: boolean) => setDialog(open ? 'create' : '');
    const setIsDeleteDialogOpen = (_open: boolean) => setDialog('');

    const { data, isLoading, isError } = useSuppliersQuery(page, 10, searchTerm);
    const saveMutation = useSaveSupplierMutation();
    const deleteMutation = useDeleteSupplierMutation();

    const suppliers = data?.items || [];
    const selectedSupplier = (data?.items ?? []).find(item => item.supplierId === itemId) ?? null;
    const isSlideOverOpen = (dialog === 'create' && canCreate) || (dialog.startsWith('edit:') && canEdit && !!selectedSupplier);
    const isDeleteDialogOpen = dialog.startsWith('delete:') && canDelete && !!selectedSupplier;
    const totalPages = data?.totalPages || 1;
    const totalCount = data?.totalCount || 0;

    const handleFormSubmit = async (formData: SupplierFormData) => {
        try {
            await saveMutation.mutateAsync({
                ...formData,
                supplierId: selectedSupplier?.supplierId || 0
            });
            setIsSlideOverOpen(false);
        } catch (err) {
            console.error(err);
        }
    };

    const handleConfirmDelete = async () => {
        if (!selectedSupplier) return;
        try {
            await deleteMutation.mutateAsync(selectedSupplier.supplierId);
            setIsDeleteDialogOpen(false);
        } catch (err) {
            console.error(err);
        }
    };

    const columns = useMemo<ColumnDef<SupplierResponse>[]>(() => [
        { header: 'ID', accessor: 'supplierId', className: 'w-16' },
        { header: 'Company Name', accessor: 'companyName', className: 'font-bold text-slate-800' },
        { header: 'Contact', accessor: 'contactName' },
        { header: 'Email', accessor: 'email' },
        { header: 'Phone', accessor: 'phone' },
        {
            header: 'Actions',
            accessor: 'actions',
            className: 'text-center w-32',
            render: (item) => (
                <div>
                    {canEdit && <button 
                        onClick={() => { setDialog(`edit:${item.supplierId}`); }}
                        className="text-emerald-600 hover:text-emerald-800 font-medium mr-3 transition-colors">
                        Edit
                    </button>}
                    {canDelete && <button 
                        onClick={() => { setDialog(`delete:${item.supplierId}`); }}
                        className="text-red-600 hover:text-red-800 font-medium transition-colors">
                        Delete
                    </button>}
                </div>
            )
        }
    ], [canEdit, canDelete]);

    return (
        <div className="space-y-6">
            {/* Header Area */}
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-slate-800">Suppliers</h2>
                {canCreate && <button 
                    onClick={() => { setIsSlideOverOpen(true); }}
                    className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-md text-sm font-medium transition-colors shadow-sm">
                    + Add Supplier
                </button>}
            </div>

            {/* Filter Area */}
            <div className="flex bg-white p-1 rounded-md shadow-sm border border-slate-200 max-w-md">
                <input 
                    type="text" 
                    placeholder="Search by company, contact, or email..." 
                    value={searchTerm}
                    onChange={(e) => { patch({ search: e.target.value, page: null }); }}
                    className="w-full px-3 py-2 outline-none text-sm bg-transparent"
                />
            </div>

            {/* Error Display */}
            {isError && (
                <div className="p-3 bg-red-50 text-red-600 border border-red-200 rounded text-sm">
                    Failed to load Suppliers.
                </div>
            )}

            {/* Data Table */}
            <DataTable 
                data={suppliers}
                columns={columns}
                isLoading={isLoading}
                page={page}
                totalPages={totalPages}
                totalCount={totalCount}
                onPageChange={setPage}
            />

            {/* Form Modal */}
            <SlideOver
                isOpen={isSlideOverOpen}
                onClose={() => setIsSlideOverOpen(false)}
                title={selectedSupplier ? `Edit ${selectedSupplier.companyName}` : "Register Supplier"}
            >
                <SupplierForm
                    initialData={selectedSupplier}
                    onSubmit={handleFormSubmit}
                    onCancel={() => setIsSlideOverOpen(false)}
                />
            </SlideOver>

            {/* Delete Confirmation */}
            <ConfirmDialog
                isOpen={isDeleteDialogOpen}
                title="Delete Supplier"
                message={`Are you sure you want to delete "${selectedSupplier?.companyName}"?`}
                onConfirm={handleConfirmDelete}
                onCancel={() => { setIsDeleteDialogOpen(false); }}
                isProcessing={deleteMutation.isPending}
            />
        </div>
    );
}
