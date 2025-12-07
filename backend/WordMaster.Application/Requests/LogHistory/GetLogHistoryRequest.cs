namespace WordMaster.Application.Requests.LogHistory;

public class GetLogHistoryRequest
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool SearchInEmail { get; set; } = true;
    public bool SearchInUserId { get; set; } = true;
    public bool SearchInIp { get; set; } = true;
}
