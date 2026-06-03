export interface Product {
  productId: number;
  name: string;
  categoryId: number;
  categoryName: string;
  supplierId: number | null;
  quantity: number;
  price: number;
  costPrice: number;
}