import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import type { StoreResponse } from '../../api/storeService';
import { useEffect } from 'react';
import L from 'leaflet';
import { MapPin } from 'lucide-react';
import { renderToStaticMarkup } from 'react-dom/server';


const createDivIcon = (color: string, selected: boolean) =>
    L.divIcon({
        html: renderToStaticMarkup(
            <div className={`pin-wrapper ${selected ? 'pin-selected' : ''}`}>
                <MapPin
                    size={34}
                    color="white"
                    fill={color}
                    strokeWidth={1.5}
                    className="pin-icon"
                />
            </div>
        ),
        className: 'custom-pin-icon',
        iconSize: [34, 34],
        iconAnchor: [17, 34],
        popupAnchor: [0, -34],
    });

const defaultIcon = createDivIcon('#0ea5e9', false);
const selectedIcon = createDivIcon('#10b981', true);


L.Marker.prototype.options.icon = defaultIcon;

interface MapCanvasProps {
    center: [number, number];
    stores: StoreResponse[];
    selectedStoreIds: number[];
    onStoreClick: (storeId: number) => void;
}

const MapUpdater = ({ center }: { center: [number, number] }) => {
    const map = useMap();
    useEffect(() => { map.setView(center); }, [center, map]);
    return null;
};

export const StoreMapCanvas = ({ center, stores, selectedStoreIds, onStoreClick }: MapCanvasProps) => {
    return (
        <div className="h-125 w-full rounded-lg overflow-hidden border border-slate-300 shadow-sm z-0">
            <MapContainer center={center} zoom={13} scrollWheelZoom={true} className="h-full w-full">
                <TileLayer
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                <MapUpdater center={center} />

                {/* Draw the store pins */}
                {stores.map(store => {
                    const isSelected = selectedStoreIds.includes(store.storeId);
                    return(
                        <Marker 
                            key={store.storeId}     
                            position={[store.latitude, store.longitude]}
                            icon={isSelected ? selectedIcon : defaultIcon}
                            eventHandlers={{
                                click: () => onStoreClick(store.storeId)
                                }}
                        >
                            <Popup>
                                <strong>{store.name}</strong><br />
                                {store.address} <br />
                                <span className="text-xs text-slate-500 mt-1 block">
                                    {isSelected ? 'Click pin to remove filter' : 'Click pin to filter by store'}
                                </span>
                            </Popup>
                        </Marker>
                    )    
                })}
            </MapContainer>
        </div>
    );
};