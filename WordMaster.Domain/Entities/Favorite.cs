namespace WordMaster.Domain.Entities;

public class Favorite
{
    public long Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public long WordId { get; set; }
    public Word? Word { get; set; }
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
}
