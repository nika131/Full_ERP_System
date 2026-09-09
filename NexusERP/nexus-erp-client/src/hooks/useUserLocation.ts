import { useState, useEffect } from 'react';

const DEFAULT_CENTER: [number, number] = [41.7151, 44.8271]; // Tbilisi fallback

export function useUserLocation() {
    const [location, setLocation] = useState<[number, number]>(DEFAULT_CENTER);
    const [isLocating, setIsLocating] = useState(true);

    useEffect(() => {
        if (!navigator.geolocation) {
            setIsLocating(false);
            return;
        }

        navigator.geolocation.getCurrentPosition(
            (position) => {
                setLocation([position.coords.latitude, position.coords.longitude]);
                setIsLocating(false);
            },
            (error) => {
                console.warn('Geolocation blocked or unavailable. Falling back to default center.', error);
                setIsLocating(false);
            },
            { timeout: 5000 }
        );
    }, []);

    return { location, isLocating };
}