import { useMemo, useState } from 'react';
import { DataTable, type ColumnDef } from '../components/Ui/DataTable';
import { SlideOver } from '../components/Ui/SlideOver';
import { StoreForm } from '../components/forms/StoreForm';
import { StoreMapCanvas } from '../components/maps/StoreMapCanvas';
import { useAllStoresQuery, useCreateStoreMutation, useLookupStoresQuery, usePagedStoresQuery, useUpdateStoreMutation } from '../hooks/queries/useStoreQueries';
import type { StoreResponse } from '../api/storeService';
import type { StoreFormData } from '../schemas/storeSchema';

export default function StoreList() {
    const [page, setPage] = useState(1);
    const [searchTerm, setSearchTerm] = useState('');
    const [isSlideOverOpen, setIsSlideOverOpen] = useState(false);
    const [selectedStore, setSelectedStore] = useState<StoreResponse | null>(null);

    // Hardcode a default map center (Tbilisi) to show all locations
    const mapCenter: [number, number] = [41.7151, 44.8271];

    const { data: allStores = [] } = useLookupStoresQuery();

    const { data: pagedData, isLoading, isError } = usePagedStoresQuery(page, 10, searchTerm);

    const createMutation = useCreateStoreMutation();
    const updateMutation = useUpdateStoreMutation();

    const stores = pagedData?.items || [];
    const totalCount = pagedData?.totalCount || 0;
    const totalPages = Math.ceil(totalCount / 10);

    const handleAddClick = () => {
        setSelectedStore(null);
        setIsSlideOverOpen(true);
    };

    const handleEditClick = (store: StoreResponse) => {
        setSelectedStore(store);
        setIsSlideOverOpen(true);
    };

    const handleFormSubmit = async (formData: StoreFormData) => {
        try {
            if (selectedStore) {
                await updateMutation.mutateAsync({ id: selectedStore.storeId, payload: formData });
            } else {
                await createMutation.mutateAsync(formData);
            }
            setIsSlideOverOpen(false);
        } catch (err) {
            console.error("Failed to save store", err);
        }
    };

    const handleToggleActive = async (store: StoreResponse) => {
        try {
            await updateMutation.mutateAsync({
                id: store.storeId,
                payload: { ...store, isActive: !store.isActive }
            });
        } catch (err) {
            console.error("Failed to toggle status", err);
        }
    };

    const columns = useMemo<ColumnDef<StoreResponse>[]>(() => [
        { header: 'ID', accessor: 'storeId', className: 'w-16' },
        { header: 'Store Name', accessor: 'name', className: 'font-medium' },
        { header: 'Address', accessor: 'address' },
        { 
            header: 'Status', 
            accessor: 'isActive',
            render: (item) => (
                <span className={`px-2 py-1 rounded text-xs font-semibold ${item.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-700'}`}>
                    {item.isActive ? 'Active' : 'Inactive'}
                </span>
            )
        },
        {
            header: 'Actions', 
            accessor: 'actions', 
            className: 'text-right w-48',
            render: (item) => (
                <div className="flex justify-end space-x-3">
                    <button onClick={() => handleEditClick(item)} className="text-emerald-600 hover:text-emerald-800 font-medium text-sm">Edit</button>
                    <button onClick={() => handleToggleActive(item)} className={`${item.isActive ? 'text-red-600 hover:text-red-800' : 'text-blue-600 hover:text-blue-800'} font-medium text-sm`}>
                        {item.isActive ? 'Deactivate' : 'Activate'}
                    </button>
                </div>
            )
        }
    ], []);

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-slate-800">Store Management</h2>
                <button 
                    onClick={handleAddClick}
                    className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-md text-sm font-medium transition-colors shadow-sm">
                + Add Store
                </button>
            </div>

            {/* Global Store Map */}
            <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200">
                <div className="mb-4">
                    <h3 className="text-lg font-bold text-slate-800">Geographic Distribution</h3>
                    <p className="text-xs text-slate-500">Overview of all registered store locations.</p>
                </div>
                <div className="h-[400px] w-full relative z-0 rounded-lg overflow-hidden border border-slate-200">
                    <StoreMapCanvas 
                        center={mapCenter} 
                        stores={allStores} 
                        selectedStoreIds={[]}
                        onStoreClick={() => {}}/>
                </div>
            </div>

            {/* Search Bar for the Table */}
            <div className="flex bg-white p-1 rounded-md shadow-sm border border-slate-200 max-w-md">
                <input 
                    type="text" 
                    placeholder="Search stores by name or address..." 
                    value={searchTerm}
                    onChange={(e) => {
                        setSearchTerm(e.target.value);
                        setPage(1); 
                    }}
                    className="w-full px-3 py-2 outline-none text-sm bg-transparent"
                />
            </div>

            {isError && (
                <div className="p-3 bg-red-50 text-red-600 border border-red-200 rounded text-sm">
                Failed to load Store Data. Please try again later.
                </div>
            )}

            <DataTable 
                data={stores}
                columns={columns}
                isLoading={isLoading}
                page={page}
                totalPages={totalPages}
                totalCount={stores.length}
                onPageChange={(newPage) => setPage(newPage)}
            />

            <SlideOver
                isOpen={isSlideOverOpen}
                onClose={() => setIsSlideOverOpen(false)}
                title={selectedStore ? `Edit ${selectedStore.name}` : "Create New Store"}
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