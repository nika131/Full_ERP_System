import React, { useState } from "react";
import { View, Text, FlatList, Pressable, TextInput, Alert } from "react-native";
import { ShoppingCart, Tag, Minus, Plus } from "lucide-react-native";
import { useCartStore } from "../store/cartStore";
import { CartItemRow } from "./CartItemRow";
import { ModalSheet } from "./ModalSheet";
import { formatCurrency, round2 } from "../utils/currency";
import type { CartItem } from "../types";
import { useTerminalStore } from "@/store/terminalStore";

interface CartPanelProps {
  onCheckout: () => void;
  disabled?: boolean;
}

export function CartPanel({ onCheckout, disabled }: CartPanelProps) {
  const items = useCartStore((s) => s.items);
  const cartDiscountAmount = useCartStore((s) => s.cartDiscountAmount);
  const subtotal = useCartStore((s) => s.subtotal());
  const total = useCartStore((s) => s.total());
  const incrementQuantity = useCartStore((s) => s.incrementQuantity);
  const setQuantity = useCartStore((s) => s.setQuantity);
  const setItemDiscount = useCartStore((s) => s.setItemDiscount);
  const removeItem = useCartStore((s) => s.removeItem);
  const setCartDiscount = useCartStore((s) => s.setCartDiscount);
  const maxCartDiscountPercentage = useTerminalStore(
    (s) => s.maxCartDiscountPercentage
  );

  const [editingItem, setEditingItem] = useState<CartItem | null>(null);
  const [discountModalOpen, setDiscountModalOpen] = useState(false);

  return (
    <View className="flex-1 border-l border-slate-200 bg-white">
      <View className="flex-row items-center gap-2 border-b border-slate-100 p-4">
        <ShoppingCart size={18} color="#334155" />
        <Text className="text-[15px] font-bold text-slate-800">
          Current Sale
        </Text>
      </View>

      {items.length === 0 ? (
        <View className="flex-1 items-center justify-center gap-1">
          <ShoppingCart size={36} color="#cbd5e1" />
          <Text className="mt-2 font-semibold text-slate-500">
            Cart is empty
          </Text>
          <Text className="text-xs text-slate-400">
            Tap a product to add it
          </Text>
        </View>
      ) : (
        <FlatList
          data={items}
          keyExtractor={(i) => String(i.productId)}
          renderItem={({ item }) => (
            <CartItemRow
              item={item}
              onPress={() => setEditingItem(item)}
              onIncrement={() => incrementQuantity(item.productId)}
              onRemove={() => removeItem(item.productId)}
            />
          )}
          ItemSeparatorComponent={() => <View className="h-1.5 bg-slate-50" />}
          className="flex-1"
          showsVerticalScrollIndicator={false}
        />
      )}

      <View className="gap-3 border-t border-slate-200 p-4">
        <Pressable
          className="flex-row items-center justify-between rounded-lg bg-emerald-50 p-3"
          onPress={() => setDiscountModalOpen(true)}
        >
          <View className="flex-row items-center gap-1.5">
            <Tag size={14} color="#059669" />
            <Text className="text-[13px] font-semibold text-emerald-700">
              Receipt discount
            </Text>
          </View>

          <Text className="text-[13px] font-bold text-emerald-700">
            {cartDiscountAmount > 0
              ? `− ${formatCurrency(cartDiscountAmount)}`
              : "Add"}
          </Text>
        </Pressable>

        <View className="gap-1">
          <View className="flex-row justify-between">
            <Text className="text-[13px] text-slate-500">Subtotal</Text>
            <Text className="text-[13px] font-semibold text-slate-700">
              {formatCurrency(subtotal)}
            </Text>
          </View>

          <View className="flex-row justify-between">
            <Text className="text-base font-bold text-slate-800">
              Total
            </Text>
            <Text className="text-xl font-extrabold text-emerald-600">
              {formatCurrency(total)}
            </Text>
          </View>
        </View>

        <Pressable
          disabled={disabled || items.length === 0}
          onPress={onCheckout}
          className={[
            "items-center rounded-xl py-3.5",
            disabled || items.length === 0
              ? "bg-slate-300"
              : "bg-emerald-600",
          ].join(" ")}
        >
          <Text className="text-[15px] font-bold text-white">
            Checkout
          </Text>
        </Pressable>
      </View>

      <ModalSheet
        visible={!!editingItem}
        onClose={() => setEditingItem(null)}
      >
        {editingItem && (
          <ItemEditor
            item={editingItem}
            onChangeQuantity={(q) =>
              setQuantity(editingItem.productId, q)
            }
            onChangeDiscount={(d) =>
              setItemDiscount(editingItem.productId, d)
            }
            onDone={() => setEditingItem(null)}
          />
        )}
      </ModalSheet>

      <ModalSheet
        visible={discountModalOpen}
        onClose={() => setDiscountModalOpen(false)}
      >
        <CartDiscountEditor
          value={cartDiscountAmount}
          maxValue={subtotal}
          maxDiscountPercentage={maxCartDiscountPercentage}
          onSave={(v) => {
            setCartDiscount(v);
            setDiscountModalOpen(false);
          }}
        />
      </ModalSheet>
    </View>
  );
}

function ItemEditor({
  item,
  onChangeQuantity,
  onChangeDiscount,
  onDone,
}: {
  item: CartItem;
  onChangeQuantity: (q: number) => void;
  onChangeDiscount: (d: number) => void;
  onDone: () => void;
}) {
  const [qty, setQty] = useState(item.quantity);

  const currentDiscountPercentage =
    item.unitPrice * item.quantity > 0
      ? round2(
          (item.manualItemDiscount /
            (item.unitPrice * item.quantity)) *
            100
        )
      : 0;

  const [discount, setDiscount] = useState(
    currentDiscountPercentage > 0
      ? String(currentDiscountPercentage)
      : ""
  );

  const maxDiscount = round2(
    (item.maxDiscountPercentage / 100) * item.unitPrice * qty
  );

  return (
    <View>
      <Text
        className="mb-4 text-base font-bold text-slate-800"
        numberOfLines={2}
      >
        {item.name}
      </Text>

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500">
        Quantity
      </Text>

      <View className="flex-row items-center gap-4">
        <Pressable
          className="h-10 w-10 items-center justify-center rounded-lg bg-slate-100"
          onPress={() => setQty((q) => Math.max(1, q - 1))}
        >
          <Minus size={18} color="#334155" />
        </Pressable>

        <Text className="min-w-8 text-center text-lg font-bold text-slate-800">
          {qty}
        </Text>

        <Pressable
          className="h-10 w-10 items-center justify-center rounded-lg bg-slate-100"
          onPress={() => setQty((q) => q + 1)}
        >
          <Plus size={18} color="#334155" />
        </Pressable>
      </View>

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500">
        Discount %{" "}
        {item.maxDiscountPercentage > 0
          ? `(max ${item.maxDiscountPercentage}%)`
          : ""}
      </Text>

      <TextInput
        value={discount}
        onChangeText={setDiscount}
        keyboardType="decimal-pad"
        placeholder="0"
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px]"
      />

      <Pressable
        className="mt-6 items-center rounded-lg bg-emerald-600 py-3"
        onPress={() => {
          const parsed =
            discount.trim() === "" ? 0 : parseFloat(discount);

          if (!Number.isFinite(parsed) || parsed < 0) {
            Alert.alert(
              "Invalid discount",
              "Please enter a valid discount percentage."
            );
            return;
          }

          if (parsed > item.maxDiscountPercentage) {
            Alert.alert(
              "Discount not allowed",
              `The maximum discount for this product is ${item.maxDiscountPercentage}%.`
            );
            return;
          }

          const discountAmount = round2(
            (parsed / 100) * item.unitPrice * qty
          );

          onChangeQuantity(qty);
          onChangeDiscount(discountAmount);
          onDone();
        }}
      >
        <Text className="font-bold text-white">Save</Text>
      </Pressable>
    </View>
  );
}

function CartDiscountEditor({
  value,
  maxValue,
  maxDiscountPercentage,
  onSave,
}: {
  value: number;
  maxValue: number;
  maxDiscountPercentage: number;
  onSave: (v: number) => void;
}) {
  const currentPercentage =
    maxValue > 0 ? round2((value / maxValue) * 100) : 0;

  const [amount, setAmount] = useState(
    currentPercentage > 0 ? String(currentPercentage) : ""
  );

  return (
    <View>
      <Text className="mb-4 text-base font-bold text-slate-800">
        Whole-receipt discount
      </Text>

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500">
        Discount % (max {maxDiscountPercentage}%)
      </Text>

      <TextInput
        value={amount}
        onChangeText={setAmount}
        keyboardType="decimal-pad"
        placeholder="0"
        autoFocus
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px]"
      />

      <Text className="mt-2 text-[11px] text-slate-400">
        Subject to this store's maximum cart discount — checkout will
        reject amounts over the allowed limit.
      </Text>

      <Pressable
        className="mt-6 items-center rounded-lg bg-emerald-600 py-3"
        onPress={() => {
          const parsed =
            amount.trim() === "" ? 0 : parseFloat(amount);

          if (!Number.isFinite(parsed) || parsed < 0) {
            Alert.alert(
              "Invalid discount",
              "Please enter a valid discount percentage."
            );
            return;
          }

          if (parsed > maxDiscountPercentage) {
            Alert.alert(
              "Discount not allowed",
              `The maximum receipt discount for this store is ${maxDiscountPercentage}%.`
            );
            return;
          }

          const discountAmount = round2(
            (parsed / 100) * maxValue
          );

          onSave(discountAmount);
        }}
      >
        <Text className="font-bold text-white">Apply</Text>
      </Pressable>
    </View>
  );
}