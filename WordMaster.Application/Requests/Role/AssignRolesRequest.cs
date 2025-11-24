using WordMaster.Application.ViewModels.Role;

namespace WordMaster.Application.Requests.Role;

public class AssignRolesRequest
{
    public string UserId { get; set; } = null!;
    public List<AssignToRoleViewModel> Roles { get; set; } = [];
}

