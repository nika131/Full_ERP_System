import React from "react";
import { Pressable, Text, View } from "react-native";
import { Check, ChevronLeft, ChevronRight, Pencil } from "lucide-react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import type { Product } from "@/types";
import { MenuButton } from "@/components/MenuButton";
import { ProductBox } from "./ProductBox";

interface DesktopProductGridProps {
  columns: number;
  rows: number;
  page: number;
  pageSlotIndexes: number[];
  isEditing: boolean;
  productById: Map<number, Product>;
  getProductIdAt: (slotIndex: number) => number | null;
  onPreviousPage: () => void;
  onNextPage: () => void;
  onToggleEditing: () => void;
  onBoxPress: (slotIndex: number) => void;
  onDeleteSlot: (slotIndex: number) => void;
}

export function DesktopProductGrid({
  columns,
  rows,
  page,
  pageSlotIndexes,
  isEditing,
  productById,
  getProductIdAt,
  onPreviousPage,
  onNextPage,
  onToggleEditing,
  onBoxPress,
  onDeleteSlot,
}: DesktopProductGridProps) {
  const insets = useSafeAreaInsets();

  return (
    <View className="flex-1" style={{ paddingBottom: insets.bottom }}>
      <View className="flex-row items-center justify-between border-b border-slate-200 bg-white px-4 py-3">
        <View className="flex-row items-center gap-2">
          <MenuButton />
          <Pressable
            disabled={page === 0}
            onPress={onPreviousPage}
            className={`h-9 w-9 items-center justify-center rounded-md border border-slate-200 ${
              page === 0 ? "opacity-40" : ""
            }`}
          >
            <ChevronLeft size={18} color="#475569" />
          </Pressable>

          <Text className="px-2 text-sm font-semibold text-slate-600">
            Page {page + 1}
          </Text>

          <Pressable
            onPress={onNextPage}
            className="h-9 w-9 items-center justify-center rounded-md border border-slate-200"
          >
            <ChevronRight size={18} color="#475569" />
          </Pressable>
        </View>

        <Pressable
          onPress={onToggleEditing}
          className={`flex-row items-center gap-1.5 rounded-md border px-3 py-2 ${
            isEditing
              ? "border-emerald-600 bg-emerald-600"
              : "border-slate-200 bg-white"
          }`}
        >
          {isEditing ? (
            <Check size={16} color="white" />
          ) : (
            <Pencil size={16} color="#475569" />
          )}
          <Text
            className={`text-sm font-semibold ${
              isEditing ? "text-white" : "text-slate-600"
            }`}
          >
            {isEditing ? "Done" : "Edit Layout"}
          </Text>
        </Pressable>
      </View>

      <View className="flex-1">
        {Array.from({ length: rows }, (_, rowIndex) => (
          <View key={rowIndex} className="flex-1 flex-row">
            {pageSlotIndexes
              .slice(rowIndex * columns, rowIndex * columns + columns)
              .map((slotIndex) => {
                const productId = getProductIdAt(slotIndex);
                const product =
                  productId !== null ? productById.get(productId) ?? null : null;

                return (
                  <ProductBox
                    key={slotIndex}
                    product={product}
                    isEditing={isEditing}
                    onPress={() => onBoxPress(slotIndex)}
                    onDelete={() => onDeleteSlot(slotIndex)}
                  />
                );
              })}
          </View>
        ))}
      </View>
    </View>
  );
}
