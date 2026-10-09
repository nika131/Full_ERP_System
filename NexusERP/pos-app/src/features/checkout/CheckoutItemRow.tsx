import React from "react";
import { View, Text } from "react-native";

import type { CartItem } from "@/types";
import { formatCurrency } from "@/utils/currency";
import { calculateCartLine } from "@/utils/cartPricing";

export function CheckoutItemRow({ item }: { item: CartItem }) {
  const pricing = calculateCartLine(item);

  return (
    <View className="flex-row justify-between border-b border-slate-100 py-2">
      <View className="flex-1 pr-3">
        <Text className="text-sm font-semibold text-slate-800" numberOfLines={1}>
          {item.name}
        </Text>

        <Text className="mt-0.5 text-xs text-slate-500">
          {item.quantity} × {formatCurrency(item.unitPrice)}
        </Text>

        {item.marketDiscountPercentage > 0 ? (
          <Text className="text-xs text-red-600">
            Market discount: −{item.marketDiscountPercentage}%: −
            {formatCurrency(pricing.marketDiscountAmount)}
          </Text>
        ) : null}

        {item.manualItemDiscountPercentage > 0 ? (
          <Text className="text-xs text-emerald-600">
            Manual discount: −{item.manualItemDiscountPercentage}%: −
            {formatCurrency(pricing.manualDiscountAmount)}
          </Text>
        ) : null}
      </View>

      <Text className="text-sm font-bold text-slate-800">
        {formatCurrency(pricing.afterManualDiscount)}
      </Text>
    </View>
  );
}
