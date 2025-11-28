namespace WordMaster.Application.Responses.Admin;

public class LoginStatisticsResponse
{
    public int TotalLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public IReadOnlyList<DailyLoginStatResponse> DailyLoginStats { get; set; } = [];
}
