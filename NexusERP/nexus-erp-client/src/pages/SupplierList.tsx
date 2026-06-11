import { useEffect, useState, useMemo } from 'react';
import { supplierService } from '../api/supplierService';
import { type SupplierResponse } from '../types/supplier';
import { DataTable, type ColumnDef } from '../components/Ui/DataTable'
import { SlideOver } from '../components/Ui/SlideOver';
import { SupplierForm } from '../components/forms/SupplierForm';
import { ConfirmDialog } from '../components/Ui/ConfirmDialog';
import type { SupplierFormData } from '../schemas/supplierSchema';

export default function SupplierList() {
    const [suppliers, setSuppliers] = useState<SupplierResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [searchTerm, setSearchTerm] = useState('');

    const [isSlideOverOpen, setIsSlideOverOpen] = useState(false);
    const [selectedSupplier, setSelectedSupplier] = useState<SupplierResponse | null>(null);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);

    useEffect(() => {
        const controller = new AbortController();
        const timer = setTimeout(() => {
            loadSuppliers(controller.signal);
        }, 300);
        return () => {
            clearTimeout(timer);
            controller.abort();
        };
    }, [page, searchTerm]);

    const loadSuppliers = async (signal: AbortSignal) => {
        try {
            setError('');
            setIsLoading(true);
            const data = await supplierService.getSuppliers(page, 10, searchTerm, signal);
            setSuppliers(data.items);
            setTotalPages(data.totalPages);
            setTotalCount(data.totalCount);
        } catch (err: any) {
            if (err.name === 'CanceledError' || err.message === 'canceled') return;
            console.error(err);
            setError('Failed to load Suppliers.');
        } finally {
            setIsLoading(false);
        }
    };

    const handleFormSubmit = async (formData: SupplierFormData) => {
        try {
            const payload = {
                ...formData,
                supplierId: selectedSupplier?.supplierId || 0
            };
            await supplierService.saveSupplier(payload);
            setIsSlideOverOpen(false);
            const controller = new AbortController();
            loadSuppliers(controller.signal);
        } catch (err) {
            console.error("Failed to save", err);
        }
    };

    const handleConfirmDelete = async () => {
        if (!selectedSupplier) return;
        try {
            setIsDeleting(true);
            await supplierService.deleteSupplier(selectedSupplier.supplierId);
            setIsDeleteDialogOpen(false);
            setSelectedSupplier(null);
            const controller = new AbortController();
            loadSuppliers(controller.signal);
        } catch (err) {
            console.error("Failed to delete", err);
        } finally {
            setIsDeleting(false);
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
                    <button 
                        onClick={() => { setSelectedSupplier(item); setIsSlideOverOpen(true); }}
                        className="text-emerald-600 hover:text-emerald-800 font-medium mr-3 transition-colors">
                        Edit
                    </button>
                    <button 
                        onClick={() => { setSelectedSupplier(item); setIsDeleteDialogOpen(true); }}
                        className="text-red-600 hover:text-red-800 font-medium transition-colors">
                        Delete
                    </button>
                </div>
            )
        }
    ], []);

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-slate-800">Suppliers</h2>
                <button 
                    onClick={() => { setSelectedSupplier(null); setIsSlideOverOpen(true); }}
                    className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-md text-sm font-medium transition-colors shadow-sm">
                    + Add Supplier
                </button>
            </div>

            <div className="flex bg-white p-1 rounded-md shadow-sm border border-slate-200 max-w-md">
                <input 
                    type="text" 
                    placeholder="Search by company, contact, or email..." 
                    value={searchTerm}
                    onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
                    className="w-full px-3 py-2 outline-none text-sm bg-transparent"
                />
            </div>

            {error && <div className="p-3 bg-red-50 text-red-600 border border-red-200 rounded text-sm">{error}</div>}

            <DataTable 
                data={suppliers}
                columns={columns}
                isLoading={isLoading}
                page={page}
                totalPages={totalPages}
                totalCount={totalCount}
                onPageChange={setPage}
            />

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

            <ConfirmDialog
                isOpen={isDeleteDialogOpen}
                title="Delete Supplier"
                message={`Are you sure you want to delete "${selectedSupplier?.companyName}"?`}
                onConfirm={handleConfirmDelete}
                onCancel={() => { setIsDeleteDialogOpen(false); setSelectedSupplier(null); }}
                isProcessing={isDeleting}
            />
        </div>
    );
}