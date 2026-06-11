import { useForm } from "react-hook-form";
import { productSchema, type ProductFormData } from "../../schemas/productSchema";
import type { Product } from "../../types/product";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import type { CategoryLookup } from "../../types/category";
import type { SupplierLookup } from "../../types/supplier";
import { productService } from "../../api/productService";


interface ProductFormProps {
    initialData?: Product | null;
    onSubmit: (data: ProductFormData) => Promise<void>;
    onCancel: () => void;
}

export function ProductForm({ initialData, onSubmit, onCancel }: ProductFormProps) {
    const [categories, setCatgeories] = useState<CategoryLookup[]>([]);
    const [suppliers, setSuppliers] = useState<SupplierLookup[]>([]);
    const [isLoadingDropdowns, setIsLoadingDropdowns] = useState(true);
    
    const {
        register,
        handleSubmit,
        reset,
        formState: { errors, isSubmitting } 
    } = useForm<ProductFormData>({
        resolver: zodResolver(productSchema),
        defaultValues: {
            name: '',
            categoryId: 0,
            supplierId: 0,
            quantity: 0,
            price: 0,
            costPrice: 0
        }
    });

    useEffect(() => {
        const fetchDropdownData = async () => {
            try {
                const [cats, sups] = await Promise.all([
                    productService.getCategories(),
                    productService.getSuppliers()
                ]);
                setCatgeories(cats);
                setSuppliers(sups);
            } catch (err) {
                console.error("Failed to load dropdown data", err);
            } finally {
                setIsLoadingDropdowns(false);
            }
        };

        fetchDropdownData();
    }, []);

    useEffect(() => {
        if (isLoadingDropdowns) return;

        if (initialData) {
            reset({
                name: initialData.name,
                categoryId: initialData.categoryId,
                supplierId: initialData.supplierId ?? 0,
                quantity: initialData.quantity,
                price: initialData.price,
                costPrice: initialData.costPrice
            });
        } else {
            reset();
        }
    }, [initialData, reset, isLoadingDropdowns]);

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 flex flex-col h-full">
            <div className="flex-1 space-y-4">
                
                {/* Name Input */}
                <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Product Name</label>
                <input
                    {...register('name')}
                    className={`w-full px-3 py-2 border rounded outline-none transition-colors ${errors.name ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                />
                {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name.message}</p>}
                </div>

                {/* The New Relational Dropdowns */}
                <div className="grid grid-cols-1 gap-4 bg-slate-50 p-4 border border-slate-200 rounded">
                <div>
                    <label className="block text-sm font-medium text-slate-700 mb-1">Category</label>
                    <select
                    {...register('categoryId', { valueAsNumber: true })}
                    disabled={isLoadingDropdowns}
                    className={`w-full px-3 py-2 border rounded outline-none bg-white transition-colors ${errors.categoryId ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                    >
                    <option value={0} disabled>Select a category...</option>
                    {categories.map((cat) => (
                        <option key={cat.categoryId} value={cat.categoryId}>
                        {cat.name}
                        </option>
                    ))}
                    </select>
                    {errors.categoryId && <p className="text-red-500 text-xs mt-1">{errors.categoryId.message}</p>}
                </div>

                <div>
                    <label className="block text-sm font-medium text-slate-700 mb-1">Supplier</label>
                    <select
                    {...register('supplierId', { valueAsNumber: true })}
                    disabled={isLoadingDropdowns}
                    className={`w-full px-3 py-2 border rounded outline-none bg-white transition-colors ${errors.supplierId ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                    >
                    <option value={0}>-- No Supplier --</option>
                    {suppliers.map((sup) => (
                        <option key={sup.supplierId} value={sup.supplierId}>
                        {sup.companyName}
                        </option>
                    ))}
                    </select>
                    {errors.supplierId && <p className="text-red-500 text-xs mt-1">{errors.supplierId.message}</p>}
                </div>
                </div>

                {/* Numeric Inputs Grid */}
                <div className="grid grid-cols-2 gap-4">
                <div>
                    <label className="block text-sm font-medium text-slate-700 mb-1">Quantity</label>
                    <input
                    type="number"
                    disabled
                    {...register('quantity', { valueAsNumber: true })}
                    className={`w-full px-3 py-2 border rounded outline-none bg-slate-100 text-slate-500 cursor-not-allowed ${errors.quantity ? 'border-red-500' : 'border-slate-300'}`}
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-slate-700 mb-1">Price</label>
                    <input
                    type="number"
                    step="0.01"
                    {...register('price', { valueAsNumber: true })}
                    className={`w-full px-3 py-2 border rounded outline-none transition-colors ${errors.price ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                    />
                    {errors.price && <p className="text-red-500 text-xs mt-1">{errors.price.message}</p>}
                </div>

                <div>
                    <label className="block text-sm font-medium text-slate-700 mb-1">Cost Price</label>
                    <input
                    type="number"
                    step="0.01"
                    {...register('costPrice', { valueAsNumber: true })}
                    className={`w-full px-3 py-2 border rounded outline-none transition-colors ${errors.costPrice ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                    />
                    {errors.costPrice && <p className="text-red-500 text-xs mt-1">{errors.costPrice.message}</p>}
                </div>
                </div>
            </div>

            <div className="pt-4 border-t border-slate-200 flex justify-end space-x-3 mt-auto">
                <button
                type="button"
                onClick={onCancel}
                disabled={isSubmitting}
                className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded hover:bg-slate-50 transition-colors"
                >
                Cancel
                </button>
                <button
                type="submit"
                disabled={isSubmitting}
                className="px-4 py-2 text-sm font-medium text-white bg-emerald-600 rounded hover:bg-emerald-700 transition-colors disabled:opacity-50"
                >
                {isSubmitting ? 'Saving...' : 'Save Product'}
                </button>
            </div>
        </form>
    );
}