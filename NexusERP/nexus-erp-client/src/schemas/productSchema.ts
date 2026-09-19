import { z } from 'zod';

export const productSchema = z.object({
    name: z.string().min(1, "Product name is required"),
    categoryId: z.number().min(1, "Category is required"),
    supplierId: z.number().optional().nullable(),
    price: z.number().min(0.01, "Price must be greater than 0"),
    costPrice: z.number().min(0, "Cost price must be positive"),
    quantity: z.number().min(0, "Quantity cannot be negative"),

    lowStockThreshold: z.number().optional().nullable(),
    vatRate: z.number().min(0, "VAT cannot be negative"),
    marketDiscountRate: z.number().min(0, "Market discount cannot be negative"),
    maxDiscountPercentage: z.number().min(0).max(100, "Max discount cannot exceed 100%"),
    barcode: z.string().optional().nullable(),
    imageUrl: z.string().optional().nullable(),
    shapeType: z.string().optional().nullable(),
    shapeColor: z.string().optional().nullable(),
    shapeText: z.string().optional().nullable(),
    displayMode: z.enum(['image', 'shape']),
});

export type ProductFormData = z.infer<typeof productSchema>;