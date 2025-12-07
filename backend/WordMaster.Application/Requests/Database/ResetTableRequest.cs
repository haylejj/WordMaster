namespace WordMaster.Application.Requests.Database;

public class ResetTableRequest
{
    public string TableName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? TargetUserId { get; set; }
}
