import React from "react";
import { FlatList, Image, Pressable, Text, TextInput, View } from "react-native";
import { Check, Pencil, Plus, Search, ShoppingCart, SlidersHorizontal, Trash2, X } from "lucide-react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { API_BASE_URL } from "@/api/client";
import { MenuButton } from "@/components/MenuButton";
import { ShapeIcon } from "@/components/ShapeIcon";
import type { Product } from "@/types";
import { formatCurrency } from "@/utils/currency";

interface PhoneProductListProps {
  products: Product[];
  categories: string[];
  searchText: string;
  selectedCategory: string | null;
  isEditing: boolean;
  itemCount: number;
  onSearchTextChange: (value: string) => void;
  onSelectedCategoryChange: (value: string | null) => void;
  onToggleEditing: () => void;
  onProductPress: (product: Product) => void;
  onProductDelete: (productId: number) => void;
  onAddProduct: () => void;
  onOpenCart: () => void;
}

export function PhoneProductList({
  products,
  categories,
  searchText,
  selectedCategory,
  isEditing,
  itemCount,
  onSearchTextChange,
  onSelectedCategoryChange,
  onToggleEditing,
  onProductPress,
  onProductDelete,
  onAddProduct,
  onOpenCart,
}: PhoneProductListProps) {
  const insets = useSafeAreaInsets();
  const floatingBottom = Math.max(insets.bottom, 12) + 8;

  return (
    <View className="flex-1 bg-slate-50">
      <View className="border-b border-slate-200 bg-white px-3 pb-2 pt-3">
        <View className="flex-row items-center gap-2">
          <MenuButton />

          <View className="h-11 flex-1 flex-row items-center rounded-lg border border-slate-200 bg-slate-50 px-3">
            <Search size={18} color="#64748b" />
            <TextInput
              value={searchText}
              onChangeText={onSearchTextChange}
              placeholder="Search product or ID..."
              placeholderTextColor="#94a3b8"
              className="ml-2 flex-1 text-sm text-slate-700"
            />
            {searchText.length > 0 ? (
              <Pressable onPress={() => onSearchTextChange("")}>
                <X size={18} color="#64748b" />
              </Pressable>
            ) : null}
          </View>

          <Pressable
            onPress={onToggleEditing}
            className={`h-11 flex-row items-center gap-1.5 rounded-lg border px-3 ${
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

        <View className="mt-2 flex-row items-center">
          <SlidersHorizontal size={16} color="#64748b" />
          <FlatList
            horizontal
            showsHorizontalScrollIndicator={false}
            className="ml-2"
            contentContainerClassName="gap-2"
            data={["All", ...categories]}
            keyExtractor={(item) => item}
            renderItem={({ item }) => {
              const active =
                item === "All"
                  ? selectedCategory === null
                  : selectedCategory === item;

              return (
                <Pressable
                  onPress={() =>
                    onSelectedCategoryChange(item === "All" ? null : item)
                  }
                  className={`rounded-full px-3 py-1.5 ${
                    active ? "bg-emerald-600" : "bg-slate-100"
                  }`}
                >
                  <Text
                    className={`text-xs font-semibold ${
                      active ? "text-white" : "text-slate-600"
                    }`}
                  >
                    {item}
                  </Text>
                </Pressable>
              );
            }}
          />
        </View>
      </View>

      <FlatList
        data={products}
        keyExtractor={(item) => String(item.productId)}
        contentContainerStyle={{ paddingBottom: floatingBottom + 76 }}
        renderItem={({ item }) => (
          <View className="flex-row items-center border-b border-slate-200 bg-white px-4 py-3">
            <PhoneProductPreview product={item} />

            <View className="ml-3 flex-1">
              <Text className="text-base font-semibold text-slate-700" numberOfLines={2}>
                {item.name}
              </Text>
              <Text className="mt-1 text-sm font-bold text-emerald-600">
                {formatCurrency(item.price)}
              </Text>
              {item.marketDiscountRate > 0 ? (
                <Text className="mt-0.5 text-xs font-bold text-red-600">
                  Market discount: -{item.marketDiscountRate}%
                </Text>
              ) : null}
              <Text className="mt-0.5 text-xs text-slate-400">
                ID: {item.productId}
              </Text>
            </View>

            {isEditing ? (
              <Pressable
                onPress={() => onProductDelete(item.productId)}
                hitSlop={8}
                className="ml-2 h-12 w-12 items-center justify-center rounded-full bg-red-50"
              >
                <Trash2 size={22} color="#dc2626" strokeWidth={2.5} />
              </Pressable>
            ) : (
              <Pressable
                onPress={() => onProductPress(item)}
                hitSlop={8}
                className="ml-2 h-12 w-12 items-center justify-center rounded-full bg-emerald-600 active:bg-emerald-700"
              >
                <Plus size={24} color="white" strokeWidth={2.5} />
              </Pressable>
            )}
          </View>
        )}
        ListEmptyComponent={
          <View className="items-center justify-center gap-2 px-6 py-16">
            <Text className="text-sm text-slate-400">
              No products on your list yet.
            </Text>
            <Pressable
              onPress={onAddProduct}
              className="mt-2 flex-row items-center gap-1.5 rounded-md bg-emerald-600 px-4 py-2"
            >
              <Plus size={16} color="white" />
              <Text className="text-sm font-semibold text-white">
                Add a product
              </Text>
            </Pressable>
          </View>
        }
      />

      <Pressable
        onPress={onOpenCart}
        className="absolute right-5 h-14 w-14 items-center justify-center rounded-full bg-emerald-600 shadow-lg"
        style={{ bottom: floatingBottom }}
      >
        <ShoppingCart size={23} color="white" />
        {itemCount > 0 ? (
          <View className="absolute -right-1 -top-1 min-w-5 items-center justify-center rounded-full bg-red-500 px-1.5 py-0.5">
            <Text className="text-[10px] font-bold text-white">{itemCount}</Text>
          </View>
        ) : null}
      </Pressable>

      {isEditing ? (
        <Pressable
          onPress={onAddProduct}
          hitSlop={8}
          className="absolute left-5 h-14 w-14 items-center justify-center rounded-full bg-emerald-600 shadow-lg"
          style={{ bottom: floatingBottom }}
        >
          <Plus size={24} color="white" strokeWidth={2.5} />
        </Pressable>
      ) : null}
    </View>
  );
}

function PhoneProductPreview({ product }: { product: Product }) {
  const isImage = product.displayMode === "Image" && !!product.imageUrl;
  const isShape = product.displayMode === "Shape" && !!product.shapeType;

  return (
    <View className="h-16 w-16 overflow-hidden rounded-full bg-slate-100">
      {isImage ? (
        <Image
          source={{ uri: resolveImageUrl(product.imageUrl!) }}
          className="h-full w-full"
          resizeMode="cover"
        />
      ) : isShape ? (
        <View className="h-full w-full items-center justify-center">
          <ShapeIcon
            shapeType={product.shapeType!}
            color={product.shapeColor || "#ffffff"}
            text={product.shapeText}
            size={52}
          />
        </View>
      ) : (
        <View className="h-full w-full items-center justify-center bg-white">
          <Text
            className="px-1 text-center text-[10px] font-semibold text-slate-600"
            numberOfLines={3}
          >
            {product.name}
          </Text>
        </View>
      )}
    </View>
  );
}

function resolveImageUrl(path: string) {
  const origin = API_BASE_URL.replace(/\/api$/, "");
  return `${origin}${path}`;
}
