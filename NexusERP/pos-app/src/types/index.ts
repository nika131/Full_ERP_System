export type DisplayMode = "Image" | "Shape";
export type ShapeType =
  | "square"
  | "Circle"
  | "Triangle"
  | "Pentagon"
  | "Star"
  | "Diamond"
  | "Heart";

export interface Product {
  productId: number;
  name: string;
  categoryId: number;
  categoryName: string;
  supplierId: number | null;
  companyName: string;
  quantity: number;
  lowStockThreshold: number | null;
  price: number;
  costPrice: number;
  vatRate: number;
  marketDiscountRate: number;
  maxDiscountPercentage: number;
  barcode: string | null;
  imageUrl: string | null;
  shapeType: ShapeType | null;
  shapeColor: string | null;
  shapeText: string | null;
  displayMode: DisplayMode;
}

export interface AuthUser {
  userId: number;
  fullName: string;
  username: string;
  role: string;
  permissions: string[];
}

export interface UserLookup {
  userId: number;
  fullName: string;
  username: string;
}

export interface StoreLookup {
  storeId: number;
  name: string;
  maxCartDiscountPercentage: number;
}

export interface CartItem {
  productId: number;
  name: string;
  unitPrice: number;
  quantity: number;
  manualItemDiscount: number;
  maxDiscountPercentage: number;
}

export interface CurrentShift {
  shiftId: number;
  storeId: number;
  storeName: string;
  startDate: string;
  startingCash: number;
  expectedEndingCash: number;
  totalSales: number;
  totalProfit: number;
  cashSales: number;
  cardSales: number;
  voucherSales: number;
  totalPayIns: number;
  totalPayOuts: number;
  receiptCount: number;
  notes: string | null;
}

export interface ShiftHistoryItem {
  shiftId: number;
  cashierName: string;
  startDate: string;
  endDate: string | null;
  totalSales: number;
  startingCash: number;
  actualEndingCash: number | null;
  status: "Open" | "Closed";
  notes: string | null;
}

export interface ShiftReceiptSummary {
  receiptId: number;
  receiptNumber: string;
  createdAt: string;
  finalTotal: number;
  paymentMethod: "Cash" | "Card" | "Voucher";
  itemCount: number;
}

export interface ReceiptLine {
  productName: string;
  quantity: number;
  unitPrice: number;
  marketDiscountAmount: number;
  manualItemDiscountAmount: number;
  lineTotal: number;
}

export interface ReceiptDetail {
  receiptId: number;
  receiptNumber: string;
  createdAt: string;
  cashierName: string;
  storeName: string;
  subTotal: number;
  cartDiscountAmount: number;
  totalVatAmount: number;
  finalTotal: number;
  paymentMethod: "Cash" | "Card" | "Voucher";
  lines: ReceiptLine[];
}

export interface PosSlot {
  slotIndex: number;
  productId: number | null;
}
