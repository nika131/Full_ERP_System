import { create } from "zustand";
import AsyncStorage from "@react-native-async-storage/async-storage";
import type { PosSlot } from "../types";

const STORAGE_KEY = "pos_grid_layout_v1";

interface PosLayoutState {
  slots: PosSlot[];
  isLoaded: boolean;
  load: () => Promise<void>;
  assignProduct: (slotIndex: number, productId: number | null) => void;
  appendProduct: (productId: number) => void; // NEW
  getProductIdAt: (slotIndex: number) => number | null;
}

export const usePosLayoutStore = create<PosLayoutState>((set, get) => ({
  slots: [],
  isLoaded: false,

  load: async () => {
    try {
      const raw = await AsyncStorage.getItem(STORAGE_KEY);
      const slots: PosSlot[] = raw ? JSON.parse(raw) : [];
      set({ slots, isLoaded: true });
    } catch {
      set({ slots: [], isLoaded: true });
    }
  },

  assignProduct: (slotIndex, productId) => {
    set((state) => {
      const others = state.slots.filter((s) => s.slotIndex !== slotIndex);
      const next = [...others, { slotIndex, productId }];
      AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(next)).catch(() => {});
      return { slots: next };
    });
  },

  appendProduct: (productId) => {
    set((state) => {
      const maxIndex = state.slots.length
        ? Math.max(...state.slots.map((s) => s.slotIndex))
        : -1;
      const next = [...state.slots, { slotIndex: maxIndex + 1, productId }];
      AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(next)).catch(() => {});
      return { slots: next };
    });
  },

  getProductIdAt: (slotIndex) => {
    return get().slots.find((s) => s.slotIndex === slotIndex)?.productId ?? null;
  },
}));