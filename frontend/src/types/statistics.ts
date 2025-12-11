export interface WordStatItem {
    id: number;
    englishWord: string;
    turkishWord: string;
    correctCount: number;
    wrongCount: number;
}

export interface DailyActivityStat {
    date: string;
    count: number;
}

export interface DashboardStatisticsResponse {
    totalWords: number;
    totalLearnedWords: number;
    totalCorrectCount: number;
    totalWrongCount: number;
    accuracyRate: number;
    currentStreak: number;
    topBestWords: WordStatItem[];
    topWorstWords: WordStatItem[];
    lastActivities: DailyActivityStat[];
}
