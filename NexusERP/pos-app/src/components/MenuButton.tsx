import React from "react";
import { Pressable } from "react-native";
import { DrawerActions, useNavigation } from "@react-navigation/native";
import { Menu } from "lucide-react-native";
import { useIsWideLayout } from "@/hooks/useIsWideLayout";

interface MenuButtonProps {
  className?: string;
}

export function MenuButton({ className }: MenuButtonProps) {
  const navigation = useNavigation();
  const isWide = useIsWideLayout();

  if (isWide) return null;

  return (
    <Pressable
      onPress={() => navigation.dispatch(DrawerActions.openDrawer())}
      hitSlop={8}
      className={
        className ?? "h-9 w-9 items-center justify-center rounded-lg bg-slate-100"
      }
    >
      <Menu size={20} color="#334155" />
    </Pressable>
  );
}