namespace WordMaster.Application.Responses.Statistics;

public class WordStatItem
{
    public long Id { get; set; }
    public string EnglishWord { get; set; } = string.Empty;
    public string TurkishWord { get; set; } = string.Empty;
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
}
