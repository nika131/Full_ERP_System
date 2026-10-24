
import { create } from "zustand";

import { posService } from "@/api/posService";
import { getErrorMessage } from "@/api/client";
import type { PosSlot, Product } from "@/types";

const sortSlots = (slots: PosSlot[]) =>
  [...slots].sort((a, b) => a.slotIndex - b.slotIndex);

interface PosLayoutState {
  slots: PosSlot[];
  products: Product[];

  isLoaded: boolean;
  loadedStoreId: number | null;
  loadError: string | null;

  reset: () => void;
  load: (storeId: number) => Promise<void>;

  assignProduct: (
    slotIndex: number,
    productId: number | null
  ) => Promise<void>;

  appendProduct: (productId: number) => Promise<void>;

  getProductIdAt: (slotIndex: number) => number | null;
}

export const usePosLayoutStore = create<PosLayoutState>(
  (set, get) => ({
    slots: [],
    products: [],

    isLoaded: false,
    loadedStoreId: null,
    loadError: null,

    reset: () => {
      set({
        slots: [],
        products: [],
        isLoaded: false,
        loadedStoreId: null,
        loadError: null,
      });
    },

    load: async (storeId) => {
      set({
        slots: [],
        products: [],
        isLoaded: false,
        loadedStoreId: storeId,
        loadError: null,
      });

      try {
        const layout = await posService.getPosLayout(storeId);

        // Ignore a response if the store changed or
        // the layout was reset while loading.
        if (get().loadedStoreId !== storeId) {
          return;
        }

        set({
          slots: sortSlots(layout.slots),
          products: layout.products,
          isLoaded: true,
          loadError: null,
        });
      } catch (err) {
        if (get().loadedStoreId !== storeId) {
          return;
        }

        set({
          slots: [],
          products: [],
          isLoaded: true,
          loadError: getErrorMessage(err),
        });
      }
    },

    assignProduct: async (slotIndex, productId) => {
      const state = get();
      const storeId = state.loadedStoreId;

      if (storeId === null) {
        return;
      }

      const withoutCurrentSlot = state.slots.filter(
        (slot) => slot.slotIndex !== slotIndex
      );

      const next =
        productId === null
          ? sortSlots(withoutCurrentSlot)
          : sortSlots([
              ...withoutCurrentSlot,
              {
                slotIndex,
                productId,
              },
            ]);

      const saved = await posService.savePosLayout(
        storeId,
        next
      );

      // Ignore results from a previous store.
      if (get().loadedStoreId !== storeId) {
        return;
      }

      set({
        slots: sortSlots(saved.slots),
        products: saved.products,
        loadError: null,
      });
    },

    appendProduct: async (productId) => {
      const state = get();
      const storeId = state.loadedStoreId;

      if (storeId === null) {
        return;
      }

      if (
        state.slots.some(
          (slot) => slot.productId === productId
        )
      ) {
        return;
      }

      const occupiedIndexes = new Set(
        state.slots.map((slot) => slot.slotIndex)
      );

      let slotIndex = 0;

      while (occupiedIndexes.has(slotIndex)) {
        slotIndex++;
      }

      const next = sortSlots([
        ...state.slots,
        {
          slotIndex,
          productId,
        },
      ]);

      const saved = await posService.savePosLayout(
        storeId,
        next
      );

      if (get().loadedStoreId !== storeId) {
        return;
      }

      set({
        slots: sortSlots(saved.slots),
        products: saved.products,
        loadError: null,
      });
    },

    getProductIdAt: (slotIndex) =>
      get().slots.find(
        (slot) => slot.slotIndex === slotIndex
      )?.productId ?? null,
  })
);
