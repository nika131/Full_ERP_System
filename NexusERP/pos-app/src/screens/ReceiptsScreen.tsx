import React from "react";
import {
  View,
  Text,
  FlatList,
  Pressable,
  ActivityIndicator,
} from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import {
  Receipt as ReceiptIcon,
  ChevronRight,
  Banknote,
  CreditCard,
  Ticket,
} from "lucide-react-native";
import { useTerminalStore } from "../store/terminalStore";
import {
  useCurrentShiftQuery,
  useShiftReceiptsQuery,
} from "../hooks/queries/usePosQueries";
import { formatCurrency } from "../utils/currency";
import type { ReceiptsStackParamList } from "../navigation/types";
import type { ShiftReceiptSummary } from "../types";
import { MenuButton } from "@/components/MenuButton";

type Props = NativeStackScreenProps<
  ReceiptsStackParamList,
  "ReceiptsHome"
>;

function paymentIcon(method: ShiftReceiptSummary["paymentMethod"]) {
  if (method === "Cash") {
    return <Banknote size={16} color="#059669" />;
  }

  if (method === "Card") {
    return <CreditCard size={16} color="#2563eb" />;
  }

  return <Ticket size={16} color="#d97706" />;
}

export function ReceiptsScreen({ navigation }: Props) {
  const { storeId } = useTerminalStore();
  const { data: shiftData } = useCurrentShiftQuery(storeId);
  const shiftId = shiftData?.shift?.shiftId ?? null;

  const {
    data: receipts,
    isLoading,
    isError,
  } = useShiftReceiptsQuery(shiftId);

  if (!shiftId) {
    return (
      <View className="flex-1 items-center justify-center gap-1.5 bg-slate-50 p-6">
        <ReceiptIcon size={40} color="#cbd5e1" />

        <Text className="text-[15px] font-bold text-slate-600">
          No open shift
        </Text>

        <Text className="text-center text-[13px] text-slate-400">
          Open a shift to see its receipts here.
        </Text>
      </View>
    );
  }

  if (isLoading) {
    return (
      <View className="flex-1 items-center justify-center bg-slate-50">
        <ActivityIndicator color="#059669" />
      </View>
    );
  }

  if (isError) {
    return (
      <View className="flex-1 items-center justify-center bg-slate-50">
        <Text className="text-red-600">
          Failed to load receipts.
        </Text>
      </View>
    );
  }

  return (
    <View className="flex-1 bg-slate-50">
      <View className="flex-row items-center border-b border-slate-200 bg-white p-4">
        <View className="flex-row items-center gap-3 border-b border-slate-200 bg-white p-4">
          <MenuButton />
          <View>
            <Text className="text-[17px] font-extrabold text-slate-800">Current Shift Receipts</Text>
            <Text className="mt-0.5 text-xs text-slate-500">{receipts?.length ?? 0} sales</Text>
          </View>
        </View>
      </View>
      <FlatList
        data={receipts || []}
        keyExtractor={(r) => String(r.receiptId)}
        contentContainerClassName="p-4"
        ListEmptyComponent={
          <View className="flex-1 items-center justify-center gap-1.5 p-6">
            <ReceiptIcon size={40} color="#cbd5e1" />

            <Text className="text-[15px] font-bold text-slate-600">
              No sales yet
            </Text>
          </View>
        }
        renderItem={({ item }) => (
          <Pressable
            className="mb-2 flex-row items-center gap-3 rounded-xl border border-slate-200 bg-white p-3"
            onPress={() =>
              navigation.navigate("ReceiptDetail", {
                receiptId: item.receiptId,
              })
            }
          >
            <View className="h-9 w-9 items-center justify-center rounded-lg bg-slate-50">
              {paymentIcon(item.paymentMethod)}
            </View>

            <View className="flex-1">
              <Text className="text-sm font-bold text-slate-800">
                #{item.receiptNumber}
              </Text>

              <Text className="mt-0.5 text-xs text-slate-500">
                {new Date(item.createdAt).toLocaleTimeString([], {
                  hour: "2-digit",
                  minute: "2-digit",
                })}{" "}
                · {item.itemCount} item
                {item.itemCount === 1 ? "" : "s"}
              </Text>
            </View>

            <Text className="mr-1 text-[15px] font-extrabold text-emerald-600">
              {formatCurrency(item.finalTotal)}
            </Text>

            <ChevronRight size={18} color="#cbd5e1" />
          </Pressable>
        )}
      />
    </View>
  );
}