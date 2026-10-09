import React from "react";
import { Modal, Pressable, Text, View } from "react-native";
import { X } from "lucide-react-native";

import { CartPanel } from "@/features/cart/CartPanel";

export function PhoneCartModal({
  visible,
  onClose,
  onCheckout,
}: {
  visible: boolean;
  onClose: () => void;
  onCheckout: () => void;
}) {
  return (
    <Modal
      visible={visible}
      transparent
      animationType="slide"
      onRequestClose={onClose}
    >
      <View className="flex-1 justify-end bg-black/40">
        <View className="h-[85%] overflow-hidden rounded-t-2xl bg-white">
          <View className="flex-row items-center justify-between border-b border-slate-200 px-4 py-3">
            <Text className="text-base font-bold text-slate-700">Cart</Text>
            <Pressable
              onPress={onClose}
              className="h-9 w-9 items-center justify-center rounded-full bg-slate-100"
            >
              <X size={18} color="#475569" />
            </Pressable>
          </View>

          <View className="flex-1">
            <CartPanel onCheckout={onCheckout} />
          </View>
        </View>
      </View>
    </Modal>
  );
}
