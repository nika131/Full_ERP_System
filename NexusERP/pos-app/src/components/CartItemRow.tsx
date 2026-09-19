import React, { useRef } from "react";
import { View, Text, Pressable } from "react-native";
import { Trash2 } from "lucide-react-native";
import { formatCurrency, round2 } from "../utils/currency";
import type { CartItem } from "../types";

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
  const lastTapRef = useRef<number>(0);

  const lineTotal = round2(
    item.unitPrice * item.quantity - item.manualItemDiscount
  );

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
            {item.manualItemDiscount > 0
              ? ` − ${formatCurrency(item.manualItemDiscount)}`
              : ""}
          </Text>
        </View>

        <View className="mr-3">
          <Text className="text-sm font-bold text-slate-800">
            {formatCurrency(lineTotal)}
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