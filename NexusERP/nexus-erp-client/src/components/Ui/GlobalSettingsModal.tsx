import { useState, useEffect } from "react";
import apiClient from "../../api/apiClient";
import { InfoTooltip } from "./InfoTooltip";

export function GlobalSettingsModal({ onClose }: { onClose: () => void }) {
    const [threshold, setThreshold] = useState("5");
    const [allowNegative, setAllowNegative] = useState(false);
    const [discountPolicy, setDiscountPolicy] = useState("Enabled");
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        Promise.all([
            apiClient.get("/settings/GlobalLowStockThreshold").catch(() => ({ data: { value: "5" } })),
            apiClient.get("/settings/AllowNegativeInventory").catch(() => ({ data: { value: "false" } })),
            apiClient.get("/settings/DiscountPolicy").catch(() => ({ data: { value: "Enabled" } }))
        ]).then(([thresholdRes, negativeRes, policyRes]) => {
            if (thresholdRes?.data?.value) setThreshold(thresholdRes.data.value);
            if (negativeRes?.data?.value) setAllowNegative(negativeRes.data.value === "true" || negativeRes.data.value === true);
            if (policyRes?.data?.value) setDiscountPolicy(policyRes.data.value);
        });
    }, []);

    const handleSave = async () => {
        setIsLoading(true);
        try {
            await Promise.all([
                apiClient.put("/settings/GlobalLowStockThreshold", { value: threshold.toString() }),
                apiClient.put("/settings/AllowNegativeInventory", { value: allowNegative.toString().toLowerCase() }),
                apiClient.put("/settings/DiscountPolicy", { value: discountPolicy })
            ]);
            onClose();
        } catch (error) {
            console.error("Failed to save settings", error);
        } finally {
            setIsLoading(false);
        }
    };

    return (
       <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-3 sm:p-4">
            <div
                role="dialog"
                aria-modal="true"
                aria-label="System Settings"
                className="flex max-h-[calc(100dvh-1.5rem)] w-full max-w-md flex-col overflow-hidden rounded-xl bg-white shadow-xl"
            >
                <div className="p-4 border-b border-slate-200 flex justify-between items-center">
                    <h3 className="font-bold text-lg text-slate-800">System Settings</h3>
                </div>
                
                <div className="tenex-scroll min-h-0 flex-1 space-y-4 overflow-y-auto p-4 text-left">
                    {/* Threshold Setting */}
                    <div>
                        <div className="mb-1 flex items-center gap-1.5">
                            <label className="text-sm font-semibold text-slate-700">
                                Global Low Stock Threshold
                            </label>
                            <InfoTooltip text="The default low-stock alert level for products without a custom threshold. For example, a value of 5 marks products as low-stock when their quantity reaches 5 or fewer." />
                        </div>
                        <input 
                            type="number" 
                            value={threshold}
                            onChange={(e) => setThreshold(e.target.value)}
                            className="w-full border border-slate-300 rounded p-2 outline-none focus:border-emerald-500"
                        />
                    </div>

                    {/* Negative Inventory Setting */}
                    <div className="flex items-center justify-between pt-4 border-t border-slate-100">
                        <div className="flex items-center gap-1.5">
                            <label className="text-sm font-semibold text-slate-700">
                                Allow Negative Inventory
                            </label>
                            <InfoTooltip text="When enabled, stock deductions and sales can reduce product quantities below zero. When disabled, operations that would create negative inventory are blocked." />
                        </div>
                        <input 
                            type="checkbox"
                            checked={allowNegative}
                            onChange={(e) => setAllowNegative(e.target.checked)}
                            className="w-5 h-5 accent-emerald-600 rounded cursor-pointer shrink-0"
                        />
                    </div>

                    {/* Discount Policy Setting */}
                    <div className="flex flex-col gap-3 border-t border-slate-100 pt-4 sm:flex-row sm:items-center sm:justify-between">
                        <div className="flex items-center gap-1.5">
                            <label className="text-sm font-semibold text-slate-700">
                                Manual Discount Policy
                            </label>
                            <InfoTooltip text="Enabled allows manual checkout discounts within configured limits. Admin Only restricts them to administrators. Disabled prevents manual discounts. Automatic market discounts are unaffected." />
                        </div>
                        <select 
                            value={discountPolicy}
                            onChange={(e) => setDiscountPolicy(e.target.value)}
                            className="border border-slate-300 rounded p-1.5 text-sm outline-none focus:border-emerald-500 bg-white cursor-pointer shrink-0"
                        >
                            <option value="Enabled">Enabled (All)</option>
                            <option value="AdminOnly">Admin Only</option>
                            <option value="Disabled">Disabled</option>
                        </select>
                    </div>
                </div>

                <div className="flex shrink-0 justify-end gap-2 border-t border-slate-200 bg-slate-50 p-4">
                    <button 
                        onClick={onClose}
                        className="px-4 py-2 text-sm font-semibold text-slate-600 bg-white border border-slate-300 rounded hover:bg-slate-100"
                    >
                        Cancel
                    </button>
                    <button 
                        onClick={handleSave}
                        disabled={isLoading}
                        className="px-4 py-2 text-sm font-semibold text-white bg-emerald-600 rounded hover:bg-emerald-700 disabled:opacity-50"
                    >
                        {isLoading ? "Saving..." : "Save Changes"}
                    </button>
                </div>
            </div>
        </div>
    );
}