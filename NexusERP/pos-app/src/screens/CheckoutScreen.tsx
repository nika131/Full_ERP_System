import React, { useMemo, useState } from "react";
import { ActivityIndicator, FlatList, Pressable, Text, View } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { ArrowLeft, CheckCircle2 } from "lucide-react-native";
import type { SalesStackParamList } from "@/navigation/types";
import { useCartStore } from "@/store/cartStore";
import { useTerminalStore } from "@/store/terminalStore";
import { useCheckoutMutation, useCheckoutQuoteQuery } from "@/hooks/queries/usePosQueries";
import { getErrorMessage } from "@/api/client";
import { formatCurrency, round2 } from "@/utils/currency";
import { AppScreen } from "@/components/layout/AppScreen";
import { ScreenHeader } from "@/components/layout/ScreenHeader";
import { CheckoutItemRow } from "@/features/checkout/CheckoutItemRow";
import {
  CheckoutPaymentPanel,
  type CheckoutPaymentMethod,
} from "@/features/checkout/CheckoutPaymentPanel";

type Props = NativeStackScreenProps<SalesStackParamList, "Checkout">;

export function CheckoutScreen({ navigation }: Props) {
  const items = useCartStore((state) => state.items);
  const cartDiscountPercentage = useCartStore(
    (state) => state.cartDiscountPercentage
  );
  const subtotal = useCartStore((state) => state.subtotal());
  const clearCart = useCartStore((state) => state.clear);
  const { storeId } = useTerminalStore();

  const [paymentMethod, setPaymentMethod] =
    useState<CheckoutPaymentMethod>("Cash");
  const [amountTendered, setAmountTendered] = useState("");
  const [successReceipt, setSuccessReceipt] = useState<string | null>(null);
  const [checkoutError, setCheckoutError] = useState<string | null>(null);

  const checkoutMutation = useCheckoutMutation();

  const quotePayload = useMemo(
    () =>
      storeId == null || items.length === 0
        ? null
        : {
            storeId,
            cartDiscountPercentage,
            items: items.map((item) => ({
              productId: item.productId,
              quantity: item.quantity,
              manualItemDiscountPercentage:
                item.manualItemDiscountPercentage,
            })),
          },
    [storeId, cartDiscountPercentage, items]
  );

  const {
    data: checkoutQuote,
    isLoading: isQuoteLoading,
    isError: isQuoteError,
    error: quoteError,
  } = useCheckoutQuoteQuery(quotePayload);

  const backendTotal = checkoutQuote?.finalTotal ?? null;
  const tendered = parseFloat(amountTendered) || 0;
  const change = useMemo(
    () =>
      backendTotal == null
        ? 0
        : round2(Math.max(0, tendered - backendTotal)),
    [tendered, backendTotal]
  );

  const hasCashAmount = amountTendered.trim().length > 0;
  const cashPaymentValid =
    paymentMethod === "Card" ||
    (hasCashAmount && backendTotal != null && tendered >= backendTotal);

  const canConfirm =
    items.length > 0 &&
    storeId != null &&
    backendTotal != null &&
    !isQuoteLoading &&
    !isQuoteError &&
    cashPaymentValid &&
    !checkoutMutation.isPending;

  const handleConfirm = async () => {
    if (!storeId) return;

    setCheckoutError(null);

    try {
      const result = await checkoutMutation.mutateAsync({
        storeId,
        cartDiscountPercentage,
        paymentMethod,
        items: items.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
          manualItemDiscountPercentage:
            item.manualItemDiscountPercentage,
        })),
      });

      setSuccessReceipt(result.receiptNumber);
      clearCart();
    } catch (err) {
      setCheckoutError(getErrorMessage(err));
    }
  };

  if (successReceipt) {
    return (
      <AppScreen className="flex-1 items-center justify-center gap-2 bg-white">
        <CheckCircle2 size={64} color="#059669" />
        <Text className="mt-3 text-[22px] font-extrabold text-slate-800">
          Sale complete
        </Text>
        <Text className="mb-6 text-sm text-slate-500">
          Receipt #{successReceipt}
        </Text>
        <Pressable
          className="rounded-xl bg-emerald-600 px-8 py-3.5"
          onPress={() => navigation.navigate("SalesHome")}
        >
          <Text className="text-[15px] font-bold text-white">New Sale</Text>
        </Pressable>
      </AppScreen>
    );
  }

  return (
    <AppScreen edges={["top"]}>
      <ScreenHeader
        title="Checkout"
        left={
          <Pressable onPress={() => navigation.goBack()} className="p-1">
            <ArrowLeft size={20} color="#334155" />
          </Pressable>
        }
        right={<View className="w-8" />}
      />

      <FlatList
        data={items}
        keyExtractor={(item) => String(item.productId)}
        className="flex-1"
        contentContainerClassName="p-4"
        renderItem={({ item }) => <CheckoutItemRow item={item} />}
        ListFooterComponent={
          <View className="mt-4 gap-1 border-t border-slate-200 pt-4">
            <View className="flex-row justify-between">
              <Text className="text-[13px] text-slate-500">Subtotal</Text>
              <Text className="text-[13px] font-semibold text-slate-700">
                {formatCurrency(subtotal)}
              </Text>
            </View>

            {cartDiscountPercentage > 0 ? (
              <View className="flex-row justify-between">
                <Text className="text-[13px] text-slate-500">
                  Receipt discount
                </Text>
                <Text className="text-[13px] font-semibold text-red-600">
                  − {cartDiscountPercentage}%
                </Text>
              </View>
            ) : null}

            <View className="mt-2 flex-row justify-between">
              <Text className="text-[17px] font-extrabold text-slate-800">
                Total
              </Text>
              <Text className="text-[22px] font-extrabold text-emerald-600">
                {backendTotal != null ? formatCurrency(backendTotal) : "-"}
              </Text>
            </View>
          </View>
        }
      />

      <CheckoutPaymentPanel
        paymentMethod={paymentMethod}
        backendTotal={backendTotal}
        amountTendered={amountTendered}
        change={change}
        checkoutError={checkoutError}
        quoteErrorMessage={isQuoteError ? getErrorMessage(quoteError) : null}
        isQuoteLoading={isQuoteLoading}
        canConfirm={canConfirm}
        isSubmitting={checkoutMutation.isPending}
        onPaymentMethodChange={setPaymentMethod}
        onAmountTenderedChange={setAmountTendered}
        onConfirm={handleConfirm}
      />
    </AppScreen>
  );
}
