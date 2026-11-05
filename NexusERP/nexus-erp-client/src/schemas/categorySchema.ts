import { z } from 'zod';

export const categorySchema = z.object({
    name: z.string().trim()
    .min(2, "Name must be at least 2 characters.")
    .max(200, "Category name cannot exceed 200 characters."),
})

export type CategoryFormData = z.infer<typeof categorySchema>;