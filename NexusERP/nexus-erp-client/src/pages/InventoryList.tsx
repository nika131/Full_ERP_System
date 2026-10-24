import { useAuth } from '../context/AuthContext';
import { Permissions } from '../constants/permissions';
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
import { useUrlNumber, useUrlString, useUrlQuery } from '../hooks/useUrlState';

export default function InventoryList() {
    const { hasPermission } = useAuth();
    const canCreate = hasPermission(Permissions.CreateProducts);
    const canEdit = hasPermission(Permissions.EditProducts);
    const canDelete = hasPermission(Permissions.DeleteProducts);
    const canStock = hasPermission(Permissions.AdjustStock) || hasPermission(Permissions.PerformInboundTransactions);
    const canManageSettings = hasPermission(Permissions.ManageSettings);
    const canViewSettings = hasPermission(Permissions.ViewSettings);
    const { patch } = useUrlQuery();
    const [page, setPage] = useUrlNumber('page');
    const [searchTerm] = useUrlString('search');
    const [categoryFilter] = useUrlString('category');
    const [supplierFilter] = useUrlString('supplier');
    const { params } = useUrlQuery();
    const lowStockOnly = params.get('lowStock') === '1';
    const [dialog, setDialog] = useUrlString('dialog');
    const itemId = Number(dialog.split(':')[1]) || 0;
    const closeDialog = () => setDialog('');
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
    const totalValue = productsData?.totalValue || 0;

    const selectedProduct = dialog.startsWith('edit:') || dialog.startsWith('delete:')
        ? products.find(product => product.productId === itemId) ?? null : null;
    const selectedStockProduct = dialog.startsWith('stock:')
        ? products.find(product => product.productId === itemId) ?? null : null;
    const isSlideOverOpen = dialog === 'create' && canCreate || dialog.startsWith('edit:') && canEdit && !!selectedProduct;
    const isDeleteDialogOpen = dialog.startsWith('delete:') && canDelete && !!selectedProduct;
    const isStockSlideOverOpen = dialog.startsWith('stock:') && canStock && !!selectedStockProduct;
    const isSettingsModalOpen = dialog === 'settings' && canManageSettings;

    const handleAddClick = () => setDialog('create');
    const handleEditClick = (product: Product) => setDialog(`edit:${product.productId}`);

    const handleFormSubmit = async (FormData: ProductFormData) => {
        try {
            const payload = {
                ...FormData,
                productId: selectedProduct?.productId
            };
            await saveProductMutation.mutateAsync(payload);
            closeDialog();
        } catch (err) {
            console.error("Failed to save", err);
        }
    }

    const handleDeleteClick = (product: Product) => setDialog(`delete:${product.productId}`);

    const handleConfirmDelete = async () => {
        if (!selectedProduct) return;
        try {
            await deleteProductMutation.mutateAsync(selectedProduct.productId);
            closeDialog();
        } catch (err) { console.error('Failed to delete product', err); }
    };

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
            closeDialog();
        } catch (err) {
            console.error("Transaction failed", err);
        }
    };

    useEffect(() => {
        if (!canViewSettings) return;
        apiClient.get('/settings/GlobalLowStockThreshold')
            .then(res => setGlobalThreshold(Number(res.data.value)))
            .catch(err => console.error("Failed to load global threshold", err));
    }, [canViewSettings]);



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
                {canStock && <button 
                    onClick={() => setDialog(`stock:${item.productId}`)} 
                    className="text-blue-600 hover:text-blue-800 font-medium text-sm">
                    Manage Stock
                </button>}
                {canEdit && <button onClick={() => handleEditClick(item)} className="text-emerald-600 hover:text-emerald-800 font-medium text-sm">Edit</button>}
                {canDelete && <button onClick={() => handleDeleteClick(item)} className="text-red-600 hover:text-red-800 font-medium text-sm">Delete</button>}
            </div>
        )
        }
    ], [globalThreshold, canStock, canEdit, canDelete]);

    return (
        <div className="space-y-4">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 sm:gap-0">
                <h2 className="text-2xl font-bold text-slate-800">Inventory</h2>
                
                {/* Dynamic Total Value KPI */}
                <div className="w-full sm:w-auto bg-emerald-50 text-emerald-700 px-4 py-2 rounded-lg border border-emerald-200 font-semibold shadow-sm flex items-center justify-between sm:justify-start gap-2">
                    <span className="text-sm font-medium text-emerald-600 uppercase tracking-wider">Total Value:</span>
                    <span className="text-lg">
                        {isLoading ? '...' : `$${totalValue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`}
                    </span>
                </div>
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
                            patch({ search: e.target.value, page: null });
                        }}
                        className="w-full sm:flex-1 border border-slate-300 rounded p-2 text-sm outline-none"
                    />

                    {/* Action Buttons*/}
                    <div className="flex w-full sm:w-auto gap-2 shrink-0">
                        
                        <button
                            onClick={() => {
                                patch({ lowStock: !lowStockOnly, page: null });
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

                        {canManageSettings && <button
                            onClick={() => setDialog('settings')}
                            className="w-12 sm:w-auto p-2 flex-none rounded border border-slate-300 bg-white text-slate-600 hover:bg-slate-50 flex items-center justify-center"
                            title="System Settings"
                        >
                            <Settings size={18} />
                        </button>}

                        {canCreate && <button 
                            onClick={handleAddClick}
                            className="flex-1 sm:flex-none bg-emerald-600 text-white px-3 py-2 rounded flex items-center justify-center gap-1 text-sm font-semibold hover:bg-emerald-700 whitespace-nowrap"
                        >
                            <Plus size={16} />
                            <span className="hidden lg:inline">New Product</span>
                        </button>}
                    </div>
                </div>

                {/* BOTTOM TIER: Dropdowns */}
                <div className="flex flex-row gap-3 w-full">
                    <select
                        className="flex-1 w-1/2 bg-white px-3 py-2 rounded border border-slate-300 text-sm outline-none" 
                        value={categoryFilter}
                        onChange={(e) => patch({ category: e.target.value, page: null })}
                    >
                        <option value="">All Categories</option>
                        {categories.map((category) => (
                            <option key={category.categoryId} value={category.name}>{category.name}</option>
                        ))}
                    </select>

                    <select
                        className="flex-1 w-1/2 bg-white px-3 py-2 rounded border border-slate-300 text-sm outline-none" 
                        value={supplierFilter}
                        onChange={(e) => patch({ supplier: e.target.value, page: null })}
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

            {dialog.includes(':') && itemId > 0 && !selectedProduct && !selectedStockProduct && !isLoading && (
                <p className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900">
                    This item is not on the current results page. Change the page or filters to find it; your shared link remains intact.
                </p>
            )}
            {/* Edit/Create Modal */}
            <SlideOver
                isOpen={isSlideOverOpen}
                onClose={closeDialog}
                title={selectedProduct ? `Edit ${selectedProduct.name}` : "Create New Product"}
            >
                <ProductForm
                    initialData={selectedProduct}
                    onSubmit={handleFormSubmit}
                    onCancel={closeDialog}
                />
            </SlideOver>

            {/* Stock Adjustment Modal */}
            <SlideOver 
                isOpen={isStockSlideOverOpen} 
                onClose={closeDialog} 
                title="Inventory Adjustment"
            >
                {selectedStockProduct && (
                    <StockManagementForm 
                        product={selectedStockProduct} 
                        onSubmit={handleStockSubmit} 
                        onCancel={closeDialog} 
                    />
                )}
            </SlideOver>

            {/* Delete Dialog */}
            <ConfirmDialog
                isOpen={isDeleteDialogOpen}
                title="Delete Product"
                message={`Are you sure you want to delete "${selectedProduct?.name}"? This action cannot be undone.`}
                onConfirm={handleConfirmDelete}
                onCancel={closeDialog}
                isProcessing={deleteProductMutation.isPending}
            />

            {/* Global Settings Modal (Hidden by default) */}
            {isSettingsModalOpen && (
                <GlobalSettingsModal onClose={closeDialog} />
            )}
            
        </div>
    );
}
