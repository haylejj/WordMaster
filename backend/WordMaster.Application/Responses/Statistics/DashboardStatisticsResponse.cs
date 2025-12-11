namespace WordMaster.Application.Responses.Statistics;

public class DashboardStatisticsResponse
{
    public int TotalWords { get; set; }
    public int TotalLearnedWords { get; set; }
    public int TotalCorrectCount { get; set; }
    public int TotalWrongCount { get; set; }
    public double AccuracyRate { get; set; }
    public int CurrentStreak { get; set; }
    public List<WordStatItem> TopBestWords { get; set; } = new();
    public List<WordStatItem> TopWorstWords { get; set; } = new();
    public List<DailyActivityStat> LastActivities { get; set; } = new();
}
