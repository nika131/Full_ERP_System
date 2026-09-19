import { useForm } from "react-hook-form";
import { productSchema, type ProductFormData } from "../../schemas/productSchema";
import type { Product } from "../../types/product";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useSupplierLookupQuery } from "../../hooks/queries/useSupplierQueries";
import { useLookupCategoriesQuery } from "../../hooks/queries/useCategoryQueries";
import { Upload, X, Loader2, Image as ImageIcon, Shapes } from "lucide-react";
import { productService } from "../../api/productService";
interface ProductFormProps {
    initialData?: Product | null;
    onSubmit: (data: ProductFormData) => Promise<void>;
    onCancel: () => void;
}

export function ProductForm({ initialData, onSubmit, onCancel }: ProductFormProps) {
    const { data: categories = [], isLoading: isLoadingCategories } = useLookupCategoriesQuery();
    const { data: suppliers = [], isLoading: isLoadingSuppliers } = useSupplierLookupQuery();
    
    const isLoadingDropdowns = isLoadingCategories || isLoadingSuppliers;
    
    const [isUploading, setIsUploading] = useState(false);
    const [uploadError, setUploadError] = useState("");

    const [activeMode, setActiveMode] = useState<'image' | 'shape'>('image');
    
    const {
        register,
        handleSubmit,
        reset,
        watch,
        setValue,
        formState: { errors, isSubmitting } 
    } = useForm<ProductFormData>({
        resolver: zodResolver(productSchema),
        defaultValues: {
            name: '',
            categoryId: 0,
            supplierId: 0,
            quantity: 0,
            price: 0,
            costPrice: 0,
            vatRate: 0,
            marketDiscountRate: 0,
            maxDiscountPercentage: 0,
            barcode: '',
            imageUrl: null,
            shapeType: null,
            shapeColor: '#0ea5e9',
            shapeText: '',
            displayMode: 'image' 
        }
    });

    const currentImageUrl = watch('imageUrl');

    useEffect(() => {
        if (isLoadingDropdowns) return;

        if (initialData) {
            console.log('displayMode from server:', initialData.displayMode, typeof initialData.displayMode);
            const normalizedMode = String(initialData.displayMode).toLowerCase() === 'shape' ? 'shape' : 'image';

            setActiveMode(normalizedMode);

            reset({
                name: initialData.name,
                categoryId: initialData.categoryId,
                supplierId: initialData.supplierId ?? 0,
                quantity: initialData.quantity,
                price: initialData.price,
                costPrice: initialData.costPrice,
                vatRate: initialData.vatRate ?? 0,
                marketDiscountRate: initialData.marketDiscountRate ?? 0,
                maxDiscountPercentage: initialData.maxDiscountPercentage ?? 0,
                lowStockThreshold: initialData.lowStockThreshold ?? null,
                barcode: initialData.barcode ?? '',
                imageUrl: initialData.imageUrl ?? null,
                shapeType: initialData.shapeType ?? null,
                shapeColor: initialData.shapeColor ?? '#0ea5e9',
                shapeText: initialData.shapeText ?? '',
                displayMode: normalizedMode as "image" | "shape"
            });
        } else {
            reset();
            setActiveMode('image')
            setValue('displayMode', 'image');
        }
    }, [initialData, reset, isLoadingDropdowns, setValue]);

    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        if (file.size > 5 * 1024 * 1024) {
            setUploadError("Image must be smaller than 5MB");
            return;
        }

        setIsUploading(true);
        setUploadError("");

        try {
            const url = await productService.uploadImage(file);
            setValue('imageUrl', url, { shouldValidate: true, shouldDirty: true });
        } catch (err) {
            setUploadError("Failed to upload image.");
        } finally {
            setIsUploading(false);
        }
    };

    const handleFormSubmit = async (data: ProductFormData) => {
        await onSubmit(data);
    };


    return (
        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4 flex flex-col h-full overflow-y-auto pr-2">

            <div className="flex-1 space-y-6">
                
                {/* Basic Info Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="sm:col-span-2">
                        <label className="block text-sm font-medium text-slate-700 mb-1">Product Name</label>
                        <input
                            {...register('name')}
                            className={`w-full px-3 py-2 border rounded outline-none transition-colors ${errors.name ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                        />
                        {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name.message}</p>}
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Barcode (Optional)</label>
                        <input
                            {...register('barcode')}
                            className="w-full px-3 py-2 border border-slate-300 rounded outline-none focus:border-emerald-500"
                        />
                    </div>
                </div>

                {/* POS DISPLAY SECTION */}
                <div className="bg-slate-50 border border-slate-200 rounded-lg p-4">
                    <div className="flex items-center justify-between mb-4">
                        <label className="block text-sm font-bold text-slate-700">POS Display Representation</label>
                        
                        {/* The Toggle Switch*/}
                        <div className="flex bg-slate-200 p-1 rounded-md">
                            <button
                                type="button"
                                onClick={() => {
                                    setActiveMode('image');
                                    setValue('displayMode', 'image', { shouldDirty: true });
                                }}
                                className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold rounded-sm transition-all ${activeMode === 'image' ? 'bg-white shadow-sm text-emerald-600' : 'text-slate-500 hover:text-slate-700'}`}
                            >
                                <ImageIcon size={14} /> Photo
                            </button>
                            <button
                                type="button"
                                onClick={() => {
                                    setActiveMode('shape');
                                    setValue('displayMode', 'shape', { shouldDirty: true });
                                }}
                                className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold rounded-sm transition-all ${activeMode === 'shape' ? 'bg-white shadow-sm text-emerald-600' : 'text-slate-500 hover:text-slate-700'}`}
                            >
                                <Shapes size={14} /> Vector Shape
                            </button>
                        </div>
                    </div>

                    {/* Show this IF Image is selected */}
                    {activeMode === 'image' && (
                        <div className="animate-in fade-in slide-in-from-top-2 duration-200">
                            {currentImageUrl ? (
                                <div className="relative w-full max-w-[200px] h-32 rounded-lg border border-slate-200 overflow-hidden group mx-auto">
                                    <img 
                                        src={`${(import.meta.env.VITE_API_URL || 'https://localhost:7001').replace(/\/api$/, '')}${currentImageUrl}`} 
                                        alt="Preview" 
                                        className="w-full h-full object-contain bg-white"
                                    />
                                    <button 
                                        type="button"
                                        onClick={() => setValue('imageUrl', null, { shouldDirty: true })}
                                        className="absolute top-2 right-2 p-1.5 bg-red-500 text-white rounded-full shadow-md hover:bg-red-600 transition-colors"
                                    >
                                        <X size={16} />
                                    </button>
                                </div>
                            ) : (
                                <div className="flex items-center justify-center w-full">
                                    <label className="flex flex-col items-center justify-center w-full h-32 border-2 border-slate-300 border-dashed rounded-lg cursor-pointer bg-white hover:bg-slate-50 transition-colors">
                                        <div className="flex flex-col items-center justify-center pt-5 pb-6">
                                            {isUploading ? (
                                                <Loader2 className="w-8 h-8 text-emerald-500 animate-spin mb-2" />
                                            ) : (
                                                <Upload className="w-8 h-8 text-slate-400 mb-2" />
                                            )}
                                            <p className="text-sm text-slate-500">
                                                {isUploading ? "Uploading..." : <><span className="font-semibold text-emerald-600">Click to upload</span> or drag and drop</>}
                                            </p>
                                        </div>
                                        <input type="file" className="hidden" accept="image/png, image/jpeg, image/webp" onChange={handleImageUpload} disabled={isUploading} />
                                    </label>
                                </div>
                            )}
                            {uploadError && <p className="text-red-500 text-xs mt-1 text-center">{uploadError}</p>}
                        </div>
                    )}

                    {/* Show this IF Shape is selected */}
                    {activeMode === 'shape' && (
                        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 animate-in fade-in slide-in-from-top-2 duration-200">
                            <div>
                                <label className="block text-xs font-medium text-slate-500 mb-1 uppercase tracking-wider">Type</label>
                                <select 
                                    {...register('shapeType', { setValueAs: v => v === "" ? null : v })} 
                                    className="w-full border border-slate-300 rounded p-2 text-sm outline-none focus:border-emerald-500 bg-white"
                                >
                                    <option value="" disabled>Select Shape...</option>
                                    <option value="square">Square</option>
                                    <option value="Circle">Circle</option>
                                    <option value="Triangle">Triangle</option>
                                    <option value="Pentagon">Pentagon</option>
                                    <option value="Star">Star</option>
                                    <option value="Diamond">Diamond</option>
                                    <option value="Heart">Heart</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-slate-500 mb-1 uppercase tracking-wider">Color Base</label>
                                <input 
                                    type="color" 
                                    {...register('shapeColor')} 
                                    className="w-full h-[38px] border border-slate-300 rounded cursor-pointer p-0.5 bg-white" 
                                />
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-slate-500 mb-1 uppercase tracking-wider">Center Text</label>
                                <input 
                                    type="text" 
                                    {...register('shapeText')} 
                                    placeholder="e.g. 500ml" 
                                    className="w-full border border-slate-300 rounded p-2 text-sm outline-none focus:border-emerald-500" 
                                />
                            </div>
                        </div>
                    )}
                </div>

                {/* Categorization Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Category</label>
                        <select
                            {...register('categoryId', { valueAsNumber: true })}
                            disabled={isLoadingDropdowns}
                            className={`w-full px-3 py-2 border rounded outline-none bg-white transition-colors ${errors.categoryId ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                        >
                            <option value={0} disabled>Select a category...</option>
                            {categories.map((cat) => (
                                <option key={cat.categoryId} value={cat.categoryId}>{cat.name}</option>
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
                                <option key={sup.supplierId} value={sup.supplierId}>{sup.companyName}</option>
                            ))}
                        </select>
                        {errors.supplierId && <p className="text-red-500 text-xs mt-1">{errors.supplierId.message}</p>}
                    </div>
                </div>

                {/* Stock & Pricing Grid */}
                <div className="grid grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Quantity</label>
                        <input
                            type="number"
                            readOnly
                            {...register('quantity', { valueAsNumber: true })}
                            className={`w-full px-3 py-2 border rounded outline-none bg-slate-100 text-slate-500 cursor-not-allowed ${errors.quantity ? 'border-red-500' : 'border-slate-300'}`}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Low Stock Alert At</label>
                        <input
                            type="number"
                            placeholder="Use Global (5)"
                            {...register('lowStockThreshold', { 
                                setValueAs: (v) => (v === "" || v === null || isNaN(parseInt(v, 10))) ?  null : parseInt(v, 10) 
                            })}
                            className="w-full px-3 py-2 border border-slate-300 rounded outline-none focus:border-emerald-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Selling Price</label>
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

                {/* Tax & Discount Grid */}
                <div className="grid grid-cols-3 gap-4 pb-2">
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">VAT Rate (%)</label>
                        <input
                            type="number"
                            step="0.1"
                            {...register('vatRate', { valueAsNumber: true })}
                            className="w-full px-3 py-2 border border-slate-300 rounded outline-none focus:border-emerald-500"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Market Disc. (%)</label>
                        <input
                            type="number"
                            step="0.1"
                            {...register('marketDiscountRate', { valueAsNumber: true })}
                            className="w-full px-3 py-2 border border-slate-300 rounded outline-none focus:border-emerald-500"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Max Disc. (%)</label>
                        <input
                            type="number"
                            step="0.1"
                            {...register('maxDiscountPercentage', { valueAsNumber: true })}
                            className="w-full px-3 py-2 border border-slate-300 rounded outline-none focus:border-emerald-500"
                        />
                    </div>
                </div>

            </div>

            {/* Actions */}
            <div className="pt-4 border-t border-slate-200 flex justify-end space-x-3 mt-auto pb-4">
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