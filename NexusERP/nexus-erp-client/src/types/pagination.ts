export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}

export interface CursorPagedResult<T> {
    items: T[];
    nextCreatedAt: string | null;
    nextLogId?: number | null;         
    nextTransactionId?: number | null; 
    pageSize: number;
    hasMorePages: boolean;
}