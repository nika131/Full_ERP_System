export interface SupplierResponse {
  supplierId: number;
  companyName: string;
  contactName?: string | null;
  phone?: string | null;
  email?: string | null;
}

export interface SupplierLookup {
  supplierId: number;
  companyName: string;
}