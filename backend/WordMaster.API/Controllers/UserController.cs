using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordMaster.API.Extensions;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Kullanıcı profil işlemlerini yöneten controller.
/// </summary>
[Route("api/user")]
[EnableRateLimiting("GeneralPolicy")]
public class UserController(IUserService userService) : BaseController
{
    /// <summary>
    /// Giriş yapmış kullanıcının profil bilgilerini getirir.
    /// </summary>
    /// <returns>Kullanıcı profil bilgileri.</returns>
    /// <response code="200">Profil bilgileri başarıyla getirildi.</response>
    /// <response code="401">Yetkisiz erişim.</response>
    /// <response code="404">Kullanıcı bulunamadı.</response>
    [HttpGet("profile")]
    [RequirePermission("Public", "User", "GetProfile", "GET", "Profil bilgilerini getir")]
    public async Task<IActionResult> GetProfile()
    {
        Guid userId = User.GetUserId();
        ServiceResult<UserProfileResponse> result = await userService.GetProfileByIdAsync(userId.ToString());
        return CreateResult(result);
    }

    /// <summary>
    /// Giriş yapmış kullanıcının profil bilgilerini günceller.
    /// </summary>
    /// <param name="request">Güncellenecek profil bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    /// <response code="204">Profil başarıyla güncellendi.</response>
    /// <response code="400">Geçersiz istek veya validasyon hatası.</response>
    /// <response code="401">Yetkisiz erişim.</response>
    /// <response code="404">Kullanıcı bulunamadı.</response>
    [HttpPut("profile")]
    [RequirePermission("Public", "User", "UpdateProfile", "PUT", "Profil bilgilerini güncelle")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateRequest request)
    {
        Guid userId = User.GetUserId();
        request.Id = userId.ToString();
        ServiceResult result = await userService.UpdateUserAsync(request);
        return CreateResult(result);
    }
}
