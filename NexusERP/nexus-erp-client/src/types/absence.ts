export interface LeaveRequestPayload {
    type: string;
    startDate: string;
    endDate: string;
    notes: string | null;
}

export interface LeaveReviewPayload {
    status: string;
    reviewerComments: string | null;
}

export interface LeaveResponse {
    absenceId: number;
    userId: number;
    employeeName: string;
    type: string;
    startDate: string;
    endDate: string;
    notes: string | null;
    status: string;
    reviewerName: string | null;
    reviewerComments: string | null;
    createdAt: string;
}