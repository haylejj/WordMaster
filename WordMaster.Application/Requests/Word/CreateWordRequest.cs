namespace WordMaster.Application.Requests.Word;

public class CreateWordRequest
{
    public string EnglishWord { get; set; } = null!;
    public string TurkishWord { get; set; } = null!;
}
