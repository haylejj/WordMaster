using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// Kullanıcı yönetim işlemlerini gerçekleştiren controller.
/// </summary>
[Route("api/admin/users")]
public class UserController(IUserService userService) : BaseController
{
    /// <summary>
    /// Kullanıcıları sayfalı olarak listeler.
    /// </summary>
    /// <param name="search">Aranacak kelime (Kullanıcı adı veya Email).</param>
    /// <param name="page">Sayfa numarası.</param>
    /// <param name="pageSize">Sayfa boyutu.</param>
    /// <returns>Sayfalanmış kullanıcı listesi.</returns>
    [HttpGet]
    [RequirePermission("Admin", "AdminUsers", "GetPagedUsers", "GET", "Kullanıcıları listele")]
    public async Task<IActionResult> GetPagedUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        ServiceResult<PagedResult<UserWithRolesResponse>> result = await userService.GetPagedUsersAsync(search, page, pageSize);
        return CreateResult(result);
    }

    /// <summary>
    /// Tüm kullanıcıları listeler (Sayfalama olmadan).
    /// </summary>
    /// <returns>Kullanıcı listesi.</returns>
    [HttpGet("all")]
    [RequirePermission("Admin", "AdminUsers", "GetAllUsers", "GET", "Tüm kullanıcıları listele")]
    public async Task<IActionResult> GetAllUsers()
    {
        ServiceResult<List<UserResponse>> result = await userService.GetUsersAsync();
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip kullanıcının detaylarını getirir (Düzenleme için).
    /// </summary>
    /// <param name="id">Kullanıcı ID'si.</param>
    /// <returns>Kullanıcı detayları.</returns>
    [HttpGet("{id}")]
    [RequirePermission("Admin", "AdminUsers", "GetUser", "GET", "Kullanıcı detayını getir")]
    public async Task<IActionResult> GetUser(string id)
    {
        ServiceResult<UserProfileResponse> result = await userService.GetProfileByIdAsync(id);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip kullanıcının detaylı bilgilerini getirir (Görüntüleme için).
    /// </summary>
    /// <param name="id">Kullanıcı ID'si.</param>
    /// <returns>Kullanıcı detayları.</returns>
    [HttpGet("{id}/detail")]
    [RequirePermission("Admin", "AdminUsers", "GetUserDetail", "GET", "Kullanıcı detaylı bilgisini getir")]
    public async Task<IActionResult> GetUserDetail(string id)
    {
        ServiceResult<UserDetailResponse> result = await userService.GetUserDetailAsync(id);
        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcı bilgilerini günceller.
    /// </summary>
    /// <param name="request">Güncellenecek kullanıcı bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPut]
    [RequirePermission("Admin", "AdminUsers", "UpdateUser", "PUT", "Kullanıcı güncelle")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request)
    {
        ServiceResult result = await userService.UpdateUserAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcıyı siler.
    /// </summary>
    /// <param name="id">Silinecek kullanıcı ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpDelete("{id}")]
    [RequirePermission("Admin", "AdminUsers", "DeleteUser", "DELETE", "Kullanıcı sil")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        ServiceResult result = await userService.DeleteUserAsync(id);
        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcının şifresini sıfırlar ve yeni şifreyi e-posta ile gönderir.
    /// </summary>
    /// <param name="id">Kullanıcı ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost("{id}/reset-password")]
    [RequirePermission("Admin", "AdminUsers", "ResetPassword", "POST", "Kullanıcı şifresini sıfırla")]
    public async Task<IActionResult> ResetPassword(string id)
    {
        ServiceResult<string> result = await userService.ResetUserPasswordAsync(id);
        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcının rollerini değiştirir.
    /// </summary>
    /// <param name="id">Kullanıcı ID'si.</param>
    /// <param name="roles">Yeni roller.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost("{id}/change-role")]
    [RequirePermission("Admin", "AdminUsers", "ChangeRole", "POST", "Kullanıcı rolünü değiştir")]
    public async Task<IActionResult> ChangeRole(string id, [FromBody] List<string> roles)
    {
        ChangeUserRoleRequest request = new() { UserId = id, Roles = roles };
        ServiceResult result = await userService.ChangeUserRoleAsync(request);
        return CreateResult(result);
    }
}

