import { MapContainer, TileLayer, Marker, Popup, Circle, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import type { StoreResponse } from '../../api/storeService';
import { useEffect } from 'react';

import L from 'leaflet';
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';

let DefaultIcon = L.icon({ iconUrl: icon, shadowUrl: iconShadow, iconAnchor: [12, 41] });
L.Marker.prototype.options.icon = DefaultIcon;

interface MapCanvasProps {
    center: [number, number];
    radius: number;
    stores: StoreResponse[];
}

const MapUpdater = ({ center }: { center: [number, number] }) => {
    const map = useMap();
    useEffect(() => { map.setView(center); }, [center, map]);
    return null;
};

export const StoreMapCanvas = ({ center, radius, stores }: MapCanvasProps) => {
    return (
        <div className="h-125 w-full rounded-lg overflow-hidden border border-slate-300 shadow-sm z-0">
            <MapContainer center={center} zoom={13} scrollWheelZoom={true} className="h-full w-full">
                <TileLayer
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                <MapUpdater center={center} />
                
                {/* Draw the search radius boundary */}
                <Circle center={center} radius={radius} pathOptions={{ color: '#10b981', fillColor: '#10b981', fillOpacity: 0.1 }} />

                {/* Draw the store pins */}
                {stores.map(store => (
                    <Marker key={store.storeId} position={[store.latitude, store.longitude]}>
                        <Popup>
                            <strong>{store.name}</strong><br />
                            {store.address}
                        </Popup>
                    </Marker>
                ))}
            </MapContainer>
        </div>
    );
};