using WordMaster.Application.Dto.Favorite;
using WordMaster.Application.Dto.Unknows;

namespace WordMaster.Application.Dto.Word;

public class WordDto
{
    public long Id { get; set; }
    public string? EnglishWord { get; set; }
    public string? TurkishWord { get; set; }
    public FavoriteDto? Favorite { get; set; }
    public UnknowsDto? Unknows { get; set; }
}
