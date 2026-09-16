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
  VatRate?: number;
  MarketDiscountRate?: number; 
  MaxDiscountPercentage?: number;
  Barcode?: number;
  ImageUrl?: string; 
  ShapeType?: string;
  ShapeColor?: string;
  ShapeText?: string;
}