export interface Product {
  productId: number;
  name: string;
  categoryId: number;
  categoryName: string;
  supplierId: number | null;
  companyName: string;
  quantity: number;
  lowStockThreshold?: number | null;
  price: number;
  costPrice: number;
  vatRate?: number;
  marketDiscountRate?: number; 
  maxDiscountPercentage?: number;
  barcode?: string;
  imageUrl?: string; 
  shapeType?: string;
  shapeColor?: string;
  shapeText?: string;
  displayMode: 'image' | 'shape';
}