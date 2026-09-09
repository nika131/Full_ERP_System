export interface GeocodingResult {
    displayName: string;
    latitude: number;
    longitude: number;
}

export const geocodingService = {
    searchAddress: async (query: string): Promise<GeocodingResult[]> => {
        if (!query || query.trim().length < 3) return [];

        const response = await fetch(
            `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&addressdetails=1&limit=5`,
            {
                headers: {
                    'Accept-Language': 'en',
                }
            }
        );

        if (!response.ok) return []
        const data = await response.json();

        return data.map((item: any) => ({
            displayName: item.display_name,
            latitude: parseFloat(item.lat),
            longitude: parseFloat(item.lon),
        }));
    },

    reverseGeocode: async (lat: number, lon: number): Promise<string> =>  {
        const response = await fetch(
            `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}`,
            {
                headers: {
                    'Accept-Language': 'en',
                }
            }
        );

        if (!response.ok) return ''
        const data = await response.json();
        return data.display_name || '';
    }
}