using System;

namespace WordMaster.Application.Responses.Admin;

public class DailyLoginStatResponse
{
    public DateOnly Date { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
}
