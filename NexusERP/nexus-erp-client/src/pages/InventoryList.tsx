import { useEffect, useState, useMemo } from 'react';
import { productService } from '../api/productService';
import { type Product } from '../types/product';
import { DataTable, type ColumnDef } from '../components/Ui/DataTable';

export default function InventoryList() {
    const [products, setProducts] = useState<Product[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);

    const [searchTerm, setSearchTerm] = useState('');

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
      render: () => (
        <div>
          <button className="text-emerald-600 hover:text-emerald-800 font-medium mr-3 transition-colors">Edit</button>
          <button className="text-red-600 hover:text-red-800 font-medium transition-colors">Delete</button>
        </div>
      )
    }
  ], []);

    return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-slate-800">Inventory</h2>
        <button className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-md text-sm font-medium transition-colors shadow-sm">
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
    </div>
  );
}