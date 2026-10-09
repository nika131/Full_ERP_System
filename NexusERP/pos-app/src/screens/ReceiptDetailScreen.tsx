import React from "react";
import { ActivityIndicator, Pressable, ScrollView, Text, View } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { ArrowLeft } from "lucide-react-native";
import type { ReceiptsStackParamList } from "@/navigation/types";
import { useReceiptDetailQuery } from "@/hooks/queries/usePosQueries";
import { formatCurrency } from "@/utils/currency";
import { AppScreen } from "@/components/layout/AppScreen";
import { ScreenHeader } from "@/components/layout/ScreenHeader";

type Props = NativeStackScreenProps<ReceiptsStackParamList, "ReceiptDetail">;

export function ReceiptDetailScreen({ navigation, route }: Props) {
  const { receiptId } = route.params;
  const { data: receipt, isLoading, isError } = useReceiptDetailQuery(receiptId);

  const discountedSubtotal =
    receipt?.lines.reduce((total, line) => total + line.lineTotal, 0) ?? 0;
  const itemDiscountAmount = receipt
    ? receipt.subTotal - discountedSubtotal
    : 0;
  const cartDiscountAmount = receipt
    ? discountedSubtotal - receipt.finalTotal
    : 0;
  const totalDiscountAmount = receipt
    ? receipt.subTotal - receipt.finalTotal
    : 0;

  return (
    <AppScreen>
      <ScreenHeader
        title="Receipt"
        left={
          <Pressable onPress={() => navigation.goBack()} className="p-1">
            <ArrowLeft size={20} color="#334155" />
          </Pressable>
        }
        right={<View className="w-8" />}
      />

      {isLoading ? (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator color="#059669" />
        </View>
      ) : null}

      {isError ? (
        <View className="flex-1 items-center justify-center">
          <Text className="text-sm text-red-600">Failed to load receipt.</Text>
        </View>
      ) : null}

      {receipt ? (
        <ScrollView contentContainerClassName="items-center p-4">
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

            {receipt.lines.map((line, index) => (
              <View key={`${line.productName}-${index}`} className="mb-2 flex-row justify-between">
                <View className="flex-1 pr-3">
                  <Text className="text-sm font-semibold text-slate-800">
                    {line.productName}
                  </Text>
                  <View className="mt-1">
                    <Text className="text-xs text-slate-500">
                      {line.quantity} × {formatCurrency(line.unitPrice)}
                    </Text>
                    {line.marketDiscountPercentage > 0 ? (
                      <Text className="text-xs text-red-600">
                        Market discount: {line.marketDiscountPercentage}%
                      </Text>
                    ) : null}
                    {line.manualItemDiscountPercentage > 0 ? (
                      <Text className="text-xs text-red-600">
                        Manual discount: {line.manualItemDiscountPercentage}%
                      </Text>
                    ) : null}
                    {line.marketDiscountPercentage > 0 ||
                    line.manualItemDiscountPercentage > 0 ? (
                      <>
                        <Text className="text-xs text-red-600">
                          Original: {formatCurrency(line.unitPrice * line.quantity)}
                        </Text>
                        <Text className="text-xs font-semibold text-emerald-600">
                          After item discounts: {formatCurrency(line.lineTotal)}
                        </Text>
                      </>
                    ) : null}
                  </View>
                </View>

                <Text className="text-sm font-bold text-slate-800">
                  {formatCurrency(line.lineTotal)}
                </Text>
              </View>
            ))}

            <View className="my-4 h-px bg-slate-200" />

            <SummaryRow label="Original subtotal" value={receipt.subTotal} />
            {itemDiscountAmount > 0 ? (
              <SummaryRow
                label="Item discounts"
                value={-itemDiscountAmount}
                emphasize="discount"
              />
            ) : null}
            {receipt.cartDiscountPercentage > 0 ? (
              <SummaryRow
                label={`Receipt discount (${receipt.cartDiscountPercentage}%)`}
                value={-cartDiscountAmount}
                emphasize="discount"
              />
            ) : null}
            {totalDiscountAmount > 0 ? (
              <SummaryRow
                label="Total saved"
                value={-totalDiscountAmount}
                emphasize="saved"
              />
            ) : null}
            <SummaryRow label="After discounts" value={receipt.finalTotal} />
            <SummaryRow label="VAT" value={receipt.totalVatAmount} />

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
      ) : null}
    </AppScreen>
  );
}

function SummaryRow({
  label,
  value,
  emphasize,
}: {
  label: string;
  value: number;
  emphasize?: "discount" | "saved";
}) {
  const labelClass =
    emphasize === "saved"
      ? "font-semibold text-red-600"
      : "text-slate-500";
  const valueClass = emphasize
    ? "font-bold text-red-600"
    : "font-semibold text-slate-700";

  return (
    <View className="mb-1 flex-row justify-between">
      <Text className={`text-[13px] ${labelClass}`}>{label}</Text>
      <Text className={`text-[13px] ${valueClass}`}>
        {value < 0 ? "− " : ""}
        {formatCurrency(Math.abs(value))}
      </Text>
    </View>
  );
}
