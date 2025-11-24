using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests.Role;
using WordMaster.Application.Responses;
using WordMaster.Application.Responses.Role;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IRoleService
{
    Task<ServiceResult<List<RoleResponse>>> GetRoleListAsync();
    Task<ServiceResult> CreateRoleAsync(RoleCreateRequest request);
    Task<ServiceResult<RoleUpdateResponse>> FindByIdReturnRoleUpdateViewModelAsync(string id);
    Task<ServiceResult> UpdateRoleAsync(RoleUpdateRequest request);
    Task<ServiceResult> DeleteRoleAsync(string id);
    Task<ServiceResult<List<AssignToRoleResponse>>> GetRoleByIdReturnAssignToRoleAsync(string id);
    Task<ServiceResult> AssignRoleAsync(AssignRolesRequest request);
}
