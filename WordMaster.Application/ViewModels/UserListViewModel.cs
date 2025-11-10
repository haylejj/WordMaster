using WordMaster.Domain.Entities;

namespace WordMaster.Application.ViewModels;

public class UserListViewModel
{
    public List<UserWithRolesViewModel> Users { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string? Search { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class UserWithRolesViewModel
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = [];
}

