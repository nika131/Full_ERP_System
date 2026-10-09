import React from "react";
import { ActivityIndicator, Pressable, ScrollView, Text, View } from "react-native";
import { ArrowDownCircle, ArrowUpCircle, Store } from "lucide-react-native";
import { useStoresQuery } from "@/hooks/queries/usePosQueries";
import { formatCurrency } from "@/utils/currency";
import type { CurrentShift, StoreLookup } from "@/types";

export function StorePickerView({
  onPicked,
}: {
  onPicked: (store: StoreLookup) => void;
}) {
  const { data: stores, isLoading } = useStoresQuery();

  return (
    <View className="flex-1 items-center justify-center gap-1.5 p-6">
      <Store size={36} color="#cbd5e1" />

      <Text className="mt-3 text-[17px] font-bold text-slate-700 md:text-xl">
        Which store is this device in?
      </Text>

      <Text className="text-center text-[13px] text-slate-400 md:text-sm">
        This is remembered on this device going forward.
      </Text>

      {isLoading ? (
        <ActivityIndicator color="#059669" className="mt-4" />
      ) : (
        <View className="mt-6 w-full max-w-[340px] gap-2">
          {(stores || []).map((store) => (
            <Pressable
              key={store.storeId}
              onPress={() => onPicked(store)}
              className="items-center rounded-lg border border-slate-200 bg-white p-3 md:p-4"
            >
              <Text className="font-semibold text-slate-700 md:text-base">
                {store.name}
              </Text>
            </Pressable>
          ))}
        </View>
      )}
    </View>
  );
}

export function ActiveShiftView({
  shift,
  onCashIn,
  onCashOut,
  onClose,
}: {
  shift: CurrentShift;
  onCashIn: () => void;
  onCashOut: () => void;
  onClose: () => void;
}) {
  return (
    <ScrollView contentContainerClassName="gap-4 p-4 md:gap-5 md:p-6">
      <View className="flex-row gap-3 md:gap-4">
        <Pressable
          className="flex-1 flex-row items-center justify-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 py-4 md:py-5"
          onPress={onCashIn}
        >
          <ArrowDownCircle size={22} color="#047857" />
          <Text className="text-sm font-bold text-emerald-700 md:text-base">
            Cash In
          </Text>
        </Pressable>

        <Pressable
          className="flex-1 flex-row items-center justify-center gap-2 rounded-xl border border-red-200 bg-red-50 py-4 md:py-5"
          onPress={onCashOut}
        >
          <ArrowUpCircle size={22} color="#b91c1c" />
          <Text className="text-sm font-bold text-red-700 md:text-base">
            Cash Out
          </Text>
        </Pressable>
      </View>

      <View className="rounded-2xl bg-white p-6 shadow-sm md:p-8">
        <Text className="mb-3 text-lg font-extrabold text-slate-800 md:text-xl">
          Shift Summary
        </Text>

        <StatRow label="Started" value={new Date(shift.startDate).toLocaleString()} />
        <StatRow label="Starting cash" value={formatCurrency(shift.startingCash)} />
        <StatRow
          label="Expected cash now"
          value={formatCurrency(shift.expectedEndingCash)}
          highlight
        />

        <View className="my-2 h-px bg-slate-100" />

        <StatRow label="Total sales" value={formatCurrency(shift.totalSales)} />
        <StatRow label="Total profit" value={formatCurrency(shift.totalProfit)} />

        <View className="my-2 h-px bg-slate-100" />

        <StatRow label="Cash sales" value={formatCurrency(shift.cashSales)} />
        <StatRow label="Card sales" value={formatCurrency(shift.cardSales)} />
        {shift.voucherSales > 0 ? (
          <StatRow label="Voucher sales" value={formatCurrency(shift.voucherSales)} />
        ) : null}

        <View className="my-2 h-px bg-slate-100" />

        <StatRow label="Pay-ins" value={formatCurrency(shift.totalPayIns)} />
        <StatRow label="Pay-outs" value={formatCurrency(shift.totalPayOuts)} />
        <StatRow label="Receipts" value={String(shift.receiptCount)} />
      </View>

      <Pressable
        className="items-center rounded-xl bg-red-600 py-3.5 md:py-4"
        onPress={onClose}
      >
        <Text className="text-sm font-bold text-white md:text-base">
          Close Shift
        </Text>
      </Pressable>
    </ScrollView>
  );
}

function StatRow({
  label,
  value,
  highlight,
}: {
  label: string;
  value: string;
  highlight?: boolean;
}) {
  return (
    <View className="flex-row justify-between py-1.5 md:py-2">
      <Text className="text-sm text-slate-500 md:text-base">{label}</Text>
      <Text
        className={`text-sm font-semibold md:text-base ${
          highlight
            ? "text-lg font-extrabold text-emerald-600 md:text-xl"
            : "text-slate-700"
        }`}
      >
        {value}
      </Text>
    </View>
  );
}
