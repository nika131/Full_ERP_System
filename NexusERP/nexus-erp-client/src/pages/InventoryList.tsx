import { useEffect, useMemo, useState } from 'react';
import { type Product } from '../types/product';
import { DataTable, type ColumnDef } from '../components/Ui/DataTable';
import type { ProductFormData } from '../schemas/productSchema';
import { SlideOver } from '../components/Ui/SlideOver';
import { ProductForm } from '../components/forms/ProductForm';
import { ConfirmDialog } from '../components/Ui/ConfirmDialog';
import type { StockFormData } from '../schemas/stockSchema';
import { StockManagementForm } from '../components/forms/StockManagementForm';
import { useProductsQuery, useSaveProductMutation, useDeleteProductMutation, useTransactionMutation } from '../hooks/queries/useInventoryQueries';
import { useLookupCategoriesQuery } from '../hooks/queries/useCategoryQueries';
import { useSupplierLookupQuery } from '../hooks/queries/useSupplierQueries';
import { AlertTriangle, Plus, Settings } from 'lucide-react';
import { GlobalSettingsModal } from '../components/Ui/GlobalSettingsModal';
import apiClient from '../api/apiClient';

export default function InventoryList() {
    const [page, setPage] = useState(1);
    const [searchTerm, setSearchTerm] = useState('');
    const [categoryFilter, setCategoryFilter] = useState('')
    const [supplierFilter, setSupplierFilter] = useState('')
    const [lowStockOnly, setLowStockOnly] = useState(false);
    const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);

    const [isSildeOverOpen, setIsSlideOverOpen] = useState(false);
    const [selectedProduct, setSelectedProduct]= useState<Product | null>(null);

    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

    const [isStockSlideOverOpen, setIsStockSlideOverOpen] = useState(false);
    const [selectedStockProduct, setSelectedStockProduct] = useState<Product | null>(null);

    const [globalThreshold, setGlobalThreshold] = useState(5);

    const { data: productsData, isLoading, isError } = useProductsQuery(
        page, 
        10, 
        searchTerm, 
        categoryFilter, 
        supplierFilter,
        lowStockOnly
    );
    const saveProductMutation = useSaveProductMutation();
    const deleteProductMutation = useDeleteProductMutation();
    const transactionMutation = useTransactionMutation();

    const { data: categories = [] } = useLookupCategoriesQuery();
    const { data: suppliers = [] } = useSupplierLookupQuery();

    const products = productsData?.items || [];
    const totalPages = productsData?.totalPages || 1;
    const totalCount = productsData?.totalCount || 0;

    const handleAddClick = () => {
        setSelectedProduct(null);
        setIsSlideOverOpen(true);
    }

    const handleEditClick = (product: Product) => {
        setSelectedProduct(product);
        setIsSlideOverOpen(true);
    };

    const handleFormSubmit = async (FormData: ProductFormData) => {
        try {
            const payload = {
                ...FormData,
                productId: selectedProduct?.productId
            };
            await saveProductMutation.mutateAsync(payload);
            setIsSlideOverOpen(false);
        } catch (err) {
            console.error("Failed to save", err);
        }
    }

    const handleDeleteClick = (product: Product) => {
        setSelectedProduct(product);
        setIsDeleteDialogOpen(true);
    }

    const handleConfirmDelete = async () => {
        if (!selectedProduct) return;
        try {
            await deleteProductMutation.mutateAsync(selectedProduct.productId);
            setIsDeleteDialogOpen(false);
            setSelectedProduct(null);
        } catch (err) {
            console.error("Failed to delete product", err);
        }
    }

    const handleStockSubmit = async (formData: StockFormData) => {
        if (!selectedStockProduct) return;
        try {
            await transactionMutation.mutateAsync({
                productId: selectedStockProduct.productId,
                supplierId: selectedStockProduct.supplierId || null, 
                productPrice: selectedStockProduct.price,
                costPrice: selectedStockProduct.costPrice,
                transactionType: formData.transactionType,
                quantity: formData.quantity 
            });
            setIsStockSlideOverOpen(false);
        } catch (err) {
            console.error("Transaction failed", err);
        }
    };

    useEffect(() => {
        apiClient.get('/settings/GlobalLowStockThreshold')
            .then(res => setGlobalThreshold(Number(res.data.value)))
            .catch(err => console.error("Failed to load global threshold", err));
    }, []);

    const columns = useMemo<ColumnDef<Product>[]>(() => [
        { header: 'ID', accessor: 'productId', className: 'w-16' },
        { header: 'Product Name', accessor: 'name', className: 'font-medium' },
        { 
        header: 'Category', 
        accessor: 'categoryName',
        render: (item) => (
            <span className="bg-slate-100 text-slate-700 px-2 py-1 rounded text-xs border border-slate-200">
            {item.categoryName}
            </span>
        )
        },
        {
            header: 'Supplier',
            accessor: 'companyName',
            render: (item) => (
                <span className="bg-slate-100 text-slate-700 px-2 py-1 rounded text-xs border border-slate-200">
                {item.companyName}
                </span>
            )
        },
        { 
            header: 'Quantity', 
            accessor: 'quantity', 
            className: 'text-right',
            render: (item) => {
                const activeThreshold = item.lowStockThreshold ?? globalThreshold;
                const isLowStock = item.quantity <= activeThreshold;

                return (
                    <span className={`font-medium ${isLowStock ? 'text-red-600' : 'text-emerald-600'}`}>
                        {item.quantity}
                    </span>
                )
            }
        },
        { 
        header: 'Price', 
        accessor: 'price', 
        className: 'text-right',
        render: (item) => `$${item.price.toFixed(2)}`
        },
        {
        header: 'Actions', 
        accessor: 'actions', 
        className: 'text-right w-48',
        render: (item) => (
            <div className="flex justify-end space-x-3">
                <button 
                    onClick={() => { setSelectedStockProduct(item); setIsStockSlideOverOpen(true); }} 
                    className="text-blue-600 hover:text-blue-800 font-medium text-sm">
                    Manage Stock
                </button>
                <button onClick={() => handleEditClick(item)} className="text-emerald-600 hover:text-emerald-800 font-medium text-sm">Edit</button>
                <button onClick={() => handleDeleteClick(item)} className="text-red-600 hover:text-red-800 font-medium text-sm">Delete</button>
            </div>
        )
        }
    ], [globalThreshold]);

    return (
        <div className="space-y-4">
            {/* Header */}
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-slate-800">Inventory</h2>
            </div>

            {/* TWO-TIER ACTION BAR */}
            <div className="bg-white p-3 rounded-lg border border-slate-200 flex flex-col gap-3">
                
                {/* TOP TIER: Search & Primary Actions */}
                <div className="flex flex-col sm:flex-row justify-between gap-3">
                    
                    {/* Search */}
                    <input 
                        type="text" 
                        placeholder="Search name or ID..." 
                        value={searchTerm}
                        onChange={(e) => {
                            setSearchTerm(e.target.value);
                            setPage(1);
                        }}
                        className="w-full sm:flex-1 border border-slate-300 rounded p-2 text-sm outline-none"
                    />

                    {/* Action Buttons*/}
                    <div className="flex w-full sm:w-auto gap-2 shrink-0">
                        
                        <button
                            onClick={() => {
                                setLowStockOnly(!lowStockOnly);
                                setPage(1);
                            }}
                            className={`flex-1 sm:flex-none flex items-center justify-center gap-2 px-3 py-2 rounded text-sm font-semibold transition-colors border ${
                                lowStockOnly 
                                ? 'bg-amber-100 border-amber-300 text-amber-800' 
                                : 'bg-white border-slate-300 text-slate-600 hover:bg-slate-50'
                            }`}
                            title="Show only low stock products"
                        >
                            <AlertTriangle size={16} />
                            <span className="hidden lg:inline whitespace-nowrap">Low Stock</span>
                        </button>

                        <button
                            onClick={() => setIsSettingsModalOpen(true)}
                            className="w-12 sm:w-auto p-2 flex-none rounded border border-slate-300 bg-white text-slate-600 hover:bg-slate-50 flex items-center justify-center"
                            title="System Settings"
                        >
                            <Settings size={18} />
                        </button>

                        <button 
                            onClick={handleAddClick}
                            className="flex-1 sm:flex-none bg-emerald-600 text-white px-3 py-2 rounded flex items-center justify-center gap-1 text-sm font-semibold hover:bg-emerald-700 whitespace-nowrap"
                        >
                            <Plus size={16} />
                            <span className="hidden lg:inline">New Product</span>
                        </button>
                    </div>
                </div>

                {/* BOTTOM TIER: Dropdowns */}
                <div className="flex flex-row gap-3 w-full">
                    <select
                        className="flex-1 w-1/2 bg-white px-3 py-2 rounded border border-slate-300 text-sm outline-none" 
                        value={categoryFilter}
                        onChange={(e) => { setCategoryFilter(e.target.value); setPage(1); }}
                    >
                        <option value="">All Categories</option>
                        {categories.map((category) => (
                            <option key={category.categoryId} value={category.name}>{category.name}</option>
                        ))}
                    </select>

                    <select
                        className="flex-1 w-1/2 bg-white px-3 py-2 rounded border border-slate-300 text-sm outline-none" 
                        value={supplierFilter}
                        onChange={(e) => { setSupplierFilter(e.target.value); setPage(1); }}
                    >
                        <option value="">All Suppliers</option>
                        {suppliers.map((supplier) => (
                            <option key={supplier.supplierId} value={supplier.companyName}>{supplier.companyName}</option>
                        ))}
                    </select>
                </div>
            </div>
            
            {isError && (
                <div className="p-3 bg-red-50 text-red-600 border border-red-200 rounded text-sm">
                Failed to load Inventory Data. Please try again later.
                </div>
            )}

            {/* Data Table */}
            <DataTable 
                data={products}
                columns={columns}
                isLoading={isLoading}
                page={page}
                totalPages={totalPages}
                totalCount={totalCount}
                onPageChange={(newPage) => setPage(newPage)}
            />

            {/* Edit/Create Modal */}
            <SlideOver
                isOpen={isSildeOverOpen}
                onClose={() => setIsSlideOverOpen(false)}
                title={selectedProduct ? `Edit ${selectedProduct.name}` : "Create New Product"}
            >
                <ProductForm
                    initialData={selectedProduct}
                    onSubmit={handleFormSubmit}
                    onCancel={() => setIsSlideOverOpen(false)}
                />
            </SlideOver>

            {/* Stock Adjustment Modal */}
            <SlideOver 
                isOpen={isStockSlideOverOpen} 
                onClose={() => setIsStockSlideOverOpen(false)} 
                title="Inventory Adjustment"
            >
                {selectedStockProduct && (
                    <StockManagementForm 
                        product={selectedStockProduct} 
                        onSubmit={handleStockSubmit} 
                        onCancel={() => setIsStockSlideOverOpen(false)} 
                    />
                )}
            </SlideOver>

            {/* Delete Dialog */}
            <ConfirmDialog
                isOpen={isDeleteDialogOpen}
                title="Delete Product"
                message={`Are you sure you want to delete "${selectedProduct?.name}"? This action cannot be undone.`}
                onConfirm={handleConfirmDelete}
                onCancel={() => {
                setIsDeleteDialogOpen(false);
                setSelectedProduct(null);
                }}
                isProcessing={deleteProductMutation.isPending}
            />

            {/* Global Settings Modal (Hidden by default) */}
            {isSettingsModalOpen && (
                <GlobalSettingsModal onClose={() => setIsSettingsModalOpen(false)} />
            )}
            
        </div>
    );
}