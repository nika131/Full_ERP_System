import { useUrlNumber, useUrlString, useUrlQuery } from '../hooks/useUrlState';
import { useAuth } from '../context/AuthContext';
import { Permissions } from '../constants/permissions';
import { useMemo } from "react";
import type { CategoryResponse } from "../types/category";
import type { CategoryFormData } from "../schemas/categorySchema";
import { DataTable, type ColumnDef } from "../components/Ui/DataTable";
import { SlideOver } from "../components/Ui/SlideOver";
import { ConfirmDialog } from "../components/Ui/ConfirmDialog";
import { CategoryForm } from "../components/forms/CategoryForm";
import { useCategoriesQuery, useSaveCategoryMutation, useDeleteCategoryMutation } from "../hooks/queries/useCategoryQueries";

export default function CategoryList() {
    const { hasPermission } = useAuth();
    const canCreate = hasPermission(Permissions.CreateCategories);
    const canEdit = hasPermission(Permissions.EditCategories);
    const canDelete = hasPermission(Permissions.DeleteCategories);
    const { patch } = useUrlQuery();
    const [page, setPage] = useUrlNumber('page');
    const [searchTerm] = useUrlString('search');

    const [dialog, setDialog] = useUrlString('dialog');
    const itemId = Number(dialog.split(':')[1]) || 0;
    const setIsSlideOverOpen = (open: boolean) => setDialog(open ? 'create' : '');
    const setIsDeleteDialogOpen = (_open: boolean) => setDialog('');

    const { data, isLoading } = useCategoriesQuery(page, 10, searchTerm);
    const saveMutation = useSaveCategoryMutation();
    const deleteMutation = useDeleteCategoryMutation();

    const categories = data?.items || [];
    const selectedCategory = (data?.items ?? []).find(item => item.categoryId === itemId) ?? null;
    const isSlideOverOpen = (dialog === 'create' && canCreate) || (dialog.startsWith('edit:') && canEdit && !!selectedCategory);
    const isDeleteDialogOpen = dialog.startsWith('delete:') && canDelete && !!selectedCategory;
    const totalPages = data?.totalPages || 1;
    const totalCount = data?.totalCount || 0;

    const handleFormSubmit = async (formData: CategoryFormData) => {
        try {
            await saveMutation.mutateAsync({ 
                ...formData, 
                categoryId: selectedCategory?.categoryId || 0 
            });
            setIsSlideOverOpen(false);
        } catch (err) { 
            console.error(err); 
        }
    };

    const handleConfirmDelete = async () => {
        if (!selectedCategory) return;
        try {
            await deleteMutation.mutateAsync(selectedCategory.categoryId);
            setIsDeleteDialogOpen(false);
        } catch (err) {
            console.error(err);
        }
    };

    const columns = useMemo<ColumnDef<CategoryResponse>[]>(() => [
        { header: 'ID', accessor: 'categoryId', className: 'w-16' },
        { header: 'Category Name', accessor: 'name', className: 'font-bold text-slate-800' },
        {
            header: 'Actions', accessor: 'actions', className: 'text-center w-32',
            render: (item) => (
                <div>
                    {canEdit && <button onClick={() => { setDialog(`edit:${item.categoryId}`); }} className="text-emerald-600 hover:text-emerald-800 font-medium mr-3">Edit</button>}
                    {canDelete && <button onClick={() => { setDialog(`delete:${item.categoryId}`); }} className="text-red-600 hover:text-red-800 font-medium">Delete</button>}
                </div>
            )
        }
    ], [canEdit, canDelete]);

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-slate-800">Categories</h2>
                {canCreate && <button onClick={() => { setIsSlideOverOpen(true); }} className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-md text-sm font-medium">
                    + Add Category
                </button>}
            </div>

            {/* Filters */}
            <div className="flex bg-white p-1 rounded-md shadow-sm border border-slate-200 max-w-md">
                <input type="text" placeholder="Search categories..." value={searchTerm} onChange={(e) => { patch({ search: e.target.value, page: null }); }} className="w-full px-3 py-2 outline-none text-sm bg-transparent" />
            </div>

            {/* Data Table */}
            <DataTable data={categories} columns={columns} isLoading={isLoading} page={page} totalPages={totalPages} totalCount={totalCount} onPageChange={setPage} />
            
            {/* Modals */}
            <SlideOver isOpen={isSlideOverOpen} onClose={() => setIsSlideOverOpen(false)} title={selectedCategory ? `Edit Category` : "New Category"}>
                <CategoryForm initialData={selectedCategory} onSubmit={handleFormSubmit} onCancel={() => setIsSlideOverOpen(false)} />
            </SlideOver>

            <ConfirmDialog isOpen={isDeleteDialogOpen} title="Delete Category" message={`Delete "${selectedCategory?.name}"?`} onConfirm={handleConfirmDelete} onCancel={() => { setIsDeleteDialogOpen(false); }} isProcessing={deleteMutation.isPending} />
        </div>
    );
}
