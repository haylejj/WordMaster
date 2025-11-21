using WordMaster.Application.Dto.Word;

namespace WordMaster.Application.Dto.Favorite;

public class FavoriteWithWordDto
{
    public long Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public long WordId { get; set; }
    public Guid? UserId { get; set; }
    public WordDto? Word { get; set; }
}

