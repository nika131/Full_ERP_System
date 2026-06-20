import { type ReactNode } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';

export interface ColumnDef<T> {
  header: string;
  accessor: keyof T | string;
  render?: (item: T) => ReactNode;
  className?: string;
}

interface CursorDataTableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  isLoading: boolean;
  hasMorePages: boolean;
  isFirstPage: boolean;
  onNext: () => void;
  onPrevious: () => void;
}

export function CursorDataTable<T>({ 
  data, 
  columns, 
  isLoading, 
  hasMorePages,
  isFirstPage,
  onNext,
  onPrevious 
}: CursorDataTableProps<T>) {

  return (
    <div className="bg-white rounded-lg shadow-sm border border-slate-200 overflow-hidden flex flex-col">
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200">
              {columns.map((col, index) => (
                <th key={index} className={`p-4 text-xs font-semibold text-slate-500 uppercase tracking-wider ${col.className || ''}`}>
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 text-sm">
            {isLoading ? (
              <tr>
                <td colSpan={columns.length} className="p-8 text-center text-slate-500">
                  Loading data...
                </td>
              </tr>
            ) : data.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="p-8 text-center text-slate-500">
                  No records found.
                </td>
              </tr>
            ) : (
              data.map((item, rowIndex) => (
                <tr key={rowIndex} className="hover:bg-slate-50 transition-colors">
                  {columns.map((col, colIndex) => (
                    <td key={colIndex} className={`p-4 text-slate-700 ${col.className || ''}`}>
                      {col.render ? col.render(item) : String(item[col.accessor as keyof T] || '')}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="p-4 border-t border-slate-200 bg-slate-50 flex items-center justify-between">
        <div className="text-sm text-slate-500">
           <span>Data stream optimized for high volume</span>
        </div>
        
        <div className="flex items-center space-x-4">
          <button 
            onClick={onPrevious}
            disabled={isFirstPage || isLoading}
            className="flex items-center space-x-1 px-3 py-2 border border-slate-200 rounded text-slate-600 bg-white hover:bg-slate-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronLeft size={16} />
            <span className="text-sm font-medium">Previous</span>
          </button>
          
          <button 
            onClick={onNext}
            disabled={!hasMorePages || isLoading}
            className="flex items-center space-x-1 px-3 py-2 border border-slate-200 rounded text-slate-600 bg-white hover:bg-slate-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <span className="text-sm font-medium">Next</span>
            <ChevronRight size={16} />
          </button>
        </div>
      </div>
    </div>
  );
}