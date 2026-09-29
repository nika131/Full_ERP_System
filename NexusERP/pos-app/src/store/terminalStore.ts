import { create } from "zustand";
import { posService } from "../api/posService";
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

    if (!raw) {
      set({ isLoaded: true });
      return;
    }

    const parsed = JSON.parse(raw);
    const stores = await posService.getStores();

    const currentStore = stores.find(
      (store) => store.storeId === parsed.storeId
    );

      if (!currentStore) {
        await AsyncStorage.removeItem(STORAGE_KEY);

        set({
          storeId: null,
          storeName: null,
          isLoaded: true,
        });

        return;
      }

      set({
        storeId: currentStore.storeId,
        storeName: currentStore.name,
        maxCartDiscountPercentage:
          currentStore.maxCartDiscountPercentage ?? 0,
        isLoaded: true,
      });
    } catch {
      set({ isLoaded: true });
    }
  },

  setStore: async (
    storeId,
    storeName,
    maxCartDiscountPercentage
  ) => {
    await AsyncStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({
        storeId,
        storeName,
        maxCartDiscountPercentage,
      })
    );

    set({
      storeId,
      storeName,
      maxCartDiscountPercentage,
    });
  },

  clearStore: async () => {
    await AsyncStorage.removeItem(STORAGE_KEY);
    set({ storeId: null, storeName: null });
  },
}));
