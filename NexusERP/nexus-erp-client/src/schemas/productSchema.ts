import { z } from 'zod';

export const productSchema = z.object({
    name: z.string().min(2, "Product name must be at least 2 characters."),
    categoryId: z.number().min(1, "Please select a category."),
    supplierId: z.number().nullable().optional(),
    quantity: z.number().min(0, "Quantity cannot be negative."),
    price: z.number().min(0.01, "Price must be greater than 0."),
    costPrice: z.number().min(0.01, "Cost price must be greater than 0.")
});

export type ProductFormData = z.infer<typeof productSchema>;