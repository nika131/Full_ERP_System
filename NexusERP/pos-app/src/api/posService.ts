import apiClient from "./client";
import type {
  CurrentShift,
  ShiftHistoryItem,
  ShiftReceiptSummary,
  ReceiptDetail,
  StoreLookup,
} from "../types";

export interface OpenShiftPayload {
  storeId: number;
  startingCash: number;
  note?: string;
}

export interface CloseShiftPayload {
  actualEndingCash: number;
  note?: string;
}

export interface CashMovementPayload {
  storeId: number;
  movementType: "PayIn" | "PayOut";
  amount: number;
  reason?: string;
}

export interface CheckoutItemPayload {
  productId: number;
  quantity: number;
  manualItemDiscountPercentage: number;
}

export interface CheckoutPayload {
  storeId: number;
  cartDiscountPercentage: number;
  paymentMethod: "Cash" | "Card" | "Voucher";
  items: CheckoutItemPayload[];
}

export interface CheckoutResult {
  message: string;
  receiptNumber: string;
  total: number;
}

export type CheckoutQuotePayload = Omit<CheckoutPayload, "paymentMethod">;

export interface CheckoutQuoteResult {
  finalTotal: number;
}

export const posService = {
  getCurrentShift: async (
    storeId: number
  ): Promise<{ hasOpenShift: boolean; shift: CurrentShift | null }> => {
    const res = await apiClient.get("/pos/shift/current", { params: { storeId } });
    return res.data;
  },

  openShift: async (payload: OpenShiftPayload): Promise<{ shiftId: number }> => {
    const res = await apiClient.post("/pos/shift/open", payload);
    return res.data;
  },

  closeShift: async (
    shiftId: number,
    payload: CloseShiftPayload
  ): Promise<{ variance: number }> => {
    const res = await apiClient.post(`/pos/shift/${shiftId}/close`, payload);
    return res.data;
  },

  addCashMovement: async (shiftId: number, payload: CashMovementPayload) => {
    const res = await apiClient.post(`/pos/shift/${shiftId}/cash-movement`, payload);
    return res.data;
  },

  checkout: async (payload: CheckoutPayload): Promise<CheckoutResult> => {
    const res = await apiClient.post("/pos/checkout", payload);
    return res.data;
  },

  getShiftHistory: async (storeId: number): Promise<ShiftHistoryItem[]> => {
    const res = await apiClient.get("/pos/shift/history", { params: { storeId } });
    return res.data;
  },

  getShiftReceipts: async (shiftId: number): Promise<ShiftReceiptSummary[]> => {
    const res = await apiClient.get(`/pos/shift/${shiftId}/receipts`);
    return res.data;
  },

  getReceiptDetail: async (receiptId: number): Promise<ReceiptDetail> => {
    const res = await apiClient.get(`/pos/receipts/${receiptId}`);
    return res.data;
  },

  getStores: async (): Promise<StoreLookup[]> => {
    const res = await apiClient.get("/stores");
    return (res.data as any[]).map((s) => ({        
      storeId: s.storeId, 
      name: s.name,
      maxCartDiscountPercentage: s.maxCartDiscountPercentage,
    }));
  },

  getCheckoutQuote: async (
    payload: CheckoutQuotePayload
  ): Promise<CheckoutQuoteResult> => {
    const res = await apiClient.post("/pos/checkout/quote", payload);
    return res.data;
  },
};
