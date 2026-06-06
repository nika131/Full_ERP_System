export interface AuditLog {
    logId: number;
    userName: string;
    entityType: string;
    entityId: number;
    action: string;
    changesMade: string;
    createdAt: string;
}