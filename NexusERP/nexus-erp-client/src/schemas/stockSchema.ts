import { z } from 'zod';

const actionTypes = ['Restock', 'Loss', 'Damage'] as const;

export const stockSchema = z.object({
  transactionType: z.enum(actionTypes, {
    message: "Please select an action type",
  }),
  quantity: z.number({ 
    message: "Quantity must be a valid number" 
  }).min(1, "Quantity must be at least 1"),
});

export type StockFormData = z.infer<typeof stockSchema>;