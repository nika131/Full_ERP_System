import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { posService } from "../../api/posService";
import type {
  OpenShiftPayload,
  CloseShiftPayload,
  CashMovementPayload,
  CheckoutPayload,
} from "../../api/posService";

export const useCurrentShiftQuery = (storeId: number | null) =>
  useQuery({
    queryKey: ["pos", "currentShift", storeId],
    queryFn: () => posService.getCurrentShift(storeId as number),
    enabled: storeId !== null,
    refetchInterval: 30_000,
  });

export const useShiftHistoryQuery = (storeId: number | null, enabled: boolean) =>
  useQuery({
    queryKey: ["pos", "shiftHistory", storeId],
    queryFn: () => posService.getShiftHistory(storeId as number),
    enabled: enabled && storeId !== null,
  });

export const useShiftReceiptsQuery = (shiftId: number | null) =>
  useQuery({
    queryKey: ["pos", "shiftReceipts", shiftId],
    queryFn: () => posService.getShiftReceipts(shiftId as number),
    enabled: shiftId !== null,
  });

export const useReceiptDetailQuery = (receiptId: number | null) =>
  useQuery({
    queryKey: ["pos", "receiptDetail", receiptId],
    queryFn: () => posService.getReceiptDetail(receiptId as number),
    enabled: receiptId !== null,
  });

export const useStoresQuery = () =>
  useQuery({
    queryKey: ["pos", "stores"],
    queryFn: () => posService.getStores(),
    staleTime: 5 * 60 * 1000,
  });

export const useOpenShiftMutation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: OpenShiftPayload) => posService.openShift(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["pos", "currentShift"] }),
  });
};

export const useCloseShiftMutation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ shiftId, payload }: { shiftId: number; payload: CloseShiftPayload }) =>
      posService.closeShift(shiftId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["pos", "currentShift"] });
      qc.invalidateQueries({ queryKey: ["pos", "shiftHistory"] });
    },
  });
};

export const useCashMovementMutation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ shiftId, payload }: { shiftId: number; payload: CashMovementPayload }) =>
      posService.addCashMovement(shiftId, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["pos", "currentShift"] }),
  });
};

export const useCheckoutMutation = () => {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: (payload: CheckoutPayload) => posService.checkout(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["pos", "currentShift"] });
      qc.invalidateQueries({ queryKey: ["pos", "shiftReceipts"] });
    },
  });
};
