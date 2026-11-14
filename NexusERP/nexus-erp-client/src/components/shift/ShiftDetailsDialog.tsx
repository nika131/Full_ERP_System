import { useEffect, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import { X } from 'lucide-react';
import apiClient from '../../api/apiClient';
import type { ReactNode } from 'react'
import { HistoricalUserName } from '../Ui/HistoricalUserName';

interface CashMovement {
    movementId: number;
    userId: number;
    userName: string | null;
    movementType: 'PayIn' | 'PayOut';
    amount: number;
    reason: string | null;
    createdAt: string;
    userDeletedRecently: boolean
}

interface ShiftDetails {
    shiftId: number;
    storeName: string;
    cashierName: string;
    status: string;
    startDate: string;
    endDate: string | null;
    startingCash: number;
    expectedEndingCash: number;
    actualEndingCash: number | null;
    variance: number | null;
    totalSales: number;
    totalProfit: number;
    cashSales: number;
    cardSales: number;
    voucherSales: number;
    totalPayIns: number;
    totalPayOuts: number;
    receiptCount: number;
    notes: string | null;
    movements: CashMovement[];
    cashierDeletedRecently: boolean
}

const money = (value: number) => `$${value.toFixed(2)}`;

const dateTime = (value: string) => {
    const hasTimezone = /Z$|[+-]\d{2}:\d{2}$/.test(value);
    return new Date(hasTimezone ? value : `${value}Z`)
        .toLocaleString();
};

function Stat({
    label,
    value
}: {
    label: string;
    value: ReactNode;
}) {
    return (
        <div className="flex justify-between gap-4 py-2 text-sm">
            <span className="text-slate-500">{label}</span>
            <span className="text-right font-semibold text-slate-800">
                {value}
            </span>
        </div>
    );
}

export function ShiftDetailsDialog({
    shiftId,
    onClose
}: {
    shiftId: number | null;
    onClose: () => void;
}) {
    const dialogRef = useRef<HTMLDialogElement>(null);

    const { data, isPending, isError, refetch } = useQuery({
        queryKey: ['shift-details', shiftId],
        enabled: shiftId !== null,
        queryFn: async () => {
            const response = await apiClient.get<ShiftDetails>(
                `/reports/shifts/${shiftId}`
            );
            return response.data;
        },
        staleTime: 0
    });

    useEffect(() => {
        const dialog = dialogRef.current;
        if (!dialog) return;

        if (shiftId !== null && !dialog.open) {
            dialog.showModal();
        } else if (shiftId === null && dialog.open) {
            dialog.close();
        }
    }, [shiftId]);

    return (
        <dialog
            ref={dialogRef}
            aria-labelledby="shift-details-heading"
            onClose={onClose}
            onClick={event => {
                if (event.target === event.currentTarget) {
                    event.currentTarget.close();
                }
            }}
            className="fixed inset-0 m-auto w-[95vw] max-w-3xl max-h-[90dvh] overflow-y-auto rounded-2xl bg-white p-0 text-slate-800 shadow-2xl backdrop:bg-slate-950/60"
        >
            <header className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
                <h2
                    id="shift-details-heading"
                    className="text-lg font-bold"
                >
                    Shift #{shiftId}
                </h2>

                <button
                    type="button"
                    onClick={() => dialogRef.current?.close()}
                    aria-label="Close shift details"
                    className="rounded-lg p-2 hover:bg-slate-100"
                >
                    <X size={20} />
                </button>
            </header>

            <div className="space-y-6 p-5 sm:p-6">
                {isPending && (
                    <p className="py-10 text-center text-slate-500">
                        Loading shift details...
                    </p>
                )}

                {isError && (
                    <div className="py-10 text-center">
                        <p>Could not load shift details.</p>
                        <button
                            type="button"
                            onClick={() => void refetch()}
                            className="mt-3 font-semibold text-emerald-700"
                        >
                            Try again
                        </button>
                    </div>
                )}

                {data && !isError && (
                    <>
                        <section className="rounded-xl bg-slate-50 p-4">
                            <div className="mb-3 flex items-center justify-between gap-3">
                                <h3 className="font-bold">
                                    {data.storeName}
                                </h3>
                                <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold">
                                    {data.status}
                                </span>
                            </div>

                            <Stat label="Opened by" value={
                                <HistoricalUserName
                                name={data.cashierName}
                                deletedRecently={data.cashierDeletedRecently}
                                />
                            } />
                            <Stat label="Started" value={dateTime(data.startDate)} />
                            {data.endDate && (
                                <Stat label="Closed" value={dateTime(data.endDate)} />
                            )}
                            {data.notes && (
                                <p className="mt-3 whitespace-pre-wrap text-sm text-slate-600">
                                    {data.notes}
                                </p>
                            )}
                        </section>

                        <section>
                            <h3 className="mb-3 text-base font-bold">
                                Shift summary
                            </h3>

                            <div className="grid gap-x-8 md:grid-cols-2">
                                <div>
                                    <Stat label="Starting cash" value={money(data.startingCash)} />
                                    <Stat label="Expected ending cash" value={money(data.expectedEndingCash)} />
                                    {data.actualEndingCash !== null && (
                                        <Stat label="Actual ending cash" value={money(data.actualEndingCash)} />
                                    )}
                                    {data.variance !== null && (
                                        <Stat label="Cash variance" value={money(data.variance)} />
                                    )}
                                </div>

                                <div>
                                    <Stat label="Total sales" value={money(data.totalSales)} />
                                    <Stat label="Total profit" value={money(data.totalProfit)} />
                                    <Stat label="Receipts" value={String(data.receiptCount)} />
                                </div>
                            </div>
                        </section>

                        <section>
                            <h3 className="mb-3 text-base font-bold">
                                Payment breakdown
                            </h3>

                            <div className="grid gap-x-8 md:grid-cols-2">
                                <Stat label="Cash sales" value={money(data.cashSales)} />
                                <Stat label="Card sales" value={money(data.cardSales)} />
                                <Stat label="Voucher sales" value={money(data.voucherSales)} />
                            </div>
                        </section>

                        <section>
                            <h3 className="mb-3 text-base font-bold">
                                Cash movements
                            </h3>

                            <div className="mb-4 grid grid-cols-2 gap-3">
                                <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4">
                                    <p className="text-xs font-medium text-emerald-700">
                                        Total cash in
                                    </p>
                                    <p className="mt-1 text-xl font-bold text-emerald-800">
                                        {money(data.totalPayIns)}
                                    </p>
                                </div>

                                <div className="rounded-xl border border-red-200 bg-red-50 p-4">
                                    <p className="text-xs font-medium text-red-700">
                                        Total cash out
                                    </p>
                                    <p className="mt-1 text-xl font-bold text-red-800">
                                        {money(data.totalPayOuts)}
                                    </p>
                                </div>
                            </div>

                            {data.movements.length === 0 ? (
                                <p className="rounded-xl border border-slate-200 p-5 text-center text-sm text-slate-500">
                                    No cash movements recorded.
                                </p>
                            ) : (
                                <div className="divide-y divide-slate-100 rounded-xl border border-slate-200">
                                    {data.movements.map(movement => {
                                        const isIn = movement.movementType === 'PayIn';

                                        return (
                                            <div
                                                key={movement.movementId}
                                                className="flex flex-col justify-between gap-3 p-4 sm:flex-row"
                                            >
                                                <div className="min-w-0">
                                                    <p className={`font-bold ${isIn ? 'text-emerald-700' : 'text-red-700'}`}>
                                                        {isIn ? 'Cash In' : 'Cash Out'}
                                                    </p>

                                                    <p className="mt-1 whitespace-pre-wrap break-words text-sm text-slate-700">
                                                        {movement.reason?.trim() || 'No reason recorded'}
                                                    </p>

                                                    <p className="mt-2 text-xs text-slate-500">
                                                        <HistoricalUserName
                                                            name={movement.userName || 'Deleted user'}
                                                            deletedRecently={movement.userDeletedRecently}
                                                        />
                                                        {' · '}ID: {movement.userId}
                                                        {' · '}{dateTime(movement.createdAt)}
                                                    </p>
                                                </div>

                                                <p className={`shrink-0 text-lg font-bold ${isIn ? 'text-emerald-700' : 'text-red-700'}`}>
                                                    {isIn ? '+' : '-'}{money(movement.amount)}
                                                </p>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </section>
                    </>
                )}
            </div>
        </dialog>
    );
}
