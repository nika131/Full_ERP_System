import React, { useEffect, useMemo, useState } from "react";
import {View, Text, Pressable,TextInput, FlatList, Modal, Image, ActivityIndicator, useWindowDimensions, } from "react-native";
import { Pencil, Check, ChevronLeft, ChevronRight, ShoppingCart, AlertTriangle, Search, SlidersHorizontal, X, Plus, Trash2, } from "lucide-react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";

import { ProductBox } from "../components/ProductBox";
import { CartPanel } from "../components/CartPanel";
import { ProductPickerModal } from "../components/ProductPickerModal";
import { ShapeIcon } from "../components/ShapeIcon";

import { usePosLayoutStore } from "../store/posLayoutStore";
import { useCartStore } from "../store/cartStore";
import { useTerminalStore } from "../store/terminalStore";

import { useProductsByIdsQuery } from "../hooks/queries/useProductQueries";
import { useCurrentShiftQuery } from "../hooks/queries/usePosQueries";
``
import { API_BASE_URL } from "../api/client";
import { formatCurrency } from "../utils/currency";

import type { Product } from "../types";
import type { SalesStackParamList } from "../navigation/types";
import { MenuButton } from "@/components/MenuButton";

type Props = NativeStackScreenProps<SalesStackParamList, "SalesHome">;

const APPEND_SLOT = -1;

function useGridConfig() {
  const { width } = useWindowDimensions();

  return useMemo(() => {
    if (width < 600) {
      return {
        columns: 1,
        rows: 1,
        isPhone: true,
        cartWidth: 0,
      };
    }

    if (width < 1000) {
      return {
        columns: 4,
        rows: 3,
        isPhone: false,
        cartWidth: 320,
      };
    }

    return {
      columns: 5,
      rows: 5,
      isPhone: false,
      cartWidth: 360,
    };
  }, [width]);
}

function resolveImageUrl(path: string) {
  const origin = API_BASE_URL.replace(/\/api$/, "");
  return `${origin}${path}`;
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

export function SalesScreen({ navigation }: Props) {
  const grid = useGridConfig();
  const slotsPerPage = grid.columns * grid.rows;

  const { storeId } = useTerminalStore();

  const { data: shiftData, isLoading: isShiftLoading } = useCurrentShiftQuery(storeId);
  const hasOpenShift = shiftData?.hasOpenShift ?? false;

  const { slots, isLoaded, load, assignProduct, appendProduct, getProductIdAt } =
    usePosLayoutStore();

  useEffect(() => {
    if (!isLoaded) {
      load();
    }
  }, [isLoaded, load]);

  const [page, setPage] = useState(0);
  const [isEditing, setIsEditing] = useState(false);
  const [pickerSlot, setPickerSlot] = useState<number | null>(null);

  const [searchText, setSearchText] = useState("");
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);

  const [cartOpen, setCartOpen] = useState(false);

  const addProduct = useCartStore((s) => s.addProduct);
  const itemCount = useCartStore((s) => s.itemCount());

  const assignedIds = useMemo(
    () => slots.map((s) => s.productId).filter((id): id is number => id !== null),
    [slots]
  );

  const { data: hydratedProducts } = useProductsByIdsQuery(assignedIds);

  const productById = useMemo(() => {
    const map = new Map<number, Product>();
    (hydratedProducts || []).forEach((product) => {
      map.set(product.productId, product);
    });
    return map;
  }, [hydratedProducts]);

  const phoneProducts = useMemo(() => {
    const products = [...productById.values()];
    const search = searchText.trim().toLowerCase();

    return products.filter((product) => {
      const matchesSearch =
        !search ||
        product.name.toLowerCase().includes(search) ||
        String(product.productId).includes(search);

      const categoryName =
        "categoryName" in product
          ? String((product as Product & { categoryName?: string }).categoryName ?? "")
          : "";

      const matchesCategory = !selectedCategory || categoryName === selectedCategory;

      return matchesSearch && matchesCategory;
    });
  }, [productById, searchText, selectedCategory]);

  const categories = useMemo(() => {
    const values = new Set<string>();
    productById.forEach((product) => {
      if ("categoryName" in product) {
        const categoryName = (product as Product & { categoryName?: string }).categoryName;
        if (categoryName) {
          values.add(categoryName);
        }
      }
    });
    return Array.from(values).sort();
  }, [productById]);

  const pageSlotIndexes = useMemo(
    () => Array.from({ length: slotsPerPage }, (_, i) => page * slotsPerPage + i),
    [page, slotsPerPage]
  );

  const handleBoxPress = (slotIndex: number) => {
    if (isEditing) {
      setPickerSlot(slotIndex);
      return;
    }

    const productId = getProductIdAt(slotIndex);
    if (productId === null) return;

    const product = productById.get(productId);
    if (product) {
      addProduct(product);
    }
  };

  const handlePhoneProductPress = (product: Product) => {
    addProduct(product);
  };

  const handlePhoneProductDelete = (productId: number) => {
    const slotIndex = slots.findIndex(
      (slot) => slot.productId === productId
    );

    if (slotIndex !== -1) {
      assignProduct(slotIndex, null);
    }
  };

  const handleOpenAppendPicker = () => {
    setPickerSlot(APPEND_SLOT);
  };

  const handlePickerSelect = (product: Product) => {
    if (pickerSlot === APPEND_SLOT) {
      if (!assignedIds.includes(product.productId)) {
        appendProduct(product.productId);
      }
    } else if (pickerSlot !== null) {
      assignProduct(pickerSlot, product.productId);
    }
    setPickerSlot(null);
  };

  const handlePickerClearSlot = () => {
    if (pickerSlot !== null && pickerSlot !== APPEND_SLOT) {
      assignProduct(pickerSlot, null);
    }
    setPickerSlot(null);
  };


  if (isShiftLoading) {
    return (
      <View className="flex-1 items-center justify-center bg-slate-50">
        <ActivityIndicator color="#059669" />
      </View>
    );
  }

  if (!hasOpenShift) {
    return (
      <View className="flex-1 bg-slate-50">
        <View className="border-b border-slate-200 bg-white px-4 py-3">
          <MenuButton />
        </View>

        <View className="flex-1 items-center justify-center gap-2 p-6">
          <AlertTriangle size={32} color="#d97706" />

          <Text className="text-center text-base font-bold text-slate-700">
            No shift is open
          </Text>

          <Text className="text-center text-sm text-slate-500">
            Open a shift to start selling.
          </Text>

          <Pressable
            onPress={() => navigation.getParent()?.navigate("Shift")}
            className="mt-2 rounded-md bg-emerald-600 px-4 py-2.5"
          >
            <Text className="text-sm font-semibold text-white">
              Go to Shift
            </Text>
          </Pressable>
        </View>
      </View>
    );
  }

  /*
   * DESKTOP / IPAD GRID
   */
  const productGrid = (
    <View className="flex-1 p-0">
      <View className="flex-row items-center justify-between border-b border-slate-200 bg-white px-4 py-3">
        <View className="flex-row items-center gap-2">
          <MenuButton />
          <Pressable
            disabled={page === 0}
            onPress={() => setPage((current) => Math.max(0, current - 1))}
            className={`h-9 w-9 items-center justify-center rounded-md border border-slate-200 ${
              page === 0 ? "opacity-40" : ""
            }`}
          >
            <ChevronLeft size={18} color="#475569" />
          </Pressable>

          <Text className="px-2 text-sm font-semibold text-slate-600">Page {page + 1}</Text>

          <Pressable
            onPress={() => setPage((current) => current + 1)}
            className="h-9 w-9 items-center justify-center rounded-md border border-slate-200"
          >
            <ChevronRight size={18} color="#475569" />
          </Pressable>
        </View>

        <Pressable
          onPress={() => setIsEditing((current) => !current)}
          className={`flex-row items-center gap-1.5 rounded-md border px-3 py-2 ${
            isEditing ? "border-emerald-600 bg-emerald-600" : "border-slate-200 bg-white"
          }`}
        >
          {isEditing ? <Check size={16} color="white" /> : <Pencil size={16} color="#475569" />}
          <Text className={`text-sm font-semibold ${isEditing ? "text-white" : "text-slate-600"}`}>
            {isEditing ? "Done" : "Edit Layout"}
          </Text>
        </Pressable>
      </View>

      <View className="flex-1">
        {Array.from({ length: grid.rows }, (_, rowIndex) => (
          <View key={rowIndex} className="flex-1 flex-row">
            {pageSlotIndexes
              .slice(rowIndex * grid.columns, rowIndex * grid.columns + grid.columns)
              .map((slotIndex) => {
                const productId = getProductIdAt(slotIndex);
                const product = productId !== null ? productById.get(productId) ?? null : null;

                return (
                  <ProductBox
                    key={slotIndex}
                    product={product}
                    isEditing={isEditing}
                    onPress={() => handleBoxPress(slotIndex)}
                    onDelete={() => assignProduct(slotIndex, null)}
                  />
                );
              })}
          </View>
        ))}
      </View>
    </View>
  );

  /*
   * PHONE PRODUCT LIST
   */
  const phoneProductList = (
    <View className="flex-1 bg-slate-50">
      {/* Search + Add */}
      <View className="border-b border-slate-200 bg-white px-3 pb-2 pt-3">
        <View className="flex-row items-center gap-2">
           <MenuButton />
          <View className="h-11 flex-1 flex-row items-center rounded-lg border border-slate-200 bg-slate-50 px-3">
            <Search size={18} color="#64748b" />

            <TextInput
              value={searchText}
              onChangeText={setSearchText}
              placeholder="Search product or ID..."
              placeholderTextColor="#94a3b8"
              className="ml-2 flex-1 text-sm text-slate-700"
            />
            {searchText.length > 0 && (
              <Pressable onPress={() => setSearchText("")}>
                <X size={18} color="#64748b" />
              </Pressable>
            )}
          </View>

          <Pressable
            onPress={() => setIsEditing((current) => !current)}
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

        {/* Categories */}
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
              const active = item === "All" ? selectedCategory === null : selectedCategory === item;
              return (
                <Pressable
                  onPress={() => setSelectedCategory(item === "All" ? null : item)}
                  className={`rounded-full px-3 py-1.5 ${active ? "bg-emerald-600" : "bg-slate-100"}`}
                >
                  <Text className={`text-xs font-semibold ${active ? "text-white" : "text-slate-600"}`}>
                    {item}
                  </Text>
                </Pressable>
              );
            }}
          />
        </View>
      </View>

      {/* Product list */}
      <FlatList
        data={phoneProducts}
        keyExtractor={(item) => String(item.productId)}
        contentContainerClassName="pb-28"
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
              {item.marketDiscountRate > 0 && (
                <Text className="mt-0.5 text-xs font-bold text-red-600">
                  Market discount: -{item.marketDiscountRate}%
                </Text>
              )}
              <Text className="mt-0.5 text-xs text-slate-400">ID: {item.productId}</Text>
            </View>

            {isEditing ? (
              <Pressable
                onPress={() => handlePhoneProductDelete(item.productId)}
                hitSlop={8}
                className="ml-2 h-12 w-12 items-center justify-center rounded-full bg-red-50"
              >
                <Trash2
                  size={22}
                  color="#dc2626"
                  strokeWidth={2.5}
                />
              </Pressable>
            ) : (
              <Pressable
                onPress={() => handlePhoneProductPress(item)}
                hitSlop={8}
                className="ml-2 h-12 w-12 items-center justify-center rounded-full bg-emerald-600 active:bg-emerald-700"
              >
                <Plus size={24} color="white" strokeWidth={2.5} />
              </Pressable>
            )}
          </View>
        )}
        ListEmptyComponent={
          <View className="items-center justify-center px-6 py-16 gap-2">
            <Text className="text-sm text-slate-400">No products on your list yet.</Text>
            <Pressable
              onPress={handleOpenAppendPicker}
              className="mt-2 flex-row items-center gap-1.5 rounded-md bg-emerald-600 px-4 py-2"
            >
              <Plus size={16} color="white" />
              <Text className="text-sm font-semibold text-white">Add a product</Text>
            </Pressable>
          </View>
        }
      />

      {/* Floating cart */}
      <Pressable
        onPress={() => setCartOpen(true)}
        className="absolute bottom-5 right-5 h-14 w-14 items-center justify-center rounded-full bg-emerald-600 shadow-lg"
      >
        <ShoppingCart size={23} color="white" />
        {itemCount > 0 && (
          <View className="absolute -right-1 -top-1 min-w-5 items-center justify-center rounded-full bg-red-500 px-1.5 py-0.5">
            <Text className="text-[10px] font-bold text-white">{itemCount}</Text>
          </View>
        )}
      </Pressable>

      {isEditing && (
        <Pressable
          onPress={handleOpenAppendPicker}
          hitSlop={8}
          className="absolute bottom-5 left-5 h-14 w-14 items-center justify-center rounded-full bg-emerald-600 shadow-lg"
        >
          <Plus size={24} color="white" strokeWidth={2.5} />
        </Pressable>
      )}
          </View>
        );

  /*
   * PHONE CART OVERLAY
   */
  const phoneCartOverlay = (
    <Modal visible={cartOpen} transparent animationType="slide" onRequestClose={() => setCartOpen(false)}>
      <View className="flex-1 justify-end bg-black/40">
        <View className="h-[85%] overflow-hidden rounded-t-2xl bg-white">
          <View className="flex-row items-center justify-between border-b border-slate-200 px-4 py-3">
            <Text className="text-base font-bold text-slate-700">Cart</Text>
            <Pressable
              onPress={() => setCartOpen(false)}
              className="h-9 w-9 items-center justify-center rounded-full bg-slate-100"
            >
              <X size={18} color="#475569" />
            </Pressable>
          </View>

          <View className="flex-1">
            <CartPanel
              onCheckout={() => {
                setCartOpen(false);
                navigation.navigate("Checkout");
              }}
            />
          </View>
        </View>
      </View>
    </Modal>
  );

  const sharedPickerModal = (
    <ProductPickerModal
      visible={pickerSlot !== null}
      onClose={() => setPickerSlot(null)}
      onSelect={handlePickerSelect}
      onClearSlot={handlePickerClearSlot}
      hasExistingProduct={
        pickerSlot !== null && pickerSlot !== APPEND_SLOT && getProductIdAt(pickerSlot) !== null
      }
      excludeIds={assignedIds}
    />
  );

  /*
   * PHONE
   */
  if (grid.isPhone) {
    return (
      <View className="flex-1 bg-slate-50">
        {phoneProductList}
        {phoneCartOverlay}
        {sharedPickerModal}
      </View>
    );
  }

  /*
   * IPAD / DESKTOP
   */
  return (
    <View className="flex-1 bg-slate-50">
      <View className="flex-1 flex-row">
        <View className="flex-1">{productGrid}</View>

        <View style={{ width: grid.cartWidth }} className="border-l border-slate-200 bg-white">
          <CartPanel onCheckout={() => navigation.navigate("Checkout")} />
        </View>
      </View>

      {sharedPickerModal}
    </View>
  );
}
