import { create } from "zustand";
import AsyncStorage from "@react-native-async-storage/async-storage";
import type { PosSlot } from "../types";

const getStorageKey = (storeId: number) =>
  `pos_grid_layout_v2_store_${storeId}`;

const sortSlots = (slots: PosSlot[]) =>
  [...slots].sort((a, b) => a.slotIndex - b.slotIndex);

interface PosLayoutState {
  slots: PosSlot[];
  isLoaded: boolean;
  loadedStoreId: number | null;

  load: (storeId: number) => Promise<void>;

  assignProduct: (
    slotIndex: number,
    productId: number | null
  ) => void;

  appendProduct: (productId: number) => void;

  getProductIdAt: (
    slotIndex: number
  ) => number | null;
}

export const usePosLayoutStore =
  create<PosLayoutState>((set, get) => ({
    slots: [],
    isLoaded: false,
    loadedStoreId: null,

    load: async (storeId) => {
      set({
        slots: [],
        isLoaded: false,
        loadedStoreId: storeId,
      });

      try {
        const raw = await AsyncStorage.getItem(
          getStorageKey(storeId)
        );

        const slots: PosSlot[] = raw
          ? JSON.parse(raw)
          : [];

        if (get().loadedStoreId !== storeId) {
          return;
        }

        set({
          slots: sortSlots(slots),
          isLoaded: true,
        });
      } catch {
        if (get().loadedStoreId !== storeId) {
          return;
        }

        set({
          slots: [],
          isLoaded: true,
        });
      }
    },

    assignProduct: (slotIndex, productId) => {
      set((state) => {
        const storeId = state.loadedStoreId;

        if (storeId === null) {
          return state;
        }

        const withoutCurrentSlot =
          state.slots.filter(
            (slot) =>
              slot.slotIndex !== slotIndex
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

        AsyncStorage.setItem(
          getStorageKey(storeId),
          JSON.stringify(next)
        ).catch(() => {});

        return {
          slots: next,
        };
      });
    },

    appendProduct: (productId) => {
      set((state) => {
        const storeId = state.loadedStoreId;

        if (storeId === null) {
          return state;
        }

        if (
          state.slots.some(
            (slot) =>
              slot.productId === productId
          )
        ) {
          return state;
        }

        const occupiedIndexes = new Set(
          state.slots.map(
            (slot) => slot.slotIndex
          )
        );

        let slotIndex = 0;

        while (
          occupiedIndexes.has(slotIndex)
        ) {
          slotIndex++;
        }

        const next = sortSlots([
          ...state.slots,
          {
            slotIndex,
            productId,
          },
        ]);

        AsyncStorage.setItem(
          getStorageKey(storeId),
          JSON.stringify(next)
        ).catch(() => {});

        return {
          slots: next,
        };
      });
    },

    getProductIdAt: (slotIndex) =>
      get().slots.find(
        (slot) =>
          slot.slotIndex === slotIndex
      )?.productId ?? null,
  }));