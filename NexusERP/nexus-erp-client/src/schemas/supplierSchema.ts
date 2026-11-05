import { z } from 'zod';

export const supplierSchema = z.object({
    companyName: z.string().trim()
    .min(2, "Company name must be at least 2 characters.")
    .max(200, "Supplier name cannot exceed 200 characters."),
    contactName: z.string().optional().or(z.literal('')),
    phone: z.string().optional().or(z.literal('')),
    email: z.string().email("Invalid email format.").optional().or(z.literal('')),
});

export type SupplierFormData = z.infer<typeof supplierSchema>;