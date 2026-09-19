import React, { useState } from "react";
import { View, Text, TextInput, FlatList, Pressable, ActivityIndicator } from "react-native";
import { Search, X } from "lucide-react-native";
import { ModalSheet } from "./ModalSheet";
import { useProductSearchQuery } from "../hooks/queries/useProductQueries";
import { formatCurrency } from "../utils/currency";
import type { Product } from "../types";

interface ProductPickerModalProps {
  visible: boolean;
  onClose: () => void;
  onSelect: (product: Product) => void;
  onClearSlot: () => void;
  hasExistingProduct: boolean;
  excludeIds?: number[];
}

export function ProductPickerModal({
  visible,
  onClose,
  onSelect,
  onClearSlot,
  hasExistingProduct,
  excludeIds = [],
}: ProductPickerModalProps) {
  const [search, setSearch] = useState("");
  // The backend now excludes these before paging, so what comes back is
  // already a correctly-filled page — no filtering needed here anymore.
  const { data: results, isFetching } = useProductSearchQuery(search, excludeIds);

  return (
    <ModalSheet visible={visible} onClose={onClose} maxWidth={520}>
      <View className="mb-4 flex-row items-center justify-between">
        <Text className="text-base font-bold text-slate-800">Choose a product</Text>

        <Pressable onPress={onClose}>
          <X size={20} color="#64748b" />
        </Pressable>
      </View>

      <View className="flex-row items-center gap-2 rounded-lg border border-slate-300 px-3 py-2.5">
        <Search size={16} color="#94a3b8" />

        <TextInput
          value={search}
          onChangeText={setSearch}
          placeholder="Search by name or ID..."
          className="flex-1 text-[15px]"
          autoFocus
        />
      </View>

      {hasExistingProduct && (
        <Pressable className="mt-3 items-center rounded-lg bg-red-50 py-2.5" onPress={onClearSlot}>
          <Text className="text-[13px] font-semibold text-red-600">Clear this box</Text>
        </Pressable>
      )}

      <View className="mt-3">
        {isFetching ? (
          <ActivityIndicator color="#059669" className="mt-6" />
        ) : (
          <FlatList
            data={results || []}
            keyExtractor={(p) => String(p.productId)}
            className="max-h-[340px]"
            ListEmptyComponent={
              <Text className="py-6 text-center text-slate-400">
                {search.trim().length === 0 ? "No products available" : "No products found"}
              </Text>
            }
            renderItem={({ item }) => (
              <Pressable
                className="flex-row items-center border-b border-slate-100 py-3"
                onPress={() => onSelect(item)}
              >
                <View className="flex-1">
                  <Text className="text-sm font-semibold text-slate-800" numberOfLines={1}>
                    {item.name}
                  </Text>

                  <Text className="mt-0.5 text-xs text-slate-500">{item.categoryName}</Text>
                </View>

                <Text className="text-sm font-bold text-emerald-600">
                  {formatCurrency(item.price)}
                </Text>
              </Pressable>
            )}
          />
        )}
      </View>
    </ModalSheet>
  );
}