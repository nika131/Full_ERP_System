import React from "react";
import { ActivityIndicator, Pressable, Text, TextInput, View } from "react-native";
import { Banknote, CreditCard } from "lucide-react-native";

import { BottomActionBar } from "@/components/layout/AppScreen";
import { formatCurrency } from "@/utils/currency";

export type CheckoutPaymentMethod = "Cash" | "Card";

interface CheckoutPaymentPanelProps {
  paymentMethod: CheckoutPaymentMethod;
  backendTotal: number | null;
  amountTendered: string;
  change: number;
  checkoutError: string | null;
  quoteErrorMessage: string | null;
  isQuoteLoading: boolean;
  canConfirm: boolean;
  isSubmitting: boolean;
  onPaymentMethodChange: (method: CheckoutPaymentMethod) => void;
  onAmountTenderedChange: (value: string) => void;
  onConfirm: () => void;
}

export function CheckoutPaymentPanel({
  paymentMethod,
  backendTotal,
  amountTendered,
  change,
  checkoutError,
  quoteErrorMessage,
  isQuoteLoading,
  canConfirm,
  isSubmitting,
  onPaymentMethodChange,
  onAmountTenderedChange,
  onConfirm,
}: CheckoutPaymentPanelProps) {
  return (
    <BottomActionBar>
      <View className="flex-row gap-2">
        <PaymentMethodButton
          active={paymentMethod === "Cash"}
          label="Cash"
          icon="cash"
          onPress={() => onPaymentMethodChange("Cash")}
        />
        <PaymentMethodButton
          active={paymentMethod === "Card"}
          label="Card"
          icon="card"
          onPress={() => onPaymentMethodChange("Card")}
        />
      </View>

      {paymentMethod === "Cash" ? (
        <View className="flex-row gap-3">
          <View className="flex-1">
            <Text className="mb-1.5 text-[11px] font-semibold text-slate-500">
              Amount received
            </Text>
            <TextInput
              value={amountTendered}
              onChangeText={onAmountTenderedChange}
              keyboardType="decimal-pad"
              placeholder={backendTotal == null ? "-" : formatCurrency(backendTotal)}
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
      ) : null}

      {checkoutError ? <ErrorBox message={checkoutError} /> : null}
      {quoteErrorMessage ? <ErrorBox message={quoteErrorMessage} /> : null}

      {isQuoteLoading ? (
        <View className="flex-row items-center justify-center gap-2 py-1">
          <ActivityIndicator size="small" color="#059669" />
          <Text className="text-xs font-semibold text-slate-500">
            Verifying final amount…
          </Text>
        </View>
      ) : null}

      <Pressable
        className={`items-center rounded-xl py-4 ${
          canConfirm ? "bg-emerald-600" : "bg-slate-300"
        }`}
        disabled={!canConfirm}
        onPress={onConfirm}
      >
        {isSubmitting ? (
          <ActivityIndicator color="#ffffff" />
        ) : (
          <Text className="text-base font-bold text-white">
            Confirm {backendTotal == null ? "-" : formatCurrency(backendTotal)}
          </Text>
        )}
      </Pressable>
    </BottomActionBar>
  );
}

function PaymentMethodButton({
  active,
  label,
  icon,
  onPress,
}: {
  active: boolean;
  label: string;
  icon: "cash" | "card";
  onPress: () => void;
}) {
  const Icon = icon === "cash" ? Banknote : CreditCard;

  return (
    <Pressable
      className={`flex-1 flex-row items-center justify-center gap-1.5 rounded-lg py-3 ${
        active ? "bg-emerald-600" : "bg-slate-100"
      }`}
      onPress={onPress}
    >
      <Icon size={16} color={active ? "#ffffff" : "#475569"} />
      <Text className={`text-sm font-semibold ${active ? "text-white" : "text-slate-600"}`}>
        {label}
      </Text>
    </Pressable>
  );
}

function ErrorBox({ message }: { message: string }) {
  return (
    <View className="rounded-xl border border-red-200 bg-red-50 p-3">
      <Text className="text-sm font-semibold text-red-700">{message}</Text>
    </View>
  );
}
