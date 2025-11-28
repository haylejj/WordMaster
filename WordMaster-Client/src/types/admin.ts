export interface DailyLoginStatResponse {
    date: string; // DateOnly comes as string in JSON usually
    successCount: number;
    failCount: number;
}

export interface AdminDashboardResponse {
    totalFolders: number;
    totalWords: number;
    totalFavorites: number;
    totalUnknows: number;
    totalUsers: number;
    lockedUserCount: number;
    activeUsers24h: number;
    newWords24h: number;
    newFolders24h: number;

    // Entity Stats
    lastWordId: number;
    lastUnknowsId: number;
    lastFavoriteId: number;
    totalWordsInFolders: number;

    totalLogins: number;
    successfulLogins: number;
    failedLogins: number;
    dailyLoginStats: DailyLoginStatResponse[];
}
