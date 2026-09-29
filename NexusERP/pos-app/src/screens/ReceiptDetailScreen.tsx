import React from "react";
import { View, Text, ScrollView, Pressable, ActivityIndicator } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { ArrowLeft } from "lucide-react-native";
import { useReceiptDetailQuery } from "../hooks/queries/usePosQueries";
import { formatCurrency } from "../utils/currency";
import type { ReceiptsStackParamList } from "../navigation/types";

type Props = NativeStackScreenProps<ReceiptsStackParamList, "ReceiptDetail">;

export function ReceiptDetailScreen({ navigation, route }: Props) {
  const { receiptId } = route.params;
  const { data: receipt, isLoading, isError } = useReceiptDetailQuery(receiptId);

  const itemDiscountAmount =
    receipt?.lines.reduce(
      (total, line) =>
        total +
        line.unitPrice *
          line.quantity *
          ((line.marketDiscountPercentage +
            line.manualItemDiscountPercentage) /
            100),
      0
    ) ?? 0;

  const discountedSubtotal =
    receipt ? receipt.subTotal - itemDiscountAmount : 0;

  const cartDiscountAmount =
    receipt
      ? discountedSubtotal * (receipt.cartDiscountPercentage / 100)
      : 0;

  const totalDiscountAmount =
    itemDiscountAmount + cartDiscountAmount;

  return (
    <View className="flex-1 bg-slate-50">
      <View className="flex-row items-center justify-between border-b border-slate-200 bg-white p-4">
        <Pressable onPress={() => navigation.goBack()} className="p-1">
          <ArrowLeft size={20} color="#334155" />
        </Pressable>

        <Text className="text-base font-bold text-slate-800">Receipt</Text>

        <View className="w-8" />
      </View>

      {isLoading && (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator color="#059669" />
        </View>
      )}

      {isError && (
        <View className="flex-1 items-center justify-center">
          <Text className="text-sm text-red-600">Failed to load receipt.</Text>
        </View>
      )}

      {receipt && (
        <ScrollView
          contentContainerClassName="items-center p-4"
        >
          <View className="w-full max-w-[420px] rounded-2xl bg-white p-6 shadow-md">
            <Text className="text-center text-lg font-extrabold text-slate-800">
              {receipt.storeName}
            </Text>

            <Text className="mt-0.5 text-center text-[13px] text-slate-500">
              #{receipt.receiptNumber}
            </Text>

            <Text className="mt-1 text-center text-xs text-slate-400">
              {new Date(receipt.createdAt).toLocaleString()} · {receipt.cashierName}
            </Text>

            <View className="my-4 h-px bg-slate-200" />

            {receipt.lines.map((line, idx) => (
              <View key={idx} className="mb-2 flex-row justify-between">
                <View className="flex-1">
                  <Text className="text-sm font-semibold text-slate-800">
                    {line.productName}
                  </Text>

                  <View className="mt-1">
                    <Text className="text-xs text-slate-500">
                      {line.quantity} × {formatCurrency(line.unitPrice)}
                    </Text>

                    {line.marketDiscountPercentage > 0 && (
                      <Text className="text-xs text-red-600">
                        Market discount: {line.marketDiscountPercentage}%
                      </Text>
                    )}

                    {line.manualItemDiscountPercentage > 0 && (
                      <Text className="text-xs text-red-600">
                        Manual discount: {line.manualItemDiscountPercentage}%
                      </Text>
                    )}

                    {(line.marketDiscountPercentage > 0 ||
                      line.manualItemDiscountPercentage > 0) && (
                      <>
                        <Text className="text-xs text-red-600">
                          Original:{" "}
                          {formatCurrency(line.unitPrice * line.quantity)}
                        </Text>

                        <Text className="text-xs font-semibold text-emerald-600">
                          After item discounts: {formatCurrency(line.lineTotal)}
                        </Text>
                      </>
                    )}
                  </View>

                </View>

                <Text className="text-sm font-bold text-slate-800">
                  {formatCurrency(line.lineTotal)}
                </Text>
              </View>
            ))}

            <View className="my-4 h-px bg-slate-200" />

            <View className="mb-1 flex-row justify-between">
              <Text className="text-[13px] text-slate-500">
                Original subtotal
              </Text>

              <Text className="text-[13px] font-semibold text-slate-700">
                {formatCurrency(receipt.subTotal)}
              </Text>
            </View>

            {itemDiscountAmount > 0 && (
              <View className="mb-1 flex-row justify-between">
                <Text className="text-[13px] text-slate-500">
                  Item discounts
                </Text>

                <Text className="text-[13px] font-semibold text-red-600">
                  − {formatCurrency(itemDiscountAmount)}
                </Text>
              </View>
            )}

            {receipt.cartDiscountPercentage > 0 && (
              <View className="mb-1 flex-row justify-between">
                <Text className="text-[13px] text-slate-500">
                  Receipt discount ({receipt.cartDiscountPercentage}%)
                </Text>

                <Text className="text-[13px] font-semibold text-red-600">
                  − {formatCurrency(cartDiscountAmount)}
                </Text>
              </View>
            )}

            {totalDiscountAmount > 0 && (
              <View className="mb-1 flex-row justify-between">
                <Text className="text-[13px] font-semibold text-red-600">
                  Total saved
                </Text>

                <Text className="text-[13px] font-bold text-red-600">
                  − {formatCurrency(totalDiscountAmount)}
                </Text>
              </View>
            )}

            <View className="mb-1 flex-row justify-between">
              <Text className="text-[13px] text-slate-500">
                After discounts
              </Text>

              <Text className="text-[13px] font-semibold text-slate-700">
                {formatCurrency(discountedSubtotal - cartDiscountAmount)}
              </Text>
            </View>

            <View className="mb-1 flex-row justify-between">
              <Text className="text-[13px] text-slate-500">
                VAT
              </Text>

              <Text className="text-[13px] font-semibold text-slate-700">
                {formatCurrency(receipt.totalVatAmount)}
              </Text>
            </View>

            <View className="my-4 h-px bg-slate-200" />

            <View className="flex-row justify-between">
              <Text className="text-base font-extrabold text-slate-800">
                Final total
              </Text>

              <Text className="text-xl font-extrabold text-emerald-600">
                {formatCurrency(receipt.finalTotal)}
              </Text>
            </View>

            <View className="mt-4 self-center rounded-full bg-emerald-50 px-4 py-1.5">
              <Text className="text-xs font-bold text-emerald-700">
                Paid by {receipt.paymentMethod}
              </Text>
            </View>
          </View>
        </ScrollView>
      )}
    </View>
  );
}