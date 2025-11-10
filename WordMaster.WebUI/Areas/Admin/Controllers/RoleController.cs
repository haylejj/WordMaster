using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class RoleController(IRoleService roleService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var roles = await roleService.GetRoleListAsync();
        var viewModel = new RoleListViewModel
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

        var result = await roleService.FindByIdReturnRoleUpdateViewModelAsync(id);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Rol bulunamadı." });
        }

        var role = result.Data;

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
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        var result = await roleService.UpdateRoleAsync(request);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessage ?? "Rol güncellenirken bir hata oluştu.";
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
        var result = await roleService.DeleteRoleAsync(id);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessage ?? "Rol silinirken bir hata oluştu.";
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
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        var result = await roleService.CreateRoleAsync(request);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessage ?? "Rol oluşturulurken bir hata oluştu.";
            if (result.Data != null && result.Data.Any())
            {
                errorMessage = string.Join(", ", result.Data.Select(e => e.Description));
            }
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Rol başarıyla oluşturuldu." });
    }
}

