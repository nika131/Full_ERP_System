import { create } from "zustand";

import type { CartItem, Product } from "@/types";
import { calculateCartTotals } from "@/utils/cartPricing";

interface CartState {
  items: CartItem[];
  cartDiscountPercentage: number;
  addProduct: (product: Product) => void;
  incrementQuantity: (productId: number, by?: number) => void;
  decrementQuantity: (productId: number) => void;
  setQuantity: (productId: number, quantity: number) => void;
  setItemDiscount: (productId: number, discountPercentage: number) => void;
  removeItem: (productId: number) => void;
  setCartDiscount: (discountPercentage: number) => void;
  clear: () => void;
  subtotal: () => number;
  totalDiscount: () => number;
  total: () => number;
  itemCount: () => number;
}

export const useCartStore = create<CartState>((set, get) => ({
  items: [],
  cartDiscountPercentage: 0,

  addProduct: (product) =>
    set((state) => {
      const existing = state.items.find(
        (item) => item.productId === product.productId
      );

      if (existing) {
        return {
          items: state.items.map((item) =>
            item.productId === product.productId
              ? { ...item, quantity: item.quantity + 1 }
              : item
          ),
        };
      }

      const newItem: CartItem = {
        productId: product.productId,
        name: product.name,
        unitPrice: product.price,
        quantity: 1,
        marketDiscountPercentage: product.marketDiscountRate ?? 0,
        manualItemDiscountPercentage: 0,
        maxDiscountPercentage: product.maxDiscountPercentage,
      };

      return { items: [...state.items, newItem] };
    }),

  incrementQuantity: (productId, by = 1) =>
    set((state) => ({
      items: state.items.map((item) =>
        item.productId === productId
          ? { ...item, quantity: item.quantity + by }
          : item
      ),
    })),

  decrementQuantity: (productId) =>
    set((state) => ({
      items: state.items
        .map((item) =>
          item.productId === productId
            ? { ...item, quantity: item.quantity - 1 }
            : item
        )
        .filter((item) => item.quantity > 0),
    })),

  setQuantity: (productId, quantity) =>
    set((state) => ({
      items:
        quantity <= 0
          ? state.items.filter((item) => item.productId !== productId)
          : state.items.map((item) =>
              item.productId === productId ? { ...item, quantity } : item
            ),
    })),

  setItemDiscount: (productId, discountPercentage) =>
    set((state) => ({
      items: state.items.map((item) =>
        item.productId === productId
          ? {
              ...item,
              manualItemDiscountPercentage: discountPercentage,
            }
          : item
      ),
    })),

  removeItem: (productId) =>
    set((state) => ({
      items: state.items.filter((item) => item.productId !== productId),
    })),

  setCartDiscount: (discountPercentage) =>
    set({ cartDiscountPercentage: discountPercentage }),

  clear: () =>
    set({
      items: [],
      cartDiscountPercentage: 0,
    }),

  subtotal: () =>
    calculateCartTotals(
      get().items,
      get().cartDiscountPercentage
    ).subtotal,

  totalDiscount: () =>
    calculateCartTotals(
      get().items,
      get().cartDiscountPercentage
    ).totalDiscount,

  total: () =>
    calculateCartTotals(
      get().items,
      get().cartDiscountPercentage
    ).total,

  itemCount: () =>
    get().items.reduce((sum, item) => sum + item.quantity, 0),
}));
