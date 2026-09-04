import { z } from 'zod';

export const storeSchema = z.object({
    name: z.string()
        .min(2, "Store name must be at least 2 characters")
        .max(100, "Store name cannot exceed 100 characters"),
        
    address: z.string()
        .min(5, "Address must be at least 5 characters")
        .max(250, "Address cannot exceed 250 characters"),
        
    latitude: z.number({ message: "Latitude must be a valid number and is required" })
        .min(-90, "Latitude must be between -90 and 90")
        .max(90, "Latitude must be between -90 and 90"),
        
    longitude: z.number({ message: "Longitude must be a valid number and is required" })
        .min(-180, "Longitude must be between -180 and 180")
        .max(180, "Longitude must be between -180 and 180"),
        
    isActive: z.boolean().default(true)
});

export type StoreFormData = z.infer<typeof storeSchema>;