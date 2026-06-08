export interface Product {
  productId: number;
  name: string;
  categoryId: number;
  categoryName: string;
  supplierId: number | null;
  companyName: string;
  quantity: number;
  price: number;
  costPrice: number;
}