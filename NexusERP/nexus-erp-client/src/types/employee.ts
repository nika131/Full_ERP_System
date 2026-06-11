export interface EmployeeResponse {
    userId: number;
    fullName: string;
    username: string;
    roleName: string;
    roleId: number;
    salary: number | null; 
    createdAt: string;
}

export interface EmployeeUpdatePayload {
    fullName: string;
    username: string;
    roleId: number;
    salary: number | null;
}

export interface SalaryRecordResponse {
    salaryRecordId: number;
    amount: number;
    effectiveDate: string;
    notes: string | null;
    createdAt: string;
}

export interface SalaryRecordCreatePayload {
    amount: number;
    effectiveDate: string;
    notes: string | null;
}