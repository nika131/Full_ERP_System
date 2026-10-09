import React from "react";
import { View, Text } from "react-native";

interface ScreenHeaderProps {
  title?: string;
  subtitle?: string;
  left?: React.ReactNode;
  right?: React.ReactNode;
  className?: string;
}

export function ScreenHeader({
  title,
  subtitle,
  left,
  right,
  className = "flex-row items-center justify-between border-b border-slate-200 bg-white px-4 py-3",
}: ScreenHeaderProps) {
  return (
    <View className={className}>
      <View className="flex-row flex-1 items-center gap-3">
        {left}
        {(title || subtitle) && (
          <View className="flex-1">
            {title ? (
              <Text className="text-base font-bold text-slate-800" numberOfLines={1}>
                {title}
              </Text>
            ) : null}
            {subtitle ? (
              <Text className="mt-0.5 text-xs text-slate-500" numberOfLines={1}>
                {subtitle}
              </Text>
            ) : null}
          </View>
        )}
      </View>
      {right ? <View className="ml-3">{right}</View> : null}
    </View>
  );
}
