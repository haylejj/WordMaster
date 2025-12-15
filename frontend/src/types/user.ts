export interface UserWithRolesResponse {
    id: string;
    userName: string;
    email: string;
    isLockedOut: boolean;
    roles: string[];
}

export interface UserDetailResponse {
    id: string;
    userName: string;
    email: string;
    phone?: string;
    firstName?: string;
    lastName?: string;

    // Login Statistics
    totalLoginAttempts: number;
    successfulLogins: number;
    failedLogins: number;
    lastLoginDate?: string;
    lastLoginIpAddress?: string;

    // Word Statistics
    wordCount: number;
    favoriteCount: number;
    unknowsCount: number;
    lastPracticeDate?: string;
}

export interface UserUpdateRequest {
    id: string;
    userName: string;
    email: string;
    phone?: string;
    firstName?: string;
    lastName?: string;
}

export interface UserProfileResponse {
    userName: string;
    email: string;
    phone?: string;
    firstName?: string;
    lastName?: string;
}

export interface UpdateProfileRequest {
    email: string;
    phone?: string;
    firstName?: string | null;
    lastName?: string | null;
}
