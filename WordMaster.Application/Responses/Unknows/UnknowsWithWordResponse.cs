namespace WordMaster.Application.Responses.Unknows;

public class UnknowsWithWordResponse
{
    public long Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public long WordId { get; set; }
    public Guid? UserId { get; set; }
    public UnknowsWordResponse? Word { get; set; }
}
