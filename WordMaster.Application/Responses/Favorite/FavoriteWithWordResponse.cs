using System;
using WordMaster.Application.Responses.Word;

namespace WordMaster.Application.Responses.Favorite;

public class FavoriteWithWordResponse
{
    public long Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public long WordId { get; set; }
    public Guid? UserId { get; set; }
    public WordResponse? Word { get; set; }
}
