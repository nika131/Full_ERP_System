import { useEffect, useMemo, useState } from "react";
import type { CategoryResponse } from "../types/category";
import { categoryService } from "../api/categoryService";
import type { CategoryFormData } from "../schemas/categorySchema";
import { DataTable, type ColumnDef } from "../components/Ui/DataTable";
import { SlideOver } from "../components/Ui/SlideOver";
import { ConfirmDialog } from "../components/Ui/ConfirmDialog";
import { CategoryForm } from "../components/forms/CategoryForm";


export default function CategoryList() {
    const [categroies, setCategories] = useState<CategoryResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true); 
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [searchTerm, setSearchTerm] = useState('');

    const [isSlideOverOpen, setIsSlideOverOpen] = useState(false);
    const [selectedCategory, setSelectedCategory] = useState<CategoryResponse | null>(null);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);

    useEffect(() => {
        const controller = new AbortController();
        const timer = setTimeout(() => {
            loadCategories(controller.signal);
        }, 300);
        return () => { clearTimeout(timer); controller.abort(); }
    }, [page, searchTerm]);

    const loadCategories = async (signal: AbortSignal) => {
        try {
            setIsLoading(true);
            const data = await categoryService.getCategories(page, 10, searchTerm, signal);
            setCategories(data.items);
            setTotalPages(data.totalPages);
            setTotalCount(data.totalCount);
        } catch (err: any) {
            if (err.name !== 'CanceledError') console.error(err);
        } finally {
            setIsLoading(false);
        }
    };

    const handleFormSubmit = async (FormData: CategoryFormData) => {
        try {
            await categoryService.saveCategory({ ...FormData, categoryId: selectedCategory?.categoryId || 0 });
            setIsSlideOverOpen(false);
            loadCategories(new AbortController().signal);
        } catch (err) { console.error(err); }
    };

    const handleConfirmDelete = async () => {
        if (!selectedCategory) return;
        try {
            setIsDeleting(true);
            await categoryService.deleteCategory(selectedCategory.categoryId);
            setIsDeleteDialogOpen(false);
            setSelectedCategory(null);
            loadCategories(new AbortController().signal);
        } catch (err) {
            console.error(err);
        } finally {
            setIsDeleting(false);
        }
    };

    const columns = useMemo<ColumnDef<CategoryResponse>[]>(() => [
        { header: 'ID', accessor: 'categoryId', className: 'w-16' },
        { header: 'Category Name', accessor: 'name', className: 'font-bold text-slate-800' },
        {
            header: 'Actions', accessor: 'actions', className: 'text-center w-32',
            render: (item) => (
                <div>
                    <button onClick={() => { setSelectedCategory(item); setIsSlideOverOpen(true); }} className="text-emerald-600 hover:text-emerald-800 font-medium mr-3">Edit</button>
                    <button onClick={() => { setSelectedCategory(item); setIsDeleteDialogOpen(true); }} className="text-red-600 hover:text-red-800 font-medium">Delete</button>
                </div>
            )
        }
    ], []);

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-slate-800">Categories</h2>
                <button onClick={() => { setSelectedCategory(null); setIsSlideOverOpen(true); }} className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-md text-sm font-medium">
                    + Add Category
                </button>
            </div>
            <div className="flex bg-white p-1 rounded-md shadow-sm border border-slate-200 max-w-md">
                <input type="text" placeholder="Search categories..." value={searchTerm} onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }} className="w-full px-3 py-2 outline-none text-sm bg-transparent" />
            </div>
            <DataTable data={categroies} columns={columns} isLoading={isLoading} page={page} totalPages={totalPages} totalCount={totalCount} onPageChange={setPage} />
            
            <SlideOver isOpen={isSlideOverOpen} onClose={() => setIsSlideOverOpen(false)} title={selectedCategory ? `Edit Category` : "New Category"}>
                <CategoryForm initialData={selectedCategory} onSubmit={handleFormSubmit} onCancel={() => setIsSlideOverOpen(false)} />
            </SlideOver>

            <ConfirmDialog isOpen={isDeleteDialogOpen} title="Delete Category" message={`Delete "${selectedCategory?.name}"?`} onConfirm={handleConfirmDelete} onCancel={() => { setIsDeleteDialogOpen(false); setSelectedCategory(null); }} isProcessing={isDeleting} />
        </div>
    );
}