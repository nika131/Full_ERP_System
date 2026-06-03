import { useEffect, useState } from 'react';
import { productService } from '../api/productService';
import { type Product } from '../types/product';

export default function InventoryList() {
    const [products, setProducts] = useState<Product[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        loadProducts();
    }, []);

    const loadProducts = async () => {
        try {
            setIsLoading(true);
            const data = await productService.getAll();
            setProducts(data);
        } catch (err) {
            console.error(err);
            setError('Failed to load products. Please try again later.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200">
      <div className="p-6 border-b border-gray-200 flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-800">Inventory Management</h2>
        <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded text-sm font-medium transition-colors">
          + Add Product
        </button>
      </div>

      <div className="p-6">
        {error && (
          <div className="mb-4 p-3 bg-red-50 text-red-600 border border-red-200 rounded text-sm">
            {error}
          </div>
        )}

        {isLoading ? (
          <div className="text-center py-8 text-gray-500">Loading data...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-gray-50 text-gray-600 text-sm uppercase tracking-wider border-b border-gray-200">
                  <th className="p-4 font-medium">ID</th>
                  <th className="p-4 font-medium">Product Name</th>
                  <th className="p-4 font-medium">Category</th>
                  <th className="p-4 font-medium text-right">Quantity</th>
                  <th className="p-4 font-medium text-right">Price</th>
                  <th className="p-4 font-medium text-center">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 text-sm">
                {products.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="p-4 text-center text-gray-500">
                      No products found.
                    </td>
                  </tr>
                ) : (
                  products.map((product) => (
                    <tr key={product.productId} className="hover:bg-gray-50 transition-colors">
                      <td className="p-4 text-gray-500">#{product.productId}</td>
                      <td className="p-4 font-medium text-gray-900">{product.name}</td>
                      <td className="p-4 text-gray-500">
                        <span className="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs">
                          {product.categoryName}
                        </span>
                      </td>
                      <td className="p-4 text-right">
                        <span className={`font-medium ${product.quantity < 10 ? 'text-red-600' : 'text-green-600'}`}>
                          {product.quantity}
                        </span>
                      </td>
                      <td className="p-4 text-right text-gray-900 font-medium">
                        ${product.price.toFixed(2)}
                      </td>
                      <td className="p-4 text-center">
                        <button className="text-blue-600 hover:text-blue-800 font-medium mr-3">Edit</button>
                        <button className="text-red-600 hover:text-red-800 font-medium">Delete</button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}