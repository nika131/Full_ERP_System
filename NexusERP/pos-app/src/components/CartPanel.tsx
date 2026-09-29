import React, { useState } from "react";
import { View, Text, FlatList, Pressable, TextInput } from "react-native";
import { ShoppingCart, Tag, Minus, Plus } from "lucide-react-native";
import { useCartStore } from "../store/cartStore";
import { CartItemRow } from "./CartItemRow";
import { ModalSheet } from "./ModalSheet";
import { formatCurrency, round2  } from "../utils/currency";
import type { CartItem } from "../types";
import { useTerminalStore } from "@/store/terminalStore";

interface CartPanelProps {
  onCheckout: () => void;
  disabled?: boolean;
}

export function CartPanel({ onCheckout, disabled }: CartPanelProps) {
  const items = useCartStore((s) => s.items);
  const cartDiscountPercentage = useCartStore(
    (s) => s.cartDiscountPercentage
  );
  const subtotal = useCartStore((s) => s.subtotal());

  const totalDiscount = useCartStore((s) => s.totalDiscount());

  const marketDiscountTotal = items.reduce(
    (sum, item) =>
      sum +
      round2(
        item.unitPrice *
          item.quantity *
          (item.marketDiscountPercentage / 100)
      ),
    0
  );

  const manualItemDiscountTotal = items.reduce(
    (sum, item) =>
      sum +
      round2(
        item.unitPrice *
          item.quantity *
          (item.manualItemDiscountPercentage / 100)
      ),
    0
  );

  const subtotalAfterItemDiscounts = round2(
    subtotal - marketDiscountTotal - manualItemDiscountTotal
  );

  const receiptDiscountAmount = round2(
    subtotalAfterItemDiscounts *
      (cartDiscountPercentage / 100)
  );

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
          ItemSeparatorComponent={() => (
            <View className="h-1.5 bg-slate-50" />
          )}
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
            {cartDiscountPercentage > 0
              ? `${cartDiscountPercentage}%`
              : "Add"}
          </Text>
        </Pressable>

        <View className="gap-1">
          <View className="flex-row justify-between">
            <Text className="text-[13px] text-slate-500">
              Subtotal
            </Text>

            <Text className="text-[13px] font-semibold text-slate-700">
              {formatCurrency(subtotal)}
            </Text>
          </View>

          {marketDiscountTotal > 0 && (
            <View className="flex-row justify-between">
              <Text className="text-[13px] text-slate-500">
                market discounts
              </Text>

              <Text className="text-[13px] font-semibold text-emerald-600">
                −{formatCurrency(marketDiscountTotal)}
              </Text>
            </View>
          )}

          {manualItemDiscountTotal > 0 && (
            <View className="flex-row justify-between">
              <Text className="text-[13px] text-slate-500">
                Manual item discounts
              </Text>

              <Text className="text-[13px] font-semibold text-emerald-600">
                −{formatCurrency(manualItemDiscountTotal)}
               </Text>
             </View>
           )}

          {cartDiscountPercentage > 0 && (
            <View className="flex-row justify-between">
              <Text className="text-[13px] text-slate-500">
                Receipt discount ({cartDiscountPercentage}%)
              </Text>

              <Text className="text-[13px] font-semibold text-emerald-600">
                −{formatCurrency(receiptDiscountAmount)}
              </Text>
            </View>
          )}

          <View className="mt-1 flex-row justify-between border-t border-slate-100 pt-2">
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
            onChangeDiscount={(percentage) =>
              setItemDiscount(
                editingItem.productId,
                percentage
              )
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
          value={cartDiscountPercentage}
          maxDiscountPercentage={maxCartDiscountPercentage}
          onSave={(percentage) => {
            setCartDiscount(percentage);
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
  onChangeDiscount: (percentage: number) => void;
  onDone: () => void;
}) {
  const [qty, setQty] = useState(item.quantity);

  const [discount, setDiscount] = useState(
    item.manualItemDiscountPercentage > 0
      ? String(item.manualItemDiscountPercentage)
      : ""
  );

  const [discountError, setDiscountError] = useState<string | null>(
    null
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
        onChangeText={(value) => {
          setDiscount(value);
          setDiscountError(null);
        }}
        keyboardType="decimal-pad"
        placeholder="0"
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px]"
      />

      {discountError && (
        <Text className="mt-1.5 text-xs font-semibold text-red-600">
          {discountError}
        </Text>
      )}

      <Pressable
        className="mt-6 items-center rounded-lg bg-emerald-600 py-3"
        onPress={() => {
          const parsed =
            discount.trim() === ""
              ? 0
              : parseFloat(discount);

          if (!Number.isFinite(parsed) || parsed < 0) {
            setDiscountError(
              "Please enter a valid discount percentage."
            );
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
        }}
      >
        <Text className="font-bold text-white">
          Save
        </Text>
      </Pressable>
    </View>
  );
}

function CartDiscountEditor({
  value,
  maxDiscountPercentage,
  onSave,
}: {
  value: number;
  maxDiscountPercentage: number;
  onSave: (percentage: number) => void;
}) {
  const [discount, setDiscount] = useState(
    value > 0 ? String(value) : ""
  );

  const [discountError, setDiscountError] = useState<string | null>(
    null
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

      {discountError && (
        <Text className="mt-1.5 text-xs font-semibold text-red-600">
          {discountError}
        </Text>
      )}

      <Text className="mt-2 text-[11px] text-slate-400">
        Subject to this store's maximum cart discount — checkout
        will reject percentages over the allowed limit.
      </Text>

      <Pressable
        className="mt-6 items-center rounded-lg bg-emerald-600 py-3"
        onPress={() => {
          const parsed =
            discount.trim() === ""
              ? 0
              : parseFloat(discount);

          if (!Number.isFinite(parsed) || parsed < 0) {
            setDiscountError(
              "Please enter a valid discount percentage."
            );
            return;
          }

          if (parsed > maxDiscountPercentage) {
            setDiscountError(
              `The maximum cart discount is ${maxDiscountPercentage}%.`
            );
            return;
          }

          onSave(parsed);
        }}
      >
        <Text className="font-bold text-white">
          Apply
        </Text>
      </Pressable>
    </View>
  );
}