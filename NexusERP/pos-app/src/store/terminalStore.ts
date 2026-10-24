import { create } from "zustand";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { posService } from "../api/posService";

interface TerminalState {
  companyId: number | null;
  loadedCompanyId: number | null;
  storeId: number | null;
  storeName: string | null;
  maxCartDiscountPercentage: number;
  isLoaded: boolean;
  load: (companyId: number) => Promise<void>;
  setStore: (storeId: number, storeName: string, maxCartDiscountPercentage: number) => Promise<void>;
  clearStore: () => Promise<void>;
  reset: () => void;
}
const storageKey = (companyId: number) => `pos_terminal_store_v2_${companyId}`;
const blank = { companyId: null, loadedCompanyId: null, storeId: null, storeName: null, maxCartDiscountPercentage: 0, isLoaded: false };

export const useTerminalStore = create<TerminalState>((set, get) => ({
  ...blank,
  reset: () => set(blank),
  load: async (companyId) => {
    set({ ...blank, companyId });
    try {
      const [raw, stores] = await Promise.all([AsyncStorage.getItem(storageKey(companyId)), posService.getStores()]);
      if (get().companyId !== companyId) return;
      const previousId = raw ? Number(JSON.parse(raw).storeId) : 0;
      const store = stores.find(s => s.storeId === previousId);
      set({ companyId, loadedCompanyId: companyId, storeId: store?.storeId ?? null,
        storeName: store?.name ?? null, maxCartDiscountPercentage: store?.maxCartDiscountPercentage ?? 0, isLoaded: true });
      if (previousId && !store) await AsyncStorage.removeItem(storageKey(companyId));
    } catch {
      if (get().companyId === companyId) set({ companyId, loadedCompanyId: companyId, isLoaded: true });
    }
  },
  setStore: async (storeId, storeName, maxCartDiscountPercentage) => {
    const companyId = get().companyId;
    if (!companyId) throw new Error("Select a company before choosing a store.");
    await AsyncStorage.setItem(storageKey(companyId), JSON.stringify({ storeId }));
    set({ storeId, storeName, maxCartDiscountPercentage });
  },
  clearStore: async () => {
    const companyId = get().companyId;
    if (companyId) await AsyncStorage.removeItem(storageKey(companyId));
    set({ storeId: null, storeName: null, maxCartDiscountPercentage: 0 });
  },
}));
