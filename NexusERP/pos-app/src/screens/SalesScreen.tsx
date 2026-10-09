import React, { useEffect, useMemo, useState } from "react";
import { ActivityIndicator, Pressable, Text, useWindowDimensions, View } from "react-native";
import { AlertTriangle } from "lucide-react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import type { SalesStackParamList } from "@/navigation/types";
import type { Product } from "@/types";
import { usePosLayoutStore } from "@/store/posLayoutStore";
import { useCartStore } from "@/store/cartStore";
import { useTerminalStore } from "@/store/terminalStore";
import { useProductsByIdsQuery } from "@/hooks/queries/useProductQueries";
import { useCurrentShiftQuery } from "@/hooks/queries/usePosQueries";
import { MenuButton } from "@/components/MenuButton";
import { AppScreen } from "@/components/layout/AppScreen";
import { CartPanel } from "@/features/cart/CartPanel";
import { DesktopProductGrid } from "@/features/sales/DesktopProductGrid";
import { PhoneProductList } from "@/features/sales/PhoneProductList";
import { PhoneCartModal } from "@/features/sales/PhoneCartModal";
import { ProductPickerModal } from "@/features/sales/ProductPickerModal";

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
    isLoaded,
    load,
    assignProduct,
    appendProduct,
    getProductIdAt,
  } = usePosLayoutStore();

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

  const addProduct = useCartStore((state) => state.addProduct);
  const itemCount = useCartStore((state) => state.itemCount());

  const assignedIds = useMemo(
    () =>
      slots
        .map((slot) => slot.productId)
        .filter((id): id is number => id !== null),
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
      const matchesCategory =
        !selectedCategory || product.categoryName === selectedCategory;

      return matchesSearch && matchesCategory;
    });
  }, [productById, searchText, selectedCategory]);

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

  const handlePhoneProductDelete = (productId: number) => {
    const slotIndex = slots.findIndex((slot) => slot.productId === productId);
    if (slotIndex !== -1) assignProduct(slotIndex, null);
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

  if (isShiftLoading) {
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
            onDeleteSlot={(slotIndex) => assignProduct(slotIndex, null)}
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
