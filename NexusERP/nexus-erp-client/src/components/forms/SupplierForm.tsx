import { useForm } from "react-hook-form";
import { supplierSchema, type SupplierFormData } from "../../schemas/supplierSchema";
import type { SupplierResponse } from "../../types/supplier";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";


interface SupplierFormProps {
    initialData?: SupplierResponse | null;
    onSubmit: (data: SupplierFormData) => Promise<void>;
    onCancel: () => void;
}

export function SupplierForm({ initialData, onSubmit, onCancel }: SupplierFormProps) {
    const {
        register,
        handleSubmit,
        reset,
        formState: { errors, isSubmitting }
    } = useForm<SupplierFormData>({
        resolver: zodResolver(supplierSchema),
        defaultValues: { companyName: '', contactName: '', phone: '', email: '' }
    });

    useEffect(() => {
        if (initialData) {
            reset({
                companyName: initialData.companyName,
                contactName: initialData.companyName || '',
                phone: initialData.phone || '',
                email: initialData.email || ''
            });
        } else {
            reset();
        }
    }, [initialData, reset]);


    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 flex flex-col h-full">
            <div className="flex-1 space-y-4">
                <div>
                    <label className="block text-sm font-medium text-slate-700 mb-1">Company Name *</label>
                    <input
                        {...register('companyName')}
                        className={`w-full px-3 py-2 border rounded outline-none ${errors.companyName ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                    />
                    {errors.companyName && <p className="text-red-500 text-xs mt-1">{errors.companyName.message}</p>}
                </div>

                <div>
                    <label className="block text-sm font-medium text-slate-700 mb-1">Contact Name</label>
                    <input
                        {...register('contactName')}
                        className="w-full px-3 py-2 border border-slate-300 rounded outline-none focus:border-emerald-500"
                    />
                </div>

                <div className="grid grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Phone</label>
                        <input
                        {...register('phone')}
                        className="w-full px-3 py-2 border border-slate-300 rounded outline-none focus:border-emerald-500"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1">Email</label>
                        <input
                        {...register('email')}
                        className={`w-full px-3 py-2 border rounded outline-none ${errors.email ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
                        />
                        {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
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
                    {isSubmitting ? 'Saving...' : 'Save Supplier'}
                </button>
            </div>
        </form>
    );
}