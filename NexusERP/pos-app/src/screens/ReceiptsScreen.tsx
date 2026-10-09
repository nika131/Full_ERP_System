import React from "react";
import { ActivityIndicator, FlatList, Pressable, Text, View } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { Banknote, ChevronRight, CreditCard, Receipt as ReceiptIcon, Ticket } from "lucide-react-native";
import type { ReceiptsStackParamList } from "@/navigation/types";
import type { ShiftReceiptSummary } from "@/types";
import { useTerminalStore } from "@/store/terminalStore";
import { useCurrentShiftQuery, useShiftReceiptsQuery } from "@/hooks/queries/usePosQueries";
import { formatCurrency } from "@/utils/currency";
import { MenuButton } from "@/components/MenuButton";
import { AppScreen } from "@/components/layout/AppScreen";
import { ScreenHeader } from "@/components/layout/ScreenHeader";

type Props = NativeStackScreenProps<ReceiptsStackParamList, "ReceiptsHome">;

export function ReceiptsScreen({ navigation }: Props) {
  const { storeId } = useTerminalStore();
  const { data: shiftData } = useCurrentShiftQuery(storeId);
  const shiftId = shiftData?.shift?.shiftId ?? null;
  const { data: receipts, isLoading, isError } = useShiftReceiptsQuery(shiftId);

  if (!shiftId) {
    return (
      <AppScreen>
        <ScreenHeader left={<MenuButton />} />
        <View className="flex-1 items-center justify-center gap-1.5 p-6">
          <ReceiptIcon size={40} color="#cbd5e1" />
          <Text className="text-[15px] font-bold text-slate-600">
            No shift is open
          </Text>
          <Text className="text-center text-[13px] text-slate-400">
            Open a shift to start selling.
          </Text>
          <Pressable
            onPress={() => navigation.getParent()?.navigate("Shift")}
            className="mt-2 rounded-md bg-emerald-600 px-4 py-2.5"
          >
            <Text className="text-sm font-semibold text-white">Go to Shift</Text>
          </Pressable>
        </View>
      </AppScreen>
    );
  }

  return (
    <AppScreen>
      <ScreenHeader
        title="Current Shift Receipts"
        subtitle={`${receipts?.length ?? 0} sales`}
        left={<MenuButton />}
      />

      {isLoading ? (
        <CenteredState>
          <ActivityIndicator color="#059669" />
        </CenteredState>
      ) : isError ? (
        <CenteredState>
          <Text className="text-red-600">Failed to load receipts.</Text>
        </CenteredState>
      ) : (
        <FlatList
          data={receipts || []}
          keyExtractor={(receipt) => String(receipt.receiptId)}
          contentContainerClassName="p-4"
          ListEmptyComponent={
            <View className="items-center justify-center gap-1.5 p-6">
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
                <PaymentIcon method={item.paymentMethod} />
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
                  · {item.itemCount} item{item.itemCount === 1 ? "" : "s"}
                </Text>
              </View>

              <Text className="mr-1 text-[15px] font-extrabold text-emerald-600">
                {formatCurrency(item.finalTotal)}
              </Text>
              <ChevronRight size={18} color="#cbd5e1" />
            </Pressable>
          )}
        />
      )}
    </AppScreen>
  );
}

function PaymentIcon({ method }: { method: ShiftReceiptSummary["paymentMethod"] }) {
  if (method === "Cash") return <Banknote size={16} color="#059669" />;
  if (method === "Card") return <CreditCard size={16} color="#2563eb" />;
  return <Ticket size={16} color="#d97706" />;
}

function CenteredState({ children }: { children: React.ReactNode }) {
  return <View className="flex-1 items-center justify-center">{children}</View>;
}
