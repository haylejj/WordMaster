using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

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
