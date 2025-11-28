namespace WordMaster.Application.Responses.Admin;

public class AdminDashboardResponse
{
    public int TotalFolders { get; set; }
    public int TotalWords { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalUnknows { get; set; }
    public int TotalUsers { get; set; }
    public int LockedUserCount { get; set; }
    public int ActiveUsers24h { get; set; }
    public int NewWords24h { get; set; }
    public int NewFolders24h { get; set; }

    // Entity Stats
    public long LastWordId { get; set; }
    public long LastUnknowsId { get; set; }
    public long LastFavoriteId { get; set; }
    public int TotalWordsInFolders { get; set; }

    public int TotalLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public IReadOnlyList<DailyLoginStatResponse> DailyLoginStats { get; set; } = [];
}
