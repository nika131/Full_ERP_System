import React, { useCallback, useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Alert, Pressable, Text, useWindowDimensions, View } from "react-native";
import { AlertTriangle } from "lucide-react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import type { SalesStackParamList } from "@/navigation/types";
import type { Product } from "@/types";
import { usePosLayoutStore } from "@/store/posLayoutStore";
import { useCartStore } from "@/store/cartStore";
import { useTerminalStore } from "@/store/terminalStore";
import { useCurrentShiftQuery } from "@/hooks/queries/usePosQueries";
import { MenuButton } from "@/components/MenuButton";
import { AppScreen } from "@/components/layout/AppScreen";
import { CartPanel } from "@/features/cart/CartPanel";
import { DesktopProductGrid } from "@/features/sales/DesktopProductGrid";
import { PhoneProductList } from "@/features/sales/PhoneProductList";
import { PhoneCartModal } from "@/features/sales/PhoneCartModal";
import { ProductPickerModal } from "@/features/sales/ProductPickerModal";
import { getErrorMessage } from "@/api/client";
import { useFocusEffect } from "@react-navigation/native";

type Props = NativeStackScreenProps<SalesStackParamList, "SalesHome">;

const APPEND_SLOT = -1;

export function SalesScreen({ navigation }: Props) {
  const grid = useSalesGridConfig();
  const slotsPerPage = grid.columns * grid.rows;
  const { storeId } = useTerminalStore();

  const { data: shiftData, isLoading: isShiftLoading } =
    useCurrentShiftQuery(storeId);
  const hasOpenShift = shiftData?.hasOpenShift ?? false;

  const {
    slots,
    products,
    isLoaded,
    loadedStoreId,
    loadError,
    load,
    assignProduct,
    appendProduct,
    getProductIdAt,
  } = usePosLayoutStore();


  useFocusEffect(
    useCallback(() => {
      if (storeId != null && hasOpenShift) {
        void load(storeId);
      }
    }, [storeId, hasOpenShift, load])
  );

  const isLayoutLoading = hasOpenShift && storeId != null &&
  (
    !isLoaded ||
    loadedStoreId !== storeId
  );

  const [page, setPage] = useState(0);

  useEffect(() => {
    setPage(0);
  }, [storeId]);

  const [isEditing, setIsEditing] = useState(false);
  const [pickerSlot, setPickerSlot] = useState<number | null>(null);
  const [searchText, setSearchText] = useState("");
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [cartOpen, setCartOpen] = useState(false);

  const addProduct = useCartStore((state) => state.addProduct);
  const itemCount = useCartStore((state) => state.itemCount());

  const assignedIds = useMemo(
    () =>
      slots
        .map((slot) => slot.productId)
        .filter((id): id is number => id !== null),
    [slots]
  );

  const productById = useMemo(() => {
    const map = new Map<number, Product>();

    products.forEach((product) => {
      map.set(product.productId, product);
    });

    return map;
  }, [products]);

  const orderedSlots = useMemo(
      () =>
        [...slots]
          .filter((slot) => slot.productId !== null)
          .sort((a, b) => a.slotIndex - b.slotIndex),
      [slots]
    );

    const orderedProducts = useMemo(
      () =>
        orderedSlots
          .map((slot) =>
            slot.productId !== null
              ? productById.get(slot.productId)
              : undefined
          )
          .filter((product): product is Product => product !== undefined),
      [orderedSlots, productById]
    );

  const phoneProducts = useMemo(() => {
    const search = searchText.trim().toLowerCase();

    return orderedProducts.filter((product) => {
      const matchesSearch =
        !search ||
        product.name.toLowerCase().includes(search) ||
        String(product.productId).includes(search);
      
        const matchesCategory =
        !selectedCategory || product.categoryName === selectedCategory;

      return matchesSearch && matchesCategory;
    });
  }, [searchText, selectedCategory, orderedProducts]);

  const categories = useMemo(
    () =>
      Array.from(
        new Set(
          [...productById.values()]
            .map((product) => product.categoryName)
            .filter(Boolean)
        )
      ).sort(),
    [productById]
  );

  const pageSlotIndexes = useMemo(
    () =>
      Array.from(
        { length: slotsPerPage },
        (_, index) => page * slotsPerPage + index
      ),
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
    if (product) addProduct(product);
  };

  const handlePhoneProductDelete = async (productId: number) => {
      const slot = slots.find((slot) =>
          slot.productId === productId
      );

      if (!slot) return;

      try {
        await assignProduct(slot.slotIndex, null);
      } catch (err) {
        Alert.alert("Layout not saved", getErrorMessage(err));
      }
  };

  const handleDeleteSlot = async (slotIndex: number) => {
      try {
        await assignProduct(slotIndex, null);
      } catch (err) {
        Alert.alert("Layout not saved", getErrorMessage(err));
    }
  };

  const handlePickerSelect = async (product: Product) => {
    const targetSlot = pickerSlot;

    setPickerSlot(null);

    try {
      if (targetSlot === APPEND_SLOT) {
        if (!assignedIds.includes(product.productId)) {
          await appendProduct(product.productId);
        }
      } else if (targetSlot !== null) {
        await assignProduct(targetSlot, product.productId);
      }
    }
    catch (err) {
      Alert.alert("Layout not saved", getErrorMessage(err));
    };
  };

  const handlePickerClearSlot = async () => {
    const targetSlot = pickerSlot;

    setPickerSlot(null);

    if (
      targetSlot === null ||
      targetSlot === APPEND_SLOT
    ) {
      return;
    }

    try {
      await assignProduct(
        targetSlot,
        null
      );
    } catch (err) {
      Alert.alert(
        "Layout not saved",
        getErrorMessage(err)
      );
    }
  };

  const sharedPickerModal = (
    <ProductPickerModal
      visible={pickerSlot !== null}
      onClose={() => setPickerSlot(null)}
      onSelect={handlePickerSelect}
      onClearSlot={handlePickerClearSlot}
      hasExistingProduct={
        pickerSlot !== null &&
        pickerSlot !== APPEND_SLOT &&
        getProductIdAt(pickerSlot) !== null
      }
      excludeIds={assignedIds}
    />
  );

  if (isShiftLoading || isLayoutLoading) {
    return (
      <AppScreen className="flex-1 items-center justify-center bg-slate-50">
        <ActivityIndicator color="#059669" />
      </AppScreen>
    );
  }

  if (!hasOpenShift) {
    return (
      <AppScreen>
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
            <Text className="text-sm font-semibold text-white">Go to Shift</Text>
          </Pressable>
        </View>
      </AppScreen>
    );
  }

  if (loadError) {
    return (
      <AppScreen>
        <View className="flex-1 items-center justify-center gap-3 p-6">
          <Text className="text-base font-bold text-slate-700">
            Could not load POS layout
          </Text>

          <Text className="text-center text-sm text-slate-500">
            {loadError}
          </Text>

          <Pressable
            className="rounded-lg bg-emerald-600 px-5 py-3"
            onPress={() => {
              if (storeId != null) {
                void load(storeId);
              }
            }}
          >
            <Text className="font-semibold text-white">
              Retry
            </Text>
          </Pressable>
        </View>
      </AppScreen>
    );
  }

  if (grid.isPhone) {
    return (
      <AppScreen edges={["top"]}>
        <PhoneProductList
          products={phoneProducts}
          categories={categories}
          searchText={searchText}
          selectedCategory={selectedCategory}
          isEditing={isEditing}
          itemCount={itemCount}
          onSearchTextChange={setSearchText}
          onSelectedCategoryChange={setSelectedCategory}
          onToggleEditing={() => setIsEditing((current) => !current)}
          onProductPress={addProduct}
          onProductDelete={handlePhoneProductDelete}
          onAddProduct={() => setPickerSlot(APPEND_SLOT)}
          onOpenCart={() => setCartOpen(true)}
        />

        <PhoneCartModal
          visible={cartOpen}
          onClose={() => setCartOpen(false)}
          onCheckout={() => {
            setCartOpen(false);
            navigation.navigate("Checkout");
          }}
        />

        {sharedPickerModal}
      </AppScreen>
    );
  }

  return (
    <AppScreen edges={["top"]}>
      <View className="flex-1 flex-row">
        <View className="flex-1">
          <DesktopProductGrid
            columns={grid.columns}
            rows={grid.rows}
            page={page}
            pageSlotIndexes={pageSlotIndexes}
            isEditing={isEditing}
            productById={productById}
            getProductIdAt={getProductIdAt}
            onPreviousPage={() => setPage((current) => Math.max(0, current - 1))}
            onNextPage={() => setPage((current) => current + 1)}
            onToggleEditing={() => setIsEditing((current) => !current)}
            onBoxPress={handleBoxPress}
            onDeleteSlot={(slotIndex) => { void handleDeleteSlot(slotIndex);}}
          />
        </View>

        <View
          style={{ width: grid.cartWidth }}
          className="border-l border-slate-200 bg-white"
        >
          <CartPanel onCheckout={() => navigation.navigate("Checkout")} />
        </View>
      </View>

      {sharedPickerModal}
    </AppScreen>
  );
}

function useSalesGridConfig() {
  const { width } = useWindowDimensions();

  return useMemo(() => {
    if (width < 900) {
      return {
        columns: 1,
        rows: 1,
        isPhone: true,
        cartWidth: 0,
      };
    }

    return {
      columns: 5,
      rows: 5,
      isPhone: false,
      cartWidth:
        width < 1200
          ? 300
          : 360,
    };
  }, [width]);
}
