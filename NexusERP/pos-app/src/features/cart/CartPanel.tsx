import React, { useState } from "react";
import { View, Text, FlatList, Pressable } from "react-native";
import { ShoppingCart, Tag } from "lucide-react-native";

import { useCartStore } from "@/store/cartStore";
import { useTerminalStore } from "@/store/terminalStore";
import { formatCurrency } from "@/utils/currency";
import type { CartItem } from "@/types";
import { ModalSheet } from "@/components/ModalSheet";
import { BottomActionBar } from "@/components/layout/AppScreen";
import { CartItemRow } from "./CartItemRow";
import { CartDiscountEditor, ItemEditor } from "./CartEditors";
import { calculateCartTotals } from "@/utils/cartPricing";

interface CartPanelProps {
  onCheckout: () => void;
  disabled?: boolean;
}

export function CartPanel({ onCheckout, disabled }: CartPanelProps) {
  const items = useCartStore((state) => state.items);
  const cartDiscountPercentage = useCartStore(
    (state) => state.cartDiscountPercentage
  );
  const incrementQuantity = useCartStore((state) => state.incrementQuantity);
  const setQuantity = useCartStore((state) => state.setQuantity);
  const setItemDiscount = useCartStore((state) => state.setItemDiscount);
  const removeItem = useCartStore((state) => state.removeItem);
  const setCartDiscount = useCartStore((state) => state.setCartDiscount);
  const maxCartDiscountPercentage = useTerminalStore(
    (state) => state.maxCartDiscountPercentage
  );

  const totals = calculateCartTotals(items, cartDiscountPercentage);
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
          <Text className="mt-2 font-semibold text-slate-500">Cart is empty</Text>
          <Text className="text-xs text-slate-400">Tap a product to add it</Text>
        </View>
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => String(item.productId)}
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

      <BottomActionBar className="gap-3 border-t border-slate-200 bg-white px-4 pt-4">
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
            {cartDiscountPercentage > 0 ? `${cartDiscountPercentage}%` : "Add"}
          </Text>
        </Pressable>

        <View className="gap-1">
          <SummaryRow label="Subtotal" value={formatCurrency(totals.subtotal)} />

          {totals.marketDiscountTotal > 0 ? (
            <SummaryRow
              label="Market discounts"
              value={`−${formatCurrency(totals.marketDiscountTotal)}`}
              valueClassName="text-emerald-600"
            />
          ) : null}

          {totals.manualItemDiscountTotal > 0 ? (
            <SummaryRow
              label="Manual item discounts"
              value={`−${formatCurrency(totals.manualItemDiscountTotal)}`}
              valueClassName="text-emerald-600"
            />
          ) : null}

          {cartDiscountPercentage > 0 ? (
            <SummaryRow
              label={`Receipt discount (${cartDiscountPercentage}%)`}
              value={`−${formatCurrency(totals.receiptDiscountAmount)}`}
              valueClassName="text-emerald-600"
            />
          ) : null}

          <View className="mt-1 flex-row justify-between border-t border-slate-100 pt-2">
            <Text className="text-base font-bold text-slate-800">Total</Text>
            <Text className="text-xl font-extrabold text-emerald-600">
              {formatCurrency(totals.total)}
            </Text>
          </View>
        </View>

        <Pressable
          disabled={disabled || items.length === 0}
          onPress={onCheckout}
          className={`items-center rounded-xl py-3.5 ${
            disabled || items.length === 0 ? "bg-slate-300" : "bg-emerald-600"
          }`}
        >
          <Text className="text-[15px] font-bold text-white">Checkout</Text>
        </Pressable>
      </BottomActionBar>

      <ModalSheet visible={!!editingItem} onClose={() => setEditingItem(null)}>
        {editingItem ? (
          <ItemEditor
            item={editingItem}
            onChangeQuantity={(quantity) =>
              setQuantity(editingItem.productId, quantity)
            }
            onChangeDiscount={(percentage) =>
              setItemDiscount(editingItem.productId, percentage)
            }
            onDone={() => setEditingItem(null)}
          />
        ) : null}
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

function SummaryRow({
  label,
  value,
  valueClassName = "text-slate-700",
}: {
  label: string;
  value: string;
  valueClassName?: string;
}) {
  return (
    <View className="flex-row justify-between">
      <Text className="text-[13px] text-slate-500">{label}</Text>
      <Text className={`text-[13px] font-semibold ${valueClassName}`}>
        {value}
      </Text>
    </View>
  );
}
