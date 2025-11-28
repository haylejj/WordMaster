namespace WordMaster.Application.Responses.Practice;

public class PracticeWordResponse
{
    public long Id { get; set; }
    public string EnglishWord { get; set; } = default!;
    public string TurkishWord { get; set; } = default!;
}
