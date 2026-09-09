import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { storeSchema, type StoreFormData } from '../../schemas/storeSchema';
import { geocodingService, type GeocodingResult } from '../../api/geocodingService';
import { useUserLocation } from '../../hooks/useUserLocation';
import { Search, MapPin, Loader2 } from 'lucide-react';
import { LocationPickerMap } from '../maps/LocationPickerMap';

interface StoreFormProps {
    initialData?: StoreFormData | null;
    onSubmit: (data: StoreFormData) => void;
    onCancel: () => void;
}

export function StoreForm({ initialData, onSubmit, onCancel }: StoreFormProps) {
    const { location: userCoords, isLocating } = useUserLocation();

    const { register, handleSubmit, setValue, watch, reset, formState: { errors, isSubmitting } } = useForm<StoreFormData>({
        resolver: zodResolver(storeSchema),
        defaultValues: {
            name: '',
            address: '',
            latitude: userCoords[0],
            longitude: userCoords[1],
            isActive: true
        }
    });

    const selectedLat = watch('latitude');
    const selectedLng = watch('longitude');

    const [searchQuery, setSearchQuery] = useState('');
    const [searchResults, setSearchResults] = useState<GeocodingResult[]>([]);
    const [isSearching, setIsSearching] = useState(false);

    useEffect(() => {
        if (initialData) {
            reset(initialData);
        } else if (!isLocating) {
            setValue('latitude', userCoords[0]);
            setValue('longitude', userCoords[1]);
        }
    }, [initialData, userCoords, isLocating, reset, setValue]);

    const handleSelectAddress = (result: GeocodingResult) => {
        setValue('address', result.displayName, { shouldValidate: true });
        setValue('latitude', result.latitude, { shouldValidate: true });
        setValue('longitude', result.longitude, { shouldValidate: true });
        setSearchResults([]);
        setSearchQuery('');
    };

    const handleSearchSubmit = async () => {
        if (!searchQuery.trim()) return;
        setIsSearching(true);
        const results = await geocodingService.searchAddress(searchQuery);
        setSearchResults(results);
        setIsSearching(false);
    };

    const handleMapClick = async (lat: number, lon: number) => {
        // 1. Update the hidden form fields
        setValue('latitude', lat, { shouldValidate: true });
        setValue('longitude', lon, { shouldValidate: true });

        // 2. Fetch the street name
        const address = await geocodingService.reverseGeocode(lat, lon);
        
        // 3. Auto-fill the address input box
        if (address) {
            setValue('address', address, { shouldValidate: true });
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5 p-4">
            {/* Store Name */}
            <div>
                <label className="block text-sm font-semibold text-slate-700">Store Name</label>
                <input 
                    {...register('name')} 
                    placeholder="e.g. Nexus Central Hub"
                    className="mt-1 w-full px-3 py-2 border border-slate-300 rounded-md outline-none focus:border-emerald-500 text-sm" 
                />
                {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name.message}</p>}
            </div>

            {/* Address Search / Autocomplete */}
            <div className="relative">
                <label className="block text-sm font-semibold text-slate-700 mb-1">Search Address / Location</label>
                <div className="flex gap-2">
                    <div className="relative flex-1">
                        <input 
                            type="text"
                            placeholder="Type a street, city, or landmark..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); handleSearchSubmit(); } }}
                            className="w-full pl-9 pr-3 py-2 border border-slate-300 rounded-md outline-none focus:border-emerald-500 text-sm"
                        />
                        <Search className="w-4 h-4 text-slate-400 absolute left-3 top-3" />
                    </div>
                    <button 
                        type="button" 
                        onClick={handleSearchSubmit}
                        disabled={isSearching}
                        className="px-4 py-2 bg-slate-800 text-white rounded-md text-sm font-medium hover:bg-slate-700 transition-colors disabled:opacity-50 flex items-center gap-1"
                    >
                        {isSearching ? <Loader2 className="w-4 h-4 animate-spin" /> : 'Find'}
                    </button>
                </div>

                {/* Search Results Dropdown */}
                {searchResults.length > 0 && (
                    <ul className="absolute z-20 left-0 right-0 mt-1 bg-white border border-slate-200 rounded-md shadow-lg max-h-56 overflow-y-auto">
                        {searchResults.map((res, i) => (
                            <li 
                                key={i}
                                onClick={() => handleSelectAddress(res)}
                                className="px-3 py-2 text-xs text-slate-700 hover:bg-emerald-50 hover:text-emerald-700 cursor-pointer border-b border-slate-100 flex items-start gap-2"
                            >
                                <MapPin className="w-3.5 h-3.5 mt-0.5 shrink-0 text-emerald-600" />
                                <span>{res.displayName}</span>
                            </li>
                        ))}
                    </ul>
                )}
            </div>

            {/* Resolved Address Field */}
            <div>
                <label className="block text-sm font-semibold text-slate-700">Official Store Address</label>
                <input 
                    {...register('address')} 
                    placeholder="Selected address will appear here..."
                    className="mt-1 w-full px-3 py-2 border border-slate-300 rounded-md outline-none focus:border-emerald-500 text-sm bg-slate-50" 
                />
                {errors.address && <p className="text-red-500 text-xs mt-1">{errors.address.message}</p>}
            </div>

            {/*NTERACTIVE MAP */}
            <div>
                <label className="block text-sm font-semibold text-slate-700">Pinpoint Location</label>
                <p className="text-xs text-slate-500 mb-1">Click anywhere on the map to set the exact store coordinates.</p>
                
                <LocationPickerMap 
                    latitude={selectedLat || 41.7151} // Default to Tbilisi if 0
                    longitude={selectedLng || 44.8271} 
                    onLocationSelect={handleMapClick} 
                />
            </div>

            {/* Visual Location Preview (Coordinates displayed as read-only badges, NOT manual inputs) */}
            <div className="bg-slate-50 p-3 rounded-md border border-slate-200 flex items-center justify-between text-xs text-slate-600">
                <div className="flex items-center gap-1.5">
                    <MapPin className="w-4 h-4 text-emerald-600" />
                    <span>Coordinates:</span>
                </div>
                <div className="font-mono font-medium">
                    {selectedLat ? selectedLat.toFixed(5) : '0.00000'}, {selectedLng ? selectedLng.toFixed(5) : '0.00000'}
                </div>
            </div>

            {/* Hidden Fields: The values still cleanly submit to your API */}
            <input type="hidden" {...register('latitude', { valueAsNumber: true })} />
            <input type="hidden" {...register('longitude', { valueAsNumber: true })} />

            {/* Active Switch */}
            <div className="flex items-center">
                <input 
                    type="checkbox" 
                    {...register('isActive')} 
                    className="h-4 w-4 text-emerald-600 focus:ring-emerald-500 border-gray-300 rounded"
                />
                <label className="ml-2 block text-sm text-slate-700">Store is Active</label>
            </div>

            {/* Action Buttons */}
            <div className="flex justify-end space-x-3 pt-4 border-t border-slate-200">
                <button type="button" onClick={onCancel} className="px-4 py-2 text-sm text-slate-600 hover:bg-slate-100 rounded border border-slate-200">
                    Cancel
                </button>
                <button type="submit" disabled={isSubmitting} className="px-4 py-2 text-sm text-white bg-emerald-600 hover:bg-emerald-700 rounded disabled:opacity-50">
                    {isSubmitting ? 'Saving...' : 'Save Store'}
                </button>
            </div>
        </form>
    );
}