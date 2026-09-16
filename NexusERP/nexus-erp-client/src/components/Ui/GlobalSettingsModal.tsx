import { useState, useEffect } from "react";
import apiClient from "../../api/apiClient";

export function GlobalSettingsModal({ onClose }: { onClose: () => void }) {
    const [threshold, setThreshold] = useState("5");
    const [isLoading, setIsLoading] = useState(false);

    // Fetch the current setting when modal opens
    useEffect(() => {
        apiClient.get("/settings/GlobalLowStockThreshold").then(res => {
            setThreshold(res.data.value);
        });
    }, []);

    const handleSave = async () => {
        setIsLoading(true);
        await apiClient.put("/settings/GlobalLowStockThreshold", { value: threshold });
        setIsLoading(false);
        onClose();
    };

    return (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-sm overflow-hidden">
                <div className="p-4 border-b border-slate-200">
                    <h3 className="font-bold text-lg text-slate-800">System Settings</h3>
                </div>
                
                <div className="p-4 space-y-4">
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
                        <p className="text-xs text-slate-500 mt-2">
                            This number is used for any product that does not have a custom threshold assigned.
                        </p>
                    </div>
                </div>

                <div className="p-4 bg-slate-50 flex justify-end gap-2">
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