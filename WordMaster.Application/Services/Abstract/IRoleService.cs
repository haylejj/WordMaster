using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests.Role;
using WordMaster.Application.ViewModels.Role;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IRoleService
{
    Task<ServiceResult<List<RoleViewModel>>> GetRoleListAsync();
    Task<ServiceResult> CreateRoleAsync(RoleCreateRequest request);
    Task<ServiceResult<RoleUpdateViewModel>> FindByIdReturnRoleUpdateViewModelAsync(string id);
    Task<ServiceResult> UpdateRoleAsync(RoleUpdateRequest request);
    Task<ServiceResult> DeleteRoleAsync(string id);
    Task<ServiceResult<List<AssignToRoleViewModel>>> GetRoleByIdReturnAssignToRoleAsync(string id);
    Task<ServiceResult> AssignRoleAsync(AssignRolesRequest request);
}
