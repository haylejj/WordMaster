using WordMaster.Domain.Entities;

namespace WordMaster.Application.ViewModels;

public class FavoriteListViewModel
{
    public List<Word> Words { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string? Search { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}


