namespace WordMaster.Application.Requests.Word;

public class CheckTranslationRequest
{
    public string EnglishWord { get; set; } = default!;
    public string TurkishWord { get; set; } = default!;
}

