using WordMaster.Application.Dto.Word;

namespace WordMaster.Application.Dto.Favorite;

public class FavoriteWithWordDto
{
    public int Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public int WordId { get; set; }
    public string? UserId { get; set; }
    public WordDto? Word { get; set; }
}

