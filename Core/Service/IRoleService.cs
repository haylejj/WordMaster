using Core.Requests;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Core.Results;

namespace Core.Service;

public interface IRoleService
{
    Task<List<RoleViewModel>> GetRoleListAsync();
    Task<Result<IEnumerable<IdentityError>>> CreateRoleAsync(RoleCreateRequest request);
    Task<Result<RoleUpdateViewModel>> FindByIdReturnRoleUpdateViewModelAsync(string id);
    Task<Result<IEnumerable<IdentityError>>> UpdateRoleAsync(RoleUpdateRequest request);
    Task<Result<IEnumerable<IdentityError>>> DeleteRoleAsync(string id);
    Task<List<AssignToRoleViewModel>> GetRoleByIdReturnAssignToRoleAsync(string id);
    Task AssignRoleAsync(string id, List<AssignToRoleViewModel> request);
}
