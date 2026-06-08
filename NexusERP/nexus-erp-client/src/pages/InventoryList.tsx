import { useEffect, useState, useMemo } from 'react';
import { productService, type TransactionPayLoad } from '../api/productService';
import { type Product } from '../types/product';
import { DataTable, type ColumnDef } from '../components/Ui/DataTable';
import type { ProductFormData } from '../schemas/productSchema';
import { SlideOver } from '../components/Ui/SlideOver';
import { ProductForm } from '../components/forms/ProductForm';
import { ConfirmDialog } from '../components/Ui/ConfirmDialog';

export default function InventoryList() {
    const [products, setProducts] = useState<Product[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);

    const [searchTerm, setSearchTerm] = useState('');

    const [isSildeOverOpen, setIsSlideOverOpen] = useState(false);
    const [selectedProduct, setSelectedProduct]= useState<Product | null>(null);

    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);

    const [isSellModalOpen, setIsSellModalOpen] = useState(false);
    const [sellQuantity, setSellQuantity] = useState<number>(1);
    const [isSelling, setIsSelling] = useState(false);

    useEffect(() => {
        const controller = new AbortController();

        const timer = setTimeout(() => {
            loadProducts(controller.signal);
        }, 300);

        return () => {
            clearTimeout(timer);
            controller.abort();
        };
    }, [page, searchTerm]);

    const loadProducts = async (signal: AbortSignal) => {
        try {
            setError('');
            setIsLoading(true);

            const data = await productService.getProducts(page, 10, searchTerm, signal);
            
            setProducts(data.items);
            setTotalPages(data.totalPages);
            setTotalCount(data.totalCount);

            setIsLoading(false);
        } catch (err: any) {

            if (err.name === 'CanceledError' || err.name === 'canceled') {
                return;
            }

            console.error(err);
            setError('Failed to load Inventory Data. Please try again later.');
        
            setIsLoading(false);
        } 
    };

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

            await productService.saveProduct(payload);
            setIsSlideOverOpen(false);

            const controller = new AbortController();
            loadProducts(controller.signal);
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
            setIsDeleting(true);
            await productService.deleteProduct(selectedProduct.productId);

            setIsDeleteDialogOpen(false);
            setSelectedProduct(null);

            const controller = new AbortController();
            loadProducts(controller.signal);
        } catch (err) {
            console.error("Failed to delete product", err);
        } finally {
            setIsDeleting(false);
        }
    }

    const handleSellClick = (product: Product) => {
        setSelectedProduct(product);
        setSellQuantity(1);
        setIsSellModalOpen(true);
    }

    const handleConfrimSell = async () => {
        if (!selectedProduct) return;

        try {
            setIsSelling(true);
            const payload: TransactionPayLoad = {
                productId: selectedProduct.productId,
                supplierId: selectedProduct.supplierId || null,
                transactionType: "Sale",
                soldQty: sellQuantity,
                productPrice: selectedProduct.price,
                costPrice: selectedProduct.costPrice 
            };

            await productService.makeTransaction(payload);

            setIsSellModalOpen(false);
            setSelectedProduct(null);

            const controller = new AbortController()
            loadProducts(controller.signal);
        } catch (err) {
            console.error("Failed to process sale", err);
        } finally {
            setIsSelling(false);
        }
    }

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
        render: (item) => (
            <span className={`font-medium ${item.quantity < 10 ? 'text-red-600' : 'text-emerald-600'}`}>
            {item.quantity}
            </span>
        )
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
        className: 'text-center',
        render: (item) => (
            <div>
                <button
                    onClick={() => handleSellClick(item)}
                    className="text-blue-600 hover:text-blue-800 font-medium mr-3 transition-colors">
                    Sell
                </button>
                <button 
                    onClick={() => handleEditClick(item)}
                    className="text-emerald-600 hover:text-emerald-800 font-medium mr-3 transition-colors">
                        Edit
                </button>
                <button 
                    onClick={() => handleDeleteClick(item)}
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
                <h2 className="text-2xl font-bold text-slate-800">Inventory</h2>
                <button 
                    onClick={handleAddClick}
                    className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-md text-sm font-medium transition-colors shadow-sm">
                + Add Product
                </button>
            </div>

            <div className="flex bg-white p-1 rounded-md shadow-sm border border-slate-200 max-w-md">
                <input 
                type="text" 
                placeholder="Search products by name or ID..." 
                value={searchTerm}
                onChange={(e) => {
                    setSearchTerm(e.target.value);
                    setPage(1); 
                }}
                className="w-full px-3 py-2 outline-none text-sm bg-transparent"
                />
            </div>

            {error && (
                <div className="p-3 bg-red-50 text-red-600 border border-red-200 rounded text-sm">
                {error}
                </div>
            )}

            <DataTable 
                data={products}
                columns={columns}
                isLoading={isLoading}
                page={page}
                totalPages={totalPages}
                totalCount={totalCount}
                onPageChange={(newPage) => setPage(newPage)}
            />

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

        
            <ConfirmDialog
                isOpen={isDeleteDialogOpen}
                title="Delete Product"
                message={`Are you sure you want to delete "${selectedProduct?.name}"? This action cannot be undone.`}
                onConfirm={handleConfirmDelete}
                onCancel={() => {
                setIsDeleteDialogOpen(false);
                setSelectedProduct(null);
                }}
                isProcessing={isDeleting}
            />


            {/*Sell Modal Overlay */}
            {isSellModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-sm overflow-hidden">
                        <div className="p-6">
                            <h3 className="text-lg font-semibold text-slate-900 mb-2">Process Sale</h3>
                            <p className="text-slate-600 text-sm mb-4">
                                Product: <span className="font-semibold text-slate-800">{selectedProduct?.name}</span><br/>
                                Current Stock: <span className="font-semibold text-slate-800">{selectedProduct?.quantity}</span>
                            </p>
                            
                            <label className="block text-sm font-medium text-slate-700 mb-1">Quantity to Sell</label>
                            <input 
                                type="number" 
                                min="1"
                                max={selectedProduct?.quantity} 
                                value={sellQuantity}
                                onChange={(e) => setSellQuantity(parseInt(e.target.value) || 1)}
                                className="w-full px-3 py-2 border border-slate-300 rounded outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                            />
                        </div>
                        
                        <div className="px-6 py-4 bg-slate-50 border-t border-slate-200 flex justify-end space-x-3">
                            <button
                                onClick={() => { setIsSellModalOpen(false); setSelectedProduct(null); }}
                                disabled={isSelling}
                                className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded hover:bg-slate-100 transition-colors"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleConfrimSell}
                                disabled={isSelling || sellQuantity > (selectedProduct?.quantity || 0) || sellQuantity < 1}
                                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded hover:bg-blue-700 transition-colors disabled:opacity-50"
                            >
                                {isSelling ? 'Processing...' : 'Confirm Sale'}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}