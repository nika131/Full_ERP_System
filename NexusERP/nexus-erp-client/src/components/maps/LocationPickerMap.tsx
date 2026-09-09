import { MapContainer, TileLayer, Marker, useMapEvents, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import { useEffect } from 'react';

// Fix for default Leaflet marker icons in React
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
    iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
    iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

interface LocationPickerProps {
    latitude: number;
    longitude: number;
    onLocationSelect: (lat: number, lng: number) => void;
}

// 1. Invisible component to catch clicks
function ClickHandler({ onLocationSelect }: { onLocationSelect: (lat: number, lng: number) => void }) {
    useMapEvents({
        click(e) {
            onLocationSelect(e.latlng.lat, e.latlng.lng);
        },
    });
    return null;
}

// 2. Invisible component to recenter map when search changes the coordinates
function MapCenterer({ lat, lng }: { lat: number; lng: number }) {
    const map = useMap();
    useEffect(() => {
        map.setView([lat, lng], map.getZoom());
    }, [lat, lng, map]);
    return null;
}

export function LocationPickerMap({ latitude, longitude, onLocationSelect }: LocationPickerProps) {
    return (
        <div className="h-64 w-full relative z-0 rounded-md overflow-hidden border border-slate-300 mt-2">
            <MapContainer 
                center={[latitude, longitude]} 
                zoom={14} 
                style={{ height: '100%', width: '100%' }}
            >
                <TileLayer
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    attribution='&copy; <a href="https://osm.org/copyright">OpenStreetMap</a> contributors'
                />
                <ClickHandler onLocationSelect={onLocationSelect} />
                <MapCenterer lat={latitude} lng={longitude} />
                
                {/* Draw the pin where the user clicked or searched */}
                {(latitude !== 0 && longitude !== 0) && (
                    <Marker position={[latitude, longitude]} />
                )}
            </MapContainer>
        </div>
    );
}