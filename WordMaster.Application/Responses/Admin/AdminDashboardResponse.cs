using System.Collections.Generic;
using WordMaster.Application.Responses;

namespace WordMaster.Application.Responses.Admin;

public class AdminDashboardResponse
{
    public int TotalWords { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalUnknows { get; set; }
    public int TotalUsers { get; set; }
    public int TotalLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public IReadOnlyList<DailyLoginStatResponse> DailyLoginStats { get; set; } = [];
}
