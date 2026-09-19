import React, { useMemo, useState } from "react";
import { View, Text, Pressable, TextInput, FlatList, ActivityIndicator, Alert } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { ArrowLeft, CreditCard, Banknote, CheckCircle2 } from "lucide-react-native";
import { useCartStore } from "../store/cartStore";
import { useTerminalStore } from "../store/terminalStore";
import { useCheckoutMutation } from "../hooks/queries/usePosQueries";
import { getErrorMessage } from "../api/client";
import { formatCurrency, round2 } from "../utils/currency";
import type { SalesStackParamList } from "../navigation/types";

type Props = NativeStackScreenProps<SalesStackParamList, "Checkout">;

type PaymentMethod = "Cash" | "Card";

export function CheckoutScreen({ navigation }: Props) {
  const items = useCartStore((s) => s.items);
  const cartDiscountAmount = useCartStore((s) => s.cartDiscountAmount);
  const subtotal = useCartStore((s) => s.subtotal());
  const total = useCartStore((s) => s.total());
  const clearCart = useCartStore((s) => s.clear);
  const { storeId } = useTerminalStore();

  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>("Cash");
  const [amountTendered, setAmountTendered] = useState("");
  const [successReceipt, setSuccessReceipt] = useState<string | null>(null);
  const [checkoutError, setCheckoutError] = useState<string | null>(null);

  const checkoutMutation = useCheckoutMutation();

  const tendered = parseFloat(amountTendered) || 0;

  const change = useMemo(
    () => round2(Math.max(0, tendered - total)),
    [tendered, total]
  );

  const hasCashAmount = amountTendered.trim().length > 0;

  const cashPaymentValid =
    paymentMethod === "Card" ||
    !hasCashAmount ||
    tendered >= total;

  const canConfirm =
    items.length > 0 &&
    storeId != null &&
    cashPaymentValid &&
    !checkoutMutation.isPending;

  const handleConfirm = async () => {
    if (!storeId) return;

    setCheckoutError(null);

    try {
      const result = await checkoutMutation.mutateAsync({
        storeId,
        cartDiscountAmount,
        paymentMethod,
        items: items.map((i) => ({
          productId: i.productId,
          quantity: i.quantity,
          manualItemDiscount: i.manualItemDiscount,
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
      <View className="flex-1 items-center justify-center gap-2 bg-white">
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
          <Text className="text-[15px] font-bold text-white">
            New Sale
          </Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-slate-50">
      <View className="flex-row items-center justify-between border-b border-slate-200 bg-white p-4">
        <Pressable onPress={() => navigation.goBack()} className="p-1">
          <ArrowLeft size={20} color="#334155" />
        </Pressable>

        <Text className="text-base font-bold text-slate-800">
          Checkout
        </Text>

        <View className="w-8" />
      </View>

      <FlatList
        data={items}
        keyExtractor={(i) => String(i.productId)}
        className="flex-1"
        contentContainerClassName="p-4"
        renderItem={({ item }) => {
          const lineTotal = round2(
            item.unitPrice * item.quantity - item.manualItemDiscount
          );

          return (
            <View className="flex-row justify-between border-b border-slate-100 py-2">
              <View className="flex-1">
                <Text
                  className="text-sm font-semibold text-slate-800"
                  numberOfLines={1}
                >
                  {item.name}
                </Text>

                <Text className="mt-0.5 text-xs text-slate-500">
                  {item.quantity} × {formatCurrency(item.unitPrice)}
                </Text>
              </View>

              <Text className="text-sm font-bold text-slate-800">
                {formatCurrency(lineTotal)}
              </Text>
            </View>
          );
        }}
        ListFooterComponent={
          <View className="mt-4 gap-1 border-t border-slate-200 pt-4">
            <View className="flex-row justify-between">
              <Text className="text-[13px] text-slate-500">
                Subtotal
              </Text>

              <Text className="text-[13px] font-semibold text-slate-700">
                {formatCurrency(subtotal)}
              </Text>
            </View>

            {cartDiscountAmount > 0 && (
              <View className="flex-row justify-between">
                <Text className="text-[13px] text-slate-500">
                  Receipt discount
                </Text>

                <Text className="text-[13px] font-semibold text-red-600">
                  − {formatCurrency(cartDiscountAmount)}
                </Text>
              </View>
            )}

            <View className="mt-2 flex-row justify-between">
              <Text className="text-[17px] font-extrabold text-slate-800">
                Total
              </Text>

              <Text className="text-[22px] font-extrabold text-emerald-600">
                {formatCurrency(total)}
              </Text>
            </View>
          </View>
        }
      />

      <View className="gap-3 border-t border-slate-200 bg-white p-4 shadow-md">
        <View className="flex-row gap-2">
          <Pressable
            className={`flex-1 flex-row items-center justify-center gap-1.5 rounded-lg py-3 ${
              paymentMethod === "Cash" ? "bg-emerald-600" : "bg-slate-100"
            }`}
            onPress={() => setPaymentMethod("Cash")}
          >
            <Banknote
              size={16}
              color={paymentMethod === "Cash" ? "#ffffff" : "#475569"}
            />

            <Text
              className={`text-sm font-semibold ${
                paymentMethod === "Cash" ? "text-white" : "text-slate-600"
              }`}
            >
              Cash
            </Text>
          </Pressable>

          <Pressable
            className={`flex-1 flex-row items-center justify-center gap-1.5 rounded-lg py-3 ${
              paymentMethod === "Card" ? "bg-emerald-600" : "bg-slate-100"
            }`}
            onPress={() => setPaymentMethod("Card")}
          >
            <CreditCard
              size={16}
              color={paymentMethod === "Card" ? "#ffffff" : "#475569"}
            />

            <Text
              className={`text-sm font-semibold ${
                paymentMethod === "Card" ? "text-white" : "text-slate-600"
              }`}
            >
              Card
            </Text>
          </Pressable>
        </View>

        {paymentMethod === "Cash" && (
          <View className="flex-row gap-3">
            <View className="flex-1">
              <Text className="mb-1.5 text-[11px] font-semibold text-slate-500">
                Amount received
              </Text>

              <TextInput
                value={amountTendered}
                onChangeText={setAmountTendered}
                keyboardType="decimal-pad"
                placeholder={formatCurrency(total)}
                className="rounded-lg border border-slate-300 px-3 py-2.5 text-base font-semibold"
              />
            </View>

            <View className="flex-1 items-end">
              <Text className="mb-1.5 text-[11px] font-semibold text-slate-500">
                Change due
              </Text>

              <Text className="text-[22px] font-extrabold text-emerald-600">
                {formatCurrency(change)}
              </Text>
            </View>
          </View>
        )}

        {checkoutError && (
          <View className="rounded-xl border border-red-200 bg-red-50 p-3">
            <Text className="text-sm font-semibold text-red-700">
              {checkoutError}
            </Text>
          </View>
        )}

        <Pressable
          className={`items-center rounded-xl py-4 ${
            canConfirm ? "bg-emerald-600" : "bg-slate-300"
          }`}
          disabled={!canConfirm}
          onPress={handleConfirm}
        >
          {checkoutMutation.isPending ? (
            <ActivityIndicator color="#ffffff" />
          ) : (
            <Text className="text-base font-bold text-white">
              Confirm {formatCurrency(total)}
            </Text>
          )}
        </Pressable>
      </View>
    </View>
  );
}