using Microsoft.AspNetCore.Identity;

namespace Core.Entity
{
    public class Favorite
    {
        public int Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public int WordId { get; set; }
        public Word? Word { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
