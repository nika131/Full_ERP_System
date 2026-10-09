import React, { useRef } from "react";
import { View, Text, Pressable } from "react-native";
import { Trash2 } from "lucide-react-native";

import type { CartItem } from "@/types";
import { formatCurrency } from "@/utils/currency";
import { calculateCartLine } from "@/utils/cartPricing";

interface CartItemRowProps {
  item: CartItem;
  onPress: () => void;
  onIncrement: () => void;
  onRemove: () => void;
}

const DOUBLE_TAP_MS = 280;

export function CartItemRow({
  item,
  onPress,
  onIncrement,
  onRemove,
}: CartItemRowProps) {
  const lastTapRef = useRef(0);
  const pricing = calculateCartLine(item);
  const totalItemDiscountAmount =
    pricing.marketDiscountAmount + pricing.manualDiscountAmount;

  const handlePress = () => {
    const now = Date.now();

    if (now - lastTapRef.current < DOUBLE_TAP_MS) {
      onIncrement();
      lastTapRef.current = 0;
      return;
    }

    lastTapRef.current = now;

    setTimeout(() => {
      if (Date.now() - lastTapRef.current >= DOUBLE_TAP_MS - 20) {
        onPress();
      }
    }, DOUBLE_TAP_MS);
  };

  return (
    <View className="flex-row items-center border-b border-slate-100 bg-white">
      <Pressable
        onPress={handlePress}
        className="flex-1 flex-row items-center justify-between py-3 pl-3"
      >
        <View className="flex-1 pr-2">
          <Text
            className="text-sm font-semibold text-slate-800"
            numberOfLines={1}
          >
            {item.name}
          </Text>

          <Text className="mt-0.5 text-xs text-slate-500">
            {item.quantity} × {formatCurrency(item.unitPrice)}
          </Text>

          {item.marketDiscountPercentage > 0 && (
            <Text className="mt-0.5 text-xs font-medium text-red-600">
              Market discount: −{item.marketDiscountPercentage}%: −
              {formatCurrency(pricing.marketDiscountAmount)}
            </Text>
          )}

          {item.manualItemDiscountPercentage > 0 && (
            <Text className="mt-0.5 text-xs font-medium text-emerald-600">
              Manual discount {item.manualItemDiscountPercentage}%: −
              {formatCurrency(pricing.manualDiscountAmount)}
            </Text>
          )}
        </View>

        <View className="mr-3 items-end">
          {totalItemDiscountAmount > 0 && (
            <Text className="text-xs text-slate-400 line-through">
              {formatCurrency(pricing.baseAmount)}
            </Text>
          )}

          <Text className="text-sm font-bold text-slate-800">
            {formatCurrency(pricing.afterManualDiscount)}
          </Text>
        </View>
      </Pressable>

      <Pressable
        onPress={onRemove}
        hitSlop={8}
        className="mr-1 h-12 w-12 items-center justify-center rounded-lg bg-red-50"
      >
        <Trash2 size={18} color="#dc2626" />
      </Pressable>
    </View>
  );
}
