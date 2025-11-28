namespace WordMaster.Application.Responses.Unknows;

public class UnknowsWordResponse
{
    public long Id { get; set; }
    public string EnglishWord { get; set; } = default!;
    public string TurkishWord { get; set; } = default!;
}
