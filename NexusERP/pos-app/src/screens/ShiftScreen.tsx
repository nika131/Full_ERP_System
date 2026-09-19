import React, { useState } from "react";
import { View, Text, Pressable, TextInput, ScrollView, ActivityIndicator, FlatList, Alert, useWindowDimensions } from "react-native";
import { History, ArrowDownCircle, ArrowUpCircle, Store, X } from "lucide-react-native";
import { useTerminalStore } from "../store/terminalStore";
import { useCurrentShiftQuery, useOpenShiftMutation, useCloseShiftMutation, useCashMovementMutation, useShiftHistoryQuery, useStoresQuery } from "../hooks/queries/usePosQueries";
import { ModalSheet } from "../components/ModalSheet";
import { getErrorMessage } from "../api/client";
import { formatCurrency } from "../utils/currency";
import type { StoreLookup } from "../types";
import { MenuButton } from "@/components/MenuButton";

export function ShiftScreen() {
  const { storeId, storeName, setStore } = useTerminalStore();
  const [storePickerOpen, setStorePickerOpen] = useState(!storeId);
  const [historyOpen, setHistoryOpen] = useState(false);
  const [cashModal, setCashModal] = useState<"in" | "out" | null>(null);
  const [closeModalOpen, setCloseModalOpen] = useState(false);

  const { data: shiftData, isLoading } = useCurrentShiftQuery(storeId);
  const shift = shiftData?.shift ?? null;

  if (storePickerOpen || !storeId) {
    return (
      <StorePickerScreen
        onPicked={(s) => {
          setStore(s.storeId, s.name, s.maxCartDiscountPercentage);
          setStorePickerOpen(false);
        }}
      />
    );
  }

  return (
    <View className="flex-1 bg-slate-50">
      <View className="flex-row items-center justify-between border-b border-slate-200 bg-white p-4">
        <View className="flex-row items-center gap-3">
          <MenuButton />
          <View className="flex-row items-center gap-1.5">
            <Store size={18} color="#64748b" />
            <Text className="text-sm font-bold text-slate-700 md:text-base">{storeName}</Text>
          </View>
        </View>

        <Pressable
          onPress={() => setHistoryOpen(true)}
          className="h-9 w-9 items-center justify-center rounded-lg bg-slate-50"
        >
          <History size={20} color="#475569" />
        </Pressable>
      </View>

      <View className="w-full max-w-5xl flex-1 self-center">
        {isLoading ? (
          <View className="flex-1 items-center justify-center">
            <ActivityIndicator color="#059669" />
          </View>
        ) : shift ? (
          <ActiveShiftView
            shift={shift}
            onCashIn={() => setCashModal("in")}
            onCashOut={() => setCashModal("out")}
            onClose={() => setCloseModalOpen(true)}
          />
        ) : (
          <OpenShiftForm storeId={storeId} />
        )}

        <ModalSheet
          visible={historyOpen}
          onClose={() => setHistoryOpen(false)}
          maxWidth={480}
        >
          <ShiftHistoryList
            storeId={storeId}
            onClose={() => setHistoryOpen(false)}
          />
        </ModalSheet>

        <ModalSheet
          visible={cashModal !== null}
          onClose={() => setCashModal(null)}
        >
          {cashModal && shift && (
            <CashMovementForm
              type={cashModal}
              storeId={storeId}
              shiftId={shift.shiftId}
              onDone={() => setCashModal(null)}
            />
          )}
        </ModalSheet>

        <ModalSheet
          visible={closeModalOpen}
          onClose={() => setCloseModalOpen(false)}
        >
          {shift && (
            <CloseShiftForm
              shiftId={shift.shiftId}
              onDone={() => setCloseModalOpen(false)}
            />
          )}
        </ModalSheet>
      </View>
    </View>
  );
}

function StorePickerScreen({
  onPicked,
}: {
  onPicked: (s: StoreLookup) => void;
}) {
  const { data: stores, isLoading } = useStoresQuery();

  return (
    <View className="flex-1 bg-slate-50">
      <View className="flex-1 items-center justify-center gap-1.5 p-6">
        <Store size={36} color="#cbd5e1" />

        <Text className="mt-3 text-[17px] font-bold text-slate-700 md:text-xl">
          Which store is this device in?
        </Text>

        <Text className="text-center text-[13px] text-slate-400 md:text-sm">
          This is remembered on this device going forward.
        </Text>

        {isLoading ? (
          <ActivityIndicator
            color="#059669"
            className="mt-4"
          />
        ) : (
          <View className="mt-6 w-full max-w-[340px] gap-2">
            {(stores || []).map((s) => (
              <Pressable
                key={s.storeId}
                onPress={() => onPicked(s)}
                className="items-center rounded-lg border border-slate-200 bg-white p-3 md:p-4"
              >
                <Text className="font-semibold text-slate-700 md:text-base">
                  {s.name}
                </Text>
              </Pressable>
            ))}
          </View>
        )}
      </View>
    </View>
  );
}

function ActiveShiftView({
  shift,
  onCashIn,
  onCashOut,
  onClose,
}: {
  shift: NonNullable<ReturnType<typeof useCurrentShiftQuery>["data"]>["shift"];
  onCashIn: () => void;
  onCashOut: () => void;
  onClose: () => void;
}) {
  if (!shift) return null;

  return (
    <ScrollView
      contentContainerClassName="gap-4 p-4 md:gap-5 md:p-6"
    >
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

        <StatRow
          label="Started"
          value={new Date(shift.startDate).toLocaleString()}
        />

        <StatRow
          label="Starting cash"
          value={formatCurrency(shift.startingCash)}
        />

        <StatRow
          label="Expected cash now"
          value={formatCurrency(shift.expectedEndingCash)}
          highlight
        />

        <View className="my-2 h-px bg-slate-100" />

        <StatRow
          label="Total sales"
          value={formatCurrency(shift.totalSales)}
        />

        <StatRow
          label="Total profit"
          value={formatCurrency(shift.totalProfit)}
        />

        <View className="my-2 h-px bg-slate-100" />

        <StatRow
          label="Cash sales"
          value={formatCurrency(shift.cashSales)}
        />

        <StatRow
          label="Card sales"
          value={formatCurrency(shift.cardSales)}
        />

        {shift.voucherSales > 0 && (
          <StatRow
            label="Voucher sales"
            value={formatCurrency(shift.voucherSales)}
          />
        )}

        <View className="my-2 h-px bg-slate-100" />

        <StatRow
          label="Pay-ins"
          value={formatCurrency(shift.totalPayIns)}
        />

        <StatRow
          label="Pay-outs"
          value={formatCurrency(shift.totalPayOuts)}
        />

        <StatRow
          label="Receipts"
          value={String(shift.receiptCount)}
        />
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
      <Text className="text-sm text-slate-500 md:text-base">
        {label}
      </Text>

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

function OpenShiftForm({ storeId }: { storeId: number }) {
  const [startingCash, setStartingCash] = useState("");
  const [note, setNote] = useState("");
  const openShiftMutation = useOpenShiftMutation();

  const handleOpen = async () => {
    const parsed = parseFloat(startingCash);

    if (!Number.isFinite(parsed) || parsed < 0) {
      Alert.alert(
        "Invalid amount",
        "Enter a valid starting cash amount."
      );
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

        <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500 md:text-sm">
          Starting cash
        </Text>

        <TextInput
          value={startingCash}
          onChangeText={setStartingCash}
          keyboardType="decimal-pad"
          placeholder="0.00"
          className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
        />

        <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500 md:text-sm">
          Note (optional)
        </Text>

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
            <Text className="font-bold text-white md:text-base">
              Open Shift
            </Text>
          )}
        </Pressable>
      </View>
    </View>
  );
}

function CashMovementForm({
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
      Alert.alert(
        "Invalid amount",
        "Enter an amount greater than zero."
      );
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

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500 md:text-sm">
        Amount
      </Text>

      <TextInput
        value={amount}
        onChangeText={setAmount}
        keyboardType="decimal-pad"
        placeholder="0.00"
        autoFocus
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
      />

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500 md:text-sm">
        Comment
      </Text>

      <TextInput
        value={reason}
        onChangeText={setReason}
        placeholder={
          type === "in"
            ? "e.g. Change float top-up"
            : "e.g. Petty cash purchase"
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
          <Text className="font-bold text-white md:text-base">
            Confirm
          </Text>
        )}
      </Pressable>
    </View>
  );
}

function CloseShiftForm({
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
      Alert.alert(
        "Invalid amount",
        "Enter the actual cash counted."
      );
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
          : `Variance: ${
              result.variance > 0 ? "+" : ""
            }${formatCurrency(result.variance)}`
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

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500 md:text-sm">
        Actual ending cash counted
      </Text>

      <TextInput
        value={actualCash}
        onChangeText={setActualCash}
        keyboardType="decimal-pad"
        placeholder="0.00"
        autoFocus
        className="rounded-lg border border-slate-300 px-3 py-2.5 text-[15px] md:text-base"
      />

      <Text className="mb-1.5 mt-3 text-xs font-semibold text-slate-500 md:text-sm">
        Note (optional)
      </Text>

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
          <Text className="font-bold text-white md:text-base">
            Close Shift
          </Text>
        )}
      </Pressable>
    </View>
  );
}

function ShiftHistoryList({
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
        <ActivityIndicator
          color="#059669"
          className="mt-4"
        />
      ) : (
        <FlatList
          data={data || []}
          keyExtractor={(s) => String(s.shiftId)}
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
                    "{item.notes}"
                  </Text>
                ) : null}
              </View>

              <View className="items-end">
                <Text className="mr-1 text-sm font-extrabold text-emerald-600 md:text-base">
                  {formatCurrency(item.totalSales)}
                </Text>

                <View
                  className={`mt-1 rounded-full px-2 py-0.5 ${
                    item.status === "Open"
                      ? "bg-amber-100"
                      : "bg-slate-100"
                  }`}
                >
                  <Text
                    className={`text-[10px] font-bold md:text-xs ${
                      item.status === "Open"
                        ? "text-amber-800"
                        : "text-slate-600"
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