namespace WordMaster.Application.ViewModels;

public class DailyLoginStatViewModel
{
    public DateOnly Date { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
}

