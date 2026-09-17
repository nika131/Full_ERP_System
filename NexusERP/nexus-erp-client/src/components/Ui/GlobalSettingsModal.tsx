import { useState, useEffect } from "react";
import apiClient from "../../api/apiClient";

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
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-md overflow-hidden">
                <div className="p-4 border-b border-slate-200 flex justify-between items-center">
                    <h3 className="font-bold text-lg text-slate-800">System Settings</h3>
                </div>
                
                <div className="p-4 space-y-2">
                    {/* Threshold Setting */}
                    <div>
                        <label className="block text-sm font-semibold text-slate-700 mb-1">
                            Global Low Stock Threshold
                        </label>
                        <input 
                            type="number" 
                            value={threshold}
                            onChange={(e) => setThreshold(e.target.value)}
                            className="w-full border border-slate-300 rounded p-2 outline-none focus:border-emerald-500"
                        />
                        <p className="text-xs text-slate-500 mt-1">
                            Used for any product that does not have a custom threshold assigned.
                        </p>
                    </div>

                    {/* Negative Inventory Setting */}
                    <div className="flex items-center justify-between pt-4 border-t border-slate-100">
                        <div className="pr-4">
                            <label className="block text-sm font-semibold text-slate-700">
                                Allow Negative Inventory
                            </label>
                            <p className="text-xs text-slate-500 mt-1">
                                Allow stock deductions (loss/damage/sales) to push quantities below zero.
                            </p>
                        </div>
                        <input 
                            type="checkbox"
                            checked={allowNegative}
                            onChange={(e) => setAllowNegative(e.target.checked)}
                            className="w-5 h-5 accent-emerald-600 rounded cursor-pointer shrink-0"
                        />
                    </div>

                    {/* Discount Policy Setting */}
                    <div className="flex items-center justify-between pt-4 border-t border-slate-100">
                        <div className="pr-4">
                            <label className="block text-sm font-semibold text-slate-700">
                                Manual Discount Policy
                            </label>
                            <p className="text-xs text-slate-500 mt-1">
                                Control who can apply manual cart and item discounts during checkout.
                            </p>
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

                <div className="p-4 bg-slate-50 flex justify-end gap-2 border-t border-slate-200">
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