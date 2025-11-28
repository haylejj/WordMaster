namespace WordMaster.Application.Responses.Favorite;

public class FavoriteWordResponse
{
    public long Id { get; set; }
    public string EnglishWord { get; set; } = default!;
    public string TurkishWord { get; set; } = default!;
}
