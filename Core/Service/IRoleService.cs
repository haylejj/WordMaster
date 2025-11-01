using Core.Requests;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Core.Service;

public interface IRoleService
{
    Task<List<RoleViewModel>> GetRoleListAsync();
    Task<(bool, IEnumerable<IdentityError>?)> CreateRoleAsync(RoleCreateRequest request);
    Task<(bool, RoleUpdateViewModel?)> FindByIdReturnRoleUpdateViewModelAsync(string id);
    Task<(bool, IEnumerable<IdentityError>?)> UpdateRoleAsync(RoleUpdateRequest request);
    Task<(bool, IEnumerable<IdentityError>?)> DeleteRoleAsync(string id);
    Task<List<AssignToRoleViewModel>> GetRoleByIdReturnAssignToRoleAsync(string id);
    Task AssignRoleAsync(string id, List<AssignToRoleViewModel> request);
}
