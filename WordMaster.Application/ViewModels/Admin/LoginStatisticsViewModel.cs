namespace WordMaster.Application.ViewModels.Admin;

public class LoginStatisticsViewModel
{
    public int TotalLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public IReadOnlyList<DailyLoginStatViewModel> DailyLoginStats { get; set; } = [];
}

