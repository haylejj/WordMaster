namespace WordMaster.Application.Requests.Word;

public class UpdateWordRequest
{
    public long Id { get; set; }
    public string EnglishWord { get; set; } = null!;
    public string TurkishWord { get; set; } = null!;
}
