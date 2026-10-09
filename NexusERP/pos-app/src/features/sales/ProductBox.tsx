import React from "react";
import { View, Text, Pressable, Image } from "react-native";
import { Pencil, Plus, Trash2 } from "lucide-react-native";
import { ShapeIcon } from "@/components/ShapeIcon";
import { API_BASE_URL } from "@/api/client";
import { formatCurrency } from "@/utils/currency";
import type { Product } from "@/types";

interface ProductBoxProps {
  product: Product | null;
  isEditing: boolean;
  onPress: () => void;
  onDelete: () => void;
}

function resolveImageUrl(path: string) {
  const origin = API_BASE_URL.replace(/\/api$/, "");
  return `${origin}${path}`;
}

export function ProductBox({
  product,
  isEditing,
  onPress,
  onDelete,
}: ProductBoxProps) {
  const isImageMode =
    product?.displayMode === "Image" && !!product.imageUrl;

  const isShapeMode =
    product?.displayMode === "Shape" && !!product.shapeType;

  const isNormalMode =
    !!product && !isImageMode && !isShapeMode;

  return (
    <Pressable
      onPress={onPress}
      className={[
        "relative flex-1 overflow-hidden border border-slate-200 bg-white",
        (product?.marketDiscountRate ?? 0) > 0 ? "border-red-300" : "",
        isEditing ? "border-dashed border-emerald-300" : "",
      ].join(" ")}
    >
      {product && (product.marketDiscountRate ?? 0) > 0 && (
        <View className="absolute left-2 top-2 z-20 rounded-md bg-red-600 px-2 py-1 shadow-sm">
          <Text className="text-[11px] font-extrabold text-white">
            -{product.marketDiscountRate ?? 0}%
          </Text>
        </View>
      )}

      {isEditing && (
        <View className="absolute right-2 top-2 z-20 h-7 w-7 items-center justify-center rounded-full bg-emerald-600">
          <Pencil size={12} color="white" />
        </View>
      )}

      {product ? (
        <>
          {isImageMode && (
            <Image
              source={{ uri: resolveImageUrl(product.imageUrl!) }}
              className="absolute inset-0 h-full w-full"
              resizeMode="cover"
            />
          )}

          {isShapeMode && (
            <View className="absolute inset-0 items-center justify-center bg-white">
              <ShapeIcon
                shapeType={product.shapeType!}
                color={product.shapeColor || "#ffffff"}
                text={product.shapeText}
                size={220}
              />
            </View>
          )}

          {isNormalMode && (
            <View className="absolute inset-0 items-center justify-center bg-white">
              <Text
                className="px-4 text-center text-lg font-semibold text-slate-700"
                numberOfLines={3}
              >
                {product.name}
              </Text>

              <Text className="mt-2 text-base font-bold text-emerald-600">
                {formatCurrency(product.price)}
              </Text>
            </View>
          )}

          {isEditing && (
            <Pressable
              onPress={(event) => {
                event.stopPropagation();
                onDelete();
              }}
              hitSlop={8}
              className="absolute right-2 top-11 z-20 h-9 w-9 items-center justify-center rounded-full bg-red-600"
            >
              <Trash2 size={17} color="white" />
            </Pressable>
          )}
        </>
      ) : (
        <View className="flex-1 items-center justify-center bg-white">
          <Plus size={22} color="#cbd5e1" />

          {isEditing && (
            <Text className="mt-1 text-xs text-slate-400">
              Add product
            </Text>
          )}
        </View>
      )}
    </Pressable>
  );
}
