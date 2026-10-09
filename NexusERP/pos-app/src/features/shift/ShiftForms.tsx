import React, { useState } from "react";
import { ActivityIndicator, Alert, FlatList, Pressable, Text, TextInput, View } from "react-native";
import { X } from "lucide-react-native";
import { useCashMovementMutation, useCloseShiftMutation, useOpenShiftMutation, useShiftHistoryQuery } from "@/hooks/queries/usePosQueries";
import { getErrorMessage } from "@/api/client";
import { formatCurrency } from "@/utils/currency";

export function OpenShiftForm({ storeId }: { storeId: number }) {
  const [startingCash, setStartingCash] = useState("");
  const [note, setNote] = useState("");
  const openShiftMutation = useOpenShiftMutation();

  const handleOpen = async () => {
    const parsed = parseFloat(startingCash);

    if (!Number.isFinite(parsed) || parsed < 0) {
      Alert.alert("Invalid amount", "Enter a valid starting cash amount.");
      return;
    }

    try {
      await openShiftMutation.mutateAsync({
        storeId,
        startingCash: parsed,
        note,
      });
    } catch (err) {
      Alert.alert("Could not open shift", getErrorMessage(err));
    }
  };

  return (
    <View className="flex-1 items-center justify-center p-6">
      <View className="w-full max-w-[360px] rounded-2xl bg-white p-6 shadow-md md:max-w-[420px] md:p-8">
        <Text className="mb-4 text-base font-extrabold text-slate-800 md:text-xl">
          Open Shift
        </Text>

        <FieldLabel>Starting cash</FieldLabel>
        <TextInput
          value={startingCash}
          onChangeText={setStartingCash}
          keyboardType="decimal-pad"
          placeholder="0.00"
          className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
        />

        <FieldLabel>Note (optional)</FieldLabel>
        <TextInput
          value={note}
          onChangeText={setNote}
          placeholder="e.g. Morning shift"
          className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
        />

        <Pressable
          className="mt-6 items-center rounded-lg bg-emerald-600 py-3 md:py-3.5"
          onPress={handleOpen}
          disabled={openShiftMutation.isPending}
        >
          {openShiftMutation.isPending ? (
            <ActivityIndicator color="#ffffff" />
          ) : (
            <Text className="font-bold text-white md:text-base">Open Shift</Text>
          )}
        </Pressable>
      </View>
    </View>
  );
}

export function CashMovementForm({
  type,
  storeId,
  shiftId,
  onDone,
}: {
  type: "in" | "out";
  storeId: number;
  shiftId: number;
  onDone: () => void;
}) {
  const [amount, setAmount] = useState("");
  const [reason, setReason] = useState("");
  const mutation = useCashMovementMutation();

  const handleSubmit = async () => {
    const parsed = parseFloat(amount);

    if (!Number.isFinite(parsed) || parsed <= 0) {
      Alert.alert("Invalid amount", "Enter an amount greater than zero.");
      return;
    }

    try {
      await mutation.mutateAsync({
        shiftId,
        payload: {
          storeId,
          movementType: type === "in" ? "PayIn" : "PayOut",
          amount: parsed,
          reason,
        },
      });
      onDone();
    } catch (err) {
      Alert.alert("Failed", getErrorMessage(err));
    }
  };

  return (
    <View>
      <Text className="mb-4 text-base font-extrabold text-slate-800 md:text-xl">
        {type === "in" ? "Cash In" : "Cash Out"}
      </Text>

      <FieldLabel>Amount</FieldLabel>
      <TextInput
        value={amount}
        onChangeText={setAmount}
        keyboardType="decimal-pad"
        placeholder="0.00"
        autoFocus
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
      />

      <FieldLabel>Comment</FieldLabel>
      <TextInput
        value={reason}
        onChangeText={setReason}
        placeholder={
          type === "in" ? "e.g. Change float top-up" : "e.g. Petty cash purchase"
        }
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
      />

      <Pressable
        className="mt-6 items-center rounded-lg bg-emerald-600 py-3 md:py-3.5"
        onPress={handleSubmit}
        disabled={mutation.isPending}
      >
        {mutation.isPending ? (
          <ActivityIndicator color="#ffffff" />
        ) : (
          <Text className="font-bold text-white md:text-base">Confirm</Text>
        )}
      </Pressable>
    </View>
  );
}

export function CloseShiftForm({
  shiftId,
  onDone,
}: {
  shiftId: number;
  onDone: () => void;
}) {
  const [actualCash, setActualCash] = useState("");
  const [note, setNote] = useState("");
  const mutation = useCloseShiftMutation();

  const handleSubmit = async () => {
    const parsed = parseFloat(actualCash);

    if (!Number.isFinite(parsed) || parsed < 0) {
      Alert.alert("Invalid amount", "Enter the actual cash counted.");
      return;
    }

    try {
      const result = await mutation.mutateAsync({
        shiftId,
        payload: {
          actualEndingCash: parsed,
          note,
        },
      });

      Alert.alert(
        "Shift closed",
        result.variance === 0
          ? "Cash matched exactly."
          : `Variance: ${result.variance > 0 ? "+" : ""}${formatCurrency(
              result.variance
            )}`
      );
      onDone();
    } catch (err) {
      Alert.alert("Failed", getErrorMessage(err));
    }
  };

  return (
    <View>
      <Text className="mb-4 text-base font-extrabold text-slate-800 md:text-xl">
        Close Shift
      </Text>

      <FieldLabel>Actual ending cash counted</FieldLabel>
      <TextInput
        value={actualCash}
        onChangeText={setActualCash}
        keyboardType="decimal-pad"
        placeholder="0.00"
        autoFocus
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
      />

      <FieldLabel>Note (optional)</FieldLabel>
      <TextInput
        value={note}
        onChangeText={setNote}
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
      />

      <Pressable
        className="mt-6 items-center rounded-lg bg-red-600 py-3 md:py-3.5"
        onPress={handleSubmit}
        disabled={mutation.isPending}
      >
        {mutation.isPending ? (
          <ActivityIndicator color="#ffffff" />
        ) : (
          <Text className="font-bold text-white md:text-base">Close Shift</Text>
        )}
      </Pressable>
    </View>
  );
}

export function ShiftHistoryList({
  storeId,
  onClose,
}: {
  storeId: number;
  onClose: () => void;
}) {
  const { data, isLoading } = useShiftHistoryQuery(storeId, true);

  return (
    <View>
      <View className="mb-3 flex-row items-center justify-between">
        <Text className="text-base font-extrabold text-slate-800 md:text-xl">
          Shift History
        </Text>
        <Pressable onPress={onClose}>
          <X size={20} color="#64748b" />
        </Pressable>
      </View>

      {isLoading ? (
        <ActivityIndicator color="#059669" className="mt-4" />
      ) : (
        <FlatList
          data={data || []}
          keyExtractor={(shift) => String(shift.shiftId)}
          style={{ maxHeight: 420 }}
          ListEmptyComponent={
            <Text className="py-6 text-center text-slate-400 md:text-sm">
              No past shifts yet.
            </Text>
          }
          renderItem={({ item }) => (
            <View className="flex-row justify-between border-b border-slate-100 py-3 md:py-4">
              <View className="flex-1">
                <Text className="text-sm font-bold text-slate-800 md:text-base">
                  {item.cashierName}
                </Text>
                <Text className="mt-0.5 text-xs text-slate-500 md:text-sm">
                  {new Date(item.startDate).toLocaleString()}
                </Text>
                {item.notes ? (
                  <Text className="mt-0.5 text-xs italic text-slate-400 md:text-sm">
                    &quot;{item.notes}&quot;
                  </Text>
                ) : null}
              </View>

              <View className="items-end">
                <Text className="mr-1 text-sm font-extrabold text-emerald-600 md:text-base">
                  {formatCurrency(item.totalSales)}
                </Text>
                <View
                  className={`mt-1 rounded-full px-2 py-0.5 ${
                    item.status === "Open" ? "bg-amber-100" : "bg-slate-100"
                  }`}
                >
                  <Text
                    className={`text-[10px] font-bold md:text-xs ${
                      item.status === "Open" ? "text-amber-800" : "text-slate-600"
                    }`}
                  >
                    {item.status}
                  </Text>
                </View>
              </View>
            </View>
          )}
        />
      )}
    </View>
  );
}

function FieldLabel({ children }: { children: React.ReactNode }) {
  return (
    <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500 md:text-sm">
      {children}
    </Text>
  );
}
