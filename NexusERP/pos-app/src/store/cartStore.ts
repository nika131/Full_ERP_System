import { create } from "zustand";
import type { CartItem, Product } from "../types";
import { round2 } from "../utils/currency";

interface CartState {
  items: CartItem[];
  cartDiscountAmount: number;
  addProduct: (product: Product) => void;
  incrementQuantity: (productId: number, by?: number) => void;
  decrementQuantity: (productId: number) => void;
  setQuantity: (productId: number, quantity: number) => void;
  setItemDiscount: (productId: number, discount: number) => void;
  removeItem: (productId: number) => void;
  setCartDiscount: (amount: number) => void;
  clear: () => void;
  subtotal: () => number;
  totalDiscount: () => number;
  total: () => number;
  itemCount: () => number;
}

export const useCartStore = create<CartState>((set, get) => ({
  items: [],
  cartDiscountAmount: 0,

  addProduct: (product) =>
    set((state) => {
      const existing = state.items.find((i) => i.productId === product.productId);
      if (existing) {
        return {
          items: state.items.map((i) =>
            i.productId === product.productId ? { ...i, quantity: i.quantity + 1 } : i
          ),
        };
      }
      const newItem: CartItem = {
        productId: product.productId,
        name: product.name,
        unitPrice: product.price,
        quantity: 1,
        manualItemDiscount: 0,
        maxDiscountPercentage: product.maxDiscountPercentage,
      };
      return { items: [...state.items, newItem] };
    }),

  incrementQuantity: (productId, by = 1) =>
    set((state) => ({
      items: state.items.map((i) =>
        i.productId === productId ? { ...i, quantity: i.quantity + by } : i
      ),
    })),

  decrementQuantity: (productId) =>
    set((state) => ({
      items: state.items
        .map((i) => (i.productId === productId ? { ...i, quantity: i.quantity - 1 } : i))
        .filter((i) => i.quantity > 0),
    })),

  setQuantity: (productId, quantity) =>
    set((state) => ({
      items:
        quantity <= 0
          ? state.items.filter((i) => i.productId !== productId)
          : state.items.map((i) => (i.productId === productId ? { ...i, quantity } : i)),
    })),

  setItemDiscount: (productId, discount) =>
    set((state) => ({
      items: state.items.map((i) =>
        i.productId === productId ? { ...i, manualItemDiscount: discount } : i
      ),
    })),

  removeItem: (productId) =>
    set((state) => ({ items: state.items.filter((i) => i.productId !== productId) })),

  setCartDiscount: (amount) => set({ cartDiscountAmount: amount }),

  clear: () => set({ items: [], cartDiscountAmount: 0 }),

  subtotal: () => round2(get().items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0)),

  totalDiscount: () =>
    round2(
      get().items.reduce((sum, i) => sum + i.manualItemDiscount, 0) + get().cartDiscountAmount
    ),

  total: () => round2(get().subtotal() - get().totalDiscount()),

  itemCount: () => get().items.reduce((sum, i) => sum + i.quantity, 0),
}));
