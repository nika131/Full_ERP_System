export interface AuditLog {
    logId: number;
    userId: number;
    username: string;
    entityType: string;
    entityId: number;
    action: string;
    changesMade: string;
    createdAt: string;
}