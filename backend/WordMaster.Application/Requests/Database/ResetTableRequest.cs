namespace WordMaster.Application.Requests.Database;

public class ResetTableRequest
{
    public string TableName { get; set; }
    public string Password { get; set; }
    public string? TargetUserId { get; set; }
}
