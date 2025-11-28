namespace WordMaster.Application.Responses.User;

public class UserLoginStatsResponse
{
    public int TotalLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
}
