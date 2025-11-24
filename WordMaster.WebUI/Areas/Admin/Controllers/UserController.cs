using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Role;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Responses.Role;
using WordMaster.Application.Responses.User;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class UserController(IUserService userService, IRoleService roleService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        ServiceResult<PagedResult<UserWithRolesResponse>> pageResult = await userService.GetPagedUsersAsync(search, page, pageSize);
        (List<UserWithRolesResponse>? users, int totalCount) = pageResult.IsSuccess && pageResult.Data != null
            ? (pageResult.Data.Items, pageResult.Data.TotalCount)
            : (new List<UserWithRolesResponse>(), 0);

        UserListResponse viewModel = new()
        {
            Users = users,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Search = search
        };

        return View(viewModel);
    }

    [HttpGet("GetUser")]
    public async Task<IActionResult> GetUser(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { success = false, message = "Kullanıcı ID gerekli." });
        }

        ServiceResult<UserEditResponse> result = await userService.GetUserEditViewModelByIdAsync(id);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        UserEditResponse user = result.Data;
        string birthDateStr = user.BirthDate?.ToString("yyyy-MM-dd") ?? "";

        return Json(new
        {
            success = true,
            user = new
            {
                id,
                userName = user.UserName,
                email = user.Email,
                phone = user.Phone ?? "",
                birthDate = birthDateStr,
                gender = user.Gender?.ToString() ?? ""
            }
        });
    }

    [HttpPost("UpdateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        ServiceResult result = await userService.UpdateUserAsync(request);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage() ?? "Kullanıcı güncellenirken bir hata oluştu.";
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Kullanıcı başarıyla güncellendi." });
    }

    [HttpPost("DeleteUser")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        ServiceResult result = await userService.DeleteUserAsync(id);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage() ?? "Kullanıcı silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kullanıcı başarıyla silindi." });
    }

    [HttpGet("GetUserDetail")]
    public async Task<IActionResult> GetUserDetail(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { success = false, message = "Kullanıcı ID gerekli." });
        }

        ServiceResult<UserDetailResponse> result = await userService.GetUserDetailAsync(id);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        UserDetailResponse detail = result.Data;

        return Json(new
        {
            success = true,
            detail = new
            {
                id = detail.Id,
                userName = detail.UserName,
                email = detail.Email,
                phone = detail.Phone ?? "-",
                birthDate = detail.BirthDate?.ToString("dd.MM.yyyy") ?? "-",
                gender = detail.Gender?.ToString() ?? "-",
                totalLoginAttempts = detail.TotalLoginAttempts,
                successfulLogins = detail.SuccessfulLogins,
                failedLogins = detail.FailedLogins,
                lastLoginDate = detail.LastLoginDate?.ToString("dd.MM.yyyy HH:mm") ?? "-",
                lastLoginIpAddress = detail.LastLoginIpAddress ?? "-",
                wordCount = detail.WordCount,
                favoriteCount = detail.FavoriteCount,
                unknowsCount = detail.UnknowsCount,
                lastPracticeDate = detail.LastPracticeDate?.ToString("dd.MM.yyyy HH:mm") ?? "-"
            }
        });
    }

    [HttpGet("GetUserRoles")]
    public async Task<IActionResult> GetUserRoles(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { success = false, message = "Kullanıcı ID gerekli." });
        }

        ServiceResult<List<AssignToRoleResponse>> result = await roleService.GetRoleByIdReturnAssignToRoleAsync(id);
        List<AssignToRoleResponse>? roles = result.Data;

        if (!result.IsSuccess || roles == null || roles.Count == 0)
        {
            return Json(new { success = false, message = "Roller bulunamadı." });
        }

        return Json(new
        {
            success = true,
            roles = roles.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                exist = r.Exist
            }).ToList()
        });
    }

    [HttpPost("UpdateUserRoles")]
    public async Task<IActionResult> UpdateUserRoles([FromBody] UpdateUserRolesRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return Json(new { success = false, message = "Kullanıcı ID gerekli." });
        }

        if (request.Roles == null || request.Roles.Count == 0)
        {
            return Json(new { success = false, message = "Rol bilgisi gerekli." });
        }

        AssignRolesRequest assignRequest = new()
        {
            UserId = request.UserId,
            Roles = request.Roles.Select(r => new AssignToRoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Exist = r.Exist
            }).ToList()
        };

        await roleService.AssignRoleAsync(assignRequest);

        return Json(new { success = true, message = "Kullanıcı rolleri başarıyla güncellendi." });
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { success = false, message = "Kullanıcı ID gerekli." });
        }

        ServiceResult<string> result = await userService.ResetUserPasswordAsync(id);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage() ?? "Şifre sıfırlanırken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Şifre başarıyla sıfırlandı ve kullanıcıya email olarak gönderildi." });
    }
}
