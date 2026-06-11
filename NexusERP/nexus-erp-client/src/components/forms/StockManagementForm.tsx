import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { stockSchema, type StockFormData } from '../../schemas/stockSchema';
import type { Product } from '../../types/product';

interface Props {
  product: Product;
  onSubmit: (data: StockFormData) => Promise<void>;
  onCancel: () => void;
}

export function StockManagementForm({ product, onSubmit, onCancel }: Props) {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<StockFormData>({
    resolver: zodResolver(stockSchema),
    defaultValues: { quantity: 1, transactionType: 'Restock' }
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 flex flex-col h-full">
      <div className="flex-1 space-y-4">
        
        <div className="bg-slate-50 p-4 rounded-md border border-slate-200">
          <p className="text-sm text-slate-500 mb-1">Target Product</p>
          <p className="font-bold text-slate-800">{product.name}</p>
          <p className="text-sm text-slate-600">Current Stock: <span className="font-bold">{product.quantity} units</span></p>
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Action Type *</label>
          <select 
            {...register('transactionType')}
            className={`w-full px-3 py-2 border rounded outline-none ${errors.transactionType ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
          >
            <option value="Restock">Restock (Add Inventory)</option>
            <option value="Loss">Loss (Unaccounted Missing)</option>
            <option value="Damage">Damage (Destroyed/Unsellable)</option>
          </select>
          {errors.transactionType && <p className="text-red-500 text-xs mt-1">{errors.transactionType.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Quantity *</label>
          <input 
            type="number"
            min="1"
            {...register('quantity', { valueAsNumber: true })}
            className={`w-full px-3 py-2 border rounded outline-none ${errors.quantity ? 'border-red-500' : 'border-slate-300 focus:border-emerald-500'}`}
          />
          {errors.quantity && <p className="text-red-500 text-xs mt-1">{errors.quantity.message}</p>}
        </div>

      </div>

      <div className="pt-4 border-t border-slate-200 flex justify-end space-x-3 mt-auto">
        <button type="button" onClick={onCancel} disabled={isSubmitting} className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded hover:bg-slate-50">
          Cancel
        </button>
        <button type="submit" disabled={isSubmitting} className="px-4 py-2 text-sm font-medium text-white bg-emerald-600 rounded hover:bg-emerald-700 disabled:opacity-50">
          {isSubmitting ? 'Processing...' : 'Confirm Action'}
        </button>
      </div>
    </form>
  );
}