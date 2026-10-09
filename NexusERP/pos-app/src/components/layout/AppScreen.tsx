import React from "react";
import type { ViewProps } from "react-native";
import { View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

type SafeEdge = "top" | "bottom" | "left" | "right";

interface AppScreenProps extends ViewProps {
  children: React.ReactNode;
  edges?: SafeEdge[];
  className?: string;
}

export function AppScreen({
  children,
  edges = ["top", "bottom"],
  className = "flex-1 bg-slate-50",
  style,
  ...props
}: AppScreenProps) {
  const insets = useSafeAreaInsets();

  return (
    <View
      {...props}
      className={className}
      style={[
        {
          paddingTop: edges.includes("top") ? insets.top : 0,
          paddingBottom: edges.includes("bottom") ? insets.bottom : 0,
          paddingLeft: edges.includes("left") ? insets.left : 0,
          paddingRight: edges.includes("right") ? insets.right : 0,
        },
        style,
      ]}
    >
      {children}
    </View>
  );
}

interface BottomActionBarProps extends ViewProps {
  children: React.ReactNode;
  className?: string;
  minimumBottomPadding?: number;
}

export function BottomActionBar({
  children,
  className = "gap-3 border-t border-slate-200 bg-white px-4 pt-4 shadow-md",
  minimumBottomPadding = 16,
  style,
  ...props
}: BottomActionBarProps) {
  const insets = useSafeAreaInsets();

  return (
    <View
      {...props}
      className={className}
      style={[
        { paddingBottom: Math.max(insets.bottom, minimumBottomPadding) },
        style,
      ]}
    >
      {children}
    </View>
  );
}
