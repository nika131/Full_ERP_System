import { create } from "zustand";
import AsyncStorage from "@react-native-async-storage/async-storage";

const STORAGE_KEY = "pos_terminal_store_id";

interface TerminalState {
  storeId: number | null;
  storeName: string | null;
  maxCartDiscountPercentage: number;
  isLoaded: boolean;
  load: () => Promise<void>;
  setStore: (
    storeId: number,
    storeName: string,
    maxCartDiscountPercentage: number
  ) => Promise<void>;
  clearStore: () => Promise<void>;
}

export const useTerminalStore = create<TerminalState>((set) => ({
  storeId: null,
  storeName: null,
  maxCartDiscountPercentage: 0,
  isLoaded: false,

  load: async () => {
    try {
      const raw = await AsyncStorage.getItem(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw);
        set({ storeId: parsed.storeId, storeName: parsed.storeName, isLoaded: true });
      } else {
        set({ isLoaded: true });
      }
    } catch {
      set({ isLoaded: true });
    }
  },

  setStore: async (storeId, storeName) => {
    await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify({ storeId, storeName }));
    set({ storeId, storeName });
  },

  clearStore: async () => {
    await AsyncStorage.removeItem(STORAGE_KEY);
    set({ storeId: null, storeName: null });
  },
}));
