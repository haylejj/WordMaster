export interface UserWithRolesResponse {
    id: string;
    userName: string;
    email: string;
    isLockedOut: boolean;
    gender?: string;
    roles: string[];
}

export interface UserDetailResponse {
    id: string;
    userName: string;
    email: string;
    phone?: string;
    birthDate?: string;
    gender?: string; // "Kadın" or "Erkek"

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
    birthDate?: string;
    gender?: string | number; // Can be string ("Kadın") or number (enum value) depending on usage
}

export interface UserProfileResponse {
    userName: string;
    email: string;
    phone?: string;
    birthDate?: string;
    gender?: number;
}

export interface UpdateProfileRequest {
    userName: string;
    email: string;
    phone?: string;
    birthDate?: string | null;
    gender?: number | null;
}
