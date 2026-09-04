import { useState } from 'react';
import { StoreMapCanvas } from '../components/maps/StoreMapCanvas';
import { useNearbyStoresQuery, useSaveStoreMutation } from '../hooks/queries/useStoreQueries';
import { useDebounce } from '../hooks/useDebounce';
import { SlideOver } from '../components/Ui/SlideOver';
import { StoreForm } from '../components/forms/StoreForm'; 
import type { StoreResponse } from '../api/storeService';

export default function StoreMapDashboard() {
    const [radius, setRadius] = useState(5000); 
    const [center, setCenter] = useState<[number, number]>([41.7151, 44.8271]); 

    const [isSlideOverOpen, setIsSlideOverOpen] = useState(false);
    const [selectedStore, setSelectedStore] = useState<StoreResponse | null>(null);

    const debouncedRadius = useDebounce(radius, 500);

    const { data: stores = [], isLoading, isError } = useNearbyStoresQuery(center[0], center[1], debouncedRadius);
    const saveMutation = useSaveStoreMutation();

    const handleFormSubmit = async (formData: any) => {
        try {
            await saveMutation.mutateAsync({ 
                ...formData, 
                storeId: selectedStore?.storeId || 0 
            });
            setIsSlideOverOpen(false);
        } catch (err) { 
            console.error(err); 
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-end">
                <div>
                    <h2 className="text-2xl font-bold text-slate-800">Operational Territory</h2>
                    <p className="text-sm text-slate-500">Spatial radius filtering via SRID 4326.</p>
                </div>
                <button 
                    onClick={() => { setSelectedStore(null); setIsSlideOverOpen(true); }}
                    className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-md text-sm font-medium shadow-sm"
                >
                    + Register Store
                </button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                
                {/* Left Panel: The Map Canvas */}
                <div className="lg:col-span-2 space-y-4">
                    <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200 flex items-center space-x-4">
                        <label className="text-sm font-medium text-slate-700 whitespace-nowrap">Search Radius: {radius / 1000} km</label>
                        <input 
                            type="range" 
                            min="1000" 
                            max="50000" 
                            step="1000" 
                            value={radius}
                            onChange={(e) => setRadius(Number(e.target.value))}
                            className="w-full h-2 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-emerald-600"
                        />
                        {/* Visual indicator that the engine is processing the debounced network request */}
                        {isLoading && radius === debouncedRadius && <span className="text-xs text-slate-400">Syncing...</span>}
                    </div>
                    <StoreMapCanvas center={center} radius={radius} stores={stores} />
                </div>

                {/* Right Panel: The Data Grid */}
                <div className="bg-white border border-slate-200 rounded-lg shadow-sm flex flex-col h-[575px]">
                    <div className="p-4 border-b border-slate-200 bg-slate-50 shrink-0">
                        <h3 className="font-bold text-slate-800">Stores in Radius</h3>
                        <p className="text-xs text-slate-500">{stores.length} locations found</p>
                    </div>
                    
                    <div className="flex-1 overflow-y-auto p-4 space-y-3">
                        {/* ... mapping logic ... */}
                        {stores.map(store => (
                            <div key={store.storeId} className="p-3 border border-slate-200 rounded">
                                <div className="flex justify-between items-start">
                                    <h4 
                                        className="font-bold text-sm text-slate-800 cursor-pointer hover:text-emerald-600"
                                        onClick={() => setCenter([store.latitude, store.longitude])} 
                                    >
                                        {store.name}
                                    </h4>
                                    <button 
                                        onClick={() => { setSelectedStore(store); setIsSlideOverOpen(true); }}
                                        className="text-xs font-medium text-blue-600 hover:text-blue-800"
                                    >
                                        Edit
                                    </button>
                                </div>
                                <p className="text-xs text-slate-500 mt-1">{store.address}</p>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            {/* CRUD SLIDEOVER */}
            <SlideOver 
                isOpen={isSlideOverOpen} 
                onClose={() => setIsSlideOverOpen(false)} 
                title={selectedStore ? `Edit ${selectedStore.name}` : "Register Store"}
            >
                <StoreForm 
                    initialData={selectedStore} 
                    onSubmit={handleFormSubmit} 
                    onCancel={() => setIsSlideOverOpen(false)} 
                />
            </SlideOver>
        </div>
    );
}