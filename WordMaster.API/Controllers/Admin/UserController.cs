using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// Kullanıcı yönetim işlemlerini gerçekleştiren controller.
/// </summary>
[Authorize(Roles = "admin")]
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
    public async Task<IActionResult> ResetPassword(string id)
    {
        ServiceResult<string> result = await userService.ResetUserPasswordAsync(id);
        return CreateResult(result);
    }
}

