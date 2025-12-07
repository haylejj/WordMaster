import type { PagedResult } from "./api";

export interface LogHistoryResponse {
    id: number;
    appUserId?: string;
    email?: string;
    ipAddress?: string;
    isSuccessful: boolean;
    attemptedAt: string;
    source?: string;
}

export interface GetLogHistoryRequest {
    page: number;
    size: number;
    searchTerm?: string;
    searchInEmail: boolean;
    searchInUserId: boolean;
    searchInIp: boolean;
}

export interface LogHistoryListResponse extends PagedResult<LogHistoryResponse> { }
