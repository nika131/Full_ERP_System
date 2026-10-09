import React, { useState } from "react";
import { View, Text, Pressable, TextInput } from "react-native";
import { Minus, Plus } from "lucide-react-native";
import type { CartItem } from "@/types";

export function ItemEditor({
  item,
  onChangeQuantity,
  onChangeDiscount,
  onDone,
}: {
  item: CartItem;
  onChangeQuantity: (quantity: number) => void;
  onChangeDiscount: (percentage: number) => void;
  onDone: () => void;
}) {
  const [qty, setQty] = useState(item.quantity);
  const [discount, setDiscount] = useState(
    item.manualItemDiscountPercentage > 0
      ? String(item.manualItemDiscountPercentage)
      : ""
  );
  const [discountError, setDiscountError] = useState<string | null>(null);

  const handleSave = () => {
    const parsed = discount.trim() === "" ? 0 : parseFloat(discount);

    if (!Number.isFinite(parsed) || parsed < 0) {
      setDiscountError("Please enter a valid discount percentage.");
      return;
    }

    if (parsed > item.maxDiscountPercentage) {
      setDiscountError(
        `The maximum discount for this product is ${item.maxDiscountPercentage}%.`
      );
      return;
    }

    onChangeQuantity(qty);
    onChangeDiscount(parsed);
    onDone();
  };

  return (
    <View>
      <Text className="mb-4 text-base font-bold text-slate-800" numberOfLines={2}>
        {item.name}
      </Text>

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500">
        Quantity
      </Text>

      <View className="flex-row items-center gap-4">
        <Pressable
          className="h-10 w-10 items-center justify-center rounded-lg bg-slate-100"
          onPress={() => setQty((value) => Math.max(1, value - 1))}
        >
          <Minus size={18} color="#334155" />
        </Pressable>

        <Text className="min-w-8 text-center text-lg font-bold text-slate-800">
          {qty}
        </Text>

        <Pressable
          className="h-10 w-10 items-center justify-center rounded-lg bg-slate-100"
          onPress={() => setQty((value) => value + 1)}
        >
          <Plus size={18} color="#334155" />
        </Pressable>
      </View>

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500">
        Discount % {item.maxDiscountPercentage > 0 ? `(max ${item.maxDiscountPercentage}%)` : ""}
      </Text>

      <TextInput
        value={discount}
        onChangeText={(value) => {
          setDiscount(value);
          setDiscountError(null);
        }}
        keyboardType="decimal-pad"
        placeholder="0"
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px]"
      />

      {discountError ? (
        <Text className="mt-1.5 text-xs font-semibold text-red-600">
          {discountError}
        </Text>
      ) : null}

      <Pressable
        className="mt-6 items-center rounded-lg bg-emerald-600 py-3"
        onPress={handleSave}
      >
        <Text className="font-bold text-white">Save</Text>
      </Pressable>
    </View>
  );
}

export function CartDiscountEditor({
  value,
  maxDiscountPercentage,
  onSave,
}: {
  value: number;
  maxDiscountPercentage: number;
  onSave: (percentage: number) => void;
}) {
  const [discount, setDiscount] = useState(value > 0 ? String(value) : "");
  const [discountError, setDiscountError] = useState<string | null>(null);

  const handleSave = () => {
    const parsed = discount.trim() === "" ? 0 : parseFloat(discount);

    if (!Number.isFinite(parsed) || parsed < 0) {
      setDiscountError("Please enter a valid discount percentage.");
      return;
    }

    if (parsed > maxDiscountPercentage) {
      setDiscountError(
        `The maximum cart discount is ${maxDiscountPercentage}%.`
      );
      return;
    }

    onSave(parsed);
  };

  return (
    <View>
      <Text className="mb-4 text-base font-bold text-slate-800">
        Whole-receipt discount
      </Text>

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500">
        Discount % (max {maxDiscountPercentage}%)
      </Text>

      <TextInput
        value={discount}
        onChangeText={(value) => {
          setDiscount(value);
          setDiscountError(null);
        }}
        keyboardType="decimal-pad"
        placeholder="0"
        autoFocus
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px]"
      />

      {discountError ? (
        <Text className="mt-1.5 text-xs font-semibold text-red-600">
          {discountError}
        </Text>
      ) : null}

      <Text className="mt-2 text-[11px] text-slate-400">
        Subject to this store&apos;s maximum cart discount — checkout will reject
        percentages over the allowed limit.
      </Text>

      <Pressable
        className="mt-6 items-center rounded-lg bg-emerald-600 py-3"
        onPress={handleSave}
      >
        <Text className="font-bold text-white">Apply</Text>
      </Pressable>
    </View>
  );
}
