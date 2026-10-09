import React, { useState } from "react";
import { ActivityIndicator, Pressable, View } from "react-native";
import { History, Store } from "lucide-react-native";
import { useTerminalStore } from "@/store/terminalStore";
import { useCurrentShiftQuery } from "@/hooks/queries/usePosQueries";
import { ModalSheet } from "@/components/ModalSheet";
import { MenuButton } from "@/components/MenuButton";
import { AppScreen } from "@/components/layout/AppScreen";
import { ScreenHeader } from "@/components/layout/ScreenHeader";
import { ActiveShiftView, StorePickerView } from "@/features/shift/ShiftViews";
import { CashMovementForm, CloseShiftForm, OpenShiftForm, ShiftHistoryList } from "@/features/shift/ShiftForms";

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
      <AppScreen>
        <StorePickerView
          onPicked={(store) => {
            setStore(
              store.storeId,
              store.name,
              store.maxCartDiscountPercentage
            );
            setStorePickerOpen(false);
          }}
        />
      </AppScreen>
    );
  }

  return (
    <AppScreen>
      <ScreenHeader
        title={storeName ?? "Store"}
        left={
          <>
            <MenuButton />
            <Store size={18} color="#64748b" />
          </>
        }
        right={
          <Pressable
            onPress={() => setHistoryOpen(true)}
            className="h-9 w-9 items-center justify-center rounded-lg bg-slate-50"
          >
            <History size={20} color="#475569" />
          </Pressable>
        }
      />

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
      </View>

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

      <ModalSheet visible={cashModal !== null} onClose={() => setCashModal(null)}>
        {cashModal && shift ? (
          <CashMovementForm
            type={cashModal}
            storeId={storeId}
            shiftId={shift.shiftId}
            onDone={() => setCashModal(null)}
          />
        ) : null}
      </ModalSheet>

      <ModalSheet
        visible={closeModalOpen}
        onClose={() => setCloseModalOpen(false)}
      >
        {shift ? (
          <CloseShiftForm
            shiftId={shift.shiftId}
            onDone={() => setCloseModalOpen(false)}
          />
        ) : null}
      </ModalSheet>
    </AppScreen>
  );
}
