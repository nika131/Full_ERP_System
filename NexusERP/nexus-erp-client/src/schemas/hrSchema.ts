import { z } from 'zod';

export const leaveRequestSchema = z.object({
    type: z.enum(['Vacation', 'Sick', 'Personal', 'Unpaid'], {
        message: "Please select a valid absence type."
    }),
    startDate: z.string().min(1, "Start date is required."),
    endDate: z.string().min(1, "End date is required."),
    notes: z.string().max(500, "Notes cannot exceed 500 characters.").optional()
}).refine((data) => {
    const start = new Date(data.startDate);
    const end = new Date(data.endDate);
    return end >= start;
}, {
    message: "End date cannot be before the start date.",
    path: ["endDate"]
});

export const employeeProfileSchema = z.object({
    fullName: z.string().min(2, "Full name must be at least 2 characters.").max(100, "Name is too long."),
    username: z.string().min(3, "Username must be at least 3 characters.").max(50),
    password: z.string().min(6, "Password must be at least 6 characters."),
    roleId: z.number().positive("You must select a valid system role.")
});

export const salaryRecordSchema = z.object({
    amount: z.number()
        .positive("Salary amount must be greater than zero.")
        .max(1000000, "Amount exceeds maximum allowed limit."),
    effectiveDate: z.string().min(1, "Effective date is required."),
    notes: z.string().min(3, "You must provide a reason for the salary contract.").max(200)
});

export const roleSchema = z.object({
    name: z.string()
        .min(2, "Role name must be at least 2 characters.")
        .max(50, "Role name cannot exceed 50 characters.")
        .refine(val => val.toLowerCase() !== 'admin', {
            message: "Cannot manually create or override the core 'Admin' role."
        }),
    permissions: z.array(z.string()).min(1, "You must select at least one permission.")
});

export type LeaveRequestFormData = z.infer<typeof leaveRequestSchema>;
export type EmployeeProfileFormData = z.infer<typeof employeeProfileSchema>;
export type SalaryRecordFormData = z.infer<typeof salaryRecordSchema>;
export type RoleFormData = z.infer<typeof roleSchema>;