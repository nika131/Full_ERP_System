import { z } from 'zod';

export const categorySchema = z.object({
    CategoryName: z.string().min(2, "Name must be at least 2 characters.")
})

export type CategoryFormData = z.infer<typeof categorySchema>;
