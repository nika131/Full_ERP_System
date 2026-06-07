import { useForm } from "react-hook-form";
import { categorySchema, type CategoryFormData } from "../../schemas/categorySchema";
import type { Category } from "../../types/category";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";


interface Props {
    initialData?: Category | null;
    onSubmit: (data: CategoryFormData) => Promise<void>;
    onCancel: () => void; 
}

export function CategoryForm({ initialData, onSubmit, onCancel }: Props) {
    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<CategoryFormData>({
        resolver: zodResolver(categorySchema),
        defaultValues: { name: ''}
    });

    useEffect(() => {
        if (initialData) reset({ name: initialData.categoryName});
        else reset();
    }, [initialData, reset]);

    return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 flex flex-col h-full">
      <div className="flex-1 space-y-4">
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Category Name *</label>
          <input {...register('name')} className={`w-full px-3 py-2 border rounded outline-none ${errors.name ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`} />
          {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name.message}</p>}
        </div>
      </div>
      <div className="pt-4 border-t border-slate-200 flex justify-end space-x-3 mt-auto">
        <button type="button" onClick={onCancel} disabled={isSubmitting} className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded hover:bg-slate-50">Cancel</button>
        <button type="submit" disabled={isSubmitting} className="px-4 py-2 text-sm font-medium text-white bg-emerald-600 rounded hover:bg-emerald-700 disabled:opacity-50">
          {isSubmitting ? 'Saving...' : 'Save Category'}
        </button>
      </div>
    </form>
  );
}