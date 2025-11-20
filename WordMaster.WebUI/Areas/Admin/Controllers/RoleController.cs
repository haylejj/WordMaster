using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Role;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Role;
using WordMaster.Domain.Results;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class RoleController(IRoleService roleService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<RoleViewModel> roles = await roleService.GetRoleListAsync();
        RoleListViewModel viewModel = new()
        {
            Roles = roles
        };
        return View(viewModel);
    }

    [HttpGet("GetRole")]
    public async Task<IActionResult> GetRole(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { success = false, message = "Rol ID gerekli." });
        }

        Result<RoleUpdateViewModel> result = await roleService.FindByIdReturnRoleUpdateViewModelAsync(id);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Rol bulunamadı." });
        }

        RoleUpdateViewModel role = result.Data;

        return Json(new
        {
            success = true,
            role = new
            {
                id = role.Id,
                name = role.Name
            }
        });
    }

    [HttpPost("UpdateRole")]
    public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        Result<IEnumerable<IdentityError>> result = await roleService.UpdateRoleAsync(request);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage ?? "Rol güncellenirken bir hata oluştu.";
            if (result.Data != null && result.Data.Any())
            {
                errorMessage = string.Join(", ", result.Data.Select(e => e.Description));
            }
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Rol başarıyla güncellendi." });
    }

    [HttpPost("DeleteRole")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        Result<IEnumerable<IdentityError>> result = await roleService.DeleteRoleAsync(id);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage ?? "Rol silinirken bir hata oluştu.";
            if (result.Data != null && result.Data.Any())
            {
                errorMessage = string.Join(", ", result.Data.Select(e => e.Description));
            }
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Rol başarıyla silindi." });
    }

    [HttpPost("CreateRole")]
    public async Task<IActionResult> CreateRole([FromBody] RoleCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        Result<IEnumerable<IdentityError>> result = await roleService.CreateRoleAsync(request);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage ?? "Rol oluşturulurken bir hata oluştu.";
            if (result.Data != null && result.Data.Any())
            {
                errorMessage = string.Join(", ", result.Data.Select(e => e.Description));
            }
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Rol başarıyla oluşturuldu." });
    }
}

