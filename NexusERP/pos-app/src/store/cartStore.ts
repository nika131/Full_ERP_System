import { create } from "zustand";
import type { CartItem, Product } from "../types";
import { round2 } from "../utils/currency";

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
        (i) => i.productId === product.productId
      );

      if (existing) {
        return {
          items: state.items.map((i) =>
            i.productId === product.productId
              ? { ...i, quantity: i.quantity + 1 }
              : i
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
      items: state.items.map((i) =>
        i.productId === productId
          ? { ...i, quantity: i.quantity + by }
          : i
      ),
    })),

  decrementQuantity: (productId) =>
    set((state) => ({
      items: state.items
        .map((i) =>
          i.productId === productId
            ? { ...i, quantity: i.quantity - 1 }
            : i
        )
        .filter((i) => i.quantity > 0),
    })),

  setQuantity: (productId, quantity) =>
    set((state) => ({
      items:
        quantity <= 0
          ? state.items.filter((i) => i.productId !== productId)
          : state.items.map((i) =>
              i.productId === productId ? { ...i, quantity } : i
            ),
    })),

  setItemDiscount: (productId, discountPercentage) =>
    set((state) => ({
      items: state.items.map((i) =>
        i.productId === productId
          ? {
              ...i,
              manualItemDiscountPercentage: discountPercentage,
            }
          : i
      ),
    })),

  removeItem: (productId) =>
    set((state) => ({
      items: state.items.filter((i) => i.productId !== productId),
    })),

  setCartDiscount: (discountPercentage) =>
    set({ cartDiscountPercentage: discountPercentage }),

  clear: () =>
    set({
      items: [],
      cartDiscountPercentage: 0,
    }),

  subtotal: () =>
    round2(
      get().items.reduce(
        (sum, i) => sum + i.unitPrice * i.quantity,
        0
      )
    ),

  totalDiscount: () => {
    const itemDiscount = get().items.reduce(
      (sum, i) =>
        sum +
        round2(
          i.unitPrice *
            i.quantity *
            ((i.manualItemDiscountPercentage + i.marketDiscountPercentage)  / 100)
        ),
      0
    );

    const subtotalAfterItemDiscounts = round2(
      get().subtotal() - itemDiscount
    );

    const cartDiscount = round2(
      subtotalAfterItemDiscounts *
        (get().cartDiscountPercentage / 100)
    );

    return round2(itemDiscount + cartDiscount);
  },

  total: () =>
    round2(get().subtotal() - get().totalDiscount()),

  itemCount: () =>
    get().items.reduce((sum, i) => sum + i.quantity, 0),
}));