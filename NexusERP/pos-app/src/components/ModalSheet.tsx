import React from "react";
import { Modal, View, Pressable, KeyboardAvoidingView, Platform } from "react-native";

interface ModalSheetProps {
  visible: boolean;
  onClose: () => void;
  children: React.ReactNode;
  maxWidth?: number;
}

export function ModalSheet({
  visible,
  onClose,
  children,
  maxWidth = 420,
}: ModalSheetProps) {
  return (
    <Modal
      visible={visible}
      transparent
      animationType="fade"
      onRequestClose={onClose}
    >
      <Pressable
        className="flex-1 items-center justify-center bg-slate-900/45 p-4"
        onPress={onClose}
      >
        <KeyboardAvoidingView
          behavior={Platform.OS === "ios" ? "padding" : undefined}
          className="w-full items-center"
        >
          <Pressable
            className="w-full rounded-2xl bg-white p-6 shadow-md"
            style={{ maxWidth }}
            onPress={(e) => e.stopPropagation()}
          >
            {children}
          </Pressable>
        </KeyboardAvoidingView>
      </Pressable>
    </Modal>
  );
}