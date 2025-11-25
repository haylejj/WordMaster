using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Role;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Results;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class UserController(IUserService userService, IRoleService roleService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        Result<(List<UserWithRolesViewModel> Users, int TotalCount)> pageResult = await userService.GetPagedUsersAsync(search, page, pageSize);
        (List<UserWithRolesViewModel>? users, int totalCount) = pageResult.IsSuccess ? pageResult.Data : (new List<UserWithRolesViewModel>(), 0);

        UserListViewModel viewModel = new()
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

        Result<UserEditViewModel> result = await userService.GetProfileByIdAsync(id);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        UserEditViewModel user = result.Data;
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

        UserEditRequest userEditRequest = new()
        {
            UserName = request.UserName,
            Email = request.Email,
            Phone = request.Phone,
            BirthDate = request.BirthDate,
            Gender = request.Gender
        };

        Result<IEnumerable<IdentityError>> result = await userService.UpdateUserAsync(request.Id, userEditRequest);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage ?? "Kullanıcı güncellenirken bir hata oluştu.";
            if (result.Data != null && result.Data.Any())
            {
                errorMessage = string.Join(", ", result.Data.Select(e => e.Description));
            }
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Kullanıcı başarıyla güncellendi." });
    }

    [HttpPost("DeleteUser")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        Result<bool> result = await userService.DeleteUserAsync(id);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Kullanıcı silinirken bir hata oluştu." });
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

        Result<UserDetailViewModel> result = await userService.GetUserDetailAsync(id);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        UserDetailViewModel detail = result.Data;

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

        List<AssignToRoleViewModel> roles = await roleService.GetRoleByIdReturnAssignToRoleAsync(id);

        if (roles == null || roles.Count == 0)
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

        List<AssignToRoleViewModel> assignToRoleViewModels = request.Roles.Select(r => new AssignToRoleViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Exist = r.Exist
        }).ToList();

        await roleService.AssignRoleAsync(request.UserId, assignToRoleViewModels);

        return Json(new { success = true, message = "Kullanıcı rolleri başarıyla güncellendi." });
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { success = false, message = "Kullanıcı ID gerekli." });
        }

        Result<string> result = await userService.ResetUserPasswordAsync(id);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Şifre sıfırlanırken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Şifre başarıyla sıfırlandı ve kullanıcıya email olarak gönderildi." });
    }
}


