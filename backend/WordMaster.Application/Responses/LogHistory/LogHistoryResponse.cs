namespace WordMaster.Application.Responses.LogHistory;

public class LogHistoryResponse
{
    public long Id { get; set; }
    public Guid? AppUserId { get; set; }
    public string? Email { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSuccessful { get; set; }
    public DateTime AttemptedAt { get; set; }
    public string? Source { get; set; }
}
