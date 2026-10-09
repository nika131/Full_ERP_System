import type { CartItem } from "@/types";
import { round2 } from "@/utils/currency";

export interface CartLinePricing {
  baseAmount: number;
  marketDiscountAmount: number;
  afterMarketDiscount: number;
  manualDiscountAmount: number;
  afterManualDiscount: number;
}

export interface CartPricingTotals {
  subtotal: number;
  marketDiscountTotal: number;
  manualItemDiscountTotal: number;
  subtotalAfterItemDiscounts: number;
  receiptDiscountAmount: number;
  totalDiscount: number;
  total: number;
}

export function calculateCartLine(item: CartItem): CartLinePricing {
  const baseAmount = item.unitPrice * item.quantity;
  const marketDiscountAmount =
    baseAmount * (item.marketDiscountPercentage / 100);
  const afterMarketDiscount = baseAmount - marketDiscountAmount;
  const manualDiscountAmount =
    afterMarketDiscount * (item.manualItemDiscountPercentage / 100);
  const afterManualDiscount = afterMarketDiscount - manualDiscountAmount;

  return {
    baseAmount,
    marketDiscountAmount,
    afterMarketDiscount,
    manualDiscountAmount,
    afterManualDiscount,
  };
}

export function calculateCartTotals(
  items: CartItem[],
  cartDiscountPercentage: number
): CartPricingTotals {
  let subtotal = 0;
  let marketDiscountTotal = 0;
  let manualItemDiscountTotal = 0;
  let subtotalAfterItemDiscounts = 0;

  for (const item of items) {
    const line = calculateCartLine(item);
    subtotal += line.baseAmount;
    marketDiscountTotal += line.marketDiscountAmount;
    manualItemDiscountTotal += line.manualDiscountAmount;
    subtotalAfterItemDiscounts += line.afterManualDiscount;
  }

  const receiptDiscountAmount =
    subtotalAfterItemDiscounts * (cartDiscountPercentage / 100);
  const total = subtotalAfterItemDiscounts - receiptDiscountAmount;
  const totalDiscount = subtotal - total;

  return {
    subtotal: round2(subtotal),
    marketDiscountTotal: round2(marketDiscountTotal),
    manualItemDiscountTotal: round2(manualItemDiscountTotal),
    subtotalAfterItemDiscounts: round2(subtotalAfterItemDiscounts),
    receiptDiscountAmount: round2(receiptDiscountAmount),
    totalDiscount: round2(totalDiscount),
    total: round2(total),
  };
}
