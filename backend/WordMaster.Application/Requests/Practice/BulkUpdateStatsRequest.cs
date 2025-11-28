namespace WordMaster.Application.Requests.Practice;

public class PracticeResultItem
{
    public int WordId { get; set; }
    public bool IsCorrect { get; set; }
}

public class BulkUpdateStatsRequest
{
    public List<PracticeResultItem> Results { get; set; } = [];
}
