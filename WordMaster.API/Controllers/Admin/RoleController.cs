using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.Role;
using WordMaster.Application.Responses.Role;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// Rol yönetim işlemlerini gerçekleştiren controller.
/// </summary>
[Authorize(Roles = "admin")]
[Route("api/admin/roles")]
public class RoleController(IRoleService roleService) : BaseController
{
    /// <summary>
    /// Tüm rolleri listeler.
    /// </summary>
    /// <returns>Rol listesi.</returns>
    [HttpGet]
    [RequirePermission("Admin", "Roles", "GetRoles", "GET", "Rolleri listele")]
    public async Task<IActionResult> GetRoles()
    {
        ServiceResult<List<RoleResponse>> result = await roleService.GetRoleListAsync();
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip rolü getirir.
    /// </summary>
    /// <param name="id">Rol ID'si.</param>
    /// <returns>Rol detayları.</returns>
    [HttpGet("{id}")]
    [RequirePermission("Admin", "Roles", "GetRole", "GET", "Rol detayını getir")]
    public async Task<IActionResult> GetRole(string id)
    {
        ServiceResult<RoleUpdateResponse> result = await roleService.FindByIdReturnRoleUpdateViewModelAsync(id);
        return CreateResult(result);
    }

    /// <summary>
    /// Yeni bir rol oluşturur.
    /// </summary>
    /// <param name="request">Oluşturulacak rol bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost]
    [RequirePermission("Admin", "Roles", "CreateRole", "POST", "Rol oluştur")]
    public async Task<IActionResult> CreateRole([FromBody] RoleCreateRequest request)
    {
        ServiceResult result = await roleService.CreateRoleAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Mevcut bir rolü günceller.
    /// </summary>
    /// <param name="request">Güncellenecek rol bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPut]
    [RequirePermission("Admin", "Roles", "UpdateRole", "PUT", "Rol güncelle")]
    public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateRequest request)
    {
        ServiceResult result = await roleService.UpdateRoleAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen rolü siler.
    /// </summary>
    /// <param name="id">Silinecek rol ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpDelete("{id}")]
    [RequirePermission("Admin", "Roles", "DeleteRole", "DELETE", "Rol sil")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        ServiceResult result = await roleService.DeleteRoleAsync(id);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen kullanıcıya atanabilecek rolleri listeler.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    /// <returns>Rol atama listesi.</returns>
    [HttpGet("assign/{userId}")]
    [RequirePermission("Admin", "Roles", "GetRolesForAssign", "GET", "Atanabilir rolleri listele")]
    public async Task<IActionResult> GetRolesForAssign(string userId)
    {
        ServiceResult<List<AssignToRoleResponse>> result = await roleService.GetRoleByIdReturnAssignToRoleAsync(userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcıya rol ataması yapar.
    /// </summary>
    /// <param name="request">Atanacak roller ve kullanıcı bilgisi.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost("assign")]
    [RequirePermission("Admin", "Roles", "AssignRoles", "POST", "Rol ata")]
    public async Task<IActionResult> AssignRoles([FromBody] AssignRolesRequest request)
    {
        ServiceResult result = await roleService.AssignRoleAsync(request);
        return CreateResult(result);
    }
}

